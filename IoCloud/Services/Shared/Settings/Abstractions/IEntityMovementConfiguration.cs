namespace IoCloud.Shared.Settings.Abstractions
{
    interface IEntityMovementConfiguration
    {
        string DatabaseName { get; set; }
        string PartitionKeyPath { get; set; }
        string LeaseContainerName { get; set; }
        string EntityContainerName { get; set; }
        string AuditContainerName { get; set; }
        string InboxMessageContainerName { get; set; }
        string OutboxMessageContainerName { get; set; }
    }
}