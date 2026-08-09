using Microsoft.Kiota.Abstractions;

namespace JamfDotNet.Core;

/// <summary>
/// Maps Kiota <see cref="ApiException"/> failures to <see cref="JamfApiException"/>.
/// </summary>
public static class JamfKiotaExceptionExtensions
{
    /// <summary>
    /// Awaits a Kiota call and remaps <see cref="ApiException"/> to <see cref="JamfApiException"/>.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="task">Kiota task.</param>
    /// <returns>The task result.</returns>
    /// <exception cref="JamfApiException">Thrown when the inner task throws <see cref="ApiException"/>.</exception>
    public static async Task<T> AsJamfApiAsync<T>(this Task<T> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            throw JamfApiException.FromApiException(ex);
        }
    }

    /// <summary>
    /// Awaits a Kiota call and remaps <see cref="ApiException"/> to <see cref="JamfApiException"/>.
    /// </summary>
    /// <param name="task">Kiota task.</param>
    /// <exception cref="JamfApiException">Thrown when the inner task throws <see cref="ApiException"/>.</exception>
    public static async Task AsJamfApiAsync(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            throw JamfApiException.FromApiException(ex);
        }
    }
}
