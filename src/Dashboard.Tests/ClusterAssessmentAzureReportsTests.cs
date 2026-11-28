namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterAssessment.AzureReports" /> joins the state words of a read AKS service the way a sentence does,
/// in <see cref="Dashboard.Health.NamedList.Join" />: both the availability and the power state when both are said,
/// the one that is when only one is, and the provisioning state when neither is - no verdict at all, nothing is said.
/// </summary>
public class ClusterAssessmentAzureReportsTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static AksService Service(string? availability, string? power, string? provisioning) =>
        new(
            Generated: null,
            Name: null,
            ResourceGroup: null,
            Location: null,
            Availability: availability is { } state ? new AksAvailability(state, null, null, null) : null,
            PowerState: power,
            ProvisioningState: provisioning,
            KubernetesVersion: null,
            Tier: null,
            Pools: [],
            Metrics: null);

    /// <summary>Both state words are said, joined the way a sentence does: <c>Available and Running</c>.</summary>
    [Fact]
    public void BothTheAvailabilityAndThePowerStateAreJoinedWithAnd() =>
        Assert.Equal(
            "Azure reports the AKS service Available and Running.",
            ClusterAssessment.AzureReports(ClusterFixture.Read(Service("Available", "Running", null)), Generated, Utc));

    /// <summary>The availability alone is said alone: no <c> and </c> joins it to nothing.</summary>
    [Fact]
    public void OnlyTheAvailabilityIsSaidAloneWithoutAnAnd() =>
        Assert.Equal(
            "Azure reports the AKS service Available.",
            ClusterAssessment.AzureReports(ClusterFixture.Read(Service("Available", null, null)), Generated, Utc));

    /// <summary>The power state alone is said alone: no <c> and </c> joins it to nothing.</summary>
    [Fact]
    public void OnlyThePowerStateIsSaidAloneWithoutAnAnd() =>
        Assert.Equal(
            "Azure reports the AKS service Running.",
            ClusterAssessment.AzureReports(ClusterFixture.Read(Service(null, "Running", null)), Generated, Utc));

    /// <summary>No availability and no power state: the provisioning state stands where the verdict would be.</summary>
    [Fact]
    public void NeitherTheAvailabilityNorThePowerStateSaysTheProvisioningState() =>
        Assert.Equal(
            "Azure reports the AKS service Updating.",
            ClusterAssessment.AzureReports(ClusterFixture.Read(Service(null, null, "Updating")), Generated, Utc));

    /// <summary>No availability, no power state and no provisioning state: Azure's facts are said of nothing.</summary>
    [Fact]
    public void NoneOfTheThreeStatesIsSaid() =>
        Assert.Null(ClusterAssessment.AzureReports(ClusterFixture.Read(Service(null, null, null)), Generated, Utc));
}
