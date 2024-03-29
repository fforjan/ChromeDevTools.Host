namespace ChromeDevTools.Host.Runtime.CSS
{
    using Newtonsoft.Json;

    /// <summary>
    /// ShorthandEntry
    /// </summary>
    public sealed class ShorthandEntry
    {
        /// <summary>
        /// Shorthand name.
        ///</summary>
        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// Shorthand value.
        ///</summary>
        [JsonProperty("value")]
        public string Value
        {
            get;
            set;
        }
        /// <summary>
        /// Whether the property has "!important" annotation (implies `false` if absent).
        ///</summary>
        [JsonProperty("important", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Important
        {
            get;
            set;
        }
    }
}