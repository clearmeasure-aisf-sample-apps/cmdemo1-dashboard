using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the rest of <c>topology.json</c> as records:
/// <see cref="Topology" /> itself, its <see cref="SystemInfo" /> and <see cref="DashboardInfo" />, an
/// <see cref="EnvironmentInfo" /> with its <see cref="PageLink" /> pages, and the <see cref="ClusterInfo" /> a
/// system may name.
/// <para>
/// What the other files already pin is left alone. <see cref="TopologyParserTests" /> pins what the file becomes:
/// the four parts of a topology read out of it, the moment it says it was generated, the absent and the null
/// cluster, an environment's name, tier, version addresses and pages, a page's tooltip, that a topology knows
/// whether any deployable of the file has a Front Door, and the addresses a file may not name.
/// <see cref="WorkItemsTests" /> pins the work items address beside a <c>delivery.json</c> the file named and the
/// nothing that stands when it names none; <see cref="DashboardBuildTests" /> and
/// <see cref="DeployableFactsTests" /> pin the dashboard the file names; <see cref="ClusterMonitorEdgeCaseTests" />
/// pins what a monitor makes of a cluster's two addresses; and the page reads the rest of it, as
/// <see cref="EmptyStateStatusRegionTests" /> and <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pin.
/// </para>
/// <para>
/// <see cref="DeployableInfo" /> and <see cref="NodeInfo" /> - the two records of the file that are not covered
/// here - have their own boundary tests in <see cref="DeployableInfoEdgeCaseTests" /> and
/// <see cref="NodeInfoEdgeCaseTests" />, including their three fixed paths, their two role words, the path a probe
/// is answered with, the pin file and the primary decision. What is pinned here is the rest of the file on its own:
/// that the parts of each record come back to the property of their own name and that everything a caller may leave
/// out stands absent rather than filled in; that the empty word is a word and not nothing, and an empty list not the
/// list that is absent; the two fixed words <see cref="DashboardInfo.DefaultName" /> and a page's tooltip are made
/// of, at the nothing they are asked about; that the two files a system derives from its delivery facts stand beside
/// them - in their directory, never under them, and with the query and the fragment of the address they are derived
/// from dropped; that a topology asks nothing of a list that is not there; that the moment a topology was generated
/// is weighed as an instant and not as the offset it was written in; and the members every record is asked -
/// comparing, hashing, falling apart, copying and saying itself - with the lists a topology or an environment holds
/// compared by the instance they stand on and not by their contents.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every address is named and never fetched, and
/// every moment is one the test brings. The one member that would need a clock of its own - when a topology was
/// generated - is a part the caller hands over and the record only keeps.
/// </para>
/// </summary>
public class TopologyEdgeCaseTests
{
    private static readonly Uri Door = new("https://demo.azurefd.net");
    private static readonly Uri OtherDoor = new("https://fd-other.example.net");
    private static readonly Uri Repository = new("https://github.example.net/org/demo-system");
    private static readonly Uri Delivery = new("https://raw.example.net/org/demo-system/status/delivery.json");
    private static readonly Uri Cost = new("https://raw.example.net/org/demo-system/status/cost.json");
    private static readonly Uri Deployments = new("https://raw.example.net/org/demo-system/deployments/deployments.json");
    private static readonly Uri Versions = new("https://raw.example.net/org/demo-system/main/environments/uat/versions.json");
    private static readonly Uri VersionsHistory = new("https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json");
    private static readonly Uri StatusUrl = new("https://raw.example.net/org/demo-system/status/cluster.json");
    private static readonly Uri ServiceUrl = new("https://raw.example.net/org/demo-system/status/aks.json");
    private static readonly Uri PageAddress = new("https://ca-demo-uat-scorecard.example.net/scorecard");
    private static readonly Uri ReportsAddress = new("https://reports.example.net/");
    private static readonly Uri NodeAddress = new("https://ui.uat.example.net");

