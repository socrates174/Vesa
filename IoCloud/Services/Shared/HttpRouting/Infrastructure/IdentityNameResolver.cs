using Microsoft.AspNetCore.Http;
using IoCloud.Shared.HttpRouting.Abstractions;
using System.Security.Claims;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{
    /// <summary>
    /// Resolves the name of Http requestor via Claims Principal Identity
    /// </summary>
    public class IdentityNameResolver : IRequestedByResolver
    {
        public string Resolve(HttpRequest request, ClaimsPrincipal user) => user?.Identity?.Name;
    }
}
