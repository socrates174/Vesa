using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.EventHub.Infrastructure;
using IoCloud.Shared.Messaging.Publication.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace IoCloud.Shared.Messaging.Publication.Infrastructure
{
    public class EventHubCloudEventPublisher : EventHubCloudEventPublisher<CloudEventMessage>
    {
        public EventHubCloudEventPublisher
        (
            EventHubProducerClient<CloudEventMessage> client,
            ILogger<EventHubCloudEventPublisher> logger
        )
            : base(client, logger)
        {
        }
    }

    /// <summary>
    /// Publishes an event (Cloud Event) to an Event Hub
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class EventHubCloudEventPublisher<TMessage> : IMessagePublisher<TMessage>
        where TMessage : CloudEventMessage
    {
        protected bool disposedValue;
        protected readonly EventHubProducerClient _client;
        protected readonly ILogger<EventHubCloudEventPublisher<TMessage>> _logger;

        public EventHubCloudEventPublisher
        (
            EventHubProducerClient<TMessage> client,
            ILogger<EventHubCloudEventPublisher<TMessage>> logger
        )
        {
            _client = client;
            _logger = logger;
        }

        public virtual async Task PublishAsync(TMessage message, CancellationToken cancellation)
        {
            try
            {
                EventDataBatch eventBatch = await _client.CreateBatchAsync(new CreateBatchOptions { PartitionKey = message.Subject });
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))));
                await _client.SendAsync(eventBatch);
            }

            catch (EventHubsException ex)
            {
                _logger.LogError($"{nameof(EventHubsException)} - error details: {ex.Message}");
                throw;
            }
        }

        public virtual async Task PublishAsync(IEnumerable<TMessage> messages, CancellationToken cancellation)
        {
            try
            {
                var partitionKey = messages.First().Subject;
                EventDataBatch eventBatch = await _client.CreateBatchAsync(new CreateBatchOptions { PartitionKey = partitionKey });
                bool keepAdding = true;

                foreach (var message in messages)
                {
                    if (message.Subject == partitionKey)
                    {
                        keepAdding = eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))));
                    }
                    else
                    {
                        keepAdding = false;
                    }

                    if (!keepAdding)
                    {
                        await _client.SendAsync(eventBatch);
                        partitionKey = message.Subject;
                        eventBatch = await _client.CreateBatchAsync(new CreateBatchOptions { PartitionKey = partitionKey });
                    }
                }
            }

            catch (EventHubsException ex)
            {
                _logger.LogError($"{nameof(EventHubsException)} - error details: {ex.Message}");
                throw;
            }
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
