namespace ChromeDevTools.Host.Runtime.Runtime
{
    using Newtonsoft.Json;

    /// <summary>
    /// SetCustomObjectFormatterEnabled
    /// </summary>
    public sealed class SetCustomObjectFormatterEnabledCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Runtime.setCustomObjectFormatterEnabled";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

        /// <summary>
        /// Gets or sets the enabled
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled
        {
            get;
            set;
        }
    }

    public sealed class SetCustomObjectFormatterEnabledCommandResponse : ICommandResponse<SetCustomObjectFormatterEnabledCommand>
    {
    }
}