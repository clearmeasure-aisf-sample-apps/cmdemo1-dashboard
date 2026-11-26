namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still open for <see cref="DeployableStatus" />, declared in
/// <c>src/Dashboard/Health/TargetStatus.cs</c>: what one row of <c>topology.json</c> leaves as tiles, and what the four
/// memories of a round - the pin, the build, the version it was read for and the serving decision - hold.
/// <para>
/// <see cref="DeployableStatusEdgeCaseTests" /> already holds the class's edge cases under the name this issue asked
/// for, and <see cref="DeployableStatusBoundaryEdgeCaseTests" /> the build paths and roles at the ends of the names and
/// the pin a row starts on, so these stand in a file of their own and leave every one of those exactly as they are.
/// Between them and <see cref="TargetStatusTests" />, <see cref="TargetStatusEdgeCaseTests" />,
/// <see cref="RotationTests" />, <see cref="ServingAssessmentTests" />, <see cref="ServingAssessmentEdgeCaseTests" />,
/// <see cref="EnvironmentStatusEdgeCaseTests" />, <see cref="PinnedVersionsTests" />, <see cref="BuildInfoTests" /> and
/// <see cref="DeployableFactsTests" /> the tiles, the three answers of <see cref="DeployableStatus.BuildSource" />,
/// the sixteen pairs <see cref="DeployableStatus.BuildIsDue" /> compares, both pin memories, the recording of a
/// decision and the whole declared surface of the class are pinned - including that a deployable of no nodes, of two
/// nodes and of a rotation of three decides what it decides, and that the tiles named back by
/// <see cref="DeployableStatus.AssessServing" /> are the tiles the deployable holds and not copies of them.
/// </para>
/// <para>
/// Left open, and pinned here: the tile of the public address being given a name, an address and the deployable's own
/// links and nothing else - no region, no role, no primacy - and never the node the build facts are asked of, not even
/// when it is the deployable's only target; an empty name being a name, kept by the node's tile and not rewritten to
/// the address's fixed label; the links a row names for a deployable with no public address leading nowhere, held by
/// the row alone and by no tile; <see cref="DeployableStatus.Nodes" /> being one list read again and again, whatever
/// the tiles go on answering; a deployable of <em>one</em> node - the shape of most rows - giving that very tile back
/// from every way of deciding, naming no tile expected while nothing is checked and writing both
/// <c>out</c> arguments even when it has nothing to name; <see cref="DeployableStatus.BuildIsDue" /> reading the
/// version remembered and never the build held, so that a read handed one and the other is due again for the word the
/// build says; the pin being held whole - the shared readings stored as the instances they are, and a reading that
/// names another deployable kept unfiltered rather than cut to this one's name; and a decision recorded being the
/// instance handed, from whichever deployable it was decided, a snapshot of that round rather than a view of the next.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks the clock what time it is: a deployable is handed its checks,
/// pins and decisions by its monitor, and every one of them below is a value the test brought at a fixed instant.
/// </para>
/// </summary>
public class DeployableStatusTileAndRecordEdgeCaseTests
{
    /// <summary>The moment every check below was made from, so that no test asks the clock.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly Uri Door = new("https://demo.azurefd.net");
    private static readonly Uri West = new("https://app-west.example.net");
    private static readonly Uri East = new("https://app-east.example.net");
    private static readonly Uri OwnPin = new("https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/kustomization.yaml");

    /// <summary>The links of a deployable, and the links of one of its nodes: two sets for two different ends.</summary>
    private static readonly LinkSet DeployableLinks = new([new KeyValuePair<string, Uri>(LinkSet.FrontDoor, new Uri("https://portal.example.net/#fd"))]);

    private static readonly LinkSet NodeLinks = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#app-west"))]);

    /// <summary>A node's row of the topology, at an address of its own name unless one is named.</summary>
    private static NodeInfo Node(string name, string? region = "westus3", string role = NodeInfo.PrimaryRole, Uri? url = null) =>
        new(name, region, role, url ?? new Uri($"https://{name}.example.net"));

