using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="NodeInfo" />: one regional node of a deployable,
/// the five parts it is made of and the one decision it makes of them.
/// <para>
/// What the other files already pin is left alone. <see cref="TopologyParserTests" /> pins what the file becomes:
/// the name a node falls back to when the file gives it none, the role it falls back to when the file names none -
/// the first node of a deployable being the primary - the role read out of the file in whatever case it was written,
/// and a node's address refused when it is no absolute http or https address. <see cref="LinkTests" /> pins the
/// links a node carries out of the file, the five keys of the sample and the node that holds none.
/// <see cref="TargetStatusTests" /> pins that the tile of a node whose role is <c>primary</c> is primary and the one
/// whose role is <c>standby</c> is not, and <see cref="RotationTests" /> and <see cref="TelemetryTests" /> read the
/// parts of the nodes of a running monitor. <see cref="DeployableInfoEdgeCaseTests" /> walks past the node itself and
/// compares it only as the contents of a deployable's list.
/// </para>
/// <para>
/// What is pinned here is the record on its own: the two words of the two roles, that they are two words and that
/// they are the words <see cref="IsPrimary" /> weighs a role against; that the five parts come back to the property
/// of their own name and the one the caller does not name stands absent; that a node refuses nothing it is given -
/// the empty word is a word and not nothing, the spaces about a word stay, and a part may even be handed as nothing
/// to a property the record says is there; that <see cref="IsPrimary" /> reads the whole word and nothing else, in
/// any case, trimmed by nobody, with the empty word, the word of spaces and no word at all all standing outside it;
/// and the members every record is asked - comparing, hashing, falling apart into the five, copying and saying
/// itself - with the link set a node holds compared by the instance it stands on and not by its keys, and an address
/// compared as an address is.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a node holds no moment and calls nothing, and
/// an address is named here and never fetched.
/// </para>
/// </summary>
public class NodeInfoEdgeCaseTests
{
    private static readonly Uri West = new("https://app-west.example.net");
    private static readonly Uri East = new("https://app-east.example.net");

    private static readonly LinkSet OneLink = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#app-west"))]);
    private static readonly LinkSet OtherLink = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#app-west"))]);

    /// <summary>A node with every part named, the five values apart from one another and the role of the two the
    /// record names, so that an argument landing on another property is seen instead of quietly matching a second
    /// part of the record.</summary>
    private static NodeInfo Full => new("app-west", "westus3", NodeInfo.PrimaryRole, West, OneLink);

    /// <summary>A standby node, the second of a deployable, named by all five parts as well and standing apart from
    /// <see cref="Full" /> in the region, the role, the address and the links.</summary>
    private static NodeInfo Standby => new("app-east", "eastus2", NodeInfo.StandbyRole, East, OtherLink);

    /// <summary>A node named by nothing but the four a caller must name, the links standing as they stand when
    /// nobody names them.</summary>
    private static NodeInfo Bare => new("app", "westus3", NodeInfo.PrimaryRole, West);

    // ------------------------------------------------------------------ the two role words

    /// <summary>The two roles a node is given, each the very word of the part it plays: they are spelled out here
    /// where a change of one is a change the tests see, the two being two different words and both of them the
    /// lower-case word <see cref="TopologyParser" /> writes and <see cref="RuntimePayloadBuilder" /> says.</summary>
    [Fact]
    public void TheTwoRoleWordsSayTheTwoRolesTheyStandFor()
    {
        Assert.Equal("primary", NodeInfo.PrimaryRole);
        Assert.Equal("standby", NodeInfo.StandbyRole);

        var roles = new[] { NodeInfo.PrimaryRole, NodeInfo.StandbyRole };

        Assert.Equal(2, roles.Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            roles,
            role =>
            {
                Assert.NotEqual(string.Empty, role);
                Assert.Equal(role, role.Trim());
                Assert.DoesNotContain(role, char.IsUpper);
            });
    }

    // ------------------------------------------------------------------ the five parts a node keeps

    /// <summary>The constructor's five parts, position by position, each back as the property of its own name -
    /// the order the parser hands the node over in, and the order <see cref="TargetStatus" /> builds a tile from.
    /// The address and the links come back as the very instances the node was handed, never copied.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var node = Full;

