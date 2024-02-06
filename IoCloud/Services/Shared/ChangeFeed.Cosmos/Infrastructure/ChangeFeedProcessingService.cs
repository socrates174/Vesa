using IoCloud.Shared.ChangeFeed.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoCloud.Shared.ChangeFeed.Infrastructure
{
    /// <summary>
    /// A background service that listens to changes (adds/updates) to a Cosmos container via Change Feed
    /// </summary>
    public class ChangeFeedProcessingService<TChangedEntity, TRootEntity> : ChangeFeedProcessingService<TChangedEntity>
        where TChangedEntity : class
    {
        public ChangeFeedProcessingService
        (
            IChangeFeedProcessorFactory<TChangedEntity, TRootEntity> changeFeedProcessorFactory,
            ILogger<ChangeFeedProcessingService<TChangedEntity, TRootEntity>> logger
        )
            : base(changeFeedProcessorFactory, logger)
        {
        }
    }

    public class ChangeFeedProcessingService<TChangedEntity> : BackgroundService
        where TChangedEntity : class
    {
        protected readonly IChangeFeedProcessorFactory<TChangedEntity> _changeFeedProcessorFactory;
        protected readonly ILogger<ChangeFeedProcessingService<TChangedEntity>> _logger;

        public ChangeFeedProcessingService
        (
            IChangeFeedProcessorFactory<TChangedEntity> changeFeedProcessorFactory,
            ILogger<ChangeFeedProcessingService<TChangedEntity>> logger
        )
        {
            _changeFeedProcessorFactory = changeFeedProcessorFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var changeFeedProcessor = _changeFeedProcessorFactory.CreateProcessor();

            _logger.LogInformation($"{nameof(ChangeFeedProcessingService<TChangedEntity>)} started: {DateTimeOffset.Now}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await changeFeedProcessor.StartAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                await Task.Delay(1000, stoppingToken);
            }

            await changeFeedProcessor.StopAsync();

            _logger.LogInformation($"{nameof(ChangeFeedProcessingService<TChangedEntity>)} ended: {DateTimeOffset.Now}");
        }
    }
}
