using API.Entities.Enums;

namespace API.Entities;

public class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public string NormalizedName { get; set; }

    public string Description { get; set; }

    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; }
    public Dictionary<int, float> Prices { get; set; }
    /// <summary>
    /// This value is valid inside a category, not between
    /// </summary>
    public int SortValue { get; set; }
    public ProductType  Type { get; set; }

    public bool IsTracked { get; set; }
    public bool Enabled { get; set; }
}
