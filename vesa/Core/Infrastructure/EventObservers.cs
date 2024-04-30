using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public class EventObservers<TEvent> : IEventObservers
    where TEvent : IEvent
{
    private readonly IEnumerable<IEventHandler<TEvent>> _eventHandlers;

    public EventObservers(IEnumerable<IEventHandler<TEvent>> eventHandlers)
    {
        _eventHandlers = eventHandlers;
    }

    public async Task NotifyAsync(IEvent @event, CancellationToken cancellationToken)
    {
        if (@event is TEvent observedEvent)
        {
            foreach (var observer in _eventHandlers)
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
