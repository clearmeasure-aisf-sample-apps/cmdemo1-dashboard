using static Dashboard.Tests.NodeObservations;

namespace Dashboard.Tests;

/// <summary>
/// The words <see cref="EventDetector"/> names once and says in many sentences: the change arrow between what was
/// and what is now, the first check, and the node that serves traffic again. Each sentence is pinned letter for
/// letter, so a change to one shared word shows in every sentence that says it.
/// </summary>
public class EventDetectorWordingTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 27, 9, 0, 0, TimeSpan.Zero);
    private static readonly string[] Regions = ["westus3", "eastus2"];

    private static ServingAssessment Only(HealthState state, bool primary) =>
        ServingAssessment.Assess([new NodeHealth("uat-ui", null, primary, state)], null);

    private static ServingAssessment Rotation(HealthState[] primaries) =>
        ServingAssessment.Assess([.. NodeHealths.Primaries(Regions, primaries), NodeHealths.Standby(HealthState.Healthy, region: "japaneast")], HealthState.Healthy);

    [Fact]
    public void AVersionChangeSaysBothVersionsAcrossTheArrow()
    {
        var version = Assert.Single(EventCalls.Node(Seen(HealthState.Healthy, "1.0"), Seen(HealthState.Healthy, "1.1"), at: Now));

        Assert.Equal((EventKind.Version, "ui: 1.0 → 1.1, deployed"), (version.Kind, version.Text));
    }

    [Fact]
    public void ANodeUnhealthyAtTheFirstCheckSaysSo()
    {
        var first = Assert.Single(EventCalls.Node(Seen(HealthState.Pending, null), Seen(HealthState.Unhealthy, null, status: 503), at: Now));

        Assert.Equal((EventKind.Health, "ui: Unhealthy at the first check (HTTP 503)"), (first.Kind, first.Text));
    }

    [Fact]
    public void AnEntryUnhealthyAtTheFirstCheckSaysSo()
    {
        var after = Seen(HealthState.Healthy) with { Checks = HealthDetailTests.Detail(("DataAccess", "Unhealthy")) };

        var entry = Assert.Single(EventDetector.HealthChecks(null, after.Checks, "uat", "ui", "westus3", Now));

        Assert.Equal((EventKind.HealthCheck, "ui: DataAccess Unhealthy at the first check: DataAccess says unhealthy"), (entry.Kind, entry.Text));
    }

    [Fact]
    public void APrimaryThatServesAfterNothingCouldServesTrafficAgain()
    {
        var back = EventDetector.Serving(Only(HealthState.Unreachable, primary: true), Only(HealthState.Healthy, primary: true), "uat", "ui", Now);

        Assert.Equal((EventLevel.Good, "uat-ui serves traffic again."), (back!.Level, back.Text));
    }

    [Fact]
    public void AStandbyOnlyNodeThatServesAfterNothingCouldServesTrafficAgain()
    {
        var back = EventDetector.Serving(Only(HealthState.Unreachable, primary: false), Only(HealthState.Healthy, primary: false), "uat", "ui", Now);

        Assert.Equal((EventLevel.Good, "uat-ui serves traffic again."), (back!.Level, back.Text));
    }

    [Fact]
    public void APinnedChangeSaysBothPinsAcrossTheArrow()
    {
        var pinned = EventDetector.Pinned(PinnedVersions.Parse("""{ "ui": "1.0" }"""), PinnedVersions.Parse("""{ "ui": "1.1" }"""), "uat", "ui", Now);

        Assert.Equal("ui: pinned 1.0 → 1.1 in Git", pinned!.Text);
    }

    [Fact]
    public void ARotationFailoverAndFailbackSayTheRotationAcrossTheArrow()
    {
        var one = Rotation([HealthState.Unreachable, HealthState.Healthy]);
        var failedOver = Rotation([HealthState.Unreachable, HealthState.Unreachable]);

        Assert.Equal(
            ["Warning: Failover: the rotation → japaneast. No primary node is healthy; japaneast is expected to serve traffic."],
            ServingEventReaders.Events(one, failedOver, "uat", Now));
        Assert.Equal(
            ["Good: Failback: japaneast → the rotation. 1 of 2 regions serve traffic."],
            ServingEventReaders.Events(failedOver, one, "uat", Now));
    }
}
