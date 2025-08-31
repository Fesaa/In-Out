using System.Globalization;
using System.Text;
using API.Data;
using API.Entities;
using API.Exceptions;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;

namespace API.Services.Exporters;

public class CsvExportContext : ExportContext
{
    public IList<Product> SortedProducts  { get; init; }
}

public class CsvExporter(IUnitOfWork unitOfWork) : BaseExporter<CsvExportContext>(unitOfWork)
{
    protected override Task<CsvExportContext> ConstructContext(ExportContext ctx)
    {
        var sorted = ctx.AllProducts.Values.OrderBy(product => product.Id).ToList();

        return Task.FromResult(new CsvExportContext
        {
            Request = ctx.Request,
            Deliveries = ctx.Deliveries,
            AllProducts = ctx.AllProducts,
            AllProductCategories = ctx.AllProductCategories,
            SortedProducts = sorted,
        });
    }

    protected override async Task<FileResult?> ConstructExportFile(CsvExportContext ctx)
    {
        if (ctx.Request.CsvExportConfigurationDto == null)
        {
            throw new InOutException("CSV export configuration is missing");
        }
        
        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        foreach (var headerName in ctx.Request.CsvExportConfigurationDto.HeaderNames)
        {
            csv.WriteField(headerName);
        }
        
        await csv.NextRecordAsync();

        foreach (var delivery in ctx.Deliveries)
        {
            foreach (var header in ctx.Request.CsvExportConfigurationDto.HeaderOrder)
            {
                csv.WriteField(GetDeliveryFieldAsString(ctx, delivery, header));
            }
            
            await csv.NextRecordAsync();
        }

        var csvContent = writer.ToString();
        var bytes = Encoding.UTF8.GetBytes(csvContent);
    
        var fileName = $"deliveries_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
    
        return new FileContentResult(bytes, "text/csv")
        {
            FileDownloadName = fileName
        };
    }

    protected override string ExportLines(CsvExportContext ctx, IList<DeliveryLine> lines)
    {
        var builder = new StringBuilder();

        foreach (var quantity in ctx.SortedProducts.Select(product => lines.FirstOrDefault(line => line.ProductId == product.Id)?.Quantity ?? 0))
        {
            builder.Append(quantity).Append(',');
        }
        
        return builder.ToString();
    }
}