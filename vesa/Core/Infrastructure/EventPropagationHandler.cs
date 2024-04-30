using Microsoft.Extensions.DependencyInjection;
using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public class EventPropagationHandler<TEvent, TDefaultStateView> : IEventHandler<TEvent>
    where TEvent : class, IEvent
    where TDefaultStateView : class, IStateView, new()
{
    private readonly IFactory<TDefaultStateView> _defaultStateViewFactory;
    private readonly IServiceProvider _serviceProvider;
    protected readonly IEventStore _eventStore;

    public EventPropagationHandler
    (
        IFactory<TDefaultStateView> defaultStateViewFactory,
        IServiceProvider serviceProvider,
        IEventStore eventStore
    )
    {
        _defaultStateViewFactory = defaultStateViewFactory;
        _serviceProvider = serviceProvider;
        _eventStore = eventStore;
    }

    public virtual async Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
    {
        // if the event has the default subject
        var defaultStateView = _defaultStateViewFactory.Create();

        // this check prevents duplication of event creation for non-default subjects
        if (@event.SubjectPrefix == defaultStateView.SubjectPrefix)
        {
            // save the same event with different subjects to feed multiple state views
            await AddEventsWithStateViewSubjectsAsync(@event, cancellationToken);
        }
    }

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
