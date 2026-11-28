namespace Dashboard.Runtime;

/// <summary>One environment's diagram: its manifest and the SVG as text, or the reasons there is none.</summary>
public sealed record RuntimeDiagram(string Environment, RuntimeManifest? Manifest, string? Svg, IReadOnlyList<string> Errors)
{
    public bool IsValid => PairParts.Both(Manifest, Svg);

    /// <summary>The sentence under the picture naming the boxes the manifest has and the SVG lacks; null when none is missing.</summary>
    public static string? Lacks(IReadOnlyList<string> missing) =>
        missing.Count == 0
            ? null
            : $"The diagram lacks {string.Join(", ", missing)}: the manifest and the SVG of this deployment do not match.";
}
