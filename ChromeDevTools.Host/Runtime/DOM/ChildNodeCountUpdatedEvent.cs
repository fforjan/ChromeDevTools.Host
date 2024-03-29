namespace ChromeDevTools.Host.Runtime.DOM
{
    using Newtonsoft.Json;

    /// <summary>
    /// Fired when `Container`'s child node count has changed.
    /// </summary>
    public sealed class ChildNodeCountUpdatedEvent : IEvent
    {
        /// <summary>
        /// Id of the node that has changed.
        /// </summary>
        [JsonProperty("nodeId")]
        public long NodeId
        {
            get;
            set;
        }
        /// <summary>
        /// New node count.
        /// </summary>
        [JsonProperty("childNodeCount")]
        public long ChildNodeCount
        {
            get;
            set;
        }
    }
}