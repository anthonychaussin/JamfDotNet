using System.Diagnostics;

namespace JamfDotNet.Core.Diagnostics;

/// <summary>
/// Shared diagnostics primitives for JamfDotNet (ActivitySource for OpenTelemetry bridging).
/// </summary>
public static class JamfDiagnostics
{
    /// <summary>
    /// Activity source name consumers should register (for example <c>AddSource("JamfDotNet")</c>).
    /// </summary>
    public const string ActivitySourceName = "JamfDotNet";

    /// <summary>
    /// Shared <see cref="ActivitySource"/> for JamfDotNet instrumentation.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(ActivitySourceName, "1.0.0");
}
