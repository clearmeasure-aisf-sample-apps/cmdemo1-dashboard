using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeployableStatus" />, one deployable of an
/// environment: the tiles of its endpoints that a row of <c>topology.json</c> is turned into, the pin, the build and
/// the serving decision it remembers, declared in <c>src/Dashboard/Health/TargetStatus.cs</c>.
/// <para>
/// What is pinned where it belongs is left alone. <see cref="TargetStatusTests" /> pins the tiles a deployable is
/// built with and reads - the Front Door endpoint named for its end and listed first, the nodes after it in the order
/// of the file, <see cref="DeployableStatus.HasFrontDoor" /> both ways, a deployable without a <c>frontDoorUrl</c>
/// holding no Front Door tile, and the tile <see cref="DeployableStatus.Assess" /> expects to serve when the primary
/// is down and when nothing is; <see cref="RotationTests" /> pins <see cref="DeployableStatus.AssessServing" /> over a
/// rotation of four nodes and the runtime view drawn from it; <see cref="TargetStatusEdgeCaseTests" />,
/// <see cref="TargetKindEdgeCaseTests" /> and <see cref="HistoryBufferTests" /> pin the tile every part of a
/// deployable is made of; <see cref="ServingAssessmentEdgeCaseTests" /> and <see cref="ServingStateEdgeCaseTests" />
/// pin the decision worked out of the nodes, the words it says and the instances it keeps;
/// <see cref="ServingAssessmentTests" /> and <see cref="RotationTests" /> pin the states that decide it.
/// <see cref="KustomizationPinMonitorTests" /> pins that a deployable whose row names a <c>pinUrl</c> starts at
/// <see cref="PinnedVersions.Pending" /> and one that names none at nothing, and reads both through the monitor;
/// <see cref="ApplicationTextEdgeCaseTests" /> pins that the page reads a deployable's own pin rather than the
/// environment's; <see cref="VersionAssessmentTests" /> and <see cref="PinnedVersionsTests" /> pin the pin's own
/// record. <see cref="BuildInfoTests" /> and <see cref="DeployableFactsTests" /> pin that a deployable whose row
/// names no <c>buildPath</c> has no <see cref="DeployableStatus.BuildSource" /> and that the monitor asks the primary
/// node once per version; <see cref="RuntimePayloadHelperTests" /> pins that a build recorded reaches the diagram.
/// </para>
/// <para>
/// What all of that leaves open, and what follows: the row that is not there, refused by name; the tiles being made
/// once from the row as it stood, so that neither a node added to the file's list afterwards nor a copy of the record
/// moves one; the four parts each node tile takes from its own row, the absent region and the empty one included, and
/// the deployable's links going to the Front Door tile alone; a deployable of no nodes at all - no tiles, no targets,
/// no build source, and a decision of nothing serving; <see cref="DeployableStatus.Targets" /> being the node list
/// itself without a Front Door endpoint and a fresh sequence over the very same tiles with one, read after a check as
/// before it; the three answers of <see cref="DeployableStatus.BuildSource" /> where the file names a build path -
/// the first primary, the first node when none is, and nobody when there is none;
/// <see cref="DeployableStatus.BuildIsDue" /> before anything has been asked, and the sixteen pairs of version it is
/// asked about afterwards, compared without regard to case and with everything else kept; a build read being kept
/// until another read replaces it, the absence of an answer among them; the pin of <c>pinUrl</c> standing where the
/// last reading left it, good or failed, the reading that is not there refused by name, and
/// <see cref="DeployableStatus.RememberPin" /> refusing even nothing - the one asymmetry of the two; the serving
/// decision remembered only when it is recorded and never by the deciding; and the surface the class declares, so
/// that the day a member is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a deployable holds no timer and calls
/// nothing - the build, the pin and the serving decision are handed to it by its monitor - and every check, version,
/// pin and build below is a value the test brought.
/// </para>
/// </summary>
public class DeployableStatusEdgeCaseTests
{
    /// <summary>The moment every check below was made from, so that no test asks the clock.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly Uri Door = new("https://demo.azurefd.net");
    private static readonly Uri West = new("https://app-west.example.net");
    private static readonly Uri East = new("https://app-east.example.net");
    private static readonly Uri South = new("https://app-south.example.net");
    private static readonly Uri OwnPin = new("https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/kustomization.yaml");

    /// <summary>The links of a deployable and of one of its nodes, two sets that stand for two different ends.</summary>
    private static readonly LinkSet DeployableLinks = new([new KeyValuePair<string, Uri>(LinkSet.FrontDoor, new Uri("https://portal.example.net/#fd"))]);

    private static readonly LinkSet NodeLinks = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#app-west"))]);

    /// <summary>A node's row of the topology, at an address of its own name unless one is named.</summary>
    private static NodeInfo Node(string name, string? region = "westus3", string role = NodeInfo.PrimaryRole, Uri? url = null) =>
        new(name, region, role, url ?? new Uri($"https://{name}.example.net"));

