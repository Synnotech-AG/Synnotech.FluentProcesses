using System;

namespace Synnotech.FluentProcesses.Tests;

public static class Constants
{
    public const string BuildConfiguration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    public static string SampleConsoleAppExe { get; } =
#if NET6_0
        OperatingSystem.IsWindows() ? "SampleConsoleApp.exe" : "SampleConsoleApp";
#else
        "SampleConsoleApp.exe";
#endif
}