using System.Diagnostics;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Represents the different behaviors for logging
/// used with the Standard Output and Standard Error
/// streams of <see cref="Process" /> instances. 
/// </summary>
public enum LoggingBehavior : byte
{
    /// <summary>
    /// Indicates that no logging will be performed for the corresponding output.
    /// </summary>
    NoLogging,
    
    /// <summary>
    /// Indicates that the corresponding events of the <see cref="Process" />
    /// will be used to get notified when a line was written to the Standard Output
    /// or Standard Error streams. Each line will be logged individually.
    /// This is the default behavior when logging is enabled. 
    /// </summary>
    LogOnEvent,
    
    /// <summary>
    /// <para>
    /// Indicates that logging will only occur after the process has exited.
    /// The corresponding stream will be read to the end and the resulting
    /// string will be passed to the logger.
    /// </para>
    /// <para>
    /// We only recommend using this behavior when a process call will be
    /// very short and when you can be certain that the process will not hang.   
    /// </para>
    /// </summary>
    LogAfterProcessExit
}