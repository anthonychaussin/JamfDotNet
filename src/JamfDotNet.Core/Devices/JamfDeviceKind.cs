namespace JamfDotNet.Core;

/// <summary>
/// Coarse device family used by Jamf product surfaces (computers vs mobile).
/// </summary>
public enum JamfDeviceFamily
{
    /// <summary>Family could not be determined.</summary>
    Unknown = 0,

    /// <summary>Computer inventory (Mac or Windows).</summary>
    Computer,

    /// <summary>Mobile-device inventory (iPhone, iPad, Apple TV, Watch, Vision, …).</summary>
    Mobile,
}

/// <summary>
/// Precise Apple / computer device kind inferred from Jamf fields and Apple model identifiers.
/// </summary>
public enum JamfDeviceKind
{
    /// <summary>Kind could not be determined.</summary>
    Unknown = 0,

    /// <summary>macOS computer.</summary>
    Mac,

    /// <summary>Windows computer enrolled in Jamf Pro.</summary>
    WindowsComputer,

    /// <summary>iPhone.</summary>
    IPhone,

    /// <summary>iPad.</summary>
    IPad,

    /// <summary>iPod touch.</summary>
    IPod,

    /// <summary>Apple TV.</summary>
    AppleTv,

    /// <summary>Apple Watch.</summary>
    AppleWatch,

    /// <summary>Apple Vision Pro.</summary>
    VisionPro,

    /// <summary>
    /// Jamf reports an iOS-family device, but the model identifier did not distinguish iPhone / iPad / iPod.
    /// </summary>
    IosDevice,
}

/// <summary>
/// Result of classifying a Jamf-managed device.
/// </summary>
/// <param name="Kind">Precise device kind.</param>
/// <param name="Family">Computer vs mobile family.</param>
/// <param name="ModelIdentifier">Apple / hardware model identifier when provided.</param>
/// <param name="SourceType">Raw Jamf <c>type</c>, <c>deviceType</c>, or <c>platform</c> value when provided.</param>
public sealed record JamfDeviceClassification(
    JamfDeviceKind Kind,
    JamfDeviceFamily Family,
    string? ModelIdentifier,
    string? SourceType);
