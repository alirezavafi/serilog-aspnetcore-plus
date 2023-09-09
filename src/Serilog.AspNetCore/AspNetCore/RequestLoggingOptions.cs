// Copyright 2019-2020 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Http;
using Serilog.Events;
using System;
using System.Collections.Generic;
using Serilog.Core;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Serilog.AspNetCore;

/// <summary>
/// Contains options for the <see cref="RequestLoggingMiddleware"/>.
/// </summary>
public class RequestLoggingOptions
{
    const string DefaultRequestCompletionMessageTemplate =
        "HTTP Request Completed {@Context}";

    static IEnumerable<LogEventProperty> DefaultGetMessageTemplateProperties(HttpContextInformation httpContextInfo, ILogger logger)
    {
        logger.BindProperty("Context", httpContextInfo, true, out var prop);
        if (prop != null)
            return new[] { prop };

        return Array.Empty<LogEventProperty>();
    }
    
    static LogEventLevel DefaultGetLevel(HttpContext ctx, double _, Exception? ex)
    {
        var level = LogEventLevel.Information;
        if (ctx.Response.StatusCode >= 500)
        {
            level = LogEventLevel.Error;
        }
        else if (ctx.Response.StatusCode >= 400)
        {
            level = LogEventLevel.Warning;
        }
        else if (ex != null)
        {
            level = LogEventLevel.Error;
        }

        return level;
    }

    /// <summary>
    /// Gets or sets the message template. The default value is
    /// <c>"HTTP Request Completed {@Context}"</c>. The
    /// template can contain any of the placeholders from the default template, names of properties
    /// added by ASP.NET Core, and names of properties added to the <see cref="IDiagnosticContext"/>.
    /// </summary>
    /// <value>
    /// The message template.
    /// </value>
    public string MessageTemplate { get; set; }
    
    /// <summary>
    /// A function to specify the values of the MessageTemplateProperties.
    /// </summary>
    public Func<HttpContextInformation, ILogger, IEnumerable<LogEventProperty>> GetMessageTemplateProperties { get; set; }

    /// <summary>
    /// A callback that can be used to set additional properties on the request completion event.
    /// </summary>
    public Action<IDiagnosticContext, HttpContext>? EnrichDiagnosticContext { get; set; }

    /// <summary>
    /// A function returning the <see cref="LogEventLevel"/> based on the <see cref="HttpContext"/>, the number of
    /// elapsed milliseconds required for handling the request, and an <see cref="Exception" /> if one was thrown.
    /// The default behavior returns <see cref="LogEventLevel.Error"/> when the response status code is greater than 499 or if the
    /// <see cref="Exception"/> is not null. Also default log level for 4xx range errors set to <see cref="LogEventLevel.Warning"/>   
    /// </summary>
    /// <value>
    /// A function returning the <see cref="LogEventLevel"/>.
    /// </value>
    public Func<HttpContext, double, Exception?, LogEventLevel> GetLevel { get; set; }

    /// <summary>
    /// The logger through which request completion events will be logged. The default is to use the
    /// static <see cref="Log"/> class.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Determines when logging requests information. Default is true.
    /// </summary>
    public LogMode LogMode { get; set; } = LogMode.LogAll;

    /// <summary>
    /// Determines when logging request headers
    /// </summary>
    public LogMode RequestHeaderLogMode { get; set; } = LogMode.LogAll;

    /// <summary>
    /// Determines when logging request body data
    /// </summary>
    public LogMode RequestBodyLogMode { get; set; } = LogMode.LogAll;

    /// <summary>
    /// Determines when logging response headers
    /// </summary>
    public LogMode ResponseHeaderLogMode { get; set; } = LogMode.LogAll;

    /// <summary>
    /// Determines when logging response body data
    /// </summary>
    public LogMode ResponseBodyLogMode { get; set; } = LogMode.LogFailures;

    /// <summary>
    /// Properties to mask before logging to output to prevent sensitive data leakage
    /// </summary>
    public IList<string> MaskedProperties { get; } =
        new List<string>()
            {"*password*", "*token*", "*clientsecret*", "*bearer*", "*authorization*", "*client-secret*", "*otp"};

    /// <summary>
    /// Mask format to replace with masked data
    /// </summary>
    public string MaskFormat { get; set; } = "*** MASKED ***";

    /// <summary>
    /// Maximum allowed length of response body text to capture in logs
    /// </summary>
    public int ResponseBodyLogTextLengthLimit { get; set; } = 4000;

    /// <summary>
    /// Maximum allowed length of request body text to capture in logs
    /// </summary>
    public int RequestBodyLogTextLengthLimit { get; set; } = 4000;

    /// <summary>
    /// Include the full URL query string in the <c>RequestPath</c> property
    /// that is attached to request log events. The default is <c>false</c>.
    /// </summary>
    public bool IncludeQueryInRequestPath { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public RequestLoggingOptions()
    {
        GetLevel = DefaultGetLevel;
        MessageTemplate = DefaultRequestCompletionMessageTemplate;
        GetMessageTemplateProperties = DefaultGetMessageTemplateProperties;
    }
}