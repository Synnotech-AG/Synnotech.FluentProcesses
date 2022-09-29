﻿using System;
using System.Diagnostics;
using System.Security;
using System.Text;
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