using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Infrastructure;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{

    /// <summary>
    /// Routes a url to delegate which dispatches the query to a query handler using Minimal API
    /// WARNING: AVOID USING UNTIL MICROSOFT FIXES THE LIMITATION BELOW:
    /// Will work only for queries that use static BindAsync and not TryParse for parameter binding.
    /// Unfortunately, that means bringing in HttpContext into the Service project which is a not a good idea.
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public abstract class HttpGetRouter<TQuery, TResult> : IHttpRouter
        where TQuery : IQuery<TResult>
    {
        protected readonly IIocMediator _mediator;
        protected readonly string _url;

        public HttpGetRouter
        (
            IIocMediator mediator,
            string url
        )
        {
            _mediator = mediator;
            _url = url;
        }

        // This does not work for Query that use static TryParse parameter binding
        // The name of the query or route parameter (in _url) must match the name of the input variable
        public virtual void Route(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet(_url, async (TQuery query) =>
            {
                var reply = await _mediator.Send(query);
                if (query.ValidationResult != null && !query.ValidationResult.IsValid)
                {
                    return Results.ValidationProblem(query.ValidationResult.ToDictionary());
                }
                else
                {
                    return Results.Ok(reply);
                }
            });
        }
    }
}