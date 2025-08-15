using API.Constants;
using API.DTOs;
using API.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{

    public const string OpenIdConnect = nameof(OpenIdConnect);

    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {

        var openIdConnectConfig = configuration.GetSection(OpenIdConnect).Get<OidcConfigurationDto>();
        if (openIdConnectConfig == null)
            throw new Exception("OpenIdConnect configuration is missing");

        services.AddTransient<IClaimsTransformation, KeyCloakRolesMapper>();
        services.AddAuthentication(OpenIdConnect)
            .AddJwtBearer(OpenIdConnect, options =>
            {
                options.Authority =  openIdConnectConfig.Authority;
                options.Audience = openIdConnectConfig.ClientId;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = openIdConnectConfig.ClientId,
                    ValidIssuer = openIdConnectConfig.Authority,

                    ValidateIssuer = true,
                    ValidateAudience = true,

                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    RequireSignedTokens = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = SetTokenFromQuery,
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyConstants.CreateForOthers)
            .AddPolicy(PolicyConstants.ViewAllDeliveries)
            .AddPolicy(PolicyConstants.HandleDeliveries)
            .AddPolicy(PolicyConstants.ManageStock)
            .AddPolicy(PolicyConstants.ManageApplication)
            ;

        return services;
    }

    private static AuthorizationBuilder AddPolicy(this AuthorizationBuilder builder, string roleName)
    {
        return builder.AddPolicy(roleName, policy => 
            policy.RequireRole(roleName, roleName.ToLower(), roleName.ToUpper()));
    }
    
    private static Task SetTokenFromQuery(MessageReceivedContext context)
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        // Only use query string based token on SignalR hubs
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs")) context.Token = accessToken;

        return Task.CompletedTask;
    }
    
}