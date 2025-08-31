using API.DTOs;
using API.DTOs.Enum;
using API.Entities;
using API.Exceptions;
using API.Helpers.Telemetry;
using API.Services.Exporters;
using Microsoft.AspNetCore.Mvc;

namespace API.Services;

public class ExportService: IExporter
{
    
    private readonly Dictionary<ExportKind, IExporter>  _exporters = [];

    public ExportService(ILogger<ExportService> logger, IServiceProvider keyedServiceProvider)
    {
        foreach (var kind in Enum.GetValues<ExportKind>())
        {
            var exporter = keyedServiceProvider.GetRequiredKeyedService<IExporter>(kind.ToString());
            _exporters[kind] = exporter;
        }
    }
    
    public async Task<FileResult?> Export(IList<Delivery> deliveries, ExportRequestDto request)
    {
        if (!_exporters.TryGetValue(request.Kind, out var exporter))
        {
            throw new InOutException($"No exporter found for {request.Kind}");
        }

        using var tracker = TelemetryHelper.TrackOperation("export_deliveries", new Dictionary<string, object?>
        {
            ["kind"] = request.Kind.ToString(),
        });
        
        return await exporter.Export(deliveries, request);
    }
}