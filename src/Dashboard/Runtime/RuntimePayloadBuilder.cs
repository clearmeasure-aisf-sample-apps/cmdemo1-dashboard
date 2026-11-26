using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>
/// The runtime view's update from the monitor's current state: the same checks, serving decision and version
/// comparison the health view shows, mapped onto the diagram's elements by the manifest. A node of the manifest is
/// matched with a target of the monitor by its address (web app, Front Door endpoint), within the environment of the
/// same name; a node the monitor does not check is drawn neutral, with the reason in words. The database takes no call
/// from a browser: it is drawn reachable when the health check of a web app that uses it passes, or, where no web app
/// uses it, when the health check of a container app that uses it passes. The numbers on the
/// relationships are the web apps' own counts of the last minute (<see cref="TelemetrySnapshot"/>); a dash where a web
/// app reports none. A container app's arrow to the database carries no count: the page does not ask it for telemetry. A web app's tile also shows its process's vitals and, next to the numbers, their trend over the
/// last checks; where the topology has a link for a number or a name, the payload carries it. Where a web app
/// answers its detailed health check (<see cref="HealthDetail"/>), its tile has one mark per entry, and a dependency of
/// its deployable (a node of the manifest outside the subscription) takes its state from the entry the manifest names.
/// A deployment in flight (<see cref="DeploymentMark"/>) marks every node of its deployable, by the deployable the
/// manifest names for the node; and what the system's deployments file says about that deployable in words
/// (<see cref="DeploymentActivity.OfNode"/>) follows the node's own lines. A container app's box has the page's own
/// probe of it (<see cref="ContainerAppProbe"/>) and, under its address, what the pipeline's hourly health report last
/// found there (<see cref="HourlyCheckText"/>). A box the browser cannot ask (a resource, and the database or a static
/// site where nothing else tells) says what Resource Health last said of it (<see cref="ResourceHealthText"/>), in
/// neutral words.
/// </summary>
public static class RuntimePayloadBuilder
{
    public const string Healthy = "healthy";
    public const string Unhealthy = "unhealthy";
    public const string Unreachable = "unreachable";
    public const string Checking = "checking";
    public const string Neutral = "neutral";

    /// <summary>The number line's placeholder where no web app reports its calls.</summary>
    public const string NoNumber = "–";
    public const string CallsUnit = "calls/min";

    /// <summary>The label of a neutral tile this page does not check.</summary>
    private const string NotProbed = "Not probed";

    /// <summary>The label of a neutral tile whose address is not known.</summary>
    private const string NoAddress = "No address";

    // The rest of this page's tile-label vocabulary, one name per word (#1672): a word is one word here, so a typo
    // at a call site cannot silently split the page's vocabulary. "Not known" (the checks say nothing about it) and
    // "Not confirmed" (an entry speaks well of it, but nothing vouches for the speaker) are different words and keep
    // separate names. Two labels the vocabulary might have needed are not literals of this file at all: "Degraded"
    // comes from the detailed health check's own words (HealthDetailText.Label) and "Available" from Resource Health
    // (ResourceHealthText.Label), each named in its own file. The lowercase words of a region mark (Region) and the
    // state words at the JSON boundary (RuntimeStateWords) are other vocabularies still, deliberately kept apart.

    /// <summary>The label of a neutral tile whose state the detailed health checks of the apps that use it do not tell: no entry of that name, no answer, or an entry that knows nothing.</summary>
    private const string NotKnown = "Not known";

    /// <summary>The label of a neutral tile that no passing health check confirms: the entry may be healthy while every app that reports it fails its own check, so neither side is called the cause.</summary>
    private const string NotConfirmed = "Not confirmed";

    /// <summary>The label of a neutral tile for an address the topology this page loaded does not name, so no check runs against it.</summary>
    private const string NotChecked = "Not checked";

    /// <summary>The label of the static site that serves this very dashboard.</summary>
    private const string ThisPage = "This page";

    /// <summary>The label of a tile waiting for the health checks that tell its state; the state word it rides with is the lowercase <see cref="Checking"/> at the JSON boundary, which is why this name carries the suffix.</summary>
    private const string CheckingLabel = "Checking";

    /// <summary>The label of a tile a passing health check reached; the browser itself calls neither the database nor a dependency.</summary>
    private const string Reachable = "Reachable";

    // The nouns naming which apps a tile waits for or was reached through (#1708): WaitingTile and PassedLine say the
    // same pair in two sentences, so each noun has one name here and a typo cannot split them.

    /// <summary>The apps a web app's health check speaks for, as a tile's words name them.</summary>
    private const string WebApps = "web apps";

    /// <summary>The apps a container app's probe speaks for, as a tile's words name them.</summary>
    private const string ContainerApps = "container apps";

    /// <summary>How many marks of a detailed health check a tile's line holds; the entries that are not healthy come first.</summary>
    public const int MostMarks = 8;

    /// <summary>How many characters of a health check's own words fit the line of a dependency's tile.</summary>
    public const int DescriptionLength = 36;

    /// <summary>How many characters of a health check entry's name fit the line of a dependency's tile, where no description stands in for it.</summary>
    private const int CheckNameLength = 14;

    /// <summary>The origin priority that serves first: the origin a Front Door edge says "first, while healthy" about.</summary>
    private const int FirstPriority = 1;

    /// <param name="Serves">True for a node the serving decision expects to serve: one node, or every healthy primary of a rotation.</param>
    private sealed record Entry(DeployableStatus Deployable, TargetStatus Target, ServingAssessment Assessment, bool Serves);

    /// <param name="manifest">The environment's manifest.</param>
    /// <param name="environment">The monitor's environment of the same name; null when the topology has none.</param>
    /// <param name="page">The dashboard's own address: the static site that serves it is "this page".</param>
    /// <param name="zone">The viewer's time zone, for the tooltips.</param>
    /// <param name="deployments">
    /// What is marked as being deployed in the environment, with <see cref="Deployables"/> as the deployables a mark
    /// may belong to; null without the file.
    /// </param>
    /// <param name="activity">
    /// The activity lines of a deployable in the environment (<see cref="DeploymentActivity.OfNode"/>), by its name;
    /// null without the file.
    /// </param>
    /// <param name="delivery">
    /// The delivery facts, with what the hourly health reports found for each container app; null without the file.
    /// </param>
    /// <param name="now">The moment of the update, for the age of a report; null leaves the reports out.</param>
    /// <param name="containerApps">What the page's probes saw of a container app; null before any probe.</param>
    /// <param name="resources">What Resource Health said of the system's resources; null without it.</param>
    public static RuntimePayload Build(
        RuntimeManifest manifest,
        EnvironmentStatus? environment,
        Uri? page,
        TimeZoneInfo zone,
        IReadOnlyList<DeploymentMark>? deployments = null,
        Func<string, IReadOnlyList<ActivityLine>>? activity = null,
        DeliveryReport? delivery = null,
        DateTimeOffset? now = null,
        Func<ContainerAppTarget, ContainerAppStatus?>? containerApps = null,
        ResourceHealthReport? resources = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(zone);
        var hourly = now is null ? null : delivery?.ContainerAppsOf(manifest.Environment);
        var health = new HealthSource(resources, now, zone);
        var probed = Probes(manifest, containerApps);
        var entries = Index(environment);
        var index = new RuntimeDiagramIndex<Entry>(manifest, node => Find(entries, node));

        var tiles = manifest.Nodes
            .Where(node => node.Kind != RuntimeNodeKind.Person)
            .Select(node => Named(
                Deploying(
                    node.Kind switch
                    {
                        RuntimeNodeKind.Sql => DatabaseTile(node, Clients(index, node, RuntimeEdgeKind.Sql), ContainerSqlApps(index, node, probed), zone, environment?.Info.Links, health),
                        RuntimeNodeKind.Dependency => DependencyTile(node, Clients(index, node, RuntimeEdgeKind.Dependency), zone),
                        RuntimeNodeKind.ContainerApp => ContainerAppTile(node, containerApps, hourly, now, zone, health),
                        RuntimeNodeKind.Resource => UnprobedNodeTile(node, health),
                        _ => Tile(node, index.Entry(node.Alias), environment, page, zone, health),
                    },
                    node,
                    deployments,
                    activity),
                node))
            .ToList();
        var reachable = manifest.Nodes
            .Where(node => node.Kind == RuntimeNodeKind.Sql && tiles.Any(tile => tile.Alias == node.Alias && tile.State == Healthy))
            .Select(node => node.RegionAlias)
            .ToHashSet(StringComparer.Ordinal);
        var regions = manifest.Regions
            .Select(region => Region(region, index, reachable.Contains(region.Alias)) with { NameLink = RuntimeBoxLink.Of(region) })
            .ToList();
        var edges = manifest.Edges.Select(edge => Edge(edge, index, probed)).ToList();
        return new RuntimePayload(tiles, regions, edges, Names(manifest));
    }

