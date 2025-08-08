using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

[Flags]
public enum StockIncludes
{
    None = 0,
    Product = 1,
    History = 2,
}

public interface IStockRepository
{
    
    Task<IList<Stock>> GetAllAsync(StockIncludes includes = StockIncludes.None);
    Task<IList<StockDto>> GetAllDtoAsync(StockIncludes includes = StockIncludes.None);
    Task<Stock?> GetForProduct(int productId, StockIncludes includes = StockIncludes.None);
    Task<StockDto?> GetDtoForProduct(int productId, StockIncludes includes = StockIncludes.None);
    Task<Stock?> GetByIdAsync(int id, StockIncludes includes = StockIncludes.None);
    Task<IList<Stock>> GetByIdsAsync(IEnumerable<int> ids, StockIncludes includes = StockIncludes.None);
    Task<IList<Stock>> GetByProductIdsAsync(IEnumerable<int> ids, StockIncludes includes = StockIncludes.None);
    Task<StockDto?> GetDtoByIdAsync(int id, StockIncludes includes = StockIncludes.None);

    Task<IList<StockHistory>> GetHistory(int stockId);
    Task<IList<StockHistoryDto>> GetHistoryDto(int stockId);
    
    void Add(Stock stock);
    void Add(StockHistory stockHistory);
    void AddRange(IEnumerable<StockHistory>  stockHistory);
    void Update(Stock stock);
    void Update(StockHistory stockHistory);
    void UpdateRange(IEnumerable<StockHistory> stockHistory);
    void UpdateRange(IEnumerable<Stock> stockHistory);
    void Remove(Stock stock);
    void Remove(StockHistory stockHistory);
    void RemoveRange(IEnumerable<StockHistory> stockHistory);
    
    
}

public class StockRepository(DataContext ctx, IMapper mapper): IStockRepository
{
    public async Task<IList<Stock>> GetAllAsync(StockIncludes includes = StockIncludes.None)
    {
        return await ctx.ProductStock
            .Includes(includes)
            .ToListAsync();
    }

    public async Task<IList<StockDto>> GetAllDtoAsync(StockIncludes includes = StockIncludes.None)
    {
        return await ctx.ProductStock
            .Includes(includes)
            .ProjectTo<StockDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Stock?> GetForProduct(int productId, StockIncludes includes = StockIncludes.None)
    {
        return await ctx.ProductStock
            .Includes(includes)
            .Where(s => s.ProductId == productId)
            .FirstOrDefaultAsync();   
    }

    public async Task<StockDto?> GetDtoForProduct(int productId, StockIncludes includes = StockIncludes.None)
    {
        return await ctx.ProductStock
            .Includes(includes)
            .Where(s => s.ProductId == productId)
            .ProjectTo<StockDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();  
    }

    public async Task<Stock?> GetByIdAsync(int id, StockIncludes includes = StockIncludes.None)
    {
        return await ctx.ProductStock
            .Includes(includes)
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IList<Stock>> GetByIdsAsync(IEnumerable<int> ids, StockIncludes includes = StockIncludes.None)
    {
        return await ctx.ProductStock
            .Includes(includes)
            .Where(s =>  ids.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<IList<Stock>> GetByProductIdsAsync(IEnumerable<int> ids, StockIncludes includes = StockIncludes.None)
    {
        return await ctx.ProductStock
            .Includes(includes)
            .Where(s =>  ids.Contains(s.ProductId))
            .ToListAsync();
    }

    public async Task<StockDto?> GetDtoByIdAsync(int id, StockIncludes includes = StockIncludes.None)
    {
        return mapper.Map<StockDto>(await ctx.ProductStock
            .Includes(includes)
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync());
    }

    public async Task<IList<StockHistory>> GetHistory(int stockId)
    {
        return await ctx.StockHistory
            .Where(s => s.StockId == stockId)
            .ToListAsync();
    }

    public async Task<IList<StockHistoryDto>> GetHistoryDto(int stockId)
    {
        return await ctx.StockHistory
            .Where(s => s.StockId == stockId)
            .OrderByDescending(s => s.CreatedUtc)
            .ProjectTo<StockHistoryDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void Add(Stock stock)
    {
        ctx.ProductStock.Add(stock).State = EntityState.Added;
    }

    public void Add(StockHistory stockHistory)
    {
        ctx.StockHistory.Add(stockHistory).State = EntityState.Added;
    }

    public void AddRange(IEnumerable<StockHistory> stockHistory)
    {
        ctx.StockHistory.AddRange(stockHistory);
    }

    public void Update(Stock stock)
    {
        ctx.ProductStock.Update(stock).State = EntityState.Modified;
    }

    public void Update(StockHistory stockHistory)
    {
        ctx.StockHistory.Update(stockHistory).State = EntityState.Modified;
    }

    public void UpdateRange(IEnumerable<StockHistory> stockHistory)
    {
        ctx.StockHistory.UpdateRange(stockHistory);
    }

    public void UpdateRange(IEnumerable<Stock> stockHistory)
    {
        ctx.ProductStock.UpdateRange(stockHistory);
    }

    public void Remove(Stock stock)
    {
        ctx.ProductStock.Remove(stock).State = EntityState.Deleted;
    }

    public void Remove(StockHistory stockHistory)
    {
        ctx.StockHistory.Remove(stockHistory).State = EntityState.Deleted;
    }

    public void RemoveRange(IEnumerable<StockHistory> stockHistory)
    {
        ctx.StockHistory.RemoveRange(stockHistory);
    }
}