namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still left open for <see cref="Deployment" />, declared in
/// <c>src/Dashboard/Health/DeploymentsReport.cs</c>.
/// <para>
/// <see cref="DeploymentEdgeCaseTests" /> and <see cref="DeploymentBoundaryEdgeCaseTests" /> already pin the record's
/// ten parts, its equality, hash, copy and words, the flight state by state, and the ten minutes of the marking to the
/// tick, at both ends of the clock and in another hour of the end. Neither is replaced or repeated here.
/// </para>
/// <para>
/// Left open by both, and pinned here: a deployment that <see cref="DeploymentState.Succeeded" /> with no end the page
/// can read is marked at no moment (the walk of the stranded states there names failed, canceled and a number beyond
/// the names, never succeeded); the moment a deployment started has nothing to say about the marking of one that
/// ended, neither when it stands inside the ten minutes while the end does not, nor when it is missing, nor when the
/// file wrote it after the end; and the ten minutes close on the same instant when the moment the page is asked at,
/// rather than the end, is spelled in another hour.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class DeploymentStartAndEndEdgeCaseTests
{
    /// <summary>Thursday a few minutes past four, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 4, 23, 24, TimeSpan.Zero);

    /// <summary>Five minutes before the page is asked: the moment a deployment that ended ended.</summary>
    private static readonly DateTimeOffset Ended = new(2026, 10, 8, 4, 18, 0, TimeSpan.Zero);

    /// <summary>The hours five and a half east of the meridian.</summary>
    private static readonly TimeSpan East = TimeSpan.FromMinutes(330);

    /// <summary>The hours eight west of the meridian.</summary>
    private static readonly TimeSpan West = TimeSpan.FromHours(-8);

    /// <summary>One deployment of a state and its two moments, of nothing else.</summary>
    private static Deployment Of(DeploymentState state, DateTimeOffset? since, DateTimeOffset? finished) =>
        new("demo-ui", "uat", "1", state, state.ToString().ToLowerInvariant(), since, finished, null);

    // ----- a success the page cannot date

    /// <summary>A deployment that succeeded at a time the page cannot read has ended and has no window to stand in:
    /// it is marked at no moment at all, at either end of the clock, at the moment it started, or now. Given the end,
    /// the same deployment is marked now.</summary>
    [Fact]
    public void ASuccessWithNoEndIsMarkedAtNoMoment()
    {
        var succeeded = Of(DeploymentState.Succeeded, Ended.AddMinutes(-3), null);

        Assert.False(succeeded.InFlight);
        Assert.False(succeeded.IsMarkedAt(Now));
        Assert.False(succeeded.IsMarkedAt(Ended));
        Assert.False(succeeded.IsMarkedAt(Ended.AddMinutes(-3)));
        Assert.False(succeeded.IsMarkedAt(DateTimeOffset.MinValue));
        Assert.False(succeeded.IsMarkedAt(DateTimeOffset.MaxValue));

        Assert.True((succeeded with { Finished = Ended }).IsMarkedAt(Now));
    }

    // ----- the moment it started, which the marking of an ended deployment never asks

    /// <summary>The start of a deployment that ended decides nothing of its marking: a start inside the ten minutes
    /// does not keep an end outside them marked, no start at all does not unmark an end inside them, and a start the
    /// file wrote after the end (a file that disagrees with itself) changes neither answer.</summary>
    [Theory]
    [InlineData(DeploymentState.Succeeded)]
    [InlineData(DeploymentState.Failed)]
    [InlineData(DeploymentState.Canceled)]
    [InlineData(DeploymentState.Unknown)]
    public void TheStartOfAnEndedDeploymentDecidesNothingOfItsMarking(DeploymentState state)
    {
        var longAgo = Ended.AddHours(-1);

        // An end an hour old, whatever the start.
        Assert.False(Of(state, Now, longAgo).IsMarkedAt(Now));
        Assert.False(Of(state, Now.AddMinutes(-1), longAgo).IsMarkedAt(Now));
        Assert.False(Of(state, null, longAgo).IsMarkedAt(Now));
        Assert.False(Of(state, longAgo.AddMinutes(-5), longAgo).IsMarkedAt(Now));

        // An end five minutes old, whatever the start.
        Assert.True(Of(state, null, Ended).IsMarkedAt(Now));
        Assert.True(Of(state, Ended.AddDays(-30), Ended).IsMarkedAt(Now));
        Assert.True(Of(state, Ended.AddDays(30), Ended).IsMarkedAt(Now));
        Assert.True(Of(state, DateTimeOffset.MinValue, Ended).IsMarkedAt(Now));
        Assert.True(Of(state, DateTimeOffset.MaxValue, Ended).IsMarkedAt(Now));

        // The window closes on the end's instant alone, with or without a start.
        Assert.False(Of(state, null, Ended).IsMarkedAt(Ended + DeploymentsReport.EndedFor));
        Assert.False(Of(state, Ended + DeploymentsReport.EndedFor, Ended).IsMarkedAt(Ended + DeploymentsReport.EndedFor));
        Assert.True(Of(state, null, Ended).IsMarkedAt(Ended + DeploymentsReport.EndedFor - TimeSpan.FromTicks(1)));

        // The start is never what ends the flight either.
        Assert.False(Of(state, null, Ended).InFlight);
        Assert.False(Of(state, Now, Ended).InFlight);
    }

    /// <summary>For a deployment in flight the start is no more a part of the marking than for one that ended: with a
    /// start, with none, and with a start after the moment asked, it is marked.</summary>
    [Theory]
    [InlineData(DeploymentState.Queued)]
    [InlineData(DeploymentState.Executing)]
    [InlineData(DeploymentState.Waiting)]
    public void TheStartOfADeploymentInFlightDecidesNothingOfItsMarking(DeploymentState state)
    {
        Assert.True(Of(state, null, null).IsMarkedAt(Now));
        Assert.True(Of(state, Now.AddYears(1), null).IsMarkedAt(Now));
        Assert.True(Of(state, Now.AddYears(-1), null).IsMarkedAt(Now));
        Assert.True(Of(state, DateTimeOffset.MinValue, null).IsMarkedAt(DateTimeOffset.MaxValue));
    }

    // ----- the moment asked, spelled in another hour

    /// <summary>The ten minutes close on the same instant whichever hour the moment the page is asked at is written in,
    /// east or west of the meridian: a tick before, marked; at the tenth minute and after, not; and the answer is the
    /// answer of the same instant written in UTC.</summary>
    [Fact]
    public void TheTenMinutesCloseOnTheSameInstantWhicheverHourTheAskingIsSpelledIn()
    {
        var failed = Of(DeploymentState.Failed, Ended.AddMinutes(-2), Ended);
        var closed = Ended + DeploymentsReport.EndedFor;
        var open = closed - TimeSpan.FromTicks(1);

        foreach (var offset in new[] { TimeSpan.Zero, East, West, TimeSpan.FromHours(14), TimeSpan.FromHours(-14) })
        {
            Assert.Equal(offset, open.ToOffset(offset).Offset);
            Assert.True(failed.IsMarkedAt(open.ToOffset(offset)));
            Assert.False(failed.IsMarkedAt(closed.ToOffset(offset)));
            Assert.False(failed.IsMarkedAt(closed.AddSeconds(1).ToOffset(offset)));
            Assert.True(failed.IsMarkedAt(Ended.ToOffset(offset)));
        }

        // The same wall-clock reading in another hour is another instant, and so another answer: 04:27:59 in the west
        // is eight hours after the end, long past the window.
        var sameWallClockInTheWest = new DateTimeOffset(open.DateTime, West);

        Assert.Equal(open.DateTime, sameWallClockInTheWest.DateTime);
        Assert.False(failed.IsMarkedAt(sameWallClockInTheWest));
    }

    /// <summary>The length the marking is counted in is the fixed ten minutes: exactly 6,000,000,000 ticks.</summary>
    [Fact]
    public void TheWindowIsExactlyTenMinutesOfTicks()
    {
        Assert.Equal(6_000_000_000L, DeploymentsReport.EndedFor.Ticks);

        var failed = Of(DeploymentState.Failed, null, Ended);

        Assert.True(failed.IsMarkedAt(Ended.AddTicks(5_999_999_999L)));
        Assert.False(failed.IsMarkedAt(Ended.AddTicks(6_000_000_000L)));
    }
}
