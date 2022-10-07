using System;
using System.Diagnostics;
using Light.GuardClauses;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Represents a structure that encapsulates two optional event handlers,
/// one for receiving data for the standard output, one for the
/// standard error.
/// </summary>
/// <param name="StandardOutputHandler">The handler that will be called when data is received from the standard output.</param>
/// <param name="StandardErrorHandler">The handler that will be called when data is received from the standard error.</param>
public readonly record struct StreamHandlers(DataReceivedEventHandler? StandardOutputHandler,
                                             DataReceivedEventHandler? StandardErrorHandler)
{
    /// <summary>
    /// Attaches the <see cref="StandardOutputHandler" /> and <see cref="StandardErrorHandler" />
    /// to the specified process. The handlers are only attached if they are not null.
    /// </summary>
    /// <param name="process">The process the handlers will be attached to.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="process" /> is null.</exception>
    public void AttachHandlersIfNecessary(Process process)
    {
        process.MustNotBeNull();

        if (StandardOutputHandler is not null)
        {
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += StandardOutputHandler;
        }
        
        if (StandardErrorHandler is not null)
        {
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += StandardErrorHandler;
        }
    }
}