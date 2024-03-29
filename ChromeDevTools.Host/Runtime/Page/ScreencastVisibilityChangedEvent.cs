namespace ChromeDevTools.Host.Runtime.Page
{
    using Newtonsoft.Json;

    /// <summary>
    /// Fired when the page with currently enabled screencast was shown or hidden `.
    /// </summary>
    public sealed class ScreencastVisibilityChangedEvent : IEvent
    {
        /// <summary>
        /// True if the page is visible.
        /// </summary>
        [JsonProperty("visible")]
        public bool Visible
        {
            get;
            set;
        }
    }
}