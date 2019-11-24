namespace FwkConsoleApp.Scripts
{
    using ChromeDevTools.Host.Handlers.Debugging;

    public static class Fibonaci
    {
        public const string F0 = nameof(F0);
        public const string F1 = nameof(F0);
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
                    new BreakableScriptPoint(Fibonaci.F0, (1, 3, "f0")),
                    new BreakableScriptPoint(Fibonaci.F1, (2, 3, "f1")),
                    new BreakableScriptPoint(Fibonaci.FN1Rec, (3, 10, "FN1")),
                    new BreakableScriptPoint(Fibonaci.FN2Rec, (3, 26, "FN2")),
                    new BreakableScriptPoint(Fibonaci.FNSum, (3, 3, "fsum"))
                )
        {
        }
    }
}