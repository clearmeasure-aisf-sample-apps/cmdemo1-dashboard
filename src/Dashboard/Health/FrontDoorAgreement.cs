namespace Dashboard.Health;

public enum FrontDoorAgreement
{
    /// <summary>The deployable has no Front Door endpoint.</summary>
    NotPresent,

    /// <summary>The Front Door endpoint or the nodes have not been checked yet.</summary>
    Pending,

    /// <summary>Front Door is healthy exactly when a node is.</summary>
    Agrees,

    /// <summary>Front Door is healthy and no node is, or the other way around.</summary>
    Disagrees,
}
