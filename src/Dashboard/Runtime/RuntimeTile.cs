namespace Dashboard.Runtime;

/// <summary>The tile drawn into a node's slot, and the look of its box.</summary>
/// <param name="Alias">The node's alias.</param>
/// <param name="State"><c>healthy</c>, <c>unhealthy</c>, <c>unreachable</c>, <c>checking</c> or <c>neutral</c> (not probed).</param>
/// <param name="Label">The badge's word: the state, never colour alone.</param>
/// <param name="Facts">Next to the badge: HTTP status and latency, or why there is none.</param>
/// <param name="Lines">The lines under the badge, top to bottom.</param>
/// <param name="History">The last checks, oldest first, as states; null for a node that is not checked.</param>
/// <param name="Title">The tooltip of the whole node.</param>
/// <param name="Link">Where the badge leads (Live Metrics); null without a link.</param>
/// <param name="NameLink">Where the node's name leads (the resource in the Azure portal); null without a link.</param>
/// <param name="Deployment">The mark of a deployment of the node's deployable to this environment; null without one.</param>
public sealed record RuntimeTile(
    string Alias,
    string State,
    string Label,
    string? Facts,
    IReadOnlyList<RuntimeTileLine> Lines,
    IReadOnlyList<string>? History,
    string Title,
    RuntimeLink? Link = null,
    RuntimeLink? NameLink = null,
    RuntimeDeployment? Deployment = null);
