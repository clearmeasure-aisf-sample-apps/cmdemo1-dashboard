using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// Pins what the diagram and the database view share since they read their manifest and SVG through one pair
/// (#1437): both parts and no errors, or neither part and every reason, in the same words for both views.
/// </summary>
public class RuntimeFilesPairTests
{
    private const string DatabaseManifest = """
        {
          "environment": "tdd",
          "database": "sqldb-cmdemo1-tdd",
          "readAt": "2026-10-12T10:00:00Z",
          "source": "https://raw.githubusercontent.com/org/repo/main/environments/tdd/database.json",
          "truncated": false,
          "plantuml": "1.2026.8",
          "tables": [ { "alias": "t_dbo_Employee", "qualifiedName": "db.t_dbo_Employee", "schema": "dbo", "name": "Employee", "rows": 123, "columns": 7 } ],
          "references": []
        }
        """;

    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";

    private static RuntimeLoader Loader(Func<HttpRequestMessage, HttpResponseMessage> serve) =>
        new(new HttpClient(new StubHandler(serve)) { BaseAddress = new Uri("https://dashboard.example.net/") });

    private static Func<HttpRequestMessage, HttpResponseMessage> Serving(string manifestPath, string manifest, string svg) => request =>
        request.RequestUri!.AbsolutePath == manifestPath ? StubHandler.Answer(HttpStatusCode.OK, manifest)
        : request.RequestUri!.AbsolutePath.EndsWith(".svg", StringComparison.Ordinal) ? StubHandler.Answer(HttpStatusCode.OK, svg)
        : StubHandler.Answer(HttpStatusCode.NotFound);

    private static Task<DatabaseDiagram> LoadDatabase(Func<HttpRequestMessage, HttpResponseMessage> serve) =>
        Loader(serve).LoadDatabaseAsync(new RuntimeDatabaseFiles("tdd.database.svg", "tdd.database.json"), CancellationToken.None);

    private static Task<RuntimeDiagram> LoadDiagram(Func<HttpRequestMessage, HttpResponseMessage> serve) =>
        Loader(serve).LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

    [Fact]
    public async Task ADatabaseViewWithAGoodManifestAndNoSvgKeepsNeitherPart()
    {
        var diagram = await LoadDatabase(Serving("/runtime/tdd.database.json", DatabaseManifest, "not a picture"));

        Assert.False(diagram.IsValid);
        Assert.Null(diagram.Manifest);
        Assert.Null(diagram.Svg);
        Assert.Equal(["runtime/tdd.database.svg: not an SVG."], diagram.Errors);
    }

    [Fact]
    public async Task ADiagramWithAGoodManifestAndNoSvgKeepsNeitherPart()
    {
        var diagram = await LoadDiagram(Serving("/runtime/uat.json", RuntimeManifestParserTests.Sample("uat.json"), "not a picture"));

        Assert.False(diagram.IsValid);
        Assert.Null(diagram.Manifest);
        Assert.Null(diagram.Svg);
        Assert.Equal(["runtime/uat.svg: not an SVG."], diagram.Errors);
    }

    [Fact]
    public async Task ADatabaseViewWithABadManifestAndAGoodSvgKeepsNeitherPart()
    {
        var diagram = await LoadDatabase(Serving("/runtime/tdd.database.json", "{ }", Picture));

        Assert.False(diagram.IsValid);
        Assert.Null(diagram.Manifest);
        Assert.Null(diagram.Svg);
        Assert.Equal(
            ["runtime/tdd.database.json: environment: missing or empty.", "runtime/tdd.database.json: database: missing or empty."],
            diagram.Errors);
    }

    [Fact]
    public async Task ADatabaseViewWithBothPartsIsValidAndKeepsTheSvgAsItCame()
    {
        var diagram = await LoadDatabase(Serving("/runtime/tdd.database.json", DatabaseManifest, Picture));

        Assert.True(diagram.IsValid);
        Assert.Equal(Picture, diagram.Svg);
        Assert.Empty(diagram.Errors);
    }

    [Fact]
    public async Task BothViewsReportMissingFilesInTheSameWordsAndOrder()
    {
        static HttpResponseMessage Nothing(HttpRequestMessage _) => StubHandler.Answer(HttpStatusCode.NotFound);

        var database = await LoadDatabase(Nothing);
        var diagram = await LoadDiagram(Nothing);

        Assert.Equal(
            ["The server answered HTTP 404 for runtime/tdd.database.json.", "The server answered HTTP 404 for runtime/tdd.database.svg."],
            database.Errors);
        Assert.Equal(
            ["The server answered HTTP 404 for runtime/uat.json.", "The server answered HTTP 404 for runtime/uat.svg."],
            diagram.Errors);
        Assert.False(database.IsValid);
        Assert.False(diagram.IsValid);
    }

    [Fact]
    public async Task APairAsksForExactlyItsTwoFiles()
    {
        var asked = new List<string>();
        var loader = Loader(request =>
        {
            lock (asked)
            {
                asked.Add(request.RequestUri!.AbsolutePath);
            }

            return Serving("/runtime/tdd.database.json", DatabaseManifest, Picture)(request);
        });

        await loader.LoadDatabaseAsync(new RuntimeDatabaseFiles("tdd.database.svg", "tdd.database.json"), CancellationToken.None);

        Assert.Equal(["/runtime/tdd.database.json", "/runtime/tdd.database.svg"], asked.Order(StringComparer.Ordinal));
    }
}
