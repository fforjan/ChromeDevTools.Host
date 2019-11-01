namespace FwkConsoleApp
{
    using System.IO;
    using ChromeDevTools.Host.Handlers;
    using System.Linq;

    /// <summary>
    /// Simple implementation of the Heap Profiler
    /// </summary>
    public class MyHeapProfilerHandler : HeapProfilerHandler
    {
        protected override string GetHeapSnapshot()
        {
            // We simple return the file of a previously saved snapshot
            return File.ReadAllText("output.heapsnapshot");
        }
    }
}