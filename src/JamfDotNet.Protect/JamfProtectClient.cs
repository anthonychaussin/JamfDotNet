using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using JamfDotNet.Core.Http;
using JamfDotNet.Protect.Models;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Protect;

/// <summary>
/// Typed client for the Jamf Protect GraphQL API.
/// <para>
/// This is the Protect product GraphQL surface (<c>{base}/app</c>), not the Jamf Pro
/// <c>/v1/jamf-protect</c> registration endpoints.
/// </para>
/// </summary>
public sealed class JamfProtectClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly Uri _graphQlEndpoint;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>
    /// Creates a Protect client.
    /// </summary>
    /// <param name="httpClient">HTTP client (bearer auth expected on the pipeline or already configured).</param>
    /// <param name="graphQlEndpoint">GraphQL endpoint (<c>{base}/app</c>).</param>
    /// <param name="ownsHttpClient">When <see langword="true"/>, disposes <paramref name="httpClient"/>.</param>
    public JamfProtectClient(HttpClient httpClient, Uri graphQlEndpoint, bool ownsHttpClient = false)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _graphQlEndpoint = graphQlEndpoint ?? throw new ArgumentNullException(nameof(graphQlEndpoint));
        _ownsHttpClient = ownsHttpClient;
    }

    /// <summary>
    /// Creates a client from options and a token provider.
    /// </summary>
    /// <param name="options">Protect options.</param>
    /// <param name="tokenProvider">Token provider used to build a bearer handler when <paramref name="httpClient"/> is null.</param>
    /// <param name="httpClient">Optional HTTP client. When null, a client with <see cref="BearerAuthHandler"/> is created.</param>
    /// <returns>Configured <see cref="JamfProtectClient"/>.</returns>
    public static JamfProtectClient Create(JamfProtectOptions options, IJamfTokenProvider tokenProvider, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tokenProvider);
        options.Validate();
        var owns = httpClient is null;
        if (httpClient is null)
        {
            httpClient = new HttpClient(new BearerAuthHandler(tokenProvider) { InnerHandler = new HttpClientHandler() });
        }

        return new JamfProtectClient(httpClient, options.GraphQlEndpoint, owns);
    }

    /// <summary>
    /// Creates a client from DI options (HTTP client should already include bearer auth).
    /// </summary>
    /// <param name="options">Options monitor.</param>
    /// <param name="httpClient">Named Protect API HTTP client.</param>
    /// <returns>Configured <see cref="JamfProtectClient"/>.</returns>
    public static JamfProtectClient Create(IOptions<JamfProtectOptions> options, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        options.Value.Validate();
        return new JamfProtectClient(httpClient, options.Value.GraphQlEndpoint, ownsHttpClient: false);
    }

    /// <summary>
    /// Escape hatch: execute an arbitrary GraphQL document and return the raw JSON payload.
    /// </summary>
    /// <param name="query">GraphQL query or mutation document.</param>
    /// <param name="variables">Optional variables object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed JSON document (caller owns disposal).</returns>
    public async Task<JsonDocument> ExecuteAsync(string query, object? variables = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        var payload = await SendGraphQlAsync(query, variables, cancellationToken).ConfigureAwait(false);
        return JsonDocument.Parse(payload);
    }

    /// <summary>Lists Protect RBAC roles.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Role connection.</returns>
    public Task<ProtectConnection<ProtectRole>> ListRolesAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListRoles($input: RoleQueryInput) {
              listRoles(input: $input) {
                items { id name created updated }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectRole>(query, "listRoles", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists Protect plans.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Plan connection.</returns>
    public Task<ProtectConnection<ProtectPlan>> ListPlansAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListPlans($input: PlanQueryInput) {
              listPlans(input: $input) {
                items { id uuid name description created updated profileVersion }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectPlan>(query, "listPlans", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists analytic sets.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Analytic set connection.</returns>
    public Task<ProtectConnection<ProtectAnalyticSet>> ListAnalyticSetsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListAnalyticSets($input: AnalyticSetsQueryInput) {
              listAnalyticSets(input: $input) {
                items { uuid name description created updated managed }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectAnalyticSet>(query, "listAnalyticSets", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists Protect computers (devices).</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Computer connection.</returns>
    public Task<ProtectConnection<ProtectComputer>> ListComputersAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListComputers($input: ComputerQueryInput) {
              listComputers(input: $input) {
                items { uuid serial hostName osString version checkin created updated }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectComputer>(query, "listComputers", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists USB control sets.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>USB control set connection.</returns>
    public Task<ProtectConnection<ProtectUsbControlSet>> ListUsbControlSetsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListUsbControlSets($input: USBControlSetsQueryInput) {
              listUSBControlSets(input: $input) {
                items { id name description created updated }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectUsbControlSet>(query, "listUSBControlSets", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists prevent lists.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prevent list connection.</returns>
    public Task<ProtectConnection<ProtectPreventList>> ListPreventListsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListPreventLists($input: PreventListQueryInput) {
              listPreventLists(input: $input) {
                items { id name description type count created }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectPreventList>(query, "listPreventLists", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists alerts.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Alert connection.</returns>
    public Task<ProtectConnection<ProtectAlert>> ListAlertsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListAlerts($input: AlertQueryInput!) {
              listAlerts(input: $input) {
                items { uuid severity status created updated computer { uuid serial hostName } }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectAlert>(query, "listAlerts", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists groups.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Group connection.</returns>
    public Task<ProtectConnection<ProtectGroup>> ListGroupsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListGroups($input: GroupQueryInput) {
              listGroups(input: $input) {
                items { id name created updated }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectGroup>(query, "listGroups", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists API clients.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>API client connection.</returns>
    public Task<ProtectConnection<ProtectApiClient>> ListApiClientsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListApiClients($input: ApiClientQueryInput) {
              listApiClients(input: $input) {
                items { clientId name created }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectApiClient>(query, "listApiClients", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists action configurations.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Action config connection.</returns>
    public Task<ProtectConnection<ProtectActionConfig>> ListActionConfigsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListActionConfigs($input: ActionConfigsQueryInput) {
              listActionConfigs(input: $input) {
                items { id name description created updated }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectActionConfig>(query, "listActionConfigs", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists exception sets.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Exception set connection.</returns>
    public Task<ProtectConnection<ProtectExceptionSet>> ListExceptionSetsAsync(int? pageSize = null, string? next = null, CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListExceptionSets($input: ExceptionSetsQueryInput) {
              listExceptionSets(input: $input) {
                items { uuid name description created updated managed }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectExceptionSet>(query, "listExceptionSets", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Gets a single alert by UUID.</summary>
    /// <param name="uuid">Alert UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Alert, or <see langword="null"/> when not found.</returns>
    public Task<ProtectAlert?> GetAlertAsync(string uuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        const string query = """
            query GetAlert($uuid: ID!) {
              getAlert(uuid: $uuid) {
                uuid severity status created updated computer { uuid serial hostName }
              }
            }
            """;
        return QueryObjectAsync<ProtectAlert>(query, "getAlert", new { uuid }, cancellationToken);
    }

    /// <summary>Gets a single computer by UUID.</summary>
    /// <param name="uuid">Computer UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Computer, or <see langword="null"/> when not found.</returns>
    public Task<ProtectComputer?> GetComputerAsync(string uuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        const string query = """
            query GetComputer($uuid: ID!) {
              getComputer(uuid: $uuid) {
                uuid serial hostName osString version checkin created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectComputer>(query, "getComputer", new { uuid }, cancellationToken);
    }

    /// <summary>Gets a single plan by id.</summary>
    /// <param name="id">Plan id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Plan, or <see langword="null"/> when not found.</returns>
    public Task<ProtectPlan?> GetPlanAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        const string query = """
            query GetPlan($id: ID!) {
              getPlan(id: $id) {
                id uuid name description created updated profileVersion
              }
            }
            """;
        return QueryObjectAsync<ProtectPlan>(query, "getPlan", new { id }, cancellationToken);
    }

    /// <summary>Updates alert statuses.</summary>
    /// <param name="uuids">Alert UUIDs to update.</param>
    /// <param name="status">Target status (<c>New</c>, <c>InProgress</c>, <c>Resolved</c>, or <c>AutoResolved</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated alert connection.</returns>
    public Task<ProtectConnection<ProtectAlert>> UpdateAlertsAsync(
        IEnumerable<string> uuids,
        string status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uuids);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        var uuidList = uuids as IList<string> ?? uuids.ToList();
        if (uuidList.Count == 0)
        {
            throw new ArgumentException("At least one alert UUID is required.", nameof(uuids));
        }

        const string query = """
            mutation UpdateAlerts($input: AlertUpdateInput!) {
              updateAlerts(input: $input) {
                items { uuid severity status created updated computer { uuid serial hostName } }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectAlert>(
            query,
            "updateAlerts",
            new { input = new { uuids = uuidList, status } },
            cancellationToken);
    }

    /// <summary>Creates a Protect group.</summary>
    /// <param name="name">Group name.</param>
    /// <param name="roleIds">Optional role ids.</param>
    /// <param name="accessGroup">Whether the group is an access group.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created group.</returns>
    public async Task<ProtectGroup> CreateGroupAsync(
        string name,
        IEnumerable<string>? roleIds = null,
        bool? accessGroup = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        const string query = """
            mutation CreateGroup($input: GroupCreateInput!) {
              createGroup(input: $input) {
                id name created updated
              }
            }
            """;
        var group = await QueryObjectAsync<ProtectGroup>(
            query,
            "createGroup",
            new { input = new { name, roleIds, accessGroup } },
            cancellationToken).ConfigureAwait(false);
        return group ?? throw new JamfApiException("Protect createGroup returned no group.", System.Net.HttpStatusCode.OK);
    }

    /// <summary>Deletes a Protect group.</summary>
    /// <param name="id">Group id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted group summary when returned by the API.</returns>
    public Task<ProtectGroup?> DeleteGroupAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        const string query = """
            mutation DeleteGroup($id: ID!) {
              deleteGroup(id: $id) {
                id name created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectGroup>(query, "deleteGroup", new { id }, cancellationToken);
    }

    /// <summary>Gets the Protect organization.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Organization summary, or <see langword="null"/> when absent.</returns>
    public Task<ProtectOrganization?> GetOrganizationAsync(CancellationToken cancellationToken = default)
    {
        const string query = """
            query GetOrganization {
              getOrganization {
                uuid configFreeze
              }
            }
            """;
        return QueryObjectAsync<ProtectOrganization>(query, "getOrganization", new { }, cancellationToken);
    }

    /// <summary>Updates organization configuration freeze.</summary>
    /// <param name="configFreeze">Whether configuration freeze is enabled.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated organization.</returns>
    public Task<ProtectOrganization?> UpdateOrganizationConfigFreezeAsync(
        bool configFreeze,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            mutation UpdateOrganizationConfigFreeze($input: ConfigFreezeInput!) {
              updateOrganizationConfigFreeze(input: $input) {
                uuid configFreeze
              }
            }
            """;
        return QueryObjectAsync<ProtectOrganization>(
            query,
            "updateOrganizationConfigFreeze",
            new { input = new { configFreeze } },
            cancellationToken);
    }

    /// <summary>Gets a Protect role by id.</summary>
    /// <param name="id">Role id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Role, or <see langword="null"/> when not found.</returns>
    public Task<ProtectRole?> GetRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        const string query = """
            query GetRole($id: ID!) {
              getRole(id: $id) {
                id name created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectRole>(query, "getRole", new { id }, cancellationToken);
    }

    /// <summary>Creates a Protect role.</summary>
    /// <param name="request">Role write payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created role.</returns>
    public async Task<ProtectRole> CreateRoleAsync(ProtectRoleWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        const string query = """
            mutation CreateRole($input: RoleInput!) {
              createRole(input: $input) {
                id name created updated
              }
            }
            """;
        var role = await QueryObjectAsync<ProtectRole>(
            query,
            "createRole",
            new
            {
                input = new
                {
                    name = request.Name,
                    readResources = request.ReadResources,
                    writeResources = request.WriteResources,
                },
            },
            cancellationToken).ConfigureAwait(false);
        return role ?? throw new JamfApiException("Protect createRole returned no role.", System.Net.HttpStatusCode.OK);
    }

    /// <summary>Updates a Protect role.</summary>
    /// <param name="id">Role id.</param>
    /// <param name="request">Role write payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated role.</returns>
    public Task<ProtectRole?> UpdateRoleAsync(string id, ProtectRoleWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        const string query = """
            mutation UpdateRole($id: ID!, $input: RoleInput!) {
              updateRole(id: $id, input: $input) {
                id name created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectRole>(
            query,
            "updateRole",
            new
            {
                id,
                input = new
                {
                    name = request.Name,
                    readResources = request.ReadResources,
                    writeResources = request.WriteResources,
                },
            },
            cancellationToken);
    }

    /// <summary>Deletes a Protect role.</summary>
    /// <param name="id">Role id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted role summary when returned.</returns>
    public Task<ProtectRole?> DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        const string query = """
            mutation DeleteRole($id: ID!) {
              deleteRole(id: $id) {
                id name created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectRole>(query, "deleteRole", new { id }, cancellationToken);
    }

    /// <summary>Lists Protect users.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User connection.</returns>
    public Task<ProtectConnection<ProtectUser>> ListUsersAsync(
        int? pageSize = null,
        string? next = null,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListUsers($input: UserQueryInput) {
              listUsers(input: $input) {
                items { id email sub source receiveEmailAlert emailAlertMinSeverity lastLogin created updated }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectUser>(query, "listUsers", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Gets a Protect user by id.</summary>
    /// <param name="id">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User, or <see langword="null"/> when not found.</returns>
    public Task<ProtectUser?> GetUserAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        const string query = """
            query GetUser($id: ID!) {
              getUser(id: $id) {
                id email sub source receiveEmailAlert emailAlertMinSeverity lastLogin created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectUser>(query, "getUser", new { id }, cancellationToken);
    }

    /// <summary>Creates a Protect user.</summary>
    /// <param name="request">User create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user.</returns>
    public async Task<ProtectUser> CreateUserAsync(ProtectUserCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        const string query = """
            mutation CreateUser($input: UserCreateInput!) {
              createUser(input: $input) {
                id email sub source receiveEmailAlert emailAlertMinSeverity lastLogin created updated
              }
            }
            """;
        var user = await QueryObjectAsync<ProtectUser>(
            query,
            "createUser",
            new
            {
                input = new
                {
                    email = request.Email,
                    connectionId = request.ConnectionId,
                    roleIds = request.RoleIds,
                    groupIds = request.GroupIds,
                    receiveEmailAlert = request.ReceiveEmailAlert,
                    emailAlertMinSeverity = request.EmailAlertMinSeverity,
                },
            },
            cancellationToken).ConfigureAwait(false);
        return user ?? throw new JamfApiException("Protect createUser returned no user.", System.Net.HttpStatusCode.OK);
    }

    /// <summary>Updates a Protect user.</summary>
    /// <param name="id">User id.</param>
    /// <param name="request">User update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user.</returns>
    public Task<ProtectUser?> UpdateUserAsync(string id, ProtectUserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        const string query = """
            mutation UpdateUser($id: ID!, $input: UserUpdateInput!) {
              updateUser(id: $id, input: $input) {
                id email sub source receiveEmailAlert emailAlertMinSeverity lastLogin created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectUser>(
            query,
            "updateUser",
            new
            {
                id,
                input = new
                {
                    roleIds = request.RoleIds,
                    groupIds = request.GroupIds,
                    receiveEmailAlert = request.ReceiveEmailAlert,
                    emailAlertMinSeverity = request.EmailAlertMinSeverity,
                },
            },
            cancellationToken);
    }

    /// <summary>Deletes a Protect user.</summary>
    /// <param name="id">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted user summary when returned.</returns>
    public Task<ProtectUser?> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        const string query = """
            mutation DeleteUser($id: ID!) {
              deleteUser(id: $id) {
                id email sub source receiveEmailAlert created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectUser>(query, "deleteUser", new { id }, cancellationToken);
    }

    /// <summary>Lists Protect insights.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Insight list.</returns>
    public async Task<IReadOnlyList<ProtectInsight>> ListInsightsAsync(CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListInsights {
              listInsights {
                uuid label description section totalPass totalFail totalNone enabled
              }
            }
            """;
        var json = await SendGraphQlAsync(query, variables: null, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data)
            || !data.TryGetProperty("listInsights", out var element))
        {
            throw new JamfApiException("Protect GraphQL response did not include data.listInsights.", System.Net.HttpStatusCode.OK, json);
        }

        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return Array.Empty<ProtectInsight>();
        }

        return element.Deserialize<List<ProtectInsight>>(JsonOptions) ?? [];
    }

    /// <summary>Lists Protect telemetry v2 configurations.</summary>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Telemetry connection.</returns>
    public Task<ProtectConnection<ProtectTelemetryV2>> ListTelemetriesV2Async(
        int? pageSize = null,
        string? next = null,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListTelemetriesV2($input: TelemetryQueryInput) {
              listTelemetriesV2(input: $input) {
                items { id name description created updated }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectTelemetryV2>(query, "listTelemetriesV2", new { input = new { pageSize, next } }, cancellationToken);
    }

    /// <summary>Lists audit logs for a date range.</summary>
    /// <param name="startDate">Inclusive start (UTC).</param>
    /// <param name="endDate">Inclusive end (UTC).</param>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="next">Optional pagination cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Audit log connection.</returns>
    public Task<ProtectConnection<ProtectAuditLog>> ListAuditLogsByDateAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        int? pageSize = null,
        string? next = null,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            query ListAuditLogsByDate($input: AuditLogsDateQueryInput) {
              listAuditLogsByDate(input: $input) {
                items { date op user resourceId error ips }
                pageInfo { next total }
              }
            }
            """;
        return QueryConnectionAsync<ProtectAuditLog>(
            query,
            "listAuditLogsByDate",
            new
            {
                input = new
                {
                    next,
                    pageSize,
                    condition = new
                    {
                        dateRange = new
                        {
                            startDate,
                            endDate,
                        },
                    },
                },
            },
            cancellationToken);
    }

    /// <summary>Gets alert status counts.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Status count response.</returns>
    public Task<ProtectAlertStatusCountResponse?> GetAlertStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        const string query = """
            query GetAlertStatusCounts {
              getAlertStatusCounts {
                New InProgress Resolved AutoResolved
              }
            }
            """;
        return QueryObjectAsync<ProtectAlertStatusCountResponse>(query, "getAlertStatusCounts", new { }, cancellationToken);
    }

    /// <summary>Creates a Protect plan.</summary>
    /// <param name="request">Plan write payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created plan.</returns>
    public async Task<ProtectPlan> CreatePlanAsync(ProtectPlanWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        const string query = """
            mutation CreatePlan($input: PlanInput!) {
              createPlan(input: $input) {
                id uuid name description created updated profileVersion
              }
            }
            """;
        var plan = await QueryObjectAsync<ProtectPlan>(
            query,
            "createPlan",
            new { input = ToPlanInput(request) },
            cancellationToken).ConfigureAwait(false);
        return plan ?? throw new JamfApiException("Protect createPlan returned no plan.", System.Net.HttpStatusCode.OK);
    }

    /// <summary>Updates a Protect plan.</summary>
    /// <param name="id">Plan id.</param>
    /// <param name="request">Plan write payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated plan.</returns>
    public Task<ProtectPlan?> UpdatePlanAsync(string id, ProtectPlanWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        const string query = """
            mutation UpdatePlan($id: ID!, $input: PlanInput!) {
              updatePlan(id: $id, input: $input) {
                id uuid name description created updated profileVersion
              }
            }
            """;
        return QueryObjectAsync<ProtectPlan>(query, "updatePlan", new { id, input = ToPlanInput(request) }, cancellationToken);
    }

    /// <summary>Deletes a Protect plan.</summary>
    /// <param name="id">Plan id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted plan summary when returned.</returns>
    public Task<ProtectPlan?> DeletePlanAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        const string query = """
            mutation DeletePlan($id: ID!) {
              deletePlan(id: $id) {
                id uuid name description created updated profileVersion
              }
            }
            """;
        return QueryObjectAsync<ProtectPlan>(query, "deletePlan", new { id }, cancellationToken);
    }

    /// <summary>Assigns a plan to a computer.</summary>
    /// <param name="computerUuid">Computer UUID.</param>
    /// <param name="planId">Plan id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated computer.</returns>
    public Task<ProtectComputer?> SetComputerPlanAsync(
        string computerUuid,
        string planId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(computerUuid);
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);
        const string query = """
            mutation SetComputerPlan($uuid: ID!, $input: ComputerPlan!) {
              setComputerPlan(uuid: $uuid, input: $input) {
                uuid serial hostName osString version checkin created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectComputer>(
            query,
            "setComputerPlan",
            new { uuid = computerUuid, input = new { plan = planId } },
            cancellationToken);
    }

    /// <summary>Updates editable computer fields.</summary>
    /// <param name="uuid">Computer UUID.</param>
    /// <param name="label">Optional label.</param>
    /// <param name="tags">Optional tags.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated computer.</returns>
    public Task<ProtectComputer?> UpdateComputerAsync(
        string uuid,
        string? label = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        const string query = """
            mutation UpdateComputer($uuid: ID!, $input: ComputerEditableInput!) {
              updateComputer(uuid: $uuid, input: $input) {
                uuid serial hostName osString version checkin created updated
              }
            }
            """;
        return QueryObjectAsync<ProtectComputer>(
            query,
            "updateComputer",
            new { uuid, input = new { label, tags } },
            cancellationToken);
    }

    /// <summary>Deletes a Protect computer.</summary>
    /// <param name="uuid">Computer UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted computer summary when returned.</returns>
    public Task<ProtectComputer?> DeleteComputerAsync(string uuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        const string query = """
            mutation DeleteComputer($uuid: ID!) {
              deleteComputer(uuid: $uuid) {
                uuid serial hostName
              }
            }
            """;
        return QueryObjectAsync<ProtectComputer>(query, "deleteComputer", new { uuid }, cancellationToken);
    }

    private static object ToPlanInput(ProtectPlanWriteRequest request) => new
    {
        actionConfigs = request.ActionConfigsId,
        description = request.Description,
        name = request.Name,
        logLevel = request.LogLevel,
        exceptionSets = request.ExceptionSets,
        telemetry = request.TelemetryId,
        telemetryV2 = request.TelemetryV2Id,
        autoUpdate = request.AutoUpdate,
        usbControlSet = request.UsbControlSetId,
        threatPreventionStrategy = request.ThreatPreventionStrategy,
    };

    private async Task<ProtectConnection<T>> QueryConnectionAsync<T>(
        string query,
        string fieldName,
        object variables,
        CancellationToken cancellationToken)
    {
        var json = await SendGraphQlAsync(query, variables, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data)
            || !data.TryGetProperty(fieldName, out var connectionElement))
        {
            throw new JamfApiException($"Protect GraphQL response did not include data.{fieldName}.", System.Net.HttpStatusCode.OK, json);
        }

        var connection = connectionElement.Deserialize<ProtectConnection<T>>(JsonOptions)
            ?? new ProtectConnection<T>();
        return connection;
    }

    private async Task<T?> QueryObjectAsync<T>(
        string query,
        string fieldName,
        object variables,
        CancellationToken cancellationToken)
        where T : class
    {
        var json = await SendGraphQlAsync(query, variables, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data)
            || !data.TryGetProperty(fieldName, out var element))
        {
            throw new JamfApiException($"Protect GraphQL response did not include data.{fieldName}.", System.Net.HttpStatusCode.OK, json);
        }

        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return element.Deserialize<T>(JsonOptions);
    }

    private async Task<string> SendGraphQlAsync(string query, object? variables, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _graphQlEndpoint);
        request.Content = JsonContent.Create(new GraphQlRequest(query, variables), options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Protect GraphQL request failed with status {(int)response.StatusCode}.",
                response.StatusCode,
                body);
        }

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.TryGetProperty("errors", out var errors)
            && errors.ValueKind == JsonValueKind.Array
            && errors.GetArrayLength() > 0)
        {
            throw new JamfApiException("Protect GraphQL response contained errors.", response.StatusCode, body);
        }

        return body;
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

    private sealed record GraphQlRequest(
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("variables")] object? Variables);
}
