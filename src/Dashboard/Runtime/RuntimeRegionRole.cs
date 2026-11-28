using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>
/// The roles a deployment writes for a region into the manifest (<see cref="RuntimeRegion.Roles"/>), in the
/// deployment's own spelling. A region keeps whatever words it is handed; these are the four the page knows, and any
/// other word is shown as the deployment wrote it. <see cref="Primary"/> and <see cref="Standby"/> are the node role words of
/// <see cref="NodeInfo"/> (<see cref="NodeInfo.PrimaryRole"/>, <see cref="NodeInfo.StandbyRole"/>): a region's roles and
/// a node's role are the same deployment words, spelled once.
/// </summary>
public static class RuntimeRegionRole
{
    /// <summary>The region whose apps take the traffic.</summary>
    public const string Primary = NodeInfo.PrimaryRole;

    /// <summary>The region whose apps stand by for the traffic.</summary>
    public const string Standby = NodeInfo.StandbyRole;

    /// <summary>The region of the database.</summary>
    public const string Data = "data";

    /// <summary>The region of the static sites.</summary>
    public const string Static = "static";

    /// <summary>The page word of the <see cref="Data"/> role, spelled once so <see cref="DatabaseReachable"/> can't drift from it.</summary>
    private const string DatabaseWords = "database";

    /// <summary>The separator between two unprobed roles in <see cref="NotProbed"/>.</summary>
    private const string RoleSeparator = ", ";

    /// <summary>The tail that says the named roles are the ones nothing in the region probes.</summary>
    private const string NotProbedTail = ": not probed";

    /// <summary>The separator between the reachable-database clause and the unprobed roles in <see cref="Unprobed"/>.</summary>
    private const string ClauseSeparator = "; ";

    /// <summary>
    /// The words for a role on the region's box, where nothing in the region is probed: <see cref="DatabaseWords"/> for
    /// <see cref="Data"/>, "static sites" for <see cref="Static"/>, any other role as it is written.
    /// </summary>
    public static string Words(string role) => role switch
    {
        Data => DatabaseWords,
        Static => "static sites",
        _ => role,
    };

    /// <summary>
    /// What a region with no web app says when its database answered a health check: the <see cref="DatabaseWords"/>
    /// page word and the reachable tail.
    /// </summary>
    public const string DatabaseReachable = DatabaseWords + ": reachable";

    /// <summary>
    /// What a region with no web app says when there is nothing to say: no roles left to name and no database
    /// reachable. Today that is the empty label, kept as it reaches the page.
    /// </summary>
    public const string NothingToSay = "";

    /// <summary>
    /// The roles nothing in the region probes, in the page's words and in the deployment's order, joined with
    /// <see cref="RoleSeparator"/> and closed with <see cref="NotProbedTail"/>.
    /// </summary>
    public static string NotProbed(IEnumerable<string> roles) => string.Join(RoleSeparator, roles.Select(Words)) + NotProbedTail;

    /// <summary>
    /// The label of a region with no web app: <see cref="DatabaseReachable"/> when its database is reachable, then the
    /// remaining roles as <see cref="NotProbed"/>, joined with <see cref="ClauseSeparator"/>; <see cref="NothingToSay"/>
    /// when neither applies. A <see cref="Data"/> role is left out of the unprobed roles once the database is said to be
    /// reachable.
    /// </summary>
    public static string Unprobed(IEnumerable<string> roles, bool databaseReachable)
    {
        var others = roles.Where(role => !(databaseReachable && role == Data)).ToList();
        return (databaseReachable, others.Count > 0) switch
        {
            (true, true) => DatabaseReachable + ClauseSeparator + NotProbed(others),
            (true, false) => DatabaseReachable,
            (false, true) => NotProbed(others),
            (false, false) => NothingToSay,
        };
    }
}
