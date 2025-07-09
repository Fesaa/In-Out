using API.Data;
using API.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel
            .Information()
            .CreateBootstrapLogger();

        try
        {
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<DataContext>();

                logger.LogDebug("Migrating database");
                await context.Database.MigrateAsync();

                logger.LogDebug("Seeding database");



            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((_, services, configuration) =>
            {
                LogLevelOptions.CreateConfig(configuration);
            })
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.Sources.Clear();
                
                var env = ctx.HostingEnvironment;
                cfg.AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile($"config/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseKestrel(opts =>
                {
                    opts.ListenAnyIP(5000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2;});
                });
                
                builder.UseStartup<Startup>();
            });
}
