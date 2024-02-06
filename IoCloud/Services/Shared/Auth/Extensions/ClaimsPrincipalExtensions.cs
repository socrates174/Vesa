using System.Security.Claims;

namespace IoCloud.Shared.Auth.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.Claims.SingleOrDefault(c => c.Type.ToLower() == "userid")?.Value;
        }
    }
}
