namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers.Debugging;

    public class MyScript : ScriptInfo
    {
        public MyScript() :
            base("while(true)\n" +
                "{\n" +
                "  sleep(1000);\n" +
                "  console.log(i);\n" +
                "}", "Main",
                    new BreakPoint("sleep", (2, 3, "sleep")),
                    new BreakPoint("log", (3, 12, "log"))
                )
        {
        }
    }
}