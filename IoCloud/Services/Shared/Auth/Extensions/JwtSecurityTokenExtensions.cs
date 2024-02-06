using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IoCloud.Shared.Auth.Extensions
{
    public static class JwtSecurityTokenExtensions
    {
        public static string GetUserName(this JwtSecurityToken token)
        {
            return (token.Claims as List<Claim>).SingleOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }

        public static string GetUserId(this JwtSecurityToken token)
        {
            return (token.Claims as List<Claim>).SingleOrDefault(c => c.Type.ToLower() == "userid")?.Value;
        }
        public static string GetClaimValue(this JwtSecurityToken token, string claimType)
        {
            return (token.Claims as List<Claim>).SingleOrDefault(c => c.Type.ToLower() == claimType)?.Value;
        }

        public static IEnumerable<string> GetUserCapabilities(this JwtSecurityToken token)
        {
            return (token.Claims as List<Claim>).Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        }
    }
}
