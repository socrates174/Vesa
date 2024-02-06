using IoCloud.Shared.Messages;

namespace IoCloud.Shared.Messaging.Consumption.Abstractions
{
    public interface IMessageProcessor<TMessage>
         where TMessage : class, IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messagePayloadTypeName">The type of the cloud event message.  </param>
        /// <param name="externalMessagePayloadType">The type of the external payload object</param>
        /// <param name="internalMessagePayloadType">The type of the internal type that the external type is mapped to</param>
        void AddMessagePayloadType(string messagePayloadTypeName, Type externalMessagePayloadType, Type internalMessagePayloadType = null);

        Task<bool> ProcessAsync(TMessage message, CancellationToken cancellationToken);
    }
}
