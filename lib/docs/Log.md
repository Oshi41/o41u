# Log

A simple, ready-to-use logging facility for the application built on top of Serilog.

The implementation lives in `lib/Services/Log.cs` as a static class `AppLog` that exposes:

- `ILogger Log` — the global Serilog logger instance you use across the app;
- `LoggerConfiguration Cfg` — the underlying configuration, should you need to tweak/extend it.

## Features

- Console logging with colored theme
- Daily rolling file logs at `/logs/app/{Date:yyyy-MM-dd}.log`
- Minimum level: Debug (with an internal level switch set to Verbose)
- 20 MB file size limit per rolling log file
- Enrichers:
  - ThreadId
  - Caller (method and file)
  - ApplicationName = "o41u"

## Output template

```
{Timestamp:yyyy-MMM-dd HH:mm:ss.fff} | {ThreadId} | {Level:u6} | {Caller} | {SourceContext}{Message}{NewLine}{Exception}
```

Example line:

```
2025-Aug-12 14:40:21.123 | 8 |  INFO | MyType.MyMethod() in MyType.cs:42 | Hello, World!
```

## Quick start

```csharp
using lib.Services;

class Program
{
    static void Main()
    {
        AppLog.Log.Information("App started at {At}", DateTimeOffset.Now);

        try
        {
            // your code
        }
        catch (Exception ex)
        {
            AppLog.Log.Error(ex, "Unhandled exception");
        }
    }
}
```

## Structured logging

Serilog encourages structured logs. Pass values as parameters rather than string concatenation:

```csharp
AppLog.Log.Information("User {UserId} placed order {OrderId}", userId, orderId);
```

## Changing configuration

If you need to extend or override sinks/enrichers, you can start from `AppLog.Cfg`:

```csharp
using Serilog;

var logger = AppLog.Cfg
    .WriteTo.Debug() // add more sinks
    .CreateLogger();

logger.Warning("This goes to existing sinks plus Debug sink");
```

Note: `AppLog.Log` stays unchanged; use the `logger` instance you created for customized behavior, or replace `AppLog.Log` in your own composition root if that suits your app design.

## File locations and limits

- Files are written to `/logs/app/{Date}.log` (relative to the process working directory)
- Each file is limited to 20 MB (`ByteSizes.FromMb(20)`) before rolling

Ensure the process has write permissions to the `logs` directory.

## Thread safety

Serilog loggers are thread-safe. You can safely call `AppLog.Log` from multiple threads.

## Dependencies

- Serilog
- Serilog.Enrichers.WithCaller
- Serilog.Sinks.SystemConsole

These are referenced in the library; make sure your application restores NuGet packages accordingly.
