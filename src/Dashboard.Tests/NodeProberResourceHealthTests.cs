using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// Probe-level edge cases of <see cref="NodeProber.ReadResourceHealthAsync" />: what one answer for
/// <c>containers.json</c> becomes, without a <see cref="DashboardMonitor" /> around it. A courtesy like the other
/// optional reads: anything but a 200 with a JSON object that has <c>resources</c> is no report, and only the caller's
/// cancellation throws.
/// </summary>
public class NodeProberResourceHealthTests
{
    private static readonly Uri Containers = new("https://raw.githubusercontent.com/org/demo-system/status/containers.json");

    private const string AReport =
        """{ "generated": "2026-10-04T21:37:00Z", "resources": { "nonprod": [ { "id": "/s/kv", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:37:00Z" } } ] } }""";

    private readonly SignallingTimeProvider _time = new();

    private NodeProber Prober(StubHandler handler) => new(new HttpClient(handler), _time);

    private Task<ResourceHealthReport?> ReadAsync(HttpStatusCode status, string body) =>
        Prober(new StubHandler(_ => StubHandler.Answer(status, body))).ReadResourceHealthAsync(Containers, CancellationToken.None);

    [Fact]
    public async Task A200WithResourcesIsTheReportReadFromTheGivenAddress()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, AReport));

        var report = await Prober(handler).ReadResourceHealthAsync(Containers, CancellationToken.None);

        Assert.NotNull(report);
        Assert.Equal("available", report.Find("/s/kv")?.Status);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 21, 37, 0, TimeSpan.Zero), report.Generated);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(Containers, request.RequestUri);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.MovedPermanently)]
    public async Task AnAnswerThatIsNot2xxIsNoReportEvenWithAValidBody(HttpStatusCode status)
    {
        Assert.Null(await ReadAsync(status, AReport));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<!DOCTYPE html><html><body>Not found</body></html>")]
    [InlineData("404: Not Found")]
    [InlineData("{ \"resources\": ")]
    [InlineData("{ resources: {} }")]
    public async Task A200ThatIsNotJsonIsNoReport(string body)
    {
        Assert.Null(await ReadAsync(HttpStatusCode.OK, body));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "generated": "2026-10-04T21:37:00Z" }""")]
    [InlineData("""{ "Resources": { "nonprod": [] } }""")]
    [InlineData("""{ "resources": null }""")]
    [InlineData("""{ "resources": [] }""")]
    [InlineData("""{ "resources": "nonprod" }""")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("\"resources\"")]
    public async Task A200WithJsonButNoResourcesObjectIsNoReport(string body)
    {
        Assert.Null(await ReadAsync(HttpStatusCode.OK, body));
    }

    [Fact]
    public async Task A200WithAnEmptyResourcesObjectIsAnEmptyReportNotNoReport()
    {
        var report = await ReadAsync(HttpStatusCode.OK, """{ "resources": {} }""");

        Assert.NotNull(report);
        Assert.Empty(report.Readings);
        Assert.Null(report.Generated);
    }

    [Fact]
    public async Task ANetworkOrCorsFailureIsNoReport()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("TypeError: Failed to fetch"));

        Assert.Null(await Prober(handler).ReadResourceHealthAsync(Containers, CancellationToken.None));
    }

    [Fact]
    public async Task NoAnswerWithinTheTimeoutIsNoReport()
    {
        var handler = new StubHandler((_, cancellationToken) => StubHandler.NeverAsync(cancellationToken));

        var read = Prober(handler).ReadResourceHealthAsync(Containers, CancellationToken.None);
        await _time.TimerCreatedAsync();
        _time.Advance(NodeProber.DefaultTimeout - TimeSpan.FromSeconds(1));
        Assert.False(read.IsCompleted);
        _time.Advance(TimeSpan.FromSeconds(1));

        Assert.Null(await read);
    }

    [Fact]
    public async Task TheCallersCancellationThrows()
    {
        using var cancel = new CancellationTokenSource();
        var handler = new StubHandler((_, cancellationToken) => StubHandler.NeverAsync(cancellationToken));

        var read = Prober(handler).ReadResourceHealthAsync(Containers, cancel.Token);
        await cancel.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read);
    }
}
