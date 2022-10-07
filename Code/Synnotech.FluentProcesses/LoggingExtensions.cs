using System;
using System.Diagnostics;
using Light.GuardClauses;
using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Provides extension methods for logging with <see cref="Process" /> instances.
/// </summary>
public static partial class LoggingExtensions
{
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
    public static void EnableLoggingIfNecessary(this Process process,
                                                LoggingSettings loggingSettings)
    {
        process.MustNotBeNull();

        if (!loggingSettings.CheckIfLoggingIsEnabled())
            return;

        var startInfo = process.StartInfo;
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
        }
    }

    /// <summary>
    /// Logs the standard output stream and/or the standard error stream
    /// if the logging settings are configured to do so.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="process" /> is null.</exception>
    public static void LogAfterExitIfNecessary(this Process process,
                                               LoggingSettings loggingSettings)
    {
        process.MustNotBeNull();

        if (loggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogAfterProcessExit &&
            loggingSettings.StandardOutputLogLevel != LogLevel.None)
        {
            loggingSettings.GetLoggerOrThrow()
                           .LogReceivedData(process.StandardOutput.ReadToEnd(),
                                            loggingSettings.StandardOutputLogLevel);
        }

        if (loggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogAfterProcessExit &&
            loggingSettings.StandardErrorLogLevel != LogLevel.None)
        {
            loggingSettings.GetLoggerOrThrow()
                           .LogReceivedData(process.StandardError.ReadToEnd(),
                                            loggingSettings.StandardErrorLogLevel);
        }
    }

    /// <summary>
    /// Logs the exit code of the specified <paramref name="process" />
    /// if the logging settings are configured to do so.
    /// </summary>
    /// <param name="process">The process that has already exited.</param>
    /// <param name="loggingSettings">The instance indicating how process logging should occur.</param>
    /// <param name="isExitCodeValid">
    /// The value indicating whether the exit code is valid. Based on this value,
    /// <see cref="LoggingSettings.ValidExitCodeLogLevel" /> or <see cref="LoggingSettings.InvalidExitCodeLogLevel" />
    /// will be used for logging.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="process" /> is null.</exception>
    public static void LogExitCodeIfNecessary(this Process process,
                                              LoggingSettings loggingSettings,
                                              bool isExitCodeValid = true)
    {
        process.MustNotBeNull();

        var logLevel = isExitCodeValid ?
                           loggingSettings.ValidExitCodeLogLevel :
                           loggingSettings.InvalidExitCodeLogLevel;
        if (logLevel == LogLevel.None)
            return;

        loggingSettings.GetLoggerOrThrow()
                       .LogExitCode(process.StartInfo.FileName,
                                    process.StartInfo.Arguments,
                                    process.ExitCode,
                                    logLevel);
    }
}