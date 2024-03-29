namespace ChromeDevTools.Host.Runtime.Console
{
    using Newtonsoft.Json;

    /// <summary>
    /// Enables console domain, sends the messages collected so far to the client by means of the
    /// `messageAdded` notification.
    /// </summary>
    public sealed class EnableCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Console.enable";
        
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