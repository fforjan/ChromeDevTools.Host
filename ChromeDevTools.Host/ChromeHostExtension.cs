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

namespace ChromeDevTools.Host
{
    public static class ChromeHostExtension
    {

        public static void HostChromeProtocol(this IApplicationBuilder app)
        {
            var chromeSessionLogger = app.ApplicationServices.GetService<ILogger<ChromeSession>>();

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            var address = serverAddressesFeature.Addresses.First().Replace("localhost", "127.0.0.1");

            var noPrefixAddress = address.Substring(address.IndexOf('/') + 2);

            var wsAddress = "ws" + address.Substring(address.IndexOf(':'));

            app.Use(async (context, next) =>
            {
                switch (context.Request.Path)
                {
                    case "/chrome":
                        {
                            if (context.WebSockets.IsWebSocketRequest)
                            {
                                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                                await Chrome(context, webSocket, chromeSessionLogger);
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
                        var response =
                            @"[ {
  ""description"": ""virtual instance"",
  ""devtoolsFrontendUrl"": ""chrome-devtools://devtools/bundled/js_app.html?experiments=true&v8only=true&ws=" + noPrefixAddress + @"chrome\\"",
  ""devtoolsFrontendUrlCompat"": ""chrome-devtools://devtools/bundled/inspector.html?experiments=true&v8only=true&ws=" + noPrefixAddress + @"chrome\\"",
  ""faviconUrl"": ""https://scontent-lax3-2.xx.fbcdn.net/v/t31.0-8/26850424_10215610615764193_3403737823383610422_o.jpg?_nc_cat=105&_nc_oc=AQmrv1vPT2ln4k0aEVP5lols-Jabc-VynxvBqV11LSLI7rma9_7-iRSwuLOcx2EVzALcoBotSdD76ryX_JQC42Di&_nc_ht=scontent-lax3-2.xx&oh=a0881f639de78a72d7f550a188ba4aa6&oe=5E204509"",
  ""id"": ""67b14650-5755-42ae-a255-25f9e8329fe0"",
  ""title"": ""virtual instance for fred"",
  ""type"": ""node"",
  ""url"": ""file://"",
  ""webSocketDebuggerUrl"": """ + wsAddress + @"chrome""
} ]
";

                        context.Response.Headers.Add("Content-Length", response.Length.ToString());

                        await context.Response.WriteAsync(response);
                        break;
                    default:
                        await next();
                        break;
                }

            });
        }


        private static async Task Chrome(HttpContext context, WebSocket webSocket, ILogger<ChromeSession> logger)
        {


            var session = new ChromeSession(logger, webSocket);
            chromeSessions.Add(session);

            await session.Process(CancellationToken.None);

            chromeSessions.Remove(session);

        }


        private static List<ChromeSession> chromeSessions = new List<ChromeSession>();

        public static IReadOnlyList<ChromeSession> ChromeSessions { get => chromeSessions; }
    }    
}
