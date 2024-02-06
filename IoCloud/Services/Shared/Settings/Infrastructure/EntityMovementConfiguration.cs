using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class EntityMovementConfiguration : IEntityMovementConfiguration
    {
        public string DatabaseName { get; set; }
        public string PartitionKeyPath { get; set; }
        public string LeaseContainerName { get; set; }
        public string EntityContainerName { get; set; }
        public string AuditContainerName { get; set; }
        public string InboxMessageContainerName { get; set; }
        public string OutboxMessageContainerName { get; set; }
    }
}
