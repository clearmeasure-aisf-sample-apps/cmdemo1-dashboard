using System.Net;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// <c>DeployableFacts.razor</c> is the row of two cards under one deployable in one environment: the code it runs, as
/// its own node reports it, and how it got there, as the system's delivery file says. A card is there only when its
/// source answered; with neither answered there is no row at all, and the two cards are told the same context, the
/// same element id prefix, the same time zone and the same clock.
/// </summary>
public class DeployableFactsTests
{
    private const string DeliveryPath = "/org/demo-system/status/delivery.json";
    private const string WorkItemsPath = "/org/demo-system/status/workitems.json";

    /// <summary>The clock of <see cref="SignallingTimeProvider"/>: every "ago" below is counted from it.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The release the "ui" of uat runs, with the two work items it carries.</summary>
    private const string WorkItems = """
        { "generatedAt": "2026-10-04T21:30:00Z",
          "releases": [ { "version": "2.4.14+9f8e7d6", "sha": "9f8e7d6c5b4a", "items": [15, 12] } ] }
        """;

    /// <summary>One environment, one deployable whose primary node answers for its build, with the system's delivery file.</summary>
    private const string Bare = """
        {
          "system": { "slug": "demo", "name": "Demo",
            "deliveryUrl": "https://raw.example.net/org/demo-system/status/delivery.json" },
          "environments": [ {
            "name": "tdd",
            "deployables": [ {
              "name": "api",
              "buildPath": "/_build",
              "nodes": [ { "name": "tdd-west", "region": "westus3", "role": "primary", "url": "https://tdd-west.example.net" } ]
            } ]
          } ]
        }
        """;

    /// <summary>The same system with neither of the row's two sources: no build path on the deployable, no delivery file.</summary>
    private const string NoSources = """
        {
          "system": { "slug": "demo", "name": "Demo" },
          "environments": [ {
            "name": "tdd",
            "deployables": [ {
              "name": "api",
              "nodes": [ { "name": "tdd-west", "region": "westus3", "role": "primary", "url": "https://tdd-west.example.net" } ]
            } ]
          } ]
        }
        """;

    private readonly SignallingTimeProvider _time = new();

