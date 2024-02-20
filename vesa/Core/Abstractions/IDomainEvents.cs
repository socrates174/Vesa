namespace vesa.Core.Abstractions;

public interface IDomainEvents : IEnumerable<IEvent>
{
    IDomainEvents Add<TEvent>(TEvent @event) where TEvent : class, IEvent;
    void Clear();
}