using API.DTOs.Filter;
using API.Entities;
using API.Entities.Enums;

namespace API.Extensions.Filter;

public static class DeliveryFilter
{

    public static IQueryable<Delivery> ApplySort(this IQueryable<Delivery> query, SortOptions options)
    {
        query = options.SortField switch
        {
            SortField.From => query.OrderBy(d => d.From.NormalizedName),
            SortField.Recipient => query.OrderBy(d => d.Recipient.NormalizedName),
            SortField.CreationDate => query.OrderBy(d => d.CreatedUtc),
            _ => query,
        };
        
        if (!options.IsAscending)
        {
            query = query.Reverse();
        }
        

        return query;
    }

    public static IQueryable<Delivery> HasState(this IQueryable<Delivery> query, bool guard, FilterComparison comparison, List<DeliveryState> states)
    {
        if (!guard) return query;

        return comparison switch
        {
            FilterComparison.Equals => query.Where(d => d.State == states[0]),
            FilterComparison.NotEquals => query.Where(d => d.State != states[0]),
            FilterComparison.Contains => query.Where(d => states.Contains(d.State)),
            FilterComparison.NotContains => query.Where(d => !states.Contains(d.State)),
            _ => throw new ArgumentException($"Invalid comparison type: {comparison}"),
        };
    }

    public static IQueryable<Delivery> HasFrom(this IQueryable<Delivery> query, bool guard, FilterComparison comparison, List<int> ids)
    {
        if (!guard) return query;

        return comparison switch
        {
            FilterComparison.Contains => query.Where(d => ids.Contains(d.UserId)),
            FilterComparison.NotContains => query.Where(d => !ids.Contains(d.UserId)),
            FilterComparison.Equals => query.Where(d => d.UserId == ids[0]),
            FilterComparison.NotEquals =>  query.Where(d => d.UserId != ids[0]),
            _ => throw new ArgumentException($"Invalid comparison type: {comparison}"),
        };
    }
    
    public static IQueryable<Delivery> HasRecipient(this IQueryable<Delivery> query, bool guard, FilterComparison comparison, List<int> ids)
    {
        if (!guard) return query;

        return comparison switch
        {
            FilterComparison.Contains => query.Where(d => ids.Contains(d.Recipient.Id)),
            FilterComparison.NotContains =>  query.Where(d => !ids.Contains(d.Recipient.Id)),
            FilterComparison.Equals => query.Where(d => d.Recipient.Id == ids[0]),
            FilterComparison.NotEquals => query.Where(d => !ids.Contains(d.Recipient.Id)),
            _ => throw new ArgumentException($"Invalid comparison type: {comparison}"),
        };
    }
    
    public static IQueryable<Delivery> HasLines(this IQueryable<Delivery> query, bool guard, FilterComparison comparison, int lineCount)
    {
        if (!guard) return query;

        return comparison switch
        {
            FilterComparison.Equals => query.Where(d => d.Lines.Count == lineCount),
            FilterComparison.NotEquals => query.Where(d => d.Lines.Count != lineCount),
            FilterComparison.GreaterThan =>  query.Where(d => d.Lines.Count > lineCount),
            FilterComparison.GreaterThanOrEquals => query.Where(d => d.Lines.Count >= lineCount),
            FilterComparison.LessThan =>   query.Where(d => d.Lines.Count < lineCount),
            FilterComparison.LessThanOrEquals => query.Where(d => d.Lines.Count <= lineCount),
            _ => throw new ArgumentException($"Invalid comparison type: {comparison}"),
        };
    }
    
    public static IQueryable<Delivery> HasProducts(this IQueryable<Delivery> query, bool guard, FilterComparison comparison, List<int> ids)
    {
        if (!guard) return query;

        return comparison switch
        {
            FilterComparison.Contains => query.Where(d => d.Lines.Any(l => ids.Contains(l.ProductId))),
            FilterComparison.NotContains =>  query.Where(d => d.Lines.Any(l => !ids.Contains(l.ProductId))),
            FilterComparison.Equals =>  query.Where(d => d.Lines.Any(l => l.ProductId == ids[0])),
            FilterComparison.NotEquals => query.Where(d => d.Lines.Any(l => !ids.Contains(l.ProductId))),
            _ => throw new ArgumentException($"Invalid comparison type: {comparison}"),
        };
    }
    
    public static IQueryable<Delivery> HasCreated(this IQueryable<Delivery> query, bool guard, FilterComparison comparison, DateTime created)
    {
        if (!guard) return query;
        
        var createdDate = created.Date;
        return comparison switch
        {
            FilterComparison.Equals => query.Where(d => d.CreatedUtc.Date == createdDate),
            FilterComparison.NotEquals => query.Where(d => d.CreatedUtc.Date != createdDate),
            FilterComparison.GreaterThan => query.Where(d => d.CreatedUtc > created),
            FilterComparison.GreaterThanOrEquals =>  query.Where(d => d.CreatedUtc >= created),
            FilterComparison.LessThan =>  query.Where(d => d.CreatedUtc < created),
            FilterComparison.LessThanOrEquals =>  query.Where(d => d.CreatedUtc <= created),
            _ => throw new ArgumentException($"Invalid comparison type: {comparison}"),
        };
    }
    
    public static IQueryable<Delivery> HasLastModified(this IQueryable<Delivery> query, bool guard, FilterComparison comparison, DateTime lastModified)
    {
        if (!guard) return query;

        var lastModifiedDate = lastModified.Date;
        return comparison switch
        {
            FilterComparison.Equals => query.Where(d => d.LastModifiedUtc.Date == lastModifiedDate),
            FilterComparison.NotEquals => query.Where(d => d.LastModifiedUtc.Date != lastModifiedDate),
            FilterComparison.GreaterThan => query.Where(d => d.LastModifiedUtc > lastModified),
            FilterComparison.GreaterThanOrEquals =>  query.Where(d => d.LastModifiedUtc >= lastModified),
            FilterComparison.LessThan =>  query.Where(d => d.LastModifiedUtc < lastModified),
            FilterComparison.LessThanOrEquals =>  query.Where(d => d.LastModifiedUtc <= lastModified),
            _ => throw new ArgumentException($"Invalid comparison type: {comparison}"),
        };
    }
    
}