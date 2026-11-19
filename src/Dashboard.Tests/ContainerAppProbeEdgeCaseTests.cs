namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ContainerAppProbe" />, the page's own rule for one
/// container app: what a run is, when the next probe of it is due, what an answer does to the state it is in, and
/// whether a body a browser read says the app is healthy.
/// <para>
/// <see cref="ContainerAppProbeTests" /> walks the table of answers a browser really gets - a first silence waking,
/// a 404 and two minutes of it not responding, a 5xx unhealthy, a 2xx with a healthy payload healthy, an unread
/// answer responding, an app that slept six minutes waking once more - and holds the lengths of the run to their
/// whole seconds; <see cref="ContainerAppStateEdgeCaseTests" /> pins the three lengths as the ticks they are, their
/// order and the tick each rule turns on, the state an answer of every edge status lands on, the refusal remembered
/// through every later state, the grace measured from a probe that was never started, and the one argument
/// <see cref="ContainerAppProbe.Record" /> refuses by name; <see cref="ContainerAppTargetEdgeCaseTests" /> and
/// <see cref="ContainerAppAnswerEdgeCaseTests" /> hold the target a probe stands on and the answer it is handed.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: that a probe is a probe and not a value - the target it was given is
/// the target it keeps, an equal target makes no second of one probe, and each probe holds its own status, which is
/// the one <see cref="ContainerAppStatus.NotYet" /> instance until an answer is recorded and is the very answer then
/// carried; that starting is not answering, and that a second start - whether it stands after the last one or
/// behind it, inside the five minutes or at their very tick - moves neither the grace the run counts to nor the
/// state the probe is in, only the moment the next probe is due; that both the wait and the grace are counted in
/// instants, so a moment spelled in another viewer's hours is the same moment and neither extends nor forgives
/// anything; that <see cref="ContainerAppProbe.IsNew" /> says one thing and says it forever once it is false; that
/// an answer carrying both a status and the browser-could-not-read mark is classed by the status where the path it
/// asked is a health path and by nothing at all where it is not; how the three lengths stand to one another, since
/// the run's states come out of that ratio; and nearly the whole of
/// <see cref="ContainerAppProbe.IsHealthyPayload" />, which the table above touches at a handful of points only - the status
/// window it reads, at either end and outside it, the plain word in any case and around any whitespace, the JSON
/// body whose own <c>status</c> says the word and the ones whose does not (a value in another type, another case of
/// the name, a nested answer, an array, no property at all), the body that opens like JSON and is not JSON, and the
/// body that is not there.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the members that would are
/// <see cref="NodeProber.ProbeContainerAppAsync" /> and the monitor's round, both pinned with a handler the test
/// answers in <see cref="ContainerAppProbeTests" />. Every moment below is one the test brought, in the hours the
/// test chose.
/// </para>
/// </summary>
public class ContainerAppProbeEdgeCaseTests
{
    /// <summary>The moment the page opened and every run below is started from: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Opened = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly Uri App = new("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io");

    /// <summary>The target of every probe below: an app whose deployable names its health path.</summary>
    private static readonly ContainerAppTarget Named = new(App, HealthPath);

    /// <summary>The path a container app's health is read at, the one the target above names and the page asks.</summary>
    private const string HealthPath = "/_healthcheck";

    /// <summary>An answer at the health path, at the given moment, of the given status, with a payload the page read
    /// as healthy or not.</summary>
    private static ContainerAppAnswer Answer(int status, bool healthyPayload = false, DateTimeOffset? at = null) =>
        new(HealthPath, true, status, false, 85, at ?? Opened, HealthyPayload: healthyPayload);

    /// <summary>No answer at all, at the given moment: the app said nothing and the page says why.</summary>
    private static ContainerAppAnswer Silent(DateTimeOffset at) =>
        new(HealthPath, true, null, false, null, at, "No answer within 10 s.");

    /// <summary>A probe started at <see cref="Opened" />, so its run counts its grace from there.</summary>
    private static ContainerAppProbe Started(ContainerAppTarget? target = null)
    {
        var probe = new ContainerAppProbe(target ?? Named);
        probe.Start(Opened);

        return probe;
    }

    /// <summary>The moment so many seconds after the page opened, spelled in another viewer's hours: the same instant
    /// with another offset written on it, which is the only difference a probe may not see.</summary>
    private static DateTimeOffset At(int seconds, double offsetHours) =>
        Opened.AddSeconds(seconds).ToOffset(TimeSpan.FromHours(offsetHours));

