using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Enrichers;

/// <summary>
/// Enrich requests using correlation id
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string CorrelationIdPropertyName = "X-Correlation-ID";
    private const string CorrelationIdItemName = "CorrelationId";

    /// <summary>
    /// 
    /// </summary>
    public CorrelationIdEnricher() : this(new HttpContextAccessor())
    {
    }

    internal CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// enrich logs with unique correlation id for each request
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyFactory"></param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = GetCorrelationId();
        if (string.IsNullOrWhiteSpace(correlationId))
            return;
            
        var correlationIdProperty = new LogEventProperty(CorrelationIdPropertyName, new ScalarValue(correlationId));
        logEvent.AddOrUpdateProperty(correlationIdProperty);
    }

    private string? GetCorrelationId()
    {
        var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = _httpContextAccessor.HttpContext?.Items[CorrelationIdItemName]?.ToString();
           
        return correlationId;
    }
}