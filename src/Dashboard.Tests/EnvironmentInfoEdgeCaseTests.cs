using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="EnvironmentInfo" /> declared in
/// <c>src/Dashboard/Health/Topology.cs</c>: one environment of the topology, the eight parts it is made of and the
/// nothing it decides of them.
/// <para>
/// What the files beside it already pin is left alone. <see cref="TopologyEdgeCaseTests" /> pins the rest of that
/// file as records - a <see cref="Topology" />, its <see cref="SystemInfo" />, a <see cref="DashboardInfo" />, a
/// <see cref="ClusterInfo" /> and a <see cref="PageLink" /> - and, beside them, an environment's eight parts as they
/// come back, the words it keeps, its equality, its <c>with</c>, the line of words it says and the surface it shows
/// in reflection. <see cref="TopologyParserTests" /> pins what the file becomes: an environment's name and tier, its
/// two version addresses read out of it and refused when they are no absolute http or https address, the pages it
/// names and the page left out for want of a name or an address, and the namespace a cluster is read with.
/// <see cref="LinkTests" /> pins the links and the pages the environment's part of the dashboard is rendered from,
/// <see cref="TargetStatusTests" /> and <see cref="TelemetryTests" /> the tiles and the readings of the deployables
/// it holds, <see cref="ClusterGroupsTests" /> and <see cref="ClusterEventDetectorTests" /> the namespace the cluster
/// view groups its pods by, and <see cref="PinnedVersionsMonitorTests" /> and
/// <see cref="KustomizationPinMonitorTests" /> the two addresses a running monitor reads the pinned versions from.
/// </para>
/// <para>
/// What is pinned here is the record on its own, which all of that walks past: that the eight parts come back to the
/// property of their own name, and that the five a caller leaves out stand absent rather than filled in; that the
/// empty word is a word and not nothing, that a list with nothing in it is not the list that is absent, and that an
/// environment refuses nothing it is given - a name, a tier, a list and an address may all be handed to it as
/// nothing; that its two addresses are two addresses, neither being the other, and that each is compared as an
/// address is - the host in upper case, the scheme's own port written out, a dot segment in the path and the
/// fragment after the <c>#</c> leaving it where it stood, and a trailing slash, a query after the path, another
/// scheme and another environment's directory moving it; that the members every record is asked -
/// comparing, hashing, falling apart into the eight, copying and saying itself - hold, with the two lists compared by
/// the instance they stand on and the link set by the hold it stands on; that every part is written only at the
/// making, and which of the eight the record itself says may be nothing; and the surface it shows in reflection, its
/// eight parts named in their order and of the type the constructor says, and nothing else to call.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every address is named and never fetched, and
/// the record decides nothing of its own.
/// </para>
/// </summary>
public class EnvironmentInfoEdgeCaseTests
{
    private static readonly Uri NodeAddress = new("https://ui.uat.example.net");
    private static readonly Uri PageAddress = new("https://ca-demo-uat-scorecard.example.net/scorecard");

    /// <summary>The two addresses of the demo system: where the file that holds the versions pinned in Git is read,
    /// and the page with the history of that same file. Two addresses of two different places, and the one file.</summary>
    private static readonly Uri Versions = new("https://raw.example.net/org/demo-system/main/environments/uat/versions.json");
    private static readonly Uri VersionsHistory = new("https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json");

