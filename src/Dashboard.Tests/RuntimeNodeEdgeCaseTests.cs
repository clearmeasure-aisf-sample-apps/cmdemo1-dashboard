using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeNode" />: one drawn box of an environment's
/// runtime diagram, the three parts it must be told and the eleven it may be told. The record decides nothing of its
/// own - no verdict, no words for the page, no moment - so what is pinned here is that each part comes back as the
/// part it went in as, that the record refuses nothing it is handed, that a part nobody names stands the absence
/// rather than a blank of its own, and that the members every record is asked compare, hash, fall apart, copy and
/// say the fourteen parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeManifestParserTests" /> and
/// <see cref="RuntimeManifestParserEdgeCaseTests" /> pin the read out of a file: that <c>alias</c> is required, that a
/// node with no <c>name</c> is called by its alias, that every word of a kind - in any case, trimmed, and any word
/// the dashboard has never heard - becomes the <see cref="RuntimeNodeKind" /> it becomes, that only the role is
/// brought to lower case, that an address is kept only when it is an absolute http or https one, and which health
/// paths, ids and links the reader keeps and which it leaves out. <see cref="ApplicationViewTests" /> pins that an
/// <c>architecture</c> survives that read only for a web app or a container app and only over https, and
/// <see cref="RuntimePayloadBuilderTests" />, <see cref="DependencyTests" /> and
/// <see cref="RuntimeWorkHelperCoverageTests" /> read the parts back through the tiles the page is built out of.
/// <see cref="RuntimeBoxLinkEdgeCaseTests" /> and <see cref="RuntimeBoxLinkTests" /> pin what a box's
/// <see cref="Links" /> lead to and say; what is pinned here of that part is only that the box holds the set it was
/// handed, as it does the ids and the two addresses.
/// <see cref="NodeInfoEdgeCaseTests" /> pins the same shape for the topology's node - a different record, of five
/// parts and with a decision of its own, so nothing of it is taken as pinned for this one.
/// </para>
/// <para>
/// What follows is the record on its own, at the ends the tests above leave open: the fourteen parts by the names of
/// their own properties and the eleven standing absent when nobody names them, the empty word that is a word and the
/// spaces and the letters that only look alike, the kind the record neither requires to be one it knows nor weighs
/// against the box it draws, the addresses and the link set held as the instances they stand on while an address is
/// compared as an address is and a list of ids as a list, the box that refuses nothing - no alias, no name, no kind
/// at all - and the whole public surface, so that the day a fifteenth part is added the list of them says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a box holds no moment and calls nothing, every
/// address below is one the test brought and none of them is fetched.
/// </para>
/// </summary>
public class RuntimeNodeEdgeCaseTests
{
    private static readonly Uri Url = new("https://app-ui.example.net/_health");
    private static readonly Uri Diagrams = new("https://releases.example.net/runtime");
    private static readonly Uri OtherUrl = new("https://app-ui.example.io/_health");
    private static readonly Uri Portal = new("https://portal.example.net/#app_ui");

    /// <summary>Two link sets of the one key and the one address: the box opens neither of them, so which of the two
    /// it stands on is the whole of the difference between these two tests' boxes.</summary>
    private static readonly LinkSet OneLink = new([new KeyValuePair<string, Uri>(LinkSet.Portal, Portal)]);

    private static readonly LinkSet OtherLink = new([new KeyValuePair<string, Uri>(LinkSet.Portal, Portal)]);

    /// <summary>The ids of <see cref="Full" />, held once so that two boxes built from it stand on one list: the
    /// record compares the list it is handed and not what stands in it, which is what these tests are about.</summary>
    private static readonly List<string> TheIds = ["/subscriptions/sub/resourceGroups/rg-uat/providers/Microsoft.Web/sites/ca-demo-uat-ui"];

    /// <summary>An array rather than a list, so that the words a box says of its ids name the array they stand
    /// on.</summary>
    private static readonly string[] OneId = ["/subscriptions/sub"];

    /// <summary>A box with every part named, the values all apart from one another so that an argument landing on
    /// another property is seen instead of quietly matching a second part of the record.</summary>
    private static RuntimeNode Full => new(
        "app_ui",
        RuntimeNodeKind.WebApp,
        "ca-demo-uat-ui",
        Url,
        "ui",
        "primary",
        "westus3",
        "Region_Primary",
        "LlmGateway",
        "external",
        OneLink,
        "/_healthcheck",
        TheIds,
        Diagrams);

    /// <summary>A box named by nothing but the three a caller must name: the eleven the caller may leave out, left
    /// out.</summary>
    private static RuntimeNode Bare => new("app_ui", RuntimeNodeKind.WebApp, "ca-demo-uat-ui");

    /// <summary>The box of nothing at all: no alias, no kind named, no name - the parts the record will hold although
    /// no file may give them.</summary>
    private static RuntimeNode Nothing => new(null!, default, null!);

    /// <summary>The box of empty words: parts said as nothing, which the record keeps as words and not as the absence
    /// of one.</summary>
    private static RuntimeNode Empty => new(string.Empty, RuntimeNodeKind.Other, string.Empty);

    // --------------------------------------------- the fourteen parts, by the names of their own properties

    /// <summary>The constructor's fourteen parts, position by position, each back as the property of its own name -
    /// the order the reader of a manifest hands a box over in. The two addresses, the link set and the list of ids
    /// come back as the very instances the box was handed, never copied or opened.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var node = Full;

