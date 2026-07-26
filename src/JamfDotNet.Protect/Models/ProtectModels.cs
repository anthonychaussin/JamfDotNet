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
