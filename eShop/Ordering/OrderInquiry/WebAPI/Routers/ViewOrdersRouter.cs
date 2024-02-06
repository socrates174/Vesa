using eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrders;
using IoCloud.Shared.HttpRouting.Abstractions;
using MediatR;

namespace eShop.Ordering.OrderInquiry.WebAPI.Routers
{
    public class ViewOrdersRouter : IHttpRouter
    {
        private const string URL = "/orders/{emailAddress:string}";

        private readonly IMediator _mediator;

        public ViewOrdersRouter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public virtual void Route(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet(URL, async (ViewOrdersQuery query) =>
            {
                var result = await _mediator.Send(query);
                return Results.Ok(result);
            });
        }
    }
}
