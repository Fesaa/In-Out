using API.Entities.Interfaces;

namespace API.Entities;

public class Stock: IHasConcurrencyToken
{
    public int Id { get; set; }
    
    public Product Product { get; set; }
    public int ProductId { get; set; }
    
    /// <summary>
    /// Will default the to name of the product
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description to prevent confusion
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public IList<StockHistory>  History { get; set; } = [];

    public uint RowVersion { get; private set; }
    
    public void OnSavingChanges()
    {
        RowVersion++;
    }
}