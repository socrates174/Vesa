using eShop.Ordering.Infrastructure.SQL.Context;
using eShop.Ordering.Infrastructure.SQL.Entities;
using eShop.Ordering.Inquiry.Service.Slices.GetCustomerOrders;
using eShop.Ordering.Inquiry.Service.Slices.GetDailyOrders;
using eShop.Ordering.Inquiry.Service.Slices.GetOrder;
using eShop.Ordering.Inquiry.Service.Slices.GetStatusOrders;
using eShop.Ordering.Inquiry.StateViews;
using eShop.Ordering.Management.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using vesa.Blob.Extensions;
using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;
using vesa.Cosmos.Extensions;
using vesa.Cosmos.Infrastructure;
using vesa.File.Extensions;
using vesa.File.Infrastructure;
using vesa.SQL.Extensions;
using vesa.SQL.Infrastructure;

namespace eShop.Ordering.Inquiry.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Configure(this IServiceCollection services, IConfiguration configuration)
    {
        // settings will automatically be used by JsonConvert.SerializeObject/DeserializeObject
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        //Initialize stores - these creates the required filesystem directories on startup if they do not exist.

        // EventStore and EventStoreListener Registration
        switch (configuration["EventStore"])
        {
            case "File":
                services.AddFileEventStore(configuration);
                services.AddFileEventListeners(configuration);
                break;
            case "Blob":
                services.AddBlobEventStore(configuration);
                services.AddBlobEventStoreListener(configuration);
                break;
            case "SQL":
                services.AddSQLStore<OrderingContext>(configuration, ServiceLifetime.Singleton);
                services.AddSQLEventListeners(configuration);
                services.AddTransient<IEventStore, SQLEventStore>();
                break;
            case "Cosmos":
                services.AddCosmosEventStore(configuration);
                services.AddCosmosEventStoreListener(configuration);
                break;
        }

        switch (configuration["StateViewStore"])
        {
            case "File":
                services.AddScoped(typeof(IStateViewStore<>), typeof(FileStateViewStore<>));
                break;
            case "SQL":
                services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
                if (configuration["EventStore"] != "SQL")
                {
                    services.AddSQLStore<OrderingContext>(configuration, ServiceLifetime.Singleton);
                }
                services.AddTransient(typeof(IStateViewStore<OrderStateView>), typeof(SQLStateViewStore<OrderStateViewJson, OrderStateView>));
                services.AddTransient(typeof(IStateViewStore<CustomerOrdersStateView>), typeof(SQLStateViewStore<CustomerOrdersStateViewJson, CustomerOrdersStateView>));
                services.AddTransient(typeof(IStateViewStore<StatusOrdersStateView>), typeof(SQLStateViewStore<StatusOrdersStateViewJson, StatusOrdersStateView>));
                services.AddTransient(typeof(IStateViewStore<DailyOrdersStateView>), typeof(SQLStateViewStore<DailyOrdersStateViewJson, DailyOrdersStateView>));
                break;
            case "Cosmos":
                if (configuration["EventStore"] != "Cosmos")
                {
                    services.AddCosmosClient(configuration);
                    services.AddCosmosContainerConfiguration(configuration);
                    services.InitializeDatabase(configuration);
                }
                services.AddCosmosContainerConfiguration<IStateView>(configuration, "StateViewCosmosContainerConfiguration");
                services.AddTransient(typeof(IStateViewStore<>), typeof(CosmosStateViewStore<>));
                break;
        }

        // OrderStateView updaters
        services.AddTransient<IEventHandler<OrderPlacedEvent>, OrderStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderCancelledEvent>, OrderStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderReturnedEvent>, OrderStateViewUpdater>();

        // CustomerOrdersStateView updaters
        services.AddTransient<IEventHandler<OrderPlacedEvent>, CustomerOrdersStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderCancelledEvent>, CustomerOrdersStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderReturnedEvent>, CustomerOrdersStateViewUpdater>();

        // StatusOrdersStateView updaters
        services.AddTransient<IEventHandler<OrderPlacedEvent>, StatusOrdersStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderCancelledEvent>, StatusOrdersStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderReturnedEvent>, StatusOrdersStateViewUpdater>();

        //DailyOrdersStateView updaters
        services.AddTransient<IEventHandler<OrderPlacedEvent>, DailyOrdersStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderCancelledEvent>, DailyOrdersStateViewUpdater>();
        services.AddTransient<IEventHandler<OrderReturnedEvent>, DailyOrdersStateViewUpdater>();

        // Event observers
        services.AddScoped<IEventObservers, EventObservers<OrderPlacedEvent>>();
        services.AddScoped<IEventObservers, EventObservers<OrderCancelledEvent>>();
        services.AddScoped<IEventObservers, EventObservers<OrderReturnedEvent>>();

        return services;
    }
}