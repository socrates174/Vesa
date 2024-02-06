using AutoMapper;
using eShop.Ordering.OrderManagement.Data.Entities;
using FluentValidation;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Extensions;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using IoCloud.Shared.Querying.NoSql.Abstractions;

namespace eShop.Ordering.OrderManagement.Service.UseCases.CancelOrder
{
    public class CancelOrderHandler : ICommandHandler<CancelOrderCommand, CancelOrderReply>, IDisposable
    {
        private readonly IValidator<CancelOrderCommand> _cancelOrderValidator;
        private readonly INoSqlQueryRepository<Order> _queryRepository;
        private readonly IMapper _mapper;
        private readonly INoSqlEntityMessageStore<Order, Guid, Audit, Guid, InboxCommand, OutboxEvent> _entityMessageStore;
        private bool disposedValue;

        public CancelOrderHandler
        (
            IValidator<CancelOrderCommand> cancelOrderValidator,
            INoSqlQueryRepository<Order> queryRepository,
            IMapper mapper,
            INoSqlEntityMessageStore<Order, Guid, Audit, Guid, InboxCommand, OutboxEvent> entityMessageStore
        )
        {
            _cancelOrderValidator = cancelOrderValidator;
            _queryRepository = queryRepository;
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

        public async Task<CancelOrderReply> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
        {
            command.ValidationResult = _cancelOrderValidator.Validate(command);
            if (command.ValidationResult != null && !command.ValidationResult.IsValid)
            {
                return null;
            }

            // retrieve the data entity
            var order = await _queryRepository.GetAsync(command.OrderId);

            // project from data entity to domain entity
            var cancelOrderDomain = _mapper.Map<Order, CancelOrderDomain>(order);

            // process the command
            cancelOrderDomain.Process(command);

            // map domain entity to data entity
            _mapper.Map(cancelOrderDomain, order);
            var inboxCommand = _mapper.Map<CancelOrderCommand, InboxCommand>(command);
            var outboxMessages = cancelOrderDomain.DomainMessages.ToOutboxMessages<OutboxEvent>(_mapper);

            await _entityMessageStore.SaveAsync(order, inboxCommand, outboxMessages, command.Header.RequestedBy, cancellationToken);

            return new CancelOrderReply { OrderId = order.Id };
        }
    }
}