    /// <summary>
    /// The tile with where its node's name leads: the link of the topology where it has one (a web app, a Front Door
    /// endpoint, the database), and else the one the deployment wrote into the manifest (a static site, a node of an
    /// application with its own runtime).
    /// </summary>
    private static RuntimeTile Named(RuntimeTile tile, RuntimeNode node) =>
        tile.NameLink is not null ? tile : tile with { NameLink = RuntimeBoxLink.Of(node) };

    /// <summary>
    /// The boxes with a name and nothing else to update, each with its link: the frames that are no region, and the
    /// browser. Null when the manifest has a link for none (a diagram from before the links), so the payload is as
    /// it was.
    /// </summary>
    private static List<RuntimeName>? Names(RuntimeManifest manifest)
    {
        var names = new List<RuntimeName>();
        foreach (var node in manifest.Nodes.Where(node => node.Kind == RuntimeNodeKind.Person))
        {
            if (RuntimeBoxLink.Of(node) is { } link)
            {
                names.Add(new RuntimeName(node.Alias, link));
            }
        }

        foreach (var frame in manifest.Frames ?? [])
        {
            if (RuntimeBoxLink.Of(frame) is { } link)
            {
                names.Add(new RuntimeName(frame.Alias, link));
            }
        }

        return names.Count == 0 ? null : names;
    }

