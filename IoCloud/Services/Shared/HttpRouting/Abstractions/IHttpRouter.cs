using Microsoft.AspNetCore.Routing;

namespace IoCloud.Shared.HttpRouting.Abstractions
{
    public interface IHttpRouter
    {
        void Route(IEndpointRouteBuilder endpointRouteBuilder);
    }
}
