using Dashboard.Cluster;

namespace Dashboard.Tests;

/// <summary>
/// Each part of <see cref="AksService" /> is read from its own word of <c>aks.json</c> (#1137). Every part below
/// carries a value no other part carries, so a reader that stood two parts in each other's place - in particular the
/// six <see langword="string" /> parts, which the compiler cannot tell apart - turns this red. The shipped sample
/// cannot say as much: it writes <c>Running</c>, <c>Succeeded</c> and the Kubernetes version more than once.
/// </summary>
public class AksServicePartsTests
{
    private const string File = """
        {
          "generated": "2026-11-25T18:50:40Z",
          "name": "the-name",
          "resourceGroup": "the-resource-group",
          "location": "the-location",
          "availability": { "state": "the-availability" },
          "powerState": "the-power-state",
          "provisioningState": "the-provisioning-state",
          "kubernetesVersion": "the-kubernetes-version",
          "tier": "the-tier",
          "pools": [ { "name": "the-pool" } ],
          "metrics": { "windowMinutes": 7 }
        }
        """;

    [Fact]
    public void EachPartIsReadFromItsOwnWord()
    {
        var parsed = AksService.Parse(File);

        Assert.Null(parsed.Problem);
        var service = Assert.IsType<AksService>(parsed.Value);
        Assert.Equal(new DateTimeOffset(2026, 11, 25, 18, 50, 40, TimeSpan.Zero), service.Generated);
        Assert.Equal("the-name", service.Name);
        Assert.Equal("the-resource-group", service.ResourceGroup);
        Assert.Equal("the-location", service.Location);
        Assert.Equal(new AksAvailability("the-availability", null, null, null), service.Availability);
        Assert.Equal("the-power-state", service.PowerState);
        Assert.Equal("the-provisioning-state", service.ProvisioningState);
        Assert.Equal("the-kubernetes-version", service.KubernetesVersion);
        Assert.Equal("the-tier", service.Tier);
        Assert.Equal(new AksPool("the-pool", null, null, null, null, null, null, null), Assert.Single(service.Pools));
        Assert.Equal(new AksMetrics(7, null, null, null), service.Metrics);
    }
}
