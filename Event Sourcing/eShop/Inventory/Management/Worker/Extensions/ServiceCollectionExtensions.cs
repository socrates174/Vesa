﻿using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;
using vesa.Cosmos.Extensions;
using vesa.EventHub.Extensions;
using vesa.File.Extensions;
using vesa.Kafka.Extensions;
using eShop.Inventory.Core.Events;
using eShop.Inventory.Core.IntegrationEvents;
using eShop.Inventory.Management.Core.Abstractions;
using eShop.Inventory.Management.Core.Infrastructure;
using eShop.Inventory.Management.Service.Slices.ReorderStock;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using vesa.Blob.Extensions;

namespace eShop.Inventory.Management.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Configure(this IServiceCollection services, IConfiguration configuration)
    {
        //Initialize stores - these creates the required filesystem directories on startup if they do not exist.

        // EventStore Registration
        //services.AddFileEventStore(configuration);
        //services.AddCosmosEventStore(configuration);

        // Event Hub Consumers and Publishers Registration
        //services.AddFileEventHub(configuration);

        // Event Listeners Registration
        //services.AddFileEventListeners(configuration);
        //services.AddCosmosEventStoreListener(configuration);

        // Add Event Hub Publication
        //services.AddEventHubPublication(configuration);

        // Add Event Hub Consumption with CheckPointing and event processed check
        //
        //// Blob Storage client needed for CloudEvent processing because EventHub uses BlobStorage for checkpointing
        //services.AddBlobContainerClient(configuration);

        //// Add Event Hub Consumption Service with CheckPointing and event processed check
        //services.AddEventHubConsumption(configuration);
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };


        switch (configuration["EventStore"])
        {
            case "File":
                services.AddFileEventStore(configuration);
                services.AddFileEventListeners(configuration);
                break;
            case "Blob":
                //services.AddBlobEventStore(configuration);
                //services.AddBlobEventStoreListener(configuration);
                break;
            case "Cosmos":
                services.AddCosmosEventStore(configuration);
                services.AddCosmosEventStoreListener(configuration);
                break;
        }


        switch (configuration["MessageHub"])
        {
            case "File":
                services.AddFileEventHub(configuration);
                break;
            case "Kafka":
                services.AddEventMappings(configuration);
                services.AddKafkaEventConsumption(configuration);
                break;
            case "EventHub":
                //TODO
                //services.AddEventHubPublication(configuration);
                services.AddEventMappings(configuration);
                services.AddBlobContainerClient(configuration);
                services.AddEventHubConsumption(configuration);
                break;

        }

        // Domain and Command handlers

        services.AddTransient<IDomainEvents, DomainEvents>();

        // ReorderStock Slice
        services.AddTransient<ICommandHandler<ReorderStockCommand>, ReorderStockHandler>();
        services.AddTransient<IDomain<ReorderStockCommand>, ReorderStockDomain>();

        // Event observers
        services.AddTransient<IEventHandler<StockReorderedIntegrationEvent>, ReorderStockHandler>();
        services.AddTransient<IEventHandler<StockReorderedEvent>, ReorderStockHandler>();

        // Event observeds
        services.AddSingleton<IEventObservers, EventObservers<StockReorderedIntegrationEvent>>();
        services.AddSingleton<IEventObservers, EventObservers<StockReorderedEvent>>();

        // Miscellaneous registrations
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddTransient<ISupplier>(_ => new Supplier("XXX", "Supplier XXX", "orders@xxx.com", new EmailSender()));

        return services;
    }
}
