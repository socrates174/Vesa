using eShop.Ordering.OrderInquiry.Data.Entities;
using IoCloud.Shared.Messaging.EventHub.Extensions;
using IoCloud.Shared.Persistence.Cosmos.Extensions;
using IoCloud.Shared.Querying.Cosmos.Extensions;
using IoCloud.Shared.Storage.Documents.Extensions;
using IoCloud.Shared.Utility;
using MediatR;

namespace eShop.Ordering.OrderInquiry.HubAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void Configure(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceAssembly = AssemblyUtils.GetAssemblyByName("eShop.Ordering.OrderInquiry.Service");

            // Load command, events, handlers
            services.AddMediatR(serviceAssembly);

            // Load all the profiles defined in the use cases
            services.AddAutoMapper(serviceAssembly);

            // Add Cosmos
            services.AddCosmosClient(configuration);
            services.AddCosmosContainerConfiguration<Order>(configuration, "OrderContainerConfiguration");
            services.AddCosmosContainerConfiguration<InboxEvent>(configuration, "InboxEventContainerConfiguration");

            services.AddCosmosPersistence();
            services.AddCosmosQuerying();


            // Add Event Hub Consumption with CheckPointing and event processed check

            // Blob Storage client needed for CloudEvent processing
            services.AddBlobContainerClient(configuration);

            // Add Event Hub Consumption Service with CheckPointing and event processed check
            services.AddEventHubConsumption(configuration);
        }
    }
}
