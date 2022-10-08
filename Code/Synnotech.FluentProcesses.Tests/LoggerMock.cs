using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Synnotech.FluentProcesses.Tests;

public sealed class LoggerMock : ILogger
{
    public LoggerMock(LogLevel minimumLevel = LogLevel.Trace, ITestOutputHelper? output = null)
    {
        MinimumLevel = minimumLevel;
        Output = output;
    }

    private LogLevel MinimumLevel { get; }
    private ITestOutputHelper? Output { get; }
    public List<LogMessage> CapturedMessages { get; } = new ();

    public void Log<TState>(LogLevel logLevel,
                            EventId eventId,
                            TState state,
                            Exception? exception,
                            Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        CapturedMessages.Add(new (logLevel, message));
        Output?.WriteLine($"{message} ({logLevel})");
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinimumLevel;

    public IDisposable BeginScope<TState>(TState state) =>
        throw new NotSupportedException("We do not use scopes in FluentProcesses");
}