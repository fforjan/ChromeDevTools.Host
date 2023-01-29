namespace ChromeDevTools.Host
{
    using System.Net.WebSockets;

    /// <summary>
    /// Application should implement to describe their environment available for the chrome dev protocol
    /// </summary>
    public interface IChromeSessionProvider {

        /// <summary>
        /// Get the protocol version information for the current application
        /// </summary>
        /// <returns></returns>
        ChromeSessionProtocolVersion GetProtocolVersion();

        /// <summary>
        /// Get the list of session available for debugging.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="serverPort"></param>
        /// <returns></returns>
        ChromeSessionInstanceDescription[] GetSessionInstanceDescriptions(string serverName, int serverPort);

        /// <summary>
        /// Create a session using a specific web socket.
        /// </summary>
        /// <param name="webSocket">web socket communication</param>
        /// <param name="id">chrome session instance id. it should be matching a <see cref="ChromeSessionInstanceDescription.Id"/>
        /// returned by <see cref="GetSessionInstanceDescriptions(string, int)"/></param>
        /// <returns>a new session</returns>
        ChromeProtocolSession CreateSession(WebSocket webSocket, string id);
    }
}
