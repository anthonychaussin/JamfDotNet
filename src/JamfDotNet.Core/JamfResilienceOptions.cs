namespace JamfDotNet.Core;

/// <summary>
/// Options controlling <see cref="Http.JamfResilienceHandler"/> retry behaviour.
/// </summary>
public sealed class JamfResilienceOptions
{
    /// <summary>
    /// Configuration section name (<c>Jamf:Resilience</c>).
    /// </summary>
    public const string SectionName = "Jamf:Resilience";

    /// <summary>
    /// Maximum retries for HTTP 429 / 503 (default 2).
    /// </summary>
    public int MaxTransientRetries { get; set; } = 2;

    /// <summary>
    /// Base delay for exponential backoff when <c>Retry-After</c> is absent (default 250 ms).
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// When <see langword="true"/> (default), a single 401 triggers token invalidation and one retry.
    /// </summary>
    public bool EnableUnauthorizedRefresh { get; set; } = true;
}
