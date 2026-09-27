using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JamfDotNet.Core;
using JamfDotNet.Core.Http;
using JamfDotNet.School.Models;
using Microsoft.Extensions.Options;

namespace JamfDotNet.School;

/// <summary>
/// Typed HTTP client for the Jamf School REST API.
/// </summary>
/// <remarks>
/// When constructed via DI (<see cref="JamfSchoolServiceCollectionExtensions"/>),
/// <see cref="BasicAuthHandler"/> attaches credentials and the protocol version header.
/// Standalone <see cref="Create(JamfSchoolOptions, HttpClient?)"/> also applies those headers per request as a fallback.
/// </remarks>
public sealed class JamfSchoolClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly JamfSchoolOptions _options;
    private readonly bool _ownsHttpClient;
    private readonly bool _applyAuthPerRequest;
    private bool _disposed;

    /// <summary>
    /// Creates a School client.
    /// </summary>
    /// <param name="httpClient">HTTP client used for School API calls.</param>
    /// <param name="options">School connection options.</param>
    /// <param name="ownsHttpClient">When <see langword="true"/>, disposes <paramref name="httpClient"/>.</param>
    /// <param name="applyAuthPerRequest">
    /// When <see langword="true"/>, attaches Basic auth and protocol version on each request.
    /// Set to <see langword="false"/> when <see cref="BasicAuthHandler"/> is already registered on the pipeline.
    /// </param>
    public JamfSchoolClient(
        HttpClient httpClient,
        JamfSchoolOptions options,
        bool ownsHttpClient = false,
        bool applyAuthPerRequest = true)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
        _ownsHttpClient = ownsHttpClient;
        _applyAuthPerRequest = applyAuthPerRequest;
        Devices = new SchoolDevicesResource(this);
        DeviceGroups = new SchoolDeviceGroupsResource(this);
        Users = new SchoolUsersResource(this);
        Groups = new SchoolUserGroupsResource(this);
        Classes = new SchoolClassesResource(this);
        Profiles = new SchoolProfilesResource(this);
        Apps = new SchoolAppsResource(this);
        Locations = new SchoolLocationsResource(this);
    }

    /// <summary>Device inventory resource.</summary>
    public SchoolDevicesResource Devices { get; }

    /// <summary>Device groups resource.</summary>
    public SchoolDeviceGroupsResource DeviceGroups { get; }

    /// <summary>Users resource.</summary>
    public SchoolUsersResource Users { get; }

    /// <summary>User groups resource.</summary>
    public SchoolUserGroupsResource Groups { get; }

    /// <summary>Classes resource.</summary>
    public SchoolClassesResource Classes { get; }

    /// <summary>Profiles resource.</summary>
    public SchoolProfilesResource Profiles { get; }

    /// <summary>Apps resource.</summary>
    public SchoolAppsResource Apps { get; }

    /// <summary>Locations resource.</summary>
    public SchoolLocationsResource Locations { get; }

    /// <summary>
    /// Creates a client from options.
    /// </summary>
    /// <param name="options">School connection options.</param>
    /// <param name="httpClient">Optional HTTP client. When null, a dedicated client is created and owned.</param>
    /// <returns>Configured <see cref="JamfSchoolClient"/>.</returns>
    public static JamfSchoolClient Create(JamfSchoolOptions options, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        if (httpClient is null)
        {
            var handler = new BasicAuthHandler(Options.Create(options))
            {
                InnerHandler = new HttpClientHandler(),
            };
            httpClient = new HttpClient(handler);
            return new JamfSchoolClient(httpClient, options, ownsHttpClient: true, applyAuthPerRequest: false);
        }

        return new JamfSchoolClient(httpClient, options, ownsHttpClient: false, applyAuthPerRequest: true);
    }

    /// <summary>
    /// Creates a client from DI options (auth expected on the <see cref="HttpClient"/> pipeline).
    /// </summary>
    /// <param name="options">Options monitor.</param>
    /// <param name="httpClient">Named School HTTP client.</param>
    /// <returns>Configured <see cref="JamfSchoolClient"/>.</returns>
    public static JamfSchoolClient Create(IOptions<JamfSchoolOptions> options, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        return new JamfSchoolClient(httpClient, options.Value, ownsHttpClient: false, applyAuthPerRequest: false);
    }

    /// <summary>
    /// Escape hatch: GET a path and return the raw JSON document.
    /// </summary>
    /// <param name="relativePath">Path relative to <c>/api/</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed JSON document (caller owns disposal).</returns>
    public Task<JsonDocument> GetDocumentAsync(string relativePath, CancellationToken cancellationToken = default) =>
        GetDocumentInternalAsync(relativePath, cancellationToken);

    /// <summary>
    /// Escape hatch: send an arbitrary JSON request under <c>/api/</c> and return the raw JSON document.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="relativePath">Path relative to <c>/api/</c>.</param>
    /// <param name="body">Optional JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed JSON document (caller owns disposal).</returns>
    public async Task<JsonDocument> SendDocumentAsync(
        HttpMethod method,
        string relativePath,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);
        HttpContent? content = body is null ? null : JsonContent.Create(body, options: JsonOptions);
        using var response = await SendAsync(method, relativePath, content, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Jamf School {method} {relativePath} failed with status {(int)response.StatusCode}.",
                response.StatusCode,
                responseBody);
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return JsonDocument.Parse("{}");
        }

        return JsonDocument.Parse(responseBody);
    }

    internal async Task<T> GetAsync<T>(string relativePath, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, relativePath, content: null, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Jamf School GET {relativePath} failed with status {(int)response.StatusCode}.",
                response.StatusCode,
                body);
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
            ?? throw new JamfApiException($"Jamf School GET {relativePath} returned an empty body.", response.StatusCode, body);
    }

    internal async Task<T> SendJsonAsync<T>(HttpMethod method, string relativePath, object? body, CancellationToken cancellationToken)
    {
        HttpContent? content = body is null ? null : JsonContent.Create(body, options: JsonOptions);
        using var response = await SendAsync(method, relativePath, content, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Jamf School {method} {relativePath} failed with status {(int)response.StatusCode}.",
                response.StatusCode,
                responseBody);
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return Activator.CreateInstance<T>()
                ?? throw new JamfApiException($"Jamf School {method} {relativePath} returned an empty body.", response.StatusCode, responseBody);
        }

        return JsonSerializer.Deserialize<T>(responseBody, JsonOptions)
            ?? throw new JamfApiException($"Jamf School {method} {relativePath} returned an empty body.", response.StatusCode, responseBody);
    }

    internal async Task SendNoContentAsync(HttpMethod method, string relativePath, object? body, CancellationToken cancellationToken)
    {
        HttpContent? content = body is null ? null : JsonContent.Create(body, options: JsonOptions);
        using var response = await SendAsync(method, relativePath, content, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new JamfApiException(
                $"Jamf School {method} {relativePath} failed with status {(int)response.StatusCode}.",
                response.StatusCode,
                responseBody);
        }
    }

    internal async Task<JsonDocument> GetDocumentInternalAsync(string relativePath, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, relativePath, content: null, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Jamf School GET {relativePath} failed with status {(int)response.StatusCode}.",
                response.StatusCode,
                body);
        }

        return JsonDocument.Parse(body);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var uri = new Uri(_options.ApiBaseUrl, relativePath.TrimStart('/'));
        var request = new HttpRequestMessage(method, uri) { Content = content };
        if (_applyAuthPerRequest)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.NetworkId}:{_options.ApiKey}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            request.Headers.TryAddWithoutValidation("X-Server-Protocol-Version", _options.ProtocolVersion);
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}

