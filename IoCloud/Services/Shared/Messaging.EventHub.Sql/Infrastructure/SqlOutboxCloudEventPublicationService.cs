using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messaging.Publication.Abstractions;

namespace IoCloud.Shared.Messaging.Publication.Infrastructure
{
    public class SqlOutboxCloudEventPublicationService<TEvent> : BackgroundService
        where TEvent : CloudEventMessage
    {
        const int MESSAGE_BATCH_SIZE = 10;
        private readonly IOutboxMessagePublisher<TEvent> _outBoxMessagePublisher;
        private readonly ILogger<SqlOutboxCloudEventPublicationService<TEvent>> _logger;

        public SqlOutboxCloudEventPublicationService
        (
            IOutboxMessagePublisher<TEvent> outboxMessagePublisher,
            ILogger<SqlOutboxCloudEventPublicationService<TEvent>> logger
        )
        {
            _outBoxMessagePublisher = outboxMessagePublisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var messages = await _outBoxMessagePublisher.GetMessages().OrderBy(m => m.Time).Take(MESSAGE_BATCH_SIZE).ToListAsync();
                foreach (var message in messages)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            await _outBoxMessagePublisher.PublishAsync(message, stoppingToken);

                            _logger.LogInformation($"Processed message Id: {message.Id}, Type: {message.Type}, Created: {message.Time}, Processed: {DateTimeOffset.Now}, Payload: {message.Data}");
                        }
                        catch (Exception ex)
                        {
                            //TODO: handle the message failure
                            _logger.LogError($"Unable to process message Id: {message.Id}, Type: {message.Type}, Created: {message.Time}, Rejected: {DateTimeOffset.Now}, Payload: {message.Data}");
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
        }
    }
}
