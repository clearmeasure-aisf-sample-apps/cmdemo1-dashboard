namespace Dashboard.Tests;

/// <summary>
/// The <see cref="Nodes" /> object mother in <c>Fixtures/Nodes.cs</c>, pinned where it lives: the number one past the
/// four states a check leaves behind (<see cref="Unnamed" />), the reading of the pinned-version file the version
/// assessments are made against (<see cref="Pinned" />, which pins both of the deployables it names), and the three
/// regional <see cref="NodeVersion" /> builders (<see cref="West" />, <see cref="East" /> and <see cref="Central" />,
/// each over <c>(state, version)</c>). The state words themselves are held by <see cref="HealthStates" /> in
/// <c>TestSupport.cs</c>, which <see cref="HealthStatesObjectMotherTests" /> pins; this file holds the nodes, the
/// one-past sentinel and the reading that stand on them, so the version tests may name them from one place and a new
/// <c>HealthState</c> or a moved region is written once, not in every file.
/// </summary>
public class NodesObjectMotherTests
{
    // ------------------------------- the number one past the four states

    /// <summary><see cref="Unnamed" /> is the number one step past the four states a check leaves behind: the fourth
    /// named state is <see cref="HealthState.Unreachable" /> on its number three, and <c>Unnamed</c> stands on four -
    /// a state the enum does not name, which is why <c>Enum.IsDefined</c> says so and the name comes back nothing.</summary>
    [Fact]
    public void UnnamedIsTheNumberOnePastTheFourStates()
    {
        Assert.Equal(0, (int)HealthState.Pending);
        Assert.Equal(1, (int)HealthState.Healthy);
        Assert.Equal(2, (int)HealthState.Unhealthy);
        Assert.Equal(3, (int)HealthState.Unreachable);
        Assert.Equal(4, (int)Nodes.Unnamed);
        Assert.False(Enum.IsDefined<HealthState>(Nodes.Unnamed));
        Assert.Null(Enum.GetName<HealthState>(Nodes.Unnamed));
    }

    // ------------------------------- the reading of the pinned-version file

    /// <summary><see cref="Pinned" /> is a reading that was read: the file answered and is <c>Read</c>, with no
    /// reason standing behind it, and pins the two deployables it names - <c>dashboard</c> to 1.0.1 and <c>ui</c> to
    /// 2.4.7 - and nothing else: a deployable the file does not name has no entry, so its version is nothing.</summary>
    [Fact]
    public void PinnedIsTheReadingOfBothDeployablesItNames()
    {
        Assert.Equal(PinnedVersionsState.Read, Nodes.Pinned.State);
        Assert.Null(Nodes.Pinned.Detail);
        Assert.Equal("1.0.1", Nodes.Pinned.Of("dashboard"));
        Assert.Equal("2.4.7", Nodes.Pinned.Of("ui"));
        Assert.Equal(["dashboard", "ui"], Nodes.Pinned.Versions.Keys.OrderBy(name => name, StringComparer.Ordinal));
        Assert.Null(Nodes.Pinned.Of("api"));
    }

    /// <summary>The reading is the one the file's text parses to: the same <c>versions.json</c> the reader reads,
    /// parsed the way the reader parses it, gives the very reading the mother keeps - the same state, the same two
    /// deployables pinned to the same versions, and nothing else - so a test that stands on <c>Nodes.Pinned</c> stands
    /// on the answer <c>PinnedVersions.Parse</c> gives for that file, and not a copy.</summary>
    [Fact]
    public void PinnedIsWhatTheFileParsesTo()
    {
        var parsed = PinnedVersions.Parse("""{ "dashboard": "1.0.1", "ui": "2.4.7" }""");

        Assert.Equal(parsed.State, Nodes.Pinned.State);
        Assert.Equal(parsed.Of("dashboard"), Nodes.Pinned.Of("dashboard"));
        Assert.Equal(parsed.Of("ui"), Nodes.Pinned.Of("ui"));
        Assert.Equal(parsed.Detail, Nodes.Pinned.Detail);
        Assert.Equal(parsed.Versions.Count, Nodes.Pinned.Versions.Count);
    }

    // ------------------------------- the three regional node builders

    /// <summary>Each of the three builders says the region it names - <c>westus3</c>, <c>eastus2</c> and
    /// <c>centralus</c> - and nothing else: the label is the region and the node asks nothing of the topology behind
    /// it, so the region the test named is the label the node says, for every state and version the builder is
    /// handed.</summary>
    [Theory]
    [InlineData("westus3")]
    [InlineData("eastus2")]
    [InlineData("centralus")]
    public void EachBuilderNamesItsRegion(string region)
    {
        foreach (var state in new[] { HealthState.Healthy, HealthState.Pending, HealthState.Unhealthy, HealthState.Unreachable, Nodes.Unnamed })
        {
            var node = NodeOf(region, state, "2.4.7");

            Assert.Equal(region, node.Label);
        }
    }

    /// <summary>The region the test names is the region the builder says: <c>westus3</c> is the west node, <c>eastus2</c>
    /// the east node and <c>centralus</c> the central node, each of the state and version handed.</summary>
    private static NodeVersion NodeOf(string region, HealthState state, string? version) => region switch
    {
        "westus3" => Nodes.West(state, version),
        "eastus2" => Nodes.East(state, version),
        _ => Nodes.Central(state, version),
    };

    /// <summary>A builder keeps the state and the version it is handed, over every state the enum names and over the
    /// number it does not: the label is the region and the two parts stand where they are handed, whatever they are -
    /// a version of nothing included.</summary>
    [Theory]
    [InlineData(HealthState.Healthy, "2.4.7", "2.4.7")]
    [InlineData(HealthState.Healthy, null, null)]
    [InlineData(HealthState.Pending, "2.4.7", "2.4.7")]
    [InlineData(HealthState.Unhealthy, "", "")]
    [InlineData(HealthState.Unreachable, "2.4.6", "2.4.6")]
    [InlineData(Nodes.Unnamed, "2.4.7", "2.4.7")]
    public void ABuilderKeepsTheStateAndVersionItIsHanded(HealthState state, string? version, string? keptVersion)
    {
        foreach (var build in new[] { Nodes.West, Nodes.East, Nodes.Central })
        {
            var node = build(state, version);

            Assert.Equal(state, node.State);
            Assert.Equal(keptVersion, node.Version);
        }
    }

    /// <summary>The three builders make three different nodes of the same two parts: the label is what puts them
    /// apart, so the westus3 node and the eastus2 node of the same state and version are not one, and the centralus
    /// node is apart from both.</summary>
    [Fact]
    public void TheThreeBuildersMakeThreeApartNodes()
    {
        var west = Nodes.West(HealthState.Healthy, "2.4.7");
        var east = Nodes.East(HealthState.Healthy, "2.4.7");
        var central = Nodes.Central(HealthState.Healthy, "2.4.7");

        Assert.Equal("westus3", west.Label);
        Assert.Equal("eastus2", east.Label);
        Assert.Equal("centralus", central.Label);
        Assert.NotEqual(west, east);
        Assert.NotEqual(west, central);
        Assert.NotEqual(east, central);
    }
}
