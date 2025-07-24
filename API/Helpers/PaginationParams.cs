namespace API.Helpers;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int PageNumber { get; init; } = 1;

    public int PageSize
    {
        get => Math.Min(_pageSize, MaxPageSize);
        init => _pageSize = (value <= 0 ? MaxPageSize : value);
    }

    public static readonly PaginationParams Default = new()
    {
        PageSize = 20,
        PageNumber = 1,
    };
}