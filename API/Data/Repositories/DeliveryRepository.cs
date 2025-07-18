using API.DTOs;
using API.DTOs.Filter;
using API.Entities;
using API.Entities.Enums;
using API.Extensions;
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
    public Task<PagedList<DeliveryDto>> GetDeliveries(FilterDto filter, PaginationParams pagination, DeliveryIncludes includes = DeliveryIncludes.None);
    public Task<Delivery?> GetDeliveryById(int deliveryId, DeliveryIncludes includes = DeliveryIncludes.None);
    public Task<DeliveryDto?> GetDelivery(int deliveryId, DeliveryIncludes includes = DeliveryIncludes.From);
    public Task<IList<Delivery>> GetDeliveriesForClient(int clientId, IList<DeliveryState> states, DeliveryIncludes includes = DeliveryIncludes.None);

    void Add(Delivery delivery);
    void Update(Delivery delivery);
    void Remove(Delivery delivery);
}

public class DeliveryRepository(DataContext ctx, IMapper mapper): IDeliveryRepository
{

    public async Task<PagedList<DeliveryDto>> GetDeliveries(FilterDto filter, PaginationParams pagination, DeliveryIncludes includes = DeliveryIncludes.None)
    {
        var includedQuery = ctx.Deliveries.AsNoTracking().Includes(includes);
        var query = await CreateFilteredQueryable(filter, includedQuery);
        
        return await query.ToPagedListAsync(pagination.PageNumber, pagination.PageSize);
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

    private async Task<IQueryable<DeliveryDto>> CreateFilteredQueryable(FilterDto filter, IQueryable<Delivery>? query = null)
    {
        query ??= ctx.Deliveries.AsNoTracking();
        
        return query.ProjectTo<DeliveryDto>(mapper.ConfigurationProvider);
    }
    
}