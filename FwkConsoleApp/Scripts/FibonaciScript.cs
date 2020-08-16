namespace FwkConsoleApp.Scripts
{
    using ChromeDevTools.Host.Handlers.Debugging;

    /// <summary>
    /// Breakable statement definition for fibonaci method
    /// </summary>
    public static class Fibonaci
    {
        public const string F0 = nameof(F0);
        public const string F1 = nameof(F1);
        public const string FN1Rec = nameof(FN1Rec);
        public const string FN2Rec = nameof(FN2Rec);
        public const string FNSum = nameof(FNSum);
    }

    public class FibonaciScript : ScriptInfo
    {
        public FibonaciScript() :
            base("Fibonaci(int n) {\n" +
                "   if(n == 0) return 0;\n" +
                "   if(n == 1) return 1;\n" +
                "   return Fibonaci(n-1) + Fibonaci(n-2);\n" +
                "}", nameof(Fibonaci),
                    new BreakableScriptPoint(Fibonaci.F0, (1, 3, nameof(Fibonaci))),
                    new BreakableScriptPoint(Fibonaci.F1, (2, 3, nameof(Fibonaci))),
                    new BreakableScriptPoint(Fibonaci.FN1Rec, (3, 10, nameof(Fibonaci))),
                    new BreakableScriptPoint(Fibonaci.FN2Rec, (3, 26, nameof(Fibonaci))),
                    new BreakableScriptPoint(Fibonaci.FNSum, (3, 3, nameof(Fibonaci)))
                )
        {
        }
    }
}