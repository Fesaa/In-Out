using API.Helpers;

namespace API.Extensions;

public static class QueryExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        return await PagedList<T>.CreateAsync(source, pageNumber, pageSize);
    }
}