    /// <summary>A deployable of the parts named: one primary node when nobody names any, the three paths a row falls
    /// back to standing behind it, and every other part of it absent.</summary>
    private static DeployableStatus Deployable(
        Uri? frontDoor = null,
        NodeInfo[]? nodes = null,
        string? buildPath = null,
        Uri? pinUrl = null,
        LinkSet? links = null,
        string name = "ui") =>
        new(new DeployableInfo(
            name,
            frontDoor,
            DeployableInfo.DefaultHealthPath,
            DeployableInfo.DefaultAlivePath,
            DeployableInfo.DefaultVersionPath,
            nodes ?? [Node("app-west")],
            BuildPath: buildPath,
            PinUrl: pinUrl,
            Links: links));

    /// <summary>A check of the state named, at the second named: what a tile is left holding after one round.</summary>
    private static ProbeResult Answer(HealthState state, int second = 0) =>
        new(state, state == HealthState.Unreachable ? null : 200, state == HealthState.Unreachable ? null : 12, Start.AddSeconds(second));

    /// <summary>The build an endpoint answers with for the version named.</summary>
    private static BuildInfo Build(string version) => BuildInfo.Parse($$"""{ "version": "{{version}}", "commit": "0a1b2c3" }""")!;

    /// <summary>The pin a file answers with for the version named, in the words of <c>versions.json</c>.</summary>
    private static PinnedVersions Pin(string version) => PinnedVersions.Parse($$"""{ "ui": "{{version}}" }""");

    // ----- the row a deployable is made of

