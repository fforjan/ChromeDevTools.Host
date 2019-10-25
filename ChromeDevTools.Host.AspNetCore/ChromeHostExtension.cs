using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace ChromeDevTools.Host.AspNetCore
{

    public static class ChromeHostExtension
    {
        public static void HostChromeProtocol(this IApplicationBuilder app)
        {
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            var serveruri = new Uri(serverAddressesFeature.Addresses.First());

            app.Use(async (context, next) =>
            {
                switch (context.Request.Path)
                {
                    case "/chrome":
                        {
                            if (context.WebSockets.IsWebSocketRequest)
                            {
                                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                                await Chrome(webSocket);
                            }
                            else
                            {
                                context.Response.StatusCode = 400;
                            }

                            break;
                        }

                    case "/json/version":
                        context.Response.ContentType = "application/json; charset=UTF-8";
                        await context.Response.WriteAsync(
                        @"{  
                                ""Browser"": ""node.js/v10.14.2"",
                                ""Protocol-Version"": ""1.3""
                           }");
                        break;
                    case "/json":
                    case "/json/list":
                        var responseObj = new[]
                        {
                            ChromeSessionInstanceDescription.CreateFrom(
                                $"{serveruri.Host}:{serveruri.Port}",
                                "virtual instance for AspNetCore",
                                "virtual instance",
                                "https://scontent-lax3-2.xx.fbcdn.net/v/t31.0-8/26850424_10215610615764193_3403737823383610422_o.jpg?_nc_cat=105&_nc_oc=AQmrv1vPT2ln4k0aEVP5lols-Jabc-VynxvBqV11LSLI7rma9_7-iRSwuLOcx2EVzALcoBotSdD76ryX_JQC42Di&_nc_ht=scontent-lax3-2.xx&oh=a0881f639de78a72d7f550a188ba4aa6&oe=5E204509",
                                Guid.NewGuid()
                            )
                        };

                        var response = JsonConvert.SerializeObject(responseObj);
                        context.Response.ContentType = "application/json; charset=UTF-8";
                        context.Response.Headers.Add("Content-Length", response.Length.ToString());

                        await context.Response.WriteAsync(response);
                        break;
                    default:
                        await next();
                        break;
                }

            });
        }


        private static async Task Chrome(WebSocket webSocket)
        {
            var session = new ChromeProtocolSession(webSocket, new IRuntimeHandle[] { new RuntimeHandle(), new DebuggerHandler(), new ProfilerHandler()  });
            chromeSessions.Add(session);

            await session.Process(CancellationToken.None);

            chromeSessions.Remove(session);

        }


        private static List<ChromeProtocolSession> chromeSessions = new List<ChromeProtocolSession>();

        public static IReadOnlyList<ChromeProtocolSession> ChromeSessions { get => chromeSessions; }
    }    
}