/// <summary>Devices resource.</summary>
public sealed class SchoolDevicesResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolDevicesResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists devices.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Device list envelope.</returns>
    public Task<SchoolDeviceListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolDeviceListResponse>("devices", cancellationToken);

    /// <summary>Gets a device by UDID.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Device detail envelope.</returns>
    public Task<SchoolDeviceResponse> GetAsync(string udid, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolDeviceResponse>($"devices/{Uri.EscapeDataString(udid)}", cancellationToken);

    /// <summary>Lists devices as a raw JSON document.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Raw JSON (caller owns disposal).</returns>
    public Task<JsonDocument> ListDocumentAsync(CancellationToken cancellationToken = default) =>
        _client.GetDocumentAsync("devices", cancellationToken);

    /// <summary>Restarts a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task RestartAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "restart", cancellationToken);

    /// <summary>Wipes a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task WipeAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "wipe", cancellationToken);

    /// <summary>Locks a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task LockAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "lock", cancellationToken);

    /// <summary>Clears the passcode on a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task ClearPasscodeAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "clearpasscode", cancellationToken);

    /// <summary>Shuts down a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task ShutdownAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "shutdown", cancellationToken);

    /// <summary>Sends a blank push (check-in nudge) to a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task BlankPushAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "blankpush", cancellationToken);

    /// <summary>Updates inventory for a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task UpdateInventoryAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "updateinventory", cancellationToken);

    /// <summary>Enables lost mode on a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task EnableLostModeAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "lostmode", cancellationToken);

    /// <summary>Disables lost mode on a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DisableLostModeAsync(string udid, CancellationToken cancellationToken = default) =>
        SendDeviceCommandAsync(udid, "disablelostmode", cancellationToken);

    /// <summary>Sends an arbitrary MDM command path segment for a device.</summary>
    /// <param name="udid">Device UDID.</param>
    /// <param name="command">Command path segment under <c>/devices/{udid}/</c>.</param>
    /// <param name="body">Optional JSON body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task SendCommandAsync(
        string udid,
        string command,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(udid);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        return _client.SendNoContentAsync(
            HttpMethod.Post,
            $"devices/{Uri.EscapeDataString(udid)}/{command.Trim('/')}",
            body,
            cancellationToken);
    }

    private Task SendDeviceCommandAsync(string udid, string command, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(udid);
        return _client.SendNoContentAsync(HttpMethod.Post, $"devices/{Uri.EscapeDataString(udid)}/{command}", body: null, cancellationToken);
    }
}

