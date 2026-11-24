namespace Dashboard.Tests;

/// <summary>
/// The rules <see cref="TopologyParser" /> says once for every array it reads (#1042): an entry that is no object is
/// refused by its place, once, at every level, and the reading goes on; an address that is not one is said in one of
/// two sentences, the required one also naming "missing".
/// </summary>
public class TopologyParserSharedRulesTests
{
    [Fact]
    public void EveryLevelRefusesAnEntryThatIsNoObjectByItsPlaceOnceAndReadsOn()
    {
        const string json = """
            {
              "environments": [
                42,
                { "name": "dev", "deployables": [
                    "app",
                    { "nodes": [ null, { "url": "https://a.example" }, [] ] }
                ] },
                true
              ]
            }
            """;

        var result = TopologyParser.Parse(json);

        Assert.Equal(
            [
                "environments[0]: not an object.",
                "environments[1].deployables[0]: not an object.",
                "environments[1].deployables[1].nodes[0]: not an object.",
                "environments[1].deployables[1].nodes[2]: not an object.",
                "environments[2]: not an object.",
            ],
            result.Errors);
    }

    [Fact]
    public void ARefusedNodeIsTheOnlyErrorAndItsNeighbourReadsCleanly()
    {
        const string json = """
            { "environments": [ { "name": "dev", "deployables": [ { "nodes": [ 1, { "url": "https://a.example" } ] } ] } ] }
            """;

        var result = TopologyParser.Parse(json);

        Assert.Equal(["environments[0].deployables[0].nodes[0]: not an object."], result.Errors);
    }

    [Fact]
    public void AnAddressThatIsNotOneIsSaidInItsTwoSentencesRequiredOrNot()
    {
        const string json = """
            {
              "environments": [ { "name": "dev", "versionsUrl": "ftp://x", "deployables": [
                { "frontDoor": 3, "nodes": [ {}, { "url": "not an address" } ] }
              ] } ]
            }
            """;

        var result = TopologyParser.Parse(json);

        Assert.Equal(
            [
                "environments[0].versionsUrl: not an absolute http or https address.",
                "environments[0].deployables[0].frontDoor: not an absolute http or https address.",
                "environments[0].deployables[0].nodes[0].url: missing or not an absolute http or https address.",
                "environments[0].deployables[0].nodes[1].url: missing or not an absolute http or https address.",
            ],
            result.Errors);
    }

    [Fact]
    public void WithoutARoleTheSecondNodeIsAStandbyAndTheFirstThePrimary()
    {
        const string json = """
            { "environments": [ { "name": "dev", "deployables": [ { "nodes": [
                { "url": "https://a.example" }, { "url": "https://b.example" } ] } ] } ] }
            """;

        var result = TopologyParser.Parse(json);

        Assert.True(result.IsValid, string.Join(" | ", result.Errors));
        var nodes = result.Topology!.Environments[0].Deployables[0].Nodes;
        Assert.Equal([NodeInfo.PrimaryRole, NodeInfo.StandbyRole], nodes.Select(node => node.Role));
    }
}
