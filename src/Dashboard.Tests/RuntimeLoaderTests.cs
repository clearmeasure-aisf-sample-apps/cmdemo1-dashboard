using System.Net;

namespace Dashboard.Tests;

public class RuntimeLoaderTests
{
    private static RuntimeLoader Loader(StubHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://dashboard.example.net/") });

    private static HttpResponseMessage Serve(HttpRequestMessage request) => request.RequestUri!.AbsolutePath switch
    {
        "/runtime/index.json" => StubHandler.Answer(HttpStatusCode.OK, RuntimeManifestParserTests.Sample("index.json")),
        "/runtime/uat.json" => StubHandler.Answer(HttpStatusCode.OK, RuntimeManifestParserTests.Sample("uat.json")),
        "/runtime/uat.svg" => StubHandler.Answer(HttpStatusCode.OK, RuntimeManifestParserTests.Sample("uat.svg")),
        _ => StubHandler.Answer(HttpStatusCode.NotFound),
    };

    [Fact]
    public async Task TheIndexOfADeploymentWithDiagramsIsAvailable()
    {
        var handler = new StubHandler(Serve);

        var result = await Loader(handler).LoadIndexAsync(CancellationToken.None);

        Assert.Equal(RuntimeAvailability.Available, result.Availability);
        Assert.Equal(["tdd", "uat"], result.Index!.Environments.Select(entry => entry.Name));
        Assert.Equal(["https://dashboard.example.net/runtime/index.json"], handler.RequestedUrls);
    }

    [Fact]
    public async Task ADeploymentWithoutRuntimeIsNotAnError()
    {
        var result = await Loader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound))).LoadIndexAsync(CancellationToken.None);

        Assert.Equal(RuntimeAvailability.NotDeployed, result.Availability);
        Assert.Null(result.Index);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task AHostThatServesThePageForEveryAddressHasNoRuntimeEither()
    {
        var result = await Loader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, "<!DOCTYPE html><html></html>")))
            .LoadIndexAsync(CancellationToken.None);

        Assert.Equal(RuntimeAvailability.NotDeployed, result.Availability);
    }

    [Fact]
    public async Task WhitespaceBeforeTheHtmlTagIsStillAFallbackRoute()
    {
        var result = await Loader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, "  \n  <html></html>")))
            .LoadIndexAsync(CancellationToken.None);

        Assert.Equal(RuntimeAvailability.NotDeployed, result.Availability);
    }

    [Fact]
    public async Task AnIndexThatCannotBeReadIsReportedWithItsReason()
    {
        var failed = await Loader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.InternalServerError))).LoadIndexAsync(CancellationToken.None);
        var broken = await Loader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, "{ }"))).LoadIndexAsync(CancellationToken.None);
        var offline = await Loader(new StubHandler(_ => throw new HttpRequestException("offline"))).LoadIndexAsync(CancellationToken.None);

        Assert.Equal(RuntimeAvailability.Failed, failed.Availability);
        Assert.Equal(["The server answered HTTP 500 for runtime/index.json."], failed.Errors);
        Assert.Equal(["runtime/index.json: environments: missing or not an array."], broken.Errors);
        Assert.Equal(["runtime/index.json could not be loaded: offline"], offline.Errors);
    }

    [Fact]
    public async Task ADiagramIsItsManifestAndItsSvg()
    {
        var diagram = await Loader(new StubHandler(Serve)).LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        Assert.True(diagram.IsValid);
        Assert.Equal("uat", diagram.Manifest!.Environment);
        Assert.StartsWith("<svg", diagram.Svg, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ADiagramWithoutItsSvgOrManifestIsNotShown()
    {
        var diagram = await Loader(new StubHandler(Serve)).LoadDiagramAsync(new RuntimeIndexEntry("prod", "prod.json", "prod.svg"), CancellationToken.None);

        Assert.False(diagram.IsValid);
        Assert.Null(diagram.Manifest);
        Assert.Equal(
            ["The server answered HTTP 404 for runtime/prod.json.", "The server answered HTTP 404 for runtime/prod.svg."],
            diagram.Errors);
    }

    [Fact]
    public async Task AnSvgThatIsNoSvgIsReported()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath.EndsWith(".svg", StringComparison.Ordinal)
            ? StubHandler.Answer(HttpStatusCode.OK, "not a picture")
            : Serve(request));

        var diagram = await Loader(handler).LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        Assert.Equal(["runtime/uat.svg: not an SVG."], diagram.Errors);
    }

    /// <summary>
    /// #1684: <see cref="RuntimeLoaders" /> answers what this file's own <see cref="Serve" /> answers, on the one
    /// base address its <see cref="Loader" /> builds the client on - pinned beside the copies it stands in for, so
    /// that the lane which takes the copies out rewrites nothing the tests do not already hold. The copies stay
    /// until then: the two ways of asking are held equal here and nowhere else.
    /// </summary>
    [Fact]
    public async Task TheSharedFixtureAnswersWhatThisFilesOwnStubAnswers()
    {
        var shared = RuntimeLoaders.From(RuntimeLoaders.ServingSamples("index.json", "uat.json", "uat.svg"));
        var alone = Loader(new StubHandler(Serve));

        var index = await shared.LoadIndexAsync(CancellationToken.None);
        var indexAlone = await alone.LoadIndexAsync(CancellationToken.None);

        Assert.Equal(indexAlone.Availability, index.Availability);
        Assert.Equal(indexAlone.Index!.Environments, index.Index!.Environments);
        Assert.Equal(indexAlone.Errors, index.Errors);

        var entry = new RuntimeIndexEntry("uat", "uat.json", "uat.svg");
        var diagram = await shared.LoadDiagramAsync(entry, CancellationToken.None);
        var diagramAlone = await alone.LoadDiagramAsync(entry, CancellationToken.None);

        Assert.True(diagram.IsValid);
        Assert.Equal(diagramAlone.Manifest!.Environment, diagram.Manifest!.Environment);
        Assert.Equal(diagramAlone.Manifest.Nodes.Select(node => node.Alias), diagram.Manifest.Nodes.Select(node => node.Alias));
        Assert.Equal(diagramAlone.Svg, diagram.Svg);
        Assert.Equal(diagramAlone.Errors, diagram.Errors);
    }
}
