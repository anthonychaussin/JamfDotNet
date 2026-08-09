namespace JamfDotNet.Core;

/// <summary>
/// Classifies Jamf-managed devices into precise kinds (iPhone, iPad, Mac, Apple TV, …).
/// </summary>
/// <remarks>
/// Jamf mobile APIs typically expose an OS family (<c>ios</c>, <c>tvos</c>, …) where iPhone and iPad
/// both appear as <c>ios</c>. Prefer combining that family with Apple <c>modelIdentifier</c> prefixes.
/// </remarks>
public static class JamfDeviceClassifier
{
    /// <summary>
    /// Classifies a device from an Apple model identifier and an optional Jamf type / platform string.
    /// </summary>
    /// <param name="modelIdentifier">
    /// Hardware model identifier (for example <c>iPhone16,2</c>, <c>iPad14,1</c>, <c>Mac14,2</c>, <c>AppleTV14,1</c>).
    /// </param>
    /// <param name="deviceTypeOrPlatform">
    /// Optional Jamf value such as mobile <c>type</c>/<c>deviceType</c>
    /// (<c>ios</c>, <c>tvos</c>, <c>watchos</c>, <c>visionos</c>) or computer <c>platform</c>
    /// (<c>Mac</c>, <c>Windows</c>).
    /// </param>
    /// <returns>Classification result.</returns>
    public static JamfDeviceClassification Classify(string? modelIdentifier, string? deviceTypeOrPlatform = null)
    {
        var normalizedId = Normalize(modelIdentifier);
        var normalizedType = Normalize(deviceTypeOrPlatform);

        var fromId = ClassifyFromModelIdentifier(normalizedId);
        if (fromId != JamfDeviceKind.Unknown)
        {
            return new JamfDeviceClassification(fromId, FamilyOf(fromId), modelIdentifier, deviceTypeOrPlatform);
        }

        var fromType = ClassifyFromTypeOrPlatform(normalizedType);
        if (fromType != JamfDeviceKind.Unknown)
        {
            return new JamfDeviceClassification(fromType, FamilyOf(fromType), modelIdentifier, deviceTypeOrPlatform);
        }

        return new JamfDeviceClassification(JamfDeviceKind.Unknown, JamfDeviceFamily.Unknown, modelIdentifier, deviceTypeOrPlatform);
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="kind"/> is a computer (Mac or Windows).
    /// </summary>
    /// <param name="kind">Device kind.</param>
    /// <returns><see langword="true"/> for computer kinds.</returns>
    public static bool IsComputer(JamfDeviceKind kind) =>
        kind is JamfDeviceKind.Mac or JamfDeviceKind.WindowsComputer;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="kind"/> is a mobile-device inventory kind.
    /// </summary>
    /// <param name="kind">Device kind.</param>
    /// <returns><see langword="true"/> for mobile kinds.</returns>
    public static bool IsMobile(JamfDeviceKind kind) =>
        kind is JamfDeviceKind.IPhone
            or JamfDeviceKind.IPad
            or JamfDeviceKind.IPod
            or JamfDeviceKind.AppleTv
            or JamfDeviceKind.AppleWatch
            or JamfDeviceKind.VisionPro
            or JamfDeviceKind.IosDevice;

    private static JamfDeviceKind ClassifyFromModelIdentifier(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return JamfDeviceKind.Unknown;
        }

        if (id.StartsWith("iphone", StringComparison.Ordinal))
        {
            return JamfDeviceKind.IPhone;
        }

        if (id.StartsWith("ipad", StringComparison.Ordinal))
        {
            return JamfDeviceKind.IPad;
        }

        if (id.StartsWith("ipod", StringComparison.Ordinal))
        {
            return JamfDeviceKind.IPod;
        }

        if (id.StartsWith("appletv", StringComparison.Ordinal))
        {
            return JamfDeviceKind.AppleTv;
        }

        if (id.StartsWith("watch", StringComparison.Ordinal))
        {
            return JamfDeviceKind.AppleWatch;
        }

        if (id.StartsWith("realitydevice", StringComparison.Ordinal)
            || id.StartsWith("applevision", StringComparison.Ordinal))
        {
            return JamfDeviceKind.VisionPro;
        }

        if (id.StartsWith("mac", StringComparison.Ordinal)
            || id.StartsWith("imac", StringComparison.Ordinal)
            || id.StartsWith("macbook", StringComparison.Ordinal)
            || id.StartsWith("macpro", StringComparison.Ordinal)
            || id.StartsWith("macmini", StringComparison.Ordinal)
            || id.StartsWith("macstudio", StringComparison.Ordinal)
            || id.StartsWith("xserve", StringComparison.Ordinal))
        {
            return JamfDeviceKind.Mac;
        }

        return JamfDeviceKind.Unknown;
    }

    private static JamfDeviceKind ClassifyFromTypeOrPlatform(string? typeOrPlatform)
    {
        if (string.IsNullOrEmpty(typeOrPlatform))
        {
            return JamfDeviceKind.Unknown;
        }

        return typeOrPlatform switch
        {
            "tvos" or "appletv" or "apple tv" => JamfDeviceKind.AppleTv,
            "watchos" or "watch" => JamfDeviceKind.AppleWatch,
            "visionos" or "vision" or "visionpro" or "vision pro" => JamfDeviceKind.VisionPro,
            "ios" or "iphone os" => JamfDeviceKind.IosDevice,
            "mac" or "macos" => JamfDeviceKind.Mac,
            "windows" or "win" => JamfDeviceKind.WindowsComputer,
            _ => JamfDeviceKind.Unknown,
        };
    }

    private static JamfDeviceFamily FamilyOf(JamfDeviceKind kind) =>
        kind switch
        {
            JamfDeviceKind.Mac or JamfDeviceKind.WindowsComputer => JamfDeviceFamily.Computer,
            JamfDeviceKind.Unknown => JamfDeviceFamily.Unknown,
            _ => JamfDeviceFamily.Mobile,
        };

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }
}
