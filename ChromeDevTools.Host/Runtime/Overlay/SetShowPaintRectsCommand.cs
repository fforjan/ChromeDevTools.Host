namespace ChromeDevTools.Host.Runtime.Overlay
{
    using Newtonsoft.Json;

    /// <summary>
    /// Requests that backend shows paint rectangles
    /// </summary>
    public sealed class SetShowPaintRectsCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Overlay.setShowPaintRects";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

        /// <summary>
        /// True for showing paint rectangles
        /// </summary>
        [JsonProperty("result")]
        public bool Result
        {
            get;
            set;
        }
    }

    public sealed class SetShowPaintRectsCommandResponse : ICommandResponse<SetShowPaintRectsCommand>
    {
    }
}