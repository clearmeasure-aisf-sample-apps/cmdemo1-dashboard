using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The no-answer shape of <see cref="NodeProber.ProbeContainerAppAsync" /> and <see cref="NodeProber.ProbeAsync" />:
/// the three nulls (StatusCode, LatencyMs) and Opaque == false that every failure arm hands back, and the exact
/// sentences they carry. The existing tests in <see cref="NodeProberTests" /> and <see cref="ContainerAppProbeTests" />
/// pin the states and the words loosely; this file pins the full shape and the character-for-character sentences.
/// </summary>
public class NodeProberNoAnswerTests
{
    private static readonly Uri Node = new("https://app-demo-tdd-ui.azurewebsites.net");
    private static readonly Uri App = new("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io");
    private static readonly ContainerAppTarget Target = new(App, "/_healthcheck");
    private readonly SignallingTimeProvider _time = new();

    private NodeProber Prober(StubHandler handler) => new(new HttpClient(handler), _time);

    /// <summary>
    /// A container app whose handler throws <see cref="HttpRequestException" /> on both the read and the no-cors
    /// request answers with no status, not opaque, no latency, and the exact network-failure sentence.
    /// </summary>
    [Fact]
    public async Task AContainerAppThatFailsTheNetworkAnswersWithNullsAndTheExactSentence()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("TypeError: Failed to fetch"));

        var answer = await Prober(handler).ProbeContainerAppAsync(Target, ProbeKind.Health, false, CancellationToken.None);

        Assert.Null(answer.StatusCode);
        Assert.False(answer.Opaque);
        Assert.Null(answer.LatencyMs);
        Assert.Equal("The browser got no answer: the network failed, or nothing answers at the address.", answer.Detail);
    }

    /// <summary>
    /// A container app that times out answers with the same three nulls/false and the exact timeout sentence
    /// for the default timeout.
    /// </summary>
    [Fact]
    public async Task AContainerAppThatTimesOutAnswersWithNullsAndTheTimeoutSentence()
    {
        var handler = new StubHandler((_, cancellationToken) => StubHandler.NeverAsync(cancellationToken));

        var probe = Prober(handler).ProbeContainerAppAsync(Target, ProbeKind.Health, false, CancellationToken.None);
        await _time.TimerCreatedAsync();
        _time.Advance(NodeProber.DefaultTimeout);
        var answer = await probe.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Null(answer.StatusCode);
        Assert.False(answer.Opaque);
        Assert.Null(answer.LatencyMs);
        Assert.Equal("No answer within 10 s.", answer.Detail);
    }

    /// <summary>
    /// A node whose handler throws <see cref="HttpRequestException" /> gives <see cref="ProbeResult.Detail" /> with
    /// the exact CORS sentence.
    /// </summary>
    [Fact]
    public async Task ANodeThatFailsTheNetworkGivesTheExactCorsSentence()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("TypeError: Failed to fetch"));

        var result = await Prober(handler).ProbeAsync(Node, "/_healthcheck", null, CancellationToken.None);

        Assert.Equal(HealthState.Unreachable, result.State);
        Assert.Null(result.StatusCode);
        Assert.Null(result.LatencyMs);
        Assert.Equal(
            "The browser could not read an answer: the network failed, or the node does not allow this origin (CORS).",
            result.Detail);
    }
}
