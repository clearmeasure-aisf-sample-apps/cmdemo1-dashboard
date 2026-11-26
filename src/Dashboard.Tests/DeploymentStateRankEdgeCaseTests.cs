namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still open for <see cref="DeploymentState" />, declared in
/// <c>src/Dashboard/Health/DeploymentsReport.cs</c>, as the marks of a report rank and carry it.
/// <para>
/// <see cref="DeploymentStateEdgeCaseTests" /> already holds the enum's own edge cases (its numbers, names, order,
/// unset value, values outside the seven, enum parsing and its declared surface), and
/// <see cref="DeploymentStateWordEdgeCaseTests" /> the file's words in any case and culture, so these tests sit in a
/// file of their own rather than over either. <see cref="DeploymentsTests" /> pins the order of the marks, but only
/// for a report read from a file, which can give no state outside the seven.
/// </para>
/// <para>
/// Left open and pinned here, through a report built by hand: the whole rank across all nine places a state can
/// stand (an unknown state in flight between queued and failed, and one that ended, or a number outside the seven,
/// after succeeded in the report's own order); a number outside the seven with no end being neither in flight nor
/// marked; and each state carried into its mark as the very state it was, with its flight, its minutes, its
/// sentence, its tooltip and its dot.
/// </para>
/// <para>
/// Nothing here reaches the network or asks the clock: every moment is one the test supplies.
/// </para>
/// </summary>
public class DeploymentStateRankEdgeCaseTests
{
    /// <summary>Thursday a few minutes past four, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 4, 23, 24, TimeSpan.Zero);

    /// <summary>Three minutes before the page is asked: when a deployment in flight started.</summary>
    private static readonly DateTimeOffset Since = new(2026, 10, 8, 4, 20, 7, TimeSpan.Zero);

    /// <summary>Five minutes before the page is asked: when a deployment that ended ended.</summary>
    private static readonly DateTimeOffset Ended = new(2026, 10, 8, 4, 18, 0, TimeSpan.Zero);

    /// <summary>A deployment of <paramref name="project" /> to uat in <paramref name="state" />, said as <c>retired</c>.</summary>
    private static Deployment Of(string project, DeploymentState state, DateTimeOffset? finished) =>
        new(project, "uat", "1", state, "retired", Since, finished, null);

    private static IReadOnlyList<DeploymentMark> Marks(params Deployment[] deployments) =>
        new DeploymentsReport(Now, deployments).Marks("uat", ActivityLines.Slug, ActivityLines.Ui, Now);

    // ----- the rank

    /// <summary>Waiting, executing, queued, then a state the page does not know while it has no end, then failed,
    /// canceled and succeeded, then everything else that ended - an unknown state with an end and every number outside
    /// the seven - in the report's own order, whatever order the report gives the rest in.</summary>
    [Fact]
    public void EveryStateStandsInItsPlaceAndWhatThePageDoesNotNameComesLast()
    {
        var marks = Marks(
            Of("demo-beyond", (DeploymentState)7, Ended),
            Of("demo-succeeded", DeploymentState.Succeeded, Ended),
            Of("demo-unknown-ended", DeploymentState.Unknown, Ended),
            Of("demo-canceled", DeploymentState.Canceled, Ended),
            Of("demo-below", (DeploymentState)(-1), Ended),
            Of("demo-failed", DeploymentState.Failed, Ended),
            Of("demo-unknown", DeploymentState.Unknown, null),
            Of("demo-queued", DeploymentState.Queued, null),
            Of("demo-top", (DeploymentState)int.MaxValue, Ended),
            Of("demo-executing", DeploymentState.Executing, null),
            Of("demo-waiting", DeploymentState.Waiting, null));

        Assert.Equal(
            [
                "demo-waiting", "demo-executing", "demo-queued", "demo-unknown", "demo-failed", "demo-canceled",
                "demo-succeeded", "demo-beyond", "demo-unknown-ended", "demo-below", "demo-top",
            ],
            marks.Select(mark => mark.Project));
        Assert.Equal([true, true, true, true, false, false, false, false, false, false, false], marks.Select(mark => mark.InFlight));
    }

