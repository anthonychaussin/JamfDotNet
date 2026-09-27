using System.Collections;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.Kiota.Abstractions;

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

    /// <summary>
    /// Maps a Kiota <see cref="ApiException"/> to <see cref="JamfApiException"/> for a unified catch surface.
    /// </summary>
    /// <param name="exception">Kiota API exception.</param>
    /// <returns>Equivalent <see cref="JamfApiException"/>.</returns>
    public static JamfApiException FromApiException(ApiException exception) =>
        FromApiException(exception, responseBody: null);

    /// <summary>
    /// Maps a Kiota <see cref="ApiException"/> to <see cref="JamfApiException"/>, optionally supplying a response body.
    /// </summary>
    /// <param name="exception">Kiota API exception.</param>
    /// <param name="responseBody">
    /// Optional response body. When null, the mapper attempts to extract a body from the exception
    /// (known property names, <see cref="Exception.Data"/>, or response headers).
    /// </param>
    /// <returns>Equivalent <see cref="JamfApiException"/>.</returns>
    public static JamfApiException FromApiException(ApiException exception, string? responseBody)
    {
        ArgumentNullException.ThrowIfNull(exception);

        HttpStatusCode? statusCode = exception.ResponseStatusCode is > 0
            ? (HttpStatusCode)exception.ResponseStatusCode
            : null;

        var body = responseBody ?? TryExtractResponseBody(exception);

        return new JamfApiException(
            string.IsNullOrWhiteSpace(exception.Message)
                ? "Jamf API request failed."
                : exception.Message,
            statusCode,
            body,
            exception);
    }

    private static string? TryExtractResponseBody(ApiException exception)
    {
        foreach (DictionaryEntry entry in exception.Data)
        {
            if (entry.Key is string key
                && entry.Value is string text
                && !string.IsNullOrWhiteSpace(text)
                && key.Contains("body", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }
        }

        foreach (var property in exception.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.PropertyType != typeof(string) || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (!property.Name.Contains("Body", StringComparison.OrdinalIgnoreCase)
                && !property.Name.Contains("Content", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                if (property.GetValue(exception) is string text && !string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
            catch
            {
                // Ignore reflection failures on exotic exception subtypes.
            }
        }

        if (exception.ResponseHeaders is { Count: > 0 })
        {
            var builder = new StringBuilder();
            foreach (var header in exception.ResponseHeaders)
            {
                builder.Append(header.Key).Append(": ").Append(string.Join(", ", header.Value)).AppendLine();
            }

            return builder.Length > 0 ? builder.ToString() : null;
        }

        return null;
    }
}