/// <summary>Device groups resource.</summary>
public sealed class SchoolDeviceGroupsResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolDeviceGroupsResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists device groups.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Device group list envelope.</returns>
    public Task<SchoolDeviceGroupListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolDeviceGroupListResponse>("devices/groups", cancellationToken);

    /// <summary>Gets a device group by id.</summary>
    /// <param name="id">Group id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Device group envelope.</returns>
    public Task<SchoolDeviceGroupResponse> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolDeviceGroupResponse>($"devices/groups/{id}", cancellationToken);

    /// <summary>Creates a device group.</summary>
    /// <param name="request">Create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created device group envelope.</returns>
    public Task<SchoolDeviceGroupResponse> CreateAsync(SchoolDeviceGroupWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolDeviceGroupResponse>(HttpMethod.Post, "devices/groups", request, cancellationToken);
    }

    /// <summary>Updates a device group.</summary>
    /// <param name="id">Group id.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated device group envelope.</returns>
    public Task<SchoolDeviceGroupResponse> UpdateAsync(int id, SchoolDeviceGroupWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolDeviceGroupResponse>(HttpMethod.Put, $"devices/groups/{id}", request, cancellationToken);
    }

    /// <summary>Deletes a device group.</summary>
    /// <param name="id">Group id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _client.SendNoContentAsync(HttpMethod.Delete, $"devices/groups/{id}", body: null, cancellationToken);
}

