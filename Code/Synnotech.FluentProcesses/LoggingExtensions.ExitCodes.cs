using Light.GuardClauses;
using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses;

public static partial class LoggingExtensions
{
    [LoggerMessage(EventId = 91901,
                   EventName = "Synnotech.FluentProcesses - Log Exit Code Without Arguments",
                   Message = "Process \"{ProcessFile}\" exited with code {ExitCode}")]
    private static partial void LogExitCodeWithoutArguments(this ILogger logger,
                                                            LogLevel logLevel,
                                                            string processFile,
                                                            int exitCode);

    [LoggerMessage(EventId = 91902,
                   EventName = "Synnotech.FluentProcesses - Log Exit Code With Arguments",
                   Message = "Process \"{ProcessFile} {Arguments}\" exited with code {ExitCode}")]
    private static partial void LogExitCodeWithArguments(this ILogger logger,
                                                         LogLevel logLevel,
                                                         string processFile,
                                                         string arguments,
                                                         int exitCode);

    private static void LogExitCode(this ILogger logger,
                                    string processFileName,
                                    string? arguments,
                                    int exitCode,
                                    LogLevel logLevel)
    {
        if (arguments.IsNullOrWhiteSpace())
            LogExitCodeWithoutArguments(logger, logLevel, processFileName, exitCode);
        else
            LogExitCodeWithArguments(logger, logLevel, processFileName, arguments, exitCode);
    }
}