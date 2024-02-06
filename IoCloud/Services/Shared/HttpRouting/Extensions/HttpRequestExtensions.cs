using Microsoft.AspNetCore.Http;

namespace IoCloud.Shared.HttpRouting.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetAccessToken(this HttpRequest request)
        {
            string accessToken = null;
            if (request.Headers.ContainsKey("Authorization"))
            {
                var bearerToken = request.Headers["Authorization"].ToString();
                accessToken =  bearerToken.Substring(7);
            }
            return accessToken;
        }
    }
}
