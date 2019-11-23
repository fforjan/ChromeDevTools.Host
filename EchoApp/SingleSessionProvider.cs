using System;
using System.Net.WebSockets;
using ChromeDevTools.Host;
using ChromeDevTools.Host.Handlers;
using ChromeDevTools.Host.Handlers.Debugging;
using Microsoft.AspNetCore.Builder;

namespace EchoApp
{
    internal class SingleSessionProvider : IChromeSessionProvider
    {
        private readonly Guid SingleSessiongId = Guid.NewGuid();

        public ChromeProtocolSession CreateSession(WebSocket webSocket, string guid)
        {
            return new ChromeProtocolSession(webSocket, new RuntimeHandler(), new DebuggerHandler(), new ProfilerHandler());
        }

        public ChromeSessionProtocolVersion GetProtocolVersion()
        {
            return ChromeSessionProtocolVersion.CreateFrom("AspNetCore", typeof(IApplicationBuilder).Assembly.GetName().Version);
        }

        public ChromeSessionInstanceDescription[] GetSessionInstanceDescriptions(string serverName, int serverPort)
        {
            return new[] { ChromeSessionInstanceDescription.CreateFrom(
                                serverName, serverPort,
                                "virtual instance for AspNetCore",
                                "virtual instance",
                                "https://scontent-lax3-2.xx.fbcdn.net/v/t31.0-8/26850424_10215610615764193_3403737823383610422_o.jpg?_nc_cat=105&_nc_oc=AQmrv1vPT2ln4k0aEVP5lols-Jabc-VynxvBqV11LSLI7rma9_7-iRSwuLOcx2EVzALcoBotSdD76ryX_JQC42Di&_nc_ht=scontent-lax3-2.xx&oh=a0881f639de78a72d7f550a188ba4aa6&oe=5E204509",
                                SingleSessiongId
                            )};
        }
    }
}