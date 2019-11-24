using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ChromeDevTools.Host;
using ChromeDevTools.Host.FwkSelfHost;
using ChromeDevTools.Host.Handlers;
using ChromeDevTools.Host.Handlers.Runtime;

namespace FwkConsoleApp
{

    class Program
    {
        private static bool echo = true;
        public static void EchoOff()
        {
            echo = false;
        }

        public static void EchoOn()
        {
            echo = true;
        }

        public static int[] GetValues(int maxNumer)
        {
            return Enumerable.Range(0, maxNumer).ToArray();
        }

        public static IEnumerable<IPublicValue> GetValuesAsInterface(int maxNumer)
        {
                return Enumerable.Range(0, maxNumer).Select(_ => new PublicValues { Value = _ });
        }

        public static IPublicValue GetValueAsInterface(int number)
        {
            return new PublicValues { Value = number };
        }

        public interface IPublicValue
        {
            int Value { get; }
        }

        public class PublicValues : IPublicValue
        {

            public int Value { get; set; }

            int IPublicValue.Value { get => this.Value; }
        }


        static async Task Main()
        {
            var sessions = new ChromeProtocolSessions();

            var server = ChromeSessionWebServer.Start(sessions, "http://127.0.0.1:12345/",
                new SingleSessionProvider(),
                CancellationToken.None);

            int i = 0;
            while (true)
            {
                await sessions.BreakOn("Main", "sleep", null);
                await Task.Delay(1000);

                await sessions.BreakOn("Main", "Fibonaci", new { i });
                await Fibonaci(sessions, i);

                await sessions.BreakOn("Main", "log", new { i });
                if (echo)
                {
                    switch (i % 4)
                    {
                        case 0: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Log($"Ticks : <message> {i}")); break;
                        case 1: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Warning($"Ticks : <warning> {i}")); break;
                        case 2: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Error($"Ticks : <error> {i}")); break;
                        case 3: await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Debug($"Ticks : <debug> {i}")); break;
                    }
                }

                i++;
            }
        }

        public static async Task<int> Fibonaci(ChromeProtocolSessions sessions, int n)
        {
            var context = new { n };
            await sessions.BreakOn("Fibonaci", "f0", context);
            if(n ==0) { return 0; }

            await sessions.BreakOn("Fibonaci", "f1", context);
            if (n == 1) { return 1; }

            await sessions.BreakOn("Fibonaci", "fsum", context);
            return await Fibonaci(sessions, n - 1) + await Fibonaci(sessions, n - 2);
        }
    }
}
