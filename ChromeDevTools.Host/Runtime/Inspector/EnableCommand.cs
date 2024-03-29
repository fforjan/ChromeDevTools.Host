namespace ChromeDevTools.Host.Runtime.Inspector
{
    using Newtonsoft.Json;

    /// <summary>
    /// Enables inspector domain notifications.
    /// </summary>
    public sealed class EnableCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Inspector.enable";
        
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