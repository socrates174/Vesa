using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace IoCloud.Shared.Messaging.EventHub.Infrastructure
{
    public class EventHubProducerClient<TEvent> : EventHubProducerClient
    {
        public EventHubProducerClient(string connectionString) : base(connectionString)
        {
        }

        public EventHubProducerClient(string connectionString, EventHubProducerClientOptions clientOptions) : base(connectionString, clientOptions)
        {
        }

        public EventHubProducerClient(string connectionString, string eventHubName) : base(connectionString, eventHubName)
        {
        }

        public EventHubProducerClient(string connectionString, string eventHubName, EventHubProducerClientOptions clientOptions) : base(connectionString, eventHubName, clientOptions)
        {
        }

        public EventHubProducerClient(EventHubConnection connection, EventHubProducerClientOptions clientOptions = default) : base(connection, clientOptions)
        {
        }
    }
}