    /// <summary>The one argument a deployable refuses: the row of the topology it is made of. The refusal is by name,
    /// and nothing is half-built on the way to it.</summary>
    [Fact]
    public void ADeployableWithoutTheRowItIsMadeOfIsRefusedByName()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new DeployableStatus(null!));

        Assert.Equal("info", error.ParamName);
        Assert.Contains("info", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The tiles are made from the row once, at the moment the deployable is built, and are nobody's copy
    /// afterwards: the row stands as the very instance it was handed, its Front Door address and its links are the
    /// Front Door tile's own, a node the file's list goes on to gain is no tile, and the record copied with its Front
    /// Door taken away leaves the tiles already made standing. The deployable rewrites nothing of the row and reads it
    /// again later.</summary>
    [Fact]
    public void TheTilesAreMadeFromTheRowOnceAndOutliveEveryCopyOfIt()
    {
        var listed = new List<NodeInfo> { Node("app-west", "westus3", NodeInfo.PrimaryRole, West), Node("app-east", "eastus2", NodeInfo.StandbyRole, East) };
        var info = new DeployableInfo(
            "ui",
            Door,
            DeployableInfo.DefaultHealthPath,
            DeployableInfo.DefaultAlivePath,
            DeployableInfo.DefaultVersionPath,
            listed,
            BuildPath: "/_build",
            Links: DeployableLinks,
            PinUrl: OwnPin);
        var deployable = new DeployableStatus(info);

        Assert.Same(info, deployable.Info);
        Assert.Same(info.FrontDoor, deployable.FrontDoor!.Url);
        Assert.Same(info.Links, deployable.FrontDoor.Links);
        Assert.Equal(["app-west", "app-east"], deployable.Nodes.Select(node => node.Name));

        // A node the file goes on to add is no tile: the deployable was made from the row as it stood.
        listed.Add(Node("app-south", "northeurope"));

        Assert.Equal(3, info.Nodes.Count);
        Assert.Equal(2, deployable.Nodes.Count);

        // And another row copied from the first - its Front Door, its links and its name gone - is another row.
        var copied = info with { FrontDoor = null, Links = null, Name = "api" };

        Assert.True(copied.FrontDoor is null);
        Assert.Equal("api", copied.Name);
        Assert.NotNull(deployable.FrontDoor);
        Assert.True(deployable.HasFrontDoor);
        Assert.Equal("ui", deployable.Info.Name);
        Assert.Same(info, deployable.Info);
    }

    /// <summary>Every node tile carries the four parts of its own row and nothing of its neighbours': the name, the
    /// region, the role and the address, each as it was written - the role read without regard to case, the empty
    /// region a region and the absent one nothing. The links a deployable is built with go to its Front Door tile and
    /// to no node; a node that names none carries the empty set.</summary>
    [Fact]
    public void EachNodeTileCarriesThePartsOfItsOwnRowOfTheTopology()
    {
        var west = new NodeInfo("app-west", "westus3", NodeInfo.PrimaryRole, West, NodeLinks);
        var east = new NodeInfo("app-east", null, NodeInfo.StandbyRole, East);
        var south = new NodeInfo("app-south", string.Empty, "PRIMARY", South);
        var deployable = Deployable(frontDoor: Door, nodes: [west, east, south], links: DeployableLinks);

        Assert.All(deployable.Nodes, node => Assert.Equal(TargetKind.Node, node.Kind));
        Assert.Equal(["app-west", "app-east", "app-south"], deployable.Nodes.Select(node => node.Name));
        Assert.Equal([West, East, South], deployable.Nodes.Select(node => node.Url));
        Assert.Equal("westus3", deployable.Nodes[0].Region);
        Assert.Null(deployable.Nodes[1].Region);
        Assert.Equal(string.Empty, deployable.Nodes[2].Region);
        Assert.Equal([NodeInfo.PrimaryRole, NodeInfo.StandbyRole, "PRIMARY"], deployable.Nodes.Select(node => node.Role));
        Assert.Equal([true, false, true], deployable.Nodes.Select(node => node.IsPrimary));
        Assert.All(deployable.Nodes, node => Assert.Equal(HealthState.Pending, node.State));

        // The node's own links, and the empty set where the row named none - never the deployable's.
        Assert.Same(NodeLinks, deployable.Nodes[0].Links);
        Assert.Same(LinkSet.None, deployable.Nodes[1].Links);
        Assert.Same(DeployableLinks, deployable.FrontDoor!.Links);

        // A tile of its own per row, however many rows say one thing: no node is shared and none is skipped.
        var twins = Deployable(nodes: [west, west with { }]);

        Assert.Equal(2, twins.Nodes.Count);
        Assert.NotSame(twins.Nodes[0], twins.Nodes[1]);
        Assert.Equal(twins.Nodes[0].Name, twins.Nodes[1].Name);
    }

    // ----- the tiles a deployable holds, at the empty end

    /// <summary>A deployable the file gives no node at all holds no tile, offers no target, has nobody to ask for its
    /// build and decides that nothing serves - the one empty list read twice being empty twice. Its public address
    /// alone is the same decision with one tile in it: the Front Door tile is a target, is not a node, and leaves the
    /// agreement <see cref="FrontDoorAgreement.Pending" /> because it has not been checked.</summary>
    [Fact]
    public void ADeployableOfNoNodesHoldsNoTilesAndDecidesThatNothingServes()
    {
        var deployable = Deployable(nodes: [], buildPath: "/_build", pinUrl: OwnPin);

        Assert.Empty(deployable.Nodes);
        Assert.Null(deployable.FrontDoor);
        Assert.False(deployable.HasFrontDoor);
        Assert.Null(deployable.BuildSource);
        Assert.Empty(deployable.Targets);
        Assert.Empty(deployable.Targets);

        // Without a Front Door endpoint there is nothing to prepend: the read is the node list it always was.
        Assert.Same(deployable.Nodes, deployable.Targets);

        var assessment = deployable.Assess(out var expected);
        var decided = deployable.AssessServing(out var serving);

        Assert.Equal(ServingState.NoNodes, assessment.State);
        Assert.Equal("No nodes in the topology", assessment.Headline);
        Assert.Null(assessment.Expected);
        Assert.Null(expected);
        Assert.Empty(serving);
        Assert.Equal(ServingState.NoNodes, decided.State);
        Assert.Equal(FrontDoorAgreement.NotPresent, assessment.FrontDoor);

        // No public address at all is the absence of a state beside the decision, and not the state of nothing.
        Assert.Null(assessment.FrontDoorState);

        // The public address of a deployable with no node behind it is a target and no node.
        var alone = Deployable(frontDoor: Door, nodes: []);

        Assert.True(alone.HasFrontDoor);
        Assert.Same(alone.FrontDoor, Assert.Single(alone.Targets));
        Assert.Equal(ServingState.NoNodes, alone.Assess(out var nobody).State);
        Assert.Null(nobody);
        Assert.Equal(FrontDoorAgreement.Pending, alone.Assess().FrontDoor);
    }

    /// <summary>The targets of a deployable are its tiles and not pictures of them: with a Front Door endpoint the
    /// read is a sequence made again on every call - which is why reading it twice is not reading the same object -
    /// and every element of it is the tile the deployable also answers by name for, state and all. A check recorded on
    /// a tile is seen through the read, and a read the page walks twice says the same thing twice.</summary>
    [Fact]
    public void TheTargetsAreTheSameTilesOnEveryReadAndASequenceOfTheirOwnWithAPublicAddress()
    {
        var deployable = Deployable(
            frontDoor: Door,
            nodes: [Node("app-west", "westus3", NodeInfo.PrimaryRole), Node("app-east", "eastus2", NodeInfo.StandbyRole)],
            links: DeployableLinks);
        var frontDoor = deployable.FrontDoor!;

        Assert.NotSame(deployable.Targets, deployable.Targets);
        Assert.Equal(deployable.Nodes.Count + 1, deployable.Targets.Count());
        Assert.Equal([frontDoor, deployable.Nodes[0], deployable.Nodes[1]], deployable.Targets);
        Assert.Equal([frontDoor, deployable.Nodes[0], deployable.Nodes[1]], deployable.Targets);

        // Nothing has been checked: every tile is pending, and so is the deployable that holds them.
        Assert.All(deployable.Targets, target => Assert.Equal(HealthState.Pending, target.State));
        Assert.Equal(ServingState.Pending, deployable.Assess().State);

        deployable.Nodes[0].Record(Answer(HealthState.Healthy));
        frontDoor.Record(Answer(HealthState.Unreachable, 30));

        Assert.Equal([HealthState.Unreachable, HealthState.Healthy, HealthState.Pending], deployable.Targets.Select(target => target.State));
        Assert.Same(deployable.Nodes[0], deployable.Targets.Skip(1).First());
        Assert.Single(frontDoor.History);
        Assert.Equal(HealthState.Unreachable, frontDoor.History[0].State);
        Assert.Empty(deployable.Nodes[1].History);

        // Without the public address the read is the node list, and its count is the deployable's own.
        var bare = Deployable(nodes: [Node("app-west"), Node("app-east", "eastus2", NodeInfo.StandbyRole)]);

        Assert.Same(bare.Nodes, bare.Targets);
        Assert.Equal(2, bare.Targets.Count());
    }

    // ----- the node the build facts come from

    /// <summary>The node asked for a deployable's build facts, once the row names a build path: the first primary of
    /// the order of the file - not the first node, and not a later primary - the first node of that order when no row
    /// is primary, and nobody when the deployable has no node at all. The tile chosen is the node's own tile and not
    /// a copy of it, which is what lets the monitor compare a check's tile with it, and it is never the public
    /// address.</summary>
    [Fact]
    public void TheBuildSourceIsTheFirstPrimaryNodeOrTheFirstNodeAtAllOrNobody()
    {
        var noPrimary = Deployable(
            frontDoor: Door,
            nodes: [Node("app-west", "westus3", NodeInfo.StandbyRole), Node("app-east", "eastus2", NodeInfo.StandbyRole)],
            buildPath: "/_build");

        Assert.Same(noPrimary.Nodes[0], noPrimary.BuildSource);
        Assert.Equal("app-west", noPrimary.BuildSource!.Name);

        var twoPrimaries = Deployable(
            frontDoor: Door,
            nodes:
            [
                Node("app-west", "westus3", NodeInfo.StandbyRole),
                Node("app-east", "eastus2", "PRIMARY"),
                Node("app-south", "northeurope", NodeInfo.PrimaryRole),
            ],
            buildPath: "/_build");

        Assert.Same(twoPrimaries.Nodes[1], twoPrimaries.BuildSource);
        Assert.Equal(TargetKind.Node, twoPrimaries.BuildSource!.Kind);
        Assert.NotSame(twoPrimaries.FrontDoor, twoPrimaries.BuildSource);

        // A node checked and found unreachable is still the node the build facts are asked of.
        twoPrimaries.Nodes[1].Record(Answer(HealthState.Unreachable));

        Assert.Same(twoPrimaries.Nodes[1], twoPrimaries.BuildSource);
        Assert.Equal(HealthState.Unreachable, twoPrimaries.BuildSource.State);

        // No node at all is no build source, build path or none.
        Assert.Null(Deployable(nodes: [], buildPath: "/_build").BuildSource);
    }

    // ----- the build the deployable runs

    /// <summary>Before the build endpoint has been asked once, the deployable holds no build, says it has read
    /// nothing, remembers no version, and is due for a read of every version the node can report - of no version
    /// among them, the empty word and a real one. <see cref="DeployableStatus.BuildWasRead" /> is what separates that
    /// from a read that answered nothing, and it is the only one of the three that says a round happened.</summary>
    [Fact]
    public void BeforeAnythingIsAskedTheBuildIsDueForEveryVersionAndForNone()
    {
        var deployable = Deployable(buildPath: "/_build", nodes: [Node("app-west")]);

        Assert.Null(deployable.Build);
        Assert.False(deployable.BuildWasRead);
        Assert.Null(deployable.BuildReadForVersion);
        Assert.True(deployable.BuildIsDue(null));
        Assert.True(deployable.BuildIsDue(string.Empty));
        Assert.True(deployable.BuildIsDue("2.4.15"));

        // A read of the pin, of the serving decision and of a node's check is no read of the build.
        deployable.RecordPin(Pin("2.4.15"));
        deployable.RecordServing(deployable.Assess());
        deployable.Nodes[0].Record(Answer(HealthState.Healthy));

        Assert.False(deployable.BuildWasRead);
        Assert.True(deployable.BuildIsDue("2.4.15"));
    }

    /// <summary>Once the build endpoint has been asked, another read is due for another version only: the two words
    /// compared without regard to case, and by nothing else - not trimmed, not cut at the build metadata, not read as
    /// a version by anything that knows what a version means. The absence of a version and the empty word are two
    /// different things the node said, and each of them matches only itself.</summary>
    [Theory]
    [InlineData("2.4.15", "2.4.15", false)]
    [InlineData("2.4.15", "2.4.16", true)]
    [InlineData("2.4.15", "2.4.15+0a1b2c3", true)]
    [InlineData("2.4.15+0a1b2c3", "2.4.15", true)]
    [InlineData("2.4.15", " 2.4.15", true)]
    [InlineData("2.4.15", "2.4.15 ", true)]
    [InlineData("2.4.15", "", true)]
    [InlineData("2.4.15", null, true)]
    [InlineData("REL-1", "rel-1", false)]
    [InlineData("rel-1", "REL-1", false)]
    [InlineData(null, null, false)]
    [InlineData(null, "", true)]
    [InlineData(null, "2.4.15", true)]
    [InlineData("", "", false)]
    [InlineData("", null, true)]
    [InlineData("", "2.4.15", true)]
    public void TheBuildIsDueAgainForAnotherVersionThanTheOneItWasReadFor(string? readFor, string? version, bool due)
    {
        var deployable = Deployable(buildPath: "/_build");
        deployable.RecordBuild(null, readFor);

        // A read that answered nothing is still a read: the flag says the endpoint was asked, the version what for.
        Assert.True(deployable.BuildWasRead);
        Assert.Null(deployable.Build);
        Assert.Equal(readFor, deployable.BuildReadForVersion);
        Assert.Equal(due, deployable.BuildIsDue(version));
    }

    /// <summary>What a read of the build endpoint left behind is kept until the next one replaces it - the answer of a
    /// node, the absence of one, and the version the node reported at the moment it was asked. The deployable decides
    /// nothing about the two: <see cref="DeployableStatus.RecordBuild" /> guards nothing, so a failed read clears a
    /// good build and remembers <see langword="null" /> as the version it was read for, which is exactly the pair the
    /// monitor hands it.</summary>
    [Fact]
    public void ABuildIsKeptUntilAnotherBuildOrNoAnswerReplacesIt()
    {
        var deployable = Deployable(buildPath: "/_build");
        var first = Build("2.4.15");

        deployable.RecordBuild(first, "2.4.15");

        Assert.Same(first, deployable.Build);
        Assert.True(deployable.BuildWasRead);
        Assert.Equal("2.4.15", deployable.BuildReadForVersion);
        Assert.False(deployable.BuildIsDue("2.4.15"));
        Assert.True(deployable.BuildIsDue("2.4.16"));

        // Another version, another build: the word remembered is the word the node reported, kept as it was said.
        var second = Build("2.4.16");
        deployable.RecordBuild(second, " 2.4.16 ");

        Assert.Same(second, deployable.Build);
        Assert.Equal(" 2.4.16 ", deployable.BuildReadForVersion);
        Assert.True(deployable.BuildIsDue("2.4.16"));
        Assert.False(deployable.BuildIsDue(" 2.4.16 "));

        // The endpoint going silent replaces the build it had with nothing, and is not asked again for the same node.
        deployable.RecordBuild(null, null);

        Assert.Null(deployable.Build);
        Assert.True(deployable.BuildWasRead);
        Assert.Null(deployable.BuildReadForVersion);
        Assert.False(deployable.BuildIsDue(null));
        Assert.True(deployable.BuildIsDue(" 2.4.16 "));
    }

    // ----- the pin of a deployable of its own

    /// <summary>The pin of a deployable is the last reading of it, in good case or bad: a reading recorded stands as
    /// the very instance it was handed, a failed one replaces a good one without softening it, and the property says
    /// nothing of the row - a deployable the topology names no file for still takes a reading when one is handed to
    /// it. The reading that is not there is refused by name, and the refusal costs the deployable the reading it
    /// already held.</summary>
    [Fact]
    public void ThePinStandsWhereTheLastReadingLeftItGoodOrFailed()
    {
        var deployable = Deployable(pinUrl: OwnPin);
        var read = Pin("2.4.14");

        deployable.RecordPin(read);

        Assert.Same(read, deployable.Pinned);
        Assert.Equal(PinnedVersionsState.Read, deployable.Pinned!.State);
        Assert.Equal("2.4.14", deployable.Pinned.Of("ui"));

        // The last good reading is not remembered by a recording: only RememberPin remembers it.
        Assert.Null(deployable.LastReadPinned);

        // A reading that failed is a reading, and it replaces the good one whole.
        var failed = PinnedVersions.Unavailable("the server answered HTTP 502.");
        deployable.RecordPin(failed);

        Assert.Same(failed, deployable.Pinned);
        Assert.Equal(PinnedVersionsState.Unavailable, deployable.Pinned!.State);
        Assert.Null(deployable.Pinned.Of("ui"));

        // Nothing to record is refused by name, and leaves the failed reading where it was.
        Assert.Equal("pinned", Assert.Throws<ArgumentNullException>(() => deployable.RecordPin(null!)).ParamName);

        Assert.Same(failed, deployable.Pinned);

        // A deployable the file names no pin for has no pin of its own - until something is recorded for it.
        var unnamed = Deployable();

        Assert.Null(unnamed.Pinned);

        unnamed.RecordPin(read);

        Assert.Same(read, unnamed.Pinned);
    }

    /// <summary>The memory of the last reading that succeeded is written by
    /// <see cref="DeployableStatus.RememberPin" /> and by nothing else: it stands through the failed recordings
    /// <see cref="DeployableStatus.RecordPin" /> goes on making, is replaced only by another remembering, and is the
    /// one part of a deployable that refuses nothing - not a second reading, and not the absence of one, which clears
    /// it. The two members are two memories, and neither touches the other.</summary>
    [Fact]
    public void TheLastGoodPinStandsThroughTheReadingsThatFailedAndIsRefusedNothing()
    {
        var deployable = Deployable(pinUrl: OwnPin);
        var good = Pin("2.4.14");

        Assert.Null(deployable.LastReadPinned);

        deployable.RememberPin(good);
        deployable.RecordPin(good);

        Assert.Same(good, deployable.LastReadPinned);
        Assert.Same(good, deployable.Pinned);

        // The rounds that read nothing go by; the memory of the last reading that came back is what the next is
        // compared with, so a change in between is not hidden by the failure in the middle.
        deployable.RecordPin(PinnedVersions.Unavailable("the server answered HTTP 502."));
        deployable.RecordPin(PinnedVersions.Missing);

        Assert.Equal(PinnedVersionsState.Missing, deployable.Pinned!.State);
        Assert.Same(good, deployable.LastReadPinned);

        // Remembering writes one thing and reads nothing: another reading replaces it, and no reading clears it.
        var newer = Pin("2.4.15");
        deployable.RememberPin(newer);

        Assert.Same(newer, deployable.LastReadPinned);
        Assert.Equal(PinnedVersionsState.Missing, deployable.Pinned!.State);

        // The member guards nothing, unlike the recording next to it - which is why the monitor remembers only ever
        // a reading that came back, and why the memory of the last good pin can be moved back to nothing.
        deployable.RememberPin(null!);

        Assert.Null(deployable.LastReadPinned);
        Assert.Equal(PinnedVersionsState.Missing, deployable.Pinned!.State);
    }

    // ----- the serving decision a round ends with

    /// <summary>The decision a round ends with is remembered only when the round says so: none of the three ways of
    /// deciding - <see cref="DeployableStatus.Assess" />, the same with the tile it expects to serve, and the same
    /// with every tile of a rotation - writes <see cref="DeployableStatus.LastServing" />, which is what lets the next
    /// round compare itself with the last one's decision rather than with its own. What is recorded is kept as the
    /// instance it was handed, replaced by the next recording and by nothing else - and, like
    /// <see cref="DeployableStatus.RememberPin" />, refused by no argument at all.</summary>
    [Fact]
    public void TheServingDecisionIsRememberedOnlyWhenTheRoundRecordsIt()
    {
        var deployable = Deployable(frontDoor: Door, nodes: [Node("app-west"), Node("app-east", "eastus2", NodeInfo.StandbyRole)]);

        Assert.Null(deployable.LastServing);

        var first = deployable.Assess();

        Assert.Null(deployable.LastServing);

        deployable.Assess(out _);
        deployable.AssessServing(out _);

        Assert.Null(deployable.LastServing);

        deployable.RecordServing(first);

        Assert.Same(first, deployable.LastServing);

        // A second decision over the very same states is another object saying the same thing, and the memory stands
        // where it was until it is recorded again.
        var again = deployable.Assess();

        Assert.NotSame(first, again);
        Assert.Equal(first.State, again.State);
        Assert.Same(first, deployable.LastServing);

        deployable.RecordServing(again);

        Assert.Same(again, deployable.LastServing);

        // No decision at all is an argument like any other: the member has no guard, and the monitor never hands it
        // one - the first round of a deployable is compared with nothing, which is what says it has no history yet.
        deployable.RecordServing(null!);

        Assert.Null(deployable.LastServing);
    }

    /// <summary>The decision is worked out of the deployable's node tiles as they stand at the call, and of the public
    /// address as nobody who could serve: its state arrives beside the decision as the agreement and as itself, never
    /// as one of the nodes - an address healthy in front of nothing but unreachable nodes leaves the deployable down
    /// and disagreeing. A check recorded on a tile moves the next decision and leaves the one already handed out
    /// untouched.</summary>
    [Fact]
    public void TheDecisionReadsTheNodeTilesAsTheyStandAndNeverThePublicAddressAsOneOfThem()
    {
        var deployable = Deployable(frontDoor: Door, nodes: [Node("app-west"), Node("app-east", "eastus2", NodeInfo.StandbyRole)]);

        // Nothing checked yet: pending, no tile expected, and the state of the public address held out beside it.
        var pending = deployable.Assess(out var nobody);

        Assert.Equal(ServingState.Pending, pending.State);
        Assert.Null(nobody);
        Assert.Equal(HealthState.Pending, pending.FrontDoorState);
        Assert.Equal(FrontDoorAgreement.Pending, pending.FrontDoor);

        deployable.Nodes[0].Record(Answer(HealthState.Healthy));

        var primary = deployable.Assess(out var west);

        Assert.Equal(ServingState.Primary, primary.State);
        Assert.Same(deployable.Nodes[0], west);
        Assert.Equal(["app-west"], primary.Serving.Select(node => node.Name));

        // The same states decided again are two decisions, and the first of them says what it said.
        Assert.Equal(primary.State, deployable.Assess().State);
        Assert.NotSame(primary.Serving, deployable.Assess().Serving);

        // The public address disagrees and is still no node: the deployable serves what its node says.
        deployable.FrontDoor!.Record(Answer(HealthState.Unreachable));

        var disagreeing = deployable.Assess();

        Assert.Equal(FrontDoorAgreement.Disagrees, disagreeing.FrontDoor);
        Assert.Equal(HealthState.Unreachable, disagreeing.FrontDoorState);
        Assert.Equal(ServingState.Primary, disagreeing.State);

        // The node going down decides nothing while the standby has not been checked: a node of a higher priority
        // than any healthy one keeps the deployable waiting, and the agreement of the public address waits with it.
        deployable.Nodes[0].Record(Answer(HealthState.Unreachable));

        var waiting = deployable.Assess(out var stillNobody);

        Assert.Equal(ServingState.Pending, waiting.State);
        Assert.Equal(FrontDoorAgreement.Pending, waiting.FrontDoor);
        Assert.Null(stillNobody);

        // The standby down as well: nothing serves, and a public address that is unreachable with it agrees.
        deployable.Nodes[1].Record(Answer(HealthState.Unreachable));

        var down = deployable.Assess(out var none);

        Assert.Equal(ServingState.Down, down.State);
        Assert.Equal(FrontDoorAgreement.Agrees, down.FrontDoor);
        Assert.Null(none);

        var last = deployable.AssessServing(out var serving);

        Assert.Equal(ServingState.Down, last.State);
        Assert.Empty(serving);
        Assert.DoesNotContain(deployable.FrontDoor!, serving);

        // The public address answering while nothing behind it serves is a disagreement about a deployable that is
        // down all the same: the endpoint's own state never turns a node healthy.
        deployable.FrontDoor!.Record(Answer(HealthState.Healthy));

        var atOdds = deployable.Assess();

        Assert.Equal(ServingState.Down, atOdds.State);
        Assert.Equal(FrontDoorAgreement.Disagrees, atOdds.FrontDoor);
        Assert.Equal(HealthState.Healthy, atOdds.FrontDoorState);
    }

    /// <summary>A rotation's decision names every tile the deployable holds that serves, in the order of the topology,
    /// and the tile <see cref="DeployableStatus.Assess" /> expects to serve is the first of them. The match back from
    /// the decision's nodes to the deployable's tiles is by the tile and not by what it says: three rows of the file
    /// that write one name, one region, one role and one address are three tiles, and all three stand in the list -
    /// which is what no comparison by value would ever answer. Every read makes its own list, and a list empties as
    /// the twins stop serving.</summary>
    [Fact]
    public void EveryTileOfARotationThatServesIsNamedAndTheFirstOfThemIsTheOneExpected()
    {
        var twin = new NodeInfo("app", "westus3", NodeInfo.PrimaryRole, West);
        var deployable = Deployable(frontDoor: Door, nodes: [twin, twin with { }, twin with { }]);

        Assert.All(deployable.Nodes, node => node.Record(Answer(HealthState.Healthy)));

        var assessment = deployable.AssessServing(out var serving);

        Assert.True(assessment.IsRotation);
        Assert.Equal(ServingState.Primary, assessment.State);
        Assert.Equal(3, assessment.Rotation!.Count);
        Assert.Equal(3, serving.Count);
        Assert.Equal(3, serving.Distinct().Count());
        Assert.Same(deployable.Nodes[0], serving[0]);
        Assert.Same(deployable.Nodes[1], serving[1]);
        Assert.Same(deployable.Nodes[2], serving[2]);
        Assert.DoesNotContain(deployable.FrontDoor!, serving);
        Assert.Equal(["app", "app", "app"], serving.Select(tile => tile.Name));

        // The tile expected to serve is the first of those named, and every read of the list is a list of its own.
        var decision = deployable.Assess(out var expected);

        Assert.Equal(ServingState.Primary, decision.State);
        Assert.Same(serving[0], expected);
        Assert.Same(deployable.Nodes[0], expected);
        Assert.NotSame(serving, deployable.AssessServing(out _));

        // Two of the three stop answering: the twin left standing is the one named, and the only one.
        deployable.Nodes[0].Record(Answer(HealthState.Unreachable));
        deployable.Nodes[1].Record(Answer(HealthState.Unhealthy));

        var one = deployable.Assess(out var lastOfThem);

        deployable.AssessServing(out var tiles);

        Assert.Equal(ServingState.Primary, one.State);
        Assert.Same(deployable.Nodes[2], Assert.Single(tiles));
        Assert.Same(deployable.Nodes[2], lastOfThem);

        // And with none of them healthy the list is empty and nobody is expected.
        deployable.Nodes[2].Record(Answer(HealthState.Unreachable));

        var down = deployable.AssessServing(out var nothing);

        Assert.Equal(ServingState.Down, down.State);
        Assert.Empty(nothing);
        Assert.Equal(ServingState.Down, deployable.Assess(out var nobody).State);
        Assert.Null(nobody);
    }

    // ----- the four recordings, each in its own part

    /// <summary>The deployable keeps four memories and writes them separately: a pin read records no build and decides
    /// no state, a build read remembers no pin, a decision recorded reads nothing, and the tile of a node that
    /// answered holds none of the three. Nothing of the row moves either - the tiles, the links and the name stand
    /// where the topology put them.</summary>
    [Fact]
    public void EachRecordingOfADeployableTouchesItsOwnMemoryAndNoOther()
    {
        var deployable = Deployable(frontDoor: Door, nodes: [Node("app-west")], buildPath: "/_build", pinUrl: OwnPin, links: DeployableLinks);

        deployable.RecordPin(Pin("2.4.14"));

        Assert.Null(deployable.Build);
        Assert.False(deployable.BuildWasRead);
        Assert.Null(deployable.LastServing);
        Assert.Equal(HealthState.Pending, deployable.Nodes[0].State);

        deployable.RecordBuild(Build("2.4.14"), "2.4.14");

        Assert.Equal(PinnedVersionsState.Read, deployable.Pinned!.State);
        Assert.Null(deployable.LastReadPinned);
        Assert.Null(deployable.LastServing);
        Assert.Empty(deployable.Nodes[0].History);

        deployable.RememberPin(Pin("2.4.14"));
        deployable.RecordServing(deployable.Assess());

        Assert.NotNull(deployable.LastServing);
        Assert.NotNull(deployable.LastReadPinned);
        Assert.Null(deployable.FrontDoor!.Last);
        Assert.Equal("ui", deployable.Info.Name);
        Assert.Same(DeployableLinks, deployable.FrontDoor.Links);

        // And a check of a node is none of the three: the deployable's own part of it is the state it is decided from.
        deployable.Nodes[0].Record(Answer(HealthState.Healthy));

        Assert.Equal(PinnedVersionsState.Read, deployable.Pinned!.State);
        Assert.Equal("2.4.14", deployable.Build!.Version);
        Assert.Equal(ServingState.Pending, deployable.LastServing!.State);
        Assert.Equal(HealthState.Healthy, deployable.Nodes[0].State);
    }

    // ----- the surface

    /// <summary>The whole public surface the class declares for itself: its twelve parts named in the order they
    /// stand, the seven members that decide or record something, the one argument of its constructor, no field, no
    /// event, no nested type and no interface - and the six parts a round writes kept to the class alone. A member
    /// added here has to be written down here too, which is the point: a part of a deployable nobody decided the
    /// meaning of cannot be shown on a page.</summary>
    [Fact]
    public void TheDeployableDeclaresTheMembersListedHereAndNothingElse()
    {
        var properties = typeof(DeployableStatus)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(
            [
                "Pinned", "LastReadPinned", "BuildSource", "Build", "BuildWasRead", "BuildReadForVersion",
                "LastServing", "Info", "FrontDoor", "HasFrontDoor", "Nodes", "Targets",
            ],
            [.. properties.Keys]);

        Assert.Equal(
            ["Assess", "AssessServing", "BuildIsDue", "RecordBuild", "RecordPin", "RecordServing", "RememberPin"],
            PublicMembers());

        // The twelve parts, each of the type the class says it is.
        Assert.Equal(typeof(PinnedVersions), properties["Pinned"].PropertyType);
        Assert.Equal(typeof(PinnedVersions), properties["LastReadPinned"].PropertyType);
        Assert.Equal(typeof(TargetStatus), properties["BuildSource"].PropertyType);
        Assert.Equal(typeof(BuildInfo), properties["Build"].PropertyType);
        Assert.Equal(typeof(bool), properties["BuildWasRead"].PropertyType);
        Assert.Equal(typeof(string), properties["BuildReadForVersion"].PropertyType);
        Assert.Equal(typeof(ServingAssessment), properties["LastServing"].PropertyType);
        Assert.Equal(typeof(DeployableInfo), properties["Info"].PropertyType);
        Assert.Equal(typeof(TargetStatus), properties["FrontDoor"].PropertyType);
        Assert.Equal(typeof(bool), properties["HasFrontDoor"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<TargetStatus>), properties["Nodes"].PropertyType);
        Assert.Equal(typeof(IEnumerable<TargetStatus>), properties["Targets"].PropertyType);

        // Everything is readable from outside. Four of the twelve are still writable from nowhere else; the pin
        // pair (Pinned, LastReadPinned) is no longer written by a set method - the two memories are owned by one
        // Reading<PinnedVersions> field and moved by RecordPin and RememberPin, which is why they are read-only here.
        Assert.All(properties.Values, property => Assert.NotNull(property.GetMethod));
        Assert.Equal(
            ["Build", "BuildReadForVersion", "BuildWasRead", "LastServing"],
            [.. properties.Values.Where(property => property.SetMethod is not null).Select(property => property.Name).Order(StringComparer.Ordinal)]);
        Assert.All(properties.Values, property => Assert.True(property.SetMethod is null || property.SetMethod.IsPrivate));

        // The one constructor, naming its one part as the property it is kept as.
        var parameters = typeof(DeployableStatus).GetConstructors().Single().GetParameters();

        Assert.Single(parameters);
        Assert.Equal("info", parameters[0].Name);
        Assert.Equal(typeof(DeployableInfo), parameters[0].ParameterType);
        Assert.False(parameters[0].HasDefaultValue);

        Assert.Empty(typeof(DeployableStatus).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(DeployableStatus).GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(DeployableStatus).GetNestedTypes(BindingFlags.Public));
        Assert.Empty(typeof(DeployableStatus).GetInterfaces());
    }

    /// <summary>The public members the deployable declares itself beyond its parts: the methods that are not the get
    /// and the set of one of them, with the two <c>Assess</c> overloads standing as one name and the constructor named
    /// by the test above rather than by this list.</summary>
    private static List<string> PublicMembers() =>
        [.. typeof(DeployableStatus)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OfType<MethodInfo>()
            .Where(method => !method.IsSpecialName && !method.Name.Contains('<', StringComparison.Ordinal))
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