    private static string DeployableFactsComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "DeployableFacts.razor"));

    private static string CodeCardComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "CodeCard.razor"));

    private static string DeliveryCardComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "DeliveryCard.razor"));

    private static string DeployableSectionComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "DeployableSection.razor"));

    private static string RuntimeViewComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "RuntimeView.razor"));

    /// <summary>The uat environment and its "ui", as the optics fixture's system has them after one round of checks.</summary>
    private static (EnvironmentStatus Environment, DeployableStatus Deployable) UatUi(DashboardMonitor monitor) =>
        Named(monitor, "uat");

    /// <summary>The tdd environment and its "ui".</summary>
    private static (EnvironmentStatus Environment, DeployableStatus Deployable) TddUi(DashboardMonitor monitor) =>
        Named(monitor, "tdd");

    private static (EnvironmentStatus, DeployableStatus) Named(DashboardMonitor monitor, string environment)
    {
        var entry = monitor.Environments.Single(candidate => candidate.Name == environment);
        return (entry, entry.Deployables.Single(deployable => deployable.Info.Name == "ui"));
    }

    [Fact]
    public void TheRowIsThereForEitherCardAloneAndAbsentForNeither()
    {
        var text = File.ReadAllText(DeployableFactsComponent());

        // The three reads the row decides on: its node's build, the file's entry for it, and the one context of both.
        Assert.Contains("var build = Deployable.Build;", text, StringComparison.Ordinal);
        Assert.Contains("var delivery = Monitor.Delivery?.Find(Environment.Name, Deployable.Info.Name);", text, StringComparison.Ordinal);
        Assert.Contains("var context = DeliveryText.Context(Deployable.Info.Name, Environment.Name);", text, StringComparison.Ordinal);

        var guard = text.IndexOf("@if (build is not null || delivery is not null)", StringComparison.Ordinal);
        Assert.True(guard >= 0);
        // Before the guard the file has only its comment and those three reads: nothing to render without a card.
        Assert.True(text.IndexOf('<', StringComparison.Ordinal) > guard);

        Assert.Contains("""        @if (build is not null)""", text, StringComparison.Ordinal);
        Assert.Contains("""        @if (delivery is not null)""", text, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, """<div class="facts-row">"""));
        Assert.Equal(1, CountOccurrences(text, "<CodeCard "));
        Assert.Equal(1, CountOccurrences(text, "<DeliveryCard "));
    }

    [Fact]
    public void EachCardIsToldTheSameContextTheSamePrefixAndTheSameClock()
    {
        var text = File.ReadAllText(DeployableFactsComponent());

        Assert.Contains(
            """<CodeCard Build="build" Context="@context" IdPrefix="@IdPrefix" Zone="Zone" Now="Now" />""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<DeliveryCard Entry="delivery" Environment="@Environment.Name" First="@Monitor.FirstEnvironment" Context="@context" IdPrefix="@IdPrefix" Zone="Zone" Now="Now" Changed="Monitor.Delivery?.Generated" Carries="@WorkItemsText.Carries(Monitor.WorkItems, delivery.Version)" />""",
            text,
            StringComparison.Ordinal);

        // One prefix for the row; each card makes its own heading id out of it, so the two never share one.
        Assert.Equal(2, CountOccurrences(text, """Context="@context"""));
        Assert.Equal(2, CountOccurrences(text, """"IdPrefix="@IdPrefix""""));
        Assert.Contains(""""aria-labelledby="@($"{IdPrefix}-code")"""", File.ReadAllText(CodeCardComponent()), StringComparison.Ordinal);
        Assert.Contains(""""aria-labelledby="@($"{IdPrefix}-delivery")"""", File.ReadAllText(DeliveryCardComponent()), StringComparison.Ordinal);

        // The card's "as of" is the file's own change, and nothing when the file gave none.
        var card = File.ReadAllText(DeliveryCardComponent());
        Assert.Contains("""private string? AsOf => Changed is { } changed ? $"Delivery facts as of the last change: {TimeText.DateAndTime(changed, Zone)}" : null;""", card, StringComparison.Ordinal);
        Assert.Contains("""<h4 class="card__title" id="@($"{IdPrefix}-delivery")" title="@AsOf">""", card, StringComparison.Ordinal);

        // Everything the row needs to say anything at all is required of its caller.
        Assert.Equal(6, CountOccurrences(text, "[Parameter, EditorRequired]"));
    }

    [Fact]
    public void TheRowIsTheOnlyPlaceThatPutsTheTwoCardsTogether()
    {
        var section = File.ReadAllText(DeployableSectionComponent());
        var runtime = File.ReadAllText(RuntimeViewComponent());

        // Both views reach the two cards through this one component, with their own environment and deployable.
        Assert.Contains(
            """<DeployableFacts Monitor="Monitor" Environment="Environment" Deployable="Deployable" IdPrefix="@IdPrefix" Zone="Zone" Now="Now" />""",
            section,
            StringComparison.Ordinal);
        Assert.Contains(
            "<DeployableFacts @key=\"deployable\" Monitor=\"Monitor\" Environment=\"facts\" Deployable=\"deployable\"",
            runtime,
            StringComparison.Ordinal);
        Assert.DoesNotContain("<CodeCard ", section, StringComparison.Ordinal);
        Assert.DoesNotContain("<DeliveryCard ", section, StringComparison.Ordinal);
        Assert.DoesNotContain("<CodeCard ", runtime, StringComparison.Ordinal);
        Assert.DoesNotContain("<DeliveryCard ", runtime, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithBothSourcesTheRowIsTheCodeOfTheNodeAndTheDeliveryOfTheFile()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath == WorkItemsPath
            ? StubHandler.Answer(HttpStatusCode.OK, WorkItems)
            : Optics.Answer(request));
        var monitor = Optics.Monitor(handler, _time);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);
        Assert.Equal(Now, _time.GetUtcNow());

        var (environment, deployable) = UatUi(monitor);
        var build = deployable.Build!;
        var delivery = monitor.Delivery!.Find(environment.Name, deployable.Info.Name)!;

        // What the row is told, from the two sources it reads.
        Assert.Equal("ui in uat", DeliveryText.Context(deployable.Info.Name, environment.Name));
        Assert.Equal("tdd", monitor.FirstEnvironment);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 21, 0, 0, TimeSpan.Zero), monitor.Delivery!.Generated);
        Assert.Equal("carries #12, #15", WorkItemsText.Carries(monitor.WorkItems, delivery.Version));

        // The code card: what the node said about its build, every line the node measured.
        Assert.Equal("2.4.15", build.Version);
        Assert.Equal("0a1b2c3", BuildText.ShortCommit(build.Commit!));
        Assert.Equal("2 h ago", TimeText.Ago(build.BuiltAt!.Value, Now));
        Assert.Equal("84,210 lines in 1,203 files", BuildText.Size(build.Code!));
        Assert.Equal("1,417: 1,009 unit, 240 integration, 168 acceptance", BuildText.Tests(build.Tests!));
        Assert.Equal("81.2 % of lines, 70.1 % of branches", BuildText.Coverage(build.Coverage!));
        Assert.Equal("average 1.9, worst 34 (5,210 methods)", BuildText.Complexity(build.Complexity!));
        Assert.Equal("worst 28.5, none over 30", BuildText.Crap(build.Crap!));
        Assert.False(BuildText.CrapIsOver(build.Crap!));
        Assert.Equal("no problems", BuildText.Problems(build.QodanaProblems!.Value));

        // The delivery card: the last deployment of that version to that environment.
        Assert.Equal("2.4.14", delivery.Version);
        Assert.Equal("2026-10-04T16:37:00Z", TimeText.Iso(delivery.DeployedAt!.Value));
        Assert.Equal("2026-10-04 16:37 +00:00", TimeText.DateAndTime(delivery.DeployedAt!.Value, TimeZoneInfo.Utc));
        Assert.Equal("5 h ago", TimeText.Ago(delivery.DeployedAt!.Value, Now));
        Assert.Equal("2 versions, 3 days behind tdd", DeliveryText.Behind(delivery.Behind, environment.Name, monitor.FirstEnvironment));
        Assert.Equal("4 deployments, none failed", DeliveryText.Frequency(delivery));
        Assert.Equal("5.2 h", DeliveryText.LeadTime(delivery.LeadTimeHours!.Value));

        // What the delivery card titles its heading with: the moment the file changed, in the viewer's zone.
        Assert.Equal("2026-10-04 21:00 +00:00", TimeText.DateAndTime(monitor.Delivery!.Generated!.Value, TimeZoneInfo.Utc));

        // "Last by a person" outlives the automatic sign-off of the deployment shown, and says its own reason.
        Assert.Equal("cm-ai-ops", delivery.SignedOffBy);
        Assert.Equal("2.4.11", delivery.LastPersonSignOff!.Version);
        Assert.Equal("pat.morgan", delivery.LastPersonSignOff.By);
        Assert.Equal("3 d ago", TimeText.Ago(delivery.LastPersonSignOff.At!.Value, Now));
        Assert.NotEqual(delivery.Reason, delivery.LastPersonSignOff.Reason);
    }

    [Fact]
    public async Task TheTwoCardsOfARowTellEachEnvironmentSeparately()
    {
        var handler = new StubHandler(request => Optics.Answer(request));
        var monitor = Optics.Monitor(handler, _time);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        // The same deployable in the two environments: the node's own build answer, and each environment's entry.
        var (tdd, tddUi) = TddUi(monitor);
        var (uat, uatUi) = UatUi(monitor);
        var inTdd = monitor.Delivery!.Find(tdd.Name, tddUi.Info.Name)!;
        var inUat = monitor.Delivery!.Find(uat.Name, uatUi.Info.Name)!;

        Assert.Equal("2.4.15", tddUi.Build!.Version);
        Assert.Equal("2.4.15", uatUi.Build!.Version);
        Assert.Equal("ui in tdd", DeliveryText.Context(tddUi.Info.Name, tdd.Name));
        Assert.Equal("ui in uat", DeliveryText.Context(uatUi.Info.Name, uat.Name));

        Assert.Equal(
            new DeliveryEntry(
                "ui",
                "2.4.15",
                new DateTimeOffset(2026, 10, 4, 20, 30, 0, TimeSpan.Zero),
                null,
                null,
                "0a1b2c3d4e5f",
                new DateTimeOffset(2026, 10, 4, 19, 45, 0, TimeSpan.Zero),
                0.75,
                new BehindFirst(0, 0),
                9,
                1,
                new Uri("https://octopus.example.net/r/2.4.15"),
                null),
            inTdd);
        Assert.Equal("45 min", DeliveryText.LeadTime(inTdd.LeadTimeHours!.Value));
        Assert.Equal("9 deployments, 1 failed", DeliveryText.Frequency(inTdd));
        Assert.Equal("90 min ago", TimeText.Ago(inTdd.DeployedAt!.Value, Now));

        // The first environment is compared with nobody: its row's "Compared" line is absent.
        Assert.Null(DeliveryText.Behind(inTdd.Behind, tdd.Name, monitor.FirstEnvironment));
        Assert.Equal("2 versions, 3 days behind tdd", DeliveryText.Behind(inUat.Behind, uat.Name, monitor.FirstEnvironment));
    }

    [Fact]
    public async Task ADeployableTheDeliveryFileDoesNotNameKeepsOnlyItsCodeCard()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath switch
        {
            "/_build" => StubHandler.Answer(HttpStatusCode.OK, """{ "version": "1.0.2", "commit": "0a1b2c3d4e5f60718293a4b5c6d7e8f901234567" }"""),
            DeliveryPath => StubHandler.Answer(HttpStatusCode.OK, """{ "environments": [ { "name": "tdd", "deployables": [ { "name": "other", "version": "9.9.9" } ] } ] }"""),
            "/_version" => StubHandler.Answer(HttpStatusCode.OK, """{"version":"1.0.2+0a1b2c3"}"""),
            _ => StubHandler.Answer(HttpStatusCode.OK, "Healthy"),
        });
        var monitor = Optics.Monitor(handler, _time, topology: Bare);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        var environment = Assert.Single(monitor.Environments);
        var deployable = Assert.Single(environment.Deployables);

        Assert.NotNull(deployable.Build);
        Assert.Null(monitor.Delivery!.Find(environment.Name, deployable.Info.Name));
        Assert.Equal("1.0.2", deployable.Build!.Version);
        Assert.Equal("api in tdd", DeliveryText.Context(deployable.Info.Name, environment.Name));

        // The file's entry for another deployable is nobody's here: the lookup is by both names.
        Assert.Null(monitor.Delivery!.Find(environment.Name, "api"));
        Assert.Equal("9.9.9", monitor.Delivery!.Find(environment.Name, "other")!.Version);
    }

    [Fact]
    public async Task WithTheLeastEachSourceCanSayBothCardsAreThereAndHaveNoFacts()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath switch
        {
            "/_build" => StubHandler.Answer(HttpStatusCode.OK, """{ "version": "1.0.2" }"""),
            DeliveryPath => StubHandler.Answer(HttpStatusCode.OK, """{ "environments": [ { "name": "tdd", "deployables": [ { "name": "api" } ] } ] }"""),
            "/_version" => StubHandler.Answer(HttpStatusCode.OK, """{"version":"1.0.2"}"""),
            _ => StubHandler.Answer(HttpStatusCode.OK, "Healthy"),
        });
        var monitor = Optics.Monitor(handler, _time, topology: Bare);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        var environment = Assert.Single(monitor.Environments);
        var deployable = Assert.Single(environment.Deployables);
        var build = deployable.Build!;
        var delivery = monitor.Delivery!.Find(environment.Name, deployable.Info.Name)!;

        // The row stands: both reads gave something. The code card has its one line and no fact of its own.
        Assert.NotNull(build);
        Assert.NotNull(delivery);
        Assert.Equal("1.0.2", build.Version);
        Assert.Null(build.Commit);
        Assert.Null(build.CommitUrl);
        Assert.Null(build.BuiltAt);
        Assert.Null(build.BuildUrl);
        Assert.Null(build.Code);
        Assert.Null(build.Tests);
        Assert.Null(build.Coverage);
        Assert.Null(build.Complexity);
        Assert.Null(build.Crap);
        Assert.Null(build.QodanaProblems);

        // The delivery card has its heading and nothing under it: no deployment, no sign-off, no comparison, no note.
        Assert.Equal(new DeliveryEntry("api", null, null, null, null, null, null, null, null, null, null, null), delivery);
        Assert.Null(monitor.Delivery!.Generated);
        Assert.Null(DeliveryText.Behind(delivery.Behind, environment.Name, monitor.FirstEnvironment));
        Assert.Null(delivery.LeadTimeHours);
        Assert.Null(DeliveryText.Frequency(delivery));
        Assert.Null(WorkItemsText.Carries(monitor.WorkItems, delivery.Version));
        Assert.True(monitor.WorkItemsAsked);
        Assert.Null(monitor.WorkItems);
    }

    [Fact]
    public async Task WithoutABuildPathAndWithoutDeliveryFactsThereIsNoRowAtAll()
    {
        var handler = new StubHandler(request => StubHandler.Answer(HttpStatusCode.OK, "Healthy"));
        var monitor = Optics.Monitor(handler, _time, topology: NoSources);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        var environment = Assert.Single(monitor.Environments);
        var deployable = Assert.Single(environment.Deployables);

        // The row's own condition, with both of its reads empty.
        Assert.Null(deployable.Build);
        Assert.Null(monitor.Delivery);
        Assert.False(deployable.Build is not null || monitor.Delivery?.Find(environment.Name, deployable.Info.Name) is not null);

        // Nothing was asked either: no build endpoint for the node, and no file addressed by the topology.
        Assert.Equal(0, Optics.Count(handler, "/_build"));
        Assert.Equal(0, Optics.Count(handler, DeliveryPath));
        Assert.Null(deployable.BuildSource);
        Assert.Null(monitor.Topology.System.DeliveryUrl);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }
}

