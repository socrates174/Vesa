using IoCloud.Shared.Messages;

namespace IoCloud.Shared.Messaging.Publication.Abstractions
{
    public interface IMessagePublisher<TMessage>
        where TMessage : class, IMessage
    {
        Task PublishAsync(TMessage message, CancellationToken cancellation);
        Task PublishAsync(IEnumerable<TMessage> messages, CancellationToken cancellation);
    }
}
