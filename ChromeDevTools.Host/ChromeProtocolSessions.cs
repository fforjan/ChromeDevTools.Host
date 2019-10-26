namespace ChromeDevTools.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ChromeProtocolSessions
    {
        private readonly object locker = new object();

        private List<ChromeProtocolSession> sessions = new List<ChromeProtocolSession>();

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
                }
            }
        }
    }
}
