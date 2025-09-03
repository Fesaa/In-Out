using API.Helpers;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.WithCaller;
using Serilog.Events;
using Serilog.Templates;

namespace API.Logging;

public static class LogLevelOptions
{
    private const string LogFile = "config/logs/In-Out.log";
    private static readonly LoggingLevelSwitch LogLevelSwitch = new ();
    private static readonly LoggingLevelSwitch MicrosoftLogLevelSwitch = new (LogEventLevel.Error);
    private static readonly LoggingLevelSwitch MicrosoftHostingLifetimeLogLevelSwitch = new (LogEventLevel.Error);
    private static readonly LoggingLevelSwitch AspNetCoreLogLevelSwitch = new (LogEventLevel.Error);

    public static LoggerConfiguration CreateConfig(LoggerConfiguration configuration)
    {
        var outputTemplate =
            $"[{BuildInfo.AppName}] [{{@t:yyyy-MM-dd HH:mm:ss.fff zzz}}] [{{@l}}] {{SourceContext}} {{@m:lj}}\n{{@x}}";
        return configuration
            .MinimumLevel.ControlledBy(LogLevelSwitch)
            .MinimumLevel.Override("Microsoft", MicrosoftLogLevelSwitch)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", MicrosoftHostingLifetimeLogLevelSwitch)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Internal.WebHost", AspNetCoreLogLevelSwitch)
            .MinimumLevel.Override("Microsoft.AspNetCore.ResponseCaching.ResponseCachingMiddleware", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
            .Enrich.FromLogContext()
            .Enrich.WithCaller()
            .WriteTo.Console(new ExpressionTemplate(outputTemplate))
            .WriteTo.File(LogFile, shared: true, rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
            .Filter.ByExcluding("RequestPath like '/metric%'")
            .Filter.ByIncludingOnly(ShouldIncludeLogStatement);
    }
    
    private static bool ShouldIncludeLogStatement(LogEvent e)
    {
        var isRequestLoggingMiddleware = e.Properties.ContainsKey("SourceContext") &&
                                         e.Properties["SourceContext"].ToString().Replace("\"", string.Empty) ==
                                         "Serilog.AspNetCore.RequestLoggingMiddleware";

        if (!isRequestLoggingMiddleware) return true;

        return LogLevelSwitch.MinimumLevel <= LogEventLevel.Information;
    }

    public static void SwitchLogLevel(LogEventLevel level)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                LogLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                MicrosoftLogLevelSwitch.MinimumLevel = LogEventLevel.Information;
                MicrosoftHostingLifetimeLogLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                AspNetCoreLogLevelSwitch.MinimumLevel = LogEventLevel.Information;
                break;
            case LogEventLevel.Debug:
                LogLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                MicrosoftLogLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                MicrosoftHostingLifetimeLogLevelSwitch.MinimumLevel = LogEventLevel.Information;
                AspNetCoreLogLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                break;
            case LogEventLevel.Information:
                LogLevelSwitch.MinimumLevel = LogEventLevel.Information;
                MicrosoftLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                MicrosoftHostingLifetimeLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                AspNetCoreLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                break;
            case LogEventLevel.Warning:
                LogLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                MicrosoftLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                MicrosoftHostingLifetimeLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                AspNetCoreLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                break;
            case LogEventLevel.Error:
                LogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                MicrosoftLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                MicrosoftHostingLifetimeLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                AspNetCoreLogLevelSwitch.MinimumLevel = LogEventLevel.Error;
                break;
            case LogEventLevel.Fatal:
                LogLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                MicrosoftLogLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                MicrosoftHostingLifetimeLogLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                AspNetCoreLogLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                break;
        }
    }
}