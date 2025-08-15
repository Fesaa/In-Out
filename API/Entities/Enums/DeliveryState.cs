using System.ComponentModel;

namespace API.Entities.Enums;

public enum DeliveryState
{
    /// <summary>
    /// The delivery is still being worked on
    /// </summary>
    [Description("In Progress")]
    InProgress,
    /// <summary>
    /// The delivery is completed, and not expected to change. But could forcefully be changed to <see cref="InProgress"/> again
    /// </summary>
    [Description("Completed")]
    Completed,
    /// <summary>
    /// The delivery has been handled, and can no longer be changed again
    /// </summary>
    [Description("Handled")]
    Handled,
    /// <summary>
    /// The delivery has been cancelled, all items will be returned to stock. Cannot be changed
    /// </summary>
    [Description("Cancelled")]
    Cancelled,
}