using System.Reflection;

namespace API.Helpers;

public class BuildInfo
{
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    public static string AppName { get; } = Assembly.GetExecutingAssembly().GetName().Name ?? "In-Out";
}