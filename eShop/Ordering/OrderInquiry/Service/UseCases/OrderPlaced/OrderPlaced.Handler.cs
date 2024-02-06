using AutoMapper;
using eShop.Ordering.OrderInquiry.Data.Entities;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Persistence.NoSql.Abstractions;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.OrderPlaced
{
    public class OrderPlacedHandler : IEventHandler<OrderPlacedEvent>, IDisposable
    {
        private readonly IMapper _mapper;
        private readonly INoSqlEntityMessageStore<Order, Guid, InboxEvent> _entityMessageStore;
        private bool disposedValue;

        public OrderPlacedHandler(IMapper mapper, INoSqlEntityMessageStore<Order, Guid, InboxEvent> entityMessageStore)
        {
            _mapper = mapper;
            _entityMessageStore = entityMessageStore;
        }

        public async Task Handle(OrderPlacedEvent theEvent, CancellationToken cancellationToken)
        {
            // since this is a brand new order, we can map theEvent directly to a new order data entity
            var order = _mapper.Map<OrderPlacedEvent, Order>(theEvent);

            // reformat the inbox event as a CloudEvent
            var inboxEvent = _mapper.Map<OrderPlacedEvent, InboxEvent>(theEvent);

            // Save the entity and the inbox event
            await _entityMessageStore.SaveAsync(order, inboxEvent, theEvent.Header.RequestedBy, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _entityMessageStore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~OrderPlacedEventHandler()
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