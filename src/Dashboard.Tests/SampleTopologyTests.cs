namespace Dashboard.Tests;

/// <summary>
/// The shipped cmdemo1 sample: only the ui container app, never AISF or fworker, with a Health-view tile fed by
/// <c>/_healthcheck/detailed</c>.
/// </summary>
public class SampleTopologyTests
{
    private static Topology SampleTopology =>
        TopologyParser.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "topology.sample.json"))).Topology!;

    private static RuntimeManifest SampleManifest(string environment) =>
        RuntimeManifestParser.ParseManifest(RuntimeManifestParserTests.Sample($"{environment}.json")).Value!;

    [Fact]
    public void NoEnvironmentNamesAisfOrFworkerInTopologyRuntimeOrPages()
    {
        foreach (var environment in SampleTopology.Environments)
        {
            Assert.DoesNotContain(environment.Deployables, deployable => deployable.Name is "aisf" or "fworker");
            Assert.DoesNotContain(environment.Pages ?? [], page => page.Deployable is "aisf" or "fworker" || page.Name == "Factory");
            Assert.DoesNotContain(
                environment.Pages ?? [],
                page => page.Url.AbsoluteUri.Contains("-aisf", StringComparison.OrdinalIgnoreCase)
                    || page.Url.AbsoluteUri.Contains("-fworker", StringComparison.OrdinalIgnoreCase));
        }

        foreach (var environment in new[] { "tdd", "uat" })
        {
            var manifest = SampleManifest(environment);
            Assert.DoesNotContain(manifest.Nodes, node => node.Deployable is "aisf" or "fworker" || node.Alias.Contains("aisf", StringComparison.Ordinal));
            Assert.DoesNotContain(manifest.Edges, edge => edge.Id.Contains("aisf", StringComparison.Ordinal) || edge.To.Contains("aisf", StringComparison.Ordinal));
            Assert.DoesNotContain(RuntimeManifestParserTests.Sample($"{environment}.svg"), "aisf", StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheHealthViewHasAUiTileFedByTheDetailedHealthPathInEveryEnvironment()
    {
        Assert.All(SampleTopology.Environments, environment =>
        {
            var ui = Assert.Single(environment.Deployables);
            Assert.Equal("ui", ui.Name);
            Assert.Equal("/_healthcheck", ui.HealthPath);
            Assert.Equal("/_healthcheck/detailed", ui.HealthDetailPath);
            var node = Assert.Single(ui.Nodes);
            Assert.Equal($"ca-cmdemo1-{environment.Name}-ui", node.Name);
            Assert.Equal($"https://ca-cmdemo1-{environment.Name}-ui.example.southcentralus.azurecontainerapps.io/", node.Url.AbsoluteUri);
        });
    }

    [Fact]
    public void AnEmptyDeployablesListStillParsesSoOlderTopologiesKeepWorking()
    {
        var topology = TopologyParser.Parse("""{ "environments": [ { "name": "tdd", "deployables": [] }, { "name": "prod" } ] }""");

        Assert.True(topology.IsValid);
        Assert.All(topology.Topology!.Environments, environment => Assert.Empty(environment.Deployables));
    }
}
