namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeZoomChange"/>, the one command the runtime view raises for the database view and the application
/// view alike: each target comes to exactly the address the two former handlers of <c>App.razor</c> made of a
/// <see cref="RuntimeDatabaseChange"/> and a <see cref="RuntimeApplicationChange"/>, so the zoom in, the back out and
/// the correction in place go where they went.
/// </summary>
public class RuntimeZoomChangeTests
{
    /// <summary>What App.razor made of a database change before the one command.</summary>
    private static ViewAddress Before(RuntimeDatabaseChange change) =>
        new(DashboardView.Runtime, change.Environment, change.Shown);

    /// <summary>What App.razor made of an application change before the one command.</summary>
    private static ViewAddress Before(RuntimeApplicationChange change) =>
        new(DashboardView.Runtime, change.Environment, Application: change.Deployable);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TheDatabaseZoomComesToTheDatabaseViewsAddress(bool step)
    {
        var change = new RuntimeZoomChange("prod", RuntimeZoomTarget.TheDatabase, step);

        Assert.Equal(Before(new RuntimeDatabaseChange("prod", Shown: true, step)), change.Address);
        Assert.Equal("runtime/prod/database", change.Address.ToHash());
        Assert.Equal(step, change.Step);
    }

    [Theory]
    [InlineData("web")]
    [InlineData("Worker_2")]
    public void TheBoxZoomComesToTheApplicationViewsAddress(string deployable)
    {
        var change = new RuntimeZoomChange("prod", RuntimeZoomTarget.Box(deployable), Step: true);

        Assert.Equal(Before(new RuntimeApplicationChange("prod", deployable, Step: true)), change.Address);
        Assert.Equal(deployable, change.Address.Application);
        Assert.False(change.Address.Database);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BackingOutComesToTheDiagramFromEitherBox(bool step)
    {
        var change = new RuntimeZoomChange("test", RuntimeZoomTarget.Diagram, step);

        Assert.Equal(Before(new RuntimeDatabaseChange("test", Shown: false, step)), change.Address);
        Assert.Equal(Before(new RuntimeApplicationChange("test", null, step)), change.Address);
        Assert.Equal("runtime/test", change.Address.ToHash());
    }

    [Fact]
    public void ABoxOfNoDeployableIsTheDiagram()
    {
        Assert.Equal(RuntimeZoomTarget.Diagram, RuntimeZoomTarget.Box(null));
    }

    [Fact]
    public void TheEnvironmentIsKeptInItsOwnLetters()
    {
        var change = new RuntimeZoomChange(" Prod ", RuntimeZoomTarget.TheDatabase, Step: true);

        Assert.Equal(" Prod ", change.Address.Environment);
        Assert.Equal(" Prod ", change.Environment);
    }

    [Fact]
    public void TheThreeTargetsAreDistinct()
    {
        Assert.NotEqual(RuntimeZoomTarget.Diagram, RuntimeZoomTarget.TheDatabase);
        Assert.NotEqual(RuntimeZoomTarget.Diagram, RuntimeZoomTarget.Box("web"));
        Assert.NotEqual(RuntimeZoomTarget.TheDatabase, RuntimeZoomTarget.Box("web"));
        Assert.Equal(RuntimeZoomTarget.Box("web"), RuntimeZoomTarget.Box("web"));
    }
}
