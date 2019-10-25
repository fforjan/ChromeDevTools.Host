using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChromeDevTools.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;

namespace EchoApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole()
                    .AddDebug()
                    .AddFilter<ConsoleLoggerProvider>(category: null, level: LogLevel.Debug)
                    .AddFilter<DebugLoggerProvider>(category: null, level: LogLevel.Debug);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var chromeSessionLogger= app.ApplicationServices.GetService<ILogger<ChromeSession>>();

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            var address = serverAddressesFeature.Addresses.First().Replace("localhost", "127.0.0.1");
            var noPrefixAddress =  address.Substring(address.IndexOf('/') +2);
            var wsAddress = "ws" +address.Substring(address.IndexOf(':'));

            #region UseWebSocket
            var webSocketOptions = new WebSocketOptions() 
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);
            #endregion

            #region AcceptWebSocket
            app.Use(async (context, next) =>
            {
                switch (context.Request.Path)
                {
                    case "/ws":
                        {
                            if (context.WebSockets.IsWebSocketRequest)
                            {
                                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                                await Echo(context, webSocket);
                            }
                            else
                            {
                                context.Response.StatusCode = 400;
                            }

                            break;
                        }

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
#endregion
            app.UseFileServer();
        }
#region Echo
        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
             await Task.WhenAll(this.chromeConnection.Select(_ => _.RuntimeHandle.Log("New Connection")));

            this.communicationSession.Add(webSocket);
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var rawMessage = new ArraySegment<byte>(buffer, 0, result.Count);
                await Task.WhenAll(communicationSession.Select( _ => 
                     _.SendAsync(rawMessage, result.MessageType, result.EndOfMessage, CancellationToken.None)
                ));

                if(result.MessageType == WebSocketMessageType.Text) {
                    var message = Encoding.ASCII.GetString(rawMessage);
                    await Task.WhenAll(this.chromeConnection.Select(_ => _.RuntimeHandle.Log($"Received message: " + message)));
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            this.communicationSession.Remove(webSocket);

            await Task.WhenAll(this.chromeConnection.Select(_ => _.RuntimeHandle.Log("Closed Connection")));
            
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task Chrome(HttpContext context, WebSocket webSocket, ILogger<ChromeSession> logger) {

           
                var session = new ChromeSession(logger, webSocket);
                chromeConnection.Add(session);

                await session.Process(CancellationToken.None);

                chromeConnection.Remove(session);
           
        }
#endregion
        private List<ChromeSession> chromeConnection = new List<ChromeSession>();
        private List<WebSocket> communicationSession = new List<WebSocket>();
    }   
}
