using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Represents an exception that is thrown when an invalid exit code is
/// returned by a process.
/// </summary>
public class InvalidExitCodeException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="InvalidExitCodeException" />.
    /// </summary>
    /// <param name="process">The process instance whose exit code is invalid.</param>
    /// <param name="validExitCodes">The valid exit codes for the process.</param>
    /// <param name="message">The message of the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="process" /> or <paramref name="validExitCodes" /> are null.</exception>
    /// <exception cref="EmptyCollectionException">Thrown when <paramref name="validExitCodes" /> is an empty array.</exception>
    public InvalidExitCodeException(FluentProcess process, int[] validExitCodes, string message) : base(message)
    {
        Process = process.MustNotBeNull();
        ValidExitCodes = validExitCodes.MustNotBeNullOrEmpty();
    }

    /// <summary>
    /// Gets the process whose exit code is invalid.
    /// </summary>
    public FluentProcess Process { get; }

    /// <summary>
    /// Gets the array containing all valid exit codes.
    /// </summary>
    public int[] ValidExitCodes { get; }
}