namespace IoCloud.Shared.Settings.Abstractions
{
    public interface IEventPublicationConfiguration
    {
        string DatabaseName { get; set; }
        string PartitionKeyPath { get; set; }
        string LeaseContainerName { get; set; }
        string OutboxMessageContainerName { get; set; }
    }
}