namespace ChromeDevTools.Host.Runtime.DOMStorage
{
    using Newtonsoft.Json;

    /// <summary>
    /// Clear
    /// </summary>
    public sealed class ClearCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "DOMStorage.clear";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

        /// <summary>
        /// Gets or sets the storageId
        /// </summary>
        [JsonProperty("storageId")]
        public StorageId StorageId
        {
            get;
            set;
        }
    }

    public sealed class ClearCommandResponse : ICommandResponse<ClearCommand>
    {
    }
}