using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SampleConsoleApp;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddCommandLine(args)
                                                      .Build();
        var delayInterval = configuration.GetValue("delayInterval", 50);
        Console.WriteLine("Hello from Sample Console App");
        await Task.Delay(delayInterval);
        Console.WriteLine("Here is another message");
        await Task.Delay(delayInterval);

        var writeToErrorOutput = configuration.GetValue("writeToErrorOutput", true);
        
        if (writeToErrorOutput)
        {
            await Console.Error.WriteLineAsync("Here is an error message");
            await Task.Delay(delayInterval);
            await Console.Error.WriteLineAsync("Here are more errors");
            await Task.Delay(delayInterval);
        }

        var exitCode = configuration.GetValue("exitCode", 0);
        return exitCode;
    }
}
