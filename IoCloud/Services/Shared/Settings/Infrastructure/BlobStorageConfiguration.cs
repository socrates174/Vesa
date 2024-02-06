using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class BlobStorageConfiguration : IBlobStorageConfiguration
    {
        public string ContainerName { get; set; }
        public string ConnectionStringKey { get; set; }
        public int MaxRetries { get; set; } = 5;
        public int DelayInSeconds { get; set; } = 3;
        public bool IsLoggingEnabled { get; set; }
        public string ApplicationId { get; set; }
    }
}
