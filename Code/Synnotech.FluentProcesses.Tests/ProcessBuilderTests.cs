using System;
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

    public static TheoryData<string?> InvalidStrings { get; } =
        new ()
        {
            null,
            string.Empty,
            "\t"
        };
}