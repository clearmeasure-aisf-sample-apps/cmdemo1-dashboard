using Dashboard.Work;

namespace Dashboard.Runtime;

public enum DashboardView
{
    /// <summary>The tiles of every environment: the default.</summary>
    Health,

    /// <summary>One environment's runtime diagram, updated live.</summary>
    Runtime,

    /// <summary>The cluster the system runs in: the AKS service, its nodes and its pods. Only for a topology with a cluster.</summary>
    Cluster,

    /// <summary>The work items the system's bots are handling. The view is there even before its file is published.</summary>
    WorkItems,
}

/// <summary>
/// The view in the page's address (its hash, without <c>#</c>): <c>runtime</c> or <c>runtime/&lt;environment&gt;</c> is the
/// runtime view, <c>runtime/&lt;environment&gt;/database</c> the database view of that environment (the runtime view,
/// zoomed into its database), <c>runtime/&lt;environment&gt;/app/&lt;deployable&gt;</c> the application view of that
/// deployable there (the runtime view, zoomed into one of its boxes), <c>cluster</c> the cluster view,
/// <c>work-items</c> the work items view, <c>work-items/bot</c> and <c>work-items/team</c> that view filtered to bot
/// or team work, anything else the health view. A link with the hash opens that view.
/// </summary>
/// <param name="Database">The runtime view shows the environment's database view; only with an environment.</param>
/// <param name="Application">
/// The deployable whose application view the runtime view shows; only with an environment, and only a name of
/// letters, digits, <c>-</c> and <c>_</c> (<see cref="ApplicationText.IsName"/>).
/// </param>
/// <param name="Audience">Which work the work items view shows. All unless the hash says bot or team.</param>
public sealed record ViewAddress(DashboardView View, string? Environment = null, bool Database = false, string? Application = null, WorkAudience Audience = WorkAudience.All)
{
    private const string RuntimeName = "runtime";
    private const string ClusterName = "cluster";
    private const string WorkItemsName = "work-items";
    private const string DatabaseName = "database";
    private const string ApplicationName = "app";

    public static ViewAddress Health { get; } = new(DashboardView.Health);

    public static ViewAddress Cluster { get; } = new(DashboardView.Cluster);

    public static ViewAddress WorkItems { get; } = new(DashboardView.WorkItems);

    public static ViewAddress Parse(string? hash)
    {
        var text = (hash ?? string.Empty).Trim().TrimStart('#').Trim('/');
        var slash = text.IndexOf('/', StringComparison.Ordinal);
        var head = slash < 0 ? text : text[..slash];
        if (string.Equals(head, ClusterName, StringComparison.OrdinalIgnoreCase))
        {
            return Cluster;
        }

        var tail = slash < 0 ? string.Empty : text[(slash + 1)..].Trim('/');

        if (string.Equals(head, WorkItemsName, StringComparison.OrdinalIgnoreCase))
        {
            return new ViewAddress(DashboardView.WorkItems, Audience: WorkItemsText.AudienceOf(AudienceSegment(tail)));
        }

        if (!string.Equals(head, RuntimeName, StringComparison.OrdinalIgnoreCase))
        {
            return Health;
        }

        return RuntimeOf(tail);
    }

    /// <summary>
    /// The first segment of a work items tail: the filter's word, or nothing. A trimmed tail has no slash at either
    /// end, so the first segment is the whole of what stands before the first slash inside it.
    /// </summary>
    private static string AudienceSegment(string tail) => tail.Split('/', 2)[0];

    /// <summary>
    /// The runtime view a trimmed tail names: no tail is the plain runtime view; a tail whose last segment is
    /// <c>database</c> is that environment's database view; a tail whose text before the last slash ends with
    /// <c>/app</c> is that environment's application view of the deployable named in the last segment - a name the
    /// address refuses is no view, and the environment's diagram is shown instead; anything else is the environment's
    /// diagram, its name read whole, marks and all.
    /// </summary>
    private static ViewAddress RuntimeOf(string tail)
    {
        // A trimmed tail has no slash at either end.
        if (string.IsNullOrWhiteSpace(tail))
        {
            return new ViewAddress(DashboardView.Runtime);
        }

        var last = tail.LastIndexOf('/');
        if (last < 0)
        {
            return new ViewAddress(DashboardView.Runtime, tail);
        }

        var beforeLastSlash = tail[..last];
        var lastSegment = tail[(last + 1)..];

        // Application zoom: the text before the last slash ends with "/app".
        // The trimmed tail never begins with a slash, so EndsWith is the whole check - no length guard needed.
        if (beforeLastSlash.EndsWith($"/{ApplicationName}", StringComparison.OrdinalIgnoreCase))
        {
            // Strip the "app" word; TrimEnd handles the edge case of consecutive slashes.
            var environment = beforeLastSlash[..^ApplicationName.Length].TrimEnd('/');
            return string.IsNullOrWhiteSpace(environment)
                ? new ViewAddress(DashboardView.Runtime)
                : new ViewAddress(DashboardView.Runtime, environment, Application: ApplicationText.IsName(lastSegment) ? lastSegment : null);
        }

        // Database zoom: the last segment is the whole word "database".
        if (string.Equals(lastSegment, DatabaseName, StringComparison.OrdinalIgnoreCase))
        {
            // TrimEnd handles the edge case of consecutive slashes ("uat//database" → "uat/").
            var environment = beforeLastSlash.TrimEnd('/');
            return string.IsNullOrWhiteSpace(environment)
                ? new ViewAddress(DashboardView.Runtime)
                : new ViewAddress(DashboardView.Runtime, environment, Database: true);
        }

        // No zoom: the whole tail is the environment's name.
        return new ViewAddress(DashboardView.Runtime, tail);
    }

    /// <summary>The hash for this view, without <c>#</c>: empty for the health view.</summary>
    public string ToHash() => View switch
    {
        DashboardView.Runtime => RuntimeHash(Environment, Database, Application),
        DashboardView.Cluster => ClusterName,
        DashboardView.WorkItems => WorkItemsHash(Audience),
        DashboardView.Health => string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(View), View, "A view has no address."),
    };

    /// <summary>
    /// The runtime view's hash, by one rule in order: no environment is the plain word (no zoom without one); then an
    /// application the address accepts is that deployable's view; then the database view; then the environment's
    /// diagram. An application name <see cref="ApplicationText.IsName"/> refuses falls back to the database, then to
    /// the diagram.
    /// </summary>
    private static string RuntimeHash(string? environment, bool database, string? application)
    {
        if (environment is null)
        {
            return RuntimeName;
        }

        if (ApplicationText.IsName(application))
        {
            return $"{RuntimeName}/{environment}/{ApplicationName}/{application}";
        }

        return database
            ? $"{RuntimeName}/{environment}/{DatabaseName}"
            : $"{RuntimeName}/{environment}";
    }

    /// <summary>
    /// The work items view's hash: its word, then the filter's segment from <see cref="WorkItemsText.AudienceHash"/>
    /// when it has one. All has an empty segment, and a filter that is none of the three is no filter: both leave the
    /// word alone.
    /// </summary>
    private static string WorkItemsHash(WorkAudience audience) =>
        Enum.IsDefined(audience) && WorkItemsText.AudienceHash(audience) is { Length: > 0 } segment
            ? $"{WorkItemsName}/{segment}"
            : WorkItemsName;
}
