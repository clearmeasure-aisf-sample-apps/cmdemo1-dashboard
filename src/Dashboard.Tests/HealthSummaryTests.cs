namespace Dashboard.Tests;

public class HealthSummaryTests
{
    [Fact]
    public void AllHealthy()
    {
        var summary = HealthSummary.Of(Enumerable.Repeat(HealthState.Healthy, 7));

        Assert.Equal(SummaryLevel.AllHealthy, summary.Level);
        Assert.Equal("All 7 nodes healthy", summary.Text);
        Assert.Equal(0, summary.NotHealthy);
    }

    [Fact]
    public void UnhealthyAndUnreachableBothCountAsNotHealthy()
    {
        var summary = HealthSummary.Of([HealthState.Healthy, HealthState.Unhealthy, HealthState.Healthy, HealthState.Unreachable, HealthState.Healthy, HealthState.Healthy, HealthState.Healthy]);

        Assert.Equal(SummaryLevel.Problems, summary.Level);
        Assert.Equal(2, summary.NotHealthy);
        Assert.Equal("2 of 7 nodes not healthy", summary.Text);
    }

    [Fact]
    public void BeforeTheFirstCheckEverythingIsBeingChecked()
    {
        var summary = HealthSummary.Of([HealthState.Pending, HealthState.Pending, HealthState.Pending]);

        Assert.Equal(SummaryLevel.Pending, summary.Level);
        Assert.Equal("Checking 3 nodes", summary.Text);
    }

    [Fact]
    public void WhileAnswersArriveTheHealthyOnesAreCounted()
    {
        var summary = HealthSummary.Of([HealthState.Healthy, HealthState.Pending, HealthState.Pending]);

        Assert.Equal(SummaryLevel.Pending, summary.Level);
        Assert.Equal("1 of 3 nodes healthy, 2 being checked", summary.Text);
    }

    [Fact]
    public void AProblemIsReportedBeforeEveryAnswerArrived()
    {
        var summary = HealthSummary.Of([HealthState.Unreachable, HealthState.Pending, HealthState.Healthy]);

        Assert.Equal(SummaryLevel.Problems, summary.Level);
        Assert.Equal("1 of 3 nodes not healthy", summary.Text);
    }

    [Fact]
    public void NoNodes()
    {
        var summary = HealthSummary.Of([]);

        Assert.Equal(SummaryLevel.Empty, summary.Level);
        Assert.Equal("No nodes in the topology", summary.Text);
    }

    [Theory]
    [InlineData(HealthState.Healthy, "The only node is healthy")]
    [InlineData(HealthState.Unhealthy, "1 of 1 node not healthy")]
    [InlineData(HealthState.Pending, "Checking 1 node")]
    public void ASingleNodeReadsAsSingular(HealthState state, string expected) =>
        Assert.Equal(expected, HealthSummary.Of([state]).Text);
}
