using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChromeDevTools.Host;
using ChromeDevTools.Host.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

         
            #region UseWebSocket
            var webSocketOptions = new WebSocketOptions() 
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);
            #endregion

            #region AcceptWebSocket

            app.HostChromeProtocol();

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
             await Task.WhenAll(ChromeHostExtension.ChromeSessions.Select(_ => Extensions.GetService<RuntimeHandle>(_).Log("New Connection")));

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
                    await Task.WhenAll(ChromeHostExtension.ChromeSessions.Select(_ => Extensions.GetService<RuntimeHandle>(_).Log($"Received message: " + message)));
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            this.communicationSession.Remove(webSocket);

            await Task.WhenAll(ChromeHostExtension.ChromeSessions.Select(_ => Extensions.GetService<RuntimeHandle>(_).Log("Closed Connection")));
            
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

       
#endregion
        private List<WebSocket> communicationSession = new List<WebSocket>();
    }   
}
