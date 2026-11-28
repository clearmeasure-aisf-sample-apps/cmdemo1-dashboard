namespace Dashboard.Tests;

/// <summary>
/// The one home of the dashboard's interval file reads (#1804), in the words the rule itself states: nothing is
/// due before it was first asked, and once it was, it is due again only when its interval has passed since the
/// last asking, a read exactly one interval later going ahead; the asking is stamped before the read, so a slow
/// answer does not buy a second asking; a read that answers nothing keeps the last good answer. The database
/// view's live-file read and the monitor's file reads are on it - <see cref="DatabaseViewTests" /> pins the view's
/// read on the home, <see cref="DashboardMonitorTests" /> pins the monitor's six file reads on it, and the
/// interval the two bring is pinned where it stands, in <see cref="IntervalDueEdgeCaseTests" /> and
/// <see cref="DashboardMonitorEdgeCaseTests" />.
/// </summary>
public class IntervalReadTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private static readonly CancellationToken Never = CancellationToken.None;

    private static Task<string?> Answer(string? answer) => Task.FromResult(answer);

    /// <summary>Nothing is due before it was first asked, and once it was, only when the interval has passed -
    /// the boundary inclusive: a read exactly one interval later goes ahead, one second short of it does not.</summary>
    [Fact]
    public async Task TheFirstAskingIsDueAndTheNextIsDueOnlyWhenTheIntervalHasPassed()
    {
        var read = new IntervalRead<string>(Interval);

        Assert.True(read.IsDue(At), "a read that was never asked is due");
        Assert.Null(read.LastAsked);

        Assert.Equal("one", await read.ReadIfDueAsync(At, _ => Answer("one"), Never));
        Assert.Equal(At, read.LastAsked);

        Assert.False(read.IsDue(At.AddSeconds(59)));
        Assert.True(read.IsDue(At.AddSeconds(60)));
    }

    /// <summary>A read that answers nothing keeps the last good answer, and says so: it keeps nothing, and the
    /// view keeps showing what it last showed.</summary>
    [Fact]
    public async Task AReadingThatAnswersNothingKeepsTheLastGoodOne()
    {
        var read = new IntervalRead<string>(Interval);

        Assert.Equal("one", await read.ReadIfDueAsync(At, _ => Answer("one"), Never));
        Assert.Null(await read.ReadIfDueAsync(At.AddSeconds(60), _ => Answer(null), Never));

        Assert.Equal("one", read.Last);
        Assert.True(read.Asked);
    }

    /// <summary>A read that answers keeps the answer it kept, replacing what it kept before.</summary>
    [Fact]
    public async Task AReadingThatAnswersKeepsTheAnswerItKept()
    {
        var read = new IntervalRead<string>(Interval);

        Assert.Equal("one", await read.ReadIfDueAsync(At, _ => Answer("one"), Never));
        Assert.Equal("two", await read.ReadIfDueAsync(At.AddSeconds(60), _ => Answer("two"), Never));

        Assert.Equal("two", read.Last);
    }

    /// <summary>The asking is stamped before the read: while the answer is still out, the stamp stands, so a
    /// second asking within the interval makes no read - the slow answer has bought no second asking - and the
    /// late answer does not move the stamp.</summary>
    [Fact]
    public async Task ASlowAnswerDoesNotBuyASecondAsking()
    {
        var read = new IntervalRead<string>(Interval);
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var asking = read.ReadIfDueAsync(At, _ => pending.Task, Never);
        var asked = 0;

        Assert.Equal(At, read.LastAsked);
        Assert.Null(await read.ReadIfDueAsync(At.AddSeconds(30), _ => { asked++; return Answer("second"); }, Never));
        Assert.Equal(0, asked);

        pending.SetResult("one");
        Assert.Equal("one", await asking);
        Assert.Equal(At, read.LastAsked);
        Assert.False(read.IsDue(At.AddSeconds(59)));
        Assert.True(read.IsDue(At.AddSeconds(60)));
    }

    /// <summary>A read that is not due asks nothing: no read is made, nothing is kept and the stamp stands where
    /// the last asking set it.</summary>
    [Fact]
    public async Task AReadingThatIsNotDueAsksNothing()
    {
        var read = new IntervalRead<string>(Interval);
        _ = await read.ReadIfDueAsync(At, _ => Answer("one"), Never);
        var asked = 0;

        Assert.Null(await read.ReadIfDueAsync(At.AddSeconds(30), _ => { asked++; return Answer("two"); }, Never));

        Assert.Equal(0, asked);
        Assert.Equal("one", read.Last);
        Assert.Equal(At, read.LastAsked);
    }

    /// <summary>Asked stands once a reading has come back - kept or not - and never unset; a read that is not due
    /// has not been made.</summary>
    [Fact]
    public async Task AskedStandsOnceAReadingHasComeBackKeptOrNot()
    {
        var read = new IntervalRead<string>(Interval);

        Assert.False(read.Asked);

        Assert.Null(await read.ReadIfDueAsync(At, _ => Answer(null), Never));
        Assert.True(read.Asked, "a reading that found nothing has still been made");
        Assert.Null(read.Last);

        var asked = 0;
        Assert.Null(await read.ReadIfDueAsync(At.AddSeconds(30), _ => { asked++; return Answer(null); }, Never));
        Assert.Equal(0, asked);
    }

    /// <summary>The ask takes the answer as it is, kept or not - the read whose no-answer is itself the answer -
    /// and keeps no last good one of its own: the answer it takes stands where the caller keeps it.</summary>
    [Fact]
    public async Task AskTakesTheAnswerAsItIsAndKeepsNoLastGoodOne()
    {
        var read = new IntervalRead<string>(Interval);

        Assert.Equal("one", await read.AskIfDueAsync(At, _ => Answer("one"), Never));
        Assert.Null(read.Last);

        Assert.Null(await read.AskIfDueAsync(At.AddSeconds(60), _ => Answer(null), Never));
        Assert.Null(read.Last);
        Assert.True(read.Asked);
    }

    /// <summary>The ask, like the read, asks where it is due only: a second ask before the interval has passed
    /// makes no read, and a failed ask has stamped the asking all the same.</summary>
    [Fact]
    public async Task AnAskThatIsNotDueAsksNothing()
    {
        var read = new IntervalRead<string>(Interval);
        _ = await read.AskIfDueAsync(At, _ => Answer("one"), Never);
        var asked = 0;

        Assert.Null(await read.AskIfDueAsync(At.AddSeconds(30), _ => { asked++; return Answer("two"); }, Never));
        Assert.Equal(0, asked);

        await read.AskIfDueAsync(At.AddSeconds(60), _ => Answer(null), Never);
        Assert.False(read.IsDue(At.AddSeconds(61)), "the failed ask stamped its asking");
        Assert.True(read.IsDue(At.AddSeconds(120)));
    }

    /// <summary>A reading that never comes back - the round cancelled - throws the cancellation, keeps the last
    /// good one, and leaves the stamp where the asking set it, so the next asking is due only when the interval
    /// has passed since the cancelled one.</summary>
    [Fact]
    public async Task AReadingThatNeverComesBackKeepsTheLastGoodOneAndLeavesTheStampWhereTheAskingSetIt()
    {
        var read = new IntervalRead<string>(Interval);
        _ = await read.ReadIfDueAsync(At, _ => Answer("one"), Never);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => read.ReadIfDueAsync(At.AddSeconds(60), token => Task.FromCanceled<string?>(token), cancelled.Token));

        Assert.Equal("one", read.Last);
        Assert.Equal(At.AddSeconds(60), read.LastAsked);
        Assert.True(read.IsDue(At.AddSeconds(120)));
        Assert.False(read.IsDue(At.AddSeconds(119)));
    }

    /// <summary>...and it has not been made: asked does not stand for a reading that did not come back, not even
    /// long after the asking.</summary>
    [Fact]
    public async Task AReadingThatNeverComesBackHasNotBeenMade()
    {
        var read = new IntervalRead<string>(Interval);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => read.ReadIfDueAsync(At, token => Task.FromCanceled<string?>(token), cancelled.Token));

        Assert.Null(read.Last);
        Assert.Equal(At, read.LastAsked);
        Assert.False(read.Asked);
    }

    /// <summary>The read and the ask share one stamp: an ask after a read within the interval asks nothing, because
    /// the read stamped the asking and the interval has not passed since it.</summary>
    [Fact]
    public async Task AnAskAfterAReadWithinTheIntervalAsksNothing()
    {
        var read = new IntervalRead<string>(Interval);
        var asked = 0;

        Assert.Equal("one", await read.ReadIfDueAsync(At, _ => Answer("one"), Never));
        Assert.Null(await read.AskIfDueAsync(At.AddSeconds(30), _ => { asked++; return Answer("two"); }, Never));

        Assert.Equal(0, asked);
        Assert.Equal(At, read.LastAsked);
    }

    /// <summary>An ask that answers never sets <c>Last</c>, even after an earlier read kept one: the last good one
    /// the read kept stands.</summary>
    [Fact]
    public async Task AnAskThatAnswersNeverSetsLast()
    {
        var read = new IntervalRead<string>(Interval);

        Assert.Equal("one", await read.ReadIfDueAsync(At, _ => Answer("one"), Never));
        Assert.Equal("two", await read.AskIfDueAsync(At.AddSeconds(60), _ => Answer("two"), Never));

        Assert.Equal("one", read.Last);
    }

    /// <summary>A read whose reading throws leaves <c>Asked</c> false and the stamp at the asking, as the ask does:
    /// the stamp stands, the reading has not been made.</summary>
    [Fact]
    public async Task AReadWhoseReadingThrowsLeavesAskedFalseAndTheStampAtTheAsking()
    {
        var read = new IntervalRead<string>(Interval);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => read.ReadIfDueAsync(At, _ => throw new InvalidOperationException("the read failed"), Never));

        Assert.False(read.Asked);
        Assert.Equal(At, read.LastAsked);
    }
}
