namespace API.DTOs;

public class ProductCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public bool AutoCollapse { get; set; }
    public int SortValue { get; set; }
}