using System.IdentityModel.Tokens.Jwt;

namespace IoCloud.Shared.Auth.Extensions
{
    public static class AccessTokenExtensions
    {
        public static JwtSecurityToken GetJwtSecurityToken(this string accessToken)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            return jwtSecurityTokenHandler.ReadJwtToken(accessToken);
        }
    }
}
