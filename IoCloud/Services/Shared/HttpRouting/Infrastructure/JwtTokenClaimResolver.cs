using Microsoft.AspNetCore.Http;
using IoCloud.Shared.Auth.Extensions;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.HttpRouting.Extensions;
using System.Security.Claims;

namespace IoCloud.Shared.HttpRouting.Infrastructure
{
    /// <summary>
    /// Resolves the value of a Claim searching for a given Claim type in the JWT token Claims collection
    /// </summary>
    public class JwtTokenClaimResolver : IRequestedByResolver
    {
        private readonly string _claimType;

        public JwtTokenClaimResolver(string claimType)
        {
            _claimType = claimType;
        }

        public string Resolve(HttpRequest request, ClaimsPrincipal user)
        {
            string requestedBy = null;
            var accessToken = request.GetAccessToken();
            if (accessToken != null)
            {
                var jwtSecurityToken = accessToken.GetJwtSecurityToken();
                requestedBy = jwtSecurityToken.GetClaimValue(_claimType);
            }
            return requestedBy;
        }
    }
}