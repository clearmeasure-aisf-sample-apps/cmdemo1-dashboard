using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeLoaders.Routing" /> and its entries, <see cref="Served" /> (#1706): the answers
/// <see cref="RuntimeLoaders.Serving" /> cannot give, given per file rather than per handler.
/// <see cref="RuntimeLoaders.Serving" /> answers the files it is named with 200 and everything else 404, which is
/// every answer the two files it was written for need; a file that answers 500, a file answered the page at a status
/// that is not the fallback route's own, or a file whose request throws <see cref="HttpRequestException" /> while its
/// pair is fine had each to drop to a raw <see cref="StubHandler" /> and lose the one base address and the one routing
/// - the three handlers of <see cref="RuntimeLoaderTests.AnIndexThatCannotBeReadIsReportedWithItsReason" /> are that
/// cost paid three times over.
/// <para>
/// What is pinned here is the shape of the two new answers and nothing of the loader's reading of them, which its own
/// tests pin: an entry is answered at its path with the status it carries (HTTP 200 when the test named none, which is
/// what makes <see cref="RuntimeLoaders.Serving" /> and this routing answer a file of both the same way - pinned by
/// the two compared below), a failing entry throws an <see cref="HttpRequestException" /> of its reason, and every
/// address neither entry names is the 404 both routings give. Then the four shapes the new answers make possible,
/// each read through the one site the fixture builds.
/// </para>
/// <para>
/// Nothing here reaches the network: every answer, thrown or served, is one the fixture built.
/// </para>
/// </summary>
public class RuntimeLoadersRoutingTests
{
    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";

    /// <summary>The page a host answers every unknown address with - what a fallback route serves, and what
    /// <see cref="RuntimeLoader"/> reads as "this site has no <c>runtime/</c>" at the status that route answers with.</summary>
    private const string Page = "<!DOCTYPE html><html><body>The page itself</body></html>";

    /// <summary>The reason a failing entry throws with, worded as a socket would and not as a status would.</summary>
    private const string Closed = "the socket closed before the answer";

    private static readonly string UatManifest = RuntimeManifestParserTests.Sample("uat.json");

