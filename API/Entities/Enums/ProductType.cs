using System.ComponentModel;

namespace API.Entities.Enums;

public enum ProductType
{
    /**
     * The product can be bought as many times as wanted in one delivery
     */
    [Description("Consumable")]
    Consumable = 0,
    /**
     * The product can only be bought once per delivery
     */
    [Description("OneTime")]
    OneTime = 1,
}