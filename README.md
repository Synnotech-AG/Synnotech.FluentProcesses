# Synnotech.FluentProcesses
*A lightweight .NET library that makes using the `System.Diagnostics.Process` class easy.*


[![Synnotech Logo](synnotech-large-logo.png)](https://www.synnotech.de/)

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Synnotech-AG/Synnotech.FluentProcesses/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.0.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages/Synnotech.FluentProcesses/)

- 🌊 Fluent API for configuring `ProcessStartInfo`
- 🪵 Support for [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging)
- 📃 Automatically log standard output, standard error, and the exit code of the called process 
- 🔬 Automatically check exit codes of processes
- 🛃 Easily provide custom handlers for `OutputDataReceived` and `ErrorDataReceived`

# How to install

Synnotech.FluentProcesses is built against .NET Standard 2.0 and .NET 6, and thus supports all major platforms like .NET Framework 4.6.1 or newer, .NET Core 3.1, UWP, or Unity.

Synnotech.Core is available as a [NuGet package](https://www.nuget.org/packages/Synnotech.FluentProcesses/) and can be installed via:

- **Package Reference in csproj**: `<PackageReference Include="Synnotech.FluentProcesses" Version="1.0.0" />`
- **dotnet CLI**: `dotnet add package Synnotech.FluentProcesses`
- **Visual Studio Package Manager Console**: `Install-Package Synnotech.FluentProcesses`

# What does Synnotech.FluentProcesses offer you?

.NET allows you to start other processes with the `System.Diagnostics.Process` class. However, configuring a process can be quite tedious and time-consuming and this is where **Synnotech.FluentProcesses** shines:

```csharp
int exitCode =
    new ProcessBuilder()
        .WithCreateNoWindow()
        .DisableShellExecute()
        .EnableLogging(Logger)
        .RunProcess(
            "./Subdirectory/DataAggregator.exe",
            "--performFastRun --logLevel Information"
        );
```

The above piece of code configures the `Process.StartInfo` to create no window, not run on the shell (command line on Windows), redirects and logs the standard output and standard error streams using the specified logger, as well as logging the exit code of the application. The process is then executed and the exit code of the process is returned. By default, **Synnotech.FluentProcesses** validates that the exit code of a process is 0 (zero), or otherwise throws an `InvalidExitCodeException`.

If you are using .NET 6 or newer and want to wait for the process to finish asynchronously, simply call `RunProcessAsync` instead of `RunProcess` and await the returned `Task<int>`.

Pretty neat for such a small piece of code, heh? Let's check out how you can further configure and use **Synnotech.FluentProcesses** in the upcoming sections. Also, you can explore the source code yourself, every API is fully documented with XML comments.

## Fluent API for ProcessStartInfo

The `ProcessStartInfo` class is the main way to configure a process in .NET. The `ProcessBuilder` class offers at least one method for each property of `ProcessStartInfo` for easy configuration. Here is an example:

```csharp
// Consider this code to be used in a .NET Framework app on Windows
int exitCode =
    new ProcessBuilder()
        .WithFileName(@"C:\Tools\MyCADApp.exe")
        .WithArguments("--openFile plan.cad")
        .WithUseShellExecute(false) // this is the same as DisableShellExecute()
        .WithWindowStyle(ProcessWindowStyle.Maximized)
        .WithLoadUserProfile()
        .WithErrorDialog()
        .AddEnvironmentVariable("CAD_DefaultUnit", "mm")
        .RunProcess();
```

As you can see, some methods of the Fluent API are only applicable on Windows. In .NET 6, **Synnotech.FluentProcesses** uses the `SupportedOSPlatformAttribute` on the corresponding methods to warn you about inappropriate calls.

## Automatic Process Logging

You simply need to call `EnableLogging` to log the standard output, standard error, and the exit code of a process.

```csharp
ProcessBuilder.EnableLogging(logger);
```

You must pass an instance of `Microsoft.Extensions.Logging.ILogger`. This call will configure the following things:

- The standard output stream will be logged with `LogLevel.Information`. The `Process.OutputDataReceived` event will be used to log the output as soon as it is available. `ProcessStartInfo.RedirectStandardOutput` will be automatically set to true (you do not have to call `WithRedirectStandardOutput`).
- The error output stream will be logged with `LogLevel.Error`. The `Process.ErrorDataReceived` event will be used to log the output as soon as it is available.`ProcessStartInfo.RedirectStandardError` will be automatically set to true (you do not have to call `WithRedirectStandardError`).
- The exit code will be logged when you call `ProcessBuilder.RunProcess`, `ProcessBuilder.RunProcessAsync`, or `FluentProcess.VerifyAndLogAfterExit` (after the process has exited). Depending on whether the exit code is valid (by default, the exit code must be 0 to be valid), `LogLevel.Information` or `LogLevel.Error` is used to log the exit code.

You can customize this default behavior with the following settings:

- You can change the log levels by using `WithStandardOutputLogLevel`, `WithStandardErrorLogLevel`, `WithValidExitCodeLogLevel`, and `WithInvalidExitCodeLogLevel`, or by using the optional parameters of `EnableLogging`. Using `LogLevel.None` for any of these values disables logging for the corresponding messages.
- You can change the logging behavior of the standard output and standard error streams to `LoggingBehavior.LogAfterProcessExit`. If you set this behavior, logging of the corresponding streams will only take place after the process has exited. You can use this by calling `WithStandardOutputLogging` and `WithStandardErrorLogging`, or by using the corresponding parameters on `EnableLogging`. BEWARE: we do not recommend this behavior, as you will receive no logging messages when the process hangs. You should only use this behavior to save performance and when you know the called process will exit quickly.
- When the exit code is logged, the corresponding message is 'Process "{ProcessName} {Arguments}" has exited with code {ExitCode}'. If your arguments contain sensitive data that you don't want to end up in a log file, you might want to disable automatic exit code logging and do it yourself.

## Automatic Checks of Exit Codes

By default, **Synnotech.FluentProcesses** verifies the exit code of a process to be zero. You can customize this behavior by calling the `WithValidExitCodes` extensions method:

```csharp
ProcessBuilder.WithValidExitCodes(0, 3020);
```

You can specify one or more valid exit codes in this case. If the called process won't return a valid exit code, an `InvalidExitCodeException` will be thrown when you call `ProcessBuilder.RunProcess`, `ProcessBuilder.RunProcessAsync`, or `FluentProcess.VerifyAndLogAfterExit`.

If you don't want automatic checks of exit codes to happen, then simply disable them by calling `ProcessBuilder.DisableExitCodeVerification()`.

## Handle Standard Output and Standard Error Yourself

If you want to attach to the `Process.OutputDataReceived` event and/or `Process.ErrorDataReceived` event, simply call:

```csharp
ProcessBuilder.AddOutputReceivedHandler(HandleData)
              .AddErrorReceivedHandler(HandleData);
              
private void HandleData(object sender, DataReceivedEventArgs e)
{
    var process = (Process) sender;
    var outputLine = e.Data;
    // Do something useful here
}
```

You do not need to call `WithRedirectStandardOutput` and `WithRedirectStandardError` in this case, the corresponding values are set automatically for you.

If you also enabled logging, the logging handler on these events will always execute first before your custom handler is called.

## Create ProcessBuilder Copies to Avoid Code Duplication

You need to call several processes in the same project and some settings are the same for all these calls? You can avoid code duplication in these scenarios by calling `ProcessBuilder.Clone`:

```csharp
var firstProcessBuilder =
    new ProcessBuilder()
        .DisableShellExecute()
        .WithCreateNoWindow()
        .AddEnvironmentVariable("CI", "True");
        
// The following instruction will create a deep clone
// of the firstProcessBuilder
var secondProcessBuilder = firstProcessBuilder.Clone();

firstProcessBuilder
    .AddEnvironmentVariable("Environment", "Staging")
    .RunProcess("./SubFolder/app1.exe");

secondProcessBuilder.RunProcess("./SubFolder/app2.exe");
```

In the above example, some properties are configured on the firstProcessBuilder. Then, `Clone` is called to create a deep copy of the `ProcessBuilder` which also implies creating a deep copy of the internal `ProcessStartInfo` instance (there also is a handy public `Clone` extension method for `ProcessStartInfo` if you need to use it in other contexts). This way, the environment variable "Environment = Staging" which is set after `Clone` only affects the first process builder, but not the second one.

## What happens on RunProcess?

When you call `ProcessBuilder.RunProcess`, the following things happen:

```csharp
public int RunProcess(string fileName = "", string arguments = "")
{
    SetFileNameAndArgumentsIfNecessary(fileName, arguments);

    using FluentProcess process = CreateProcess();
    process.Start();
    process.WaitForExit();
    process.VerifyAndLogAfterExit();
    return process.ExitCode;
}

private void SetFileNameAndArgumentsIfNecessary(string fileName, string arguments)
{
    if (!fileName.IsNullOrWhiteSpace())
        WithFileName(fileName);
    if (!arguments.IsNullOrWhiteSpace())
        WithArguments(arguments);
}
```

First of all, the optional `fileName` and `arguments` values will be set if necessary. Then, the builder will create a process instance which actually is a `FluentProcess`. This class is just a simple wrapper around the actual `Process` and exposes similar APIs. It is responsible for logging and exit code verification.

The process is then started and the builder waits for the process to exit. After that, `VerifyAndLogAfterExit` is called which logs the standard output and standard error if `LoggingBehavior.LogAfterProcessExit` was specified, and to perform exit code verification and logging.

And that's it! If you want to learn more about the internals, simply check out the source code. **Synnotech.FluentProcesses** also provides the PDB files on NuGet (.snupkg) so that you can directly debug into its source code in your IDE. Check out this [article](https://devblogs.microsoft.com/nuget/improved-package-debugging-experience-with-the-nuget-org-symbol-server/) that shows you how to setup Visual Studio to consume .snupkg files from nuget.org.