using API.Entities;

namespace API.DTOs.Enum;

public enum DeliveryExportField
{
    /// <summary>
    /// <see cref="Delivery.Id"/>
    /// </summary>
    Id = 0,
    /// <summary>
    /// <see cref="Delivery.State"/>
    /// </summary>
    State = 1,
    /// <summary>
    /// <see cref="User.Id"/>
    /// </summary>
    FromId = 2,
    /// <summary>
    /// <see cref="User.Name"/>
    /// </summary>
    From = 3,
    /// <summary>
    /// <see cref="Client.Id"/>
    /// </summary>
    RecipientId = 4,
    /// <summary>
    /// <see cref="Client.Name"/>
    /// </summary>
    RecipientName = 5,
    /// <summary>
    /// <see cref="Client.InvoiceEmail"/>
    /// </summary>
    RecipientEmail = 6,
    /// <summary>
    /// <see cref="Client.CompanyNumber"/>
    /// </summary>
    CompanyNumber = 7,
    /// <summary>
    /// <see cref="Delivery.Message"/>
    /// </summary>
    Message = 8,
    /// <summary>
    /// <see cref="Delivery.Lines"/>
    /// All products accross exported deliveries will be added as headers. Each row will have 0 or n as value
    /// </summary>
    Products = 9,
    /// <summary>
    /// <see cref="Delivery.CreatedUtc"/>
    /// </summary>
    CreatedUtc = 10,
    /// <summary>
    /// <see cref="Delivery.LastModifiedUtc"/>
    /// </summary>
    LastModifiedUtc = 11,
}