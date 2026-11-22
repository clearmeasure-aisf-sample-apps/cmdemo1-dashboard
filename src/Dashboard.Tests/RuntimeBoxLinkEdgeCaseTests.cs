namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeBoxLink" />: the four keys of the manifest as
/// the words the deployment spells, the three reads of a frame, a region and a node, and what each says when the
/// links it is handed are none, empty, spelled another way, hold no address, hold another element's keys, or hold
/// several of the six at once. What the sample diagrams do with these links is pinned in
/// <see cref="RuntimeBoxLinkTests" />, and the words of one link of each kind of box in
/// <see cref="RuntimeWorkHelperCoverageTests" />; what they leave open is pinned here: that only the portal key says
/// what the element <em>is</em> while the other five say its <em>name</em>, that the order of the six rules a region
/// and a node as it rules a frame, that a kind is read exactly as it is spelled, that an empty name is said as
/// nothing, that the href is the whole address as <c>Uri</c> writes it while the title says the host alone, and that
/// this read asks nothing of an address it is handed - the https rule lives in the parser, and a link built by hand
/// is linked whatever its scheme. The suffix of a browser box, <see cref="RuntimeBoxLink.NewTabSuffix" />, is the
/// one constant of the class the tests above already pin to its words, so it is only used here, never re-pinned.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a box link is two strings out of a link set,
/// and every address is one the test brings.
/// </para>
/// </summary>
public class RuntimeBoxLinkEdgeCaseTests
{
    /// <summary>The words the app appends - written out here rather than taken from the app, so a changed suffix
    /// breaks a test instead of quietly moving along with it.</summary>
    private const string PortalHint = " (opens in a new tab; the Azure portal asks you to sign in)";

    private const string OctopusHint = " (opens in a new tab; Octopus Deploy asks you to sign in)";
    private const string NewTabHint = " (opens in a new tab)";

    private static readonly Uri Portal = new("https://portal.example.net/#/overview");
    private static readonly Uri Graph = new("https://portal.example.net/#blade/HubsExtension/ArgQueryBlade/query/");
    private static readonly Uri Namespaces = new("https://portal.example.net/#blade/HubBlade/namespaces");
    private static readonly Uri Workloads = new("https://portal.example.net/#blade/HubBlade/workloads");
    private static readonly Uri Octopus = new("https://octopus.example.net/app#/Spaces-1/projects/demo");
    private static readonly Uri Site = new("https://demo.example.io/");

    // ----- The four keys -----

