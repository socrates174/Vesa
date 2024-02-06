using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.Consumption.Abstractions;

namespace IoCloud.Shared.Messaging.Consumption.Infrastructure
{
    /// <summary>
    /// Base message processor that maps a CloudEvent.Type to a concrete event class and stub for processing the event
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageProcessor<TMessage> : IMessageProcessor<TMessage>
        where TMessage : class, IMessage, new()
    {
        protected bool disposedValue;

        protected Dictionary<string, (Type ExternalMessagePayloadType, Type InternalMessagePayloadType)> _messagePayloadTypes { get; } = new();

        public virtual void AddMessagePayloadType(string messagePayloadTypeName, Type externalMessagePayloadType, Type internalMessagePayloadType = null)
        {
            if (!_messagePayloadTypes.ContainsKey(messagePayloadTypeName))
            {
                _messagePayloadTypes.Add
                (
                    messagePayloadTypeName,
                    (ExternalMessagePayloadType: externalMessagePayloadType, InternalMessagePayloadType: internalMessagePayloadType)
                );
            }
        }

        public virtual async Task<bool> ProcessAsync(TMessage message, CancellationToken cancellationToken)
        {
            return await Task.FromResult(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageConsumer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}