using eShop.Ordering.Inquiry.StateViews;
using eShop.Ordering.Management.Events;
using vesa.Core.Abstractions;
using vCI = vesa.Core.Infrastructure;

namespace eShop.Ordering.Management.Service.Slices.PlaceOrder;

public class OrderReturnedHandler : vCI.EventHandler<OrderReturnedEvent>
{
    public OrderReturnedHandler
    (
        IServiceProvider serviceProvider,
        IEventStore eventStore
    )
        : base(serviceProvider, eventStore)
    {
    }

    public override async Task HandleAsync(OrderReturnedEvent @event, CancellationToken cancellationToken)
    {
        // if the event has the default subject
        if (@event.Subject == OrderStateView.GetDefaultSubject(@event.OrderNumber))
        {
            // save the same event with different subjects to feed multiple state views
            await AddEventsWithStateViewSubjectsAsync(@event, cancellationToken);
        }
    }
}
