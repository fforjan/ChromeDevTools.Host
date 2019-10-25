using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChromeDevTools.Host.FwkSelfHost
{
    public class ChromeSessionWebServer
    {
        public static List<ChromeProtocolSession> Sessions { get; } = new List<ChromeProtocolSession>();
        public static async Task Start(string listeningAddress, CancellationToken cancellationToken)
        {
            try
            {
                var listener = new HttpListener();
                listener.Prefixes.Add(listeningAddress);
                listener.Start();
                Console.Out.WriteLine("listening on "+ listeningAddress);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var context = await listener.GetContextAsync();

                    switch (context.Request.Url.AbsolutePath)
                    {
                        case "/chrome":
                        {
                            if (context.Request.IsWebSocketRequest)
                            {
                                _ = Task.Run(async () =>
                                {
                                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                                    var session = new ChromeProtocolSession(null,
                                        webSocketContext.WebSocket);
                                    Sessions.Add(session);
                                    await session.Process(cancellationToken);
                                    Sessions.Remove(session);
                                });
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
                                    "localhost:12345",
                                    "virtual instance for FwkConsoleApp",
                                    "virtual instance",
                                    "https://scontent-lax3-2.xx.fbcdn.net/v/t31.0-8/26850424_10215610615764193_3403737823383610422_o.jpg?_nc_cat=105&_nc_oc=AQmrv1vPT2ln4k0aEVP5lols-Jabc-VynxvBqV11LSLI7rma9_7-iRSwuLOcx2EVzALcoBotSdD76ryX_JQC42Di&_nc_ht=scontent-lax3-2.xx&oh=a0881f639de78a72d7f550a188ba4aa6&oe=5E204509",
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
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }
}