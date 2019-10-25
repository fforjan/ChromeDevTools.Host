using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ChromeDevTools.Host.FwkConsoleApp
{
    public class ChromeSessionWebServer
    {
        public static async Task Start(CancellationToken cancellationToken)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:4242");
            listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            { 
                var context = await listener.GetContextAsync();

                switch (context.Request.Url.GetLeftPart(UriPartial.Path))
                {
                    case "/chrome":
                        if (context.Request.IsWebSocketRequest)
                        {
                            _ = Task.Run(async () =>
                            {
                                var webSocketContext = await context.AcceptWebSocketAsync(null);
                                var session = new ChromeProtocolSession(null,
                                    webSocketContext.WebSocket);
                                await session.Process(cancellationToken);
                            });
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                        }

                        break;
                }
            }
        }
    }
}