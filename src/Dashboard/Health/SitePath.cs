namespace Dashboard.Health;

/// <summary>
/// The one home of the front-slash rule of a path, in both of its halves (#1541):
/// <list type="bullet">
/// <item><description>as a path of a deployable in <c>topology.json</c> is <em>kept</em> - it begins with <c>/</c>,
/// and only the front it lacks is added: <c>//health</c> is recorded as <c>//health</c>, the rest of a path is the
/// app's own (<see cref="WithFront" />, <see cref="Parse" />);</description></item>
/// <item><description>as a path is <em>asked at</em> - from the root of the site it is joined to, whatever run of
/// slashes fronts it: <c>//health</c> is asked at as <c>/health</c> (<see cref="FromRoot" />, which
/// <see cref="ProbeUrl" /> and <see cref="UriExtensions" /> read).</description></item>
/// </list>
/// So a path is recorded as it was written and asked at from the root; the front slashes are settled here, and
/// nowhere else.
/// </summary>
public static class SitePath
{
    /// <summary>
    /// A word of the file as a path: read over its spaces and given its front slash; null for a word of nothing
    /// (null, empty or spaces), which is no path.
    /// </summary>
    public static string? Parse(string? text) =>
        text?.Trim() is { Length: > 0 } word ? WithFront(word) : null;

    /// <summary>
    /// The path as written, with the front slash it lacks and nothing else changed - not even its spaces. The traffic
    /// paths are read this way on purpose (#1041): a padded entry is kept as the path it was written as. A run of
    /// front slashes is kept too; it is settled only when the path is asked at, by <see cref="FromRoot" />.
    /// </summary>
    public static string WithFront(string path) => path.StartsWith('/') ? path : $"/{path}";

    /// <summary>
    /// The path as it is asked at, taken from the root it is joined to: every front slash dropped, so the joiner
    /// adds exactly one. Nothing after the front is changed - the slashes in its middle and at its back stand.
    /// </summary>
    public static string FromRoot(string path) => path.TrimStart('/');
}
