using System;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ChromeDevTools.Host;
using ChromeDevTools.Host.FwkSelfHost;
using ChromeDevTools.Host.Handlers;

namespace FwkConsoleApp
{

    class Program
    {
        static async Task Main()
        {
            var sessions = new ChromeProtocolSessions();

            var server = ChromeSessionWebServer.Start(sessions, "http://127.0.0.1:12345/",
                new SingleSessionProvider(),
                CancellationToken.None);

            int i = 0;
            while (true)
            {
                Thread.Sleep(1000);

                switch (i % 4)
                {
                    case 0: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Log($"Ticks : <message> {i}")); break;
                    case 1: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Warning($"Ticks : <warning> {i}")); break;
                    case 2: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Error($"Ticks : <error> {i}"));break;
                    case 3: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Debug($"Ticks : <debug> {i}")); break;
                }

                i++;
            }
        }
    }
}
