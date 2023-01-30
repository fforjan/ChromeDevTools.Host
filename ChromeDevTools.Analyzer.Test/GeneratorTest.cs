using FluentAssertions;

namespace ChromeDevTools.Analyzer.Test;
[TestClass]
public class GeneratorTest
{
    [TestMethod]
    public void NothingTodo()
    {
        // act
        var result = BreakPointInfo.ParseScript(TestData.FinalScript);

        // assert
        result.finalScript.Should().Be(TestData.FinalScript);
    }

    [TestMethod]
    public void Parsing()
    {
        // act
        var result = BreakPointInfo.ParseScript(TestData.InputScript);

        // assert
        result.finalScript.Should().Be(TestData.FinalScript);
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

    [TestMethod]
    public void GeneratingScriptClassWithNamespace()
    {
        //arrange
        var info = (TestData.FinalScript,
                new List<BreakPointInfo>
                {
                    new BreakPointInfo { BreakpointName= "NMinus1", Column=19, Line = 2, FunctionName="fibonacci"},
                    new BreakPointInfo { BreakpointName= "NMinus2", Column=19, Line = 3, FunctionName="fibonacci"},
                    new BreakPointInfo { BreakpointName= "Add", Column=12, Line = 5, FunctionName="Add"},
                }
            );

        //act
        var script = CSharpGenerator.GenerateScript("Composed.Namespace.Fibonacci", info);

        //assert
        script.Should().Be(
            "namespace Composed.Namespace {\n" +
            TestData.GenerateScriptClass +
            "}\n");
    }

    [TestMethod]
    public void GeneratingScriptClassWithoutNamespace()
    {
        //arrange
        var info = (TestData.FinalScript,
                new List<BreakPointInfo>
                {
                    new BreakPointInfo { BreakpointName= "NMinus1", Column=19, Line = 2, FunctionName="fibonacci"},
                    new BreakPointInfo { BreakpointName= "NMinus2", Column=19, Line = 3, FunctionName="fibonacci"},
                    new BreakPointInfo { BreakpointName= "Add", Column=12, Line = 5, FunctionName="Add"},
                }
            );

        //act
        var script = CSharpGenerator.GenerateScript("Fibonacci", info);

        //assert
        script.Should().Be(TestData.GenerateScriptClass);
    }
}
