namespace Dashboard.Runtime;

/// <summary>
/// The roles a deployment writes for a region into the manifest (<see cref="RuntimeRegion.Roles"/>), in the
/// deployment's own spelling. A region keeps whatever words it is handed; these are the four the page knows, and any
/// other word is shown as the deployment wrote it.
/// </summary>
public static class RuntimeRegionRole
{
    /// <summary>The region whose apps take the traffic.</summary>
    public const string Primary = "primary";

    /// <summary>The region whose apps stand by for the traffic.</summary>
    public const string Standby = "standby";

    /// <summary>The region of the database.</summary>
    public const string Data = "data";

    /// <summary>The region of the static sites.</summary>
    public const string Static = "static";

    /// <summary>
    /// The words for a role on the region's box, where nothing in the region is probed: "database" for
    /// <see cref="Data"/>, "static sites" for <see cref="Static"/>, any other role as it is written.
    /// </summary>
    public static string Words(string role) => role switch
    {
        Data => "database",
        Static => "static sites",
        _ => role,
    };
}
