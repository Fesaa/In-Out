using API.DTOs.Enum;

namespace API.DTOs;

public class CsvExportConfigurationDto
{
    /// <summary>
    /// The order in which to export fields, if empty or unset exports in the natural enum order
    /// </summary>
    public IList<DeliveryExportField> HeaderOrder  { get; set; } = [];
}