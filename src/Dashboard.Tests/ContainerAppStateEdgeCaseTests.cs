namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ContainerAppState" /> - the six words the page has
/// for what it last heard of one container app - and of the rule that lands an app on one of them,
/// <see cref="ContainerAppProbe.Record" />, with the three lengths that rule counts to:
/// <see cref="ContainerAppProbe.Interval" />, <see cref="ContainerAppProbe.Grace" /> and
/// <see cref="ContainerAppProbe.Asleep" />.
/// <para>
/// What the states are worth is pinned where it is decided and said: <see cref="ContainerAppProbeTests" /> walks the
/// table - a first silence waking, a 404 and two minutes of it not responding, a 5xx unhealthy, a 2xx with a healthy
/// payload healthy, an unread answer responding, and an app that slept six minutes waking once more - and holds the
/// answers a browser really gets, read through a handler the test answers itself, as well as the monitor's round
/// that asks the apps it shows once a minute and nothing else;
/// <see cref="ContainerAppTargetEdgeCaseTests" /> holds the target the probe stands on;
/// <see cref="HealthHelperCoverageTests" /> pins every state's word in the badge and the tooltip of a healthy, a
/// gone, a refused, a waking and a not-yet-asked app, and <see cref="RuntimePayloadHelperTests" /> the whole facts
/// line of every combination of status, latency, opacity and state;
/// <see cref="RuntimePayloadBuilderTests" /> what each state is worth in a tile.
/// </para>
/// <para>
/// Left open by all of that, and pinned here, is the rest: the six states as the numbers, the names and the order
/// they are declared in, and nothing outside them however it is arrived at - cast, read out of text, the badge's own
/// wording offered to the parser, or made by combining two states, which this enum neither offers nor forbids. Then
/// the three lengths as the ticks they are, and the rule stepped across each of them a tick at a time: a probe due
/// on the very tick the minute ends, an app still waking a tick before its grace and not responding on it, a run
/// begun again on the tick the sleep ends and not a tick before. Then the edges the table in
/// <see cref="ContainerAppProbeTests" /> walks up to but never stands on: the 199 and the 300 of a body that says
/// healthy, the 403 and the 405 around the one status that means gone, and the answer at a path that is no health
/// path at all - which no status, not even a 2xx with a healthy body and not even a 404, ever decides. Then what a
/// state remembers: the run's one answer keeping an app out of its grace forever after, a refusal of the page's
/// origin surviving every later state, and an answer recorded before the probe was ever started counting its grace
/// from itself. Last the two holders a state stands in - the status with its state named and nothing else, the
/// answer's <c>Answered</c> across the combinations of a status it read and an opacity it could not - and the one
/// argument the rule refuses by name.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the members that would are
/// <see cref="NodeProber.ProbeContainerAppAsync" /> and the monitor's own round, both pinned with a handler the test
/// answers in <see cref="ContainerAppProbeTests" />. Every moment below is one the test brought, which is what lets
/// each boundary be stood on to the tick.
/// </para>
/// </summary>
public class ContainerAppStateEdgeCaseTests
{
    /// <summary>The moment the page opened and the first probe was started: every moment below is counted from it.</summary>
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

    /// <summary>An answer the browser could not read (CORS): the app answered, and nothing more is known.</summary>
    private static ContainerAppAnswer Unreadable(DateTimeOffset at) => new(HealthPath, true, null, true, 85, at);

    /// <summary>A probe started at <see cref="Opened" /> - so its run counts its grace from there.</summary>
    private static ContainerAppProbe Started()
    {
        var probe = new ContainerAppProbe(Named);
        probe.Start(Opened);

        return probe;
    }

    // ------------------------------------------------------------------------ the six states

