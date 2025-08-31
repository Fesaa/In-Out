using API.DTOs.Enum;

namespace API.DTOs;

public class ExportRequestDto
{
    
    public ExportKind Kind { get; set; }
    
    public IList<int> DeliveryIds { get; set; }
    
}