/// <summary>Users resource.</summary>
public sealed class SchoolUsersResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolUsersResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists users.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User list envelope.</returns>
    public Task<SchoolUserListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolUserListResponse>("users", cancellationToken);

    /// <summary>Gets a user by id.</summary>
    /// <param name="id">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User envelope.</returns>
    public Task<SchoolUserResponse> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolUserResponse>($"users/{id}", cancellationToken);

    /// <summary>Creates a user.</summary>
    /// <param name="request">User create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user envelope.</returns>
    public Task<SchoolUserResponse> CreateAsync(SchoolUserWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolUserResponse>(HttpMethod.Post, "users", request, cancellationToken);
    }

    /// <summary>Updates a user.</summary>
    /// <param name="id">User id.</param>
    /// <param name="request">User update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user envelope.</returns>
    public Task<SchoolUserResponse> UpdateAsync(int id, SchoolUserWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolUserResponse>(HttpMethod.Put, $"users/{id}", request, cancellationToken);
    }

    /// <summary>Deletes a user.</summary>
    /// <param name="id">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _client.SendNoContentAsync(HttpMethod.Delete, $"users/{id}", body: null, cancellationToken);
}

/// <summary>User groups resource.</summary>
public sealed class SchoolUserGroupsResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolUserGroupsResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists user groups.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User group list envelope.</returns>
    public Task<SchoolUserGroupListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolUserGroupListResponse>("users/groups", cancellationToken);

    /// <summary>Gets a user group by id.</summary>
    /// <param name="id">Group id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User group envelope.</returns>
    public Task<SchoolUserGroupResponse> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolUserGroupResponse>($"users/groups/{id}", cancellationToken);

    /// <summary>Creates a user group.</summary>
    /// <param name="request">Create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user group envelope.</returns>
    public Task<SchoolUserGroupResponse> CreateAsync(SchoolUserGroupWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolUserGroupResponse>(HttpMethod.Post, "users/groups", request, cancellationToken);
    }

    /// <summary>Updates a user group.</summary>
    /// <param name="id">Group id.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user group envelope.</returns>
    public Task<SchoolUserGroupResponse> UpdateAsync(int id, SchoolUserGroupWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolUserGroupResponse>(HttpMethod.Put, $"users/groups/{id}", request, cancellationToken);
    }

    /// <summary>Deletes a user group.</summary>
    /// <param name="id">Group id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _client.SendNoContentAsync(HttpMethod.Delete, $"users/groups/{id}", body: null, cancellationToken);
}

/// <summary>Classes resource.</summary>
public sealed class SchoolClassesResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolClassesResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists classes.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Class list envelope.</returns>
    public Task<SchoolClassListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolClassListResponse>("classes", cancellationToken);

    /// <summary>Gets a class by UUID.</summary>
    /// <param name="uuid">Class UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Class envelope.</returns>
    public Task<SchoolClassResponse> GetAsync(string uuid, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolClassResponse>($"classes/{Uri.EscapeDataString(uuid)}", cancellationToken);

    /// <summary>Creates a class.</summary>
    /// <param name="request">Create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created class envelope.</returns>
    public Task<SchoolClassResponse> CreateAsync(SchoolClassWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolClassResponse>(HttpMethod.Post, "classes", request, cancellationToken);
    }

    /// <summary>Updates a class.</summary>
    /// <param name="uuid">Class UUID.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated class envelope.</returns>
    public Task<SchoolClassResponse> UpdateAsync(string uuid, SchoolClassWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolClassResponse>(HttpMethod.Put, $"classes/{Uri.EscapeDataString(uuid)}", request, cancellationToken);
    }

    /// <summary>Deletes a class.</summary>
    /// <param name="uuid">Class UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(string uuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        return _client.SendNoContentAsync(HttpMethod.Delete, $"classes/{Uri.EscapeDataString(uuid)}", body: null, cancellationToken);
    }
}