    /// <summary>A deployable's row of the parts named: one primary node when nobody names any, and every part left out
    /// absent rather than empty.</summary>
    private static DeployableInfo Row(
        Uri? frontDoor = null,
        NodeInfo[]? nodes = null,
        string? buildPath = null,
        Uri? pinUrl = null,
        LinkSet? links = null,
        string name = "ui") =>
        new(
            name,
            frontDoor,
            DeployableInfo.DefaultHealthPath,
            DeployableInfo.DefaultAlivePath,
            DeployableInfo.DefaultVersionPath,
            nodes ?? [Node("app-west")],
            BuildPath: buildPath,
            PinUrl: pinUrl,
            Links: links);

    /// <summary>A check of the state named, at the fixed instant plus the seconds named: what a tile is left holding
    /// after one round.</summary>
    private static ProbeResult Answer(HealthState state, int second = 0) =>
        new(state, state == HealthState.Unreachable ? null : 200, state == HealthState.Unreachable ? null : 12, Start.AddSeconds(second));

    /// <summary>The build an endpoint answers with for the version named.</summary>
    private static BuildInfo Build(string version) => BuildInfo.Parse($$"""{ "version": "{{version}}", "commit": "0a1b2c3" }""")!;

    // ----- the tile of the public address

    /// <summary>The Front Door tile is made from three parts of the row and nothing more: the fixed label, the row's own
    /// address and the deployable's own links. It has no region, no role and no primacy - it is no node - so it never
    /// stands in <see cref="DeployableStatus.Nodes" /> and is never the <see cref="DeployableStatus.BuildSource" />, not
    /// even as the deployable's only target: a build path named with nobody behind the address leaves nobody to ask.</summary>
    [Fact]
    public void ThePublicAddressTileHoldsOnlyItsLabelItsAddressAndTheDeployablesLinks()
    {
        var deployable = new DeployableStatus(Row(frontDoor: Door, nodes: [Node("app-west")], buildPath: "/_build", links: DeployableLinks));
        var frontDoor = deployable.FrontDoor!;

        Assert.Equal(TargetKind.FrontDoor, frontDoor.Kind);
        Assert.Equal("Front Door", frontDoor.Name);
        Assert.Same(Door, frontDoor.Url);
        Assert.Same(DeployableLinks, frontDoor.Links);
        Assert.Null(frontDoor.Region);
        Assert.Null(frontDoor.Role);
        Assert.False(frontDoor.IsPrimary);
        Assert.Equal(HealthState.Pending, frontDoor.State);

        // A target and no node: the decision, the node list and the build source all pass it by.
        Assert.Same(frontDoor, deployable.Targets[0]);
        Assert.DoesNotContain(frontDoor, deployable.Nodes);
        Assert.Same(deployable.Nodes[0], deployable.BuildSource);

        // The address alone, with a build path named, is still nobody to ask.
        var alone = new DeployableStatus(Row(frontDoor: Door, nodes: [], buildPath: "/_build"));

        Assert.True(alone.HasFrontDoor);
        Assert.Empty(alone.Nodes);
        Assert.Null(alone.BuildSource);
        Assert.Same(alone.FrontDoor, Assert.Single(alone.Targets));
        Assert.False(alone.FrontDoor!.IsPrimary);

        // And a node the file gives the same label is no address: the label belongs to the address's tile alone.
        var named = new DeployableStatus(Row(nodes: [Node("Front Door", "westus3", NodeInfo.PrimaryRole, Door)], buildPath: "/_build"));

        Assert.Null(named.FrontDoor);
        Assert.Equal("Front Door", Assert.Single(named.Nodes).Name);
        Assert.Same(named.Nodes[0], named.BuildSource);
    }

