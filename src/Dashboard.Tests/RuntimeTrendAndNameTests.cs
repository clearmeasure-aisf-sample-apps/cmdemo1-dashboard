using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The two records of the runtime payload that carry a thing rather than a state: the sparkline beside a number, copied
/// out of the health trend the probes built (<see cref="RuntimeTrend"/>), and the name of a box that has nothing else to
/// update (<see cref="RuntimeName"/>). Read straight, with fixed values and with none.
/// </summary>
public class RuntimeTrendAndNameTests
{
    private const string Href = "https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000";
    private const string Title = "The Azure subscription in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)";
    private const string OtherTitle = "The resource group rg-cmdemo1-nonprod in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)";

    private static readonly RuntimeLink Portal = new(Href, Title);

    /// <summary>The payload of one mark of a relationship, as the script is handed it.</summary>
    private static string EdgeJson(RuntimeEdgeMark edge) => new RuntimePayload([], [], [edge]).ToJson();

    /// <summary>The payload of these boxes that are nothing but a name, as the script is handed it.</summary>
    private static string NamesJson(params RuntimeName[] names) => new RuntimePayload([], [], [], names).ToJson();

    // ---------------------------------------------------------------- RuntimeTrend

    [Fact]
    public void ATrendOfTheProbesBecomesTheHeightsAndTheWordsTheScriptDraws()
    {
        var trend = RuntimeTrend.Of(Trend.Of([0, 10, 40, 20], "Requests per minute"))!;

        Assert.Equal([0, 0.25, 1, 0.5], trend.Points);
        Assert.Equal("Requests per minute, last 4 checks: 0 to 40, now 20", trend.Title);
    }

    [Fact]
    public void TheWordsKeepTheUnitAndTheCountOfChecksTheTrendSaid()
    {
        var trend = RuntimeTrend.Of(Trend.Of([2.5, 14, 3.2], "CPU of the process", " %"))!;

        Assert.Equal([0.179, 1, 0.229], trend.Points);
        Assert.Equal("CPU of the process, last 3 checks: 2.5 to 14 %, now 3.2 %", trend.Title);
    }

    [Fact]
    public void WhereTheProbesMadeNoLineTheScriptGetsNoTrend()
    {
        Assert.Null(RuntimeTrend.Of(null));
        // One reading makes no trend on the health side, so there is nothing to copy here either.
        Assert.Null(RuntimeTrend.Of(Trend.Of([7d], "Requests per minute")));
    }

    [Fact]
    public void ATrendOfNoReadingsIsCopiedWithNothingToDrawAndItsWordsStill()
    {
        var trend = RuntimeTrend.Of(new Trend([], "Requests per minute"))!;

        Assert.Empty(trend.Points);
        Assert.Equal("Requests per minute", trend.Title);
        // An empty line is still a trend the script is told about; only a missing one is left out of the payload.
        using var drawn = JsonDocument.Parse(EdgeJson(new RuntimeEdgeMark("e1", "neutral", null, null, null, "The browser to the Front Door.", null, trend)));
        var mark = drawn.RootElement.GetProperty("edges")[0].GetProperty("trend");

        Assert.Equal(["points", "title"], mark.EnumerateObject().Select(property => property.Name));
        Assert.Equal("[]", mark.GetProperty("points").GetRawText());
    }

    [Fact]
    public void TheHeightsAreCopiedAsTheyStandAndScaledAgainByNobody()
    {
        // What the health side decided is what is drawn: RuntimeTrend neither clamps nor rounds.
        var trend = new RuntimeTrend([-1, 0.5, 2, 1.5], "SQL commands per minute");

        Assert.Equal([-1, 0.5, 2, 1.5], trend.Points);
        Assert.Equal("SQL commands per minute", trend.Title);
    }

    [Fact]
    public void TwoCopiesOfOneTrendAreTheSameTrendAndAnotherLineIsNot()
    {
        var source = Trend.Of([0, 5, 10], "SQL commands per minute")!;
        var copy = RuntimeTrend.Of(source)!;

        Assert.Equal([0, 0.5, 1], copy.Points);
        Assert.Equal(RuntimeTrend.Of(source), copy);
        Assert.NotEqual(new RuntimeTrend(source.Points, "Requests per minute"), copy);
        Assert.NotEqual(new RuntimeTrend([0, 0.5, 0.75], copy.Title), copy);
    }