/// <summary>Profiles resource.</summary>
public sealed class SchoolProfilesResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolProfilesResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists profiles.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Profile list envelope.</returns>
    public Task<SchoolProfileListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolProfileListResponse>("profiles", cancellationToken);

    /// <summary>Gets a profile by id.</summary>
    /// <param name="id">Profile id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Profile envelope.</returns>
    public Task<SchoolProfileResponse> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolProfileResponse>($"profiles/{id}", cancellationToken);

    /// <summary>Creates a profile.</summary>
    /// <param name="request">Create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created profile envelope.</returns>
    public Task<SchoolProfileResponse> CreateAsync(SchoolProfileWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolProfileResponse>(HttpMethod.Post, "profiles", request, cancellationToken);
    }

    /// <summary>Updates a profile.</summary>
    /// <param name="id">Profile id.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated profile envelope.</returns>
    public Task<SchoolProfileResponse> UpdateAsync(int id, SchoolProfileWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolProfileResponse>(HttpMethod.Put, $"profiles/{id}", request, cancellationToken);
    }

    /// <summary>Deletes a profile.</summary>
    /// <param name="id">Profile id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _client.SendNoContentAsync(HttpMethod.Delete, $"profiles/{id}", body: null, cancellationToken);
}

/// <summary>Apps resource.</summary>
public sealed class SchoolAppsResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolAppsResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists apps.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>App list envelope.</returns>
    public Task<SchoolAppListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolAppListResponse>("apps", cancellationToken);

    /// <summary>Gets an app by id.</summary>
    /// <param name="id">App id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>App envelope.</returns>
    public Task<SchoolAppResponse> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolAppResponse>($"apps/{id}", cancellationToken);

    /// <summary>Creates an app.</summary>
    /// <param name="request">Create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created app envelope.</returns>
    public Task<SchoolAppResponse> CreateAsync(SchoolAppWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolAppResponse>(HttpMethod.Post, "apps", request, cancellationToken);
    }

    /// <summary>Updates an app.</summary>
    /// <param name="id">App id.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated app envelope.</returns>
    public Task<SchoolAppResponse> UpdateAsync(int id, SchoolAppWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolAppResponse>(HttpMethod.Put, $"apps/{id}", request, cancellationToken);
    }

    /// <summary>Deletes an app.</summary>
    /// <param name="id">App id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _client.SendNoContentAsync(HttpMethod.Delete, $"apps/{id}", body: null, cancellationToken);
}

/// <summary>Locations resource.</summary>
public sealed class SchoolLocationsResource
{
    private readonly JamfSchoolClient _client;

    internal SchoolLocationsResource(JamfSchoolClient client) => _client = client;

    /// <summary>Lists locations.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Location list envelope.</returns>
    public Task<SchoolLocationListResponse> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolLocationListResponse>("locations", cancellationToken);

    /// <summary>Gets a location by id.</summary>
    /// <param name="id">Location id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Location envelope.</returns>
    public Task<SchoolLocationResponse> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<SchoolLocationResponse>($"locations/{id}", cancellationToken);

    /// <summary>Creates a location.</summary>
    /// <param name="request">Create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created location envelope.</returns>
    public Task<SchoolLocationResponse> CreateAsync(SchoolLocationWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolLocationResponse>(HttpMethod.Post, "locations", request, cancellationToken);
    }

    /// <summary>Updates a location.</summary>
    /// <param name="id">Location id.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated location envelope.</returns>
    public Task<SchoolLocationResponse> UpdateAsync(int id, SchoolLocationWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.SendJsonAsync<SchoolLocationResponse>(HttpMethod.Put, $"locations/{id}", request, cancellationToken);
    }

    /// <summary>Deletes a location.</summary>
    /// <param name="id">Location id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _client.SendNoContentAsync(HttpMethod.Delete, $"locations/{id}", body: null, cancellationToken);
}
