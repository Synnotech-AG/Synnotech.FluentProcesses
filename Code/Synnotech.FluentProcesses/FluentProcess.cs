using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Represents a wrapper around a <see cref="Process" /> that enables
/// additional functionality like logging and exit code verification.
/// </summary>
public sealed class FluentProcess : IDisposable
{
    /// <summary>
    /// Initializes a new instance of <see cref="FluentProcess" />.
    /// </summary>
    /// <param name="actualProcess">The process instance that will be orchestrated.</param>
    /// <param name="loggingSettings">The settings object specifying how logging is handled.</param>
    /// <param name="validExitCodes">The array which contains all valid exit codes for the process.</param>
    /// <param name="streamHandlers">The instance that encapsulates the optional handlers for standard output and standard error.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="actualProcess" /> is null.</exception>
    public FluentProcess(Process actualProcess,
                         LoggingSettings loggingSettings = default,
                         int[]? validExitCodes = null,
                         StreamHandlers streamHandlers = default)
    {
        ActualProcess = actualProcess.MustNotBeNull();
        LoggingSettings = loggingSettings;
        ValidExitCodes = validExitCodes;
        StreamHandlers = streamHandlers;
    }

    /// <summary>
    /// Gets the actual process instance wrapped by this instance.
    /// </summary>
    public Process ActualProcess { get; }

    /// <summary>
    /// Gets the start info of the <see cref="ActualProcess" /> object.
    /// </summary>
    public ProcessStartInfo StartInfo => ActualProcess.StartInfo;

    /// <summary>
    /// Gets the exit code of the <see cref="ActualProcess" /> object.
    /// </summary>
    public int ExitCode => ActualProcess.ExitCode;

    /// <summary>
    /// Gets the settings object that determines the logging behavior of this process.
    /// </summary>
    public LoggingSettings LoggingSettings { get; }

    /// <summary>
    /// Gets the array of valid exit codes. You need to call
    /// </summary>
    public int[]? ValidExitCodes { get; }

    /// <summary>
    /// Gets the instance that encapsulates the optional handlers for standard output and standard error.
    /// </summary>
    public StreamHandlers StreamHandlers { get; }

    private bool WasStarted { get; set; }

    /// <summary>
    /// Disposes the <see cref="ActualProcess" />.
    /// </summary>
    public void Dispose() => ActualProcess.Dispose();

    /// <summary>
    /// Starts (or restarts) the process. Logging will be enabled if necessary.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No file name was specified in the Process component's StartInfo. -or-
    /// The UseShellExecute member of the StartInfo property is true while
    /// RedirectStandardInput, RedirectStandardOutput, or RedirectStandardError is true.
    /// </exception>
    /// <exception cref="Win32Exception">There was an error in opening the associated file.</exception>
    /// <exception cref="ObjectDisposedException">The process object has already been disposed.</exception>
    /// <exception cref="PlatformNotSupportedException">
    /// Method not supported on operating systems without shell support such as Nano Server (.NET Core only).
    /// </exception>
    public void Start()
    {
        if (!WasStarted)
        {
            ActualProcess.EnableLoggingIfNecessary(LoggingSettings);
            StreamHandlers.AttachHandlersIfNecessary(ActualProcess);
        }

        ActualProcess.Start();

        if (!WasStarted)
        {
            if (LoggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogOnEvent || StreamHandlers.StandardOutputHandler is not null)
                ActualProcess.BeginOutputReadLine();

            if (LoggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogOnEvent || StreamHandlers.StandardErrorHandler is not null)
                ActualProcess.BeginErrorReadLine();
        }

        WasStarted = true;
    }

    /// <summary>
    /// Instructs the <see cref="ActualProcess" /> to wait indefinitely for the associated process to exit.
    /// </summary>
    /// <exception cref="Win32Exception">The wait setting could not be accessed.</exception>
    /// <exception cref="SystemException">
    /// No process Id has been set, and a Handle from which the Id property can be determined does not exist. -or-
    /// There is no process associated with <see cref="ActualProcess" /> object. -or-
    /// You are attempting to call WaitForExit() for a process that is running on a remote computer.
    /// This method is available only for processes that are running on the local computer.
    /// </exception>
    public void WaitForExit() => ActualProcess.WaitForExit();

    /// <summary>
    /// Instructs the <see cref="ActualProcess" /> to wait the specified number of milliseconds
    /// for the associated process to exit.
    /// </summary>
    /// <param name="milliseconds">
    /// The amount of time, in milliseconds, to wait for the associated process to exit.
    /// The maximum is the largest possible value of a 32-bit integer, which represents infinity to the operating system.
    /// </param>
    /// <returns>true if the associated process has exited; otherwise, false.</returns>
    /// <exception cref="Win32Exception">The wait setting could not be accessed.</exception>
    /// <exception cref="SystemException">
    /// No process Id has been set, and a Handle from which the Id property can be determined does not exist. -or-
    /// There is no process associated with <see cref="ActualProcess" /> object. -or-
    /// You are attempting to call WaitForExit() for a process that is running on a remote computer.
    /// This method is available only for processes that are running on the local computer.
    /// </exception>
    public bool WaitForExit(int milliseconds) => ActualProcess.WaitForExit(milliseconds);

#if NET6_0
    /// <summary>
    /// Instructs the <see cref="ActualProcess" /> to wait for the associated process to exit,
    /// or for the cancellationToken to be cancelled.
    /// </summary>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task that will complete when the process has exited, cancellation has been requested, or an error occurs.
    /// </returns>
    /// <exception cref="Win32Exception">The wait setting could not be accessed.</exception>
    /// <exception cref="SystemException">
    /// No process Id has been set, and a Handle from which the Id property can be determined does not exist. -or-
    /// There is no process associated with <see cref="ActualProcess" /> object. -or-
    /// You are attempting to call WaitForExit() for a process that is running on a remote computer.
    /// This method is available only for processes that are running on the local computer.
    /// </exception>
    public Task WaitForExitAsync(CancellationToken cancellationToken = default) => ActualProcess.WaitForExitAsync(cancellationToken);
#endif

    /// <summary>
    /// <para>
    /// Checks if the exit code of the <see cref="ActualProcess" /> is valid.
    /// Also performs logging after exit if necessary.
    /// </para>
    /// <para>
    /// If <see cref="ValidExitCodes" /> is null, then no check will be performed.
    /// Otherwise, if the array does not contain the exit code of the process,
    /// an <see cref="InvalidExitCodeException" /> will be thrown.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidExitCodeException">Thrown when the exit code of this process is not one of the <see cref="ValidExitCodes" />.</exception>
    public void VerifyAndLogAfterExit()
    {
        ActualProcess.LogAfterExitIfNecessary(LoggingSettings);

        var exitCode = ActualProcess.ExitCode;
        var isExitCodeConsideredValid = CheckIfExitCodeIsConsideredValid(exitCode, ValidExitCodes);
        ActualProcess.LogExitCodeIfNecessary(LoggingSettings, isExitCodeConsideredValid);

        if (!isExitCodeConsideredValid)
            throw new InvalidExitCodeException(this, ValidExitCodes!, $"Process \"{StartInfo.FileName} {StartInfo.Arguments}\" exited with invalid code {exitCode}.");
    }

    private static bool CheckIfExitCodeIsConsideredValid(int exitCode, [NotNullWhen(false)] int[]? validExitCodes)
    {
        if (validExitCodes.IsNullOrEmpty())
            return true;

        for (var i = 0; i < validExitCodes.Length; i++)
        {
            if (validExitCodes[i] == exitCode)
                return true;
        }

        return false;
    }
}