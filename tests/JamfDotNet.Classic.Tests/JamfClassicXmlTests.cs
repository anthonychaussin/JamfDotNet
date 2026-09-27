using System.Xml.Linq;
using JamfDotNet.Classic;

namespace JamfDotNet.Classic.Tests;

public sealed class JamfClassicXmlTests
{
    [Fact]
    public void Category_Builds_Expected_Xml()
    {
        var xml = JamfClassicXml.Category("Apps", priority: 9).ToString(SaveOptions.DisableFormatting);
        Assert.Equal("<category><name>Apps</name><priority>9</priority></category>", xml);
    }

    [Fact]
    public void Building_Omits_Empty_Optional_Fields()
    {
        var xml = JamfClassicXml.Building("HQ", city: "Cupertino").ToString(SaveOptions.DisableFormatting);
        Assert.Equal("<building><name>HQ</name><city>Cupertino</city></building>", xml);
    }

    [Fact]
    public void ToStream_Writes_Utf8_Xml()
    {
        using var stream = JamfClassicXml.ToStream(JamfClassicXml.Category("Apps"));
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();
        Assert.Contains("<category>", text);
        Assert.Contains("<name>Apps</name>", text);
    }
}