    // ---------------------------------------------------------------- the three lengths, against one another

    /// <summary>
    /// The three lengths as the ticks they are, their order and the words they are told in stand in
    /// <see cref="ContainerAppStateEdgeCaseTests" />. What stands here is the ratio between them, because the states
    /// of a run come out of it: five minutes of nothing are exactly five of the minute a probe waits by, so a page
    /// that hid and came back finds an app that may have scaled to zero rather than one it has been asking for; and
    /// ninety seconds of grace are exactly one and a half of those minutes - longer than the wait to a second probe,
    /// shorter than the wait to a third - so a run of silence is woken twice and given up on the third time, never
    /// four. A length moved on its own moves these ratios, and a ratio kept keeps the states as they are.
    /// </summary>
    [Fact]
    public void TheThreeLengthsKeepTheRhythmTheRunIsProbedIn()
    {
        Assert.Equal(5 * ContainerAppProbe.Interval.Ticks, ContainerAppProbe.Asleep.Ticks);
        Assert.Equal(3 * ContainerAppProbe.Interval.Ticks, 2 * ContainerAppProbe.Grace.Ticks);

        Assert.True(ContainerAppProbe.Grace > ContainerAppProbe.Interval);
        Assert.True(ContainerAppProbe.Grace < 2 * ContainerAppProbe.Interval);
        Assert.True(ContainerAppProbe.Asleep > 2 * ContainerAppProbe.Grace);

        // And the run itself, in the whole probes it holds: two waking answers, the third not responding.
        var probe = Started();
        probe.Record(Silent(Opened));
        Assert.Equal(ContainerAppState.Waking, probe.Status.State);
        probe.Record(Silent(Opened + ContainerAppProbe.Interval));
        Assert.Equal(ContainerAppState.Waking, probe.Status.State);
        probe.Record(Silent(Opened + 2 * ContainerAppProbe.Interval));
        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
    }

    // ---------------------------------------------------------------- the probe: its target and its own status

    /// <summary>
    /// A probe is a probe and not a value: it keeps the very target it was given, whatever it goes on to record, and
    /// nothing about it is shared - two probes of one app are two probes, standing on two equal targets, each with
    /// its own status. A monitor that holds one probe per app therefore shows one app's answer on one app's badge
    /// and never on the other's, and an equal target, an equal answer or the same answer twice over makes no second
    /// of one probe the first.
    /// </summary>
    [Fact]
    public void AProbeIsItsOwnProbeStandingOnTheTargetItWasGiven()
    {
        var target = new ContainerAppTarget(App, HealthPath);
        var probe = new ContainerAppProbe(target);
        var other = new ContainerAppProbe(new ContainerAppTarget(App, HealthPath));

        Assert.Equal(Named, target);
        Assert.Same(target, probe.Target);
        Assert.NotSame(probe, other);
        Assert.False(probe.Equals(other));
        Assert.False(probe == other);
        Assert.True(probe != other);

        // What one of them is told of an answer stays with it, and moves neither its target nor the other's state.
        probe.Record(Answer(200, healthyPayload: true));

        Assert.Same(target, probe.Target);
        Assert.Equal(ContainerAppState.Healthy, probe.Status.State);
        Assert.Equal(ContainerAppStatus.NotYet, other.Status);
        Assert.True(other.IsNew);
        Assert.NotSame(probe.Target, other.Target);
    }

    /// <summary>
    /// The status a probe holds before anything is recorded is the one <see cref="ContainerAppStatus.NotYet" />
    /// instance, and starting a probe leaves that very instance standing: a probe started and never answered still
    /// says nothing but <c>Probing</c>, with no answer and no refusal. The first answer replaces it with a new
    /// status carrying the very answer object it was handed, and every later answer carries a new status of its own
    /// - equal to the last when the same answer is recorded twice, since a status is what the rule made of it and
    /// nothing else.
    /// </summary>
    [Fact]
    public void TheStatusIsTheOneInstanceOfNothingUntilAnAnswerIsRecordedAndThenTheAnswerGiven()
    {
        var probe = new ContainerAppProbe(Named);

        Assert.Same(ContainerAppStatus.NotYet, probe.Status);

        probe.Start(Opened);
        probe.Start(Opened + ContainerAppProbe.Interval);

        Assert.Same(ContainerAppStatus.NotYet, probe.Status);
        Assert.Equal(ContainerAppState.Probing, probe.Status.State);
        Assert.Null(probe.Status.Last);
        Assert.False(probe.Status.CorsRefused);

        var answer = Answer(200, healthyPayload: true);
        probe.Record(answer);
        var first = probe.Status;

        Assert.NotSame(ContainerAppStatus.NotYet, probe.Status);
        Assert.Same(answer, probe.Status.Last);
        Assert.Equal(ContainerAppState.Healthy, first.State);
        Assert.False(first.CorsRefused);

        probe.Record(answer);

        Assert.NotSame(first, probe.Status);
        Assert.Equal(first, probe.Status);
        Assert.Same(answer, probe.Status.Last);
    }

