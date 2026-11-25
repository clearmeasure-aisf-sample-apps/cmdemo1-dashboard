using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The counting sentences of the edge tooltips (#1734): "the web app reports none" and "Last minute, counted by the
/// web app:" have one home in <c>RuntimePayloadBuilder</c>, and every counted edge (an origin, the database, a
/// dependency, a public address) still says them word for word.
/// </summary>
public class RuntimeEdgeCountingWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    private static RuntimeManifest Manifest() =>
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    private static EnvironmentStatus Uat() =>
        new(SampleTopology.Environments.Single(environment => environment.Name == "uat"));

    private static ProbeResult Answer(int status) =>
        new(HealthClassifier.FromStatusCode(status), status, 40, Now, "2.4.21");

    private static string Title(EnvironmentStatus uat, string id, RuntimeManifest? manifest = null) =>
        RuntimePayloadBuilder.Build(manifest ?? Manifest(), uat, Page, TimeZoneInfo.Utc).Edges.Single(edge => edge.Id == id).Title;

    private static EnvironmentStatus Counting(TelemetrySnapshot telemetry)
    {
        var uat = Uat();
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(telemetry);
        return uat;
    }

    [Fact]
    public void AnOriginSaysWhatTheWebAppCountedLastMinute()
    {
        var title = Title(Counting(TelemetrySnapshots.Quiet(Now) with { FromFrontDoor = 10, FrontDoorProbes = 3 }), "fd_ui-to-app_ui_primary");

        Assert.EndsWith(" Last minute, counted by the web app: 10 requests forwarded by Front Door, 3 Front Door health probes.", title, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDatabaseSaysWhatTheWebAppCountedLastMinute()
    {
        var whole = Title(Counting(TelemetrySnapshots.WithSql(10, 20, Now)), "app_ui_primary-to-sqldb");
        var split = Title(Counting(TelemetrySnapshots.WithSplitSql(10, 20, 15, 5, Now)), "app_ui_primary-to-sqldb");

        Assert.EndsWith(" Last minute, counted by the web app: 20 SQL commands; its health checks query the database too.", whole, StringComparison.Ordinal);
        Assert.EndsWith(
            " Last minute, counted by the web app: 15 SQL commands while handling requests (the number shown: what traffic causes) and 5 in the background (mostly the message bus polling the database), 20 in all; its health checks query the database too.",
            split,
            StringComparison.Ordinal);
    }

    [Fact]
    public void AnAppThatReportsNoneSaysSoInItsUnit()
    {
        var uat = Uat();

        Assert.EndsWith(" Calls per minute: the web app reports none.", Title(uat, "fd_ui-to-app_ui_primary"), StringComparison.Ordinal);
        Assert.EndsWith(" Queries per minute: the web app reports none.", Title(uat, "app_ui_primary-to-sqldb"), StringComparison.Ordinal);
    }

    [Fact]
    public void APublicAddressOfAWebAppSaysItsCountInTheCallsUnit()
    {
        var manifest = Manifest();
        var edges = manifest.Edges
            .Where(edge => edge.Id != "browser-to-fd_ui" && edge.Id != "fd_ui-to-app_ui_primary" && edge.Id != "fd_ui-to-app_ui_standby")
            .Append(new RuntimeEdge("browser-to-app_ui_primary", "browser", "app_ui_primary", RuntimeEdgeKind.Public))
            .ToList();
        manifest = manifest with { Edges = edges };

        var silent = Uat();
        silent.Deployables[0].Nodes[0].Record(Answer(200));

        Assert.EndsWith(" Calls per minute: counted by the web app.", Title(Counting(TelemetrySnapshots.Quiet(Now) with { Direct = 10 }), "browser-to-app_ui_primary", manifest), StringComparison.Ordinal);
        Assert.EndsWith(" Calls per minute: the web app reports none.", Title(silent, "browser-to-app_ui_primary", manifest), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCountingWordsAreWrittenOnce()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs")));

        Assert.Equal(1, Count(source, "the web app reports none"));
        Assert.Equal(1, Count(source, "Last minute, counted by the web app:"));
        Assert.Equal(1, Count(source, " per minute: {"));
        Assert.DoesNotMatch(@"""(Calls|Queries) per minute:", source);
    }

    private static int Count(string text, string part) =>
        (text.Length - text.Replace(part, string.Empty, StringComparison.Ordinal).Length) / part.Length;
}
