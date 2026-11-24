namespace Dashboard.Health;

/// <summary>
/// A page of the system itself that the environment's part of the dashboard leads to
/// (<c>environments[].pages</c> of <c>topology.json</c>): a deployable of the system names it
/// (<c>dashboardLink</c> in <c>system.json</c>), and the deployment writes the address the deployable has in this
/// environment. It is public like the page it is on, so its tooltip names no sign-in.
/// </summary>
/// <param name="Name">The words of the link.</param>
/// <param name="Url">Where it leads.</param>
/// <param name="Deployable">The deployable whose page it is; null when the topology does not say.</param>
public sealed record PageLink(string Name, Uri Url, string? Deployable = null)
{
    /// <summary>The link's tooltip: its words, and that it opens a new tab.</summary>
    public string Title => Name + LinkText.GitHubSuffix;
}
