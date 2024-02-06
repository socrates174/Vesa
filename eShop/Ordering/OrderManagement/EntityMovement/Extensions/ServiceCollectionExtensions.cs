using eShop.Ordering.OrderManagement.Data.Entities;
using IoCloud.Shared.ChangeFeed.Cosmos.Extensions;
using IoCloud.Shared.Persistence.Cosmos.Extensions;
using IoCloud.Shared.Utility;
using MediatR;

namespace eShop.Ordering.OrderManagement.EntityMovement.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void Configure(this IServiceCollection services, IConfiguration configuration)
        {
            // Get assembliess with command and domainEvent definitions
            var changeFeedAssembly = AssemblyUtils.GetAssemblyByName("IoCloud.Shared.ChangeFeed.Cosmos");

            // Load command, events, handlers
            services.AddMediatR(changeFeedAssembly);

            // Load all the profiles defined
            services.AddAutoMapper(changeFeedAssembly);

            // Get a Cosmos client
            var cosmosClient = services.GetCosmosClient(configuration);

            // Add Change Feed mover
            services.AddChangeFeedEntityMovement<Order, Audit, Guid, InboxCommand, OutboxEvent>
            (
                configuration,
                cosmosClient,
                "OrderManagementChangeFeedProcessorConfiguration",
                "OrderManagementEntityMovementConfiguration"
            );
        }
    }
}