    private static readonly LinkSet Links = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]);
    private static readonly LinkSet OtherLinks = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]);

    private static readonly NodeInfo West = new("ca-demo-uat-ui", "westus3", NodeInfo.PrimaryRole, NodeAddress);
    private static readonly IReadOnlyList<NodeInfo> OneNode = [West];

    /// <summary>The two deployables of the environment, each at its three default paths and with the one node of the
    /// environment behind it - the record holding no address of a deployable of its own.</summary>
    private static readonly DeployableInfo Ui = DeployableAt("ui");
    private static readonly DeployableInfo Api = DeployableAt("api");

    private static readonly IReadOnlyList<DeployableInfo> TwoDeployables = new[] { Ui, Api };
    private static readonly IReadOnlyList<DeployableInfo> OneDeployable = new[] { Ui };
    private static readonly IReadOnlyList<DeployableInfo> NoDeployables = [];

    private static readonly PageLink Scorecard = new("Scorecard", PageAddress, "scorecard");
    private static readonly IReadOnlyList<PageLink> OnePage = new[] { Scorecard };
    private static readonly IReadOnlyList<PageLink> NoPages = [];

    /// <summary>An environment with all eight parts named and every value apart from every other, so that an
    /// argument landing on another property is seen instead of quietly matching a second part of the record.</summary>
    private static readonly EnvironmentInfo FullEnvironment = new(
        "uat",
        "nonprod",
        TwoDeployables,
        Versions,
        VersionsHistory,
        Links,
        "demo-uat",
        OnePage);

    /// <summary>An environment named by the three parts a caller must name: a name, no tier and nothing to check in
    /// it - the five parts a caller may leave out all standing absent.</summary>
    private static readonly EnvironmentInfo BareEnvironment = new("tdd", null, NoDeployables);

    /// <summary>A deployable of the name named, at no public address, asked at the three paths every deployable of
    /// the dashboard is asked at, and answered by the environment's one node.</summary>
    private static DeployableInfo DeployableAt(string name) =>
        new(
            name,
            FrontDoor: null,
            HealthPath: DeployableInfo.DefaultHealthPath,
            AlivePath: DeployableInfo.DefaultAlivePath,
            VersionPath: DeployableInfo.DefaultVersionPath,
            Nodes: OneNode);

    // ------------------------------------------------------------------ the eight parts

    /// <summary>The constructor's eight parts, each back as the property of its own name - the order the reader of
    /// the file hands an environment over in. The two lists and the link set come back as the very holds they were
    /// handed, never copied, and the two addresses as the very <see cref="Uri" /> instances they were.</summary>
    [Fact]
    public void TheConstructorHandsEachOfTheEightPartsToThePropertyOfItsName()
    {
        var environment = FullEnvironment;

        Assert.Equal("uat", environment.Name);
        Assert.Equal("nonprod", environment.Tier);
        Assert.Same(TwoDeployables, environment.Deployables);
        Assert.Same(Versions, environment.VersionsUrl);
        Assert.Same(VersionsHistory, environment.VersionsHistoryUrl);
        Assert.Same(Links, environment.Links);
        Assert.Equal("demo-uat", environment.Namespace);
        Assert.Same(OnePage, environment.Pages);

        Assert.Equal(2, environment.Deployables.Count);
        Assert.Single(environment.Pages!);
    }

    /// <summary>The five parts a caller may leave out stand absent when it does: no tier, no version address, no
    /// history page, no links, no namespace and no page - and the deployables it must name stand where it names
    /// them, an empty list of them being an environment the page has nothing to check.</summary>
    [Fact]
    public void TheFivePartsTheCallerDoesNotNameStandAbsent()
    {
        var environment = BareEnvironment;

        Assert.Equal("tdd", environment.Name);
        Assert.Null(environment.Tier);
        Assert.Same(NoDeployables, environment.Deployables);
        Assert.Empty(environment.Deployables);
        Assert.Null(environment.VersionsUrl);
        Assert.Null(environment.VersionsHistoryUrl);
        Assert.Null(environment.Links);
        Assert.Null(environment.Namespace);
        Assert.Null(environment.Pages);

        // Naming the five and naming them as nothing is the same environment as not naming them at all.
        Assert.Equal(environment, new EnvironmentInfo("tdd", null, NoDeployables, null, null, null, null, null));
        Assert.Equal(
            environment,
            new EnvironmentInfo("tdd", null, NoDeployables, VersionsUrl: null, VersionsHistoryUrl: null, Links: null, Namespace: null, Pages: null));
    }

    /// <summary>The environment keeps the words it is given and changes none of them: the empty tier is a tier and
    /// not the nothing a tier of no word is, a namespace of spaces is a namespace, a name of no letters is still a
    /// name, and no part is trimmed, lower-cased or filled in - the reader of the file does all of that
    /// (<see cref="TopologyParserTests" />).</summary>
    [Fact]
    public void AnEnvironmentKeepsItsWordsAsItIsGivenThem()
    {
        var empty = new EnvironmentInfo(string.Empty, string.Empty, NoDeployables, Links: null, Namespace: string.Empty, Pages: NoPages);
        var spaced = new EnvironmentInfo("  uat  ", "  nonprod  ", NoDeployables, Namespace: "  demo-uat  ");
        var shortest = new EnvironmentInfo("u", "n", NoDeployables, Pages: NoPages);

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.Tier);
        Assert.Equal(string.Empty, empty.Namespace);
        Assert.Same(NoPages, empty.Pages);
        Assert.Empty(empty.Pages!);

        Assert.Equal("  uat  ", spaced.Name);
        Assert.Equal("  nonprod  ", spaced.Tier);
        Assert.Equal("  demo-uat  ", spaced.Namespace);

        // A word of one letter is weighed at its length and not guessed at.
        Assert.Equal("u", shortest.Name);
        Assert.Equal("n", shortest.Tier);
        Assert.NotEqual(shortest, FullEnvironment);
    }

    /// <summary>The record makes no decision of its own about what a part may be: an environment will hold no name,
    /// no deployable list, no address and no page at all, since it is the file that is read - and the reader that
    /// refuses an environment whose address is no absolute http or https address says so in its own words
    /// (<see cref="TopologyParserTests" />).</summary>
    [Fact]
    public void AnEnvironmentRefusesNothingItIsGiven()
    {
        var nothing = new EnvironmentInfo(null!, null, null!);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Tier);
        Assert.Null(nothing.Deployables);
        Assert.Null(nothing.VersionsUrl);
        Assert.Null(nothing.VersionsHistoryUrl);
        Assert.Null(nothing.Links);
        Assert.Null(nothing.Namespace);
        Assert.Null(nothing.Pages);

        // Only the parts that stand apart tell the two environments apart: fill in the parts that stand apart and the
        // environment of nothing at all is one with an environment whose words were blanked out.
        Assert.Equal(
            nothing with { Tier = "nonprod", Deployables = NoDeployables },
            new EnvironmentInfo(null!, "nonprod", NoDeployables));
    }

    /// <summary>The empty list and no list at all are two different things, twice over: an environment of no
    /// deployable still holds the list it was given, an environment that names no page holds nothing, and a second
    /// list of the very same deployables is another list to a record that opens neither.</summary>
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
        Assert.NotEqual(emptied.GetHashCode(), unnamed.GetHashCode());

        // The list of the one deployable is neither the list of the two nor the list with nothing in it.
        Assert.NotEqual(FullEnvironment, FullEnvironment with { Deployables = OneDeployable });
        Assert.NotEqual(FullEnvironment, FullEnvironment with { Deployables = NoDeployables });

        // A second list of the one page is another list, and so another environment, although the page in it is the
        // very page the environment holds.
        var copied = FullEnvironment with { Pages = [Scorecard] };

        Assert.Single(copied.Pages!);
        Assert.Same(Scorecard, copied.Pages![0]);
        Assert.NotSame(OnePage, copied.Pages);
        Assert.NotEqual(FullEnvironment, copied);
    }

    // ------------------------------------------------------------------ the two addresses

    /// <summary>The two addresses of an environment are two addresses of two places: the pin file is not its history
    /// page and the history page is not the pin file, neither by the address nor by the environment that names one
    /// in the other's place - although the two stand in the same environment, both read back as they were named.</summary>
    [Fact]
    public void TheTwoVersionAddressesAreTwoAddressesAndNeitherIsTheOther()
    {
        Assert.NotEqual(Versions, VersionsHistory);
        Assert.NotSame(FullEnvironment.VersionsUrl, FullEnvironment.VersionsHistoryUrl);

        Assert.Equal(Versions, FullEnvironment.VersionsUrl);
        Assert.Equal(VersionsHistory, FullEnvironment.VersionsHistoryUrl);

        // Swapping the two makes another environment, and so does naming one of them twice.
        Assert.NotEqual(FullEnvironment, FullEnvironment with { VersionsUrl = VersionsHistory, VersionsHistoryUrl = Versions });
        Assert.NotEqual(FullEnvironment, FullEnvironment with { VersionsHistoryUrl = Versions });
        Assert.NotEqual(FullEnvironment, FullEnvironment with { VersionsUrl = VersionsHistory });
    }

    /// <summary>An address is compared as an address is, the environment saying nothing of its own about it: the
    /// same address with its host in upper case, with the scheme's own port written out, with a dot segment in the
    /// path and with a fragment after the <c>#</c> is the same address and makes one environment of two - the
    /// fragment being read nowhere and kept everywhere. A trailing slash, a query after the path, another scheme
    /// and another environment's directory are four other addresses, and so four other environments. (That the
    /// file's address must be absolute http or https is <see cref="TopologyParserTests" />'s; nothing here fetches
    /// one.)</summary>
    [Theory]
    [InlineData("https://RAW.example.net/org/demo-system/main/environments/uat/versions.json", true)]
    [InlineData("https://raw.example.net:443/org/demo-system/main/environments/uat/versions.json", true)]
    [InlineData("https://raw.example.net/org/demo-system/main/environments/uat/a/../versions.json", true)]
    [InlineData("https://raw.example.net/org/demo-system/main/environments/uat/versions.json/", false)]
    [InlineData("https://raw.example.net/org/demo-system/main/environments/uat/versions.json?sig=abc", false)]
    [InlineData("https://raw.example.net/org/demo-system/main/environments/uat/versions.json#top", true)]
    [InlineData("http://raw.example.net/org/demo-system/main/environments/uat/versions.json", false)]
    [InlineData("https://raw.example.net/org/demo-system/main/environments/tdd/versions.json", false)]
    public void AVersionAddressIsComparedAsAnAddressIs(string versionsUrl, bool sameEnvironment) =>
        Assert.Equal(sameEnvironment, FullEnvironment with { VersionsUrl = new Uri(versionsUrl) } == FullEnvironment);

    /// <summary>An address the machinery cannot read is refused at the making: the <see cref="Uri" /> it is made
    /// from refuses the words that are no absolute address with its own <see cref="UriFormatException" />, and the
    /// environment says nothing of its own about the address it is handed - the record refuses nothing, so the
    /// only refusal the making gives is the address's own.</summary>
    [Fact]
    public void AnAddressTheMachineryCannotReadIsRefusedAtTheMaking()
    {
        Assert.Throws<UriFormatException>(() =>
            new EnvironmentInfo("uat", "nonprod", TwoDeployables, VersionsUrl: new Uri("not an address")));
    }

    /// <summary>The address a part of the environment is held as the very <see cref="Uri" /> instance it was handed,
    /// and the record neither normalises it nor reads anything out of it: the shape the address itself answers with
    /// stands, whatever the environment holds it in - and an environment that holds no address at all is not an
    /// environment that holds an address saying nothing.</summary>
    [Fact]
    public void TheAddressStandsAsItWasHandedAndTheNothingThatIsNoAddressStandsApart()
    {
        var written = new Uri("https://raw.example.net:443/org/demo-system/main/environments/uat/versions.json?sv=2026#pin");
        var holding = FullEnvironment with { VersionsUrl = written };

        Assert.Same(written, holding.VersionsUrl);
        Assert.Equal("https", holding.VersionsUrl!.Scheme);
        Assert.Equal("raw.example.net", holding.VersionsUrl.Host);
        Assert.Equal(443, holding.VersionsUrl.Port);
        Assert.Equal("/org/demo-system/main/environments/uat/versions.json", holding.VersionsUrl.AbsolutePath);
        Assert.Equal("?sv=2026", holding.VersionsUrl.Query);
        Assert.Equal("#pin", holding.VersionsUrl.Fragment);
        Assert.Equal("https://raw.example.net/org/demo-system/main/environments/uat/versions.json?sv=2026#pin", holding.VersionsUrl.AbsoluteUri);

        // The address that is not there and the address of no path at all are two different parts of an environment.
        Assert.NotEqual(FullEnvironment with { VersionsUrl = null }, FullEnvironment with { VersionsUrl = new Uri("https://raw.example.net") });
        Assert.Equal("https://raw.example.net/", new Uri("https://raw.example.net").AbsoluteUri);
    }

    /// <summary>The four nothings beside an address: the address that is absent, and the empty word that is no
    /// address at all - so the slot is pinned with the relative address instead, which the record holds as it was
    /// handed, no absolute and read back by its own <c>ToString</c>; two environments naming that same relative
    /// address are one, and the relative address and the absolute one that ends on the same file are two.</summary>
    [Fact]
    public void TheFourNothingsBesideAnAddress()
    {
        var absent = FullEnvironment with { VersionsUrl = null };

        Assert.Null(absent.VersionsUrl);

        // The empty word is no address at all, no environment holding it: the slot is pinned with the relative
        // address instead, and the record holds it as it was handed.
        Assert.Throws<UriFormatException>(() => new Uri(string.Empty));

        var relative = new Uri("versions.json", UriKind.Relative);
        var holding = FullEnvironment with { VersionsUrl = relative };

        Assert.Equal(relative, holding.VersionsUrl);
        Assert.False(holding.VersionsUrl!.IsAbsoluteUri);
        Assert.Equal(
            "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], "
                + "VersionsUrl = versions.json, "
                + "VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, "
                + "Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }",
            holding.ToString());

        // Two environments naming that same relative address are one, the second naming it by its own <see cref="Uri" />.
        var namedTheSame = new EnvironmentInfo(
            "uat",
            "nonprod",
            TwoDeployables,
            VersionsUrl: new Uri("versions.json", UriKind.Relative),
            VersionsHistoryUrl: VersionsHistory,
            Links: Links,
            Namespace: "demo-uat",
            Pages: OnePage);

        Assert.Equal(holding, namedTheSame);

        // The relative address and the absolute one that ends on the same file are two environments.
        Assert.NotEqual(holding, FullEnvironment with { VersionsUrl = Versions });
    }

    /// <summary>VersionsUrl and VersionsHistoryUrl are two slots and never one: the environment holding the versions
    /// file in one and the history page in the other is not the environment holding them the other way round, either
    /// one alone stands apart from both, and the environment with neither named is a third - the hash following each
    /// of these, and the <c>Deconstruct</c> of the eight reading the two back in their own order.</summary>
    [Fact]
    public void TheTwoVersionSlotsAreTwoSlotsAndNeverOne()
    {
        var one = FullEnvironment;
        var swapped = FullEnvironment with { VersionsUrl = VersionsHistory, VersionsHistoryUrl = Versions };

        // The environment holding the versions file in one and the history page in the other is not the environment
        // holding them the other way round.
        Assert.NotEqual(one, swapped);

        // Either one alone stands apart from both.
        var versionsAlone = FullEnvironment with { VersionsHistoryUrl = null };
        var historyAlone = FullEnvironment with { VersionsUrl = null };

        Assert.NotEqual(one, versionsAlone);
        Assert.NotEqual(swapped, versionsAlone);
        Assert.NotEqual(one, historyAlone);
        Assert.NotEqual(swapped, historyAlone);
        Assert.NotEqual(versionsAlone, historyAlone);

        // The environment with neither named is a third.
        var neither = FullEnvironment with { VersionsUrl = null, VersionsHistoryUrl = null };

        Assert.NotEqual(one, neither);
        Assert.NotEqual(swapped, neither);
        Assert.NotEqual(versionsAlone, neither);
        Assert.NotEqual(historyAlone, neither);

        // The hash following each of these: the same on the second asking, and the five standing under five keys.
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Equal(swapped.GetHashCode(), swapped.GetHashCode());
        Assert.Equal(versionsAlone.GetHashCode(), versionsAlone.GetHashCode());
        Assert.Equal(historyAlone.GetHashCode(), historyAlone.GetHashCode());
        Assert.Equal(neither.GetHashCode(), neither.GetHashCode());

        Assert.Equal(
            5,
            new HashSet<int>
            {
                one.GetHashCode(),
                swapped.GetHashCode(),
                versionsAlone.GetHashCode(),
                historyAlone.GetHashCode(),
                neither.GetHashCode(),
            }.Count);

        // The Deconstruct of the eight reading the two back in their own order: the versions file back in the fourth
        // and the history page in the fifth of the one, and the two read back in the order the swap holds them.
        var (_, _, _, versionsSlot, historySlot, _, _, _) = one;

        Assert.Same(Versions, versionsSlot);
        Assert.Same(VersionsHistory, historySlot);

        var (_, _, _, swapFirst, swapSecond, _, _, _) = swapped;

        Assert.Same(VersionsHistory, swapFirst);
        Assert.Same(Versions, swapSecond);
    }

    // ------------------------------------------------------------------ the record itself

    /// <summary>Two environments are one while their eight parts are: the two operators, <c>Equals</c> both ways and
    /// the object it takes, and the hash code that follows them and stands from call to call. An environment is one
    /// with nothing that is not an environment - and with a second instance built on the very same eight holds.</summary>
    [Fact]
    public void TwoEnvironmentsAreOneWhileTheirEightPartsAre()
    {
        var one = FullEnvironment;
        var two = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", OnePage);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("uat"));
        Assert.False(one.Equals(Versions));
        Assert.False(one.Equals(BareEnvironment));
    }

    /// <summary>Any one of the eight parts tells two environments apart - and nothing is compared loosely: the empty
    /// tier and the absent one are two parts, the absent link set and the one with nothing in it are two, the empty
    /// page list and the absent one are two, a second list of the very same deployables is another list, a name in
    /// another case is another name, and a second link set of the one and the same link is another hold.</summary>
    [Fact]
    public void AnEnvironmentDiffersInAnyOneOfItsEightParts()
    {
        var environment = FullEnvironment;

        Assert.NotEqual(environment, environment with { Name = string.Empty });
        Assert.NotEqual(environment, environment with { Name = "UAT" });
        Assert.NotEqual(environment, environment with { Name = "prod" });
        Assert.NotEqual(environment, environment with { Tier = null });
        Assert.NotEqual(environment, environment with { Tier = string.Empty });
        Assert.NotEqual(environment, environment with { Tier = "NONPROD" });
        Assert.NotEqual(environment, environment with { Deployables = NoDeployables });
        Assert.NotEqual(environment, environment with { Deployables = OneDeployable });
        Assert.NotEqual(environment, environment with { Deployables = [.. TwoDeployables] });
        Assert.NotEqual(environment, environment with { VersionsUrl = null });
        Assert.NotEqual(environment, environment with { VersionsHistoryUrl = null });
        Assert.NotEqual(environment, environment with { Links = null });
        Assert.NotEqual(environment, environment with { Links = LinkSet.None });
        Assert.NotEqual(environment, environment with { Links = OtherLinks });
        Assert.NotEqual(environment, environment with { Namespace = string.Empty });
        Assert.NotEqual(environment, environment with { Namespace = "demo-prod" });
        Assert.NotEqual(environment, environment with { Pages = null });
        Assert.NotEqual(environment, environment with { Pages = NoPages });

        // The empty word and the nothing that is no word are two parts, twice over.
        Assert.NotEqual(new EnvironmentInfo("uat", string.Empty, NoDeployables), new EnvironmentInfo("uat", null, NoDeployables));
        Assert.NotEqual(new EnvironmentInfo("uat", "nonprod", NoDeployables, Pages: NoPages), BareEnvironment);
    }

    /// <summary>The record's hash combines its eight parts, so every one-part change of the full environment is an
    /// environment apart and a hash apart: the name to another word and to the empty word, the tier to nothing and
    /// to the empty word, the deployables to the empty list, each version address to nothing, the links to nothing,
    /// the namespace to the empty word and the pages to nothing - each making another environment, and its hash
    /// code landing on another number. And the loop closed the other way: the two environments that differ only in
    /// which <see cref="LinkSet" /> instance they hold, the one and the same link in both, are two environments with
    /// two hashes, and their version address told in the other spelling the address machinery calls the same - the
    /// host in another case, the scheme's own port written out - leaves each one environment with one hash, the
    /// spelling <see cref="AVersionAddressIsComparedAsAnAddressIs" /> pins and this fact holding the environment in
    /// it.</summary>
    [Fact]
    public void EachOnePartChangeMakesAnotherEnvironmentAndItsHashLandsOnAnotherNumber()
    {
        var environment = FullEnvironment;
        var hash = environment.GetHashCode();

        // Each one-part change makes another environment and lands its hash on another number: the record's hash
        // combines the parts, so a part moved is a key moved.
        Assert.NotEqual(environment, environment with { Name = "prod" });
        Assert.NotEqual(hash, (environment with { Name = "prod" }).GetHashCode());
        Assert.NotEqual(environment, environment with { Name = string.Empty });
        Assert.NotEqual(hash, (environment with { Name = string.Empty }).GetHashCode());
        Assert.NotEqual(environment, environment with { Tier = null });
        Assert.NotEqual(hash, (environment with { Tier = null }).GetHashCode());
        Assert.NotEqual(environment, environment with { Tier = string.Empty });
        Assert.NotEqual(hash, (environment with { Tier = string.Empty }).GetHashCode());
        Assert.NotEqual(environment, environment with { Deployables = NoDeployables });
        Assert.NotEqual(hash, (environment with { Deployables = NoDeployables }).GetHashCode());
        Assert.NotEqual(environment, environment with { VersionsUrl = null });
        Assert.NotEqual(hash, (environment with { VersionsUrl = null }).GetHashCode());
        Assert.NotEqual(environment, environment with { VersionsHistoryUrl = null });
        Assert.NotEqual(hash, (environment with { VersionsHistoryUrl = null }).GetHashCode());
        Assert.NotEqual(environment, environment with { Links = null });
        Assert.NotEqual(hash, (environment with { Links = null }).GetHashCode());
        Assert.NotEqual(environment, environment with { Namespace = string.Empty });
        Assert.NotEqual(hash, (environment with { Namespace = string.Empty }).GetHashCode());
        Assert.NotEqual(environment, environment with { Pages = null });
        Assert.NotEqual(hash, (environment with { Pages = null }).GetHashCode());

        // The loop closed the other way: the two environments that differ only in which LinkSet instance they hold
        // - the one and the same link in both - are two environments with two hashes, and their version address
        // told in the other spelling the address machinery calls the same - the host in another case, the scheme's
        // own port written out - leaves each one environment with one hash.
        var one = FullEnvironment;
        var other = FullEnvironment with { Links = OtherLinks };

        Assert.NotEqual(one, other);
        Assert.NotEqual(one.GetHashCode(), other.GetHashCode());

        var spelled = new Uri("https://RAW.example.net:443/org/demo-system/main/environments/uat/versions.json");
        var oneSpelled = one with { VersionsUrl = spelled };
        var otherSpelled = other with { VersionsUrl = spelled };

        Assert.Equal(one, oneSpelled);
        Assert.Equal(oneSpelled.GetHashCode(), one.GetHashCode());
        Assert.Equal(other, otherSpelled);
        Assert.Equal(otherSpelled.GetHashCode(), other.GetHashCode());
    }

    /// <summary>The links an environment holds are compared as the hold they stand on and not as their keys: the
    /// very same link set makes two environments one, and a second set of the one and the same link makes two
    /// environments of them - the record opening neither to look inside it.</summary>
    [Fact]
    public void TheLinksAnEnvironmentHoldsAreComparedAsTheHoldTheyStandOn()
    {
        var one = FullEnvironment;
        var holdingTheSameLinks = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", OnePage);
        var holdingOtherLinks = one with { Links = OtherLinks };

        Assert.Equal(one, holdingTheSameLinks);
        Assert.Equal(one.GetHashCode(), holdingTheSameLinks.GetHashCode());

        Assert.Equal(one.Links!.Keys, holdingOtherLinks.Links!.Keys);
        Assert.Equal(one.Links[LinkSet.Portal], holdingOtherLinks.Links[LinkSet.Portal]);
        Assert.NotSame(one.Links, holdingOtherLinks.Links);
        Assert.True(one != holdingOtherLinks);
        Assert.NotEqual(one, holdingOtherLinks);
    }

    /// <summary>An environment is none of the things it is not, and none of them is an environment: the deployable it
    /// holds, the node behind that deployable, the page its part of the dashboard leads to, the link set it stands
    /// on, the address of its pin file, the word of its name and the nothing that is no environment at all.</summary>
    [Fact]
    public void AnEnvironmentIsNoneOfTheThingsItIsNot()
    {
        var environment = FullEnvironment;

        Assert.False(environment.Equals(Ui));
        Assert.False(environment.Equals(West));
        Assert.False(environment.Equals(Scorecard));
        Assert.False(environment.Equals(Links));
        Assert.False(environment.Equals(Versions));
        Assert.False(environment.Equals("uat"));
        Assert.False(environment.Equals(null));
        Assert.False(environment.Equals(new object()));

        Assert.False(Ui.Equals(environment));
        Assert.False(West.Equals(environment));
        Assert.False(Scorecard.Equals(environment));
    }

    /// <summary>The hash code an environment says stands where it stood: asked twice of the one instance, asked of a
    /// second instance built on the very same eight holds, and asked of the environment that names none of them -
    /// and it follows the parts the environment is compared by, so two environments that are two are two keys.</summary>
    [Fact]
    public void TheHashCodeOfAnEnvironmentStandsFromCallToCall()
    {
        var environment = FullEnvironment;

        Assert.Equal(environment.GetHashCode(), environment.GetHashCode());
        Assert.Equal(environment.GetHashCode(), (environment with { }).GetHashCode());
        Assert.Equal(BareEnvironment.GetHashCode(), new EnvironmentInfo("tdd", null, NoDeployables).GetHashCode());

        // The hash code leads to the environment: a second instance of the one parts is found under the first, and an
        // environment that moves a part is no key in the dictionary the first stands in.
        var keyed = new Dictionary<EnvironmentInfo, string> { [environment] = "uat" };

        Assert.Equal("uat", keyed[environment with { }]);
        Assert.False(keyed.ContainsKey(environment with { Namespace = "demo-prod" }));
        Assert.Equal(2, new HashSet<EnvironmentInfo> { environment, environment with { Namespace = "demo-prod" } }.Count);
    }

    /// <summary>
    /// The hash the environment says, stood where it stood under the culture the reader reads it in: asked twice
    /// of the one instance, asked of the second instance built on the very same eight holds, and asked of the
    /// <c>with</c> copy that changes nothing - all the one number, and it stands alike under the invariant
    /// culture, the <c>en-US</c> one and the <c>de-DE</c> one. It keys the environment: the set of the full
    /// environment, its <c>with</c> copy and the environment that names none of the eight stands on two of them,
    /// and the dictionary written on the full environment reads it back under its <c>with</c> copy and under the
    /// second instance - and never finds the environment that names none of the eight.
    /// </summary>
    [Fact]
    public void TheHashCodeOfAnEnvironmentStandsUnderTheThreeCulturesAndKeysIt()
    {
        var environment = FullEnvironment;
        var copy = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", OnePage);

        Assert.Equal(environment.GetHashCode(), environment.GetHashCode());
        Assert.Equal(environment.GetHashCode(), copy.GetHashCode());
        Assert.Equal(environment.GetHashCode(), (environment with { }).GetHashCode());

        Assert.Equal(environment.GetHashCode(), HashUnder(CultureInfo.InvariantCulture));
        Assert.Equal(environment.GetHashCode(), HashUnder(new CultureInfo("en-US")));
        Assert.Equal(environment.GetHashCode(), HashUnder(new CultureInfo("de-DE")));

        Assert.Equal(2, new HashSet<EnvironmentInfo> { environment, environment with { }, BareEnvironment }.Count);

        var keyed = new Dictionary<EnvironmentInfo, string> { [environment] = "uat" };

        Assert.Equal("uat", keyed[environment with { }]);
        Assert.Equal("uat", keyed[copy]);
        Assert.False(keyed.ContainsKey(BareEnvironment));
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
        Assert.Same(Versions, moved.VersionsUrl);
        Assert.Same(OnePage, moved.Pages);
        Assert.Equal("demo-uat", environment.Namespace);
        Assert.Equal("nonprod", environment.Tier);

        // The environment named by the three parts a caller must name, its deployables the two it was given: every
        // part a caller may leave out named as nothing, and the three the copy carries standing where they stood.
        var unnamed = environment with { Tier = null, VersionsUrl = null, VersionsHistoryUrl = null, Links = null, Namespace = null, Pages = null };

        Assert.Equal(new EnvironmentInfo("uat", null, TwoDeployables), unnamed);
        Assert.Null(unnamed.Tier);
        Assert.Null(unnamed.Pages);
        Assert.Same(TwoDeployables, unnamed.Deployables);
        Assert.NotEqual(BareEnvironment, unnamed);
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
        Assert.Same(Versions, versionsUrl);
        Assert.Same(VersionsHistory, versionsHistoryUrl);
        Assert.Same(Links, links);
        Assert.Equal("demo-uat", @namespace);
        Assert.Same(OnePage, pages);

        var (bareName, bareTier, bareDeployables, bareVersions, bareHistory, bareLinks, bareNamespace, barePages) = BareEnvironment;

        Assert.Equal("tdd", bareName);
        Assert.Null(bareTier);
        Assert.Same(NoDeployables, bareDeployables);
        Assert.Null(bareVersions);
        Assert.Null(bareHistory);
        Assert.Null(bareLinks);
        Assert.Null(bareNamespace);
        Assert.Null(barePages);

        var (nameOnly, _, oneDeployable, _, _, _, _, onePage) = FullEnvironment with { Deployables = OneDeployable, Pages = NoPages };

        Assert.Equal("uat", nameOnly);
        Assert.Same(OneDeployable, oneDeployable);
        Assert.Same(NoPages, onePage);
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

        // The environment that was handed nothing at all: every part named, and every one of them saying nothing.
        Assert.Equal(
            "EnvironmentInfo { Name = , Tier = , Deployables = , VersionsUrl = , VersionsHistoryUrl = , Links = , "
                + "Namespace = , Pages =  }",
            new EnvironmentInfo(null!, null, null!).ToString());

        // The words the environment is told stand in the line as they were told: nothing of them escaped and none of
        // them cut short.
        var words = "a\"b = c {d} <e> [f] g/h \\ i";

        Assert.Equal(
            "EnvironmentInfo { Name = " + words + ", Tier = , Deployables = Dashboard.Health.DeployableInfo[], "
                + "VersionsUrl = , VersionsHistoryUrl = , Links = , Namespace = , Pages =  }",
            new EnvironmentInfo(words, null, NoDeployables).ToString());
    }

    // ------------------------------------------------------------------ the record's own shape

    /// <summary>The environment's whole public surface as it stands: its eight parts, named in the order the
    /// constructor names them and of the type the constructor says, and the members every record is asked - falling
    /// apart, comparing, hashing and saying itself - and nothing else to call: an environment decides nothing of its
    /// own. A ninth part added here has to be written down here too, which is the point.</summary>
    [Fact]
    public void TheEnvironmentHasEightPartsAndDecidesNothingOfItsOwn()
    {
        Assert.Equal(
            ["Name", "Tier", "Deployables", "VersionsUrl", "VersionsHistoryUrl", "Links", "Namespace", "Pages"],
            [.. typeof(EnvironmentInfo)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [
                "Deconstruct", "Deployables", "Equals", "GetHashCode", "Links", "Name", "Namespace", "Pages",
                "Tier", "ToString", "VersionsHistoryUrl", "VersionsUrl", "op_Equality", "op_Inequality",
            ],
            PublicSurface());

        var properties = typeof(EnvironmentInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(string), properties["Tier"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<DeployableInfo>), properties["Deployables"].PropertyType);
        Assert.Equal(typeof(Uri), properties["VersionsUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["VersionsHistoryUrl"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);
        Assert.Equal(typeof(string), properties["Namespace"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<PageLink>), properties["Pages"].PropertyType);

        // The one constructor, naming its eight parts as the properties are named: a part may be left out only from
        // the fourth on, the three before it being what an environment must be told.
        var parameters = typeof(EnvironmentInfo).GetConstructors().Single().GetParameters();

        Assert.Equal(
            ["Name", "Tier", "Deployables", "VersionsUrl", "VersionsHistoryUrl", "Links", "Namespace", "Pages"],
            [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false, false, true, true, true, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);

        // The three a caller must name carry no default at all, and the five it may leave out default to nothing: the
        // record holds no address, no link set, no namespace and no page of its own.
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.All(parameters.Skip(3), parameter => Assert.Null(parameter.DefaultValue));

        // The nullable annotations stand as the record declares them: the tier may be nothing although it may not be
        // left out, the deployables and the name may not be nothing, and the five after them may.
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, true, false, true, true, true, true, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        Assert.Equal([typeof(IEquatable<EnvironmentInfo>)], typeof(EnvironmentInfo).GetInterfaces());

        // The contract every record compares by is the compiler's private one: no public part of an environment.
        Assert.Null(typeof(EnvironmentInfo).GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(typeof(EnvironmentInfo).GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));

        // No word of its own, no default of its own and nothing fixed the record carries: an environment is the eight
        // parts it was handed and nothing more.
        Assert.Empty(typeof(EnvironmentInfo).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    /// <summary>The eight properties say of themselves, as the compiler writes it into each of them, which may be
    /// nothing and which may not - read out of the property and written back to it, the same eight answers the
    /// constructor's eight parameters give: the record promises the page a name and a list of deployables, and
    /// nothing of its own for the six a caller may leave out - and what the caller asks after the making and a
    /// reader of a record read back leans on.</summary>
    [Fact]
    public void TheEightPartsSayOfThemselvesWhichMayBeNothing()
    {
        var nullability = new NullabilityInfoContext();
        NullabilityState[] expected =
        [
            NullabilityState.NotNull,
            NullabilityState.Nullable,
            NullabilityState.NotNull,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
        ];

        var properties = typeof(EnvironmentInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(expected, [.. properties.Select(property => nullability.Create(property).ReadState)]);
        Assert.Equal(expected, [.. properties.Select(property => nullability.Create(property).WriteState)]);
    }

    /// <summary>Every part is written only at the making: the property reads and its setter stands, and that setter
    /// is the compiler's <c>init</c> - carrying the custom modifier the compiler puts on it - so no caller turns a
    /// part of an environment it was handed; the <c>with</c> copy reaches the parts from inside the record.</summary>
    [Fact]
    public void EveryPartIsWrittenOnlyAtTheMakingAndNeverTurnedAfter()
    {
        var properties = typeof(EnvironmentInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.GetMethod);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });

        // The page of parts the reader of the file hands over is read, not written: the eight of the environment
        // stand as they were made, whatever is asked of them in between.
        var environment = FullEnvironment;
        _ = environment.Deployables.Count;
        _ = environment.Pages!.Count;

        Assert.Same(TwoDeployables, environment.Deployables);
        Assert.Same(OnePage, environment.Pages);
    }

    // ------------------------------------------------------------------ the machinery

    /// <summary>The public members the environment declares itself: its properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(EnvironmentInfo)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>Every word the reader of the file hands an environment is kept as-is, not trimmed, not normalised,
    /// not escaped, not put through a format: the property returns the very string it was handed, the same word
    /// stands whole in the line of words the environment says, and an environment with that word is not one with
    /// the word trimmed, upper-cased or turned to nothing - not for the name, not for the tier, not for the
    /// namespace, not for an empty word, not for a word of only spaces, not for a word with a newline and a tab
    /// inside it, not for a word with letters outside ASCII and a dash and an emoji, not for a word of five
    /// hundred and twelve letters, not for a word of one letter, not for a word that reads like markup and not
    /// for a word that reads like a format hole.</summary>
    [Theory]
    [MemberData(nameof(AllWords))]
    public void AWordIsKeptAsItWasHanded(string word)
    {
        Assert.NotNull(word);

        // The name: the property says back the very word it was handed, and the word stands whole in the line.
        var named = new EnvironmentInfo(word, null, NoDeployables);

        Assert.Equal(word, named.Name);
        Assert.Contains(word, named.ToString(), StringComparison.Ordinal);

        // The same word, trimmed, upper-cased and turned to nothing, makes an environment apart. A word of only
        // spaces has no upper case and cannot be trimmed to nothing, and the name may not be null, so those three
        // are weighed only when the word can be trimmed and has letters.
        var trimmed = word.Trim();

        if (word != trimmed)
        {
            Assert.NotEqual(named, new EnvironmentInfo(trimmed, null, NoDeployables));
        }

        var upper = word.ToUpper(CultureInfo.InvariantCulture);

        if (word != upper)
        {
            Assert.NotEqual(named, new EnvironmentInfo(upper, null, NoDeployables));
        }

        // The tier: a tier may be nothing, so the word and nothing are compared, and the word and its upper case
        // are compared when the word has letters.
        var tiered = new EnvironmentInfo("uat", word, NoDeployables);

        Assert.Equal(word, tiered.Tier);
        Assert.Contains(word, tiered.ToString(), StringComparison.Ordinal);

        Assert.NotEqual(tiered, new EnvironmentInfo("uat", null, NoDeployables));

        if (word != upper)
        {
            Assert.NotEqual(tiered, new EnvironmentInfo("uat", upper, NoDeployables));
        }

        // The namespace: the word and the word trimmed, the word and its upper case, the word and nothing.
        var namespaced = new EnvironmentInfo("uat", "nonprod", NoDeployables, Namespace: word);

        Assert.Equal(word, namespaced.Namespace);
        Assert.Contains(word, namespaced.ToString(), StringComparison.Ordinal);

        Assert.NotEqual(namespaced, new EnvironmentInfo("uat", "nonprod", NoDeployables, Namespace: null));

        if (word != trimmed)
        {
            Assert.NotEqual(namespaced, new EnvironmentInfo("uat", "nonprod", NoDeployables, Namespace: trimmed));
        }

        if (word != upper)
        {
            Assert.NotEqual(namespaced, new EnvironmentInfo("uat", "nonprod", NoDeployables, Namespace: upper));
        }
    }

    /// <summary>The ten words weighed by the theory above, including the five hundred and twelve-letter word that
    /// stands as a string built at test time, since no attribute of xUnit takes an expression of that length.
    /// Every one is passed through the same three ways of holding it (name, tier, namespace) and the same three
    /// ways it may be changed (trimmed, upper-cased, turned to nothing).</summary>
    public static IEnumerable<object[]> AllWords => new[]
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "   " },
        new object[] { "  uat  " },
        new object[] { "a\nb\tc" },
        new object[] { "café – 😀" },
        new object[] { new string('n', 512) },
        new object[] { "u" },
        new object[] { "<b>uat</b>" },
        new object[] { "{0} {1}" },
    };

    /// <summary>Environment names are exact strings, not culture-aware words: the same eight parts build one
    /// environment in every culture, two names that are one word in one culture are two environments in every
    /// culture, and the words the environment says about itself stand alike whatever the reader speaks. The
    /// Turkish I is the proof: in Turkish, the upper case of <c>i</c> is not <c>I</c> but <c>İ</c>, and a case
    /// insensitive comparison says <c>I</c> and <c>i</c> are one word; the record compares the letters and says
    /// two environments, in Turkish as in German as in Arabic. The culture is set on the thread so that
    /// <c>ToString</c>, <c>GetHashCode</c> and the string comparison each see the culture, and put back so the
    /// test leaves the reader as it found it.</summary>
    [Fact]
    public void TheEnvironmentIsTheSameInEveryCultureAndTwoNamesAreTwoEnvironmentsInEveryCulture()
    {
        var cultures = new[]
        {
            new CultureInfo("tr-TR"),
            new CultureInfo("de-DE"),
            new CultureInfo("ar-SA"),
        };

        foreach (var culture in cultures)
        {
            var reader = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = culture;

                // The same eight parts build one environment in every culture.
                var rebuilt = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", OnePage);

                Assert.True(FullEnvironment == rebuilt);
                Assert.True(FullEnvironment.Equals(rebuilt));
                Assert.Equal(FullEnvironment.GetHashCode(), rebuilt.GetHashCode());

                // Two names that are one word in one culture are two environments in every culture.
                var uppercaseI = new EnvironmentInfo("I", null, NoDeployables);
                var lowercaseI = new EnvironmentInfo("i", null, NoDeployables);
                var uppercaseIstanbul = new EnvironmentInfo("ISTANBUL", null, NoDeployables);
                var lowercaseIstanbul = new EnvironmentInfo("istanbul", null, NoDeployables);

                Assert.NotEqual(uppercaseI, lowercaseI);
                Assert.True(uppercaseI != lowercaseI);
                Assert.False(uppercaseI.Equals(lowercaseI));
                Assert.NotEqual(uppercaseIstanbul, lowercaseIstanbul);
                Assert.True(uppercaseIstanbul != lowercaseIstanbul);
                Assert.False(uppercaseIstanbul.Equals(lowercaseIstanbul));

                // Even though in Turkish a case insensitive comparison says they are one word.
                if (culture.Name == "tr-TR")
                {
                    bool iAreSame = string.Equals("I", "i", StringComparison.OrdinalIgnoreCase);
                    Assert.True(iAreSame);

                    bool istanbulAreSame = string.Equals("ISTANBUL", "istanbul", StringComparison.OrdinalIgnoreCase);
                    Assert.True(istanbulAreSame);
                }

                // The words the environment says about itself stand alike whatever the reader speaks.
                var words = FullEnvironment.ToString();

                Assert.StartsWith("EnvironmentInfo { Name = uat, Tier = nonprod", words, StringComparison.Ordinal);
                Assert.Contains("demo-uat", words, StringComparison.Ordinal);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = reader;
            }
        }
    }

    /// <summary>The hash the environment says under the culture named, the reader's own culture restored after.</summary>
    private static int HashUnder(CultureInfo culture)
    {
        var reader = Thread.CurrentThread.CurrentCulture;

        try
        {
            Thread.CurrentThread.CurrentCulture = culture;
            return FullEnvironment.GetHashCode();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
