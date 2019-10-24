#define UseOptions // or NoOptions or UseOptionsAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaristaLabs.ChromeDevTools.Runtime;
using BaristaLabs.ChromeDevTools.Runtime.Log;
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

#if NoOptions
            #region UseWebSockets
            app.UseWebSockets();
            #endregion
#endif
#if UseOptions
            #region UseWebSocketsOptions
            var webSocketOptions = new WebSocketOptions() 
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);
            #endregion
#endif

#if UseOptionsAO
            #region UseWebSocketsOptionsAO
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            webSocketOptions.AllowedOrigins.Add("https://client.com");
            webSocketOptions.AllowedOrigins.Add("https://www.client.com");

            app.UseWebSockets(webSocketOptions);
            #endregion
#endif

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
  ""Protocol-Version"": ""1.1""
                           }");
                        break;
                    case "/json":
                    case "/json/list":
                     context.Response.ContentType = "application/json; charset=UTF-8";
                    
                        var response = 
                        @"[ {
  ""description"": ""node.js instance"",
  ""devtoolsFrontendUrl"": ""chrome-devtools://devtools/bundled/js_app.html?experiments=true&v8only=true&ws=" + noPrefixAddress + @"chrome"",
  ""devtoolsFrontendUrlCompat"": ""chrome-devtools://devtools/bundled/inspector.html?experiments=true&v8only=true&ws=" + noPrefixAddress + @"chrome"",
  ""faviconUrl"": ""https://nodejs.org/static/favicon.ico"",
  ""id"": ""67b14650-5755-42ae-a255-25f9e8329fe0"",
  ""title"": ""node[fred]"",
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
            this.communicationSession.Add(webSocket);
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await Task.WhenAll(communicationSession.Select( _ => 
                     _.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None)
                ));
                
                if(result.MessageType == WebSocketMessageType.Text) {
                    var logEntry = GetLogEvent(Encoding.ASCII.GetString(buffer));
                    await Task.WhenAll(this.chromeConnection.Select(_ => _.SendEvent(logEntry)));
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private Task Chrome(HttpContext context, WebSocket webSocket, ILogger<ChromeSession> logger) {

            return Task.Run( async () =>  {
                var session = new ChromeSession(logger, webSocket);
                chromeConnection.Add(session);

                await session.Process(CancellationToken.None);

                chromeConnection.Remove(session);
            });
        }
#endregion
        private List<ChromeSession> chromeConnection = new List<ChromeSession>();
        private List<WebSocket> communicationSession = new List<WebSocket>();

        private EntryAddedEvent GetLogEvent(string logMessage) {
            var logEvent = new EntryAddedEvent {
                Entry = new LogEntry {
                    Source = "other",
                    Level = "info",
                    Text = logMessage,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                }
            };

            return logEvent;
        }
    }   
}
