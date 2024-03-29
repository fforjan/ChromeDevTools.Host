namespace ChromeDevTools.Host.Runtime.Overlay
{
    using Newtonsoft.Json;

    /// <summary>
    /// Fired when the node should be highlighted. This happens after call to `setInspectMode`.
    /// </summary>
    public sealed class NodeHighlightRequestedEvent : IEvent
    {
        /// <summary>
        /// Gets or sets the nodeId
        /// </summary>
        [JsonProperty("nodeId")]
        public long NodeId
        {
            get;
            set;
        }
    }
}