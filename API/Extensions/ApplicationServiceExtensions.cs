using System.IO.Abstractions;
using System.Reflection;
using API.Data;
using API.Data.Repositories;
using API.Helpers;
using API.Services;
using API.Services.Store;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(a => a.AddMaps(typeof(AutoMapperProfiles).Assembly));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMemoryCache, MemoryCache>();
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IOidcService, OidcService>();
        services.AddScoped<IServerSettingsService, ServerSettingsService>();

        services.AddSignalR(opt => opt.EnableDetailedErrors = true);
        
        services.AddPostgres(configuration);
        services.AddRedis(configuration);
        services.AddSingleton<ITicketStore, CustomTicketStore>();
        
        services.AddSwaggerGen(g =>
        {
            g.UseInlineDefinitionsForEnums();
            g.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
        });
    }

    private static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                                    ?? configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddDistributedMemoryCache();
            return;
        }
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = BuildInfo.AppName;
        });
    }

    private static void AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var pgConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                                 ?? configuration.GetConnectionString("Postgres");

        services.AddDbContextPool<DataContext>(options =>
        {
            options.UseNpgsql(pgConnectionString, builder =>
            {
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
            options.EnableDetailedErrors();
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

    }
}