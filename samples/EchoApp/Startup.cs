namespace EchoApp
{
    using System.Net.WebSockets;
    using ChromeDevTools.Host;
    using ChromeDevTools.Host.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.Logging.Debug;
    using Microsoft.Extensions.Hosting;

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

            services.AddSingleton<IChromeSessionProvider>(new SingleSessionProvider());
            services.AddSingleton(new EchoService());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

         
            #region UseWebSocket
            app.UseWebSockets();
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
                                await ((EchoService)app.ApplicationServices.GetService(typeof(EchoService))).Echo(webSocket);
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
    }   
}
