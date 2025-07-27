namespace API.DTOs.Filter;

public class FilterDto
{
    public ICollection<FilterStatementDto> Statements { get; set; } = [];
    public FilterCombination Combination { get; set; } = FilterCombination.And;
    public SortOptions SortOptions { get; set; }
    public int Limit { get; set; } = 0;
}