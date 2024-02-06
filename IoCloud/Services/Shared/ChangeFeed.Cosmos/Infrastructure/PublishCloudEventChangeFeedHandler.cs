using IoCloud.Shared.ChangeFeed.Abstractions;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.Publication.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IoCloud.Shared.ChangeFeed.Infrastructure
{
    /// <summary>
    /// Publishes an event added to a domain events container by the entity movement handler to Event Hub
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class PublishCloudEventChangeFeedHandler<TEvent> : IChangeFeedHandler<TEvent>
        where TEvent : CloudEventMessage
    {
        private readonly IMessagePublisher<TEvent> _publisher;
        private readonly ILogger<PublishCloudEventChangeFeedHandler<TEvent>> _logger;

        public PublishCloudEventChangeFeedHandler
        (
            IMessagePublisher<TEvent> publisher,
            ILogger<PublishCloudEventChangeFeedHandler<TEvent>> logger
        )
        {
            _publisher = publisher;
            _logger = logger;
        }

        public async Task HandleChangeAsync(TEvent message, CancellationToken cancellationToken)
        {
            try
            {
                await _publisher.PublishAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}: Message: {JsonConvert.SerializeObject(message)}");
                throw;
            }
        }
    }
}
