using Microsoft.AspNetCore.Routing;
using IoCloud.Shared.HttpRouting.Abstractions;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{
    /// <summary>
    /// Maps the url routes via a collection of HttpRouter(s) that are injected to the constructor
    /// </summary>
    public class HttpRouterCollection : IHttpRouterCollection
    {
        private readonly IEnumerable<IHttpRouter> _routers;

        public HttpRouterCollection(IEnumerable<IHttpRouter> routers)
        {
            _routers = routers;
        }

        public void Route(IEndpointRouteBuilder endpointRouteBuilder)
        {
            foreach(var router in _routers)
            {
                router.Route(endpointRouteBuilder);
            }

        }
    }
}
