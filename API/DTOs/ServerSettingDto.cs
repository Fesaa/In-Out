using Serilog.Events;

namespace API.DTOs;

public sealed record ServerSettingDto
{
    public CsvExportConfigurationDto  CsvExportConfiguration { get; set; }
    
    public LogEventLevel LogLevel { get; set; }   
}