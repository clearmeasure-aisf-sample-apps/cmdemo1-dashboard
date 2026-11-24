using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Characterization tests for RuntimePayloadBuilder edge and region mark builders.
/// Step 1 of #534: pin the behavior before splitting the builder.
/// </summary>
public class RuntimePayloadEdgeRegionTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    private static RuntimeManifest Manifest(string environment) =>
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample($"{environment}.json")).Value!;

    private static EnvironmentStatus Environment(string name) =>
        new(SampleTopology.Environments.Single(environment => environment.Name == name));

    private static ProbeResult Answer(int status, string version = "2.4.21") =>
        new(HealthClassifier.FromStatusCode(status), status, 40, Now, version);

    private static readonly ProbeResult NoAnswer = new(HealthState.Unreachable, null, null, Now, null, "No answer within 10 s.");

    private static RuntimePayload Build(EnvironmentStatus? environment, string manifest = "uat") =>
        RuntimePayloadBuilder.Build(Manifest(manifest), environment, Page, TimeZoneInfo.Utc);

    private static RuntimePayload Build(EnvironmentStatus? environment, RuntimeManifest manifest) =>
        RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc);

    private static string Edge(RuntimePayload payload, string id) => payload.Edges.Single(edge => edge.Id == id).State;
    private static RuntimeEdgeMark EdgeMark(RuntimePayload payload, string id) => payload.Edges.Single(edge => edge.Id == id);
    private static string Region(RuntimePayload payload, string alias) => payload.Regions.Single(region => region.Alias == alias).State;
    private static RuntimeRegionMark RegionMark(RuntimePayload payload, string alias) => payload.Regions.Single(region => region.Alias == alias) with { NameLink = null };

    /// <summary>A healthy primary origin with Front Door probes in telemetry.</summary>
    [Fact]
    public void OriginEdgeWithFrontDoorProbesShowsProbeCount()
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(TelemetrySnapshots.Quiet(Now) with { FromFrontDoor = 10, FrontDoorProbes = 3 });
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat);
        var mark = EdgeMark(payload, "fd_ui-to-app_ui_primary");

        Assert.Equal("active", mark.State);
        Assert.Equal("10", mark.Number);
        Assert.Equal("first, while healthy · 3 probes", mark.Text);
        Assert.Contains("3 Front Door health probes", mark.Title);
    }

    /// <summary>A primary origin in a rotation (multiple healthy primaries at priority 1).</summary>
    [Fact]
    public void OriginEdgeInRotationSaysInTheRotation()
    {
        // Create a topology with two primaries at the same priority
        var topology = TopologyParser.Parse($$"""
            { "system": { "slug": "demo", "name": "Demo", "repository": "https://github.com/example/demo" },
              "environments": [ { "name": "uat", "tier": "nonprod",
                "deployables": [ { "name": "ui", "frontDoor": "https://fd-uat.example.net",
                  "nodes": [
                    { "name": "app-ui-west", "role": "primary", "url": "https://app-ui-west.example.net" },
                    { "name": "app-ui-east", "role": "primary", "url": "https://app-ui-east.example.net" }
                  ] } ] } ] }
            """).Topology!;
        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                new RuntimeNode("fd_ui", RuntimeNodeKind.FrontDoor, "fd-uat", new Uri("https://fd-uat.example.net"), "ui"),
                new RuntimeNode("app_ui_west", RuntimeNodeKind.WebApp, "app-ui-west", new Uri("https://app-ui-west.example.net"), "ui", "primary", "westus3", "region_west"),
                new RuntimeNode("app_ui_east", RuntimeNodeKind.WebApp, "app-ui-east", new Uri("https://app-ui-east.example.net"), "ui", "primary", "eastus2", "region_east"),
            ],
            [
                new RuntimeRegion("region_west", "westus3", new List<string> { "primary" }),
                new RuntimeRegion("region_east", "eastus2", new List<string> { "primary" }),
            ],
            [
                new RuntimeEdge("browser-to-fd_ui", "browser", "fd_ui", RuntimeEdgeKind.Public),
                new RuntimeEdge("fd_ui-to-app_ui_west", "fd_ui", "app_ui_west", RuntimeEdgeKind.Origin, 1),
                new RuntimeEdge("fd_ui-to-app_ui_east", "fd_ui", "app_ui_east", RuntimeEdgeKind.Origin, 1),
            ]);

        var uat = new EnvironmentStatus(topology.Environments[0]);
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat, manifest);
        var mark = EdgeMark(payload, "fd_ui-to-app_ui_east");

        Assert.Equal("active", mark.State);
        Assert.Contains("in the rotation", mark.Text);
    }

    /// <summary>A dependency edge with a single dependency.</summary>
    [Fact]
    public void DependencyEdgeWithSingleDependencyShowsHttpCalls()
    {
        var manifest = Manifest("uat");
        var mail = new RuntimeNode("dep_mail", RuntimeNodeKind.Dependency, "Mail", null, "Mail");
        var edges = manifest.Edges.ToList();
        edges.Add(new RuntimeEdge("app_ui_primary-to-dep_mail", "app_ui_primary", "dep_mail", RuntimeEdgeKind.Dependency));
        manifest = manifest with { Nodes = [.. manifest.Nodes, mail], Edges = edges };

        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(TelemetrySnapshots.WithHttp(10, 5, Now));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat, manifest);
        var mark = EdgeMark(payload, "app_ui_primary-to-dep_mail");

        Assert.Equal("active", mark.State);
        Assert.Equal("5", mark.Number);
        Assert.Equal("outgoing HTTP calls", mark.Text);
        Assert.Contains("5 outgoing HTTP calls", mark.Title);
    }

    /// <summary>A dependency edge when the web app has multiple dependencies.</summary>
    [Fact]
    public void DependencyEdgeWithMultipleDependenciesIsNotCountedApart()
    {
        var manifest = Manifest("uat");
        var mail = new RuntimeNode("dep_mail", RuntimeNodeKind.Dependency, "Mail", null, "Mail");
        var search = new RuntimeNode("dep_search", RuntimeNodeKind.Dependency, "Search", null, "Search");
        var edges = manifest.Edges.ToList();
        edges.Add(new RuntimeEdge("app_ui_primary-to-dep_mail", "app_ui_primary", "dep_mail", RuntimeEdgeKind.Dependency));
        edges.Add(new RuntimeEdge("app_ui_primary-to-dep_search", "app_ui_primary", "dep_search", RuntimeEdgeKind.Dependency));
        manifest = manifest with { Nodes = [.. manifest.Nodes, mail, search], Edges = edges };

        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(TelemetrySnapshots.WithHttp(10, 15, Now));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat, manifest);
        var mark = EdgeMark(payload, "app_ui_primary-to-dep_mail");

        Assert.Equal("active", mark.State);
        Assert.Equal(RuntimePayloadBuilder.NoNumber, mark.Number);
        Assert.Equal("not counted apart", mark.Text);
        Assert.Contains("it has more than one dependency", mark.Title);
    }

    /// <summary>A public edge to a Front Door endpoint with origins that report telemetry.</summary>
    [Fact]
    public void PublicEdgeToFrontDoorSumsOriginCalls()
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(TelemetrySnapshots.Traffic(10, Now));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));
        uat.Deployables[0].Nodes[1].RecordTelemetry(TelemetrySnapshots.Traffic(5, Now));

        var payload = Build(uat);
        var mark = EdgeMark(payload, "browser-to-fd_ui");

        Assert.Equal("active", mark.State);
        Assert.Equal("15", mark.Number);
        Assert.Contains("sum of what its origins counted from Front Door", mark.Title);
    }

    /// <summary>A public edge to a web app directly (no Front Door).</summary>
    [Fact]
    public void PublicEdgeToWebAppShowsDirectCalls()
    {
        var manifest = Manifest("uat");
        // Remove the Front Door edges and add a direct public edge
        var edges = manifest.Edges
            .Where(edge => edge.Id != "browser-to-fd_ui" && edge.Id != "fd_ui-to-app_ui_primary" && edge.Id != "fd_ui-to-app_ui_standby")
            .ToList();
        edges.Add(new RuntimeEdge("browser-to-app_ui_primary", "browser", "app_ui_primary", RuntimeEdgeKind.Public));
        manifest = manifest with { Edges = edges };

        var uat = Environment("uat");
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(TelemetrySnapshots.Quiet(Now) with { Direct = 10 });
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat, manifest);
        var mark = EdgeMark(payload, "browser-to-app_ui_primary");

        Assert.Equal("active", mark.State);
        Assert.Equal("10", mark.Number);
        Assert.Contains("counted by the web app", mark.Title);
    }

    /// <summary>A dashboard edge when the target node is not in the topology.</summary>
    [Fact]
    public void DashboardEdgeWithNullTargetIsNeutral()
    {
        var manifest = Manifest("uat");
        // Create a dashboard edge to a node not in the manifest
        var edges = manifest.Edges.ToList();
        edges.Add(new RuntimeEdge("browser-to-unknown_dashboard", "browser", "unknown_dashboard", RuntimeEdgeKind.Dashboard));
        manifest = manifest with { Edges = edges };

        var payload = Build(Environment("uat"), manifest);
        var mark = EdgeMark(payload, "browser-to-unknown_dashboard");

        Assert.Equal("neutral", mark.State);
        Assert.Contains("browser", mark.Title);
    }

    /// <summary>An edge of an unknown kind falls through to neutral.</summary>
    [Fact]
    public void UnknownEdgeKindIsNeutral()
    {
        var manifest = Manifest("uat");
        var unknownNode = new RuntimeNode("unknown", RuntimeNodeKind.Other, "Unknown");
        var edges = manifest.Edges.ToList();
        edges.Add(new RuntimeEdge("browser-to-unknown", "browser", "unknown", (RuntimeEdgeKind)99));
        manifest = manifest with { Nodes = [.. manifest.Nodes, unknownNode], Edges = edges };

        var payload = Build(Environment("uat"), manifest);
        var mark = EdgeMark(payload, "browser-to-unknown");

        Assert.Equal("neutral", mark.State);
        Assert.Contains("browser", mark.Title);
    }

    /// <summary>A region with no web apps but database reachable.</summary>
    [Fact]
    public void RegionWithNoWebAppsButReachableDatabase()
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat);
        // region_data has roles [data, static] but no web apps
        var mark = RegionMark(payload, "region_data");

        Assert.Equal("neutral", mark.State);
        Assert.Equal("database: reachable; static sites: not probed", mark.Label);
    }

    /// <summary>A region with no web apps and database not reachable.</summary>
    [Fact]
    public void RegionWithNoWebAppsAndUnreachableDatabase()
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(503));
        uat.Deployables[0].Nodes[1].Record(NoAnswer);

        var payload = Build(uat);
        var mark = RegionMark(payload, "region_data");

        Assert.Equal("neutral", mark.State);
        Assert.Equal("database, static sites: not probed", mark.Label);
    }

    /// <summary>A region whose web apps are not in the topology.</summary>
    [Fact]
    public void RegionWithWebAppsNotInTopology()
    {
        var manifest = Manifest("uat");
        // Replace the standby node with one not in the topology
        var nodes = manifest.Nodes
            .Where(node => node.Alias != "app_ui_standby")
            .ToList();
        nodes.Add(new RuntimeNode(
            "app_ui_standby_westeurope",
            RuntimeNodeKind.WebApp,
            "app-cmdemo2-uat-ui-westeurope",
            new Uri("https://app-cmdemo2-uat-ui-westeurope.azurewebsites.net"),
            "ui",
            "standby",
            "westeurope",
            "region_standby"));
        manifest = manifest with { Nodes = nodes };

        var payload = Build(Environment("uat"), manifest);
        var mark = RegionMark(payload, "region_standby");

        Assert.Equal("neutral", mark.State);
        Assert.Equal("not checked", mark.Label);
    }

    /// <summary>A region with mixed app states: some pending.</summary>
    [Fact]
    public void RegionWithPendingAppsIsChecking()
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        // Don't check the standby - it stays pending

        var payload = Build(uat);
        var mark = RegionMark(payload, "region_standby");

        Assert.Equal("checking", mark.State);
        Assert.Equal("checking", mark.Label);
    }

    /// <summary>A SQL edge for a container app.</summary>
    [Fact]
    public void SqlEdgeForContainerAppShowsNoNumber()
    {
        var manifest = Manifest("uat");
        var app = new RuntimeNode(
            "ca_scorecard",
            RuntimeNodeKind.ContainerApp,
            "ca-demo-uat-scorecard",
            new Uri("https://ca-demo-uat-scorecard.graybush-1.southcentralus.azurecontainerapps.io"),
            "scorecard");
        manifest = manifest with { Nodes = [.. manifest.Nodes, app] };
        // Add an edge from the container app to the database
        var edges = manifest.Edges.ToList();
        edges.Add(new RuntimeEdge("ca_scorecard-to-sqldb", "ca_scorecard", "sqldb", RuntimeEdgeKind.Sql));
        manifest = manifest with { Edges = edges };

        var payload = RuntimePayloadBuilder.Build(
            manifest,
            Environment("uat"),
            Page,
            TimeZoneInfo.Utc,
            containerApps: _ => new ContainerAppStatus(
                ContainerAppState.Healthy,
                new ContainerAppAnswer("/_healthcheck", true, 200, false, 85, Now)));

        var mark = EdgeMark(payload, "ca_scorecard-to-sqldb");

        Assert.Equal("active", mark.State);
        Assert.Equal(RuntimePayloadBuilder.NoNumber, mark.Number);
        Assert.Contains("does not count a container app's SQL commands", mark.Title);
    }

    /// <summary>A SQL edge with SplitsSql telemetry.</summary>
    [Fact]
    public void SqlEdgeWithSplitSqlShowsBackground()
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(TelemetrySnapshots.WithSplitSql(10, 20, 15, 5, Now));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat);
        var mark = EdgeMark(payload, "app_ui_primary-to-sqldb");

        Assert.Equal("active", mark.State);
        Assert.Equal("15", mark.Number);
        Assert.Equal("app queries · 5 background", mark.Text);
        Assert.Contains("15 SQL commands while handling requests", mark.Title);
    }

    /// <summary>A SQL edge when the web app is down (idle, not the database's fault).</summary>
    [Fact]
    public void SqlEdgeWithDownWebAppIsIdle()
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(503));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat);
        var mark = EdgeMark(payload, "app_ui_primary-to-sqldb");

        Assert.Equal("idle", mark.State);
    }

    /// <summary>A dependency edge when the web app is down (idle, not the dependency's fault).</summary>
    [Fact]
    public void DependencyEdgeWithDownWebAppIsIdle()
    {
        var manifest = Manifest("uat");
        var mail = new RuntimeNode("dep_mail", RuntimeNodeKind.Dependency, "Mail", null, "Mail");
        var edges = manifest.Edges.ToList();
        edges.Add(new RuntimeEdge("app_ui_primary-to-dep_mail", "app_ui_primary", "dep_mail", RuntimeEdgeKind.Dependency));
        manifest = manifest with { Nodes = [.. manifest.Nodes, mail], Edges = edges };

        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(503));
        uat.Deployables[0].Nodes[1].Record(Answer(200, "2.4.20"));

        var payload = Build(uat, manifest);
        var mark = EdgeMark(payload, "app_ui_primary-to-dep_mail");

        Assert.Equal("idle", mark.State);
    }
}
