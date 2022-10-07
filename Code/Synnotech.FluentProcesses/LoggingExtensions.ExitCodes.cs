using System;
using Light.GuardClauses;
using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses;

public static partial class LoggingExtensions
{
    private static Action<ILogger, string, int, Exception?> LogExitCodeTrace { get; } =
        LoggerMessage.Define<string, int>(LogLevel.Trace, 0, "Process \"{ProcessFile}\" exited with code {ExitCode}");

    private static Action<ILogger, string, int, Exception?> LogExitCodeDebug { get; } =
        LoggerMessage.Define<string, int>(LogLevel.Debug, 0, "Process \"{ProcessFile}\" exited with code {ExitCode}");

    private static Action<ILogger, string, int, Exception?> LogExitCodeInformation { get; } =
        LoggerMessage.Define<string, int>(LogLevel.Information, 0, "Process \"{ProcessFile}\" exited with code {ExitCode}");

    private static Action<ILogger, string, int, Exception?> LogExitCodeWarning { get; } =
        LoggerMessage.Define<string, int>(LogLevel.Warning, 0, "Process \"{ProcessFile}\" exited with code {ExitCode}");

    private static Action<ILogger, string, int, Exception?> LogExitCodeError { get; } =
        LoggerMessage.Define<string, int>(LogLevel.Error, 0, "Process \"{ProcessFile}\" exited with code {ExitCode}");

    private static Action<ILogger, string, int, Exception?> LogExitCodeCritical { get; } =
        LoggerMessage.Define<string, int>(LogLevel.Critical, 0, "Process \"{ProcessFile}\" exited with code {ExitCode}");

    private static Action<ILogger, string, string, int, Exception?> LogExitCodeWithArgumentsTrace { get; } =
        LoggerMessage.Define<string, string, int>(LogLevel.Trace, 0, "Process \"{ProcessFile} {Arguments}\" exited with code {ExitCode}");

    private static Action<ILogger, string, string, int, Exception?> LogExitCodeWithArgumentsDebug { get; } =
        LoggerMessage.Define<string, string, int>(LogLevel.Debug, 0, "Process \"{ProcessFile} {Arguments}\" exited with code {ExitCode}");

    private static Action<ILogger, string, string, int, Exception?> LogExitCodeWithArgumentsInformation { get; } =
        LoggerMessage.Define<string, string, int>(LogLevel.Information, 0, "Process \"{ProcessFile} {Arguments}\" exited with code {ExitCode}");

    private static Action<ILogger, string, string, int, Exception?> LogExitCodeWithArgumentsWarning { get; } =
        LoggerMessage.Define<string, string, int>(LogLevel.Warning, 0, "Process \"{ProcessFile} {Arguments}\" exited with code {ExitCode}");

    private static Action<ILogger, string, string, int, Exception?> LogExitCodeWithArgumentsError { get; } =
        LoggerMessage.Define<string, string, int>(LogLevel.Error, 0, "Process \"{ProcessFile} {Arguments}\" exited with code {ExitCode}");

    private static Action<ILogger, string, string, int, Exception?> LogExitCodeWithArgumentsCritical { get; } =
        LoggerMessage.Define<string, string, int>(LogLevel.Critical, 0, "Process \"{ProcessFile} {Arguments}\" exited with code {ExitCode}");

    private static void LogExitCode(this ILogger logger,
                                    string processFileName,
                                    string? arguments,
                                    int exitCode,
                                    LogLevel logLevel)
    {
        if (arguments.IsNullOrWhiteSpace())
            LogExitCodeWithoutArguments(logger, processFileName, exitCode, logLevel);
        else
            LogExitCodeWithArguments(logger, processFileName, arguments, exitCode, logLevel);
    }

    private static void LogExitCodeWithoutArguments(this ILogger logger,
                                                    string processFileName,
                                                    int exitCode,
                                                    LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                LogExitCodeTrace(logger, processFileName, exitCode, null);
                break;
            case LogLevel.Debug:
                LogExitCodeDebug(logger, processFileName, exitCode, null);
                break;
            case LogLevel.Information:
                LogExitCodeInformation(logger, processFileName, exitCode, null);
                break;
            case LogLevel.Warning:
                LogExitCodeWarning(logger, processFileName, exitCode, null);
                break;
            case LogLevel.Error:
                LogExitCodeError(logger, processFileName, exitCode, null);
                break;
            case LogLevel.Critical:
                LogExitCodeCritical(logger, processFileName, exitCode, null);
                break;
        }
    }

    private static void LogExitCodeWithArguments(this ILogger logger,
                                                 string processFileName,
                                                 string arguments,
                                                 int exitCode,
                                                 LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                LogExitCodeWithArgumentsTrace(logger, processFileName, arguments, exitCode, null);
                break;
            case LogLevel.Debug:
                LogExitCodeWithArgumentsDebug(logger, processFileName, arguments, exitCode, null);
                break;
            case LogLevel.Information:
                LogExitCodeWithArgumentsInformation(logger, processFileName, arguments, exitCode, null);
                break;
            case LogLevel.Warning:
                LogExitCodeWithArgumentsWarning(logger, processFileName, arguments, exitCode, null);
                break;
            case LogLevel.Error:
                LogExitCodeWithArgumentsError(logger, processFileName, arguments, exitCode, null);
                break;
            case LogLevel.Critical:
                LogExitCodeWithArgumentsCritical(logger, processFileName, arguments, exitCode, null);
                break;
        }
    }
}