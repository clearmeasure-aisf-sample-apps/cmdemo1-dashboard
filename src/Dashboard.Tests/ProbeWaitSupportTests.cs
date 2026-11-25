using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The tests of the one shared helper <see cref="ProbeWait" /> gathers: the shared probe wait - the one value
/// <see cref="NodeProber" />, <see cref="PinnedVersionsReader" /> and <see cref="ClusterReader" /> all hold by
/// default, the four guard files that used to pin it by their own literals now pin through it.
/// </summary>
public class ProbeWaitSupportTests
{
    /// <summary>The one new test that still states the number: the shared wait is ten seconds, the value the four
    /// guard files pin by their own literals and the one <see cref="ProbeDefaultsEdgeCaseTests" /> pins by name -
    /// kept so the value itself stays guarded while the other tests say only "the same wait as the shared value".</summary>
    [Fact]
    public void TheSharedWaitIsTenSeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(10), ProbeWait.SharedWait());
    }

    /// <summary>The helper reads the field it names, and only that field: the class it reaches is the internal
    /// static <c>ProbeDefaults</c>, the field is the public static init-only <c>TimeSpan</c> named
    /// <c>DefaultTimeout</c>, and the helper's answer is the very value read straight off that field - so a helper
    /// that drifts from the app's one home of the wait fails here rather than quietly agreeing with assertions
    /// that no longer mean what they say.</summary>
    [Fact]
    public void TheSharedWaitIsTheOneFieldTheProbeDefaultsNames()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;

        Assert.False(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed, "ProbeDefaults is a static class.");

        var field = type.GetField("DefaultTimeout", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field);
        Assert.True(field.IsInitOnly);
        Assert.Equal(typeof(TimeSpan), field.FieldType);

        var direct = (TimeSpan)field.GetValue(null)!;
        Assert.Equal(direct, ProbeWait.SharedWait());
    }
}
