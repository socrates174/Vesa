using eShop.Ordering.OrderManagement.Data.Entities;
using IoCloud.Shared.ChangeFeed.Cosmos.Extensions;

namespace eShop.Ordering.OrderManagement.EventPublication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void Configure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Change Feed Publication
            services.AddChangeFeedEventPublication<OutboxEvent>(configuration);
        }
    }
}
