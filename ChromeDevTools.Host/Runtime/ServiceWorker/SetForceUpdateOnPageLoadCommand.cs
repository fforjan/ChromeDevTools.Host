namespace ChromeDevTools.Host.Runtime.ServiceWorker
{
    using Newtonsoft.Json;

    /// <summary>
    /// SetForceUpdateOnPageLoad
    /// </summary>
    public sealed class SetForceUpdateOnPageLoadCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "ServiceWorker.setForceUpdateOnPageLoad";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

        /// <summary>
        /// Gets or sets the forceUpdateOnPageLoad
        /// </summary>
        [JsonProperty("forceUpdateOnPageLoad")]
        public bool ForceUpdateOnPageLoad
        {
            get;
            set;
        }
    }

    public sealed class SetForceUpdateOnPageLoadCommandResponse : ICommandResponse<SetForceUpdateOnPageLoadCommand>
    {
    }
}