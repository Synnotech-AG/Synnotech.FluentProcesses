using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.FluentProcesses.Tests;

public sealed class ProcessIntegrationTests
{
    public ProcessIntegrationTests(ITestOutputHelper output)
    {
        var solutionDirectory = FindSolutionDirectory();
        var exePath = Path.Combine(solutionDirectory,
                                   "SampleConsoleApp",
                                   "bin",
                                   Constants.BuildConfiguration,
                                   "net6.0",
                                   "SampleConsoleApp.exe");
        Logger = new LoggerMock(output: output);
        ProcessBuilder = new ProcessBuilder().WithFileName(exePath)
                                             .WithCreateNoWindow()
                                             .DisableShellExecute()
                                             .EnableLogging(Logger);
    }

    private LoggerMock Logger { get; }
    private ProcessBuilder ProcessBuilder { get; }

    [Fact]
    public void ExecuteProcessAndLog()
    {
        var exitCode = ProcessBuilder.WithArguments("--delayInterval 100")
                                     .RunProcess();

        exitCode.Should().Be(0);
        CheckDefaultLoggingMessages();
    }

    [Fact]
    public async Task ExecuteProcessAndLogAsync()
    {
        var exitCode = await ProcessBuilder.RunProcessAsync();

        exitCode.Should().Be(0);
        CheckDefaultLoggingMessages();
    }

    [Fact]
    public void RunProcessSeveralTimes()
    {
        using var process = ProcessBuilder.CreateProcess();

        process.Start();
        process.WaitForExit();
        process.ExitCode.Should().Be(0);

        process.Start();
        process.WaitForExit();
        process.ExitCode.Should().Be(0);
    }

    [Theory]
    [InlineData(LogLevel.Trace, LogLevel.Warning)]
    [InlineData(LogLevel.Debug, LogLevel.Critical)]
    public void ChangeLogLevels(LogLevel standardOutputLogLevel, LogLevel standardErrorLogLevel)
    {
        ProcessBuilder.WithStandardOutputLogLevel(standardOutputLogLevel)
                      .WithStandardErrorLogLevel(standardErrorLogLevel)
                      .RunProcess();

        var expectedMessage = new LogMessage[]
        {
            new (standardOutputLogLevel, "Hello from Sample Console App"),
            new (standardOutputLogLevel, "Here is another message"),
            new (standardErrorLogLevel, "Here is an error message"),
            new (standardErrorLogLevel, "Here are more errors"),
        };
        Logger.CapturedMessages.Should().Equal(expectedMessage);
    }

    private void CheckDefaultLoggingMessages()
    {
        var expectedMessages = new LogMessage[]
        {
            new (LogLevel.Information, "Hello from Sample Console App"),
            new (LogLevel.Information, "Here is another message"),
            new (LogLevel.Error, "Here is an error message"),
            new (LogLevel.Error, "Here are more errors"),
        };
        Logger.CapturedMessages.Should().Equal(expectedMessages);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(87)]
    public async Task ChangeValidExitCodes(int exitCode)
    {
        var actualExitCode = await ProcessBuilder.WithArguments("--exitCode " + exitCode)
                                                 .WithValidExitCodes(42, 87)
                                                 .RunProcessAsync();

        actualExitCode.Should().Be(exitCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(20)]
    public async Task InvalidExitCode(int exitCode)
    {
        ProcessBuilder.WithArguments("--exitCode " + exitCode)
                      .WithValidExitCodes(42, 87);

        var act = () => ProcessBuilder.RunProcessAsync();

        await act.Should().ThrowAsync<InvalidExitCodeException>();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(-1)]
    public void DisableExitCodeVerification(int exitCode)
    {
        var actualExitCode = ProcessBuilder.WithArguments("--exitCode " + exitCode)
                                           .DisableExitCodeVerification()
                                           .RunProcess();

        actualExitCode.Should().Be(exitCode);
    }

    private static string FindSolutionDirectory()
    {
        var directoryInfo = new DirectoryInfo(".");
        while (directoryInfo.Parent is not null)
        {
            directoryInfo = directoryInfo.Parent;
            if (directoryInfo.EnumerateFiles("*.sln").Any())
                return directoryInfo.FullName;
        }

        throw new InvalidOperationException("Could not find directory with sln file");
    }
}