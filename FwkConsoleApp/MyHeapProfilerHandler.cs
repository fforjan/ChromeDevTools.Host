using System.IO;
using ChromeDevTools.Host;

namespace FwkConsoleApp
{
    public class MyHeapProfilerHandler : HeapProfilerHandler
    {
        protected override string GetHeapSnapshot()
        {
            return File.ReadAllText("output.heapsnapshot");
        }
    }
}