    private static readonly LinkSet Links = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]);
    private static readonly LinkSet OtherLinks = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]);

    private static readonly DateTimeOffset Generated = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly NodeInfo West = new("ca-demo-uat-ui", "westus3", NodeInfo.PrimaryRole, NodeAddress);
    private static readonly IReadOnlyList<NodeInfo> OneNode = [West];

    private static readonly DashboardInfo Dashboard = new(DashboardInfo.DefaultName, "/build-facts.json");

    private static DeployableInfo Deployable(Uri? frontDoor) =>
        new("ui", frontDoor, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode);

    private static readonly DeployableInfo AtDoor = Deployable(Door);
    private static readonly DeployableInfo NoDoor = Deployable(null);
    private static readonly IReadOnlyList<DeployableInfo> TwoDeployables = new[] { NoDoor, AtDoor };
    private static readonly IReadOnlyList<DeployableInfo> NoDeployables = [];

    private static readonly PageLink Scorecard = new("Scorecard", PageAddress, "scorecard");
    private static readonly PageLink Reports = new("Reports", ReportsAddress);
    private static readonly IReadOnlyList<PageLink> TwoPages = new[] { Scorecard, Reports };
    private static readonly IReadOnlyList<PageLink> NoPages = [];

    /// <summary>A system with every part named and the seven values apart from one another, so that an argument
    /// landing on another property is seen instead of quietly matching a second part of the record.</summary>
    private static readonly SystemInfo FullSystem = new("cmdemo1", "Demo system", Repository, Delivery, Cost, Dashboard, Deployments);

    /// <summary>A system named by nothing but its two required parts, the five a caller may leave out standing as
    /// they stand when nobody names them - and with no delivery facts to derive a file from.</summary>
    private static readonly SystemInfo BareSystem = new("demo", "Demo");

    /// <summary>An environment with every part named: its tier, its two version addresses, its links, its
    /// namespace, its two pages and its two deployables - the second of them with a Front Door.</summary>
    private static readonly EnvironmentInfo FullEnvironment = new("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);

    /// <summary>An environment named by nothing but its name and its deployables, the six a caller may leave out
    /// standing absent.</summary>
    private static readonly EnvironmentInfo BareEnvironment = new("tdd", null, NoDeployables);

    private static readonly ClusterInfo Cluster = new("aks-demo", StatusUrl, ServiceUrl, Links);

    private static readonly IReadOnlyList<EnvironmentInfo> OneEnvironment = new[] { BareEnvironment };
    private static readonly IReadOnlyList<EnvironmentInfo> TwoEnvironments = new[] { BareEnvironment, FullEnvironment };
    private static readonly IReadOnlyList<EnvironmentInfo> NoEnvironments = [];

    /// <summary>A topology with every part named: the system, the moment it was generated, two environments - the
    /// second of them holding the deployable with a Front Door - and the cluster it runs in.</summary>
    private static Topology Full => new(FullSystem, Generated, TwoEnvironments, Cluster);

    /// <summary>A topology named by nothing but the two parts a caller must name: a bare system and no environment
    /// at all, the moment and the cluster standing as they stand when nobody names them.</summary>
    private static Topology Bare => new(BareSystem, null, NoEnvironments);

    // ------------------------------------------------------------------ the four parts of a topology

    /// <summary>The constructor's four parts, each back as the property of its own name: the system, the moment,
    /// the environments and the cluster. A list is held as the very list it was handed, never copied.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var topology = Full;

        Assert.Same(FullSystem, topology.System);
        Assert.Equal(Generated, topology.Generated);
        Assert.Same(TwoEnvironments, topology.Environments);
        Assert.Same(Cluster, topology.Cluster);
    }

    /// <summary>The cluster is the one part a caller may leave out, and it stands absent when it does - absent, and
    /// not a cluster of nothing at all, which is what the page would be shown instead of no cluster view. The
    /// moment may be named as nothing too: a topology the file says nothing about when it was written says
    /// nothing.</summary>
    [Fact]
    public void ThePartsTheCallerDoesNotNameStandAbsent()
    {
        var topology = Bare;

        Assert.Same(BareSystem, topology.System);
        Assert.Null(topology.Generated);
        Assert.Empty(topology.Environments);
        Assert.Null(topology.Cluster);

        Assert.Null(new Topology(BareSystem, null, OneEnvironment, null).Cluster);
        Assert.Null(new Topology(BareSystem, null, OneEnvironment, Cluster: null).Cluster);
    }

    /// <summary>The record refuses nothing it is given and fills in nothing of its own: a topology will hold no
    /// system and no environment list at all - it is the reader of the file that refuses those, not the record -
    /// the two ends of the moment are kept as they are, and a list copied from an empty one is a second list, not
    /// the one it was copied out of. (That a file's moment is read as a moment, and that its lists are always made,
    /// is <see cref="TopologyParserTests" />'s.)</summary>
    [Fact]
    public void ATopologyHoldsTheNothingItIsGiven()
    {
        var nothing = new Topology(null!, null, null!);

        Assert.Null(nothing.System);
        Assert.Null(nothing.Generated);
        Assert.Null(nothing.Environments);
        Assert.Null(nothing.Cluster);

        Assert.Equal(DateTimeOffset.MinValue, new Topology(BareSystem, DateTimeOffset.MinValue, NoEnvironments).Generated);
        Assert.Equal(DateTimeOffset.MaxValue, new Topology(BareSystem, DateTimeOffset.MaxValue, NoEnvironments).Generated);
        Assert.Equal(
            TimeSpan.FromHours(-11),
            new Topology(BareSystem, new DateTimeOffset(2026, 10, 4, 23, 0, 0, TimeSpan.FromHours(-11)), NoEnvironments).Generated!.Value.Offset);

        var emptied = new Topology(BareSystem, Generated, NoEnvironments) with { Environments = [.. NoEnvironments] };

        Assert.Empty(emptied.Environments);
        Assert.NotSame(NoEnvironments, emptied.Environments);
    }

    // ------------------------------------------------------------------ whether any deployable has a front door

    /// <summary>A Front Door anywhere makes the answer true, however deep it stands: the second deployable of the
    /// second environment counts as much as the first of the first, and a deployable of no node at all is still a
    /// deployable with a Front Door. Only when nowhere is there one is the answer false - an environment of no
    /// deployable, a deployable whose Front Door is the nothing it may be, and no environment at all.</summary>
    [Theory]
    [MemberData(nameof(FrontDoorShapes))]
    public void ATopologyHasAFrontDoorWhenAnyDeployableOfAnyEnvironmentHasOne(IReadOnlyList<EnvironmentInfo> environments, bool expected) =>
        Assert.Equal(expected, new Topology(BareSystem, null, environments).HasFrontDoor);

    public static TheoryData<IReadOnlyList<EnvironmentInfo>, bool> FrontDoorShapes()
    {
        var doorWithoutNodes = new DeployableInfo("api", Door, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, []);
        var secondDoor = Deployable(OtherDoor);

        return new TheoryData<IReadOnlyList<EnvironmentInfo>, bool>
        {
            { [new EnvironmentInfo("uat", null, [AtDoor])], true },
            { [new EnvironmentInfo("uat", null, [NoDoor, doorWithoutNodes])], true },
            { [new EnvironmentInfo("prod", null, [NoDoor, secondDoor])], true },
            { [BareEnvironment, new EnvironmentInfo("prod", null, [NoDoor, AtDoor])], true },
            { [new EnvironmentInfo("uat", null, [NoDoor, NoDoor])], false },
            { [new EnvironmentInfo("uat", null, []), new EnvironmentInfo("prod", null, [NoDoor])], false },
            { [], false },
        };
    }

    /// <summary>The answer is made of nothing but the Front Doors of the deployables: it says the same thing twice,
    /// and it does not move with the system, with the moment the topology was generated, with the cluster, with a
    /// deployable's own paths or with the links a deployable holds.</summary>
    [Fact]
    public void TheFrontDoorAnswerSaysNothingAboutTheOtherParts()
    {
        var topology = Full;

        Assert.True(topology.HasFrontDoor);
        Assert.True(topology.HasFrontDoor);

        Assert.True((topology with { System = BareSystem, Generated = null, Cluster = null }).HasFrontDoor);
        Assert.True((topology with { Environments = [FullEnvironment with { Deployables = [AtDoor with { HealthPath = "/health", VersionPath = "/v" }] }] }).HasFrontDoor);
        Assert.True((topology with { Environments = [FullEnvironment with { Deployables = [AtDoor with { Links = Links }] }] }).HasFrontDoor);

        Assert.False((topology with { Environments = [FullEnvironment with { Deployables = [NoDoor] }] }).HasFrontDoor);
        Assert.False((topology with { Environments = [BareEnvironment, FullEnvironment with { Deployables = [] }] }).HasFrontDoor);
    }

    /// <summary>The two lists the answer walks over are asked as lists: a topology that holds none and an
    /// environment that holds none are refused outright rather than answered with a false that would read as "no
    /// Front Door anywhere" - the nothing the record keeps where a caller hands it over, and an error the reader of
    /// the file never lets through (<see cref="TopologyParserTests" />).</summary>
    [Fact]
    public void ATopologyAsksNothingOfAListThatIsNotThere()
    {
        var noEnvironments = new Topology(BareSystem, null, null!);
        var noDeployables = new Topology(BareSystem, null, [new EnvironmentInfo("uat", null, null!)]);

        Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => noEnvironments.HasFrontDoor).ParamName);
        Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => noDeployables.HasFrontDoor).ParamName);
    }

    // ------------------------------------------------------------------ the topology itself

    /// <summary>Two topologies are one while their four parts are: the two operators, <c>Equals</c> both ways and
    /// the object it takes, and the hash code that follows them and stands from call to call. A topology is one with
    /// nothing that is not a topology - not with the system it names, and not with the file it was read from.</summary>
    [Fact]
    public void TwoTopologiesAreOneWhileTheirFourPartsAre()
    {
        var one = Full;
        var two = new Topology(FullSystem, Generated, TwoEnvironments, Cluster);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("topology.json"));
        Assert.False(one.Equals(FullSystem));
        Assert.False(one.Equals(Bare));
    }

    /// <summary>The moment is weighed as an instant: the same moment written in another offset is the one moment to
    /// a record - <see cref="DateTimeOffset" /> comparing what happened and not how it was written - while a moment
    /// a minute earlier and no moment at all are two other topologies.</summary>
    [Fact]
    public void TheMomentATopologyWasGeneratedIsWeighedAsAnInstant()
    {
        var written = new DateTimeOffset(2026, 10, 4, 23, 0, 0, TimeSpan.FromHours(1));

        Assert.Equal(Generated.ToUnixTimeMilliseconds(), written.ToUnixTimeMilliseconds());
        Assert.Equal(Full, Full with { Generated = written });
        Assert.Equal(Full.GetHashCode(), (Full with { Generated = written }).GetHashCode());

        Assert.NotEqual(Full, Full with { Generated = Generated.AddMinutes(1) });
        Assert.NotEqual(Full, Full with { Generated = null });
    }

    /// <summary>Any one of the four parts tells two topologies apart, and nothing is compared loosely: the absent
    /// list and the empty one are two parts, as are the absent cluster and a cluster of no name, and a second list
    /// of the one and the same environment makes two topologies although the environment in it is the very
    /// environment.</summary>
    [Fact]
    public void ATopologyDiffersInAnyOneOfItsParts()
    {
        var topology = Full;

        Assert.NotEqual(topology, topology with { System = BareSystem });
        Assert.NotEqual(topology, topology with { System = null! });
        Assert.NotEqual(topology, topology with { Generated = null });
        Assert.NotEqual(topology, topology with { Cluster = null });
        Assert.NotEqual(topology, topology with { Cluster = new ClusterInfo(null) });
        Assert.NotEqual(topology, topology with { Environments = NoEnvironments });
        Assert.NotEqual(topology, topology with { Environments = OneEnvironment });
        Assert.NotEqual(topology, topology with { Environments = [.. TwoEnvironments] });
        Assert.NotEqual(topology, topology with { Environments = null! });

        Assert.True(topology != topology with { Environments = [.. TwoEnvironments] });
    }

    /// <summary><c>with</c> names the part it changes and carries the other three: a copy that names nothing is one
    /// with its original and not the same object, and the topology it was copied from is untouched.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var topology = Full;
        var copy = topology with { };

        Assert.Equal(topology, copy);
        Assert.NotSame(topology, copy);

        var moved = topology with { Cluster = null, Generated = null };

        Assert.Null(moved.Cluster);
        Assert.Null(moved.Generated);
        Assert.Same(FullSystem, moved.System);
        Assert.Same(TwoEnvironments, moved.Environments);
        Assert.Equal(Generated, topology.Generated);
        Assert.Same(Cluster, topology.Cluster);
    }

    /// <summary>The four parts read out of the topology in the order they stand in it, each as the value it holds,
    /// and the absent ones reading out as the nothing they are.</summary>
    [Fact]
    public void TheFourPartsReadOutOfTheTopologyInTheOrderTheyStand()
    {
        var (system, generated, environments, cluster) = Full;

        Assert.Same(FullSystem, system);
        Assert.Equal(Generated, generated);
        Assert.Same(TwoEnvironments, environments);
        Assert.Same(Cluster, cluster);

        var (bareSystem, bareGenerated, bareEnvironments, bareCluster) = Bare;

        Assert.Same(BareSystem, bareSystem);
        Assert.Null(bareGenerated);
        Assert.Same(NoEnvironments, bareEnvironments);
        Assert.Null(bareCluster);
    }

    /// <summary>The words the topology says about itself: its type, then its four parts in their order, each named -
    /// the system as the system it is, saying its own nine words, the environment list as the collection it stands
    /// on, and an absent moment and an absent cluster named and left blank - with the Front Door answer last, which
    /// the record says of its own although no caller wrote it. (The whole line of the one that names nothing stands
    /// here, so a part moved about in the record is a change the tests see; no line names a moment, because a
    /// moment is said in the reader's own culture - what it stands for is pinned above.)</summary>
    [Fact]
    public void TheTopologySaysItsPartsInTheirOrder()
    {
        Assert.Equal(
            "Topology { System = SystemInfo { Slug = demo, Name = Demo, Repository = , DeliveryUrl = , CostUrl = , "
                + "Dashboard = , DeploymentsUrl = , ContainersUrl = , WorkItemsUrl =  }, Generated = , "
                + "Environments = Dashboard.Health.EnvironmentInfo[], Cluster = , HasFrontDoor = False }",
            Bare.ToString());

        var spoken = new Topology(BareSystem, null, TwoEnvironments, Cluster);
        var line = spoken.ToString();

        Assert.StartsWith("Topology { System = SystemInfo {", line, StringComparison.Ordinal);
        Assert.Contains("Generated = , ", line, StringComparison.Ordinal);
        Assert.Contains("Environments = Dashboard.Health.EnvironmentInfo[]", line, StringComparison.Ordinal);
        Assert.Contains("Cluster = ClusterInfo { Name = aks-demo, StatusUrl = https://raw.example.net/org/demo-system/status/cluster.json, ", line, StringComparison.Ordinal);

        // The environments are said by the collection they stand on and never opened: a line that says nothing of
        // the environment inside it is what a reader of a topology sees.
        Assert.DoesNotContain("uat", line, StringComparison.Ordinal);
        Assert.EndsWith("HasFrontDoor = True }", line, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the seven parts of a system

    /// <summary>The constructor's seven parts, each back as the property of its own name - the order the reader of
    /// the file hands them over in - the four addresses coming back as the addresses they were handed and the
    /// dashboard as the very dashboard it was handed, never copied.</summary>
    [Fact]
    public void TheConstructorHandsEachSystemArgumentToThePropertyOfItsName()
    {
        var system = FullSystem;

        Assert.Equal("cmdemo1", system.Slug);
        Assert.Equal("Demo system", system.Name);
        Assert.Equal(Repository, system.Repository);
        Assert.Equal(Delivery, system.DeliveryUrl);
        Assert.Equal(Cost, system.CostUrl);
        Assert.Same(Dashboard, system.Dashboard);
        Assert.Equal(Deployments, system.DeploymentsUrl);
    }

    /// <summary>The five parts a caller may leave out stand absent when it does, and only then: the two it must name
    /// stand where it names them, and the record fills in no address, no name and no dashboard of its own - the
    /// "System" a system of no name is shown under is the reader's doing, not the record's
    /// (<see cref="TopologyParserTests" />).</summary>
    [Fact]
    public void TheSystemPartsTheCallerDoesNotNameStandAbsent()
    {
        var system = BareSystem;

        Assert.Equal("demo", system.Slug);
        Assert.Equal("Demo", system.Name);
        Assert.Null(system.Repository);
        Assert.Null(system.DeliveryUrl);
        Assert.Null(system.CostUrl);
        Assert.Null(system.Dashboard);
        Assert.Null(system.DeploymentsUrl);

        Assert.Null(new SystemInfo("demo", "Demo", Repository: null, DeliveryUrl: null, CostUrl: null, Dashboard: null, DeploymentsUrl: null).Repository);
    }

    /// <summary>The system keeps the words it is given and changes none of them: an empty slug is a slug, a name of
    /// no letters is a name, the spaces about a word stay about it, and a part the record says is there may still be
    /// handed as nothing - the reader of the file that trims what it says and refuses an address that is no address
    /// being <see cref="TopologyParser" />.</summary>
    [Fact]
    public void ASystemKeepsItsWordsAsItIsGivenThem()
    {
        var empty = new SystemInfo(string.Empty, string.Empty);
        var spaced = new SystemInfo("  cmdemo1  ", "  Demo system  ", Repository);
        var nothing = new SystemInfo(null!, null!);

        Assert.Equal(string.Empty, empty.Slug);
        Assert.Equal(string.Empty, empty.Name);
        Assert.Null(empty.Repository);

        Assert.Equal("  cmdemo1  ", spaced.Slug);
        Assert.Equal("  Demo system  ", spaced.Name);
        Assert.Equal(Repository, spaced.Repository);

        Assert.Null(nothing.Slug);
        Assert.Null(nothing.Name);
        Assert.Null(nothing.ContainersUrl);
        Assert.Null(nothing.WorkItemsUrl);
    }

    // ------------------------------------------------------------------ the two files beside the delivery facts

    /// <summary>Without delivery facts there is nothing to stand beside: both derived addresses are the nothing they
    /// are, and stay nothing however the other parts are named - the cost address, the deployments address and the
    /// repository are neighbours of the delivery address and say nothing of their own.</summary>
    [Fact]
    public void TheSystemSaysNeitherFileWhenItSaysNoDeliveryFacts()
    {
        Assert.Null(BareSystem.ContainersUrl);
        Assert.Null(BareSystem.WorkItemsUrl);

        var named = BareSystem with { Repository = Repository, CostUrl = Cost, DeploymentsUrl = Deployments, Dashboard = Dashboard };

        Assert.Null(named.ContainersUrl);
        Assert.Null(named.WorkItemsUrl);

        var taken = FullSystem with { DeliveryUrl = null };

        Assert.Null(taken.ContainersUrl);
        Assert.Null(taken.WorkItemsUrl);
        Assert.Equal(Cost, taken.CostUrl);
        Assert.Equal(Deployments, taken.DeploymentsUrl);
    }

    /// <summary>Both files are read beside the <c>delivery.json</c> the system names: the directory of the delivery
    /// address is kept, its own file name replaced, a directory that ends on a slash kept as a directory, a port
    /// carried over, and the query and the fragment of the address they are derived from dropped - they are two
    /// other files, not the delivery file asked a different way. A path whose last word is a directory with no
    /// slash after it is a file to an address, and so is replaced too.</summary>
    [Theory]
    [InlineData("https://raw.example.net/org/demo-system/status/delivery.json", "https://raw.example.net/org/demo-system/status/containers.json", "https://raw.example.net/org/demo-system/status/workitems.json")]
    [InlineData("https://raw.example.net/delivery.json", "https://raw.example.net/containers.json", "https://raw.example.net/workitems.json")]
    [InlineData("https://raw.example.net/org/status/", "https://raw.example.net/org/status/containers.json", "https://raw.example.net/org/status/workitems.json")]
    [InlineData("https://raw.example.net/org/status", "https://raw.example.net/org/containers.json", "https://raw.example.net/org/workitems.json")]
    [InlineData("https://raw.example.net:8443/a/b/delivery.json", "https://raw.example.net:8443/a/b/containers.json", "https://raw.example.net:8443/a/b/workitems.json")]
    [InlineData("https://raw.example.net/org/status/delivery.json?sv=2024-01-01&sig=abc", "https://raw.example.net/org/status/containers.json", "https://raw.example.net/org/status/workitems.json")]
    [InlineData("https://raw.example.net/org/status/delivery.json#readme", "https://raw.example.net/org/status/containers.json", "https://raw.example.net/org/status/workitems.json")]
    [InlineData("http://localhost:8080/delivery.json", "http://localhost:8080/containers.json", "http://localhost:8080/workitems.json")]
    public void TheTwoFilesStandBesideTheDeliveryFacts(string deliveryUrl, string containersUrl, string workItemsUrl)
    {
        var system = BareSystem with { DeliveryUrl = new Uri(deliveryUrl) };

        Assert.Equal(containersUrl, system.ContainersUrl!.AbsoluteUri);
        Assert.Equal(workItemsUrl, system.WorkItemsUrl!.AbsoluteUri);
        Assert.DoesNotContain("delivery", system.ContainersUrl.AbsoluteUri, StringComparison.Ordinal);
        Assert.DoesNotContain("delivery", system.WorkItemsUrl.AbsoluteUri, StringComparison.Ordinal);
    }

    /// <summary>The two derived addresses are two files and not one, each named by the very word of what it holds:
    /// the Resource Health readings as <c>containers.json</c> - a word the record keeps to itself, no constant
    /// spelling it - and the work items as <c>workitems.json</c>, which is <see cref="WorkItemsReport.FileName" />.
    /// Neither is the address it is derived from, and both stand in the directory of that address.</summary>
    [Fact]
    public void TheTwoDerivedFilesAreTwoNamesApartFromTheFileTheyStandBeside()
    {
        var system = FullSystem;

        Assert.Equal("containers.json", system.ContainersUrl!.Segments.Last());
        Assert.Equal("workitems.json", system.WorkItemsUrl!.Segments.Last());
        Assert.Equal(WorkItemsReport.FileName, system.WorkItemsUrl.Segments.Last());
        Assert.NotEqual(system.ContainersUrl, system.WorkItemsUrl);
        Assert.NotEqual(Delivery, system.ContainersUrl);
        Assert.NotEqual(Delivery, system.WorkItemsUrl);

        // Both stand in the directory of the delivery facts: the address up to its last slash is the one words.
        var directory = Delivery.AbsoluteUri[..Delivery.AbsoluteUri.LastIndexOf('/')];

        Assert.Equal(directory, system.ContainersUrl!.AbsoluteUri[..system.ContainersUrl.AbsoluteUri.LastIndexOf('/')]);
        Assert.Equal(directory, system.WorkItemsUrl!.AbsoluteUri[..system.WorkItemsUrl.AbsoluteUri.LastIndexOf('/')]);
    }

    /// <summary>Both are asked of the address the system holds as it stands, and are made anew each time they are
    /// asked: the answer of two calls is one address and not one object, and a copy that moves the delivery facts
    /// moves both files with it while the system it was copied from keeps its own.</summary>
    [Fact]
    public void TheTwoFilesAreMadeAnewOfTheAddressTheSystemHolds()
    {
        var system = FullSystem;

        Assert.Equal(system.ContainersUrl, system.ContainersUrl);
        Assert.NotSame(system.ContainersUrl, system.ContainersUrl);
        Assert.Equal(system.WorkItemsUrl, system.WorkItemsUrl);
        Assert.NotSame(system.WorkItemsUrl, system.WorkItemsUrl);

        var moved = system with { DeliveryUrl = new Uri("https://other.example.net/elsewhere/delivery.json") };

        Assert.Equal("https://other.example.net/elsewhere/containers.json", moved.ContainersUrl!.AbsoluteUri);
        Assert.Equal("https://other.example.net/elsewhere/workitems.json", moved.WorkItemsUrl!.AbsoluteUri);
        Assert.Equal("https://raw.example.net/org/demo-system/status/containers.json", system.ContainersUrl!.AbsoluteUri);
    }

    // ------------------------------------------------------------------ the system itself

    /// <summary>The two words the system is asked for - its slug and its name - and the five it may be told nothing
    /// of: two systems are one while their seven parts are, the operators following <c>Equals</c> and the hash code
    /// following them, and a system is one with nothing that is not a system.</summary>
    [Fact]
    public void TwoSystemsAreOneWhileTheirSevenPartsAre()
    {
        var one = FullSystem;
        var two = new SystemInfo("cmdemo1", "Demo system", Repository, Delivery, Cost, Dashboard, Deployments);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("cmdemo1"));
        Assert.False(one.Equals(BareSystem));
        Assert.False(one.Equals(new Topology(one, null, NoEnvironments)));
    }

    /// <summary>Any one of the seven parts tells two systems apart, and nothing is compared loosely: the empty word
    /// and the nothing that is not a word are two slugs, the absent dashboard and one of another path are two
    /// dashboards, and an address told in another spelling is another address only where an address is told apart
    /// at all - the words after a <c>#</c> being nothing a <see cref="Uri" /> weighs.</summary>
    [Fact]
    public void ASystemDiffersInAnyOneOfItsParts()
    {
        var system = FullSystem;

        Assert.NotEqual(system, system with { Slug = string.Empty });
        Assert.NotEqual(system, system with { Slug = "Cmdemo1" });
        Assert.NotEqual(system, system with { Name = null! });
        Assert.NotEqual(system, system with { Repository = null });
        Assert.NotEqual(system, system with { Repository = new Uri("https://github.example.net/org/demo-system/commits") });
        Assert.NotEqual(system, system with { CostUrl = null });
        Assert.NotEqual(system, system with { DeploymentsUrl = null });
        Assert.NotEqual(system, system with { Dashboard = null });
        Assert.NotEqual(system, system with { Dashboard = new DashboardInfo("dashboard", "/facts.json") });
        Assert.NotEqual(system, system with { DeliveryUrl = null });
        Assert.NotEqual(system, system with { DeliveryUrl = new Uri("https://raw.example.net/elsewhere/delivery.json") });

        // An address is compared as an address is: the same site written with a word after the # is the same address
        // to a Uri - the canonical form a Uri weighs leaves it out - and the two systems are one, the two derived
        // files being the two same files. A slash after the file's name is another path, and so another address.
        Assert.Equal(
            system,
            system with { DeliveryUrl = new Uri("https://raw.example.net/org/demo-system/status/delivery.json#readme") });
        Assert.NotEqual(
            system,
            system with { DeliveryUrl = new Uri("https://raw.example.net/org/demo-system/status/delivery.json/") });
    }

    /// <summary>The seven parts read out of the system in the order they stand in it - the two derived files are no
    /// part and are read out of no falling apart - and the absent ones reading out as the nothing they are.</summary>
    [Fact]
    public void TheSevenPartsReadOutOfTheSystemInTheOrderTheyStand()
    {
        var (slug, name, repository, deliveryUrl, costUrl, dashboard, deploymentsUrl) = FullSystem;

        Assert.Equal("cmdemo1", slug);
        Assert.Equal("Demo system", name);
        Assert.Equal(Repository, repository);
        Assert.Equal(Delivery, deliveryUrl);
        Assert.Equal(Cost, costUrl);
        Assert.Same(Dashboard, dashboard);
        Assert.Equal(Deployments, deploymentsUrl);

        var (bareSlug, bareName, bareRepository, bareDelivery, bareCost, bareDashboard, bareDeployments) = BareSystem;

        Assert.Equal("demo", bareSlug);
        Assert.Equal("Demo", bareName);
        Assert.Null(bareRepository);
        Assert.Null(bareDelivery);
        Assert.Null(bareCost);
        Assert.Null(bareDashboard);
        Assert.Null(bareDeployments);
    }

    /// <summary><c>with</c> names the part it changes and carries the other six: a copy that names nothing is one
    /// with its original and not the same object.</summary>
    [Fact]
    public void WithChangesOnlyTheSystemPartItNames()
    {
        var system = FullSystem;
        var copy = system with { };

        Assert.Equal(system, copy);
        Assert.NotSame(system, copy);

        var named = system with { Slug = "cmdemo2" };

        Assert.Equal("cmdemo2", named.Slug);
        Assert.Equal("Demo system", named.Name);
        Assert.Same(Dashboard, named.Dashboard);
        Assert.Equal(Delivery, named.DeliveryUrl);
        Assert.Equal("cmdemo1", system.Slug);
    }

    /// <summary>The words the system says about itself: its type, then its seven parts in their order and the two
    /// files it derives after them, each named and an absent part named and left blank - the dashboard saying its
    /// own two parts inside the line, and an address saying itself with the slash a site's root is written with.
    /// (The whole line stands here, so a part moved about in the record is a change the tests see.)</summary>
    [Fact]
    public void TheSystemSaysItsPartsAndItsTwoDerivedFiles()
    {
        Assert.Equal(
            "SystemInfo { Slug = cmdemo1, Name = Demo system, Repository = https://github.example.net/org/demo-system, "
                + "DeliveryUrl = https://raw.example.net/org/demo-system/status/delivery.json, "
                + "CostUrl = https://raw.example.net/org/demo-system/status/cost.json, "
                + "Dashboard = DashboardInfo { Name = dashboard, BuildPath = /build-facts.json }, "
                + "DeploymentsUrl = https://raw.example.net/org/demo-system/deployments/deployments.json, "
                + "ContainersUrl = https://raw.example.net/org/demo-system/status/containers.json, "
                + "WorkItemsUrl = https://raw.example.net/org/demo-system/status/workitems.json }",
            FullSystem.ToString());

        Assert.Equal(
            "SystemInfo { Slug = demo, Name = Demo, Repository = , DeliveryUrl = , CostUrl = , Dashboard = , "
                + "DeploymentsUrl = , ContainersUrl = , WorkItemsUrl =  }",
            BareSystem.ToString());
    }

    /// <summary>The system's whole public surface as it stands: its seven parts, named in the order the constructor
    /// names them and of the type the constructor says, the two files it derives, and the members every record is
    /// asked - falling apart, comparing, hashing and saying itself - and nothing else to call. An eighth part added
    /// here has to be written down here too, which is the point.</summary>
    [Fact]
    public void TheSystemHasSevenPartsAndTheTwoFilesItDerives()
    {
        Assert.Equal(
            [
                "Slug", "Name", "Repository", "DeliveryUrl", "CostUrl", "Dashboard", "DeploymentsUrl",
                "ContainersUrl", "WorkItemsUrl",
            ],
            NamesOf<SystemInfo>());

        Assert.Equal(
            [
                "ContainersUrl", "CostUrl", "Dashboard", "Deconstruct", "DeliveryUrl", "DeploymentsUrl", "Equals",
                "GetHashCode", "Name", "Repository", "Slug", "ToString", "WorkItemsUrl", "op_Equality",
                "op_Inequality",
            ],
            PublicSurfaceOf<SystemInfo>());

        var properties = PropertiesOf<SystemInfo>();

        Assert.Equal(typeof(string), properties["Slug"].PropertyType);
        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Repository"].PropertyType);
        Assert.Equal(typeof(Uri), properties["DeliveryUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["CostUrl"].PropertyType);
        Assert.Equal(typeof(DashboardInfo), properties["Dashboard"].PropertyType);
        Assert.Equal(typeof(Uri), properties["DeploymentsUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["ContainersUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["WorkItemsUrl"].PropertyType);

        var parameters = ParametersOf<SystemInfo>();

        Assert.Equal(
            ["Slug", "Name", "Repository", "DeliveryUrl", "CostUrl", "Dashboard", "DeploymentsUrl"],
            [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false, true, true, true, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Skip(2), parameter => Assert.Null(parameter.DefaultValue));
        Assert.Equal(
            [false, false, true, true, true, true, true],
            [.. parameters.Select(IsNullable)]);

        Assert.Equal([typeof(IEquatable<SystemInfo>)], typeof(SystemInfo).GetInterfaces());
        Assert.Empty(NamesOfFields<SystemInfo>());
    }

    // ------------------------------------------------------------------ the dashboard itself

    /// <summary>The one word the dashboard is named by when its part of the file names none: the very word the
    /// reader of the file writes into the record, spelled out here where a change of it is a change the tests see -
    /// a word of letters, of no capitals, and of no spaces about it.</summary>
    [Fact]
    public void TheDefaultDashboardNameIsTheWordDashboard()
    {
        Assert.Equal("dashboard", DashboardInfo.DefaultName);
        Assert.NotEqual(DashboardInfo.DefaultName, string.Empty);
        Assert.Equal(DashboardInfo.DefaultName, DashboardInfo.DefaultName.Trim());
        Assert.Equal(DashboardInfo.DefaultName, DashboardInfo.DefaultName.ToLowerInvariant());
        Assert.DoesNotContain(" ", DashboardInfo.DefaultName, StringComparison.Ordinal);
    }

    /// <summary>The two parts of the dashboard come back as they are given, empty words and all: the record trims
    /// nothing, gives a path its leading slash and fills in no name of its own - all three are the reader of the
    /// file's (<see cref="TopologyParserTests" />, which hands a dashboard that names no name the
    /// <see cref="DashboardInfo.DefaultName" />).</summary>
    [Fact]
    public void TheDashboardKeepsItsTwoWordsAsItIsGivenThem()
    {
        var dashboard = new DashboardInfo("dashboard", "/build-facts.json");
        var empty = new DashboardInfo(string.Empty, string.Empty);
        var spaced = new DashboardInfo("  Dashboard  ", " build-facts.json ");
        var nothing = new DashboardInfo(null!, null!);

        Assert.Equal("dashboard", dashboard.Name);
        Assert.Equal("/build-facts.json", dashboard.BuildPath);

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.BuildPath);

        Assert.Equal("  Dashboard  ", spaced.Name);
        Assert.Equal(" build-facts.json ", spaced.BuildPath);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.BuildPath);

        Assert.Equal(DashboardInfo.DefaultName, new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json").Name);
    }

    /// <summary>Two dashboards are one while their two parts are, and differ in either; they fall apart into the two
    /// words they are made of, copy one word at a time, and say themselves with the two parts in their order.</summary>
    [Fact]
    public void TheDashboardIsARecordOfItsTwoParts()
    {
        var one = Dashboard;
        var two = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(new SystemInfo(DashboardInfo.DefaultName, "dashboard")));

        Assert.NotEqual(one, one with { Name = "Dashboard" });
        Assert.NotEqual(one, one with { Name = string.Empty });
        Assert.NotEqual(one, one with { BuildPath = "build-facts.json" });
        Assert.NotEqual(one, one with { BuildPath = null! });

        var (name, buildPath) = one;
        var renamed = one with { Name = "ui" };

        Assert.Equal("dashboard", name);
        Assert.Equal("/build-facts.json", buildPath);
        Assert.Equal("ui", renamed.Name);
        Assert.Equal("/build-facts.json", renamed.BuildPath);
        Assert.Equal("dashboard", one.Name);

        Assert.Equal("DashboardInfo { Name = dashboard, BuildPath = /build-facts.json }", one.ToString());
        Assert.Equal("DashboardInfo { Name = , BuildPath =  }", new DashboardInfo(string.Empty, string.Empty).ToString());
    }

    /// <summary>The dashboard's whole public surface: its two parts and the one fixed word it carries, the members
    /// every record is asked, and nothing else to call - the page reads its own build facts from the path it is
    /// given and decides nothing of its own.</summary>
    [Fact]
    public void TheDashboardHasTwoPartsAndOneFixedName()
    {
        Assert.Equal(["Name", "BuildPath"], NamesOf<DashboardInfo>());
        Assert.Equal(["BuildPath", "Deconstruct", "Equals", "GetHashCode", "Name", "ToString", "op_Equality", "op_Inequality"], PublicSurfaceOf<DashboardInfo>());

        var properties = PropertiesOf<DashboardInfo>();

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(string), properties["BuildPath"].PropertyType);

        var parameters = ParametersOf<DashboardInfo>();

        Assert.Equal(["Name", "BuildPath"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters, parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.Equal([false, false], [.. parameters.Select(IsNullable)]);

        Assert.Equal([typeof(IEquatable<DashboardInfo>)], typeof(DashboardInfo).GetInterfaces());
        Assert.Equal(["DefaultName"], NamesOfFields<DashboardInfo>());
        Assert.Equal("dashboard", typeof(DashboardInfo).GetField("DefaultName")!.GetRawConstantValue());
    }

    // ------------------------------------------------------------------ the eight parts of an environment

    /// <summary>The constructor's eight parts, each back as the property of its own name - the order the reader of
    /// the file hands an environment over in. The two lists and the link set come back as the very holds they were
    /// handed, never copied.</summary>
    [Fact]
    public void TheConstructorHandsEachEnvironmentArgumentToThePropertyOfItsName()
    {
        var environment = FullEnvironment;

        Assert.Equal("uat", environment.Name);
        Assert.Equal("nonprod", environment.Tier);
        Assert.Same(TwoDeployables, environment.Deployables);
        Assert.Equal(Versions, environment.VersionsUrl);
        Assert.Equal(VersionsHistory, environment.VersionsHistoryUrl);
        Assert.Same(Links, environment.Links);
        Assert.Equal("demo-uat", environment.Namespace);
        Assert.Same(TwoPages, environment.Pages);
    }

    /// <summary>The six parts a caller may leave out stand absent when it does: no tier, no version address, no
    /// links, no namespace and no page - and the deployables it must name stand where it names them, an empty list
    /// of them being an environment the page has nothing to check.</summary>
    [Fact]
    public void TheEnvironmentPartsTheCallerDoesNotNameStandAbsent()
    {
        var environment = BareEnvironment;

        Assert.Equal("tdd", environment.Name);
        Assert.Null(environment.Tier);
        Assert.Empty(environment.Deployables);
        Assert.Null(environment.VersionsUrl);
        Assert.Null(environment.VersionsHistoryUrl);
        Assert.Null(environment.Links);
        Assert.Null(environment.Namespace);
        Assert.Null(environment.Pages);

        Assert.Null(new EnvironmentInfo("tdd", null, NoDeployables, Links: null, Namespace: null, Pages: null).Links);
    }

    /// <summary>The environment keeps the words it is given and changes none of them: an empty tier is a tier and
    /// not the nothing a tier of no word is, a namespace of spaces is a namespace, a name of no letters is still a
    /// name, and no part is trimmed, lower-cased or filled in - the reader of the file does all of that
    /// (<see cref="TopologyParserTests" />).</summary>
    [Fact]
    public void AnEnvironmentKeepsItsWordsAsItIsGivenThem()
    {
        var empty = new EnvironmentInfo(string.Empty, string.Empty, NoDeployables, Links: null, Namespace: string.Empty, Pages: NoPages);
        var spaced = new EnvironmentInfo("  uat  ", "  nonprod  ", NoDeployables, Namespace: "  demo-uat  ");
        var nothing = new EnvironmentInfo(null!, null, null!);

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.Tier);
        Assert.Equal(string.Empty, empty.Namespace);
        Assert.Same(NoPages, empty.Pages);
        Assert.Empty(empty.Pages!);

        Assert.Equal("  uat  ", spaced.Name);
        Assert.Equal("  nonprod  ", spaced.Tier);
        Assert.Equal("  demo-uat  ", spaced.Namespace);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Tier);
        Assert.Null(nothing.Deployables);
        Assert.Null(nothing.Pages);
    }

    /// <summary>The empty list and no list at all are two different things, twice over: an environment of no
    /// deployable still holds the list it was given, an environment that names no page holds nothing, and a second
    /// list of the two very pages is another list to a record that opens neither.</summary>
    [Fact]
    public void AnEmptyListOfAnEnvironmentOwnIsNotTheListThatIsAbsent()
    {
        var emptied = FullEnvironment with { Deployables = NoDeployables, Pages = NoPages };
        var unnamed = FullEnvironment with { Pages = null };

        Assert.Empty(emptied.Deployables);
        Assert.Empty(emptied.Pages!);
        Assert.Same(NoDeployables, emptied.Deployables);
        Assert.Same(NoPages, emptied.Pages);

        Assert.Null(unnamed.Pages);
        Assert.NotEqual(emptied, unnamed);

        var copied = FullEnvironment with { Pages = [Scorecard, Reports] };

        Assert.Equal(FullEnvironment.Pages, copied.Pages);
        Assert.NotSame(FullEnvironment.Pages, copied.Pages);
        Assert.NotEqual(FullEnvironment, copied);
    }

    /// <summary>Two environments are one while their eight parts are - the operators following <c>Equals</c> and the
    /// hash code following them - and any one of the eight tells two apart: the empty tier and the absent one, the
    /// absent link set and the empty one, the empty page list and the absent one, a second list of the two very
    /// deployables, and a version address told in another spelling.</summary>
    [Fact]
    public void TwoEnvironmentsAreOneWhileTheirEightPartsAre()
    {
        var one = FullEnvironment;
        var two = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("uat"));
        Assert.False(one.Equals(BareEnvironment));
        Assert.False(one.Equals(AtDoor));

        Assert.NotEqual(one, one with { Name = string.Empty });
        Assert.NotEqual(one, one with { Name = "UAT" });
        Assert.NotEqual(one, one with { Tier = null });
        Assert.NotEqual(one, one with { Tier = string.Empty });
        Assert.NotEqual(one, one with { Deployables = NoDeployables });
        Assert.NotEqual(one, one with { Deployables = [.. TwoDeployables] });
        Assert.NotEqual(one, one with { VersionsUrl = null });
        Assert.NotEqual(one, one with { VersionsUrl = new Uri("https://raw.example.net/org/demo-system/main/environments/tdd/versions.json") });
        Assert.NotEqual(one, one with { VersionsHistoryUrl = null });
        Assert.NotEqual(one, one with { Links = null });
        Assert.NotEqual(one, one with { Links = LinkSet.None });
        Assert.NotEqual(one, one with { Links = OtherLinks });
        Assert.NotEqual(one, one with { Namespace = string.Empty });
        Assert.NotEqual(one, one with { Pages = null });
        Assert.NotEqual(one, one with { Pages = NoPages });
    }

    /// <summary>The links an environment holds are compared as the hold they stand on and not as their keys: the
    /// very same link set makes two environments one, and a second set of the one and the same link makes two
    /// environments of them - the record opening neither to look inside it.</summary>
    [Fact]
    public void TheLinksAnEnvironmentHoldsAreComparedAsTheHoldTheyStandOn()
    {
        var one = FullEnvironment;
        var holdingTheSameLinks = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);
        var holdingOtherLinks = one with { Links = OtherLinks };

        Assert.Equal(one, holdingTheSameLinks);
        Assert.Equal(one.GetHashCode(), holdingTheSameLinks.GetHashCode());

        Assert.Equal(one.Links!.Keys, holdingOtherLinks.Links!.Keys);
        Assert.Equal(one.Links[LinkSet.Portal], holdingOtherLinks.Links[LinkSet.Portal]);
        Assert.NotSame(one.Links, holdingOtherLinks.Links);
        Assert.True(one != holdingOtherLinks);
    }

    /// <summary>The eight parts read out of the environment in the order they stand in it, each as the value it
    /// holds, and the absent ones reading out as the nothing they are.</summary>
    [Fact]
    public void TheEightPartsReadOutOfTheEnvironmentInTheOrderTheyStand()
    {
        var (name, tier, deployables, versionsUrl, versionsHistoryUrl, links, @namespace, pages) = FullEnvironment;

        Assert.Equal("uat", name);
        Assert.Equal("nonprod", tier);
        Assert.Same(TwoDeployables, deployables);
        Assert.Equal(Versions, versionsUrl);
        Assert.Equal(VersionsHistory, versionsHistoryUrl);
        Assert.Same(Links, links);
        Assert.Equal("demo-uat", @namespace);
        Assert.Same(TwoPages, pages);

        var (bareName, bareTier, bareDeployables, bareVersions, bareHistory, bareLinks, bareNamespace, barePages) = BareEnvironment;

        Assert.Equal("tdd", bareName);
        Assert.Null(bareTier);
        Assert.Same(NoDeployables, bareDeployables);
        Assert.Null(bareVersions);
        Assert.Null(bareHistory);
        Assert.Null(bareLinks);
        Assert.Null(bareNamespace);
        Assert.Null(barePages);
    }

    /// <summary><c>with</c> names the part it changes and carries the other seven: a copy that names nothing is one
    /// with its original and not the same object, and the environment it was copied from is untouched.</summary>
    [Fact]
    public void WithChangesOnlyTheEnvironmentPartItNames()
    {
        var environment = FullEnvironment;
        var copy = environment with { };

        Assert.Equal(environment, copy);
        Assert.NotSame(environment, copy);

        var moved = environment with { Namespace = "demo-prod", Tier = "prod" };

        Assert.Equal("demo-prod", moved.Namespace);
        Assert.Equal("prod", moved.Tier);
        Assert.Equal("uat", moved.Name);
        Assert.Same(TwoDeployables, moved.Deployables);
        Assert.Same(TwoPages, moved.Pages);
        Assert.Equal("demo-uat", environment.Namespace);
        Assert.Equal("nonprod", environment.Tier);
    }

    /// <summary>The words the environment says about itself: its type, then its eight parts in their order, each
    /// named and an absent part named and left blank - a list saying the collection it stands on, a link set saying
    /// the type it is, and neither of them opened. (The whole line stands here, so a part moved about in the record
    /// is a change the tests see.)</summary>
    [Fact]
    public void TheEnvironmentSaysItsPartsInTheirOrder()
    {
        Assert.Equal(
            "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], "
                + "VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, "
                + "VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, "
                + "Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }",
            FullEnvironment.ToString());

        Assert.Equal(
            "EnvironmentInfo { Name = tdd, Tier = , Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = , "
                + "VersionsHistoryUrl = , Links = , Namespace = , Pages =  }",
            BareEnvironment.ToString());
    }

    /// <summary>Each of the six parts a caller may leave out, named as nothing one at a time while the other seven
    /// are named: the line keeps its eight names, in the order Name, Tier, Deployables, VersionsUrl,
    /// VersionsHistoryUrl, Links, Namespace, Pages, and the part that is nothing is said as an empty value - the
    /// word before the <c>=</c> is still there, the word after it is gone. The words before the changed part and
    /// the words after it are untouched, and the count of <c>=</c> is eight in every line. (The concrete list type
    /// spelling is left to the framework; the part names and their order are what the environment says.)</summary>
    [Theory]
    [InlineData("Tier", "EnvironmentInfo { Name = uat, Tier = , Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("VersionsUrl", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = , VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("VersionsHistoryUrl", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = , Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("Links", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = , Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("Namespace", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = , Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("Pages", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages =  }")]
    public void TheEnvironmentSaysEveryPartNameEvenTheOnesItIsNotTold(string partName, string expected)
    {
        var environment = partName switch
        {
            "Tier" => new EnvironmentInfo("uat", null, TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages),
            "VersionsUrl" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, null, VersionsHistory, Links, "demo-uat", TwoPages),
            "VersionsHistoryUrl" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, null, Links, "demo-uat", TwoPages),
            "Links" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, null, "demo-uat", TwoPages),
            "Namespace" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, null, TwoPages),
            "Pages" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", null),
            _ => throw new ArgumentOutOfRangeException(nameof(partName)),
        };

        var line = environment.ToString();

        // The full line is pinned: every part name is said, in order, with the absent part as an empty value.
        Assert.Equal(expected, line);

        // The count of the words that name a part is eight, even when one part is nothing.
        var occurrences = CountOccurrences(line, " = ");
        Assert.Equal(8, occurrences);
    }

    /// <summary>The empty word and the nothing that is not a word are two different tiers to the record's
    /// equality, but the line they say about themselves is one: an environment whose tier is the empty word and an
    /// environment whose tier is nothing say exactly the same words, while <c>Equals</c> and <c>==</c> hold that
    /// they are not one. The words are no answer to whether a tier is there.</summary>
    [Fact]
    public void AnEnvironmentWithNoTierAndOneWithAnEmptyTierSayTheSameWords()
    {
        var absent = new EnvironmentInfo("uat", null, TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);
        var empty = new EnvironmentInfo("uat", string.Empty, TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);

        Assert.NotEqual(absent, empty);
        Assert.True(absent != empty);
        Assert.NotEqual(absent.GetHashCode(), empty.GetHashCode());

        Assert.Equal(absent.ToString(), empty.ToString());
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    /// <summary>The environment's whole public surface as it stands: its eight parts, named in the order the
    /// constructor names them and of the type the constructor says, and the members every record is asked - falling
    /// apart, comparing, hashing and saying itself - and nothing else to call: an environment decides nothing of its
    /// own. A ninth part added here has to be written down here too, which is the point.</summary>
    [Fact]
    public void TheEnvironmentHasEightPartsAndDecidesNothingOfItsOwn()
    {
        Assert.Equal(
            ["Name", "Tier", "Deployables", "VersionsUrl", "VersionsHistoryUrl", "Links", "Namespace", "Pages"],
            NamesOf<EnvironmentInfo>());

        Assert.Equal(
            [
                "Deconstruct", "Deployables", "Equals", "GetHashCode", "Links", "Name", "Namespace", "Pages",
                "Tier", "ToString", "VersionsHistoryUrl", "VersionsUrl", "op_Equality", "op_Inequality",
            ],
            PublicSurfaceOf<EnvironmentInfo>());

        var properties = PropertiesOf<EnvironmentInfo>();

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(string), properties["Tier"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<DeployableInfo>), properties["Deployables"].PropertyType);
        Assert.Equal(typeof(Uri), properties["VersionsUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["VersionsHistoryUrl"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);
        Assert.Equal(typeof(string), properties["Namespace"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<PageLink>), properties["Pages"].PropertyType);

        var parameters = ParametersOf<EnvironmentInfo>();

        Assert.Equal(
            ["Name", "Tier", "Deployables", "VersionsUrl", "VersionsHistoryUrl", "Links", "Namespace", "Pages"],
            [.. parameters.Select(parameter => parameter.Name!)]);
        // The name and the deployables are what an environment must be told - the tier too, although it may be
        // named as nothing - and the five after them may be left out, each standing as nothing when it is.
        Assert.Equal([false, false, false, true, true, true, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.All(parameters.Skip(3), parameter => Assert.Null(parameter.DefaultValue));
        Assert.Equal(
            [false, true, false, true, true, true, true, true],
            [.. parameters.Select(IsNullable)]);

        Assert.Equal([typeof(IEquatable<EnvironmentInfo>)], typeof(EnvironmentInfo).GetInterfaces());
        Assert.Empty(NamesOfFields<EnvironmentInfo>());
    }

    // ------------------------------------------------------------------ a page of the system itself

    /// <summary>The tooltip a page's link is given: its own words and then the words that say a new tab opens -
    /// the suffix <see cref="LinkText" /> spells, and no word between them. The page that leads to nothing but
    /// itself still says it opens a new tab: the words are the whole of the tooltip and nothing else is added to
    /// them, whatever the address is or whether the deployable is named.</summary>
    [Theory]
    [InlineData("Scorecard", "Scorecard (opens in a new tab)")]
    [InlineData("", " (opens in a new tab)")]
    [InlineData("  Scorecard  ", "  Scorecard   (opens in a new tab)")]
    [InlineData("reports", "reports (opens in a new tab)")]
    [InlineData("Maps \u2013 EU", "Maps \u2013 EU (opens in a new tab)")]
    public void TheTitleOfAPageIsItsWordsAndTheNewTabWords(string name, string title)
    {
        var page = new PageLink(name, PageAddress);

        Assert.Equal(title, page.Title);
        Assert.Equal(page.Title, page.Title);
        Assert.EndsWith(LinkText.GitHubSuffix, page.Title, StringComparison.Ordinal);
        Assert.Equal(name + LinkText.GitHubSuffix, page.Title);
    }

    /// <summary>The tooltip is made of the name as it stands and of nothing else: a page whose name was never given
    /// says the new-tab words alone - the nothing a name is handed as being said as no word rather than as the word
    /// "null" - and the address and the deployable the page belongs to move the answer no more than the words of
    /// another part do, the page saying the same thing twice.</summary>
    [Fact]
    public void TheTitleOfAPageSaysNothingAboutItsOtherParts()
    {
        var nothing = new PageLink(null!, PageAddress);

        Assert.Equal(" (opens in a new tab)", nothing.Title);

        foreach (var page in new[] { Scorecard, Scorecard with { Deployable = null }, Scorecard with { Url = ReportsAddress } })
        {
            Assert.Equal("Scorecard (opens in a new tab)", page.Title);
            Assert.Equal(page.Title, page.Title);
        }
    }

    /// <summary>The three parts of a page come back as they are given, the one a caller may leave out stands
    /// absent, and an empty word is a word: a page of no name is a page with an empty name, and a page will hold no
    /// name and no address at all - the reader of the file that leaves such an entry out being
    /// <see cref="TopologyParser" /> (<see cref="TopologyParserTests" />).</summary>
    [Fact]
    public void APageKeepsItsPartsAsItIsGivenThem()
    {
        var named = new PageLink("Reports", ReportsAddress, "reports");

        Assert.Equal("Reports", named.Name);
        Assert.Same(ReportsAddress, named.Url);
        Assert.Equal("reports", named.Deployable);

        Assert.Null(Reports.Deployable);
        Assert.Null(new PageLink("Reports", ReportsAddress, Deployable: null).Deployable);
        Assert.Equal(string.Empty, new PageLink(string.Empty, ReportsAddress, string.Empty).Name);
        Assert.Equal(string.Empty, new PageLink(string.Empty, ReportsAddress, string.Empty).Deployable);

        var nothing = new PageLink(null!, null!);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Url);
        Assert.Null(nothing.Deployable);
    }

    /// <summary>Two pages are one while their three parts are, and any one of the three tells them apart - the
    /// deployable a page belongs to, the empty word and the nothing that is not a word, and an address told in
    /// another spelling all three making another page. It falls apart into its three words, copies one of them at a
    /// time and says itself with the tooltip last, which no caller wrote.</summary>
    [Fact]
    public void ThePageIsARecordOfItsThreeParts()
    {
        var one = Scorecard;
        var two = new PageLink("Scorecard", PageAddress, "scorecard");

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.True(one.Equals((object)two));
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("Scorecard"));
        Assert.False(one.Equals(Links));

        Assert.NotEqual(one, one with { Name = string.Empty });
        Assert.NotEqual(one, one with { Name = "scorecard" });
        Assert.NotEqual(one, one with { Deployable = null });
        Assert.NotEqual(one, one with { Deployable = string.Empty });
        Assert.NotEqual(one, one with { Url = ReportsAddress });
        Assert.NotEqual(one, one with { Url = new Uri("https://ca-demo-uat-scorecard.example.net/scorecard/") });

        var (name, url, deployable) = one;
        var moved = one with { Url = ReportsAddress };

        Assert.Equal("Scorecard", name);
        Assert.Same(PageAddress, url);
        Assert.Equal("scorecard", deployable);
        Assert.Same(ReportsAddress, moved.Url);
        Assert.Same(PageAddress, one.Url);

        Assert.Equal(
            "PageLink { Name = Scorecard, Url = https://ca-demo-uat-scorecard.example.net/scorecard, "
                + "Deployable = scorecard, Title = Scorecard (opens in a new tab) }",
            one.ToString());
        Assert.Equal(
            "PageLink { Name = Reports, Url = https://reports.example.net/, Deployable = , "
                + "Title = Reports (opens in a new tab) }",
            Reports.ToString());
    }

    /// <summary>The page's whole public surface: its three parts and the tooltip it makes of one of them, and the
    /// members every record is asked - and nothing else to call.</summary>
    [Fact]
    public void ThePageHasThreePartsAndTheOneTitleItMakes()
    {
        Assert.Equal(["Name", "Url", "Deployable", "Title"], NamesOf<PageLink>());
        Assert.Equal(
            ["Deconstruct", "Deployable", "Equals", "GetHashCode", "Name", "Title", "ToString", "Url", "op_Equality", "op_Inequality"],
            PublicSurfaceOf<PageLink>());

        var properties = PropertiesOf<PageLink>();

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Url"].PropertyType);
        Assert.Equal(typeof(string), properties["Deployable"].PropertyType);
        Assert.Equal(typeof(string), properties["Title"].PropertyType);

        var parameters = ParametersOf<PageLink>();

        Assert.Equal(["Name", "Url", "Deployable"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.Equal(DBNull.Value, parameters[0].DefaultValue);
        Assert.Equal(DBNull.Value, parameters[1].DefaultValue);
        Assert.Null(parameters[2].DefaultValue);
        Assert.Equal([false, false, true], [.. parameters.Select(IsNullable)]);

        Assert.Equal([typeof(IEquatable<PageLink>)], typeof(PageLink).GetInterfaces());
        Assert.Empty(NamesOfFields<PageLink>());
    }

    // ------------------------------------------------------------------ the cluster a system may name

    /// <summary>The cluster's four parts, each back as the property of its own name: it is the whole of the cluster
    /// view's addresses, and every one of them may be named as nothing - a cluster the file says no name of is a
    /// cluster with no name, and not the nothing a system that runs in no cluster holds.</summary>
    [Fact]
    public void TheConstructorHandsEachClusterArgumentToThePropertyOfItsName()
    {
        var cluster = Cluster;

        Assert.Equal("aks-demo", cluster.Name);
        Assert.Equal(StatusUrl, cluster.StatusUrl);
        Assert.Equal(ServiceUrl, cluster.ServiceUrl);
        Assert.Same(Links, cluster.Links);
    }

    /// <summary>The three parts a caller may leave out stand absent when it does, and only then: the name it must
    /// name stands where it names it, even as the empty word or as the nothing the record says is there, and two
    /// addresses that name no file are two absences rather than a file of no name.</summary>
    [Fact]
    public void TheClusterPartsTheCallerDoesNotNameStandAbsent()
    {
        var alone = new ClusterInfo("aks-demo");

        Assert.Equal("aks-demo", alone.Name);
        Assert.Null(alone.StatusUrl);
        Assert.Null(alone.ServiceUrl);
        Assert.Null(alone.Links);

        Assert.Null(new ClusterInfo("aks-demo", null, null, null).StatusUrl);
        Assert.Null(new ClusterInfo(null).Name);
        Assert.Equal(string.Empty, new ClusterInfo(string.Empty).Name);
    }

    /// <summary>Two clusters are one while their four parts are, and any one of the four tells them apart - the
    /// empty name and the nothing that is not a name, the absent link set and the one with nothing in it, and an
    /// address the cluster does not name at all. It falls apart into the four, copies one of them at a time, and
    /// says itself with its four parts in their order.</summary>
    [Fact]
    public void TheClusterIsARecordOfItsFourParts()
    {
        var one = Cluster;
        var two = new ClusterInfo("aks-demo", StatusUrl, ServiceUrl, Links);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("aks-demo"));
        Assert.False(one.Equals(Bare with { Cluster = one }));

        Assert.NotEqual(one, one with { Name = null });
        Assert.NotEqual(one, one with { Name = string.Empty });
        Assert.NotEqual(one, one with { StatusUrl = null });
        Assert.NotEqual(one, one with { StatusUrl = ServiceUrl });
        Assert.NotEqual(one, one with { ServiceUrl = null });
        Assert.NotEqual(one, one with { Links = null });
        Assert.NotEqual(one, one with { Links = LinkSet.None });
        Assert.NotEqual(one, one with { Links = OtherLinks });

        var (name, statusUrl, serviceUrl, links) = one;
        var moved = one with { Name = "aks-other" };

        Assert.Equal("aks-demo", name);
        Assert.Equal(StatusUrl, statusUrl);
        Assert.Equal(ServiceUrl, serviceUrl);
        Assert.Same(Links, links);
        Assert.Equal("aks-other", moved.Name);
        Assert.Equal(StatusUrl, moved.StatusUrl);

        Assert.Equal(
            "ClusterInfo { Name = aks-demo, StatusUrl = https://raw.example.net/org/demo-system/status/cluster.json, "
                + "ServiceUrl = https://raw.example.net/org/demo-system/status/aks.json, Links = Dashboard.Health.LinkSet }",
            one.ToString());
        Assert.Equal("ClusterInfo { Name = , StatusUrl = , ServiceUrl = , Links =  }", new ClusterInfo(string.Empty).ToString());
    }

    /// <summary>The cluster's whole public surface: its four parts - the name it may be told nothing of and the two
    /// addresses and the links a caller may leave out - and the members every record is asked, and nothing else to
    /// call: which of the two files the cluster view reads is the monitor's question, not the record's
    /// (<see cref="ClusterMonitorEdgeCaseTests" />).</summary>
    [Fact]
    public void TheClusterHasFourPartsAndDecidesNothingOfItsOwn()
    {
        Assert.Equal(["Name", "StatusUrl", "ServiceUrl", "Links"], NamesOf<ClusterInfo>());
        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Links", "Name", "ServiceUrl", "StatusUrl", "ToString", "op_Equality", "op_Inequality"],
            PublicSurfaceOf<ClusterInfo>());

        var properties = PropertiesOf<ClusterInfo>();

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(Uri), properties["StatusUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["ServiceUrl"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);

        var parameters = ParametersOf<ClusterInfo>();

        Assert.Equal(["Name", "StatusUrl", "ServiceUrl", "Links"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, true, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Skip(1), parameter => Assert.Null(parameter.DefaultValue));
        Assert.Equal(DBNull.Value, parameters[0].DefaultValue);
        Assert.Equal([true, true, true, true], [.. parameters.Select(IsNullable)]);

        Assert.Equal([typeof(IEquatable<ClusterInfo>)], typeof(ClusterInfo).GetInterfaces());
        Assert.Empty(NamesOfFields<ClusterInfo>());
    }

    // ------------------------------------------------------------------ the topology's own shape

    /// <summary>The topology's whole public surface as it stands: its four parts, named in the order the constructor
    /// names them and of the type the constructor says, the one decision it makes of them
    /// (<see cref="Topology.HasFrontDoor" />), and the members every record is asked - falling apart, comparing,
    /// hashing and saying itself - and nothing else to call. A fifth part added here has to be written down here
    /// too, which is the point: the page cannot be shown a part nobody decided what it means.</summary>
    [Fact]
    public void TheTopologyHasFourPartsAndTheOneDecisionItMakes()
    {
        Assert.Equal(["System", "Generated", "Environments", "Cluster", "HasFrontDoor"], NamesOf<Topology>());

        Assert.Equal(
            [
                "Cluster", "Deconstruct", "Environments", "Equals", "Generated", "GetHashCode", "HasFrontDoor",
                "System", "ToString", "op_Equality", "op_Inequality",
            ],
            PublicSurfaceOf<Topology>());

        var properties = PropertiesOf<Topology>();

        Assert.Equal(typeof(SystemInfo), properties["System"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["Generated"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<EnvironmentInfo>), properties["Environments"].PropertyType);
        Assert.Equal(typeof(ClusterInfo), properties["Cluster"].PropertyType);
        Assert.Equal(typeof(bool), properties["HasFrontDoor"].PropertyType);

        // The one constructor, naming its four parts as the properties are named: only the cluster may be left out,
        // the system and its environments being what a topology must be told, and the moment is a part a caller
        // must name although it may name it as nothing.
        var parameters = ParametersOf<Topology>();

        Assert.Equal(["System", "Generated", "Environments", "Cluster"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false, false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.Null(parameters[3].DefaultValue);
        Assert.Equal([false, true, false, true], [.. parameters.Select(IsNullable)]);

        Assert.Equal([typeof(IEquatable<Topology>)], typeof(Topology).GetInterfaces());
        Assert.Empty(NamesOfFields<Topology>());
    }

    // ------------------------------------------------------------------ the machinery

    /// <summary>The public properties a record declares itself, in the order they stand in it: its parts as the
    /// constructor names them, and the decisions it makes of them after.</summary>
    private static string[] NamesOf<T>() =>
        [.. typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)];

    /// <summary>The public members a record declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by each record's own test rather than by this list.</summary>
    private static string[] PublicSurfaceOf<T>() =>
        [.. typeof(T)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The public properties of a record, by name, so that a test can ask one of them what type it
    /// says it is.</summary>
    private static Dictionary<string, PropertyInfo> PropertiesOf<T>() =>
        NamesOf<T>()
            .ToDictionary(
                name => name,
                name => typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!,
                StringComparer.Ordinal);

    /// <summary>The parts a record's one constructor names, in the order it names them.</summary>
    private static ParameterInfo[] ParametersOf<T>() => typeof(T).GetConstructors().Single().GetParameters();

    /// <summary>True when the record says a part may be nothing: what the compiler writes into the signature, and
    /// what the reader of the file leans on.</summary>
    private static bool IsNullable(ParameterInfo parameter) =>
        new NullabilityInfoContext().Create(parameter).WriteState == NullabilityState.Nullable;

    /// <summary>The fixed words a record carries, by name: the two records that carry three paths and two role
    /// words have them pinned where they stand
    /// (<see cref="DeployableInfoEdgeCaseTests" /> and <see cref="NodeInfoEdgeCaseTests" />), and the rest of them
    /// carry nothing at all.</summary>
    private static string[] NamesOfFields<T>() =>
        [.. typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(field => field.Name)
            .Order(StringComparer.Ordinal)];
}
