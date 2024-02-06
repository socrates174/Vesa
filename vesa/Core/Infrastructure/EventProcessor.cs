using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public class EventProcessor : IEventProcessor
{
    private readonly IDictionary<string, IEventObservers> _eventObserveds = new Dictionary<string, IEventObservers>();

    public EventProcessor(IEnumerable<IEventObservers> eventObserveds)
    {
        foreach (var eventObserved in eventObserveds)
        {
            var key = eventObserved.GetType().FullName;
            if (!_eventObserveds.ContainsKey(key))
            {
                _eventObserveds.Add(key, eventObserved);
            }
        }
    }

    public async Task<bool> ProcessAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var processed = false;
        Type eventObservedType = typeof(EventObservers<>);
        Type genericEventObservedType = eventObservedType.MakeGenericType(@event.GetType());
        if (_eventObserveds.ContainsKey(genericEventObservedType.FullName))
        {
            var eventObserved = _eventObserveds[genericEventObservedType.FullName];
            await eventObserved.NotifyAsync(@event, cancellationToken);
            processed = true;
        }
        return processed;
    }
}



