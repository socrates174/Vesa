using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class CosmosContainerConfiguration : ICosmosContainerConfiguration
    {
        public CosmosContainerConfiguration()
        {
        }

        public CosmosContainerConfiguration(string databaseName, string containerName, string partitionKeyPath) : this()
        {
            DatabaseName = databaseName;
            ContainerName = containerName;
            PartitionKeyPath = partitionKeyPath;
        }

        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public string PartitionKeyPath { get; set; }
        public string[] UniqueKeyPaths { get; set; } = null;
    }

    public class CosmosContainerConfiguration<TEntity> : CosmosContainerConfiguration, ICosmosContainerConfiguration<TEntity>
        where TEntity : class
    {
        public CosmosContainerConfiguration() : base()
        {
        }

        public CosmosContainerConfiguration(string databaseName, string containerName, string partitionKeyPath)
            : base(databaseName, containerName, partitionKeyPath)
        {
        }
    }
}