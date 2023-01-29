namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers.Debugging;

    public class MyDebuggerHandler : DebuggerHandler
    {
        public MyDebuggerHandler(params ScriptInfo[] scripts)
        {
            foreach (var script in scripts)
            {
                this.RegisterScripts(script);
            }
        }
    }

}