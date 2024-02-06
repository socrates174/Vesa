using IoCloud.Shared.ChangeFeed.Abstractions;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IoCloud.Shared.ChangeFeed.Cosmos.Infrastructure
{
    /// <summary>
    /// When a domain event is added to an domain events container, extract the payload (CloudEventPayload) to a handler
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class RouteEventPayloadChangeFeedHandler<TEvent> : IChangeFeedHandler<TEvent>
        where TEvent : CloudEventMessage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IIocMediatorFactory _mediatorFactory;
        private readonly ILogger<RouteEventPayloadChangeFeedHandler<TEvent>> _logger;

        public RouteEventPayloadChangeFeedHandler
        (
            IServiceProvider serviceProvider,
            IIocMediatorFactory mediatorFactory,
            ILogger<RouteEventPayloadChangeFeedHandler<TEvent>> logger
        )
        {
            _serviceProvider = serviceProvider;
            _mediatorFactory = mediatorFactory;
            _logger = logger;
        }

        public async Task HandleChangeAsync(TEvent theEvent, CancellationToken cancellationToken)
        {
            try
            {
                var payload = GetPayload(theEvent);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var mediator = _mediatorFactory.CreateMediator
                    (
                        type => scope.ServiceProvider.GetRequiredService(type)
                    );
                    await mediator.Publish(payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}: Message: {JsonConvert.SerializeObject(theEvent)}");
                throw;
            }
        }

        private object GetPayload(TEvent theEvent)
        {
            var payloadJson = JsonConvert.SerializeObject(theEvent.Data);
            var payloadType = TypeUtils.GetType((theEvent.Data as dynamic).PayloadType as string);
            var payload = JsonConvert.DeserializeObject(payloadJson, payloadType);
            return payload;
        }
    }
}
