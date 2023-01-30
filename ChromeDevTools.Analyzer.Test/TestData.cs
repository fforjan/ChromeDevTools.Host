namespace ChromeDevTools.Analyzer.Test;

public static class TestData
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

    public const string GenerateScriptClass = @"public partial class Fibonacci : ChromeDevTools.Host.Handlers.Debugging.ScriptInfo
{
    public const string Script = @""fibonacci(int n)
{
    let nMinus1 = fibonacci(n-1);
    let nMinus2 = fibonacci(n-2);

    return (nMinus1 + nMinus2);
}"";

    public Fibonacci() : base(Script, nameof(Fibonacci),
        new ChromeDevTools.Host.Handlers.Debugging.BreakableScriptPoint(NMinus1, (2, 19, ""fibonacci"")),
        new ChromeDevTools.Host.Handlers.Debugging.BreakableScriptPoint(NMinus2, (3, 19, ""fibonacci"")),
        new ChromeDevTools.Host.Handlers.Debugging.BreakableScriptPoint(Add, (5, 12, ""Add""))
    )
    {}
}
";
}
