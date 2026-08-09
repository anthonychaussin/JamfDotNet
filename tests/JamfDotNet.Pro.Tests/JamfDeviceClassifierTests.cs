using JamfDotNet.Core;

namespace JamfDotNet.Pro.Tests;

public sealed class JamfDeviceClassifierTests
{
    [Theory]
    [InlineData("iPhone16,2", null, JamfDeviceKind.IPhone, JamfDeviceFamily.Mobile)]
    [InlineData("iPad14,1", "ios", JamfDeviceKind.IPad, JamfDeviceFamily.Mobile)]
    [InlineData("iPod9,1", null, JamfDeviceKind.IPod, JamfDeviceFamily.Mobile)]
    [InlineData("AppleTV14,1", "tvos", JamfDeviceKind.AppleTv, JamfDeviceFamily.Mobile)]
    [InlineData("Watch7,4", "watchos", JamfDeviceKind.AppleWatch, JamfDeviceFamily.Mobile)]
    [InlineData("RealityDevice14,1", "visionos", JamfDeviceKind.VisionPro, JamfDeviceFamily.Mobile)]
    [InlineData("Mac14,2", "Mac", JamfDeviceKind.Mac, JamfDeviceFamily.Computer)]
    [InlineData("MacBookPro18,3", null, JamfDeviceKind.Mac, JamfDeviceFamily.Computer)]
    [InlineData(null, "Windows", JamfDeviceKind.WindowsComputer, JamfDeviceFamily.Computer)]
    [InlineData(null, "ios", JamfDeviceKind.IosDevice, JamfDeviceFamily.Mobile)]
    [InlineData(null, null, JamfDeviceKind.Unknown, JamfDeviceFamily.Unknown)]
    public void Classify_Maps_Identifier_And_Type(
        string? modelIdentifier,
        string? typeOrPlatform,
        JamfDeviceKind expectedKind,
        JamfDeviceFamily expectedFamily)
    {
        var result = JamfDeviceClassifier.Classify(modelIdentifier, typeOrPlatform);
        Assert.Equal(expectedKind, result.Kind);
        Assert.Equal(expectedFamily, result.Family);
        Assert.Equal(modelIdentifier, result.ModelIdentifier);
    }

    [Fact]
    public void ModelIdentifier_Wins_Over_Generic_Ios_Type()
    {
        var result = JamfDeviceClassifier.Classify("iPhone15,2", "ios");
        Assert.Equal(JamfDeviceKind.IPhone, result.Kind);
        Assert.True(JamfDeviceClassifier.IsMobile(result.Kind));
        Assert.False(JamfDeviceClassifier.IsComputer(result.Kind));
    }
}
