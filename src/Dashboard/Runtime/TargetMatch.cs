using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>
/// Which monitor target a node of the manifest is: the kind of target the monitor checks it as, and the address it
/// checks. A web app is a node of the topology, and so is a static site (a dashboard the topology lists as a node,
/// another home of this page, is checked like a web app); a Front Door endpoint is the deployable's public address.
/// Any other node is no monitor target.
/// </summary>
public sealed record TargetMatch(TargetKind Kind, Uri Url)
{
    /// <summary>The target a node is checked as; null for a kind the monitor does not check, and for a node without an address.</summary>
    public static TargetMatch? Of(RuntimeNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        TargetKind? kind = node.Kind switch
        {
            RuntimeNodeKind.WebApp or RuntimeNodeKind.StaticSite => TargetKind.Node,
            RuntimeNodeKind.FrontDoor => TargetKind.FrontDoor,
            _ => null,
        };
        return kind is { } targetKind && node.Url is { } url ? new TargetMatch(targetKind, url) : null;
    }

    /// <summary>True for the target of this kind at this address.</summary>
    public bool Matches(TargetStatus target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.Kind == Kind && target.Url == Url;
    }
}
