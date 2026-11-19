namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the types of <c>ContainerAppProbe.cs</c>: the runtime view's
/// watch over its container apps - <see cref="RuntimeWatch" /> and the <see cref="ContainerAppTarget" />s it
/// names, the six <see cref="ContainerAppState" />s, the <see cref="ContainerAppAnswer" />s and
/// <see cref="ContainerAppStatus" />es the page's probes hold, the <see cref="ContainerAppProbe" /> itself and the
/// words <see cref="ContainerAppText" /> says of it. What the page shows is pinned where it belongs - the probe's
/// rounds, its runs and its answers against the network in <see cref="ContainerAppProbeTests" />, the health path
/// the page asks and the number line next to the badge in <see cref="RuntimePayloadHelperTests" />, and the badge's
/// word and the tooltip's sentences in <see cref="HealthHelperCoverageTests" /> - and what they leave open is pinned
/// here: the three graces as the numbers they are, the six states as the numbers they are, the default path as the
/// path it is, a health path that is the empty string, a watch that names no app, the two parts a watch keeps and
/// the reference by which it sees its list of apps, an answer that is or is not an answer by its status and its
/// opacity, the status before the first probe as the value it is, a probe that starts and does not answer, a record
/// without a start, the ninetyth second of the grace, the five hundredth and ninety-ninth minute of the sleep, the
/// hundred that ends the two-hundreds, the four-hundreds that are not 404, an answer asked at a path that is not
/// the health path, and the words the tooltip says when the last probe has no answer and no word for why.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings, and the
/// one zone is UTC.
/// </para>
/// </summary>
public class RuntimeWatchEdgeCaseTests
{
    private static readonly DateTimeOffset Opened = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri App = new("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io");
    private static readonly Uri Demo = new("https://ca-demo.example.io");
    private static readonly ContainerAppTarget Declared = new(App, "/_healthcheck");
    private static readonly ContainerAppTarget Undeclared = new(App);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static ContainerAppAnswer NoAnswer(DateTimeOffset at) =>
        new("/_healthcheck", true, null, false, null, at, "No answer within 10 s.");

    private static ContainerAppAnswer Status(int status, DateTimeOffset at, bool healthyPayload = false, bool readsHealth = true, string path = "/_healthcheck") =>
        new(path, readsHealth, status, false, 85, at, HealthyPayload: healthyPayload);

    /// <summary>A probe started and recorded at <paramref name="seconds"/> after the page opened.</summary>
    private static void Probe(ContainerAppProbe probe, int seconds, Func<DateTimeOffset, ContainerAppAnswer> answer)
    {
        var at = Opened.AddSeconds(seconds);
        probe.Start(at);
        probe.Record(answer(at));
    }

    // ----- The three graces, as the numbers they are -----

    /// <summary>The page asks once a minute, waits ninety seconds for the first answer of a run, and gives five
    /// minutes before it lets the app scale to zero: the three numbers every boundary in this file is counted
    /// from, in the order the waiting goes.</summary>
    [Fact]
    public void TheProbeAsksAgainEveryMinuteWaitsNinetySecondsAndGivesFiveMinutesToSleep()
    {
        Assert.Equal(TimeSpan.FromSeconds(60), ContainerAppProbe.Interval);
        Assert.Equal(TimeSpan.FromSeconds(90), ContainerAppProbe.Grace);
        Assert.Equal(TimeSpan.FromMinutes(5), ContainerAppProbe.Asleep);
        Assert.True(ContainerAppProbe.Interval < ContainerAppProbe.Grace);
        Assert.True(ContainerAppProbe.Grace < ContainerAppProbe.Asleep);
    }

    // ----- The six states, as the numbers they are -----

