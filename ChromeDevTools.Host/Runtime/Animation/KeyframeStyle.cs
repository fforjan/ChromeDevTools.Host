namespace ChromeDevTools.Host.Runtime.Animation
{
    using Newtonsoft.Json;

    /// <summary>
    /// Keyframe Style
    /// </summary>
    public sealed class KeyframeStyle
    {
        /// <summary>
        /// Keyframe's time offset.
        ///</summary>
        [JsonProperty("offset")]
        public string Offset
        {
            get;
            set;
        }
        /// <summary>
        /// `AnimationEffect`'s timing function.
        ///</summary>
        [JsonProperty("easing")]
        public string Easing
        {
            get;
            set;
        }
    }
}