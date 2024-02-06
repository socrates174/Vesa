using Microsoft.AspNetCore.Http;
using IoCloud.Shared.Auth.Extensions;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.HttpRouting.Extensions;
using System.Security.Claims;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{
    /// <summary>
    /// Resolves the name of a Http requestor searching for "UserId" in JWT token's Claims (Type property) collection
    /// </summary>
    public class JwtTokenUserIdResolver : IRequestedByResolver
    {
        public string Resolve(HttpRequest request, ClaimsPrincipal user)
        {
            string requestedBy = null;
            var accessToken = request.GetAccessToken();
            if (accessToken != null)
            {
                var jwtSecurityToken = accessToken.GetJwtSecurityToken();
                requestedBy = jwtSecurityToken.GetUserId();
            }
            return requestedBy;
        }
    }
}