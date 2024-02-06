using AutoMapper;
using IoCloud.Shared.ChangeFeed.Abstractions;
using IoCloud.Shared.ChangeFeed.Infrastructure;
using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.MessageHandling.Infrastructure;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.EventHub.Extensions;
using IoCloud.Shared.Persistence.Cosmos.Extensions;
using IoCloud.Shared.Persistence.NoSql.Infrastructure;
using IoCloud.Shared.Settings.Infrastructure;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Dynamic;

namespace IoCloud.Shared.ChangeFeed.Cosmos.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChangeFeedEntityMovement<TEntity, TAuditEntity, TAuditKey, TInboxMessage, TOutboxMessage>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            CosmosClient cosmosClient,
            string changeFeedProcessorConfigurationSection = "ChangeFeedProcessorConfiguration",
            string entityMovementConfigurationSection = "EntityMovementConfiguration"
        )
            where TEntity : class
            where TAuditEntity : class, IEntity<TAuditKey>, ISoftDelete, IAuditable, IOptimisticConcurrency, IPartitionKey, new()
            where TInboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
            where TOutboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
        {
            services.Configure<EntityMovementConfiguration>(entityMovementConfigurationSection, configuration.GetSection(entityMovementConfigurationSection));
            var entityMovementConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EntityMovementConfiguration>>().Get(entityMovementConfigurationSection);

            var leaseContainerConfiguration = new CosmosContainerConfiguration
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.LeaseContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(leaseContainerConfiguration);

            var sourceContainerConfiguration = new CosmosContainerConfiguration<TEntity>
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.EntityContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(sourceContainerConfiguration);

            var auditContainerConfiguration = new CosmosContainerConfiguration<TAuditEntity>
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.AuditContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(auditContainerConfiguration);

            var inboxMessageContainerConfiguration = new CosmosContainerConfiguration<TInboxMessage>
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.InboxMessageContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(inboxMessageContainerConfiguration);

            var outboxMessageContainerConfiguration = new CosmosContainerConfiguration<TOutboxMessage>
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.OutboxMessageContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(outboxMessageContainerConfiguration);

            // Configure handlers

            return services.AddChangeFeedEntityMovement<TEntity, TAuditEntity, TAuditKey, TInboxMessage, TOutboxMessage>
            (
                configuration,
                changeFeedProcessorConfigurationSection,
                sourceContainerConfiguration,
                auditContainerConfiguration,
                inboxMessageContainerConfiguration,
                outboxMessageContainerConfiguration,
                cosmosClient
            );
        }

        public static IServiceCollection AddChangeFeedEntityMovement<TEntity, TInboxMessage, TOutboxMessage>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            CosmosClient cosmosClient,
            string changeFeedProcessorConfigurationSection = "ChangeFeedProcessorConfiguration",
            string entityMovementConfigurationSection = "EntityMovementConfiguration"
        )
            where TEntity : class
            where TInboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
            where TOutboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
        {
            services.Configure<EntityMovementConfiguration>(entityMovementConfigurationSection, configuration.GetSection(entityMovementConfigurationSection));
            var entityMovementConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EntityMovementConfiguration>>().Get(entityMovementConfigurationSection);

            var leaseContainerConfiguration = new CosmosContainerConfiguration
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.LeaseContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(leaseContainerConfiguration);

            var sourceContainerConfiguration = new CosmosContainerConfiguration<TEntity>
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.EntityContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(sourceContainerConfiguration);

            var inboxMessageContainerConfiguration = new CosmosContainerConfiguration<TInboxMessage>
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.InboxMessageContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(inboxMessageContainerConfiguration);

            var outboxMessageContainerConfiguration = new CosmosContainerConfiguration<TOutboxMessage>
            (
                entityMovementConfiguration.DatabaseName,
                entityMovementConfiguration.OutboxMessageContainerName,
                entityMovementConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(outboxMessageContainerConfiguration);

            // Configure handlers

            return services.AddChangeFeedEntityMovement<TEntity, TInboxMessage, TOutboxMessage>
            (
                configuration,
                changeFeedProcessorConfigurationSection,
                sourceContainerConfiguration,
                inboxMessageContainerConfiguration,
                outboxMessageContainerConfiguration,
                cosmosClient
            );
        }

        public static IServiceCollection AddChangeFeedEntityMovement<TEntity, TAuditEntity, TAuditKey, TInboxMessage, TOutboxMessage>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string changeFeedProcessorConfigurationSection,
            CosmosContainerConfiguration<TEntity> sourceContainerConfiguration,
            CosmosContainerConfiguration<TAuditEntity> auditContainerConfiguration,
            CosmosContainerConfiguration<TInboxMessage> inboxMessageContainerConfiguration,
            CosmosContainerConfiguration<TOutboxMessage> outboxMessageContainerConfiguration,
            CosmosClient cosmosClient
        )
            where TEntity : class
            where TAuditEntity : class, IEntity<TAuditKey>, ISoftDelete, IAuditable, IOptimisticConcurrency, IPartitionKey, new()
            where TInboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
            where TOutboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
        {

            // Configure handlers

            services.AddMoveEntityCommandHandler<TAuditEntity, TAuditKey, TEntity>
            (
                configuration,
                sourceContainerConfiguration,
                auditContainerConfiguration,
                cosmosClient
            );

            services.AddMoveEntityCommandHandler<TInboxMessage, string, TEntity>
            (
                configuration,
                sourceContainerConfiguration,
                inboxMessageContainerConfiguration,
                cosmosClient
            );

            services.AddMoveEntityCommandHandler<TOutboxMessage, string, TEntity>
            (
                configuration,
                sourceContainerConfiguration,
                outboxMessageContainerConfiguration,
                cosmosClient
            );

            // Configure change feed processor factory

            services.AddChangeFeedProcessorFactoryWithRouteCommandHandler<TEntity>
            (
                configuration,
                cosmosClient,
                changeFeedProcessorConfigurationSection,
                typeof(TAuditEntity),
                typeof(TInboxMessage),
                typeof(TOutboxMessage)
            );

            return services;
        }

        public static IServiceCollection AddChangeFeedEntityMovement<TEntity, TInboxMessage, TOutboxMessage>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string changeFeedProcessorConfigurationSection,
            CosmosContainerConfiguration<TEntity> sourceContainerConfiguration,
            CosmosContainerConfiguration<TInboxMessage> inboxMessageContainerConfiguration,
            CosmosContainerConfiguration<TOutboxMessage> outboxMessageContainerConfiguration,
            CosmosClient cosmosClient
        )
            where TEntity : class
            where TInboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
            where TOutboxMessage : class, IEntity<string>, IOptimisticConcurrency, IPartitionKey
        {

            // Configure handlers

            services.AddMoveEntityCommandHandler<TInboxMessage, string, TEntity>
            (
                configuration,
                sourceContainerConfiguration,
                inboxMessageContainerConfiguration,
                cosmosClient
            );

            services.AddMoveEntityCommandHandler<TOutboxMessage, string, TEntity>
            (
                configuration,
                sourceContainerConfiguration,
                outboxMessageContainerConfiguration,
                cosmosClient
            );

            // Configure change feed processor factory

            services.AddChangeFeedProcessorFactoryWithRouteCommandHandler<TEntity>
            (
                configuration,
                cosmosClient,
                changeFeedProcessorConfigurationSection,
                typeof(TInboxMessage),
                typeof(TOutboxMessage)
            );

            return services;
        }

        public static IServiceCollection AddMoveEntityCommandHandler<TMoveEntity, TMoveKey, TRootEntity>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            CosmosContainerConfiguration<TRootEntity> sourceCosmosContainerConfiguration,
            CosmosContainerConfiguration<TMoveEntity> targetCosmosContainerConfiguration,
            CosmosClient cosmosClient
        )
            where TMoveEntity : class, IEntity<TMoveKey>, IOptimisticConcurrency, IPartitionKey
            where TRootEntity : class
        {
            var serviceProvider = services.BuildServiceProvider();

            // Configure Cosmos Source

            var sourceLogger = serviceProvider.GetRequiredService<ILogger<CosmosCommandRepository<TMoveEntity, TMoveKey, TRootEntity>>>();
            var sourceCommandRepository = new CosmosCommandRepository<TMoveEntity, TMoveKey, TRootEntity>
            (
                sourceCosmosContainerConfiguration,
                cosmosClient,
                sourceLogger
            );


            // Configure Cosmos Target

            var targetLogger = serviceProvider.GetRequiredService<ILogger<CosmosCommandRepository<TMoveEntity, TMoveKey>>>();
            var targetCommandRepository = new CosmosCommandRepository<TMoveEntity, TMoveKey>
            (
                targetCosmosContainerConfiguration,
                cosmosClient,
                targetLogger
            );

            var moveLogger = serviceProvider.GetRequiredService<ILogger<MoveEntityCommandHandler<TMoveEntity, TMoveKey, TRootEntity>>>();
            var moveEntityCommandHandler = new MoveEntityCommandHandler<TMoveEntity, TMoveKey, TRootEntity>
            (
                sourceCommandRepository,
                targetCommandRepository,
                services.BuildServiceProvider().GetRequiredService<IMapper>(),
                moveLogger
            );

            // temporary fix until figure out why ICommandHandler deose not work
            services.AddSingleton<IRequestHandler<ChangeFeedCommand<TMoveEntity, TRootEntity>, VoidReply>>(moveEntityCommandHandler);
            //services.AddSingleton<ICommandHandler<ChangeFeedCommand<TEntity>, VoidReply>>(moveEntityCommandHandler);

            return services;
        }

        public static IServiceCollection AddChangeFeedProcessorFactoryWithRouteCommandHandler<TRootEntity>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            CosmosClient sourceCosmosClient,
            string changeFeedProcessorConfigurationSection = "ChangeFeedProcessorConfiguration",
            params Type[] typesToProcess
        )
        {
            // Configure Change Feed

            services.Configure<ChangeFeedProcessorConfiguration>(changeFeedProcessorConfigurationSection, configuration.GetSection(changeFeedProcessorConfigurationSection));
            var changeFeedProcessorConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<ChangeFeedProcessorConfiguration>>().Get(changeFeedProcessorConfigurationSection);

            services.AddScoped(typeof(IChangeFeedHandler<ExpandoObject, TRootEntity>), typeof(RouteCommandChangeFeedHandler<TRootEntity>));
            var routeCommandChangeFeedHandler = services.BuildServiceProvider().GetRequiredService<IChangeFeedHandler<ExpandoObject, TRootEntity>>();

            var changeFeedProcessorFactoryLogger = services.BuildServiceProvider().GetRequiredService<ILogger<ChangeFeedProcessorFactory<ExpandoObject, TRootEntity>>>();
            var changeFeedProcessorFactory = new ChangeFeedProcessorFactory<ExpandoObject, TRootEntity>
            (
                changeFeedProcessorConfiguration,
                sourceCosmosClient,
                routeCommandChangeFeedHandler,
                changeFeedProcessorFactoryLogger,
                typesToProcess
            );

            services.AddSingleton<IChangeFeedProcessorFactory<ExpandoObject, TRootEntity>>(changeFeedProcessorFactory);

            return services;
        }

        public static IServiceCollection AddChangeFeedEventPublication<TEvent>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string eventPublicationConfigurationSection = "EventPublicationConfiguration"
        )
            where TEvent : CloudEventMessage
        {
            // Configure client
            services.Configure<EventPublicationConfiguration>(eventPublicationConfigurationSection, configuration.GetSection(eventPublicationConfigurationSection));
            var eventPublicationConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EventPublicationConfiguration>>().Get(eventPublicationConfigurationSection);

            var cosmosClient = services.GetCosmosClient(configuration);

            var leaseContainerConfiguration = new CosmosContainerConfiguration
            (
                eventPublicationConfiguration.DatabaseName,
                eventPublicationConfiguration.LeaseContainerName,
                eventPublicationConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(leaseContainerConfiguration);


            var outboxMessageContainerConfiguration = new CosmosContainerConfiguration
            (
                eventPublicationConfiguration.DatabaseName,
                eventPublicationConfiguration.OutboxMessageContainerName,
                eventPublicationConfiguration.PartitionKeyPath
            );
            cosmosClient.Configure(outboxMessageContainerConfiguration);


            // Configure handlers

            services.AddEventHubPublication<TEvent>(configuration);


            // Configure change feed processor factory

            services.AddChangeFeedProcessorFactoryWithPublishEventHandler<TEvent>
            (
                configuration,
                cosmosClient
            );

            return services;
        }

        public static IServiceCollection AddChangeFeedProcessorFactoryWithPublishEventHandler<TEvent>
        (
            this IServiceCollection services,
            IConfiguration configuration,
            CosmosClient cosmosClient
        )
            where TEvent : CloudEventMessage
        {
            // Configure Change Feed

            services.Configure<ChangeFeedProcessorConfiguration>(configuration.GetSection(nameof(ChangeFeedProcessorConfiguration)));
            var changeFeedProcessorConfiguration = services.BuildServiceProvider().GetRequiredService<IOptions<ChangeFeedProcessorConfiguration>>().Value;
            //services.AddSingleton<IChangeFeedProcessorConfiguration>(changeFeedProcessorConfiguration);

            services.AddScoped<IChangeFeedHandler<TEvent>, PublishCloudEventChangeFeedHandler<TEvent>>();
            var publishEventChangeFeedHandler = services.BuildServiceProvider().GetRequiredService<IChangeFeedHandler<TEvent>>();

            var changeFeedProcessorFactoryLogger = services.BuildServiceProvider().GetRequiredService<ILogger<ChangeFeedProcessorFactory<TEvent>>>();
            var changeFeedProcessorFactory = new ChangeFeedProcessorFactory<TEvent>
            (
                changeFeedProcessorConfiguration,
                cosmosClient,
                publishEventChangeFeedHandler,
                changeFeedProcessorFactoryLogger,
                typeof(TEvent)
            );

            services.AddSingleton<IChangeFeedProcessorFactory<TEvent>>(changeFeedProcessorFactory);

            return services;
        }
    }
}