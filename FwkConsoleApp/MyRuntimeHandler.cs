namespace FwkConsoleApp
{
    using ChromeDevTools.Host.Handlers;
    using System.Linq;

    public class MyRuntimeHandler : RuntimeHandler
    {
        protected override string Evaluate(string expr)
        {
            return new string(expr.Reverse().ToArray());
        }
    }
}