/// <summary>
/// <c>DashboardCode.razor</c> is the same Code card for the dashboard itself: the build that serves the page, from the
/// file its own site publishes (<c>system.dashboard</c> of the topology). It is there only when the topology names
/// that file and the file has answered; with a heading it is a section of its own, without one a row of cards.
/// </summary>
public class DashboardCodeTests
{
    private const string FactsPath = "/build-facts.json";

    /// <summary>The build that serves the page, with every part a build may measure.</summary>
    private const string Facts = """
        { "version": "1.0.57+5c0ffee", "commit": "5c0ffee5c0ffee5c0ffee5c0ffee5c0ffee5c0ff",
          "commitUrl": "https://github.example.net/o/demo-dashboard/commit/5c0ffee5c0ffee5c0ffee5c0ffee5c0ffee5c0ff",
          "builtAt": "2026-10-04T20:00:00Z", "buildUrl": "https://github.example.net/o/demo-dashboard/actions/runs/57",
          "code": { "linesOfCode": 20100, "files": 160, "languages": [
            { "name": "C#", "lines": 14000, "files": 100 }, { "name": "Razor", "lines": 2600, "files": 33 }, { "name": "CSS", "lines": 1900, "files": 1 } ] },
          "tests": { "unit": 760, "integration": 40, "acceptance": null },
          "coverage": { "linePercent": 81.2, "branchPercent": 70.1 },
          "complexity": { "average": 1.9, "max": 34, "methods": 5210 },
          "crap": { "max": 41, "threshold": 30, "overThreshold": 3 },
          "analysis": { "qodanaProblems": 2 } }
        """;

