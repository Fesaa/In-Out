namespace API.DTOs;

public class StockDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    
    public IList<StockHistoryDto> History { get; set; }
}