using System;
using System.Diagnostics;
using System.Security;
using FluentAssertions;
using Synnotech.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.FluentProcesses.Tests;

public sealed class ProcessBuilderTests
{
    public ProcessBuilderTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }
    private ProcessBuilder ProcessBuilder { get; } = new ();

    [Theory]
    [InlineData("Foo", "Bar")]
    [InlineData("baz", "qux")]
    public void AddEnvironmentVariable(string name, string value)
    {
        using var process = ProcessBuilder.AddEnvironmentVariable(name, value)
                                          .CreateProcess();

        process.StartInfo.Environment.Should().Contain(name, value);
    }

    [Theory]
    [MemberData(nameof(InvalidStrings))]
    public void AddEnvironmentVariableInvalidName(string invalidName)
    {
        var act = () => ProcessBuilder.AddEnvironmentVariable(invalidName, "Foo");

        act.Should().Throw<ArgumentException>()
           .Which.ShouldBeWrittenTo(Output);
    }

    [Theory]
    [MemberData(nameof(InvalidStrings))]
    public void AddEnvironmentVariableInvalidValue(string invalidValue)
    {
        var act = () => ProcessBuilder.AddEnvironmentVariable("Foo", invalidValue);

        act.Should().Throw<ArgumentException>()
           .Which.ShouldBeWrittenTo(Output);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--all")]
    [InlineData("/p:Configuration=Release /t:clean;restore;build")]
    public void SetArguments(string arguments)
    {
        using var process = ProcessBuilder.WithArguments(arguments)
                                          .CreateProcess();

        process.StartInfo.Arguments.Should().BeSameAs(arguments);
    }

    [Fact]
    public void UnsetArguments()
    {
        using var process = ProcessBuilder.WithArguments("--all")
                                          .WithArguments(string.Empty)
                                          .CreateProcess();

        process.StartInfo.Arguments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("site.contoso.com")]
    [InlineData("synnotech.de")]
    public void SetDomain(string domain)
    {
        using var process = ProcessBuilder.WithDomain(domain)
                                          .CreateProcess();

#pragma warning disable CA1416 // The setter can be called although AD domains are only supported on Windows
        process.StartInfo.Domain.Should().BeSameAs(domain);
#pragma warning restore CA1416
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void UnsetDomain(string unsetValue)
    {
        using var process = ProcessBuilder.WithDomain("Foo")
                                          .WithDomain(unsetValue)
                                          .CreateProcess();

#pragma warning disable CA1416 // The setter can be called although AD domains are only supported on Windows
        process.StartInfo.Domain.Should().BeEmpty();
#pragma warning restore CA1416
    }

    [Theory]
    [MemberData(nameof(BooleanValues))]
    public void SetCreateNoWindow(bool value)
    {
        using var process = ProcessBuilder.WithCreateNoWindow(value)
                                          .CreateProcess();

        process.StartInfo.CreateNoWindow.Should().Be(value);
    }

    [SkippableFact]
    public void SetPassword()
    {
        Skip.IfNot(OperatingSystem.IsWindows());
        
        var secureString = CreateSecureString();
        
        using var process = ProcessBuilder.WithPassword(secureString)
                                          .CreateProcess();

#pragma warning disable CA1416 // The setter can be called although user and password for processes only work on Windows
        process.StartInfo.Password.Should().BeSameAs(secureString);
#pragma warning restore CA1416
    }

    [SkippableFact]
    public void UnsetPassword()
    {
        Skip.IfNot(OperatingSystem.IsWindows());
        
        var secureString = CreateSecureString();

        using var process = ProcessBuilder.WithPassword(secureString)
                                          .WithPassword(null)
                                          .CreateProcess();
        
#pragma warning disable CA1416 // The setter can be called although user and password for processes only work on Windows
        process.StartInfo.Password.Should().BeNull();
#pragma warning restore CA1416
    }

    private static SecureString CreateSecureString()
    {
        var secureString = new SecureString();
        secureString.AppendChar('s');
        secureString.AppendChar('e');
        secureString.AppendChar('c');
        secureString.AppendChar('r');
        secureString.AppendChar('e');
        secureString.AppendChar('t');
        secureString.MakeReadOnly();
        return secureString;
    }

    [Theory]
    [InlineData("")]
    [InlineData("Foo")]
    [InlineData("bar")]
    public void SetVerb(string verb)
    {
        using var process = ProcessBuilder.WithVerb(verb)
                                          .CreateProcess();

        process.StartInfo.Verb.Should().BeSameAs(verb);
    }

    [Theory]
    [MemberData(nameof(BooleanValues))]
    public void SetErrorDialog(bool value)
    {
        using var process = ProcessBuilder.WithErrorDialog(value)
                                          .CreateProcess();

        process.StartInfo.ErrorDialog.Should().Be(value);
    }

    [Theory]
    [InlineData("MyApp.exe")]
    [InlineData(@"C:\SomeFolder\SomeApp.exe")]
    [InlineData(@".\MyExcelFile.xlsx")]
    public void SetFileName(string fileName)
    {
        using var process = ProcessBuilder.WithFileName(fileName)
                                          .CreateProcess();

        process.StartInfo.FileName.Should().BeSameAs(fileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Frederic")]
    [InlineData("jon.doe@contoso.com")]
    public void SetUserName(string userName)
    {
        using var process = ProcessBuilder.WithUserName(userName)
                                          .CreateProcess();

        process.StartInfo.UserName.Should().BeSameAs(userName);
    }

    [Theory]
    [InlineData(ProcessWindowStyle.Normal)]
    [InlineData(ProcessWindowStyle.Hidden)]
    [InlineData(ProcessWindowStyle.Minimized)]
    [InlineData(ProcessWindowStyle.Maximized)]
    public void SetWindowStyle(ProcessWindowStyle windowStyle)
    {
        using var process = ProcessBuilder.WithWindowStyle(windowStyle)
                                          .CreateProcess();

        process.StartInfo.WindowStyle.Should().Be(windowStyle);
    }

    [Theory]
    [InlineData(@"C:\Temp")]
    [InlineData("./SomeOtherDirectory")]
    public void SetWorkingDirectory(string workingDirectory)
    {
        using var process = ProcessBuilder.WithWorkingDirectory(workingDirectory)
                                          .CreateProcess();

        process.StartInfo.WorkingDirectory.Should().BeSameAs(workingDirectory);
    }

    [Theory]
    [MemberData(nameof(BooleanValues))]
    public void SetLoadUserProfile(bool value)
    {
        using var process = ProcessBuilder.WithLoadUserProfile(value)
                                          .CreateProcess();

#pragma warning disable CA1416 // The setter can be called although Load User Profile only works on Windows
        process.StartInfo.LoadUserProfile.Should().Be(value);
#pragma warning restore CA1416
    }

    [Theory]
    [MemberData(nameof(BooleanValues))]
    public void SetRedirectStandardError(bool value)
    {
        using var process = ProcessBuilder.WithRedirectStandardError(value)
                                          .CreateProcess();

        process.StartInfo.RedirectStandardError.Should().Be(value);
    }
    
    [Theory]
    [MemberData(nameof(BooleanValues))]
    public void SetRedirectStandardInput(bool value)
    {
        using var process = ProcessBuilder.WithRedirectStandardInput(value)
                                          .CreateProcess();

        process.StartInfo.RedirectStandardInput.Should().Be(value);
    }

    public static TheoryData<string?> InvalidStrings { get; } =
        new ()
        {
            null,
            string.Empty,
            "\t"
        };

    public static TheoryData<bool> BooleanValues { get; } =
        new () { true, false };
}