using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.FluentProcesses.Tests;

public sealed class ProcessLoggingIntegrationTests
{
    public ProcessLoggingIntegrationTests(ITestOutputHelper output)
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