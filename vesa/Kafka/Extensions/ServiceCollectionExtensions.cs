using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;
using vesa.Kafka.Abstractions;
using vesa.Kafka.Infrastructure;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace vesa.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventMappings
    (
        this IServiceCollection services,
        IConfiguration configuration,
        string eventMappingsName = "EventMappings"
    )
    {
        var eventMappings = configuration.GetSection(eventMappingsName).Get<List<EventMapping>>();
        services.AddSingleton<IEnumerable<IEventMapping>>(eventMappings);
        return services;
    }

    public static IServiceCollection AddKafkaEventConsumption
    (
        this IServiceCollection services,
        IConfiguration configuration,
        string kafkaConsumerConfigurationSectionName = "KafkaConsumerConfiguration"
    )
    {
        var kafkaConsumerConfiguration = configuration.GetSection(kafkaConsumerConfigurationSectionName).Get<KafkaConsumerConfiguration>();

        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaConsumerConfiguration.BootstrapServers,
            GroupId = kafkaConsumerConfiguration.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SaslPassword = configuration[kafkaConsumerConfiguration.SaslPassword],
            SaslUsername = configuration[kafkaConsumerConfiguration.SaslUsername],
            SaslMechanism = SaslMechanism.ScramSha256,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(kafkaConsumerConfiguration.Topic);

        services.AddSingleton<IKafkaConsumerConfiguration>(kafkaConsumerConfiguration);
        services.AddSingleton(consumer);

        services.AddSingleton<IEventListener, KafkaEventListener>();
        services.AddSingleton<IKafkaEventConsumer, KafkaEventConsumer>();
        services.AddSingleton<KafkaEventListener>();
        services.AddSingleton<IEventProcessor, EventProcessor>();

        return services;
    }


    public static IServiceCollection AddKafkaEventPublication
    (
        this IServiceCollection services,
        IConfiguration configuration,
        string kafkaPublisherConfigurationSectionName = "KafkaPublisherConfiguration"
    )
    {
        var kafkaPublisherConfiguration = configuration.GetSection(kafkaPublisherConfigurationSectionName).Get<KafkaPublisherConfiguration>();

        var config = new ProducerConfig
        {
            BootstrapServers = kafkaPublisherConfiguration.BootstrapServers,
            SaslPassword = configuration[kafkaPublisherConfiguration.SaslPassword],
            SaslUsername = configuration[kafkaPublisherConfiguration.SaslUsername],
            SaslMechanism = SaslMechanism.ScramSha256,
            SecurityProtocol = SecurityProtocol.SaslSsl

        };

        var producer = new ProducerBuilder<Ignore, string>(config)
            .SetValueSerializer(Serializers.Utf8)
            .SetKeySerializer(new IgnoreSerializer())
            .Build();

        services.AddSingleton<IKafkaPublisherConfiguration>(kafkaPublisherConfiguration);
        //services.AddSingleton(kafkaPublisherConfiguration);
        services.AddSingleton(producer);
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
 
        return services;
    }
}
