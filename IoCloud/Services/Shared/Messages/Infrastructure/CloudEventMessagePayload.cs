using Newtonsoft.Json;

namespace IoCloud.Shared.Messages
{
    /// <summary>
    /// Is the payload of a message (event/command) consumed/published from a message hub with the metadata found in the Header property
    /// </summary>
    public abstract class CloudEventMessagePayload : ICloudEventMessagePayload
    {
        [JsonIgnore]
        public CloudEventMessageHeader Header { get; set; } = new();
        public string PayloadType => this.GetType().FullName;

    }
}