    /// <summary>The six states of a probed container app, in the order the enum declares them: the order a cast to
    /// a number keeps, and the count the order runs to.</summary>
    [Fact]
    public void TheStatesOfAProbedContainerAppAreSixInTheOrderTheyAreDeclared()
    {
        Assert.Equal(0, (int)ContainerAppState.Probing);
        Assert.Equal(1, (int)ContainerAppState.Waking);
        Assert.Equal(2, (int)ContainerAppState.Responding);
        Assert.Equal(3, (int)ContainerAppState.Healthy);
        Assert.Equal(4, (int)ContainerAppState.Unhealthy);
        Assert.Equal(5, (int)ContainerAppState.NotResponding);
        Assert.Equal(6, Enum.GetValues<ContainerAppState>().Length);
    }

    // ----- ContainerAppTarget -----

    /// <summary>The path the page asks when the deployable names none, as the path it is.</summary>
    [Fact]
    public void TheDefaultHealthPathIsTheUnderscoreHealthcheckPath() =>
        Assert.Equal("/_healthcheck", ContainerAppTarget.DefaultHealthPath);

    /// <summary>A health path the deployable names is the path the probe asks, for every kind of probe, and the
    /// target reads health.</summary>
    [Fact]
    public void TheEffectiveHealthPathIsTheDeclaredOneWhenOneIsDeclared()
    {
        var target = new ContainerAppTarget(Demo, "/health");

        Assert.Equal("/health", target.EffectiveHealthPath);
        Assert.Equal("/health", target.PathFor(ProbeKind.Health));
        Assert.True(target.ReadsHealth(ProbeKind.Health));
    }

    /// <summary>A health path that is the empty string is no health path: the target says it asks nothing that is
    /// a health check, so its probes are never read as health, whatever they answer.</summary>
    [Fact]
    public void AHealthPathThatIsTheEmptyStringIsNoHealthPath()
    {
        var target = new ContainerAppTarget(Demo, string.Empty);

        Assert.Equal(string.Empty, target.EffectiveHealthPath);
        Assert.Equal(string.Empty, target.PathFor(ProbeKind.Liveness));
        Assert.False(target.ReadsHealth(ProbeKind.Health));
        Assert.False(target.ReadsHealth(ProbeKind.Liveness));
    }

    /// <summary>Two targets are one while both parts are - the address and the path, the path the deployable does
    /// not name standing as the nothing it is - and they say their parts in the order they are declared.</summary>
    [Fact]
    public void TwoTargetsAreOneOnlyWhileTheirPartsAreAndTheySayThemInTheirOrder()
    {
        var one = new ContainerAppTarget(Demo, "/health");

        Assert.Equal(one, new ContainerAppTarget(Demo, "/health"));
        Assert.NotEqual(one, new ContainerAppTarget(Demo, "/other"));
        Assert.NotEqual(one, new ContainerAppTarget(new Uri("https://other.example.io"), "/health"));
        Assert.Equal(
            "ContainerAppTarget { Url = https://ca-demo.example.io/, HealthPath = /health, EffectiveHealthPath = /health }",
            one.ToString());
        Assert.Equal(
            "ContainerAppTarget { Url = https://ca-demo.example.io/, HealthPath = , EffectiveHealthPath = /_healthcheck }",
            new ContainerAppTarget(Demo).ToString());
    }

    // ----- RuntimeWatch -----

    /// <summary>The watch that names no app is the one that asks nothing: no apps, and no box of the diagram that
    /// shows what Resource Health said of its resources.</summary>
    [Fact]
    public void TheWatchThatNamesNoAppHasNoAppsAndNoResourceHealthBox()
    {
        Assert.Empty(RuntimeWatch.None.Apps);
        Assert.False(RuntimeWatch.None.ResourceHealth);
    }

    /// <summary>A watch keeps the very list of apps it is given and the flag as it is given it: the page watches
    /// what it shows, and nothing more.</summary>
    [Fact]
    public void AWatchKeepsTheAppsItNamesAndTheBoxResourceHealthShows()
    {
        var apps = new List<ContainerAppTarget> { Declared, Undeclared };
        var watch = new RuntimeWatch(apps, true);

        Assert.Same(apps, watch.Apps);
        Assert.Equal([Declared, Undeclared], watch.Apps);
        Assert.True(watch.ResourceHealth);

        Assert.Empty(new RuntimeWatch([], false).Apps);
        Assert.False(new RuntimeWatch([], false).ResourceHealth);
    }

