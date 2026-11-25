using Microsoft.Extensions.Time.Testing;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="Poller" />: the loop that runs a check, waits
/// for the interval and repeats while the dashboard is neither paused nor hidden. What is pinned here is what
/// <see cref="PollerTests" /> leaves open, and only that: the constructor keeping any interval it is given -
/// the zero and the negative are checked by <see cref="Poller.SetInterval" />, not by the constructor - and
/// the interval read back as the very ticks it was; the three states of a poller that has never been started;
/// the constructor refusing nothing until a null check is run and a null clock is asked; the interval at its
/// two ends and a refused interval leaving the one it would replace; a new interval made while paused; a
/// pause, a resume and a visibility set that changes nothing waking the loop at most once; a check requested
/// before the run, while paused and during a check; and the run itself - the token the check is given, a
/// failing check failing the run with its own failure, a token cancelled before and during the run, a hidden
/// poller never asking the clock, and the clock being asked for exactly the interval the poller holds.
/// <para>
/// What <see cref="PollerTests" /> already pins is left alone: the first check at once and the checks after
/// every interval, the pause and the hide with their checks at once as they end, the new interval checking at
/// once, the same interval changing nothing, the check now at once, paused and hidden, and the pause during a
/// check.
/// </para>
/// <para>
/// Nothing here reaches the network: every check is one the test counts, and every clock is a fake the test
/// advances.
/// </para>
/// </summary>
public sealed class PollerEdgeCaseTests : IAsyncDisposable
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Quiet = TimeSpan.FromMilliseconds(150);

    private readonly SignallingTimeProvider _time = new();
    private readonly SemaphoreSlim _checks = new(0);
    private readonly CancellationTokenSource _stop = new();
    private readonly Poller _poller;
    private Task _running = Task.CompletedTask;
    private int _count;

    public PollerEdgeCaseTests()
    {
        _poller = new Poller(
            async _ =>
            {
                Interlocked.Increment(ref _count);
                _checks.Release();
            },
            _time,
            Interval);
    }

    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _running.WaitAsync(Patience));
        _stop.Dispose();
        _checks.Dispose();
    }

    private void Start() => _running = _poller.RunAsync(_stop.Token);

    private async Task CheckRanAsync() =>
        Assert.True(await _checks.WaitAsync(Patience), "The poller did not run a check.");

    /// <summary>The first check ran and the poller waits for the interval.</summary>
    private async Task StartAndWaitForTheFirstIntervalAsync()
    {
        Start();
        await CheckRanAsync();
        await _time.TimerCreatedAsync();
    }

    // ------------------------------------------------- the constructor and the fresh state

    /// <summary>
    /// The interval is checked by <see cref="Poller.SetInterval" />, not by the constructor: the zero and the
    /// negative are kept as the values they are, beside the smallest positive and the two that do not divide
    /// into a second.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1234567)]
    [InlineData(999999999)]
    public void TheConstructorKeepsAnyIntervalItIsGiven(long ticks)
    {
        var interval = TimeSpan.FromTicks(ticks);
        var poller = new Poller(_ => Task.CompletedTask, TimeProvider.System, interval);

        Assert.Equal(interval, poller.Interval);
    }

    /// <summary>A poller that has never been started is not paused, is visible and is active, with the
    /// interval it was given standing where it was set.</summary>
    [Fact]
    public void AFreshPollerIsNotPausedIsVisibleAndIsActive()
    {
        var poller = new Poller(_ => Task.CompletedTask, TimeProvider.System, Interval);

        Assert.Equal(Interval, poller.Interval);
        Assert.False(poller.IsPaused);
        Assert.True(poller.IsVisible);
        Assert.True(poller.IsActive);
    }

    /// <summary>The constructor refuses nothing: the null check is only refused when the poller runs it.</summary>
    [Fact]
    public async Task ANullCheckIsRefusedOnlyWhenThePollerRunsIt()
    {
        using var stop = new CancellationTokenSource();
        var poller = new Poller(null!, TimeProvider.System, Interval);

        var running = poller.RunAsync(stop.Token);

        await Assert.ThrowsAsync<NullReferenceException>(() => running.WaitAsync(Patience));
        stop.Cancel();
    }

    /// <summary>The null clock is only refused when the poller asks it: the first check runs, the first wait
    /// does not.</summary>
    [Fact]
    public async Task ANullClockIsRefusedOnlyWhenThePollerAsksIt()
    {
        using var stop = new CancellationTokenSource();
        var checkedOnce = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        TimeProvider? noClock = null;
        var poller = new Poller(_ =>
            {
                checkedOnce.TrySetResult();
                return Task.CompletedTask;
            }, noClock!, Interval);

        var running = poller.RunAsync(stop.Token);

        await checkedOnce.Task.WaitAsync(Patience);
        await Assert.ThrowsAsync<ArgumentNullException>(() => running.WaitAsync(Patience));
        stop.Cancel();
    }

    // ---------------------------------------------------------------- the interval at its ends

    /// <summary>
    /// The interval at the end the constructor does not check and <see cref="Poller.SetInterval" /> does: the
    /// one tick is the smallest interval the poller may hold, the largest ticks the largest, and the odd ticks
    /// are kept as the values they are.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(1234567)]
    [InlineData(long.MaxValue)]
    public void IntervalsOfOneTickOrMoreAreKeptAsGiven(long ticks)
    {
        var poller = new Poller(_ => Task.CompletedTask, TimeProvider.System, Interval);
        var interval = TimeSpan.FromTicks(ticks);

        poller.SetInterval(interval);

        Assert.Equal(interval, poller.Interval);
    }

    /// <summary>The intervals of no ticks or fewer are refused: the one tick below the zero and the smallest
    /// the type holds.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void IntervalsOfNoTicksOrFewerAreRefused(long ticks) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Poller(_ => Task.CompletedTask, TimeProvider.System, Interval).SetInterval(TimeSpan.FromTicks(ticks)));

    /// <summary>A refused interval is refused before it is set: the interval it would replace stands where it
    /// was.</summary>
    [Fact]
    public void ARefusedIntervalLeavesTheOneItWouldReplace()
    {
        var poller = new Poller(_ => Task.CompletedTask, TimeProvider.System, Interval);
        poller.SetInterval(TimeSpan.FromSeconds(20));

        Assert.Throws<ArgumentOutOfRangeException>(() => poller.SetInterval(TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => poller.SetInterval(TimeSpan.FromTicks(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => poller.SetInterval(TimeSpan.MinValue));

        Assert.Equal(TimeSpan.FromSeconds(20), poller.Interval);
    }

    /// <summary>
    /// A new interval made while paused checks nothing at once - the pause holds the checks, not the interval -
    /// and the resume's check is followed by the new interval, not the one it replaced.
    /// </summary>
    [Fact]
    public async Task ANewIntervalWhilePausedChecksNothingUntilTheResume()
    {
        await StartAndWaitForTheFirstIntervalAsync();
        _poller.Pause();

        _poller.SetInterval(TimeSpan.FromSeconds(10));
        Assert.Equal(TimeSpan.FromSeconds(10), _poller.Interval);
        Assert.False(await _checks.WaitAsync(Quiet), "A paused poller does not check at once for a new interval.");

        _poller.Resume();
        await CheckRanAsync();
        await _time.TimerCreatedAsync();
        Assert.Equal(2, _count);

        _time.Advance(TimeSpan.FromSeconds(9));
        Assert.Equal(2, _count);
        _time.Advance(TimeSpan.FromSeconds(1));
        await CheckRanAsync();
        Assert.Equal(3, _count);
    }

    // ------------------------------------------------------- the changes that change nothing

    /// <summary>
    /// The second pause changes nothing, so it wakes the loop nothing: the resume's one check is the only one
    /// until the interval, and no earlier one is queued by the pause that was no change.
    /// </summary>
    [Fact]
    public async Task PausingTwiceWakesThePollerOnlyOnce()
    {
        await StartAndWaitForTheFirstIntervalAsync();

        _poller.Pause();
        _poller.Pause();
        Assert.True(_poller.IsPaused);
        Assert.Equal(1, _count);

        _poller.Resume();
        await CheckRanAsync();
        await _time.TimerCreatedAsync();
        Assert.Equal(2, _count);

        _time.Advance(Interval - TimeSpan.FromSeconds(1));
        Assert.Equal(2, _count);
    }

    /// <summary>A resume of a poller that is not paused changes nothing: no check, no new wait on the
    /// clock.</summary>
    [Fact]
    public async Task ResumingANotPausedPollerChangesNothing()
    {
        await StartAndWaitForTheFirstIntervalAsync();

        _poller.Resume();
        Assert.False(_poller.IsPaused);
        Assert.Equal(1, _count);
        Assert.False(await _time.TimerCreatedWithinAsync(Quiet), "A resume that changes nothing must not start a new wait.");
    }

    /// <summary>Making the visible poller visible changes nothing: no check, no new wait on the
    /// clock.</summary>
    [Fact]
    public async Task MakingTheVisiblePollerVisibleChangesNothing()
    {
        await StartAndWaitForTheFirstIntervalAsync();

        _poller.SetVisible(true);
        Assert.True(_poller.IsVisible);
        Assert.Equal(1, _count);
        Assert.False(await _time.TimerCreatedWithinAsync(Quiet), "A visibility that changes nothing must not start a new wait.");
    }

    /// <summary>Hiding the hidden poller changes nothing: no check, no new wait on the
    /// clock.</summary>
    [Fact]
    public async Task HidingTheHiddenPollerChangesNothing()
    {
        await StartAndWaitForTheFirstIntervalAsync();
        _poller.SetVisible(false);
        Assert.False(await _checks.WaitAsync(Quiet));

        _poller.SetVisible(false);
        Assert.Equal(1, _count);
        Assert.False(await _time.TimerCreatedWithinAsync(Quiet), "A visibility that changes nothing must not start a new wait.");
    }

    // ------------------------------------------------------------ the check that was requested

    /// <summary>
    /// A check requested before the run is honoured at once when it starts, even while paused and hidden, and
    /// it is one check: the poller waits on the signal that follows it, not on a timer.
    /// </summary>
    [Fact]
    public async Task ACheckRequestedBeforeTheRunIsHonouredAtOnce()
    {
        _poller.Pause();
        _poller.SetVisible(false);
        _poller.CheckNow();

        Start();
        await CheckRanAsync();

        Assert.Equal(1, _count);
        Assert.False(await _time.TimerCreatedWithinAsync(Quiet),
            "After the requested check, a paused and hidden poller waits on the signal, not a timer.");
    }

    /// <summary>
    /// The two requests for a check are one check: the request is a flag, not a count, so the second adds
    /// nothing before the interval.
    /// </summary>
    [Fact]
    public async Task TwoRequestsForACheckAreOneCheck()
    {
        await StartAndWaitForTheFirstIntervalAsync();

        _poller.CheckNow();
        _poller.CheckNow();
        await CheckRanAsync();
        await _time.TimerCreatedAsync();
        Assert.Equal(2, _count);

        _time.Advance(Interval - TimeSpan.FromSeconds(1));
        Assert.Equal(2, _count);
    }

    /// <summary>
    /// A check requested while one is running is not lost: the running one ends, and the requested one runs at
    /// once as it does, without the time it would otherwise have waited for passing.
    /// </summary>
    [Fact]
    public async Task ARequestMadeDuringACheckChecksAgainAsItEnds()
    {
        using var stop = new CancellationTokenSource();
        using var checks = new SemaphoreSlim(0);
        var checking = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var time = new SignallingTimeProvider();
        var count = 0;
        var poller = new Poller(async _ =>
            {
                checking.TrySetResult();
                await release.Task;
                Interlocked.Increment(ref count);
                checks.Release();
            }, time, Interval);

        var running = poller.RunAsync(stop.Token);
        await checking.Task.WaitAsync(Patience);
        poller.CheckNow();
        release.SetResult();

        Assert.True(await checks.WaitAsync(Patience), "The poller did not run a check.");
        Assert.True(await checks.WaitAsync(Patience), "The poller did not run the requested check.");
        await time.TimerCreatedAsync();
        Assert.Equal(2, count);

        time.Advance(Interval - TimeSpan.FromSeconds(1));
        Assert.Equal(2, count);

        stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running.WaitAsync(Patience));
    }

    /// <summary>
    /// A check requested while paused and the resume that follows are two checks: the request runs one while
    /// the pause holds the interval, and the resume runs its own at once.
    /// </summary>
    [Fact]
    public async Task ARequestWhilePausedAndTheResumeAreTwoChecks()
    {
        await StartAndWaitForTheFirstIntervalAsync();
        _poller.Pause();

        _poller.CheckNow();
        await CheckRanAsync();
        Assert.Equal(2, _count);

        _poller.Resume();
        await CheckRanAsync();
        Assert.Equal(3, _count);
    }

    // --------------------------------------------------------------------------- the run itself

    /// <summary>
    /// A token cancelled before the run is cancelled: the run throws as it starts, and no check runs.
    /// </summary>
    [Fact]
    public async Task ATokenCancelledBeforeTheRunThrowsBeforeAnyCheck()
    {
        _stop.Cancel();

        Start();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _running.WaitAsync(Patience));
        Assert.False(await _checks.WaitAsync(Quiet));
        Assert.Equal(0, _count);
    }

    /// <summary>The check is given the very token the run was given, not a copy and not a new
    /// one.</summary>
    [Fact]
    public async Task TheCheckIsGivenTheTokenTheRunWasGiven()
    {
        using var stop = new CancellationTokenSource();
        var given = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        var poller = new Poller(token =>
            {
                given.TrySetResult(token);
                return Task.CompletedTask;
            }, new SignallingTimeProvider(), Interval);

        var running = poller.RunAsync(stop.Token);

        Assert.Equal(stop.Token, await given.Task.WaitAsync(Patience));
        stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running.WaitAsync(Patience));
    }

    /// <summary>
    /// A check that fails fails the run with its own failure, the instance the check threw - the failure the
    /// page's poller hands to Blazor.
    /// </summary>
    [Fact]
    public async Task AFailingCheckFailsTheRunWithItsOwnFailure()
    {
        using var stop = new CancellationTokenSource();
        var failure = new InvalidOperationException("the check failed");
        var poller = new Poller(_ => throw failure, new SignallingTimeProvider(), Interval);

        var running = poller.RunAsync(stop.Token);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => running.WaitAsync(Patience));
        Assert.Same(failure, thrown);
        stop.Cancel();
    }

    /// <summary>
    /// Cancelling the run while it waits throws, and no further check runs after: the time that passes then is
    /// no time for the poller.
    /// </summary>
    [Fact]
    public async Task CancellingTheWaitThrowsAndNoFurtherCheckRuns()
    {
        await StartAndWaitForTheFirstIntervalAsync();

        _stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _running.WaitAsync(Patience));

        _time.Advance(Interval * 10);
        Assert.False(await _checks.WaitAsync(Quiet));
        Assert.Equal(1, _count);
    }

    /// <summary>
    /// A poller that is hidden never asks the clock: it waits on the signal, not a timer, until it becomes
    /// visible, and the clock is asked for the interval only from then on.
    /// </summary>
    [Fact]
    public async Task AHiddenPollerNeverAsksTheClockUntilItBecomesVisible()
    {
        _poller.SetVisible(false);
        Start();
        Assert.False(await _time.TimerCreatedWithinAsync(Quiet));
        Assert.False(await _checks.WaitAsync(Quiet));

        _poller.SetVisible(true);
        await CheckRanAsync();
        await _time.TimerCreatedAsync();
        Assert.Equal(1, _count);
    }

    /// <summary>
    /// The clock is asked for exactly the interval the poller holds: the one it was given, and the one
    /// <see cref="Poller.SetInterval" /> made of it.
    /// </summary>
    [Fact]
    public async Task TheClockIsAskedForTheIntervalThePollerHolds()
    {
        using var stop = new CancellationTokenSource();
        using var checks = new SemaphoreSlim(0);
        var time = new NotingTimeProvider();
        var poller = new Poller(_ =>
            {
                checks.Release();
                return Task.CompletedTask;
            }, time, Interval);

        var running = poller.RunAsync(stop.Token);

        Assert.True(await checks.WaitAsync(Patience), "The poller did not run a check.");
        Assert.Equal(Interval, await time.DueTimeAsync(0));

        poller.SetInterval(TimeSpan.FromSeconds(10));
        Assert.True(await checks.WaitAsync(Patience), "The poller did not run a check for the new interval.");
        Assert.Equal(TimeSpan.FromSeconds(10), await time.DueTimeAsync(1));

        stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running.WaitAsync(Patience));
    }

    /// <summary>
    /// Fake time that notes the due time of every timer it is asked to create, so a test can pin the exact
    /// wait the code under test asked for - the <see cref="SignallingTimeProvider" /> writes its due times to
    /// a channel its readers leave there.
    /// </summary>
    private sealed class NotingTimeProvider : FakeTimeProvider
    {
        private readonly List<TimeSpan> _dueTimes = [];

        public NotingTimeProvider()
            : base(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero))
        {
        }

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            lock (_dueTimes)
            {
                _dueTimes.Add(dueTime);
            }

            return base.CreateTimer(callback, state, dueTime, period);
        }

        /// <summary>The due time of the timer with the index, once it was created.</summary>
        public async Task<TimeSpan> DueTimeAsync(int index)
        {
            using var patience = new CancellationTokenSource(Patience);
            while (true)
            {
                lock (_dueTimes)
                {
                    if (_dueTimes.Count > index)
                    {
                        return _dueTimes[index];
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(5), patience.Token);
            }
        }
    }
}
