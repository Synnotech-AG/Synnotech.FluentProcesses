using Microsoft.Extensions.Logging;

namespace Synnotech.FluentProcesses.Tests;

// ReSharper disable once NotAccessedPositionalProperty.Global -- EventId is necessary for equality comparison
public readonly record struct LogMessage(LogLevel LogLevel, string Message)
{
    public override string ToString() => $"{Message} ({LogLevel})";
}