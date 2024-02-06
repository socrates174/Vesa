using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace IoCloud.Shared.HttpRouting.Abstractions
{
    public interface IRequestedByResolver
    {
        string Resolve(HttpRequest request, ClaimsPrincipal user);
    }
}
