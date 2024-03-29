namespace ChromeDevTools.Host.Runtime.Network
{
    using Newtonsoft.Json;

    /// <summary>
    /// Returns all browser cookies. Depending on the backend support, will return detailed cookie
    /// information in the `cookies` field.
    /// </summary>
    public sealed class GetAllCookiesCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Network.getAllCookies";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    }

    public sealed class GetAllCookiesCommandResponse : ICommandResponse<GetAllCookiesCommand>
    {
        /// <summary>
        /// Array of cookie objects.
        ///</summary>
        [JsonProperty("cookies")]
        public Cookie[] Cookies
        {
            get;
            set;
        }
    }
}