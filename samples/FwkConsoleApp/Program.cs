namespace FwkConsoleApp
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ChromeDevTools.Host;
    using ChromeDevTools.Host.FwkSelfHost;
    using ChromeDevTools.Host.Handlers.Debugging;
    using ChromeDevTools.Host.Handlers.Runtime;

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

        public static int[] GetValues(int maxNumber)
        {
            return Enumerable.Range(0, maxNumber).ToArray();
        }

        public static IEnumerable<IPublicValue> GetValuesAsInterface(int maxNumber)
        {
                return Enumerable.Range(0, maxNumber).Select(_ => new PublicValues { Value = _ });
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
                await sessions.BreakOn(nameof(Scripts.Main), Scripts.Main.SleepMethod, null);
                await Task.Delay(1000);

                await sessions.BreakOn(nameof(Scripts.Main), Scripts.Main.FibonacciMethod, new { i });
                var fibonacci = await Fibonacci(sessions, i);

                await sessions.BreakOn(nameof(Scripts.Main), Scripts.Main.LogMethod, new { i });

                await sessions.ForEach(_ => _.GetService<RuntimeHandler>().Log($"Fibonacci({i}) = {fibonacci}"));

                if (echo)
                {
                    switch (i % 4)
                    {
                        case 0: await sessions.Log($"Ticks : <message> {i}"); break;
                        case 1: await sessions.Warning($"Ticks : <warning> {i}"); break;
                        case 2: await sessions.Error($"Ticks : <error> {i}"); break;
                        case 3: await sessions.Debug($"Ticks : <debug> {i}"); break;
                    }
                }

                i++;
            }
        }

        public static async Task<int> Fibonacci(ChromeProtocolSessions sessions, int n)
        {
            var context = new FibonacciContext
            {
                n = n,
                NMinus1 = -1,
                NMinus2 = -2
            };

            await sessions.BreakOn(nameof(Scripts.Fibonacci), Scripts.Fibonacci.F0, context);
            if(context.n ==0) { return 0; }

            await sessions.BreakOn(nameof(Scripts.Fibonacci), Scripts.Fibonacci.F1, context);
            if (context.n == 1) { return 1; }

            await sessions.BreakOn(nameof(Scripts.Fibonacci), Scripts.Fibonacci.FN1Rec, context);
            context.NMinus1 = await Fibonacci(sessions, n - 1);

            await sessions.BreakOn(nameof(Scripts.Fibonacci), Scripts.Fibonacci.FN2Rec, context);
            context.NMinus2 = await Fibonacci(sessions, n - 2);

            await sessions.BreakOn(nameof(Scripts.Fibonacci), Scripts.Fibonacci.FNSum, context);         
            return context.NMinus1 + context.NMinus2;
        }

        public struct FibonacciContext
        {
            public int n;
            public int NMinus1;
            public int NMinus2;
        }
    }
}
