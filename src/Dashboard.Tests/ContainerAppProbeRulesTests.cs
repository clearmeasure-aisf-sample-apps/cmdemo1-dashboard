namespace Dashboard.Tests;

/// <summary>
/// The named rules <see cref="ContainerAppProbe.Record" /> asks in order - healthy at the health path, gone at the
/// health path, unhealthy at the health path, answered, still waking, and otherwise not responding - pinned one by
/// one and, above all, where two of them could both hold, so the order they are asked in cannot drift. The rules are
/// private to an app that declares no <c>InternalsVisibleTo</c>, so they are pinned through <c>Record</c>.
/// </summary>
public class ContainerAppProbeRulesTests
{
    private static readonly DateTimeOffset Opened = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly ContainerAppTarget Target =
        new(new Uri("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io"));

    private static ContainerAppState StateAfter(ContainerAppAnswer answer, DateTimeOffset? startedAt = null)
    {
        var probe = new ContainerAppProbe(Target);
        probe.Start(startedAt ?? Opened);
        probe.Record(answer);
        return probe.Status.State;
    }

    private static ContainerAppAnswer At(
        int? status, bool readsHealth = true, bool opaque = false, bool healthyPayload = false, DateTimeOffset? at = null) =>
        new(ContainerAppTarget.DefaultHealthPath, readsHealth, status, opaque, 85, at ?? Opened, HealthyPayload: healthyPayload);

    [Theory]
    [InlineData(200)]
    [InlineData(204)]
    [InlineData(299)]
    public void HealthyPayloadIn2xxAtHealthPathIsHealthy(int status) =>
        Assert.Equal(ContainerAppState.Healthy, StateAfter(At(status, healthyPayload: true)));

    [Theory]
    [InlineData(199)]
    [InlineData(300)]
    [InlineData(500)]
    public void HealthyPayloadOutside2xxIsUnhealthy(int status) =>
        Assert.Equal(ContainerAppState.Unhealthy, StateAfter(At(status, healthyPayload: true)));

    [Fact]
    public void NotFoundAtHealthPathIsGoneEvenWithHealthyPayloadMark() =>
        Assert.Equal(ContainerAppState.NotResponding, StateAfter(At(404, healthyPayload: true)));

    [Fact]
    public void NotFoundAtHealthPathIsGoneInsideGraceAndWhenOpaque() =>
        Assert.Equal(ContainerAppState.NotResponding, StateAfter(At(404, opaque: true)));

    [Theory]
    [InlineData(200)]
    [InlineData(302)]
    [InlineData(403)]
    [InlineData(405)]
    [InlineData(500)]
    [InlineData(503)]
    [InlineData(599)]
    [InlineData(600)]
    public void AnyOtherStatusAtHealthPathIsUnhealthy(int status) =>
        Assert.Equal(ContainerAppState.Unhealthy, StateAfter(At(status)));

    [Theory]
    [InlineData(200, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    [InlineData(null, true)]
    public void AnswerOffHealthPathIsOnlyResponding(int? status, bool opaque) =>
        Assert.Equal(
            ContainerAppState.Responding,
            StateAfter(At(status, readsHealth: false, opaque: opaque, healthyPayload: true)));

    [Fact]
    public void UnreadAnswerAtHealthPathIsRespondingNotWaking() =>
        Assert.Equal(ContainerAppState.Responding, StateAfter(At(null, opaque: true)));

    [Fact]
    public void SilenceIsWakingUntilTheGraceTickAndNotRespondingAtIt()
    {
        Assert.Equal(ContainerAppState.Waking, StateAfter(At(null, at: Opened + ContainerAppProbe.Grace - TimeSpan.FromTicks(1))));
        Assert.Equal(ContainerAppState.NotResponding, StateAfter(At(null, at: Opened + ContainerAppProbe.Grace)));
    }

    [Fact]
    public void SilenceAfterAnAnswerInTheRunIsNotRespondingInsideGrace()
    {
        var probe = new ContainerAppProbe(Target);
        probe.Start(Opened);
        probe.Record(At(200, healthyPayload: true));
        probe.Record(At(null, at: Opened + TimeSpan.FromSeconds(1)));
        Assert.Equal(ContainerAppState.NotResponding, probe.Status.State);
    }
}
