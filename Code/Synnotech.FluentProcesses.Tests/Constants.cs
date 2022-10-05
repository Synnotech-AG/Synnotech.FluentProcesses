namespace Synnotech.FluentProcesses.Tests;

public static class Constants
{
    public const string BuildConfiguration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif
}