namespace ChromeDevTools.Host.Runtime.DOMStorage
{
    using Newtonsoft.Json;

    /// <summary>
    /// DomStorageItemsCleared
    /// </summary>
    public sealed class DomStorageItemsClearedEvent : IEvent
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
    }
}