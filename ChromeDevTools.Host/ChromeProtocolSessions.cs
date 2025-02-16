namespace ChromeDevTools.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manage the overall set of sessions.
    /// The provided methods are all concurrent-friendly.
    /// </summary>
    public class ChromeProtocolSessions
    {
        /// <summary>
        /// Locker object for being concurrent-friendly
        /// </summary>
        private readonly object locker = new();

        /// <summary>
        /// Cancellation token source for <see cref="CancelOnLastSessionDisposed"/>.
        /// </summary>
        private CancellationTokenSource lastSessionDisposed = new();

        /// <summary>
        /// List of active sessions.
        /// </summary>
        private List<ChromeProtocolSession> sessions = new();

        /// <summary>
        /// Get active sessions.
        /// Note: it is preferable to use <see cref="ForEach(Func{ChromeProtocolSession, Task})"/> when executing actions on all sessions.
        /// </summary>
        public IReadOnlyList<ChromeProtocolSession> Sessions
        {
            get => sessions;
        }

        /// <summary>
        /// Register a session.
        /// The session is considered active up to the point the <see cref="IDisposable.Dispose"/>
        /// method is call on the returned valud
        /// </summary>
        /// <param name="session">session to register as active</param>
        /// <returns>disposable object.</returns>
        public IDisposable Register(ChromeProtocolSession session)
        {
            lock (locker)
            {
                sessions = new List<ChromeProtocolSession>(sessions) { session };
            }

            return new Dispose(this, session);
        }

        /// <summary>
        /// Apply action 
        /// </summary>
        /// <param name="chromeSessionAction"></param>
        /// <returns></returns>
        public Task ForEach(Func<ChromeProtocolSession, Task> chromeSessionAction)
        {
            var currentSessions = sessions;
            if(currentSessions.Count == 0)
            {
                return Task.CompletedTask; // avoid time spend on when all construction
            }

            return Task.WhenAll(sessions.Select(chromeSessionAction));
        }  

      
        /// <summary>
        /// Cancellation token for current sessions.
        /// This token will be in cancelled when the last session will be closed.
        /// Note: the token will not be in cancelled state if there is no session.
        /// It will be cancelled after one or more added sessions are closed.
        /// </summary>
        public  CancellationToken CancelOnLastSessionDisposed { get { return lastSessionDisposed.Token; } }

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
