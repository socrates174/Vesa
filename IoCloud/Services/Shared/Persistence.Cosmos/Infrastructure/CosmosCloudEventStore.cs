using AutoMapper;
using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.MessageHandling.Infrastructure;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using IoCloud.Shared.Settings.Abstractions;
using IoCloud.Shared.Utility;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Text.Json;

namespace IoCloud.Shared.Persistence.Cosmos.Infrastructure
{
    public class CosmosCloudEventStore<TEvent> : ICloudEventStore<TEvent>
       where TEvent : CloudEventMessage, IPartitionKey
    {
        private readonly ICosmosContainerConfiguration<TEvent> _configuration;
        private readonly CosmosClient _client;
        private readonly IMapper _mapper;
        private readonly ILogger<CosmosCloudEventStore<TEvent>> _logger;
        private TransactionalBatch _batch;
        private bool disposedValue;

        public CosmosCloudEventStore
        (
            ICosmosContainerConfiguration<TEvent> configuration,
            CosmosClient client,
            IMapper mapper,
            ILogger<CosmosCloudEventStore<TEvent>> logger
        )
        {
            _configuration = configuration;
            _client = client;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddAsync(TEvent theEvent, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(theEvent.Subject))
            {
                throw new ArgumentException("Event Subject cannot be empty");
            }

            try
            {
                var container = GetContainer();
                theEvent.Sequence = GetNextSequence(theEvent, container);
                if (requestedBy != null)
                {
                    theEvent.RequestedBy = requestedBy;
                }

                var partitionKeyValue = theEvent.Subject;
                var partitionKey = new PartitionKey(partitionKeyValue);
                theEvent.PartitionKey = partitionKeyValue;
                var itemResponse = await container.CreateItemAsync(theEvent, partitionKey, null, cancellationToken);
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"New event with ID: {theEvent.Id} was not added successfully - error details: {ex.Message}");
                throw;
            }

        }

        public async Task AddAsync(IEnumerable<TEvent> events, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (events == null || !events.Any())
            {
                throw new ArgumentException("Missing outboxEvents");
            }

            try
            {
                var container = GetContainer();
                var partitionKeyValue = events.First().Subject;
                var partitionKey = new PartitionKey(partitionKeyValue);
                _batch = container.CreateTransactionalBatch(partitionKey);

                var nextSequence = GetNextSequence(events.First(), container);

                foreach (var outboxEvent in events)
                {
                    outboxEvent.Sequence = (nextSequence++);
                    if (requestedBy != null)
                    {
                        outboxEvent.RequestedBy = requestedBy;
                        outboxEvent.PartitionKey = partitionKeyValue;
                    }
                    _batch.CreateItem(outboxEvent);
                }

                using (TransactionalBatchResponse batchResponse = await _batch.ExecuteAsync(cancellationToken))
                {
                    if (!batchResponse.IsSuccessStatusCode)
                    {
                        throw new Exception("TransactionalBatch execution failed");
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Events with IDs: {String.Join(',', events.Select(e => e.Id))} was not added successfully - error details: {ex.Message}");
                throw;
            }

        }

        public async Task<TEntity> ReplayAsync<TEntity>(Guid id, DateTimeOffset toDate) => await ReplayAsync<TEntity>(id, anEvent => anEvent.Time <= toDate);

        public async Task<TEntity> ReplayAsync<TEntity>(Guid id, Expression<Func<TEvent, bool>> filter = null) => await ReplayAsync<TEntity, Guid>(id, filter);

        public async Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, DateTimeOffset toDate) => await ReplayAsync<TEntity, TKey>(id, anEvent => anEvent.Time <= toDate);

        public async Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, Expression<Func<TEvent, bool>> filter = null)
            => await ReplayStreamAsync<TEntity>(GetEventStreamKey<TEntity, TKey>(id), filter);

