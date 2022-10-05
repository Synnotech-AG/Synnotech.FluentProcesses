using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Represents the logging settings for a <see cref="Process" /> instance.
/// </summary>
/// <param name="Logger">The logger that will receive the messages.</param>
/// <param name="StandardOutputLoggingBehavior">The logging behavior that will be applied to the standard output stream.</param>
/// <param name="StandardErrorLoggingBehavior">The logging behavior that will be applied to the standard error stream.</param>
/// <param name="StandardOutputLogLevel">The log level that is used when logging standard output messages.</param>
/// <param name="StandardErrorLogLevel">The log level that is used when logging standard error messages.</param>
public readonly record struct LoggingSettings(ILogger? Logger,
                                              LoggingBehavior StandardOutputLoggingBehavior = LoggingBehavior.NoLogging,
                                              LoggingBehavior StandardErrorLoggingBehavior = LoggingBehavior.NoLogging,
                                              LogLevel StandardOutputLogLevel = LogLevel.Information,
                                              LogLevel StandardErrorLogLevel = LogLevel.Error)
{
    /// <summary>
    /// Gets the value indicating whether the
    /// Standard Output logging behavior is not <see cref="LoggingBehavior.NoLogging" />.  
    /// </summary>
    public bool IsStandardOutputLoggingEnabled =>
        StandardOutputLoggingBehavior != LoggingBehavior.NoLogging;

    /// <summary>
    /// Gets the value indicating whether the
    /// Standard Error logging behavior is not <see cref="LoggingBehavior.NoLogging" />.
    /// </summary>
    public bool IsStandardErrorLoggingEnabled =>
        StandardErrorLoggingBehavior != LoggingBehavior.NoLogging;
    
    /// <summary>
    /// Gets the value indicating whether any logging
    /// behavior is enabled (i.e. at least one of the logging behavior
    /// enum values is not <see cref="LoggingBehavior.NoLogging" />).
    /// </summary>
    public bool IsLoggingEnabled =>
        IsStandardOutputLoggingEnabled || IsStandardErrorLoggingEnabled;
    
    /// <summary>
    /// Gets the logger instance. If it is not set, an <see cref="InvalidOperationException" /> will be thrown.
    /// The latter case indicates that either this instance is not properly configured or that the calling code
    /// did not properly use this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Logger" /> is null.</exception>
    public ILogger GetLoggerOrThrow() =>
        Logger ?? throw CreateLoggerIsMissingException();

    /// <summary>
    /// Checks if any logging behavior is enabled and validates that the logger is properly set. 
    /// </summary>
    /// <returns>True if logging is enabled on either the Standard Output or Standard Error stream, else false.</returns>
    public bool CheckIfLoggingIsEnabled()
    {
        if (!IsLoggingEnabled)
            return false;

        if (Logger is null)
            throw CreateLoggerIsMissingException();
        return true;
    }

    private InvalidOperationException CreateLoggerIsMissingException() =>
        new ($"Logging is enabled but the logger is not configured on this settings instance: {ToString()}");
}