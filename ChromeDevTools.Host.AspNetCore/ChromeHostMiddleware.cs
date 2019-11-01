namespace ChromeDevTools.Host.AspNetCore
{
    using System;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using System.Linq;
    using System.Threading;
    using Newtonsoft.Json;
    using ChromeDevTools.Host.Handlers;

    public class ChromeHostMiddleware 
    {
        private readonly RequestDelegate next;
        private readonly IChromeSessionProvider chromeSessionProvider;

        public ChromeHostMiddleware(RequestDelegate next, IChromeSessionProvider chromeSessionProvider)
        {
            this.next = next;
            this.chromeSessionProvider = chromeSessionProvider;
        }

        public async Task Invoke(HttpContext context)
        {
                var serveruri = context.Request.Host;
                string path = context.Request.Path;

                if (path.StartsWith("/json/session", StringComparison.InvariantCultureIgnoreCase) && context.WebSockets.IsWebSocketRequest)
                {

                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    var session = chromeSessionProvider.CreateSession(webSocket, path.Split('/').Last());

                    using (ChromeHostExtension.Sessions.Register(session))
                    {
                        await session.Process(CancellationToken.None);
                    }

                }
                else
                {
                    switch (context.Request.Path)
                    {
                        case "/json/version":
                            {
                                var responseObj = chromeSessionProvider.GetProtocolVersion();

                                var response = JsonConvert.SerializeObject(responseObj);

                                context.Response.Headers.Add("Content-Length", response.Length.ToString());

                                await context.Response.WriteAsync(response);
                                break;
                            }
                        case "/json":
                        case "/json/list":
                            {
                                var responseObj = new[]
                                {
                            ChromeSessionInstanceDescription.CreateFrom(
                                serveruri.Host, serveruri.Port.Value,
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
                            }
                        default:
                            await next(context);
                            break;
                    }
                }
        }

      
    }
}
