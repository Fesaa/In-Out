using System.Buffers.Text;
using System.IO.Compression;
using System.Reflection;
using API.Data;
using API.Exceptions;
using API.Extensions;
using API.Helpers;
using API.ManualMigrations;
using API.Middleware;
using Flurl.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Events;

namespace API;

public class Startup(IConfiguration cfg, IWebHostEnvironment env)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationServices(cfg);

        services.AddControllers();
        services.AddCors();
        services.AddIdentityServices(cfg, env);
        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = $"{BuildInfo.AppName} API",
                Description = $"{BuildInfo.AppName} API",
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });
            
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //opt.IncludeXmlComments(filePath, true);
            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                In = ParameterLocation.Header,
                Description = "JWT token obtained from OIDC",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        }
                    },
                    Array.Empty<string>()
                }
            });

            opt.AddServer(new OpenApiServer
            {
                Url = "{protocol}://{hostpath}",
                Variables = new Dictionary<string, OpenApiServerVariable>
                {
                    { "protocol", new OpenApiServerVariable { Default = "http", Enum = ["http", "https"] } },
                    { "hostpath", new OpenApiServerVariable { Default = "localhost:5000" } },
                }
            });
        });
        services.AddResponseCompression(opt =>
        {
            opt.Providers.Add<BrotliCompressionProvider>();
            opt.Providers.Add<GzipCompressionProvider>();
            opt.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["image/jpeg", "image/jpg", "image/png", "image/avif", "image/gif", "image/webp", "image/tiff"]);
            opt.EnableForHttps = true;
        });
        services.Configure<BrotliCompressionProviderOptions>(opt =>
        {
            opt.Level = CompressionLevel.Fastest;
        });
        services.AddResponseCaching();
        // TODO: Rate limitter

        services.AddOpenTelemetry()
            .ConfigureResource(src => src
                .AddService(BuildInfo.AppName))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsqlInstrumentation()
                .AddMeter(BuildInfo.AppName)
                .AddPrometheusExporter());
    }

    public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            Task.Run(async () =>
            {
                var ctx = serviceProvider.GetRequiredService<DataContext>();
                
                logger.LogInformation("Running Migrations");
                
                await ManualMigrationAddStockForExistingProducts.Migrate(ctx, logger);
                await ManualMigrationAddProductSortValues.Migrate(ctx, logger);
                
                logger.LogInformation("Running Migrations - complete");
            }).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred during migration");
        }
        
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "In-Out API " + BuildInfo.Version);
            });
        }

        app.UseSerilogRequestLogging(opts =>
        {
            //opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest;
            opts.IncludeQueryInRequestPath = true;
        });
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseResponseCompression();
        app.UseRouting();

        // Ordering is important. Cors, authentication, authorization
        if (env.IsDevelopment())
        {
            app.UseCors(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins("http://localhost:4300", "http://localhost:5000", "http://192.168.1.61:4300")
                .WithExposedHeaders("Content-Disposition", "Pagination"));
        }
        else
        {
            app.UseCors(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition", "Pagination"));
        }

        app.UseResponseCaching();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles(new StaticFileOptions
        {
            HttpsCompression = HttpsCompressionMode.Compress,
            OnPrepareResponse = ctx =>
            {
                if (ctx.Context.User.Identity?.IsAuthenticated ?? false)
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + TimeSpan.FromHours(24);
                    ctx.Context.Response.Headers["X-Robots-Tag"] = "noindex,nofollow";
                }
                else
                {
                    ctx.Context.Response.Redirect($"/Auth/login?returnUrl={Uri.EscapeDataString(ctx.Context.Request.Path)}");
                }
            },
        });
        app.UseDefaultFiles();



        var apiKey = cfg.GetValue<string>("ApiKey");
        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.StartsWithSegments("/metrics"))
            {
                if (!ctx.Request.Query.TryGetValue("api-key", out var key) ||
                    key != apiKey || string.IsNullOrWhiteSpace(apiKey))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await ctx.Response.WriteAsync("Unauthorized");
                    return;
                }
            }

            await next();
        });
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            //endpoints.MapHub<MessageHub>("hubs/messages");
            //endpoints.MapHub<LogHub>("hubs/logs");
            endpoints.MapFallbackToController("Index", "Fallback");
            endpoints.MapPrometheusScrapingEndpoint().DisableHttpMetrics();
        });
        
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                OverrideFaviconIfSet(logger);
                logger.LogInformation("{Name} - v{Version}", BuildInfo.AppName, BuildInfo.Version);
            }
            catch (Exception)
            {
                /* Swallow Exception */
                Console.WriteLine($"{BuildInfo.AppName} - v{BuildInfo.Version}");
            }
        });
    }

    private void OverrideFaviconIfSet(ILogger<Program> logger)
    {
        try
        {
            var favicon = cfg.GetValue<string>("Favicon");
            if (string.IsNullOrWhiteSpace(favicon)) return;

            if (!Directory.Exists("wwwroot")) return;

            var filePath = Path.Join("wwwroot", "favicon.ico");

            var data = Convert.FromBase64String(favicon);;
            
            File.WriteAllBytes(filePath, data);
            logger.LogDebug("Overwritten favicon from configuration");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during overwrite of favicon");
        }
    }
}