        Assert.Equal("app-west", node.Name);
        Assert.Equal("westus3", node.Region);
        Assert.Equal("primary", node.Role);
        Assert.Same(West, node.Url);
        Assert.Same(OneLink, node.Links);
    }

    /// <summary>The links are the one part a caller may leave out, and they stand absent when it does - absent, and
    /// not the empty set the page could be asked to open. The region is a part a caller must name but may name as
    /// nothing: a node of no region is a node the file said no region of.</summary>
    [Fact]
    public void ThePartTheCallerDoesNotNameStandsAbsent()
    {
        var node = Bare;

        Assert.Equal("app", node.Name);
        Assert.Equal("westus3", node.Region);
        Assert.Equal("primary", node.Role);
        Assert.Equal(West, node.Url);
        Assert.Null(node.Links);

        Assert.Null(new NodeInfo("app", null, NodeInfo.StandbyRole, West).Region);
        Assert.Null(new NodeInfo("app", null, NodeInfo.StandbyRole, West, Links: null).Links);
    }

    /// <summary>The node keeps the words it is given and changes none of them: the empty word is a word and not
    /// nothing, the spaces about a word stay about it, a name of no letters is still a name, and a word of one letter
    /// is weighed at its length and not guessed at - an address of one label and no path is read as it stands and a
    /// one-letter role is no role the node knows. (The parser that trims what the file says, lower-cases the role
    /// and gives a node the host of its address for a name is <see cref="TopologyParserTests" />'s.)</summary>
    [Fact]
    public void ANodeKeepsItsWordsAsItIsGivenThem()
    {
        var empty = new NodeInfo(string.Empty, string.Empty, string.Empty, West);
        var spaced = new NodeInfo("  app-west  ", "  westus3  ", "  primary  ", East, OneLink);
        var shortest = new NodeInfo("a", " ", "p", new Uri("https://a"));

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.Region);
        Assert.Equal(string.Empty, empty.Role);
        Assert.Null(empty.Links);

        Assert.Equal("a", shortest.Name);
        Assert.Equal(" ", shortest.Region);
        Assert.Equal("p", shortest.Role);
        Assert.False(shortest.IsPrimary);
        Assert.Equal("https://a/", shortest.Url.AbsoluteUri);

        Assert.Equal("  app-west  ", spaced.Name);
        Assert.Equal("  westus3  ", spaced.Region);
        Assert.Equal("  primary  ", spaced.Role);
        Assert.Same(East, spaced.Url);
        Assert.Same(OneLink, spaced.Links);
    }

    /// <summary>The record makes no decision of its own about what a part may be: a node will hold no name, no role
    /// and no address at all, since it is the file that is read - and the reader that refuses a node without an
    /// address is <see cref="TopologyParser" />, which says so in its own words
    /// (<see cref="TopologyParserTests" />). Only <see cref="IsPrimary" /> is asked what it makes of the nothing.</summary>
    [Fact]
    public void ANodeRefusesNothingItIsGiven()
    {
        var nothing = new NodeInfo(null!, null, null!, null!);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Region);
        Assert.Null(nothing.Role);
        Assert.Null(nothing.Url);
        Assert.Null(nothing.Links);
        Assert.False(nothing.IsPrimary);

        // Only the parts that stand apart tell the two nodes apart: fill the role and the region in and the node of
        // nothing at all is one with a node whose name and address were blanked out.
        Assert.Equal(nothing with { Region = "westus3", Role = NodeInfo.PrimaryRole }, Full with { Name = null!, Url = null!, Links = null });
    }

    // ------------------------------------------------------------------ the one decision a node makes

    /// <summary>The primary word in any case is the primary word, and the case the page is given it in changes
    /// nothing: the four spellings of one word make one answer.</summary>
    [Theory]
    [InlineData("primary")]
    [InlineData("PRIMARY")]
    [InlineData("Primary")]
    [InlineData("pRiMaRy")]
    public void ThePrimaryWordIsPrimaryInAnyCase(string role) => Assert.True(new NodeInfo("app", "westus3", role, West).IsPrimary);

    /// <summary>Nothing but the whole primary word is primary, and the word is weighed as it is written: the other
    /// role's word, a word of no letters, a word of spaces, the primary word with a space or another word about it,
    /// the primary word one letter short and one letter over, and no word at all all stand outside it - and a
    /// primary written in letters that only look the same, being another word to an ordinal comparison, stands
    /// outside it too.</summary>
    [Theory]
    [InlineData("standby")]
    [InlineData("STANDBY")]
    [InlineData("idle")]
    [InlineData("primary ")]
    [InlineData(" primary")]
    [InlineData("\tprimary")]
    [InlineData("primary-east")]
    [InlineData("primar")]
    [InlineData("primaryy")]
    [InlineData("p")]
    [InlineData("primaries")]
    [InlineData("the primary")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ｐｒｉｍａｒｙ")]
    [InlineData(null)]
    public void EveryOtherWordIsNotPrimary(string? role) => Assert.False(new NodeInfo("app", "westus3", role!, West).IsPrimary);

    /// <summary>The decision is read off the role as it stands, so the two words of the record are the two the
    /// decision is made of: the node built on the constant is primary and the one built on the other constant is
    /// not, and a copy that changes nothing else turns the answer about while the node it was copied from keeps
    /// its own.</summary>
    [Fact]
    public void TheTwoRoleWordsAreTheTwoTheDecisionIsMadeOf()
    {
        Assert.True(Full.IsPrimary);
        Assert.False(Standby.IsPrimary);

        var swapped = Full with { Role = NodeInfo.StandbyRole };
        var raised = Standby with { Role = NodeInfo.PrimaryRole };

        Assert.False(swapped.IsPrimary);
        Assert.True(raised.IsPrimary);
        Assert.True(Full.IsPrimary);
        Assert.False(Standby.IsPrimary);

        // The role is the whole of the difference between the two words: a primary with its role put to standby is
        // that standby in every other part.
        Assert.Equal(swapped, Standby with { Name = "app-west", Region = "westus3", Url = West, Links = OneLink });
    }

    /// <summary>A node says the same thing twice and says it of the part it holds: the answer does not move with the
    /// name, the region, the address or the links, and a node whose role is the primary word in another case is
    /// primary while standing no two ways.</summary>
    [Fact]
    public void TheDecisionSaysNothingAboutTheOtherParts()
    {
        foreach (var node in new[] { Full, Full with { Name = "app-east" }, Full with { Region = null }, Full with { Url = East }, Full with { Links = LinkSet.None }, Full with { Links = null } })
        {
            Assert.Equal(NodeInfo.PrimaryRole, node.Role);
            Assert.True(node.IsPrimary);
        }

        // The case the word is written in changes the answer no more than the parts about it do, and the answer
        // stands from call to call.
        var spelled = new NodeInfo("app", "westus3", "PRIMARY", West);

        Assert.True(spelled.IsPrimary);
        Assert.True(spelled.IsPrimary);
    }

    // ------------------------------------------------------------------ the record itself

    /// <summary>Two nodes are one while their five parts are: the two operators, <c>Equals</c> both ways and the
    /// object it takes, and the hash code that follows them and stands from call to call. A node is one with
    /// nothing that is not a node - not with the tile it becomes, and not with the word it is named by.</summary>
    [Fact]
    public void TwoNodesAreOneWhileTheirFivePartsAre()
    {
        var one = Full;
        var two = new NodeInfo("app-west", "westus3", NodeInfo.PrimaryRole, West, OneLink);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("app-west"));
        Assert.False(one.Equals(new TargetStatus(TargetKind.Node, "app-west", West, "westus3", NodeInfo.PrimaryRole, OneLink)));
        Assert.False(one.Equals(Full with { Name = "app-east" }));
    }

    /// <summary>Any one of the five parts tells two nodes apart - and nothing is compared loosely: the empty word
    /// and the nothing that is not a word are two parts, a region left out and a region of no letters are two parts,
    /// the absent link set and the one with nothing in it are two parts, and a role in another case is another role
    /// to a record even while <see cref="IsPrimary" /> makes one decision of the two.</summary>
    [Fact]
    public void ANodeDiffersInAnyOneOfItsParts()
    {
        var node = Full;

        Assert.NotEqual(node, node with { Name = string.Empty });
        Assert.NotEqual(node, node with { Name = "APP-WEST" });
        Assert.NotEqual(node, node with { Region = null });
        Assert.NotEqual(node, node with { Region = string.Empty });
        Assert.NotEqual(node, node with { Region = "eastus2" });
        Assert.NotEqual(node, node with { Role = string.Empty });
        Assert.NotEqual(node, node with { Role = "PRIMARY" });
        Assert.NotEqual(node, node with { Role = NodeInfo.StandbyRole });
        Assert.NotEqual(node, node with { Url = null! });
        Assert.NotEqual(node, node with { Url = new Uri("https://app-west.example.com") });
        Assert.NotEqual(node, node with { Links = null });
        Assert.NotEqual(node, node with { Links = LinkSet.None });
        Assert.NotEqual(node, node with { Links = OtherLink });

        Assert.NotEqual(new NodeInfo("app", string.Empty, NodeInfo.PrimaryRole, West), new NodeInfo("app", null, NodeInfo.PrimaryRole, West));
        Assert.NotEqual(new NodeInfo("app", "westus3", NodeInfo.PrimaryRole, West, LinkSet.None), Bare);
    }

    /// <summary>An address is compared as an address is, the node saying nothing of its own about it: the same site
    /// written with a trailing slash, with a default port or with a fragment after the <c>#</c> is the same address
    /// and makes one node of two, while another host, another scheme, another port and another path are four other
    /// addresses. (That the file's address must be absolute http or https is
    /// <see cref="TopologyParserTests" />'s; nothing here fetches one.)</summary>
    [Theory]
    [InlineData("https://app-west.example.net/", true)]
    [InlineData("https://app-west.example.net:443/", true)]
    [InlineData("https://app-west.example.net/#tiles", true)]
    [InlineData("https://app-east.example.net/", false)]
    [InlineData("http://app-west.example.net/", false)]
    [InlineData("https://app-west.example.net:8443/", false)]
    [InlineData("https://app-west.example.net/alive", false)]
    public void AnAddressIsComparedAsAnAddressIs(string url, bool sameNode) =>
        Assert.Equal(sameNode, Full with { Url = new Uri(url) } == Full);

    /// <summary>The links a node holds are compared as the hold they stand on and not as their keys: the very same
    /// link set makes two nodes one, and a second set of the one and the same link makes two nodes of them - the
    /// record opening neither to look inside it.</summary>
    [Fact]
    public void TheLinksANodeHoldsAreComparedAsTheHoldTheyStandOn()
    {
        var one = Full;
        var holdingTheSameLinks = new NodeInfo("app-west", "westus3", NodeInfo.PrimaryRole, West, OneLink);
        var holdingOtherLinks = Full with { Links = OtherLink };

        Assert.Equal(one, holdingTheSameLinks);
        Assert.Equal(one.GetHashCode(), holdingTheSameLinks.GetHashCode());

        Assert.Equal(one.Links!.Keys, holdingOtherLinks.Links!.Keys);
        Assert.Equal(one.Links[LinkSet.Portal], holdingOtherLinks.Links[LinkSet.Portal]);
        Assert.NotSame(one.Links, holdingOtherLinks.Links);
        Assert.True(one != holdingOtherLinks);
        Assert.NotEqual(one, holdingOtherLinks);
    }

    /// <summary><c>with</c> names the part it changes and carries the other four: a copy that names nothing is one
    /// with its original and not the same object, and the node it was copied from is untouched.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var node = Full;
        var copy = node with { };

        Assert.Equal(node, copy);
        Assert.NotSame(node, copy);

        var moved = node with { Url = new Uri("https://app-west.example.io") };

        Assert.Equal("app-west.example.io", moved.Url.Host);
        Assert.Equal("https://app-west.example.io/", moved.Url.AbsoluteUri);
        Assert.Equal("app-west", moved.Name);
        Assert.Equal("westus3", moved.Region);
        Assert.Equal("primary", moved.Role);
        Assert.Same(OneLink, moved.Links);
        Assert.True(moved.IsPrimary);
        Assert.Same(West, node.Url);

        var unnamed = node with { Region = null, Links = null, Role = NodeInfo.StandbyRole };

        Assert.Null(unnamed.Region);
        Assert.Null(unnamed.Links);
        Assert.Equal("standby", unnamed.Role);
        Assert.False(unnamed.IsPrimary);
        Assert.Equal("app-west", unnamed.Name);
    }

    /// <summary>The five parts read out of the node in the order they stand in it, each as the value it holds, and
    /// the absent ones reading out as the nothing they are.</summary>
    [Fact]
    public void TheFivePartsReadOutOfTheNodeInTheOrderTheyStand()
    {
        var (name, region, role, url, links) = Full;

        Assert.Equal("app-west", name);
        Assert.Equal("westus3", region);
        Assert.Equal("primary", role);
        Assert.Same(West, url);
        Assert.Same(OneLink, links);

        var (bareName, bareRegion, bareRole, bareUrl, bareLinks) = Bare;

        Assert.Equal("app", bareName);
        Assert.Equal("westus3", bareRegion);
        Assert.Equal("primary", bareRole);
        Assert.Same(West, bareUrl);
        Assert.Null(bareLinks);

        var (nameOnly, _, roleOnly, _, _) = Standby;

        Assert.Equal("app-east", nameOnly);
        Assert.Equal("standby", roleOnly);
    }

    /// <summary>The words the node says about itself: its type, then its five parts in their order, each named and
    /// an absent part named and left blank - and the decision it makes of the role last, which the record says of
    /// its own although no caller wrote it. A link set says the type it is, the line never opening it. (The whole
    /// line stands here, so a part moved about in the record is a change the tests see.)</summary>
    [Fact]
    public void TheNodeSaysItsPartsInTheirOrder()
    {
        Assert.Equal(
            "NodeInfo { Name = app-west, Region = westus3, Role = primary, Url = https://app-west.example.net/, "
                + "Links = Dashboard.Health.LinkSet, IsPrimary = True }",
            Full.ToString());

        Assert.Equal(
            "NodeInfo { Name = app-east, Region = eastus2, Role = standby, Url = https://app-east.example.net/, "
                + "Links = Dashboard.Health.LinkSet, IsPrimary = False }",
            Standby.ToString());

        // The same line with the part a caller may leave out, named and saying nothing.
        Assert.Equal(
            "NodeInfo { Name = app, Region = westus3, Role = primary, Url = https://app-west.example.net/, "
                + "Links = , IsPrimary = True }",
            Bare.ToString());

        // And the node that holds nothing at all: every part named and every one of them saying nothing.
        Assert.Equal(
            "NodeInfo { Name = , Region = , Role = , Url = , Links = , IsPrimary = False }",
            new NodeInfo(null!, null, null!, null!).ToString());
    }

    // ------------------------------------------------------------------ the record's own shape

    /// <summary>The node's whole public surface as it stands: its five parts, named in the order the constructor
    /// names them and of the type the constructor says, the one member that decides anything
    /// (<see cref="IsPrimary" />), the two role words, and the members every record is asked - falling apart,
    /// comparing, hashing and saying itself - and nothing else to call. A sixth part added here has to be written
    /// down here too, which is the point: a node the page reads cannot carry a part nobody decided what it
    /// means.</summary>
    [Fact]
    public void TheNodeHasFivePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Name", "Region", "Role", "Url", "Links", "IsPrimary"],
            [.. typeof(NodeInfo)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [
                "Deconstruct", "Equals", "GetHashCode", "IsPrimary", "Links", "Name", "Region", "Role", "ToString",
                "Url", "op_Equality", "op_Inequality",
            ],
            PublicSurface());

        var properties = typeof(NodeInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(string), properties["Region"].PropertyType);
        Assert.Equal(typeof(string), properties["Role"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Url"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);
        Assert.Equal(typeof(bool), properties["IsPrimary"].PropertyType);

        // The one constructor, naming its five parts as the properties are named: a part may be left out only from
        // the fifth on, the four before it being what a node must be told.
        var parameters = typeof(NodeInfo).GetConstructors().Single().GetParameters();

        Assert.Equal(["Name", "Region", "Role", "Url", "Links"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false, false, false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);

        // The four a caller must name carry no default at all, and the one it may leave out defaults to nothing: the
        // record holds no link set of its own.
        Assert.All(parameters.Take(4), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.Null(parameters[4].DefaultValue);

        // The nullable annotations stand as the record declares them: the region and the links may be nothing, the
        // name, the role and the address may not - which is what the reader of the file leans on.
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, true, false, false, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        Assert.Equal([typeof(IEquatable<NodeInfo>)], typeof(NodeInfo).GetInterfaces());

        // The two role words are the only fields the record carries, and both are words.
        Assert.Equal(
            ["PrimaryRole", "StandbyRole"],
            [.. typeof(NodeInfo)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(field => field.Name)
                .Order(StringComparer.Ordinal)]);
    }

    // ------------------------------------------------------------------ the machinery

    /// <summary>The public members the node declares itself: its properties and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(NodeInfo)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
