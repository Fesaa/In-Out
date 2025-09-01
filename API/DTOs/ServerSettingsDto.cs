using Serilog.Events;

namespace API.DTOs;

public sealed record ServerSettingsDto
{
    public CsvExportConfigurationDto  CsvExportConfiguration { get; set; }
    
    public LogEventLevel LogLevel { get; set; }   
}