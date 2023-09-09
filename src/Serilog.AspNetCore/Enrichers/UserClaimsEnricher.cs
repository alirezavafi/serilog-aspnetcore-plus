using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Enrichers;

/// <summary>
/// Enriches user claims in log entries
/// </summary>
public class UserClaimsEnricher : ILogEventEnricher
{
    private HashSet<string> ignoredClaims = new HashSet<string>();

    /// <summary>
    /// Enriches user claims except ignoredclaims
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyFactory"></param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContextAccessor = new HttpContextAccessor();
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity == null ||
            !httpContext.User.Identity.IsAuthenticated)
        {
            return;
        }

        var user = httpContext.User;
        var userClaimsByType = user.Claims.Where(x => !ignoredClaims.Contains(x.Type)).GroupBy(x => x.Type);
        var claimsDic = new Dictionary<string, object>();
        foreach (var claim in userClaimsByType)
        {
            if (claim.Count() > 1)
                claimsDic.Add(claim.Key, claim.Select(x => x.Value).ToArray());
            else
                claimsDic.Add(claim.Key, claim.First().Value);
        }
            
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("User", claimsDic));
    }
}