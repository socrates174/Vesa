using eShop.Ordering.Inquiry.StateViews;
using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;

namespace eShop.Ordering.Management.Service.CancelOrder;

public class CancelOrderHandler : CommandHandler<CancelOrderCommand, OrderStateView>
{
    public CancelOrderHandler
    (
        IStateViewStore<OrderStateView> stateViewStore,
        IServiceProvider serviceProvider,
        IDomain<CancelOrderCommand, OrderStateView> domain,
        IEventStore eventStore
    )
        : base(stateViewStore, serviceProvider, domain, eventStore)
    {
    }

    public override async Task<IEnumerable<IEvent>> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.OrderNumber))
        {
            throw new ArgumentException("Missing order number");
        }
        _subject = OrderStateView.GetDefaultSubject(command.OrderNumber);   // must be set for state view hydration
        return await base.HandleAsync(command, cancellationToken);
    }
}
