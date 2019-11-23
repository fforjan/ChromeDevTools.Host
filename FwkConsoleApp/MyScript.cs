namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers.Debugging;

    public class MyScript : ScriptInfo
    {
        public MyScript() :
            base("console.log('Hello world')", "Hello World",
                    new BreakPoint("ConsoleLog", (0, 8, "log"))
                )
        {
        }
    }
}