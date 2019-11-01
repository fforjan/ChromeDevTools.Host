namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers;
    using ChromeDevTools.Host.Runtime.Runtime;
    using System.Linq;

    public class MyRuntimeHandler : RuntimeHandler
    {
        protected override RemoteObject Evaluate(string expr)
        {
            return new RemoteObject
            {
                Type = "string",
                Value = new string(expr.Reverse().ToArray())
            };
        }
    }
}