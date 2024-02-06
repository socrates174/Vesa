using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class EventPublicationConfiguration : IEventPublicationConfiguration
    {
        public string DatabaseName { get; set; }
        public string PartitionKeyPath { get; set; }
        public string LeaseContainerName { get; set; }
        public string OutboxMessageContainerName { get; set; }
    }
}
