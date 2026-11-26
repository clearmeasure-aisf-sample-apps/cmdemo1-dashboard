namespace Dashboard.Runtime;

public enum RuntimeNodeKind
{
    /// <summary>A kind this dashboard does not know: drawn, not updated.</summary>
    Other,

    /// <summary>The browser: a person, no tile.</summary>
    Person,

    /// <summary>The Front Door endpoint of a deployable: the public address.</summary>
    FrontDoor,

    /// <summary>A web app (a regional node).</summary>
    WebApp,

    /// <summary>The environment's database (Azure SQL, or SQL Server in a cluster): the browser cannot ask it.</summary>
    Sql,

    /// <summary>A Static Web App: the dashboard.</summary>
    StaticSite,

    /// <summary>
    /// Something a deployable depends on and the system does not own (an external service), drawn outside the
    /// subscription: the browser cannot ask it, and a web app's detailed health check tells its state.
    /// </summary>
    Dependency,

    /// <summary>
    /// A container app the system creates, or one its resource groups hold: the page probes its public address while
    /// it shows the diagram (<see cref="Health.ContainerAppProbe"/>), and its box also says what the pipeline's hourly
    /// health report last found there.
    /// </summary>
    ContainerApp,

    /// <summary>
    /// A box of the system's other resources (the vault, the SQL server, the identities): the browser cannot ask it,
    /// so its box says what Azure Resource Health last said of the resources it stands for (<see cref="RuntimeNode.Ids"/>).
    /// </summary>
    Resource,
}
