namespace ChromeDevTools.Host.Runtime.Page
{
    using Newtonsoft.Json;

    /// <summary>
    /// Compressed image data requested by the `startScreencast`.
    /// </summary>
    public sealed class ScreencastFrameEvent : IEvent
    {
        /// <summary>
        /// Base64-encoded compressed image.
        /// </summary>
        [JsonProperty("data")]
        public string Data
        {
            get;
            set;
        }
        /// <summary>
        /// Screencast frame metadata.
        /// </summary>
        [JsonProperty("metadata")]
        public ScreencastFrameMetadata Metadata
        {
            get;
            set;
        }
        /// <summary>
        /// Frame number.
        /// </summary>
        [JsonProperty("sessionId")]
        public long SessionId
        {
            get;
            set;
        }
    }
}