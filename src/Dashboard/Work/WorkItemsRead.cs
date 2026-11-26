namespace Dashboard.Work;

/// <summary>
/// What the last round left of the work items: whether the topology names an address to read them at, whether a
/// read was asked, and the report that came back. The three facts by name, so no caller hands two booleans over
/// the wrong way round; <see cref="WorkItemsText.Availability(WorkItemsRead)" /> settles them on one state.
/// </summary>
/// <param name="HasAddress">True when the topology names a delivery file, so there is a <c>workitems.json</c> next to it.</param>
/// <param name="Asked">True after a read of the work items was attempted.</param>
/// <param name="Report">The work items as last read; null when nothing was read.</param>
public sealed record WorkItemsRead(bool HasAddress, bool Asked, WorkItemsReport? Report);
