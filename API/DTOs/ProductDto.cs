using API.Entities.Enums;

namespace API.DTOs;

public sealed record ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int SortValue { get; set; }
    public ProductType Type { get; set; }
    public bool IsTracked { get; set; }
    public bool Enabled { get; set; }
    public Dictionary<int, float> Prices { get; set; }
}
