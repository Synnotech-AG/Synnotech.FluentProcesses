using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses.Tests;

public readonly record struct LogMessage(LogLevel LogLevel, string Message, EventId EventId)
{
    public LogMessage(LogLevel logLevel, string message) : this(logLevel, message, 0) { }

    public override string ToString() => $"{Message} ({LogLevel})";
}