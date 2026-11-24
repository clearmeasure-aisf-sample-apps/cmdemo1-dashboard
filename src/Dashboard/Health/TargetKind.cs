namespace Dashboard.Health;

public enum TargetKind
{
    /// <summary>The public address of a deployable in an environment.</summary>
    FrontDoor,

    /// <summary>One regional web app behind the public address.</summary>
    Node,
}
