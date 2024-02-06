using eShop.Ordering.OrderManagement.Data.Entities;
using eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder;
using eShop.Ordering.OrderManagment.WebAPI.Routers;
using FluentValidation;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.HttpRouting.Infrastructure;
using IoCloud.Shared.Persistence.Cosmos.Extensions;
using IoCloud.Shared.Querying.Cosmos.Extensions;
using IoCloud.Shared.Utility;
using MediatR;

namespace eShop.Ordering.OrderManagement.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void Configure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var serviceAssembly = AssemblyUtils.GetAssemblyByName("eShop.Ordering.OrderManagement.Service");

            services.AddTransient<IPlaceOrderDomain, PlaceOrderDomain>();

            // Load command, events, handlers
            services.AddMediatR(serviceAssembly);

            // Add command validation
            services.AddValidatorsFromAssemblyContaining<CancelOrderValidator>();

            // Load all the profiles defined in the use cases
            services.AddAutoMapper(serviceAssembly);

            // HttpRouters
            //services.AddTransient<IRequestedByResolver>(sp => new JwtTokenClaimResolver("UserId")); // this is another way of calling the line below
            services.AddTransient<IRequestedByResolver, JwtTokenUserIdResolver>();
            services.AddTransient<IHttpRouterCollection, HttpRouterCollection>();
            services.AddTransient<IHttpRouter, PlaceOrderRouter>();

            // Add Cosmos
            services.AddCosmosClient(configuration);
            services.AddCosmosContainerConfiguration<Order>(configuration, "OrderContainerConfiguration");
            services.InitializeDatabase(configuration);

            services.AddCosmosPersistence();
            services.AddCosmosQuerying();
        }
    }
}
