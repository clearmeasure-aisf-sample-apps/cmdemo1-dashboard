using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>
/// Where a monitor target's link of a kind leads, titled for its subject. A web app's tile and the arrows to and from
/// it (<see cref="RuntimePayloadBuilder"/>, <see cref="RuntimeEdgeMarks"/>) both say it, so it has one home (#1829).
/// </summary>
internal static class RuntimeTargetLink
{
    /// <summary>The target's link of <paramref name="key"/> (<see cref="LinkSet"/>), titled for <paramref name="subject"/>; null where it has none.</summary>
    internal static RuntimeLink? To(TargetStatus target, string key, string subject) =>
        RuntimeLink.To(target.Links[key], LinkText.For(key, subject));
}
