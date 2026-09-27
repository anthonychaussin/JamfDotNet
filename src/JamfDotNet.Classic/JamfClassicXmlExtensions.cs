namespace JamfDotNet.Classic;

/// <summary>
/// Convenience Classic write helpers that post/put XML bodies built by <see cref="JamfClassicXml"/>.
/// Prefer Jamf Pro for new integrations when an equivalent endpoint exists.
/// </summary>
public static class JamfClassicXmlExtensions
{
    /// <summary>
    /// Creates a category via Classic <c>POST /categories/id/0</c> with an XML body.
    /// </summary>
    /// <param name="client">Classic client.</param>
    /// <param name="name">Category name.</param>
    /// <param name="priority">Optional priority.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task CreateCategoryAsync(
        this JamfClassicClient client,
        string name,
        int? priority = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        await using var stream = JamfClassicXml.ToStream(JamfClassicXml.Category(name, priority));
        await client.Api.Categories.Id[0].PostAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a category via Classic <c>PUT /categories/id/{id}</c> with an XML body.
    /// </summary>
    /// <param name="client">Classic client.</param>
    /// <param name="id">Category id.</param>
    /// <param name="name">Category name.</param>
    /// <param name="priority">Optional priority.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task UpdateCategoryAsync(
        this JamfClassicClient client,
        int id,
        string name,
        int? priority = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        await using var stream = JamfClassicXml.ToStream(JamfClassicXml.Category(name, priority));
        await client.Api.Categories.Id[id].PutAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a building via Classic <c>POST /buildings/id/0</c> with an XML body.
    /// </summary>
    /// <param name="client">Classic client.</param>
    /// <param name="name">Building name.</param>
    /// <param name="streetAddress1">Optional street address.</param>
    /// <param name="city">Optional city.</param>
    /// <param name="stateProvince">Optional state/province.</param>
    /// <param name="zipPostalCode">Optional postal code.</param>
    /// <param name="country">Optional country.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task CreateBuildingAsync(
        this JamfClassicClient client,
        string name,
        string? streetAddress1 = null,
        string? city = null,
        string? stateProvince = null,
        string? zipPostalCode = null,
        string? country = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        await using var stream = JamfClassicXml.ToStream(
            JamfClassicXml.Building(name, streetAddress1, city, stateProvince, zipPostalCode, country));
        await client.Api.Buildings.Id[0].PostAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a building via Classic <c>PUT /buildings/id/{id}</c> with an XML body.
    /// </summary>
    /// <param name="client">Classic client.</param>
    /// <param name="id">Building id.</param>
    /// <param name="name">Building name.</param>
    /// <param name="streetAddress1">Optional street address.</param>
    /// <param name="city">Optional city.</param>
    /// <param name="stateProvince">Optional state/province.</param>
    /// <param name="zipPostalCode">Optional postal code.</param>
    /// <param name="country">Optional country.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task UpdateBuildingAsync(
        this JamfClassicClient client,
        int id,
        string name,
        string? streetAddress1 = null,
        string? city = null,
        string? stateProvince = null,
        string? zipPostalCode = null,
        string? country = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        await using var stream = JamfClassicXml.ToStream(
            JamfClassicXml.Building(name, streetAddress1, city, stateProvince, zipPostalCode, country));
        await client.Api.Buildings.Id[id].PutAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
