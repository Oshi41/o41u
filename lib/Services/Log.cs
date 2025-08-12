using lib.Model;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.WithCaller;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace lib.Services;

public static class AppLog
{
    public static ILogger Log { get; }
    public static LoggerConfiguration Cfg { get; }

    static AppLog()
    {
        var switcher = new LoggingLevelSwitch(LogEventLevel.Verbose);
        var fileDateFormat = "{Date:yyyy-MM-dd}";
        var template =
            "{Timestamp:yyyy-MMM-dd HH:mm:ss.fff} | {ThreadId} | {Level:u6} | {Caller} | {SourceContext}{Message}{NewLine}{Exception}";
        
        Cfg = new LoggerConfiguration().MinimumLevel.Debug()
            .WriteTo.RollingFile(
                pathFormat:$"/logs/app/{fileDateFormat}.log",
                switcher.MinimumLevel,
                template, 
                levelSwitch: switcher,
                fileSizeLimitBytes: ByteSizes.FromMb(20)
                )
            .WriteTo.Console(
                switcher.MinimumLevel,
                template,
                levelSwitch: switcher,
                theme: SystemConsoleTheme.Colored)
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ApplicationName", "o41u")
            .Enrich.WithCaller();
        
        Log = Cfg.CreateLogger();
    }
}