using Microsoft.AspNetCore.Http;
using IoCloud.Shared.Auth.Extensions;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.HttpRouting.Extensions;
using System.Security.Claims;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{
    public class JwtTokenNameResolver : IRequestedByResolver
    {
        /// <summary>
        /// Resolves the name of a Http requestor searching for the ClaimTypes.Name in JWT token's Claims (Type property) collection
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public string Resolve(HttpRequest request, ClaimsPrincipal user)
        {
            string requestedBy = null;
            var accessToken = request.GetAccessToken();
            if (accessToken != null)
            {
                var jwtSecurityToken = accessToken.GetJwtSecurityToken();
                requestedBy = jwtSecurityToken.GetUserName();
            }
            return requestedBy;
        }
    }
}