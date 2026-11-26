namespace Dashboard.Runtime;

/// <param name="Id"><c>&lt;from&gt;-to-&lt;to&gt;</c>.</param>
/// <param name="Priority">For an origin: Front Door's priority (1 first).</param>
public sealed record RuntimeEdge(string Id, string From, string To, RuntimeEdgeKind Kind, int? Priority = null);
