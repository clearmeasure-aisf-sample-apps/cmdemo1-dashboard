namespace Dashboard.Tests;

/// <summary>
/// <see cref="NodeRole.IsPrimary" /> is the one rule for which role word names a primary node, and the node, the
/// tile and the application view all ask it: a role spelled any way is decided one way only, whoever asks.
/// </summary>
public class NodeRoleTests
{
    private static readonly Uri West = new("https://app-west.example.net");

    [Theory]
    [InlineData("primary", true)]
    [InlineData("Primary", true)]
    [InlineData("PRIMARY", true)]
    [InlineData(" PRIMARY ", false)]
    [InlineData(" primary", false)]
    [InlineData("standby", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TheNodeAndTheTileAskTheOneRule(string? role, bool expected)
    {
        Assert.Equal(expected, NodeRole.IsPrimary(role));
        Assert.Equal(expected, new NodeInfo("app", "westus3", role!, West).IsPrimary);

        var tile = new TargetStatus(TargetKind.Node, "app", West, "westus3", role);
        Assert.Equal(expected, tile.IsPrimary);
        Assert.Equal(expected, tile.ToNodeHealth().IsPrimary);
    }

    [Fact]
    public void TheRuleWeighsTheRoleAgainstTheWordTheTopologyWrites()
    {
        Assert.True(NodeRole.IsPrimary(NodeInfo.PrimaryRole));
        Assert.False(NodeRole.IsPrimary(NodeInfo.StandbyRole));
    }
}