    /// <summary>
    /// <c>IsNew</c> says one thing only: whether anything was ever started. It is true of a probe no moment has been
    /// offered, false from the first <see cref="ContainerAppProbe.Start" /> to the last and false again at either end
    /// of the clock, whatever the moment the start stood on - and no answer recorded afterwards, however many and
    /// whatever they say, brings it back. That an answer recorded before any start leaves it true is
    /// <see cref="ContainerAppStateEdgeCaseTests" />'s; so is the false right after a start. What is added here is
    /// that the false is forever, at the ends of the clock and through every state a run goes on to pass through.
    /// </summary>
    [Fact]
    public void IsNewSaysOnlyWhetherAnythingWasEverStartedAndIsFalseFromThenOn()
    {
        var early = new ContainerAppProbe(Named);
        Assert.True(early.IsNew);
        early.Start(DateTimeOffset.MinValue);
        Assert.False(early.IsNew);

        var late = new ContainerAppProbe(Named);
        late.Start(DateTimeOffset.MaxValue);
        Assert.False(late.IsNew);

        var probe = Started();
        foreach (var answer in new[]
                 {
                     Answer(200, healthyPayload: true),
                     Answer(503),
                     Silent(Opened + ContainerAppProbe.Interval),
                     Answer(404, at: Opened + ContainerAppProbe.Asleep),
                 })
        {
            probe.Record(answer);
            Assert.False(probe.IsNew);
        }

        // A new run - five minutes of nothing, then a start and an answer - is a new run of the same probe, and the
        // probe that has been asking for ten minutes is no newer than the one that opened.
        probe.Start(Opened + ContainerAppProbe.Asleep + TimeSpan.FromMinutes(2));
        Assert.False(probe.IsNew);

        var woke = new ContainerAppProbe(Named);
        woke.Start(Opened);
        woke.Record(Answer(200, healthyPayload: true));
        woke.Start(Opened + ContainerAppProbe.Asleep);
        woke.Record(Silent(Opened + ContainerAppProbe.Asleep));

        Assert.False(woke.IsNew);
        Assert.Equal(ContainerAppState.Waking, woke.Status.State);
    }

    // ---------------------------------------------------------------- when the next probe is due

    /// <summary>
    /// The wait is counted in instants, not in the hours a moment happens to be written in: the very tick the minute
    /// ends is the tick a probe is due again whether the moment asked about is spelled in UTC, in the hours of a
    /// viewer five and a half hours east of the meridian or of one eight hours west of it, and the same instant a
    /// tick short of that tick is not due in any of them. The tick itself, in UTC, stands in
    /// <see cref="ContainerAppStateEdgeCaseTests" />; the offsets are the part left open, which is the part a page
    /// opened in a viewer's own hours hands it.
    /// </summary>
    [Theory]
    [InlineData(59, 0, false)]
    [InlineData(60, 0, true)]
    [InlineData(61, 0, true)]
    [InlineData(0, 5.5, false)]
    [InlineData(59, 5.5, false)]
    [InlineData(60, 5.5, true)]
    [InlineData(0, -8, false)]
    [InlineData(59, -8, false)]
    [InlineData(60, -8, true)]
    [InlineData(3600, -12, true)]
    public void AProbeIsDueByTheInstantOfTheMomentAskedAndNotTheHoursItIsWrittenIn(int seconds, double offsetHours, bool due)
    {
        Assert.Equal(Opened, At(0, offsetHours));
        Assert.Equal(Opened.UtcDateTime, At(0, offsetHours).UtcDateTime);

        var probe = Started();

        Assert.Equal(due, probe.IsDue(At(seconds, offsetHours)));
    }

