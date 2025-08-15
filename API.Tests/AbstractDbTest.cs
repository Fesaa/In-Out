using API.Data;
using API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace API.Tests;

public class AbstractDbTest(ITestOutputHelper testOutputHelper): IAsyncDisposable
{

    private readonly HashSet<PostgreSqlContainer> _containers = [];
    private readonly SemaphoreSlim _lock = new (1);
    
    protected async Task<(IUnitOfWork, DataContext, IMapper)> CreateDatabase()
    {
        var context = await CreateDataContext();

        await context.Database.EnsureCreatedAsync();

        await SeedDb(context);


        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfiles>());
        var mapper = config.CreateMapper();

        var unitOfWork = new UnitOfWork(context, mapper, null!);

        return (unitOfWork, context, mapper);
    }

    private async Task<DataContext> CreateDataContext()
    {
        await _lock.WaitAsync();

        try
        {
            var postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17.4-alpine")
                .Build();

            await postgreSqlContainer.StartAsync();

            _containers.Add(postgreSqlContainer);

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseNpgsql(postgreSqlContainer.GetConnectionString())
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new DataContext(options);
            return ctx;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<bool> SeedDb(DataContext context)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();
            
            
            return true;
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"[SeedDb] Error: {ex.Message}");
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var container in _containers)
        {
            await container.StopAsync();
            await container.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }
}