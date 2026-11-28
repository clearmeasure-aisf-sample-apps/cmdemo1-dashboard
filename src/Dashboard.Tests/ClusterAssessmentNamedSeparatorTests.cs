using static Dashboard.Tests.ClusterFixture;

namespace Dashboard.Tests;

/// <summary>
/// The separator <see cref="ClusterAssessment.Named"/> joins its named problems with: the same <c>; </c> between
/// the named items and before the <c>and N more</c> tail, named once as the private const
/// <c>ProblemSeparator</c> (#2348). The output is unchanged from before the const was named - these facts pin it.
/// </summary>
public class ClusterAssessmentNamedSeparatorTests
{
    // ----- NamedProblems items join with "; " and have no tail -----

    /// <summary>
    /// Exactly <see cref="ClusterAssessment.NamedProblems"/> items are joined with <c>"; </c> between them and
    /// have no <c>and N more</c> tail: <c>a; b; c</c>, not <c>a; b; c; and 0 more</c>.
    /// </summary>
    [Fact]
    public void ExactlyNamedProblemsItemsJoinWithSemicolonSpaceAndHaveNoTail()
    {
        Assert.Equal("a; b; c", ClusterAssessment.Named(["a", "b", "c"]));
        Assert.Equal("a; b; c", ClusterAssessment.Named(["a", "b", "c"]));
    }

    /// <summary>
    /// Three problems from a real status are joined with <c>; </c> between them in the Unhealthy assessment's
    /// detail: the separator is the same between every pair and no tail follows.
    /// </summary>
    [Fact]
    public void ThreeProblemsInAnUnhealthyAssessmentJoinWithSemicolonSpace()
    {
        var down = ClusterFixture.Node("aks-vm-000000", ready: false);
        var pressured = ClusterFixture.Node("aks-vm-100000", ready: true, "MemoryPressure");
        var broken = ClusterFixture.Pod("api", ready: false, restarts: 3, reason: "CrashLoopBackOff");

        var status = ClusterFixture.Status([down, pressured], ClusterFixture.Space("apps", broken));
        var assessment = ClusterAssessment.Live(ClusterFixture.Read(status), null, Now, Utc);

        Assert.Equal(ClusterState.Unhealthy, assessment.State);
        Assert.Equal(
            "Node aks-…000000 is not ready; Node aks-…100000: memory pressure; api in apps: CrashLoopBackOff, 3 restarts",
            assessment.Headline);
    }

    // ----- NamedProblems + 1 items end with "; and 1 more" -----

    /// <summary>
    /// One more than <see cref="ClusterAssessment.NamedProblems"/> items ends with <c>"; and 1 more</c>: the
    /// separator before the tail is the same <c>; </c> as between the named items.
    /// </summary>
    [Fact]
    public void NamedProblemsPlusOneEndsWithSemicolonSpaceAndOneMore()
    {
        Assert.Equal("a; b; c; and 1 more", ClusterAssessment.Named(["a", "b", "c", "d"]));
        Assert.Equal("a; b; c; and 1 more", ClusterAssessment.Named(["a", "b", "c", "d"]));
    }

    /// <summary>
    /// Five items name three and count two: <c>a; b; c; and 2 more</c> - the tail follows the same
    /// <c>; </c> separator as the items before it.
    /// </summary>
    [Fact]
    public void FiveItemsNameThreeAndCountTwo()
    {
        Assert.Equal("a; b; c; and 2 more", ClusterAssessment.Named(["a", "b", "c", "d", "e"]));
    }

    // ----- A single item is returned unchanged -----

    /// <summary>
    /// One item has no separator: it is returned as it is, with no <c>; </c> before or after it.
    /// </summary>
    [Fact]
    public void ASingleItemIsReturnedUnchanged()
    {
        Assert.Equal("a", ClusterAssessment.Named(["a"]));
    }

