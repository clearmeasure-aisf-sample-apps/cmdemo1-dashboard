namespace Dashboard.Runtime;

/// <summary>
/// A region boundary of the diagram. Unlike the manifest's other records it is not made of words alone: it holds the
/// caller's <see cref="Roles"/> list and its <see cref="Links"/> as they were handed, neither copied nor weighed by
/// content, so <c>==</c> on two regions asks whether they stand on the same list and the same link set. Two regions of
/// the same words in two lists are two regions; a caller that wants "the same region of the same words" must compare
/// the parts (<see cref="Alias"/>, <see cref="Name"/>, the roles in sequence, the links key by key).
/// </summary>
/// <param name="Alias">The region boundary's alias.</param>
/// <param name="Name">The Azure region.</param>
/// <param name="Roles">
/// What the region is for, in the deployment's words: <see cref="RuntimeRegionRole.Primary"/>,
/// <see cref="RuntimeRegionRole.Standby"/>, <see cref="RuntimeRegionRole.Data"/>, <see cref="RuntimeRegionRole.Static"/>;
/// any other word is kept as written.
/// </param>
/// <param name="Links">Where the region's name leads (<see cref="RuntimeBoxLink"/>); null without a link.</param>
public sealed record RuntimeRegion(string Alias, string Name, IReadOnlyList<string> Roles, Health.LinkSet? Links = null);
