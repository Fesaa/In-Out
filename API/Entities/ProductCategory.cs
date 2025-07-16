namespace API.Entities;

public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NormalizedName { get; set; }
    
    public bool Enabled { get; set; }
    public bool AutoCollapse { get; set; }
    public int SortValue { get; set; }
}