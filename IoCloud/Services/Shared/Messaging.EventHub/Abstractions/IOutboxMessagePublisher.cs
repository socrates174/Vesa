using IoCloud.Shared.Messages;

namespace IoCloud.Shared.Messaging.Publication.Abstractions
{
    public interface IOutboxMessagePublisher<TMessage> : IMessagePublisher<TMessage>
        where TMessage : class, IMessage
    {
        IQueryable<TMessage> GetMessages();
    }
}
