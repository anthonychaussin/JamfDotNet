namespace JamfDotNet.Protect.Models;

/// <summary>GraphQL connection page metadata.</summary>
public sealed class ProtectPageInfo
{
    /// <summary>Opaque cursor for the next page.</summary>
    public string? Next { get; set; }

    /// <summary>Total item count when provided by the API.</summary>
    public int? Total { get; set; }
}

/// <summary>Generic Protect connection envelope.</summary>
public sealed class ProtectConnection<T>
{
    /// <summary>Items in the current page.</summary>
    public IReadOnlyList<T>? Items { get; set; }

    /// <summary>Pagination info.</summary>
    public ProtectPageInfo? PageInfo { get; set; }
}

/// <summary>Protect RBAC role summary.</summary>
public sealed class ProtectRole
{
    /// <summary>Role id.</summary>
    public string? Id { get; set; }

    /// <summary>Role name.</summary>
    public string? Name { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }
}

/// <summary>Protect plan summary.</summary>
public sealed class ProtectPlan
{
    /// <summary>Plan id.</summary>
    public string? Id { get; set; }

    /// <summary>Plan UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Plan name.</summary>
    public string? Name { get; set; }

    /// <summary>Plan description.</summary>
    public string? Description { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>Profile version.</summary>
    public int? ProfileVersion { get; set; }
}

/// <summary>Protect analytic set summary.</summary>
public sealed class ProtectAnalyticSet
{
    /// <summary>Set UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Set name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>Whether the set is managed.</summary>
    public bool? Managed { get; set; }
}

/// <summary>Protect computer (device) summary.</summary>
public sealed class ProtectComputer
{
    /// <summary>Computer UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Serial number.</summary>
    public string? Serial { get; set; }

    /// <summary>Host name.</summary>
    public string? HostName { get; set; }

    /// <summary>OS string.</summary>
    public string? OsString { get; set; }

    /// <summary>Agent version.</summary>
    public string? Version { get; set; }

    /// <summary>Last check-in.</summary>
    public string? Checkin { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }
}

/// <summary>USB control set summary.</summary>
public sealed class ProtectUsbControlSet
{
    /// <summary>Set id.</summary>
    public string? Id { get; set; }

    /// <summary>Set name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }
}

/// <summary>Prevent list summary.</summary>
public sealed class ProtectPreventList
{
    /// <summary>List id.</summary>
    public string? Id { get; set; }

    /// <summary>List name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>List type.</summary>
    public string? Type { get; set; }

    /// <summary>Entry count.</summary>
    public int? Count { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }
}

/// <summary>Protect alert summary.</summary>
public sealed class ProtectAlert
{
    /// <summary>Alert UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Severity.</summary>
    public string? Severity { get; set; }

    /// <summary>Status.</summary>
    public string? Status { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>Related computer summary when present.</summary>
    public ProtectComputer? Computer { get; set; }
}

/// <summary>Protect group summary.</summary>
public sealed class ProtectGroup
{
    /// <summary>Group id.</summary>
    public string? Id { get; set; }

    /// <summary>Group name.</summary>
    public string? Name { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }
}

/// <summary>Protect API client summary.</summary>
public sealed class ProtectApiClient
{
    /// <summary>API client id.</summary>
    public string? ClientId { get; set; }

    /// <summary>API client name.</summary>
    public string? Name { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }
}

/// <summary>Protect action configuration summary.</summary>
public sealed class ProtectActionConfig
{
    /// <summary>Action config id.</summary>
    public string? Id { get; set; }

    /// <summary>Name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }
}

/// <summary>Protect exception set summary.</summary>
public sealed class ProtectExceptionSet
{
    /// <summary>Exception set UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>Whether the set is managed.</summary>
    public bool? Managed { get; set; }
}

/// <summary>Protect organization summary.</summary>
public sealed class ProtectOrganization
{
    /// <summary>Organization UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Whether configuration freeze is enabled.</summary>
    public bool? ConfigFreeze { get; set; }
}

/// <summary>Protect user summary.</summary>
public sealed class ProtectUser
{
    /// <summary>User id.</summary>
    public string? Id { get; set; }

    /// <summary>Email.</summary>
    public string? Email { get; set; }

    /// <summary>Subject claim.</summary>
    public string? Sub { get; set; }

    /// <summary>User source.</summary>
    public string? Source { get; set; }

    /// <summary>Whether the user receives email alerts.</summary>
    public bool? ReceiveEmailAlert { get; set; }

    /// <summary>Minimum severity for email alerts.</summary>
    public string? EmailAlertMinSeverity { get; set; }

    /// <summary>Last login timestamp.</summary>
    public DateTimeOffset? LastLogin { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }
}

/// <summary>Protect insight summary.</summary>
public sealed class ProtectInsight
{
    /// <summary>Insight UUID.</summary>
    public string? Uuid { get; set; }

