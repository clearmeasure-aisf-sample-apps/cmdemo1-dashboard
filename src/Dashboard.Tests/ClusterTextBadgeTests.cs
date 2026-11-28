namespace Dashboard.Tests;

/// <summary>
/// The state and word each cluster table's badge shows, on <see cref="ClusterText"/> where the tables' other words
/// live: <see cref="ClusterText.PodBadge" /> for a pod row's colour, <see cref="ClusterText.NodeBadge" /> and
/// <see cref="ClusterText.NodeWords" /> for a node row's colour and word. Both rules were private in the markup
/// (<c>PodTable.razor</c>'s <c>StateOf</c>, <c>ClusterNodes.razor</c>'s inline ternaries), where no unit test could
/// call them; they are pinned arm for arm here, and the output must stay the same for every input: ready is
/// <c>Healthy</c>/<c>Ready</c>, a node that is not ready is <c>Unreachable</c>/<c>NotReady</c>, a starting pod is
/// <c>Checking</c>, an unhealthy one <c>Unhealthy</c>, and a finished job stays <c>Neutral</c>.
/// </summary>
public class ClusterTextBadgeTests
{
    // ----- PodBadge: the pod row's state -----

    /// <summary>Every pod state through the badge, arm for arm: ready is healthy, starting is checking, unhealthy is unhealthy.</summary>
    [Theory]
    [InlineData(PodState.Ready, ClusterState.Healthy)]
    [InlineData(PodState.Starting, ClusterState.Checking)]
    [InlineData(PodState.Unhealthy, ClusterState.Unhealthy)]
    public void EveryPodStateHasItsBadge(PodState state, ClusterState expected)
    {
        Assert.Equal(expected, ClusterText.PodBadge(state));
    }

    /// <summary>A finished job is no problem and no warning: its badge stays neutral.</summary>
    [Fact]
    public void AFinishedPodStaysNeutral()
    {
        Assert.Equal(ClusterState.Neutral, ClusterText.PodBadge(PodState.Finished));
    }

    // ----- NodeBadge: the node row's state -----

    /// <summary>A ready node is healthy; a node that is not ready is unreachable.</summary>
    [Theory]
    [InlineData(true, ClusterState.Healthy)]
    [InlineData(false, ClusterState.Unreachable)]
    public void TheNodeBadgeFollowsItsReadiness(bool ready, ClusterState expected)
    {
        var node = ClusterFixture.Node() with { Ready = ready };
        Assert.Equal(expected, ClusterText.NodeBadge(node));
    }

    /// <summary>The null argument is named at the throw.</summary>
    [Fact]
    public void ANullNodeIsNamedAtTheBadge()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.NodeBadge(null!));
        Assert.Equal("node", thrown.ParamName);
    }

    // ----- NodeWords: the node row's word -----

    /// <summary>The badge says <c>Ready</c> for a ready node and the Kubernetes condition word <c>NotReady</c> otherwise.</summary>
    [Theory]
    [InlineData(true, "Ready")]
    [InlineData(false, "NotReady")]
    public void TheNodeWordsFollowItsReadiness(bool ready, string expected)
    {
        var node = ClusterFixture.Node() with { Ready = ready };
        Assert.Equal(expected, ClusterText.NodeWords(node));
    }

    /// <summary>The null argument is named at the throw.</summary>
    [Fact]
    public void ANullNodeIsNamedAtTheWords()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.NodeWords(null!));
        Assert.Equal("node", thrown.ParamName);
    }

    // ----- PodWords: the shared "Ready" word -----

    /// <summary>A ready pod still says <c>Ready</c> in its row's words, from the same word the node badge reads.</summary>
    [Fact]
    public void AReadyPodStillSaysReady()
    {
        var pod = ClusterFixture.Pod("ui");
        Assert.Equal("Ready", ClusterText.PodWords(pod, PodState.Ready, DateTimeOffset.UnixEpoch));
    }
}
