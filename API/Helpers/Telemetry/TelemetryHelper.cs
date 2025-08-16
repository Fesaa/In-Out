using System.Diagnostics;
using System.Diagnostics.Metrics;
using Flurl.Util;

namespace API.Helpers.Telemetry;

public static class TelemetryHelper
{

    private static readonly ActivitySource ActivitySource;
    private static readonly Meter Meter;

    private static readonly Histogram<double> MethodTiming;
    private static readonly Counter<long> InvalidOperationCounts;

    static TelemetryHelper()
    {
        var serviceName = BuildInfo.AppName;

        ActivitySource = new ActivitySource(serviceName);
        Meter = new Meter(serviceName);

        MethodTiming = Meter.CreateHistogram<double>(
            "method_timing_duration_ms",
            "ms",
            "Duration of specific method, see method tag"
            );

        InvalidOperationCounts = Meter.CreateCounter<long>(
            "invalid_operations_counter",
            "absolute",
            "Amount of invalid operations performed by users");
    }

    public static OperationTracker TrackOperation(string operationName, Dictionary<string, object?>? tags = null)
    {
        return new OperationTracker(MethodTiming, operationName, tags);
    }

    public static void InvalidOperation(string operationName, Dictionary<string, object?>? tags = null)
    {
        tags ??= [];
        tags.Add("operation", operationName);
        InvalidOperationCounts.Add(1, tags.ToArray());
    }
}