using System;
using System.Diagnostics;
using Light.GuardClauses;

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

    private ProcessStartInfo ProcessStartInfo { get; }
    private bool WasEnvironmentVariableSet { get; set; }

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
    /// Creates the process instance using all information gathered by this builder instance.
    /// </summary>
    public Process CreateProcess()
    {
        var process = new Process { StartInfo = ProcessStartInfo };
        // TODO: we need to set additional stuff here
        return process;
    }
}