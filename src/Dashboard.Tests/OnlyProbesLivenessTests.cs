namespace Dashboard.Tests;

/// <summary>
/// "Every app that reports was probed for liveness only" is one specification of <c>RuntimePayloadBuilder</c>
/// (<c>OnlyProbesLiveness</c>, #1728), asked by the database's tile and by a dependency's. These tests pin where it
/// stops holding - one client probed for health, or one not answered yet - and that the question is written once.
/// </summary>
public class OnlyProbesLivenessTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    private static RuntimeManifest Manifest() =>
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    private static EnvironmentStatus Uat() =>
        new(SampleTopology.Environments.Single(environment => environment.Name == "uat"));

    private static ProbeResult Answer(int status, ProbeKind probe) =>
        new ProbeResult(HealthClassifier.FromStatusCode(status), status, 40, Now, "2.4.21") with { Probe = probe };

    private static RuntimeTile Database(EnvironmentStatus uat) =>
        RuntimePayloadBuilder.Build(Manifest(), uat, Page, TimeZoneInfo.Utc).Nodes.Single(tile => tile.Alias == "sqldb");

    [Fact]
    public void OneWebAppProbedForHealthMeansTheLivenessLineIsNotSaid()
    {
        var uat = Uat();
        uat.Deployables[0].Nodes[0].Record(Answer(503, ProbeKind.Liveness));
        uat.Deployables[0].Nodes[1].Record(Answer(503, ProbeKind.Health));

        var database = Database(uat);

        Assert.Equal(("neutral", "Not confirmed"), (database.State, database.Label));
        Assert.Equal(new RuntimeTileLine("no health check of its apps passes", "muted"), Assert.Single(database.Lines));
    }

    [Fact]
    public void AWebAppNotAnsweredYetMeansTheLivenessLineIsNotSaid()
    {
        var uat = Uat();
        uat.Deployables[0].Nodes[0].Record(Answer(200, ProbeKind.Liveness));

        var database = Database(uat);

        Assert.Equal(("checking", "Checking"), (database.State, database.Label));
        Assert.Equal(new RuntimeTileLine("waiting for the health checks", "muted"), Assert.Single(database.Lines));
    }

    [Fact]
    public void TheLivenessQuestionIsWrittenOnceAndAskedByBothArms()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs")));

        Assert.Equal(1, Count(source, "Last is { Probe: ProbeKind.Liveness }"));
        Assert.Equal(2, Count(source, "if (OnlyProbesLiveness(clients))"));
    }

    private static int Count(string text, string part) =>
        (text.Length - text.Replace(part, string.Empty, StringComparison.Ordinal).Length) / part.Length;
}
