using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class EventToCurrentStateConfiguration : IEventToCurrentStateConfiguration
    {
        public string DatabaseName { get; set; }
        public string PartitionKeyPath { get; set; }
        public string LeaseContainerName { get; set; }
        public string OutboxMessageContainerName { get; set; }
        public int MaxRetryAttemptsOnRateLimitedRequests { get; set; } = 9; // Microsoft Default Value
        public int MaxRetryWaitTimeOnRateLimitedRequestsInSeconds { get; set; } = 30; // Microsoft Default Value
    }
}
