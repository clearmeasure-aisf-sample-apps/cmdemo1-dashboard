using System.Text.Json;
using Dashboard.Health;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// Every box of the runtime diagram leads somewhere: the frames, the regions and the nodes the topology has no link
/// for take theirs from the manifest, and the payload carries each as the link of the box's name.
/// </summary>
public class RuntimeBoxLinkTests
{
    private const string Portal = "https://portal.azure.com/#@00000000-0000-0000-0000-000000000000/resource/subscriptions/00000000-0000-0000-0000-000000000000";

    private static RuntimeManifest Sample(string environment) =>
        RuntimeManifestParser.ParseManifest(RuntimeManifestParserTests.Sample($"{environment}.json")).Value!;

    private static RuntimePayload Payload(RuntimeManifest manifest) =>
        RuntimePayloadBuilder.Build(manifest, null, null, TimeZoneInfo.Utc);

    [Fact]
    public void TheSampleManifestListsEveryFrameThatIsNoRegionWithItsLink()
    {
        var manifest = Sample("uat");
        var frames = manifest.Frames!;

        Assert.Equal(
            [("sub", "subscription"), ("rg_tier", "resourceGroup"), ("cae", "containerAppsEnvironment")],
            frames.Select(frame => (frame.Alias, frame.Kind)));
        Assert.DoesNotContain(frames, frame => frame.Kind == "plan");
        Assert.Equal($"{Portal}/overview", frames[0].Links![LinkSet.Portal]!.AbsoluteUri);
        Assert.Equal($"{Portal}/resourceGroups/rg-cmdemo1-nonprod/overview", frames[1].Links![LinkSet.Portal]!.AbsoluteUri);
        Assert.Equal(
            $"{Portal}/resourceGroups/rg-cmdemo1-nonprod/providers/Microsoft.App/managedEnvironments/cae-cmdemo1-uat/overview",
            frames[2].Links![LinkSet.Portal]!.AbsoluteUri);
        Assert.All(manifest.Regions, region => Assert.Contains("/blade/HubsExtension/ArgQueryBlade/query/", region.Links![RuntimeBoxLink.Resources]!.AbsoluteUri, StringComparison.Ordinal));
        Assert.Contains(Uri.EscapeDataString("location =~ \"centralus\""), manifest.Regions[0].Links![RuntimeBoxLink.Resources]!.AbsoluteUri, StringComparison.Ordinal);
        Assert.Equal("https://ca-cmdemo1-uat-ui.example.southcentralus.azurecontainerapps.io/", manifest.Nodes[0].Links![RuntimeBoxLink.Site]!.AbsoluteUri);
        Assert.Equal(
            $"{Portal}/resourceGroups/rg-cmdemo1-nonprod/providers/Microsoft.Web/staticSites/swa-cmdemo1-uat-dashboard/overview",
            manifest.Nodes.Single(node => node.Kind == RuntimeNodeKind.StaticSite).Links![LinkSet.Portal]!.AbsoluteUri);
    }

    [Fact]
    public void EveryFrameOfTheSampleManifestsIsInItsSvg()
    {
        foreach (var environment in new[] { "tdd", "uat" })
        {
            var svg = RuntimeManifestParserTests.Sample($"{environment}.svg");
            Assert.All(Sample(environment).Frames!, frame => Assert.Matches($"class=\"cluster\" data-qualified-name=\"([a-z_]+\\.)*{frame.Alias}\"", svg));
        }
    }

    [Fact]
    public void EveryBoxOfTheSampleDiagramsHasALink()
    {
        foreach (var environment in new[] { "tdd", "uat" })
        {
            var manifest = Sample(environment);
            // The sample topology gives the web apps, the Front Door endpoint and the database theirs.
            var monitor = RuntimeFixtureLinks.Linked(manifest);
            var payload = Payload(manifest);

            Assert.Equal(
                manifest.Frames!.Select(frame => frame.Alias).Prepend("browser"),
                payload.Names!.Select(name => name.Alias));
            Assert.All(payload.Regions, region => Assert.NotNull(region.NameLink));
            Assert.All(payload.Nodes.Where(tile => !monitor.Contains(tile.Alias)), tile => Assert.NotNull(tile.NameLink));
        }
    }

