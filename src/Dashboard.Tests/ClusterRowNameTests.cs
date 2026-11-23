namespace Dashboard.Tests;

/// <summary>
/// The one rule every row of the cluster's file shares (#1184): a node, a namespace, a pod or a volume without a name
/// is left out, and the named rows beside it stand. The rule lives in one place of <see cref="ClusterStatus" />, so it
/// is pinned here for each of the four row types and for each way a name can be missing, through
/// <see cref="ClusterStatus.Parse" /> (the readers are internal).
/// </summary>
public class ClusterRowNameTests
{
    public static TheoryData<string> NoName =>
    [
        "{}",
        """{ "name": null }""",
        """{ "name": "" }""",
        """{ "name": "   " }""",
        """{ "name": 7 }""",
        """{ "name": { "first": "x" } }""",
        """{ "Name": "x" }""",
    ];

    private static ClusterStatus Read(string json) => ClusterStatus.Parse(json).Value!;

    [Theory]
    [MemberData(nameof(NoName))]
    public void ANodeWithoutANameIsLeftOut(string unnamed) =>
        Assert.Equal(["a", "b"], Read($$"""{ "nodes": [ { "name": "a" }, {{unnamed}}, { "name": "b" } ] }""").Nodes.Select(node => node.Name));

    [Theory]
    [MemberData(nameof(NoName))]
    public void ANamespaceWithoutANameIsLeftOut(string unnamed) =>
        Assert.Equal(["a", "b"], Read($$"""{ "namespaces": [ { "name": "a" }, {{unnamed}}, { "name": "b" } ] }""").Namespaces.Select(space => space.Name));

    [Theory]
    [MemberData(nameof(NoName))]
    public void APodWithoutANameIsLeftOut(string unnamed) =>
        Assert.Equal(
            ["a", "b"],
            Read($$"""{ "namespaces": [ { "name": "apps", "pods": [ { "name": "a" }, {{unnamed}}, { "name": "b" } ] } ] }""").Pods.Select(row => row.Pod.Name));

    [Theory]
    [MemberData(nameof(NoName))]
    public void AVolumeWithoutANameIsLeftOut(string unnamed) =>
        Assert.Equal(
            ["a", "b"],
            Assert.Single(Read($$"""{ "namespaces": [ { "name": "apps", "volumes": [ { "name": "a" }, {{unnamed}}, { "name": "b" } ] } ] }""").Namespaces)
                .Volumes.Select(volume => volume.Name));

    [Fact]
    public void ANamespaceWithoutANameTakesItsPodsAndVolumesWithIt()
    {
        var status = Read("""{ "namespaces": [ { "pods": [ { "name": "p" } ], "volumes": [ { "name": "v" } ] } ] }""");

        Assert.Empty(status.Namespaces);
        Assert.Empty(status.Pods);
    }

    [Fact]
    public void TheNameOfEveryRowIsTrimmed()
    {
        var status = Read("""
            { "nodes": [ { "name": " node-1 " } ],
              "namespaces": [ { "name": "\tapps\n", "pods": [ { "name": "  web-1" } ], "volumes": [ { "name": "data  " } ] } ] }
            """);

        Assert.Equal("node-1", Assert.Single(status.Nodes).Name);
        var space = Assert.Single(status.Namespaces);
        Assert.Equal(("apps", "web-1", "data"), (space.Name, Assert.Single(space.Pods).Name, Assert.Single(space.Volumes).Name));
    }

    [Fact]
    public void ARowThatIsNoObjectIsLeftOut() =>
        Assert.Equal(["a"], Read("""{ "nodes": [ "a", 3, null, [ { "name": "x" } ], { "name": "a" } ] }""").Nodes.Select(node => node.Name));
}
