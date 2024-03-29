namespace ChromeDevTools.Host.Runtime.DOM
{
    using Newtonsoft.Json;

    /// <summary>
    /// Mirrors `DOMNodeRemoved` event.
    /// </summary>
    public sealed class ChildNodeRemovedEvent : IEvent
    {
        /// <summary>
        /// Parent id.
        /// </summary>
        [JsonProperty("parentNodeId")]
        public long ParentNodeId
        {
            get;
            set;
        }
        /// <summary>
        /// Id of the node that has been removed.
        /// </summary>
        [JsonProperty("nodeId")]
        public long NodeId
        {
            get;
            set;
        }
    }
}