using API.Helpers;

namespace API.Extensions;

public static class QueryExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        return await PagedList<T>.CreateAsync(source, pageNumber, pageSize);
    }

    public static IQueryable<TSource> SortBy<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> sortFunc, bool isAscending)
    {
        return isAscending ? source.OrderBy(e => sortFunc.Invoke(e)) : source.OrderByDescending(e => sortFunc.Invoke(e));

    }
}