    /// <summary>An empty name is a name: the tile keeps it as the row wrote it, and is not renamed to the address's
    /// label, left out, or shared with its neighbour. An unnamed deployable is unnamed in the row it holds too, and its
    /// empty region and empty role are the absent region and no primary the constructor was given.</summary>
    [Fact]
    public void AnEmptyNameIsANameAndIsKeptByTheTileRatherThanRewritten()
    {
        var deployable = new DeployableStatus(Row(
            nodes: [Node(string.Empty, string.Empty, string.Empty, West), Node(string.Empty, "eastus2", NodeInfo.PrimaryRole, East)],
            buildPath: "/_build",
            name: string.Empty));

        Assert.Equal(2, deployable.Nodes.Count);
        Assert.Equal(["", ""], deployable.Nodes.Select(node => node.Name));
        Assert.NotSame(deployable.Nodes[0], deployable.Nodes[1]);
        Assert.Equal([West, East], deployable.Nodes.Select(node => node.Url));
        Assert.Equal(string.Empty, deployable.Nodes[0].Region);
        Assert.Equal(string.Empty, deployable.Nodes[0].Role);
        Assert.False(deployable.Nodes[0].IsPrimary);
        Assert.True(deployable.Nodes[1].IsPrimary);
        Assert.Equal(string.Empty, deployable.Info.Name);
        Assert.Equal([TargetKind.Node, TargetKind.Node], deployable.Nodes.Select(node => node.Kind));
        Assert.Same(deployable.Nodes[1], deployable.BuildSource);
    }

    // ----- the links a row names with no address to lead

    /// <summary>The deployable's own links belong to its Front Door tile alone: a row that names links and no
    /// <c>frontDoorUrl</c> leaves them held by the row and by no tile, so every node carries the empty set - its own
    /// links when its row names them, never the deployable's.</summary>
    [Fact]
    public void LinksOfADeployableWithNoPublicAddressAreHeldByTheRowAndByNoTile()
    {
        var deployable = new DeployableStatus(Row(
            nodes: [Node("app-west"), new NodeInfo("app-east", "eastus2", NodeInfo.StandbyRole, East, NodeLinks)],
            links: DeployableLinks));

        Assert.Null(deployable.FrontDoor);
        Assert.False(deployable.HasFrontDoor);
        Assert.Same(DeployableLinks, deployable.Info.Links);
        Assert.Same(LinkSet.None, deployable.Nodes[0].Links);
        Assert.NotSame(DeployableLinks, deployable.Nodes[0].Links);
        Assert.Same(NodeLinks, deployable.Nodes[1].Links);
        Assert.All(deployable.Targets, target => Assert.NotSame(DeployableLinks, target.Links));

        // A row naming neither links nor an address: nothing to lead anywhere, and the empty set everywhere.
        var bare = new DeployableStatus(Row(nodes: [Node("app-west")]));

        Assert.Null(bare.Info.Links);
        Assert.All(bare.Targets, target => Assert.Same(LinkSet.None, target.Links));
    }

    // ----- the lists a deployable is read by

    /// <summary>The tiles are made once, so the lists are the same objects on every read:
    /// <see cref="DeployableStatus.Nodes" /> is one list and the tiles in it one apiece, whatever the rounds go on
    /// recording - which is what lets a table keyed on a tile keep its rows - and <see cref="DeployableStatus.Targets" />
    /// is the stored list of the very same tiles, the Front Door tile first where there is one, read again as itself
    /// on every call and standing beside the node list rather than being it.</summary>
    [Fact]
    public void TheNodeListIsTheSameListAndTheSameTilesOnEveryRead()
    {
        var deployable = new DeployableStatus(Row(
            frontDoor: Door,
            nodes: [Node("app-west"), Node("app-east", "eastus2", NodeInfo.StandbyRole)],
            links: DeployableLinks));

        Assert.Same(deployable.Nodes, deployable.Nodes);
        Assert.Same(deployable.Nodes[0], deployable.Nodes[0]);
        // One stored list on every read, standing beside the node list: a different instance that holds the very
        // same tiles, the Front Door tile first.
        Assert.Same(deployable.Targets, deployable.Targets);
        Assert.NotSame(deployable.Nodes, deployable.Targets);
        Assert.Equal(2, deployable.Nodes.Count);
        Assert.Equal(3, deployable.Targets.Count);

        deployable.Nodes[0].Record(Answer(HealthState.Healthy));
        deployable.FrontDoor!.Record(Answer(HealthState.Unreachable));

        Assert.Same(deployable.Nodes, deployable.Nodes);
        Assert.Same(deployable.Nodes[0], deployable.Targets[1]);
        Assert.Same(deployable.FrontDoor, deployable.Targets[0]);

        // The empty list of a deployable of no node is one list too, read again as itself.
        var none = new DeployableStatus(Row(nodes: []));

        Assert.Same(none.Nodes, none.Nodes);
        Assert.Empty(none.Nodes);
    }

