using System;
using System.ComponentModel;
using System.Diagnostics;
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="actualProcess" /> is null.</exception>
    public FluentProcess(Process actualProcess, LoggingSettings loggingSettings)
    {
        ActualProcess = actualProcess.MustNotBeNull();
        LoggingSettings = loggingSettings;
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

    private LoggingSettings LoggingSettings { get; }
    private bool WasStarted { get; set; }

    /// <summary>
    /// Disposes the <see cref="ActualProcess" />.
    /// </summary>
    public void Dispose() => ActualProcess.Dispose();

    /// <summary>
    /// Starts the process. Logging will be enabled if necessary.
    /// </summary>
    public void Start()
    {
        if (!WasStarted)
            ActualProcess.EnableLoggingIfNecessary(LoggingSettings);

        ActualProcess.Start();

        if (!WasStarted)
        {
            if (LoggingSettings.StandardOutputLoggingBehavior == LoggingBehavior.LogOnEvent)
                ActualProcess.BeginOutputReadLine();

            if (LoggingSettings.StandardErrorLoggingBehavior == LoggingBehavior.LogOnEvent)
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
}