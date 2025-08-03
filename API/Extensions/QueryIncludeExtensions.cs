using API.Data.Repositories;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class QueryIncludeExtensions
{
    public static IQueryable<Delivery> Includes(this IQueryable<Delivery> queryable, DeliveryIncludes includes)
    {
        if (includes.HasFlag(DeliveryIncludes.Lines))
        {
            queryable = queryable.Include(d => d.Lines);
        }

        if (includes.HasFlag(DeliveryIncludes.Recipient))
        {
            queryable = queryable.Include(d => d.Recipient);
        }

        if (includes.HasFlag(DeliveryIncludes.From))
        {
            queryable = queryable.Include(d => d.From);
        }
        
        return queryable;
    }

    public static IQueryable<Stock> Includes(this IQueryable<Stock> queryable, StockIncludes includes)
    {
        if (includes.HasFlag(StockIncludes.Product))
        {
            queryable = queryable.Include(d => d.Product);
        }

        if (includes.HasFlag(StockIncludes.History))
        {
            queryable = queryable.Include(d => d.History)
                .ThenInclude(h => h.User);
        }
        
        return queryable;
    }
}