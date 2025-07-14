namespace API.Entities;

public static class QueryableExtensions
{

    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Predicate<T> predicate)
    {
        return condition ? query.Where(t => predicate(t)) : query;
    }
    
}