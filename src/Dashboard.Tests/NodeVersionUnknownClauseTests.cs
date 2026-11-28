using System.Reflection;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Pins <c>NodeVersion.UnknownClause</c>: the label and the reason it is not known, as one clause of the version
/// sentence. The member is internal (so the record's public surface and its <c>ToString</c> stay as pinned) and the
/// app declares no <c>InternalsVisibleTo</c>, so it is read by reflection.
/// </summary>
public class NodeVersionUnknownClauseTests
{
    private static string UnknownClause(NodeVersion node) =>
        (string)typeof(NodeVersion)
            .GetProperty("UnknownClause", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(node)!;

    [Fact]
    public void APendingNodeIsBeingChecked() =>
        Assert.Equal("centralus is being checked", UnknownClause(new NodeVersion("centralus", HealthState.Pending, null)));

    [Fact]
    public void AnUnreachableNodeIsUnreachable() =>
        Assert.Equal("eastus2 is unreachable", UnknownClause(new NodeVersion("eastus2", HealthState.Unreachable, "2.4.7")));

    [Fact]
    public void AHealthyNodeWithNoVersionReportsNoVersion() =>
        Assert.Equal("westus3 reports no version", UnknownClause(new NodeVersion("westus3", HealthState.Healthy, null)));

    [Fact]
    public void AnUnhealthyNodeWithNoVersionReportsNoVersion() =>
        Assert.Equal("westus3 reports no version", UnknownClause(new NodeVersion("westus3", HealthState.Unhealthy, null)));

    [Fact]
    public void ALabelWithSpacesIsPassedThroughUnchanged() =>
        Assert.Equal("north europe is unreachable", UnknownClause(new NodeVersion("north europe", HealthState.Unreachable, null)));

    [Theory]
    [InlineData(HealthState.Pending)]
    [InlineData(HealthState.Unreachable)]
    [InlineData(HealthState.Healthy)]
    [InlineData(HealthState.Unhealthy)]
    public void TheClauseIsTheLabelASpaceAndTheReason(HealthState state)
    {
        var node = new NodeVersion("westus3", state, null);

        Assert.Equal($"{node.Label} {node.UnknownReason}", UnknownClause(node));
    }
}
