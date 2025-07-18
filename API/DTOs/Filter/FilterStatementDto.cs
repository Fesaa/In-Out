namespace API.DTOs.Filter;

public class FilterStatementDto
{
    public FilterComparison Comparison { get; set; }
    public FilterField Field { get; set; }
    public string Value { get; set; }
}