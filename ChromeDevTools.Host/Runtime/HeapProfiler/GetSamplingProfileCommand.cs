namespace ChromeDevTools.Host.Runtime.HeapProfiler
{
    using Newtonsoft.Json;

    /// <summary>
    /// GetSamplingProfile
    /// </summary>
    public sealed class GetSamplingProfileCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "HeapProfiler.getSamplingProfile";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    }

    public sealed class GetSamplingProfileCommandResponse : ICommandResponse<GetSamplingProfileCommand>
    {
        /// <summary>
        /// Return the sampling profile being collected.
        ///</summary>
        [JsonProperty("profile")]
        public SamplingHeapProfile Profile
        {
            get;
            set;
        }
    }
}