    // ----- the deployable of one node

    /// <summary>The commonest row of all - one node, no public address - is decided and named back the same way as a
    /// rotation: while nothing has been checked no tile is expected and no tile is serving, both written over whatever
    /// the caller's variables held; the single node's own health stands beside the decision as the node it is about;
    /// and when it is healthy, then unreachable, the tile named back is that very tile and never a copy - until it
    /// stops and nothing is named at all.</summary>
    [Fact]
    public void ADeployableOfOneNodeGivesThatVeryTileBackFromEveryWayOfDeciding()
    {
        var deployable = new DeployableStatus(Row(nodes: [new NodeInfo("app-west", null, NodeInfo.PrimaryRole, West)]));
        var tile = deployable.Nodes[0];
        TargetStatus? expected = tile;
        IReadOnlyList<TargetStatus> serving = [tile];

        var pending = deployable.Assess(out expected);

        Assert.Null(expected);
        Assert.Equal(ServingState.Pending, pending.State);
        Assert.NotNull(pending.OnlyNode);
        Assert.Equal("app-west", pending.OnlyNode!.Name);
        Assert.Null(pending.OnlyNode.Region);
        Assert.True(pending.OnlyNode.IsPrimary);
        Assert.Equal(tile.State, pending.OnlyNode.State);

        var waiting = deployable.AssessServing(out serving);

        Assert.Empty(serving);
        Assert.Equal(ServingState.Pending, waiting.State);
        Assert.Null(waiting.Expected);

        tile.Record(Answer(HealthState.Healthy));

        var primary = deployable.Assess(out expected);
        var served = deployable.AssessServing(out serving);

        Assert.Same(tile, expected);
        Assert.Same(tile, Assert.Single(serving));
        Assert.Equal(ServingState.Primary, primary.State);
        Assert.Equal(ServingState.Primary, served.State);
        Assert.Same(primary.Expected, primary.OnlyNode);
        Assert.Equal(["app-west"], primary.Serving.Select(node => node.Name));
        Assert.False(primary.IsRotation);

        tile.Record(Answer(HealthState.Unreachable, 30));

        var down = deployable.Assess(out expected);
        deployable.AssessServing(out serving);

        Assert.Null(expected);
        Assert.Empty(serving);
        Assert.Equal(ServingState.Down, down.State);

        // The same node behind a public address is no longer the only one the page can name: the decision says which
        // region serves, and still hands back the very tile.
        var withDoor = new DeployableStatus(Row(frontDoor: Door, nodes: [new NodeInfo("app-west", null, NodeInfo.PrimaryRole, West)]));
        withDoor.Nodes[0].Record(Answer(HealthState.Healthy));

        var decided = withDoor.Assess(out var one);
        withDoor.AssessServing(out var tiles);

        Assert.Equal(ServingState.Primary, decided.State);
        Assert.Null(decided.OnlyNode);
        Assert.Same(withDoor.Nodes[0], one);
        Assert.Same(withDoor.Nodes[0], Assert.Single(tiles));
    }

    // ----- the build and the version it was read for

