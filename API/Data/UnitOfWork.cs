using API.Data.Repositories;
using API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public interface IUnitOfWork
{
    ISettingsRepository SettingsRepository { get; }
    IProductRepository ProductRepository { get; }
    IUsersRepository UsersRepository { get; }
    IClientRepository ClientRepository { get; }
    IDeliveryRepository DeliveryRepository { get; }
    IStockRepository StockRepository { get; }

    Task<bool> CommitAsync();
    bool HasChanges();
    Task<bool> RollbackAsync();
    
    /// <summary>
    /// Executes an operation with automatic retry logic for concurrency conflicts
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
    /// <param name="baseDelayMs">Base delay between retries in milliseconds (default: 100)</param>
    /// <returns>Result of the operation</returns>
    Task<Result<T>> ExecuteWithRetryAsync<T>(Func<Task<Result<T>>> operation, int maxRetries = 3, int baseDelayMs = 100);
    
    /// <summary>
    /// Clears the change tracker to reset entity state after concurrency conflicts
    /// </summary>
    void ResetContext();
}

public class UnitOfWork(DataContext ctx, IMapper mapper, ILogger<UnitOfWork> logger): IUnitOfWork
{
    public ISettingsRepository SettingsRepository { get; } = new SettingsRepository(ctx, mapper);
    public IProductRepository ProductRepository { get; } = new ProductRepository(ctx, mapper);
    public IUsersRepository UsersRepository { get; } = new UsersRepository(ctx, mapper);
    public IClientRepository ClientRepository { get; } = new ClientRepository(ctx, mapper);
    public IDeliveryRepository DeliveryRepository { get; } = new DeliveryRepository(ctx, mapper);
    public IStockRepository StockRepository { get; } = new StockRepository(ctx, mapper);

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

    public async Task<Result<T>> ExecuteWithRetryAsync<T>(Func<Task<Result<T>>> operation, int maxRetries = 3, int baseDelayMs = 100)
    {
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var result = await operation();

                // Do not retry if logical failure
                if (result.IsFailure)
                {
                    return result;
                }

                await CommitAsync();
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogWarning("Concurrency conflict on attempt {Attempt}/{MaxRetries}. Retrying...", 
                    attempt + 1, maxRetries);
                
                if (attempt == maxRetries - 1)
                {
                    logger.LogError(ex, "Failed after {MaxRetries} attempts due to concurrency conflicts", maxRetries);
                    return Result<T>.Failure("Operation failed after multiple attempts due to concurrency conflicts. Please try again.");
                }
                
                ResetContext();
                
                var delay = baseDelayMs * (int)Math.Pow(2, attempt);
                var jitter = Random.Shared.Next(0, delay / 4); // Add up to 25% jitter
                await Task.Delay(delay + jitter);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during operation on attempt {Attempt}", attempt + 1);
                return Result<T>.Failure($"Operation failed: {ex.Message}");
            }
        }
        
        return Result<T>.Failure("Operation failed after maximum retry attempts");
    }

    public void ResetContext()
    {
        ctx.ChangeTracker.Clear();
    }
}