using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// What the background SQL commands are (#2125): "in the background (mostly the message bus polling the database)"
/// has one home, <see cref="RuntimePayloadBuilder.BackgroundSql" />, and the app-to-database edge, the database tile
/// and the web-app tile's SQL tooltip all say it from there.
/// </summary>
public class RuntimeBackgroundSqlWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    private static RuntimeManifest Manifest() =>
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    private static ProbeResult Answer(int status) =>
        new(HealthClassifier.FromStatusCode(status), status, 40, Now, "2.4.21");

    private static RuntimePayload SplitSqlPayload()
    {
        var uat = new EnvironmentStatus(SampleTopology.Environments.Single(environment => environment.Name == "uat"));
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(TelemetrySnapshots.WithSplitSql(10, 20, 15, 5, Now));
        return RuntimePayloadBuilder.Build(Manifest(), uat, Page, TimeZoneInfo.Utc);
    }

    [Fact]
    public void TheBackgroundSqlWordsAreTheOnesTheTilesHaveAlwaysSaid() =>
        Assert.Equal("in the background (mostly the message bus polling the database)", RuntimePayloadBuilder.BackgroundSql);

    [Fact]
    public void TheDatabaseTileAndTheAppToDatabaseEdgeSayTheSharedBackgroundSqlWords()
    {
        var payload = SplitSqlPayload();

        var database = payload.Nodes.Single(tile => tile.Alias == "sqldb");
        var edge = payload.Edges.Single(mark => mark.Id == "app_ui_primary-to-sqldb");

        Assert.Contains($"5 {RuntimePayloadBuilder.BackgroundSql}.", database.Title, StringComparison.Ordinal);
        Assert.Contains($"5 {RuntimePayloadBuilder.BackgroundSql},", edge.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWebAppTileSaysTheBackgroundSqlWordsFromTheirOneHome()
    {
        var source = MarkupGuards.ComponentText("TargetTile");

        Assert.Contains("{RuntimePayloadBuilder.BackgroundSql}", source, StringComparison.Ordinal);
        Assert.DoesNotContain("mostly the message bus polling the database", source, StringComparison.Ordinal);
    }
}
