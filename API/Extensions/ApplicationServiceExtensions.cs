using System.IO.Abstractions;
using API.Data;
using API.Data.Repositories;
using API.Helpers;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        services.AddSignalR(opt => opt.EnableDetailedErrors = true);
        
        services.AddPostgres(configuration);
        
        services.AddSwaggerGen(g =>
        {
            g.UseInlineDefinitionsForEnums();
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
            options.EnableSensitiveDataLogging();
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

    }
}