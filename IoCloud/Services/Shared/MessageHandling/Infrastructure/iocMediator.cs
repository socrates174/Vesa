using MediatR;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Exceptions;
using IoCloud.Shared.MessageHandling.Extensions;

namespace IoCloud.Shared.MessageHandling.Infrastructure
{
    /// <summary>
    /// Dispatches a message (event/command/query) to a handler (event handler/command handler/query handler)
    /// </summary>
    public class IocMediator : IIocMediator
    {
        private readonly IMediator _mediator;

        public IocMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            if (request is IBaseCommand)
            {
                var command = (IBaseCommand)request;
                if (!command.HasHandler())
                {
                    throw new UnhandledCommandException();
                }
            }
            return await _mediator.Send(request, cancellationToken);
        }

        public async Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            if (notification is IEvent)
            {
                var theEvent = (IEvent)notification;
                if (!theEvent.HasHandler())
                {
                    throw new UnhandledEventException();
                }
            }

            await _mediator.Publish(notification, cancellationToken);
        }
    }
}
