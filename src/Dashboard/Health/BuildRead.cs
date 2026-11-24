namespace Dashboard.Health;

/// <summary>
/// What a deployable knows about its build endpoint: whether it was asked, the version the node ran when it was, and
/// what it answered. The rule it owns: ask once, and ask again only when the node reports another version.
/// </summary>
public sealed record BuildRead
{
    private BuildRead(BuildInfo? build, string? version, bool wasRead)
    {
        Build = build;
        Version = version;
        WasRead = wasRead;
    }

    /// <summary>The build endpoint was not asked yet: no build, no version, due for any version.</summary>
    public static BuildRead NotYet { get; } = new(null, null, wasRead: false);

    /// <summary>The build endpoint was asked while the node ran <paramref name="version"/>; null when it answered no build.</summary>
    public static BuildRead Read(BuildInfo? build, string? version) => new(build, version, wasRead: true);

    /// <summary>The build the endpoint answered; null before it was asked or when it answered none.</summary>
    public BuildInfo? Build { get; }

    /// <summary>The version the node ran when the build endpoint was asked: another version is another build.</summary>
    public string? Version { get; }

    /// <summary>True once the build endpoint was asked.</summary>
    public bool WasRead { get; }

    /// <summary>
    /// True when the build endpoint should be asked now: it was not asked yet, or the node reports another version
    /// (ignoring case) than when it was. Not every round: the answer changes only with a deployment.
    /// </summary>
    public bool IsDue(string? version) =>
        !WasRead || !string.Equals(version, Version, StringComparison.OrdinalIgnoreCase);
}
