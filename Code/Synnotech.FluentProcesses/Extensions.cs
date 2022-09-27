using System.Diagnostics;

namespace Synnotech.FluentProcesses;

public static class Extensions
{
    public static ProcessStartInfo Clone(this ProcessStartInfo processStartInfo, bool copyEnvironment = false)
    {
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