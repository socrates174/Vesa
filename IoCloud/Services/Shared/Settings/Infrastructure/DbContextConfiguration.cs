using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class DbContextConfiguration : IDbContextConfiguration
    {
        public string ConnectionStringKey { get; set; }
        public string DatabaseName { get; set; }
        public int RetryCount { get; set; }
        public int RetryAttemptIntervalMaxCapInSeconds { get; set; }
        public int TimeoutInSeconds { get; set; }
    }
}
