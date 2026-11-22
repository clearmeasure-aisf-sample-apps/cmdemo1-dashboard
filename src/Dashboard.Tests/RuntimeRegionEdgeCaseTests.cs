using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeRegion" />: one region of an environment's
/// runtime diagram, the two words that say which region it is and what Azure calls it, the list of the roles its web
/// apps are given, and the links its name may lead to. The record decides nothing of its own - no verdict, no words
/// for the page, no moment, no address. What is pinned here is that each part comes back as the part it went in as,
/// that the record refuses nothing it is handed, that the two parts which are not words are <em>held</em> rather than
/// copied or weighed word for word, and that the members every record is asked compare, hash, fall apart, copy and
/// say the four parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeManifestParserEdgeCaseTests" /> pins the read out
/// of a file: that an <c>alias</c> missing or empty stops the manifest, that a region with no <c>name</c> - or one of
/// spaces, or no string at all - is called by its alias, that a name arrives trimmed, that the <c>roles</c> are the
/// strings of the file's list with nothing else kept and nothing refused, and that <c>links</c> of no https address
/// leave the part absent. <see cref="RuntimeManifestParserTests" /> pins which roles the sample manifests carry.
/// <see cref="RuntimeBoxLinkEdgeCaseTests" /> and <see cref="RuntimeWorkHelperCoverageTests" /> pin what the
/// <see cref="RuntimeRegion.Links" /> mean - that a region which is not there is refused by the name of the part, that
/// no links and links that hold nothing both lead nowhere, and that a region's own name is what the href and the
/// title are written out of. <see cref="RotationTests" /> and <see cref="RuntimePayloadBuilderTests" /> pin what the
/// parts are good for: that <see cref="RuntimeRegion.Alias" /> is how a node says which region it stands in, that a
/// role of <c>data</c> and one of <c>static</c> are the two words the page says back as <c>database</c> and
/// <c>static sites</c>, and that a region of no web app is marked <c>not probed</c> or <c>not checked</c>.
/// </para>
/// <para>
/// What follows is the record on its own, at the ends the tests above leave open: the four parts by the names of their
/// own properties, the fourth standing absent when the caller does not name it, the empty word that is a word and the
/// spaces and the letters that only look alike, the name never fallen back on the alias and the alias never weighed
/// against the name, the roles kept as the very list the caller brought - so that two regions of the same two role
/// words are two regions unless they stand on one list, which is the one thing that makes a region unlike
/// <see cref="RuntimeEdge" />, whose every part is a word and which <see cref="DependencyTests" /> pins equal to the
/// edge read out of a file - and the whole public surface, so that the day a fifth part is added the list of them says
/// so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a region holds two words, a list and a link set,
/// calls nothing at all, and every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimeRegionEdgeCaseTests
{
    private const string Alias = "region_primary";
    private const string Name = "centralus";

    private static readonly Uri Portal = new("https://portal.example.net/#/overview");
    private static readonly Uri Graph = new("https://portal.example.net/#blade/HubsExtension/ArgQueryBlade/query/");

    /// <summary>The roles of a region that serves and holds the database, held as one list: every fixture below stands
    /// on it, so the fixtures are one region's copies rather than four regions that only look alike - which is the
    /// part's behaviour, pinned for itself further down.</summary>
    private static readonly IReadOnlyList<string> Roles = new List<string> { "primary", "data" };

    /// <summary>No roles at all, as one list too: a region of none is a region, and the list of them is a part of its
    /// own rather than the absence of one.</summary>
    private static readonly IReadOnlyList<string> NoRoles = new List<string>();

    /// <summary>The same two role words as an array rather than a list, held as one array: an array is another part
    /// from a list of the same two words, and it says itself by the name of an array.</summary>
    private static readonly string[] RoleArray = ["primary", "data"];

    /// <summary>A role array of one word, for the words a region says about itself.</summary>
    private static readonly string[] OneRole = ["primary"];

    /// <summary>The one link a region's name takes: its portal page.</summary>
    private static readonly LinkSet PortalLinks = Links((LinkSet.Portal, Portal));

    /// <summary>A region with every part named, the two words apart from one another and its roles two words deep, so
    /// that a part landing on another property is seen instead of quietly matching.</summary>
    private static RuntimeRegion Full => new(Alias, Name, Roles, PortalLinks);

    /// <summary>A region named by nothing but the three a caller must name, the links standing as they stand when
    /// nobody names them.</summary>
    private static RuntimeRegion Bare => new(Alias, Name, Roles);

    /// <summary>The region of nothing at all: no words, no roles and no links - the parts the record will hold
    /// although the file may not give them.</summary>
    private static RuntimeRegion Nothing => new(null!, null!, null!, null);

    /// <summary>The region of empty words: a part said as nothing, which the record keeps as a word and not as the
    /// absence of one, and a list of no roles, which is a list.</summary>
    private static RuntimeRegion Empty => new(string.Empty, string.Empty, NoRoles);

    // ------------------------------------------------------- the four parts, by the names of their own properties

    /// <summary>The constructor's four parts, position by position, come back as the property of the same name - the
    /// order the reader of a manifest hands a region over in. Each part keeps the value it was given and nothing about
    /// it is worked out of the others: a region whose alias says <c>region_data</c> and whose name says another
    /// region's word keeps both, and the record puts neither word in the other's place.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var region = Full;

        Assert.Equal(Alias, region.Alias);
        Assert.Equal(Name, region.Name);
        Assert.Equal(2, region.Roles.Count);
        Assert.Equal("primary", region.Roles[0]);
        Assert.Equal("data", region.Roles[1]);
        Assert.Same(PortalLinks, region.Links);

        var japan = new RuntimeRegion("region_japan", "japaneast", new List<string> { "standby" });

        Assert.Equal("region_japan", japan.Alias);
        Assert.Equal("japaneast", japan.Name);
        Assert.Equal(["standby"], japan.Roles);
        Assert.Null(japan.Links);

        // The alias is the boundary's word in the PlantUML, the name Azure's: the record reads nothing out of either.
        var crossed = new RuntimeRegion("region_data", "westus3", NoRoles, null);

        Assert.Equal("region_data", crossed.Alias);
        Assert.Equal("westus3", crossed.Name);
        Assert.NotEqual(crossed.Alias, crossed.Name);
        Assert.Empty(crossed.Roles);
        Assert.Null(crossed.Links);
    }

    /// <summary>The links are the one part a caller may leave out, and they stand absent when it does - absent, and not
    /// the empty set of links a box could be read as having been given and asked nothing of. Naming them as nothing is
    /// the same as not naming them, and the declaration of that default - four parts, one of them defaulting to the
    /// nothing - is read off the type in the surface test at the end of this file.</summary>
    [Fact]
    public void TheLinksTheCallerDoesNotNameStandAbsent()
    {
        Assert.Null(Bare.Links);
        Assert.Null(new RuntimeRegion(Alias, Name, Roles, null).Links);
        Assert.Null(Empty.Links);
        Assert.Null(Nothing.Links);

        // The absence of the part is the absence of a link set, and the region whose links are named as nothing is the
        // region whose links are not named at all.
        Assert.Equal(Bare, Bare with { Links = null });
        Assert.Equal(Bare.GetHashCode(), (Bare with { Links = null }).GetHashCode());
        Assert.Single(new HashSet<RuntimeRegion> { Bare, new RuntimeRegion(Alias, Name, Roles, null) });

        // A set of links that holds nothing is still a set, and another part from no set at all: what the box does
        // with each of them - nothing, in both cases - is the read's and is pinned with RuntimeBoxLink.
        Assert.NotNull((Bare with { Links = LinkSet.None }).Links);
        Assert.Equal(0, (Bare with { Links = LinkSet.None }).Links!.Count);
        Assert.NotEqual(Bare, Bare with { Links = LinkSet.None });
        Assert.NotEqual(Empty, Empty with { Links = LinkSet.None });

        // Two regions handed the shared empty set are one region, since they stand on the very same set.
        Assert.Equal(new RuntimeRegion(Alias, Name, Roles, LinkSet.None), new RuntimeRegion(Alias, Name, Roles, LinkSet.None));
    }

    /// <summary>Both words are kept exactly as they are handed: the empty word is a word and not nothing, the spaces
    /// about a word stay about it, a word of one letter is a word, and a word of letters that only look like the Latin
    /// ones is kept letter for letter. (That the file's words arrive trimmed, and that a region with no name is given
    /// its alias for one, is the reader's and is pinned with the reader.)</summary>
    [Fact]
    public void ARegionKeepsItsWordsExactlyAsTheyAreGiven()
    {
        var empty = Empty;

        Assert.Equal(string.Empty, empty.Alias);
        Assert.Equal(string.Empty, empty.Name);

        var oneLetter = new RuntimeRegion("r", "c", new List<string> { "p" });

        Assert.Equal("r", oneLetter.Alias);
        Assert.Equal("c", oneLetter.Name);
        Assert.Equal(["p"], oneLetter.Roles);

        var spaced = new RuntimeRegion("  region_primary  ", " centralus ", new List<string> { " primary " });

        Assert.Equal("  region_primary  ", spaced.Alias);
        Assert.Equal(" centralus ", spaced.Name);
        Assert.Equal(" primary ", spaced.Roles[0]);

        var written = new RuntimeRegion("rēgion_primary", "ｃｅｎｔｒａｌｕｓ", new List<string> { "ｐｒｉｍａｒｙ", string.Empty });

        Assert.Equal("rēgion_primary", written.Alias);
        Assert.Equal("ｃｅｎｔｒａｌｕｓ", written.Name);
        Assert.Equal("ｐｒｉｍａｒｙ", written.Roles[0]);
        Assert.Equal(string.Empty, written.Roles[1]);

        // A word of a thousand letters is kept at its length, and a region of a thousand roles keeps them all.
        var longWord = new string('x', 1_000);
        var many = new List<string>();
        many.AddRange(Enumerable.Repeat("role", 1_000));

        var boundless = new RuntimeRegion(longWord, longWord, many);

        Assert.Equal(longWord, boundless.Alias);
        Assert.Equal(longWord, boundless.Name);
        Assert.Equal(1_000, boundless.Roles.Count);
    }

    /// <summary>The record makes no decision of its own about what a part may be: a region will hold no words at all
    /// and no list at all, since it is the file that is read and the reader that refuses an alias and invents a name.
    /// Only the two words and the roles are non-nullable in the type, and a caller may still hand the record the
    /// nothing of any of the three.</summary>
    [Fact]
    public void ARegionRefusesNothingItIsGiven()
    {
        var nothing = Nothing;

        Assert.Null(nothing.Alias);
        Assert.Null(nothing.Name);
        Assert.Null(nothing.Roles);
        Assert.Null(nothing.Links);

        // The region of no roles at all is another region from the one with no list of them, and both are regions.
        Assert.NotEqual(nothing, Empty);
        Assert.True(nothing != Empty);
        Assert.False(nothing.Equals(Empty));
        Assert.Null(new RuntimeRegion(Alias, Name, null!).Roles);
        Assert.NotNull(Bare.Roles);

        // Two regions of nothing at all, made by two calls, are one region: the nothing of a list weighs the same
        // twice over, where the list itself does not (pinned below).
        Assert.Equal(nothing, new RuntimeRegion(null!, null!, null!, null));
        Assert.Single(new HashSet<RuntimeRegion> { nothing, new RuntimeRegion(null!, null!, null!, null) });

        // The alias is the word the page finds a region by, and the name the word it draws; the record insists on
        // neither and falls back on neither.
        Assert.Null(new RuntimeRegion(null!, Name, Roles).Alias);
        Assert.Null(new RuntimeRegion(Alias, null!, Roles).Name);
    }

    // --------------------------------------------------------------------------------------------- the roles

    /// <summary>The roles are held as the list they were handed, in the order they were handed and with every word of
    /// them standing: a role repeated is repeated, the empty word is a role, a word of spaces is a role, and a role
    /// that is no word at all is a role of the nothing the caller put in the list. Nothing here is trimmed, lowered,
    /// distincted or refused - the meaning of a role is the builder's, and the words the file gives are the reader's.</summary>
    [Fact]
    public void TheRolesAreHeldAsTheyAreHandedAndInTheirOrder()
    {
        var list = new List<string> { "primary", "standby", "data" };
        var region = new RuntimeRegion(Alias, Name, list);

        Assert.Same(list, region.Roles);
        Assert.Equal(["primary", "standby", "data"], region.Roles);
        Assert.Equal(3, region.Roles.Count);
        Assert.Equal("data", region.Roles[2]);

        // A role said twice is said twice, and the empty word and the word of spaces are roles.
        var twice = new RuntimeRegion(Alias, Name, new List<string> { "primary", "primary", string.Empty, " " });

        Assert.Equal(4, twice.Roles.Count);
        Assert.Equal("primary", twice.Roles[1]);
        Assert.Equal(string.Empty, twice.Roles[2]);
        Assert.Equal(" ", twice.Roles[3]);

        // The list of no roles is an empty list, and a role that is no word at all is kept as the nothing it is.
        Assert.Empty(new RuntimeRegion(Alias, Name, new List<string>()).Roles);
        Assert.Null(new RuntimeRegion(Alias, Name, new List<string> { null! }).Roles[0]);

        // The roles of the region the reader built are the strings of the file, in the file's own order - and the
        // reader is what leaves out a JSON value that is no string at all (pinned with the reader).
        var read = Assert.Single(
            RuntimeManifestParser.ParseManifest("""{ "environment": "tdd", "regions": [ { "alias": "region_primary", "name": "centralus", "roles": [ "data", "primary" ] } ] }""").Value!
                .Regions);

        Assert.Equal(["data", "primary"], read.Roles);
        Assert.Equal(Alias, read.Alias);
        Assert.Equal(Name, read.Name);
    }

    /// <summary>The record takes the list it is handed and does not copy it: a caller that keeps on filling the list
    /// fills the region with it, and a region is what its parts are at the moment it is asked, not at the moment it
    /// was built. (The reader hands every region a list of its own and never touches it again, so this is a fact about
    /// the record rather than a hole in the app - and it is the reason a region is a part to be handed once.)</summary>
    [Fact]
    public void TheRegionHoldsItsCallersListRatherThanACopyOfIt()
    {
        var list = new List<string> { "primary" };
        var region = new RuntimeRegion(Alias, Name, list);

        Assert.Single(region.Roles);

        list.Add("data");

        Assert.Equal(2, region.Roles.Count);
        Assert.Equal("data", region.Roles[1]);
        Assert.Same(list, region.Roles);

        // A region built of the grown list is one with the region that watched it grow, and a region built of what
        // the list was before it grew is another.
        Assert.Equal(region, new RuntimeRegion(Alias, Name, list));
        Assert.NotEqual(region, new RuntimeRegion(Alias, Name, new List<string> { "primary" }));

        // The links are held the same way: the very set, and no copy of it.
        var links = Links((LinkSet.Portal, Portal));

        Assert.Same(links, new RuntimeRegion(Alias, Name, Roles, links).Links);
    }

    /// <summary>The two parts that are no words are weighed as the values they are, and a plain list and a plain link
    /// set weigh themselves by reference: two regions of the same two role words in two lists are two regions, in two
    /// sets of a hash and in two entries of a dictionary, and only the very list the one region stands on makes
    /// another region one with it. This is what makes a region unlike a word - and unlike <see cref="RuntimeEdge" />,
    /// every part of which is a word, which is why the edge read out of a file is equal to the edge built by hand
    /// (<see cref="DependencyTests" />) and the region read out of a file is not the region built by hand.</summary>
    [Fact]
    public void TheRolesAndTheLinksAreWeighedAsTheValuesHeldAndNotAsTheirWords()
    {
        var one = new RuntimeRegion(Alias, Name, new List<string> { "primary", "data" });
        var two = new RuntimeRegion(Alias, Name, new List<string> { "primary", "data" });

        // The roles are the same words in the same order; the region is not the same region.
        Assert.Equal(one.Roles, two.Roles);
        Assert.NotSame(one.Roles, two.Roles);
        Assert.NotEqual(one, two);
        Assert.True(one != two);
        Assert.False(one.Equals(two));
        Assert.False(one.Equals((object)two));
        Assert.Equal(2, new HashSet<RuntimeRegion> { one, two }.Count);

        // The very list makes them one, and the hash follows it.
        var shared = new RuntimeRegion(Alias, Name, one.Roles);

        Assert.True(one == shared);
        Assert.Equal(one.GetHashCode(), shared.GetHashCode());
        Assert.Single(new HashSet<RuntimeRegion> { one, shared });

        // The same story told by a dictionary, which is how the page keys a part: by the reference it was indexed at.
        var indexed = new Dictionary<RuntimeRegion, string> { [one] = "one" };

        Assert.True(indexed.ContainsKey(shared));
        Assert.False(indexed.ContainsKey(two));

        // A list and an array of the same words are two parts, and two empty lists are two parts as much as two lists
        // of two words are - while the empty list handed twice from one place is one part.
        Assert.NotEqual(new RuntimeRegion(Alias, Name, new List<string>()), new RuntimeRegion(Alias, Name, new List<string>()));
        Assert.NotEqual(one, new RuntimeRegion(Alias, Name, RoleArray));
        Assert.Equal(Empty, Empty);

        // The same for the links: a set of the one key and the one address, in another reference, is another part.
        var first = Links((LinkSet.Portal, Portal));
        var second = Links((LinkSet.Portal, Portal));

        Assert.NotSame(first, second);
        Assert.NotEqual(new RuntimeRegion(Alias, Name, Roles, first), new RuntimeRegion(Alias, Name, Roles, second));
        Assert.Equal(new RuntimeRegion(Alias, Name, Roles, first), new RuntimeRegion(Alias, Name, Roles, first));

        // A region of another key is another region, and a region whose only link is another box's key is a region
        // all the same - what the read makes of that key is pinned with RuntimeBoxLink.
        Assert.NotEqual(
            new RuntimeRegion(Alias, Name, Roles, Links((LinkSet.Portal, Portal))),
            new RuntimeRegion(Alias, Name, Roles, Links((RuntimeBoxLink.Resources, Graph))));
        Assert.Single(new RuntimeRegion(Alias, Name, Roles, Links(("workloadsOfAnotherElement", Graph))).Links!.Keys);
    }

    /// <summary>The consequence for the file: a region read out of a manifest is not the region built by hand out of
    /// the very words, since the reader brings its own list of the roles every time. The reader's tests get round that
    /// by asking the parts one by one; two reads of one text are two regions, and only the region that stands on the
    /// read's own list is one with the read.</summary>
    [Fact]
    public void ARegionReadFromTheFileIsNotTheRegionBuiltByHandOutOfTheSameWords()
    {
        var text = """{ "environment": "tdd", "regions": [ { "alias": "region_primary", "name": "centralus", "roles": [ "primary", "data" ] } ] }""";
        var read = Assert.Single(RuntimeManifestParser.ParseManifest(text).Value!.Regions);
        var again = Assert.Single(RuntimeManifestParser.ParseManifest(text).Value!.Regions);

        Assert.Equal(read.Alias, again.Alias);
        Assert.Equal(read.Name, again.Name);
        Assert.Equal(read.Roles, again.Roles);
        Assert.Null(read.Links);
        Assert.NotEqual(read, again);

        // The hand-built region of the same two role words is a different region; the one that stands on the read's
        // own list is the same one.
        Assert.NotEqual(read, new RuntimeRegion(Alias, Name, ["primary", "data"]));
        Assert.Equal(read, new RuntimeRegion(read.Alias, read.Name, read.Roles, read.Links));
    }

    // --------------------------------------------------------------------------------------------- the links

    /// <summary>What the links part holds, in the region's own terms: no link at all, the empty set, a set of one key
    /// and a set of several - the part is kept, counted and asked for its address by the key it is spelled with, and
    /// a key with nothing behind it is no address. Where a region's link leads and what it is called to say is the
    /// read's, and is pinned with <see cref="RuntimeBoxLinkEdgeCaseTests" />.</summary>
    [Fact]
    public void ARegionHoldsTheLinksItWasGivenAndCountsThem()
    {
        Assert.Null(Bare.Links);
        Assert.Equal(0, LinkSet.None.Count);

        var one = new RuntimeRegion(Alias, Name, Roles, Links((LinkSet.Portal, Portal)));

        Assert.Equal(1, one.Links!.Count);
        Assert.Equal(Portal, one.Links[LinkSet.Portal]);
        Assert.Null(one.Links[RuntimeBoxLink.Resources]);
        Assert.Null(one.Links["nothingOfAKey"]);

        var several = new RuntimeRegion(Alias, Name, Roles, Links((RuntimeBoxLink.Resources, Graph), (LinkSet.Portal, Portal), (RuntimeBoxLink.Site, Portal)));

        Assert.Equal(3, several.Links!.Count);
        Assert.Equal([LinkSet.Portal, RuntimeBoxLink.Resources, RuntimeBoxLink.Site], [.. several.Links.Keys.Order(StringComparer.Ordinal)]);
        Assert.Null(several.Links[LinkSet.Workloads]);

        // The order a set is built in is no part of the region: what the read picks out of the six keys is the read's.
        Assert.NotEqual(
            new RuntimeRegion(Alias, Name, Roles, Links((LinkSet.Portal, Portal), (RuntimeBoxLink.Resources, Graph))),
            new RuntimeRegion(Alias, Name, Roles, Links((RuntimeBoxLink.Resources, Graph), (LinkSet.Portal, Portal))));

        // The set a region is built with is the set a copy of it is asked about, whatever the copy does to its words.
        var renamed = one with { Name = "eastus2" };

        Assert.Same(one.Links, renamed.Links);
        Assert.Equal("eastus2", renamed.Name);
    }

    // ---------------------------------------------------------------------------- the record of the four parts

    /// <summary>Two regions are one while their four parts are, whichever of the ways of asking is used: the record's
    /// own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, and the hash that follows them
    /// and stands from call to call. A region is compared, not looked through: two calls on the same two words, the
    /// same list and the same set are one region, and a set of them holds them once.</summary>
    [Fact]
    public void TwoRegionsAreOneWhileTheirFourPartsAre()
    {
        var one = Full;
        var two = new RuntimeRegion(Alias, Name, Roles, PortalLinks);

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
        Assert.Single(new HashSet<RuntimeRegion> { one, two });

        // The parts asked out and put back in make the same region again: the equality is of the values, not of the
        // call that gathered them.
        one.Deconstruct(out var alias, out var name, out var roles, out var links);

        Assert.Equal(one, new RuntimeRegion(alias, name, roles, links));
        Assert.Equal(one.GetHashCode(), new RuntimeRegion(alias, name, roles, links).GetHashCode());
    }

    /// <summary>Any one of the four parts tells two regions apart, and nothing is weighed loosely: the two words
    /// ordinally, so an alias or a name in another case, or with a space on it, or with a letter that only looks like
    /// the one expected, is another word. The empty word, the word of spaces and the nothing that is no word are three
    /// parts of their own.</summary>
    [Fact]
    public void ARegionDiffersInAnyOneOfItsParts()
    {
        var region = Full;

        Assert.NotEqual(region, region with { Alias = string.Empty });
        Assert.NotEqual(region, region with { Alias = "  " });
        Assert.NotEqual(region, region with { Alias = null! });
        Assert.NotEqual(region, region with { Alias = "REGION_PRIMARY" });
        Assert.NotEqual(region, region with { Alias = "region_primary " });
        Assert.NotEqual(region, region with { Alias = "region_standby" });
        Assert.NotEqual(region, region with { Name = string.Empty });
        Assert.NotEqual(region, region with { Name = null! });
        Assert.NotEqual(region, region with { Name = "Centralus" });
        Assert.NotEqual(region, region with { Name = "centralus2" });
        Assert.NotEqual(region, region with { Name = "westus3" });
        Assert.NotEqual(region, region with { Roles = null! });
        Assert.NotEqual(region, region with { Roles = NoRoles });
        Assert.NotEqual(region, region with { Links = null });
        Assert.NotEqual(region, region with { Links = LinkSet.None });

        // The very key and the very address in a set of its own is another part: the links are the set, not what it
        // holds - which is the second half of the sentence pinned above.
        Assert.NotEqual(region, region with { Links = Links((LinkSet.Portal, Portal)) });

        // A word of one letter is as much an alias as a word of fourteen: the record reads nothing into either.
        Assert.NotEqual(region, region with { Alias = "r" });

        // The empty word and the nothing are two parts, and a word of spaces is a third.
        Assert.NotEqual(new RuntimeRegion(string.Empty, Name, Roles), new RuntimeRegion(null!, Name, Roles));
        Assert.NotEqual(new RuntimeRegion(" ", Name, Roles), new RuntimeRegion(null!, Name, Roles));
        Assert.NotEqual(new RuntimeRegion(Alias, string.Empty, Roles), new RuntimeRegion(Alias, null!, Roles));

        // The regions that differ from this one by one part are five apart: the region itself and one of each of its
        // four parts changed.
        Assert.Equal(
            5,
            new HashSet<RuntimeRegion>
            {
                region,
                region with { Alias = "region_standby" },
                region with { Name = "eastus2" },
                region with { Roles = new List<string> { "primary" } },
                region with { Links = Links((RuntimeBoxLink.Resources, Graph)) },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two regions that are no region
    /// at all are one, a region and no region are two - the equality and the inequality each saying it, from either
    /// side - and the region asked about itself says no to the nothing, to the two words it is named by, to the list
    /// of its roles and to the set of its links. The region of nothing at all is still a region.</summary>
    [Fact]
    public void NoRegionIsOneWithNoRegionAndTheRegionOfNothingIsARegion()
    {
        RuntimeRegion? nothing = null;
        RuntimeRegion? alsoNothing = null;
        var there = Full;

        Assert.Null(default(RuntimeRegion));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((RuntimeRegion?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(Alias));
        Assert.False(there.Equals(Name));
        Assert.False(there.Equals(Roles));
        Assert.False(there.Equals(PortalLinks));
        Assert.False(there.Equals(Portal));
        Assert.False(there.Equals(new RuntimeFrame(Alias, "cluster", Name, null)));
        Assert.False(there.Equals(new RuntimeNode("app_ui_primary", RuntimeNodeKind.WebApp, Name, RegionAlias: Alias)));
        Assert.False(there.Equals(new RuntimeEdge("browser-to-fd_ui", "browser", "fd_ui", RuntimeEdgeKind.Public)));
        Assert.False(there.Equals(new RuntimeRegionMark(Alias, "serving", "serving traffic")));
        Assert.False(there.Equals(new RuntimeManifest("tdd", [], [there], [])));

        var alsoEmpty = Empty;

        Assert.Equal(Empty, alsoEmpty);
        Assert.NotSame(Empty, alsoEmpty);
        Assert.Equal(Empty.GetHashCode(), alsoEmpty.GetHashCode());
        Assert.True(Empty == alsoEmpty);
        Assert.Single(new HashSet<RuntimeRegion> { Empty, alsoEmpty });
    }

    /// <summary>The alias is held as it is written and is never weighed against anything: not against the name of the
    /// Azure region it is drawn over, and not against the <c>regionAlias</c> of the nodes that stand in it. Only the
    /// very word the constructor was given makes the copied region the region it was copied from.</summary>
    [Theory]
    [InlineData("region_primary", true)]
    [InlineData("REGION_PRIMARY", false)]
    [InlineData("region_primary ", false)]
    [InlineData("centralus", false)]
    [InlineData("region_standby", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void TheAliasIsHeldAsItIsWrittenAndNeverWeighedAgainstTheName(string? alias, bool isTheVeryRegion)
    {
        var region = Full with { Alias = alias! };

        Assert.Equal(alias, region.Alias);
        Assert.Equal(Name, region.Name);
        Assert.Equal(Roles, region.Roles);
        Assert.Same(PortalLinks, region.Links);
        Assert.Contains($"Alias = {alias}", region.ToString(), StringComparison.Ordinal);
        Assert.Equal(isTheVeryRegion, region == Full);
    }

    /// <summary>The name is the Azure region as the deployment wrote it - and the record neither requires one nor
    /// invents one: the empty word, the word of spaces and the nothing are three names the region keeps, and it never
    /// falls back on its own alias. That fallback, and the trimming in front of it, are the reader's and are pinned
    /// with the reader; the page draws whatever word the record is holding.</summary>
    [Theory]
    [InlineData("centralus", true)]
    [InlineData("Centralus", false)]
    [InlineData(" centralus", false)]
    [InlineData("centralus ", false)]
    [InlineData("eastus2", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void TheNameIsHeldAsItIsWrittenAndNeverFallenBackOnTheAlias(string? name, bool isTheVeryRegion)
    {
        var region = Full with { Name = name! };

        Assert.Equal(name, region.Name);
        Assert.Equal(Alias, region.Alias);
        Assert.Equal(Roles, region.Roles);
        Assert.Contains($"Name = {name}", region.ToString(), StringComparison.Ordinal);
        Assert.Equal(isTheVeryRegion, region == Full);

        // A region named by nothing says nothing by its own mouth: the alias comes into the words only because the
        // caller put it in the alias and nothing else.
        if (string.IsNullOrWhiteSpace(name))
        {
            Assert.DoesNotContain($"Name = {Alias}", region.ToString(), StringComparison.Ordinal);
        }
    }

    // ------------------------------------------------------------------------------------------ copies

    /// <summary><c>with</c> names the part it changes and carries the other three: a copy that names nothing is one
    /// with its original in another reference - its list and its set of links included, since a copy copies the parts
    /// and not what they are made of - the original is left where it stood, and the fourth part may be put on and
    /// taken off as the two words are.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var region = Full;
        var copy = region with { };

        Assert.Equal(region, copy);
        Assert.NotSame(region, copy);
        Assert.True(region == copy);
        Assert.Same(region.Roles, copy.Roles);
        Assert.Same(region.Links, copy.Links);

        var ofAnotherRegion = region with { Alias = "region_standby", Name = "eastus2" };

        Assert.Equal("region_standby", ofAnotherRegion.Alias);
        Assert.Equal("eastus2", ofAnotherRegion.Name);
        Assert.Same(Roles, ofAnotherRegion.Roles);
        Assert.Same(PortalLinks, ofAnotherRegion.Links);
        Assert.NotEqual(region, ofAnotherRegion);

        var unlinked = region with { Links = null };

        Assert.Null(unlinked.Links);
        Assert.Equal(Alias, unlinked.Alias);
        Assert.Equal(Name, unlinked.Name);
        Assert.Same(Roles, unlinked.Roles);
        Assert.Equal(PortalLinks, region.Links);

        var roles = region with { Roles = new List<string> { "data" } };

        Assert.Equal(["data"], roles.Roles);
        Assert.Equal(2, region.Roles.Count);

        // A copy that names the parts the region already holds is the region, the links put on as they may be taken
        // off, and a copy of the region of nothing is the region of nothing.
        Assert.Equal(region, region with { Alias = Alias, Name = Name, Roles = Roles, Links = PortalLinks });
        Assert.Equal(Bare, Bare with { Links = null });
        Assert.Equal(Full, Bare with { Links = PortalLinks });
        Assert.Equal(Nothing, Nothing with { Alias = null!, Name = null!, Roles = null!, Links = null });
    }

    /// <summary>The four parts fall out of a region in the order they went in, each as the value it holds, and what
    /// came out goes back in the same way; the part the region was not told falls out as the nothing it is, and the
    /// region that was told nothing at all is a region all the same, list and links and all.</summary>
    [Fact]
    public void TheFourPartsFallOutOfARegionInTheOrderTheyWentIn()
    {
        var (alias, name, roles, links) = Full;

        Assert.Equal(Alias, alias);
        Assert.Equal(Name, name);
        Assert.Same(Roles, roles);
        Assert.Same(PortalLinks, links);
        Assert.Equal(Full, new RuntimeRegion(alias, name, roles, links));

        var bare = Bare;
        bare.Deconstruct(out var bareAlias, out var bareName, out var bareRoles, out var bareLinks);

        Assert.Equal(Alias, bareAlias);
        Assert.Equal(Name, bareName);
        Assert.Equal(["primary", "data"], bareRoles);
        Assert.Null(bareLinks);
        Assert.Equal(bare, new RuntimeRegion(bareAlias, bareName, bareRoles, bareLinks));

        var (nothingAlias, nothingName, nothingRoles, nothingLinks) = Nothing;

        Assert.Null(nothingAlias);
        Assert.Null(nothingName);
        Assert.Null(nothingRoles);
        Assert.Null(nothingLinks);

        // One part asked out of the four, the rest of them left where they are.
        var (only, _, _, _) = new RuntimeRegion("region_japan", "japaneast", NoRoles);

        Assert.Equal("region_japan", only);

        // The roles fall out as the list, whole and in order, and are still the caller's own list.
        var list = new List<string> { "standby" };
        var (_, _, fallen, _) = new RuntimeRegion(Alias, Name, list);

        Assert.Same(list, fallen);
        Assert.Single(fallen);
    }

    // --------------------------------------------------------------------------------------------- words

    /// <summary>The words a region says about itself - its name, then its four parts in the order they are declared,
    /// each named and standing blank when there is nothing of it - are the record's own and not the page's: the page
    /// is given its words by the payload builder. The roles are said as whatever list the caller brought says itself,
    /// which is why these words name a kind of part rather than a region's roles, and the links likewise: the whole
    /// line stands here so that a part moved about in the record is a change the tests see.</summary>
    [Fact]
    public void ARegionSaysItsFourPartsInTheirOrder()
    {
        Assert.Equal(
            "RuntimeRegion { Alias = region_primary, Name = centralus, Roles = System.Collections.Generic.List`1[System.String], Links = Dashboard.Health.LinkSet }",
            Full.ToString());

        // The part a caller may leave out is named and says nothing.
        Assert.Equal(
            "RuntimeRegion { Alias = region_primary, Name = centralus, Roles = System.Collections.Generic.List`1[System.String], Links =  }",
            Bare.ToString());

        // The empty words stand blank, the list of no roles is still a list, and no links at all say nothing.
        Assert.Equal(
            "RuntimeRegion { Alias = , Name = , Roles = System.Collections.Generic.List`1[System.String], Links =  }",
            Empty.ToString());
        Assert.Equal(
            "RuntimeRegion { Alias = , Name = , Roles = , Links =  }",
            Nothing.ToString());

        // An array of roles says itself as an array: the words of the part are the part's own, and the record puts in
        // no word of them.
        Assert.Equal(
            "RuntimeRegion { Alias = r, Name = c, Roles = System.String[], Links =  }",
            new RuntimeRegion("r", "c", OneRole).ToString());

        // A word is said as it stands, its spaces and all, and no part is put in quotes.
        Assert.Equal(
            "RuntimeRegion { Alias =   region_primary  , Name = centralus, Roles = System.Collections.Generic.List`1[System.String], Links =  }",
            new RuntimeRegion("  region_primary  ", Name, Roles).ToString());
    }

    /// <summary>A region's words are two words and two parts that say their own type's name, so no reader's culture
    /// turns them: a viewer of German is told the same region as the invariant reader, and the same list of roles,
    /// which is what a log of one of these regions may count on whatever the reader of the machine happens to be.</summary>
    [Fact]
    public void TheRegionsOwnWordsAreTheSameWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "RuntimeRegion { Alias = region_primary, Name = centralus, Roles = System.Collections.Generic.List`1[System.String], Links = Dashboard.Health.LinkSet }",
                Full.ToString());
            Assert.Equal(
                "RuntimeRegion { Alias = , Name = , Roles = , Links =  }",
                Nothing.ToString());
            Assert.Equal(
                "RuntimeRegion { Alias = r, Name = c, Roles = System.String[], Links =  }",
                new RuntimeRegion("r", "c", OneRole).ToString());
            Assert.Equal(Full, new RuntimeRegion(Alias, Name, Roles, PortalLinks));
        });
    }

    /// <summary>The parts are weighed as words and not as a reader spells them: two regions whose words differ only in
    /// case are two regions whatever culture the thread runs under, and the words that are one word are one region
    /// under another reader too - the comparison is ordinal, as the manifest's words are.</summary>
    [Fact]
    public void TheWordsOfARegionAreWeighedOrdinallyWhateverTheReaderSpeaks()
    {
        var upper = new RuntimeRegion("REGION_PRIMARY", "CENTRALUS", Roles, PortalLinks);

        ViewerCulture.German(() =>
        {
            Assert.NotEqual(Full, upper);
            Assert.True(Full != upper);
            Assert.False(Full.Equals((object)upper));
            Assert.Equal(Full, new RuntimeRegion(Alias, Name, Roles, PortalLinks));
        });

        Assert.NotEqual(Full, Full with { Alias = "ＲＥＧＩＯＮ_PRIMARY" });
        Assert.NotEqual(Full, Full with { Name = "ＣＥＮＴＲＡＬＵＳ" });
        Assert.NotEqual(Full, Full with { Name = "Centralus" });
        Assert.Equal(Full, new RuntimeRegion(Alias, Name, Roles, PortalLinks));

        // The dotted and the dotless <c>i</c>, which one reader's capital of a small letter turns into the other's,
        // are two letters of two aliases.
        Assert.NotEqual(new RuntimeRegion("ist", Name, Roles), new RuntimeRegion("ıst", Name, Roles));
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>The region's whole public surface as it stands: its four parts, named in the order the constructor
    /// names them and of the type the constructor says, the three a caller must name and the one it may leave out as
    /// nothing, and the members every record is asked - falling apart, comparing, hashing and saying itself - and
    /// nothing else to call. The two members the compiler gives a record in secret, its own name to compare by and
    /// its walk over its parts, are no part of the surface a caller may use. There is no verdict on a region to read
    /// and none to set: what a region is doing comes from its roles and its nodes, and the builder decides from them.
    /// A fifth part added to the record has to be written down here as well, which is the point: a region of a diagram
    /// cannot carry a part nobody decided what it means.</summary>
    [Fact]
    public void TheRegionHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Alias", "Name", "Roles", "Links"],
            [.. typeof(RuntimeRegion)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Alias", "Deconstruct", "Equals", "GetHashCode", "Links", "Name", "Roles", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);
        Assert.DoesNotContain("PrintMembers", PublicSurface(), StringComparer.Ordinal);

        var properties = typeof(RuntimeRegion)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Alias"].PropertyType);
        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), properties["Roles"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);

        Assert.All(
            properties.Values,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.True(property.CanWrite);
            });

        // The one public constructor, naming its four parts as the properties are named; the copy constructor the
        // compiler writes for a record is not one a caller outside the record calls.
        var constructors = typeof(RuntimeRegion).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Alias", "Name", "Roles", "Links"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(IReadOnlyList<string>), typeof(LinkSet)],
            [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.Equal([false, false, false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.Null(parameters[3].DefaultValue);

        // The nullable annotations stand as the record declares them: the links may be nothing, the two words and the
        // list may not - which is what the reader of the file leans on, and what the record itself does not enforce (a
        // caller may still hand it the nothing of a word or of a list, pinned above).
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        List<PropertyInfo> parts = [properties["Alias"], properties["Name"], properties["Roles"], properties["Links"]];

        Assert.Equal(
            [false, false, false, true],
            [.. parts.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        // The two members a record keeps to itself are there, and are not for a caller to ask.
        Assert.NotNull(typeof(RuntimeRegion).GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.NotNull(typeof(RuntimeRegion).GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance));

        Assert.Equal([typeof(IEquatable<RuntimeRegion>)], typeof(RuntimeRegion).GetInterfaces());
        Assert.True(typeof(RuntimeRegion).IsSealed);
        Assert.Equal(typeof(object), typeof(RuntimeRegion).BaseType);
        Assert.Empty(typeof(RuntimeRegion).GetEvents());
        Assert.Empty(typeof(RuntimeRegion).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the region declares itself: its parts and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeRegion)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The links of a box of the diagram, as the deployment writes them: a key of the six and the address
    /// behind it, the set built in the order the pairs are named.</summary>
    private static LinkSet Links(params (string Key, Uri Address)[] links) =>
        new(links.Select(link => new KeyValuePair<string, Uri>(link.Key, link.Address)));
}
