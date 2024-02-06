
using Microsoft.AspNetCore.Routing;

namespace IoCloud.Shared.HttpRouting.Abstractions
{
    public interface IHttpRouterCollection
    {
        void Route(IEndpointRouteBuilder endpointRouteBuilder);
    }
}