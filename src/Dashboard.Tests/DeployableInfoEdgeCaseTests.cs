using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeployableInfo" />: one deployable of an
/// environment, its fourteen parts and the two decisions it makes of them.
/// <para>
/// What the other files already pin is left alone. <see cref="TopologyParserTests" /> pins what the file becomes:
/// the three paths a deployable falls back to when the file names none, the path it gives a leading slash, the
/// absent and the null <see cref="FrontDoor" />, <see cref="ProjectUrl" /> and <see cref="PinUrl" />, the name
/// <see cref="PinFile" /> reads out of a kustomization and out of no address at all, and <see cref="PathFor" /> for
/// the two probes of a deployable that names its own paths. <see cref="TelemetryTests" /> pins the telemetry and
/// the traffic paths as the file gives them, <see cref="KustomizationPinMonitorTests" /> the pin file of three
/// deployables of a running monitor, and <see cref="PinnedVersionsTests" /> the last part of an address that
/// <see cref="PinFile" /> leans on. <see cref="ViewLinksTests" /> and <see cref="TargetStatusTests" /> pin what the
/// links and the tiles are made of.
/// </para>
/// <para>
/// What is pinned here is the record itself, which all of that walks past: the three words of the defaults; that
/// the fourteen parts come back to the property of their own name, and that the eight the caller does not name
/// stand absent while the six it must name stand where it names them; that a deployable refuses nothing it is
/// given - the empty word is a word and not nothing, the spaces about a word stay, and an empty list is not the
/// absent one; that <see cref="PathFor" /> answers with the path it holds and never with the default, for either
/// probe and for a number the enum does not name; that <see cref="PinFile" /> names the file of whatever address is
/// named and <c>versions.json</c> only while none is; and the members every record is asked - comparing, hashing,
/// falling apart into the fourteen, copying and saying itself - with the two lists a deployable holds compared by
/// the instance they stand on and not by their contents.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a deployable holds no moment and calls
/// nothing, and an address is read here for the name of its file only, never fetched.
/// </para>
/// </summary>
public class DeployableInfoEdgeCaseTests
{
    private static readonly Uri Door = new("https://demo.azurefd.net");
    private static readonly Uri Project = new("https://octopus.example.net/app#/Spaces-42/projects/demo-ui");
    private static readonly Uri Pin = new("https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/kustomization.yaml");
    private static readonly Uri PinHistory = new("https://github.example.net/org/demo-system/commits/main/gitops/environments/uat/ui/kustomization.yaml");
    private static readonly NodeInfo West = new("app-west", "westus3", NodeInfo.PrimaryRole, new Uri("https://app-west.example.net"));
    private static readonly NodeInfo East = new("app-east", "eastus2", NodeInfo.StandbyRole, new Uri("https://app-east.example.net"));

    private static readonly IReadOnlyList<NodeInfo> OneNode = [West];
    private static readonly IReadOnlyList<NodeInfo> TwoNodes = [West, East];
    private static readonly IReadOnlyList<NodeInfo> NoNodes = [];
    private static readonly IReadOnlyList<string> TwoPaths = ["/", "/api/workorders/summary"];
    private static readonly IReadOnlyList<string> NoPaths = [];
    private static readonly LinkSet Links = new([new KeyValuePair<string, Uri>(LinkSet.FrontDoor, new Uri("https://portal.example.net/#afd"))]);

    /// <summary>A deployable with every part named and the fourteen values apart from one another, so that an
    /// argument landing on another property is seen instead of quietly matching a second part of the record.</summary>
    private static DeployableInfo Full => new(
        "ui",
        Door,
        "/_healthcheck",
        "/alive",
        "/_version",
        OneNode,
        Project,
        "/_telemetry",
        TwoPaths,
        "/_build",
        Links,
        Pin,
        PinHistory,
        "/_healthcheck/detailed");

    /// <summary>A deployable named by nothing but its six required parts, the eight a caller may leave out all
    /// standing as they stand when nobody names them.</summary>
    private static DeployableInfo Bare => new("api", null, "/_healthcheck", "/alive", "/_version", NoNodes);