    [Fact]
    public void TheTitleOfABoxSaysWhatItIsAndWhereItLeads()
    {
        var payload = Payload(Sample("uat"));
        var names = payload.Names!.ToDictionary(name => name.Alias, name => name.NameLink.Title);

        Assert.Equal("Browser: ca-cmdemo1-uat-ui.example.southcentralus.azurecontainerapps.io, as a browser opens it (opens in a new tab)", names["browser"]);
        Assert.Equal(LinkText.Portal("The Azure subscription in the Azure portal"), names["sub"]);
        Assert.Equal(LinkText.Portal("The resource group rg-cmdemo1-nonprod in the Azure portal"), names["rg_tier"]);
        Assert.Equal(LinkText.Portal("cae-cmdemo1-uat in the Azure portal"), names["cae"]);
        Assert.DoesNotContain(names, name => name.Value.Contains("App Service", StringComparison.Ordinal));
        Assert.Equal(
            LinkText.Portal("The resources in centralus, a query in Azure Resource Graph Explorer"),
            payload.Regions.Single(region => region.Alias == "region_data").NameLink!.Title);
        Assert.Equal(
            LinkText.Portal("The Static Web App swa-cmdemo1-uat-dashboard in the Azure portal"),
            payload.Nodes.Single(tile => tile.Alias == "swa_dashboard").NameLink!.Title);
    }

