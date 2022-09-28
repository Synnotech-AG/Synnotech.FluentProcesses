using System;
using System.Diagnostics;
using Light.GuardClauses;

namespace Synnotech.FluentProcesses;

/// <summary>
/// Provides extensions methods for processes.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Creates a deep clone of the <paramref name="processStartInfo" /> instance.
    /// Environment variables will only be copied when
    /// <paramref name="copyEnvironment" /> is set to true. This is done to
    /// prevent loading environment variables unless absolutely necessary
    /// (the environment variables are loaded when
    /// <see cref="ProcessStartInfo.Environment" /> is accessed for the first time).
    /// </summary>
    /// <param name="processStartInfo">The instance that should be cloned.</param>
    /// <param name="copyEnvironment">
    /// The value indicating whether the environment variables associated with the
    /// process start info instance should be copied, too. The default value is false.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="processStartInfo" /> is null.</exception>
    public static ProcessStartInfo Clone(this ProcessStartInfo processStartInfo, bool copyEnvironment = false)
    {
        processStartInfo.MustNotBeNull();

        var clone = new ProcessStartInfo
        {
            Arguments = processStartInfo.Arguments,
            Domain = processStartInfo.Domain,
            CreateNoWindow = processStartInfo.CreateNoWindow,
            Password = processStartInfo.Password,
            Verb = processStartInfo.Verb,
            ErrorDialog = processStartInfo.ErrorDialog,
            FileName = processStartInfo.FileName,
            UserName = processStartInfo.UserName,
            WindowStyle = processStartInfo.WindowStyle,
            WorkingDirectory = processStartInfo.WorkingDirectory,
            LoadUserProfile = processStartInfo.LoadUserProfile,
            RedirectStandardError = processStartInfo.RedirectStandardError,
            RedirectStandardInput = processStartInfo.RedirectStandardInput,
            RedirectStandardOutput = processStartInfo.RedirectStandardOutput,
            StandardErrorEncoding = processStartInfo.StandardErrorEncoding,
            StandardOutputEncoding = processStartInfo.StandardOutputEncoding,
            UseShellExecute = processStartInfo.UseShellExecute,
            ErrorDialogParentHandle = processStartInfo.ErrorDialogParentHandle,
            PasswordInClearText = processStartInfo.PasswordInClearText
        };

        if (!copyEnvironment)
            return clone;

        foreach (var keyValuePair in processStartInfo.Environment)
        {
            clone.Environment.Add(keyValuePair);
        }

        return clone;
    }
}