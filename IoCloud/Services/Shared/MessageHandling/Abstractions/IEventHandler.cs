using MediatR;

namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface IEventHandler<in TEvent> : INotificationHandler<TEvent>
        where TEvent : IEvent
    {
    }
}
