using Microsoft.AspNetCore.Http;
using IoCloud.Shared.Auth.Extensions;
using IoCloud.Shared.HttpRouting.Abstractions;
using System.Security.Claims;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{
    public class ClaimsPrincipalUserIdResolver : IRequestedByResolver
    {
        public string Resolve(HttpRequest request, ClaimsPrincipal user) => user.GetUserId();
    }
}