    [Fact]
    public void TheTrendTheScriptGetsIsWordsAndHeightsWithNoDrawingMathsOfItsOwn()
    {
        // The heights are laid into a box by the script; only the health trend knows the geometry of an SVG.
        Assert.Equal(["Points", "Title"], typeof(RuntimeTrend).GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.Null(typeof(RuntimeTrend).GetMethod("Polyline"));
        Assert.Null(typeof(RuntimeTrend).GetMethod("Last"));
        Assert.NotNull(typeof(Trend).GetMethod("Polyline"));
    }

    [Fact]
    public void ATrendSitsBesideTheWordsOfTheLineItBelongsTo()
    {
        var line = RuntimeTileLine.Of("plain", [new RuntimeTextPart("40 req/min")], RuntimeTrend.Of(Trend.Of([10, 40], "Requests per minute")));

        Assert.Equal("40 req/min", line.Text);
        Assert.Equal("plain", line.Tone);
        Assert.Equal([0.25, 1], line.Trend!.Points);
        Assert.Equal("Requests per minute, last 2 checks: 10 to 40, now 40", line.Trend.Title);
    }

    [Fact]
    public void TheScriptReadsATrendAsHeightsAndThenWords()
    {
        using var json = JsonDocument.Parse(EdgeJson(new RuntimeEdgeMark(
            "app_ui_primary-to-sqldb", "active", "12", "sql/min", null, "The web app to the database.", null,
            RuntimeTrend.Of(Trend.Of([10, 40], "SQL commands per minute")))));
        var edge = json.RootElement.GetProperty("edges")[0];
        var trend = edge.GetProperty("trend");

        Assert.Equal(["id", "state", "number", "unit", "title", "trend"], edge.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["points", "title"], trend.EnumerateObject().Select(property => property.Name));
        Assert.Equal([0.25, 1], trend.GetProperty("points").EnumerateArray().Select(point => point.GetDouble()));
        Assert.Equal("SQL commands per minute, last 2 checks: 10 to 40, now 40", trend.GetProperty("title").GetString());
    }

    [Fact]
    public void AnEdgeWithNoSparklineIsWrittenWithoutOne()
    {
        using var json = JsonDocument.Parse(EdgeJson(new RuntimeEdgeMark("browser-to-fd_ui", "neutral", null, null, null, "The browser to the Front Door.")));

        Assert.Equal(
            ["id", "state", "title"],
            json.RootElement.GetProperty("edges")[0].EnumerateObject().Select(property => property.Name));
    }

    // ---------------------------------------------------------------- RuntimeName

    [Fact]
    public void ANameCarriesTheAliasOfItsBoxAndWhereItsNameLeads()
    {
        var name = new RuntimeName("sub", Portal);

        Assert.Equal("sub", name.Alias);
        Assert.Equal(Href, name.NameLink.Href);
        Assert.Equal(Title, name.NameLink.Title);
        Assert.Equal(new RuntimeLink(Href, Title), name.NameLink);
    }

    [Fact]
    public void TwoNamesOfTheSameBoxAndTheSameLinkAreOneName()
    {
        Assert.Equal(new RuntimeName("sub", Portal), new RuntimeName("sub", new RuntimeLink(Href, Title)));
        Assert.NotEqual(new RuntimeName("rg_tier", Portal), new RuntimeName("sub", Portal));
        Assert.NotEqual(new RuntimeName("sub", new RuntimeLink(Href, OtherTitle)), new RuntimeName("sub", Portal));
        Assert.NotEqual(new RuntimeName("sub", new RuntimeLink("https://portal.example.net/#other", Title)), new RuntimeName("sub", Portal));
    }

    [Fact]
    public void ABoxIsNamedByItsAliasAndItsLinkAndByNothingElse() =>
        Assert.Equal(["Alias", "NameLink"], typeof(RuntimeName).GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal));

    [Fact]
    public void ANameOfNoAliasAndNoWordsStandsAsItIs()
    {
        // The record checks nothing of its own: the manifest parser is what refuses a box without an alias or without an https address.
        using var json = JsonDocument.Parse(NamesJson(new RuntimeName(string.Empty, new RuntimeLink(string.Empty, string.Empty))));
        var name = json.RootElement.GetProperty("names")[0];

        Assert.Equal(string.Empty, name.GetProperty("alias").GetString());
        Assert.Equal(string.Empty, name.GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal("""{"alias":"","nameLink":{"href":"","title":""}}""", name.GetRawText());
    }

    [Fact]
    public void TheScriptReadsANameAsItsAliasAndTheLinkOfItsName()
    {
        using var json = JsonDocument.Parse(NamesJson(new RuntimeName("cae", Portal)));
        var drawn = json.RootElement.GetProperty("names")[0];

        Assert.Equal(["alias", "nameLink"], drawn.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["href", "title"], drawn.GetProperty("nameLink").EnumerateObject().Select(property => property.Name));
        Assert.Equal("cae", drawn.GetProperty("alias").GetString());
        Assert.Equal(Href, drawn.GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal(Title, drawn.GetProperty("nameLink").GetProperty("title").GetString());
    }

    [Fact]
    public void AnEmptyListOfNamesIsWrittenAndNoListOfThemIsLeftOut()
    {
        using var empty = JsonDocument.Parse(NamesJson());
        using var none = JsonDocument.Parse(new RuntimePayload([], [], [], null).ToJson());

        Assert.Equal(["nodes", "regions", "edges", "names"], empty.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("[]", empty.RootElement.GetProperty("names").GetRawText());
        Assert.Equal(["nodes", "regions", "edges"], none.RootElement.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public void TheBuilderNamesEveryBoxOfALinkAndLeavesTheOnesWithoutOneOut()
    {
        var manifest = RuntimeManifestParser.ParseManifest("""
            {
              "environment": "tdd",
              "nodes": [ { "alias": "browser", "kind": "person", "name": "Browser", "links": { "site": "https://demo.example.net" } } ],
              "frames": [
                { "alias": "sub", "kind": "subscription", "name": "cmdemo1", "links": { "portal": "https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000" } },
                { "alias": "rg_tier", "kind": "resourceGroup", "name": "rg-cmdemo1-nonprod" }
              ]
            }
            """).Value!;
        var names = RuntimePayloadBuilder.Build(manifest, null, null, TimeZoneInfo.Utc).Names!;

        Assert.Equal(
            [
                ("browser", "https://demo.example.net/", "Browser: demo.example.net, as a browser opens it (opens in a new tab)"),
                ("sub", Href, Title),
            ],
            names.Select(name => (name.Alias, name.NameLink.Href, name.NameLink.Title)));
    }
}
