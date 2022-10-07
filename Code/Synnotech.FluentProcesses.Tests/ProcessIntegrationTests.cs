using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using Light.GuardClauses;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
#if NET6_0
using System.Threading.Tasks;
#endif

namespace Synnotech.FluentProcesses.Tests;

public sealed class ProcessIntegrationTests
{
    static ProcessIntegrationTests()
    {
        var solutionDirectory = FindSolutionDirectory();
        ExePath = Path.Combine(solutionDirectory,
                               "SampleConsoleApp",
                               "bin",
                               Constants.BuildConfiguration,
                               "net6.0",
                               Constants.SampleConsoleAppExe);
    }

    public ProcessIntegrationTests(ITestOutputHelper output)
    {
        Logger = new LoggerMock(output: output);
        ProcessBuilder = new ProcessBuilder().WithFileName(ExePath)
                                             .WithCreateNoWindow()
                                             .DisableShellExecute()
                                             .EnableLogging(Logger);
    }

    private static string ExePath { get; }

    private LoggerMock Logger { get; }
    private ProcessBuilder ProcessBuilder { get; }

    [Fact]
    public void ExecuteProcessAndLog()
    {
        const string arguments = "--delayInterval 100";
        var exitCode = ProcessBuilder.WithArguments(arguments)
                                     .RunProcess();

        exitCode.Should().Be(0);
        CheckDefaultLoggingMessages(arguments, exitCode);
    }

#if NET6_0
    [Fact]
    public async Task ExecuteProcessAndLogAsync()
    {
        var exitCode = await ProcessBuilder.RunProcessAsync();

        exitCode.Should().Be(0);
        CheckDefaultLoggingMessages(null, exitCode);
    }
#endif

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
    [InlineData(LogLevel.Trace, LogLevel.Warning, LogLevel.Debug, LogLevel.Error, 0)]
    [InlineData(LogLevel.Debug, LogLevel.Critical, LogLevel.Information, LogLevel.Critical, 42)]
    [InlineData(LogLevel.Debug, LogLevel.Critical, LogLevel.Debug, LogLevel.Error, 0)]
    [InlineData(LogLevel.Information, LogLevel.Warning, LogLevel.Trace, LogLevel.Warning, 42)]
    public void ChangeLogLevels(LogLevel standardOutputLogLevel,
                                LogLevel standardErrorLogLevel,
                                LogLevel validExitCodeLevel,
                                LogLevel invalidExitCodeLevel,
                                int exitCode)
    {
        var arguments = "--exitCode " + exitCode;
        ProcessBuilder.WithStandardOutputLogLevel(standardOutputLogLevel)
                      .WithStandardErrorLogLevel(standardErrorLogLevel)
                      .WithValidExitCodeLogLevel(validExitCodeLevel)
                      .WithInvalidExitCodeLogLevel(invalidExitCodeLevel)
                      .WithArguments(arguments);
        
        var act = () => ProcessBuilder.RunProcess();

        var isInvalidExitCode = exitCode != 0;
        if (isInvalidExitCode)
            act.Should().Throw<InvalidExitCodeException>();
        else
            act.Should().NotThrow();
        var exitCodeMessage = CreateExitCodeMessage(arguments, exitCode);
        var exitCodeLogLevel = isInvalidExitCode ? invalidExitCodeLevel : validExitCodeLevel;
        var expectedMessage = new LogMessage[]
        {
            new (standardOutputLogLevel, "Hello from Sample Console App"),
            new (standardOutputLogLevel, "Here is another message"),
            new (standardErrorLogLevel, "Here is an error message"),
            new (standardErrorLogLevel, "Here are more errors"),
            new (exitCodeLogLevel, exitCodeMessage)
        };
        Logger.CapturedMessages.Should().Equal(expectedMessage);
    }

    private void CheckDefaultLoggingMessages(string? arguments, int exitCode)
    {
        var exitCodeMessage = CreateExitCodeMessage(arguments, exitCode);
        var expectedMessages = new LogMessage[]
        {
            new (LogLevel.Information, "Hello from Sample Console App"),
            new (LogLevel.Information, "Here is another message"),
            new (LogLevel.Error, "Here is an error message"),
            new (LogLevel.Error, "Here are more errors"),
            new (LogLevel.Information, exitCodeMessage)
        };
        Logger.CapturedMessages.Should().Equal(expectedMessages);
    }

    private static string CreateExitCodeMessage(string? arguments, int exitCode)
    {
        var exitCodeMessage = arguments.IsNullOrWhiteSpace() ?
                                  $"Process \"{ExePath}\" exited with code {exitCode}" :
                                  $"Process \"{ExePath} {arguments}\" exited with code {exitCode}";
        return exitCodeMessage;
    }

#if NET6_0
    [Theory]
    [InlineData(42)]
    [InlineData(87)]
    public async Task ChangeValidExitCodesAsync(int exitCode)
    {
        var actualExitCode = await ProcessBuilder.WithArguments("--exitCode " + exitCode)
                                                 .WithValidExitCodes(42, 87)
                                                 .RunProcessAsync();

        actualExitCode.Should().Be(exitCode);
    }
#endif

    [Theory]
    [InlineData(42)]
    [InlineData(87)]
    public void ChangeValidExitCodes(int exitCode)
    {
        var actualExitCode = ProcessBuilder.WithArguments("--exitCode " + exitCode)
                                           .WithValidExitCodes(42, 87)
                                           .RunProcess();

        actualExitCode.Should().Be(exitCode);
    }

#if NET6_0
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(20)]
    public async Task InvalidExitCodeAsync(int exitCode)
    {
        ProcessBuilder.WithArguments("--exitCode " + exitCode)
                      .WithValidExitCodes(42, 87);

        var act = () => ProcessBuilder.RunProcessAsync();

        await act.Should().ThrowAsync<InvalidExitCodeException>();
    }
#endif

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(20)]
    public void InvalidExitCode(int exitCode)
    {
        ProcessBuilder.WithArguments("--exitCode " + exitCode)
                      .WithValidExitCodes(42, 87);

        var act = () => ProcessBuilder.RunProcess();

        act.Should().Throw<InvalidExitCodeException>();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(128)]
    public void DisableExitCodeVerification(int exitCode)
    {
        var actualExitCode = ProcessBuilder.WithArguments("--exitCode " + exitCode)
                                           .DisableExitCodeVerification()
                                           .RunProcess();

        actualExitCode.Should().Be(exitCode);
    }

    [Fact]
    public void AttachCustomHandlers()
    {
        var capturedOutput = new List<string?>();
        var capturedErrors = new List<string?>();

        ProcessBuilder.AddOutputReceivedHandler((_, e) => capturedOutput.Add(e.Data))
                      .AddErrorReceivedHandler((_, e) => capturedErrors.Add(e.Data))
                      .RunProcess();

        var expectedOutput = new[] { "Hello from Sample Console App", "Here is another message", null };
        capturedOutput.Should().Equal(expectedOutput);
        var expectedErrors = new[] { "Here is an error message", "Here are more errors", null };
        capturedErrors.Should().Equal(expectedErrors);
    }

    [Fact]
    public void RemoveHandlers()
    {
        var capturedData = new List<string?>();

        ProcessBuilder.AddOutputReceivedHandler(Handler)
                      .AddErrorReceivedHandler(Handler)
                      .RemoveOutputReceivedHandler(Handler)
                      .RemoveErrorReceivedHandler(Handler)
                      .RunProcess();

        capturedData.Should().BeEmpty();

        void Handler(object _, DataReceivedEventArgs e) => capturedData.Add(e.Data);
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