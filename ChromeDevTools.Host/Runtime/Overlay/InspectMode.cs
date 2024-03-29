namespace ChromeDevTools.Host.Runtime.Overlay
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime.Serialization;

    /// <summary>
    /// InspectMode
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InspectMode
    {
        [EnumMember(Value = "searchForNode")]
        SearchForNode,
        [EnumMember(Value = "searchForUAShadowDOM")]
        SearchForUAShadowDOM,
        [EnumMember(Value = "none")]
        None,
    }
}