    /// <summary>Two watches are one while both parts are - the apps and the flag - and a difference in either is
    /// another watch. Record equality sees the list of apps by the reference it stands on: equal apps in another
    /// list are another watch, and a copy keeps the list it was given.</summary>
    [Fact]
    public void TwoWatchesAreOneWhileBothPartsAreAndTheAppsAreSeenByReference()
    {
        var apps = new List<ContainerAppTarget> { Declared, Undeclared };
        var one = new RuntimeWatch(apps, true);
        var again = new RuntimeWatch(apps, true);

        Assert.Equal(one, again);
        Assert.True(one == again);
        Assert.True(one.Equals((object)again));
        Assert.Equal(one.GetHashCode(), again.GetHashCode());
        Assert.False(one.Equals(null));

        var copy = one with { };
        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);
        Assert.Same(one.Apps, copy.Apps);

        Assert.NotEqual(one, new RuntimeWatch(apps, false));
        // Record equality sees the list of apps by the reference it stands on: equal apps in another list are another watch.
        Assert.NotEqual(one, new RuntimeWatch([Declared, Undeclared], true));
        Assert.NotEqual(one, new RuntimeWatch([], true));
    }

    // ----- ContainerAppAnswer -----

    /// <summary>An answer is an answer when the browser carried a status back, or when it asked without reading
    /// because the app does not allow this page's origin: neither of them alone is no answer, and the two of them
    /// together are an answer still.</summary>
    [Theory]
    [InlineData(200, false, true)]
    [InlineData(null, false, false)]
    [InlineData(200, true, true)]
    [InlineData(null, true, true)]
    public void AnAnswerIsAnAnswerWhenItCarriesAStatusOrThePageMayNotReadIt(int? statusCode, bool opaque, bool answered) =>
        Assert.Equal(answered, new ContainerAppAnswer("/_healthcheck", true, statusCode, opaque, 85, Opened).Answered);

    // ----- ContainerAppStatus -----

    /// <summary>The status before the first probe is being probed, with no answer yet and no refusal remembered:
    /// the value <c>NotYet</c> is, said in the parts it is made of and the words it says them in.</summary>
    [Fact]
    public void TheStatusBeforeTheFirstProbeIsProbingWithNoAnswerAndNoRefusal()
    {
        Assert.Equal(new ContainerAppStatus(ContainerAppState.Probing), ContainerAppStatus.NotYet);
        Assert.Equal(ContainerAppState.Probing, ContainerAppStatus.NotYet.State);
        Assert.Null(ContainerAppStatus.NotYet.Last);
        Assert.False(ContainerAppStatus.NotYet.CorsRefused);
        Assert.Equal("ContainerAppStatus { State = Probing, Last = , CorsRefused = False }", ContainerAppStatus.NotYet.ToString());
    }

    /// <summary>Two statuses are one while all three parts are - the state, the last probe, the refusal once
    /// remembered - the equality of the operators and of <c>Equals</c> and the hash code that follows them, and a
    /// difference in any one part is another one: an answer that is equal but not the one kept included.</summary>
    [Fact]
    public void TwoStatusesAreOneWhileTheirThreePartsAre()
    {
        var one = new ContainerAppStatus(ContainerAppState.Healthy, Status(200, Opened, healthyPayload: true), false);
        var again = new ContainerAppStatus(ContainerAppState.Healthy, Status(200, Opened, healthyPayload: true), false);

        Assert.Equal(one, again);
        Assert.True(one == again);
        Assert.True(one.Equals((object)again));
        Assert.Equal(one.GetHashCode(), again.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.Equal(one, one with { });

        Assert.NotEqual(one, one with { State = ContainerAppState.Unhealthy });
        Assert.NotEqual(one, one with { Last = null });
        Assert.NotEqual(one, one with { Last = Status(503, Opened) });
        Assert.NotEqual(one, one with { CorsRefused = true });
    }

    // ----- ContainerAppProbe: the target and the state it keeps -----

    /// <summary>The probe keeps the target it probes, stands as being probed with no answer yet, and is new
    /// before its first probe.</summary>
    [Fact]
    public void TheProbeKeepsTheTargetItProbesAndStartsNew()
    {
        var probe = new ContainerAppProbe(Declared);

        Assert.Same(Declared, probe.Target);
        Assert.Equal(ContainerAppStatus.NotYet, probe.Status);
        Assert.True(probe.IsNew);
    }

    /// <summary>A start is a probe: the probe is not new any more, is not due again at the very moment it asked,
    /// and is due again at the minute it asked a minute ago - and a start records nothing, so the status stands as
    /// it stood.</summary>
    [Fact]
    public void AStartAloneStartsTheProbeAndNotAnAnswer()
    {
        var probe = new ContainerAppProbe(Declared);

        probe.Start(Opened);

        Assert.False(probe.IsNew);
        Assert.Equal(ContainerAppStatus.NotYet, probe.Status);
        Assert.False(probe.IsDue(Opened));
        Assert.True(probe.IsDue(Opened.AddSeconds(60)));
    }

    /// <summary>A record without a start is a no-answer within no run at all: the app is waking, the answer is
    /// kept, and the probe is still new, since a record does not mark a probe as started.</summary>
    [Fact]
    public void ARecordWithoutAStartIsWakingAndLeavesTheProbeNew()
    {
        var probe = new ContainerAppProbe(Declared);
        var answer = NoAnswer(Opened);

        probe.Record(answer);

        Assert.Equal(ContainerAppState.Waking, probe.Status.State);
        Assert.Same(answer, probe.Status.Last);
        Assert.False(probe.Status.CorsRefused);
        Assert.True(probe.IsNew);
        Assert.True(probe.IsDue(Opened));
    }

    // ----- ContainerAppProbe.Record: the boundaries of the states -----

    /// <summary>An answer that is not there is not recorded: the rule says so before it says anything else.</summary>
    [Fact]
    public void AnAnswerThatIsNotThereIsNotRecorded()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new ContainerAppProbe(Declared).Record(null!));

        Assert.Equal("answer", error.ParamName);
    }

    /// <summary>The grace ends where the ninetyth second is: still waking on the eighty-ninth second of the run's
    /// first no-answer, not responding on the ninetyth - the waiting is up to the grace, not past it.</summary>
    [Fact]
    public void AtTheNinetythSecondTheWakingAppIsNotResponding()
    {
        var probe = new ContainerAppProbe(Declared);

        Probe(probe, 0, NoAnswer);
        Assert.Equal(ContainerAppState.Waking, probe.Status.State);

        Probe(probe, 89, NoAnswer);
        Assert.Equal(ContainerAppState.Waking, probe.Status.State);

        Probe(probe, 90, NoAnswer);
        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
    }

    /// <summary>The app may have slept at exactly five minutes without a probe and is waking once more, but just
    /// before the five minutes it may not have, and a run that had answered says not responding at once.</summary>
    [Fact]
    public void AtExactlyFiveMinutesTheAppMayHaveSleptButJustBeforeItMayNotHave()
    {
        var justBefore = new ContainerAppProbe(Declared);
        Probe(justBefore, 0, at => Status(200, at, healthyPayload: true));
        Assert.Equal(ContainerAppState.Healthy, justBefore.Status.State);

        Probe(justBefore, 299, NoAnswer);
        Assert.Equal(ContainerAppState.NotResponding, justBefore.Status.State);

        var atFive = new ContainerAppProbe(Declared);
        Probe(atFive, 0, at => Status(200, at, healthyPayload: true));

        Probe(atFive, 300, NoAnswer);
        Assert.Equal(ContainerAppState.Waking, atFive.Status.State);
    }

    /// <summary>A two-hundred is healthy only while it is a two-hundred: the last one, with the healthy payload,
    /// is healthy, and the first three-hundred with the same payload is unhealthy, since a health check that says
    /// so with any other status is not the healthy one.</summary>
    [Theory]
    [InlineData(299, ContainerAppState.Healthy)]
    [InlineData(300, ContainerAppState.Unhealthy)]
    public void TheTwoHundredRangeOfAHealthPathEndsWhereTheThreeHundradsBegin(int status, ContainerAppState expected)
    {
        var probe = new ContainerAppProbe(Declared);

        Probe(probe, 0, at => Status(status, at, healthyPayload: true));
        Assert.Equal(expected, probe.Status.State);
    }

    /// <summary>A four-hundred that is not the 404 of the health path is still unhealthy: the app answers, and an
    /// answer that is not the healthy one is unhealthy.</summary>
    [Theory]
    [InlineData(401)]
    [InlineData(429)]
    [InlineData(418)]
    public void AFourHundredOfAnotherKindAtTheHealthPathIsUnhealthy(int status)
    {
        var probe = new ContainerAppProbe(Declared);

        Probe(probe, 0, at => Status(status, at));
        Assert.Equal(ContainerAppState.Unhealthy, probe.Status.State);
    }

    /// <summary>An answer at a path that is not the health path is only that the app answered: the page cannot
    /// tell its health from it, however healthy its payload says, however gone its 404 says, and the one the
    /// browser could not read is remembered as the refusal it is.</summary>
    [Fact]
    public void AnAnswerAtAPathThatIsNotTheHealthPathIsOnlyResponding()
    {
        var asked = new ContainerAppProbe(Declared);
        Probe(asked, 0, at => Status(200, at, healthyPayload: true, readsHealth: false, path: "/"));
        Assert.Equal(ContainerAppState.Responding, asked.Status.State);
        Assert.False(asked.Status.CorsRefused);

        var gone = new ContainerAppProbe(Declared);
        Probe(gone, 0, at => Status(404, at, readsHealth: false, path: "/"));
        Assert.Equal(ContainerAppState.Responding, gone.Status.State);

        var refused = new ContainerAppProbe(Declared);
        Probe(refused, 0, at => new ContainerAppAnswer("/", false, null, true, 85, at));
        Assert.Equal(ContainerAppState.Responding, refused.Status.State);
        Assert.True(refused.Status.CorsRefused);
    }

    // ----- ContainerAppProbe.IsHealthyPayload -----

    /// <summary>A healthy payload is a two-hundred body that says healthy, no other: the first two-hundred is the
    /// first that can say it and the first three-hundred is the last that cannot, a body that is not there or is
    /// only spaces says nothing, the word is said in any casing and with the spaces around it trimmed, the JSON
    /// says it by its <c>status</c> said as a word and not by any other property or any other kind of value, and a
    /// body that is not the JSON object it opens is not read.</summary>
    [Theory]
    [InlineData(199, "Healthy", false)]
    [InlineData(299, "Healthy", true)]
    [InlineData(300, "Healthy", false)]
    [InlineData(200, null, false)]
    [InlineData(200, "   ", false)]
    [InlineData(200, "healthy", true)]
    [InlineData(200, "HEALTHY", true)]
    [InlineData(200, " Healthy ", true)]
    [InlineData(200, "Healthy\n", true)]
    [InlineData(200, """ {"status":"healthy"} """, true)]
    [InlineData(200, """{"status":"Healthy","checks":[]}""", true)]
    [InlineData(200, """{"state":"Healthy"}""", false)]
    [InlineData(200, """{}""", false)]
    [InlineData(200, """{"status":200}""", false)]
    [InlineData(200, """{"status":null}""", false)]
    [InlineData(200, """[1,2]""", false)]
    [InlineData(200, "\"Healthy\"", false)]
    [InlineData(200, "{", false)]
    [InlineData(200, """{"status":"Healthy""", false)]
    public void OnlyATwoHundredBodyThatSaysHealthyIsAHealthyPayload(int statusCode, string? body, bool expected) =>
        Assert.Equal(expected, ContainerAppProbe.IsHealthyPayload(statusCode, body));

    // ----- ContainerAppText: what the pinned words leave open -----

    /// <summary>A status that is not there has no facts, and the rule says so by the name of the part that is not
    /// there.</summary>
    [Fact]
    public void AStatusThatIsNotThereHasNoFacts()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ContainerAppText.Facts(null!));

        Assert.Equal("status", error.ParamName);
    }

    /// <summary>A last probe that has no answer and no word for why says no answer and nothing else: the sentence
    /// ends where the detail is, with no space standing for the nothing it is.</summary>
    [Fact]
    public void ALastProbeWithoutADetailSaysNoAnswerAndNothingElse()
    {
        var title = ContainerAppText.Title(
            "ui",
            Declared,
            new ContainerAppStatus(ContainerAppState.NotResponding, new ContainerAppAnswer("/_healthcheck", true, null, false, null, Opened)),
            Utc);

        Assert.Equal(
            "ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Not responding. "
            + "This page asks it once a minute while it shows this diagram, and not while it is paused or hidden; "
            + "nothing asks it from here while no page is open, so it can scale to zero. "
            + "Last probe 22:00:00: /_healthcheck, no answer.",
            title);
    }

    /// <summary>An answer the page may not read, asked at a path that is not a health check, says both things it
    /// owes: that the app does not allow this page's origin, and that the page could not classify the answer as a
    /// health check - the two sentences that follow one another in the tooltip.</summary>
    [Fact]
    public void AnAnswerThePageMayNotReadAtAPathThatIsNotAHealthCheckSaysBothThings()
    {
        var title = ContainerAppText.Title(
            "ui",
            Declared,
            new ContainerAppStatus(ContainerAppState.Responding, new ContainerAppAnswer("/", false, null, true, 85, Opened), CorsRefused: true),
            Utc);

        Assert.Equal(
            "ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Responding. "
            + "This page asks it once a minute while it shows this diagram, and not while it is paused or hidden; "
            + "nothing asks it from here while no page is open, so it can scale to zero. "
            + "Last probe 22:00:00: / answered. The app does not allow this page's origin (CORS), so the page cannot read its answer or its health; it asks without reading the answer. "
            + "The page could not classify the answer as a health check.",
            title);
    }

    /// <summary>A name that is not there is said as nothing - the empty string and no string at all say the same
    /// - and the tooltip still says its whole: the host stands where the name was, and the wait the page gives a
    /// scaled-to-zero app is said to the second it is.</summary>
    [Fact]
    public void ANameThatIsNotThereIsSaidAsNothingAndTheTooltipStillSaysItsWhole()
    {
        var named = ContainerAppText.Title("ui", Declared, ContainerAppStatus.NotYet, Utc);
        var unnamed = ContainerAppText.Title(string.Empty, Declared, ContainerAppStatus.NotYet, Utc);
        var unheld = ContainerAppText.Title(null!, Declared, ContainerAppStatus.NotYet, Utc);

        Assert.Equal(unnamed, unheld);
        Assert.Equal(
            " (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Probing. "
            + "This page asks it once a minute while it shows this diagram, and not while it is paused or hidden; "
            + "nothing asks it from here while no page is open, so it can scale to zero. "
            + "An app that scaled to zero starts on the first request: the page waits up to 90 seconds for its first answer before it says Not responding.",
            unnamed);
        Assert.StartsWith("ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Probing.", named, StringComparison.Ordinal);
    }

    /// <summary>The tooltip says which of its parts the caller did not have, by the name of the part: the target,
    /// the status or the zone - the name the tooltip does not check is the app's, and it is said as nothing when it
    /// is not there.</summary>
    [Fact]
    public void TheTooltipSaysWhichOfItsPartsTheCallerDidNotHave()
    {
        var status = new ContainerAppStatus(ContainerAppState.Healthy, Status(200, Opened, healthyPayload: true));

        var target = Assert.Throws<ArgumentNullException>(() => ContainerAppText.Title("ui", null!, status, Utc));
        Assert.Equal("target", target.ParamName);

        var missingStatus = Assert.Throws<ArgumentNullException>(() => ContainerAppText.Title("ui", Declared, null!, Utc));
        Assert.Equal("status", missingStatus.ParamName);

        var zone = Assert.Throws<ArgumentNullException>(() => ContainerAppText.Title("ui", Declared, status, null!));
        Assert.Equal("zone", zone.ParamName);
    }
}
