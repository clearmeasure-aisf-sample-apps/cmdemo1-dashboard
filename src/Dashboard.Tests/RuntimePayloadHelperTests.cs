namespace Dashboard.Tests;

/// <summary>
/// The small public helpers the runtime payload is built from: which node of a diagram this page probes and at what
/// path, the word a mark of a detailed health check carries, where a version leads, and the number line a container
/// app's badge carries. Read straight, without a manifest or a monitor around them.
/// </summary>
public class RuntimePayloadHelperTests
{
    private static readonly DateTimeOffset Checked = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private const string AppUrl = "https://ca-demo-uat-scorecard.graybush-1.southcentralus.azurecontainerapps.io";
    private static readonly Uri App = new(AppUrl);
    private static readonly Uri Project = new("https://octopus.example.net/app#/Spaces-1/projects/demo-ui");

    private const string Release = "https://octopus.example.net/app#/Spaces-1/projects/demo-ui/deployments/releases/2.4.15";
    private const string Commit = "0a1b2c3d4e5f60718293a4b5c6d7e8f901234567";
    private const string CommitPage = "https://github.example.net/o/r/commit/0a1b2c3d4e5f60718293a4b5c6d7e8f901234567";

    // ---------------------------------------------------------------- ContainerTarget

    /// <summary>A node of the diagram, as the manifest gives one: an address, a deployable and a health path.</summary>
    private static RuntimeNode Node(
        RuntimeNodeKind kind = RuntimeNodeKind.ContainerApp,
        string? url = AppUrl,
        string? deployable = "scorecard",
        string? healthPath = "/health") =>
        new("cax_1", kind, "ca-demo-uat-scorecard", url is null ? null : new Uri(url), deployable, HealthPath: healthPath);

    [Fact]
    public void AContainerAppIsProbedAtTheHealthPathItsDeployableNames()
    {
        var target = RuntimePayloadBuilder.ContainerTarget(Node());

        Assert.Equal(new ContainerAppTarget(App, "/health"), target);
        Assert.Equal("/health", target!.PathFor(ProbeKind.Health));
        Assert.Equal("/health", target.PathFor(ProbeKind.Liveness));
    }

    [Fact]
    public void AContainerAppWhoseDeployableNamesNoHealthPathIsAskedAtTheDefaultOne()
    {
        var target = RuntimePayloadBuilder.ContainerTarget(Node(healthPath: null));

        Assert.Equal(new ContainerAppTarget(App), target);
        Assert.Null(target!.HealthPath);
        Assert.Equal("/_healthcheck", target.EffectiveHealthPath);
        Assert.True(target.ReadsHealth(ProbeKind.Health));
    }

    [Fact]
    public void TheHealthPathOfANodeNoDeployableDeclaresIsNotAsked()
    {
        // Only what system.json declares for the deployable gives a health path; a node drawn on its own keeps the default.
        var target = RuntimePayloadBuilder.ContainerTarget(Node(deployable: null));

        Assert.Equal(new ContainerAppTarget(App), target);
        Assert.Null(target!.HealthPath);
        Assert.Equal("/_healthcheck", target.EffectiveHealthPath);
    }

    [Theory]
    [InlineData(RuntimeNodeKind.Other)]
    [InlineData(RuntimeNodeKind.Person)]
    [InlineData(RuntimeNodeKind.FrontDoor)]
    [InlineData(RuntimeNodeKind.WebApp)]
    [InlineData(RuntimeNodeKind.Sql)]
    [InlineData(RuntimeNodeKind.StaticSite)]
    [InlineData(RuntimeNodeKind.Dependency)]
    [InlineData(RuntimeNodeKind.Resource)]
    public void ANodeOfAnotherKindIsNoProbeTarget(RuntimeNodeKind kind) =>
        Assert.Null(RuntimePayloadBuilder.ContainerTarget(Node(kind)));

    [Fact]
    public void AContainerAppIsTheOnlyKindThePageProbes() =>
        Assert.Equal(
            [RuntimeNodeKind.ContainerApp],
            Enum.GetValues<RuntimeNodeKind>().Where(kind => RuntimePayloadBuilder.ContainerTarget(Node(kind)) is not null));

