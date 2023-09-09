namespace Serilog.AspNetCore;

/// <summary>
/// Http response loggable information
/// </summary>
public class HttpResponseInformation
{
    /// <summary>
    /// Response Http Status Code
    /// </summary>
    public int StatusCode { get; }
    /// <summary>
    /// Indicates is response succeed or not
    /// </summary>
    public bool IsSucceed { get; }
    /// <summary>
    /// Elapsed milliseconds to respond to http request
    /// </summary>
    public double ElapsedMilliseconds { get; }
    /// <summary>
    /// Response body as string
    /// </summary>
    public string? BodyString { get; }
    /// <summary>
    /// Deserialized response body
    /// </summary>
    public object? Body { get; }
    /// <summary>
    /// Http Headers
    /// </summary>
    public Dictionary<string, object> Header { get; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="isSucceed"></param>
    /// <param name="elapsedMilliseconds"></param>
    /// <param name="bodyString"></param>
    /// <param name="body"></param>
    /// <param name="header"></param>
    public HttpResponseInformation(int statusCode, bool isSucceed, double elapsedMilliseconds, string? bodyString, object? body, Dictionary<string, object> header)
    {
        StatusCode = statusCode;
        IsSucceed = isSucceed;
        ElapsedMilliseconds = elapsedMilliseconds;
        BodyString = bodyString;
        Body = body;
        Header = header;
    }
}