using System.Globalization;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The three cards the environment and cluster views live on: the TargetTile (one endpoint of a deployable),
/// the ClusterCard (the cluster as its collector reports it) and the EnvironmentDelivery (the releases of an
/// environment that belong to no deployable the page checks). The markup each one emits for its main states is
/// pinned here, next to the real words the helpers say for fixed inputs: the tile says nothing with no checks at
/// all but a dash in each of its numbers, the cluster card always says its headline as a status region and says
/// its sums only when the status file answers, and the environment's other releases say nothing at all without
/// the delivery facts.
/// </summary>
public class TargetTileClusterCardEnvironmentDeliveryTests
{
    private static readonly DateTimeOffset CheckedAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = ClusterFixture.Generated.AddSeconds(20);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static string TargetTileComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "TargetTile.razor"));

    private static string ClusterCardComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterCard.razor"));

    private static string EnvironmentDeliveryComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "EnvironmentDelivery.razor"));

    private static TargetStatus Node() =>
        new(TargetKind.Node, "app-westus3", new Uri("https://app-westus3.example.net"), "westus3", "primary");

    // ----- The target tile -----

    [Fact]
    public void TheTileIsAnArticleWhoseClassIsTheLowercasedStateOfItsTarget()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains(
            """<article class="tile tile--@Target.State.ToString().ToLowerInvariant()" aria-labelledby="@HeadingId">""",
            text,
            StringComparison.Ordinal);

        // The state is the last check's state (TargetStatus.State): pending before the first check, then what
        // it found. The tile's class follows it, as the badge under its heading does.
        var target = Node();
        Assert.Equal(HealthState.Pending, target.State);
        Assert.Null(target.Last);
        target.Record(new ProbeResult(HealthState.Healthy, 200, 12, CheckedAt, Version: "2.4.21"));
        Assert.Equal(HealthState.Healthy, target.State);
    }

    [Theory]
    [InlineData(HealthState.Pending, "pending")]
    [InlineData(HealthState.Healthy, "healthy")]
    [InlineData(HealthState.Unhealthy, "unhealthy")]
    [InlineData(HealthState.Unreachable, "unreachable")]
    public void EveryHealthStateHasALowercasedTileClass(HealthState state, string expectedCss)
    {
        Assert.Equal($"tile tile--{expectedCss}", $"tile tile--{state.ToString().ToLowerInvariant()}");
    }

    [Fact]
    public void TheHeaderNamesTheTargetAndLinksItsBadgeToLiveMetrics()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains("""<header class="tile__header">""", text, StringComparison.Ordinal);
        Assert.Contains("""<h4 id="@HeadingId">@Title</h4>""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<DeepLink Href="StateLink" Title="@LinkText.For(LinkSet.LiveMetrics, Target.Name)" Class="deep--badge">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<StateBadge State="Target.State" />""", text, StringComparison.Ordinal);
        Assert.Contains("private string Title => IsFrontDoor ? \"Front Door\" : Target.Region ?? Target.Name;", text, StringComparison.Ordinal);
        Assert.Contains("private string Subtitle => IsFrontDoor ? \"public address\" : Target.Role ?? \"node\";", text, StringComparison.Ordinal);
        Assert.Contains("private Uri? StateLink => IsFrontDoor ? null : Target.Links[LinkSet.LiveMetrics];", text, StringComparison.Ordinal);
        Assert.Contains("private Uri? NameLink => Target.Links[IsFrontDoor ? LinkSet.FrontDoor : LinkSet.Portal];", text, StringComparison.Ordinal);

        // The words the tooltip says for this node: the badge leads to its Live Metrics, the name to its portal.
        Assert.Equal(
            "Live Metrics of Application Insights for app-westus3: requests, failures and servers as they happen (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.LiveMetrics, "app-westus3"));
        Assert.Equal(
            "The web app app-westus3 in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.Portal, "app-westus3"));
        Assert.Equal("app-westus3.example.net", new Uri("https://app-westus3.example.net").Host);
        Assert.Equal("demo", new Uri("https://demo.azurefd.net").Host.Split('.')[0]);
    }

    [Fact]
    public void TheChipsNameTheRoleAndServingOnlyWhereTheDecisionExpectsIt()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains("""<p class="tile__chips">""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="chip">@Subtitle</span>""", text, StringComparison.Ordinal);
        Assert.Contains("@if (IsExpected)", text, StringComparison.Ordinal);
        Assert.Contains(
            """<span class="chip chip--serving" title="@ServesHelp">serves traffic</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("\"The deployable's only node, and it is healthy\"", text, StringComparison.Ordinal);
        Assert.Contains("\"The Front Door rotates over the healthy primary nodes\"", text, StringComparison.Ordinal);
        Assert.Contains("\"The first healthy node in priority order: primary, then standby\"", text, StringComparison.Ordinal);
        Assert.Equal("primary", Node().Role);
        Assert.True(Node().IsPrimary);
    }

    [Fact]
    public void TheAddressLineLinksTheNameAndGivesTheTargetHost()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains("""<p class="tile__address">""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="tile__name">""", text, StringComparison.Ordinal);
        Assert.Contains("""<DeepLink Href="NameLink" Title="@NameTitle">@ResourceName</DeepLink>""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<a href="@Target.Url.AbsoluteUri" rel="noopener noreferrer">@Target.Url.Host</a>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("private string ResourceName => IsFrontDoor ? Target.Url.Host.Split('.')[0] : Target.Name;", text, StringComparison.Ordinal);
        Assert.Equal("app-westus3", Node().Name);
        Assert.Equal("https://app-westus3.example.net/", Node().Url.AbsoluteUri);
    }

    [Fact]
    public void TheTileHasNoEmptyStateItsNumbersFallBackToADash()
    {
        var text = File.ReadAllText(TargetTileComponent());

        // The tile is never empty: with no checks at all it is a pending tile whose numbers say "—", and no
        // status region or empty-state paragraph is in it.
        Assert.Contains("private const string None = \"—\";", text, StringComparison.Ordinal);
        Assert.Contains("""<dd>@(Target.Last?.StatusCode?.ToString(CultureInfo.InvariantCulture) ?? None)</dd>""", text, StringComparison.Ordinal);
        Assert.Contains("""<dd>@(Target.Last?.LatencyMs is { } latency ? $"{latency} ms" : None)</dd>""", text, StringComparison.Ordinal);
        Assert.Contains("""<time datetime="@TimeText.Iso(last.CheckedAt)">@TimeText.Clock(last.CheckedAt, Zone)</time>""", text, StringComparison.Ordinal);
        Assert.DoesNotContain("role=\"status\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"empty\"", text, StringComparison.Ordinal);

        // Before the first check every number is the dash; after one, the check's own.
        var target = Node();
        Assert.Null(target.Last?.StatusCode?.ToString(CultureInfo.InvariantCulture));
        Assert.Null(target.Last?.LatencyMs);
        target.Record(new ProbeResult(HealthState.Healthy, 200, 12, CheckedAt));
        Assert.Equal("200", target.Last?.StatusCode?.ToString(CultureInfo.InvariantCulture));
        Assert.Equal("12 ms", $"{target.Last!.LatencyMs} ms");
        Assert.Equal("2026-10-04T22:00:00Z", TimeText.Iso(CheckedAt));
        Assert.Equal("22:00:00", TimeText.Clock(CheckedAt, Utc));
    }

    [Fact]
    public void TheDeploymentMarkIsOneLineNamedAfterTheDeployable()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains(
            """<DeploymentMarks Marks="Deployments" Label="@($"Deployments of {Deployable?.Info.Name ?? ResourceName}")" Generated="DeploymentsAsOf" Zone="Zone" />""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheTelemetryBlockCountsTheAppsOwnNumbersAndWarnsOnErrors()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains("""@if (Target.Telemetry is { } telemetry)""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<h5 class="tile__label" id="@($"{HeadingId}-calls")" title="Counted by the web app itself over the last minute">Last minute</h5>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<dl class="metrics metrics--compact" aria-labelledby="@($"{HeadingId}-calls")">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<DeepLink Href="Target.Links[LinkSet.Performance]" Title="@LinkText.For(LinkSet.Performance, Target.Name)">@Count(telemetry.Requests)<span class="unit">/min</span></DeepLink>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<TrendLine Trend="Trends.Requests(Target)" />""", text, StringComparison.Ordinal);
        Assert.Contains("""<dd>@(telemetry.P95Ms is { } p95 ? $"{p95} ms" : None)</dd>""", text, StringComparison.Ordinal);
        Assert.Contains("""<div class="@(telemetry.Errors > 0 ? "metrics__warn" : null)">""", text, StringComparison.Ordinal);
        Assert.Contains("""<dt>Errors</dt>""", text, StringComparison.Ordinal);
        Assert.Contains("""<DeepLink Href="Target.Links[LinkSet.Failures]" Title="@LinkText.For(LinkSet.Failures, Target.Name)">@Count(telemetry.Errors)</DeepLink>""", text, StringComparison.Ordinal);
        Assert.Contains("""<dt title="@SqlHelp(telemetry)">SQL</dt>""", text, StringComparison.Ordinal);
        Assert.Contains("""@if (Deployable is { HasFrontDoor: true })""", text, StringComparison.Ordinal);
        Assert.Contains("""<dt title="Requests Front Door forwarded to this web app">Front Door</dt>""", text, StringComparison.Ordinal);
        Assert.Contains("\"SQL commands while handling requests: what traffic causes.", text, StringComparison.Ordinal);
        Assert.Contains("\"SQL commands the process ran, whatever caused them\"", text, StringComparison.Ordinal);

        // The number the SQL line shows: the requests' commands where the app tells them from the background
        // ones, all the commands otherwise. The counts are grouped as the page groups them.
        var splitting = new TelemetrySnapshot(42, 30, 12, 1, 120, 5, 1, 8, null, 2, CheckedAt) { SqlRequests = 6, SqlBackground = 2 };
        var counting = new TelemetrySnapshot(42, 30, 12, 0, 120, 5, 1, 8, null, 2, CheckedAt);
        Assert.True(splitting.SplitsSql);
        Assert.Equal(6, splitting.SqlOfTraffic);
        Assert.False(counting.SplitsSql);
        Assert.Equal(8, counting.SqlOfTraffic);
        Assert.Equal("1,234", BuildText.Count(1234));
        Assert.Equal("0", BuildText.Count(0));
        Assert.Equal(
            "Failures in Application Insights for app-westus3: failed requests and exceptions (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.Failures, "app-westus3"));
    }

    [Fact]
    public void TheProcessBlockSaysUptimeAndMarksARecentRestart()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains("""@if (telemetry.Process is { } vitals)""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<h5 class="tile__label" id="@($"{HeadingId}-process")" title="As the web app's process reports them">Process</h5>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """"<span class="tile__uptime @(vitals.RestartedRecently ? "tile__uptime--restarted" : null)"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """title="@(vitals.RestartedRecently ? "The process started within the last five minutes: a deployment, a restart or a crash" : "How long the process has run")">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""@VitalsText.Uptime(uptime)""", text, StringComparison.Ordinal);
        Assert.Contains("""@(vitals.CpuPercent is { } cpu ? VitalsText.Cpu(cpu) : None)""", text, StringComparison.Ordinal);
        Assert.Contains("""<TrendLine Trend="Trends.Cpu(Target)" />""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<dd title="@VitalsText.MemoryDetail(vitals)">@((vitals.WorkingSetMb ?? vitals.GcHeapMb) is { } memory ? VitalsText.Memory(memory) : None)</dd>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<dd title="Requests being answered at this moment">@(vitals.InFlight is { } inFlight ? Count(inFlight) : None)</dd>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<div class="@(vitals.ExceptionsPerMinute > 0 ? "metrics__warn" : null)">""", text, StringComparison.Ordinal);
        Assert.Contains("""<DeepLink Href="Target.Links[LinkSet.Failures]" Title="@LinkText.For(LinkSet.Failures, Target.Name)">@Count(exceptions)<span class="unit">/min</span></DeepLink>""", text, StringComparison.Ordinal);

        // Up a while is "up"; under five minutes is a restart, and the tile says so with the cross.
        var settled = new ProcessVitals(12.4, 412, 96, 41, 3, 0, 3600);
        var restarted = new ProcessVitals(12.4, 412, 96, 41, 3, 1, 120);
        Assert.False(settled.RestartedRecently);
        Assert.True(restarted.RestartedRecently);
        Assert.Equal("up 60 min", VitalsText.Uptime(3600));
        Assert.Equal("restarted 2 min ago", VitalsText.Uptime(120));
        Assert.Equal("12 %", VitalsText.Cpu(12.4));
        Assert.Equal("9.6 %", VitalsText.Cpu(9.6));
        Assert.Equal("412 MB", VitalsText.Memory(412));
        Assert.Equal("1.4 GB", VitalsText.Memory(1400));
        Assert.Equal("Working set 412 MB, of which managed heap 96 MB; 41 threads", VitalsText.MemoryDetail(settled));
    }

    [Fact]
    public void TheHealthChecksAreOneMarkPerEntryAndSayWhoIsNotHealthy()
    {
        var text = File.ReadAllText(TargetTileComponent());

        Assert.Contains("""@if (Target.HealthDetail is { } checks)""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<h5 class="tile__label" id="@($"{HeadingId}-checks")" title="The entries of the web app's detailed health check, read with every health check of this page">Health checks</h5>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<ul class="checks" aria-labelledby="@($"{HeadingId}-checks")">""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<li class="check check--@entry.State.ToString().ToLowerInvariant()" title="@HealthDetailText.Title(entry)">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<StateIcon State="HealthDetailText.Icon(entry.State)" />""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="check__name">@entry.Name</span>""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="@(entry.IsHealthy ? "visually-hidden" : "check__state")">@HealthDetailText.Label(entry.State)</span>""", text, StringComparison.Ordinal);
        Assert.Contains(
            """@if (Target.State is HealthState.Unhealthy or HealthState.Unreachable && HealthDetailText.Failed(checks) is { } failed)""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<p class="tile__detail tile__detail--failed">""", text, StringComparison.Ordinal);
        Assert.Contains("""<p class="tile__detail">@detail</p>""", text, StringComparison.Ordinal);
        Assert.Contains("""<HistoryStrip History="Target.History" Zone="Zone" />""", text, StringComparison.Ordinal);

        // A healthy entry says its state to no one (visually hidden); the others say it in the check__state
        // span, with the icon of the state and the entry's own words and duration in the tooltip.
        var database = new HealthCheckEntry("DataAccess", CheckState.Unhealthy, "Login failed", 12.3);
        var gateway = new HealthCheckEntry("LlmGateway", CheckState.Degraded, "Database connection successful", 42.1);
        var steady = new HealthCheckEntry("Sqlite", CheckState.Healthy, null, 0.04);
        Assert.True(steady.IsHealthy);
        Assert.Equal(HealthState.Healthy, HealthDetailText.Icon(steady.State));
        Assert.Equal(HealthState.Unreachable, HealthDetailText.Icon(database.State));
        Assert.Equal(HealthState.Unhealthy, HealthDetailText.Icon(gateway.State));
        Assert.Equal("DataAccess: Unhealthy. Login failed. Took 12.3 ms.", HealthDetailText.Title(database));
        Assert.Equal("LlmGateway: Degraded. Database connection successful. Took 42.1 ms.", HealthDetailText.Title(gateway));
        Assert.Equal("Not healthy: DataAccess (Unhealthy: Login failed), LlmGateway (Degraded: Database connection successful)",
            HealthDetailText.Failed(new HealthDetail([gateway, database], CheckedAt)));
        Assert.Null(HealthDetailText.Failed(new HealthDetail([steady], CheckedAt)));
        Assert.Equal("4 checks healthy",
            HealthDetailText.Summary(new HealthDetail([steady, steady, steady, steady], CheckedAt)));
    }

    // ----- The cluster card -----

    [Fact]
    public void TheCardIsASectionWhoseClassIsTheLowercasedAssessmentState()
    {
        var text = File.ReadAllText(ClusterCardComponent());

        Assert.Contains(
            """<section class="card cluster-card cluster-card--@assessment.State.ToString().ToLowerInvariant()" aria-labelledby="cluster-live-title">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<header class="cluster-card__header">""", text, StringComparison.Ordinal);
        Assert.Contains("""<h3 class="card__title" id="cluster-live-title">""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="card__context">live, as the collector inside it reports</span>""", text, StringComparison.Ordinal);
        Assert.Contains("""<ClusterBadge State="assessment.State" Label="@assessment.Label" />""", text, StringComparison.Ordinal);
        Assert.Contains("var assessment = ClusterAssessment.Live(Reading, Service, Now, Zone);", text, StringComparison.Ordinal);
        Assert.Contains("var asOf = status?.AsOf(Now) ?? Now;", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(ClusterState.Checking, "checking")]
    [InlineData(ClusterState.Healthy, "healthy")]
    [InlineData(ClusterState.Warning, "warning")]
    [InlineData(ClusterState.Unhealthy, "unhealthy")]
    [InlineData(ClusterState.Unreachable, "unreachable")]
    [InlineData(ClusterState.Neutral, "neutral")]
    public void EveryClusterStateHasALowercasedCardClass(ClusterState state, string expectedCss)
    {
        Assert.Equal($"cluster-card--{expectedCss}", $"cluster-card--{state.ToString().ToLowerInvariant()}");
    }

    [Fact]
    public void TheHeadlineIsAStatusRegionThatIsAlwaysSaid()
    {
        var text = File.ReadAllText(ClusterCardComponent());

        // The one line of the view that says whether something is wrong is a status region: a refresh that
        // finds nothing to show is announced. It stands before the status guard, so the card says it even when
        // the file does not answer.
        Assert.Contains(
            """<p class="cluster-card__headline" role="status">@assessment.Headline</p>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("@if (assessment.Detail is { } detail)", text, StringComparison.Ordinal);
        Assert.Contains("""<p class="cluster-card__detail">@detail</p>""", text, StringComparison.Ordinal);
        var headline = text.IndexOf("role=\"status\"", StringComparison.Ordinal);
        var guard = text.IndexOf("@if (status is not null)", StringComparison.Ordinal);
        Assert.True(headline >= 0 && guard > headline, "The headline must stand before the status guard.");

        // The words the region announces, for the states the card can be in.
        var pending = new SourceReading<ClusterStatus>(SourceState.Pending);
        Assert.Equal(
            new ClusterAssessment(ClusterState.Checking, "Checking", "Reading the cluster's status file"),
            ClusterAssessment.Live(pending, null, Now, Utc));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Unreachable, "Unreachable", "The cluster's status file does not answer", "No answer within 10 s."),
            ClusterAssessment.Live(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: "no answer within 10 s"), null, Now, Utc));
        var healthy = ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui")));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every node is ready and under no pressure, and every pod is ready"),
            ClusterAssessment.Live(ClusterFixture.Read(healthy), null, Now, Utc));
    }

    [Fact]
    public void TheSumsAreSaidOnlyWhenTheStatusFileAnswers()
    {
        var text = File.ReadAllText(ClusterCardComponent());

        // Without the file's answer (Reading.Value null) the card is its header and its headline only: the
        // totals, the meters and the footer all stand in the guards.
        Assert.Contains("@if (status is not null)", text, StringComparison.Ordinal);
        Assert.Contains("var totals = ClusterTotals.Of(status, asOf);", text, StringComparison.Ordinal);
        var guard = text.IndexOf("@if (status is not null)", StringComparison.Ordinal);
        var totals = text.IndexOf("<dl class=\"metrics cluster-card__totals\">", StringComparison.Ordinal);
        Assert.True(guard >= 0 && totals > guard, "The totals must stand inside the status guard.");
        Assert.Equal(1, CountOccurrences(text, "cluster-card__totals"));
        Assert.Contains("""<dd>@ClusterText.Of(totals.NodesReady, totals.Nodes)</dd>""", text, StringComparison.Ordinal);
        Assert.Contains("""<dt title="Finished jobs are in neither number">Pods ready</dt>""", text, StringComparison.Ordinal);
        Assert.Contains("""<span>@ClusterText.Of(totals.PodsReady, totals.Pods)</span>""", text, StringComparison.Ordinal);
        Assert.Contains("@if (totals.PodsFinished > 0)", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="unit">@totals.PodsFinished finished</span>""", text, StringComparison.Ordinal);
        Assert.Contains("""@if (totals.PodsOnNodes is { } pods)""", text, StringComparison.Ordinal);
        Assert.Contains("""<dt title="The pods the nodes run, of how many they can take">Pods on the nodes</dt>""", text, StringComparison.Ordinal);
        Assert.Contains("""<dd>@(totals.PodCapacity is { } capacity ? ClusterText.Of(pods, capacity) : BuildText.Count(pods))</dd>""", text, StringComparison.Ordinal);
        Assert.Contains("""<dt title="How often containers were started again, over all pods">Restarts</dt>""", text, StringComparison.Ordinal);
        Assert.Contains("""<dd>@BuildText.Count(totals.Restarts)</dd>""", text, StringComparison.Ordinal);
        Assert.Contains("@if (status.KubernetesVersion is { } version)", text, StringComparison.Ordinal);
        Assert.Contains(
            """"<UseRow Name="CPU" Label="CPU used of what the nodes offer" Value="@ClusterText.NodeCpu(totals.Cpu)"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """"Percent="ClusterText.Share(totals.Cpu.Usage, totals.Cpu.Allocatable)" Trend="CpuTrend"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """"Requested="@ClusterText.Requested(totals.Cpu)" RequestedPercent="ClusterText.Share(totals.Cpu.Requests, totals.Cpu.Allocatable)"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""RequestedLabel="CPU the pods request of what the nodes offer" />""", text, StringComparison.Ordinal);
        Assert.Contains(
            """"<UseRow Name="Memory" Label="Memory used of what the nodes offer" Value="@ClusterText.NodeMemory(totals.Memory)"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""RequestedLabel="Memory the pods request of what the nodes offer" />""", text, StringComparison.Ordinal);

        // The sums the card says for one ready node with one ready pod and one finished job.
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space("apps", ClusterFixture.Pod("ui"), ClusterFixture.Pod("job", ClusterPod.Succeeded, reason: "Completed").NotReady()));
        var totalsOf = ClusterTotals.Of(status, ClusterFixture.Generated);
        Assert.Equal((1, 1, 1, 1, 1, 0, 63, 110),
            (totalsOf.NodesReady, totalsOf.Nodes, totalsOf.PodsReady, totalsOf.Pods, totalsOf.PodsFinished, totalsOf.Restarts, totalsOf.PodsOnNodes!.Value, totalsOf.PodCapacity!.Value));
        Assert.Equal("1 of 1", ClusterText.Of(totalsOf.NodesReady, totalsOf.Nodes));
        Assert.Equal("1 of 1", ClusterText.Of(totalsOf.PodsReady, totalsOf.Pods));
        Assert.Equal("1 finished", $"{totalsOf.PodsFinished} finished");
        Assert.Equal("0", BuildText.Count(totalsOf.Restarts));
        Assert.Equal("63 of 110", ClusterText.Of(totalsOf.PodsOnNodes!.Value, totalsOf.PodCapacity!.Value));
        Assert.Equal("v1.33.3", status.KubernetesVersion);

        // The meters: the node's own CPU and memory against what it offers, and the share the pods request.
        Assert.Equal("0.81 of 3.86 cores", ClusterText.NodeCpu(totalsOf.Cpu));
        Assert.Equal("9.2 of 12.5 GiB", ClusterText.NodeMemory(totalsOf.Memory));
        Assert.Equal("63 % requested", ClusterText.Requested(totalsOf.Cpu));
        Assert.Equal(21.0363, ClusterText.Share(totalsOf.Cpu.Usage, totalsOf.Cpu.Allocatable)!.Value, 2);
    }

    [Fact]
    public void TheFootSaysWhenTheCollectorWroteAndHowOften()
    {
        var text = File.ReadAllText(ClusterCardComponent());

        // The footer is no part of the empty card: it needs a time or an interval the file says.
        Assert.Contains("@if (status is { Generated: not null } or { IntervalSeconds: not null })", text, StringComparison.Ordinal);
        Assert.Contains("""<p class="cluster-card__foot">""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<time datetime="@TimeText.Iso(generated)" title="@TimeText.DateAndTime(generated, Zone)">@ClusterText.AsOf(generated, Now, Zone)</time>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<span>the collector writes every @interval s</span>""", text, StringComparison.Ordinal);
        Assert.Equal("as of 20:15:30 (just now)", ClusterText.AsOf(ClusterFixture.Generated, Now, Utc));
        Assert.Equal("2026-10-06T20:15:30Z", TimeText.Iso(ClusterFixture.Generated));
        Assert.Equal(15, ClusterFixture.Status().IntervalSeconds);
    }

    // ----- The environment's other deliveries -----

    private const string Delivery = """
        { "generated": "2026-10-04T21:00:00Z",
          "environments": [
            { "name": "uat",
              "deployables": [
                { "name": "dashboard", "version": "1.0.7", "deployedAt": "2026-10-02T10:00:00Z" },
                { "name": "system", "version": "1.0.33", "deployedAt": "2026-10-03T11:00:00Z" },
                { "name": "ui", "version": "2.4.14", "deployedAt": "2026-10-04T16:37:00Z" } ] } ] }
        """;

    private static readonly DeliveryReport Report = DeliveryReport.Parse(Delivery)!;

    private const string WorkItems = """
        { "generated": "2026-10-04T21:00:00Z",
          "releases": [ { "version": "1.0.7", "items": [15, 12] } ] }
        """;

    [Fact]
    public void TheEnvironmentSaysNothingOfItsOtherReleasesWithoutTheDeliveryFacts()
    {
        var text = File.ReadAllText(EnvironmentDeliveryComponent());

        // Everything the component says is one group of cards, and only while there is an entry to say:
        // without the delivery facts (or with none left over) it renders nothing, and it has no status region
        // or empty-state paragraph to say so.
        Assert.Contains(
            "var others = Monitor.Delivery?.Others(Environment.Name, Environment.Deployables.Select(deployable => deployable.Info.Name)) ?? [];",
            text,
            StringComparison.Ordinal);
        Assert.Contains("@if (others.Count > 0)", text, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "@if (others.Count > 0)"));
        var guard = text.IndexOf("@if (others.Count > 0)", StringComparison.Ordinal);
        var group = text.IndexOf("<div class=\"facts-row\"", StringComparison.Ordinal);
        Assert.True(guard >= 0 && group > guard, "The group must stand inside the guard.");
        Assert.DoesNotContain("role=\"status\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"empty\"", text, StringComparison.Ordinal);

        // No delivery report, and no environment the report names: nothing to render.
        Assert.Null(DeliveryReport.Parse(null));
        Assert.Empty(Report.Others("staging", ["ui"]));
        Assert.Empty(Report.Others("uat", ["dashboard", "system", "ui"]));
    }

    [Fact]
    public void TheGroupNamesTheEnvironmentAndDrawsOneCardPerEntryTheSystemFirst()
    {
        var text = File.ReadAllText(EnvironmentDeliveryComponent());

        Assert.Contains("""<div class="facts-row" role="group" aria-label="@($"Other releases in {Environment.Name}")">""", text, StringComparison.Ordinal);
        Assert.Contains("""var index = 0;""", text, StringComparison.Ordinal);
        Assert.Contains("@foreach (var entry in others)", text, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "<DeliveryCard"));

        // What the group holds for uat while the page shows ui: the system project first (what it is), then
        // the deployables the topology leaves out, such as the dashboard.
        var others = Report.Others("uat", ["ui"]);
        Assert.Equal(["system", "dashboard"], others.Select(entry => entry.Name));
        Assert.Equal("1.0.33", others[0].Version);
        Assert.Equal("1.0.7", others[1].Version);
        Assert.Equal("the system (infrastructure and pipeline) in uat", DeliveryText.Context("system", "uat"));
        Assert.Equal("dashboard in uat", DeliveryText.Context("dashboard", "uat"));
        Assert.Equal("2026-10-04T21:00:00Z", TimeText.Iso(Report.Generated!.Value));
    }

    [Fact]
    public void EachCardNamesItsEntryItsEnvironmentItsOwnIdAndItsWorkItems()
    {
        var text = File.ReadAllText(EnvironmentDeliveryComponent());

        Assert.Contains(""""<DeliveryCard @key="entry.Name" Entry="entry" Environment="@Environment.Name" First="@Monitor.FirstEnvironment"""", text, StringComparison.Ordinal);
        Assert.Contains(""""Context="@DeliveryText.Context(entry.Name, Environment.Name)" IdPrefix="@($"{IdPrefix}-other-{index++}")"""", text, StringComparison.Ordinal);
        Assert.Contains("""Zone="Zone" Now="Now" Changed="Monitor.Delivery?.Generated" Carries="@WorkItemsText.Carries(Monitor.WorkItems, entry.Version)" />""", text, StringComparison.Ordinal);

        // The work items a release carries, in numbers, and nothing for a version the file does not name.
        var workItems = WorkItemsReport.Parse(WorkItems)!;
        Assert.Equal("carries #12, #15", WorkItemsText.Carries(workItems, "1.0.7"));
        Assert.Null(WorkItemsText.Carries(workItems, "2.4.14"));
        Assert.Null(WorkItemsText.Carries(null, "1.0.7"));
        Assert.Null(WorkItemsText.Carries(workItems, null));
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
