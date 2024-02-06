using AutoMapper;
using eShop.Ordering.OrderInquiry.Data.Entities;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using IoCloud.Shared.Querying.NoSql.Abstractions;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.OrderCancelled
{
    public class OrderCancelledHandler : IEventHandler<OrderCancelledEvent>, IDisposable
    {
        private readonly INoSqlQueryRepository<Order> _queryRepository;
        private readonly IMapper _mapper;
        private readonly INoSqlEntityMessageStore<Order, Guid, InboxEvent> _entityMessageStore;
        private bool disposedValue;

        public OrderCancelledHandler
        (
            INoSqlQueryRepository<Order> queryRepository,
            IMapper mapper, 
            INoSqlEntityMessageStore<Order, Guid, InboxEvent> entityMessageStore)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _entityMessageStore = entityMessageStore;
        }

        public async Task Handle(OrderCancelledEvent theEvent, CancellationToken cancellationToken)
        {
            var order = await _queryRepository.GetAsync(theEvent.OrderId);

            // map event to data entity
            _mapper.Map<OrderCancelledEvent, Order>(theEvent, order);

            // reformat the inbox event as a CloudEvent
            var inboxEvent = _mapper.Map<OrderCancelledEvent, InboxEvent>(theEvent);

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
        // ~OrderCancelledEventHandler()
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