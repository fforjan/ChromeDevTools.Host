using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChromeDevTools.Host.FwkSelfHost;

namespace FwkConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = ChromeSessionWebServer.Start(CancellationToken.None);

            int i = 0;
            while (true)
            {
                Thread.Sleep(1000);

                switch (i % 4)
                {
                    case 0: ChromeSessionWebServer.Sessions.ForEach(_ => _.RuntimeHandle.Log($"Ticks : <message> {i}")); break;
                    case 1: ChromeSessionWebServer.Sessions.ForEach(_ => _.RuntimeHandle.Warning($"Ticks : <warning> {i}")); break;
                    case 2: ChromeSessionWebServer.Sessions.ForEach(_ => _.RuntimeHandle.Error($"Ticks : <error> {i}"));break;
                    case 3: ChromeSessionWebServer.Sessions.ForEach(_ => _.RuntimeHandle.Info($"Ticks : <info> {i}")); break;
                }

                i++;
            }
        }
    }
}
