namespace Serilog.AspNetCore;

/// <summary>
/// HttpContext loggable information
/// </summary>
public class HttpContextInformation
{
    /// <summary>
    /// Http request loggable information
    /// </summary>
    public HttpRequestInformation Request { get; }
    /// <summary>
    /// Http response loggable information
    /// </summary>
    public HttpResponseInformation Response { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    public HttpContextInformation(HttpRequestInformation request, HttpResponseInformation response)
    {
        Request = request;
        Response = response;
    }
}