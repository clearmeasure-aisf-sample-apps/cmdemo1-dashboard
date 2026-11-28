using static Dashboard.Tests.HealthStates;
using static Dashboard.Tests.Nodes;

namespace Dashboard.Tests;

public class NodeVersionUnknownClauseTests
{
    [Fact]
    public void ThePendingClauseNamesCentralus() =>
        Assert.Equal("centralus is being checked", Central(Pending, null).UnknownClause);

    [Fact]
    public void TheUnreachableClauseNamesEastus2() =>
        Assert.Equal("eastus2 is unreachable", East(Unreachable, null).UnknownClause);

    [Theory]
    [InlineData(Healthy, "westus3 reports no version")]
    [InlineData(Unhealthy, "westus3 reports no version")]
    [InlineData(Unnamed, "westus3 reports no version")]
    public void TheReportsNoVersionClauseNamesWestus3(HealthState state, string clause) =>
        Assert.Equal(clause, West(state, null).UnknownClause);

    [Theory]
    [InlineData(Pending)]
    [InlineData(Unreachable)]
    [InlineData(Healthy)]
    [InlineData(Unhealthy)]
    [InlineData(Unnamed)]
    public void TheClauseEqualsLabelPlusReason(HealthState state)
    {
        var node = West(state, "2.4.7");

        Assert.Equal($"{node.Label} {node.UnknownReason}", node.UnknownClause);
    }

    [Fact]
    public void TheClauseIsTheWholeSentenceTheReasonPromises()
    {
        // UnknownReason's doc says it is "the end of a sentence that starts with the label"; UnknownClause is that
        // whole sentence.
        var west = West(Healthy, null);
        var east = East(Unreachable, "2.4.6");
        var central = Central(Pending, null);

        Assert.Equal("westus3 reports no version", west.UnknownClause);
        Assert.Equal("eastus2 is unreachable", east.UnknownClause);
        Assert.Equal("centralus is being checked", central.UnknownClause);

        // The reason is still just the tail; the clause is label plus reason.
        Assert.Equal("reports no version", west.UnknownReason);
        Assert.Equal("is unreachable", east.UnknownReason);
        Assert.Equal("is being checked", central.UnknownReason);
    }
}
