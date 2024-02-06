namespace IoCloud.Shared.Messages
{
    public interface ICloudEventMessagePayload
    {
        CloudEventMessageHeader Header { get; set; }
        string PayloadType { get; }
    }
}