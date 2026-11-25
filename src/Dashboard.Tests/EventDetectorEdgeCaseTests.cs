namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the members of <see cref="EventDetector"/>, declared in
/// <c>src/Dashboard/Health/DashboardEvents.cs</c>: the events of one endpoint between two checks
/// (<see cref="EventDetector.Node"/>), the entries of the detailed health check that changed
/// (<see cref="EventDetector.HealthChecks"/>), the restart (<see cref="EventDetector.Restarted"/>), the serving
/// decision and its changes (<see cref="EventDetector.Serving"/>, <see cref="EventDetector.ServingChanges"/>),
/// the pin in Git (<see cref="EventDetector.Pinned"/>) and the traffic button (<see cref="EventDetector.Traffic"/>).
/// <see cref="EventDetectorTests"/> pins each change the detector names - the state that turns, the entry that
/// turns, the uptime that goes down, the start that is later, the version that is another, the failover, the
/// failback and the nothing that serves, the first check that is not healthy, the first answer that names no
/// entry, the new entry and the pin that is new or is another - and <see cref="RotationTests"/> pins the
/// rotation - the node that leaves and comes back, the failover to the standby and the failback, and the one
/// event the deployable that is no rotation keeps - so what they leave open is pinned here, and only that: the
/// observation, the decision and the reading that are not there, each named by its argument; the state that
/// turns Pending again and the state change with no answer to say why; the version that is the same in other
/// letters or is gone; the uptime that goes to zero; the answer without entries and the answer whose answer
/// before had none in; the entry whose words end in a full stop or are empty, and the entry the answer finds by
/// its name in other letters; the uptime that stands still or that is gone and judged by the start, and the
/// start that is earlier or behind an uptime; the decision of a deployable with no nodes and the failover that
/// stands still; the first failover in all its words; the rotation that goes nodeless, the before that is
/// nodeless, and the standby that takes over from a standby; the reading that is not read in its other states,
/// the pin that is the same in other letters or with its build metadata, and the pin of another deployable;
/// and the words of the traffic button as other words, and none at all.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class EventDetectorEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 12, 1, 8, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Started = Now.AddHours(-1);
    private static readonly string[] Regions = ["westus3", "eastus2", "centralus"];

    /// <summary>One endpoint at one check: the state, and where the check answers, its words.</summary>
    private static NodeObservation Seen(HealthState state, string? version = "2.4.14", long? uptime = null, int? status = null, string? detail = null)
    {
        var telemetry = uptime is null
            ? null
            : new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Now)
            {
                Process = new ProcessVitals(null, null, null, null, null, null, uptime),
            };
        return new NodeObservation(state, version, telemetry, state == HealthState.Pending ? null : new ProbeResult(state, status, status is null ? null : 40, Now, version, detail));
    }

    private static IReadOnlyList<DashboardEvent> Node(NodeObservation before, NodeObservation after, bool isNode = true) =>
        EventDetector.Node(before, after, "uat", "ui", isNode ? "westus3" : "Front Door", isNode, Now);

    /// <summary>One telemetry reading: the uptime, the moment the process started, or neither.</summary>
    private static TelemetrySnapshot Reading(long? uptime = null, DateTimeOffset? startedAt = null) =>
        new(0, 0, 0, 0, null, 0, 0, 0, null, 0, Now)
        {
            Process = uptime is null ? null : new ProcessVitals(null, null, null, null, null, null, uptime),
            StartedAt = startedAt,
        };

    /// <summary>The serving decision of two nodes: the primary standing in westus3, the standby in eastus2.</summary>
    private static ServingAssessment Serving(HealthState primary, HealthState standby) =>
        ServingAssessment.Assess([NodeHealths.Primary(primary), NodeHealths.Standby(standby)], null);

    /// <summary>The rotation of the primaries in the regions of <see cref="Regions"/>, the standby of the state named standing in japaneast, behind a healthy Front Door.</summary>
    private static ServingAssessment Rotation(HealthState[] primaries, HealthState? standby = null) =>
        ServingAssessment.Assess(standby is { } state ? [.. NodeHealths.Primaries(Regions, primaries), NodeHealths.Standby(state, region: "japaneast")] : NodeHealths.Primaries(Regions, primaries), HealthState.Healthy);

    private static string[] ServingEvents(ServingAssessment? before, ServingAssessment after) =>
        ServingEventReaders.Events(before, after, "uat", Now);

    // ----- Node -----

    /// <summary>The observation that is not there is no observation: the rule says so before it says anything
    /// else, naming the argument.</summary>
    [Fact]
    public void ANObservationThatIsNotThereIsNoObservation()
    {
        var before = Seen(HealthState.Healthy);
        var after = Seen(HealthState.Healthy);

        var first = Assert.Throws<ArgumentNullException>(() => EventDetector.Node(null!, after, "uat", "ui", "westus3", true, Now));
        Assert.Equal("before", first.ParamName);

        var second = Assert.Throws<ArgumentNullException>(() => EventDetector.Node(before, null!, "uat", "ui", "westus3", true, Now));
        Assert.Equal("after", second.ParamName);
    }

    /// <summary>
    /// The check that has not answered yet is the page not deciding: the state that turns Pending again is no
    /// event, and so is the round in which it is still Pending.
    /// </summary>
    [Fact]
    public void AStateThatTurnsPendingAgainIsNoEvent()
    {
        Assert.Empty(Node(Seen(HealthState.Healthy, uptime: 100), Seen(HealthState.Pending)));
        Assert.Empty(Node(Seen(HealthState.Pending), Seen(HealthState.Pending)));
    }

    /// <summary>
    /// The state change the endpoint answers with no status and no words of its own is said with the states
    /// alone: the fact that is not there is not said as what it is.
    /// </summary>
    [Fact]
    public void AStateChangeWithNoAnswerIsSaidWithoutTheAnswer()
    {
        var event_ = Assert.Single(Node(Seen(HealthState.Healthy), Seen(HealthState.Unreachable)));

        Assert.Equal(new DashboardEvent(Now, EventKind.Health, EventLevel.Problem, "uat", "westus3", "ui: Healthy → Unreachable"), event_);
    }

    /// <summary>
    /// The version the page compares is the version as the app says it: the same version in other letters is no
    /// deployment, and the version that is gone is no event either.
    /// </summary>
    [Fact]
    public void TheSameVersionInOtherLettersAndAVersionThatIsGoneAreNoEvent()
    {
        Assert.Empty(Node(Seen(HealthState.Healthy, "1.0.0-rc1"), Seen(HealthState.Healthy, "1.0.0-RC1")));
        Assert.Empty(Node(Seen(HealthState.Healthy, "2.4.14"), Seen(HealthState.Healthy, null)));
    }

    /// <summary>
    /// The uptime at its boundary: the process that reports zero seconds of uptime has restarted, and the round
    /// says so with the span it has, which is the span of nothing.
    /// </summary>
    [Fact]
    public void AnUptimeThatGoesToZeroIsARestartOfAProcessThatJustStarted()
    {
        var restart = Assert.Single(Node(Seen(HealthState.Healthy, uptime: 100), Seen(HealthState.Healthy, uptime: 0)));

        Assert.Equal(new DashboardEvent(Now, EventKind.Restart, EventLevel.Warning, "uat", "westus3", "ui restarted, up 0 s"), restart);
    }

    // ----- HealthChecks -----

    /// <summary>
    /// The answer that has no entries says nothing, beside the answer that is not there the tests above pin:
    /// the empty answer the page read, with or without an answer before that had entries.
    /// </summary>
    [Fact]
    public void AnAnswerWithoutEntriesSaysNothing()
    {
        var empty = new HealthDetail([], Now);

        Assert.Empty(EventDetector.HealthChecks(null, empty, "uat", "ui", "westus3", Now));
        Assert.Empty(EventDetector.HealthChecks(new HealthDetail([new HealthCheckEntry("Queue", CheckState.Unhealthy)], Now), empty, "uat", "ui", "westus3", Now));
    }

    /// <summary>
    /// The answer before the first that had no entries at all is the first answer: the entry in it is new in the
    /// health check, the healthy one says nothing, and the unhealthy one says so with its own words.
    /// </summary>
    [Fact]
    public void AnAnswerWhoseAnswerBeforeHadNoneInSaysNew()
    {
        var before = new HealthDetail([], Now);
        var after = new HealthDetail([new HealthCheckEntry("API", CheckState.Healthy), new HealthCheckEntry("Mail", CheckState.Unhealthy, "Mail says unhealthy")], Now);

        var added = Assert.Single(EventDetector.HealthChecks(before, after, "uat", "ui", "westus3", Now));
        Assert.Equal(new DashboardEvent(Now, EventKind.HealthCheck, EventLevel.Problem, "uat", "westus3", "ui: Mail Unhealthy, new in the health check: Mail says unhealthy"), added);
    }

    /// <summary>
    /// The entry's own words are said as they are said: the description that ends in a full stop is said without
    /// it, the end of a sentence the event makes of its own.
    /// </summary>
    [Fact]
    public void WordsThatEndInAFullStopAreSaidWithoutIt()
    {
        var before = HealthDetailTests.Detail(("LlmGateway", "Healthy"));
        var after = HealthDetail.Parse("""{ "entries": [ { "name": "LlmGateway", "status": "Degraded", "description": "Chat client answered slowly: 4.2 s (threshold 2 s)." } ] }""", Now)!;

        var slower = Assert.Single(EventDetector.HealthChecks(before, after, "uat", "ui", "westus3", Now));
        Assert.Equal(new DashboardEvent(Now, EventKind.HealthCheck, EventLevel.Warning, "uat", "westus3", "ui: LlmGateway Healthy → Degraded: Chat client answered slowly: 4.2 s (threshold 2 s)"), slower);
    }

    /// <summary>
    /// The entry the answer finds is found by its name, whatever the letters it stands in: the state is compared
    /// to the state the answer before stood on, and the event is said with the name the answer after stands on.
    /// </summary>
    [Fact]
    public void AnEntryTheAnswerFindsByItsNameInOtherLettersIsComparedToThatName()
    {
        var changed = Assert.Single(EventDetector.HealthChecks(HealthDetailTests.Detail(("Api", "Healthy")), HealthDetailTests.Detail(("api", "Unhealthy")), "uat", "ui", "westus3", Now));

        Assert.Equal((EventLevel.Problem, "ui: api Healthy → Unhealthy: api says unhealthy"), (changed.Level, changed.Text));
    }

    /// <summary>
    /// The entry whose description is the empty word is said without words: the mark the state stands on stays,
    /// and the words the entry gives are the blank it gives.
    /// </summary>
    [Fact]
    public void AnEmptyDescriptionIsSaidAsTheBlankItIs()
    {
        var before = new HealthDetail([new HealthCheckEntry("Queue", CheckState.Healthy)], Now);
        var after = new HealthDetail([new HealthCheckEntry("Queue", CheckState.Unknown, string.Empty)], Now);

        var event_ = Assert.Single(EventDetector.HealthChecks(before, after, "uat", "ui", "westus3", Now));
        Assert.Equal((EventLevel.Info, "ui: Queue Healthy → Not known: "), (event_.Level, event_.Text));
    }

    // ----- Restarted -----

    /// <summary>Neither reading is a restart: the app that reports no telemetry at all, on either side, says
    /// nothing beside the side that is not there the tests above pin.</summary>
    [Fact]
    public void NeitherReadingSaysRestart()
    {
        Assert.False(EventDetector.Restarted(null, null));
    }

    /// <summary>
    /// The uptime at its boundaries: the uptime that stands still is no restart, the one that goes up by nothing
    /// is no restart, and the one that goes down by one second is the restart.
    /// </summary>
    [Fact]
    public void AnUptimeThatStandsStillOrGoesUpIsNoRestartAndOneSecondDownIs()
    {
        Assert.False(EventDetector.Restarted(Reading(uptime: 100), Reading(uptime: 100)));
        Assert.False(EventDetector.Restarted(Reading(uptime: 99), Reading(uptime: 100)));
        Assert.True(EventDetector.Restarted(Reading(uptime: 100), Reading(uptime: 99)));
    }

    /// <summary>
    /// The uptime that is gone is judged by the start: the process that stops reporting its uptime but stands on
    /// the same start is no restart, and the one whose start is later is.
    /// </summary>
    [Fact]
    public void AnUptimeThatIsGoneIsJudgedByTheStart()
    {
        var before = Reading(uptime: 9000, startedAt: Started);

        Assert.False(EventDetector.Restarted(before, Reading(startedAt: Started)));
        Assert.True(EventDetector.Restarted(before, Reading(startedAt: Started.AddHours(1))));
    }

    /// <summary>
    /// The start that is earlier than the one before is no restart: the process did not start, it told a wrong
    /// time. And the uptime that both readings stand on is the judge: it goes up and the start is later, and it
    /// is no restart, because the uptime says so.
    /// </summary>
    [Fact]
    public void AStartThatIsEarlierIsNoRestartAndTheUptimePrecedesTheStart()
    {
        Assert.False(EventDetector.Restarted(Reading(startedAt: Started), Reading(startedAt: Started.AddHours(-1))));
        Assert.False(EventDetector.Restarted(Reading(uptime: 100, startedAt: Started), Reading(uptime: 200, startedAt: Started.AddHours(1))));
    }

    // ----- Serving -----

    /// <summary>The decision that is not there is no decision: the rule says so before it says anything else,
    /// naming the argument.</summary>
    [Fact]
    public void ADecisionThatIsNotThereIsNoDecision()
    {
        var error = Assert.Throws<ArgumentNullException>(() => EventDetector.Serving(null, null!, "uat", "ui", Now));

        Assert.Equal("after", error.ParamName);
    }

    /// <summary>
    /// The deployable that has no nodes is not deciding yet, whatever it decided before: the round from a
    /// decision to no nodes is no event.
    /// </summary>
    [Fact]
    public void ANodelessDeployableIsNoChange()
    {
        var noNodes = ServingAssessment.Assess([], null);

        Assert.Equal(ServingState.NoNodes, noNodes.State);
        Assert.Null(EventDetector.Serving(Serving(HealthState.Healthy, HealthState.Healthy), noNodes, "uat", "ui", Now));
    }

    /// <summary>
    /// The failover that stands still is no event: the node expected to serve is the same on both readings, in
    /// the state the tests above leave open beside the primary that stands still and the nothing that serves.
    /// </summary>
    [Fact]
    public void TheSameFailoverIsNoEvent()
    {
        var failedOver = Serving(HealthState.Unreachable, HealthState.Healthy);

        Assert.Null(EventDetector.Serving(failedOver, failedOver, "uat", "ui", Now));
    }

    /// <summary>
    /// The first failover, in all its words: the moment, the kind, the level and the place it stands on, beside
    /// the level the tests above pin.
    /// </summary>
    [Fact]
    public void TheFirstFailoverIsSaidWithItsFullWords()
    {
        Assert.Equal(
            new DashboardEvent(Now, EventKind.Serving, EventLevel.Warning, "uat", "ui", "Failed over to eastus2. Primary westus3 is unreachable; eastus2 is expected to serve traffic."),
            EventDetector.Serving(null, Serving(HealthState.Unreachable, HealthState.Healthy), "uat", "ui", Now));
    }

    // ----- ServingChanges -----

    /// <summary>The decision that is not there is no list: the rule says so before it says anything else, naming
    /// the argument.</summary>
    [Fact]
    public void ADecisionThatIsNotThereIsNoList()
    {
        var error = Assert.Throws<ArgumentNullException>(() => EventDetector.ServingChanges(null, null!, "uat", "ui", Now));

        Assert.Equal("after", error.ParamName);
    }

    /// <summary>
    /// The rotation that goes nodeless says nothing: the deployable whose topology has no nodes is not deciding,
    /// and the round from a decided rotation to it is no event.
    /// </summary>
    [Fact]
    public void ARotationThatGoesNodelessSaysNothing()
    {
        Assert.Empty(ServingEvents(Rotation([HealthState.Healthy, HealthState.Healthy]), ServingAssessment.Assess([], HealthState.Healthy)));
    }

    /// <summary>
    /// The before that is nodeless is not known, as the before that is not there: the first decision of the
    /// rotation is an event only when no primary serves, whatever the reading before stood on.
    /// </summary>
    [Fact]
    public void ANodelessBeforeIsNotKnown()
    {
        var noNodes = ServingAssessment.Assess([], HealthState.Healthy);

        Assert.Empty(ServingEvents(noNodes, Rotation([HealthState.Healthy, HealthState.Healthy])));
        Assert.Equal(
            ["Problem: No healthy node: nothing can serve traffic."],
            ServingEvents(noNodes, Rotation([HealthState.Unreachable, HealthState.Unreachable], standby: HealthState.Unreachable)));
    }

    /// <summary>
    /// The standby that takes over from a standby is a failover: no primary is healthy on either side, the
    /// rotation is the same, and the node expected to serve is another - the move the one word of
    /// <see cref="EventDetector.Serving"/> says.
    /// </summary>
    [Fact]
    public void AStandbyThatTakesOverAnotherStandbyIsAFailover()
    {
        var japanDown = ServingAssessment.Assess([NodeHealths.Primary(HealthState.Unreachable, "westus3"), NodeHealths.Primary(HealthState.Unreachable, "eastus2"), NodeHealths.Standby(HealthState.Healthy, "japaneast")], HealthState.Healthy);
        var koreaDown = ServingAssessment.Assess([NodeHealths.Primary(HealthState.Unreachable, "westus3"), NodeHealths.Primary(HealthState.Unreachable, "eastus2"), NodeHealths.Standby(HealthState.Unhealthy, "japaneast"), NodeHealths.Standby(HealthState.Healthy, "koreacentral")], HealthState.Healthy);

        Assert.Equal(
            ["Warning: Failover: japaneast → koreacentral. No primary node is healthy; koreacentral is expected to serve traffic."],
            ServingEvents(japanDown, koreaDown));
    }

    // ----- Pinned -----

    /// <summary>The reading that is not there is no reading: the rule says so before it says anything else,
    /// naming the argument.</summary>
    [Fact]
    public void AReadingThatIsNotThereIsNoReading()
    {
        var any = PinnedVersions.Parse("""{ "ui": "2.4.14" }""");

        var first = Assert.Throws<ArgumentNullException>(() => EventDetector.Pinned(null!, any, "uat", "ui", Now));
        Assert.Equal("before", first.ParamName);

        var second = Assert.Throws<ArgumentNullException>(() => EventDetector.Pinned(any, null!, "uat", "ui", Now));
        Assert.Equal("after", second.ParamName);
    }

    /// <summary>
    /// The file that is not read is no pin, in the states the tests above leave open beside the one not read yet
    /// and the one that could not be read: the environment that tracks no file, on the side before, and the file
    /// that is not there and the one not read yet, on the side after.
    /// </summary>
    [Fact]
    public void AFileThatIsNotReadIsNoPin()
    {
        var read = PinnedVersions.Parse("""{ "ui": "2.4.14" }""");

        Assert.Null(EventDetector.Pinned(PinnedVersions.NotTracked, read, "uat", "ui", Now));
        Assert.Null(EventDetector.Pinned(PinnedVersions.Unavailable("offline"), read, "uat", "ui", Now));
        Assert.Null(EventDetector.Pinned(read, PinnedVersions.Missing, "uat", "ui", Now));
        Assert.Null(EventDetector.Pinned(read, PinnedVersions.Pending, "uat", "ui", Now));
    }

    /// <summary>
    /// The pin the page compares is the version as Git says it, without build metadata: the same version in
    /// other letters is no event, and the one with its build metadata is the version it stands on.
    /// </summary>
    [Fact]
    public void TheSamePinInOtherLettersOrWithItsBuildMetadataIsNoEvent()
    {
        var lower = PinnedVersions.Parse("""{ "ui": "1.0.0-rc1" }""");
        var upper = PinnedVersions.Parse("""{ "ui": "1.0.0-RC1" }""");

        Assert.Equal("1.0.0-rc1", lower.Of("ui"));
        Assert.Equal("1.0.0-RC1", upper.Of("ui"));
        Assert.Null(EventDetector.Pinned(lower, upper, "uat", "ui", Now));

        var plain = PinnedVersions.Parse("""{ "ui": "2.4.14" }""");
        var built = PinnedVersions.Parse("""{ "ui": "2.4.14+build.300" }""");

        Assert.Equal("2.4.14", built.Of("ui"));
        Assert.Null(EventDetector.Pinned(plain, built, "uat", "ui", Now));
    }

    /// <summary>
    /// The pin of another deployable is no event for this one: the file is read on both sides, and this
    /// deployable stands on the version it stood on before. The other deployable, in the same round, says its
    /// change.
    /// </summary>
    [Fact]
    public void APinOfAnotherDeployableIsNoEventForThisOne()
    {
        var before = PinnedVersions.Parse("""{ "ui": "2.4.14", "api": "1.0.0" }""");
        var after = PinnedVersions.Parse("""{ "ui": "2.4.14", "api": "1.0.1" }""");

        Assert.Null(EventDetector.Pinned(before, after, "uat", "ui", Now));
        Assert.Equal("api: pinned 1.0.0 → 1.0.1 in Git", EventDetector.Pinned(before, after, "uat", "api", Now)!.Text);
    }

    // ----- Traffic -----

    /// <summary>
    /// The words of the traffic button are kept as they are: the event stands on the environment it is given,
    /// with no node of its own, and the words it is given, with the moment it is given - the words that are
    /// given, and none at all.
    /// </summary>
    [Fact]
    public void TheWordsOfTheTrafficButtonAreKeptAsTheyAre()
    {
        Assert.Equal(new DashboardEvent(Now, EventKind.Traffic, EventLevel.Info, "production", null, "Traffic stopped"), EventDetector.Traffic("production", "Traffic stopped", Now));
        Assert.Equal(new DashboardEvent(Now, EventKind.Traffic, EventLevel.Info, "uat", null, string.Empty), EventDetector.Traffic("uat", string.Empty, Now));
    }
}
