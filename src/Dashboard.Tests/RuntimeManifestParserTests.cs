namespace Dashboard.Tests;

public class RuntimeManifestParserTests
{
    internal static string Sample(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "runtime.sample", file));

    [Fact]
    public void TheSampleIndexListsTheEnvironmentsOfTheSampleTopology()
    {
        var index = RuntimeManifestParser.ParseIndex(Sample("index.json")).Value!;

        Assert.Equal(
            [new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg"), new RuntimeIndexEntry("uat", "uat.json", "uat.svg")],
            index.Environments);
        Assert.Equal("1.2026.8", index.PlantUml);
    }

    [Fact]
    public void TheSampleManifestOfUatNamesTheUiContainerAppAsTheOnlySqlUser()
    {
        var manifest = RuntimeManifestParser.ParseManifest(Sample("uat.json")).Value!;

        Assert.Equal("uat", manifest.Environment);
        Assert.Equal(
            ["browser", "ca_ui", "ca_aisf", "sqldb", "swa_dashboard"],
            manifest.Nodes.Select(node => node.Alias));
        Assert.Equal(
            [RuntimeNodeKind.Person, RuntimeNodeKind.ContainerApp, RuntimeNodeKind.ContainerApp, RuntimeNodeKind.Sql, RuntimeNodeKind.StaticSite],
            manifest.Nodes.Select(node => node.Kind));
        Assert.DoesNotContain(manifest.Nodes, node => node.Kind == RuntimeNodeKind.WebApp);
        Assert.DoesNotContain(manifest.Frames!, frame => frame.Kind == "plan");
        var ui = manifest.Nodes.Single(node => node.Alias == "ca_ui");
        Assert.Equal(
            new RuntimeNode("ca_ui", RuntimeNodeKind.ContainerApp, "ca-cmdemo1-uat-ui", new Uri("https://ca-cmdemo1-uat-ui.example.southcentralus.azurecontainerapps.io"), "ui", HealthPath: "/_healthcheck"),
            ui with { Links = null });
        Assert.Equal(ui.Url, ui.Links?["site"]);
        Assert.Null(manifest.Nodes.Single(node => node.Alias == "sqldb").Url);
        Assert.Equal(["region_data"], manifest.Regions.Select(region => region.Alias));
        Assert.Equal(["data", "static"], Assert.Single(manifest.Regions).Roles);
        Assert.Equal(
            ["browser-to-ca_ui", "browser-to-ca_aisf", "ca_ui-to-sqldb", "browser-to-swa_dashboard"],
            manifest.Edges.Select(edge => edge.Id));
        Assert.Equal(RuntimeEdgeKind.Sql, manifest.Edges.Single(edge => edge.Id == "ca_ui-to-sqldb").Kind);
        Assert.DoesNotContain(manifest.Edges, edge => edge.From == "ca_aisf" && edge.Kind == RuntimeEdgeKind.Sql);
        Assert.Equal(RuntimeEdgeKind.Other, manifest.Edges.Single(edge => edge.Id == "browser-to-ca_ui").Kind);
    }

    [Fact]
    public void EveryAliasOfTheSampleManifestsIsInItsSvg()
    {
        foreach (var environment in new[] { "tdd", "uat" })
        {
            var manifest = RuntimeManifestParser.ParseManifest(Sample($"{environment}.json")).Value!;
            var svg = Sample($"{environment}.svg");
            Assert.All(manifest.Nodes, node => Assert.Matches($"data-qualified-name=\"([a-z_]+\\.)*{node.Alias}\"", svg));
            Assert.All(manifest.Regions, region => Assert.Matches($"class=\"cluster\" data-qualified-name=\"([a-z_]+\\.)*{region.Alias}\"", svg));
        }
    }

    [Fact]
    public void AnUnknownKindIsKeptAsOther()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "nodes": [ { "alias": "queue", "kind": "servicebus" } ], "edges": [ { "id": "a-to-b", "from": "a", "to": "b", "kind": "amqp" } ] }
            """).Value!;

        Assert.Equal(RuntimeNodeKind.Other, Assert.Single(manifest.Nodes).Kind);
        Assert.Equal(RuntimeEdgeKind.Other, Assert.Single(manifest.Edges).Kind);
        Assert.Empty(manifest.Regions);
    }

    [Fact]
    public void AContainerAppIsAKindOfItsOwnWithItsAddressAndItsLink()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "nodes": [ { "alias": "ca_scorecard", "kind": "containerapp", "deployable": "scorecard",
                "name": "ca-demo-tdd-scorecard", "url": "https://ca-demo-tdd-scorecard.graybush-1.southcentralus.azurecontainerapps.io",
                "links": { "site": "https://ca-demo-tdd-scorecard.graybush-1.southcentralus.azurecontainerapps.io" } } ],
              "edges": [ { "id": "browser-to-ca_scorecard", "from": "browser", "to": "ca_scorecard", "kind": "direct" } ] }
            """).Value!;

        var node = Assert.Single(manifest.Nodes);
        Assert.Equal(RuntimeNodeKind.ContainerApp, node.Kind);
        Assert.Equal("scorecard", node.Deployable);
        Assert.Equal("ca-demo-tdd-scorecard.graybush-1.southcentralus.azurecontainerapps.io", node.Url?.Host);
        Assert.Equal(node.Url, node.Links?["site"]);
        Assert.Equal(RuntimeEdgeKind.Other, Assert.Single(manifest.Edges).Kind);
    }

    [Fact]
    public void AResourceTaggedWithSeveralEnvironmentsOrNoneIsReadAsABoxOfNoDeployable()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "nodes": [
                { "alias": "res_1", "kind": "resource", "name": "aks-demo", "type": "containerservice/managedClusters", "count": 1,
                  "names": [ "aks-demo" ], "group": "rg-demo-cluster" },
                { "alias": "res_2", "kind": "resource", "name": "stdemoold", "group": "rg-demo-nonprod", "environmentTag": "qa,staging" },
                { "alias": "cax_1", "kind": "containerapp", "name": "ca-factory-gateway", "environmentTag": null } ] }
            """);

        Assert.True(manifest.IsValid, string.Join(' ', manifest.Errors));
        Assert.All(manifest.Value!.Nodes, node => Assert.Equal(
            (node.Name == "ca-factory-gateway" ? RuntimeNodeKind.ContainerApp : RuntimeNodeKind.Resource, (string?)null),
            (node.Kind, node.Deployable)));
        Assert.Empty(RuntimePayloadBuilder.Deployables(manifest.Value));
    }

    [Fact]
    public void AContainerAppsHealthPathAndABoxsResourceIdsAreRead()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "nodes": [
                { "alias": "ca_ui", "kind": "containerapp", "deployable": "ui", "name": "ca-demo-tdd-ui", "url": "https://ca-demo-tdd-ui.example.io",
                  "healthPath": "/_healthcheck", "ids": [ "/subscriptions/s/resourceGroups/g/providers/Microsoft.App/containerApps/ca-demo-tdd-ui" ] },
                { "alias": "res_1", "kind": "resource", "name": "Managed identity (2)", "ids": [ " /a ", "", 7, "/b" ] },
                { "alias": "res_2", "kind": "resource", "name": "kv-demo", "ids": [] } ] }
            """).Value!;

        Assert.Equal("/_healthcheck", manifest.Nodes[0].HealthPath);
        Assert.Equal(["/subscriptions/s/resourceGroups/g/providers/Microsoft.App/containerApps/ca-demo-tdd-ui"], manifest.Nodes[0].Ids!);
        Assert.Equal(["/a", "/b"], manifest.Nodes[1].Ids!);
        Assert.Null(manifest.Nodes[2].Ids);
        Assert.Null(manifest.Nodes[1].HealthPath);
    }

    [Theory]
    [InlineData("""https://evil.example/x""")]
    [InlineData("""//evil.example/x""")]
    [InlineData("""health""")]
    [InlineData("""/health?x=<script>""")]
    public void AHealthPathThatIsNoPlainPathIsLeftOutAndTheManifestIsRead(string path)
    {
        var manifest = RuntimeManifestParser.ParseManifest($$"""
            { "environment": "tdd", "nodes": [ { "alias": "ca_ui", "kind": "containerapp", "deployable": "ui", "url": "https://ca.example.io", "healthPath": "{{path}}" } ] }
            """);

        Assert.True(manifest.IsValid, string.Join(' ', manifest.Errors));
        Assert.Null(Assert.Single(manifest.Value!.Nodes).HealthPath);
    }

    [Theory]
    [InlineData("", "The file is empty.")]
    [InlineData("{", "The file is not valid JSON (line 1, position 2).")]
    [InlineData("[]", "The file must contain a JSON object.")]
    [InlineData("""{ "nodes": [] }""", "environment: missing or empty.")]
    [InlineData("""{ "environment": "tdd", "nodes": [ { "kind": "webapp" } ] }""", "nodes[0].alias: missing or empty.")]
    [InlineData("""{ "environment": "tdd", "nodes": [ { "alias": "a", "url": "ftp://x" } ] }""", "nodes[0].url: not an absolute http or https address.")]
    [InlineData("""{ "environment": "tdd", "edges": [ { "id": "a-to-b", "from": "a" } ] }""", "edges[0]: id, from and to are required.")]
    [InlineData("""{ "environment": "tdd", "regions": {} }""", "regions: not an array.")]
    public void ABrokenManifestIsNotRead(string json, string error)
    {
        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Contains(error, result.Errors);
    }

    [Theory]
    [InlineData("""{ }""", "environments: missing or not an array.")]
    [InlineData("""{ "environments": [ { } ] }""", "environments[0].name: missing or empty.")]
    [InlineData("""{ "environments": [ { "name": "tdd", "svg": "../secret.svg" } ] }""", "environments[0].svg: not a file name in runtime/.")]
    [InlineData("""{ "environments": [ { "name": "tdd", "manifest": "https://example.net/tdd.json" } ] }""", "environments[0].manifest: not a file name in runtime/.")]
    public void ABrokenIndexIsNotRead(string json, string error)
    {
        var result = RuntimeManifestParser.ParseIndex(json);

        Assert.False(result.IsValid);
        Assert.Contains(error, result.Errors);
    }

    [Fact]
    public void AnIndexEntryWithoutFilesUsesTheEnvironmentsName()
    {
        var index = RuntimeManifestParser.ParseIndex("""{ "environments": [ { "name": "prod" } ] }""").Value!;

        Assert.Equal(new RuntimeIndexEntry("prod", "prod.json", "prod.svg"), Assert.Single(index.Environments));
        Assert.Null(index.PlantUml);
    }
}
