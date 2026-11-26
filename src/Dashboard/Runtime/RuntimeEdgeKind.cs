namespace Dashboard.Runtime;

public enum RuntimeEdgeKind
{
    Other,

    /// <summary>The browser to a public address (the Front Door endpoint, or a web app without one).</summary>
    Public,

    /// <summary>Front Door to one of its origins, by priority.</summary>
    Origin,

    /// <summary>A web app to the database.</summary>
    Sql,

    /// <summary>The browser to the dashboard.</summary>
    Dashboard,

    /// <summary>A web app to something its deployable depends on.</summary>
    Dependency,
}
