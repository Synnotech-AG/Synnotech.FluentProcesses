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
    private static Action<ILogger, string, Exception?> LogStandardOutputDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Information,
                                     new EventId(1, "FluentProcesses - Log Standard Output"),
                                     "{StandardOutput}");

    private static Action<ILogger, string, Exception?> LogStandardErrorDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Error,
                                     new EventId(2, "FluentProcesses - Log Error Output"),
                                     "{ErrorOutput}");

    private static void LogStandardOutput(this ILogger logger, string standardOutput) =>
        LogStandardOutputDelegate(logger, standardOutput, null);

    private static void LogStandardError(this ILogger logger, string standardError) =>
        LogStandardErrorDelegate(logger, standardError, null);

    /// <summary>
    /// Enables logging on the specified <see cref="Process" /> instance according
    /// to the behaviors defined in <paramref name="loggingSettings" />.
    /// </summary>
    /// <param name="process">The process instance whose output streams should be logged.</param>
    /// <param name="loggingSettings">The settings describing how logging should be performed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="process" /> is null.</exception>
    public static Process EnableLoggingIfNecessary(this Process process,
                                                   LoggingSettings loggingSettings)
    {
        process.MustNotBeNull();

        var startInfo = process.StartInfo;
        if (!loggingSettings.CheckIfLoggingIsEnabled())
            return process;

        var isLoggingAfterExit = false;
        if (loggingSettings.IsStandardOutputLoggingEnabled)
        {
            startInfo.RedirectStandardOutput = true;
            if (loggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogOnEvent)
                process.OutputDataReceived += (_, e) => loggingSettings.GetLoggerOrThrow().LogStandardOutput(e.Data);
            else
                isLoggingAfterExit = true;
        }

        if (loggingSettings.IsStandardErrorLoggingEnabled)
        {
            startInfo.RedirectStandardError = true;
            if (loggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogOnEvent)
                process.ErrorDataReceived += (_, e) => loggingSettings.GetLoggerOrThrow().LogStandardError(e.Data);
            else
                isLoggingAfterExit = true;
        }

        if (!isLoggingAfterExit)
            return process;

        process.EnableRaisingEvents = true;
        process.Exited += (_, _) =>
        {
            var logger = loggingSettings.GetLoggerOrThrow();
            if (loggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogAfterProcessExit)
                logger.LogStandardOutput(process.StandardOutput.ReadToEnd());

            if (loggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogAfterProcessExit)
                logger.LogStandardError(process.StandardError.ReadToEnd());
        };

        return process;
    }
}