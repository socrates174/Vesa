using AutoMapper;
using IoCloud.Shared.Domain;
using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IoCloud.Shared.ChangeFeed.Cosmos.Infrastructure
{
    /// <summary>
    /// Updates the current state materialized view when an event is added to a domain events container
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TEventPayload"></typeparam>
    public abstract class SetCurrentStateEventHandler<TEntity, TEvent, TEventPayload> : SetCurrentStateEventHandler<TEntity, Guid, TEvent, TEventPayload>
        where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey
        where TEvent : CloudEventMessage
        where TEventPayload : CloudEventMessagePayload, IDomainMessage, IEvent
    {
        public SetCurrentStateEventHandler
        (
            IMapper mapper,
            ICloudEventStore<TEvent> cloudEventStore,
            INoSqlCommandRepository<TEntity> noSqlCommandRepository,
            ILogger<SetCurrentStateEventHandler<TEntity, TEvent, TEventPayload>> logger
        )
            : base(mapper, cloudEventStore, noSqlCommandRepository, logger)
        {
        }
    }

    /// <summary>
    /// Updates the current state materialized view when an event is added to a domain events container
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TEventPayload"></typeparam>
    public abstract class SetCurrentStateEventHandler<TEntity, TKey, TEvent, TEventPayload> : IEventHandler<TEventPayload>, IDisposable
    where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
    where TEvent : CloudEventMessage
    where TEventPayload : CloudEventMessagePayload, IDomainMessage, IEvent
    {
        private bool disposedValue;
        private readonly IMapper _mapper;
        private readonly ICloudEventStore<TEvent> _cloudEventStore;
        private readonly INoSqlCommandRepository<TEntity, TKey> _noSqlCommandRepository;
        private readonly ILogger<SetCurrentStateEventHandler<TEntity, TKey, TEvent, TEventPayload>> _logger;

        public SetCurrentStateEventHandler
        (
            IMapper mapper,
            ICloudEventStore<TEvent> cloudEventStore,
            INoSqlCommandRepository<TEntity, TKey> noSqlCommandRepository,
            ILogger<SetCurrentStateEventHandler<TEntity, TKey, TEvent, TEventPayload>> logger
        )
        {
            _mapper = mapper;
            _cloudEventStore = cloudEventStore;
            _noSqlCommandRepository = noSqlCommandRepository;
            _logger = logger;
        }

        public async Task Handle(TEventPayload eventPayload, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _cloudEventStore.ReplayStreamAsync<TEntity>(eventPayload.Header.Subject);
                await _noSqlCommandRepository.AddOrUpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}: Message: {JsonConvert.SerializeObject(eventPayload)}");
                throw;
            }
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
        // ~SetCurrentStateEventHandler()
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
