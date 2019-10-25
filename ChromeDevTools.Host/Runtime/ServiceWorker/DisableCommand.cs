namespace ChromeDevTools.Host.Runtime.ServiceWorker
{
    using Newtonsoft.Json;

    /// <summary>
    /// Disable
    /// </summary>
    public sealed class DisableCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "ServiceWorker.disable";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    }

    public sealed class DisableCommandResponse : ICommandResponse<DisableCommand>
    {
    }
}