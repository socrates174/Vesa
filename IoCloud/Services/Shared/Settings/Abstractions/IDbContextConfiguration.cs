namespace IoCloud.Shared.Settings.Abstractions
{
    public interface IDbContextConfiguration
    {
        string ConnectionStringKey { get; set; }
        string DatabaseName { get; set; }
        int RetryCount { get; set; }
        int RetryAttemptIntervalMaxCapInSeconds { get; set; }
        int TimeoutInSeconds { get; set; }
    }
}