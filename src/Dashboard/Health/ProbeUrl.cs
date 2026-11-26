namespace Dashboard.Health;

/// <summary>
/// The two ways the page turns a path into an address, held in one place so that a reader looking for one of them
/// finds the other:
/// <list type="bullet">
/// <item><description><see cref="Combine"/> - a path onto another node's base address, for every endpoint the page
/// probes: its health, its version, its telemetry, its detail, its build.</description></item>
/// <item><description><see cref="ResolveRelative"/> - a path of the dashboard's own site, resolved against the
/// address the page is served from, for the files the page ships beside itself: <c>topology.json</c>, the files of
/// <c>runtime/</c>, its own build facts.</description></item>
/// </list>
/// Neither one edits the path it is given beyond the slashes that front it: the path is taken from the root, as
/// <see cref="SitePath.FromRoot(string)" /> - the one home of a path's front slashes - says, and the rest of it is
/// asked for as it was written.
/// <para>
/// The one join the two readings of a base share - the base, without its trailing slash, one slash, and the path,
/// without its leading one - stands in <see cref="UriExtensions.Join(string, string)" />: <see cref="Combine" />
/// says it with the base taken up to its path, and <see cref="UriExtensions.AppendPath(Uri, string)" /> says it
/// with the base as it stands, which the release link <see cref="LinkText.Release(Uri?, string?)" /> and the
/// diagram file of the runtime view say through. The segment a caller joins is the caller's own, and its escaping
/// stays with the caller.
/// </para>
/// </summary>
public static class ProbeUrl
{
    /// <summary>
    /// The address of an endpoint: a node's base address and a path, joined with exactly one slash.
    /// <para>
    /// The refusals answer in this order: what is missing is said before what is wrong. A base address that is not
    /// there is refused first, then a path that is not there, and only then a base address that is relative to a
    /// page. So a relative base brought with no path is refused by the name <c>path</c>, and no base brought with no
    /// path is refused by the name <c>baseAddress</c>.
    /// </para>
    /// </summary>
    /// <param name="baseAddress">The node's base address. It must be absolute.</param>
    /// <param name="path">The path of the endpoint, with or without its front slash.</param>
    /// <exception cref="ArgumentNullException"><paramref name="baseAddress"/> is null, or else <paramref name="path"/> is.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="baseAddress"/> is relative, and neither argument is null.</exception>
    public static Uri Combine(Uri baseAddress, string path)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        ArgumentNullException.ThrowIfNull(path);
        return UriExtensions.Join(baseAddress.GetLeftPart(UriPartial.Path), path);
    }

    /// <summary>
    /// The path of a file of the dashboard's own site, as the address to ask for it: the path is taken from the
    /// site's root whatever it starts with - as <c>topology.json</c> is - and resolved against
    /// <paramref name="baseAddress"/>, the address the page is served from, into the address the request goes to.
    /// <para>
    /// A path that is no path - one that names another site, or carries a scheme of its own, which nothing served
    /// under this page can hold - answers with no address: null, the caller reads nothing, and the refusal is said
    /// here rather than swallowed at the call. A client with no address of its own
    /// (<see cref="HttpClient.BaseAddress"/> unset) resolves nothing, and the path stands as the relative address
    /// it is, for that client to answer for as it answers for any relative request.
    /// </para>
    /// </summary>
    /// <param name="baseAddress">The address the page is served from; null when nothing serves it.</param>
    /// <param name="path">The path of the file, from the site's root, with or without its front slash.</param>
    internal static Uri? ResolveRelative(Uri? baseAddress, string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!Uri.TryCreate(SitePath.FromRoot(path), UriKind.Relative, out var relative))
        {
            return null;
        }

        return baseAddress is null ? relative : new Uri(baseAddress, relative);
    }
}

/// <summary>
/// The one join the page says of a path that comes after an address, held in one place so that the places that
/// build an address from a base and a path - the probe's <see cref="ProbeUrl.Combine"/>, the release link
/// <see cref="LinkText.Release(Uri?, string?)"/>, the diagram file of the runtime view - all say it the same way:
/// the base, without its trailing slash, one slash, and the path, without its leading one. The join edits nothing
/// but the slashes between the base and the path: the segment a caller joins is the caller's own, and its escaping
/// stays with the caller.
/// <para>
/// The callers read the base two ways, and the join serves both: <see cref="ProbeUrl.Combine" /> takes a node's
/// base up to its path, so the query and the fragment the base carried do not stand in the address the probe asks
/// at; <see cref="AppendPath(Uri, string)"/> takes the base as it stands, so the link the caller builds is the
/// address the deployment wrote and nothing else - the fragment of an Octopus project page keeps the release the
/// join writes under it.
/// </para>
/// <para>
/// The refusals answer in this order: what is missing is said before what is wrong, a base address that is not
/// there is refused first by the name <c>baseAddress</c>, then a path that is not there by the name <c>path</c>,
/// and only then a base address that is relative to a page, which the address itself refuses in the framework's
/// own words and naming no argument - the very sentence it says when its <see cref="Uri.AbsoluteUri"/> is read.
/// </para>
/// </summary>
public static class UriExtensions
{
    /// <summary>
    /// The address a path comes after: <paramref name="baseAddress"/> as it stands and <paramref name="path"/>,
    /// joined with exactly one slash, as the absolute address the link leads to.
    /// </summary>
    /// <param name="baseAddress">The base address. It must be absolute.</param>
    /// <param name="path">The path that comes after it, with or without its front slash; its escaping is the
    /// caller's, and the join edits it not at all.</param>
    /// <returns>The joined address, absolute as the base is.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="baseAddress"/> is null, or else <paramref name="path"/> is.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="baseAddress"/> is relative, and neither argument is null.</exception>
    public static Uri AppendPath(this Uri baseAddress, string path)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        ArgumentNullException.ThrowIfNull(path);
        return Join(baseAddress.AbsoluteUri, path);
    }

    /// <summary>
    /// The one join the readings of a base share: <paramref name="root"/>, without its trailing slash, one slash,
    /// and <paramref name="path"/> taken from the root (<see cref="SitePath.FromRoot(string)" />), parsed back into the absolute address it spells.
    /// Neither argument is null here: the readings that call this refuse a missing argument by name before they
    /// reach the join.
    /// </summary>
    /// <param name="root">The base, as its reading says it: up to its path, or as it stands.</param>
    /// <param name="path">The path that comes after it, with or without its front slash.</param>
    internal static Uri Join(string root, string path) =>
        new($"{root.TrimEnd('/')}/{SitePath.FromRoot(path)}", UriKind.Absolute);
}