    /// <summary><see cref="DeployableStatus.BuildIsDue" /> compares the version it is asked about with
    /// <see cref="DeployableStatus.BuildReadForVersion" /> and with nothing else: the build held beside it says what the
    /// endpoint answered and moves the answer not at all. A read handed a build of one version and a node of another is
    /// therefore due for the word the node said, and not due for it again - and the empty word and the absence of one
    /// are two things a node can say, each matching only itself.</summary>
    [Fact]
    public void TheBuildBeingDueReadsTheVersionRememberedAndNeverTheBuildHeld()
    {
        var deployable = new DeployableStatus(Row(buildPath: "/_build"));
        var build = Build("2.4.15");

        deployable.RecordBuild(build, "2.4.16");

        Assert.Same(build, deployable.Build);
        Assert.Equal("2.4.15", deployable.Build!.Version);
        Assert.Equal("2.4.16", deployable.BuildReadForVersion);
        Assert.True(deployable.BuildWasRead);
        Assert.False(deployable.BuildIsDue("2.4.16"));
        Assert.True(deployable.BuildIsDue("2.4.16+build.7"));
        Assert.True(deployable.BuildIsDue("2.4.15"));
        Assert.True(deployable.BuildIsDue("2.4.17"));
        Assert.True(deployable.BuildIsDue(null));
        Assert.True(deployable.BuildIsDue(string.Empty));

        // A build answered with no version remembered: the absence is the memory, and the build's own word means nothing.
        deployable.RecordBuild(build, null);

        Assert.Same(build, deployable.Build);
        Assert.Null(deployable.BuildReadForVersion);
        Assert.False(deployable.BuildIsDue(null));
        Assert.True(deployable.BuildIsDue("2.4.15"));

        // The empty word is a version the node said, and a read that answered nothing is a read.
        deployable.RecordBuild(null, string.Empty);

        Assert.True(deployable.BuildWasRead);
        Assert.Null(deployable.Build);
        Assert.Equal(string.Empty, deployable.BuildReadForVersion);
        Assert.False(deployable.BuildIsDue(string.Empty));
        Assert.True(deployable.BuildIsDue(null));

        // And the flag says only that the endpoint was asked: no other recording moves it, and no read takes it back.
        deployable.RecordPin(PinnedVersions.Parse("""{ "ui": "2.4.17" }"""));
        deployable.RecordServing(deployable.Assess());

        Assert.True(deployable.BuildWasRead);
        Assert.False(deployable.BuildIsDue(string.Empty));
    }

    // ----- the pin a deployable holds

    /// <summary>The pin is the reading, held whole and as it stands: the shared readings - pending, missing, not tracked -
    /// are stored as the very instances they are and answer to no name; a reading that came back is kept unfiltered,
    /// whether or not it names this deployable, so the line of another stands in it and this deployable's absence from it
    /// is nobody's business until the two are compared; and a deployable the file names no <c>pinUrl</c> for takes any of
    /// them just the same, in both memories.</summary>
    [Fact]
    public void ThePinIsHeldWholeAndTheSharedReadingsAreHeldAsTheyStand()
    {
        var deployable = new DeployableStatus(Row(pinUrl: OwnPin));

        // Where the row's pending reading leaves the memory is the boundary file's; what follows is what a recording
        // makes of the readings themselves.
        deployable.RecordPin(PinnedVersions.Missing);

        Assert.Same(PinnedVersions.Missing, deployable.Pinned);
        Assert.Equal(PinnedVersionsState.Missing, deployable.Pinned!.State);
        Assert.Empty(deployable.Pinned.Versions);
        Assert.Null(deployable.Pinned.Detail);
        Assert.Null(deployable.Pinned.Of("ui"));
        Assert.Null(deployable.LastReadPinned);

        deployable.RecordPin(PinnedVersions.NotTracked);
        deployable.RememberPin(PinnedVersions.NotTracked);

        Assert.Same(PinnedVersions.NotTracked, deployable.Pinned);
        Assert.Same(PinnedVersions.NotTracked, deployable.LastReadPinned);

        // A reading is the file, not this deployable's line of it: named elsewhere, kept here whole.
        var other = PinnedVersions.Parse("""{ "api": "2.4.14" }""");
        deployable.RecordPin(other);

        Assert.Same(other, deployable.Pinned);
        Assert.Equal(PinnedVersionsState.Read, deployable.Pinned!.State);
        Assert.Single(deployable.Pinned.Versions);
        Assert.Null(deployable.Pinned.Of(deployable.Info.Name));
        Assert.Null(deployable.Pinned.Of(string.Empty));
        Assert.Equal("2.4.14", deployable.Pinned.Of("api"));
        Assert.Same(PinnedVersions.NotTracked, deployable.LastReadPinned);

        // And a file that names this deployable beside another is held whole too: its lines are not cut to one row's name.
        var both = PinnedVersions.Parse("""{ "ui": "2.4.15", "api": "2.4.14" }""");
        deployable.RecordPin(both);

        Assert.Same(both, deployable.Pinned);
        Assert.Equal(2, deployable.Pinned!.Versions.Count);
        Assert.Equal("2.4.15", deployable.Pinned.Of(deployable.Info.Name));
        Assert.Equal("2.4.14", deployable.Pinned.Of("api"));

        // A reading of the file that named nothing is a reading of nothing, and stands as such.
        var empty = PinnedVersions.Parse("{}");
        deployable.RecordPin(empty);

        Assert.Same(empty, deployable.Pinned);
        Assert.Equal(PinnedVersionsState.Read, deployable.Pinned!.State);
        Assert.Empty(deployable.Pinned.Versions);
        Assert.Null(deployable.Pinned.Of("ui"));

        // None of this asks the row: a deployable with no pinUrl of its own keeps both memories like any other.
        var unnamed = new DeployableStatus(Row());

        Assert.Null(unnamed.Pinned);
        Assert.Null(unnamed.LastReadPinned);

        unnamed.RecordPin(PinnedVersions.Pending);
        unnamed.RememberPin(PinnedVersions.Pending);

        Assert.Same(PinnedVersions.Pending, unnamed.Pinned);
        Assert.Same(unnamed.Pinned, unnamed.LastReadPinned);
    }

