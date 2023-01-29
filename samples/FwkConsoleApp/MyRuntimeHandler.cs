namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers.Debugging;
    using ChromeDevTools.Host.Handlers.Runtime;
    using ChromeDevTools.Host.Runtime.Runtime;
    using Jint;
    using Jint.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MyRuntimeHandler : RuntimeHandler
    {
        private Engine engine;
        private readonly ScriptInfo script;

        public MyRuntimeHandler(ScriptInfo script)
        {
            this.script = script;
        }

        private Engine CreateEngine()
        {
            return new Engine()
                .SetValue("echoOff", new Action(Program.EchoOff))
                .SetValue("echoOn", new Action(Program.EchoOn))
                .SetValue("getValues", new Func<int, int[]>(Program.GetValues))
                .SetValue("getValuesAsInterface", new Func<int, IEnumerable<Program.IPublicValue>>(Program.GetValuesAsInterface))

                .SetValue("getValueAsInterface", new Func<int, Program.IPublicValue>(Program.GetValueAsInterface))

                // allow to interface with breakpoint on the main script
                .SetValue("enableBreakpoint", new Func<string, bool>(EnableBreakPoint))
                .SetValue("disableBreakpoint", new Func<string, bool>(DisableBreakPoint))
                .SetValue("getBreakpoints", new Func<string[]>(GetBreakPoints));
        }

        private string[] GetBreakPoints()
        {
            return script.BreakableScriptPoint.Keys.ToArray();
        }

        private bool EnableBreakPoint(string breakPointName)
        {
            if(script.BreakableScriptPoint.TryGetValue(breakPointName, out var breakPoint))
            {
                return breakPoint.IsBreakPointSet = true;
            }

            return false;
        }

        private bool DisableBreakPoint(string breakPointName)
        {
            if (script.BreakableScriptPoint.TryGetValue(breakPointName, out var breakPoint))
            {
                return !(breakPoint.IsBreakPointSet = false);
            }

            return false;
        }

        protected override RemoteObject Evaluate(string expr)
        {
            engine ??= CreateEngine();
            
            if ("cls()" == expr.Trim())
            {
                engine = CreateEngine();
                return RemoteObjectCreator.Create("new engine created");
            }

            try
            {
                return RemoteObjectCreator.Create(engine.Evaluate(expr).ToObject().ToString());
            }
            catch (JavaScriptException e)
            {
                return RemoteObjectCreator.Create(e.Message);
            }
        }
    }
}