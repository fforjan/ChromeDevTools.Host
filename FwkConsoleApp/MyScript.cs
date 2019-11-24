namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers.Debugging;

    public class MyScript : ScriptInfo
    {
        public MyScript() :
            base("while(true)\n" +
                "{\n" +
                "  sleep(1000);\n" +
                "  console.log(Fibonaci(i));\n" +
                "}", "Main",
                    new BreakableScriptPoint("sleep", (2, 2, "sleep")),
                    new BreakableScriptPoint("Fibonaci", (3, 14, "Fibonaci")),
                    new BreakableScriptPoint("log", (3, 10, "log"))
                )
        {
        }
    }

    public class Fibonaci : ScriptInfo
    {
        public Fibonaci() :
            base("Fibonaci(int n) {\n" +
                "   if(n == 0) return 0;\n" +
                "   if(n == 1) return 1;\n" +
                "   return Fibonaci(n-1) + Fibonaci(n-2);\n" +
                "}", "Fibonaci",
                    new BreakableScriptPoint("f0", (1, 3, "f0")),
                    new BreakableScriptPoint("f1", (2, 3, "f1")),
                    new BreakableScriptPoint("fsum", (3, 3, "fsum"))
                )
        {
        }
    }
}