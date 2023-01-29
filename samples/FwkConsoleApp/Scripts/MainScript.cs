namespace FwkConsoleApp.Scripts
{
    using ChromeDevTools.Host.Handlers.Debugging;


    /// <summary>
    /// Breakable statement definition for main method
    /// </summary>
    public static class Main
    {
        public const string SleepMethod = nameof(SleepMethod);
        public const string FibonacciMethod = nameof(FibonacciMethod);
        public const string LogMethod = nameof(LogMethod);
    }

    public class MainScript : ScriptInfo
    {
        public MainScript() :
            base("while(true)\n" +
                "{\n" +
                "  sleep(1000);\n" +
                "  console.log(Fibonacci(i));\n" +
                "}", nameof(Main),
                    new BreakableScriptPoint(Main.SleepMethod, (2, 2, "sleep")),
                    new BreakableScriptPoint(Main.FibonacciMethod, (3, 14, "Fibonacci")),
                    new BreakableScriptPoint(Main.LogMethod, (3, 10, "log"))
                )
        {
        }
    }
}