    /// <summary>Label.</summary>
    public string? Label { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Section.</summary>
    public string? Section { get; set; }

    /// <summary>Pass count.</summary>
    public int? TotalPass { get; set; }

    /// <summary>Fail count.</summary>
    public int? TotalFail { get; set; }

    /// <summary>None count.</summary>
    public int? TotalNone { get; set; }

    /// <summary>Whether the insight is enabled.</summary>
    public bool? Enabled { get; set; }
}

/// <summary>Protect telemetry (v2) configuration summary.</summary>
public sealed class ProtectTelemetryV2
{
    /// <summary>Telemetry id.</summary>
    public string? Id { get; set; }

    /// <summary>Name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Created timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Updated timestamp.</summary>
    public DateTimeOffset? Updated { get; set; }
}

/// <summary>Protect audit log entry.</summary>
public sealed class ProtectAuditLog
{
    /// <summary>Event timestamp.</summary>
    public DateTimeOffset? Date { get; set; }

    /// <summary>Operation name.</summary>
    public string? Op { get; set; }

    /// <summary>User identity.</summary>
    public string? User { get; set; }

    /// <summary>Resource id.</summary>
    public string? ResourceId { get; set; }

    /// <summary>Error message when present.</summary>
    public string? Error { get; set; }

    /// <summary>Client IP addresses.</summary>
    public string? Ips { get; set; }
}

/// <summary>Response for <c>getAlertStatusCounts</c>.</summary>
public sealed class ProtectAlertStatusCountResponse
{
    /// <summary>Count of alerts with status New.</summary>
    public int? New { get; set; }

    /// <summary>Count of alerts with status InProgress.</summary>
    public int? InProgress { get; set; }

    /// <summary>Count of alerts with status Resolved.</summary>
    public int? Resolved { get; set; }

    /// <summary>Count of alerts with status AutoResolved.</summary>
    public int? AutoResolved { get; set; }
}

/// <summary>Payload used to create or update a Protect plan.</summary>
public sealed class ProtectPlanWriteRequest
{
    /// <summary>Plan name.</summary>
    public required string Name { get; set; }

    /// <summary>Plan description.</summary>
    public required string Description { get; set; }

    /// <summary>Action config id.</summary>
    public required string ActionConfigsId { get; set; }

    /// <summary>Optional log level.</summary>
    public string? LogLevel { get; set; }

    /// <summary>Exception set ids.</summary>
    public IReadOnlyList<string>? ExceptionSets { get; set; }

    /// <summary>Telemetry id.</summary>
    public string? TelemetryId { get; set; }

    /// <summary>Telemetry v2 id.</summary>
    public string? TelemetryV2Id { get; set; }

    /// <summary>Whether plans auto-update.</summary>
    public bool? AutoUpdate { get; set; }

    /// <summary>USB control set id.</summary>
    public string? UsbControlSetId { get; set; }

    /// <summary>Threat prevention strategy.</summary>
    public string? ThreatPreventionStrategy { get; set; }
}

/// <summary>Payload used to create a Protect user.</summary>
public sealed class ProtectUserCreateRequest
{
    /// <summary>Email address.</summary>
    public required string Email { get; set; }

    /// <summary>Whether the user receives email alerts.</summary>
    public bool ReceiveEmailAlert { get; set; }

    /// <summary>Optional connection id.</summary>
    public string? ConnectionId { get; set; }

    /// <summary>Optional role ids.</summary>
    public IReadOnlyList<string>? RoleIds { get; set; }

    /// <summary>Optional group ids.</summary>
    public IReadOnlyList<string>? GroupIds { get; set; }

    /// <summary>Optional minimum email alert severity.</summary>
    public string? EmailAlertMinSeverity { get; set; }
}

/// <summary>Payload used to update a Protect user.</summary>
public sealed class ProtectUserUpdateRequest
{
    /// <summary>Whether the user receives email alerts.</summary>
    public bool ReceiveEmailAlert { get; set; }

    /// <summary>Optional role ids.</summary>
    public IReadOnlyList<string>? RoleIds { get; set; }

    /// <summary>Optional group ids.</summary>
    public IReadOnlyList<string>? GroupIds { get; set; }

    /// <summary>Optional minimum email alert severity.</summary>
    public string? EmailAlertMinSeverity { get; set; }
}

/// <summary>Payload used to create or update a Protect role.</summary>
public sealed class ProtectRoleWriteRequest
{
    /// <summary>Role name.</summary>
    public string? Name { get; set; }

    /// <summary>Read resource privileges.</summary>
    public required IReadOnlyList<string> ReadResources { get; set; }

    /// <summary>Write resource privileges.</summary>
    public required IReadOnlyList<string> WriteResources { get; set; }
}
