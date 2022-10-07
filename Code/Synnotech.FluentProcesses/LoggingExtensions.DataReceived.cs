using System;
using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses;

public static partial class LoggingExtensions
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
}