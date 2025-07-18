namespace API.Entities;

public class DeliveryLine
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }
    public Delivery Delivery { get; set; }
    
    public int ProductId { get; set; }
    public int Quantity { get; set; }

}