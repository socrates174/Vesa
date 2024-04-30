namespace vesa.Core.Abstractions;

public interface IEventPropagationService
{
    IEnumerable<IEvent> GetPropagationEvents<TEvent>(TEvent @event) where TEvent : class, IEvent;
}