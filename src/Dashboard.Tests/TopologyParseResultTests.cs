using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="TopologyParseResult"/>, the one thing a caller of <see cref="TopologyParser"/> or
/// <see cref="TopologyLoader"/> reads: either the topology to show or the reasons there is none. The page branches on
/// the topology and prints the messages as they stand (<c>App.razor</c>: <c>result.Topology is null ? result.Errors
/// : null</c>), so the words, their number and their order are what the reader of a broken file sees.
/// </summary>
public class TopologyParseResultTests
{
    private const string Good = """
        { "system": { "slug": "demo", "name": "Demo" },
          "generated": "2026-10-04T22:00:00Z",
          "environments": [ { "name": "tdd", "tier": "nonprod", "deployables": [] } ] }
        """;

    private const string AnEnvironmentWithoutAName = """{ "environments": [ { "tier": "nonprod" } ] }""";

    [Fact]
    public void ASuccessfulReadCarriesTheTopologyAndNoMessage()
    {
        var result = TopologyParser.Parse(Good);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Topology);
        Assert.Empty(result.Errors);

        Assert.Equal("demo", result.Topology!.System.Slug);
        Assert.Equal("Demo", result.Topology.System.Name);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero), result.Topology.Generated);
        Assert.Equal(["tdd"], result.Topology.Environments.Select(environment => environment.Name));
        Assert.Null(result.Topology.Cluster);
    }

    [Fact]
    public void AFailedReadCarriesNoTopologyAndTheMessageOfTheProblem()
    {
        var result = TopologyParser.Parse(AnEnvironmentWithoutAName);

        Assert.False(result.IsValid);
        Assert.Null(result.Topology);
        Assert.Equal(["environments[0].name: missing or empty."], result.Errors);
    }

    [Theory]
    [InlineData(null, "The file is empty.")]
    [InlineData("   ", "The file is empty.")]
    [InlineData("<!DOCTYPE html><html></html>", "The file is not valid JSON (line 1, position 1).")]
    [InlineData("{ \"environments\": [\n  { \"name\": }\n] }", "The file is not valid JSON (line 2, position 13).")]
    [InlineData("[]", "The file must contain a JSON object.")]
    [InlineData("42", "The file must contain a JSON object.")]
    public void AFileThatIsNothingOrNotAnObjectSaysSoInOneMessage(string? json, string message)
    {
        var result = TopologyParser.Parse(json);

        Assert.False(result.IsValid);
        Assert.Null(result.Topology);
        Assert.Equal(message, Assert.Single(result.Errors));
    }

    [Fact]
    public void EveryMessageOfAFailedReadKeepsItsPlaceInTheFile()
    {
        var result = TopologyParser.Parse("""
            { "environments": [
                { "name": "tdd", "versionsUrl": "environments/tdd/versions.json", "deployables": [
                  { "name": "ui", "nodes": [ { "url": "ftp://ui.example.net" } ] } ] },
                "uat" ],
              "cluster": "aks-demo" }
            """);

        Assert.False(result.IsValid);
        Assert.Null(result.Topology);
        Assert.Equal(
            [
                "environments[0].versionsUrl: not an absolute http or https address.",
                "environments[0].deployables[0].nodes[0].url: missing or not an absolute http or https address.",
                "environments[1]: not an object.",
                "cluster: not an object.",
            ],
            result.Errors);
    }

    [Fact]
    public void AFailureOfTheLoadingRatherThanTheFileCarriesItsOwnMessage()
    {
        var result = TopologyParseResult.Failed("The server answered HTTP 502 for topology.json.");

        Assert.False(result.IsValid);
        Assert.Null(result.Topology);
        Assert.Equal("The server answered HTTP 502 for topology.json.", Assert.Single(result.Errors));
    }

    [Fact]
    public void AFailureWithoutAnyMessageIsStillNoTopology()
    {
        var result = TopologyParseResult.Failed();

        Assert.False(result.IsValid);
        Assert.Null(result.Topology);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void IsValidOnlyAsksForTheTopology()
    {
        var topology = TopologyParser.Parse(Good).Topology!;

        Assert.True(new TopologyParseResult(topology, []).IsValid);
        Assert.True(new TopologyParseResult(topology, ["A message the parser never gave."]).IsValid);
        Assert.False(new TopologyParseResult(null, []).IsValid);
    }

    [Fact]
    public void ThePageReadsTheTopologyOrTheMessagesNeverBoth()
    {
        foreach (var json in new string?[] { Good, AnEnvironmentWithoutAName, null, "<html>", "[]", "42" })
        {
            var result = TopologyParser.Parse(json);

            // App.razor: the messages reach the page only when there is no topology to show.
            var shown = result.Topology is null ? result.Errors : null;

            Assert.Equal(result.IsValid, shown is null);
            Assert.True(shown is null || shown.Count > 0, $"A failure with nothing to show: {json}");
        }
    }

    [Fact]
    public async Task TheLoaderHandsOverEitherTheParsedTopologyOrTheMessageOfTheAnswer()
    {
        var found = await LoadAsync(HttpStatusCode.OK, Good);
        Assert.True(found.IsValid);
        Assert.Empty(found.Errors);
        Assert.Equal("Demo", found.Topology!.System.Name);

        // The loader's own failure: the file is not there, so the parser was never asked.
        var missing = await LoadAsync(HttpStatusCode.NotFound);
        Assert.False(missing.IsValid);
        Assert.Null(missing.Topology);
        Assert.Equal("The server answered HTTP 404 for topology.json.", Assert.Single(missing.Errors));

        // The parser's failure, read from a body the static host answers everywhere.
        var html = await LoadAsync(HttpStatusCode.OK, "<!DOCTYPE html><html></html>");
        Assert.False(html.IsValid);
        Assert.Null(html.Topology);
        Assert.Equal("The file is not valid JSON (line 1, position 1).", Assert.Single(html.Errors));
    }

    private static async Task<TopologyParseResult> LoadAsync(HttpStatusCode status, string body = "")
    {
        var http = new HttpClient(new StubHandler(_ => StubHandler.Answer(status, body)))
        {
            BaseAddress = new Uri("https://dashboard.example.net/"),
        };

        return await new TopologyLoader(http).LoadAsync(CancellationToken.None);
    }
}
