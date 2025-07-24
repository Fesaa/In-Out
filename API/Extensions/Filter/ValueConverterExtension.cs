using System.Globalization;
using API.DTOs.Filter;
using API.Entities.Enums;

namespace API.Extensions.Filter;

public static class ValueConverterExtension
{
    public static object Convert(this FilterStatementDto filterStatement)
    {
        return filterStatement.Field switch
        {
            FilterField.DeliveryState => ParseList(filterStatement.Value, Enum.Parse<DeliveryState>),
            FilterField.From => ParseList(filterStatement.Value, int.Parse),
            FilterField.Recipient => ParseList(filterStatement.Value, int.Parse),
            FilterField.Lines => int.Parse(filterStatement.Value),
            FilterField.Products => ParseList(filterStatement.Value, int.Parse),
            FilterField.Created => DateTime.Parse(filterStatement.Value, CultureInfo.InvariantCulture),
            FilterField.LastModified => DateTime.Parse(filterStatement.Value, CultureInfo.InvariantCulture),
            
            _ => throw new ArgumentException($"Invalid field type: {filterStatement.Field}"),
        };
    }

    private static List<T> ParseList<T>(string value, Func<string, T> parser)
    {
        return value.Split(",")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(parser)
            .ToList();
    }
}