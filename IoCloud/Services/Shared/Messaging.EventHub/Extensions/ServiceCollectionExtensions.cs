using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.Consumption.Abstractions;
using IoCloud.Shared.Messaging.Consumption.Infrastructure;
using IoCloud.Shared.Messaging.EventHub.Infrastructure;
using IoCloud.Shared.Messaging.Publication.Abstractions;
using IoCloud.Shared.Messaging.Publication.Infrastructure;
using IoCloud.Shared.Settings.Abstractions;
using IoCloud.Shared.Settings.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IoCloud.Shared.Messaging.EventHub.Extensions
{
    /// <summary>
    /// No using IConfiguration
    /// </summary>
    /// <param name="services"></param>
    /// <param name="consumerGroup"></param>
    /// <param name="connectionString"></param>
    /// <param name="messageMappings"></param>
    /// <returns></returns>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// No using IConfiguration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="consumerGroup"></param>
        /// <param name="connectionString"></param>
        /// <param name="messageMappings"></param>
        /// <returns></returns>
        /// Using IConfiguration to read event hub configuation and event mappings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventHubConsumption
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string eventProcessorConfigurationSection = "EventProcessorConfiguration"
        )
        {
            // EventHub Consumer configuration needed for CloudEvent processing
            services.Configure<EventProcessorConfiguration>(eventProcessorConfigurationSection, configuration.GetSection(eventProcessorConfigurationSection));
            var eventProcessorConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EventProcessorConfiguration>>().Get(eventProcessorConfigurationSection);

            var sp = services.BuildServiceProvider();

            EventProcessorClientOptions eventProcessorClientOptions = SetEventProcessorClientOptions(eventProcessorConfiguration);
            var consumerGroup = configuration[eventProcessorConfiguration.ConsumerGroup];
            var eventProcessorClient = new EventProcessorClient<CloudEventMessage>
               (
                  sp.GetRequiredService<BlobContainerClient>(),
                  string.IsNullOrEmpty(consumerGroup) ? EventHubConsumerClient.DefaultConsumerGroupName : consumerGroup,
                  configuration[eventProcessorConfiguration.ConnectionStringKey],
                  eventProcessorClientOptions
               );

            services.AddSingleton(eventProcessorClient);

            var messageMappings = configuration.GetSection("MessageMappings").Get<InternalMessageMapping[]>();
            services.AddSingleton<IInternalMessageMapping[]>(messageMappings);

            services.AddSingleton<IMessageProcessor<CloudEventMessage>, CloudEventProcessor>();

            return services;
        }

        public static IServiceCollection AddEventHubConsumption<TEvent>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string eventProcessorConfigurationSection = "EventProcessorConfiguration"
        )
            where TEvent : CloudEventMessage, new()
        {
            // EventHub Consumer configuration needed for CloudEvent processing
            services.Configure<EventProcessorConfiguration>(eventProcessorConfigurationSection, configuration.GetSection(eventProcessorConfigurationSection));
            var eventProcessorConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EventProcessorConfiguration>>().Get(eventProcessorConfigurationSection);

            var sp = services.BuildServiceProvider();

            EventProcessorClientOptions eventProcessorClientOptions = SetEventProcessorClientOptions(eventProcessorConfiguration);
            var consumerGroup = configuration[eventProcessorConfiguration.ConsumerGroup];
            var eventProcessorClient = new EventProcessorClient<TEvent>
            (
                sp.GetRequiredService<BlobContainerClient>(),
                string.IsNullOrEmpty(consumerGroup) ? EventHubConsumerClient.DefaultConsumerGroupName : consumerGroup,
                configuration[eventProcessorConfiguration.ConnectionStringKey],
                eventProcessorClientOptions
            );
            ;

            services.AddSingleton(eventProcessorClient);

            var messageMappings = configuration.GetSection("MessageMappings").Get<InternalMessageMapping[]>();
            services.AddSingleton<IInternalMessageMapping[]>(messageMappings);

            services.AddSingleton(typeof(IMessageProcessor<TEvent>), typeof(CloudEventProcessor<TEvent>));

            return services;
        }

        public static IServiceCollection AddEventHubPublication
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string eventHubProducerConfigurationSection = "EventHubProducerConfiguration"
        )
        {
            services.Configure<EventHubProducerConfiguration>(eventHubProducerConfigurationSection, configuration.GetSection(eventHubProducerConfigurationSection));
            var eventHubProducerConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EventHubProducerConfiguration>>().Get(eventHubProducerConfigurationSection);

            EventHubProducerClientOptions eventHubProducerClientOptions = SetEventHubProducerClientOptions(eventHubProducerConfiguration);

            var eventHubProducerClient = new EventHubProducerClient<CloudEventMessage>
            (
                configuration[eventHubProducerConfiguration.ConnectionStringKey],
                eventHubProducerClientOptions
            );
            services.AddSingleton(eventHubProducerClient);

            services.AddScoped(typeof(IMessagePublisher<>), typeof(EventHubCloudEventPublisher));

            return services;
        }

        public static IServiceCollection AddEventHubPublication<TEvent>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string eventHubProducerConfigurationSection = "EventHubProducerConfiguration"
        )
            where TEvent : CloudEventMessage
        {
            services.Configure<EventHubProducerConfiguration>(eventHubProducerConfigurationSection, configuration.GetSection(eventHubProducerConfigurationSection));
            var eventHubProducerConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EventHubProducerConfiguration>>().Get(eventHubProducerConfigurationSection);

            EventHubProducerClientOptions eventHubProducerClientOptions = SetEventHubProducerClientOptions(eventHubProducerConfiguration);
            var eventHubProducerClient = new EventHubProducerClient<TEvent>
            (
                configuration[eventHubProducerConfiguration.ConnectionStringKey],
                eventHubProducerClientOptions
            );
            services.AddSingleton(eventHubProducerClient);

            services.AddScoped(typeof(IMessagePublisher<TEvent>), typeof(EventHubCloudEventPublisher<TEvent>));

            return services;
        }

        /// <summary>
        /// Set Event Processor Client Options
        /// </summary>
        /// <param name="eventProcessorConfiguration"></param>
        /// <returns></returns>
        private static EventProcessorClientOptions SetEventProcessorClientOptions(EventProcessorConfiguration eventProcessorConfiguration)
        {
            EventHubsRetryMode eventHubsRetryMode = EventHubsRetryMode.Exponential;

            if (!string.IsNullOrEmpty(eventProcessorConfiguration.EventHubsRetryMode) && eventProcessorConfiguration.EventHubsRetryMode.ToUpper() == "FIXED")
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

        /// <summary>
        /// Set Event Hub Producer Client Options
        /// </summary>
        /// <param name="eventHubProducerConfiguration"></param>
        /// <returns></returns>
        private static EventHubProducerClientOptions SetEventHubProducerClientOptions(EventHubProducerConfiguration eventHubProducerConfiguration)
        {
            EventHubsRetryMode eventHubsRetryMode = EventHubsRetryMode.Exponential;

            if (!string.IsNullOrEmpty(eventHubProducerConfiguration.EventHubsRetryMode) && eventHubProducerConfiguration.EventHubsRetryMode.ToUpper() == "FIXED")
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
    }
}


