namespace ChromeDevTools.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ChromeDevTools.Host.Handlers.Debugging;

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

        public Task BreakOn(string scriptUrl, string breakPointName)
        {
            return Task.WhenAll(sessions.Select(_ => BreakOn(scriptUrl, breakPointName, _)));
        }

        private Task BreakOn(string scriptUrl, string breakPointName, ChromeProtocolSession session)
        {
            var debugger = session.GetService<DebuggerHandler>();
            if (debugger.IsEnable)
            {
                return debugger.ScriptsByUrl[scriptUrl].BreakPoints[breakPointName].GetBreakPointTask(lastSessionDisposed.Token);
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
