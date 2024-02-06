namespace IoCloud.Shared.Settings.Abstractions
{
    public interface IBlobStorageConfiguration
    {
        string ContainerName { get; set; }
        string ConnectionStringKey { get; set; }
        int MaxRetries { get; set; }
        int DelayInSeconds { get; set; }
        bool IsLoggingEnabled { get; set; }
        string ApplicationId { get; set; }
    }
}
