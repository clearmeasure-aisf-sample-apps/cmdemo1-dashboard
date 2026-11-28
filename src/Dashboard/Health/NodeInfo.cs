namespace Dashboard.Health;

/// <summary>One regional node (web app) of a deployable.</summary>
/// <param name="Links">Where the node's numbers lead (<see cref="LinkSet"/>); null without links.</param>
public sealed record NodeInfo(string Name, string? Region, string Role, Uri Url, LinkSet? Links = null)
{
    public const string PrimaryRole = "primary";
    public const string StandbyRole = "standby";

    public bool IsPrimary => NodeRole.IsPrimary(Role);
}

/// <summary>The one place that decides which role word names a primary node and reads it from a file.</summary>
public static class NodeRole
{
    /// <summary>The element word of a node's role.</summary>
    internal const string RoleWord = "role";

    /// <summary>The element word of a node's region.</summary>
    internal const string RegionWord = "region";

    /// <summary>
    /// The file's role word in lower case, null when there is none: the one reading of a node's role, so the topology
    /// and the runtime manifest cannot disagree about the same node. The word is trimmed by <see cref="JsonRead.Text"/>
    /// before it is lowered; no space is trimmed again, and no word at all (or a blank one) is no role.
    /// <see cref="Default"/> still decides for a missing role where the caller wants a fallback.
    /// </summary>
    public static string? Read(System.Text.Json.JsonElement element) => JsonRead.Text(element, RoleWord)?.ToLowerInvariant();

    /// <summary>
    /// Whether <paramref name="role"/> is <see cref="NodeInfo.PrimaryRole"/>: the whole word in any case, as written,
    /// with no space trimmed; no word at all is no primary.
    /// </summary>
    public static bool IsPrimary(string? role) => string.Equals(role, NodeInfo.PrimaryRole, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The role of a node that names none: <see cref="NodeInfo.PrimaryRole"/> when no node was read before it,
    /// <see cref="NodeInfo.StandbyRole"/> otherwise. The count is of the nodes that were read, not of the entries the
    /// file refused, so a refused first entry does not leave the deployable without a primary.
    /// </summary>
    public static string Default(int nodesReadBefore) =>
        nodesReadBefore == 0 ? NodeInfo.PrimaryRole : NodeInfo.StandbyRole;
}