    /// <summary>The three in-flight states rank by state and not by end: an end the file gives a queued, executing or
    /// waiting deployment (a file that disagrees with itself) keeps it in flight and in its place.</summary>
    [Fact]
    public void AnEndDoesNotMoveAStateThatIsInFlight()
    {
        var marks = Marks(
            Of("demo-succeeded", DeploymentState.Succeeded, Ended),
            Of("demo-queued", DeploymentState.Queued, Ended),
            Of("demo-executing", DeploymentState.Executing, Ended),
            Of("demo-waiting", DeploymentState.Waiting, Ended));

        Assert.Equal(["demo-waiting", "demo-executing", "demo-queued", "demo-succeeded"], marks.Select(mark => mark.Project));
        Assert.Equal([true, true, true, false], marks.Select(mark => mark.InFlight));
    }

    /// <summary>A number outside the seven is never in flight, so without an end it is not marked at all - unlike
    /// <see cref="DeploymentState.Unknown" />, which without an end is in flight and marked.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void ANumberOutsideTheSevenWithoutAnEndIsNotMarked(int number)
    {
        Assert.Empty(Marks(Of("demo-ui", (DeploymentState)number, null)));
        Assert.Single(Marks(Of("demo-ui", DeploymentState.Unknown, null)));
    }

    // ----- the state carried into its mark

    /// <summary>Every state, and numbers outside the seven, are carried into the mark as the very state they were,
    /// with the deployment's own flight; marked when the deployment is marked and not otherwise.</summary>
    [Theory]
    [InlineData(DeploymentState.Unknown)]
    [InlineData(DeploymentState.Queued)]
    [InlineData(DeploymentState.Executing)]
    [InlineData(DeploymentState.Waiting)]
    [InlineData(DeploymentState.Succeeded)]
    [InlineData(DeploymentState.Failed)]
    [InlineData(DeploymentState.Canceled)]
    [InlineData((DeploymentState)(-1))]
    [InlineData((DeploymentState)7)]
    public void AMarkCarriesTheStateItWasGiven(DeploymentState state)
    {
        foreach (var finished in new DateTimeOffset?[] { Ended, null, Ended - DeploymentsReport.EndedFor })
        {
            var deployment = Of("demo-ui", state, finished);
            var marks = Marks(deployment);

            Assert.Equal(deployment.IsMarkedAt(Now) ? 1 : 0, marks.Count);
            Assert.All(marks, mark =>
            {
                Assert.Equal(state, mark.State);
                Assert.Equal(deployment.InFlight, mark.InFlight);
                Assert.Equal("ui", mark.Deployable);
                Assert.False(mark.OfSystem);
            });
        }
    }

    /// <summary>A number outside the seven that ended is said in the file's own word, counted from its end, and drawn
    /// with the dot of what ended; its tooltip is its sentence, with nothing so far.</summary>
    [Fact]
    public void ANumberOutsideTheSevenThatEndedIsSaidInTheFilesWord()
    {
        var mark = Assert.Single(Marks(Of("demo-ui", (DeploymentState)7, Ended)));

        Assert.Equal((DeploymentState)7, mark.State);
        Assert.Equal("demo-ui 1 in uat: retired", mark.Sentence);
        Assert.Equal(5, mark.Minutes);
        Assert.False(mark.InFlight);
        Assert.Null(mark.Detail);
        Assert.Equal("demo-ui 1 in uat: retired", DeploymentText.Title(mark));
        Assert.Equal("ended", DeploymentText.Shape(mark.State));
    }

    /// <summary>An unknown state without an end is in flight: counted from its start, and its tooltip says how long so
    /// far. With an end it is counted from the end and says nothing so far.</summary>
    [Fact]
    public void AnUnknownStateIsCountedFromItsStartInFlightAndFromItsEndOtherwise()
    {
        var running = Assert.Single(Marks(Of("demo-ui", DeploymentState.Unknown, null)));

        Assert.True(running.InFlight);
        Assert.Equal(3, running.Minutes);
        Assert.Equal("demo-ui 1 in uat: retired (3 min so far)", DeploymentText.Title(running));
        Assert.Equal("ended", DeploymentText.Shape(running.State));

        var ended = Assert.Single(Marks(Of("demo-ui", DeploymentState.Unknown, Ended)));

        Assert.False(ended.InFlight);
        Assert.Equal(5, ended.Minutes);
        Assert.Equal("demo-ui 1 in uat: retired", DeploymentText.Title(ended));
    }
}
