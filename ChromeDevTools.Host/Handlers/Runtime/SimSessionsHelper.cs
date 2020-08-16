namespace ChromeDevTools.Host.Handlers.Runtime
{
    using System.Threading.Tasks;

    /// <summary>
    /// This class contains methods for logging on top of chrome protocole sessions
    /// </summary>
    public static class SimSessionsHelper
    {
        /// <summary>
        /// Write message to the logger as log level
        /// </summary>
        public static Task Log(this ChromeProtocolSessions sessions, string message)
        {
            return sessions.ForEach(_ => _.GetService<RuntimeHandler>().Log(message));
        }

        /// <summary>
        /// Write message to the logger as warning level
        /// </summary>
        public static Task Warning(this ChromeProtocolSessions sessions, string message)
        {
            return sessions.ForEach(_ => _.GetService<RuntimeHandler>().Warning(message));
        }

        /// <summary>
        /// Write message to the logger as error level
        /// </summary>
        public static Task Error(this ChromeProtocolSessions sessions, string message)
        {
            return sessions.ForEach(_ => _.GetService<RuntimeHandler>().Error(message));
        }

        /// <summary>
        /// Write message to the logger as debug level
        /// </summary>
        public static Task Debug(this ChromeProtocolSessions sessions, string message)
        {
            return sessions.ForEach(_ => _.GetService<RuntimeHandler>().Debug(message));
        }
    }
}