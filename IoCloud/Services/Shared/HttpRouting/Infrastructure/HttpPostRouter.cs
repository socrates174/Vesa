using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Infrastructure;
using IoCloud.Shared.Messages;
using System.Linq;
using System.Security.Claims;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{
    /// <summary>
    /// Routes a url to delegate which dispatches the command to a command handler using Minimal API
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public abstract class HttpPostRouter<TCommand, TReply> : IHttpRouter
        where TCommand : class, ICommand<TReply>
    {
        protected readonly IServiceProvider _serviceProvider;
        private readonly IRequestedByResolver _requestedByResolver;
        protected readonly IIocMediatorFactory _mediatorFactory;
        protected readonly string _url;

        public HttpPostRouter
        (
            IServiceProvider serviceProvider,
            IRequestedByResolver requestedByResolver,
            IIocMediatorFactory mediatorFactory,
            string url
        )
        {
            _serviceProvider = serviceProvider;
            _requestedByResolver = requestedByResolver;
            _mediatorFactory = mediatorFactory;
            _url = url;
        }

        public virtual void Route(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapPost(_url, async (HttpRequest request, ClaimsPrincipal user) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var mediator = _mediatorFactory.CreateMediator
                    (
                        type => scope.ServiceProvider.GetRequiredService(type)
                    );
                    var command = await request.ReadFromJsonAsync<TCommand>();
                    if (command is CloudEventMessagePayload)
                    {
                        var payload = command as CloudEventMessagePayload;
                        payload.Header.Source = request.Headers[HeaderNames.Referer];
                        payload.Header.RequestedBy = _requestedByResolver.Resolve(request, user);
                    }
                    var reply = await mediator.Send(command);
                    if (command.ValidationResult != null && !command.ValidationResult.IsValid)
                    {
                        return Results.ValidationProblem(command.ValidationResult.ToDictionary());
                    }
                    else if (reply is VoidReply)
                    {
                        return Results.Ok();
                    }
                    else
                    {
                        return Results.Ok(reply);
                    }
                }
            });
        }
    }
}
