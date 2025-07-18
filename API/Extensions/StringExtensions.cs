using System.Text.RegularExpressions;
using API.Entities;

namespace API.Extensions;

public static class StringExtensions
{
    private const RegexOptions MatchOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant;
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);
    
    private static readonly Regex NormalizeRegex = new(@"[^\p{L}0-9\+!＊！＋]",
        MatchOptions, RegexTimeout);

    public static string ToNormalized(this string? value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : NormalizeRegex.Replace(value, string.Empty).Trim().ToLower();
    }

    public static SystemMessage ToSystemMessage(this string value)
    {
        return new SystemMessage
        {
            Message = value,
            CreatedUtc = DateTime.UtcNow,
        };
    }
}