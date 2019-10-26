

using System.IO;

namespace ChromeDevTools.Host
{
    using System.Threading.Tasks;
    using System;
    
    using ChromeDevTools.Host.Runtime.HeapProfiler;

    using ChromeDevTools.Host.Runtime;

    public class HeapProfilerHandler : IRuntimeHandle
    {
        private ChromeProtocolSession session;

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse());
        }

        public virtual void Register(ChromeProtocolSession session)
        {
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
            session.RegisterCommandHandler<TakeHeapSnapshotCommand>(TakeHeapSnapshot);
        }

        public virtual Task<ICommandResponse<TakeHeapSnapshotCommand>> TakeHeapSnapshot(TakeHeapSnapshotCommand comd)
        {
            throw new NotImplementedException();
        }


        protected async Task PublishHeapChunk(string chunk)
        {
            File.AppendAllText("Chunk.heapsnapshot", chunk);
            await session.SendEvent(new AddHeapSnapshotChunkEvent
            {
                Chunk = chunk
            });

        }protected async Task ReportHeapSnapshotProgress(int currentChunk, int totalChunk)
        {
            await session.SendEvent(new ReportHeapSnapshotProgressEvent
            {
                Total = totalChunk,
                Done = currentChunk,
            });

            if (currentChunk == totalChunk)
            {
                await session.SendEvent(new ReportHeapSnapshotProgressEvent
                {
                    Total = totalChunk,
                    Done = currentChunk,
                    Finished = true
                });
            }

        }
    }
}
