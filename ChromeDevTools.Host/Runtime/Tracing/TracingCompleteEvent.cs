namespace ChromeDevTools.Host.Runtime.Tracing
{
    using Newtonsoft.Json;

    /// <summary>
    /// Signals that tracing is stopped and there is no trace buffers pending flush, all data were
    /// delivered via dataCollected events.
    /// </summary>
    public sealed class TracingCompleteEvent : IEvent
    {
        /// <summary>
        /// A handle of the stream that holds resulting trace data.
        /// </summary>
        [JsonProperty("stream", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Stream
        {
            get;
            set;
        }
        /// <summary>
        /// Compression format of returned stream.
        /// </summary>
        [JsonProperty("streamCompression", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StreamCompression? StreamCompression
        {
            get;
            set;
        }
    }
}