namespace API.Helpers;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    public int PageNumber { get; init; } = 1;
    private readonly int _pageSize = MaxPageSize;

    public int PageSize
    {
        get => Math.Min(_pageSize, MaxPageSize);
        init => _pageSize = (value == 0) ? MaxPageSize : value;
    }

    public static readonly PaginationParams Default = new()
    {
        PageSize = 20,
        PageNumber = 1,
    };
}