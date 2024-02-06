namespace IoCloud.Shared.Settings.Abstractions
{
    public interface ICosmosClientConfiguration
    {
        string UrlKey { get; set; }
        string AuthKey { get; set; }
        int MaxRetryAttemptsOnRateLimitedRequests { get; set; }
        int MaxRetryWaitTimeOnRateLimitedRequestsInSeconds { get; set; }
    }

    public interface ICosmosClientConfiguration<TEntity> : ICosmosClientConfiguration
        where TEntity : class
    {
    }
}
