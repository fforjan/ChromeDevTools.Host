namespace ChromeDevTools.Host.Runtime.ServiceWorker
{
    using Newtonsoft.Json;

    /// <summary>
    /// StopWorker
    /// </summary>
    public sealed class StopWorkerCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "ServiceWorker.stopWorker";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

        /// <summary>
        /// Gets or sets the versionId
        /// </summary>
        [JsonProperty("versionId")]
        public string VersionId
        {
            get;
            set;
        }
    }

    public sealed class StopWorkerCommandResponse : ICommandResponse<StopWorkerCommand>
    {
    }
}