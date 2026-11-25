using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeLoaders" /> is the one way a test gives a <see cref="RuntimeLoader" /> the dashboard's own
/// site and the files it holds on it (#1684): <see cref="RuntimeLoaders.From(Func{HttpRequestMessage, HttpResponseMessage})" />
/// for the client with the one base address, <see cref="RuntimeLoaders.Serving" /> and
/// <see cref="RuntimeLoaders.ServingSamples" /> for the routing over <c>runtime/</c>, and
/// <see cref="RuntimeLoaders.ServingPair" /> for the manifest-and-picture shape of #1437.
/// <para>
/// What is pinned here is the shape of the fixture itself - the one site every path of <c>runtime/</c> is resolved
/// against, the files it answers and the 404 it gives elsewhere, and the pair's one picture for any SVG name - so
/// that the tests which read through it say what their loader does with a site, and never where the site stands.
/// </para>
/// <para>
/// Nothing here reaches the network: every answer is one the fixture built.
/// </para>
/// </summary>
public class RuntimeLoadersTests
{
    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";

    private static readonly string UatManifest = RuntimeManifestParserTests.Sample("uat.json");

    [Fact]
    public async Task EveryFileOfRuntimeIsAskedForOnTheDashboardsOwnSite()
    {
        var handler = new StubHandler(RuntimeLoaders.ServingSamples("index.json", "uat.json", "uat.svg"));

        var index = await RuntimeLoaders.From(handler).LoadIndexAsync(CancellationToken.None);

        Assert.Equal(RuntimeAvailability.Available, index.Availability);
        Assert.Equal(["tdd", "uat"], index.Index!.Environments.Select(entry => entry.Name));
        Assert.Equal([$"{RuntimeLoaders.Site}runtime/index.json"], handler.RequestedUrls);
    }

    [Fact]
    public async Task TheSamplesAreAnsweredWhereTheLoaderAsksForThem()
    {
        var handler = new StubHandler(RuntimeLoaders.ServingSamples("index.json", "uat.json", "uat.svg"));

        var diagram = await RuntimeLoaders.From(handler)
            .LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        Assert.True(diagram.IsValid);
        Assert.Equal("uat", diagram.Manifest!.Environment);
        Assert.StartsWith("<svg", diagram.Svg, StringComparison.Ordinal);
        Assert.Equal(
            [$"{RuntimeLoaders.Site}runtime/uat.json", $"{RuntimeLoaders.Site}runtime/uat.svg"],
            handler.RequestedUrls.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ThePathTheFixtureAnswersAtIsTheRuntimeFolderTheAppItselfNames()
    {
        // The loader asks for RuntimeLoader.Folder + file of the site the client stands on; the fixture answers at
        // that path, so the two cannot drift apart without one of these two lines going red.
        Assert.Equal("/runtime/uat.svg", RuntimeLoaders.RuntimePath("uat.svg"));
        Assert.Equal("/" + RuntimeLoader.Folder + "uat.svg", RuntimeLoaders.RuntimePath("uat.svg"));
    }

    [Fact]
    public async Task ServingAnswersTheFilesItNamesAndNothingElse()
    {
        var loader = RuntimeLoaders.From(RuntimeLoaders.Serving(
            (RuntimeLoaders.RuntimePath("one.json"), UatManifest),
            (RuntimeLoaders.RuntimePath("one.svg"), Picture)));

        var named = await loader.LoadDiagramAsync(new RuntimeIndexEntry("one", "one.json", "one.svg"), CancellationToken.None);
        var unnamed = await loader.LoadDiagramAsync(new RuntimeIndexEntry("two", "two.json", "two.svg"), CancellationToken.None);

        Assert.True(named.IsValid);
        Assert.Equal(Picture, named.Svg);
        Assert.Empty(named.Errors);
        Assert.False(unnamed.IsValid);
        Assert.Equal(
            ["The server answered HTTP 404 for runtime/two.json.", "The server answered HTTP 404 for runtime/two.svg."],
            unnamed.Errors);
    }

    [Fact]
    public async Task ServingWithNoFilesNamedServesNothingAndTheLoaderReadsThatAsNoRuntime()
    {
        var index = await RuntimeLoaders.From(RuntimeLoaders.Serving()).LoadIndexAsync(CancellationToken.None);

        Assert.Equal(RuntimeAvailability.NotDeployed, index.Availability);
        Assert.Null(index.Index);
        Assert.Empty(index.Errors);
    }

    [Fact]
    public async Task ServingPairAnswersTheManifestAtItsPathAndAnySvgWithTheOnePicture()
    {
        var loader = RuntimeLoaders.From(RuntimeLoaders.ServingPair(RuntimeLoaders.RuntimePath("uat.json"), UatManifest, Picture));

        var itsOwn = await loader.LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);
        var anotherName = await loader.LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.other.svg"), CancellationToken.None);
        var anotherManifest = await loader.LoadDiagramAsync(new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg"), CancellationToken.None);

        Assert.True(itsOwn.IsValid);
        Assert.Equal(Picture, itsOwn.Svg);
        Assert.Empty(itsOwn.Errors);

        // The pair holds one picture for the view, whatever the SVG its entry names is called.
        Assert.True(anotherName.IsValid);
        Assert.Equal(Picture, anotherName.Svg);

        // A manifest the caller named no path of is not there, and its reason is said before the picture's.
        Assert.False(anotherManifest.IsValid);
        Assert.Null(anotherManifest.Manifest);
        Assert.Null(anotherManifest.Svg);
        Assert.Equal(["The server answered HTTP 404 for runtime/tdd.json."], anotherManifest.Errors);
    }

    [Fact]
    public async Task TheFixtureBuildsItsClientOnTheAddressTheLoaderResolvesItsPathAgainst()
    {
        // The point of the fixture: a test says which files a site holds and never where the site stands. The
        // address the client is built on is the one the loader resolves a path of <c>runtime/</c> against - the
        // same composition ProbeUrl.ResolveRelative makes for a path taken from the site's root, written here from
        // the app's own two constants so a folder renamed on either side shows up as a different address.
        var handler = new StubHandler(RuntimeLoaders.ServingSamples("index.json"));
        var askedFor = new Uri(new Uri(RuntimeLoaders.Site), RuntimeLoader.Folder + RuntimeLoader.IndexFile);

        await RuntimeLoaders.From(handler).LoadIndexAsync(CancellationToken.None);

        Assert.Equal([askedFor.AbsoluteUri], handler.RequestedUrls);
        Assert.Equal("https://dashboard.example.net/runtime/index.json", askedFor.AbsoluteUri);
    }

    [Fact]
    public void ALoaderIsNeverBuiltOnNoSite()
    {
        Assert.Throws<ArgumentNullException>(() => RuntimeLoaders.From((StubHandler)null!));
        Assert.Throws<ArgumentNullException>(() => RuntimeLoaders.Serving(null!));
        Assert.Throws<ArgumentNullException>(() => RuntimeLoaders.ServingSamples(null!));
        Assert.Throws<ArgumentException>(() => RuntimeLoaders.Serving(("", UatManifest)));
        Assert.Throws<ArgumentException>(() => RuntimeLoaders.ServingPair("", UatManifest, Picture));
    }
}