    [Fact]
    public void AContainerAppWithoutAnAddressIsNoProbeTarget() =>
        Assert.Null(RuntimePayloadBuilder.ContainerTarget(Node(url: null)));

    [Fact]
    public void WithoutANodeThereIsNoTarget() =>
        Assert.Throws<ArgumentNullException>(() => RuntimePayloadBuilder.ContainerTarget(null!));

    // ---------------------------------------------------------------- MarkOf

    [Theory]
    [InlineData(CheckState.Healthy, "healthy")]
    [InlineData(CheckState.Degraded, "degraded")]
    [InlineData(CheckState.Unhealthy, "failed")]
    [InlineData(CheckState.Unknown, "unknown")]
    public void AMarkCarriesTheWordOfItsState(CheckState state, string word) =>
        Assert.Equal(word, RuntimePayloadBuilder.MarkOf(state));

    [Fact]
    public void EveryStateOfACheckHasAWordOfItsOwn() =>
        Assert.Equal(
            ["degraded", "failed", "healthy", "unknown"],
            Enum.GetValues<CheckState>().Select(RuntimePayloadBuilder.MarkOf).Order(StringComparer.Ordinal));

    /// <summary>
    /// Each current state of a container app's probe, pinned to the word <see cref="RuntimePayloadBuilder.StateOf"/>
    /// gives it: Healthy and Responding give healthy, Unhealthy gives unhealthy, NotResponding gives unreachable, and
    /// the other states give checking.
    /// </summary>
    [Theory]
    [InlineData(ContainerAppState.Probing, "checking")]
    [InlineData(ContainerAppState.Waking, "checking")]
    [InlineData(ContainerAppState.Healthy, "healthy")]
    [InlineData(ContainerAppState.Responding, "healthy")]
    [InlineData(ContainerAppState.Unhealthy, "unhealthy")]
    [InlineData(ContainerAppState.NotResponding, "unreachable")]
    public void TheWordOfEachStateOfAContainerAppIsPinned(ContainerAppState state, string word) =>
        Assert.Equal(word, RuntimePayloadBuilder.StateOf(state));

    /// <summary>
    /// Every state of a container app's probe yields a word that is not empty, and a value outside the enum is no
    /// state of it: the builder throws, naming the value it was given.
    /// </summary>
    [Fact]
    public void EveryStateOfAContainerAppYieldsAWordAndAValueOutsideTheEnumThrows()
    {
        Assert.All(
            Enum.GetValues<ContainerAppState>(),
            state => Assert.False(string.IsNullOrWhiteSpace(RuntimePayloadBuilder.StateOf(state))));

        Assert.Throws<ArgumentOutOfRangeException>(() => RuntimePayloadBuilder.StateOf((ContainerAppState)999));
    }

    [Fact]
    public void AFailingCheckIsMarkedFailedAndNotWithTheWordOfATile()
    {
        // A tile that is down says unhealthy or unreachable; a mark of a check that failed says failed.
        Assert.Equal("failed", RuntimePayloadBuilder.MarkOf(CheckState.Unhealthy));
        Assert.NotEqual(RuntimePayloadBuilder.Unhealthy, RuntimePayloadBuilder.MarkOf(CheckState.Unhealthy));
        Assert.NotEqual(RuntimePayloadBuilder.Unreachable, RuntimePayloadBuilder.MarkOf(CheckState.Unhealthy));
        Assert.NotEqual(RuntimePayloadBuilder.Checking, RuntimePayloadBuilder.MarkOf(CheckState.Unknown));
    }

    [Fact]
    public void AStatusTheDashboardDoesNotKnowIsMarkedUnknown()
    {
        var entry = Assert.Single(HealthDetailTests.Detail(("Paused", "Paused")).Entries);

        Assert.Equal(CheckState.Unknown, entry.State);
        Assert.Equal("unknown", RuntimePayloadBuilder.MarkOf(entry.State));
    }

    // ---------------------------------------------------------------- StateOf

    /// <summary>
    /// The word each state of a container app's probe carries, one row per state, walked over the enum so a new
    /// state is pinned the day it is added; the words are the tiles' own, so a state the payload does not know of
    /// fails the build.
    /// </summary>
    [Theory]
    [MemberData(nameof(StatesOfContainerApps))]
    public void EveryStateOfAContainerAppHasAWordOfItsOwn(ContainerAppState state, string word) =>
        Assert.Equal(word, RuntimePayloadBuilder.StateOf(state));

