namespace Serilog.AspNetCore;

/// <summary>
/// Http request loggable information
/// </summary>
public class HttpRequestInformation
{
    /// <summary>
    /// Extracted Client Ip from http connection or forwarded headers
    /// </summary>
    public string? ClientIp { get; }
    /// <summary>
    /// Http request method such as GET, POST, ...
    /// </summary>
    public string Method { get; }
    /// <summary>
    /// Http request schema (http, https, ...)
    /// </summary>
    public string Scheme { get; }
    /// <summary>
    /// Http host 
    /// </summary>
    public string Host { get; }
    /// <summary>
    /// Http request path
    /// </summary>
    public string? Path { get; }
    /// <summary>
    /// Http request Query String
    /// </summary>
    public string? QueryString { get; }
    /// <summary>
    /// Http request extracted Query string 
    /// </summary>
    public Dictionary<string, object> Query { get; }
    /// <summary>
    /// Http request body as text
    /// </summary>
    public string? BodyString { get; }
    /// <summary>
    /// Deserialized http request body
    /// </summary>
    public object? Body { get; }
    /// <summary>
    /// Http request headers
    /// </summary>
    public Dictionary<string, object> Header { get; }
    /// <summary>
    /// Parsed user agent data
    /// </summary>
    public Dictionary<string, string> UserAgent { get; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="clientIp"></param>
    /// <param name="method"></param>
    /// <param name="scheme"></param>
    /// <param name="host"></param>
    /// <param name="path"></param>
    /// <param name="queryString"></param>
    /// <param name="query"></param>
    /// <param name="bodyString"></param>
    /// <param name="body"></param>
    /// <param name="header"></param>
    /// <param name="userAgent"></param>
    public HttpRequestInformation(string? clientIp, string method, string scheme, string host, string? path, string? queryString, Dictionary<string, object> query, string? bodyString, object? body, Dictionary<string, object> header, Dictionary<string, string> userAgent)
    {
        ClientIp = clientIp;
        Method = method;
        Scheme = scheme;
        Host = host;
        Path = path;
        QueryString = queryString;
        Query = query;
        BodyString = bodyString;
        Body = body;
        Header = header;
        UserAgent = userAgent;
    }
}