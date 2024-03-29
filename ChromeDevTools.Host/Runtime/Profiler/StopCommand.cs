namespace ChromeDevTools.Host.Runtime.Profiler
{
    using Newtonsoft.Json;

    /// <summary>
    /// Stop
    /// </summary>
    public sealed class StopCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Profiler.stop";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    }

    public sealed class StopCommandResponse : ICommandResponse<StopCommand>
    {
        /// <summary>
        /// Recorded profile.
        ///</summary>
        [JsonProperty("profile")]
        public Profile Profile
        {
            get;
            set;
        }
    }
}