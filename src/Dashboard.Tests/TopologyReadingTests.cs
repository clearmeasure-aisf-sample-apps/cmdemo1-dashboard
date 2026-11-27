using Xunit.Sdk;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="TopologyReading"/>'s promise to the tests that read a topology through it: a file the parser refuses
/// fails the test with the parser's own messages, and a file that was meant to be refused fails it when it reads.
/// </summary>
public class TopologyReadingTests
{
    private const string Good = """{ "system": { "slug": "demo", "name": "Demo" }, "environments": [ { "name": "tdd" } ] }""";

    private const string TwoProblems = """{ "environments": [ { "tier": "nonprod" }, "uat" ] }""";

    [Fact]
    public void AFileThatReadsIsHandedBackAsItsTopology()
    {
        var topology = TopologyReading.Valid(Good);

        Assert.Equal("demo", topology.System.Slug);
        Assert.Equal(["tdd"], topology.Environments.Select(environment => environment.Name));
    }

    [Fact]
    public void AFileTheParserRefusesFailsWithEveryMessageItGave()
    {
        var failure = Assert.ThrowsAny<XunitException>(() => TopologyReading.Valid(TwoProblems));

        Assert.Contains(
            "environments[0].name: missing or empty. | environments[1]: not an object.",
            failure.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void AFileThatWasMeantToBeRefusedButReadsFails()
    {
        Assert.ThrowsAny<XunitException>(() => TopologyReading.Errors(Good));
        Assert.ThrowsAny<XunitException>(() => TopologyReading.InvalidDeployable("""{ "name": "ui", "nodes": [] }"""));
    }

    [Fact]
    public void ARefusedFileHandsBackTheParserMessagesInTheirOrder()
    {
        Assert.Equal(
            ["environments[0].name: missing or empty.", "environments[1]: not an object."],
            TopologyReading.Errors(TwoProblems));
        Assert.Equal("The file is empty.", Assert.Single(TopologyReading.Errors(null)));
    }

    [Fact]
    public void ThePartsAreReadFromTheLeastFileAroundThem()
    {
        Assert.Equal("uat", TopologyReading.Environment("""{ "name": "uat" }""").Name);
        Assert.Equal("ui", TopologyReading.Deployable("""{ "name": "ui", "nodes": [] }""").Name);
        Assert.Equal("west", TopologyReading.Node("""{ "name": "west", "url": "https://west.example.net" }""").Name);
        Assert.Null(TopologyReading.Cluster("null"));
        Assert.Null(TopologyReading.Generated("null"));
    }
}
