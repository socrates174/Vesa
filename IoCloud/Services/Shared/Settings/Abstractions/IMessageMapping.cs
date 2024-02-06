namespace IoCloud.Shared.Settings.Abstractions
{
    public interface IMessageMapping
    {
        string MessageType { get; set; }
        string ExternalType { get; set; }
    }
}