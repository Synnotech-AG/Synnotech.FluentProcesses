using System;
using System.Diagnostics;
using Light.GuardClauses;
using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Provides extension methods for logging with <see cref="Process" /> instances.
/// </summary>
public static class LoggingExtensions
{
    private static Action<ILogger, string, Exception?> LogTraceDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Trace, 0, "{Message}");
    
    private static Action<ILogger, string, Exception?> LogDebugDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Debug, 0, "{Message}");
    
    private static Action<ILogger, string, Exception?> LogInformationDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Information, 0, "{Message}");
    
    private static Action<ILogger, string, Exception?> LogWarningDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Warning, 0, "{Message}");

    private static Action<ILogger, string, Exception?> LogErrorDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Error, 0, "{Message}");
    
    private static Action<ILogger, string, Exception?> LogCriticalDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Critical, 0, "{Message}");
    
    private static void LogReceivedData(this ILogger logger, string message, LogLevel logLevel) 
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                LogTraceDelegate(logger, message, null);
                break;
            case LogLevel.Debug:
                LogDebugDelegate(logger, message, null);
                break;
            case LogLevel.Information:
                LogInformationDelegate(logger, message, null);
                break;
            case LogLevel.Warning:
                LogWarningDelegate(logger, message, null);
                break;
            case LogLevel.Error:
                LogErrorDelegate(logger, message, null);
                break;
            case LogLevel.Critical:
                LogCriticalDelegate(logger, message, null);
                break;
        }
    }

    /// <summary>
    /// <para>
    /// Enables logging on the specified <see cref="Process" /> instance according
    /// to the behaviors defined in <paramref name="loggingSettings" />.
    /// </para>
    /// <para>
    /// If you use this extension method without the usage of <see cref="FluentProcess" />,
    /// keep in mind that you might need to call <see cref="Process.BeginOutputReadLine" />
    /// and/or <see cref="Process.BeginErrorReadLine" /> after calling <see cref="Process.Start()" />,
    /// depending on the value of <see cref="LoggingSettings.StandardOutputLoggingBehavior" /> and
    /// <see cref="LoggingSettings.StandardErrorLoggingBehavior" />.
    /// </para> 
    /// </summary>
    /// <param name="process">The process instance whose output streams should be logged.</param>
    /// <param name="loggingSettings">The settings describing how logging should be performed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="process" /> is null.</exception>
    public static Process EnableLoggingIfNecessary(this Process process,
                                                   LoggingSettings loggingSettings)
    {
        process.MustNotBeNull();

        if (!loggingSettings.CheckIfLoggingIsEnabled())
            return process;

        var startInfo = process.StartInfo;
        var isLoggingAfterExit = false;
        if (loggingSettings.IsStandardOutputLoggingEnabled)
        {
            startInfo.RedirectStandardOutput = true;
            if (loggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogOnEvent)
            {
                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                        loggingSettings.GetLoggerOrThrow().LogReceivedData(e.Data, loggingSettings.StandardOutputLogLevel);
                };
            }
            else
            {
                isLoggingAfterExit = true;
            }
        }

        if (loggingSettings.IsStandardErrorLoggingEnabled)
        {
            startInfo.RedirectStandardError = true;
            if (loggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogOnEvent)
            {
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                        loggingSettings.GetLoggerOrThrow().LogReceivedData(e.Data, loggingSettings.StandardErrorLogLevel);
                };
            }
            else
            {
                isLoggingAfterExit = true;
            }
        }
        process.EnableRaisingEvents = true;

        if (!isLoggingAfterExit)
            return process;

        process.Exited += (_, _) =>
        {
            var logger = loggingSettings.GetLoggerOrThrow();
            if (loggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogAfterProcessExit && loggingSettings.StandardOutputLogLevel != LogLevel.None)
                logger.LogReceivedData(process.StandardOutput.ReadToEnd(), loggingSettings.StandardOutputLogLevel);

            if (loggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogAfterProcessExit && loggingSettings.StandardErrorLogLevel != LogLevel.None)
                logger.LogReceivedData(process.StandardError.ReadToEnd(), loggingSettings.StandardErrorLogLevel);
        };

        return process;
    }
}