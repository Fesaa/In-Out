using System.ComponentModel;

namespace API.Entities.Enums;

public enum StockOperation
{
    [Description("Add")]
    Add = 0,
    [Description("Remove")]
    Remove = 1,
    /// <summary>
    /// Set the stock to an absolute number
    /// </summary>
    [Description("Set")]
    Set = 2,
}