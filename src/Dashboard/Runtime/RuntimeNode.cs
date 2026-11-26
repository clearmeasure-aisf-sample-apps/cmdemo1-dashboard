namespace Dashboard.Runtime;

/// <param name="Alias">The element's name in the PlantUML source: the last part of its <c>data-qualified-name</c>.</param>
/// <param name="Kind">What it is.</param>
/// <param name="Name">The Azure resource's name.</param>
/// <param name="Url">
/// The address the dashboard checks (web app, Front Door endpoint) or serves from (static site); null when the
/// browser does not know one, and always for the database.
/// </param>
/// <param name="HealthCheck">
/// For a dependency: the name of the entry of its web apps' detailed health check that tells its state; null when the
/// system names none.
/// </param>
/// <param name="DependencyKind">For a dependency: what it is, in the system's own words (<c>external</c>).</param>
/// <param name="Links">
/// Where the node's name leads when the topology has no link for it (<see cref="RuntimeBoxLink"/>); null without one.
/// </param>
/// <param name="HealthPath">
/// For a container app <c>system.json</c> declares: the health path of its deployable, which the page probes; null for
/// any other node, and for an app nobody declared (the page then asks <c>/_healthcheck</c>).
/// </param>
/// <param name="Ids">
/// The Azure resource ids the box stands for, from the resource groups' listing, by which the page finds what Resource
/// Health said of them; null in a manifest from before the ids, and where the listing has none.
/// </param>
/// <param name="Architecture">
/// For a web app or a container app: where the repository of its deployable publishes the diagram of every released
/// version (<c>&lt;address&gt;/&lt;version&gt;.json</c> and <c>.svg</c>, on another origin); the box then leads into
/// the application view. Null without one, and for anything but an absolute https address.
/// </param>
public sealed record RuntimeNode(
    string Alias,
    RuntimeNodeKind Kind,
    string Name,
    Uri? Url = null,
    string? Deployable = null,
    string? Role = null,
    string? Region = null,
    string? RegionAlias = null,
    string? HealthCheck = null,
    string? DependencyKind = null,
    Health.LinkSet? Links = null,
    string? HealthPath = null,
    IReadOnlyList<string>? Ids = null,
    Uri? Architecture = null);
