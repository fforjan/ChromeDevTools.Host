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
                ChromeSessionWebServer.Sessions.ForEach(_ => _.RuntimeHandle.Log($"Ticks : {i}"));
                i++;
            }
        }
    }
}
