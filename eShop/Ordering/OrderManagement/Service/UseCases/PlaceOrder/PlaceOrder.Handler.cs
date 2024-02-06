using AutoMapper;
using eShop.Ordering.OrderManagement.Data.Entities;
using FluentValidation;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Extensions;
using IoCloud.Shared.Persistence.NoSql.Abstractions;

namespace eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder
{
    public class PlaceOrderHandler : ICommandHandler<PlaceOrderCommand, PlaceOrderReply>, IDisposable
    {
        private readonly IValidator<PlaceOrderCommand> _placeOrderValidator;
        private readonly IPlaceOrderDomain _placeOrderDomain;
        private readonly IMapper _mapper;
        private readonly INoSqlEntityMessageStore<Order, Guid, Audit, Guid, InboxCommand, OutboxEvent> _entityMessageStore;
        private bool disposedValue;

        public PlaceOrderHandler
        (
            IValidator<PlaceOrderCommand> placeOrderValidator,
            IPlaceOrderDomain placeOrderDomain,
            IMapper mapper,
            INoSqlEntityMessageStore<Order, Guid, Audit, Guid, InboxCommand, OutboxEvent> entityMessageStore
        )
        {
            _placeOrderValidator = placeOrderValidator;
            _placeOrderDomain = placeOrderDomain;
            _mapper = mapper;
            _entityMessageStore = entityMessageStore;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    //_entityMessageStore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task<PlaceOrderReply> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
        {
            command.ValidationResult = _placeOrderValidator.Validate(command);
            if (command.ValidationResult != null && !command.ValidationResult.IsValid)
            {
                return null;
            }

            // this is a new order so there is no Order data entity to
            _placeOrderDomain.Process(command);

            var order = _mapper.Map<Order>(_placeOrderDomain);
            var inboxCommand = _mapper.Map<PlaceOrderCommand, InboxCommand>(command);
            var outboxMessages = _placeOrderDomain.DomainMessages.ToOutboxMessages<OutboxEvent>(_mapper);

            await _entityMessageStore.SaveAsync(order, inboxCommand, outboxMessages, command.Header.RequestedBy, cancellationToken);

            return new PlaceOrderReply { OrderId = order.Id, ExpectedDelivery = order.ExpectedDelivery };
        }
    }
}
