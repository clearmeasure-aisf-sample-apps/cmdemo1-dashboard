using Dashboard.Cluster;

namespace Dashboard.Tests;

/// <summary>
/// The service and each of its pools share the field words <c>powerState</c>, <c>provisioningState</c> and
/// <c>kubernetesVersion</c>: they describe the same Azure fields at both levels of <c>aks.json</c>. The consts of
/// the file-local home of those words must land the values at both levels, and the refusal sentence must keep
/// naming the same words the parser reads.
/// </summary>
public class AksServiceSharedFieldTests
{
    private const string File = """
        {
          "powerState": "the-service-power",
          "provisioningState": "the-service-provisioning",
          "kubernetesVersion": "the-service-version",
          "pools": [
            {
              "name": "the-pool",
              "powerState": "the-pool-power",
              "provisioningState": "the-pool-provisioning",
              "kubernetesVersion": "the-pool-version"
            }
          ]
        }
        """;

    [Fact]
    public void TheSharedFieldsLandAtTheServiceLevelAndThePoolLevel()
    {
        var parsed = AksService.Parse(File);

        Assert.Null(parsed.Problem);
        var service = Assert.IsType<AksService>(parsed.Value);
        Assert.Equal("the-service-power", service.PowerState);
        Assert.Equal("the-service-provisioning", service.ProvisioningState);
        Assert.Equal("the-service-version", service.KubernetesVersion);
        var pool = Assert.Single(service.Pools);
        Assert.Equal("the-pool-power", pool.PowerState);
        Assert.Equal("the-pool-provisioning", pool.ProvisioningState);
        Assert.Equal("the-pool-version", pool.KubernetesVersion);
    }

    [Fact]
    public void TheRefusalSentenceNamesTheFieldsTheParserReads()
    {
        var parsed = AksService.Parse("""{ "name": "aks-only" }""");

        Assert.Null(parsed.Value);
        Assert.Equal(
            "the file says nothing about the service: no availability, no powerState and no provisioningState",
            parsed.Problem);
    }
}
