namespace API.DTOs.Filter;

public class SortOptions
{
    public SortField SortField { get; set; }
    public bool IsAscending { get; set; } = true;
}