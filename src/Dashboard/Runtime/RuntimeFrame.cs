namespace Dashboard.Runtime;

/// <summary>A frame of the diagram that is no region: it has a name and, where the deployment knows one, a link.</summary>
/// <param name="Alias">The boundary's alias.</param>
/// <param name="Kind">
/// What it is, in the deployment's word: <c>subscription</c>, <c>resourceGroup</c>, <c>frontDoorProfile</c>,
/// <c>plan</c>, <c>cluster</c>, <c>runtime</c> (the boundary of an application with its own runtime). An unknown
/// kind is linked all the same, with its name alone in the link's title.
/// </param>
/// <param name="Name">The resource's name, as drawn.</param>
/// <param name="Links">Where the frame's name leads (<see cref="RuntimeBoxLink"/>); null without a link.</param>
public sealed record RuntimeFrame(string Alias, string Kind, string Name, Health.LinkSet? Links = null);
