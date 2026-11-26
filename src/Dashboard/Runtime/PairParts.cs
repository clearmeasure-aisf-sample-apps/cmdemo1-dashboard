namespace Dashboard.Runtime;

/// <summary>
/// The one question the pair's records ask (#1842): a view needs both parts of its pair - its manifest and
/// its SVG - and is never drawn from half a pair. <see cref="RuntimeFilesRead{TManifest}"/>,
/// <see cref="RuntimeDiagram"/> and <see cref="DatabaseDiagram"/> all ask it here, once, rather than each
/// spelling the two null checks of its own.
/// </summary>
internal static class PairParts
{
    /// <summary>Both parts of the pair stand, or neither: <see langword="false" /> when either part is missing.</summary>
    internal static bool Both<TManifest>(TManifest? manifest, string? svg) where TManifest : class
        => manifest is not null && svg is not null;
}
