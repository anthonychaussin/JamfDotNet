using System.Text.Json;
using System.Text.Json.Serialization;

namespace JamfDotNet.School.Models;

/// <summary>Base envelope fields returned by many School API responses.</summary>
public class SchoolApiResponse
{
    /// <summary>HTTP-like status code from the School API payload.</summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>Additional properties not mapped to strongly typed members.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>School device summary.</summary>
public sealed class SchoolDevice
{
    /// <summary>Device UDID.</summary>
    [JsonPropertyName("UDID")]
    public string? Udid { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Asset tag.</summary>
    public string? AssetTag { get; set; }

    /// <summary>Location id.</summary>
    public int? LocationId { get; set; }

    /// <summary>Whether the device is in the trash.</summary>
    public bool? InTrash { get; set; }

    /// <summary>Whether the device is managed.</summary>
    public bool? IsManaged { get; set; }

    /// <summary>Whether the device is supervised.</summary>
    public bool? IsSupervised { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List devices response.</summary>
public sealed class SchoolDeviceListResponse : SchoolApiResponse
{
    /// <summary>Devices in the response.</summary>
    public IReadOnlyList<SchoolDevice>? Devices { get; set; }
}

/// <summary>Single device response.</summary>
public sealed class SchoolDeviceResponse : SchoolApiResponse
{
    /// <summary>Device payload.</summary>
    public SchoolDevice? Device { get; set; }
}

/// <summary>Device group summary.</summary>
public sealed class SchoolDeviceGroup
{
    /// <summary>Group id.</summary>
    public int? Id { get; set; }

    /// <summary>Group name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List device groups response.</summary>
public sealed class SchoolDeviceGroupListResponse : SchoolApiResponse
{
    /// <summary>Device groups.</summary>
    [JsonPropertyName("deviceGroups")]
    public IReadOnlyList<SchoolDeviceGroup>? DeviceGroups { get; set; }

    /// <summary>Alternate groups array used by some protocol versions.</summary>
    public IReadOnlyList<SchoolDeviceGroup>? Groups { get; set; }
}

/// <summary>Single device group response.</summary>
public sealed class SchoolDeviceGroupResponse : SchoolApiResponse
{
    /// <summary>Device group payload.</summary>
    public SchoolDeviceGroup? Group { get; set; }
}

/// <summary>School user summary.</summary>
public sealed class SchoolUser
{
    /// <summary>User id.</summary>
    public int? Id { get; set; }

    /// <summary>Username.</summary>
    public string? Username { get; set; }

    /// <summary>Email.</summary>
    public string? Email { get; set; }

    /// <summary>First name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Location id.</summary>
    public int? LocationId { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List users response.</summary>
public sealed class SchoolUserListResponse : SchoolApiResponse
{
    /// <summary>Users.</summary>
    public IReadOnlyList<SchoolUser>? Users { get; set; }
}

/// <summary>Single user response.</summary>
public sealed class SchoolUserResponse : SchoolApiResponse
{
    /// <summary>User payload.</summary>
    public SchoolUser? User { get; set; }
}

/// <summary>Payload used to create or update a School user.</summary>
public sealed class SchoolUserWriteRequest
{
    /// <summary>Username.</summary>
    public string? Username { get; set; }

    /// <summary>Email.</summary>
    public string? Email { get; set; }

    /// <summary>First name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Location id.</summary>
    public int? LocationId { get; set; }

    /// <summary>Password (when required by the School instance).</summary>
    public string? Password { get; set; }

    /// <summary>Additional properties forwarded as-is.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>User group summary.</summary>
public sealed class SchoolUserGroup
{
    /// <summary>Group id.</summary>
    public int? Id { get; set; }

    /// <summary>Group name.</summary>
    public string? Name { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List user groups response.</summary>
public sealed class SchoolUserGroupListResponse : SchoolApiResponse
{
    /// <summary>User groups.</summary>
    public IReadOnlyList<SchoolUserGroup>? Groups { get; set; }
}

/// <summary>Single user group response.</summary>
public sealed class SchoolUserGroupResponse : SchoolApiResponse
{
    /// <summary>User group payload.</summary>
    public SchoolUserGroup? Group { get; set; }
}

/// <summary>Class summary.</summary>
public sealed class SchoolClass
{
    /// <summary>Class UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Class name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List classes response.</summary>
public sealed class SchoolClassListResponse : SchoolApiResponse
{
    /// <summary>Classes.</summary>
    public IReadOnlyList<SchoolClass>? Classes { get; set; }
}

/// <summary>Single class response.</summary>
public sealed class SchoolClassResponse : SchoolApiResponse
{
    /// <summary>Class payload.</summary>
    [JsonPropertyName("class")]
    public SchoolClass? Class { get; set; }
}

/// <summary>Configuration profile summary.</summary>
public sealed class SchoolProfile
{
    /// <summary>Profile id.</summary>
    public int? Id { get; set; }

    /// <summary>Profile name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List profiles response.</summary>
public sealed class SchoolProfileListResponse : SchoolApiResponse
{
    /// <summary>Profiles.</summary>
    public IReadOnlyList<SchoolProfile>? Profiles { get; set; }
}

/// <summary>Single profile response.</summary>
public sealed class SchoolProfileResponse : SchoolApiResponse
{
    /// <summary>Profile payload.</summary>
    public SchoolProfile? Profile { get; set; }
}

/// <summary>App summary.</summary>
public sealed class SchoolApp
{
    /// <summary>App id.</summary>
    public int? Id { get; set; }

    /// <summary>App name.</summary>
    public string? Name { get; set; }

    /// <summary>Bundle identifier.</summary>
    public string? BundleId { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List apps response.</summary>
public sealed class SchoolAppListResponse : SchoolApiResponse
{
    /// <summary>Apps.</summary>
    public IReadOnlyList<SchoolApp>? Apps { get; set; }
}

/// <summary>Single app response.</summary>
public sealed class SchoolAppResponse : SchoolApiResponse
{
    /// <summary>App payload.</summary>
    public SchoolApp? App { get; set; }
}

/// <summary>Location summary.</summary>
public sealed class SchoolLocation
{
    /// <summary>Location id.</summary>
    public int? Id { get; set; }

    /// <summary>Location name.</summary>
    public string? Name { get; set; }

    /// <summary>Additional unmapped properties.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>List locations response.</summary>
public sealed class SchoolLocationListResponse : SchoolApiResponse
{
    /// <summary>Locations.</summary>
    public IReadOnlyList<SchoolLocation>? Locations { get; set; }
}

/// <summary>Single location response.</summary>
public sealed class SchoolLocationResponse : SchoolApiResponse
{
    /// <summary>Location payload.</summary>
    public SchoolLocation? Location { get; set; }
}

/// <summary>Payload used to create or update a School user group.</summary>
public sealed class SchoolUserGroupWriteRequest
{
    /// <summary>Group name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Additional properties forwarded as-is.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Payload used to create or update a School device group.</summary>
public sealed class SchoolDeviceGroupWriteRequest
{
    /// <summary>Group name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Additional properties forwarded as-is.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Payload used to create or update a School class.</summary>
public sealed class SchoolClassWriteRequest
{
    /// <summary>Class name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Additional properties forwarded as-is.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Payload used to create or update a School profile.</summary>
public sealed class SchoolProfileWriteRequest
{
    /// <summary>Profile name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Additional properties forwarded as-is.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Payload used to create or update a School app.</summary>
public sealed class SchoolAppWriteRequest
{
    /// <summary>App name.</summary>
    public string? Name { get; set; }

    /// <summary>Bundle identifier.</summary>
    public string? BundleId { get; set; }

    /// <summary>Additional properties forwarded as-is.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Payload used to create or update a School location.</summary>
public sealed class SchoolLocationWriteRequest
{
    /// <summary>Location name.</summary>
    public string? Name { get; set; }

    /// <summary>Additional properties forwarded as-is.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
