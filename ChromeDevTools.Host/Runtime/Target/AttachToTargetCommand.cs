namespace ChromeDevTools.Host.Runtime.Target
{
    using Newtonsoft.Json;

    /// <summary>
    /// Attaches to the target with given id.
    /// </summary>
    public sealed class AttachToTargetCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Target.attachToTarget";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

        /// <summary>
        /// Gets or sets the targetId
        /// </summary>
        [JsonProperty("targetId")]
        public string TargetId
        {
            get;
            set;
        }
        /// <summary>
        /// Enables "flat" access to the session via specifying sessionId attribute in the commands.
        /// </summary>
        [JsonProperty("flatten", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Flatten
        {
            get;
            set;
        }
    }

    public sealed class AttachToTargetCommandResponse : ICommandResponse<AttachToTargetCommand>
    {
        /// <summary>
        /// Id assigned to the session.
        ///</summary>
        [JsonProperty("sessionId")]
        public string SessionId
        {
            get;
            set;
        }
    }
}