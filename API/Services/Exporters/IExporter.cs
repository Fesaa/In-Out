using System.Globalization;
using API.Data;
using API.DTOs;
using API.DTOs.Enum;
using API.Entities;

namespace API.Services.Exporters;

public interface IExporter
{
    Task<(ExportMetadata, byte[])> Export(IList<Delivery> deliveries, ExportRequestDto request);
}

public class ExportMetadata
{
    public string FileName { get; set; }
    public string FileContentType  { get; set; }
}

public class ExportContext
{
    public required ExportRequestDto Request { get; init; }
    
    public required IList<Delivery> Deliveries  { get; init; }
    
    public required Dictionary<int, Product> AllProducts { get; init; }
    
    public required Dictionary<int, ProductCategory> AllProductCategories { get; init; }
}

public abstract class BaseExporter<T>(IUnitOfWork unitOfWork) : IExporter where T : ExportContext
{
    public async Task<(ExportMetadata, byte[])> Export(IList<Delivery> deliveries, ExportRequestDto request)
    {
        var products = await unitOfWork.ProductRepository.GetAll();
        var categories = await unitOfWork.ProductRepository.GetAllCategories();

        var ctx = new ExportContext
        {
            Request = request,
            Deliveries = deliveries,
            AllProducts = products.ToDictionary(x => x.Id, x => x),
            AllProductCategories = categories.ToDictionary(x => x.Id, x => x),
        };
        
        
        return await ConstructExportFile(await ConstructContext(ctx));
    }
    
    protected abstract Task<T> ConstructContext(ExportContext ctx);
    protected abstract Task<(ExportMetadata, byte[])> ConstructExportFile(T ctx);
    protected abstract string ExportLines(T ctx, IList<DeliveryLine> lines);

    protected object GetDeliveryField(T ctx, Delivery delivery, DeliveryExportField field)
    {
        return field switch
        {
            DeliveryExportField.Id => delivery.Id,
            DeliveryExportField.State => delivery.State,
            DeliveryExportField.FromId => delivery.From.Id,
            DeliveryExportField.From => delivery.From.Name,
            DeliveryExportField.RecipientId => delivery.Recipient.Id,
            DeliveryExportField.RecipientName => delivery.Recipient.Name,
            DeliveryExportField.RecipientEmail => delivery.Recipient.InvoiceEmail,
            DeliveryExportField.CompanyNumber => delivery.Recipient.CompanyNumber,
            DeliveryExportField.Message => delivery.Message,
            DeliveryExportField.Products => delivery.Lines.Select(l => new {l.ProductId, l.Quantity}),
            DeliveryExportField.CreatedUtc =>  delivery.CreatedUtc,
            DeliveryExportField.LastModifiedUtc  =>  delivery.LastModifiedUtc,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
    }

    protected string GetDeliveryFieldAsString(T ctx, Delivery delivery, DeliveryExportField field)
    {
        return field switch
        {
            DeliveryExportField.Id => delivery.Id.ToString(),
            DeliveryExportField.State => delivery.State.ToString(),
            DeliveryExportField.FromId => delivery.From.Id.ToString(),
            DeliveryExportField.From => delivery.From.Name,
            DeliveryExportField.RecipientId => delivery.Recipient.Id.ToString(),
            DeliveryExportField.RecipientName => delivery.Recipient.Name,
            DeliveryExportField.RecipientEmail => delivery.Recipient.InvoiceEmail,
            DeliveryExportField.CompanyNumber => delivery.Recipient.CompanyNumber,
            DeliveryExportField.Products => ExportLines(ctx, delivery.Lines),
            DeliveryExportField.Message => delivery.Message,
            DeliveryExportField.CreatedUtc =>  delivery.CreatedUtc.ToString(CultureInfo.InvariantCulture),
            DeliveryExportField.LastModifiedUtc  =>  delivery.LastModifiedUtc.ToString(CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
    }

}