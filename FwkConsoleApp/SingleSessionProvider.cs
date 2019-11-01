using System;
using System.Net.WebSockets;
using System.Reflection;
using ChromeDevTools.Host;
using ChromeDevTools.Host.Handlers;

namespace FwkConsoleApp
{
    internal class SingleSessionProvider : IChromeSessionProvider
    {
        private readonly Guid SingleSessiongId = Guid.NewGuid();
        public ChromeProtocolSession CreateSession(WebSocket webSocket, string guid)
        {
            return new ChromeProtocolSession(webSocket, new MyRuntimeHandler(), new DebuggerHandler(), new ProfilerHandler(), new MyHeapProfilerHandler());
        }

        public ChromeSessionProtocolVersion GetProtocolVersion()
        {
            return ChromeSessionProtocolVersion.CreateFrom("sample application", Assembly.GetEntryAssembly().GetName().Version);
        }

        public ChromeSessionInstanceDescription[] GetSessionInstanceDescriptions(string serverName, int serverPort)
        {
            return new[] { ChromeSessionInstanceDescription.CreateFrom(
                                serverName, serverPort,
                                 "sample application",
                                "sample command line application",
                                "https://scontent-lax3-2.xx.fbcdn.net/v/t31.0-8/26850424_10215610615764193_3403737823383610422_o.jpg?_nc_cat=105&_nc_oc=AQmrv1vPT2ln4k0aEVP5lols-Jabc-VynxvBqV11LSLI7rma9_7-iRSwuLOcx2EVzALcoBotSdD76ryX_JQC42Di&_nc_ht=scontent-lax3-2.xx&oh=a0881f639de78a72d7f550a188ba4aa6&oe=5E204509",
                                SingleSessiongId
                            )};
        }
    }
}
