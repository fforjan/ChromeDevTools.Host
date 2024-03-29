namespace ChromeDevTools.Host.Runtime.HeapProfiler
{
    using Newtonsoft.Json;

    /// <summary>
    /// Enable
    /// </summary>
    public sealed class EnableCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "HeapProfiler.enable";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    }

    public sealed class EnableCommandResponse : ICommandResponse<EnableCommand>
    {
    }
}