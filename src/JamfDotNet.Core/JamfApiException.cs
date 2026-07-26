using System.Net;

namespace JamfDotNet.Core;

/// <summary>
/// Exception thrown when a Jamf API call fails.
/// </summary>
public sealed class JamfApiException : Exception
{
    /// <summary>
    /// Creates a new <see cref="JamfApiException"/>.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="statusCode">HTTP status code when available.</param>
    /// <param name="responseBody">Raw response body when available.</param>
    /// <param name="innerException">Optional inner exception.</param>
    public JamfApiException(
        string message,
        HttpStatusCode? statusCode = null,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// HTTP status returned by Jamf, when available.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Raw response body, when available.
    /// </summary>
    public string? ResponseBody { get; }
}
