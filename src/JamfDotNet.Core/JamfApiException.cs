using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
    /// <param name="errors">Optional structured error messages (JSON <c>errors</c> / GraphQL).</param>
    /// <param name="retryAfter">Optional <c>Retry-After</c> delay.</param>
    /// <param name="rateLimitLimit">Optional <c>X-RateLimit-Limit</c> (or equivalent) header.</param>
    /// <param name="rateLimitRemaining">Optional <c>X-RateLimit-Remaining</c> header.</param>
    public JamfApiException(
        string message,
        HttpStatusCode? statusCode = null,
        string? responseBody = null,
        Exception? innerException = null,
        IReadOnlyList<string>? errors = null,
        TimeSpan? retryAfter = null,
        string? rateLimitLimit = null,
        string? rateLimitRemaining = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Errors = errors ?? Array.Empty<string>();
        RetryAfter = retryAfter;
        RateLimitLimit = rateLimitLimit;
        RateLimitRemaining = rateLimitRemaining;
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
    /// Structured error messages extracted from JSON bodies or GraphQL <c>errors</c>.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// <c>Retry-After</c> delay when present on the failing response.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Rate-limit ceiling header value when present.
    /// </summary>
    public string? RateLimitLimit { get; }

    /// <summary>
    /// Rate-limit remaining header value when present.
    /// </summary>
    public string? RateLimitRemaining { get; }

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
        var errors = TryParseErrorMessages(body);
        var (retryAfter, limit, remaining) = TryExtractRateLimit(exception.ResponseHeaders);

        return new JamfApiException(
            string.IsNullOrWhiteSpace(exception.Message)
                ? "Jamf API request failed."
                : exception.Message,
            statusCode,
            body,
            exception,
            errors,
            retryAfter,
            limit,
            remaining);
    }

    /// <summary>
    /// Creates an exception from an HTTP response (status, body, rate-limit headers).
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="response">HTTP response.</param>
    /// <param name="responseBody">Response body.</param>
    /// <param name="innerException">Optional inner exception.</param>
    /// <returns>Configured <see cref="JamfApiException"/>.</returns>
    public static JamfApiException FromHttpResponse(
        string message,
        HttpResponseMessage response,
        string? responseBody,
        Exception? innerException = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        var (retryAfter, limit, remaining) = TryExtractRateLimit(response.Headers, response.Content?.Headers);
        return new JamfApiException(
            message,
            response.StatusCode,
            responseBody,
            innerException,
            TryParseErrorMessages(responseBody),
            retryAfter,
            limit,
            remaining);
    }

    /// <summary>
    /// Creates an exception for a GraphQL payload that contains an <c>errors</c> array.
    /// </summary>
    /// <param name="responseBody">Raw GraphQL JSON body.</param>
    /// <param name="statusCode">HTTP status when available.</param>
    /// <returns>Configured <see cref="JamfApiException"/>.</returns>
    public static JamfApiException FromGraphQlErrors(string responseBody, HttpStatusCode? statusCode = null)
    {
        var errors = TryParseErrorMessages(responseBody);
        var message = errors.Count > 0
            ? string.Join("; ", errors)
            : "GraphQL response contained errors.";
        return new JamfApiException(message, statusCode, responseBody, errors: errors);
    }

    /// <summary>
    /// Extracts human-readable messages from Jamf JSON / GraphQL error payloads.
    /// </summary>
    /// <param name="responseBody">Raw response body.</param>
    /// <returns>Parsed messages (empty when none found).</returns>
    public static IReadOnlyList<string> TryParseErrorMessages(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return Array.Empty<string>();
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var messages = new List<string>();

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in errors.EnumerateArray())
                {
                    if (error.ValueKind == JsonValueKind.String)
                    {
                        var text = error.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            messages.Add(text);
                        }
                    }
                    else if (error.ValueKind == JsonValueKind.Object
                             && error.TryGetProperty("message", out var message)
                             && message.ValueKind == JsonValueKind.String)
                    {
                        var text = message.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            messages.Add(text);
                        }
                    }
                }
            }

            if (messages.Count == 0
                && root.TryGetProperty("httpStatus", out _)
                && root.TryGetProperty("errors", out _))
            {
                // Already handled above; Jamf Pro error shape.
            }

            if (messages.Count == 0
                && root.TryGetProperty("message", out var single)
                && single.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(single.GetString()))
            {
                messages.Add(single.GetString()!);
            }

            return messages;
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static (TimeSpan? RetryAfter, string? Limit, string? Remaining) TryExtractRateLimit(
        IDictionary<string, IEnumerable<string>>? headers)
    {
        if (headers is null || headers.Count == 0)
        {
            return (null, null, null);
        }

        TimeSpan? retryAfter = null;
        if (headers.TryGetValue("Retry-After", out var retryValues))
        {
            retryAfter = ParseRetryAfter(retryValues.FirstOrDefault());
        }

        string? limit = null;
        string? remaining = null;
        foreach (var header in headers)
        {
            if (header.Key.Equals("X-RateLimit-Limit", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("RateLimit-Limit", StringComparison.OrdinalIgnoreCase))
            {
                limit = header.Value.FirstOrDefault();
            }
            else if (header.Key.Equals("X-RateLimit-Remaining", StringComparison.OrdinalIgnoreCase)
                     || header.Key.Equals("RateLimit-Remaining", StringComparison.OrdinalIgnoreCase))
            {
                remaining = header.Value.FirstOrDefault();
            }
        }

        return (retryAfter, limit, remaining);
    }

    private static (TimeSpan? RetryAfter, string? Limit, string? Remaining) TryExtractRateLimit(
        HttpResponseHeaders headers,
        HttpContentHeaders? contentHeaders)
    {
        TimeSpan? retryAfter = null;
        if (headers.RetryAfter?.Delta is TimeSpan delta)
        {
            retryAfter = delta;
        }
        else if (headers.RetryAfter?.Date is DateTimeOffset date)
        {
            var until = date - DateTimeOffset.UtcNow;
            retryAfter = until > TimeSpan.Zero ? until : TimeSpan.Zero;
        }

        string? limit = null;
        string? remaining = null;
        if (headers.TryGetValues("X-RateLimit-Limit", out var limitValues)
            || headers.TryGetValues("RateLimit-Limit", out limitValues))
        {
            limit = limitValues.FirstOrDefault();
        }

        if (headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues)
            || headers.TryGetValues("RateLimit-Remaining", out remainingValues))
        {
            remaining = remainingValues.FirstOrDefault();
        }

        _ = contentHeaders;
        return (retryAfter, limit, remaining);
    }

    private static TimeSpan? ParseRetryAfter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, out var seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }

        if (DateTimeOffset.TryParse(value, out var date))
        {
            var until = date - DateTimeOffset.UtcNow;
            return until > TimeSpan.Zero ? until : TimeSpan.Zero;
        }

        return null;
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
