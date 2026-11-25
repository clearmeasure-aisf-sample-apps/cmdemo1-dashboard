using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The one home of the 'interval due' rule (#1780): when a reading is asked again, in the words the rule itself
/// states - nothing is due before it was first asked, and once it was, it is due again only when its interval has
/// passed, a read exactly one interval later going ahead. The monitor's file reads, the container app probe and the
/// database view's live-file read all compose it; <see cref="DashboardMonitorEdgeCaseTests" /> pins the monitor's
/// five-minute boundary through a round of checks, and <see cref="ContainerAppProbeTests" /> and
/// <see cref="DatabaseViewTests" /> pin the two public surfaces through their own rules. The home itself is internal
/// to an app that declares no <c>InternalsVisibleTo</c>, so it is reached by name under its own assembly, the way
/// <see cref="ProbeDefaultsEdgeCaseTests" /> reaches <c>ProbeDefaults</c>.
/// </summary>
public class IntervalDueEdgeCaseTests
{
    private static readonly DateTimeOffset ReadAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The internal <c>Dashboard.Health.IntervalDue</c>, reached by reflection under its own name, so the
    /// tests below write <c>IntervalDue.IsDue(...)</c> as the app does.</summary>
    private static class IntervalDue
    {
        public static bool IsDue(DateTimeOffset? last, DateTimeOffset now, TimeSpan interval)
        {
            var method = Type()
                .GetMethod("IsDue", BindingFlags.Public | BindingFlags.Static, [typeof(DateTimeOffset?), typeof(DateTimeOffset), typeof(TimeSpan)]);
            Assert.NotNull(method);
            return (bool)method.Invoke(null, [last, now, interval])!;
        }

        private static Type Type() => typeof(NodeProber).Assembly.GetType("Dashboard.Health.IntervalDue", throwOnError: true)!;
    }

    /// <summary>The rule has one home: a static class the app keeps to itself, with exactly the one method the rule
    /// takes - a nullable last asking, the now and the interval - and no second way to state the boundary beside it.</summary>
    [Fact]
    public void TheRuleHasOneStaticHomeAndOneMethod()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.IntervalDue", throwOnError: true)!;

        Assert.False(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed, "The home stays a static class.");

        var isDue = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name == "IsDue").ToList();
        Assert.Single(isDue);
        Assert.Equal([typeof(DateTimeOffset?), typeof(DateTimeOffset), typeof(TimeSpan)],
            isDue[0].GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(typeof(bool), isDue[0].ReturnType);
    }

    /// <summary>Nothing is due before it was first asked: a null last asking is due at every now, on every interval.</summary>
    [Fact]
    public void AReadingThatWasNeverAskedIsDue()
    {
        Assert.True(IntervalDue.IsDue(null, ReadAt, TimeSpan.FromSeconds(60)));
        Assert.True(IntervalDue.IsDue(null, ReadAt, TimeSpan.FromMinutes(5)));
    }

    /// <summary>The boundary the issue asked to state once, inclusive: a read exactly one interval later goes ahead,
    /// on every interval the callers bring - a second, the minute of the probe and the database read, and the five
    /// minutes of the monitor's file reads.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(300)]
    public void AReadingExactlyOneIntervalLaterGoesAhead(double seconds)
    {
        var interval = TimeSpan.FromSeconds(seconds);

        Assert.True(IntervalDue.IsDue(ReadAt, ReadAt.Add(interval), interval));
    }

    /// <summary>And not a moment before: one second short of the interval is not due, on the same intervals.</summary>
    [Theory]
    [InlineData(1, 0)]
    [InlineData(60, 59)]
    [InlineData(300, 299)]
    public void AReadingBeforeItsIntervalHasPassedIsNotDue(double seconds, double elapsedSeconds)
    {
        var interval = TimeSpan.FromSeconds(seconds);

        Assert.False(IntervalDue.IsDue(ReadAt, ReadAt.AddSeconds(elapsedSeconds), interval));
    }

    /// <summary>The two public surfaces compose the home and say what it says, on the same clock and the same
    /// intervals: the probe, from before its first ask to across its minute, and the database read, active, from
    /// before its first read to across its minute. The pause gate stays the view's own, and is pinned in
    /// <see cref="DatabaseViewTests" />.</summary>
    [Fact]
    public void TheProbeAndTheDatabaseReadSayWhatTheHomeSays()
    {
        var probe = new ContainerAppProbe(new ContainerAppTarget(new Uri("https://ca.example")));

        Assert.True(probe.IsDue(ReadAt));
        Assert.Equal(IntervalDue.IsDue(null, ReadAt, ContainerAppProbe.Interval), probe.IsDue(ReadAt));

        probe.Start(ReadAt);
        foreach (var elapsed in new[] { 0, 30, 59, 60, 3600 })
        {
            var now = ReadAt.AddSeconds(elapsed);
            Assert.Equal(IntervalDue.IsDue(ReadAt, now, ContainerAppProbe.Interval), probe.IsDue(now));
        }

        foreach (var last in new DateTimeOffset?[] { null, ReadAt })
        {
            foreach (var elapsed in new[] { 0, 30, 59, 60, 3600 })
            {
                var now = last is { } asked ? asked.AddSeconds(elapsed) : ReadAt;
                Assert.Equal(IntervalDue.IsDue(last, now, DatabaseRefresh.Interval), DatabaseRefresh.IsDue(active: true, last, now));
            }
        }
    }
}
