namespace Dashboard.Health;

/// <summary>
/// The two ways the page turns a path into an address, held in one place so that a reader looking for one of them
/// finds the other:
/// <list type="bullet">
/// <item><description><see cref="Combine"/> - a path onto another node's base address, for every endpoint the page
/// probes: its health, its version, its telemetry, its detail, its build.</description></item>
/// <item><description><see cref="ResolveRelative"/> - a path of the dashboard's own site, resolved against the
/// address the page is served from, for the files the page ships beside itself: <c>topology.json</c>, its own
/// build facts.</description></item>
/// </list>
/// Neither one edits the path it is given beyond the slashes that join it: what is asked for is what was written.
/// </summary>
public static class ProbeUrl
{
    /// <summary>The address of an endpoint: a node's base address and a path, joined with exactly one slash.</summary>
    public static Uri Combine(Uri baseAddress, string path)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        ArgumentNullException.ThrowIfNull(path);
        var root = baseAddress.GetLeftPart(UriPartial.Path).TrimEnd('/');
        return new Uri($"{root}/{path.TrimStart('/')}", UriKind.Absolute);
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
        if (!Uri.TryCreate(path.TrimStart('/'), UriKind.Relative, out var relative))
        {
            return null;
        }

        return baseAddress is null ? relative : new Uri(baseAddress, relative);
    }
}
