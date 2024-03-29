namespace ChromeDevTools.Host.Runtime.Overlay
{
    using Newtonsoft.Json;

    /// <summary>
    /// Highlights given quad. Coordinates are absolute with respect to the main frame viewport.
    /// </summary>
    public sealed class HighlightQuadCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Overlay.highlightQuad";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

        /// <summary>
        /// Quad to highlight
        /// </summary>
        [JsonProperty("quad")]
        public double[] Quad
        {
            get;
            set;
        }
        /// <summary>
        /// The highlight fill color (default: transparent).
        /// </summary>
        [JsonProperty("color", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DOM.RGBA Color
        {
            get;
            set;
        }
        /// <summary>
        /// The highlight outline color (default: transparent).
        /// </summary>
        [JsonProperty("outlineColor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DOM.RGBA OutlineColor
        {
            get;
            set;
        }
    }

    public sealed class HighlightQuadCommandResponse : ICommandResponse<HighlightQuadCommand>
    {
    }
}