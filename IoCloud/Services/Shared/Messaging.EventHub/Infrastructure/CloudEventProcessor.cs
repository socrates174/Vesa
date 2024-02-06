using AutoMapper;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Settings.Abstractions;
using IoCloud.Shared.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IoCloud.Shared.Messaging.Consumption.Infrastructure
{
    public class CloudEventProcessor : CloudEventProcessor<CloudEventMessage>
    {
        public CloudEventProcessor
        (
            IInternalMessageMapping[] messageMappings,
            IServiceProvider serviceProvider,
            IMapper mapper,
            IIocMediatorFactory mediatorFactory,
            ILogger<CloudEventProcessor> logger
        )
            : base
        (
            messageMappings,
            serviceProvider,
            mapper,
            mediatorFactory,
            logger
        )
        {
        }
    }

    /// <summary>
    /// Processes an event (Cloud Event) by routing it to an event handler via a dispatcher (SGIMediator)
    /// </summary>
    public class CloudEventProcessor<TMessage> : MessageProcessor<TMessage>
        where TMessage : CloudEventMessage, new()
    {
        private readonly IInternalMessageMapping[] _messageMappings;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IMapper _mapper;
        protected readonly IIocMediatorFactory _mediatorFactory;
        protected readonly ILogger<CloudEventProcessor<TMessage>> _logger;

        public CloudEventProcessor
        (
            IInternalMessageMapping[] messageMappings,
            IServiceProvider serviceProvider,
            IMapper mapper,
            IIocMediatorFactory mediatorFactory,
            ILogger<CloudEventProcessor<TMessage>> logger
        )
        {
            _messageMappings = messageMappings;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _mediatorFactory = mediatorFactory;
            _logger = logger;

            MapMessageTypes();
        }

        public override async Task<bool> ProcessAsync(TMessage message, CancellationToken cancellationToken)
        {
            var processed = false;
            if (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var messagePayload = GetMessagePayload(message);
                    if (messagePayload != null)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var mediator = _mediatorFactory.CreateMediator
                            (
                                type => scope.ServiceProvider.GetRequiredService(type)
                            );

                            if (messagePayload is IEvent)
                            {
                                await mediator.Publish(messagePayload);
                            }
                            else
                            {
                                await mediator.Send(messagePayload);
                            }
                            processed = true;
                        }

                        _logger.LogInformation($"Processed message Id: {message.Id}, Type: {message.Type}, Created: {message.Time}, Processed: {DateTimeOffset.Now}, Payload: {message.Data}");
                    }
                    else
                    {
                        _logger.LogInformation($"Unhandled message Id: {message.Id}, Type: {message.Type}, Created: {message.Time}, Processed: {DateTimeOffset.Now}, Payload: {message.Data}");
                    }
                }
                catch (Exception ex)
                {
                    //TODO: handle the message failure
                    _logger.LogError($"Unable to process message Id: {message.Id}, Type: {message.Type}, Created: {message.Time}, Rejected: {DateTimeOffset.Now}, Payload: {message.Data}");
                }
            }
            return processed;
        }

        private object GetMessagePayload(TMessage message)
        {
            ICloudEventMessagePayload? messagePayload = null;
            if (_messagePayloadTypes.ContainsKey(message.Type))
            {
                var externalMessagePayloadType = _messagePayloadTypes[message.Type].ExternalMessagePayloadType;
                var internalMessagePayloadType = _messagePayloadTypes[message.Type].InternalMessagePayloadType;

                var externalDataAsJson = JsonConvert.SerializeObject(message.Data);
                var externalDataAsExternalObject = JsonConvert.DeserializeObject(externalDataAsJson, externalMessagePayloadType);

                if (internalMessagePayloadType == null)
                {
                    messagePayload = externalDataAsExternalObject as ICloudEventMessagePayload;
                }
                else
                {
                    var externalDataAsInternalObject = _mapper.Map(externalDataAsExternalObject, externalMessagePayloadType, internalMessagePayloadType);
                    messagePayload = externalDataAsInternalObject as ICloudEventMessagePayload;
                }
                _mapper.Map(message, messagePayload.Header);
            }
            return messagePayload;
        }

        private void MapMessageTypes()
        {
            foreach (var messageMapping in _messageMappings)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(messageMapping.InternalType))
                    {
                        AddMessagePayloadType(messageMapping.MessageType, TypeUtils.GetType(messageMapping.ExternalType), null);
                    }
                    else
                    {
                        AddMessagePayloadType(messageMapping.MessageType, TypeUtils.GetType(messageMapping.ExternalType), TypeUtils.GetType(messageMapping.InternalType));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unknown event type {messageMapping.ExternalType} or {messageMapping.InternalType ?? ""}");
                }
            }
        }
    }
}
