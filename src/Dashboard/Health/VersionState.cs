namespace Dashboard.Health;

public enum VersionState
{
    /// <summary>The file with the pin (the environment's <c>versions.json</c>, or the deployable's own) has not been read yet.</summary>
    Pending,

    /// <summary>The file could not be read: the pinned version is not known.</summary>
    PinnedUnknown,

    /// <summary>The file was not found or has no entry for the deployable: nothing was deployed.</summary>
    NotDeployed,

    /// <summary>The pinned version is known, and no node's running version is.</summary>
    NodesUnknown,

    /// <summary>Every node whose running version is known runs the pinned version.</summary>
    InSync,

    /// <summary>At least one node runs another version than the pinned one.</summary>
    Differs,
}
