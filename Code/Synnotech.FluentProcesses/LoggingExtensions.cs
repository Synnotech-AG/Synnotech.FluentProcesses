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
        LoggerMessage.Define<string>(LogLevel.Information, 0, "{StandardOutput}");

    private static Action<ILogger, string, Exception?> LogStandardErrorDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Error, 0, "{ErrorOutput}");

    private static void LogStandardOutput(this ILogger logger, string standardOutput) =>
        LogStandardOutputDelegate(logger, standardOutput, null);

    private static void LogStandardError(this ILogger logger, string standardError) =>
        LogStandardErrorDelegate(logger, standardError, null);

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
                        loggingSettings.GetLoggerOrThrow().LogStandardOutput(e.Data);
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
                        loggingSettings.GetLoggerOrThrow().LogStandardError(e.Data);
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
            if (loggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogAfterProcessExit)
                logger.LogStandardOutput(process.StandardOutput.ReadToEnd());

            if (loggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogAfterProcessExit)
                logger.LogStandardError(process.StandardError.ReadToEnd());
        };

        return process;
    }
}