    // ----- the decision a round ends with

    /// <summary><see cref="DeployableStatus.RecordServing" /> stores the instance it is handed and asks nothing about it:
    /// the decision another deployable was worked out of stands here as it stands there, naming that deployable's node,
    /// and it is a snapshot of the round that decided it - the tiles behind it going on to answer otherwise changes
    /// nothing of what was recorded. Deciding again reads this deployable's tiles as they are and still leaves the
    /// memory to the recording alone.</summary>
    [Fact]
    public void ADecisionRecordedIsTheInstanceHandedEvenAnotherDeployablesAndASnapshotOfItsRound()
    {
        var other = new DeployableStatus(Row(nodes: [Node("app-west")]));
        other.Nodes[0].Record(Answer(HealthState.Healthy));

        var foreign = other.Assess(out var there);
        var deployable = new DeployableStatus(Row(nodes: [Node("app-east", "eastus2", NodeInfo.StandbyRole)]));

        Assert.Same(other.Nodes[0], there);
        Assert.Null(deployable.LastServing);

        deployable.RecordServing(foreign);

        Assert.Same(foreign, deployable.LastServing);
        Assert.Equal(ServingState.Primary, deployable.LastServing!.State);
        Assert.Equal("app-west", deployable.LastServing.Expected!.Name);

        // The round that was remembered: the other deployable's node going down is news of the next round, not of that.
        other.Nodes[0].Record(Answer(HealthState.Unreachable, 30));

        Assert.Equal(ServingState.Down, other.Assess().State);
        Assert.Same(foreign, deployable.LastServing);
        Assert.Equal(ServingState.Primary, deployable.LastServing!.State);
        Assert.Equal(HealthState.Healthy, deployable.LastServing.Expected!.State);

        // And the memory decides nothing: this deployable is still standing where its own tiles left it.
        Assert.Equal(ServingState.Pending, deployable.Assess().State);
        Assert.Same(foreign, deployable.LastServing);

        // A deployable with no node at all keeps another's decision as readily.
        var empty = new DeployableStatus(Row(nodes: []));
        empty.RecordServing(foreign);

        Assert.Same(foreign, empty.LastServing);
        Assert.Equal(ServingState.NoNodes, empty.Assess(out var nobody).State);
        Assert.Null(nobody);
        Assert.Same(foreign, empty.LastServing);

        // Only another recording moves it - and it moves to what the next round decided from these tiles alone.
        var here = deployable.Assess(out _);
        deployable.RecordServing(here);

        Assert.Same(here, deployable.LastServing);
        Assert.NotSame(foreign, deployable.LastServing);
        Assert.Equal(ServingState.Pending, deployable.LastServing!.State);
    }
}
