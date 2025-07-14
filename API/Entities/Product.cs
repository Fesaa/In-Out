using API.Entities.Enums;

namespace API.Entities;

public class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public string NormalizedName { get; set; }
    
    public string Description { get; set; }
    
    public ProductCategory Category { get; set; }
    public ProductType  Type { get; set; }
    
    public bool IsTracked { get; set; }
    public bool Enabled { get; set; }
}