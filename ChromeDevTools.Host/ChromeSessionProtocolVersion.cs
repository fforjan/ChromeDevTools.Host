using System;
using Newtonsoft.Json;

namespace ChromeDevTools.Host
{
    public class ChromeSessionProtocolVersion {

        [JsonProperty]
        public string Browser { get; set; }

        [JsonProperty("Protocol-Version")]
        public string ProtocolVersion { get; set; }

        public static ChromeSessionProtocolVersion CreateFrom(string productName, Version productVersion)
        {
            return new ChromeSessionProtocolVersion
            {
                Browser = $"{productName}/v{productVersion.ToString()}",
                ProtocolVersion = "1.1"
            };
        }
    }
}