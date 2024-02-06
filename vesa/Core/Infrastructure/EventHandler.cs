using Microsoft.Extensions.DependencyInjection;
using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public abstract class EventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IEventStore _eventStore;

    public EventHandler
    (
        IServiceProvider serviceProvider,
        IEventStore eventStore
    )
    {
        _serviceProvider = serviceProvider;
        _eventStore = eventStore;
    }

    public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);

    protected async Task AddEventsWithStateViewSubjectsAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        // save the same event with different subjects to feed multiple state views
        var domainEvents = _serviceProvider.GetRequiredService<IDomainEvents>();
        domainEvents.Add(@event);
        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent.Subject != @event.Subject &&
                !(await _eventStore.EventExistsAsync(domainEvent.Id, domainEvent.Subject, cancellationToken)))
            {
                await _eventStore.AddEventAsync(domainEvent, cancellationToken);
            }
        }
    }
}
