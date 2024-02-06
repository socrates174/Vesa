using eShop.Ordering.OrderInquiry.Data.Entities;
using eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrders;
using eShop.Ordering.OrderInquiry.WebAPI.Routers;
using FluentValidation;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.HttpRouting.Infrastructure;
using IoCloud.Shared.Querying.Cosmos.Extensions;
using IoCloud.Shared.Utility;
using MediatR;

namespace eShop.Ordering.OrderInquiry.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void Configure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var serviceAssembly = AssemblyUtils.GetAssemblyByName("eShop.Ordering.OrderInquiry.Service");

            // Load command, events, handlers
            services.AddMediatR(serviceAssembly);

            // Add command validation
            services.AddValidatorsFromAssemblyContaining<ViewOrdersValidator>();

            // Load all the profiles defined in the use cases
            services.AddAutoMapper(serviceAssembly);

            services.AddTransient<IHttpRouterCollection, HttpRouterCollection>();
            services.AddTransient<IHttpRouter, ViewOrderDetailsRouter>();
            services.AddTransient<IHttpRouter, ViewOrdersRouter>();

            services.AddCosmosClientQ(configuration);

            services.AddCosmosContainerConfigurationQ<Order>(configuration, "OrderContainerConfiguration");

            services.AddCosmosQuerying();
        }
    }
}
