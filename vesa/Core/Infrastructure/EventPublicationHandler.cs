using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public class EventPublicationHandler<TEvent, TDefaultStateView> : EventPropagationHandler<TEvent, TDefaultStateView>
    where TEvent : class, IEvent
    where TDefaultStateView : class, IStateView, new()
{
    private readonly IEventPublisher _eventPublisher;

    public EventPublicationHandler
    (
        IFactory<TDefaultStateView> defaultStateViewFactory,
        IDomainEvents domainEvents,
        IServiceProvider serviceProvider,
        IEventStore eventStore,
        IEventPublisher eventPublisher
    )
        : base(defaultStateViewFactory, domainEvents, serviceProvider, eventStore)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
    {
        await _eventPublisher.PublishEventAsync(@event, cancellationToken);
        await base.HandleAsync(@event, cancellationToken);
    }
}
