using API.Data;
using API.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(a => a.AddMaps(typeof(AutoMapperProfiles).Assembly));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSignalR(opt => opt.EnableDetailedErrors = true);
        
        services.AddPostgres(configuration);
        
        services.AddSwaggerGen(g =>
        {
            g.UseInlineDefinitionsForEnums();
        });
    }

    private static void AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var pgConnectionString = configuration.GetConnectionString("Postgres");

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