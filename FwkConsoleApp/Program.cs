using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChromeDevTools.Host.FwkSelfHost;

namespace FwkConsoleApp
{
    class Program
    {
        static void Main()
        {
            var server = ChromeSessionWebServer.Start("http://127.0.0.1:12345/", CancellationToken.None);

            int i = 0;
            while (true)
            {
                Thread.Sleep(1000);

                switch (i % 4)
                {
                    case 0: ChromeSessionWebServer.ForEach(_ => _.RuntimeHandle.Log($"Ticks : <message> {i}")); break;
                    case 1: ChromeSessionWebServer.ForEach(_ => _.RuntimeHandle.Warning($"Ticks : <warning> {i}")); break;
                    case 2: ChromeSessionWebServer.ForEach(_ => _.RuntimeHandle.Error($"Ticks : <error> {i}"));break;
                    case 3: ChromeSessionWebServer.ForEach(_ => _.RuntimeHandle.Debug($"Ticks : <debug> {i}")); break;
                }

                i++;
            }
        }
    }
}
