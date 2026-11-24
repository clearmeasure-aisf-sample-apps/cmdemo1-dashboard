namespace Dashboard.Runtime;

/// <summary>
/// What the runtime view of an environment shows: its diagram, the database view of its database, or the application
/// view of the box of one deployable.
/// </summary>
public sealed record RuntimeZoomTarget
{
    private RuntimeZoomTarget(bool database, string? application)
    {
        Database = database;
        Application = application;
    }

    /// <summary>The environment's database view.</summary>
    public bool Database { get; }

    /// <summary>The deployable whose application view is shown; null for the diagram and the database view.</summary>
    public string? Application { get; }

    /// <summary>The diagram: out of whichever box the view was zoomed into.</summary>
    public static RuntimeZoomTarget Diagram { get; } = new(database: false, application: null);

    /// <summary>The database view, zoomed into the database's box.</summary>
    public static RuntimeZoomTarget TheDatabase { get; } = new(database: true, application: null);

    /// <summary>The application view of a deployable, zoomed into its box; the diagram for no deployable.</summary>
    public static RuntimeZoomTarget Box(string? deployable) => new(database: false, application: deployable);
}

/// <summary>
/// The runtime view zooms into a box of an environment's diagram, or back out of it: the one command for the database
/// view and the application view alike.
/// </summary>
/// <param name="Environment">The environment's name.</param>
/// <param name="Target">What is to be shown.</param>
/// <param name="Step">The change is a step of the browser's history: its Back undoes it.</param>
public sealed record RuntimeZoomChange(string Environment, RuntimeZoomTarget Target, bool Step)
{
    /// <summary>The address the change comes to.</summary>
    public ViewAddress Address => new(DashboardView.Runtime, Environment, Target.Database, Target.Application);
}
