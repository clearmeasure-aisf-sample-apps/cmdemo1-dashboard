namespace Dashboard.Health;

/// <summary>
/// The dashboard itself as a deployable of the system (<c>system.dashboard</c> of <c>topology.json</c>): this page.
/// </summary>
/// <param name="Name">The deployable's name in the system (<c>dashboard</c>).</param>
/// <param name="BuildPath">
/// Where the site serves the facts of its own build, next to <c>index.html</c> (<c>/build-facts.json</c>, see
/// <see cref="BuildInfo"/>): the page reads it from its own address.
/// </param>
public sealed record DashboardInfo(string Name, string BuildPath)
{
    public const string DefaultName = "dashboard";
}
