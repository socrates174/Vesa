using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public class EventObservers<TEvent> : IEventObservers
    where TEvent : IEvent
{
    private readonly IEnumerable<IEventHandler<TEvent>> _eventObservers;

    public EventObservers(IEnumerable<IEventHandler<TEvent>> eventObservers)
    {
        _eventObservers = eventObservers;
    }

    public async Task NotifyAsync(IEvent @event, CancellationToken cancellationToken)
    {
        if (@event is TEvent observedEvent)
        {
            foreach (var observer in _eventObservers)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await observer.HandleAsync(observedEvent, cancellationToken);
            }
        }
    }
}
