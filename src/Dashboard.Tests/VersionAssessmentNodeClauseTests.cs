using static Dashboard.Tests.HealthStates;
using static Dashboard.Tests.Nodes;

namespace Dashboard.Tests;

public class VersionAssessmentNodeClauseTests
{
    [Fact]
    public void ThreeDifferingNodesAreJoinedWithCommaSpaceInHandedOrder()
    {
        var south = new NodeVersion("southcentralus", Healthy, "2.4.5");
        var assessment = Assess(West(Healthy, "2.4.8"), East(Healthy, "2.4.6"), south);

        Assert.Equal(VersionState.Differs, assessment.State);
        Assert.Equal(
            "Differs: westus3 runs 2.4.8, eastus2 runs 2.4.6, southcentralus runs 2.4.5.",
            assessment.Detail);
    }

    [Fact]
    public void ASingleInSyncNodeFollowedByTwoUnknownNodes()
    {
        var assessment = Assess(West(Healthy, "2.4.7"), East(Unreachable, null), Central(Pending, null));

        Assert.Equal(VersionState.InSync, assessment.State);
        Assert.Equal(
            "In sync: westus3 runs 2.4.7; eastus2 is unreachable, centralus is being checked.",
            assessment.Detail);
    }

    [Fact]
    public void NodesUnknownWithTwoUnknownNodesUsesColonSpaceSeparator()
    {
        var assessment = Assess(West(Unreachable, null), East(Pending, null));

        Assert.Equal(VersionState.NodesUnknown, assessment.State);
        Assert.Equal(
            "Not compared: westus3 is unreachable, eastus2 is being checked.",
            assessment.Detail);
    }
}