    /// <summary>
    /// The minute spaces a probe from the last time it was started rather than from the run's first probe: a second
    /// start ten seconds in moves the next due moment ten seconds with it, so the app is asked again on the tick
    /// sixty seconds after that start and not a tick before - and the run's own grace is untouched, which the test
    /// below pins.
    /// </summary>
    [Theory]
    [InlineData(68, false)]
    [InlineData(69, false)]
    [InlineData(70, true)]
    [InlineData(71, true)]
    public void TheMinuteSpacesAProbeFromTheLastStartAndNotTheFirstOfTheRun(int seconds, bool due)
    {
        var probe = Started();
        probe.Start(Opened + TimeSpan.FromSeconds(10));

        Assert.Equal(due, probe.IsDue(Opened + TimeSpan.FromSeconds(seconds)));
    }

    // ---------------------------------------------------------------- what a start does, and does not do, to a run

    /// <summary>
    /// A start is a request, not a wake-up call: a second one inside the five minutes neither opens a new run nor
    /// gives the app another ninety seconds, because the run's first probe is what the grace is counted from. Two
    /// starts of one run eighty-nine seconds apart still leave the silence waking, and ninety-five apart leave it not
    /// responding - the same two answers a single start on that tick would have given, which is what keeps a page
    /// that started a probe early, or twice, from excusing a silence that has already run its length. The tick the
    /// five minutes end on, where a start does open a run, <see cref="ContainerAppStateEdgeCaseTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData(89, ContainerAppState.Waking)]
    [InlineData(90, ContainerAppState.NotResponding)]
    [InlineData(95, ContainerAppState.NotResponding)]
    public void ASecondStartInsideTheSleepGivesTheAppNoFreshGrace(int seconds, ContainerAppState expected)
    {
        var probe = Started();
        var at = Opened + TimeSpan.FromSeconds(seconds);

        probe.Start(at);
        probe.Record(Silent(at));

        Assert.Equal(expected, probe.Status.State);
    }

    /// <summary>
    /// A start that stands behind the last one - the viewer's own clock put back, a page that asked its apps out of
    /// order - opens no run either: the wait it measured is negative, and a negative wait is nowhere near five
    /// minutes. So the run keeps counting its grace from the moment it was first started, and an app silent for
    /// ninety seconds of that is not responding however often the page went back in time on the way. What the start
    /// does move is the last probe, so the next one is due a minute after the moment written down, which for a start
    /// an hour in the past is at once.
    /// </summary>
    [Fact]
    public void AStartBehindTheLastOneOpensNoRunAndMovesTheGraceNowhere()
    {
        var probe = Started();

        probe.Start(Opened - TimeSpan.FromHours(1));

        Assert.True(probe.IsDue(Opened));

        probe.Record(Silent(Opened));
        Assert.Equal(ContainerAppState.Waking, probe.Status.State);

        probe.Record(Silent(Opened + ContainerAppProbe.Grace));
        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
    }

    /// <summary>
    /// A silence whose moment stands behind the run's start is a silence at the start: the wait the rule measures
    /// comes out negative, negative is under the ninety seconds, and the app is waking - the rule never reads a
    /// negative wait as a spent grace, and never as an answer either. The same run, measured on to the tick its
    /// grace ends, is given up on exactly as it would have been without the moment behind it.
    /// </summary>
    [Theory]
    [InlineData(-1, ContainerAppState.Waking)]
    [InlineData(0, ContainerAppState.Waking)]
    [InlineData(89, ContainerAppState.Waking)]
    [InlineData(90, ContainerAppState.NotResponding)]
    public void ASilenceWrittenBehindTheRunStaysWakingUntilTheGraceOfTheRun(int seconds, ContainerAppState expected)
    {
        var probe = Started();

        probe.Record(Silent(Opened + TimeSpan.FromSeconds(seconds)));

        Assert.Equal(expected, probe.Status.State);
        Assert.False(probe.Status.Last!.Answered);
    }

    /// <summary>
    /// An answer's moment is read as the instant it stands on and not as the hours it is written in: a silence at the
    /// very instant the run started, spelled five and a half hours east of the meridian, is a silence of no length at
    /// all and the app is waking; the same silence spelled eight hours west of it, ninety seconds on, is a silence
    /// that has run its length and the app is not responding. The grace itself, in UTC and to the tick, is
    /// <see cref="ContainerAppStateEdgeCaseTests" />'s.
    /// </summary>
    [Fact]
    public void AnAnswerIsClassedByTheInstantItStandsOnAndNotTheHoursItIsWrittenIn()
    {
        Assert.Equal(Opened, At(0, 5.5));
        Assert.Equal(Opened + ContainerAppProbe.Grace, At(90, -8));

        var probe = Started();

        probe.Record(Silent(At(0, 5.5)));
        Assert.Equal(ContainerAppState.Waking, probe.Status.State);
        Assert.Equal(At(0, 5.5), probe.Status.Last!.CheckedAt);

        probe.Record(Silent(At(90, -8)));
        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
    }

