using vesa.Core.Abstractions;
using vesa.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace vesa.Core.Infrastructure;

public class DomainEvents : List<IEvent>, IDomainEvents
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEvents(IServiceProvider serviceProvider) : base()
    {
        _serviceProvider = serviceProvider;
    }

    public IDomainEvents Add<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        // Add the event
        base.Add(@event);

        // If not an ExceptionEvent
        if (!(@event is ExceptionEvent))
        {
            // Add events for state views that are fed the order placed event
            var stateViewFeeders = _serviceProvider.GetRequiredService<IEnumerable<IStateView<TEvent>>>();
            AddRange(@event.GetStateViewFeederEvents(stateViewFeeders));
        }

        return this;
    }
}
