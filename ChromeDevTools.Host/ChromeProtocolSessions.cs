namespace ChromeDevTools.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ChromeDevTools.Host.Handlers.Debugging;
    using ChromeDevTools.Host.Handlers.Runtime;

    public class ChromeProtocolSessions
    {
        private readonly object locker = new object();

        private List<ChromeProtocolSession> sessions = new List<ChromeProtocolSession>();

        public IReadOnlyList<ChromeProtocolSession> Sessions
        {
            get => sessions;
        }

        public IDisposable Register(ChromeProtocolSession session)
        {
            lock (locker)
            {
                sessions = new List<ChromeProtocolSession>(sessions) { session };
            }

            return new Dispose(this, session);
        }

        public Task ForEach(Func<ChromeProtocolSession, Task> chromeSessionAction)
        {
            return Task.WhenAll(sessions.Select(chromeSessionAction));
        }

        public Task BreakOn(string scriptUrl, string breakPointName, object context)
        {
            return Task.WhenAll(sessions.Select(_ => BreakOn(scriptUrl, breakPointName, _, context)));
        }

        private Task BreakOn(string scriptUrl, string breakPointName, ChromeProtocolSession session, object context)
        {
            var debugger = session.GetService<DebuggerHandler>();
            if (debugger.IsEnable)
            {
                var runtimeHanlder = session.GetService<RuntimeHandler>();

                return debugger.ScriptsByUrl[scriptUrl].BreakPoints[breakPointName].GetBreakPointTask(
                    lastSessionDisposed.Token,
                    () => { runtimeHanlder.LocalObject = context; },
                    () => { runtimeHanlder.LocalObject = null; }
                    );
            }

            return Task.CompletedTask;
        }

        private CancellationTokenSource lastSessionDisposed = new CancellationTokenSource();

        private class Dispose : IDisposable
        {
            private readonly ChromeProtocolSessions sessions;
            private readonly ChromeProtocolSession session;

            public Dispose(ChromeProtocolSessions sessions, ChromeProtocolSession session)
            {
                this.sessions = sessions;
                this.session = session;
            }

            void IDisposable.Dispose()
            {
                lock (sessions.locker)
                {
                    sessions.sessions = sessions.sessions.Where(_ => _ != session).ToList();

                    if(sessions.sessions.Count == 0)
                    {
                        sessions.lastSessionDisposed.Cancel();
                        sessions.lastSessionDisposed = new CancellationTokenSource();
                    }
                }
            }
        }
    }
}
