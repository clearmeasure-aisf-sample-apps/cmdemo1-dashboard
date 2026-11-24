using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="TopologyLoader"/> and <see cref="RuntimeLoader"/> read files of the dashboard's own site through
/// <see cref="ProbeUrl.ResolveRelative"/>: they ask for the address it gives, exactly the one the client's
/// <see cref="HttpClient.BaseAddress"/> resolved before.
/// </summary>
public class LoaderOwnSiteAddressTests
{
    private static readonly Uri Site = new("https://dashboard.example.net/app/");

    [Fact]
    public async Task TheTopologyIsAskedForAtTheAddressResolveRelativeGives()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));

        await new TopologyLoader(new HttpClient(handler) { BaseAddress = Site }).LoadAsync(CancellationToken.None);

        Assert.Equal(["https://dashboard.example.net/app/topology.json"], handler.RequestedUrls);
    }

    [Fact]
    public async Task ARuntimeFileIsAskedForAtTheAddressResolveRelativeGives()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));

        await new RuntimeLoader(new HttpClient(handler) { BaseAddress = Site })
            .LoadDiagramAsync(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), CancellationToken.None);

        Assert.Equal(
            ["https://dashboard.example.net/app/runtime/uat.json", "https://dashboard.example.net/app/runtime/uat.svg"],
            handler.RequestedUrls.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ARuntimeFileThatNamesAnotherSiteStaysUnderRuntimeOfThisSite()
    {
        // Behind the runtime/ folder no path names another site, so ResolveRelative refuses none of them.
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));

        var diagram = await new RuntimeLoader(new HttpClient(handler) { BaseAddress = Site })
            .LoadDiagramAsync(new RuntimeIndexEntry("uat", "https://elsewhere.example/uat.json", "mailto:uat.svg"), CancellationToken.None);

        Assert.False(diagram.IsValid);
        Assert.All(handler.RequestedUrls, url => Assert.StartsWith("https://dashboard.example.net/app/runtime/", url, StringComparison.Ordinal));
        Assert.Equal(2, handler.RequestedUrls.Count());
        Assert.DoesNotContain(diagram.Errors, error => error.Contains("no path of this site", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheTopologyIsAskedForWithoutAClientAddressAsTheRelativePathItIs()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var client = new HttpClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() => new TopologyLoader(client).LoadAsync(CancellationToken.None));
        Assert.Empty(handler.RequestedUrls);
    }
}
