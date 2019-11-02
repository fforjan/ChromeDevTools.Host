namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers;
    using ChromeDevTools.Host.Runtime.Runtime;
    using Jint;
    using System;
    using System.Linq;

    public class MyRuntimeHandler : RuntimeHandler
    {
        protected override RemoteObject Evaluate(string expr)
        {
            var js = new Engine()
                .SetValue("echoOff", new Action(Program.EchoOff))
                .SetValue("echoOn", new Action(Program.EchoOn))
                .SetValue("getValues", new Func<int, int[]>(Program.GetValues))
                .Execute(expr);

            return new RemoteObject
            {
                Type = "string",
                Value = js.GetCompletionValue().ToString()
            };
        }
    }
}