        Assert.Equal("app_ui", node.Alias);
        Assert.Equal(RuntimeNodeKind.WebApp, node.Kind);
        Assert.Equal("ca-demo-uat-ui", node.Name);
        Assert.Same(Url, node.Url);
        Assert.Equal("ui", node.Deployable);
        Assert.Equal("primary", node.Role);
        Assert.Equal("westus3", node.Region);
        Assert.Equal("Region_Primary", node.RegionAlias);
        Assert.Equal("LlmGateway", node.HealthCheck);
        Assert.Equal("external", node.DependencyKind);
        Assert.Same(OneLink, node.Links);
        Assert.Equal("/_healthcheck", node.HealthPath);
        Assert.Equal(["/subscriptions/sub/resourceGroups/rg-uat/providers/Microsoft.Web/sites/ca-demo-uat-ui"], node.Ids);
        Assert.Same(Diagrams, node.Architecture);
    }

    /// <summary>The three parts a caller must name, and the eleven that stand absent when it names none of them -
    /// absent, and not a blank of the record's own: no empty address, no empty link set, no list with nothing in it,
    /// and the kind the caller does not name being the one kind the dashboard does not know.</summary>
    [Fact]
    public void ThePartsTheCallerDoesNotNameStandAbsent()
    {
        var node = Bare;

        Assert.Equal("app_ui", node.Alias);
        Assert.Equal(RuntimeNodeKind.WebApp, node.Kind);
        Assert.Equal("ca-demo-uat-ui", node.Name);
        Assert.Null(node.Url);
        Assert.Null(node.Deployable);
        Assert.Null(node.Role);
        Assert.Null(node.Region);
        Assert.Null(node.RegionAlias);
        Assert.Null(node.HealthCheck);
        Assert.Null(node.DependencyKind);
        Assert.Null(node.Links);
        Assert.Null(node.HealthPath);
        Assert.Null(node.Ids);
        Assert.Null(node.Architecture);

        // Naming an optional part as nothing names it: the record holds no default of its own behind any of them.
        Assert.Equal(
            Bare,
            new RuntimeNode(
                "app_ui",
                RuntimeNodeKind.WebApp,
                "ca-demo-uat-ui",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null));

        // The kind a caller leaves out is the kind nothing was said of, which is the first of the nine.
        Assert.Equal(RuntimeNodeKind.Other, Nothing.Kind);
        Assert.Equal(default(RuntimeNodeKind), Nothing.Kind);
    }

    /// <summary>A box keeps the words it is given and changes none of them - the empty word is a word and not
    /// nothing, the spaces about a word stay about it, a role and a kind word written over in caps stay caps although
    /// the reader of the file lowers the role and matches the kind whole, a health path that is no path at all is
    /// held, markup is held, and a word of letters that only look alike is held as the letters it stands on. That the
    /// file's words arrive trimmed, lowered and fallen back is the reader's, and is pinned there.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  westus3  ")]
    [InlineData("WEBAPP")]
    [InlineData("PRIMARY")]
    [InlineData("Web App")]
    [InlineData("/_healthcheck")]
    [InlineData("not a path at all")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("ünïcødé-név")]
    [InlineData("ca-demo-uat-ui")]
    public void EveryWordOfABoxIsKeptExactlyAsItIsGiven(string word)
    {
        var node = new RuntimeNode(word, RuntimeNodeKind.WebApp, word)
        {
            Deployable = word,
            Role = word,
            Region = word,
            RegionAlias = word,
            HealthCheck = word,
            DependencyKind = word,
            HealthPath = word,
        };

        Assert.Equal(word, node.Alias);
        Assert.Equal(word, node.Name);
        Assert.Equal(word, node.Deployable);
        Assert.Equal(word, node.Role);
        Assert.Equal(word, node.Region);
        Assert.Equal(word, node.RegionAlias);
        Assert.Equal(word, node.HealthCheck);
        Assert.Equal(word, node.DependencyKind);
        Assert.Equal(word, node.HealthPath);
    }

    /// <summary>The empty word, the word of spaces and the nothing that is no word are three parts of a box: the
    /// record weighs what it is handed as it stands, so a role left out, a role of no letters and a role of one space
    /// are three boxes - and the alias and the name, which the reader of the file may fall back from one to the other,
    /// are two parts here and never made equal.</summary>
    [Fact]
    public void TheEmptyWordTheWordOfSpacesAndNoWordAtAllAreThreeParts()
    {
        Assert.NotEqual(Empty, Nothing);
        Assert.NotEqual(Empty, Empty with { Alias = " " });
        Assert.NotEqual(Empty with { Role = string.Empty }, Empty with { Role = null });
        Assert.NotEqual(Empty with { Name = "web " }, Empty with { Name = "web" });
        Assert.NotEqual(Bare with { Alias = string.Empty }, Bare with { Name = string.Empty });
        Assert.NotEqual(Bare with { Alias = null! }, Bare with { Name = null! });
    }

    /// <summary>The record makes no decision of its own about what a part may be: a box will hold no alias, no name
    /// and no kind at all, and every optional part as nothing, since the file is what is read and the reader is what
    /// refuses. Only the parts that stand apart tell two boxes apart, so a box of nothing with its three named parts
    /// filled in is one with a box whose parts were blanked out.</summary>
    [Fact]
    public void ABoxRefusesNothingItIsGiven()
    {
        var node = Nothing;

        Assert.Null(node.Alias);
        Assert.Null(node.Name);
        Assert.Equal(RuntimeNodeKind.Other, node.Kind);
        Assert.Null(node.Url);
        Assert.Null(node.Deployable);
        Assert.Null(node.Role);
        Assert.Null(node.Region);
        Assert.Null(node.RegionAlias);
        Assert.Null(node.HealthCheck);
        Assert.Null(node.DependencyKind);
        Assert.Null(node.Links);
        Assert.Null(node.HealthPath);
        Assert.Null(node.Ids);
        Assert.Null(node.Architecture);

        Assert.Equal(
            node with { Alias = "app_ui", Name = "ca-demo-uat-ui", Kind = RuntimeNodeKind.WebApp },
            Bare with
            {
                Url = null,
                Deployable = null,
                Role = null,
                Region = null,
                RegionAlias = null,
                HealthCheck = null,
                DependencyKind = null,
                Links = null,
                HealthPath = null,
                Ids = null,
                Architecture = null,
            });
    }

    // ------------------------------------------------------------------ the kind of a box

    /// <summary>The nine kinds a box may be told, each held as it was given and each telling the box apart from the
    /// box of any other kind - the record weighing the kind and nothing else about it, so a database and a web app of
    /// the same eleven other parts are two boxes. <see cref="RuntimeNodeKind.Other" /> is the unset one, the zero of
    /// the nine, which is why a box built with the kind left out is a box drawn and not updated.</summary>
    [Theory]
    [InlineData(RuntimeNodeKind.Other)]
    [InlineData(RuntimeNodeKind.Person)]
    [InlineData(RuntimeNodeKind.FrontDoor)]
    [InlineData(RuntimeNodeKind.WebApp)]
    [InlineData(RuntimeNodeKind.Sql)]
    [InlineData(RuntimeNodeKind.StaticSite)]
    [InlineData(RuntimeNodeKind.Dependency)]
    [InlineData(RuntimeNodeKind.ContainerApp)]
    [InlineData(RuntimeNodeKind.Resource)]
    public void EveryKindIsHeldAndEveryKindIsAnotherBox(RuntimeNodeKind kind)
    {
        var node = Full with { Kind = kind };

        Assert.Equal(kind, node.Kind);
        Assert.Equal(
            node,
            new RuntimeNode(
                node.Alias, kind, node.Name, node.Url, node.Deployable, node.Role, node.Region, node.RegionAlias,
                node.HealthCheck, node.DependencyKind, node.Links, node.HealthPath, node.Ids, node.Architecture));
        Assert.True(node == Full with { Kind = kind });
        Assert.True(node != Full with { Kind = kind, Alias = "app_api" });
        Assert.Contains($"Kind = {kind}", node.ToString()!, StringComparison.Ordinal);

        if (kind != RuntimeNodeKind.WebApp)
        {
            Assert.NotEqual(Full, node);
            Assert.Equal(2, new HashSet<RuntimeNode> { Full, node }.Count);
        }
    }

    /// <summary>A kind the dashboard has never heard is a kind the box holds all the same: the record neither requires
    /// a defined value nor turns an unknown one into <see cref="RuntimeNodeKind.Other" /> - the file's word becoming
    /// <c>Other</c> is the reader's, and is pinned there. An unknown kind says its number, at either end of an
    /// <see cref="int" /> as anywhere, and stands apart from every kind the nine name.</summary>
    [Theory]
    [InlineData(9)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void AKindOutsideTheNineIsHeldAsTheNumberItIs(int kind)
    {
        var node = Full with { Kind = (RuntimeNodeKind)kind };

        Assert.Equal((RuntimeNodeKind)kind, node.Kind);
        Assert.False(Enum.IsDefined(node.Kind));
        Assert.NotEqual(RuntimeNodeKind.Other, node.Kind);
        Assert.NotEqual(Full, node);
        Assert.Contains($"Kind = {kind.ToString(CultureInfo.InvariantCulture)}", node.ToString()!, StringComparison.Ordinal);

        // The unknown number is one kind and not two: a box of it is one with another of the same number, and the
        // nine defined kinds are none of them that number.
        Assert.Equal(node, Full with { Kind = (RuntimeNodeKind)kind });
        Assert.Single(new HashSet<RuntimeNode> { node, Full with { Kind = (RuntimeNodeKind)kind } });
        Assert.DoesNotContain(node, Enum.GetValues<RuntimeNodeKind>().Select(defined => Full with { Kind = defined }));
    }

    // ------------------------------------------------------------------ the two addresses and the links

    /// <summary>A box holds the address it was handed and asks nothing of it: the https rule, the absolute rule and
    /// the rule that only a web app and a container app keep an architecture are the reader's, and are pinned there.
    /// A relative address, a clear one and an architecture on a database are all parts a box built by hand carries,
    /// and each says itself as <c>Uri</c> writes it.</summary>
    [Fact]
    public void ABoxKeepsTheAddressesItIsHandedWithoutAskingWhatTheyAreFor()
    {
        var relative = new Uri("_health", UriKind.Relative);
        var holding = Full with { Url = relative };

        Assert.Same(relative, holding.Url);
        Assert.Equal("_health", holding.Url!.ToString());
        Assert.Contains("Url = _health, ", holding.ToString()!, StringComparison.Ordinal);

        var clear = Full with { Url = new Uri("http://app-ui.example.net") };

        Assert.Equal("http://app-ui.example.net/", clear.Url!.AbsoluteUri);
        Assert.Equal("http://app-ui.example.net/", clear.Url.ToString());

        // An architecture of a kind the reader would have refused is kept by the record, and one with a query and a
        // fragment is kept with them.
        Assert.Equal(
            new Uri("https://releases.example.net/runtime?draft=1#two"),
            new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Architecture: new Uri("https://releases.example.net/runtime?draft=1#two")).Architecture);
        Assert.True(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Architecture: Diagrams) != Bare);
    }

    /// <summary>An address is compared as <c>Uri</c> compares one, the box saying nothing of its own about it: the
    /// same site written with another case of the scheme or the host, or with the default port on it, is the same
    /// address and makes one box of two - while another host, another scheme, another port, another path and a query
    /// are four and five other addresses, the path being weighed case-sensitively. The fragment after the <c>#</c> is
    /// the one part <c>Uri</c> itself does not weigh, so a box checked at the same place with a fragment on it is
    /// that box - which matters because the portal's addresses are all fragments, and the record is what compares
    /// them. The rule holds of the checked address and of the architecture alike.</summary>
    [Theory]
    [InlineData("https://app-ui.example.net/_health", true)]
    [InlineData("HTTPS://APP-UI.example.net/_health", true)]
    [InlineData("https://app-ui.example.net:443/_health", true)]
    [InlineData("https://app-ui.example.net/_Health", false)]
    [InlineData("http://app-ui.example.net/_health", false)]
    [InlineData("https://app-ui.example.net:8443/_health", false)]
    [InlineData("https://app-ui.example.io/_health", false)]
    [InlineData("https://app-ui.example.net/_health?x=1", false)]
    [InlineData("https://app-ui.example.net/_health#tiles", true)]
    public void AnAddressIsComparedAsAnAddressIs(string url, bool sameBox) =>
        Assert.Equal(sameBox, Full with { Url = new Uri(url) } == Full);

    [Theory]
    [InlineData("https://releases.example.net:443/runtime", true)]
    [InlineData("HTTPS://RELEASES.example.net/runtime", true)]
    [InlineData("https://releases.example.net/runtime/", false)]
    [InlineData("https://releases.example.net/runtime/1.2", false)]
    [InlineData("https://releases.example.net/runtime#draft", true)]
    [InlineData("http://releases.example.net/runtime", false)]
    public void TheArchitectureIsComparedAsAnAddressIs(string url, bool sameBox) =>
        Assert.Equal(sameBox, Full with { Architecture = new Uri(url) } == Full);

    /// <summary>Nothing is an address of its own, and the box that holds none is one box while the box that holds the
    /// empty address is another: the record opens no address and guesses at none, so a part left out and a part given
    /// as nothing are the one absence, and both stand apart from a part that was handed something.</summary>
    [Fact]
    public void TheAddressThatIsNotThereIsOnePartAndTheAddressThatIsBlankIsAnother()
    {
        Assert.Null(Bare.Url);
        Assert.Null(Bare.Architecture);
        Assert.Equal(Bare with { Url = null }, Bare);
        Assert.NotEqual(Bare, Bare with { Url = new Uri("about:blank") });
        Assert.NotEqual(Bare with { Url = new Uri("about:blank") }, Bare with { Architecture = new Uri("about:blank") });

        var blank = Full with { Url = null, Architecture = null };

        Assert.NotEqual(Full, blank);
        Assert.NotEqual(blank, blank with { Url = Url });
        Assert.NotEqual(blank, blank with { Architecture = Diagrams });
    }

    /// <summary>The links a box holds are compared as the hold they stand on and not as their keys: the very same link
    /// set makes two boxes one, and a second set of the one and the same key and address makes two boxes of them -
    /// the record opening neither. What the six keys of a set lead a box to is <see cref="RuntimeBoxLink" />'s, and
    /// is pinned there.</summary>
    [Fact]
    public void TheLinksABoxHoldsAreComparedAsTheHoldTheyStandOn()
    {
        var one = Full;
        var holdingTheSameLinks = new RuntimeNode(
            "app_ui", RuntimeNodeKind.WebApp, "ca-demo-uat-ui", Url, "ui", "primary", "westus3", "Region_Primary",
            "LlmGateway", "external", OneLink, "/_healthcheck", Full.Ids, Diagrams);
        var holdingOtherLinks = Full with { Links = OtherLink };

        Assert.Same(OneLink, holdingTheSameLinks.Links);
        Assert.Equal(one, holdingTheSameLinks);
        Assert.Equal(one.GetHashCode(), holdingTheSameLinks.GetHashCode());

        Assert.Equal(one.Links!.Keys, holdingOtherLinks.Links!.Keys);
        Assert.Equal(one.Links[LinkSet.Portal], holdingOtherLinks.Links[LinkSet.Portal]);
        Assert.NotSame(one.Links, holdingOtherLinks.Links);
        Assert.True(one != holdingOtherLinks);
        Assert.NotEqual(one, holdingOtherLinks);

        // The links that are not there and the links that are there and hold nothing are two parts.
        Assert.NotEqual(Bare, Bare with { Links = LinkSet.None });
        Assert.Equal(Bare with { Links = LinkSet.None }, Bare with { Links = LinkSet.None });
    }

    // ------------------------------------------------------------------ the ids a box stands for

    /// <summary>The ids a box stands for are kept as the list they were handed - not copied, not opened, not
    /// trimmed, not emptied out of a list that holds nothing. The empty list is a list and not the absence of one, and
    /// two lists of the same ids are two lists, since the record compares the part and not what stands in it.</summary>
    [Fact]
    public void TheIdsAreHeldAsTheListTheyStandOn()
    {
        var ids = new List<string> { "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv", " " };
        var node = new RuntimeNode("vault", RuntimeNodeKind.Resource, "kv-cmdemo1", Ids: ids);

        Assert.Same(ids, node.Ids);
        Assert.Equal(ids, node.Ids);
        Assert.Same(ids, (node with { Alias = "vault_other" }).Ids);
        Assert.False(node.Ids!.SequenceEqual(Full.Ids!));

        Assert.Null(Bare.Ids);
        Assert.Empty((Full with { Ids = [] }).Ids!);
        Assert.True((Full with { Ids = [] }).Ids!.Count == 0);

        // The list with nothing in it is not the list that is not there, and neither is the list of one word that is
        // nothing but a space the list of one empty word.
        Assert.NotEqual(Bare, Bare with { Ids = [] });
        Assert.NotEqual(Full, Full with { Ids = null });
        Assert.NotEqual(new RuntimeNode("v", RuntimeNodeKind.Resource, "kv", Ids: [""]), new RuntimeNode("v", RuntimeNodeKind.Resource, "kv", Ids: [" "]));

        // The very same list makes two boxes one; a second list of the very same ids makes two boxes of them.
        Assert.Equal(node, new RuntimeNode("vault", RuntimeNodeKind.Resource, "kv-cmdemo1", Ids: ids));
        Assert.NotEqual(node, new RuntimeNode("vault", RuntimeNodeKind.Resource, "kv-cmdemo1", Ids: [.. ids]));
        Assert.Equal(
            2,
            new HashSet<RuntimeNode>
            {
                node,
                new RuntimeNode("vault", RuntimeNodeKind.Resource, "kv-cmdemo1", Ids: [.. ids]),
            }.Count);
    }

    // ------------------------------------------------------------------ the record itself

    /// <summary>Two boxes are one while their fourteen parts are, whichever of the ways of asking is used: the
    /// record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, and the hash that
    /// follows them and stands from call to call. A box is compared, not looked through: two calls on the fourteen
    /// same parts are one box, and a set of them holds them once.</summary>
    [Fact]
    public void TwoBoxesAreOneWhileTheirFourteenPartsAre()
    {
        var one = Full;
        var two = new RuntimeNode(
            "app_ui", RuntimeNodeKind.WebApp, "ca-demo-uat-ui", Url, "ui", "primary", "westus3", "Region_Primary",
            "LlmGateway", "external", OneLink, "/_healthcheck", Full.Ids, Diagrams);

        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.True(one.Equals(one));
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<RuntimeNode> { one, two });

        // The parts asked out and put back in make the same box again: the equality is of the values, not of the call
        // that gathered them.
        one.Deconstruct(
            out var alias, out var kind, out var name, out var url, out var deployable, out var role, out var region,
            out var regionAlias, out var healthCheck, out var dependencyKind, out var links, out var healthPath,
            out var ids, out var architecture);

        Assert.Equal(
            one,
            new RuntimeNode(
                alias, kind, name, url, deployable, role, region, regionAlias, healthCheck, dependencyKind, links,
                healthPath, ids, architecture));
        Assert.Equal(
            one.GetHashCode(),
            new RuntimeNode(
                alias, kind, name, url, deployable, role, region, regionAlias, healthCheck, dependencyKind, links,
                healthPath, ids, architecture).GetHashCode());
    }

    /// <summary>Any one of the fourteen parts tells two boxes apart, and nothing is compared loosely: the words
    /// ordinally, so a word in another case or with a space on it is another word, and the empty word, the word of
    /// spaces and the nothing that is no word are three parts of their own. A part put away is as much a change as a
    /// part put on.</summary>
    [Fact]
    public void ABoxDiffersInAnyOneOfItsFourteenParts()
    {
        var node = Full;

        Assert.NotEqual(node, node with { Alias = string.Empty });
        Assert.NotEqual(node, node with { Alias = " " });
        Assert.NotEqual(node, node with { Alias = null! });
        Assert.NotEqual(node, node with { Alias = "APP_UI" });
        Assert.NotEqual(node, node with { Alias = "app_ui " });
        Assert.NotEqual(node, node with { Kind = RuntimeNodeKind.Sql });
        Assert.NotEqual(node, node with { Kind = RuntimeNodeKind.Other });
        Assert.NotEqual(node, node with { Name = string.Empty });
        Assert.NotEqual(node, node with { Name = null! });
        Assert.NotEqual(node, node with { Name = "ca-demo-uat-api" });
        Assert.NotEqual(node, node with { Url = null });
        Assert.NotEqual(node, node with { Url = OtherUrl });
        Assert.NotEqual(node, node with { Deployable = string.Empty });
        Assert.NotEqual(node, node with { Deployable = null });
        Assert.NotEqual(node, node with { Deployable = "UI" });
        Assert.NotEqual(node, node with { Role = "standby" });
        Assert.NotEqual(node, node with { Role = "PRIMARY" });
        Assert.NotEqual(node, node with { Role = null });
        Assert.NotEqual(node, node with { Region = "eastus2" });
        Assert.NotEqual(node, node with { Region = string.Empty });
        Assert.NotEqual(node, node with { RegionAlias = "region_primary" });
        Assert.NotEqual(node, node with { RegionAlias = null });
        Assert.NotEqual(node, node with { HealthCheck = "Mail" });
        Assert.NotEqual(node, node with { HealthCheck = string.Empty });
        Assert.NotEqual(node, node with { DependencyKind = "External" });
        Assert.NotEqual(node, node with { DependencyKind = null });
        Assert.NotEqual(node, node with { Links = null });
        Assert.NotEqual(node, node with { Links = LinkSet.None });
        Assert.NotEqual(node, node with { HealthPath = "/_health" });
        Assert.NotEqual(node, node with { HealthPath = string.Empty });
        Assert.NotEqual(node, node with { HealthPath = null });
        Assert.NotEqual(node, node with { Ids = null });
        Assert.NotEqual(node, node with { Ids = [] });
        Assert.NotEqual(node, node with { Architecture = null });
        Assert.NotEqual(node, node with { Architecture = new Uri("https://releases.example.net/other") });

        // Fourteen parts that differ from this box, one part each, are fourteen other boxes and a fifteenth of them.
        Assert.Equal(
            15,
            new HashSet<RuntimeNode>
            {
                node,
                node with { Alias = "app_api" },
                node with { Kind = RuntimeNodeKind.Dependency },
                node with { Name = "ca-demo-uat-api" },
                node with { Url = OtherUrl },
                node with { Deployable = "api" },
                node with { Role = "standby" },
                node with { Region = "eastus2" },
                node with { RegionAlias = "Region_Standby" },
                node with { HealthCheck = "Mail" },
                node with { DependencyKind = "internal" },
                node with { Links = OtherLink },
                node with { HealthPath = "/alive" },
                node with { Ids = ["/subscriptions/sub"] },
                node with { Architecture = new Uri("https://releases.example.net/other") },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked: two boxes that are no box at all are
    /// one, a box and no box are two - the equality and the inequality each saying it, from either side - and the box
    /// asked about itself says no to the nothing, to the alias it is named by, to the edge of the same diagram and to
    /// the topology's node, which is another record of another five parts. The box of nothing at all is still a
    /// box.</summary>
    [Fact]
    public void NoBoxIsOneWithNoBoxAndTheBoxOfNothingIsABox()
    {
        RuntimeNode? nothing = null;
        RuntimeNode? alsoNothing = null;
        var there = Full;

        Assert.Null(default(RuntimeNode));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((RuntimeNode?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals("app_ui"));
        Assert.False(there.Equals(RuntimeNodeKind.WebApp));
        Assert.False(there.Equals(new RuntimeEdge("app_ui-to-sqldb", "app_ui", "sqldb", RuntimeEdgeKind.Sql)));
        Assert.False(there.Equals(new NodeInfo("ca-demo-uat-ui", "westus3", NodeInfo.PrimaryRole, Url, OneLink)));
        Assert.False(there.Equals(Bare));

        var alsoEmpty = Empty;

        Assert.Equal(Empty, alsoEmpty);
        Assert.NotSame(Empty, alsoEmpty);
        Assert.Equal(Empty.GetHashCode(), alsoEmpty.GetHashCode());
        Assert.True(Empty == alsoEmpty);
        Assert.Equal(
            2,
            new HashSet<RuntimeNode>
            {
                Empty,
                Nothing,
            }.Count);
    }

    /// <summary><c>with</c> names the part it changes and carries the other thirteen: a copy that names nothing is one
    /// with its original in another reference, the original is left where it stood, and an optional part may be put
    /// on and taken off as the parts a caller must name are. A box copied from the box of nothing, named as the box
    /// of nothing is named, is the box of nothing.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var node = Full;
        var copy = node with { };

        Assert.Equal(node, copy);
        Assert.NotSame(node, copy);
        Assert.True(node == copy);

        var moved = node with { Region = "eastus2", RegionAlias = "Region_Standby" };

        Assert.Equal("eastus2", moved.Region);
        Assert.Equal("Region_Standby", moved.RegionAlias);
        Assert.Equal("westus3", node.Region);
        Assert.Equal("Region_Primary", node.RegionAlias);
        Assert.Equal("app_ui", moved.Alias);
        Assert.Same(Url, moved.Url);
        Assert.Same(OneLink, moved.Links);

        // A part put on and a part taken off, the thirteen about them standing where they stood.
        var quiet = node with { HealthPath = null, Ids = null, Architecture = null, Url = null, Links = null };

        Assert.Null(quiet.HealthPath);
        Assert.Null(quiet.Ids);
        Assert.Null(quiet.Architecture);
        Assert.Null(quiet.Url);
        Assert.Null(quiet.Links);
        Assert.Equal("/_healthcheck", node.HealthPath);
        Assert.Same(Url, node.Url);
        Assert.True(node != quiet);

        // A copy that names the parts the box already holds is the box, and a copy of the box of nothing is the box of
        // nothing.
        Assert.Equal(node, node with { Alias = "app_ui", Kind = RuntimeNodeKind.WebApp, Name = "ca-demo-uat-ui", Url = Url });
        Assert.Equal(Nothing, Nothing with { Alias = null!, Kind = default, Name = null!, Url = null, Ids = null });
    }

    /// <summary>The fourteen parts fall out of a box in the order they went in, each as the value it holds, and what
    /// came out goes back in the same way; the parts the box was not told fall out as the nothing they are, and the
    /// box that was told nothing at all is a box all the same.</summary>
    [Fact]
    public void TheFourteenPartsFallOutOfABoxInTheOrderTheyWentIn()
    {
        var (
            alias, kind, name, url, deployable, role, region, regionAlias, healthCheck, dependencyKind, links,
            healthPath, ids, architecture) = Full;

        Assert.Equal("app_ui", alias);
        Assert.Equal(RuntimeNodeKind.WebApp, kind);
        Assert.Equal("ca-demo-uat-ui", name);
        Assert.Same(Url, url);
        Assert.Equal("ui", deployable);
        Assert.Equal("primary", role);
        Assert.Equal("westus3", region);
        Assert.Equal("Region_Primary", regionAlias);
        Assert.Equal("LlmGateway", healthCheck);
        Assert.Equal("external", dependencyKind);
        Assert.Same(OneLink, links);
        Assert.Equal("/_healthcheck", healthPath);
        Assert.Equal(Full.Ids, ids);
        Assert.Same(Diagrams, architecture);

        Assert.Equal(
            Full,
            new RuntimeNode(
                alias, kind, name, url, deployable, role, region, regionAlias, healthCheck, dependencyKind, links,
                healthPath, ids, architecture));

        var bare = Bare;
        bare.Deconstruct(
            out var bareAlias, out var bareKind, out var bareName, out var bareUrl, out _, out _, out _, out _,
            out _, out _, out var bareLinks, out var bareHealthPath, out var bareIds, out var bareArchitecture);

        Assert.Equal("app_ui", bareAlias);
        Assert.Equal(RuntimeNodeKind.WebApp, bareKind);
        Assert.Equal("ca-demo-uat-ui", bareName);
        Assert.Null(bareUrl);
        Assert.Null(bareLinks);
        Assert.Null(bareHealthPath);
        Assert.Null(bareIds);
        Assert.Null(bareArchitecture);
        Assert.Equal(bare, new RuntimeNode(bareAlias, bareKind, bareName, bareUrl, null, null, null, null, null, null, bareLinks, bareHealthPath, bareIds, bareArchitecture));

        // One part asked out of the fourteen, the rest of them left where they are, and the box of nothing falling
        // apart into fourteen nothing.
        var (only, _, _, _, _, _, _, _, _, _, _, _, _, _) = Full;

        Assert.Equal("app_ui", only);

        var (
            nothingAlias, nothingKind, nothingName, nothingUrl, _, _, _, _, _, _, _, _, nothingIds,
            nothingArchitecture) = Nothing;

        Assert.Null(nothingAlias);
        Assert.Equal(RuntimeNodeKind.Other, nothingKind);
        Assert.Null(nothingName);
        Assert.Null(nothingUrl);
        Assert.Null(nothingIds);
        Assert.Null(nothingArchitecture);
    }

    /// <summary>The words a box says about itself - its name, then its fourteen parts in the order they are declared,
    /// each named and standing blank when there is nothing of it - are the record's own and not the page's: the page
    /// is given its words by the payload builder. An address says itself as <c>Uri</c> writes it, a kind as the enum
    /// names it and a list as the type it stands on, the line never opening any of them.</summary>
    [Fact]
    public void ABoxSaysItsFourteenPartsInTheirOrder()
    {
        Assert.Equal(
            "RuntimeNode { Alias = app_ui, Kind = WebApp, Name = ca-demo-uat-ui, "
                + "Url = https://app-ui.example.net/_health, Deployable = ui, Role = primary, Region = westus3, "
                + "RegionAlias = Region_Primary, HealthCheck = LlmGateway, DependencyKind = external, "
                + "Links = Dashboard.Health.LinkSet, HealthPath = /_healthcheck, "
                + "Ids = System.Collections.Generic.List`1[System.String], "
                + "Architecture = https://releases.example.net/runtime }",
            Full.ToString());

        // The eleven parts a caller may leave out are named and say nothing.
        Assert.Equal(
            "RuntimeNode { Alias = app_ui, Kind = WebApp, Name = ca-demo-uat-ui, Url = , Deployable = , Role = , "
                + "Region = , RegionAlias = , HealthCheck = , DependencyKind = , Links = , HealthPath = , Ids = , "
                + "Architecture =  }",
            Bare.ToString());

        Assert.Equal(
            "RuntimeNode { Alias = , Kind = Other, Name = , Url = , Deployable = , Role = , Region = , "
                + "RegionAlias = , HealthCheck = , DependencyKind = , Links = , HealthPath = , Ids = , "
                + "Architecture =  }",
            Nothing.ToString());
        Assert.Equal(
            "RuntimeNode { Alias = , Kind = Other, Name = , Url = , Deployable = , Role = , Region = , "
                + "RegionAlias = , HealthCheck = , DependencyKind = , Links = , HealthPath = , Ids = , "
                + "Architecture =  }",
            Empty.ToString());

        // A kind outside the nine says its number, and no part is put in quotes - not the word of markup, not the
        // address, not the list.
        Assert.Contains("Kind = 99, ", (Full with { Kind = (RuntimeNodeKind)99 }).ToString()!, StringComparison.Ordinal);
        Assert.StartsWith("RuntimeNode { Alias = <img src=x onerror=alert(1)>, ", (Full with { Alias = "<img src=x onerror=alert(1)>" }).ToString()!, StringComparison.Ordinal);
        Assert.Contains("Ids = System.String[], ", (Full with { Ids = OneId }).ToString()!, StringComparison.Ordinal);
    }

    /// <summary>A box's words are fourteen of them, and not one number a reader spells: no part of a box is counted
    /// out, so a reader of German is told the very same line as the invariant reader - the kind that is outside the
    /// nine with its bare number, the address with its host and its path - which is what a log of one of these boxes
    /// may count on whatever the reader of the machine happens to be.</summary>
    [Fact]
    public void TheWordsOfABoxAreTheSameWhateverTheReaderSpeaks()
    {
        var unknown = Full with { Kind = (RuntimeNodeKind)12345 };

        ViewerCulture.German(() =>
        {
            foreach (var node in new[] { Full, Bare, Empty, Nothing, unknown })
            {
                Assert.Equal(Words(node), node.ToString());
            }
        });

        // The number of an unknown kind is said without a separator, and the whole line the German reader got is the
        // line the test above pins.
        Assert.StartsWith(
            "RuntimeNode { Alias = app_ui, Kind = 12345, Name = ca-demo-uat-ui, ",
            Words(unknown),
            StringComparison.Ordinal);
        Assert.StartsWith("RuntimeNode { Alias = app_ui, Kind = WebApp, ", Words(Full), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the record's own shape

    /// <summary>The box's whole public surface as it stands: its fourteen parts, named in the order the constructor
    /// names them and of the type the constructor says, the three a caller must name and the eleven it may leave out
    /// as nothing, and the members every record is asked - falling apart, comparing, hashing and saying itself - and
    /// nothing else to call. There is no verdict on a box to read and none to set: what a box means is the kind and
    /// the parts of that kind, and the builder decides from them. A fifteenth part added to the record has to be
    /// written down here as well, which is the point: a box of a diagram cannot carry a part nobody decided what it
    /// means.</summary>
    [Fact]
    public void TheBoxHasFourteenPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            [
                "Alias", "Kind", "Name", "Url", "Deployable", "Role", "Region", "RegionAlias", "HealthCheck",
                "DependencyKind", "Links", "HealthPath", "Ids", "Architecture",
            ],
            [.. typeof(RuntimeNode)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [
                "Alias", "Architecture", "Deconstruct", "DependencyKind", "Deployable", "Equals", "GetHashCode",
                "HealthCheck", "HealthPath", "Ids", "Kind", "Links", "Name", "Region", "RegionAlias", "Role",
                "ToString", "Url", "op_Equality", "op_Inequality",
            ],
            PublicSurface());

        var properties = typeof(RuntimeNode)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Alias"].PropertyType);
        Assert.Equal(typeof(RuntimeNodeKind), properties["Kind"].PropertyType);
        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Url"].PropertyType);
        Assert.Equal(typeof(string), properties["Deployable"].PropertyType);
        Assert.Equal(typeof(string), properties["Role"].PropertyType);
        Assert.Equal(typeof(string), properties["Region"].PropertyType);
        Assert.Equal(typeof(string), properties["RegionAlias"].PropertyType);
        Assert.Equal(typeof(string), properties["HealthCheck"].PropertyType);
        Assert.Equal(typeof(string), properties["DependencyKind"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);
        Assert.Equal(typeof(string), properties["HealthPath"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), properties["Ids"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Architecture"].PropertyType);

        Assert.All(
            properties.Values,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.True(property.CanWrite);
            });

        // The one public constructor, naming its fourteen parts as the properties are named; the copy constructor the
        // compiler writes for a record is not one a caller outside the record calls.
        var constructors = typeof(RuntimeNode).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(
            [
                "Alias", "Kind", "Name", "Url", "Deployable", "Role", "Region", "RegionAlias", "HealthCheck",
                "DependencyKind", "Links", "HealthPath", "Ids", "Architecture",
            ],
            [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [false, false, false, true, true, true, true, true, true, true, true, true, true, true],
            [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.All(parameters.Skip(3), parameter => Assert.Null(parameter.DefaultValue));

        // The nullable annotations stand as the record declares them: the eleven a caller may leave out may be
        // nothing and the three it must name may not - which is what the reader of the file leans on, and what the
        // record itself does not enforce (a caller may still hand it the nothing of a word, pinned above).
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false, true, true, true, true, true, true, true, true, true, true, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        Assert.Equal(
            [
                typeof(string), typeof(RuntimeNodeKind), typeof(string), typeof(Uri), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(LinkSet), typeof(string),
                typeof(IReadOnlyList<string>), typeof(Uri),
            ],
            [.. parameters.Select(parameter => parameter.ParameterType)]);

        List<PropertyInfo> parts =
        [
            properties["Alias"], properties["Kind"], properties["Name"], properties["Url"], properties["Deployable"],
            properties["Role"], properties["Region"], properties["RegionAlias"], properties["HealthCheck"],
            properties["DependencyKind"], properties["Links"], properties["HealthPath"], properties["Ids"],
            properties["Architecture"],
        ];

        Assert.Equal(
            [false, false, false, true, true, true, true, true, true, true, true, true, true, true],
            [.. parts.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        Assert.Equal([typeof(IEquatable<RuntimeNode>)], typeof(RuntimeNode).GetInterfaces());
        Assert.True(typeof(RuntimeNode).IsSealed);
        Assert.Equal(typeof(object), typeof(RuntimeNode).BaseType);
        Assert.Empty(typeof(RuntimeNode).GetEvents());
        Assert.Empty(typeof(RuntimeNode).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(RuntimeNode).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    /// <summary>The nine kinds a box's <c>Kind</c> may be told, as the enum declares them: in the order they are
    /// declared, <see cref="RuntimeNodeKind.Other" /> first and so the kind a box with the part left out stands on,
    /// each with the number of its place. Those numbers are what a log of a box says of a kind the dashboard has
    /// never heard, and what these tests write when they mean one; the words the manifest spells and the switch that
    /// turns them into these nine are pinned by <see cref="RuntimeManifestParserEdgeCaseTests" />.</summary>
    [Fact]
    public void TheNineKindsAreTheNineAndOtherIsTheUnsetOne()
    {
        Assert.Equal(
            ["Other", "Person", "FrontDoor", "WebApp", "Sql", "StaticSite", "Dependency", "ContainerApp", "Resource"],
            Enum.GetNames<RuntimeNodeKind>());
        Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7, 8], Enum.GetValues<RuntimeNodeKind>().Select(kind => (int)kind));
        Assert.Equal(9, Enum.GetValues<RuntimeNodeKind>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(RuntimeNodeKind)));
        Assert.Equal(0, (int)default(RuntimeNodeKind));
        Assert.True(Enum.IsDefined(RuntimeNodeKind.Other));
        Assert.False(Enum.IsDefined((RuntimeNodeKind)9));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The words a box says about itself, taken in the invariant culture whatever the reader of this test
    /// happens to be: no part of a box is a number a reader spells, so these are the words every reader is
    /// given.</summary>
    private static string Words(RuntimeNode node) => ViewerCulture.Invariant(() => node.ToString()!);

    /// <summary>The public members the box declares itself: its parts and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeNode)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
