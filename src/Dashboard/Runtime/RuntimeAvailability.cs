namespace Dashboard.Runtime;

public enum RuntimeAvailability
{
    /// <summary>The deployment rendered the diagrams.</summary>
    Available,

    /// <summary>
    /// The site has no <c>runtime/</c>: a deployment from before the runtime view, or <c>dotnet run</c> without the
    /// sample. Not an error.
    /// </summary>
    NotDeployed,

    /// <summary>The files are there and cannot be read.</summary>
    Failed,
}
