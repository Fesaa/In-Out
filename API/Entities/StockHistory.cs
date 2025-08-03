using API.Entities.Enums;
using API.Entities.Interfaces;

namespace API.Entities;

public class StockHistory: IEntityDate
{
    
    public int Id { get; set; }
    
    public User User { get; set; }
    public int UserId { get; set; }
    
    public Stock Stock { get; set; }
    public int StockId { get; set; }
    
    public StockOperation Operation  { get; set; }
    public int Value { get; set; }
    
    /// <summary>
    /// Quantity before the operation
    /// </summary>
    public int QuantityBefore { get; set; }
    
    /// <summary>
    /// Quantity after the operation
    /// </summary>
    public int QuantityAfter { get; set; }
    
    /// <summary>
    /// Reference to related document (PO, Sale, etc.)
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Additional context for the operation
    /// </summary>
    public string? Notes { get; set; }


    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}