    public static IEnumerable<object[]> StatesOfContainerApps()
    {
        foreach (var state in Enum.GetValues<ContainerAppState>())
        {
            yield return state switch
            {
                ContainerAppState.Probing => [state, "checking"],
                ContainerAppState.Waking => [state, "checking"],
                ContainerAppState.Healthy => [state, "healthy"],
                ContainerAppState.Responding => [state, "healthy"],
                ContainerAppState.Unhealthy => [state, "unhealthy"],
                ContainerAppState.NotResponding => [state, "unreachable"],
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
            };
        }
    }

    /// <summary>
    /// A state the probe cannot record is no state of the enum: the word of it is not a fall-through but an error
    /// naming the value. <see cref="ContainerAppProbe.Record"/> produces only the enum's own states, so in
    /// production no undefined state reaches the word.
    /// </summary>
    [Fact]
    public void AnUndefinedStateThrowsWithTheValueItWasGiven()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => RuntimePayloadBuilder.StateOf((ContainerAppState)7));

        Assert.Equal((ContainerAppState)7, ex.ActualValue);
    }

    // ---------------------------------------------------------------- StateOf (HealthState)

    /// <summary>
    /// Each current state of a health check, pinned to the word <see cref="RuntimePayloadBuilder.StateOf"/>
    /// gives it: Healthy gives healthy, Unhealthy gives unhealthy, Unreachable gives unreachable, and Pending
    /// gives checking.
    /// </summary>
    [Theory]
    [InlineData(HealthState.Pending, "checking")]
    [InlineData(HealthState.Healthy, "healthy")]
    [InlineData(HealthState.Unhealthy, "unhealthy")]
    [InlineData(HealthState.Unreachable, "unreachable")]
    public void TheWordOfEachHealthStateIsPinned(HealthState state, string word) =>
        Assert.Equal(word, RuntimePayloadBuilder.StateOf(state));

    /// <summary>
    /// Every state of a health check yields a word that is not empty, and a value outside the enum is no
    /// state of it: the builder throws, naming the value it was given.
    /// </summary>
    [Fact]
    public void EveryHealthStateYieldsAWordAndAValueOutsideTheEnumThrows()
    {
        Assert.All(
            Enum.GetValues<HealthState>(),
            state => Assert.False(string.IsNullOrWhiteSpace(RuntimePayloadBuilder.StateOf(state))));

        Assert.Throws<ArgumentOutOfRangeException>(() => RuntimePayloadBuilder.StateOf((HealthState)999));
    }

    /// <summary>
    /// A state the health check cannot record is no state of the enum: the word of it is not a fall-through but an error
    /// naming the value. <see cref="HealthCheck.Record"/> produces only the enum's own states, so in
    /// production no undefined state reaches the word.
    /// </summary>
    [Fact]
    public void AnUndefinedHealthStateThrowsWithTheValueItWasGiven()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => RuntimePayloadBuilder.StateOf((HealthState)7));

        Assert.Equal((HealthState)7, ex.ActualValue);
    }

    // ---------------------------------------------------------------- VersionLink

    /// <summary>A deployable of the topology, with the project's page in Octopus Deploy and the build its node reports.</summary>
    private static DeployableStatus Deployable(Uri? project = null, BuildInfo? build = null)
    {
        var deployable = new DeployableStatus(new DeployableInfo("ui", null, "/_healthcheck", "/alive", "/_version", [], ProjectUrl: project));
        deployable.RecordBuild(build, build?.Version);
        return deployable;
    }

    private static BuildInfo Built(string version = "2.4.15", string commit = Commit, string? commitUrl = CommitPage)
    {
        var address = commitUrl is null ? string.Empty : $", \"commitUrl\": \"{commitUrl}\"";
        return BuildInfo.Parse($$"""{ "version": "{{version}}", "commit": "{{commit}}"{{address}} }""")!;
    }

    [Fact]
    public void AVersionLeadsToItsReleaseInTheProjectOfTheTopology() =>
        Assert.Equal(
            new RuntimeLink(Release, "Release 2.4.15 of ui in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)"),
            RuntimePayloadBuilder.VersionLink(Deployable(Project), "2.4.15"));

    [Theory]
    [InlineData("2.4.15", "https://octopus.example.net/app#/Spaces-1/projects/demo-ui/deployments/releases/2.4.15")]
    [InlineData("1.0.0-preview.7", "https://octopus.example.net/app#/Spaces-1/projects/demo-ui/deployments/releases/1.0.0-preview.7")]
    [InlineData("2.4.15+7", "https://octopus.example.net/app#/Spaces-1/projects/demo-ui/deployments/releases/2.4.15%2B7")]
    [InlineData("2.4.15 7", "https://octopus.example.net/app#/Spaces-1/projects/demo-ui/deployments/releases/2.4.15%207")]
    public void AVersionIsWrittenIntoTheReleaseAddressAsOctopusNamesIt(string version, string href) =>
        Assert.Equal(href, RuntimePayloadBuilder.VersionLink(Deployable(Project), version)!.Href);

    [Fact]
    public void AProjectAddressThatEndsInASlashStillLeadsToTheRelease() =>
        Assert.Equal(
            Release,
            RuntimePayloadBuilder.VersionLink(Deployable(new Uri("https://octopus.example.net/app#/Spaces-1/projects/demo-ui/")), "2.4.15")!.Href);

    [Fact]
    public void TheReleaseIsFoundEvenWhenTheBuildNamesAnotherCommit()
    {
        // The project's page wins: what a node runs is a release of it, whatever build the app still reports.
        var link = RuntimePayloadBuilder.VersionLink(Deployable(Project, Built("2.4.9")), "2.4.15");

        Assert.Equal(Release, link!.Href);
    }

    [Fact]
    public void WithoutAProjectTheVersionLeadsToTheCommitItsBuildNames() =>
        Assert.Equal(
            new RuntimeLink(CommitPage, "Commit 0a1b2c3 on GitHub (opens in a new tab)"),
            RuntimePayloadBuilder.VersionLink(Deployable(build: Built()), "2.4.15"));

    [Fact]
    public void TheCommitIsFoundWhicheverWayTheVersionIsCased() =>
        Assert.Equal(
            CommitPage,
            RuntimePayloadBuilder.VersionLink(Deployable(build: Built("1.0.0-RC_7")), "1.0.0-rc_7")!.Href);

    [Theory]
    [InlineData(Commit, "Commit 0a1b2c3 on GitHub (opens in a new tab)")]
    [InlineData("0a1b2c3", "Commit 0a1b2c3 on GitHub (opens in a new tab)")]
    [InlineData("0a1b2c", "Commit 0a1b2c on GitHub (opens in a new tab)")]
    public void ACommitOfMoreThanSevenCharactersIsWrittenShortAndAShorterOneStandsAsItIs(string commit, string title) =>
        Assert.Equal(title, RuntimePayloadBuilder.VersionLink(Deployable(build: Built(commit: commit)), "2.4.15")!.Title);

    [Fact]
    public void ABuildOfAnotherVersionIsNoCommitLink() =>
        Assert.Null(RuntimePayloadBuilder.VersionLink(Deployable(build: Built("2.4.14")), "2.4.15"));

    [Fact]
    public void ABuildThatNamesNoAddressForItsCommitIsNoLink() =>
        Assert.Null(RuntimePayloadBuilder.VersionLink(Deployable(build: Built(commitUrl: null)), "2.4.15"));

    [Fact]
    public void ABuildThatNamesNoCommitIsNoLink() =>
        Assert.Null(RuntimePayloadBuilder.VersionLink(Deployable(build: BuildInfo.Parse("""{ "version": "2.4.15", "commitUrl": "https://github.example.net/o/r/commit/x" }""")!), "2.4.15"));

    [Fact]
    public void WithoutAProjectAndWithoutABuildTheVersionLeadsNowhere()
    {
        // No project, and the build endpoint was never read: the version stays plain text.
        Assert.Null(RuntimePayloadBuilder.VersionLink(Deployable(build: null), "2.4.15"));
        // A build that reports no version is nobody's build: the line names no commit for it.
        Assert.Null(RuntimePayloadBuilder.VersionLink(Deployable(build: BuildInfo.Parse($$"""{ "commit": "{{Commit}}", "commitUrl": "{{CommitPage}}" }""")!), "2.4.15"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AVersionOfNoTextLeadsNowhere(string version)
    {
        // LinkText.Release names no release without a version, and no build was made of no version.
        Assert.Null(RuntimePayloadBuilder.VersionLink(Deployable(Project, Built()), version));
    }

    [Fact]
    public void WithoutADeployableThereIsNoVersionLink() =>
        Assert.Throws<ArgumentNullException>(() => RuntimePayloadBuilder.VersionLink(null!, "2.4.15"));

    // ---------------------------------------------------------------- ContainerAppText.Facts

    private static ContainerAppStatus Seen(
        ContainerAppState state,
        int? status = null,
        int? ms = null,
        bool opaque = false) =>
        new(
            state,
            new ContainerAppAnswer("/_healthcheck", true, status, opaque, ms, Checked, status is null && !opaque ? "No answer within 10 s." : null));

    /// <summary>The number line as a viewer of another culture would see it: the code asks the invariant one, so the digits and the separators never change.</summary>
    private static string Facts(ContainerAppStatus status) =>
        CultureScope.Says("de-DE", () => ContainerAppText.Facts(status));

    // The last rows are the fall-through: an answer with nothing to show, and the state the probe holds with it.
    [Theory]
    [InlineData(200, 85, false, ContainerAppState.Healthy, "HTTP 200 · 85 ms")]
    [InlineData(503, 85, false, ContainerAppState.Unhealthy, "HTTP 503 · 85 ms")]
    [InlineData(200, 1234, false, ContainerAppState.Healthy, "HTTP 200 · 1234 ms")]
    [InlineData(503, null, false, ContainerAppState.Unhealthy, "HTTP 503")]
    [InlineData(404, 85, false, ContainerAppState.NotResponding, "HTTP 404 · 85 ms")]
    [InlineData(200, 85, true, ContainerAppState.Responding, "HTTP 200 · 85 ms")]
    [InlineData(null, 85, true, ContainerAppState.Responding, "answered · 85 ms")]
    [InlineData(null, 1234, true, ContainerAppState.Responding, "answered · 1234 ms")]
    [InlineData(null, null, true, ContainerAppState.Responding, "answered")]
    [InlineData(null, 85, false, ContainerAppState.Waking, "no answer yet")]
    [InlineData(null, null, false, ContainerAppState.Waking, "no answer yet")]
    [InlineData(null, 85, false, ContainerAppState.NotResponding, "no answer")]
    [InlineData(null, null, false, ContainerAppState.NotResponding, "no answer")]
    [InlineData(null, null, false, ContainerAppState.Probing, "no answer")]
    [InlineData(null, null, false, ContainerAppState.Healthy, "no answer")]
    public void TheFactsNextToABadgeSayWhatTheLastProbeSaw(int? status, int? ms, bool opaque, ContainerAppState state, string facts) =>
        Assert.Equal(facts, Facts(Seen(state, status, ms, opaque)));

    [Fact]
    public void BeforeTheFirstProbeTheFactsSayNothingWasAsked()
    {
        Assert.Equal("not probed yet", Facts(ContainerAppStatus.NotYet));
        Assert.Equal("not probed yet", Facts(new ContainerAppStatus(ContainerAppState.Healthy)));
    }

    [Fact]
    public void AWakingAppSaysItHasNotAnsweredYetAndAHealthyOneItsStatus()
    {
        var probe = new ContainerAppProbe(new ContainerAppTarget(App, "/health"));

        probe.Start(Checked);
        probe.Record(new ContainerAppAnswer("/health", true, null, false, null, Checked, "No answer within 10 s."));
        Assert.Equal("no answer yet", Facts(probe.Status));

        probe.Start(Checked.AddSeconds(60));
        probe.Record(new ContainerAppAnswer("/health", true, 200, false, 85, Checked.AddSeconds(60), HealthyPayload: true));
        Assert.Equal(ContainerAppState.Healthy, probe.Status.State);
        Assert.Equal("HTTP 200 · 85 ms", Facts(probe.Status));
    }

    [Fact]
    public void WithoutAStatusThereAreNoFacts() =>
        Assert.Throws<ArgumentNullException>(() => ContainerAppText.Facts(null!));
}
