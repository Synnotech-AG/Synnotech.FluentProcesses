﻿using System;
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
    [InlineData(true)]
    [InlineData(false)]
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

    public static TheoryData<string?> InvalidStrings { get; } =
        new ()
        {
            null,
            string.Empty,
            "\t"
        };
}