    /// <summary>
    /// The deployables the diagram draws a node of: those a deployment's mark is placed on, also where the topology
    /// does not list them (the dashboard's static site, a node an application recorded for itself).
    /// </summary>
    public static IReadOnlyList<string> Deployables(RuntimeManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return [.. manifest.Nodes.Where(Deploys).Select(node => node.Deployable!).Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// True for a node a deployment of its deployable changes. A dependency is none: it is what the deployable calls,
    /// and the system does not deploy it.
    /// </summary>
    private static bool Deploys(RuntimeNode node) =>
        node.Deployable is not null && node.Kind is not (RuntimeNodeKind.Person or RuntimeNodeKind.Dependency);

    /// <summary>
    /// The tile with the mark of what is being deployed to its node's deployable, and with the deployable's activity
    /// in words. Both are found by the deployable the manifest names for the node, not by what the monitor checks, so
    /// a node the topology does not have carries them too. The first mark (the one a person has to act on, then what
    /// is executing) gives the dot its shape and its link; the title has every one. The activity lines are the
    /// tile's last: a slot from before them (a diagram an older deployment rendered) has no row for them, and the
    /// script draws as many lines as a slot holds, so such a diagram loses them and nothing else.
    /// </summary>
    private static RuntimeTile Deploying(
        RuntimeTile tile,
        RuntimeNode node,
        IReadOnlyList<DeploymentMark>? deployments,
        Func<string, IReadOnlyList<ActivityLine>>? activity)
    {
        if (!Deploys(node))
        {
            return tile;
        }

        var marks = deployments?.Where(mark => string.Equals(mark.Deployable, node.Deployable, StringComparison.OrdinalIgnoreCase)).ToList() ?? [];
        if (marks.Count > 0)
        {
            var title = string.Join('\n', marks.Select(DeploymentText.Title));
            tile = tile with
            {
                Title = $"{tile.Title}\n{title}",
                Deployment = new RuntimeDeployment(
                    DeploymentText.Shape(marks[0].State),
                    title,
                    RuntimeLink.To(marks[0].Url, $"{title}\n{DeploymentText.TaskTitle}")),
            };
        }

        var lines = activity?.Invoke(node.Deployable!) ?? [];
        if (lines.Count == 0)
        {
            return tile;
        }

        // What is in flight is in the title already, with the marks; a freeze and the last deployment are not.
        var more = lines.Where(line => !ActivityKind.IsInFlight(line.Kind) || marks.Count == 0).Select(line => line.Title).ToList();
        return tile with
        {
            Title = more.Count == 0 ? tile.Title : $"{tile.Title}\n{string.Join('\n', more)}",
            Lines = [.. tile.Lines, .. lines.Select(ActivityLineOf)],
        };
    }

    /// <summary>
    /// An activity line as a line of the tile: its tone is its kind, and its words are a link to the task in Octopus
    /// Deploy where the file gives the address.
    /// </summary>
    private static RuntimeTileLine ActivityLineOf(ActivityLine line) =>
        RuntimeTileLine.Of(line.Kind, [new RuntimeTextPart(line.Text, RuntimeLink.To(line.Url, $"{line.Title}\n{DeploymentText.TaskTitle}"))]);

    /// <summary>The state word of a health state, as the payload carries it, one arm per state so a new state fails the build.</summary>
    public static string StateOf(HealthState state) => TileStateOf(state).Word();

    /// <summary>The tile state of a health state, one arm per state so a new state fails the build.</summary>
    public static TileState TileStateOf(HealthState state) => state switch
    {
        HealthState.Healthy => TileState.Healthy,
        HealthState.Unhealthy => TileState.Unhealthy,
        HealthState.Unreachable => TileState.Unreachable,
        HealthState.Pending => TileState.Checking,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    private static List<Entry> Index(EnvironmentStatus? environment)
    {
        var entries = new List<Entry>();
        if (environment is null)
        {
            return entries;
        }

        foreach (var deployable in environment.Deployables)
        {
            var assessment = deployable.AssessServing(out IReadOnlyList<TargetStatus> serving);
            entries.AddRange(deployable.Targets.Select(target => new Entry(deployable, target, assessment, serving.Contains(target))));
        }

        return entries;
    }

    /// <summary>The entry of the monitor target the node is (<see cref="TargetMatch"/>); null for a node that is none.</summary>
    private static Entry? Find(List<Entry> entries, RuntimeNode node) =>
        TargetMatch.Of(node) is { } match ? entries.FirstOrDefault(entry => match.Matches(entry.Target)) : null;

    /// <summary>Resource Health as the tiles take it: what it said of the resources a box stands for.</summary>
    private sealed record HealthSource(ResourceHealthReport? Report, DateTimeOffset? Now, TimeZoneInfo Zone)
    {
        /// <summary>
        /// The tile of a box from Resource Health, neutral, with why nothing else tells first in its tooltip; null for a
        /// box without ids (a manifest from before them).
        /// </summary>
        public RuntimeTile? Tile(RuntimeNode node, string why)
        {
            if (ResourceHealthText.Of(node.Ids, Report, Now, Zone) is not { } words)
            {
                return null;
            }

            var title = why.Length > 0 ? $"{node.Name}: {why}\n{words.Title}" : $"{node.Name}\n{words.Title}";
            return new RuntimeTile(node.Alias, TileState.Neutral.Word(), words.Label, null, [new RuntimeTileLine(words.Line, words.Tone)], null, title);
        }
    }

    private static RuntimeTile Tile(RuntimeNode node, Entry? entry, EnvironmentStatus? environment, Uri? page, TimeZoneInfo zone, HealthSource health)
    {
        // The site that serves this page says so, also where the topology lists it as a node: it answers, or nobody
        // would be reading this.
        if (node.Kind == RuntimeNodeKind.StaticSite && node.Url is not null && page is not null && SameSite(node.Url, page))
        {
            return NeutralTile(
                node,
                ThisPage,
                "serves this dashboard",
                $"{node.Name} ({node.Url.Host}) serves the page you are looking at.");
        }

        if (entry is not null)
        {
            return node.Kind == RuntimeNodeKind.FrontDoor ? FrontDoorTile(node, entry, zone) : WebAppTile(node, entry, environment!, zone);
        }

        return node.Kind switch
        {
            RuntimeNodeKind.StaticSite => UncheckedStaticSiteTile(node, health),
            RuntimeNodeKind.FrontDoor when node.Url is null => UndeployedFrontDoorTile(node),
            RuntimeNodeKind.WebApp or RuntimeNodeKind.FrontDoor => NotInTopologyTile(node),
            RuntimeNodeKind.Other when node.Url is { } url => OtherNodeTile(node, url),
            _ => UnprobedNodeTile(node, health),
        };
    }

    /// <summary>
    /// The static site of the environment that is not this page: what Resource Health said of it, or not probed with
    /// its host (or why it has none). One reason, said in the same words on both sides, with the host in brackets
    /// where the node has one.
    /// </summary>
    private static RuntimeTile UncheckedStaticSiteTile(RuntimeNode node, HealthSource health)
    {
        var why = $"the dashboard of this environment{(node.Url is null ? string.Empty : $" ({node.Url.Host})")}. This page does not check it.";
        return UnprobedUnder(node, health, why, node.Url is null ? "its address is not in this deployment" : node.Url.Host);
    }

    /// <summary>A Front Door endpoint the deployment found no address for: not deployed yet.</summary>
    private static RuntimeTile UndeployedFrontDoorTile(RuntimeNode node) =>
        NeutralTile(
            node,
            NoAddress,
            "endpoint not deployed yet",
            $"{node.Name}: the deployment found no such endpoint in the Front Door profile. Deploy the dashboard again once the environment has it.");

    /// <summary>A web app or Front Door endpoint whose address the loaded topology does not have: not checked.</summary>
    private static RuntimeTile NotInTopologyTile(RuntimeNode node) =>
        NeutralTile(
            node,
            NotChecked,
            "not in topology.json",
            $"{node.Name}: the topology this page loaded has no such address, so it is not checked. Reload the topology, or deploy the dashboard again.");

    /// <summary>A kind this page does not know, with an address: drawn with it, never probed.</summary>
    private static RuntimeTile OtherNodeTile(RuntimeNode node, Uri url) =>
        NotProbedTile(node, url.Host, $"{node.Name} ({url.Host}): the dashboard does not check it.");

    /// <summary>The container app a node is, as the page probes it; null for another node, and for one without an address.</summary>
    public static ContainerAppTarget? ContainerTarget(RuntimeNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Kind == RuntimeNodeKind.ContainerApp && node.Url is { } url
            ? new ContainerAppTarget(url, node.Deployable is null ? null : node.HealthPath)
            : null;
    }

    /// <summary>What the page probes and reads for a diagram: its container apps, and Resource Health where a box shows it.</summary>
    public static RuntimeWatch Watch(RuntimeManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return new RuntimeWatch(
            [.. manifest.Nodes.Select(ContainerTarget).OfType<ContainerAppTarget>().Distinct()],
            // Also where only the list of what is in no environment shows it.
            manifest.Nodes.Any(node => node.Ids is { Count: > 0 })
            || (manifest.Unplaced?.Any(entry => entry.Ids is { Count: > 0 }) ?? false));
    }

    /// <summary>The state word of a container app's probe, one arm per state so a new state fails the build: the
    /// colours of the other states, none of its own.</summary>
    public static string StateOf(ContainerAppState state) => TileStateOf(state).Word();

    /// <summary>The tile state of a container app's probe, one arm per state so a new state fails the build.</summary>
    public static TileState TileStateOf(ContainerAppState state) => state switch
    {
        ContainerAppState.Probing => TileState.Checking,
        ContainerAppState.Waking => TileState.Checking,
        ContainerAppState.Healthy => TileState.Healthy,
        ContainerAppState.Responding => TileState.Healthy,
        ContainerAppState.Unhealthy => TileState.Unhealthy,
        ContainerAppState.NotResponding => TileState.Unreachable,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    /// <summary>
    /// A container app: the page's own probe (<see cref="ContainerAppProbe"/>), once a minute while the page shows it,
    /// is its state; "Waking" is drawn as a check in progress. Under its address, where the delivery facts say what the
    /// pipeline's hourly health report found for an app <c>system.json</c> declares (the report asks no other), the
    /// result of the last two hours and when, or that there is none, and the counts of 24 hours and 7 days, in neutral
    /// words. An app without an address the browser can ask says what Resource Health said of it.
    /// </summary>
    /// <param name="hourly">What the reports found for the environment's container apps; null without it.</param>
    private static RuntimeTile ContainerAppTile(
        RuntimeNode node,
        Func<ContainerAppTarget, ContainerAppStatus?>? probes,
        IReadOnlyList<ContainerAppReports>? hourly,
        DateTimeOffset? now,
        TimeZoneInfo zone,
        HealthSource health)
    {
        if (ContainerTarget(node) is not { } target)
        {
            const string Why = "a container app without a public address this page knows: the browser cannot ask it.";
            return HealthOrNeutral(node, health, Why, NoAddress, "no public address known");
        }

        var status = probes?.Invoke(target) ?? ContainerAppStatus.NotYet;
        var lines = new List<RuntimeTileLine> { new(target.Url.Host, RuntimeLineTone.Muted) };
        var title = ContainerAppText.Title(node.Name, target, status, zone);
        if (node.Deployable is not null && hourly is not null && now is { } moment)
        {
            var reports = hourly.FirstOrDefault(entry => string.Equals(entry.Name, node.Deployable, StringComparison.OrdinalIgnoreCase));
            lines.Add(new RuntimeTileLine(HourlyCheckText.Result(reports, moment), HourlyCheckText.RecentResult(reports, moment) is null ? RuntimeLineTone.Muted : RuntimeLineTone.Plain));
            if (HourlyCheckText.Counts(reports) is { } counts)
            {
                lines.Add(new RuntimeTileLine(counts, RuntimeLineTone.Muted));
            }

            title = $"{title}\n{HourlyCheckText.Title(reports, moment, zone)}";
        }

        return new RuntimeTile(node.Alias, StateOf(status.State), ContainerAppText.Label(status.State), ContainerAppText.Facts(status), lines, null, title);
    }

    /// <summary>What the page's probes last saw of each container app, by the node's alias.</summary>
    private static Dictionary<string, ContainerAppStatus> Probes(RuntimeManifest manifest, Func<ContainerAppTarget, ContainerAppStatus?>? probes)
    {
        var seen = new Dictionary<string, ContainerAppStatus>(StringComparer.Ordinal);
        if (probes is null)
        {
            return seen;
        }

        foreach (var node in manifest.Nodes)
        {
            if (ContainerTarget(node) is { } target && probes(target) is { } status)
            {
                seen[node.Alias] = status;
            }
        }

        return seen;
    }

    /// <summary>A container app with a SQL relationship to the database, and the probe of it when the page has one.</summary>
    private readonly record struct ContainerSqlApp(RuntimeNode App, ContainerAppStatus? Status);

    /// <summary>The container apps with a SQL relationship to the database. A web app's relationship is not one of these.</summary>
    private static List<ContainerSqlApp> ContainerSqlApps(RuntimeDiagramIndex<Entry> index, RuntimeNode database, Dictionary<string, ContainerAppStatus> probed) =>
        [.. new RuntimeClients(index.Manifest, database, RuntimeEdgeKind.Sql)
            .Of(index.ContainerApp)
            .Select(app => new ContainerSqlApp(app, probed.GetValueOrDefault(app.Alias)))];

    /// <summary>The checked web apps with a relationship of that kind to the node (the database, a dependency).</summary>
    private static List<Entry> Clients(RuntimeDiagramIndex<Entry> index, RuntimeNode node, RuntimeEdgeKind kind) =>
        [.. new RuntimeClients(index.Manifest, node, kind).Of(index.Entry)];

    /// <summary>True when every client that reports was probed for liveness only, so none of them read what the node answers.</summary>
    private static bool OnlyProbesLiveness(List<Entry> clients) =>
        clients.All(entry => entry.Target.Last is { Probe: ProbeKind.Liveness });

    /// <summary>
    /// A dependency from the detailed health checks of the web apps that use it: the entry the manifest names says its
    /// state. Reachable when that entry is healthy on a web app that passes its health check; degraded or unhealthy as
    /// the entry says; and neutral, with the reason in words, whenever no web app tells (no entry named, no detailed
    /// health check, the probe Liveness, which reads none).
    /// </summary>
    private static RuntimeTile DependencyTile(RuntimeNode node, List<Entry> clients, TimeZoneInfo zone)
    {
        if (clients.Count == 0)
        {
            return NotProbedTile(node, "no web app that uses it is checked", $"{node.Name}: this page checks no web app that uses it.");
        }

        if (node.HealthCheck is not { } check)
        {
            return NotProbedTile(
                node,
                "no health check names it",
                $"{node.Name}: the system names no entry of the web apps' detailed health check for it (dependencies[].healthCheck in system.json), so this page cannot tell its state.");
        }

        if (OnlyProbesLiveness(clients))
        {
            return NotProbedTile(
                node,
                "probe Liveness leaves it alone",
                $"{node.Name}: the probe is Liveness, which does not read what the web apps' health checks found. Choose Health check to see its state.");
        }

        var found = clients
            .Select(entry => (entry.Target, Check: entry.Target.HealthDetail?.Find(check)))
            .Where(reading => reading.Check is not null)
            .Select(reading => (reading.Target, Check: reading.Check!))
            .ToList();
        if (found.Count == 0)
        {
            if (clients.Any(entry => entry.Target.State == HealthState.Pending))
            {
                return WaitingTile(node, WebApps);
            }

            return clients.Any(entry => entry.Target.HealthDetail is not null)
                ? NeutralTile(
                    node,
                    NotKnown,
                    $"no health check entry {HealthDetailText.Brief(check, CheckNameLength)}",
                    $"{node.Name}: the detailed health check of the web apps that use it has no entry named {check}, so this page cannot tell its state.")
                : NeutralTile(
                    node,
                    NotKnown,
                    "no detailed health check answers",
                    $"{node.Name}: no web app that uses it answers its detailed health check, where the entry {check} would tell its state.");
        }

        var worst = found.OrderBy(reading => HealthDetailText.Severity(reading.Check.State)).First();
        var passed = found.Where(reading => reading.Check.IsHealthy && reading.Target.Last is { State: HealthState.Healthy, Probe: ProbeKind.Health }).ToList();
        if (passed.Count > 0)
        {
            var (target, entry) = passed[0];
            var names = string.Join(", ", passed.Select(reading => reading.Target.Name));
            var reachable = DependencyReadTitle(node, "reachable, ", names, target.Last!.CheckedAt, zone, entry);

            // One web app reaches it and another says otherwise: reachable, and the line names the one that differs.
            if (!worst.Check.IsHealthy)
            {
                return ReachableTile(
                    node,
                    new RuntimeTileLine($"{worst.Target.Label} reports it {HealthDetailText.Label(worst.Check.State).ToLowerInvariant()}", RuntimeLineTone.Warn),
                    null,
                    $"{reachable} Not so for {worst.Target.Name}: {HealthDetailText.Title(worst.Check)}");
            }

            var line = entry.Description is { } description
                ? HealthDetailText.Brief(description, DescriptionLength)
                : $"health check {HealthDetailText.Brief(check, CheckNameLength)} passed";
            return ReachableTile(node, new RuntimeTileLine(line, RuntimeLineTone.Ok), null, reachable);
        }

        // The label is the health check's own word ("Degraded", "Unhealthy", ...), owned by HealthDetailText.Label.
        var label = HealthDetailText.Label(worst.Check.State);
        var words = worst.Check.Description is { } said ? HealthDetailText.Brief(said, DescriptionLength) : $"health check {HealthDetailText.Brief(check, CheckNameLength)}: {label.ToLowerInvariant()}";
        var title = DependencyReadTitle(node, string.Empty, worst.Target.Name, worst.Target.Last?.CheckedAt ?? worst.Target.HealthDetail!.ReadAt, zone, worst.Check);
        return worst.Check.State switch
        {
            CheckState.Unhealthy or CheckState.Degraded => new RuntimeTile(node.Alias, TileState.Unhealthy.Word(), label, null, [new RuntimeTileLine(words, RuntimeLineTone.Warn)], null, title),
            CheckState.Unknown => NeutralTile(node, NotKnown, words, title),
            _ => NeutralTile(
                node,
                NotConfirmed,
                "no web app that reports it passes",
                $"{node.Name}: the entry {check} is healthy, but no web app that reports it passes its own health check, so this page does not call it reachable."),
        };
    }

    /// <summary>
    /// The title of a dependency read from the detailed health check of the web apps that use it, written once for its
    /// reachable exit (<paramref name="verdict"/> "reachable, ") and its other readings (no verdict): who read it, when,
    /// that the browser does not call it, and the entry's own words (#1753).
    /// <para>
    /// Its words differ from the database's (<see cref="DatabaseReachedTitle"/>) on purpose, so do not merge the two:
    /// the database's says its health checks <em>connected to it</em> and names its kind ("The database takes no call
    /// from a browser"), because a passing check is the database answering a connection; a dependency's is only what
    /// one entry of a web app's detailed health check reports, so it says whose check told it and quotes that entry.
    /// </para>
    /// </summary>
    private static string DependencyReadTitle(RuntimeNode node, string verdict, string names, DateTimeOffset at, TimeZoneInfo zone, HealthCheckEntry entry) =>
        $"{node.Name}: {verdict}by the health check of {names} (last {TimeText.Clock(at, zone)}). The browser does not call it. {HealthDetailText.Title(entry)}";

    /// <summary>
    /// A node reached through the health checks of the apps that use it: healthy, labelled <see cref="Reachable"/>,
    /// with one line. The database's exits and the dependency's reachable exits all build it (#1749, #1753).
    /// </summary>
    private static RuntimeTile ReachableTile(RuntimeNode node, RuntimeTileLine line, string? facts, string title) =>
        new(node.Alias, TileState.Healthy.Word(), Reachable, facts, [line], null, title);

    /// <summary>
    /// One reading of an app that uses the database, as either client source projects it (#1752): what the title
    /// names it, what the tile's line calls it (a web app's label - its region or its name - and a container app's
    /// name), and when its health check last found the database healthy.
    /// </summary>
    private readonly record struct Reached(string Name, string Label, DateTimeOffset CheckedAt);

    /// <summary>
    /// The database from the health checks of the apps that connect to it: one that passes says the database answered.
    /// One that fails does not say it did not, since the app itself may be the cause; the liveness probe leaves the
    /// database alone. A web app's check is the one the health view reads. Where no web app uses the database, a
    /// container app's own probe does: its health path queries the database, and the page asks that path while the
    /// diagram is open. Where nothing tells, what Resource Health said of it.
    /// </summary>
    private static RuntimeTile DatabaseTile(RuntimeNode node, List<Entry> clients, List<ContainerSqlApp> containerApps, TimeZoneInfo zone, LinkSet? links, HealthSource health) =>
        DatabaseTile(node, clients, containerApps, zone, health) with { NameLink = RuntimeLink.To(links?[LinkSet.Database], LinkText.For(LinkSet.Database, node.Name)) };

    private static RuntimeTile DatabaseTile(RuntimeNode node, List<Entry> clients, List<ContainerSqlApp> containerApps, TimeZoneInfo zone, HealthSource health)
    {
        if (clients.Count == 0)
        {
            return containerApps.Count == 0
                ? DatabaseUnprobed(node, health)
                : DatabaseFromContainerApps(node, containerApps, zone, health);
        }

        var reached = clients
            .Where(entry => entry.Target.Last is { State: HealthState.Healthy, Probe: ProbeKind.Health })
            .Select(entry => new Reached(entry.Target.Name, entry.Target.Label, entry.Target.Last!.CheckedAt))
            .ToList();
        if (reached.Count > 0)
        {
            var queries = clients.Select(entry => entry.Target.Telemetry).OfType<TelemetrySnapshot>().ToList();
            return DatabaseReachedTile(
                node,
                reached,
                PassedLine(reached.Count, reached[0].Label, WebApps),
                queries.Count == 0 ? null : string.Create(CultureInfo.InvariantCulture, $"{queries.Sum(telemetry => telemetry.SqlOfTraffic)} queries/min"),
                queries,
                zone);
        }

        if (OnlyProbesLiveness(clients))
        {
            const string Why = "the probe is Liveness, which does not connect to the database. Choose Health check to see whether it answers.";
            return UnprobedUnder(node, health, Why, "probe Liveness leaves it alone");
        }

        if (clients.Any(entry => entry.Target.State == HealthState.Pending))
        {
            return WaitingTile(node, WebApps);
        }

        return DatabaseUnconfirmed(node, "web app", "web app");
    }

    private static RuntimeTile NeutralTile(RuntimeNode node, string label, string line, string title) =>
        new(node.Alias, TileState.Neutral.Word(), label, null, line.Length > 0 ? [new RuntimeTileLine(line, RuntimeLineTone.Muted)] : [], null, title);

    /// <summary>A node this page does not check: neutral, labelled <see cref="NotProbed"/>.</summary>
    private static RuntimeTile NotProbedTile(RuntimeNode node, string line, string title) =>
        NeutralTile(node, NotProbed, line, title);

    /// <summary>
    /// What Resource Health said of a node, or else a neutral tile under <paramref name="label"/> with
    /// <paramref name="line"/>, titled with the node's name and <paramref name="why"/> - the reason this page has just
    /// worked out for not checking it (#1709).
    /// </summary>
    private static RuntimeTile HealthOrNeutral(RuntimeNode node, HealthSource health, string why, string label, string line) =>
        health.Tile(node, why) ?? NeutralTile(node, label, line, $"{node.Name}: {why}");

    /// <summary>What Resource Health said of a node, or else not probed under <paramref name="why"/> (#1709).</summary>
    private static RuntimeTile UnprobedUnder(RuntimeNode node, HealthSource health, string why, string line) =>
        HealthOrNeutral(node, health, why, NotProbed, line);

    /// <summary>
    /// A node with nothing of its own to say: what Resource Health said of it, or not probed under its name. The
    /// reason-free case of <see cref="UnprobedUnder"/>, kept apart because its title is the bare name, with no reason.
    /// </summary>
    private static RuntimeTile UnprobedNodeTile(RuntimeNode node, HealthSource health) =>
        health.Tile(node, string.Empty) ?? NotProbedTile(node, string.Empty, node.Name);

    /// <summary>A tile still waiting for the health checks of the apps that use the node; <paramref name="users"/> names those apps (<see cref="WebApps"/>, <see cref="ContainerApps"/>).</summary>
    private static RuntimeTile WaitingTile(RuntimeNode node, string users) =>
        new(node.Alias, TileState.Checking.Word(), CheckingLabel, null, [new RuntimeTileLine("waiting for the health checks", RuntimeLineTone.Muted)], null, $"{node.Name}: waiting for the health checks of the {users} that use it.");

    /// <summary>No app this page checks uses the database.</summary>
    private static RuntimeTile DatabaseUnprobed(RuntimeNode node, HealthSource health)
    {
        const string Why = "the database takes no call from a browser, and this page checks no web app that uses it.";
        return UnprobedUnder(node, health, Why, "not probed from the browser");
    }

    /// <summary>
    /// The database no passing health check confirms: a failed check may be the app, not the database, so it is not
    /// called down. <paramref name="user"/> names the apps that use it ("web app", "container app");
    /// <paramref name="cause"/> names the other possible cause of a failed check ("web app", "app").
    /// </summary>
    private static RuntimeTile DatabaseUnconfirmed(RuntimeNode node, string user, string cause) =>
        NeutralTile(
            node,
            NotConfirmed,
            "no health check of its apps passes",
            $"{node.Name}: no {user} that uses it passes its health check, so this page cannot tell whether it answers: the database or the {cause} may be the cause.");

    /// <summary>
    /// The title of a reachable database, naming the apps whose health checks connected to it. Both the web apps'
    /// exit and the container apps' exit say it (<see cref="DatabaseTile"/>, <see cref="DatabaseFromContainerApps"/>),
    /// so it is written once here. <paramref name="queries"/> holds the web apps' telemetry snapshots (none on the
    /// container apps' exit); when an app tells its requests' queries from its background ones, the optional
    /// sentence of the web apps' queries per minute is appended after the first sentence's period (#1733).
    /// Its words differ from a dependency's (<see cref="DependencyReadTitle"/>) on purpose, so do not merge the two:
    /// a passing check here is the database answering a connection, so it says "connected to it" (#1753).
    /// </summary>
    private static string DatabaseReachedTitle(RuntimeNode node, string names, DateTimeOffset latest, TimeZoneInfo zone, IReadOnlyList<TelemetrySnapshot> queries) =>
        $"{node.Name}: reachable. The database takes no call from a browser; the health check of {names} connected to it (last {TimeText.Clock(latest, zone)})."
        + QueriesSentence(queries);

    /// <summary>
    /// The reachable database title's optional sentence of the web apps' queries per minute, said when a web app
    /// tells its requests' queries from its background ones: it stands after the first sentence's period and owns
    /// its own leading space and full stop, so no caller can run it into that period (#1754).
    /// </summary>
    private static string QueriesSentence(IReadOnlyList<TelemetrySnapshot> queries) =>
        queries.Any(telemetry => telemetry.SplitsSql)
            ? string.Create(CultureInfo.InvariantCulture, $" Queries per minute, counted by the web apps: {queries.Sum(telemetry => telemetry.SqlOfTraffic)} while handling requests, {queries.Sum(telemetry => telemetry.SqlBackground ?? 0)} in the background (mostly the message bus polling the database).")
            : string.Empty;

    /// <summary>
    /// The tile of a database a health check of its apps reached: healthy under the <see cref="Reachable"/> label, one
    /// ok line, and the title naming the apps (<see cref="DatabaseReachedTitle"/>). Written once over the readings of
    /// the apps that use it (#1752): the names the title speaks and the latest of their health checks are read off
    /// <paramref name="reached"/>, and the words each client source contributes stay with it - <paramref name="line"/>
    /// is the source's <see cref="PassedLine"/>, and only the web apps have <paramref name="facts"/> (their queries
    /// per minute, <paramref name="queries"/>).
    /// </summary>
    private static RuntimeTile DatabaseReachedTile(RuntimeNode node, IReadOnlyList<Reached> reached, string line, string? facts, IReadOnlyList<TelemetrySnapshot> queries, TimeZoneInfo zone)
    {
        var names = string.Join(", ", reached.Select(app => app.Name));
        var latest = reached.Max(app => app.CheckedAt);
        return ReachableTile(node, new RuntimeTileLine(line, RuntimeLineTone.Ok), facts, DatabaseReachedTitle(node, names, latest, zone, queries));
    }

    /// <summary>
    /// The database from the container apps that use it. A healthy health-path probe connected to it. An app that has
    /// not been asked yet, or is still waking, leaves the database waiting. An answer that is not a health check
    /// (Liveness, or an app that refuses this page's origin) leaves it alone. A failed check does not call the
    /// database down: the app itself may be the cause.
    /// </summary>
    private static RuntimeTile DatabaseFromContainerApps(RuntimeNode node, List<ContainerSqlApp> apps, TimeZoneInfo zone, HealthSource health)
    {
        var reached = apps
            .Where(app => app.Status is { State: ContainerAppState.Healthy, Last: not null })
            .Select(app => new Reached(app.App.Name, app.App.Name, app.Status!.Last!.CheckedAt))
            .ToList();
        if (reached.Count > 0)
        {
            return DatabaseReachedTile(node, reached, PassedLine(reached.Count, reached[0].Label, ContainerApps), null, [], zone);
        }

        if (apps.Any(app => app.Status is null or { State: ContainerAppState.Probing or ContainerAppState.Waking }))
        {
            return WaitingTile(node, ContainerApps);
        }

        if (apps.All(app => app.Status!.State == ContainerAppState.Responding))
        {
            var who = Plural.Noun(apps.Count, "the container app that uses it", "the container apps that use it");
            var why = $"{who} answered without a health check (the probe was Liveness, or the app does not allow this page's origin), so this page cannot tell whether the database answers.";
            return UnprobedUnder(node, health, why, "health check not read");
        }

        return DatabaseUnconfirmed(node, "container app", "app");
    }

    /// <summary>
    /// The line of a database a health check of its apps reached: one app is named, more than one are counted.
    /// </summary>
    private static string PassedLine(int count, string name, string apps) =>
        Plural.Noun(count, $"health check of {name} passed", string.Create(CultureInfo.InvariantCulture, $"health checks of {count} {apps} passed"));

    private static RuntimeTile WebAppTile(RuntimeNode node, Entry entry, EnvironmentStatus environment, TimeZoneInfo zone)
    {
        var target = entry.Target;
        var lines = new List<RuntimeTileLine> { VersionLine(target, entry.Deployable) };
        if (PinnedLine(environment.AssessVersions(entry.Deployable), target) is { } pinned)
        {
            lines.Add(pinned);
        }

        if (target.Telemetry is { } telemetry)
        {
            lines.AddRange(TelemetryLines(node, target, telemetry));
        }

        lines.Add(entry.Assessment.OnlyNode is null ? RoleLine(target, entry.Serves) : SingleNodeLine(target, entry.Serves));

        // Last, so a diagram from before this line (a slot of seven lines) loses it and nothing else.
        if (target.HealthDetail is { } detail)
        {
            lines.Add(ChecksLine(detail));
        }

        return Checked(node, target, lines, zone, HealthDetailText.Failed(target.HealthDetail)) with
        {
            Link = Link(target, LinkSet.LiveMetrics, node.Name),
            NameLink = Link(target, LinkSet.Portal, node.Name),
        };
    }

    /// <summary>
    /// What the node's health check found, as one line: a mark per entry (those that are not healthy first when there
    /// are more than the line holds) and the summary in words.
    /// </summary>
    private static RuntimeTileLine ChecksLine(HealthDetail detail)
    {
        var failing = detail.NotHealthy;
        var shown = detail.Entries.Count <= MostMarks
            ? detail.Entries
            : [.. failing.Concat(detail.Entries.Where(entry => entry.IsHealthy)).Take(MostMarks)];
        return new RuntimeTileLine(
            HealthDetailText.Summary(detail),
            failing.Count > 0 ? RuntimeLineTone.Warn : RuntimeLineTone.Plain,
            Marks: [.. shown.Select(entry => new RuntimeCheckMark(MarkOf(entry.State), HealthDetailText.Title(entry)))]);
    }

    /// <summary>The state word of a mark, as the payload carries it.</summary>
    public static string MarkOf(CheckState state) => state switch
    {
        CheckState.Healthy => "healthy",
        CheckState.Degraded => "degraded",
        CheckState.Unhealthy => "failed",
        _ => "unknown",
    };

    /// <summary>A node's role and whether the deployable's traffic goes through it.</summary>
    private static RuntimeTileLine RoleLine(TargetStatus target, bool serves)
    {
        var role = target.Role ?? (target.IsPrimary ? NodeInfo.PrimaryRole : NodeInfo.StandbyRole);
        return serves
            ? new RuntimeTileLine($"{role}: serves traffic", RuntimeLineTone.Serving)
            : target.State switch
            {
                HealthState.Healthy => new RuntimeTileLine($"{role}: ready, no traffic", RuntimeLineTone.Muted),
                HealthState.Pending => new RuntimeTileLine(role, RuntimeLineTone.Muted),
                _ => new RuntimeTileLine($"{role}: not serving", RuntimeLineTone.Plain),
            };
    }

    /// <summary>
    /// The same line for the only node of a deployable without a Front Door endpoint: no role, since nothing stands by.
    /// </summary>
    private static RuntimeTileLine SingleNodeLine(TargetStatus target, bool serves) =>
        serves ? new RuntimeTileLine("serves traffic", RuntimeLineTone.Serving)
        : target.State == HealthState.Pending ? new RuntimeTileLine("checking", RuntimeLineTone.Muted)
        : new RuntimeTileLine("not serving", RuntimeLineTone.Plain);

    private static RuntimeLink? Link(TargetStatus target, string key, string subject) =>
        RuntimeLink.To(target.Links[key], LinkText.For(key, subject));

    /// <summary>
    /// A web app's own numbers: its traffic of the last minute with the trend of the requests, what failed, and its
    /// process's vitals (an app that reports none gets the first two lines only).
    /// </summary>
    private static IEnumerable<RuntimeTileLine> TelemetryLines(RuntimeNode node, TargetStatus target, TelemetrySnapshot telemetry)
    {
        var requests = string.Create(CultureInfo.InvariantCulture, $"{telemetry.Requests} req/min");
        var traffic = new List<RuntimeTextPart> { new(requests, Link(target, LinkSet.Performance, node.Name)) };
        if (telemetry.P95Ms is { } p95)
        {
            traffic.Add(new RuntimeTextPart(string.Create(CultureInfo.InvariantCulture, $" · p95 {p95} ms")));
        }

        yield return RuntimeTileLine.Of(RuntimeLineTone.Plain, traffic, RuntimeTrend.Of(Trends.Requests(target)));

        var vitals = telemetry.Process;
        var failed = new List<RuntimeTextPart> { new(Errors(telemetry.Errors), Link(target, LinkSet.Failures, node.Name)) };
        if (vitals?.ExceptionsPerMinute is { } exceptions)
        {
            failed.Add(new RuntimeTextPart(string.Create(CultureInfo.InvariantCulture, $" · {exceptions} exceptions/min")));
        }

        yield return RuntimeTileLine.Of(telemetry.Errors > 0 || vitals?.ExceptionsPerMinute > 0 ? RuntimeLineTone.Warn : RuntimeLineTone.Plain, failed);

        if (vitals is null)
        {
            yield break;
        }

        var process = new List<string>();
        if (vitals.CpuPercent is { } cpu)
        {
            process.Add($"CPU {VitalsText.Cpu(cpu)}");
        }

        if ((vitals.WorkingSetMb ?? vitals.GcHeapMb) is { } memory)
        {
            process.Add(VitalsText.Memory(memory));
        }

        if (vitals.InFlight is { } inFlight)
        {
            process.Add(string.Create(CultureInfo.InvariantCulture, $"{inFlight} in flight"));
        }

        if (process.Count > 0)
        {
            yield return new RuntimeTileLine(string.Join(" · ", process), RuntimeLineTone.Plain, null, RuntimeTrend.Of(Trends.Cpu(target)));
        }

        if (vitals.UptimeSeconds is { } uptime)
        {
            // A process that started within the last minutes is worth a second look: a deployment, a crash, a scale-in.
            yield return new RuntimeTileLine(VitalsText.Uptime(uptime), vitals.RestartedRecently ? RuntimeLineTone.Warn : RuntimeLineTone.Muted);
        }
    }

    private static string Errors(int errors) => Plural.Of(errors, "error");

    private static RuntimeTile FrontDoorTile(RuntimeNode node, Entry entry, TimeZoneInfo zone)
    {
        var assessment = entry.Assessment;
        var lines = new List<RuntimeTileLine> { VersionLine(entry.Target, null) };
        lines.Add(assessment.State switch
        {
            ServingState.Primary when assessment.Rotation is { } rotation => new RuntimeTileLine(
                assessment.Serving.Count == rotation.Count
                    ? string.Create(CultureInfo.InvariantCulture, $"rotates over {rotation.Count} regions")
                    : string.Create(CultureInfo.InvariantCulture, $"rotates over {assessment.Serving.Count} of {rotation.Count} regions"),
                RuntimeLineTone.Plain),
            ServingState.Primary => new RuntimeTileLine($"routes to {assessment.Expected!.Label} (priority 1)", RuntimeLineTone.Plain),
            ServingState.FailedOver => new RuntimeTileLine($"routes to {assessment.Expected!.Label} (failed over)", RuntimeLineTone.Serving),
            ServingState.Down => new RuntimeTileLine("no healthy origin", RuntimeLineTone.Plain),
            ServingState.NoNodes => new RuntimeTileLine("no origins in the topology", RuntimeLineTone.Muted),
            _ => new RuntimeTileLine("origins being checked", RuntimeLineTone.Muted),
        });
        lines.Add(assessment.FrontDoor switch
        {
            FrontDoorAgreement.Agrees => new RuntimeTileLine("agrees with the web apps", RuntimeLineTone.Ok),
            FrontDoorAgreement.Disagrees => new RuntimeTileLine("disagrees with the web apps", RuntimeLineTone.Warn),
            _ => new RuntimeTileLine("being compared with the web apps", RuntimeLineTone.Muted),
        });
        return Checked(node, entry.Target, lines, zone, assessment.FrontDoorText) with
        {
            NameLink = Link(entry.Target, LinkSet.FrontDoor, entry.Deployable.Info.Name),
        };
    }

    private static RuntimeTile Checked(RuntimeNode node, TargetStatus target, List<RuntimeTileLine> lines, TimeZoneInfo zone, string? note = null)
    {
        var last = target.Last;
        var facts = last switch
        {
            null => "not checked yet",
            { StatusCode: null } => "no answer",
            _ => HistoryText.ResultFacts(last, " · "),
        };
        var title = new List<string> { $"{node.Name}: {HealthClassifier.Label(target.State)}", target.Url.AbsoluteUri };
        if (last is not null)
        {
            title.Add($"Last check {TimeText.Clock(last.CheckedAt, zone)}{(last.Detail is { } detail ? $": {detail}" : string.Empty)}");
        }

        title.Add(HistoryText.Describe(target.History));
        if (note is not null)
        {
            title.Add(note);
        }

        return new RuntimeTile(
            node.Alias,
            StateOf(target.State),
            HealthClassifier.Label(target.State),
            facts,
            lines,
            [.. target.History.Select(result => StateOf(result.State))],
            string.Join('\n', title));
    }

    private static string Number(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? NoNumber;

    /// <summary>
    /// The version a node runs. A web app's version leads to its release in Octopus Deploy, or else to the commit its
    /// build names for that version; a Front Door endpoint (no deployable given) answers with the version of whichever
    /// node served, so its version is no link.
    /// </summary>
    private static RuntimeTileLine VersionLine(TargetStatus target, DeployableStatus? deployable)
    {
        if (target.Version is not { } version)
        {
            return new RuntimeTileLine("version not known", RuntimeLineTone.Muted);
        }

        return RuntimeTileLine.Of(RuntimeLineTone.Strong, [new RuntimeTextPart("version "), new RuntimeTextPart(version, deployable is null ? null : VersionLink(deployable, version))]);
    }

    /// <summary>Where a version leads: the release in Octopus Deploy, or else the commit of the build with that version.</summary>
    public static RuntimeLink? VersionLink(DeployableStatus deployable, string version)
    {
        ArgumentNullException.ThrowIfNull(deployable);
        if (LinkText.Release(deployable.Info.ProjectUrl, version) is { } release)
        {
            return RuntimeLink.To(release, LinkText.ReleaseTitle(deployable.Info.Name, version));
        }

        return deployable.Build is { CommitUrl: { } commit, Commit: { } sha } build && string.Equals(build.Version, version, StringComparison.OrdinalIgnoreCase)
            ? RuntimeLink.To(commit, LinkText.CommitTitle(sha))
            : null;
    }

    /// <summary>This node's part of the comparison with the version pinned in Git; null when nothing is pinned for the environment.</summary>
    private static RuntimeTileLine? PinnedLine(VersionAssessment? versions, TargetStatus target)
    {
        if (versions is null)
        {
            return null;
        }

        var label = target.Label;
        bool Has(IReadOnlyList<NodeVersion> nodes) => nodes.Any(node => node.Label == label);
        return versions.State switch
        {
            VersionState.Pending => new RuntimeTileLine("reading the pinned version", RuntimeLineTone.Unknown),
            VersionState.NotDeployed => new RuntimeTileLine("no pinned version", RuntimeLineTone.Unknown),
            VersionState.PinnedUnknown => new RuntimeTileLine("pinned version not known", RuntimeLineTone.Unknown),
            _ when Has(versions.Differing) => new RuntimeTileLine($"differs from pinned {versions.Pinned}", RuntimeLineTone.Differs),
            _ when Has(versions.Matching) => new RuntimeTileLine($"pinned {versions.Pinned}: in sync", RuntimeLineTone.InSync),
            _ => new RuntimeTileLine($"pinned {versions.Pinned}: not compared", RuntimeLineTone.Unknown),
        };
    }

    private static RuntimeRegionMark Region(RuntimeRegion region, RuntimeDiagramIndex<Entry> index, bool databaseReachable)
    {
        var apps = index.WebAppsIn(region.Alias)
            .Select(node => index.Entry(node.Alias))
            .ToList();
        if (apps.Count == 0)
        {
            var others = region.Roles
                .Where(role => !(databaseReachable && role == RuntimeRegionRole.Data))
                .Select(RuntimeRegionRole.Words)
                .ToList();
            var parts = new List<string>();
            if (databaseReachable)
            {
                parts.Add("database: reachable");
            }

            if (others.Count > 0)
            {
                parts.Add($"{string.Join(", ", others)}: not probed");
            }

            return new RuntimeRegionMark(region.Alias, RegionState.Neutral.Word(), string.Join("; ", parts));
        }

        // A web app the topology does not have is left out; a region with none the topology has is not checked.
        var known = apps.OfType<Entry>().ToList();
        return known.Count == 0 ? new RuntimeRegionMark(region.Alias, RegionState.Neutral.Word(), "not checked") : Region(region.Alias, known);
    }

    private static RuntimeRegionMark Region(string alias, List<Entry> apps)
    {
        if (apps.Any(entry => entry.Serves))
        {
            return new RuntimeRegionMark(alias, RegionState.Serving.Word(), "serving traffic");
        }

        if (apps.All(entry => entry.Target.State == HealthState.Healthy))
        {
            return new RuntimeRegionMark(alias, RegionState.Standby.Word(), "standby: ready");
        }

        return apps.Any(entry => entry.Target.State == HealthState.Pending)
            ? new RuntimeRegionMark(alias, RegionState.Checking.Word(), "checking")
            : new RuntimeRegionMark(alias, RegionState.Down.Word(), "not serving");
    }

    private static RuntimeEdgeMark Edge(RuntimeEdge edge, RuntimeDiagramIndex<Entry> index, Dictionary<string, ContainerAppStatus> probed)
    {
        var from = index.Entry(edge.From);
        var to = index.Entry(edge.To);
        return edge.Kind switch
        {
            RuntimeEdgeKind.Origin => OriginEdge(edge, to),
            RuntimeEdgeKind.Sql => SqlEdge(edge, from, index, probed),
            RuntimeEdgeKind.Dependency => DependencyEdge(edge, from, index),
            RuntimeEdgeKind.Public => PublicEdge(edge, to ?? from, index),
            RuntimeEdgeKind.Dashboard when to is not null => DashboardEdge(edge, to),
            _ => UnknownEdge(edge),
        };
    }

    /// <summary>Front Door's arrow to one of its origins: its role in the origin group and the calls it forwarded.</summary>
    private static RuntimeEdgeMark OriginEdge(RuntimeEdge edge, Entry? to)
    {
        // In a rotation the origins of the primary nodes share the traffic: none of them is the first.
        var role = to is { Assessment.IsRotation: true, Target.IsPrimary: true } ? "in the rotation, while healthy"
            : edge.Priority == FirstPriority ? "first, while healthy"
            : "when priority 1 is down";
        var state = to is null ? EdgeState.Neutral : Carries(to);
        var telemetry = to?.Target.Telemetry;
        var text = telemetry is { FrontDoorProbes: > 0 } ? string.Create(CultureInfo.InvariantCulture, $"{role} · {telemetry.FrontDoorProbes} probes") : role;
        var counted = Counted(CallsWord, telemetry is null
            ? null
            : string.Create(CultureInfo.InvariantCulture, $"{telemetry.FromFrontDoor} requests forwarded by Front Door, {telemetry.FrontDoorProbes} Front Door health probes"));
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: Number(telemetry?.FromFrontDoor),
            Unit: CallsUnit,
            Text: text,
            Title: $"Front Door to {edge.To}, origin priority {edge.Priority?.ToString(CultureInfo.InvariantCulture) ?? "not known"}: {role}. {state.Sentence()} {counted}",
            Link: to is null || telemetry is null ? null : Link(to.Target, LinkSet.Performance, to.Target.Name),
            Trend: to is null ? null : RuntimeTrend.Of(Trends.FromFrontDoor(to.Target)));
    }

    /// <summary>A web app's (or a container app's) arrow to the database: the SQL commands the app counted.</summary>
    private static RuntimeEdgeMark SqlEdge(RuntimeEdge edge, Entry? from, RuntimeDiagramIndex<Entry> index, Dictionary<string, ContainerAppStatus> probed)
    {
        if (from is null && index.Node(edge.From) is { Kind: RuntimeNodeKind.ContainerApp } app)
        {
            return ContainerSqlEdge(edge, probed.GetValueOrDefault(app.Alias));
        }

        var state = CallerState(from);
        var telemetry = from?.Target.Telemetry;
        var latency = telemetry?.SqlP95Ms is { } p95 ? string.Create(CultureInfo.InvariantCulture, $", p95 {p95} ms") : string.Empty;
        var counted = Counted(QueriesWord, telemetry switch
        {
            null => null,
            { SplitsSql: true } => string.Create(CultureInfo.InvariantCulture, $"{telemetry.SqlRequests} SQL commands while handling requests (the number shown: what traffic causes) and {telemetry.SqlBackground} in the background (mostly the message bus polling the database), {telemetry.Sql} in all{latency}; its health checks query the database too"),
            _ => string.Create(CultureInfo.InvariantCulture, $"{telemetry.Sql} SQL commands{latency}; its health checks query the database too"),
        });
        // Short words: they stand under the number, in the narrow column between two frames of the diagram.
        const string Queries = "app queries";
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: Number(telemetry?.SqlOfTraffic),
            Unit: CallsUnit,
            Text: telemetry is { SplitsSql: true } ? string.Create(CultureInfo.InvariantCulture, $"{Queries} · {telemetry.SqlBackground} background") : Queries,
            Title: $"{edge.From} to the database. {state.Sentence()} {counted}",
            Link: from is null || telemetry is null ? null : Link(from.Target, LinkSet.Dependencies, from.Target.Name),
            Trend: from is null ? null : RuntimeTrend.Of(Trends.Sql(from.Target)));
    }

    /// <summary>A web app's arrow to an outside dependency: its outgoing HTTP calls, when they are this dependency's alone.</summary>
    private static RuntimeEdgeMark DependencyEdge(RuntimeEdge edge, Entry? from, RuntimeDiagramIndex<Entry> index)
    {
        // As to the database: a web app that is down calls nothing, and the dependency is not the reason.
        var state = CallerState(from);
        var telemetry = from?.Target.Telemetry;

        // The app counts its outgoing HTTP calls as one number: it is this dependency's only when the web app
        // has no other one.
        var only = index.DependenciesFrom(edge.From) == 1;
        var counted = only
            ? Counted(CallsWord, telemetry is null
                ? null
                : string.Create(CultureInfo.InvariantCulture, $"{telemetry.Http} outgoing HTTP calls, all of them (it has this one dependency)"))
            : PerMinute(CallsWord, "the web app counts its outgoing HTTP calls as one number, and it has more than one dependency.");
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: only ? Number(telemetry?.Http) : NoNumber,
            Unit: CallsUnit,
            Text: only ? "outgoing HTTP calls" : "not counted apart",
            Title: $"{edge.From} to {index.Node(edge.To)?.Name ?? edge.To}. {state.Sentence()} {counted}",
            Link: null,
            Trend: only && from is not null ? RuntimeTrend.Of(Trends.Http(from.Target)) : null);
    }

    /// <summary>The browser's arrow to a public address: whether it answers, and the calls it took.</summary>
    private static RuntimeEdgeMark PublicEdge(RuntimeEdge edge, Entry? target, RuntimeDiagramIndex<Entry> index)
    {
        var state = target is null ? EdgeState.Neutral : AnswerState(target);
        var (number, counted, trend) = PublicCalls(edge, index);
        var requests = number == NoNumber || target is null
            ? null
            : RuntimeLink.To(target.Deployable.Info.Links?[LinkSet.Logs], LinkText.For(LinkSet.Logs, target.Deployable.Info.Name));
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: number,
            Unit: CallsUnit,
            Text: null,
            Title: $"The browser to {edge.To}: {state.Sentence()} {counted}",
            Link: requests,
            Trend: RuntimeTrend.Of(trend));
    }

    /// <summary>
    /// The browser's arrow to a dashboard the topology lists as a node (another home of this page): whether it answers.
    /// </summary>
    private static RuntimeEdgeMark DashboardEdge(RuntimeEdge edge, Entry to)
    {
        var state = AnswerState(to);
        var words = state switch
        {
            EdgeState.Active => "It answers.",
            EdgeState.Checking => "Being checked.",
            _ => "Down: it does not answer.",
        };
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: null,
            Unit: null,
            Text: null,
            Title: $"The browser to {to.Target.Name}: {words}");
    }