    private static readonly string TopologyWithDashboard = Optics.Topology.Replace(
        "\"system\": { ",
        "\"system\": { \"dashboard\": { \"name\": \"dashboard\", \"buildPath\": \"" + FactsPath + "\" }, ",
        StringComparison.Ordinal);

    private readonly SignallingTimeProvider _time = new();

    private static string DashboardCodeComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "DashboardCode.razor"));

    private static string AppComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "App.razor"));

    private static string RuntimeViewComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "RuntimeView.razor"));

    /// <summary>A monitor whose page is served from <see cref="Optics.Page"/>: the address is written to the
    /// monitor, and the client is set up without one, as the prober's clients now are.</summary>
    private DashboardMonitor Monitor(StubHandler handler, string? topology = null)
    {
        var http = new HttpClient(handler);
        return new DashboardMonitor(
            TopologyParser.Parse(topology ?? TopologyWithDashboard).Topology!,
            new NodeProber(http, _time),
            new PinnedVersionsReader(http, _time),
            _time,
            pageAddress: new Uri(Optics.Page));
    }

    /// <summary>The monitor of the dashboard's own card after the one round that reads its file.</summary>
    private async Task<DashboardMonitor> ReadAsync(string facts)
    {
        var monitor = Monitor(new StubHandler(request => Optics.OwnSite(request) ? StubHandler.Answer(HttpStatusCode.OK, facts) : Optics.Answer(request)));
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero), _time.GetUtcNow());
        return monitor;
    }

    [Fact]
    public void TheCardIsThereOnlyWhenTheFileIsNamedAndHasAnswered()
    {
        var text = File.ReadAllText(DashboardCodeComponent());

        var guard = text.IndexOf(
            "@if (Monitor.Topology.System.Dashboard is { } dashboard && Monitor.DashboardBuild is { } build)",
            StringComparison.Ordinal);
        Assert.True(guard >= 0);
        // Before the guard the file has only its comment: nothing to render when either part is missing.
        Assert.True(text.IndexOf('<', StringComparison.Ordinal) > guard);
        Assert.Contains("var context = BuildText.DashboardContext(dashboard.Name);", text, StringComparison.Ordinal);

        Assert.Contains("""    if (Heading)""", text, StringComparison.Ordinal);
        Assert.Contains("""<section class="environment environment--self" aria-labelledby="@($"{IdPrefix}-title")">""", text, StringComparison.Ordinal);
        Assert.Contains("""<h2 id="@($"{IdPrefix}-title")">@dashboard.Name</h2>""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="chip" title="@BuildText.DashboardHelp">@BuildText.ThisPage</span>""", text, StringComparison.Ordinal);
        Assert.Contains("""<CodeCard Build="build" Context="@context" IdPrefix="@IdPrefix" Zone="Zone" Now="Now" />""", text, StringComparison.Ordinal);

        // One card in each branch: the heading's section holds it, and the row alone holds the other.
        Assert.Equal(2, CountOccurrences(text, "<CodeCard "));
        Assert.Equal(2, CountOccurrences(text, """<div class="facts-row">"""));
        var section = text.IndexOf("<section", StringComparison.Ordinal);
        var end = text.IndexOf("</section>", StringComparison.Ordinal);
        Assert.True(section >= 0 && end > section);
        Assert.True(text.IndexOf("""<div class="facts-row">""", section, StringComparison.Ordinal) < end);
        Assert.True(text.IndexOf("""<div class="facts-row">""", end, StringComparison.Ordinal) > end);

        // Only the heading is the caller's choice: everything else it needs is required.
        Assert.Equal(4, CountOccurrences(text, "[Parameter, EditorRequired]"));
        Assert.Equal(1, CountOccurrences(text, "[Parameter]"));
        Assert.Contains("public bool Heading { get; set; }", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheHealthViewMakesItASectionAndTheRuntimeViewACardAndNeitherReusesTheOthersPrefix()
    {
        var app = File.ReadAllText(AppComponent());
        var runtime = File.ReadAllText(RuntimeViewComponent());

        Assert.Contains(
            """<DashboardCode Monitor="monitor" Heading="true" IdPrefix="self" Zone="Zone" Now="Time.GetUtcNow()" />""",
            app,
            StringComparison.Ordinal);
        Assert.Contains(
            """<DashboardCode Monitor="Monitor" IdPrefix="runtime-self" Zone="Zone" Now="Now" />""",
            runtime,
            StringComparison.Ordinal);

        // Two places use it, and only the health view's is a section with a heading of its own.
        Assert.Equal(1, CountOccurrences(app, "<DashboardCode "));
        Assert.Equal(1, CountOccurrences(runtime, "<DashboardCode "));
        Assert.Equal(1, CountOccurrences(app, "Heading=\"true\"") + CountOccurrences(runtime, "Heading=\"true\""));

        // And each gives its own prefix, so the same page never writes "self-title" twice.
        Assert.DoesNotContain("IdPrefix=\"self\"", runtime, StringComparison.Ordinal);
        Assert.DoesNotContain("IdPrefix=\"runtime-self\"", app, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSectionOfAFullyAnsweringPageNamesItselfAndShowsEveryFactItsBuildMeasured()
    {
        var monitor = await ReadAsync(Facts);
        var dashboard = monitor.Topology.System.Dashboard!;
        var build = monitor.DashboardBuild!;

        // The heading row: the dashboard's own name, called "this page" beside it.
        Assert.Equal(new DashboardInfo("dashboard", FactsPath), dashboard);
        Assert.Equal("dashboard (this page)", BuildText.DashboardContext(dashboard.Name));
        Assert.Equal("this page", BuildText.ThisPage);
        Assert.Equal(
            "The build that serves this page, as its own site publishes it: the same facts every app of the system reports about its build.",
            BuildText.DashboardHelp);

        // The card is the apps' card, filled with what this page's file said.
        Assert.Equal("1.0.57", build.Version);
        Assert.Equal("5c0ffee", BuildText.ShortCommit(build.Commit!));
        Assert.Equal("2 h ago", TimeText.Ago(build.BuiltAt!.Value, _time.GetUtcNow()));
        Assert.Equal("20,100 lines in 160 files", BuildText.Size(build.Code!));
        Assert.Equal("C# 76 %, Razor 14 %, CSS 10 %", BuildText.Describe(BuildText.Shares(build.Code!)));
        Assert.Equal("800: 760 unit, 40 integration", BuildText.Tests(build.Tests!));
        Assert.Equal("81.2 % of lines, 70.1 % of branches", BuildText.Coverage(build.Coverage!));
        Assert.Equal("average 1.9, worst 34 (5,210 methods)", BuildText.Complexity(build.Complexity!));
        Assert.Equal("worst 41, 3 methods over 30", BuildText.Crap(build.Crap!));
        Assert.True(BuildText.CrapIsOver(build.Crap!));
        Assert.Equal("2 problems", BuildText.Problems(build.QodanaProblems!.Value));

        // The page's own build is nobody else's: the apps keep what their nodes reported.
        Assert.All(
            monitor.Environments,
            environment => Assert.All(environment.Deployables, deployable => Assert.Equal("2.4.15", deployable.Build!.Version)));
    }

    [Fact]
    public async Task WithTheLeastItsFileCanSayTheSectionStandsAndTheCardHasOneLine()
    {
        var monitor = await ReadAsync("""{ "version": "1.0.2" }""");
        var build = monitor.DashboardBuild!;

        Assert.NotNull(monitor.Topology.System.Dashboard);
        Assert.Equal("1.0.2", build.Version);
        Assert.Equal("dashboard (this page)", BuildText.DashboardContext(monitor.Topology.System.Dashboard!.Name));

        // Every other line of the card stands or falls by its own fact: none is there here.
        Assert.Null(build.Commit);
        Assert.Null(build.CommitUrl);
        Assert.Null(build.BuiltAt);
        Assert.Null(build.BuildUrl);
        Assert.Null(build.Code);
        Assert.Null(build.Tests);
        Assert.Null(build.Coverage);
        Assert.Null(build.Complexity);
        Assert.Null(build.Crap);
        Assert.Null(build.QodanaProblems);
    }

    [Fact]
    public async Task ATopologyThatNamesNoFileForItselfShowsNothingThoughEveryAppAnswered()
    {
        var handler = new StubHandler(request => Optics.Answer(request));
        var monitor = Monitor(handler, Optics.Topology);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Null(monitor.Topology.System.Dashboard);
        Assert.Null(monitor.DashboardBuild);
        Assert.DoesNotContain(handler.Requests, Optics.OwnSite);

        // The apps' rows are untouched: each deployable still has the build its node reported.
        Assert.All(
            monitor.Environments,
            environment => Assert.All(environment.Deployables, deployable => Assert.NotNull(deployable.Build)));
    }

    [Fact]
    public async Task WhileItsOwnFileDoesNotAnswerThereIsNoCardYetAndItIsAskedAgain()
    {
        var handler = new StubHandler(request => Optics.OwnSite(request)
            ? StubHandler.Answer(HttpStatusCode.NotFound, "<html>404</html>")
            : Optics.Answer(request));
        var monitor = Monitor(handler);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        // The guard's first read is there and its second is not: nothing renders, although the apps' cards do.
        Assert.NotNull(monitor.Topology.System.Dashboard);
        Assert.Null(monitor.DashboardBuild);
        Assert.Equal(1, handler.Requests.Count(Optics.OwnSite));

        _time.Advance(DashboardMonitor.DashboardBuildRetry);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Equal(2, handler.Requests.Count(Optics.OwnSite));
        Assert.Null(monitor.DashboardBuild);

        // The deployables' own rows are unaffected: their builds come from their nodes, not from this file.
        Assert.All(
            monitor.Environments,
            environment => Assert.All(environment.Deployables, deployable => Assert.NotNull(deployable.Build)));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }
}