        public async Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, DateTimeOffset toDate) => await ReplayStreamAsync<TEntity>(eventStreamKey, anEvent => anEvent.Time <= toDate);

        public async Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, Expression<Func<TEvent, bool>> filter = null)
        {
            var storedEvents = new List<TEvent>();
            var currentState = default(TEntity);
            var container = GetContainer();

            if (filter == null)
            {
                storedEvents = await container.GetItemLinqQueryable<TEvent>().Where(e => e.Subject == eventStreamKey).OrderBy(e => e.Sequence).ToListAsync();
            }
            else
            {
                storedEvents = await container.GetItemLinqQueryable<TEvent>().Where(e => e.Subject == eventStreamKey).Where(filter).OrderBy(e => e.Sequence).ToListAsync();
            }

            if (storedEvents.Any())
            {
                Type entityType = typeof(TEntity);

                foreach (var storeEvent in storedEvents)
                {
                    var eventJson = System.Text.Json.JsonSerializer.Serialize(storeEvent.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var domainEvent = JsonConvert.DeserializeObject(eventJson, (eventJson as dynamic).PayloadType);
                    currentState = currentState == null ? _mapper.Map<TEntity>(domainEvent) : _mapper.Map(domainEvent, currentState);
                }
            }

            return currentState;
        }

        /// <summary>
        /// Use this method if you have different entity versions
        /// </summary>
        /// <typeparam name="TEntityV1"></typeparam>
        /// <param name="eventStreamKey"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, DateTimeOffset toDate)
            => await ReplayVersionedStreamAsync<TEntityV1>(eventStreamKey, anEvent => anEvent.Time <= toDate);

        /// <summary>
        /// Use this method if you have different entity versions
        /// </summary>
        /// <typeparam name="TEntityV1"></typeparam>
        /// <param name="eventStreamKey"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, Expression<Func<TEvent, bool>> filter = null)
        {
            var storedEvents = new List<TEvent>();
            object currentState = null;
            var outboxContainer = GetContainer();
            if (filter == null)
            {
                storedEvents = await outboxContainer.GetItemLinqQueryable<TEvent>().Where(e => e.Subject == eventStreamKey).OrderBy(e => e.Sequence).ToListAsync();
            }
            else
            {
                storedEvents = await outboxContainer.GetItemLinqQueryable<TEvent>().Where(e => e.Subject == eventStreamKey).Where(filter).OrderBy(e => e.Sequence).ToListAsync();
            }
            if (storedEvents.Any())
            {
                Type currentEntityVersionType = typeof(TEntityV1);

                foreach (var storeEvent in storedEvents)
                {
                    var eventJson = System.Text.Json.JsonSerializer.Serialize(storeEvent.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var domainEvent = JsonConvert.DeserializeObject(eventJson, (eventJson as dynamic).PayloadType);
                    var domainEventType = domainEvent.GetType();

                    // if the domainEvent is an EntitySchemaChangeEvent, then first validate the previous schema matches EntitySchemaChangeEvent.OldVersionType
                    if (domainEventType.Name == "EntitySchemaChangeEvent")
                    {
                        var entitySchemaChangeEvent = (domainEvent as EntitySchemaChangeEvent);
                        if (currentEntityVersionType.FullName != entitySchemaChangeEvent.OldVersionTypeName)
                        {
                            throw new InvalidDataException("Replay entity version does not match incoming EntitySchemaChangeEvent.OldVersionTypeName");
                        }
                        else
                        {
                            // Map old version to new version
                            var newEntityVersionType = TypeUtils.GetType(entitySchemaChangeEvent.NewVersionTypeName);
                            currentState = _mapper.Map(currentState, currentEntityVersionType, newEntityVersionType);
                            currentEntityVersionType = newEntityVersionType;
                        }
                    }
                    else
                    {
                        currentState = currentState == null ?
                                        _mapper.Map(domainEvent, domainEventType, currentEntityVersionType) :
                                        _mapper.Map(domainEvent, currentState, domainEventType, currentEntityVersionType);
                    }
                }
            }
            return currentState;
        }

        private int GetNextSequence(TEvent theEvent, Container container)
        {
            var query = container.GetItemLinqQueryable<TEvent>(
                 requestOptions: new QueryRequestOptions
                 {
                     MaxItemCount = 1
                 })
              .Where(e => e.Subject == theEvent.Subject)
              .OrderByDescending(e => e.Sequence);

            var queryIterator = container.GetItemQueryIterator<TEvent>(query.ToQueryDefinition());
            var lastOutboxEvent = queryIterator.ReadNextAsync().Result.Resource.FirstOrDefault();

            return (lastOutboxEvent != null ? lastOutboxEvent.Sequence + 1 : 1);
        }

        private string GetEventStreamKey<TEntity, TKey>(TKey entityId) => $"{typeof(TEntity).Name}:{entityId}";

        private Container GetContainer()
        {
            var database = _client.GetDatabase(_configuration.DatabaseName);
            var container = database.GetContainer(_configuration.ContainerName);
            return container;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CosmosEventStore()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class CosmosCloudEventStore<TInboxMessage, TOutboxEvent> : ICloudEventStore<TInboxMessage, TOutboxEvent>
        where TInboxMessage : CloudEventMessage, IPartitionKey
        where TOutboxEvent : CloudEventMessage, IPartitionKey
    {
        private readonly ICosmosContainerConfiguration<TInboxMessage> _inboxContainerConfiguration;
        private readonly ICosmosContainerConfiguration<TOutboxEvent> _outboxContainerConfiguration;
        private readonly CosmosClient _client;
        private readonly IMapper _mapper;
        private readonly ILogger<CosmosCloudEventStore<TInboxMessage, TOutboxEvent>> _logger;
        private TransactionalBatch _batch;
        private bool disposedValue;

        public CosmosCloudEventStore
        (
            ICosmosContainerConfiguration<TInboxMessage> inboxContainerConfiguration,
            ICosmosContainerConfiguration<TOutboxEvent> outboxContainerConfiguration,
            CosmosClient client,
            IMapper mapper,
            ILogger<CosmosCloudEventStore<TInboxMessage, TOutboxEvent>> logger
        )
        {
            _inboxContainerConfiguration = inboxContainerConfiguration;
            _outboxContainerConfiguration = outboxContainerConfiguration;
            _client = client;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddAsync
        (
            TInboxMessage inboxMessage,
            TOutboxEvent outboxEvent,
            string requestedBy = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            if (inboxMessage == null)
            {
                throw new ArgumentException("Missing inboxEvent");
            }
            if (outboxEvent == null)
            {
                throw new ArgumentException("Missing outboxEvent");
            }

            var inboxContainer = GetInboxContainer();

            var partitionKeyValue = outboxEvent.Subject;
            var partitionKey = new PartitionKey(partitionKeyValue);
            _batch = inboxContainer.CreateTransactionalBatch(partitionKey);
            inboxMessage.PartitionKey = partitionKeyValue;
            _batch.CreateItem(inboxMessage);

            var nextSequence = GetNextSequence(outboxEvent, inboxContainer);

            outboxEvent.Sequence = (nextSequence++);
            outboxEvent.RequestedBy = requestedBy ?? inboxMessage?.RequestedBy;
            outboxEvent.PartitionKey = partitionKeyValue;
            _batch.CreateItem(outboxEvent);

            using (TransactionalBatchResponse batchResponse = await _batch.ExecuteAsync(cancellationToken))
            {
                if (!batchResponse.IsSuccessStatusCode)
                {
                    throw new Exception("TransactionalBatch execution failed");
                }
            }
        }
        public async Task AddAsync
        (
            TInboxMessage inboxMessage,
            IEnumerable<TOutboxEvent> outboxEvents,
            string requestedBy = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            if (inboxMessage == null)
            {
                throw new ArgumentException("Missing inboxEvent");
            }
            if (outboxEvents == null || !outboxEvents.Any())
            {
                throw new ArgumentException("Missing outboxEvents");
            }

            var inboxContainer = GetInboxContainer();

            var partitionKeyValue = outboxEvents.First().Subject;
            var partitionKey = new PartitionKey(partitionKeyValue);
            _batch = inboxContainer.CreateTransactionalBatch(partitionKey);
            inboxMessage.PartitionKey = partitionKeyValue;
            _batch.CreateItem(inboxMessage);

            var nextSequence = GetNextSequence(outboxEvents.First(), inboxContainer);

            foreach (var outboxEvent in outboxEvents)
            {
                outboxEvent.Sequence = (nextSequence++);
                outboxEvent.RequestedBy = requestedBy ?? inboxMessage?.RequestedBy;
                outboxEvent.PartitionKey = partitionKeyValue;
                _batch.CreateItem(outboxEvent);
            }

            using (TransactionalBatchResponse batchResponse = await _batch.ExecuteAsync(cancellationToken))
            {
                if (!batchResponse.IsSuccessStatusCode)
                {
                    throw new Exception("TransactionalBatch execution failed");
                }
            }
        }

        public async Task<TEntity> ReplayAsync<TEntity>(Guid id, DateTimeOffset toDate) => await ReplayAsync<TEntity>(id, anEvent => anEvent.Time <= toDate);

        public async Task<TEntity> ReplayAsync<TEntity>(Guid id, Expression<Func<TOutboxEvent, bool>> filter = null) => await ReplayAsync<TEntity, Guid>(id, filter);

        public async Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, DateTimeOffset toDate) => await ReplayAsync<TEntity, TKey>(id, anEvent => anEvent.Time <= toDate);

        public async Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, Expression<Func<TOutboxEvent, bool>> filter = null)
            => await ReplayStreamAsync<TEntity>(GetEventStreamKey<TEntity, TKey>(id), filter);

        public async Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, DateTimeOffset toDate) => await ReplayStreamAsync<TEntity>(eventStreamKey, anEvent => anEvent.Time <= toDate);

        public async Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, Expression<Func<TOutboxEvent, bool>> filter = null)
        {
            var storedEvents = new List<TOutboxEvent>();
            var currentState = default(TEntity);
            var outboxContainer = GetOutboxContainer();

            if (filter == null)
            {
                storedEvents = await outboxContainer.GetItemLinqQueryable<TOutboxEvent>().Where(e => e.Subject == eventStreamKey).OrderBy(e => e.Sequence).ToListAsync();
            }
            else
            {
                storedEvents = await outboxContainer.GetItemLinqQueryable<TOutboxEvent>().Where(e => e.Subject == eventStreamKey).Where(filter).OrderBy(e => e.Sequence).ToListAsync();
            }

            if (storedEvents.Any())
            {
                Type entityType = typeof(TEntity);

                foreach (var storeEvent in storedEvents)
                {
                    var eventJson = System.Text.Json.JsonSerializer.Serialize(storeEvent.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var domainEvent = JsonConvert.DeserializeObject(eventJson, (eventJson as dynamic).PayloadType);
                    currentState = currentState == null ? _mapper.Map<TEntity>(domainEvent) : _mapper.Map(domainEvent, currentState);
                }
            }

            return currentState;
        }

        /// <summary>
        /// Use this method if you have different entity versions
        /// </summary>
        /// <typeparam name="TEntityV1"></typeparam>
        /// <param name="eventStreamKey"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, DateTimeOffset toDate)
            => await ReplayVersionedStreamAsync<TEntityV1>(eventStreamKey, anEvent => anEvent.Time <= toDate);

        /// <summary>
        /// Use this method if you have different entity versions
        /// </summary>
        /// <typeparam name="TEntityV1"></typeparam>
        /// <param name="eventStreamKey"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, Expression<Func<TOutboxEvent, bool>> filter = null)
        {
            var storedEvents = new List<TOutboxEvent>();
            object currentState = null;
            var outboxContainer = GetOutboxContainer();
            if (filter == null)
            {
                storedEvents = await outboxContainer.GetItemLinqQueryable<TOutboxEvent>().Where(e => e.Subject == eventStreamKey).OrderBy(e => e.Sequence).ToListAsync();
            }
            else
            {
                storedEvents = await outboxContainer.GetItemLinqQueryable<TOutboxEvent>().Where(e => e.Subject == eventStreamKey).Where(filter).OrderBy(e => e.Sequence).ToListAsync();
            }
            if (storedEvents.Any())
            {
                Type currentEntityVersionType = typeof(TEntityV1);

                foreach (var storeEvent in storedEvents)
                {
                    var eventJson = System.Text.Json.JsonSerializer.Serialize(storeEvent.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var domainEvent = JsonConvert.DeserializeObject(eventJson, (eventJson as dynamic).PayloadType);
                    var domainEventType = domainEvent.GetType();

                    // if the domainEvent is an EntitySchemaChangeEvent, then first validate the previous schema matches EntitySchemaChangeEvent.OldVersionType
                    if (domainEventType.Name == "EntitySchemaChangeEvent")
                    {
                        var entitySchemaChangeEvent = (domainEvent as EntitySchemaChangeEvent);
                        if (currentEntityVersionType.FullName != entitySchemaChangeEvent.OldVersionTypeName)
                        {
                            throw new InvalidDataException("Replay entity version does not match incoming EntitySchemaChangeEvent.OldVersionTypeName");
                        }
                        else
                        {
                            // Map old version to new version
                            var newEntityVersionType = TypeUtils.GetType(entitySchemaChangeEvent.NewVersionTypeName);
                            currentState = _mapper.Map(currentState, currentEntityVersionType, newEntityVersionType);
                            currentEntityVersionType = newEntityVersionType;
                        }
                    }
                    else
                    {
                        currentState = currentState == null ?
                                        _mapper.Map(domainEvent, domainEventType, currentEntityVersionType) :
                                        _mapper.Map(domainEvent, currentState, domainEventType, currentEntityVersionType);
                    }
                }
            }
            return currentState;
        }

        /// <summary>
        /// Gets the Next Sequence of the Subject Event from the given Container.
        /// Returns 1 if it is the first record for that Subject
        /// </summary>
        /// <param name="theEvent"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        private int GetNextSequence(TOutboxEvent theEvent, Container container)
        {
            var query = container.GetItemLinqQueryable<TOutboxEvent>(
                 requestOptions: new QueryRequestOptions
                 {
                     MaxItemCount = 1
                 })
              .Where(e => e.Subject == theEvent.Subject)
              .OrderByDescending(e => e.Sequence);

            var queryIterator = container.GetItemQueryIterator<TOutboxEvent>(query.ToQueryDefinition());
            var lastOutboxEvent = queryIterator.ReadNextAsync().Result.Resource.FirstOrDefault();

            return (lastOutboxEvent != null ? lastOutboxEvent.Sequence + 1 : 1);
        }

        private string GetEventStreamKey<TEntity, TKey>(TKey entityId) => $"{typeof(TEntity).Name}:{entityId.ToString()}";

        private Container GetInboxContainer()
        {
            var database = _client.GetDatabase(_inboxContainerConfiguration.DatabaseName);
            var container = database.GetContainer(_inboxContainerConfiguration.ContainerName);
            return container;
        }

        private Container GetOutboxContainer()
        {
            var database = _client.GetDatabase(_outboxContainerConfiguration.DatabaseName);
            var container = database.GetContainer(_outboxContainerConfiguration.ContainerName);
            return container;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CosmosEventStore()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
