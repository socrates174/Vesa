using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.EventHub.Infrastructure;
using IoCloud.Shared.Messaging.Publication.Abstractions;
using IoCloud.Shared.Persistence.Sql.Abstractions;
using IoCloud.Shared.Querying.Sql.Abstractions;
using Microsoft.Extensions.Logging;

namespace IoCloud.Shared.Messaging.Publication.Infrastructure
{
    public abstract class SqlOutboxCloudEventPublisher<TEvent> : EventHubCloudEventPublisher<TEvent>, IOutboxMessagePublisher<TEvent>
        where TEvent: CloudEventMessage
    {
        protected bool disposedValue;
        private readonly ISqlQueryRepository<TEvent> _queryRepository;
        private readonly ISqlCommandRepository<TEvent> _commandRepository;

        protected SqlOutboxCloudEventPublisher
        (
            ISqlQueryRepository<TEvent> queryRepository,
            ISqlCommandRepository<TEvent> commandRepository,
            EventHubProducerClient<TEvent> client,
            ILogger<EventHubCloudEventPublisher<TEvent>> logger
        )
            : base(client, logger)
        {
            _queryRepository = queryRepository;
            _commandRepository = commandRepository;
        }

        public virtual IQueryable<TEvent> GetMessages() => _queryRepository.GetAll();

        public override async Task PublishAsync(TEvent message, CancellationToken cancellationToken)
        {
            try
            {
                await base.PublishAsync(message, cancellationToken);
                await _commandRepository.DeleteAsync(message);
                await _commandRepository.SaveChangesAsyc(message.RequestedBy, cancellationToken);
            }
            catch (Exception ex)
            {
                //TODO:
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
                    _queryRepository.Dispose();
                    _commandRepository.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageConsumer()
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
