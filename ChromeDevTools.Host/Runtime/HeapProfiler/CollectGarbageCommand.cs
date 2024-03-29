namespace ChromeDevTools.Host.Runtime.HeapProfiler
{
    using Newtonsoft.Json;

    /// <summary>
    /// CollectGarbage
    /// </summary>
    public sealed class CollectGarbageCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "HeapProfiler.collectGarbage";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    }

    public sealed class CollectGarbageCommandResponse : ICommandResponse<CollectGarbageCommand>
    {
    }
}