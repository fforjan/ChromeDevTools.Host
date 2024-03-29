namespace ChromeDevTools.Host.Runtime.Page
{
    using Newtonsoft.Json;

    /// <summary>
    /// LoadEventFired
    /// </summary>
    public sealed class LoadEventFiredEvent : IEvent
    {
        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        public double Timestamp
        {
            get;
            set;
        }
    }
}