    /// <summary>An arrow this page has nothing to say about: neutral, named by its two ends.</summary>
    private static RuntimeEdgeMark UnknownEdge(RuntimeEdge edge) =>
        new RuntimeEdgeMark(
            Id: edge.Id,
            State: EdgeState.Neutral.Word(),
            Number: null,
            Unit: null,
            Text: null,
            Title: $"{edge.From} to {edge.To}");

    /// <summary>
    /// The state of a caller's arrow (to the database, to a dependency): a caller that is down sends nothing, and the
    /// callee is not the reason, so the line is idle rather than down.
    /// </summary>
    private static EdgeState CallerState(Entry? from)
    {
        var carries = from is null ? EdgeState.Neutral : Carries(from);
        return carries == EdgeState.Down ? EdgeState.Idle : carries;
    }

    /// <summary>Whether an address the browser calls answers, by its health state.</summary>
    private static EdgeState AnswerState(Entry target) => target.Target.State switch
    {
        HealthState.Healthy => EdgeState.Active,
        HealthState.Pending => EdgeState.Checking,
        _ => EdgeState.Down,
    };

    /// <summary>
    /// The calls to a public address: for a Front Door endpoint, the sum of what its origins counted as forwarded by
    /// Front Door (no caching rule is set, so every call reaches an origin); for a web app's own address, its direct calls.
    /// </summary>
    private static (string Number, string Words, Trend? Trend) PublicCalls(RuntimeEdge edge, RuntimeDiagramIndex<Entry> index)
    {
        var origins = index.OriginsOf(edge.To)
            .Select(other => index.Entry(other.To)?.Target)
            .ToList();
        if (origins.Count > 0)
        {
            var counted = origins.Select(origin => origin?.Telemetry).OfType<TelemetrySnapshot>().ToList();
            return counted.Count == 0
                ? (NoNumber, PerMinute(CallsWord, "no origin reports them."), null)
                : (Number(counted.Sum(telemetry => telemetry.FromFrontDoor)), PerMinute(CallsWord, "the sum of what its origins counted from Front Door."), Trends.Sum(origins.OfType<TargetStatus>()));
        }

        var direct = index.Entry(edge.To)?.Target;
        return direct?.Telemetry is not { } own
            ? (NoNumber, ReportsNone(CallsWord), null)
            : (Number(own.Direct), PerMinute(CallsWord, "counted by the web app."), Trends.Direct(direct));
    }

