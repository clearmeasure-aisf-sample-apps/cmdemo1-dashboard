using System.Text.Json;

namespace Dashboard.Health;

/// <summary>One regional node (web app) of a deployable.</summary>
/// <param name="Links">Where the node's numbers lead (<see cref="LinkSet"/>); null without links.</param>
public sealed record NodeInfo(string Name, string? Region, string Role, Uri Url, LinkSet? Links = null)
{
    public const string PrimaryRole = "primary";
    public const string StandbyRole = "standby";

    public bool IsPrimary => NodeRole.IsPrimary(Role);
}

/// <summary>
/// The one place that decides which role word names a primary node, and the one reader of a node's role and region
/// words: the topology's node and the manifest's node both get their role here, so the two readers cannot drift.
/// </summary>
public static class NodeRole
{
    /// <summary>The word a node's element uses for its role; the topology's node and the manifest's node both write it.</summary>
    public const string RoleWord = "role";

    /// <summary>The word a node's element uses for its region; the topology's node and the manifest's node both write it.</summary>
    public const string RegionWord = "region";

    /// <summary>
    /// The role word a node's element names, trimmed and lowercased; null when it names none. The one place of the
    /// lowercasing of a role, so the reader of <c>topology.json</c> and the reader of the manifest read the same word
    /// the same way.
    /// </summary>
    public static string? Read(JsonElement element) => JsonRead.Text(element, RoleWord)?.ToLowerInvariant();

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
