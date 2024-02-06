namespace IoCloud.Shared.Settings.Abstractions
{
    public interface IEventToCurrentStateConfiguration
    {
        string DatabaseName { get; set; }
        string LeaseContainerName { get; set; }
        int MaxRetryAttemptsOnRateLimitedRequests { get; set; }
        int MaxRetryWaitTimeOnRateLimitedRequestsInSeconds { get; set; }
        string OutboxMessageContainerName { get; set; }
        string PartitionKeyPath { get; set; }
    }
}
