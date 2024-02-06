using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.Consumption.Abstractions;
using IoCloud.Shared.Messaging.EventHub.Infrastructure;
using IoCloud.Shared.Querying.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace IoCloud.Shared.Messaging.Consumption.Infrastructure
{
    /// <summary>
    /// Background service that consumes an event (CloudEvent) from an Event Hub and hands it off to a processor
    /// </summary>
    public class EventHubCloudEventConsumptionService : EventHubCloudEventConsumptionService<CloudEventMessage>
    {
        public EventHubCloudEventConsumptionService
        (
            EventProcessorClient<CloudEventMessage> eventProcessorClient,
            IMessageProcessor<CloudEventMessage> messageProcessor,
            ILogger<EventHubCloudEventConsumptionService> logger
        )
            : base(eventProcessorClient, messageProcessor, null, logger)
        {
        }
    }

    public class EventHubCloudEventConsumptionService<TMessage> : BackgroundService
        where TMessage : CloudEventMessage
    {
        protected readonly EventProcessorClient _eventProcessorClient;
        protected readonly IMessageProcessor<TMessage> _messageProcessor;
        protected readonly IQueryExistence<TMessage, string> _queryExistence;
        protected readonly ILogger<EventHubCloudEventConsumptionService> _logger;
        protected CancellationToken _stoppingToken;

        public EventHubCloudEventConsumptionService
        (
            EventProcessorClient<TMessage> eventProcessorClient,
            IMessageProcessor<TMessage> messageProcessor,
            IQueryExistence<TMessage, string> queryExistence,
            ILogger<EventHubCloudEventConsumptionService> logger
        )
        {
            _eventProcessorClient = eventProcessorClient;
            _messageProcessor = messageProcessor;
            _queryExistence = queryExistence;
            _logger = logger;

            _eventProcessorClient.ProcessEventAsync += ProcessEventHandler;
            _eventProcessorClient.ProcessErrorAsync += ProcessErrorHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _eventProcessorClient.StartProcessingAsync(stoppingToken);

                    // Check every 1 seconds - can make configurable
                    await Task.Delay(1 * 1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex.GetType().Name} - error details: {ex.Message}");
                }
            }
            await _eventProcessorClient.StopProcessingAsync();
        }

        protected virtual async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            bool skipProcess = false;
            try
            {
                // Write the body of the event to the console window
                var payload = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
                var message = JsonConvert.DeserializeObject<TMessage>(payload);


                // If checking for already processed inbox messages
                if (_queryExistence != null && (await _queryExistence.ExistsAsync(message.Id)))
                {
                    skipProcess = true;
                }

                // skip or process the event
                if (skipProcess || (await _messageProcessor.ProcessAsync(message, _stoppingToken)))
                {

                    // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
                    // TODO: if this becomes a bottleneck, we can implement
                    // (1) batch checkpointing by event proccessed count or time duration
                    await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                var properties = String.Join(", ", eventArgs.Data.Properties.Select(x => $"[{x.Key}]:[{x.Value}]"));
                _logger.LogError
                (
                    ex,
                    "Failed during event processing.  Will not be retried.  Message Id {messageId} / partitionKey {partitionKey}. Properties {properties}",
                    eventArgs.Data.MessageId,
                    eventArgs.Data.PartitionKey,
                    properties
                );
            }
        }

        protected Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            _logger.LogError
            (
                eventArgs.Exception,
                "Error processing event in partition [ {eventPartitionId} ]. Operation: [ {eventOperation} ].",
                eventArgs.PartitionId,
                eventArgs.Operation ?? "Unknown"
            );

            return Task.CompletedTask;
        }
    }
}
