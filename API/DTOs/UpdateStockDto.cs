using API.Entities.Enums;

namespace API.DTOs;

public class UpdateStockDto
{
    public int ProductId { get; set; }
    public StockOperation Operation { get; set; }
    public int Value { get; set; }
    
    public string? Notes { get; set; }
    public string? Reference { get; set; }
    
}