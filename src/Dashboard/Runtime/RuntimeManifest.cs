namespace Dashboard.Runtime;

/// <summary>
/// <c>runtime/&lt;env&gt;.json</c>: which drawn element of the environment's diagram is which. The dashboard finds a
/// node by its alias and a relationship by its id, and never reads names out of the SVG.
/// </summary>
/// <param name="Frames">
/// The frames that are no region (the subscription, a resource group, the Front Door profile, a plan, a cluster);
/// null in a manifest from before they were listed.
/// </param>
/// <param name="Unplaced">
/// The resources of the system that are in no environment: a list under the diagram, never a box of it
/// (<see cref="UnplacedText"/>). Null in a manifest without the list: one from before it draws them as boxes of a
/// frame "In no environment", and still does.
/// </param>
public sealed record RuntimeManifest(
    string Environment,
    IReadOnlyList<RuntimeNode> Nodes,
    IReadOnlyList<RuntimeRegion> Regions,
    IReadOnlyList<RuntimeEdge> Edges,
    IReadOnlyList<RuntimeFrame>? Frames = null,
    IReadOnlyList<RuntimeUnplaced>? Unplaced = null);
