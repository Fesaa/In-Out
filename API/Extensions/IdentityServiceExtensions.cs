using API.Constants;
using API.DTOs;
using API.Helpers;
using API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace API.Extensions;

public static class IdentityServiceExtensions
{

    public const string OpenIdConnect = nameof(OpenIdConnect);

    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var openIdConnectConfig = configuration.GetSection(OpenIdConnect).Get<OidcConfigurationDto>();
        if (openIdConnectConfig == null || !openIdConnectConfig.ValidConfig())
            throw new Exception("OpenIdConnect configuration is missing or invalid");

        services.AddSingleton<ConfigurationManager<OpenIdConnectConfiguration>>(_ =>
        {
            var url = openIdConnectConfig.Authority + "/.well-known/openid-configuration";
            return new ConfigurationManager<OpenIdConnectConfiguration>(
                url,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = url.StartsWith("https") }
            );
        });
        services.AddSingleton<OidcConfigurationDto>(_ => openIdConnectConfig);
        services.AddSingleton<TicketSerializer>();

        services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<ITicketStore>((
                options, store) =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.MaxAge = TimeSpan.FromDays(30);
                options.SessionStore = store;

                options.LoginPath = "/Auth/login";
                options.LogoutPath = "/Auth/logout";

                if (environment.IsDevelopment())
                {
                    options.Cookie.Domain = null;
                }

                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = async ctx =>
                    {
                      var oidcService = ctx.HttpContext.RequestServices.GetRequiredService<IOidcService>();
                      await oidcService.RefreshCookieToken(ctx);
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") || ctx.Request.Path.StartsWithSegments("/hubs"))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            ctx.Response.Redirect($"/Auth/login?returnUrl={Uri.EscapeDataString(ctx.Request.Path)}");
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnect, options =>
            {
                options.Authority = openIdConnectConfig.Authority;
                options.ClientId = openIdConnectConfig.ClientId;
                options.ClientSecret = openIdConnectConfig.ClientSecret;
                options.RequireHttpsMetadata = options.Authority.StartsWith("https://");
                
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.CallbackPath = "/signin-oidc";
                options.SignedOutCallbackPath = "/signout-callback-oidc";

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("offline_access");
                options.Scope.Add("roles");
                options.Scope.Add("email");

                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = OidcClaimsPrincipalConverter,
                    OnRedirectToIdentityProviderForSignOut = ctx =>
                    {
                        if (!environment.IsDevelopment() && !string.IsNullOrEmpty(ctx.ProtocolMessage.PostLogoutRedirectUri))
                        {
                            ctx.ProtocolMessage.PostLogoutRedirectUri = ctx.ProtocolMessage.PostLogoutRedirectUri.Replace("http://", "https://");
                        }

                        return Task.CompletedTask;   
                    },
                    OnRedirectToIdentityProvider = ctx =>
                    {
                        if (!environment.IsDevelopment() && !string.IsNullOrEmpty(ctx.ProtocolMessage.RedirectUri))
                        {
                            ctx.ProtocolMessage.RedirectUri = ctx.ProtocolMessage.RedirectUri.Replace("http://", "https://");
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyConstants.CreateForOthers)
            .AddPolicy(PolicyConstants.ViewAllDeliveries)
            .AddPolicy(PolicyConstants.HandleDeliveries)
            .AddPolicy(PolicyConstants.ManageStock)
            .AddPolicy(PolicyConstants.ManageApplication)
            .AddPolicy(PolicyConstants.ManageProducts)
            .AddPolicy(PolicyConstants.ManageClients)
            ;

        return services;
    }

    private static AuthorizationBuilder AddPolicy(this AuthorizationBuilder builder, string roleName)
    {
        return builder.AddPolicy(roleName, policy => 
            policy.RequireRole(roleName, roleName.ToLower(), roleName.ToUpper()));
    }

    private static async Task OidcClaimsPrincipalConverter(TokenValidatedContext ctx)
    {
        if (ctx.Principal == null) return;
        
        var userService = ctx.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var oidcService = ctx.HttpContext.RequestServices.GetRequiredService<IOidcService>();
        await userService.GetUser(ctx.Principal); // Ensure user is created
        
        var tokens = CopyOidcTokens(ctx);
        ctx.Properties ??= new AuthenticationProperties();
        ctx.Properties.StoreTokens(tokens);


        var idToken = ctx.Properties.GetTokenValue(OidcService.IdToken);
        if (!string.IsNullOrEmpty(idToken))
        {
            ctx.Principal = await oidcService.ParseIdToken(idToken);
        }
        
        ctx.Success();
    }
    
    private static List<AuthenticationToken> CopyOidcTokens(TokenValidatedContext ctx)
    {
        if (ctx.TokenEndpointResponse == null)
        {
            return [];
        }

        var tokens = new List<AuthenticationToken>();

        if (!string.IsNullOrEmpty(ctx.TokenEndpointResponse.RefreshToken))
        {
            tokens.Add(new AuthenticationToken { Name = OidcService.RefreshToken, Value = ctx.TokenEndpointResponse.RefreshToken });
        }
        else
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<OidcService>>();
            logger.LogWarning("OIDC login without refresh token, automatic sync will not work for this user");
        }

        if (!string.IsNullOrEmpty(ctx.TokenEndpointResponse.IdToken))
        {
            tokens.Add(new AuthenticationToken { Name = OidcService.IdToken, Value = ctx.TokenEndpointResponse.IdToken });
        }

        if (!string.IsNullOrEmpty(ctx.TokenEndpointResponse.ExpiresIn))
        {
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(double.Parse(ctx.TokenEndpointResponse.ExpiresIn));
            tokens.Add(new AuthenticationToken { Name = OidcService.ExpiresAt, Value = expiresAt.ToString("o") });
        }

        return tokens;
    }
    
    
}