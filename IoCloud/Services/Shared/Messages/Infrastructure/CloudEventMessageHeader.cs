namespace IoCloud.Shared.Messages
{
    /// <summary>
    /// Contains all the properties of a Cloud Event (metadata) except for the Data property (payload)
    /// </summary>
    public class CloudEventMessageHeader
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Subject { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Source { get; set; }
        public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;
        public string SpecVersion { get; set; } = "1.0";
        public string? DataSchema { get; set; }
        public string? RequestedBy { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }
}
