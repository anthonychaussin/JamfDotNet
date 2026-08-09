using JamfDotNet.Core;
using JamfDotNet.Pro.Generated.Models;

namespace JamfDotNet.Pro;

/// <summary>
/// Convenience classification helpers for Jamf Pro inventory models.
/// </summary>
public static class JamfProDeviceClassificationExtensions
{
    /// <summary>
    /// Classifies a mobile device list item using <see cref="MobileDeviceV2.Type"/> and <see cref="MobileDeviceV2.ModelIdentifier"/>.
    /// </summary>
    /// <param name="device">Mobile device.</param>
    /// <returns>Classification result.</returns>
    public static JamfDeviceClassification Classify(this MobileDeviceV2 device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return JamfDeviceClassifier.Classify(device.ModelIdentifier, device.Type?.ToString());
    }

    /// <summary>
    /// Classifies a computer inventory record using <c>general.platform</c> and <c>hardware.modelIdentifier</c>.
    /// </summary>
    /// <param name="computer">Computer inventory record.</param>
    /// <returns>Classification result.</returns>
    public static JamfDeviceClassification Classify(this ComputerInventoryV4 computer)
    {
        ArgumentNullException.ThrowIfNull(computer);
        return JamfDeviceClassifier.Classify(computer.Hardware?.ModelIdentifier, computer.General?.Platform);
    }

    /// <summary>
    /// Classifies a computer inventory record using <c>general.platform</c> and <c>hardware.modelIdentifier</c>.
    /// </summary>
    /// <param name="computer">Computer inventory record.</param>
    /// <returns>Classification result.</returns>
    public static JamfDeviceClassification Classify(this ComputerInventory computer)
    {
        ArgumentNullException.ThrowIfNull(computer);
        return JamfDeviceClassifier.Classify(computer.Hardware?.ModelIdentifier, computer.General?.Platform);
    }
}
