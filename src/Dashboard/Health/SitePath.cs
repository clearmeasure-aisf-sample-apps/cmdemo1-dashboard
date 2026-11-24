namespace Dashboard.Health;

/// <summary>
/// The one home of the rule every path of a deployable in <c>topology.json</c> follows: it begins with <c>/</c>. Only
/// the front is added - <c>//health</c> stays <c>//health</c>, the rest of a path is the app's own.
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
    /// paths are read this way on purpose (#1041): a padded entry is kept as the path it was written as.
    /// </summary>
    public static string WithFront(string path) => path.StartsWith('/') ? path : $"/{path}";
}