    /// <summary>The six words the page has for a container app, in the order they stand in the enum and on the six
    /// numbers that stand from 0 to 5: a seventh would be a state no badge, no tile and no sentence of the page
    /// knows, and the order is the order of the declaration and nothing to do with which of them is the worse news -
    /// that ranking is the tiles' rule, not this enum's.</summary>
    [Fact]
    public void TheSixStatesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(
            ["Probing", "Waking", "Responding", "Healthy", "Unhealthy", "NotResponding"],
            Enum.GetNames<ContainerAppState>());
        Assert.Equal([0, 1, 2, 3, 4, 5], Enum.GetValues<ContainerAppState>().Select(state => (int)state));
        Assert.Equal(6, Enum.GetValues<ContainerAppState>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(ContainerAppState)));
    }

    /// <summary>Each of the six states on its own number and on its own name, both ways round: the number is the one
    /// the name stands on, the name is the one that number answers to, and every one of them is defined however it
    /// is arrived at. The word a viewer reads under the badge is <see cref="HealthHelperCoverageTests" />'s business;
    /// the word the enum wears is this one.</summary>
    [Theory]
    [InlineData(ContainerAppState.Probing, 0, "Probing")]
    [InlineData(ContainerAppState.Waking, 1, "Waking")]
    [InlineData(ContainerAppState.Responding, 2, "Responding")]
    [InlineData(ContainerAppState.Healthy, 3, "Healthy")]
    [InlineData(ContainerAppState.Unhealthy, 4, "Unhealthy")]
    [InlineData(ContainerAppState.NotResponding, 5, "NotResponding")]
    public void EachStateStandsOnItsOwnNumberAndItsOwnName(ContainerAppState state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (ContainerAppState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>The first state is what a container app state that was never set is - and it is the one that says
    /// nothing has been asked yet, so a half-built status reads as a page that has not probed rather than as an app
    /// that is well. Outside the six there is nothing: neither one below the first nor one above the last nor either
    /// end of <see cref="int" /> is a defined state, and a value the enum does not name has no name of its own - it
    /// says the number it stands on, the only honest thing left to say of a state nothing ever put an app in.</summary>
    [Fact]
    public void AnUnsetStateIsTheFirstOneAndNothingOutsideTheSixIsAState()
    {
        Assert.Equal(ContainerAppState.Probing, default);
        Assert.Equal(ContainerAppState.Probing, (ContainerAppState)0);

        Assert.True(Enum.IsDefined(ContainerAppState.Probing));
        Assert.True(Enum.IsDefined(ContainerAppState.NotResponding));
        Assert.False(Enum.IsDefined((ContainerAppState)(-1)));
        Assert.False(Enum.IsDefined((ContainerAppState)6));
        Assert.False(Enum.IsDefined((ContainerAppState)int.MinValue));
        Assert.False(Enum.IsDefined((ContainerAppState)int.MaxValue));

        Assert.Null(Enum.GetName((ContainerAppState)6));
        Assert.Null(Enum.GetName((ContainerAppState)(-1)));
        Assert.Null(Enum.GetName((ContainerAppState)int.MaxValue));
        Assert.Equal("6", ((ContainerAppState)6).ToString());
        Assert.Equal("-2147483648", ((ContainerAppState)int.MinValue).ToString());
    }

    /// <summary>The six states apart from one another and compared by the numbers they stand on: each is its own
    /// equal in the box as out of it, an <see cref="int" /> on the same number is no state's equal, and the order is
    /// the declaration alone - <c>Probing</c> before <c>Waking</c> before <c>Responding</c>, which are the three that
    /// say nothing about health, then <c>Healthy</c>, and the two bad words last.</summary>
    [Fact]
    public void TheSixStatesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        ContainerAppState[] states =
        [
            ContainerAppState.Probing, ContainerAppState.Waking, ContainerAppState.Responding,
            ContainerAppState.Healthy, ContainerAppState.Unhealthy, ContainerAppState.NotResponding,
        ];

        Assert.All(states, state => Assert.Single(states, other => other == state));
        Assert.Equal(states.Length, states.Distinct().Count());

        Assert.True(ContainerAppState.Probing < ContainerAppState.Waking);
        Assert.True(ContainerAppState.Waking < ContainerAppState.Responding);
        Assert.True(ContainerAppState.Responding < ContainerAppState.Healthy);
        Assert.True(ContainerAppState.Healthy < ContainerAppState.Unhealthy);
        Assert.True(ContainerAppState.Unhealthy < ContainerAppState.NotResponding);
        Assert.False(ContainerAppState.Healthy < ContainerAppState.Responding);
        Assert.Equal(1, ContainerAppState.Unhealthy.CompareTo(ContainerAppState.Healthy));
        Assert.Equal(-1, ContainerAppState.Probing.CompareTo(ContainerAppState.NotResponding));
        Assert.Equal(0, ContainerAppState.Waking.CompareTo((object)ContainerAppState.Waking));

        Assert.Equal((object)ContainerAppState.Healthy, (object)ContainerAppState.Healthy);
        Assert.False(((object)ContainerAppState.Healthy).Equals(null));
        Assert.False(ContainerAppState.Healthy.Equals(null));
        Assert.False(ContainerAppState.Healthy.Equals((object)3));
        Assert.Equal(ContainerAppState.Healthy.GetHashCode(), ((object)ContainerAppState.Healthy).GetHashCode());
    }

    /// <summary>A state is a choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so the operators
    /// work on the numbers underneath rather than on the states. Two of these numbers do combine into a third state -
    /// <c>Waking</c> with <c>Responding</c> is <c>Healthy</c>, the very word the page must never hand out for a
    /// silence that stopped and an answer it could not read - while the rest land outside the six, where no word
    /// stands. Adding <c>Probing</c> to anything changes nothing, because its number is zero, and the intersection
    /// of two states that share no bit is <c>Probing</c>: the word for nothing having happened. The numbers are held
    /// here so that the day somebody reaches for <c>|</c> on a state, the tests say what it costs.</summary>
    [Fact]
    public void TwoStatesCombinedAreAnotherStateOrNoStateAtAll()
    {
        Assert.False(typeof(ContainerAppState).IsDefined(typeof(FlagsAttribute), false));

        Assert.Equal(ContainerAppState.Healthy, ContainerAppState.Waking | ContainerAppState.Responding);
        Assert.Equal(ContainerAppState.Healthy, ContainerAppState.Responding | ContainerAppState.Waking);
        Assert.Equal(ContainerAppState.Healthy, ContainerAppState.Waking | ContainerAppState.Healthy);
        Assert.Equal(ContainerAppState.NotResponding, ContainerAppState.Unhealthy | ContainerAppState.NotResponding);
        Assert.Equal(ContainerAppState.Unhealthy, ContainerAppState.Unhealthy | ContainerAppState.Probing);
        Assert.Equal(ContainerAppState.Probing, ContainerAppState.Unhealthy & ContainerAppState.Probing);

        Assert.Equal((ContainerAppState)7, ContainerAppState.Healthy | ContainerAppState.Unhealthy);
        Assert.False(Enum.IsDefined(ContainerAppState.Healthy | ContainerAppState.Unhealthy));
        Assert.Equal((ContainerAppState)7, ContainerAppState.Responding | ContainerAppState.NotResponding);
        Assert.False(Enum.IsDefined(ContainerAppState.Responding | ContainerAppState.NotResponding));

        Assert.Equal(ContainerAppState.Probing, ContainerAppState.Waking & ContainerAppState.Responding);
        Assert.Equal(ContainerAppState.Probing, ContainerAppState.Healthy & ContainerAppState.Unhealthy);
        Assert.Equal(ContainerAppState.Unhealthy, ContainerAppState.Unhealthy & ContainerAppState.NotResponding);
        Assert.Equal(ContainerAppState.Waking, ContainerAppState.Healthy & ContainerAppState.NotResponding);
    }

    // ------------------------------------------------------------------------ a state out of text

    /// <summary>A state read out of text is the state the text names, in the case the enum spells it, or the state
    /// the number it holds stands on: a name is read whole and the spaces around it walked past - which is all the
    /// trimming the page may ever count on - and a number with a cipher in front of it is still the small number it
    /// says.</summary>
    [Theory]
    [InlineData("Probing", ContainerAppState.Probing)]
    [InlineData("Waking", ContainerAppState.Waking)]
    [InlineData("Responding", ContainerAppState.Responding)]
    [InlineData("Healthy", ContainerAppState.Healthy)]
    [InlineData("Unhealthy", ContainerAppState.Unhealthy)]
    [InlineData("NotResponding", ContainerAppState.NotResponding)]
    [InlineData("  Healthy  ", ContainerAppState.Healthy)]
    [InlineData("0", ContainerAppState.Probing)]
    [InlineData("5", ContainerAppState.NotResponding)]
    [InlineData("02", ContainerAppState.Responding)]
    public void AStateTextIsTheStateItNamesOrTheStateItNumbers(string text, ContainerAppState expected)
    {
        Assert.True(Enum.TryParse<ContainerAppState>(text, out var state));
        Assert.Equal(expected, state);
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>Text that is no state is refused - <see langword="null" />, the empty string, a blank of spaces or of
    /// a tab, a name in another case, a name with something behind it, a fraction, a hexadecimal, a word that means
    /// much the same thing, and the word the badge wears: <c>Not responding</c>, with its space, is what a viewer
    /// reads and is no name of the enum, which is why the tooltip is said from the state and never read back into
    /// one. A refused parse leaves its answer standing on <see cref="ContainerAppState.Probing" />: the
    /// <c>false</c> is the whole answer, and a caller that reads the value anyway is shown a page that has not asked
    /// yet - the mildest of the six and the only one that promises nothing.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("healthy")]
    [InlineData("NOTRESPONDING")]
    [InlineData("Probing?")]
    [InlineData("Not responding")]
    [InlineData("2.0")]
    [InlineData("0x2")]
    [InlineData("Answered")]
    public void TextThatIsNoStateIsRefusedAndLeavesProbingBehind(string? text)
    {
        Assert.False(Enum.TryParse<ContainerAppState>(text, out var state));
        Assert.Equal(ContainerAppState.Probing, state);
        Assert.False(Enum.TryParse<ContainerAppState>(text, ignoreCase: false, out _));
    }

    /// <summary>The one text that is no name and is not refused is a number outside the six: the parse answers truly
    /// and hands back a value that is no state at all, which is why a read number is worth nothing until
    /// <see cref="Enum.IsDefined(Enum)" /> has been asked about it too - the badge would say <c>Probing</c>, the
    /// label's <c>_</c>, for an app the rule never put in any state. A name in a case the enum does not write, on the
    /// other hand, is a state as soon as the reader is told to forgive the case - and is nothing until it is.</summary>
    [Fact]
    public void ANumberOutsideTheSixIsReadAsAStateThatIsNoState()
    {
        Assert.True(Enum.TryParse<ContainerAppState>("6", out var above));
        Assert.Equal((ContainerAppState)6, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));
        Assert.Equal("Probing", ContainerAppText.Label(above));

        Assert.True(Enum.TryParse<ContainerAppState>("-1", out var below));
        Assert.Equal((ContainerAppState)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<ContainerAppState>("notresponding", out _));
        Assert.True(Enum.TryParse<ContainerAppState>("notresponding", ignoreCase: true, out var forgiven));
        Assert.Equal(ContainerAppState.NotResponding, forgiven);
    }

    // ------------------------------------------------------------------------ the three lengths a state is counted in

    /// <summary>The three lengths the states are counted in, as the ticks they are rather than as the seconds a
    /// table happens to try: a minute between probes, ninety seconds of grace before a silence is called not
    /// responding, five minutes of nothing before an app may be asleep and its next probe starts a new run. They
    /// stand in that order - the grace longer than the minute a probe waits by, the sleep longer than both - and a
    /// change of any one of them moves a boundary every test below stands on. The grace is the one length the page
    /// also says out loud, the ninety seconds a container app's tooltip promises its viewer.</summary>
    [Fact]
    public void TheThreeLengthsAreAMinuteNinetySecondsAndFiveMinutes()
    {
        Assert.Equal(TimeSpan.FromSeconds(60), ContainerAppProbe.Interval);
        Assert.Equal(600_000_000L, ContainerAppProbe.Interval.Ticks);
        Assert.Equal(TimeSpan.FromSeconds(90), ContainerAppProbe.Grace);
        Assert.Equal(900_000_000L, ContainerAppProbe.Grace.Ticks);
        Assert.Equal(TimeSpan.FromMinutes(5), ContainerAppProbe.Asleep);
        Assert.Equal(3_000_000_000L, ContainerAppProbe.Asleep.Ticks);

        Assert.True(ContainerAppProbe.Interval < ContainerAppProbe.Grace);
        Assert.True(ContainerAppProbe.Grace < ContainerAppProbe.Asleep);
        Assert.Equal(90d, ContainerAppProbe.Grace.TotalSeconds);
    }

    // ------------------------------------------------------------------------ Probing: before the first answer

    /// <summary><c>Probing</c> is the state of a page that has not heard anything: it is what a new probe holds, with
    /// no last answer and no remembered refusal, and it is still what the probe holds after it has been started -
    /// starting a probe is not an answer, and the word only changes when one is recorded. The probe keeps the target
    /// it was given, as the very record it was given it in, and asks nothing of the network by being built.</summary>
    [Fact]
    public void ProbingIsTheStateUntilAnAnswerIsRecordedEvenOnceTheProbeIsStarted()
    {
        var probe = new ContainerAppProbe(Named);

        Assert.Equal(ContainerAppState.Probing, probe.Status.State);
        Assert.Null(probe.Status.Last);
        Assert.False(probe.Status.CorsRefused);
        Assert.True(probe.IsNew);
        Assert.Same(Named, probe.Target);

        probe.Start(Opened);

        Assert.Equal(ContainerAppState.Probing, probe.Status.State);
        Assert.False(probe.IsNew);
        Assert.Null(probe.Status.Last);
        Assert.Same(Named, probe.Target);
    }

    /// <summary>The one status of nothing: <see cref="ContainerAppStatus.NotYet" /> is a probing status with no
    /// answer and no refusal, the very status a new probe holds, and the one instance handed to every caller that
    /// asks - a record of its own state, equal to another status that says the same three things.</summary>
    [Fact]
    public void NotYetIsTheProbingStatusOfNoAnswerAndNoRefusalAndTheSameInstanceThroughout()
    {
        Assert.Equal(ContainerAppState.Probing, ContainerAppStatus.NotYet.State);
        Assert.Null(ContainerAppStatus.NotYet.Last);
        Assert.False(ContainerAppStatus.NotYet.CorsRefused);
        Assert.Same(ContainerAppStatus.NotYet, ContainerAppStatus.NotYet);
        Assert.Equal(new ContainerAppStatus(ContainerAppState.Probing), ContainerAppStatus.NotYet);
        Assert.NotSame(new ContainerAppStatus(ContainerAppState.Probing), ContainerAppStatus.NotYet);
        Assert.Equal("ContainerAppStatus { State = Probing, Last = , CorsRefused = False }", ContainerAppStatus.NotYet.ToString());
    }

    /// <summary>A probe is due before it has ever been started, whichever moment it is asked about, and once one has
    /// been started it is due again on the very tick the minute ends and not a tick before it - the comparison is the
    /// inclusive one, which is what lets the table in <see cref="ContainerAppProbeTests" /> turn on 59 and 60 seconds
    /// and leave the tick between them unsaid.</summary>
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void AStartedProbeIsDueAgainOnTheVeryTickTheMinuteEnds(int ticks, bool due)
    {
        var probe = Started();

        Assert.Equal(due, probe.IsDue(Opened + ContainerAppProbe.Interval + TimeSpan.FromTicks(ticks)));
    }

    /// <summary>The wait between two probes is counted and never compared in words: the moment the last one started
    /// is not due again, neither is a hair of a second short of the minute, and a moment that stands behind the last
    /// probe - the viewer's own clock put back, the bottom of the clock itself - is not due either, since the
    /// difference comes out under zero and never reaches the length it would have to pass. The minute is the only
    /// length the wait is measured against, so a probe is asked again before an app's grace has run out, and five
    /// times over before the page lets a silence stand for an app that has gone to sleep.</summary>
    [Fact]
    public void TheWaitForTheNextProbeIsCountedAndNotComparedInWords()
    {
        var probe = Started();

        Assert.False(probe.IsDue(Opened));
        Assert.False(probe.IsDue(Opened + ContainerAppProbe.Interval - TimeSpan.FromSeconds(1)));
        Assert.False(probe.IsDue(Opened - TimeSpan.FromSeconds(1)));
        Assert.False(probe.IsDue(Opened - ContainerAppProbe.Interval));
        Assert.False(probe.IsDue(DateTimeOffset.MinValue));
        Assert.True(probe.IsDue(Opened + ContainerAppProbe.Interval));
        Assert.True(probe.IsDue(Opened + ContainerAppProbe.Grace - TimeSpan.FromSeconds(1)));
        Assert.True(probe.IsDue(Opened + ContainerAppProbe.Grace));
        Assert.True(probe.IsDue(Opened + ContainerAppProbe.Asleep));
        Assert.True(probe.IsDue(DateTimeOffset.MaxValue));
    }

    /// <summary>Before anything is started, every moment is due: there is no last probe to space the next one from,
    /// so a page that has never asked asks at once - at either end of the clock and a moment behind them.</summary>
    [Fact]
    public void AProbeThatWasNeverStartedIsDueAtEveryMoment()
    {
        var probe = new ContainerAppProbe(Named);

        Assert.True(probe.IsDue(Opened));
        Assert.True(probe.IsDue(Opened - ContainerAppProbe.Interval));
        Assert.True(probe.IsDue(DateTimeOffset.MinValue));
        Assert.True(probe.IsDue(DateTimeOffset.MaxValue));
    }

    // ------------------------------------------------------------------------ Waking and NotResponding: the grace

    /// <summary>An app that has not answered is waking up to the very tick its grace ends and not responding on that
    /// tick: the comparison is a strict one, so eighty-nine seconds and a tick of silence still excuse the app and
    /// ninety seconds exactly do not. The table in <see cref="ContainerAppProbeTests" /> stands on whole seconds -
    /// 0, 60 and 120 - and leaves this tick to say here.</summary>
    [Theory]
    [InlineData(-1, ContainerAppState.Waking)]
    [InlineData(0, ContainerAppState.NotResponding)]
    [InlineData(1, ContainerAppState.NotResponding)]
    public void AnAppThatNeverAnswersTurnsOnTheTickItsGraceEnds(int ticks, ContainerAppState expected)
    {
        var probe = Started();
        var at = Opened + ContainerAppProbe.Grace + TimeSpan.FromTicks(ticks);

        probe.Record(Silent(at));

        Assert.Equal(expected, probe.Status.State);
        Assert.Equal(at, probe.Status.Last!.CheckedAt);
        Assert.False(probe.Status.Last.Answered);
    }

    /// <summary>A grace measured from nothing: a probe that was never started has no run to count from, and the
    /// answer it is given becomes the start of one - so a silence recorded on it is waking however long after the
    /// page opened the browser says it was, and the state never arrives by itself.</summary>
    [Fact]
    public void AnAnswerRecordedBeforeTheProbeWasStartedCountsItsGraceFromItself()
    {
        var probe = new ContainerAppProbe(Named);

        probe.Record(Silent(Opened));

        Assert.Equal(ContainerAppState.Waking, probe.Status.State);
        Assert.True(probe.IsNew);

        var late = new ContainerAppProbe(Named);
        late.Record(Silent(Opened + TimeSpan.FromDays(365)));

        Assert.Equal(ContainerAppState.Waking, late.Status.State);
        Assert.True(late.IsNew);
    }

    /// <summary>One answer is enough to leave the grace for good: an app the page could reach but not read (CORS)
    /// has answered, so the silence that follows it, a minute on and well inside the ninety seconds, is not a second
    /// waking but a first not responding. The same is said of an app that answered properly, which
    /// <see cref="ContainerAppProbeTests" /> pins; what is pinned here is that <c>Responding</c> counts as an answer
    /// as much as a status does, and that the state of the answer is what the rule reads and not its wording.</summary>
    [Fact]
    public void AnAppThatOnceAnsweredNeverGoesBackToWakingInsideItsGrace()
    {
        var probe = Started();

        probe.Record(Unreadable(Opened));
        Assert.Equal(ContainerAppState.Responding, probe.Status.State);

        probe.Start(Opened + ContainerAppProbe.Interval);
        probe.Record(Silent(Opened + ContainerAppProbe.Interval));

        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
        Assert.Equal("no answer", ContainerAppText.Facts(probe.Status));
    }

    // ------------------------------------------------------------------------ Waking and NotResponding: the sleep

    /// <summary>A new run begins on the very tick the sleep ends: five minutes without a probe - the page was hidden,
    /// paused or closed - and the app may have scaled to zero, so the next silence is a waking app again. A tick
    /// short of those five minutes is the same run as ever, its grace long spent, and the app is not responding.
    /// <see cref="ContainerAppProbeTests" /> pins the six-minute gap; the tick the rule turns on is pinned here.</summary>
    [Theory]
    [InlineData(-1, ContainerAppState.NotResponding)]
    [InlineData(0, ContainerAppState.Waking)]
    [InlineData(1, ContainerAppState.Waking)]
    public void ASleepAppWakesAgainOnTheTickTheFiveMinutesEnd(int ticks, ContainerAppState expected)
    {
        var probe = Started();
        probe.Record(Answer(200, healthyPayload: true));

        var at = Opened + ContainerAppProbe.Asleep + TimeSpan.FromTicks(ticks);
        probe.Start(at);
        probe.Record(Silent(at));

        Assert.Equal(expected, probe.Status.State);
    }

    // ------------------------------------------------------------------------ Healthy: the status edge

    /// <summary>The two hundred a healthy body is read in: the status must be a 2xx, and the edge of that is the
    /// number the rule reads and not the number a viewer would excuse - 199 is not healthy, 200 and 299 are, and
    /// the 300 that redirects away is not. The one status that means gone is the 404 itself: a 403 and a 405 are
    /// unhealthy like every other answer that is not a healthy two hundred, and no payload, however healthy it
    /// says it is, lifts a 5xx out of it.</summary>
    [Theory]
    [InlineData(199, ContainerAppState.Unhealthy)]
    [InlineData(200, ContainerAppState.Healthy)]
    [InlineData(204, ContainerAppState.Healthy)]
    [InlineData(299, ContainerAppState.Healthy)]
    [InlineData(300, ContainerAppState.Unhealthy)]
    [InlineData(301, ContainerAppState.Unhealthy)]
    [InlineData(403, ContainerAppState.Unhealthy)]
    [InlineData(404, ContainerAppState.NotResponding)]
    [InlineData(405, ContainerAppState.Unhealthy)]
    [InlineData(500, ContainerAppState.Unhealthy)]
    [InlineData(599, ContainerAppState.Unhealthy)]
    public void OnlyATwoHundredOfTheHealthPathWithAHealthyPayloadIsHealthy(int status, ContainerAppState expected)
    {
        var probe = Started();
        probe.Record(Answer(status, healthyPayload: true));

        Assert.Equal(expected, probe.Status.State);
    }

    /// <summary>The other edge of the same rule: the payload is read only when the path it came from is a health
    /// path. A body that says healthy of a root page is a page that happens to hold the word, a 5xx of it is still
    /// an answer the page cannot class, and a 404 of it is not the deleted app the rule reads at the health path -
    /// so all of them are <c>Responding</c>, the one state that admits the page knows nothing. Which answer reads as
    /// one from a health path is the target's own rule, pinned by <see cref="ContainerAppTargetEdgeCaseTests" />;
    /// what is pinned here is what the state machine makes of an answer that does not.</summary>
    [Theory]
    [InlineData(200, true)]
    [InlineData(204, false)]
    [InlineData(302, false)]
    [InlineData(403, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    public void AnAnswerAtAPathThatIsNoHealthCheckIsRespondingWhicheverStatusItCarries(int status, bool healthyPayload)
    {
        var probe = Started();
        probe.Record(new ContainerAppAnswer("/", false, status, false, 85, Opened, HealthyPayload: healthyPayload));

        Assert.Equal(ContainerAppState.Responding, probe.Status.State);
        Assert.True(probe.Status.Last!.Answered);
        Assert.False(probe.Status.CorsRefused);
    }

    // ------------------------------------------------------------------------ Responding: remembered

    /// <summary>A refusal of the page's origin is remembered through every state that follows it, because what the
    /// page cannot see does not become visible by asking again: the flag is not set by an answer the page read, is
    /// set by the first one it could not, and stands through a healthy two hundred and through the silence that
    /// turns it into not responding. The status it stands in is a record of the state and of that flag alone -
    /// which is why a tile may say <c>Healthy</c> next to a tooltip that says the page cannot read the answer.</summary>
    [Fact]
    public void ARefusalOfThePagesOriginStandsThroughEveryLaterState()
    {
        var probe = Started();

        probe.Record(Answer(200, healthyPayload: true));
        Assert.False(probe.Status.CorsRefused);

        probe.Start(Opened + ContainerAppProbe.Interval);
        probe.Record(Unreadable(Opened + ContainerAppProbe.Interval));
        Assert.Equal(ContainerAppState.Responding, probe.Status.State);
        Assert.True(probe.Status.CorsRefused);

        probe.Start(Opened + TimeSpan.FromSeconds(120));
        probe.Record(Answer(200, healthyPayload: true, at: Opened + TimeSpan.FromSeconds(120)));
        Assert.Equal(ContainerAppState.Healthy, probe.Status.State);
        Assert.True(probe.Status.CorsRefused);

        probe.Start(Opened + TimeSpan.FromSeconds(180));
        probe.Record(Silent(Opened + TimeSpan.FromSeconds(180)));
        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
        Assert.True(probe.Status.CorsRefused);
    }

    // ------------------------------------------------------------------------ the two holders a state stands in

    /// <summary>A status of a state and nothing else carries nothing else: of the six states, every one built with
    /// its state alone stands with no last answer and no remembered refusal, the two optional parts falling to
    /// <see langword="null" /> and <see langword="false" /> rather than to anything else, and the state printing in
    /// the record's own words as the name the enum wears - not as the word the badge shows. The state decides
    /// nothing about the parts: a <c>Healthy</c> status with no answer is possible, absurd and held exactly as
    /// given.</summary>
    [Theory]
    [InlineData(ContainerAppState.Probing, "Probing")]
    [InlineData(ContainerAppState.Waking, "Waking")]
    [InlineData(ContainerAppState.Responding, "Responding")]
    [InlineData(ContainerAppState.Healthy, "Healthy")]
    [InlineData(ContainerAppState.Unhealthy, "Unhealthy")]
    [InlineData(ContainerAppState.NotResponding, "NotResponding")]
    public void AStatusOfAStateAloneCarriesNoAnswerAndNoRefusal(ContainerAppState state, string name)
    {
        var status = new ContainerAppStatus(state);

        Assert.Equal(state, status.State);
        Assert.Null(status.Last);
        Assert.False(status.CorsRefused);
        Assert.Equal($"ContainerAppStatus {{ State = {name}, Last = , CorsRefused = False }}", status.ToString());
        Assert.Equal(status, new ContainerAppStatus(state));
        Assert.NotEqual(status, status with { CorsRefused = true });
        Assert.NotEqual(status, status with { Last = Silent(Opened) });
    }

    /// <summary>Whether an answer counts as one: the app answered if the browser read a status or if it could not,
    /// and did not answer if it got neither - and the combination that should never happen, a status the page also
    /// could not read, counts as answered rather than not at all. It is this one flag that keeps a CORS app out of
    /// its grace for ever, and it is derived from the two parts rather than given.</summary>
    [Theory]
    [InlineData(200, false, true)]
    [InlineData(404, false, true)]
    [InlineData(null, true, true)]
    [InlineData(null, false, false)]
    [InlineData(200, true, true)]
    public void AnAnswerIsAnsweredByAStatusOrByAnOpacityItCannotRead(int? status, bool opaque, bool answered)
    {
        var answer = new ContainerAppAnswer(HealthPath, true, status, opaque, null, Opened);

        Assert.Equal(answered, answer.Answered);
    }

    /// <summary>The rule's one refusal, named: no answer is not an app that is waking, and the record says which
    /// argument it wanted. A record of nothing changes neither the state the probe holds nor the flag it holds with
    /// it, which is what the page would otherwise show a viewer.</summary>
    [Fact]
    public void AnAnswerThatIsNotThereIsRefusedByName()
    {
        var probe = Started();
        var before = probe.Status;

        var error = Assert.Throws<ArgumentNullException>(() => probe.Record(null!));

        Assert.Equal("answer", error.ParamName);
        Assert.Equal(before, probe.Status);
        Assert.Equal(ContainerAppState.Probing, probe.Status.State);
    }

    // ------------------------------------------------------------------------ the states the tooltip tells apart

    /// <summary>The tooltip opens with the state's own word and promises the ninety seconds of grace to exactly the
    /// two states that have not answered - <c>Probing</c> and <c>Waking</c> - and to none of the four that have
    /// heard something or given up. Every state is said with the same answer underneath it, a 200 the page read, so
    /// what changes between the six sentences is the state and nothing else: the words of the answer stand in
    /// <see cref="HealthHelperCoverageTests" />.</summary>
    [Theory]
    [InlineData(ContainerAppState.Probing, "Probing", true)]
    [InlineData(ContainerAppState.Waking, "Waking", true)]
    [InlineData(ContainerAppState.Responding, "Responding", false)]
    [InlineData(ContainerAppState.Healthy, "Healthy", false)]
    [InlineData(ContainerAppState.Unhealthy, "Unhealthy", false)]
    [InlineData(ContainerAppState.NotResponding, "Not responding", false)]
    public void TheTooltipOpensWithTheStatesWordAndPromisesTheGraceToTwoStatesOnly(
        ContainerAppState state, string word, bool waits)
    {
        var title = ContainerAppText.Title("ui", Named, new ContainerAppStatus(state, Answer(200, healthyPayload: true)), TimeZoneInfo.Utc);

        Assert.StartsWith($"ui ({App.Host}): {word}.", title, StringComparison.Ordinal);
        Assert.Equal(waits, title.Contains("waits up to 90 seconds", StringComparison.Ordinal));
    }

    /// <summary>The one state no tooltip of the other file says: an app the page reached and found unhealthy is said
    /// as its status and nothing else - no excuse about scaling to zero, since it answered, and no sentence about
    /// the app being gone, which is the 404's own and is not said of a 503 that answers.</summary>
    [Fact]
    public void TheTooltipOfAnUnhealthyAppSaysItsStatusAndNoOthersSentence()
    {
        var title = ContainerAppText.Title(
            "ui",
            Named,
            new ContainerAppStatus(ContainerAppState.Unhealthy, new ContainerAppAnswer(HealthPath, true, 503, false, 85, Opened)),
            TimeZoneInfo.Utc);

        Assert.Contains("ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Unhealthy.", title, StringComparison.Ordinal);
        Assert.Contains("Last probe 22:00:00: /_healthcheck answered HTTP 503.", title, StringComparison.Ordinal);
        Assert.DoesNotContain("HTTP 404 at the health path", title, StringComparison.Ordinal);
        Assert.DoesNotContain("scaled to zero", title, StringComparison.Ordinal);
        Assert.DoesNotContain("could not classify", title, StringComparison.Ordinal);
    }
}
