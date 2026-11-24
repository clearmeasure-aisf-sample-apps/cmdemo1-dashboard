namespace Dashboard.Health;

public enum ServingState
{
    /// <summary>The deployable has no nodes.</summary>
    NoNodes,

    /// <summary>A node that could be the serving one has not been checked yet.</summary>
    Pending,

    /// <summary>A primary node is healthy and serves; in a rotation, every healthy primary node does.</summary>
    Primary,

    /// <summary>No primary node is healthy; a standby is, and serves.</summary>
    FailedOver,

    /// <summary>No node is healthy.</summary>
    Down,
}
