using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;
using vesa.EventHub.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace vesa.EventHub.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventHubPublication
    (
        this IServiceCollection services,
        IConfiguration configuration,
        string eventHubProducerConfigurationSectionName = "EventHubProducerConfiguration"
    )
    {
        var eventHubProducerConfiguration = configuration.GetSection(eventHubProducerConfigurationSectionName).Get<EventHubProducerConfiguration>();
        EventHubProducerClientOptions eventHubProducerClientOptions = SetEventHubProducerClientOptions(eventHubProducerConfiguration);

        var eventHubProducerClient = new EventHubProducerClient
        (
            configuration[eventHubProducerConfiguration.ConnectionStringKey],
            eventHubProducerClientOptions
        );
        services.AddSingleton(eventHubProducerClient);

        services.AddScoped<IEventPublisher, EventHubPublisher>();

        return services;
    }

    private static EventHubProducerClientOptions SetEventHubProducerClientOptions(EventHubProducerConfiguration eventHubProducerConfiguration)
    {
        EventHubsRetryMode eventHubsRetryMode = EventHubsRetryMode.Exponential;

        if (!string.IsNullOrWhiteSpace(eventHubProducerConfiguration.EventHubsRetryMode) && eventHubProducerConfiguration.EventHubsRetryMode.ToUpper() == "FIXED")
        {
            _ = Enum.TryParse(eventHubProducerConfiguration.EventHubsRetryMode, out eventHubsRetryMode);
        }

        EventHubProducerClientOptions eventHubProducerClientOptions = new()
        {
            RetryOptions = new()
            {
                MaximumRetries = eventHubProducerConfiguration.MaximumRetries,
                MaximumDelay = TimeSpan.FromSeconds(eventHubProducerConfiguration.MaximumDelay),
                TryTimeout = TimeSpan.FromSeconds(eventHubProducerConfiguration.TryTimeOut),
                Mode = eventHubsRetryMode
            },
        };
        return eventHubProducerClientOptions;
    }

    public static IServiceCollection AddEventHubConsumption
    (
        this IServiceCollection services,
        IConfiguration configuration,
        string eventProcessorConfigurationSectionName = "EventProcessorConfiguration"
    )
    {
        // EventHub Consumer configuration needed for CloudEvent processing
        var eventProcessorConfiguration = configuration.GetSection(eventProcessorConfigurationSectionName).Get<EventProcessorConfiguration>();

        var sp = services.BuildServiceProvider();

        EventProcessorClientOptions eventProcessorClientOptions = SetEventProcessorClientOptions(eventProcessorConfiguration);
        var consumerGroup = configuration[eventProcessorConfiguration.ConsumerGroup];
        var eventProcessorClient = new EventProcessorClient
           (
              sp.GetRequiredService<BlobContainerClient>(),
              string.IsNullOrWhiteSpace(consumerGroup) ? EventHubConsumerClient.DefaultConsumerGroupName : consumerGroup,
              configuration[eventProcessorConfiguration.ConnectionStringKey],
              eventProcessorClientOptions
           );

        services.AddSingleton(eventProcessorClient);


        //var eventMappings = configuration.GetSection("EventMappings").Get<InternalMessageMapping[]>();
        //services.AddSingleton<IInternalMessageMapping[]>(eventMappings);

        services.AddSingleton<IEventListener, EventHubListener>();
        services.AddSingleton<EventHubListener>();
        services.AddSingleton<IEventProcessor, EventProcessor>();

        return services;
    }


    private static EventProcessorClientOptions SetEventProcessorClientOptions(EventProcessorConfiguration eventProcessorConfiguration)
    {
        EventHubsRetryMode eventHubsRetryMode = EventHubsRetryMode.Exponential;

        if (!string.IsNullOrWhiteSpace(eventProcessorConfiguration.EventHubsRetryMode) && eventProcessorConfiguration.EventHubsRetryMode.ToUpper() == "FIXED")
        {
            _ = Enum.TryParse(eventProcessorConfiguration.EventHubsRetryMode, out eventHubsRetryMode);
        }

        EventProcessorClientOptions eventProcessorClientOptions = new()
        {
            RetryOptions = new()
            {
                MaximumRetries = eventProcessorConfiguration.MaximumRetries,
                MaximumDelay = TimeSpan.FromSeconds(eventProcessorConfiguration.MaximumDelay),
                TryTimeout = TimeSpan.FromSeconds(eventProcessorConfiguration.TryTimeOut),
                Mode = eventHubsRetryMode
            },
        };

        return eventProcessorClientOptions;
    }
}
