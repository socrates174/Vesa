using eShop.Ordering.Management.Events;
using vesa.Core.Abstractions;
using vCI = vesa.Core.Infrastructure;

namespace eShop.Ordering.Management.Service.Slices.PlaceOrder;

public class StockReorderedHandler : vCI.EventHandler<StockReorderedEvent>
{
    private readonly IEventPublisher _eventPublisher;

    public StockReorderedHandler
    (
        IServiceProvider serviceProvider,
        IEventStore eventStore,
        IEventPublisher eventPublisher
    )
        : base(serviceProvider, eventStore)
    {
        _eventPublisher = eventPublisher;
    }

    public override async Task HandleAsync(StockReorderedEvent @event, CancellationToken cancellationToken)
    {
        await _eventPublisher.PublishEventAsync(@event, cancellationToken);
    }
}
