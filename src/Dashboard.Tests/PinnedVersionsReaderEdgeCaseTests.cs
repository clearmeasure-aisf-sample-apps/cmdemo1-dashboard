using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="PinnedVersionsReader" />: the reading of the
/// versions pinned in Git - the environment's <c>versions.json</c>. <see cref="PinnedVersionsTests" /> pins the
/// file itself: its object of versions, its malformed answers, its missing file and its no answer at the
/// node's ten seconds; and <see cref="PinnedVersionsMonitorTests" /> and <see cref="KustomizationPinMonitorTests" />
/// pin the reading the page's rounds make with the file and with the pin of a Kustomize file. What they leave
/// open is pinned here: the reader's own timeout, the ten seconds it stands at by default and the shorter one
/// the init names, a limit on the answer and no delay of it; the seconds in the no answer's reason, the three
/// the init names and the one and a half that says the two it rounds to, and the zero that says the zero; the
/// timeout that is not a time, which is no result and throws where the reader asks the clock for it; the reader
/// made with no clock, which throws only where it asks the clock; the address that is not named, which the
/// browser is not asked and is said so; and the file that names one deployable twice, read with the last value
/// it says.
/// <para>
/// Nothing here reaches a network or asks a clock what time it is: every answer is one the test hands back, and
/// every moment is one the test brings.
/// </para>
/// </summary>
public class PinnedVersionsReaderEdgeCaseTests
{
    private static readonly Uri Address =
        new("https://raw.githubusercontent.com/example-org/demo-system/main/environments/tdd/versions.json");

    private readonly SignallingTimeProvider _time = new();

    private PinnedVersionsReader Reader(StubHandler handler, TimeSpan? timeout = null) =>
        timeout is { } limit
            ? new PinnedVersionsReader(new HttpClient(handler), _time) { Timeout = limit }
            : new PinnedVersionsReader(new HttpClient(handler), _time);

    // ----- the reader at its making

