using API.Entities.Enums;
using API.Entities.Interfaces;

namespace API.Entities;

public class Delivery: IEntityDate
{
    public int Id { get; set; }
    public DeliveryState State { get; set; }
    
    public int UserId { get; set; }
    public User From { get; set; }
    public Client Recipient { get; set; }
    
    public string Message { get; set; }
    public IList<SystemMessage> SystemMessages { get; set; }
    public IList<DeliveryLine> Lines { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}