    /// <summary>The four keys the read keeps of its own, as the words the manifest spells them in: a rename of one
    /// of these constants would leave every box of a diagram that is no region with no link at all, and the
    /// deployment writes the manifest, not this code.</summary>
    [Fact]
    public void TheFourKeysAreTheWordsTheManifestSpellsThemIn()
    {
        Assert.Equal("resources", RuntimeBoxLink.Resources);
        Assert.Equal("namespaces", RuntimeBoxLink.Namespaces);
        Assert.Equal("project", RuntimeBoxLink.Project);
        Assert.Equal("site", RuntimeBoxLink.Site);

        var keys = new[] { RuntimeBoxLink.Resources, RuntimeBoxLink.Namespaces, RuntimeBoxLink.Project, RuntimeBoxLink.Site };

        Assert.Equal(4, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.All(keys, key => Assert.False(string.IsNullOrWhiteSpace(key)));
        // None of the four is a key of another element's links: the six the read keeps are apart.
        Assert.DoesNotContain(LinkSet.Portal, keys);
        Assert.DoesNotContain(LinkSet.Workloads, keys);
    }

    // ----- The three reads and an element that is not there -----

    /// <summary>An element that is not there is refused by the name of the part that is not there - the frame, the
    /// region, the node - since the three reads differ by nothing else.</summary>
    [Fact]
    public void EachReadRefusesTheElementThatIsNotThereAndSaysWhichItWas()
    {
        RuntimeFrame? frame = null;
        RuntimeRegion? region = null;
        RuntimeNode? node = null;

        Assert.Equal("frame", Assert.Throws<ArgumentNullException>(() => RuntimeBoxLink.Of(frame!)).ParamName);
        Assert.Equal("region", Assert.Throws<ArgumentNullException>(() => RuntimeBoxLink.Of(region!)).ParamName);
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => RuntimeBoxLink.Of(node!)).ParamName);
    }

    // ----- No links, empty links, a key that holds no address -----

    /// <summary>A link set that is not there is no link, and so is one that is there and holds nothing: the box
    /// stays plain text either way, for a frame, a region and a node alike.</summary>
    [Fact]
    public void LinksThatAreNotThereAndLinksThatHoldNothingLeadNowhere()
    {
        Assert.Null(RuntimeBoxLink.Of(new RuntimeFrame("plan_primary", "plan", "asp-cmdemo1", LinkSet.None)));
        Assert.Null(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", [], LinkSet.None)));
        Assert.Null(RuntimeBoxLink.Of(new RuntimeNode("vault", RuntimeNodeKind.Resource, "kv-cmdemo1", Links: LinkSet.None)));
    }

    /// <summary>A key of the six that stands with no address behind it is no link, and the read goes on to the next
    /// of the six rather than stopping at it: the first key that holds an address is the box's link, however many
    /// before it hold nothing.</summary>
    [Fact]
    public void AKeyThatHoldsNoAddressIsPassedOverForTheNextOfTheSix()
    {
        var nothing = new LinkSet([new KeyValuePair<string, Uri>(LinkSet.Portal, null!)]);
        Assert.Null(RuntimeBoxLink.Of(new RuntimeFrame("plan_primary", "plan", "asp-cmdemo1", nothing)));

        var partly = new LinkSet(
        [
            new KeyValuePair<string, Uri>(LinkSet.Portal, null!),
            new KeyValuePair<string, Uri>(RuntimeBoxLink.Resources, null!),
            new KeyValuePair<string, Uri>(RuntimeBoxLink.Namespaces, null!),
            new KeyValuePair<string, Uri>(LinkSet.Workloads, null!),
            new KeyValuePair<string, Uri>(RuntimeBoxLink.Project, Octopus),
            new KeyValuePair<string, Uri>(RuntimeBoxLink.Site, null!),
        ]);

        Assert.Equal(
            (Octopus.AbsoluteUri, $"The project that deploys asp-cmdemo1 in Octopus Deploy{OctopusHint}"),
            LinkOf(RuntimeBoxLink.Of(new RuntimeFrame("plan_primary", "plan", "asp-cmdemo1", partly))));
    }

    // ----- The keys the read keeps no link of, and the spelling of them -----

    /// <summary>A box whose only link is another element's leads nowhere: the read keeps six keys and no others, and
    /// it keeps them as they are spelled - a key of another casing, of another word, or with a space on it, is no
    /// key of a box link, for a frame, a region and a node alike.</summary>
    [Theory]
    [InlineData(LinkSet.LiveMetrics)]
    [InlineData(LinkSet.Performance)]
    [InlineData(LinkSet.Failures)]
    [InlineData(LinkSet.Dependencies)]
    [InlineData(LinkSet.FrontDoor)]
    [InlineData(LinkSet.Logs)]
    [InlineData(LinkSet.ApplicationInsights)]
    [InlineData(LinkSet.ApplicationMap)]
    [InlineData(LinkSet.Database)]
    [InlineData(LinkSet.ResourceGroup)]
    [InlineData("")]
    [InlineData("Portals")]
    [InlineData("RESOURCES")]
    [InlineData("Namespaces")]
    [InlineData("PROJECT")]
    [InlineData("Site")]
    [InlineData("site ")]
    [InlineData(" namespaces")]
    public void AKeyOfTheSixTheReadKeepsIsNoLinkForAnyElement(string key)
    {
        Assert.Null(RuntimeBoxLink.Of(new RuntimeFrame("rg_tier", "resourceGroup", "rg-cmdemo1", Links((key, Portal)))));
        Assert.Null(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], Links((key, Portal)))));
        Assert.Null(RuntimeBoxLink.Of(new RuntimeNode("app_web", RuntimeNodeKind.WebApp, "ca-web", Links: Links((key, Portal)))));
    }

    // ----- Which words each key says -----

    /// <summary>Only the portal key says what the element <em>is</em>; the other five say its <em>name</em>. A
    /// subscription's frame has words of its own for the portal, and none of them for the rest: the resources, the
    /// namespaces, the workloads, the project and the address are all said with the name as it is drawn.</summary>
    [Fact]
    public void OnlyThePortalLinkSaysWhatTheElementIsTheOtherFiveSayItsName()
    {
        var frame = new RuntimeFrame("sub", "subscription", "sub-cmdemo1", Links(
            (RuntimeBoxLink.Resources, Graph),
            (RuntimeBoxLink.Namespaces, Namespaces),
            (LinkSet.Workloads, Workloads),
            (RuntimeBoxLink.Project, Octopus),
            (RuntimeBoxLink.Site, Site)));

        Assert.Equal("The Azure subscription in the Azure portal" + PortalHint, TitleOf(RuntimeBoxLink.Of(frame with { Links = Links((LinkSet.Portal, Portal)) })));
        Assert.Equal("The resources in sub-cmdemo1, a query in Azure Resource Graph Explorer" + PortalHint, TitleOf(RuntimeBoxLink.Of(frame with { Links = Links((RuntimeBoxLink.Resources, Graph)) })));
        Assert.Equal("The namespaces of the cluster in the Azure portal: sub-cmdemo1 is one of them" + PortalHint, TitleOf(RuntimeBoxLink.Of(frame with { Links = Links((RuntimeBoxLink.Namespaces, Namespaces)) })));
        Assert.Equal("The workloads of the cluster in the Azure portal: sub-cmdemo1 is one of them" + PortalHint, TitleOf(RuntimeBoxLink.Of(frame with { Links = Links((LinkSet.Workloads, Workloads)) })));
        Assert.Equal("The project that deploys sub-cmdemo1 in Octopus Deploy" + OctopusHint, TitleOf(RuntimeBoxLink.Of(frame with { Links = Links((RuntimeBoxLink.Project, Octopus)) })));
        Assert.Equal("sub-cmdemo1: demo.example.io, as a browser opens it" + NewTabHint, TitleOf(RuntimeBoxLink.Of(frame with { Links = Links((RuntimeBoxLink.Site, Site)) })));
    }

    /// <summary>A region and a node lead by every key of the six, not only by the ones the sample diagrams give
    /// them: the read is one rule for all three kinds of element, so the workloads and the project of a region and
    /// the resources, the namespaces and the project of a node are linked and worded as a frame's are.</summary>
    [Fact]
    public void ARegionAndANodeLeadByEveryKeyOfTheSixAsAFrameDoes()
    {
        Assert.Equal(
            ("https://portal.example.net/#blade/HubBlade/workloads", "The workloads of the cluster in the Azure portal: centralus is one of them" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], Links((LinkSet.Workloads, Workloads))))));
        Assert.Equal(
            (Octopus.AbsoluteUri, "The project that deploys centralus in Octopus Deploy" + OctopusHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], Links((RuntimeBoxLink.Project, Octopus))))));

        Assert.Equal(
            ("https://portal.example.net/#blade/HubsExtension/ArgQueryBlade/query/", "The resources in demo-tdd/db, a query in Azure Resource Graph Explorer" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Links: Links((RuntimeBoxLink.Resources, Graph))))));
        Assert.Equal(
            ("https://portal.example.net/#blade/HubBlade/namespaces", "The namespaces of the cluster in the Azure portal: demo-tdd/db is one of them" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Links: Links((RuntimeBoxLink.Namespaces, Namespaces))))));
        Assert.Equal(
            (Octopus.AbsoluteUri, "The project that deploys demo-tdd/db in Octopus Deploy" + OctopusHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Links: Links((RuntimeBoxLink.Project, Octopus))))));
        Assert.Equal(
            (Site.AbsoluteUri, "kv-cmdemo1: demo.example.io, as a browser opens it" + NewTabHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("vault", RuntimeNodeKind.Resource, "kv-cmdemo1", Links: Links((RuntimeBoxLink.Site, Site))))));
    }

    // ----- The order of the six, for every kind of element -----

    /// <summary>The most specific place first, for a region and for a node as for a frame: the whole ladder walked
    /// down on a region's links, and the rungs a region's diagram never shows walked on a node's.</summary>
    [Fact]
    public void TheFirstOfTheSixARegionOrANodeHasWinsOverTheRestOfThem()
    {
        var all = Links(
            (LinkSet.Portal, Portal),
            (RuntimeBoxLink.Resources, Graph),
            (RuntimeBoxLink.Namespaces, Namespaces),
            (LinkSet.Workloads, Workloads),
            (RuntimeBoxLink.Project, Octopus),
            (RuntimeBoxLink.Site, Site));

        Assert.Equal(Portal.AbsoluteUri, HrefOf(RuntimeBoxLink.Of(new RuntimeRegion("r", "centralus", ["primary"], all))));
        Assert.Equal(Portal.AbsoluteUri, HrefOf(RuntimeBoxLink.Of(new RuntimeNode("n", RuntimeNodeKind.Other, "ca-web", Links: all))));

        Assert.Equal(
            Graph.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeRegion("r", "centralus", ["primary"], Links((RuntimeBoxLink.Resources, Graph), (RuntimeBoxLink.Project, Octopus), (RuntimeBoxLink.Site, Site))))));
        Assert.Equal(
            Namespaces.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeRegion("r", "centralus", ["primary"], Links((RuntimeBoxLink.Namespaces, Namespaces), (LinkSet.Workloads, Workloads))))));
        Assert.Equal(
            Workloads.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeRegion("r", "centralus", ["primary"], Links((LinkSet.Workloads, Workloads), (RuntimeBoxLink.Project, Octopus))))));
        Assert.Equal(
            Octopus.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeRegion("r", "centralus", ["primary"], Links((RuntimeBoxLink.Project, Octopus), (RuntimeBoxLink.Site, Site))))));

        Assert.Equal(
            Namespaces.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeNode("n", RuntimeNodeKind.Sql, "demo/db", Links: Links((RuntimeBoxLink.Namespaces, Namespaces), (RuntimeBoxLink.Project, Octopus), (RuntimeBoxLink.Site, Site))))));
        Assert.Equal(
            Workloads.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeNode("n", RuntimeNodeKind.Sql, "demo/db", Links: Links((LinkSet.Workloads, Workloads), (RuntimeBoxLink.Site, Site))))));
        Assert.Equal(
            Site.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeNode("n", RuntimeNodeKind.Sql, "demo/db", Links: Links((RuntimeBoxLink.Site, Site))))));
    }

    // ----- The kind of a frame, read as it is spelled -----

    /// <summary>A frame's kind is read as it is spelled: the empty string the parser gives a frame that names no
    /// kind, and a kind of another casing or with a space on it, are all unknown to the words, and the frame is
    /// linked with its name alone - a link all the same.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("Subscription")]
    [InlineData("resourcegroup")]
    [InlineData(" plan")]
    [InlineData("clusters")]
    [InlineData("frontdoorprofile")]
    public void AFrameOfAKindTheWordsDoNotKnowIsLinkedWithItsNameAlone(string kind) =>
        Assert.Equal(
            "rg-cmdemo1-nonprod in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("rg_tier", kind, "rg-cmdemo1-nonprod", Links((LinkSet.Portal, Portal))))));

    /// <summary>The six node kinds the words name nothing for are linked with their name alone, as the unknown frame
    /// kinds are: only a database and a Static Web App are said by what they are, and those two words are pinned
    /// with the sample diagrams.</summary>
    [Theory]
    [InlineData(RuntimeNodeKind.Other)]
    [InlineData(RuntimeNodeKind.Person)]
    [InlineData(RuntimeNodeKind.FrontDoor)]
    [InlineData(RuntimeNodeKind.WebApp)]
    [InlineData(RuntimeNodeKind.Dependency)]
    [InlineData(RuntimeNodeKind.ContainerApp)]
    [InlineData(RuntimeNodeKind.Resource)]
    public void ANodeTheWordsNameNothingForIsLinkedWithItsNameAlone(RuntimeNodeKind kind) =>
        Assert.Equal(
            "ca-web in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeNode("app_web", kind, "ca-web", Links: Links((LinkSet.Portal, Portal))))));

    // ----- An empty name -----

    /// <summary>A name that is the empty string is said as nothing and the words around it stay as they are, with
    /// the space the sentence puts before it: the read invents no placeholder and refuses nothing, since a box of
    /// the diagram without a name is still a box that leads somewhere.</summary>
    [Fact]
    public void ANameThatIsTheEmptyStringIsSaidAsNothingAndTheWordsStay()
    {
        Assert.Equal(
            "The App Service plan  in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("plan_primary", "plan", string.Empty, Links((LinkSet.Portal, Portal))))));
        Assert.Equal(
            "The Azure subscription in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("sub", "subscription", string.Empty, Links((LinkSet.Portal, Portal))))));
        Assert.Equal(
            "The resources in , a query in Azure Resource Graph Explorer" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeRegion("r", string.Empty, [], Links((RuntimeBoxLink.Resources, Graph))))));
        Assert.Equal(
            "The project that deploys  in Octopus Deploy" + OctopusHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeRegion("r", string.Empty, [], Links((RuntimeBoxLink.Project, Octopus))))));
        Assert.Equal(
            ": demo.example.io, as a browser opens it" + NewTabHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeNode("n", RuntimeNodeKind.Other, string.Empty, Links: Links((RuntimeBoxLink.Site, Site))))));
    }

    // ----- The address: the href, the host, and what the read does not ask -----

    /// <summary>The href is the whole address as <c>Uri</c> writes it - the host lowercased, the root path put on,
    /// the port, the query and the fragment kept - while the words of a browser's box say the host alone, without
    /// the port, the path or the query the box actually leads to.</summary>
    [Fact]
    public void TheHrefIsTheWholeAddressWhileTheTitleSaysTheHostAlone()
    {
        var written = new Uri("https://Ca-Web.Example.IO:8443/a/b?x=1#frag");

        Assert.Equal(
            ("https://ca-web.example.io:8443/a/b?x=1#frag", "ca-web: ca-web.example.io, as a browser opens it" + NewTabHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("app_web", RuntimeNodeKind.WebApp, "ca-web", Links: Links((RuntimeBoxLink.Site, written))))));

        // The root path is the one thing a bare address is given, and the words of the portal's box keep the resource.
        Assert.Equal(
            ("https://portal.example.net/", "The AKS cluster aks-demo in the Azure portal" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeFrame("cluster", "cluster", "aks-demo", Links((LinkSet.Portal, new Uri("https://portal.example.net")))))));
    }

    /// <summary>This read asks nothing of the scheme of an address it is handed: the https rule is the manifest's,
    /// and it is pinned there. A link set built by hand and given to a box leads where it says, over http or over
    /// any other scheme a <c>Uri</c> carries - the host of a browser's box is said whatever stands before it.</summary>
    [Fact]
    public void TheReadAsksNothingOfTheSchemeOfAnAddressItIsHanded()
    {
        Assert.Equal(
            ("http://portal.example.net/#/overview", "The Azure subscription in the Azure portal" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeFrame("sub", "subscription", "sub-cmdemo1", Links((LinkSet.Portal, new Uri("http://portal.example.net/#/overview")))))));
        Assert.Equal(
            ("ftp://files.example.net/pub", "ca-web: files.example.net, as a browser opens it" + NewTabHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("app_web", RuntimeNodeKind.WebApp, "ca-web", Links: Links((RuntimeBoxLink.Site, new Uri("ftp://files.example.net/pub")))))));
    }

    // ----- What the kind says, and only for the portal -----

    /// <summary>The words a kind has of its own are the portal's alone: a plan, a cluster and a Front Door profile
    /// that lead to their project, and a Static Web App that leads to its resources or to its site, are said by
    /// their name as it is drawn, as every element of a kind the words do not know is.</summary>
    [Fact]
    public void TheWordsOfAKindAreThePortalsAloneAndTheOtherKeysSayTheName()
    {
        Assert.Equal(
            "The project that deploys asp-cmdemo1 in Octopus Deploy" + OctopusHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("plan_primary", "plan", "asp-cmdemo1", Links((RuntimeBoxLink.Project, Octopus))))));
        Assert.Equal(
            "The namespaces of the cluster in the Azure portal: aks-demo is one of them" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("cluster", "cluster", "aks-demo", Links((RuntimeBoxLink.Namespaces, Namespaces))))));
        Assert.Equal(
            "fd-cmdemo1: demo.example.io, as a browser opens it" + NewTabHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("afd", "frontDoorProfile", "fd-cmdemo1", Links((RuntimeBoxLink.Site, Site))))));
        Assert.Equal(
            "The resources in swa-demo, a query in Azure Resource Graph Explorer" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeNode("swa", RuntimeNodeKind.StaticSite, "swa-demo", Links: Links((RuntimeBoxLink.Resources, Graph))))));
        Assert.Equal(
            "swa-demo: demo.example.io, as a browser opens it" + NewTabHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeNode("swa", RuntimeNodeKind.StaticSite, "swa-demo", Links: Links((RuntimeBoxLink.Site, Site))))));
    }

    // ----- The order the links were written in -----

    /// <summary>The order of the six is the read's, not the manifest's: links written with the least specific
    /// first still lead a frame, a region and a node to the most specific place they have.</summary>
    [Fact]
    public void TheOrderTheLinksWereWrittenInIsNotTheOrderTheyAreRead()
    {
        var backwards = Links(
            (RuntimeBoxLink.Site, Site),
            (RuntimeBoxLink.Project, Octopus),
            (LinkSet.Workloads, Workloads),
            (RuntimeBoxLink.Namespaces, Namespaces),
            (RuntimeBoxLink.Resources, Graph),
            (LinkSet.Portal, Portal));

        Assert.Equal(Portal.AbsoluteUri, HrefOf(RuntimeBoxLink.Of(new RuntimeFrame("f", "plan", "asp", backwards))));
        Assert.Equal(Portal.AbsoluteUri, HrefOf(RuntimeBoxLink.Of(new RuntimeRegion("r", "centralus", [], backwards))));
        Assert.Equal(Portal.AbsoluteUri, HrefOf(RuntimeBoxLink.Of(new RuntimeNode("n", RuntimeNodeKind.Sql, "db", Links: backwards))));
        Assert.Equal(
            Graph.AbsoluteUri,
            HrefOf(RuntimeBoxLink.Of(new RuntimeRegion("r", "centralus", [], Links((RuntimeBoxLink.Site, Site), (RuntimeBoxLink.Resources, Graph))))));
    }

    // ----- A name said as it is drawn -----

    /// <summary>A name is said exactly as it is drawn: spaces around it are kept, and characters that mean
    /// something in markup or in a sentence are neither escaped nor dropped - the title is text, and the page that
    /// shows it does the escaping.</summary>
    [Theory]
    [InlineData(" web ")]
    [InlineData("<b>web</b>")]
    [InlineData("web & api: \"one\"")]
    [InlineData("ünïcødé-név")]
    public void ANameIsSaidExactlyAsItIsDrawn(string name)
    {
        Assert.Equal($"{name} in the Azure portal" + PortalHint, TitleOf(RuntimeBoxLink.Of(new RuntimeRegion("r", name, [], Links((LinkSet.Portal, Portal))))));
        Assert.Equal($"The database {name} in the Azure portal" + PortalHint, TitleOf(RuntimeBoxLink.Of(new RuntimeNode("n", RuntimeNodeKind.Sql, name, Links: Links((LinkSet.Portal, Portal))))));
        Assert.Equal($"The AKS cluster {name} in the Azure portal" + PortalHint, TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("f", "cluster", name, Links((LinkSet.Portal, Portal))))));
    }

    // ----- The host of an address that is no name -----

    /// <summary>A browser's box says the host as <c>Uri</c> gives it, also when the host is no name: an IPv4 address
    /// without its port, an IPv6 address in its brackets, and a host of one label.</summary>
    [Theory]
    [InlineData("https://10.0.0.4:8443/health", "https://10.0.0.4:8443/health", "10.0.0.4")]
    [InlineData("https://[::1]/", "https://[::1]/", "[::1]")]
    [InlineData("https://localhost", "https://localhost/", "localhost")]
    public void ABrowsersBoxSaysAHostThatIsNoNameAsUriGivesIt(string address, string href, string host) =>
        Assert.Equal(
            (href, $"ca-web: {host}, as a browser opens it" + NewTabHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("app_web", RuntimeNodeKind.WebApp, "ca-web", Links: Links((RuntimeBoxLink.Site, new Uri(address)))))));

    // ----- The same box, read twice -----

    /// <summary>The read keeps nothing between calls: the same box read twice gives two links equal in value, and
    /// the link set it reads is left as it was handed.</summary>
    [Fact]
    public void TheSameBoxReadTwiceGivesTheSameLinkAndLeavesItsLinksAsTheyWere()
    {
        var links = Links((RuntimeBoxLink.Project, Octopus), (RuntimeBoxLink.Site, Site));
        var frame = new RuntimeFrame("own_web", "runtime", "web", links);

        Assert.Equal(RuntimeBoxLink.Of(frame), RuntimeBoxLink.Of(frame));
        Assert.Equal(2, links.Count);
        Assert.Equal([RuntimeBoxLink.Project, RuntimeBoxLink.Site], links.Keys.Order(StringComparer.Ordinal));
    }

    // ----- The links a test gives a box of the diagram -----

    private static LinkSet Links(params (string Key, Uri Address)[] links) =>
        new(links.Select(link => new KeyValuePair<string, Uri>(link.Key, link.Address)));

    /// <summary>The href and the title of the link a box leads to - refusing a box that leads nowhere, so a test
    /// cannot pass on a link the rule never read.</summary>
    private static (string Href, string Title) LinkOf(RuntimeLink? link)
    {
        var value = Assert.IsType<RuntimeLink>(link);
        return (value.Href, value.Title);
    }

    private static string TitleOf(RuntimeLink? link) => LinkOf(link).Title;

    private static string HrefOf(RuntimeLink? link) => LinkOf(link).Href;
}