    /// <summary>
    /// Two items are joined with <c>; </c> between them and have no tail: <c>a; b</c>.
    /// </summary>
    [Fact]
    public void TwoItemsJoinWithSemicolonSpaceAndHaveNoTail()
    {
        Assert.Equal("a; b", ClusterAssessment.Named(["a", "b"]));
    }

    // ----- The Unhealthy arm and the starting-pods sentence both use "; " -----

    /// <summary>
    /// The Unhealthy assessment's detail, built through <see cref="ClusterAssessment.Named"/> on the Unhealthy
    /// arm, joins its problems with <c>; </c>: a status with four problems names three and counts the fourth,
    /// with the same separator between the named ones and before the tail.
    /// </summary>
    [Fact]
    public void TheUnhealthyArmJoinsProblemsWithSemicolonSpace()
    {
        var down = ClusterFixture.Node("aks-vm-000000", ready: false);
        var pressured = ClusterFixture.Node("aks-vm-100000", ready: true, "MemoryPressure");
        var broken1 = ClusterFixture.Pod("api", ready: false, restarts: 3, reason: "CrashLoopBackOff");
        var broken2 = ClusterFixture.Pod("web", ready: false, reason: "Error");

        var status = ClusterFixture.Status([down, pressured], ClusterFixture.Space("apps", broken1, broken2));
        var assessment = ClusterAssessment.Live(ClusterFixture.Read(status), null, Now, Utc);

        Assert.Equal(ClusterState.Unhealthy, assessment.State);
        Assert.NotNull(assessment.Headline);
        Assert.Contains("; ", assessment.Headline);
        Assert.EndsWith("; and 1 more", assessment.Headline);
    }

    /// <summary>
    /// The starting-pods sentence, built through <see cref="ClusterAssessment.Named"/> on the Starting arm, joins
    /// its pod names with <c>; </c>: <c>3 pods are starting: ui in apps; api in apps; web in apps</c>.
    /// </summary>
    [Fact]
    public void TheStartingPodsSentenceJoinsPodNamesWithSemicolonSpace()
    {
        var pod1 = ClusterFixture.Pod("ui", ready: false, age: TimeSpan.FromSeconds(30));
        var pod2 = ClusterFixture.Pod("api", ready: false, age: TimeSpan.FromSeconds(45));
        var pod3 = ClusterFixture.Pod("web", ready: false, age: TimeSpan.FromSeconds(60));

        var status = ClusterFixture.Status(null, ClusterFixture.Space("apps", pod1, pod2, pod3));
        var assessment = ClusterAssessment.Live(ClusterFixture.Read(status), null, Now, Utc);

        Assert.Equal(ClusterState.Checking, assessment.State);
        Assert.Equal(
            "3 pods are starting: ui in apps; api in apps; web in apps",
            assessment.Headline);
    }

    /// <summary>
    /// The starting-pods sentence with more than <see cref="ClusterAssessment.NamedProblems"/> pods ends with
    /// <c>; and N more</c>: the tail follows the same <c>; </c> separator.
    /// </summary>
    [Fact]
    public void TheStartingPodsSentenceWithMoreThanNamedProblemsEndsWithTail()
    {
        var pods = new[]
        {
            ClusterFixture.Pod("p1", ready: false, age: TimeSpan.FromSeconds(10)),
            ClusterFixture.Pod("p2", ready: false, age: TimeSpan.FromSeconds(20)),
            ClusterFixture.Pod("p3", ready: false, age: TimeSpan.FromSeconds(30)),
            ClusterFixture.Pod("p4", ready: false, age: TimeSpan.FromSeconds(40)),
            ClusterFixture.Pod("p5", ready: false, age: TimeSpan.FromSeconds(50)),
        };

        var status = ClusterFixture.Status(null, ClusterFixture.Space("apps", pods));
        var assessment = ClusterAssessment.Live(ClusterFixture.Read(status), null, Now, Utc);

        Assert.Equal(ClusterState.Checking, assessment.State);
        Assert.Equal(
            "5 pods are starting: p1 in apps; p2 in apps; p3 in apps; and 2 more",
            assessment.Headline);
    }
}
