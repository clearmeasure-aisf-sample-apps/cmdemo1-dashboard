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
}