    /// <summary>The reader's timeout stands at the node's by default: the ten seconds a node may take to answer
    /// - and the init that names a shorter one stands the shorter one, not the node's.</summary>
    [Fact]
    public void TheReaderStandsItsTimeoutAtTheNodesAndTheInitSaysSo()
    {
        var reader = new PinnedVersionsReader(new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound))), _time);

        Assert.Equal(NodeProber.DefaultTimeout, reader.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(10), reader.Timeout);

        var quicker = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound)), TimeSpan.FromSeconds(3));
        Assert.Equal(TimeSpan.FromSeconds(3), quicker.Timeout);
    }

    /// <summary>The reader made with no clock throws no error at its making: the error stands where the reader
    /// asks the clock for its timeout, the reading, naming the argument it was made without - and the browser is
    /// never asked, for the timeout is asked before the request is made.</summary>
    [Fact]
    public async Task AReaderMadeWithNoClockThrowsOnlyWhereItAsksTheClock()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var reader = new PinnedVersionsReader(new HttpClient(handler), null!);

        var error = await Assert.ThrowsAsync<ArgumentNullException>(() => reader.ReadAsync(Address, CancellationToken.None));

        Assert.Equal("timeProvider", error.ParamName);
        Assert.Empty(handler.RequestedUrls);

        await Assert.ThrowsAsync<ArgumentNullException>(() => reader.ReadKustomizationAsync(Address, "ui", CancellationToken.None));
        Assert.Empty(handler.RequestedUrls);
    }

    // ----- the timeout

    /// <summary>The timeout is a limit on the answer, not a delay of it: the answer that arrives while the
    /// reader waits is read, at the timeout that is one millisecond and not ten seconds.</summary>
    [Fact]
    public async Task AnAnswerThatArrivesInTheTimeIsNotHeldForTheTimeout()
    {
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, """{ "ui": "2.4.7" }""")), TimeSpan.FromMilliseconds(1));

        var pinned = await reader.ReadAsync(Address, CancellationToken.None);

        Assert.Equal(PinnedVersionsState.Read, pinned.State);
        Assert.Null(pinned.Detail);
        Assert.Equal("2.4.7", pinned.Of("ui"));
    }

    /// <summary>The answer that takes longer than the timeout is Unavailable at the seconds the timeout says:
    /// the three the init names says three, and the one and a half says the two it rounds to - the
    /// seconds the timeout stands on, not the moment the answer came.</summary>
    [Fact]
    public async Task AnAnswerThatTakesLongerThanTheTimeoutSaysTheSecondsTheTimeoutSays()
    {
        var never = new StubHandler((request, cancellation) => StubHandler.NeverAsync(cancellation));

        var atThree = Reader(never, TimeSpan.FromSeconds(3));
        var pin = atThree.ReadAsync(Address, CancellationToken.None);
        await _time.TimerCreatedAsync();
        _time.Advance(TimeSpan.FromSeconds(3));
        Assert.Equal(PinnedVersions.Unavailable("no answer within 3 s"), await pin);

        var atOneAndHalf = Reader(never, TimeSpan.FromSeconds(1.5));
        var reading = atOneAndHalf.ReadAsync(Address, CancellationToken.None);
        await _time.TimerCreatedAsync();
        _time.Advance(TimeSpan.FromSeconds(1.5));

        Assert.Equal(PinnedVersions.Unavailable("no answer within 2 s"), await reading);
    }

    /// <summary>The timeout that is zero says zero: the no answer is Unavailable at the seconds the timeout
    /// says - the zero that says the zero, the same reason as every other no answer, at the timeout it stands
    /// on.</summary>
    [Fact]
    public async Task AZeroTimeoutSaysZeroSeconds()
    {
        var never = new StubHandler((request, cancellation) => StubHandler.NeverAsync(cancellation));
        var reader = Reader(never, TimeSpan.Zero);

        var pinned = await reader.ReadAsync(Address, CancellationToken.None);

        Assert.Equal(PinnedVersionsState.Unavailable, pinned.State);
        Assert.Equal("no answer within 0 s", pinned.Detail);
    }

    /// <summary>The timeout that is not a time is no result: the reader throws, uncaught, where it asks the
    /// clock for the timeout - before it asks the browser - naming the argument the clock was asked with.</summary>
    [Fact]
    public async Task ATimeoutThatIsNotATimeIsNoResultAndThrows()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var reader = Reader(handler, TimeSpan.FromSeconds(-1));

        var error = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => reader.ReadAsync(Address, CancellationToken.None));

        Assert.Equal("delay", error.ParamName);
        Assert.Empty(handler.RequestedUrls);
    }

    // ----- the address

    /// <summary>The address that is not named is not read: the browser is never asked, and the reading says so
    /// without one - the invalid request URI - for the versions file and the Kustomize file alike; the address
    /// the browser is asked is one the caller made, and the reader reads only what is named.</summary>
    [Fact]
    public async Task AnAddressThatIsNotNamedIsNotReadAndSaysSo()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var reader = Reader(handler);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync(null!, CancellationToken.None));
        Assert.Contains("invalid request URI", error.Message, StringComparison.OrdinalIgnoreCase);

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadKustomizationAsync(null!, "ui", CancellationToken.None));

        Assert.Empty(handler.RequestedUrls);
    }

    // ----- the file, read with the value it says

    /// <summary>The file that names one deployable twice keeps the last value it says: the object that pins
    /// "ui" twice is read, and the version it pins is the last one the file says - the last the file's own
    /// object gives, not an error of the reader's.</summary>
    [Fact]
    public async Task AFileThatNamesADeployableTwiceKeepsTheLastValueItSays()
    {
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, """{ "ui": "2.4.7", "ui": "3.0.0" }""")));

        var pinned = await reader.ReadAsync(Address, CancellationToken.None);

        Assert.Equal(PinnedVersionsState.Read, pinned.State);
        Assert.Null(pinned.Detail);
        Assert.Equal("3.0.0", pinned.Of("ui"));
        Assert.Equal(["ui"], pinned.Versions.Keys);
    }
}
