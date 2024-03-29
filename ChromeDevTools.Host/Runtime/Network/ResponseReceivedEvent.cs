namespace ChromeDevTools.Host.Runtime.Network
{
    using Newtonsoft.Json;

    /// <summary>
    /// Fired when HTTP response is available.
    /// </summary>
    public sealed class ResponseReceivedEvent : IEvent
    {
        /// <summary>
        /// Request identifier.
        /// </summary>
        [JsonProperty("requestId")]
        public string RequestId
        {
            get;
            set;
        }
        /// <summary>
        /// Loader identifier. Empty string if the request is fetched from worker.
        /// </summary>
        [JsonProperty("loaderId")]
        public string LoaderId
        {
            get;
            set;
        }
        /// <summary>
        /// Timestamp.
        /// </summary>
        [JsonProperty("timestamp")]
        public double Timestamp
        {
            get;
            set;
        }
        /// <summary>
        /// Resource type.
        /// </summary>
        [JsonProperty("type")]
        public Page.ResourceType Type
        {
            get;
            set;
        }
        /// <summary>
        /// Response data.
        /// </summary>
        [JsonProperty("response")]
        public Response Response
        {
            get;
            set;
        }
        /// <summary>
        /// Frame identifier.
        /// </summary>
        [JsonProperty("frameId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FrameId
        {
            get;
            set;
        }
    }
}