using FluentAssertions;

namespace ChromeDevTools.Analyzer.Test;

[TestClass]
public class GeneratorTest
{
    public const string InputScript = @"fibonacci(int n)
{
    let nMinus1 = @NMinus1@fibonacci(n-1);
    let nMinus2 = @NMinus2@fibonacci(n-2);

    return @Add@(nMinus1 + nMinus2);
}";

    public const string FinalScript = @"fibonacci(int n)
{
    let nMinus1 = fibonacci(n-1);
    let nMinus2 = fibonacci(n-2);

    return (nMinus1 + nMinus2);
}";

    [TestMethod]
    public void NothingTodo()
    {
        // act
        var result = BreakPointInfo.ParseScript(FinalScript);

        // assert
        result.finalScript.Should().Be(FinalScript);
    }

    [TestMethod]
    public void Parsing()
    {
        // act
        var result = BreakPointInfo.ParseScript(InputScript);

        // assert
        result.finalScript.Should().Be(FinalScript);
        result.breakpoints.Should().HaveCount(3);

        result.breakpoints[0].FunctionName.Should().Be("fibonacci");
        result.breakpoints[0].Column.Should().Be(19);
        result.breakpoints[0].Line.Should().Be(2);
        result.breakpoints[0].BreakpointName.Should().Be("NMinus1");

        result.breakpoints[1].FunctionName.Should().Be("fibonacci");
        result.breakpoints[1].Column.Should().Be(19);
        result.breakpoints[1].Line.Should().Be(3);
        result.breakpoints[1].BreakpointName.Should().Be("NMinus2");

        result.breakpoints[2].FunctionName.Should().Be("Add");
        result.breakpoints[2].Column.Should().Be(12);
        result.breakpoints[2].Line.Should().Be(5);
        result.breakpoints[2].BreakpointName.Should().Be("Add");
    }
}
