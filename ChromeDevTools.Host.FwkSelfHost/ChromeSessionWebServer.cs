namespace ChromeDevTools.Host.FwkSelfHost
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using ChromeDevTools.Host.Handlers;
    using Newtonsoft.Json;



    public static class ChromeSessionWebServer
    {

        public static async Task Start(ChromeProtocolSessions sessions, string listeningAddress,
            string title,
            Version version,
            string description,
            string faviconUrl,
            Guid id, CancellationToken cancellationToken,
            params IRuntimeHandler[] handlers)
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

                        var path = context.Request.Url.AbsolutePath;
                        if (path.StartsWith("/json/session/", StringComparison.InvariantCultureIgnoreCase) && context.Request.IsWebSocketRequest)
                        {
                            try
                            {
                                _ = Task.Run(async () =>
                                {
                                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                                    var session = new ChromeProtocolSession(webSocketContext.WebSocket, handlers);
                                    using (sessions.Register(session))
                                    {
                                        await session.Process(cancellationToken);
                                    }
                                });
                            }
                            catch (Exception e)
                            {
                                Console.Error.WriteLine(e);
                            }
                        }
                        else
                        {
                            switch (context.Request.Url.AbsolutePath)
                            {
                                case "/json/version":
                                    {
                                        var responseString = JsonConvert.SerializeObject(ChromeSessionProtocolVersion.CreateFrom(title, version));
                                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
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
                                        id
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
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
    }
}