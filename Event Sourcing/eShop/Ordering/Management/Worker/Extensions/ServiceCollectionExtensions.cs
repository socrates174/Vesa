using eShop.Ordering.Infrastructure.SQL.Context;
using eShop.Ordering.Infrastructure.SQL.Entities;
using eShop.Ordering.Inquiry.StateViews;
using eShop.Ordering.Management.Application.Abstractions;
using eShop.Ordering.Management.Application.Infrastructure;
using eShop.Ordering.Management.Events;
using eShop.Ordering.Management.Service.Slices.CancelOrder;
using eShop.Ordering.Management.Service.Slices.PlaceOrder;
using eShop.Ordering.Management.Service.Slices.ReorderStock;
using eShop.Ordering.Management.Service.Slices.ReturnOrder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using vesa.Blob.Extensions;
using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;
using vesa.Cosmos.Extensions;
using vesa.Cosmos.Infrastructure;
using vesa.EventHub.Extensions;
using vesa.File.Extensions;
using vesa.File.Infrastructure;
using vesa.Kafka.Extensions;
using vesa.SQL.Extensions;
using vesa.SQL.Infrastructure;

namespace eShop.Ordering.Management.Worker.Extensions;

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
                services.AddSingleton(typeof(IStateViewStore<>), typeof(FileStateViewStore<>));
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

        switch (configuration["MessageHub"])
        {
            case "File":
                services.AddFileEventHub(configuration);
                break;
            case "Kafka":
                services.AddKafkaEventPublication(configuration);
                break;
            case "EventHub":
                //TODO
                services.AddEventHubPublication(configuration);
                //services.AddBlobContainerClient(configuration);
                //services.AddEventHubConsumption(configuration);
                break;

        }

        // Event Hub Consumers and Publishers Registration


        // Add Event Hub Publication
        //

        // Add Event Hub Consumption with CheckPointing and event processed check
        //
        //// Blob Storage client needed for CloudEvent processing
        //

        //// Add Event Hub Consumption Service with CheckPointing and event processed check
        //;


        // Domain and Command handlers

        // PlaceOrder Slice
        services.AddTransient<ICommandHandler<PlaceOrderCommand>, PlaceOrderHandler>();
        services.AddTransient<IDomain<PlaceOrderCommand>, PlaceOrderDomain>();

        // CancelOrder Slice
        services.AddTransient<ICommandHandler<CancelOrderCommand, OrderStateView>, CancelOrderHandler>();
        services.AddTransient<IDomain<CancelOrderCommand, OrderStateView>, CancelOrderDomain>();

        // ReturnOrder Slice
        services.AddTransient<ICommandHandler<ReturnOrderCommand, OrderStateView>, ReturnOrderHandler>();
        services.AddTransient<IDomain<ReturnOrderCommand, OrderStateView>, ReturnOrderDomain>();

        // ReorderStock Slice
        services.AddTransient<ICommandHandler<ReorderStockCommand>, ReorderStockHandler>();
        services.AddTransient<IDomain<ReorderStockCommand>, ReorderStockDomain>();

        // Event handlers
        services.AddSingleton<IEventHandler<OrderPlacedEvent>, OrderPlacedHandler>();
        services.AddSingleton<IEventHandler<OrderCancelledEvent>, OrderCancelledHandler>();
        services.AddSingleton<IEventHandler<OrderReturnedEvent>, OrderReturnedHandler>();
        services.AddSingleton<IEventHandler<OutOfStockExceptionEvent>, OutOfStockExceptionHandler>();
        services.AddSingleton<IEventHandler<StockReorderedEvent>, StockReorderedHandler>();

        // Event observers
        services.AddSingleton<IEventObservers, EventObservers<OrderPlacedEvent>>();
        services.AddSingleton<IEventObservers, EventObservers<OrderCancelledEvent>>();
        services.AddSingleton<IEventObservers, EventObservers<OrderReturnedEvent>>();
        services.AddSingleton<IEventObservers, EventObservers<OutOfStockExceptionEvent>>();
        services.AddSingleton<IEventObservers, EventObservers<StockReorderedEvent>>();

        // We need the state views' Subject in order to write the events to a partition that the state view can be hydrated from
        services.AddTransient<IDomainEvents, DomainEvents>();

        // Mapping that OrderStateView is interested in OrderPlacedEvent, OrderCancelledEvent and OrderReturnedEvent
        services.AddTransient<IStateView<OrderPlacedEvent>, OrderStateView>();
        services.AddTransient<IStateView<OrderCancelledEvent>, OrderStateView>();
        services.AddTransient<IStateView<OrderReturnedEvent>, OrderStateView>();

        // Mapping that CustomerOrdersStateView is interested in OrderPlacedEvent, OrderCancelledEvent and OrderReturnedEvent
        services.AddTransient<IStateView<OrderPlacedEvent>, CustomerOrdersStateView>();
        services.AddTransient<IStateView<OrderCancelledEvent>, CustomerOrdersStateView>();
        services.AddTransient<IStateView<OrderReturnedEvent>, CustomerOrdersStateView>();

        // Mapping that StatusOrdersStateView is interested in OrderPlacedEvent, OrderCancelledEvent and OrderReturnedEvent
        services.AddTransient<IStateView<OrderPlacedEvent>, StatusOrdersStateView>();
        services.AddTransient<IStateView<OrderCancelledEvent>, StatusOrdersStateView>();
        services.AddTransient<IStateView<OrderReturnedEvent>, StatusOrdersStateView>();

        // Mapping that DailyOrdersStateView is interested in OrderPlacedEvent, OrderCancelledEvent and OrderReturnedEvent
        services.AddTransient<IStateView<OrderPlacedEvent>, DailyOrdersStateView>();
        services.AddTransient<IStateView<OrderCancelledEvent>, DailyOrdersStateView>();
        services.AddTransient<IStateView<OrderReturnedEvent>, DailyOrdersStateView>();

        // Business components
        services.AddSingleton<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddSingleton<IInventoryChecker, InventoryChecker>();
        services.AddSingleton<IPaymentProcessor, PaymentProcessor>();
        services.AddSingleton<IDeliveryScheduler, DeliveryScheduler>();

        return services;
    }
}