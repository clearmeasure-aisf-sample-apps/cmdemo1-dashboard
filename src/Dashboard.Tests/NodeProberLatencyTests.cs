using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The one rule that turns a probe's waited time into whole milliseconds, pinned on both paths that write a
/// latency: <see cref="NodeProber.ProbeAsync" /> and <see cref="NodeProber.ProbeContainerAppAsync" />. Both go
/// through the prober's one private rounding, so an advance of the same amount must land on the same number on
/// either path, and the half-step lands where <see cref="Math.Round(double)" />'s default half-to-even rounding
/// puts it today - not where <see cref="Dashboard.Health.JsonRead" />'s away-from-zero rounding would.
/// </summary>
public class NodeProberLatencyTests
{
    private static readonly Uri Node = new("https://app-demo-tdd-ui.azurewebsites.net");
    private static readonly Uri App = new("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io");
    private static readonly ContainerAppTarget AppTarget = new(App, "/_healthcheck");
    private readonly SignallingTimeProvider _time = new();

    private NodeProber Prober(StubHandler handler) => new(new HttpClient(handler), _time);

    [Theory]
    [InlineData(42.4, 42)]
    [InlineData(42.6, 43)]
    [InlineData(42.5, 42)]
    public async Task ProbeAsyncRoundsTheWaitToWholeMillisecondsTheWayMathRoundDoes(double advanceMs, int latencyMs)
    {
        var handler = new StubHandler((request, _) =>
        {
            _time.Advance(TimeSpan.FromMilliseconds(advanceMs));
            return Task.FromResult(StubHandler.Answer(HttpStatusCode.OK));
        });

        var result = await Prober(handler).ProbeAsync(Node, "/_healthcheck", null, CancellationToken.None);

        Assert.Equal(latencyMs, result.LatencyMs);
    }

    [Theory]
    [InlineData(42.4, 42)]
    [InlineData(42.6, 43)]
    [InlineData(42.5, 42)]
    public async Task ProbeContainerAppAsyncRoundsTheWaitToWholeMillisecondsTheWayMathRoundDoes(double advanceMs, int latencyMs)
    {
        var handler = new StubHandler((request, _) =>
        {
            _time.Advance(TimeSpan.FromMilliseconds(advanceMs));
            return Task.FromResult(StubHandler.Answer(HttpStatusCode.OK, "Healthy"));
        });

        var answer = await Prober(handler).ProbeContainerAppAsync(AppTarget, ProbeKind.Health, false, CancellationToken.None);

        Assert.Equal(latencyMs, answer.LatencyMs);
    }
}
