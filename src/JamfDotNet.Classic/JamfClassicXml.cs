using System.Text;
using System.Xml.Linq;

namespace JamfDotNet.Classic;

/// <summary>
/// Minimal helpers for Classic API XML write bodies.
/// Prefer Jamf Pro (<c>JamfDotNet.Pro</c>) when an equivalent endpoint exists.
/// </summary>
public static class JamfClassicXml
{
    /// <summary>
    /// Converts an <see cref="XElement"/> to a UTF-8 XML stream suitable for Kiota <c>PostAsync</c>/<c>PutAsync</c>.
    /// </summary>
    /// <param name="element">Root XML element.</param>
    /// <returns>Memory stream positioned at the start (caller owns disposal).</returns>
    public static MemoryStream ToStream(XElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var stream = new MemoryStream();
        var settings = new System.Xml.XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false,
            Indent = false,
        };
        using (var writer = System.Xml.XmlWriter.Create(stream, settings))
        {
            element.Save(writer);
        }

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Converts a raw XML document string to a UTF-8 stream.
    /// </summary>
    /// <param name="xml">XML document text.</param>
    /// <returns>Memory stream positioned at the start (caller owns disposal).</returns>
    public static MemoryStream ToStream(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);
        var bytes = Encoding.UTF8.GetBytes(xml);
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// Builds a Classic <c>category</c> XML body.
    /// </summary>
    /// <param name="name">Category name.</param>
    /// <param name="priority">Optional display priority.</param>
    /// <returns>Root <c>category</c> element.</returns>
    public static XElement Category(string name, int? priority = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var root = new XElement("category", new XElement("name", name));
        if (priority is not null)
        {
            root.Add(new XElement("priority", priority.Value));
        }

        return root;
    }

    /// <summary>
    /// Builds a Classic <c>building</c> XML body.
    /// </summary>
    /// <param name="name">Building name.</param>
    /// <param name="streetAddress1">Optional street address.</param>
    /// <param name="city">Optional city.</param>
    /// <param name="stateProvince">Optional state/province.</param>
    /// <param name="zipPostalCode">Optional postal code.</param>
    /// <param name="country">Optional country.</param>
    /// <returns>Root <c>building</c> element.</returns>
    public static XElement Building(
        string name,
        string? streetAddress1 = null,
        string? city = null,
        string? stateProvince = null,
        string? zipPostalCode = null,
        string? country = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var root = new XElement("building", new XElement("name", name));
        AddIfPresent(root, "street_address1", streetAddress1);
        AddIfPresent(root, "city", city);
        AddIfPresent(root, "state_province", stateProvince);
        AddIfPresent(root, "zip_postal_code", zipPostalCode);
        AddIfPresent(root, "country", country);
        return root;
    }

    private static void AddIfPresent(XElement root, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            root.Add(new XElement(name, value));
        }
    }
}
