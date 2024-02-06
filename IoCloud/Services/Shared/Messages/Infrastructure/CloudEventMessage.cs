namespace IoCloud.Shared.Messages
{
    /// <summary>
    /// International standard schema for messages (events/commands) consumed/published from a message hub
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CloudEventMessage<T> : CloudEventMessage
        where T : class
    {
        public CloudEventMessage() : base()
        {
        }
    }

    public class CloudEventMessage : Message
    {
        public CloudEventMessage() : base()
        {
        }

        public string Type { get; set; }
        public string? Source { get; set; }
        public DateTimeOffset Time { get; set; }
        public string SpecVersion { get; set; }
        public string? DataSchema { get; set; }
    }
}
