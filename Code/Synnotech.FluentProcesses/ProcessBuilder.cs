using System;
using System.Diagnostics;
using System.Security;
using System.Text;
using Light.GuardClauses;
using Microsoft.Extensions.Logging;
#if NET6_0
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Versioning;
#endif

namespace Synnotech.FluentProcesses;

/// <summary>
/// Represents a builder that can create <see cref="Process" /> instances.
/// You can call the fluent API of this builder to configure the process.
/// </summary>
public sealed class ProcessBuilder
{
    /// <summary>
    /// Initializes a new instance of <see cref="ProcessBuilder" />.
    /// </summary>
    public ProcessBuilder() : this(new ()) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessBuilder" />.
    /// </summary>
    /// <param name="processStartInfo">The object that contains all configuration infos about the process</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="processStartInfo" /> is null.</exception>
    public ProcessBuilder(ProcessStartInfo processStartInfo) =>
        ProcessStartInfo = processStartInfo.MustNotBeNull();

    private static int[]? DefaultValidExitCodes { get; } = { 0 };

    private ProcessStartInfo ProcessStartInfo { get; }
    private bool WasEnvironmentVariableSet { get; set; }
    private LoggingSettings LoggingSettings { get; set; } = new (null);
    private int[]? ValidExitCodes { get; set; } = DefaultValidExitCodes;
    private StreamHandlers StreamHandlers { get; set; }

    /// <summary>
    /// Adds a custom environment variable that the process can access when executed.
    /// BEWARE: all environment variables that are defined on the operating-system level
    /// will be automatically loaded and can be accessed by the process during its run.
    /// </summary>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="value">The value of the environment variable.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name" /> or <paramref name="value" /> are null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name" /> or <paramref name="value" />
    /// are empty or contain only white space.
    /// </exception>
    public ProcessBuilder AddEnvironmentVariable(string name, string value)
    {
        name.MustNotBeNullOrWhiteSpace();
        value.MustNotBeNullOrWhiteSpace();

        ProcessStartInfo.EnvironmentVariables.Add(name, value);
        WasEnvironmentVariableSet = true;
        return this;
    }

    /// <summary>
    /// Sets the set of command-line arguments to use when starting the application.
    /// </summary>
    public ProcessBuilder WithArguments(string arguments)
    {
        ProcessStartInfo.Arguments = arguments;
        return this;
    }

