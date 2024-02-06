using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;

namespace IoCloud.Shared.Messaging.EventHub.Infrastructure
{
    public class EventProcessorClient<TEvent> : EventProcessorClient
    {
        public EventProcessorClient(BlobContainerClient checkpointStore, string consumerGroup, string connectionString)
            : base(checkpointStore, consumerGroup, connectionString)
        {
        }

        public EventProcessorClient(BlobContainerClient checkpointStore, string consumerGroup, string connectionString, EventProcessorClientOptions clientOptions)
            : base(checkpointStore, consumerGroup, connectionString, clientOptions)
        {
        }

        public EventProcessorClient(BlobContainerClient checkpointStore, string consumerGroup, string connectionString, string eventHubName)
            : base(checkpointStore, consumerGroup, connectionString, eventHubName)
        {
        }

        public EventProcessorClient(BlobContainerClient checkpointStore, string consumerGroup, string connectionString, string eventHubName, EventProcessorClientOptions clientOptions)
            : base(checkpointStore, consumerGroup, connectionString, eventHubName, clientOptions)
        {
        }

    }
}