    // The counting words of an edge's tooltip (#1734): the unit word opens a "per minute" sentence, and a number the
    // web app counted is framed as the last minute's. One home for each, so the edges cannot drift apart.
    private const string CallsWord = "Calls";
    private const string QueriesWord = "Queries";

    /// <summary>A sentence about the unit's rate: the unit word, "per minute", then the words.</summary>
    private static string PerMinute(string unit, string words) => $"{unit} per minute: {words}";

    /// <summary>The unit's sentence where the web app reports no number.</summary>
    private static string ReportsNone(string unit) => PerMinute(unit, "the web app reports none.");

    /// <summary>
    /// The tooltip sentence of a counted edge: the unit's "none" sentence where the web app reports no number, else
    /// the last minute's frame with the counted clause (given without its full stop).
    /// </summary>
    private static string Counted(string unit, string? counted) =>
        counted is null ? ReportsNone(unit) : $"Last minute, counted by the web app: {counted}.";

    /// <summary>
    /// A container app's arrow to the database. The page does not ask the app for its call counts, so the number is a
    /// dash. A healthy health check carries the arrow; an app that is down sends nothing, and the database is not why.
    /// </summary>
    private static RuntimeEdgeMark ContainerSqlEdge(RuntimeEdge edge, ContainerAppStatus? status)
    {
        var state = status is null ? EdgeState.Checking : SqlState(status.State);
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: NoNumber,
            Unit: CallsUnit,
            Text: null,
            Title: $"{edge.From} to the database. {state.Sentence()} {PerMinute(QueriesWord, "this page does not count a container app's SQL commands.")}");
    }

    /// <summary>The state of a container app's SQL arrow, one arm per state so a new state fails the build.</summary>
    private static EdgeState SqlState(ContainerAppState state) => state switch
    {
        ContainerAppState.Healthy => EdgeState.Active,
        ContainerAppState.Unhealthy => EdgeState.Idle,
        ContainerAppState.NotResponding => EdgeState.Idle,
        ContainerAppState.Probing => EdgeState.Checking,
        ContainerAppState.Waking => EdgeState.Checking,
        ContainerAppState.Responding => EdgeState.Neutral,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    /// <summary>Whether the traffic of a node's deployable goes through this node, by the serving decision.</summary>
    private static EdgeState Carries(Entry entry) =>
        entry.Serves ? EdgeState.Active
        : entry.Target.State == HealthState.Healthy ? EdgeState.Idle
        : entry.Target.State == HealthState.Pending ? EdgeState.Checking
        : EdgeState.Down;

    private static bool SameSite(Uri site, Uri page) =>
        string.Equals(site.Host, page.Host, StringComparison.OrdinalIgnoreCase) && site.Port == page.Port;
}
