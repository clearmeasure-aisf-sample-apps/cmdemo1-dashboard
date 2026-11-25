using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The one number of the shared probe wait: <c>ProbeDefaults.DefaultTimeoutSeconds</c>, the ten whole seconds both
/// the shared <c>TimeSpan</c> (<c>ProbeDefaults.DefaultTimeout</c>) and the traffic button's
/// <see cref="TrafficPlan.AnswerWithinSeconds" /> are written from, so a page that checks and a traffic run that
/// gives up cannot be given different waits. The class that holds it, <c>ProbeDefaults</c>, is internal to an app
/// that declares no <c>InternalsVisibleTo</c>, so it is found by name in the namespace where it lives, the way
/// <see cref="ProbeDefaultsEdgeCaseTests" /> finds it.
/// </summary>
public class ProbeDefaultsSharedWaitSecondsEdgeCaseTests
{
    /// <summary>The internal <c>Dashboard.Health.ProbeDefaults.DefaultTimeoutSeconds</c>, a literal read by name.</summary>
    private static int SharedDefaultSeconds()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
        var field = type.GetField("DefaultTimeoutSeconds", BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral, "The shared wait in whole seconds is a const, so no caller keeps a copy of the number.");
        Assert.False(field.IsInitOnly);
        Assert.Equal(typeof(int), field.FieldType);
        return (int)field.GetRawConstantValue()!;
    }

    /// <summary>The internal <c>Dashboard.Health.ProbeDefaults.DefaultTimeout</c>, the shared wait as a <see cref="TimeSpan" />.</summary>
    private static TimeSpan SharedDefault()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
        var field = type.GetField("DefaultTimeout", BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsInitOnly, "The shared wait stays a static readonly TimeSpan, so a caller may still be given another one.");
        return (TimeSpan)field.GetValue(null)!;
    }

    /// <summary>The whole-second form of the shared wait is ten, and it is the same wait as the <see cref="TimeSpan" />.</summary>
    [Fact]
    public void TheSharedWaitHasAWholeSecondFormOfTen()
    {
        var seconds = SharedDefaultSeconds();

        Assert.Equal(10, seconds);
        Assert.Equal(SharedDefault().TotalSeconds, (double)seconds);
        Assert.Equal(TimeSpan.FromSeconds(seconds), SharedDefault());
    }

    /// <summary>
    /// The traffic button's wait is that same shared number, and it stays a public <c>const int</c>: the JS call of
    /// <c>traffic.js</c> and the sentence of <see cref="TrafficCount" /> both take it as a whole number of seconds.
    /// </summary>
    [Fact]
    public void TheTrafficWaitIsTheSharedWholeSecondsAndStaysAPublicConstInt()
    {
        var field = typeof(TrafficPlan).GetField(nameof(TrafficPlan.AnswerWithinSeconds), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral, "The traffic wait stays a const int, so the button and its sentence read the shared number.");
        Assert.True(field.IsPublic);
        Assert.Equal(typeof(int), field.FieldType);
        Assert.Equal(SharedDefaultSeconds(), field.GetRawConstantValue());
        Assert.Equal(10, TrafficPlan.AnswerWithinSeconds);
    }

    /// <summary>
    /// The wait the traffic button gives a request is the wait a check of the page gives, as its doc says: the same
    /// ten seconds the node prober waits by default.
    /// </summary>
    [Fact]
    public void TheTrafficWaitIsTheProbeWaitOfThePage()
    {
        Assert.Equal(SharedDefault(), TimeSpan.FromSeconds(TrafficPlan.AnswerWithinSeconds));
        Assert.Equal(NodeProber.DefaultTimeout, TimeSpan.FromSeconds(TrafficPlan.AnswerWithinSeconds));
    }

    /// <summary>
    /// The traffic wait has its duration beside its whole seconds, the shape <see cref="TrafficPlan.CheckEvery"/>
    /// has beside <see cref="TrafficPlan.CheckEverySeconds"/> (#1795): a public static readonly <see cref="TimeSpan" />
    /// that is the very shared wait, so a caller that holds a length reads it instead of rebuilding one from the
    /// seconds.
    /// </summary>
    [Fact]
    public void TheTrafficWaitHasADurationBesideItsWholeSeconds()
    {
        var field = typeof(TrafficPlan).GetField(nameof(TrafficPlan.AnswerWithin), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsInitOnly, "The traffic wait stays a static readonly TimeSpan, the way ProbeDefaults.DefaultTimeout is.");
        Assert.Equal(typeof(TimeSpan), field.FieldType);
        Assert.Equal(SharedDefault(), TrafficPlan.AnswerWithin);
        Assert.Equal(TimeSpan.FromSeconds(TrafficPlan.AnswerWithinSeconds), TrafficPlan.AnswerWithin);
    }
}
