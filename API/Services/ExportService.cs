using System.Text;
using System.Text.Json;
using API.DTOs;
using API.DTOs.Enum;
using API.Entities;
using API.Exceptions;
using API.Extensions;
using API.Helpers.Telemetry;
using API.Services.Exporters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Services;

public interface IExportService
{
    Task<string> Export(IList<Delivery> deliveries, ExportRequestDto request);
    Task<FileResult?> GetExport(string uuid);
}

public class ExportService: IExportService
{
    
    private readonly Dictionary<ExportKind, IExporter>  _exporters = [];
    private readonly ILogger<ExportService> _logger;
    private readonly IDistributedCache _cache;

    private readonly DistributedCacheEntryOptions _options = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
    };
    
    public ExportService(ILogger<ExportService> logger, IServiceProvider keyedServiceProvider, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
        
        foreach (var kind in Enum.GetValues<ExportKind>())
        {
            var exporter = keyedServiceProvider.GetRequiredKeyedService<IExporter>(kind.ToString());
            _exporters[kind] = exporter;
        }
    }
    
    public async Task<string> Export(IList<Delivery> deliveries, ExportRequestDto request)
    {
        if (!_exporters.TryGetValue(request.Kind, out var exporter))
        {
            throw new InOutException($"No exporter found for {request.Kind}");
        }

        using var tracker = TelemetryHelper.TrackOperation("export_deliveries", new Dictionary<string, object?>
        {
            ["kind"] = request.Kind.ToString(),
        });
        
        var (metadata, content) = await exporter.Export(deliveries, request);
        
        var uuid = Guid.NewGuid().ToString();
        await _cache.SetAsync($"{uuid}_content", content, _options);
        await _cache.SetAsJsonAsync($"{uuid}_metadata", metadata, _options);
        return uuid;
    }

    public async Task<FileResult?> GetExport(string uuid)
    {
        if (!Guid.TryParse(uuid, out _))
        {
            return null;
        }
        
        var metadata = await _cache.GetAsJsonAsync<ExportMetadata>($"{uuid}_metadata");
        if (metadata == null) return null;
        
        var fileContent = await _cache.GetAsync($"{uuid}_content");
        if (fileContent == null) return null;
        
        return new FileContentResult(fileContent, metadata.FileContentType)
        {
            FileDownloadName = metadata.FileName,
        };
    }
}