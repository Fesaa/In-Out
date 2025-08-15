namespace API.Extensions;

public static class LinqExtensions
{

    public static IEnumerable<T> RequireNotNull<T>(this IEnumerable<T?> enumerable)
    {
        return enumerable.Where(e => e != null)!;
    }

}