using API.Data.Repositories;
using AutoMapper;

namespace API.Data;

public interface IUnitOfWork
{
    ISettingsRepository SettingsRepository { get; }
    IProductRepository ProductRepository { get; }

    Task<bool> CommitAsync();
    bool HasChanges();
    Task<bool> RollbackAsync();
}

public class UnitOfWork(DataContext ctx, IMapper mapper, ILogger<UnitOfWork> logger): IUnitOfWork
{
    public ISettingsRepository SettingsRepository { get; } = new SettingsRepository(ctx, mapper);
    public IProductRepository ProductRepository { get; } = new ProductRepository(ctx, mapper);

    public async Task<bool> CommitAsync()
    {
        return await ctx.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return ctx.ChangeTracker.HasChanges();
    }

    public async Task<bool> RollbackAsync()
    {
        try
        {
            await ctx.Database.RollbackTransactionAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to rollback a transactions");
        }

        return true;
    }
}