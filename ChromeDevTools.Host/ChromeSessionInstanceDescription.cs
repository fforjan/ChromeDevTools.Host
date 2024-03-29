namespace ChromeDevTools.Host
{
    using Newtonsoft.Json;
    using System;


    public class ChromeSessionInstanceDescription
    {
        public const string PageType = "page";
        public const string NodeType = "node";

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("devtoolsFrontendUrl")]
        public string DevtoolsFrontendUrl { get; set; }

        [JsonProperty("devtoolsFrontendUrlCompat")]
        public string DevtoolsFrontendUrlCompat { get; set; }

        [JsonProperty("faviconUrl")]
        public string FaviconUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("webSocketDebuggerUrl")]
        public string WebSocketDebuggerUrl { get; set; }


        public static ChromeSessionInstanceDescription CreateFrom(
            string serverName,
            int serverPort,
            string title,
            string description,
            string faviconUrl,
            Guid id,
            string type = NodeType
        )
        {
            return new ChromeSessionInstanceDescription
            {
                Description = description,
                DevtoolsFrontendUrl =
                    $"chrome-devtools://devtools/bundled/js_app.html?experiments=true&v8only=true&ws={serverName}:{serverPort}/json/session/{id}",
                DevtoolsFrontendUrlCompat =
                    $"chrome-devtools://devtools/bundled/inspector.html?experiments=true&v8only=true&ws={serverName}:{serverPort}/json/session/{id}",
                FaviconUrl =
                    faviconUrl,
                Id = id.ToString(),
                Title = title,
                Type = type,
                Url = "file://",
                WebSocketDebuggerUrl = $"ws://{serverName}:{serverPort}/json/session/{id}"
            };
        }
    }
}