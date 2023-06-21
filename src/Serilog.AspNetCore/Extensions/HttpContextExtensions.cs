using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Serilog.Extensions
{
    internal static class HttpContextExtensions
    {
        internal static IPAddress GetClientIp(this HttpContext httpContext)
        {
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
                remoteIp = remoteIp.MapToIPv4();
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIps))
            {
                var ipString = forwardedIps.First().Split(',')[0].Split(':')[0].Trim();
                try
                {
                    remoteIp = IPAddress.Parse(ipString);
                }
                catch (Exception e)
                {
                    Serilog.Log.Warning(e, "Cannot parse {@RemoteIp}", ipString);
                }
            }

            return remoteIp;
        }
    }
}
