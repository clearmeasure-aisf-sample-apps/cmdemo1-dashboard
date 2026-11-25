namespace Dashboard.Tests;

/// <summary>
/// Issue #1802: <see cref="ContainerAppProbe.Start" /> decides whether a probe starts a new run by composing the one
/// interval rule (<c>IntervalDue.IsDue</c>) with <see cref="ContainerAppProbe.Asleep" />, rather than stating the
/// nullable comparison and the inclusive boundary a fourth time. These tests pin the behavior that must not change
/// (nothing probed yet starts a run; a gap of exactly <see cref="ContainerAppProbe.Asleep" /> starts one; a tick less
/// does not) and that the probe's source no longer writes the rule itself.
/// </summary>
public class ContainerAppProbeNewRunIntervalDueTests
{
    private static readonly DateTimeOffset Opened = new(2026, 11, 24, 9, 0, 0, TimeSpan.Zero);

    private static ContainerAppProbe NewProbe() => new(new ContainerAppTarget(new Uri("https://app.example.test/")));

    private static ContainerAppAnswer Heard(DateTimeOffset at) =>
        new("/_healthcheck", ReadsHealth: true, StatusCode: 200, Opaque: false, LatencyMs: 10, CheckedAt: at, HealthyPayload: true);

    private static ContainerAppAnswer Silent(DateTimeOffset at) =>
        new("/_healthcheck", ReadsHealth: true, StatusCode: null, Opaque: false, LatencyMs: null, CheckedAt: at, Detail: "timeout");

    /// <summary>The state after the app answered on its first probe and then stayed silent on a probe after <paramref name="gap"/>.</summary>
    private static ContainerAppState StateAfterSilenceFollowing(TimeSpan gap)
    {
        var probe = NewProbe();
        probe.Start(Opened);
        probe.Record(Heard(Opened));
        probe.Start(Opened + gap);
        probe.Record(Silent(Opened + gap));
        return probe.Status.State;
    }

    [Fact]
    public void TheFirstProbeStartsARunSoASilenceIsWaking()
    {
        var probe = NewProbe();
        probe.Start(Opened);
        probe.Record(Silent(Opened));

        Assert.Equal(ContainerAppState.Waking, probe.Status.State);
    }

    [Fact]
    public void AGapOfExactlyAsleepStartsANewRunTheBoundaryIsInclusive() =>
        Assert.Equal(ContainerAppState.Waking, StateAfterSilenceFollowing(ContainerAppProbe.Asleep));

    [Fact]
    public void AGapLongerThanAsleepStartsANewRun() =>
        Assert.Equal(ContainerAppState.Waking, StateAfterSilenceFollowing(ContainerAppProbe.Asleep + TimeSpan.FromMinutes(1)));

    [Fact]
    public void AGapOneTickShortOfAsleepStaysInTheRunThatAlreadyAnswered() =>
        Assert.Equal(ContainerAppState.NotResponding, StateAfterSilenceFollowing(ContainerAppProbe.Asleep - TimeSpan.FromTicks(1)));

    [Fact]
    public void TheGapIsCountedFromTheLastProbeNotTheFirst()
    {
        var probe = NewProbe();
        probe.Start(Opened);
        probe.Record(Heard(Opened));
        var later = Opened + ContainerAppProbe.Interval;
        probe.Start(later);
        var nextAt = later + ContainerAppProbe.Asleep - TimeSpan.FromTicks(1);
        probe.Start(nextAt);
        probe.Record(Silent(nextAt));

        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
    }

    /// <summary>The probe composes the shared home for both its questions and writes the nullable comparison nowhere itself.</summary>
    [Fact]
    public void TheProbeComposesTheSharedHomeForTheNewRunRule()
    {
        var text = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "ContainerAppProbe.cs")));

        Assert.Contains("IntervalDue.IsDue(_lastProbeAt, now, Interval)", text, StringComparison.Ordinal);
        Assert.Contains("IntervalDue.IsDue(_lastProbeAt, now, Asleep)", text, StringComparison.Ordinal);
        Assert.DoesNotContain("_lastProbeAt is not {", text, StringComparison.Ordinal);
        Assert.DoesNotContain(">= Asleep", text, StringComparison.Ordinal);
    }
}
