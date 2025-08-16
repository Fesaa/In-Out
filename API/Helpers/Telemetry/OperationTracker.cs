using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace API.Helpers.Telemetry;

public class OperationTracker: IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly Histogram<double> _histogram;
    private readonly Dictionary<string, object?> _tags;
    private bool _disposed;

    internal OperationTracker(Histogram<double> histogram, string operationName, Dictionary<string, object?>? tags)
    {
        _histogram = histogram;
        _tags = tags ?? [];
        _stopwatch = Stopwatch.StartNew();

        _tags["operation"] = operationName;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _stopwatch.Stop();
        _histogram.Record(_stopwatch.Elapsed.TotalMilliseconds, _tags.ToArray());
        _disposed = true;
    }
}