namespace Dashboard.Tests;

/// <summary>
/// The reading of a topology, for tests: one way in that fails with the parser's own messages rather than with a
/// null reference two assertions later. Like <see cref="Optics"/> and <c>ClusterFixture</c> for the monitor and the
/// cluster, the small wrappers wrap one part of a topology in the least file that reads, and hand that part back.
/// </summary>
internal static class TopologyReading
{
    /// <summary>Reads a file that must read; a refused file fails with the parser's messages, joined by " | ".</summary>
    public static Topology Valid(string json)
    {
        var result = TopologyParser.Parse(json);

        Assert.True(result.IsValid, string.Join(" | ", result.Errors));
        Assert.Empty(result.Errors);
        return result.Topology!;
    }

    /// <summary>Reads a file that must be refused, and hands back the messages the parser gave.</summary>
    public static IReadOnlyList<string> Errors(string? json)
    {
        var result = TopologyParser.Parse(json);

        Assert.False(result.IsValid);
        Assert.Null(result.Topology);
        Assert.NotEmpty(result.Errors);
        return result.Errors;
    }

    public static DateTimeOffset? Generated(string generated) =>
        Valid($$"""{ "generated": {{generated}}, "environments": [] }""").Generated;

    public static EnvironmentInfo Environment(string environment) =>
        Valid($$"""{ "environments": [ {{environment}} ] }""").Environments[0];

    public static DeployableInfo Deployable(string deployable) =>
        Environment($$"""{ "name": "tdd", "deployables": [ {{deployable}} ] }""").Deployables[0];

    public static IReadOnlyList<string> InvalidDeployable(string deployable) =>
        Errors($$"""{ "environments": [ { "name": "tdd", "deployables": [ {{deployable}} ] } ] }""");

    public static NodeInfo Node(string node) => Deployable($$"""{ "nodes": [ {{node}} ] }""").Nodes[0];

    public static IReadOnlyList<PageLink>? Pages(string pages) =>
        Environment($$"""{ "name": "tdd", "pages": {{pages}} }""").Pages;

    public static ClusterInfo? Cluster(string cluster) =>
        Valid($$"""{ "environments": [], "cluster": {{cluster}} }""").Cluster;
}