    // ---------------------------------------------------------------- what Record makes of an odd answer

    /// <summary>
    /// The answer the browser both read and could not read: the status is what the rule reads where the path it
    /// asked is a health path - a 2xx with a healthy payload healthy, a 404 the app gone, a 503 unhealthy - and the
    /// unread mark is remembered whichever way the state lands. Where the path is no health path, neither of them is
    /// read at all and the answer is one the page cannot class: responding, exactly as a status alone or an unread
    /// mark alone would have left it.
    /// </summary>
    [Theory]
    [InlineData(200, true, true, true, ContainerAppState.Healthy)]
    [InlineData(204, true, true, true, ContainerAppState.Healthy)]
    [InlineData(404, true, true, false, ContainerAppState.NotResponding)]
    [InlineData(503, true, true, false, ContainerAppState.Unhealthy)]
    [InlineData(503, false, true, false, ContainerAppState.Responding)]
    [InlineData(200, false, true, true, ContainerAppState.Responding)]
    public void AnAnswerWithBothAStatusAndAnUnreadMarkIsClassedByItsStatusWhereThePathReadsAsHealth(
        int status, bool readsHealth, bool opaque, bool healthyPayload, ContainerAppState expected)
    {
        var probe = Started();

        probe.Record(new ContainerAppAnswer(
            readsHealth ? HealthPath : "/", readsHealth, status, opaque, 85, Opened, HealthyPayload: healthyPayload));

        Assert.Equal(expected, probe.Status.State);
        Assert.True(probe.Status.Last!.Answered);
        Assert.Equal(opaque, probe.Status.CorsRefused);
    }

    // ---------------------------------------------------------------- IsHealthyPayload: the status it reads

    /// <summary>
    /// A healthy body has to be a body the page may read at all, which is the two hundreds and nothing else: 199 and
    /// 300 stand either side of the window the rule reads, 204 and 299 stand inside it, and 404, 500 and the two ends
    /// of the number stand outside it - a body that says healthy of a status that is no answer the page takes says
    /// nothing. Both spellings of a healthy body are tried at every one of them, so the window is pinned for the word
    /// and for the JSON alike; what the states are worth on either side of it,
    /// <see cref="ContainerAppStateEdgeCaseTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue, false)]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(199, false)]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    [InlineData(301, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    [InlineData(int.MaxValue, false)]
    public void AHealthyBodyIsOnlyEverABodyOfATwoHundred(int statusCode, bool healthy)
    {
        Assert.Equal(healthy, ContainerAppProbe.IsHealthyPayload(statusCode, "Healthy"));
        Assert.Equal(healthy, ContainerAppProbe.IsHealthyPayload(statusCode, """{"status":"Healthy"}"""));
    }

    /// <summary>No body at all is no healthy body, at every status inside the window the rule reads a body at: the
    /// one the caller did not name, the empty one it named and the whitespace one - a health check that answers 200
    /// and says nothing is a check the page cannot class as healthy. The empty body of a 200,
    /// <see cref="ContainerAppProbeTests" /> pins; what is added here is the body that is not there at all, the one
    /// of spaces, and the same at the other two statuses the window holds.</summary>
    [Theory]
    [InlineData(200)]
    [InlineData(204)]
    [InlineData(299)]
    public void ABodyThatIsNotThereIsNeverHealthy(int statusCode)
    {
        Assert.False(ContainerAppProbe.IsHealthyPayload(statusCode, null));
        Assert.False(ContainerAppProbe.IsHealthyPayload(statusCode, ""));
        Assert.False(ContainerAppProbe.IsHealthyPayload(statusCode, " "));
        Assert.False(ContainerAppProbe.IsHealthyPayload(statusCode, "\t\r\n "));
    }

