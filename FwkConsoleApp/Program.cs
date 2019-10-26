using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChromeDevTools.Host;
using ChromeDevTools.Host.FwkSelfHost;
using ChromeDevTools.Host.Runtime;
using ChromeDevTools.Host.Runtime.HeapProfiler;

namespace FwkConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            var server = ChromeSessionWebServer.Start("http://127.0.0.1:12345/",
                "sample application",
                "sample command line application",
                "https://scontent-lax3-2.xx.fbcdn.net/v/t31.0-8/26850424_10215610615764193_3403737823383610422_o.jpg?_nc_cat=105&_nc_oc=AQmrv1vPT2ln4k0aEVP5lols-Jabc-VynxvBqV11LSLI7rma9_7-iRSwuLOcx2EVzALcoBotSdD76ryX_JQC42Di&_nc_ht=scontent-lax3-2.xx&oh=a0881f639de78a72d7f550a188ba4aa6&oe=5E204509",
                Guid.NewGuid(),
                CancellationToken.None,
                new RuntimeHandle(), new DebuggerHandler(), new ProfilerHandler(), new MyHeapProfilerHandler());

            int i = 0;
            while (true)
            {
                Thread.Sleep(1000);

                switch (i % 4)
                {
                    case 0: await ChromeSessionWebServer.ForEach(_ => _.GetService<RuntimeHandle>().Log($"Ticks : <message> {i}")); break;
                    case 1: await ChromeSessionWebServer.ForEach(_ => _.GetService<RuntimeHandle>().Warning($"Ticks : <warning> {i}")); break;
                    case 2: await ChromeSessionWebServer.ForEach(_ => _.GetService<RuntimeHandle>().Error($"Ticks : <error> {i}"));break;
                    case 3: await ChromeSessionWebServer.ForEach(_ => _.GetService<RuntimeHandle>().Debug($"Ticks : <debug> {i}")); break;
                }

                i++;
            }
        }
    }

    public class MyHeapProfilerHandler : HeapProfilerHandler
    {
        private  const int ChunkSize = 1024;
        public override  async Task<ICommandResponse<TakeHeapSnapshotCommand>> TakeHeapSnapshot(TakeHeapSnapshotCommand comd)
        {
            await SendSnapshot(comd.ReportProgress.HasValue && comd.ReportProgress.Value);
            return new TakeHeapSnapshotCommandResponse();
        }

        private async Task SendSnapshot(bool reportProgress)
        {
            var snapshot = Resource.HeadSnapshot;
            var totalChunkToSend = (snapshot.Length / ChunkSize) + 1;
            var currentChunk = 0;
            while (snapshot.Length > ChunkSize)
            {
                await this.PublishHeapChunk(snapshot.Substring(0, ChunkSize));
                if(reportProgress)
                {
                    await this.ReportHeapSnapshotProgress(currentChunk++, totalChunkToSend);

                }
                snapshot = snapshot.Substring(ChunkSize);
            }

            await this.PublishHeapChunk(snapshot);
            await this.ReportHeapSnapshotProgress(totalChunkToSend, totalChunkToSend);
        }
    }
}
