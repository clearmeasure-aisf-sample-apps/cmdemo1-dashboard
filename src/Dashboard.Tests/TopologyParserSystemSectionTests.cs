using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The four optional addresses of the <c>system</c> section refuse under one shared prefix: <c>system.</c>
/// (#2101). The prefix lives in one local const in <see cref="TopologyParser"/>, so this test pins all four
/// refusals together, in one place, and a typo in that one const would break all four sentences at once.
/// </summary>
public class TopologyParserSystemSectionTests
{
    [Fact]
    public void AllFourSystemAddressesRefusedTogetherUnderTheSystemPrefix()
    {
        var json = """
        {
          "system": {
            "slug": "demo",
            "repository": "example-org/demo-system",
            "deliveryUrl": "not/an/address",
            "costUrl": "relative/path.json",
            "deploymentsUrl": ""
          },
          "environments": []
        }
        """;

        var result = TopologyParser.Parse(json);

        Assert.Equal(
            [
                "system.repository: not an absolute http or https address.",
                "system.deliveryUrl: not an absolute http or https address.",
                "system.costUrl: not an absolute http or https address.",
                "system.deploymentsUrl: not an absolute http or https address.",
            ],
            result.Errors);

        Assert.All(result.Errors, error => Assert.StartsWith("system.", error));
        Assert.False(result.IsValid);
    }
}
