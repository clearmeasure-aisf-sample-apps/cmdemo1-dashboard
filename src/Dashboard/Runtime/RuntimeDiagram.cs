namespace Dashboard.Runtime;

/// <summary>One environment's diagram: its manifest and the SVG as text, or the reasons there is none.</summary>
public sealed record RuntimeDiagram(string Environment, RuntimeManifest? Manifest, string? Svg, IReadOnlyList<string> Errors)
{
    public bool IsValid => Manifest is not null && Svg is not null;
}
