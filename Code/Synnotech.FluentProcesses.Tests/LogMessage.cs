using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses.Tests;

// ReSharper disable once NotAccessedPositionalProperty.Global -- EventId is necessary for equality comparison
public readonly record struct LogMessage(LogLevel LogLevel, string Message, EventId EventId)
{
    public LogMessage(LogLevel logLevel, string message) : this(logLevel, message, 0) { }

    public override string ToString() => $"{Message} ({LogLevel})";
}