namespace Dashboard.Tests;

public class ViewAddressTests
{
    [Theory]
    [InlineData(null, DashboardView.Health, null)]
    [InlineData("", DashboardView.Health, null)]
    [InlineData("#", DashboardView.Health, null)]
    [InlineData("health", DashboardView.Health, null)]
    [InlineData("env-0", DashboardView.Health, null)]
    [InlineData("runtime", DashboardView.Runtime, null)]
    [InlineData("#runtime", DashboardView.Runtime, null)]
    [InlineData("Runtime/", DashboardView.Runtime, null)]
    [InlineData("runtime/uat", DashboardView.Runtime, "uat")]
    [InlineData("#runtime/prod/", DashboardView.Runtime, "prod")]
    [InlineData("cluster", DashboardView.Cluster, null)]
    [InlineData("#cluster", DashboardView.Cluster, null)]
    [InlineData("Cluster/", DashboardView.Cluster, null)]
    [InlineData("#cluster/nodes", DashboardView.Cluster, null)]
    [InlineData("clusters", DashboardView.Health, null)]
    [InlineData("work-items", DashboardView.WorkItems, null)]
    [InlineData("#work-items", DashboardView.WorkItems, null)]
    [InlineData("Work-Items/", DashboardView.WorkItems, null)]
    [InlineData("#work-items/board", DashboardView.WorkItems, null)]
    [InlineData("workitems", DashboardView.Health, null)]
    public void TheHashNamesTheView(string? hash, DashboardView view, string? environment)
    {
        Assert.Equal(new ViewAddress(view, environment), ViewAddress.Parse(hash));
    }

    [Theory]
    [InlineData("runtime/uat/database", "uat", true)]
    [InlineData("#runtime/uat/database/", "uat", true)]
    [InlineData("Runtime/uat/Database", "uat", true)]
    [InlineData("runtime/uat/tables", "uat/tables", false)]
    [InlineData("runtime/database", "database", false)]
    [InlineData("runtime//database", "database", false)]
    public void TheHashNamesTheDatabaseViewOfAnEnvironment(string hash, string environment, bool database)
    {
        Assert.Equal(new ViewAddress(DashboardView.Runtime, environment, database), ViewAddress.Parse(hash));
    }

    [Fact]
    public void TheDatabaseViewWritesTheHashThatOpensIt()
    {
        var address = new ViewAddress(DashboardView.Runtime, "uat", Database: true);

        Assert.Equal("runtime/uat/database", address.ToHash());
        Assert.Equal(address, ViewAddress.Parse(address.ToHash()));

        // Without an environment there is no database to show.
        Assert.Equal("runtime", new ViewAddress(DashboardView.Runtime, null, Database: true).ToHash());
    }

    [Fact]
    public void AViewWritesTheHashThatOpensIt()
    {
        Assert.Equal(string.Empty, ViewAddress.Health.ToHash());
        Assert.Equal("runtime", new ViewAddress(DashboardView.Runtime).ToHash());
        Assert.Equal("runtime/uat", new ViewAddress(DashboardView.Runtime, "uat").ToHash());
        Assert.Equal("cluster", ViewAddress.Cluster.ToHash());
        Assert.Equal("work-items", ViewAddress.WorkItems.ToHash());
        Assert.Equal("work-items/bot", new ViewAddress(DashboardView.WorkItems, Audience: WorkAudience.Bot).ToHash());
        Assert.Equal("work-items/team", new ViewAddress(DashboardView.WorkItems, Audience: WorkAudience.Team).ToHash());
        Assert.Equal(ViewAddress.Cluster, ViewAddress.Parse(ViewAddress.Cluster.ToHash()));
        Assert.Equal(ViewAddress.WorkItems, ViewAddress.Parse(ViewAddress.WorkItems.ToHash()));
        Assert.Equal(WorkAudience.Bot, ViewAddress.Parse("#work-items/bot").Audience);
        Assert.Equal(WorkAudience.Team, ViewAddress.Parse("Work-Items/team/").Audience);
        Assert.Equal(new ViewAddress(DashboardView.WorkItems, Audience: WorkAudience.Bot), ViewAddress.Parse(new ViewAddress(DashboardView.WorkItems, Audience: WorkAudience.Bot).ToHash()));
        Assert.Equal(new ViewAddress(DashboardView.Runtime, "uat"), ViewAddress.Parse(new ViewAddress(DashboardView.Runtime, "uat").ToHash()));
    }
}
