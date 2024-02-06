using Microsoft.Azure.Cosmos;
using IoCloud.Shared.Settings.Infrastructure;

namespace IoCloud.Shared.ChangeFeed.Cosmos.Extensions
{
    public static class CosmosClientExtensions
    {
        public static void Configure(this CosmosClient cosmosClient, CosmosContainerConfiguration cosmosContainerConfiguration)
        {
            var cosmosDatabaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosContainerConfiguration.DatabaseName).GetAwaiter().GetResult();
            var cosmosDatabase = cosmosDatabaseResponse.Database;

            var sourceCosmosContainerResponse = cosmosDatabase.CreateContainerIfNotExistsAsync
            (
                cosmosContainerConfiguration.ContainerName,
                cosmosContainerConfiguration.PartitionKeyPath,
                400
            )
            .GetAwaiter()
            .GetResult();
        }
    }
}
