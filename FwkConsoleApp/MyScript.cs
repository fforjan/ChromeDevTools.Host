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
                    new BreakableScriptPoint("sleep", (2, 2, "sleep")),
                    new BreakableScriptPoint("log", (3, 11, "log"))
                )
        {
        }
    }
}