using Microsoft.Extensions.DependencyInjection;
using vesa.Core.Abstractions;
using vesa.Core.Extensions;

namespace vesa.Core.Infrastructure;

public class EventPropagationService : IEventPropagationService
{
    private readonly IServiceProvider _serviceProvider;

    public EventPropagationService(IServiceProvider serviceProvider) : base()
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<IEvent> GetPropagationEvents<TEvent>(TEvent @event) where TEvent : class, IEvent
    {

        IEnumerable<IEvent> propagationEvents = new List<IEvent>();

        if (!(@event is ExceptionEvent))
        {
            // Add events for state views that are fed the order placed event
            var stateViewFeeders = _serviceProvider.GetRequiredService<IEnumerable<IStateView<TEvent>>>();
            propagationEvents = @event.GetStateViewFeederEvents(stateViewFeeders);
        }

        return propagationEvents;
    }
}
