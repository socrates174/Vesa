using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public class EventProcessor : IEventProcessor
{
    private readonly IDictionary<string, IEventObservers> _eventObservers = new Dictionary<string, IEventObservers>();

    public EventProcessor(IEnumerable<IEventObservers> eventObserveds)
    {
        foreach (var eventObserved in eventObserveds)
        {
            var key = eventObserved.GetType().FullName;
            if (!_eventObservers.ContainsKey(key))
            {
                _eventObservers.Add(key, eventObserved);
            }
        }
    }

    public async Task<bool> ProcessAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var processed = false;
        Type eventObservedType = typeof(EventObservers<>);
        Type genericEventObservedType = eventObservedType.MakeGenericType(@event.GetType());
        if (_eventObservers.ContainsKey(genericEventObservedType.FullName))
        {
            var eventObserved = _eventObservers[genericEventObservedType.FullName];
            await eventObserved.NotifyAsync(@event, cancellationToken);
            processed = true;
        }
        return processed;
    }
}



