using System.IO.Compression;
using System.Reflection;
using API.Data;
using API.Extensions;
using API.Helpers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;

namespace API;

public class Startup(IConfiguration cfg, IWebHostEnvironment env)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationServices(cfg);

        services.AddControllers();
        services.AddCors();
        services.AddIdentityServices(cfg);
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
    }

    public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            Task.Run(() =>
            {
                var ctx = serviceProvider.GetRequiredService<DataContext>();
                
                logger.LogInformation("Running Migrations");
                
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
        
        app.UseResponseCompression();
        app.UseRouting();

        // Ordering is important. Cors, authentication, authorization
        if (env.IsDevelopment())
        {
            app.UseCors(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins("http://localhost:4200", "http://localhost:5000")
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
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            HttpsCompression = HttpsCompressionMode.Compress,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + TimeSpan.FromHours(24);
                ctx.Context.Response.Headers["X-Robots-Tag"] = "noindex,nofollow";
            }
        });
        app.UseSerilogRequestLogging(opts =>
        {
            //opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest;
            opts.IncludeQueryInRequestPath = true;
        });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            //endpoints.MapHub<MessageHub>("hubs/messages");
            //endpoints.MapHub<LogHub>("hubs/logs");
            endpoints.MapFallbackToController("Index", "Fallback");
        });
        
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                logger.LogInformation("{Name} - v{Version}", BuildInfo.AppName, BuildInfo.Version);
            }
            catch (Exception)
            {
                /* Swallow Exception */
                Console.WriteLine($"{BuildInfo.AppName} - v{BuildInfo.Version}");
            }
        });
    }
}