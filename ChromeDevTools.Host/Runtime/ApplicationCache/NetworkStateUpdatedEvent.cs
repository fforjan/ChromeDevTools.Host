namespace ChromeDevTools.Host.Runtime.ApplicationCache
{
    using Newtonsoft.Json;

    /// <summary>
    /// NetworkStateUpdated
    /// </summary>
    public sealed class NetworkStateUpdatedEvent : IEvent
    {
        /// <summary>
        /// Gets or sets the isNowOnline
        /// </summary>
        [JsonProperty("isNowOnline")]
        public bool IsNowOnline
        {
            get;
            set;
        }
    }
}