namespace Dashboard.Runtime;

/// <summary>One environment's database view: its manifest and the SVG as text, or the reasons there is none.</summary>
public sealed record DatabaseDiagram(DatabaseManifest? Manifest, string? Svg, IReadOnlyList<string> Errors)
{
    public bool IsValid => PairParts.Both(Manifest, Svg);
}
