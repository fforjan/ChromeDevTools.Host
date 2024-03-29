namespace ChromeDevTools.Host.Runtime.DOMStorage
{
    using Newtonsoft.Json;

    /// <summary>
    /// DomStorageItemRemoved
    /// </summary>
    public sealed class DomStorageItemRemovedEvent : IEvent
    {
        /// <summary>
        /// Gets or sets the storageId
        /// </summary>
        [JsonProperty("storageId")]
        public StorageId StorageId
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the key
        /// </summary>
        [JsonProperty("key")]
        public string Key
        {
            get;
            set;
        }
    }
}