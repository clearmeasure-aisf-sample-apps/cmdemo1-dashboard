using System.Net;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The shared probe default: the ten seconds that <see cref="NodeProber" />, <see cref="PinnedVersionsReader" /> and
/// <see cref="ClusterReader" /> all wait by default. The class that holds it, <c>ProbeDefaults</c>, is internal to an
/// app that declares no <c>InternalsVisibleTo</c>, so the test finds it by name in the namespace where it lives.
/// </summary>
public class ProbeDefaultsEdgeCaseTests
{
    private static TimeSpan SharedDefault()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
        Assert.False(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed, "ProbeDefaults is a static class.");

        var field = type.GetField("DefaultTimeout", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field);
        Assert.True(field.IsInitOnly);
        return (TimeSpan)field.GetValue(null)!;
    }

    [Fact]
    public void TheSharedDefaultIsTenSeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(10), SharedDefault());
    }

    [Fact]
    public void TheNodeProberAndBothReadersDefaultToTheSharedValue()
    {
        var shared = SharedDefault();
        var time = new SignallingTimeProvider();
        using var http = new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound)));

        Assert.Equal(shared, NodeProber.DefaultTimeout);
        Assert.Equal(shared, new NodeProber(http, time).Timeout);
        Assert.Equal(shared, new PinnedVersionsReader(http, time).Timeout);
        Assert.Equal(shared, new ClusterReader(http, time).Timeout);
    }

    /// <summary>The internal <c>Dashboard.Health.ProbeDefaults</c>, reached by reflection under its own name, so the
    /// tests below write <c>ProbeDefaults.NoAnswerWithin(...)</c> as the app does.</summary>
    private static class ProbeDefaults
    {
        public static string NoAnswerWithin(TimeSpan timeout)
        {
            var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
            var method = type.GetMethod("NoAnswerWithin", BindingFlags.Public | BindingFlags.Static, [typeof(TimeSpan)]);
            Assert.NotNull(method);
            return (string)method.Invoke(null, [timeout])!;
        }
    }

    /// <summary>The timeout clause reads the timeout in whole seconds, rounded: ten says ten, two and a half says
    /// the three it rounds to, and zero says zero.</summary>
    [Theory]
    [InlineData(10, "no answer within 10 s")]
    [InlineData(2.5, "no answer within 3 s")]
    [InlineData(0, "no answer within 0 s")]
    public void TheTimeoutClauseReadsWholeSeconds(double seconds, string expected)
    {
        Assert.Equal(expected, ProbeDefaults.NoAnswerWithin(TimeSpan.FromSeconds(seconds)));
    }

    /// <summary>A reader of another culture reads the same clause: the seconds are formatted in the invariant culture.</summary>
    [Theory]
    [InlineData("fi-FI")]
    [InlineData("ru-RU")]
    public void TheTimeoutClauseIsTheSameInEveryCulture(string cultureName)
    {
        var invariant = CultureScope.Says(System.Globalization.CultureInfo.InvariantCulture, () => ProbeDefaults.NoAnswerWithin(TimeSpan.FromSeconds(2.5)));

        Assert.Equal(invariant, CultureScope.Says(cultureName, () => ProbeDefaults.NoAnswerWithin(TimeSpan.FromSeconds(2.5))));
        Assert.Equal("no answer within 3 s", invariant);
    }

    /// <summary>The node check and both readers say the one clause when the same timeout passes with no answer.</summary>
    [Fact]
    public async Task TheNodeProberAndBothReadersSayTheSharedTimeoutClause()
    {
        var timeout = TimeSpan.FromSeconds(2.5);
        var clause = ProbeDefaults.NoAnswerWithin(timeout);
        using var http = new HttpClient(new StubHandler((_, cancellation) => StubHandler.NeverAsync(cancellation)));

        var nodeTime = new SignallingTimeProvider();
        var node = new NodeProber(http, nodeTime) { Timeout = timeout }
            .ProbeAsync(new Uri("https://node.example/"), "/_healthcheck", null, CancellationToken.None);
        await nodeTime.TimerCreatedAsync();
        nodeTime.Advance(timeout);

        var pinnedTime = new SignallingTimeProvider();
        var pinned = new PinnedVersionsReader(http, pinnedTime) { Timeout = timeout }
            .ReadAsync(new Uri("https://pins.example/versions.json"), CancellationToken.None);
        await pinnedTime.TimerCreatedAsync();
        pinnedTime.Advance(timeout);

        var clusterTime = new SignallingTimeProvider();
        var cluster = new ClusterReader(http, clusterTime) { Timeout = timeout }
            .ReadStatusAsync(new Uri("https://cluster.example/status.json"), CancellationToken.None);
        await clusterTime.TimerCreatedAsync();
        clusterTime.Advance(timeout);

        Assert.Contains(clause, (await node).Detail!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("No answer within 3 s.", (await node).Detail);
        Assert.Equal(clause, (await pinned).Detail);
        Assert.Equal(clause, (await cluster).Detail);
    }
}
