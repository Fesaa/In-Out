using API.Entities;
using API.Entities.Enums;

namespace API.DTOs;

public class DeliveryDto
{
    public int Id { get; set; }
    public DeliveryState State { get; set; }
    
    public int FromId { get; set; }
    public UserDto From  { get; set; }
    
    public int ClientId { get; set; }
    public ClientDto Recipient { get; set; }
    
    public string Message { get; set; }
    /// <summary>
    /// Cannot be changed via the API
    /// </summary>
    public IList<SystemMessage> SystemMessages { get; set; }
    public IList<DeliveryLineDto> Lines { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}