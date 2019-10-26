using System.Linq;

namespace ChromeDevTools.Host.FwkSelfHost
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class ChromeSessionWebServer
    {
        private static readonly List<ChromeProtocolSession> sessions = new List<ChromeProtocolSession>();

        public static IReadOnlyCollection<ChromeProtocolSession> Sessions { get; } = sessions;

        public static async Task Start(string listeningAddress, 
            string title,
            string description,
            string faviconUrl,
            Guid id, CancellationToken cancellationToken,
            params IRuntimeHandle[] handlers)
        {
            try
            {
                using (var listener = new HttpListener())
                {
                    listener.Prefixes.Add(listeningAddress);
                    listener.Start();
                    Console.Out.WriteLine("listening on " + listeningAddress);

                    var listeningOnUri = new Uri(listeningAddress);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var context = await listener.GetContextAsync();

                        switch (context.Request.Url.AbsolutePath)
                        {
                            case "/chrome":
                            {
                                if (context.Request.IsWebSocketRequest)
                                {
                                    try
                                    {
                                        _ = Task.Run(async () =>
                                        {
                                            var webSocketContext = await context.AcceptWebSocketAsync(null);
                                            var session = new ChromeProtocolSession(webSocketContext.WebSocket, handlers);
                                            try
                                            {
                                                sessions.Add(session);
                                                await session.Process(cancellationToken);
                                            }
                                            finally
                                            {
                                                sessions.Remove(session);
                                            }
                                        });
                                    }
                                    catch (Exception e)
                                    {
                                        Console.Error.WriteLine(e.ToString());
                                    }
                                }
                                else
                                {
                                    context.Response.StatusCode = 400;
                                }

                                break;
                            }

                            case "/json/version":
                            {
                                var responseString = @"{  
                                ""Browser"": ""node.js/v10.14.2"",
                                ""Protocol-Version"": ""1.3""
                           }";

                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                                context.Response.ContentType = "application/json; charset=UTF-8";
                                // Get a response stream and write the response to it.
                                context.Response.ContentLength64 = buffer.Length;
                                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                context.Response.OutputStream.Close();
                                break;
                            }

                            case "/json":
                            case "/json/list":
                            {
                                var responseObj = new[]
                                {
                                    ChromeSessionInstanceDescription.CreateFrom(
                                        $"{listeningOnUri.Host}:{listeningOnUri.Port}",
                                        title,
                                        description,
                                        faviconUrl,
                                        Guid.NewGuid()
                                    )
                                };

                                var responseString = JsonConvert.SerializeObject(responseObj);
                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                                context.Response.ContentType = "application/json; charset=UTF-8";
                                // Get a response stream and write the response to it.
                                context.Response.ContentLength64 = buffer.Length;
                                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                context.Response.OutputStream.Close();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        public static Task ForEach(Func<ChromeProtocolSession, Task> chromeSessionAction)
        {
            return Task.WhenAll(sessions.Select(chromeSessionAction));
        }
    }
}