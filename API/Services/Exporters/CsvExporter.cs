using System.Globalization;
using System.Text;
using API.Data;
using API.DTOs;
using API.DTOs.Enum;
using API.Entities;
using API.Entities.Enums;
using API.Exceptions;
using CsvHelper;

namespace API.Services.Exporters;

public class CsvExportContext : ExportContext
{
    public required IList<Product> SortedProducts  { get; init; }
    
    public required CsvExportConfigurationDto Configuration { get; init; }
    
    public CsvWriter Writer { get; set; }
}

public class CsvExporter(IUnitOfWork unitOfWork, ISettingsService settingsService) : BaseExporter<CsvExportContext>(unitOfWork)
{

    private static readonly IList<string> DefaultHeaderNames = Enum.GetNames<DeliveryExportField>();
    private static readonly IList<DeliveryExportField> DefaultExportFields = Enum.GetValues<DeliveryExportField>().ToList();
    
    protected override async Task<CsvExportContext> ConstructContext(ExportContext ctx)
    {
        var sorted = ctx.AllProducts.Values.OrderBy(product => product.Id).ToList();
        var config =
            await settingsService.GetSettingsAsync<CsvExportConfigurationDto>(ServerSettingKey.CsvExportConfiguration);

        return new CsvExportContext
        {
            Request = ctx.Request,
            Deliveries = ctx.Deliveries,
            AllProducts = ctx.AllProducts,
            AllProductCategories = ctx.AllProductCategories,
            SortedProducts = sorted,
            Configuration = config,
        };
    }

    protected override async Task<(ExportMetadata, byte[])> ConstructExportFile(CsvExportContext ctx)
    {
        if (ctx.Configuration == null)
        {
            throw new InOutException("CSV export configuration is missing");
        }
        
        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        ctx.Writer = csv;
        
        var headers = ctx.Configuration.HeaderNames.Count > 0 ? ctx.Configuration.HeaderNames : DefaultHeaderNames;
        var headerOrder = ctx.Configuration.HeaderOrder.Count > 0 ? ctx.Configuration.HeaderOrder : DefaultExportFields;

        var productPlace = headerOrder.IndexOf(DeliveryExportField.Products);

        var idx = 0;
        foreach (var headerName in headers)
        {
            if (idx != productPlace)
            {
                csv.WriteField(headerName);
            }
            else
            {
                foreach (var sortedProduct in ctx.SortedProducts)
                {
                    csv.WriteField(sortedProduct.Name);
                }
            }

            idx++;
        }
        
        await csv.NextRecordAsync();

        
        foreach (var delivery in ctx.Deliveries)
        {
            foreach (var header in headerOrder)
            {
                var s = GetDeliveryFieldAsString(ctx, delivery, header);
                if (!header.Equals(DeliveryExportField.Products))
                {
                    // Products are written as a side effect of GetDeliveryFieldAsString
                    csv.WriteField(s);
                }
            }
            
            await csv.NextRecordAsync();
        }

        var content = Encoding.UTF8.GetBytes(writer.ToString());
        var metadata = new ExportMetadata
        {
            FileContentType = "text/csv",
            FileName = $"deliveries_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        
        return (metadata, content);
    }

    protected override string ExportLines(CsvExportContext ctx, IList<DeliveryLine> lines)
    {
        foreach (var quantity in ctx.SortedProducts.Select(product => lines.FirstOrDefault(line => line.ProductId == product.Id)?.Quantity ?? 0))
        {
            ctx.Writer.WriteField(quantity);
        }
        
        return "";
    }
}