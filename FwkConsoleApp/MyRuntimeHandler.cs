namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers;
    using ChromeDevTools.Host.Runtime.Runtime;
    using Jint;
    using Jint.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MyRuntimeHandler : RuntimeHandler
    {

        private Engine engine;


        private Engine CreateEngine()
        {
            return new Engine()
                .SetValue("echoOff", new Action(Program.EchoOff))
                .SetValue("echoOn", new Action(Program.EchoOn))
                .SetValue("getValues", new Func<int, int[]>(Program.GetValues))
                .SetValue("getValuesAsInterface", new Func<int, IEnumerable<Program.IPublicValue>>(Program.GetValuesAsInterface))

                .SetValue("getValueAsInterface", new Func<int, Program.IPublicValue>(Program.GetValueAsInterface));
        }

        protected override RemoteObject Evaluate(string expr)
        {
            if (engine == null)
            {
                engine = CreateEngine();
            }
            if ("cls()" == expr.Trim())
            {
                engine = CreateEngine();
                return new RemoteObject
                {
                    Type = "string",
                    Value = "new engine created"
                };
            }

            try
            {
                engine = engine.Execute(expr);

                return new RemoteObject
                {
                    Type = "string",
                    Value = engine.GetCompletionValue().ToString()
                };
            }
            catch (JavaScriptException e)
            {
                return new RemoteObject
                {
                    Type = "string",
                    Value = "exception:  " + e.Message
                };
            }
        }
    }
}