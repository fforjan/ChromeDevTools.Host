namespace ChromeDevTools.Host.Runtime.DOMSnapshot
{
    using Newtonsoft.Json;

    /// <summary>
    /// Details of post layout rendered text positions. The exact layout should not be regarded as
    /// stable and may change between versions.
    /// </summary>
    public sealed class InlineTextBox
    {
        /// <summary>
        /// The absolute position bounding box.
        ///</summary>
        [JsonProperty("boundingBox")]
        public DOM.Rect BoundingBox
        {
            get;
            set;
        }
        /// <summary>
        /// The starting index in characters, for this post layout textbox substring. Characters that
        /// would be represented as a surrogate pair in UTF-16 have length 2.
        ///</summary>
        [JsonProperty("startCharacterIndex")]
        public long StartCharacterIndex
        {
            get;
            set;
        }
        /// <summary>
        /// The number of characters in this post layout textbox substring. Characters that would be
        /// represented as a surrogate pair in UTF-16 have length 2.
        ///</summary>
        [JsonProperty("numCharacters")]
        public long NumCharacters
        {
            get;
            set;
        }
    }
}