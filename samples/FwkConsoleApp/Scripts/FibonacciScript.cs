namespace FwkConsoleApp.Scripts
{
    using ChromeDevTools.Host.Handlers.Debugging;

    /// <summary>
    /// Breakable statement definition for fibonacci method
    /// </summary>
    public static class Fibonacci
    {
        public const string F0 = nameof(F0);
        public const string F1 = nameof(F1);
        public const string FN1Rec = nameof(FN1Rec);
        public const string FN2Rec = nameof(FN2Rec);
        public const string FNSum = nameof(FNSum);
    }

    public class FibonacciScript : ScriptInfo
    {
        public const string Script = 
@"Fibonacci(int n) {
    if(n == 0) return 0;
    if(n == 1) return 1;\
    return Fibonacci(n-1) + Fibonacci(n-2);
}
";
        public FibonacciScript() :
            base("", nameof(Fibonacci),
                    new BreakableScriptPoint(Fibonacci.F0, (1, 3, nameof(Fibonacci))),
                    new BreakableScriptPoint(Fibonacci.F1, (2, 3, nameof(Fibonacci))),
                    new BreakableScriptPoint(Fibonacci.FN1Rec, (3, 10, nameof(Fibonacci))),
                    new BreakableScriptPoint(Fibonacci.FN2Rec, (3, 26, nameof(Fibonacci))),
                    new BreakableScriptPoint(Fibonacci.FNSum, (3, 3, nameof(Fibonacci)))
                )
        {
        }
    }
}