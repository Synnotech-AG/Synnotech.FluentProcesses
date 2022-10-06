using System;
using System.Diagnostics;
using System.Security;
using System.Text;
using FluentAssertions;
using Synnotech.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.FluentProcesses.Tests;

#pragma warning disable CA1416 // Some properties only work on Windows, but these tests are skipped on other platforms

public sealed class ProcessBuilderTests
{
    public ProcessBuilderTests(ITestOutputHelper output) => Output = output;

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

    [SkippableTheory]
    [InlineData("site.contoso.com")]
    [InlineData("synnotech.de")]
    public void SetDomain(string domain)
    {
        SkipIfNotWindows();
        
        using var process = ProcessBuilder.WithDomain(domain)
                                          .CreateProcess();

        process.StartInfo.Domain.Should().BeSameAs(domain);
    }

    private static void SkipIfNotWindows()
    {
#if NET6_0
        Skip.IfNot(OperatingSystem.IsWindows());
#endif
    }

    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    public void UnsetDomain(string unsetValue)
    {
        SkipIfNotWindows();
        
        using var process = ProcessBuilder.WithDomain("Foo")
                                          .WithDomain(unsetValue)
                                          .CreateProcess();

        process.StartInfo.Domain.Should().BeEmpty();
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
        SkipIfNotWindows();

        var secureString = CreateSecureString();

        using var process = ProcessBuilder.WithPassword(secureString)
                                          .CreateProcess();

        process.StartInfo.Password.Should().BeSameAs(secureString);
    }

    [SkippableFact]
    public void UnsetPassword()
    {
        SkipIfNotWindows();

        var secureString = CreateSecureString();

        using var process = ProcessBuilder.WithPassword(secureString)
                                          .WithPassword(null)
                                          .CreateProcess();

        process.StartInfo.Password.Should().BeNull();
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

    [SkippableTheory]
    [MemberData(nameof(BooleanValues))]
    public void SetLoadUserProfile(bool value)
    {
        SkipIfNotWindows();
        
        using var process = ProcessBuilder.WithLoadUserProfile(value)
                                          .CreateProcess();

        process.StartInfo.LoadUserProfile.Should().Be(value);
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

    [Theory]
    [MemberData(nameof(BooleanValues))]
    public void SetRedirectStandardOutput(bool value)
    {
        using var process = ProcessBuilder.WithRedirectStandardOutput(value)
                                          .CreateProcess();

        process.StartInfo.RedirectStandardOutput.Should().Be(value);
    }

    [Theory]
    [MemberData(nameof(Encodings))]
    public void SetStandardErrorEncoding(Encoding? encoding)
    {
        using var process = ProcessBuilder.WithStandardErrorEncoding(encoding)
                                          .CreateProcess();

        process.StartInfo.StandardErrorEncoding.Should().BeSameAs(encoding);
    }

    [Theory]
    [MemberData(nameof(Encodings))]
    public void SetStandardOutputEncoding(Encoding? encoding)
    {
        using var process = ProcessBuilder.WithStandardOutputEncoding(encoding)
                                          .CreateProcess();

        process.StartInfo.StandardOutputEncoding.Should().BeSameAs(encoding);
    }

    [Theory]
    [MemberData(nameof(Encodings))]
    public void SetEncoding(Encoding? encoding)
    {
        using var process = ProcessBuilder.WithEncoding(encoding)
                                          .CreateProcess();

        process.StartInfo.StandardOutputEncoding.Should().BeSameAs(encoding);
        process.StartInfo.StandardErrorEncoding.Should().BeSameAs(encoding);
    }

    [Theory]
    [MemberData(nameof(BooleanValues))]
    public void SetUseShellExecute(bool value)
    {
        using var process = ProcessBuilder.WithUseShellExecute(value)
                                          .CreateProcess();

        process.StartInfo.UseShellExecute.Should().Be(value);
    }

    [Fact]
    public void DisableShellExecute()
    {
        using var process = ProcessBuilder.DisableShellExecute()
                                          .CreateProcess();

        process.StartInfo.UseShellExecute.Should().BeFalse();
    }

    [Fact]
    public void SetErrorDialogParentHandle()
    {
        var handle = new IntPtr(3);
        using var process = ProcessBuilder.WithErrorDialogParentHandle(handle)
                                          .CreateProcess();

        // ReSharper disable once HeapView.BoxingAllocation -- this is only a test, not production code
        process.StartInfo.ErrorDialogParentHandle.Should().Be(handle);
    }

    [SkippableTheory]
    [InlineData("secret")]
    [InlineData("This password is really safe")]
    public void SetPasswordInClearText(string password)
    {
        SkipIfNotWindows();
        
        using var process = ProcessBuilder.WithPasswordInClearText(password)
                                          .CreateProcess();

        process.StartInfo.PasswordInClearText.Should().BeSameAs(password);
    }

    [Fact]
    public void CloneBuilder()
    {
        var builderClone = ProcessBuilder.WithFileName("SomeApp.exe")
                                         .WithArguments("--encryptEverything")
                                         .DisableShellExecute()
                                         .WithCreateNoWindow()
                                         .WithEncoding(Encoding.UTF8)
                                         .Clone();

        using var process1 = ProcessBuilder.CreateProcess();
        using var process2 = builderClone.CreateProcess();

        process1.StartInfo.Should().NotBeSameAs(process2.StartInfo);
        process1.StartInfo.Should().BeEquivalentTo(process2.StartInfo);
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

    public static TheoryData<Encoding?> Encodings { get; } =
        new () { Encoding.UTF8, Encoding.Unicode, Encoding.ASCII, null };
}