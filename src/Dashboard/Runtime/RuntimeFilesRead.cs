namespace Dashboard.Runtime;

/// <summary>
/// A manifest and its SVG, read as a pair: both parts and no errors, or neither part and the reasons why. A view is
/// never drawn from half a pair.
/// </summary>
internal sealed record RuntimeFilesRead<TManifest>(TManifest? Manifest, string? Svg, IReadOnlyList<string> Errors)
    where TManifest : class
{
    internal static RuntimeFilesRead<TManifest> Of(TManifest? manifest, string? svg, IReadOnlyList<string> errors) =>
        errors.Count > 0 || manifest is null || svg is null ? new(null, null, errors) : new(manifest, svg, []);
}
