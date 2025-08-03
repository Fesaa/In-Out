using API.Entities.Enums;

namespace API.DTOs;

public class StockHistoryDto
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public int UserId { get; set; }
    
    public StockOperation Operation { get; set; }
    public int Value { get; set; }
    
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    
}