    /// <summary>
    /// The plain word, whole: in any case the browser may show it in and around any whitespace a server may pad it
    /// with, since the rule trims and compares ordinally. Anything that is the word and something else, or something
    /// else and the word, is refused - <c>Unhealthy</c> above all, which carries the word inside it, and a word with
    /// a mark on it, and the word quoted, and the word with a space in the middle of it.
    /// </summary>
    [Theory]
    [InlineData("Healthy", true)]
    [InlineData("healthy", true)]
    [InlineData("HEALTHY", true)]
    [InlineData("HeAlThY", true)]
    [InlineData(" Healthy", true)]
    [InlineData("Healthy ", true)]
    [InlineData("\tHealthy\r\n", true)]
    [InlineData("Unhealthy", false)]
    [InlineData("Healthy?", false)]
    [InlineData("Healthy again", false)]
    [InlineData("The app is Healthy", false)]
    [InlineData("Health", false)]
    [InlineData("Healthyy", false)]
    [InlineData("Healt hy", false)]
    [InlineData("\"Healthy\"", false)]
    [InlineData("Healthy\r\nand warming", false)]
    public void ThePlainWordIsReadInAnyCaseAndAroundAnyWhitespace(string body, bool healthy) =>
        Assert.Equal(healthy, ContainerAppProbe.IsHealthyPayload(200, body));

    /// <summary>
    /// A JSON body is healthy on its own <c>status</c> and on nothing else: the name is read as it is spelled, so
    /// <c>Status</c> and <c>STATUS</c> say nothing the page reads, and the value is read as a string, so a
    /// <c>null</c>, a number, a truth and an object are all of them no answer - even an object that holds the word.
    /// The value is compared whole and untrimmed, so <c>" Healthy "</c> is no healthy answer either, while any case
    /// of the word inside the string is. Where it stands in the object does not matter - beside other parts, or under
    /// a nested part that says the opposite - and a body that is no object at all, an array or an empty brace, is no
    /// answer.</summary>
    [Theory]
    [InlineData("""{"status":"Healthy"}""", true)]
    [InlineData("""{ "status" : "Healthy" }""", true)]
    [InlineData("""{"status":"healthy"}""", true)]
    [InlineData("""{"status":"HEALTHY"}""", true)]
    [InlineData("""{"status":"Healthy","checkedAt":"2026-10-04T22:00:00Z"}""", true)]
    [InlineData("""{"checks":[],"status":"Healthy"}""", true)]
    [InlineData("""{"outer":{"status":"Unhealthy"},"status":"Healthy"}""", true)]
    [InlineData("""{"status":"Unhealthy"}""", false)]
    [InlineData("""{"status":""}""", false)]
    [InlineData("""{"status":" "}""", false)]
    [InlineData("""{"status":" Healthy "}""", false)]
    [InlineData("""{"status":"Healthy?"}""", false)]
    [InlineData("""{"status":null}""", false)]
    [InlineData("""{"status":true}""", false)]
    [InlineData("""{"status":123}""", false)]
    [InlineData("""{"status":{"value":"Healthy"}}""", false)]
    [InlineData("""{"status":["Healthy"]}""", false)]
    [InlineData("""{"Status":"Healthy"}""", false)]
    [InlineData("""{"STATUS":"Healthy"}""", false)]
    [InlineData("""{"state":"Healthy"}""", false)]
    [InlineData("""{"health":{"status":"Healthy"}}""", false)]
    [InlineData("""{}""", false)]
    [InlineData("""[]""", false)]
    [InlineData("""[{"status":"Healthy"}]""", false)]
    public void AJsonBodyIsHealthyOnItsOwnStatusAlone(string body, bool healthy) =>
        Assert.Equal(healthy, ContainerAppProbe.IsHealthyPayload(200, body));

    /// <summary>
    /// A body that opens like JSON and is not JSON is no answer, and the rule says so rather than throwing: the
    /// brace with nothing behind it, the object cut off in the middle, the text with something behind the closing
    /// brace, and the byte-order mark, which is no whitespace so the first character the rule reads is the mark and
    /// not the brace. An HTML page that holds the word is refused the same way - it does not open with a brace at
    /// all - which is the shape of the answer Azure sends for an app that has gone.</summary>
    [Theory]
    [InlineData("{")]
    [InlineData("{\"status\":")]
    [InlineData("{\"status\":\"Healthy\"")]
    [InlineData("{not json}")]
    [InlineData("{,}")]
    [InlineData("{]}")]
    [InlineData("{\"status\":\"Healthy\"} trailing")]
    [InlineData("\ufeff{\"status\":\"Healthy\"}")]
    [InlineData("<!DOCTYPE html><html><body>Healthy</body></html>")]
    [InlineData("<html><body><h1>Container App - Unavailable</h1></body></html>")]
    public void ABodyThatOpensLikeJsonAndIsNotJsonIsNoAnswer(string body) =>
        Assert.False(ContainerAppProbe.IsHealthyPayload(200, body));
}
