namespace Dashboard.Tests;

/// <summary>
/// What a tile is called on the page is owned by <see cref="TargetStatus.Label" />: <see cref="TargetStatus.FrontDoorName" />
/// for the public address, a node's region or else its name (<see cref="TargetStatus.NodeLabel" />). The health and the
/// version a tile is turned into carry the same word, and the source of <c>src/Dashboard</c> writes each rule once (#1007).
/// </summary>
public sealed class TargetLabelOwnershipTests
{
    private static readonly Uri West = new("https://app-west.example.com/");

    public static TheoryData<TargetKind, string, string?> Tiles => new()
    {
        { TargetKind.Node, "app-west", "westus3" },
        { TargetKind.Node, "app-west", null },
        { TargetKind.Node, "app-west", string.Empty },
        { TargetKind.Node, " ", " " },
        { TargetKind.FrontDoor, TargetStatus.FrontDoorName, null },
    };

    [Theory]
    [MemberData(nameof(Tiles))]
    public void TheVersionAndTheHealthOfATileCarryTheTilesLabel(TargetKind kind, string name, string? region)
    {
        var tile = new TargetStatus(kind, name, West, region);

        Assert.Equal(tile.Label, tile.ToNodeVersion().Label);
        Assert.Equal(tile.Label, tile.ToNodeHealth().Label);
    }

    [Fact]
    public void ANodeAnswersToItsRegionOrElseItsName()
    {
        Assert.Equal("westus3", TargetStatus.NodeLabel("app-west", "westus3"));
        Assert.Equal("app-west", TargetStatus.NodeLabel("app-west", null));
        Assert.Equal(string.Empty, TargetStatus.NodeLabel("app-west", string.Empty));
        Assert.Equal("westus3", new NodeHealth("app-west", "westus3", true, HealthState.Healthy).Label);
        Assert.Equal("app-west", new NodeHealth("app-west", null, true, HealthState.Healthy).Label);
    }

    [Fact]
    public void TheFrontDoorEndpointAnswersToTheOneFrontDoorWord()
    {
        var deployable = new DeployableInfo("Web", new Uri("https://web.example.com/"), DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, []);
        var tile = TargetStatus.ForFrontDoor(deployable)!;

        Assert.Equal("Front Door", TargetStatus.FrontDoorName);
        Assert.Equal(TargetStatus.FrontDoorName, tile.Label);
        Assert.Equal(TargetStatus.FrontDoorName, new TargetStatus(TargetKind.FrontDoor, "fd-endpoint", West, "westus3").Label);
    }

    /// <summary>
    /// The C# of <c>src/Dashboard</c> writes each rule once. <c>TargetTile.razor</c>'s <c>Title</c> still spells both
    /// out, since <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pins that line of markup word for word.
    /// </summary>
    [Fact]
    public void TheSourceWritesTheFrontDoorWordAndTheRegionOrNameRuleOnce()
    {
        var sources = Directory.EnumerateFiles(Path.Combine(SourceRoot(), "Dashboard"), "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(File.ReadAllLines)
            .SelectMany(lines => lines)
            .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            .ToList();

        Assert.Single(sources, line => line.Contains("\"Front Door\"", StringComparison.Ordinal));
        Assert.Single(sources, line => line.Contains("region ?? name", StringComparison.OrdinalIgnoreCase));
    }

    private static string SourceRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Dashboard", "Health", "TargetStatus.cs")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("src/Dashboard/Health/TargetStatus.cs was not found above the test's output.");
    }
}
