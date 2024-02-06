using eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrderDetails;
using IoCloud.Shared.HttpRouting.Abstractions;
using MediatR;

namespace eShop.Ordering.OrderInquiry.WebAPI.Routers
{
    public class ViewOrderDetailsRouter : IHttpRouter
    {
        private const string URL = "/order/{id:Guid}";

        private readonly IMediator _mediator;

        public ViewOrderDetailsRouter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public virtual void Route(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet(URL, async (ViewOrderDetailsQuery query) =>
            {
                var result = await _mediator.Send(query);
                return Results.Ok(result);
            });
        }
    }
}
