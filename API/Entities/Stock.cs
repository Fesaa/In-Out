using API.Entities.Interfaces;

namespace API.Entities;

public class Stock: IHasConcurrencyToken
{
    public int Id { get; set; }
    
    public Product Product { get; set; }
    public int ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    public IList<StockHistory>  History { get; set; } = [];

    public uint RowVersion { get; private set; }
    
    public void OnSavingChanges()
    {
        RowVersion++;
    }
}