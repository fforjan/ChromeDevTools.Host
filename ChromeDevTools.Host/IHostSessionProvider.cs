using System;
using System.Net.WebSockets;

namespace ChromeDevTools.Host
{
    public interface IChromeSessionProvider {

        ChromeSessionProtocolVersion GetProtocolVersion();

        ChromeSessionInstanceDescription[] GetSessionInstanceDescriptions(string serverName, int serverPort);

        ChromeProtocolSession CreateSession(WebSocket webSocket, string guid);
    }
}
