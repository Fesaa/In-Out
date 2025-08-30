using System.ComponentModel;

namespace API.Entities.Enums;

public enum ServerSettingKey
{
    /**
     * Json object of type <see cref="DTOs.CsvExportConfigurationDto"/>
     */
    [Description("Csv Export Configuration")]
    CsvExportConfiguration = 0,
    [Description("Log Level")]
    LogLevel = 1,
}