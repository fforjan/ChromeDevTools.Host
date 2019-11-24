using System;
using System.Threading;
using System.Threading.Tasks;
using ChromeDevTools.Host.Handlers.Runtime;

namespace ChromeDevTools.Host.Handlers.Debugging
{
    /// <summary>
    /// This class contains methods for applying breakpoing on top of chrome protocole sessions
    /// </summary>
    public static class SimSessionsHelper
    {
        /// <summary>
        /// This method will returns when :
        /// - the described breakable point is not enabled.
        /// - the breakabke point was hit  and the user triggered a 'continue' action
        /// Remark: while the application is still on break on this object,
        /// the context property will be available for runtime
        /// </summary>
        /// <param name="this">list of sessions</param>
        /// <param name="scriptUrl">script url</param>
        /// <param name="breakablePoint">breakable point name</param>
        /// <param name="context">context for runtime status</param>
        /// <returns></returns>
        public static Task BreakOn(this ChromeProtocolSessions @this, string scriptUrl, string breakablePoint, object context)
        {
            return @this.ForEach(_ => BreakOn(scriptUrl, breakablePoint, _, context, @this.CancelOnLastSessionDisposed));
        }

        private static Task BreakOn(string scriptUrl, string breakPointName, ChromeProtocolSession session, object context, CancellationToken cancellationToken)
        {
            var debugger = session.GetService<DebuggerHandler>();
            if (debugger.IsEnable)
            {
                var runtimeHanlder = session.GetService<RuntimeHandler>();
                var script = debugger.ScriptsByUrl[scriptUrl];
                var breakPoint = script.BreakableScriptPoint[breakPointName];

                if (debugger.BreakOn == BreakableScriptPoint.Any // any breakpoint, mainly set on pause
                    || debugger.BreakOn == breakPoint // a specific breakpoint, mainly set by run to location
                    || breakPoint.IsBreakPointSet) // an activated breakpoint
                {
                    debugger.BreakOn = null;

                    // a break point may have its own context,
                    // if required, register it
                    IDisposable localObjectContext = null;
                    if (context != null)
                    {
                        localObjectContext = runtimeHanlder.AllocateLocalObject(breakPoint.GetContextId(script), context);
                    }

                    var task = breakPoint.GetBreakPointTask(cancellationToken);

                    // do not forget to remove out context when we're done with the breakpoint
                    if (localObjectContext != null)
                    {
                        task.ContinueWith((_) => localObjectContext.Dispose());
                    }

                    return task;
                }
            }

            return Task.CompletedTask;
        }
    }
}
