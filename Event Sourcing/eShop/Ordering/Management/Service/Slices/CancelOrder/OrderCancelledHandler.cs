using eShop.Ordering.Inquiry.StateViews;
using eShop.Ordering.Management.Events;
using vesa.Core.Abstractions;
using vCI = vesa.Core.Infrastructure;

namespace eShop.Ordering.Management.Service.Slices.PlaceOrder;

public class OrderCancelledHandler : vCI.EventHandler<OrderCancelledEvent>
{
    public OrderCancelledHandler
    (
        IServiceProvider serviceProvider,
        IEventStore eventStore
    )
        : base(serviceProvider, eventStore)
    {
    }

    public override async Task HandleAsync(OrderCancelledEvent @event, CancellationToken cancellationToken)
    {
        // if the event has the default subject
        if (@event.Subject == OrderStateView.GetDefaultSubject(@event.OrderNumber))
        {
            // save the same event with different subjects to feed multiple state views
            await AddEventsWithStateViewSubjectsAsync(@event, cancellationToken);
        }
    }
}
