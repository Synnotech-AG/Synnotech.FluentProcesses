using System.Diagnostics;
using Light.GuardClauses;

namespace Synnotech.FluentProcesses;

public sealed class ProcessBuilder
{
    public ProcessBuilder() : this(new ()) { }
    
    public ProcessBuilder(ProcessStartInfo processStartInfo)
    {
        ProcessStartInfo = processStartInfo.MustNotBeNull();
    }
    
    private ProcessStartInfo ProcessStartInfo { get; }
    private bool WasEnvironmentVariableSet { get; set; }
    
    public ProcessBuilder AddEnvironmentVariable(string name, string value)
    {
        ProcessStartInfo.EnvironmentVariables.Add(name, value);
        WasEnvironmentVariableSet = true;
        return this;
    }

    public ProcessBuilder Clone()
    {
        var processStartInfoClone = ProcessStartInfo.Clone(WasEnvironmentVariableSet);
        return new (processStartInfoClone);
    }

    public Process CreateProcess()
    {
        var process = new Process { StartInfo = ProcessStartInfo };
        // TODO: we need to set additional stuff here
        return process;
    }
}