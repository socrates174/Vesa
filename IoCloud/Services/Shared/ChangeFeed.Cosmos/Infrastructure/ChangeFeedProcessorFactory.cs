using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using IoCloud.Shared.ChangeFeed.Abstractions;
using IoCloud.Shared.Settings.Abstractions;
using System.Dynamic;

namespace IoCloud.Shared.ChangeFeed.Infrastructure
{
    /// <summary>
    /// Detects all the changes in a Cosmos container and retrieves the changed items for custom processing
    /// </summary>
    /// <typeparam name="TChangedEntity"></typeparam>
    public class ChangeFeedProcessorFactory<TChangedEntity, TRootEntity> : ChangeFeedProcessorFactory<TChangedEntity>, IChangeFeedProcessorFactory<TChangedEntity, TRootEntity>
    {
        public ChangeFeedProcessorFactory
        (
            IChangeFeedProcessorConfiguration configuration,
            CosmosClient client,
            IChangeFeedHandler<TChangedEntity, TRootEntity> handler,
            ILogger<ChangeFeedProcessorFactory<TChangedEntity, TRootEntity>> logger,
            params Type[] typesToProcess
        )
            : base(configuration, client, handler, logger, typesToProcess)
        {
        }
    }

    /// <summary>
    /// Detects all the changes in a Cosmos container and retrieves the changed items for custom processing
    /// </summary>
    /// <typeparam name="TChangedEntity"></typeparam>
    public class ChangeFeedProcessorFactory<TChangedEntity> : IChangeFeedProcessorFactory<TChangedEntity>
    {
        protected readonly IChangeFeedProcessorConfiguration _configuration;
        protected readonly CosmosClient _client;
        protected readonly IChangeFeedHandler<TChangedEntity> _handler;
        protected readonly ILogger<ChangeFeedProcessorFactory<TChangedEntity>> _logger;
        protected readonly IList<string> _typeNamesToProcess;

        public ChangeFeedProcessorFactory
        (
            IChangeFeedProcessorConfiguration configuration,
            CosmosClient client,
            IChangeFeedHandler<TChangedEntity> handler,
            ILogger<ChangeFeedProcessorFactory<TChangedEntity>> logger,
            params Type[] typesToProcess
        )
        {
            _configuration = configuration;
            _client = client;
            _handler = handler;
            _logger = logger;
            _typeNamesToProcess = typesToProcess?.Select(t => t.FullName)?.ToList();

            Builder = GetSourceContainer()
                .GetChangeFeedProcessorBuilder<TChangedEntity>(processorName: _configuration.ProcessorName, onChangesDelegate: HandleChangesAsync)
                .WithInstanceName(Environment.MachineName)
                .WithLeaseContainer(GetLeaseContainer())
                .WithErrorNotification(onErrorAsync);

            if (configuration.StartDateTimeOffset != null)
            {
                Builder = Builder.WithStartTime(DateTime.SpecifyKind(Convert.ToDateTime(configuration.StartDateTimeOffset?.ToString()), DateTimeKind.Utc));
            }
        }

        public ChangeFeedProcessorBuilder Builder { get; set; }

        public ChangeFeedProcessor CreateProcessor() => Builder.Build();

        protected virtual async Task HandleChangesAsync
        (
            ChangeFeedProcessorContext context,
            IReadOnlyCollection<TChangedEntity> changes,
            CancellationToken cancellationToken
        )
        {
            if (_typeNamesToProcess != null && _typeNamesToProcess.Count > 0)
            {
                var itemType = typeof(TChangedEntity);
                if (itemType == typeof(ExpandoObject))
                {
                    foreach (var item in changes)
                    {
                        var changedItem = item as dynamic;
                        if (_typeNamesToProcess.Contains(changedItem.entityType))
                        {
                            await _handler.HandleChangeAsync(item, cancellationToken);
                        }
                    }
                }
                else
                {
                    foreach (var item in changes)
                    {
                        if (_typeNamesToProcess.Contains(item.GetType().FullName))
                        {
                            await _handler.HandleChangeAsync(item, cancellationToken);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in changes)
                {
                    await _handler.HandleChangeAsync(item, cancellationToken);
                }
            }
        }

        //TODO
        protected Container.ChangeFeedMonitorErrorDelegate onErrorAsync = (string leaseToken, Exception exception) =>
        {
            if (exception is ChangeFeedProcessorUserException userException)
            {
                Console.WriteLine($"Lease {leaseToken} processing failed with unhandled exception from user delegate {userException.InnerException}");
            }
            else
            {
                Console.WriteLine($"Lease {leaseToken} failed with {exception}");
            }

            return Task.CompletedTask;
        };

        protected Container GetSourceContainer()
        {
            var database = _client.GetDatabase(_configuration.SourceDatabaseName);
            var container = database.GetContainer(_configuration.SourceContainerName);
            return container;
        }

        protected Container GetLeaseContainer()
        {
            var database = _client.GetDatabase(_configuration.LeaseDatabaseName);
            var container = database.GetContainer(_configuration.LeaseContainerName);
            return container;
        }
    }
}
