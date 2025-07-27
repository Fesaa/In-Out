using API.DTOs;
using API.DTOs.Filter;
using API.Entities;
using API.Entities.Enums;
using API.Extensions;
using API.Extensions.Filter;
using API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

[Flags]
public enum DeliveryIncludes
{
    None = 1 << 0,
    Lines = 1 << 1,
    Recipient = 1 << 2,
    From = 1 << 3,
    
    Complete = Lines | Recipient |  From,
}

public interface IDeliveryRepository
{
    public Task<IList<DeliveryDto>> GetDeliveries(FilterDto filter, PaginationParams pagination, DeliveryIncludes includes = DeliveryIncludes.None);
    public Task<Delivery?> GetDeliveryById(int deliveryId, DeliveryIncludes includes = DeliveryIncludes.None);
    public Task<DeliveryDto?> GetDelivery(int deliveryId, DeliveryIncludes includes = DeliveryIncludes.From);
    public Task<IList<Delivery>> GetDeliveriesForClient(int clientId, IList<DeliveryState> states, DeliveryIncludes includes = DeliveryIncludes.None);

    void Add(Delivery delivery);
    void Update(Delivery delivery);
    void Remove(Delivery delivery);
    void RemoveRange(IList<DeliveryLine> lines);
}

public class DeliveryRepository(DataContext ctx, IMapper mapper): IDeliveryRepository
{

    public async Task<IList<DeliveryDto>> GetDeliveries(FilterDto filter, PaginationParams pagination, DeliveryIncludes includes = DeliveryIncludes.None)
    {
        var filteredQuery = await CreateFilteredQueryable(filter, ctx.Deliveries.AsNoTracking());
        var deliveryIds = await filteredQuery
            .Select(d => d.Id)
            .ToListAsync();

        var includedQuery = ctx.Deliveries
            .AsNoTracking()
            .Where(d => deliveryIds.Contains(d.Id))
            .ApplySort(filter.SortOptions)
            .Take(filter.Limit <= 0 ? int.MaxValue : filter.Limit)
            .Includes(includes);

        return await includedQuery
            .ProjectTo<DeliveryDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Delivery?> GetDeliveryById(int deliveryId, DeliveryIncludes includes = DeliveryIncludes.None)
    {
        return await ctx.Deliveries
            .Where(d => d.Id == deliveryId)
            .Includes(includes)
            .FirstOrDefaultAsync();
    }

    public async Task<DeliveryDto?> GetDelivery(int deliveryId, DeliveryIncludes includes = DeliveryIncludes.From)
    {
        return mapper.Map<DeliveryDto>(await ctx.Deliveries
            .Where(d => d.Id == deliveryId)
            .Includes(includes)
            .FirstOrDefaultAsync());
    }

    public async Task<IList<Delivery>> GetDeliveriesForClient(int clientId, IList<DeliveryState> states, DeliveryIncludes includes = DeliveryIncludes.None)
    {
        return await ctx.Deliveries
            .Where(d => d.Recipient.Id == clientId && states.Contains(d.State))
            .Includes(includes)
            .ToListAsync();
    }

    public void Add(Delivery delivery)
    {
        ctx.Deliveries.Add(delivery).State = EntityState.Added;
    }
    
    public void Update(Delivery delivery)
    {
        ctx.Deliveries.Update(delivery).State = EntityState.Modified;
    }
    
    public void Remove(Delivery delivery)
    {
        ctx.Deliveries.Remove(delivery).State = EntityState.Deleted;
    }

    public void RemoveRange(IList<DeliveryLine> lines)
    {
        ctx.DeliveryLines.RemoveRange(lines);
    }

    private async Task<IQueryable<Delivery>> CreateFilteredQueryable(FilterDto filter, IQueryable<Delivery>? query = null)
    {
        query ??= ctx.Deliveries.AsNoTracking();
        
        if (filter.Statements.Count == 0) return query;
        
        var queries = filter.Statements
            .Select(s => BuildFilterStatement(s, query))
            .ToList();
        
        return filter.Combination == FilterCombination.And
            ? queries.Aggregate((q1, q2) => q1.Intersect(q2))
            : queries.Aggregate((q1, q2) => q1.Union(q2));
    }

    private static IQueryable<Delivery> BuildFilterStatement(FilterStatementDto filterStatement, IQueryable<Delivery> query)
    {
        var value = filterStatement.Convert();
        return filterStatement.Field switch
        {
            FilterField.DeliveryState => query.HasState(true, filterStatement.Comparison, (List<DeliveryState>) value),
            FilterField.From => query.HasFrom(true, filterStatement.Comparison, (List<int>)value),
            FilterField.Recipient => query.HasRecipient(true, filterStatement.Comparison, (List<int>)value),
            FilterField.Lines => query.HasLines(true, filterStatement.Comparison, (int)value),
            FilterField.Products => query.HasProducts(true, filterStatement.Comparison, (List<int>)value),
            FilterField.Created => query.HasCreated(true, filterStatement.Comparison, (DateTime)value),
            FilterField.LastModified => query.HasLastModified(true, filterStatement.Comparison, (DateTime)value),
            _ => throw new ArgumentException($"Invalid field type: {filterStatement.Field}"),
        };
    }
    
}