namespace Dashboard.Tests;

/// <summary>
/// Boundary, null/empty and fixed-value tests of how a <see cref="ClusterVolume" /> is read out of the cluster's
/// status file through the public <see cref="ClusterStatus.Parse" />. <see cref="ClusterVolumeEdgeCaseTests" /> pins
/// the record itself; <see cref="ClusterParserTests" /> pins the sample volume, a volume with only a name and a
/// volume without one; <see cref="ParsedEdgeCaseTests" /> pins the name of only spaces. What is pinned here is the
/// rest: the name and the phase trimmed, an empty phase read as none, a capacity that is no count of bytes read as
/// none, and a <c>volumes</c> part that is no array read as no volumes. Every input is a fixed string.
/// </summary>
public class ClusterVolumeReadEdgeCaseTests
{
    [Fact]
    public void TheNameAndThePhaseAreReadTrimmed()
    {
        var volume = ReadSingleVolume("""{ "name": "  data-db-0\t", "capacity": 1024, "phase": " Bound " }""");

        Assert.Equal(new ClusterVolume("data-db-0", 1024d, "Bound"), volume);
    }

    [Theory]
    [InlineData("\"phase\": \"\"")]
    [InlineData("\"phase\": \"   \"")]
    [InlineData("\"phase\": null")]
    [InlineData("\"phase\": 3")]
    [InlineData("\"phase\": true")]
    [InlineData("\"phase\": [\"Bound\"]")]
    public void APhaseThatIsNoWordIsReadAsNone(string phase)
    {
        var volume = ReadSingleVolume($$"""{ "name": "data", "capacity": 1024, {{phase}} }""");

        Assert.Equal(new ClusterVolume("data", 1024d, null), volume);
    }

    [Theory]
    [InlineData("Bound")]
    [InlineData("bound")]
    [InlineData("Available")]
    public void APhaseIsKeptInTheCaseAndWordItIsWritten(string phase)
    {
        var volume = ReadSingleVolume($$"""{ "name": "data", "phase": "{{phase}}" }""");

        Assert.Equal(phase, volume.Phase);
    }

    [Theory]
    [InlineData("0", 0d)]
    [InlineData("0.5", 0.5d)]
    [InlineData("1", 1d)]
    [InlineData("8589934592", 8589934592d)]
    [InlineData("1e3", 1000d)]
    [InlineData("1.7976931348623157E+308", double.MaxValue)]
    public void ACapacityThatIsNotNegativeIsKeptAsTheNumberItIs(string capacity, double expected)
    {
        var volume = ReadSingleVolume($$"""{ "name": "data", "capacity": {{capacity}}, "phase": "Bound" }""");

        Assert.Equal(expected, volume.Capacity);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-0.5")]
    [InlineData("1e400")]
    [InlineData("\"8589934592\"")]
    [InlineData("\"8Gi\"")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("{ \"bytes\": 1024 }")]
    [InlineData("[1024]")]
    public void ACapacityThatIsNoCountOfBytesIsReadAsNone(string capacity)
    {
        var volume = ReadSingleVolume($$"""{ "name": "data", "capacity": {{capacity}}, "phase": "Bound" }""");

        Assert.Equal(new ClusterVolume("data", null, "Bound"), volume);
    }

    [Theory]
    [InlineData("\"name\": \"\"")]
    [InlineData("\"name\": null")]
    [InlineData("\"name\": 7")]
    [InlineData("\"name\": { \"value\": \"data\" }")]
    public void AVolumeWhoseNameIsNoWordIsLeftOutAndTheOthersKeepTheirOrder(string name)
    {
        var space = ReadNamespace(
            $$"""[ { "name": "first" }, { {{name}}, "capacity": 1024, "phase": "Bound" }, { "name": "last" } ]""");

        Assert.Equal(["first", "last"], space.Volumes.Select(volume => volume.Name));
    }

    [Fact]
    public void ElementsThatAreNoObjectsAreLeftOut()
    {
        var space = ReadNamespace("""[ "data", 1024, null, true, [ { "name": "nested" } ], { "name": "data" } ]""");

        Assert.Equal(new ClusterVolume("data", null, null), Assert.Single(space.Volumes));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{ \"name\": \"data\" }")]
    [InlineData("\"data\"")]
    [InlineData("[]")]
    public void VolumesThatAreNoArrayOrAnEmptyOneAreNoVolumes(string volumes)
    {
        var space = ReadNamespace(volumes);

        Assert.Empty(space.Volumes);
    }

    [Fact]
    public void ANamespaceWithoutVolumesHasNone()
    {
        var parsed = ClusterStatus.Parse("""{ "namespaces": [ { "name": "apps" } ] }""");

        Assert.Empty(Assert.Single(parsed.Value!.Namespaces).Volumes);
    }

    [Fact]
    public void TwoVolumesWithTheSameNameAreBothKept()
    {
        var space = ReadNamespace(
            """[ { "name": "data", "capacity": 1024, "phase": "Bound" }, { "name": "data", "capacity": 2048, "phase": "Pending" } ]""");

        Assert.Equal(
            [new ClusterVolume("data", 1024d, "Bound"), new ClusterVolume("data", 2048d, "Pending")],
            space.Volumes);
    }

    [Fact]
    public void UnknownFieldsOfAVolumeAreIgnored()
    {
        var volume = ReadSingleVolume(
            """{ "name": "data", "capacity": 1024, "phase": "Bound", "storageClass": "managed-csi", "accessModes": ["ReadWriteOnce"] }""");

        Assert.Equal(new ClusterVolume("data", 1024d, "Bound"), volume);
    }

    private static ClusterNamespace ReadNamespace(string volumesJson)
    {
        var parsed = ClusterStatus.Parse($$"""{ "namespaces": [ { "name": "apps", "volumes": {{volumesJson}} } ] }""");

        Assert.Null(parsed.Problem);
        return Assert.Single(parsed.Value!.Namespaces);
    }

    private static ClusterVolume ReadSingleVolume(string volumeJson) =>
        Assert.Single(ReadNamespace($"[ {volumeJson} ]").Volumes);
}