    [Fact]
    public async Task ARoutingEntryAnswersWithTheStatusItCarriesAndNotWithTwoHundred()
    {
        var loader = RuntimeLoaders.From(RuntimeLoaders.Routing(
            Served.File(RuntimeLoaders.RuntimePath("uat.json"), UatManifest, HttpStatusCode.InternalServerError),
            Served.File(RuntimeLoaders.RuntimePath("uat.svg"), Picture)));

        var diagram = await loader.LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        // The picture is served and says nothing of its own: the manifest's 500 is the one thing the read has to say,
        // and a pair with something to say keeps neither of its two parts.
        Assert.False(diagram.IsValid);
        Assert.Null(diagram.Manifest);
        Assert.Null(diagram.Svg);
        Assert.Equal(["The server answered HTTP 500 for runtime/uat.json."], diagram.Errors);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "The server answered HTTP 404 for runtime/uat.json.")]
    [InlineData(HttpStatusCode.InternalServerError, "The server answered HTTP 500 for runtime/uat.json.")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "The server answered HTTP 503 for runtime/uat.json.")]
    [InlineData(HttpStatusCode.GatewayTimeout, "The server answered HTTP 504 for runtime/uat.json.")]
    public async Task TheStatusAnEntryCarriesIsTheStatusTheLoaderSays(HttpStatusCode status, string sentence)
    {
        // The picture is served as ever, so the one thing the read has to say is the status the entry carries - and
        // it is said as the number it is, whatever that number is.
        var diagram = await RuntimeLoaders
            .From(RuntimeLoaders.Routing(
                Served.File(RuntimeLoaders.RuntimePath("uat.json"), UatManifest, status),
                Served.File(RuntimeLoaders.RuntimePath("uat.svg"), Picture)))
            .LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        Assert.Equal([sentence], diagram.Errors);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
    public async Task AnyTwoHundredStatusAnEntryCarriesIsAnsweredAsTheFileItself(HttpStatusCode status)
    {
        var diagram = await RuntimeLoaders
            .From(RuntimeLoaders.Routing(
                Served.File(RuntimeLoaders.RuntimePath("uat.json"), UatManifest, status),
                Served.File(RuntimeLoaders.RuntimePath("uat.svg"), Picture, status)))
            .LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        // The loader asks whether a status is in the two hundred and not whether it is 200, so an entry answered 203
        // is a file read like any other - the one exception being the page behind a fallback route, which the loader
        // reads at 200 only, and which the test below pins.
        Assert.True(diagram.IsValid);
        Assert.Equal("uat", diagram.Manifest!.Environment);
        Assert.Equal(Picture, diagram.Svg);
        Assert.Empty(diagram.Errors);
    }

    [Fact]
    public async Task AFileThatCannotBeReadSaysItsOwnReasonWhileItsPairIsServed()
    {
        var loader = RuntimeLoaders.From(RuntimeLoaders.Routing(
            Served.File(RuntimeLoaders.RuntimePath("uat.json"), UatManifest),
            Served.Failing(RuntimeLoaders.RuntimePath("uat.svg"), Closed)));

        var diagram = await loader.LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        // The manifest was read and parsed; the picture's request never got an answer to read. The pair keeps neither
        // part, and the one thing said is the loader's own "could not be loaded" words around the entry's reason.
        Assert.False(diagram.IsValid);
        Assert.Null(diagram.Manifest);
        Assert.Null(diagram.Svg);
        Assert.Equal([$"runtime/uat.svg could not be loaded: {Closed}"], diagram.Errors);
    }

    [Fact]
    public async Task TwoFilesThatCannotBeReadSayTheirReasonsInTheOrderOfThePair()
    {
        var loader = RuntimeLoaders.From(RuntimeLoaders.Routing(
            Served.Failing(RuntimeLoaders.RuntimePath("uat.svg"), Closed),
            Served.Failing(RuntimeLoaders.RuntimePath("uat.json"), Closed)));

        var diagram = await loader.LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        // The two entries were named picture-first and are still said manifest-first, as every pair is said: the
        // order is the loader's reading of a pair and no thing an entry can move.
        Assert.Equal(
            [$"runtime/uat.json could not be loaded: {Closed}", $"runtime/uat.svg could not be loaded: {Closed}"],
            diagram.Errors);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, RuntimeAvailability.NotDeployed)]
    [InlineData(HttpStatusCode.Created, RuntimeAvailability.Failed)]
    public async Task ThePageBehindAFallbackRouteIsNoRuntimeOnlyAtTheStatusThatRouteAnswers(
        HttpStatusCode status,
        RuntimeAvailability read)
    {
        var index = await RuntimeLoaders
            .From(RuntimeLoaders.Routing(Served.File(RuntimeLoaders.RuntimePath("index.json"), Page, status)))
            .LoadIndexAsync(CancellationToken.None);

        Assert.Equal(read, index.Availability);

        // At 200 the page is the site answering an address it holds nothing at - no runtime, and nothing to say. At
        // any other two hundred it is a file that is not JSON, which is a runtime folder that could not be read.
        Assert.Equal(read == RuntimeAvailability.NotDeployed, index.Errors.Count == 0);
        if (read == RuntimeAvailability.Failed)
        {
            Assert.StartsWith("runtime/index.json: The file is not valid JSON", Assert.Single(index.Errors), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheRoutingAnswersEveryAddressItWasGivenNoEntryForWithFourOhFour()
    {
        var loader = RuntimeLoaders.From(RuntimeLoaders.Routing(
            Served.File(RuntimeLoaders.RuntimePath("uat.json"), UatManifest, HttpStatusCode.InternalServerError),
            Served.Failing(RuntimeLoaders.RuntimePath("late.svg"), Closed)));

        var diagram = await loader.LoadDiagramAsync(new RuntimeIndexEntry("prod", "prod.json", "prod.svg"), CancellationToken.None);

        Assert.False(diagram.IsValid);
        Assert.Equal(
            ["The server answered HTTP 404 for runtime/prod.json.", "The server answered HTTP 404 for runtime/prod.svg."],
            diagram.Errors);
    }

    [Fact]
    public async Task ARoutingOfNoEntriesServesNothingAndTheLoaderReadsThatAsNoRuntime()
    {
        var index = await RuntimeLoaders.From(RuntimeLoaders.Routing()).LoadIndexAsync(CancellationToken.None);

        // The same answer Serving() of no files gives, which is the answer of a site with no runtime folder at all.
        Assert.Equal(RuntimeAvailability.NotDeployed, index.Availability);
        Assert.Null(index.Index);
        Assert.Empty(index.Errors);
    }

    [Fact]
    public async Task EveryFileOfTheRoutingAnsweredWithTwoHundredIsAnsweredAsServingAnswersIt()
    {
        // Serving cannot carry a status, so every file of its routing answers 200. Given the same files with the
        // status Serving answers with - the one the factory defaults to - the two routings answer every address the
        // same: the same status, the same body, the 404 everywhere else. That is what lets Serving keep its lines and
        // its behaviour while the answers it cannot give are said here.
        static HttpResponseMessage Ask(Func<HttpRequestMessage, HttpResponseMessage> route, string path) =>
            route(new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(RuntimeLoaders.Site), path)));

        var served = new[]
        {
            (Path: RuntimeLoaders.RuntimePath("one.json"), Body: UatManifest),
            (Path: RuntimeLoaders.RuntimePath("one.svg"), Body: Picture),
        };

        var asTuples = RuntimeLoaders.Serving(served);
        var asEntries = RuntimeLoaders.Routing([.. served.Select(file => Served.File(file.Path, file.Body))]);

        var everyPath = new[]
        {
            RuntimeLoaders.RuntimePath("one.json"),
            RuntimeLoaders.RuntimePath("one.svg"),
            RuntimeLoaders.RuntimePath("none.json"),
        };

        foreach (var path in everyPath)
        {
            using var byTuples = Ask(asTuples, path);
            using var byEntries = Ask(asEntries, path);

            Assert.Equal(byTuples.StatusCode, byEntries.StatusCode);
            Assert.Equal(
                await byTuples.Content.ReadAsStringAsync(),
                await byEntries.Content.ReadAsStringAsync());
        }
    }

    [Fact]
    public async Task EverySampleOfTheRoutingAnsweredWithTwoHundredIsAnsweredAsServingSamplesAnswersIt()
    {
        // #1714: ServingSamples is the 200 spelling of the routing, as Serving is. Given the same samples as the
        // Served.File entries that answer with the status ServingSamples answers with, the two routings answer
        // every address the same: the same status, the same body, the 404 everywhere else.
        static HttpResponseMessage Ask(Func<HttpRequestMessage, HttpResponseMessage> route, string path) =>
            route(new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(RuntimeLoaders.Site), path)));

        var samples = new[] { "tdd.json", "tdd.svg" };

        var asSamples = RuntimeLoaders.ServingSamples(samples);
        var asEntries = RuntimeLoaders.Routing([.. samples.Select(file => Served.File(RuntimeLoaders.RuntimePath(file), RuntimeManifestParserTests.Sample(file)))]);

        var everyPath = new[]
        {
            RuntimeLoaders.RuntimePath("tdd.json"),
            RuntimeLoaders.RuntimePath("tdd.svg"),
            RuntimeLoaders.RuntimePath("none.json"),
        };

        foreach (var path in everyPath)
        {
            using var bySamples = Ask(asSamples, path);
            using var byEntries = Ask(asEntries, path);

            Assert.Equal(bySamples.StatusCode, byEntries.StatusCode);
            Assert.Equal(
                await bySamples.Content.ReadAsStringAsync(),
                await byEntries.Content.ReadAsStringAsync());
        }
    }

    [Fact]
    public async Task TheEdgeCaseOfAFileIsOneMoreEntryOfTheRoutingAndNotAHandlerOfItsOwn()
    {
        // The shape #456's edge cases ask for: one site, one base address, and each per-file case written as an
        // entry of the routing - one file a 500, one the page answered at a status the fallback route does not
        // answer with, one whose request fails while its manifest is served, and the file the site holds nothing of.
        var handler = new StubHandler(RuntimeLoaders.Routing(
            Served.File(RuntimeLoaders.RuntimePath("uat.json"), UatManifest, HttpStatusCode.InternalServerError),
            Served.File(RuntimeLoaders.RuntimePath("uat.svg"), Picture),
            Served.File(RuntimeLoaders.RuntimePath("tdd.json"), Page, HttpStatusCode.Created),
            Served.Failing(RuntimeLoaders.RuntimePath("tdd.svg"), Closed)));

        var loader = RuntimeLoaders.From(handler);

        var refused = await loader.LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);
        var unreadable = await loader.LoadDiagramAsync(new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg"), CancellationToken.None);
        var absent = await loader.LoadDiagramAsync(new RuntimeIndexEntry("prod", "prod.json", "prod.svg"), CancellationToken.None);

        Assert.Equal(["The server answered HTTP 500 for runtime/uat.json."], refused.Errors);
        Assert.Equal(2, unreadable.Errors.Count);
        Assert.StartsWith("runtime/tdd.json: The file is not valid JSON", unreadable.Errors[0], StringComparison.Ordinal);
        Assert.Equal($"runtime/tdd.svg could not be loaded: {Closed}", unreadable.Errors[1]);
        Assert.Equal(
            ["The server answered HTTP 404 for runtime/prod.json.", "The server answered HTTP 404 for runtime/prod.svg."],
            absent.Errors);

        // All of it on the one site the fixture builds its client on - no test-side address is written anywhere.
        Assert.Equal(
            [
                "https://dashboard.example.net/runtime/prod.json",
                "https://dashboard.example.net/runtime/prod.svg",
                "https://dashboard.example.net/runtime/tdd.json",
                "https://dashboard.example.net/runtime/tdd.svg",
                "https://dashboard.example.net/runtime/uat.json",
                "https://dashboard.example.net/runtime/uat.svg",
            ],
            handler.RequestedUrls.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ARoutingAndItsEntriesAreNeverBuiltOnNothing()
    {
        Assert.Throws<ArgumentNullException>(() => RuntimeLoaders.Routing(null!));
        Assert.Throws<ArgumentException>(() => RuntimeLoaders.Routing(Served.File(RuntimeLoaders.RuntimePath("a.json"), Picture), Served.File("", Picture)));
        Assert.Throws<ArgumentException>(() => Served.File("", Picture));
        Assert.Throws<ArgumentNullException>(() => Served.File(RuntimeLoaders.RuntimePath("a.json"), null!));
        Assert.Throws<ArgumentException>(() => Served.Failing("", Closed));
        Assert.Throws<ArgumentException>(() => Served.Failing(RuntimeLoaders.RuntimePath("a.json"), ""));
        Assert.Throws<ArgumentNullException>(() => Served.Failing(RuntimeLoaders.RuntimePath("a.json"), null!));
    }

    [Fact]
    public async Task AFileEntryCarriesTheStatusItWasGivenAndFailingCarriesItsReasonAndNoStatus()
    {
        // The two answers the factories are: a file with a status, and a failure with a reason. A file named with no
        // status is today's answer, HTTP 200; a failure keeps its reason and answers no status at all, so the only
        // way the loader hears of it is the request failing.
        var plain = Served.File("/runtime/a.json", Picture);
        var refused = Served.File("/runtime/a.json", Picture, HttpStatusCode.InternalServerError);
        var failing = Served.Failing("/runtime/a.json", Closed);

        Assert.Equal("/runtime/a.json", plain.Path);
        Assert.Equal(Picture, plain.Body);
        Assert.Equal(HttpStatusCode.OK, plain.Status);
        Assert.Null(plain.Failure);
        Assert.Equal(HttpStatusCode.InternalServerError, refused.Status);
        Assert.Equal(Closed, failing.Failure);

        using var answered = plain.Reply();

        Assert.Equal(HttpStatusCode.OK, answered.StatusCode);
        Assert.Equal(Picture, await answered.Content.ReadAsStringAsync());
        Assert.Throws<HttpRequestException>(() => failing.Reply());
        Assert.Throws<HttpRequestException>(() => Served.Failing("/runtime/a.json", Closed).Reply());
    }
}
