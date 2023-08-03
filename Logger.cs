using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole.Themes;
using SerilogTimings.Extensions;

public class Logger
{
    private Serilog.Core.Logger? _logger;
    private LoggerConfiguration _loggerConfiguration = new LoggerConfiguration();

    public void ConfigureFileLogger(
        string path,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        string outputTemplate = "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        IFormatProvider? formatProvider = null,
        long? fileSizeLimitBytes = 1073741824,
        LoggingLevelSwitch? levelSwitch = null,
        bool buffered = false,
        bool shared = false,
        TimeSpan? flushToDiskInterval = null,
        RollingInterval rollingInterval = RollingInterval.Infinite,
        bool rollOnFileSizeLimit = false,
        int? retainedFileCountLimit = 31,
        Encoding? encoding = null,
        FileLifecycleHooks? hooks = null,
        TimeSpan? retainedFileTimeLimit = null)
    {
        _loggerConfiguration.WriteTo.File(path,
            restrictedToMinimumLevel,
            outputTemplate,
            formatProvider,
            fileSizeLimitBytes,
            levelSwitch,
            buffered,
            shared,
            flushToDiskInterval,
            rollingInterval,
            rollOnFileSizeLimit,
            retainedFileCountLimit,
            encoding,
            hooks,
            retainedFileTimeLimit);
    }

    public void ConfigureConsole(
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        string outputTemplate = "[{Timestamp:dd-MM-yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        IFormatProvider? formatProvider = null,
        LoggingLevelSwitch? levelSwitch = null,
        LogEventLevel? standardErrorFromLevel = null,
        ConsoleTheme? theme = null,
        bool applyThemeToRedirectedOutput = false,
        object? syncRoot = null)
    {
        _loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel,
            outputTemplate,
            formatProvider,
            levelSwitch,
            standardErrorFromLevel,
            theme,
            applyThemeToRedirectedOutput,
            syncRoot);
    }

    public void CreateLogger() => _logger = _loggerConfiguration.CreateLogger();

    public void Log(LogEventLevel level, String messageTemplate)
    {
        ArgumentNullException.ThrowIfNull(_logger);
        _logger.Write(level, messageTemplate);
    }

    public void LogExecutionTime(Action method, String readableDescription = "")
    {
        ArgumentNullException.ThrowIfNull(_logger);
        string description = $"Execution time for {method.Method.Name}";
        if (!String.IsNullOrEmpty(readableDescription.Trim()))
            description = readableDescription.Trim();
        using (_logger.TimeOperation(description))
        {
            method();
        }
    }

    public void LogException(LogEventLevel level, Exception ex, String messageTemplate)
    {
        ArgumentNullException.ThrowIfNull(_logger);
        _logger.Write(level, ex, messageTemplate);
    }
}