    // ------------------------------------------------------------------ the three fixed paths

    /// <summary>The three paths a deployable is built with when its file says none, each the very word of the
    /// endpoint it stands for: they are spelled out here where a change of one is a change the tests see, and the
    /// three are three different words, since one endpoint answering for another would ask the wrong thing.</summary>
    [Fact]
    public void TheThreeDefaultPathsSayTheThreeEndpointsTheyStandFor()
    {
        Assert.Equal("/_healthcheck", DeployableInfo.DefaultHealthPath);
        Assert.Equal("/alive", DeployableInfo.DefaultAlivePath);
        Assert.Equal("/_version", DeployableInfo.DefaultVersionPath);

        var paths = new[] { DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath };

        Assert.Equal(3, paths.Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            paths,
            path =>
            {
                Assert.NotEqual(string.Empty, path);
                Assert.StartsWith("/", path, StringComparison.Ordinal);
                Assert.Equal(path, path.Trim());
            });
    }

    // ------------------------------------------------------------------ the parts a deployable keeps

    /// <summary>The constructor's fourteen parts, position by position, each back as the property of its own
    /// name - the order the parser hands the file over in, and the order the page reads a deployable in.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var deployable = Full;

        Assert.Equal("ui", deployable.Name);
        Assert.Equal(Door, deployable.FrontDoor);
        Assert.Equal("/_healthcheck", deployable.HealthPath);
        Assert.Equal("/alive", deployable.AlivePath);
        Assert.Equal("/_version", deployable.VersionPath);
        Assert.Same(OneNode, deployable.Nodes);
        Assert.Equal(Project, deployable.ProjectUrl);
        Assert.Equal("/_telemetry", deployable.TelemetryPath);
        Assert.Same(TwoPaths, deployable.TrafficPaths);
        Assert.Equal("/_build", deployable.BuildPath);
        Assert.Same(Links, deployable.Links);
        Assert.Equal(Pin, deployable.PinUrl);
        Assert.Equal(PinHistory, deployable.PinHistoryUrl);
        Assert.Equal("/_healthcheck/detailed", deployable.HealthDetailPath);
    }

    /// <summary>The eight parts a caller may leave out stand absent when it does, and only then: the six it must
    /// name stand where it names them, and the record fills in nothing of its own.</summary>
    [Fact]
    public void ThePartsTheCallerDoesNotNameStandAbsent()
    {
        var deployable = Bare;

        Assert.Equal("api", deployable.Name);
        Assert.Null(deployable.FrontDoor);
        Assert.Equal("/_healthcheck", deployable.HealthPath);
        Assert.Equal("/alive", deployable.AlivePath);
        Assert.Equal("/_version", deployable.VersionPath);
        Assert.Empty(deployable.Nodes);

        Assert.Null(deployable.ProjectUrl);
        Assert.Null(deployable.TelemetryPath);
        Assert.Null(deployable.TrafficPaths);
        Assert.Null(deployable.BuildPath);
        Assert.Null(deployable.Links);
        Assert.Null(deployable.PinUrl);
        Assert.Null(deployable.PinHistoryUrl);
        Assert.Null(deployable.HealthDetailPath);
    }

    /// <summary>The deployable keeps the words it is given and changes none of them: an empty path is the empty
    /// path and not nothing, the spaces about a word stay about it, a name of no letters is still a name, and an
    /// address stands as the address it was handed. (The parser that trims what the file says and gives its paths a
    /// leading slash is <see cref="TopologyParserTests" />'s.)</summary>
    [Fact]
    public void ADeployableKeepsItsWordsAsItIsGivenThem()
    {
        var empty = new DeployableInfo(
            string.Empty, null, string.Empty, string.Empty, string.Empty, NoNodes,
            TelemetryPath: string.Empty, BuildPath: string.Empty, HealthDetailPath: string.Empty);

        var spaced = new DeployableInfo("  ui  ", Door, " /_healthcheck ", "\t/alive", "/_version ", NoNodes, TelemetryPath: " /_telemetry");

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.HealthPath);
        Assert.Equal(string.Empty, empty.AlivePath);
        Assert.Equal(string.Empty, empty.VersionPath);
        Assert.Equal(string.Empty, empty.TelemetryPath);
        Assert.Equal(string.Empty, empty.BuildPath);
        Assert.Equal(string.Empty, empty.HealthDetailPath);

        Assert.Equal("  ui  ", spaced.Name);
        Assert.Equal(" /_healthcheck ", spaced.HealthPath);
        Assert.Equal("\t/alive", spaced.AlivePath);
        Assert.Equal("/_version ", spaced.VersionPath);
        Assert.Equal(" /_telemetry", spaced.TelemetryPath);
    }

    /// <summary>The empty list and no list at all are two different things: a deployable of no nodes and no
    /// traffic paths still holds the two empty lists it was given, where the one that names no traffic path holds
    /// nothing - and each list is held as the very list it was handed, never copied.</summary>
    [Fact]
    public void AnEmptyListOfItsOwnIsNotTheListThatIsAbsent()
    {
        var emptied = Full with { Nodes = NoNodes, TrafficPaths = NoPaths };
        var unnamed = Full with { TrafficPaths = null };

        Assert.Empty(emptied.Nodes);
        Assert.Empty(emptied.TrafficPaths!);
        Assert.Same(NoNodes, emptied.Nodes);
        Assert.Same(NoPaths, emptied.TrafficPaths);

        Assert.Null(unnamed.TrafficPaths);
        Assert.NotEqual(emptied, unnamed);
    }

    // ------------------------------------------------------------------ the path a probe asks

    /// <summary>The liveness probe asks the alive path and the health probe the health path, both of them on one
    /// deployable: the two never answer for one another, whichever of them is asked first.</summary>
    [Theory]
    [InlineData(ProbeKind.Health, "/_healthcheck")]
    [InlineData(ProbeKind.Liveness, "/alive")]
    public void EachProbeAsksThePathItOwns(ProbeKind probe, string asked)
    {
        Assert.Equal(asked, Full.PathFor(probe));
        Assert.Equal(asked, Bare.PathFor(probe));
    }

    /// <summary>Only liveness is answered with the alive path: a number the enum does not name is not liveness,
    /// and the deployable answers with its health path - the branch the two named probes leave open.</summary>
    [Fact]
    public void AnyProbeThatIsNotLivenessAsksTheHealthPath()
    {
        foreach (var probe in new[] { (ProbeKind)(-1), (ProbeKind)2, (ProbeKind)7, (ProbeKind)int.MaxValue })
        {
            Assert.Equal("/_healthcheck", Full.PathFor(probe));
        }
    }

    /// <summary>The path a probe asks is the path the deployable holds, however little it is: the empty path is
    /// asked as the empty path rather than as the default, and a path without a leading slash keeps it out - the
    /// default of <see cref="DeployableInfo.DefaultHealthPath" /> is the page's container app's own
    /// (<see cref="ContainerAppProbe" />), and a deployable says nothing of its own here.</summary>
    [Fact]
    public void ADeployableAsksThePathItHoldsAndNeverTheDefault()
    {
        var empty = new DeployableInfo("ui", null, string.Empty, string.Empty, string.Empty, NoNodes);
        var unslashed = new DeployableInfo("ui", null, "health", "alive", "version", NoNodes);

        Assert.Equal(string.Empty, empty.PathFor(ProbeKind.Health));
        Assert.Equal(string.Empty, empty.PathFor(ProbeKind.Liveness));
        Assert.NotEqual(DeployableInfo.DefaultHealthPath, empty.PathFor(ProbeKind.Health));
        Assert.NotEqual(DeployableInfo.DefaultAlivePath, empty.PathFor(ProbeKind.Liveness));

        Assert.Equal("health", unslashed.PathFor(ProbeKind.Health));
        Assert.Equal("alive", unslashed.PathFor(ProbeKind.Liveness));
    }

    /// <summary>The path is read from the deployable as it stands, not as it was built: the copy that turns the
    /// two paths about asks the new one for each probe, and the deployable it was copied from is untouched.</summary>
    [Fact]
    public void ADeployableAsksThePathsTheCopyCarries()
    {
        var swapped = Full with { HealthPath = "/live", AlivePath = "/health" };

        Assert.Equal("/live", swapped.PathFor(ProbeKind.Health));
        Assert.Equal("/health", swapped.PathFor(ProbeKind.Liveness));
        Assert.Equal("/_healthcheck", Full.PathFor(ProbeKind.Health));
    }

    // ------------------------------------------------------------------ the file the pin stands in

    /// <summary>The pin file is the last part of the pin address's path, whatever that part is called: a pin of
    /// another name, an address that carries a query and a fragment, a directory that ends on a slash and so names
    /// its own word rather than a file, and an address of nothing but a host, which names no file at all and falls
    /// to <c>kustomization.yaml</c>. The address is only ever named here, never fetched.</summary>
    [Theory]
    [InlineData("https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/release.yaml", "release.yaml")]
    [InlineData("https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/kustomization.yaml?ref=main#L4", "kustomization.yaml")]
    [InlineData("https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/", "ui")]
    [InlineData("https://raw.example.net/", "kustomization.yaml")]
    public void ThePinFileIsTheFileThePinAddressNames(string pinUrl, string file) =>
        Assert.Equal(file, (Full with { PinUrl = new Uri(pinUrl) }).PinFile);

    /// <summary>The two files the record decides between: the file of the address while one is named, and
    /// <c>versions.json</c> - <see cref="PinnedVersions.FileName" /> - the moment it is taken away, one and the same
    /// deployable standing both ways. The word does not move with the history address, the name or the detailed
    /// health path, and it says the same thing twice.</summary>
    [Fact]
    public void WithoutAPinAddressThePinFileIsTheVersionsFileOfTheEnvironment()
    {
        var pinned = Full;

        Assert.Equal("kustomization.yaml", pinned.PinFile);
        Assert.Equal(pinned.PinFile, pinned.PinFile);

        var unpinned = pinned with { PinUrl = null };

        Assert.Equal(PinnedVersions.FileName, unpinned.PinFile);
        Assert.Equal("versions.json", unpinned.PinFile);

        var untouched = pinned with { PinHistoryUrl = null, Name = "api", HealthDetailPath = null };

        Assert.Equal("kustomization.yaml", untouched.PinFile);
    }

    // ------------------------------------------------------------------ the record itself

    /// <summary>Two deployables are one while their fourteen parts are: the two operators, <c>Equals</c> both ways
    /// and the object it takes, and the hash code that follows them and stands from call to call. A deployable is
    /// one with nothing that is not a deployable.</summary>
    [Fact]
    public void TwoDeployablesAreOneWhileTheirFourteenPartsAre()
    {
        var one = Full;
        var two = Full;

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ui"));
        Assert.False(one.Equals(new DeployableStatus(one)));
    }

    /// <summary>The two lists a deployable holds are compared as the holds they stand on and not as their
    /// contents: the very same list makes two deployables one, and a second list of the same two nodes - and of the
    /// same two traffic paths - makes two deployables of them.</summary>
    [Fact]
    public void TheListsADeployableHoldsAreComparedAsTheHoldsTheyStandOn()
    {
        var one = Full with { Nodes = TwoNodes, TrafficPaths = TwoPaths };
        var holdingTheSameLists = Full with { Nodes = TwoNodes, TrafficPaths = TwoPaths };
        var holdingOtherLists = Full with { Nodes = [West, East], TrafficPaths = ["/", "/api/workorders/summary"] };

        Assert.Equal(one, holdingTheSameLists);
        Assert.Equal(one.GetHashCode(), holdingTheSameLists.GetHashCode());

        Assert.Equal(one.Nodes, holdingOtherLists.Nodes);
        Assert.Equal(one.TrafficPaths, holdingOtherLists.TrafficPaths);
        Assert.NotSame(one.Nodes, holdingOtherLists.Nodes);
        Assert.NotSame(one.TrafficPaths, holdingOtherLists.TrafficPaths);
        Assert.True(one != holdingOtherLists);
        Assert.NotEqual(one, holdingOtherLists);
    }

    /// <summary>Any one of the fourteen parts tells two deployables apart - the name, the front door, the three
    /// paths, the nodes, and the eight that may stand absent - and nothing is compared loosely: an address on
    /// another host, a path in another spelling, the empty word and the nothing that is not a word are all
    /// different parts.</summary>
    [Fact]
    public void ADeployableDiffersInAnyOneOfItsParts()
    {
        var deployable = Full;

        Assert.NotEqual(deployable, deployable with { Name = "api" });
        Assert.NotEqual(deployable, deployable with { FrontDoor = null });
        Assert.NotEqual(deployable, deployable with { FrontDoor = new Uri("https://other.azurefd.net") });
        Assert.NotEqual(deployable, deployable with { HealthPath = "/healthcheck" });
        Assert.NotEqual(deployable, deployable with { HealthPath = string.Empty });
        Assert.NotEqual(deployable, deployable with { AlivePath = "/Alive" });
        Assert.NotEqual(deployable, deployable with { VersionPath = string.Empty });
        Assert.NotEqual(deployable, deployable with { Nodes = TwoNodes });
        Assert.NotEqual(deployable, deployable with { Nodes = [] });
        Assert.NotEqual(deployable, deployable with { ProjectUrl = null });
        // An address is compared as an address is: the same one with another fragment after the # is the same
        // address to a Uri, so the two deployables are one, while another host tells them apart. (That the page
        // keeps the fragment it reads is TopologyParserTests'.)
        Assert.Equal(deployable, deployable with { ProjectUrl = new Uri("https://octopus.example.net/app#/Spaces-43/projects/demo-api") });
        Assert.NotEqual(deployable, deployable with { ProjectUrl = new Uri("https://octopus-other.example.net/app#/Spaces-42/projects/demo-ui") });
        Assert.NotEqual(deployable, deployable with { TelemetryPath = string.Empty });
        Assert.NotEqual(deployable, deployable with { TrafficPaths = null });
        Assert.NotEqual(deployable, deployable with { BuildPath = "/_build/" });
        Assert.NotEqual(deployable, deployable with { Links = null });
        Assert.NotEqual(deployable, deployable with { Links = LinkSet.None });
        Assert.NotEqual(deployable, deployable with { PinUrl = null });
        Assert.NotEqual(deployable, deployable with { PinHistoryUrl = null });
        Assert.NotEqual(deployable, deployable with { HealthDetailPath = null });

        // The absent and the empty are two things in every one of the words, and no part is filled in by a copy.
        Assert.NotEqual(
            new DeployableInfo("ui", null, string.Empty, string.Empty, string.Empty, NoNodes),
            new DeployableInfo("ui", null, null!, null!, null!, NoNodes));
    }

    /// <summary><c>with</c> names the part it changes and carries the other thirteen: a copy that names nothing is
    /// one with its original and not the same object, and the part it names stands as it names it.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var deployable = Full;
        var copy = deployable with { };

        Assert.Equal(deployable, copy);
        Assert.NotSame(deployable, copy);

        var moved = deployable with { FrontDoor = new Uri("https://fd-other.example.net") };

        Assert.Equal("fd-other.example.net", moved.FrontDoor!.Host);
        Assert.Equal("https://fd-other.example.net/", moved.FrontDoor.AbsoluteUri);
        Assert.Equal("ui", moved.Name);
        Assert.Equal("/_healthcheck", moved.HealthPath);
        Assert.Same(OneNode, moved.Nodes);
        Assert.Same(Links, moved.Links);
        Assert.Equal("/_telemetry", moved.TelemetryPath);
        Assert.Equal(["/", "/api/workorders/summary"], moved.TrafficPaths!);
        Assert.Equal("/_healthcheck/detailed", moved.HealthDetailPath);

        var emptied = deployable with { TelemetryPath = null, TrafficPaths = null, Links = null };

        Assert.Null(emptied.TelemetryPath);
        Assert.Null(emptied.TrafficPaths);
        Assert.Null(emptied.Links);
        Assert.Equal("ui", emptied.Name);
    }

    /// <summary>The fourteen parts read out of the deployable in the order they stand in it, each as the value it
    /// holds, and the absent ones reading out as the nothing they are.</summary>
    [Fact]
    public void TheFourteenPartsReadOutOfTheDeployableInTheOrderTheyStand()
    {
        var (name, frontDoor, healthPath, alivePath, versionPath, nodes, projectUrl, telemetryPath, trafficPaths, buildPath, links, pinUrl, pinHistoryUrl, healthDetailPath) = Full;

        Assert.Equal("ui", name);
        Assert.Equal(Door, frontDoor);
        Assert.Equal("/_healthcheck", healthPath);
        Assert.Equal("/alive", alivePath);
        Assert.Equal("/_version", versionPath);
        Assert.Same(OneNode, nodes);
        Assert.Equal(Project, projectUrl);
        Assert.Equal("/_telemetry", telemetryPath);
        Assert.Same(TwoPaths, trafficPaths);
        Assert.Equal("/_build", buildPath);
        Assert.Same(Links, links);
        Assert.Equal(Pin, pinUrl);
        Assert.Equal(PinHistory, pinHistoryUrl);
        Assert.Equal("/_healthcheck/detailed", healthDetailPath);

        var (bareName, bareDoor, bareHealth, bareAlive, bareVersion, bareNodes, bareProject, bareTelemetry, bareTraffic, bareBuild, bareLinks, barePin, bareHistory, bareDetail) = Bare;

        Assert.Equal("api", bareName);
        Assert.Null(bareDoor);
        Assert.Equal("/_healthcheck", bareHealth);
        Assert.Equal("/alive", bareAlive);
        Assert.Equal("/_version", bareVersion);
        Assert.Same(NoNodes, bareNodes);
        Assert.Null(bareProject);
        Assert.Null(bareTelemetry);
        Assert.Null(bareTraffic);
        Assert.Null(bareBuild);
        Assert.Null(bareLinks);
        Assert.Null(barePin);
        Assert.Null(bareHistory);
        Assert.Null(bareDetail);
    }

    /// <summary>The words the deployable says about itself: its type, then its fourteen parts in their order,
    /// each named and an absent part named and left blank - and the pin file last, which the record says of its own
    /// although no caller wrote it. A list says the collection it stands on and a link set the type it is, the line
    /// never opening either. (The whole line stands here, so a part moved about in the record is a change the tests
    /// see.)</summary>
    [Fact]
    public void TheDeployableSaysItsPartsInTheirOrder()
    {
        var spoken = new DeployableInfo(
            "ui",
            Door,
            "/_healthcheck",
            "/alive",
            "/_version",
            new List<NodeInfo> { West, East },
            Project,
            "/_telemetry",
            new List<string> { "/", "/api/workorders/summary" },
            "/_build",
            Links,
            Pin,
            PinHistory,
            "/_healthcheck/detailed");

        Assert.Equal(
            "DeployableInfo { Name = ui, FrontDoor = https://demo.azurefd.net/, HealthPath = /_healthcheck, "
                + "AlivePath = /alive, VersionPath = /_version, "
                + "Nodes = System.Collections.Generic.List`1[Dashboard.Health.NodeInfo], "
                + "ProjectUrl = https://octopus.example.net/app#/Spaces-42/projects/demo-ui, "
                + "TelemetryPath = /_telemetry, TrafficPaths = System.Collections.Generic.List`1[System.String], "
                + "BuildPath = /_build, Links = Dashboard.Health.LinkSet, "
                + "PinUrl = https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/kustomization.yaml, "
                + "PinHistoryUrl = https://github.example.net/org/demo-system/commits/main/gitops/environments/uat/ui/kustomization.yaml, "
                + "HealthDetailPath = /_healthcheck/detailed, PinFile = kustomization.yaml }",
            spoken.ToString());

        // The same line with the eight parts a caller may leave out, each named and saying nothing.
        Assert.Equal(
            "DeployableInfo { Name = api, FrontDoor = , HealthPath = /_healthcheck, AlivePath = /alive, "
                + "VersionPath = /_version, Nodes = Dashboard.Health.NodeInfo[], ProjectUrl = , TelemetryPath = , "
                + "TrafficPaths = , BuildPath = , Links = , PinUrl = , PinHistoryUrl = , HealthDetailPath = , "
                + "PinFile = versions.json }",
            Bare.ToString());
    }

    // ------------------------------------------------------------------ the record's own shape

    /// <summary>The deployable's whole public surface as it stands: its fourteen parts, named in the order the
    /// constructor names them and of the type the constructor says, the two members that decide anything
    /// (<see cref="PinFile" /> and <see cref="PathFor" />), the three fixed paths, and the members every record is
    /// asked - falling apart, comparing, hashing and saying itself - and nothing else to call. A fifteenth part
    /// added here has to be written down here too, which is the point: the page cannot be shown a part nobody
    /// decided what it means.</summary>
    [Fact]
    public void TheDeployableHasFourteenPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            [
                "Name", "FrontDoor", "HealthPath", "AlivePath", "VersionPath", "Nodes", "ProjectUrl", "TelemetryPath",
                "TrafficPaths", "BuildPath", "Links", "PinUrl", "PinHistoryUrl", "HealthDetailPath", "PinFile",
            ],
            [.. typeof(DeployableInfo)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [
                "AlivePath", "BuildPath", "Deconstruct", "Equals", "FrontDoor", "GetHashCode", "HealthDetailPath",
                "HealthPath", "Links", "Name", "Nodes", "PathFor", "PinFile", "PinHistoryUrl", "PinUrl",
                "ProjectUrl", "TelemetryPath", "ToString", "TrafficPaths", "VersionPath", "op_Equality", "op_Inequality",
            ],
            PublicSurface());

        var properties = typeof(DeployableInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(Uri), properties["FrontDoor"].PropertyType);
        Assert.Equal(typeof(string), properties["HealthPath"].PropertyType);
        Assert.Equal(typeof(string), properties["AlivePath"].PropertyType);
        Assert.Equal(typeof(string), properties["VersionPath"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<NodeInfo>), properties["Nodes"].PropertyType);
        Assert.Equal(typeof(Uri), properties["ProjectUrl"].PropertyType);
        Assert.Equal(typeof(string), properties["TelemetryPath"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), properties["TrafficPaths"].PropertyType);
        Assert.Equal(typeof(string), properties["BuildPath"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);
        Assert.Equal(typeof(Uri), properties["PinUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["PinHistoryUrl"].PropertyType);
        Assert.Equal(typeof(string), properties["HealthDetailPath"].PropertyType);
        Assert.Equal(typeof(string), properties["PinFile"].PropertyType);

        // The one constructor, naming its fourteen parts as the properties are named: a part may be left out only
        // from the seventh on, the six before it being what a deployable must be told.
        var parameters = typeof(DeployableInfo).GetConstructors().Single().GetParameters();

        Assert.Equal(
            [
                "Name", "FrontDoor", "HealthPath", "AlivePath", "VersionPath", "Nodes", "ProjectUrl", "TelemetryPath",
                "TrafficPaths", "BuildPath", "Links", "PinUrl", "PinHistoryUrl", "HealthDetailPath",
            ],
            [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false, false, false, false, false, true, true, true, true, true, true, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);

        // And the eight default to nothing at all: the record keeps no path, no address and no list of its own.
        Assert.All(parameters.Skip(6), parameter => Assert.Null(parameter.DefaultValue));

        Assert.Equal([typeof(IEquatable<DeployableInfo>)], typeof(DeployableInfo).GetInterfaces());

        // The three fixed paths are the only fields the record carries, and all three are words.
        Assert.Equal(
            ["DefaultAlivePath", "DefaultHealthPath", "DefaultVersionPath"],
            [.. typeof(DeployableInfo)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(field => field.Name)
                .Order(StringComparer.Ordinal)]);
    }

    // ------------------------------------------------------------------ the machinery

    /// <summary>The public members the deployable declares itself: its properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(DeployableInfo)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