    [Fact]
    public void AClusterAndAnApplicationWithItsOwnRuntimeLeadToTheirPlaces()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            {
              "environment": "tdd",
              "nodes": [
                { "alias": "sqldb", "kind": "sql", "name": "demo-tdd/db", "links": { "workloads": "https://portal.example.net/#aks/workloads" } },
                { "alias": "app_web_1", "kind": "webapp", "name": "ca-web", "url": "https://ca-web.example.io", "links": { "site": "https://ca-web.example.io" } }
              ],
              "regions": [ { "alias": "region_primary", "name": "demo-tdd", "links": { "namespaces": "https://portal.example.net/#aks/namespaces" } } ],
              "frames": [
                { "alias": "cluster", "kind": "cluster", "name": "aks-demo", "links": { "portal": "https://portal.example.net/#aks/overview" } },
                { "alias": "own_web", "kind": "runtime", "name": "web", "links": { "project": "https://octopus.example.net/app#/Spaces-1/projects/demo-web" } },
                { "alias": "later", "kind": "something new", "name": "later", "links": { "portal": "https://portal.example.net/#later" } }
              ]
            }
            """).Value!;
        var payload = Payload(manifest);

        Assert.Equal(
            [
                ("cluster", "https://portal.example.net/#aks/overview", LinkText.Portal("The AKS cluster aks-demo in the Azure portal")),
                ("own_web", "https://octopus.example.net/app#/Spaces-1/projects/demo-web", $"The project that deploys web in Octopus Deploy{LinkText.OctopusSuffix}"),
                ("later", "https://portal.example.net/#later", LinkText.Portal("later in the Azure portal")),
            ],
            payload.Names!.Select(name => (name.Alias, name.NameLink.Href, name.NameLink.Title)));
        Assert.Equal(
            LinkText.Portal("The namespaces of the cluster in the Azure portal: demo-tdd is one of them"),
            payload.Regions[0].NameLink!.Title);
        Assert.Equal(
            LinkText.Portal("The workloads of the cluster in the Azure portal: demo-tdd/db is one of them"),
            payload.Nodes.Single(tile => tile.Alias == "sqldb").NameLink!.Title);
        Assert.Equal(
            ("https://ca-web.example.io/", "ca-web: ca-web.example.io, as a browser opens it (opens in a new tab)"),
            (payload.Nodes.Single(tile => tile.Alias == "app_web_1").NameLink!.Href, payload.Nodes.Single(tile => tile.Alias == "app_web_1").NameLink!.Title));
    }

    [Fact]
    public void ABoxWithSeveralLinksLeadsToItsResourceFirst()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "frames": [ { "alias": "own_web", "kind": "runtime", "name": "web",
              "links": { "site": "https://web.example.net", "project": "https://octopus.example.net/p", "portal": "https://portal.example.net/#web" } } ] }
            """).Value!;

        Assert.Equal("https://portal.example.net/#web", Assert.Single(Payload(manifest).Names!).NameLink.Href);
    }

    [Fact]
    public void OnlyAnHttpsAddressIsALinkAndAnythingElseIsLeftOutWithoutAnError()
    {
        var result = RuntimeManifestParser.ParseManifest("""
            {
              "environment": "tdd",
              "nodes": [ { "alias": "browser", "kind": "person", "links": { "site": "javascript:alert(1)" } } ],
              "regions": [ { "alias": "region_primary", "links": { "resources": "http://portal.example.net/plain" } } ],
              "frames": [
                { "alias": "sub", "kind": "subscription", "links": { "portal": 7 } },
                { "alias": "rg_tier", "kind": "resourceGroup", "links": "https://portal.example.net" },
                { "alias": "afd", "kind": "frontDoorProfile", "links": { "portal": "/relative" } },
                { "alias": "plan_primary", "kind": "plan" }
              ]
            }
            """);

        Assert.Empty(result.Errors);
        var manifest = result.Value!;
        Assert.Null(manifest.Nodes[0].Links);
        Assert.Null(manifest.Regions[0].Links);
        Assert.Equal(["sub", "rg_tier", "afd", "plan_primary"], manifest.Frames!.Select(frame => frame.Alias));
        Assert.All(manifest.Frames!, frame => Assert.Null(frame.Links));
        var payload = Payload(manifest);
        Assert.Null(payload.Names);
        Assert.Null(payload.Regions[0].NameLink);
    }

    [Fact]
    public void AFrameWithoutAnAliasIsAnErrorLikeARegionWithoutOne()
    {
        var result = RuntimeManifestParser.ParseManifest("""{ "environment": "tdd", "frames": [ { "kind": "plan" } ] }""");

        Assert.Equal(["frames[0].alias: missing or empty."], result.Errors);
    }

    [Fact]
    public void AManifestFromBeforeTheLinksGivesThePayloadItGaveBefore()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            {
              "environment": "tdd",
              "nodes": [ { "alias": "browser", "kind": "person" }, { "alias": "swa_dashboard", "kind": "staticsite", "name": "swa-demo-tdd-dashboard" } ],
              "regions": [ { "alias": "region_static", "name": "centralus", "roles": [ "static" ] } ]
            }
            """).Value!;
        var payload = Payload(manifest);

        Assert.Empty(manifest.Frames!);
        Assert.Null(payload.Names);
        Assert.Null(payload.Regions[0].NameLink);
        Assert.Null(payload.Nodes[0].NameLink);
        using var json = JsonDocument.Parse(payload.ToJson());
        Assert.Equal(["nodes", "regions", "edges"], json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["alias", "state", "label"], json.RootElement.GetProperty("regions")[0].EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public void ThePayloadCarriesTheNamesAsTheScriptReadsThem()
    {
        using var json = JsonDocument.Parse(Payload(Sample("tdd")).ToJson());
        var root = json.RootElement;

        Assert.Equal(["nodes", "regions", "edges", "names"], root.EnumerateObject().Select(property => property.Name));
        var first = root.GetProperty("names")[0];
        Assert.Equal(["alias", "nameLink"], first.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["href", "title"], first.GetProperty("nameLink").EnumerateObject().Select(property => property.Name));
        Assert.Equal(["alias", "state", "label", "nameLink"], root.GetProperty("regions")[0].EnumerateObject().Select(property => property.Name));
    }

    /// <summary>The nodes whose link is the topology's, which a payload without a monitor does not have.</summary>
    private static class RuntimeFixtureLinks
    {
        public static HashSet<string> Linked(RuntimeManifest manifest) =>
            [.. manifest.Nodes
                .Where(node => node.Kind is RuntimeNodeKind.WebApp or RuntimeNodeKind.FrontDoor or RuntimeNodeKind.Sql)
                .Select(node => node.Alias)];
    }
}
