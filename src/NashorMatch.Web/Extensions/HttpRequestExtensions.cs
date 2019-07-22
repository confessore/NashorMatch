using Microsoft.AspNetCore.Http;
using System.Net;

namespace NashorMatch.Web.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsLocal(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    if (connection.RemoteIpAddress == connection.LocalIpAddress)
                        return true;
                    else
                        return IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
                return true;
            return false;
        }
    }
}
