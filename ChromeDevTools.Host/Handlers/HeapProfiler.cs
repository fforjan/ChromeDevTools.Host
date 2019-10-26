namespace ChromeDevTools.Host.Handlers
{
    using System.Threading.Tasks;
    using System;
    
    using ChromeDevTools.Host.Runtime.HeapProfiler;

    using ChromeDevTools.Host.Runtime;
    using System.Collections.Generic;

    public class HeapProfilerHandler : IRuntimeHandler
    {
        private const int ChunkSize = 10000;

        private ChromeProtocolSession session;

        public virtual bool IsEnable { get; protected set; }

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            this.IsEnable = true;
            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse());
        }

        public Task<ICommandResponse<DisableCommand>> DisableCommand(DisableCommand command)
        {
            this.IsEnable = false;
            return Task.FromResult<ICommandResponse<DisableCommand>>(new DisableCommandResponse());
        }

        public virtual void Register(ChromeProtocolSession session)
        {
            this.IsEnable = false;
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
            session.RegisterCommandHandler<DisableCommand>(EnableCommand);
            session.RegisterCommandHandler<TakeHeapSnapshotCommand>(TakeHeapSnapshot);
        }

        public async Task<ICommandResponse<TakeHeapSnapshotCommand>> TakeHeapSnapshot(TakeHeapSnapshotCommand comd)
        {
            var reportProgress = comd.ReportProgress.HasValue && comd.ReportProgress.Value;

            var currentChunk = 0;
            var chunks = new List<string>();

            // this should be reimplemented with Span of text ??
            var snapshot = GetHeapSnapshot();

            while (snapshot.Length > ChunkSize)
            {
                chunks.Add(snapshot.Substring(0, ChunkSize));
                if (reportProgress)
                {
                    await this.ReportHeapSnapshotProgress(currentChunk += ChunkSize, snapshot.Length);

                }
                snapshot = snapshot.Substring(ChunkSize);
            }
            chunks.Add(snapshot);

            if (reportProgress)
            {
                await this.ReportHeapSnapshotProgress(snapshot.Length, snapshot.Length);
            }

            foreach (var chunk in chunks)
            {
                await session.SendEvent(new AddHeapSnapshotChunkEvent
                {
                    Chunk = chunk
                });
            }

            return new TakeHeapSnapshotCommandResponse();
        }


        protected virtual string GetHeapSnapshot()
        {
            throw new NotImplementedException();
        }

        protected async Task ReportHeapSnapshotProgress(int currentChunk, int totalChunk)
        {
            await session.SendEvent(new ReportHeapSnapshotProgressEvent
            {
                Total = totalChunk,
                Done = currentChunk,
            });

            // for the last one, ensure to raise finished.
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
