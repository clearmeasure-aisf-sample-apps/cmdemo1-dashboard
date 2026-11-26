namespace Dashboard.Runtime;

/// <param name="Id"><c>&lt;from&gt;-to-&lt;to&gt;</c>.</param>
/// <param name="Priority">For an origin: Front Door's priority (1 first).</param>
public sealed record RuntimeEdge(string Id, string From, string To, RuntimeEdgeKind Kind, int? Priority = null)
{
    /// <summary>
    /// The one edge the number means something on: an origin that Front Door serves before the others. The
    /// priority <c>1</c> is the first origin, and a line of any other kind - or an origin with no priority or one
    /// not <c>1</c> - is no first origin, whatever number it carries.
    /// </summary>
    public bool IsFirstOrigin => Kind == RuntimeEdgeKind.Origin && Priority == 1;
}