    /// <summary>
    /// <para>
    /// Sets a value that identifies the domain to use when starting the process.
    /// If this value is null, the UserName property must be specified in UPN format.
    /// </para>
    /// <para>
    /// While you can call this method on all platforms, starting the process with
    /// an AD domain is only supported on Windows platforms.
    /// </para>
    /// </summary>
#if NET6_0
    [SupportedOSPlatform("windows")]
#endif
    public ProcessBuilder WithDomain(string? domain)
    {
        ProcessStartInfo.Domain = domain;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether to start the process in a new window. Use
    /// true if the process should be started without creating a new window to contain it.
    /// </summary>
    public ProcessBuilder WithCreateNoWindow(bool createNoWindow = true)
    {
        ProcessStartInfo.CreateNoWindow = createNoWindow;
        return this;
    }

    /// <summary>
    /// <para>
    /// Sets a secure string that contains the user password to use when starting the process.
    /// </para>
    /// <para>
    /// Secure strings can only be created on Windows platforms.
    /// </para>
    /// </summary>
#if NET6_0
    [SupportedOSPlatform("windows")]
#endif
    public ProcessBuilder WithPassword(SecureString? password)
    {
        ProcessStartInfo.Password = password;
        return this;
    }

    /// <summary>
    /// <para>
    /// Sets the verb to use when opening the application or document.
    /// The verb identifies the action to take with the file that the process opens.
    /// The default is an empty string (""), which signifies no action.
    /// </para>
    /// <para>
    /// You can find all associated verbs with a file type by accessing the
    /// <see cref="System.Diagnostics.ProcessStartInfo.Verbs" /> property after setting the
    /// <see cref="System.Diagnostics.ProcessStartInfo.FileName" /> property.
    /// </para>
    /// </summary>
    public ProcessBuilder WithVerb(string verb)
    {
        ProcessStartInfo.Verb = verb;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether an error dialog box is displayed to the
    /// user if the process cannot be started.
    /// </summary>
    public ProcessBuilder WithErrorDialog(bool showErrorDialog = true)
    {
        ProcessStartInfo.ErrorDialog = showErrorDialog;
        return this;
    }

    /// <summary>
    /// Sets the application or document to start. This
    /// value must be set before the process can be started.
    /// </summary>
    public ProcessBuilder WithFileName(string fileName)
    {
        ProcessStartInfo.FileName = fileName;
        return this;
    }

    /// <summary>
    /// <para>
    /// Sets the user name to use when starting the process. If you use the UPN format (user@DNS_domain_name),
    /// the Domain property must be null.
    /// </para>
    /// <para>
    /// While you can call this method on all platforms, starting the process with
    /// user name and password is only supported on Windows platforms.
    /// </para>
    /// </summary>
    public ProcessBuilder WithUserName(string userName)
    {
        ProcessStartInfo.UserName = userName;
        return this;
    }

    /// <summary>
    /// Sets the window state to use when the process is started.
    /// </summary>
    public ProcessBuilder WithWindowStyle(ProcessWindowStyle windowStyle)
    {
        ProcessStartInfo.WindowStyle = windowStyle;
        return this;
    }

    /// <summary>
    /// When the UseShellExecute property is false, sets the working directory
    /// for the process to be started. When UseShellExecute is true, gets or sets the directory
    /// that contains the process to be started.
    /// </summary>
    public ProcessBuilder WithWorkingDirectory(string workingDirectory)
    {
        ProcessStartInfo.WorkingDirectory = workingDirectory;
        return this;
    }

    /// <summary>
    /// <para>
    /// Sets a value that indicates whether the Windows user profile is to be loaded from the registry.
    /// </para>
    /// <para>
    /// While you can call this method on all platforms, starting the process with
    /// this feature enabled will only work on Windows platforms.
    /// </para>
    /// </summary>
#if NET6_0
    [SupportedOSPlatform("windows")]
#endif
    public ProcessBuilder WithLoadUserProfile(bool loadUserProfile = true)
    {
        ProcessStartInfo.LoadUserProfile = loadUserProfile;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether the error output of an application is
    /// written to the <see cref="Process.StandardError" /> stream.
    /// </summary>
    public ProcessBuilder WithRedirectStandardError(bool redirectStandardError = true)
    {
        ProcessStartInfo.RedirectStandardError = redirectStandardError;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether the input for an application is written
    /// to the <see cref="Process.StandardInput" /> stream.
    /// </summary>
    public ProcessBuilder WithRedirectStandardInput(bool redirectStandardInput = true)
    {
        ProcessStartInfo.RedirectStandardInput = redirectStandardInput;
        return this;
    }

    /// <summary>
    /// Sets a value that indicates whether the textual output of an application
    /// is written to the <see cref="Process.StandardOutput" /> stream.
    /// </summary>
    public ProcessBuilder WithRedirectStandardOutput(bool redirectStandardOutput = true)
    {
        ProcessStartInfo.RedirectStandardOutput = redirectStandardOutput;
        return this;
    }

    /// <summary>
    /// Sets the preferred encoding for error output.
    /// </summary>
    public ProcessBuilder WithStandardErrorEncoding(Encoding? errorEncoding)
    {
        ProcessStartInfo.StandardErrorEncoding = errorEncoding;
        return this;
    }

    /// <summary>
    /// Sets the preferred encoding for standard output.
    /// </summary>
    public ProcessBuilder WithStandardOutputEncoding(Encoding? standardOutputEncoding)
    {
        ProcessStartInfo.StandardOutputEncoding = standardOutputEncoding;
        return this;
    }

    /// <summary>
    /// Sets the preferred encoding for both the standard output
    /// and error output.
    /// </summary>
    public ProcessBuilder WithEncoding(Encoding? encoding)
    {
        WithStandardOutputEncoding(encoding);
        WithStandardErrorEncoding(encoding);
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether to use the operating system shell to start the process.
    /// By default, <see cref="System.Diagnostics.ProcessStartInfo.UseShellExecute" /> is
    /// set to true in .NET Framework apps, and set to false in .NET (Core) apps.
    /// </summary>
    public ProcessBuilder WithUseShellExecute(bool useShellExecute)
    {
        ProcessStartInfo.UseShellExecute = useShellExecute;
        return this;
    }

    /// <summary>
    /// Sets <see cref="System.Diagnostics.ProcessStartInfo.UseShellExecute" /> to false.
    /// By default, <see cref="System.Diagnostics.ProcessStartInfo.UseShellExecute" /> is
    /// set to true in .NET Framework apps, and set to false in .NET (Core) apps.
    /// </summary>
    public ProcessBuilder DisableShellExecute()
    {
        ProcessStartInfo.UseShellExecute = false;
        return this;
    }

    /// <summary>
    /// Sets the window handle to use when an error dialog box is
    /// shown for a process that cannot be started.
    /// </summary>
    public ProcessBuilder WithErrorDialogParentHandle(IntPtr parentWindowHandle)
    {
        ProcessStartInfo.ErrorDialogParentHandle = parentWindowHandle;
        return this;
    }

    /// <summary>
    /// <para>
    /// Sets the user password in clear text to use when starting the process.
    /// </para>
    /// <para>
    /// While you can call this method on all platforms, starting the process with
    /// user name and password is only supported on Windows platforms.
    /// </para>
    /// </summary>
#if NET6_0
    [SupportedOSPlatform("windows")]
#endif
    public ProcessBuilder WithPasswordInClearText(string password)
    {
        ProcessStartInfo.PasswordInClearText = password;
        return this;
    }

    /// <summary>
    /// <para>
    /// Creates a deep copy of this process builder instance.
    /// </para>
    /// <para>
    /// This feature should usually be used when you need to configure processes
    /// differently. All common properties can be set on the initial builder, and
    /// you can then clone this instance to further configure each builder.
    /// </para>
    /// </summary>
    public ProcessBuilder Clone()
    {
        var processStartInfoClone = ProcessStartInfo.Clone(WasEnvironmentVariableSet);
        return new (processStartInfoClone) { WasEnvironmentVariableSet = WasEnvironmentVariableSet };
    }

    /// <summary>
    /// Registers the specified logger for potential usage with <see cref="Process.StandardOutput" />
    /// and <see cref="Process.StandardError" />. BEWARE: calling only this method is not enough,
    /// you must also call at least one of the <see cref="WithStandardOutputLogging" /> or
    /// <see cref="WithStandardErrorLogging" /> methods. See their XML comments for details.
    /// </summary>
    public ProcessBuilder WithLogger(ILogger? logger)
    {
        LoggingSettings = LoggingSettings with { Logger = logger };
        return this;
    }

    /// <summary>
    /// <para>
    /// Enables or disables logging of the <see cref="Process.StandardOutput" /> stream.
    /// BEWARE: calling this method alone is not enough, you must specify the logger
    /// with the <see cref="WithLogger" /> method.
    /// </para>
    /// </summary>
    public ProcessBuilder WithStandardOutputLogging(LoggingBehavior standardOutputLoggingBehavior = LoggingBehavior.LogOnEvent)
    {
        LoggingSettings = LoggingSettings with { StandardOutputLoggingBehavior = standardOutputLoggingBehavior };
        return this;
    }

    /// <summary>
    /// <para>
    /// Enables or disables logging of the <see cref="Process.StandardError" /> stream.
    /// BEWARE: calling this method alone is not enough, you must specify the logger
    /// with the <see cref="WithLogger" /> method.
    /// </para>
    /// </summary>
    public ProcessBuilder WithStandardErrorLogging(LoggingBehavior standardErrorLoggingBehavior = LoggingBehavior.LogOnEvent)
    {
        LoggingSettings = LoggingSettings with { StandardErrorLoggingBehavior = standardErrorLoggingBehavior };
        return this;
    }

    /// <summary>
    /// Sets the logging level for the standard output stream.
    /// The default log level for it is <see cref="LogLevel.Information" />.
    /// </summary>
    public ProcessBuilder WithStandardOutputLogLevel(LogLevel standardOutputLogLevel)
    {
        LoggingSettings = LoggingSettings with { StandardOutputLogLevel = standardOutputLogLevel };
        return this;
    }

    /// <summary>
    /// Sets the logging level for the standard error stream.
    /// The default log level for it is <see cref="LogLevel.Error" />.
    /// </summary>
    public ProcessBuilder WithStandardErrorLogLevel(LogLevel standardErrorLogLevel)
    {
        LoggingSettings = LoggingSettings with { StandardErrorLogLevel = standardErrorLogLevel };
        return this;
    }

    /// <summary>
    /// Sets the level which is used for logging a valid exit code of a process.
    /// This log level is also used to log the exit code if no exit code validation is configured.
    /// </summary>
    public ProcessBuilder WithValidExitCodeLogLevel(LogLevel validExitCodeLogLevel)
    {
        LoggingSettings = LoggingSettings with { ValidExitCodeLogLevel = validExitCodeLogLevel };
        return this;
    }

    /// <summary>
    /// Sets the level which is used for logging invalid exit codes.
    /// You can call <see cref="WithValidExitCodes" /> to specify the
    /// exit codes that are valid for the process. By default, only 0 is
    /// considered a valid exit code.
    /// </summary>
    public ProcessBuilder WithInvalidExitCodeLogLevel(LogLevel invalidExitCodeLogLevel)
    {
        LoggingSettings = LoggingSettings with { InvalidExitCodeLogLevel = invalidExitCodeLogLevel };
        return this;
    }

    /// <summary>
    /// Enables logging by setting the specified logger and setting the standard output and standard error
    /// logging behaviors to <see cref="LoggingBehavior.LogOnEvent" />.
    /// </summary>
    /// <param name="logger">The object used for logging.</param>
    /// <param name="standardOutputLogLevel">The level used for logging messages of the standard output stream.</param>
    /// <param name="standardErrorLogLevel">The level used for logging messages of the standard error stream.</param>
    /// <param name="validExitCodeLogLevel">
    /// The level used when logging a valid exit code.
    /// This log level is also used to log the exit code if no exit code validation is configured.
    /// </param>
    /// <param name="invalidExitCodeLogLevel">
    /// The level used when logging an invalid exit code.
    /// You can call <see cref="WithValidExitCodes" /> to specify the
    /// exit codes that are valid for the process. By default, only 0 is
    /// considered a valid exit code.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger" /> is null.</exception>
    public ProcessBuilder EnableLogging(ILogger logger,
                                        LogLevel standardOutputLogLevel = LogLevel.Information,
                                        LogLevel standardErrorLogLevel = LogLevel.Error,
                                        LogLevel validExitCodeLogLevel = LogLevel.Information,
                                        LogLevel invalidExitCodeLogLevel = LogLevel.Error) =>
        WithLogger(logger.MustNotBeNull())
           .WithStandardOutputLogging()
           .WithStandardErrorLogging()
           .WithStandardOutputLogLevel(standardOutputLogLevel)
           .WithStandardErrorLogLevel(standardErrorLogLevel)
           .WithValidExitCodeLogLevel(validExitCodeLogLevel)
           .WithInvalidExitCodeLogLevel(invalidExitCodeLogLevel);

    /// <summary>
    /// Sets the exit codes that are valid for the process. By default, FluentProcesses checks
    /// if the exit code is 0.
    /// The exit code of a process will be validated when <see cref="FluentProcess.VerifyAndLogAfterExit" />
    /// is called. This is automatically done when calling RunProcess(Async).
    /// </summary>
    public ProcessBuilder WithValidExitCodes(params int[] validExitCodes)
    {
        ValidExitCodes = validExitCodes;
        return this;
    }

    /// <summary>
    /// Disables exit code verification. By default, FluentProcesses checks
    /// if the exit code of a process is 0.
    /// </summary>
    public ProcessBuilder DisableExitCodeVerification()
    {
        ValidExitCodes = null;
        return this;
    }

    /// <summary>
    /// Adds the specified handler to the <see cref="Process.OutputDataReceived" /> event.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler" /> is null.</exception>
    public ProcessBuilder AddOutputReceivedHandler(DataReceivedEventHandler handler)
    {
        handler.MustNotBeNull();

        StreamHandlers = StreamHandlers with { StandardOutputHandler = CombineHandlers(StreamHandlers.StandardOutputHandler, handler) };
        return this;
    }

    /// <summary>
    /// Removes the specified handler from the <see cref="Process.OutputDataReceived" /> event.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlerToBeRemoved" /> is null.</exception>
    public ProcessBuilder RemoveOutputReceivedHandler(DataReceivedEventHandler handlerToBeRemoved)
    {
        handlerToBeRemoved.MustNotBeNull();

        StreamHandlers = StreamHandlers with { StandardOutputHandler = RemoveHandler(StreamHandlers.StandardOutputHandler, handlerToBeRemoved) };
        return this;
    }

    /// <summary>
    /// Adds the specified handler to the <see cref="Process.ErrorDataReceived" /> event.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler" /> is null.</exception>
    public ProcessBuilder AddErrorReceivedHandler(DataReceivedEventHandler handler)
    {
        handler.MustNotBeNull();

        StreamHandlers = StreamHandlers with { StandardErrorHandler = CombineHandlers(StreamHandlers.StandardErrorHandler, handler) };
        return this;
    }

    /// <summary>
    /// Removes the specified handler form the <see cref="Process.ErrorDataReceived" /> event.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlerToBeRemoved" /> is null.</exception>
    public ProcessBuilder RemoveErrorReceivedHandler(DataReceivedEventHandler handlerToBeRemoved)
    {
        handlerToBeRemoved.MustNotBeNull();

        StreamHandlers = StreamHandlers with { StandardErrorHandler = RemoveHandler(StreamHandlers.StandardErrorHandler, handlerToBeRemoved) };
        return this;
    }

    private static DataReceivedEventHandler CombineHandlers(DataReceivedEventHandler? target, DataReceivedEventHandler newHandler)
    {
        target += newHandler;
        return target;
    }

    private static DataReceivedEventHandler? RemoveHandler(DataReceivedEventHandler? source, DataReceivedEventHandler handlerToBeRemoved)
    {
        if (source is null)
            return source;

        source -= handlerToBeRemoved;
        return source;
    }

    /// <summary>
    /// Creates the process instance using all information gathered by this builder instance.
    /// </summary>
    public FluentProcess CreateProcess() =>
        new (new Process { StartInfo = ProcessStartInfo }, LoggingSettings, ValidExitCodes, StreamHandlers);

    /// <summary>
    /// Creates a process instance out of the information attached to this process builder instance,
    /// starts it and waits for exit. Afterwards, the process is disposed.
    /// </summary>
    /// <param name="fileName">
    /// The path to the process that you want to start (optional). <see cref="WithFileName" /> will
    /// be called internally when this parameter is set.
    /// </param>
    /// <param name="arguments">
    /// The arguments that will be passed to the target process (optional). <see cref="WithArguments" /> will
    /// be called internally when this parameter is set.
    /// </param>
    /// <returns>The exit code of the process.</returns>
    public int RunProcess(string fileName = "", string arguments = "")
    {
        SetFileNameAndArgumentsIfNecessary(fileName, arguments);

        using var process = CreateProcess();
        process.Start();
        process.WaitForExit();
        process.VerifyAndLogAfterExit();
        return process.ExitCode;
    }

#if NET6_0
    /// <summary>
    /// Creates a process instance out of the information attached to this process builder instance,
    /// starts it and waits for exit asynchronously. Afterwards, the process is disposed.
    /// </summary>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>The exit code of the process.</returns>
    public Task<int> RunProcessAsync(CancellationToken cancellationToken = default) =>
        RunProcessAsync(string.Empty, string.Empty, cancellationToken);
    
    /// <summary>
    /// Creates a process instance out of the information attached to this process builder instance,
    /// starts it and waits for exit asynchronously. Afterwards, the process is disposed.
    /// </summary>
    /// <param name="fileName">
    /// The path to the process that you want to start (optional). <see cref="WithFileName" /> will
    /// be called internally when this parameter is set.
    /// </param>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>The exit code of the process.</returns>
    public Task<int> RunProcessAsync(string fileName, CancellationToken cancellationToken = default) =>
        RunProcessAsync(fileName, string.Empty, cancellationToken);

    /// <summary>
    /// Creates a process instance out of the information attached to this process builder instance,
    /// starts it and waits for exit asynchronously. Afterwards, the process is disposed.
    /// </summary>
    /// <param name="fileName">
    /// The path to the process that you want to start (optional). <see cref="WithFileName" /> will
    /// be called internally when this parameter is set.
    /// </param>
    /// <param name="arguments">
    /// The arguments that will be passed to the target process (optional). <see cref="WithArguments" /> will
    /// be called internally when this parameter is set.
    /// </param>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>The exit code of the process.</returns>
    public async Task<int> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
    {
        SetFileNameAndArgumentsIfNecessary(fileName, arguments);
        using var process = CreateProcess();
        process.Start();
        await process.WaitForExitAsync(cancellationToken);
        process.VerifyAndLogAfterExit();
        return process.ExitCode;
    }
#endif
    
    private void SetFileNameAndArgumentsIfNecessary(string fileName, string arguments)
    {
        if (!fileName.IsNullOrWhiteSpace())
            WithFileName(fileName);
        if (!arguments.IsNullOrWhiteSpace())
            WithArguments(arguments);
    }
}