namespace API.DTOs.Filter;

public enum FilterComparison
{
    Contains = 0,
    NotContains = 9,
    Equals = 1,
    NotEquals = 2,
    StartsWith = 3,
    EndsWith = 4,
    GreaterThan = 5,
    GreaterThanOrEquals = 6,
    LessThan = 7,
    LessThanOrEquals = 8,
}