using eShop.Ordering.Inquiry.StateViews;
using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;

namespace eShop.Ordering.Management.Service.ReturnOrder;

public class ReturnOrderHandler : CommandHandler<ReturnOrderCommand, OrderStateView>
{
    public ReturnOrderHandler
    (
        IStateViewStore<OrderStateView> stateViewStore,
        IServiceProvider serviceProvider,
        IDomain<ReturnOrderCommand, OrderStateView> domain,
        IEventStore eventStore
    )
        : base(stateViewStore, serviceProvider, domain, eventStore)
    {
    }

    public override async Task<IEnumerable<IEvent>> HandleAsync(ReturnOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.OrderNumber))
        {
            throw new ArgumentException("Missing order number");
        }
        _subject = OrderStateView.GetDefaultSubject(command.OrderNumber);   // must be set for state view hydration
        return await base.HandleAsync(command, cancellationToken);
    }
}
