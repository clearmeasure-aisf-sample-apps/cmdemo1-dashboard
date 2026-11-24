namespace Dashboard.Runtime;

/// <summary>
/// The mark of a deployment in the corner of a node's tile: a small dot whose shape says the state, with the sentence
/// as its title.
/// </summary>
/// <param name="State">
/// <c>executing</c> (filled, pulsing), <c>queued</c> (hollow), <c>waiting</c> (a dot in a ring: a person has to act)
/// or <c>ended</c> (small and still).
/// </param>
/// <param name="Title">The sentence; one line per deployment when the deployable has more than one.</param>
/// <param name="Link">Where the dot leads (the task in Octopus Deploy); null when the file gives no address.</param>
public sealed record RuntimeDeployment(string State, string Title, RuntimeLink? Link = null);
