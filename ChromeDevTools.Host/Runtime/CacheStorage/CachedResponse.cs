namespace ChromeDevTools.Host.Runtime.CacheStorage
{
    using Newtonsoft.Json;

    /// <summary>
    /// Cached response
    /// </summary>
    public sealed class CachedResponse
    {
        /// <summary>
        /// Entry content, base64-encoded.
        ///</summary>
        [JsonProperty("body")]
        public string Body
        {
            get;
            set;
        }
    }
}