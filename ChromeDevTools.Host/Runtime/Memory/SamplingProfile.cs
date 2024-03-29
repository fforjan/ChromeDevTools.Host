namespace ChromeDevTools.Host.Runtime.Memory
{
    using Newtonsoft.Json;

    /// <summary>
    /// Array of heap profile samples.
    /// </summary>
    public sealed class SamplingProfile
    {
        /// <summary>
        /// samples
        ///</summary>
        [JsonProperty("samples")]
        public SamplingProfileNode[] Samples
        {
            get;
            set;
        }
        /// <summary>
        /// modules
        ///</summary>
        [JsonProperty("modules")]
        public Module[] Modules
        {
            get;
            set;
        }
    }
}