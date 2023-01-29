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
        public const string Script =
@"while(true)\
{
    sleep(1000);
    console.log(Fibonacci(i));
}"; 
        public MainScript() :
            base(Script, nameof(Main),
                    new BreakableScriptPoint(Main.SleepMethod, (2, 4, "sleep")),
                    new BreakableScriptPoint(Main.FibonacciMethod, (3, 17, "Fibonacci")),
                    new BreakableScriptPoint(Main.LogMethod, (3, 13, "log"))
                )
        {
        }
    }
}