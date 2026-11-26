using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeFrame" />: one frame of an environment's
/// runtime diagram that is no region - the subscription, a resource group, the Front Door profile, a plan, a cluster,
/// the boundary of an application with a runtime of its own - the three words it is made of and the fourth the
/// deployment may name, the links of its title. The record decides nothing of its own: no verdict, no words for the
/// page, no moment, no address, so what is pinned here is that each part comes back as the part it went in as, that
/// the record refuses nothing it is handed and derives nothing from what it is handed, and that the members every
/// record is asked compare, hash, fall apart, copy and say the four parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeManifestParserEdgeCaseTests" /> and
/// <see cref="RuntimeIndexFrameAndParseResultTests" /> pin the read out of a manifest: that a frame without an
/// <c>alias</c> stops the file, that a frame with no <c>kind</c> - or one that is no text, or an empty one - is read
/// with the empty word while a word of spaces is read trimmed, that a frame with no <c>name</c> is called by its
/// alias, and that only an absolute https address is kept as a link, the rest left out rather than refused.
/// <see cref="RuntimeIndexFrameAndParseResultTests" /> also pins the three words read back off a frame the test
/// brought, a link of it asked by its key, the absence of a key, and that two frames of the same three words and the
/// one link set are one frame while a fourth part taken away or a name changed makes two.
/// <see cref="RuntimeBoxLinkEdgeCaseTests" /> and <see cref="RuntimeWorkHelperCoverageTests" /> pin what a frame's
/// <see cref="Links" /> lead to and say: which of the six keys a frame is linked by, in what order, and what its
/// tooltip says of each kind - including the frame that is handed no links at all, the one of
/// <see cref="LinkSet.None" />, and one of a kind the dashboard has never heard.
/// </para>
/// <para>
/// What follows is the record on its own, at the ends those tests leave open: the four parts by the names of their
/// own properties and the position each was handed in, the word the record neither trims, nor lower-cases, nor falls
/// back from - the alias kept although the name stands another word, the name kept although it repeats the alias, the
/// empty word that is a word and the spaces and the letters that only look alike, the kind held as whatever word the
/// file spelled with no vocabulary to weigh it against, the links held as the very set they stand on and compared as
/// the set and not as their keys, the frame that refuses nothing - no alias, no kind, no name, no links - and the
/// whole public surface, so that the day a fifth part is added the list of them says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a frame holds three words and, where the
/// deployment recorded them, a hold of addresses it never opens, calls nothing at all, and every value below is one
/// the test brought.
/// </para>
/// </summary>
public class RuntimeFrameEdgeCaseTests
{
    private const string Alias = "sub";
    private const string Kind = "subscription";
    private const string Name = "demo";

    private static readonly Uri Portal = new("https://portal.example.net/#demo");
    private static readonly Uri AlsoPortal = new("https://portal.example.net/#demo");

    /// <summary>Two link sets of the one key and the one address: a frame opens neither of them, so which of the two
    /// it stands on is the whole of the difference between the frames of the tests that name them - the record
    /// weighing the hold and not what it holds.</summary>
    private static readonly LinkSet OneLink = new([new KeyValuePair<string, Uri>(LinkSet.Portal, Portal)]);

    private static readonly LinkSet OtherLink = new([new KeyValuePair<string, Uri>(LinkSet.Portal, AlsoPortal)]);

    /// <summary>A frame with every part named, the three words apart from one another and the links of a kind the
    /// deployment writes for a subscription, so that a part landing on another property is seen instead of quietly
    /// matching.</summary>
    private static RuntimeFrame Full => new(Alias, Kind, Name, OneLink);

    /// <summary>A frame named by nothing but the three a caller must name, the links standing as they stand when
    /// nobody names them - the frame of a manifest that records no link for its boundary.</summary>
    private static RuntimeFrame Bare => new(Alias, Kind, Name);

    /// <summary>The frame of nothing at all: no words, and no links where the deployment recorded none.</summary>
    private static RuntimeFrame Nothing => new(null!, null!, null!);

    /// <summary>The frame of empty words: a part said as nothing, which the record keeps as a word and not as the
    /// absence of one.</summary>
    private static RuntimeFrame Empty => new(string.Empty, string.Empty, string.Empty);

    /// <summary>A frame of the three words as a test names them, the links left to the test that means them.</summary>
    private static RuntimeFrame Boundary(string alias, string kind, string name) => new(alias, kind, name);

    /// <summary>A hold of links of the keys and the addresses a test brings, in the order it brings them.</summary>
    private static LinkSet Links(params (string Key, Uri Address)[] links) =>
        new(links.Select(link => new KeyValuePair<string, Uri>(link.Key, link.Address)));

    // ------------------------------------------------------- the four parts, by the names of their own properties

    /// <summary>The constructor's four parts, position by position, come back as the property of the same name - the
    /// order the reader of a manifest hands a frame over in, and the order the page writes a box out of. Each part
    /// keeps the value it was given and nothing about it is worked out of the others: a frame whose name is no word
    /// with its alias keeps both, a frame whose name repeats its alias is not the less a frame for it, and a frame of
    /// a kind nobody has heard of is a frame of that kind.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var frame = new RuntimeFrame(Alias, Kind, Name, OneLink);

        Assert.Equal(Alias, frame.Alias);
        Assert.Equal(Kind, frame.Kind);
        Assert.Equal(Name, frame.Name);
        Assert.Same(OneLink, frame.Links);

        // The fourth position is the links and no word: a caller that names them by position lands them on the part
        // the page reads a box's address out of, not on a fifth part of its own.
        Assert.Equal(Portal.AbsoluteUri, frame.Links![LinkSet.Portal]!.AbsoluteUri);

        // The alias is no part of the name and the name no part of the alias: the record falls back from neither to
        // the other, which is the reader's doing and is pinned with the reader.
        var crossed = Boundary("plan_primary", "plan", "asp-cmdemo1");

        Assert.Equal("plan_primary", crossed.Alias);
        Assert.Equal("asp-cmdemo1", crossed.Name);

        var repeated = Boundary("own_web", "runtime", "own_web");

        Assert.Equal("own_web", repeated.Alias);
        Assert.Equal("own_web", repeated.Name);
        Assert.Equal("runtime", repeated.Kind);

        // The three words of one and the same word are a frame, and a name of a space on it is another frame.
        var oneWord = Boundary("sub", "sub", "sub");

        Assert.Equal("sub", oneWord.Alias);
        Assert.Equal("sub", oneWord.Kind);
        Assert.Equal("sub", oneWord.Name);
        Assert.NotEqual(oneWord, Boundary("sub", "sub", "sub "));
    }

    /// <summary>Every word is kept exactly as it is handed: the empty word is a word and not nothing, the spaces
    /// about a word stay about it, a word of one letter is a word, and a word of letters that only look like the
    /// Latin ones is kept letter for letter. (That the file's words arrive trimmed, and that a frame the reader built
    /// has a kind and a name whether the file gave them or not, is the reader's and is pinned with the reader.)</summary>
    [Fact]
    public void AFrameKeepsItsWordsExactlyAsTheyAreGiven()
    {
        var empty = Empty;

        Assert.Equal(string.Empty, empty.Alias);
        Assert.Equal(string.Empty, empty.Kind);
        Assert.Equal(string.Empty, empty.Name);

        var oneLetter = new RuntimeFrame("a", "b", "c", LinkSet.None);

        Assert.Equal("a", oneLetter.Alias);
        Assert.Equal("b", oneLetter.Kind);
        Assert.Equal("c", oneLetter.Name);

        var spaced = new RuntimeFrame("  sub  ", " subscription ", "  demo  ", OneLink);

        Assert.Equal("  sub  ", spaced.Alias);
        Assert.Equal(" subscription ", spaced.Kind);
        Assert.Equal("  demo  ", spaced.Name);

        var written = new RuntimeFrame("ｓｕｂ", "subscrīption", "démo 日本", OneLink);

        Assert.Equal("ｓｕｂ", written.Alias);
        Assert.Equal("subscrīption", written.Kind);
        Assert.Equal("démo 日本", written.Name);

        // A word of markup and a word of a thousand letters are held as they stand: the record escapes nothing and
        // counts nothing, the page says what it says of whatever it is handed.
        var marked = Boundary("sub", "<img src=x onerror=alert(1)>", "demo");

        Assert.Equal("<img src=x onerror=alert(1)>", marked.Kind);

        var longWord = new string('x', 1_000);

        Assert.Equal(longWord, new RuntimeFrame(longWord, Kind, Name, null).Alias);
        Assert.Equal(longWord, new RuntimeFrame(Alias, longWord, Name, null).Kind);
        Assert.Equal(longWord, new RuntimeFrame(Alias, Kind, longWord, null).Name);
    }

    /// <summary>The record makes no decision of its own about what a part may be: a frame will hold no words at all,
    /// since it is the file that is read and the reader that refuses one. Only the links may be nothing in the type;
    /// the three words are non-nullable in it and the caller that hands the record a null of one finds it kept.</summary>
    [Fact]
    public void AFrameRefusesNothingItIsGiven()
    {
        var nothing = Nothing;

        Assert.Null(nothing.Alias);
        Assert.Null(nothing.Kind);
        Assert.Null(nothing.Name);
        Assert.Null(nothing.Links);

        // Nothing it is not told, and empty words it is told, are two frames: the absence of a word is no word.
        Assert.NotEqual(nothing, Empty);
        Assert.True(nothing != Empty);
        Assert.False(nothing.Equals(Empty));

        // Two frames of nothing at all, made by two calls, are one frame.
        Assert.Equal(nothing, new RuntimeFrame(null!, null!, null!, null));
        Assert.Single(new HashSet<RuntimeFrame> { nothing, new RuntimeFrame(null!, null!, null!, null) });

        // The alias is the word the page finds a drawn element by; the record still does not insist on one, nor on a
        // kind to say what the frame is, nor on a name to draw inside it.
        Assert.Null(new RuntimeFrame(null!, Kind, Name).Alias);
        Assert.Null(new RuntimeFrame(Alias, null!, Name).Kind);
        Assert.Null(new RuntimeFrame(Alias, Kind, null!).Name);
    }

    /// <summary>The links are the one part a caller may leave out, and naming them as nothing is the same frame as
    /// not naming them at all. The links that are there and hold nothing are another frame: a boundary the deployment
    /// linked with no address it knows still stands on a hold, and the page that asks it for a key is asking a
    /// present part rather than an absent one.</summary>
    [Fact]
    public void TheLinksThatAreNotThereAndTheLinksThatHoldNothingAreTwoParts()
    {
        Assert.Null(Bare.Links);
        Assert.Null(new RuntimeFrame(Alias, Kind, Name, null).Links);
        Assert.Null(Empty.Links);
        Assert.Null(Nothing.Links);

        // The absence of the part is the absence of a link, and the part named as nothing is the part not named.
        Assert.Equal(Bare, Bare with { Links = null });
        Assert.Equal(Bare.GetHashCode(), (Bare with { Links = null }).GetHashCode());
        Assert.Single(new HashSet<RuntimeFrame> { Bare, new RuntimeFrame(Alias, Kind, Name, null) });

        // The empty hold is a hold: two frames standing on the one empty hold the app hands out are one frame, and
        // neither is the frame that stands on nothing. (What <see cref="RuntimeBoxLink" /> makes of the two is
        // pinned there.)
        Assert.Empty((Bare with { Links = LinkSet.None }).Links!.Keys);
        Assert.Equal(Bare with { Links = LinkSet.None }, Bare with { Links = LinkSet.None });
        Assert.NotEqual(Bare, Bare with { Links = LinkSet.None });
        Assert.Equal(2, new HashSet<RuntimeFrame> { Bare, Bare with { Links = LinkSet.None } }.Count);

        // Two frames standing on two empty holds of their own are two frames: it is the hold that is weighed, and not
        // the nothing a hold of one's own happens to hold.
        Assert.NotEqual(new RuntimeFrame(Alias, Kind, Name, Links()), new RuntimeFrame(Alias, Kind, Name, Links()));

        // A hold of one key is neither the empty hold nor no hold.
        Assert.NotEqual(Bare with { Links = LinkSet.None }, Full);
    }

    // ------------------------------------------------------------------- the word of a kind, with none to weigh it

    /// <summary>The six words the deployment writes for a frame's kind, and six it has never written, are held as
    /// they are handed and said as they stand: the record has no vocabulary to compare a kind against, no word it
    /// prefers and none it refuses - the switch that turns a kind into the title of a box is
    /// <see cref="RuntimeBoxLink" />'s and is pinned there, and the word a file spells for a frame is the reader's.
    /// Two frames of one kind word are one frame; two frames of words that differ by case, by a space or by nothing
    /// at all are two.</summary>
    [Theory]
    [InlineData("subscription")]
    [InlineData("resourceGroup")]
    [InlineData("frontDoorProfile")]
    [InlineData("plan")]
    [InlineData("cluster")]
    [InlineData("runtime")]
    [InlineData("database")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("SUBSCRIPTION")]
    [InlineData("Subscription")]
    [InlineData(" subscription")]
    public void EveryWordOfAKindIsHeldAndSaidAsItIsGiven(string kind)
    {
        var frame = new RuntimeFrame(Alias, kind, Name, OneLink);

        Assert.Equal(kind, frame.Kind);
        Assert.Contains($"Kind = {kind}", frame.ToString(), StringComparison.Ordinal);
        Assert.Equal(frame, new RuntimeFrame(Alias, kind, Name, OneLink));
        Assert.Equal(frame.GetHashCode(), new RuntimeFrame(Alias, kind, Name, OneLink).GetHashCode());
        Assert.Single(new HashSet<RuntimeFrame> { frame, new RuntimeFrame(Alias, kind, Name, OneLink) });

        // A word of a kind is no part of the frame's other two: the page may say "the AKS cluster aks-demo" of a
        // cluster and nothing here has moved.
        Assert.Equal(Alias, frame.Alias);
        Assert.Equal(Name, frame.Name);
        Assert.Same(OneLink, frame.Links);

        // A kind is weighed as a word and not as a decision: the very word with a space on the end is another kind
        // to the record, however a page that asks case-free and trims would read the two of them.
        Assert.NotEqual(frame, new RuntimeFrame(Alias, kind + " ", Name, OneLink));
    }

    /// <summary>The six words the deployment writes for a frame's kind, each weighed against a spelling a reader who
    /// asked case-free would call the same word: two frames of the one boundary named by two such spellings are two
    /// frames, two entries of a set, and said apart. (That the reader trims the word it reads is pinned with the
    /// reader; what <see cref="RuntimeBoxLink" /> makes of a kind it has never heard is pinned there.)</summary>
    [Theory]
    [InlineData("subscription", "SUBSCRIPTION")]
    [InlineData("subscription", "Subscription")]
    [InlineData("resourceGroup", "resourcegroup")]
    [InlineData("frontDoorProfile", "FrontDoorProfile")]
    [InlineData("plan", "PLAN")]
    [InlineData("cluster", "Cluster")]
    [InlineData("runtime", "runtıme")]
    [InlineData("runtime", "ｒｕｎｔｉｍｅ")]
    public void TwoWordsThatDifferByCaseOrLetterAreTwoKinds(string kind, string spelledAnotherWay)
    {
        var one = new RuntimeFrame(Alias, kind, Name, OneLink);
        var two = new RuntimeFrame(Alias, spelledAnotherWay, Name, OneLink);

        Assert.NotEqual(one, two);
        Assert.True(one != two);
        Assert.False(one.Equals((object)two));
        Assert.Equal(2, new HashSet<RuntimeFrame> { one, two }.Count);
        Assert.Equal(Alias, two.Alias);
        Assert.Equal(Name, two.Name);
    }

    /// <summary>The frame of no kind at all - the empty word, which is what a manifest that names none is read with
    /// (pinned with the reader) - stands as a frame, says its blank, and is a frame apart from the one of the word
    /// the deployment would have written. The nothing of a word and the empty word are two kinds.</summary>
    [Fact]
    public void AFrameOfNoKindIsAFrameAndIsAPartFromTheFrameOfTheEmptyWord()
    {
        var none = Boundary("f", null!, Name);
        var blank = Boundary("f", string.Empty, Name);
        var spaces = Boundary("f", " ", Name);
        var known = Boundary("f", "plan", Name);

        Assert.Null(none.Kind);
        Assert.Equal(string.Empty, blank.Kind);
        Assert.Equal(" ", spaces.Kind);

        Assert.NotEqual(none, blank);
        Assert.NotEqual(blank, spaces);
        Assert.NotEqual(blank, known);
        Assert.Equal(4, new HashSet<RuntimeFrame> { none, blank, spaces, known }.Count);

        Assert.Equal("RuntimeFrame { Alias = f, Kind = , Name = demo, Links =  }", blank.ToString());
        Assert.Equal("RuntimeFrame { Alias = f, Kind = , Name = demo, Links =  }", none.ToString());
    }

    // ------------------------------------------------------------- the links a frame holds, as the hold they stand on

    /// <summary>The links a frame holds are compared as the hold they stand on and not as their keys: the very same
    /// link set makes two frames one, and a second set of the one and the same key and the one and the same address
    /// makes two frames of them - the record opening neither, the reader of the page asking the set it was handed.
    /// What the six keys of a set lead a frame to is <see cref="RuntimeBoxLink" />'s and is pinned there.</summary>
    [Fact]
    public void TheLinksAFrameHoldsAreComparedAsTheHoldTheyStandOn()
    {
        var one = Full;
        var holdingTheSameLinks = new RuntimeFrame(Alias, Kind, Name, OneLink);
        var holdingOtherLinks = Full with { Links = OtherLink };

        Assert.Same(OneLink, holdingTheSameLinks.Links);
        Assert.Equal(one, holdingTheSameLinks);
        Assert.Equal(one.GetHashCode(), holdingTheSameLinks.GetHashCode());
        Assert.Single(new HashSet<RuntimeFrame> { one, holdingTheSameLinks });

        // Two sets of the same key and the same address: the addresses ask as addresses, the sets do not ask at all.
        Assert.Equal(one.Links!.Keys, holdingOtherLinks.Links!.Keys);
        Assert.Equal(one.Links[LinkSet.Portal], holdingOtherLinks.Links![LinkSet.Portal]);
        Assert.NotSame(one.Links, holdingOtherLinks.Links);
        Assert.True(one != holdingOtherLinks);
        Assert.NotEqual(one, holdingOtherLinks);
        Assert.Equal(2, new HashSet<RuntimeFrame> { one, holdingOtherLinks }.Count);

        // A copy names the part again and the frame holds that set instead, by reference as it held the first; the
        // frame it was copied from stands on the set it was handed.
        var several = Links((LinkSet.Portal, AlsoPortal), (RuntimeBoxLink.Resources, Portal));
        var moved = one with { Links = several };

        Assert.Same(several, moved.Links);
        Assert.Same(OneLink, one.Links);
        Assert.Equal(2, moved.Links!.Count);
        Assert.Contains(LinkSet.Portal, moved.Links.Keys);
        Assert.Contains(RuntimeBoxLink.Resources, moved.Links.Keys);
        Assert.NotEqual(one, moved);
        Assert.Equal(one, moved with { Links = OneLink });
    }

    /// <summary>A frame that holds links and a frame that holds none are two frames whatever the three words, and the
    /// links of a frame of nothing at all are still nothing: a part put on is as much a change as a part put off, and
    /// the empty frame of a hold of links is no frame of nothing.</summary>
    [Fact]
    public void AFrameDiffersFromItselfByItsLinksAlone()
    {
        Assert.NotEqual(Empty, Empty with { Links = LinkSet.None });
        Assert.NotEqual(Nothing, Nothing with { Links = OneLink });
        Assert.Equal(Nothing, Nothing with { Links = null });
        Assert.NotEqual(
            new RuntimeFrame(null!, null!, null!, LinkSet.None),
            new RuntimeFrame(null!, null!, null!, OtherLink));
        Assert.Equal(
            new RuntimeFrame(null!, null!, null!, LinkSet.None),
            new RuntimeFrame(null!, null!, null!, LinkSet.None));
    }

    /// <summary>A frame holds the very link set it was handed - not a copy of it, not one built out of its entries,
    /// and not an empty one where the caller named none - and asks nothing of what stands in it: an address goes in as
    /// the <see cref="Uri" /> it was given and comes back as that same one, whatever the key it was put under or the
    /// scheme it carries. Which keys a frame is read by, and where each leads, is <see cref="RuntimeBoxLink" />'s and
    /// is pinned there.</summary>
    [Fact]
    public void AFrameHoldsTheLinksItIsHandedAndAsksNothingOfThem()
    {
        var held = Links((RuntimeBoxLink.Site, Portal), ("noSuchKey", AlsoPortal));
        var frame = new RuntimeFrame(Alias, Kind, Name, held);

        Assert.Same(held, frame.Links);
        Assert.Same(Portal, frame.Links![RuntimeBoxLink.Site]);
        Assert.Same(AlsoPortal, frame.Links["noSuchKey"]);
        Assert.Equal(2, frame.Links.Count);

        // A frame whose links are named as nothing holds the nothing it was named, since the record builds no empty
        // hold of its own and refuses none it is handed.
        Assert.Null(new RuntimeFrame(Alias, Kind, Name, null).Links);
        Assert.Null(Boundary(Alias, Kind, Name).Links);
        Assert.NotSame(held, new RuntimeFrame(Alias, Kind, Name, Links((RuntimeBoxLink.Site, Portal), ("noSuchKey", AlsoPortal))).Links);
    }

    // ------------------------------------------------------------------------ the record of the four parts

    /// <summary>Two frames are one while their four parts are, whichever of the ways of asking is used: the record's
    /// own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, and the hash that follows them
    /// and stands from call to call. A frame is compared, not looked through: two calls on the same three words and
    /// the same link set are one frame, and a set of them holds them once.</summary>
    [Fact]
    public void TwoFramesAreOneWhileTheirFourPartsAre()
    {
        var one = Full;
        var two = new RuntimeFrame(Alias, Kind, Name, OneLink);

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
        Assert.Single(new HashSet<RuntimeFrame> { one, two });

        // The parts asked out and put back in make the same frame again: the equality is of the values, not of the
        // call that gathered them.
        one.Deconstruct(out var alias, out var kind, out var name, out var links);

        Assert.Equal(one, new RuntimeFrame(alias, kind, name, links));
        Assert.Equal(one.GetHashCode(), new RuntimeFrame(alias, kind, name, links).GetHashCode());
    }

    /// <summary>Any one of the four parts tells two frames apart, and nothing is compared loosely: the words
    /// ordinally, so a word in another case, or with a space on it, or with a letter that only looks like the one
    /// expected, is another word. The empty word, the word of spaces and the nothing that is no word are three parts
    /// of their own, and the links are weighed by the hold they stand on.</summary>
    [Fact]
    public void AFrameDiffersInAnyOneOfItsFourParts()
    {
        var frame = Full;

        Assert.NotEqual(frame, frame with { Alias = string.Empty });
        Assert.NotEqual(frame, frame with { Alias = "  " });
        Assert.NotEqual(frame, frame with { Alias = null! });
        Assert.NotEqual(frame, frame with { Alias = "SUB" });
        Assert.NotEqual(frame, frame with { Alias = "sub " });
        Assert.NotEqual(frame, frame with { Alias = "afd" });
        Assert.NotEqual(frame, frame with { Kind = string.Empty });
        Assert.NotEqual(frame, frame with { Kind = null! });
        Assert.NotEqual(frame, frame with { Kind = "Subscription" });
        Assert.NotEqual(frame, frame with { Kind = "resourceGroup" });
        Assert.NotEqual(frame, frame with { Name = string.Empty });
        Assert.NotEqual(frame, frame with { Name = null! });
        Assert.NotEqual(frame, frame with { Name = "Demo" });
        Assert.NotEqual(frame, frame with { Name = "demo prod" });
        Assert.NotEqual(frame, frame with { Name = "ｄｅｍｏ" });
        Assert.NotEqual(frame, frame with { Links = null });
        Assert.NotEqual(frame, frame with { Links = LinkSet.None });
        Assert.NotEqual(frame, frame with { Links = OtherLink });

        // The empty word and the nothing are two parts, and a word of spaces is a third.
        Assert.NotEqual(new RuntimeFrame(string.Empty, Kind, Name), new RuntimeFrame(null!, Kind, Name));
        Assert.NotEqual(new RuntimeFrame(" ", Kind, Name), new RuntimeFrame(null!, Kind, Name));

        // The frames that differ from this one by one part are four apart.
        Assert.Equal(
            4,
            new HashSet<RuntimeFrame>
            {
                frame,
                frame with { Alias = "sub-sub" },
                frame with { Kind = "plan" },
                frame with { Name = "demo-uat" },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two frames that are no frame at
    /// all are one, a frame and no frame are two - the equality and the inequality each saying it, from either side -
    /// and the frame asked about itself says no to the nothing, to the word it is named by, to the hold of links it
    /// stands on and to the other element of a diagram that is made of the same three words. The frame of nothing at
    /// all is still a frame.</summary>
    [Fact]
    public void NoFrameIsOneWithNoFrameAndTheFrameOfNothingIsAFrame()
    {
        RuntimeFrame? nothing = null;
        RuntimeFrame? alsoNothing = null;
        var there = Full;

        Assert.Null(default(RuntimeFrame));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((RuntimeFrame?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(Alias));
        Assert.False(there.Equals(OneLink));
        Assert.False(there.Equals(Portal));
        Assert.False(there.Equals(new RuntimeRegion(Alias, Name, [], OneLink)));
        Assert.False(there.Equals(new RuntimeNode(Alias, RuntimeNodeKind.Other, Name, Links: OneLink)));
        Assert.False(there.Equals(new RuntimeIndexEntry(Alias, "demo.json", "demo.svg")));
        Assert.False(there.Equals(new RuntimeManifest(Name, [], [], [], [there])));

        var alsoEmpty = Empty;

        Assert.Equal(Empty, alsoEmpty);
        Assert.NotSame(Empty, alsoEmpty);
        Assert.Equal(Empty.GetHashCode(), alsoEmpty.GetHashCode());
        Assert.True(Empty == alsoEmpty);
        Assert.Single(new HashSet<RuntimeFrame> { Empty, alsoEmpty });
    }

    /// <summary>A frame is one with itself only while its alias is: an alias held as it was written, one of another
    /// case, one with a space on it, one that repeats the frame's own name, the empty word, a word of spaces and no
    /// word at all are seven aliases the record holds without a word of objection, and only the one that repeats what
    /// the constructor was given makes the very frame it was copied from. The alias is how the page finds a drawn
    /// element again and how the reader falls back to a name; neither is the record's.</summary>
    [Theory]
    [InlineData("sub", true)]
    [InlineData("SUB", false)]
    [InlineData(" sub", false)]
    [InlineData("subscription", false)]
    [InlineData("demo", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void TheAliasIsHeldAsItIsWrittenAndNeverCheckedAgainstTheOtherThree(string? alias, bool isTheVeryFrame)
    {
        var frame = Full with { Alias = alias! };

        Assert.Equal(alias, frame.Alias);
        Assert.Equal(Kind, frame.Kind);
        Assert.Equal(Name, frame.Name);
        Assert.Same(OneLink, frame.Links);
        Assert.Contains($"Alias = {alias}", frame.ToString(), StringComparison.Ordinal);
        Assert.Equal(isTheVeryFrame, frame == Full);
    }

    // ------------------------------------------------------------------------------------------ copies

    /// <summary><c>with</c> names the part it changes and carries the other three: a copy that names nothing is one
    /// with its original in another reference, the original is left where it stood, the fourth part may be put on and
    /// taken off as the three words are, and a copy of a frame of three words the same apart is the same frame -
    /// which is the record's point, and the reason nothing here checks a word against another.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var frame = Full;
        var copy = frame with { };

        Assert.Equal(frame, copy);
        Assert.NotSame(frame, copy);
        Assert.True(frame == copy);

        var renamed = frame with { Name = "demo prod" };

        Assert.Equal("demo prod", renamed.Name);
        Assert.Equal(Alias, renamed.Alias);
        Assert.Equal(Kind, renamed.Kind);
        Assert.Same(OneLink, renamed.Links);
        Assert.Equal(Name, frame.Name);
        Assert.NotEqual(frame, renamed);

        var unlinked = frame with { Links = null, Kind = string.Empty };

        Assert.Null(unlinked.Links);
        Assert.Equal(string.Empty, unlinked.Kind);
        Assert.Equal(Alias, unlinked.Alias);
        Assert.Equal(Name, unlinked.Name);
        Assert.Same(OneLink, frame.Links);
        Assert.Equal(Kind, frame.Kind);

        // A copy that names the parts the frame already holds is the frame, and a copy of the frame of nothing is the
        // frame of nothing.
        Assert.Equal(frame, frame with { Alias = Alias, Kind = Kind, Name = Name, Links = OneLink });
        Assert.Equal(Nothing, Nothing with { Alias = null!, Kind = null!, Name = null!, Links = null });
    }

    /// <summary>The four parts fall out of a frame in the order they went in, each as the value it holds, and what
    /// came out goes back in the same way; the part the frame was not told falls out as the nothing it is, and the
    /// frame that was told nothing at all is a frame all the same, links and all.</summary>
    [Fact]
    public void TheFourPartsFallOutOfAFrameInTheOrderTheyWentIn()
    {
        var (alias, kind, name, links) = Full;

        Assert.Equal(Alias, alias);
        Assert.Equal(Kind, kind);
        Assert.Equal(Name, name);
        Assert.Same(OneLink, links);

        var bare = Bare;
        bare.Deconstruct(out var bareAlias, out var bareKind, out var bareName, out var bareLinks);

        Assert.Equal(Alias, bareAlias);
        Assert.Equal(Kind, bareKind);
        Assert.Equal(Name, bareName);
        Assert.Null(bareLinks);
        Assert.Equal(bare, new RuntimeFrame(bareAlias, bareKind, bareName, bareLinks));

        var (nothingAlias, nothingKind, nothingName, nothingLinks) = Nothing;

        Assert.Null(nothingAlias);
        Assert.Null(nothingKind);
        Assert.Null(nothingName);
        Assert.Null(nothingLinks);
        Assert.Equal(Nothing, new RuntimeFrame(nothingAlias!, nothingKind!, nothingName!, nothingLinks));

        // One part asked out of the four, the rest of them left where they are.
        var (only, _, _, _) = Boundary("rg_tier", "resourceGroup", "rg-cmdemo1-nonprod");

        Assert.Equal("rg_tier", only);

        // The links fall out whole, and the set that comes out is the set that went in.
        var several = Links((LinkSet.Portal, Portal), (RuntimeBoxLink.Namespaces, AlsoPortal));
        var (_, _, _, theLinks) = new RuntimeFrame(Alias, Kind, Name, several);

        Assert.Same(several, theLinks);
        Assert.Equal(2, theLinks!.Count);
        Assert.All([.. theLinks.Keys], key => Assert.NotNull(theLinks[key]));
    }

    // --------------------------------------------------------------------------------------------- words

    /// <summary>The words a frame says about itself - its name, then its four parts in the order they are declared,
    /// each named and standing blank when there is nothing of it - are the record's own and not the page's: the page
    /// is given its words by the payload builder and by <see cref="RuntimeBoxLink" />. A link set says itself as the
    /// type it stands on, since the record never opens it, and no part is put in quotes. The whole frame stands here,
    /// so a part moved about in the record is a change the tests see.</summary>
    [Fact]
    public void AFrameSaysItsFourPartsInTheirOrder()
    {
        Assert.Equal(
            "RuntimeFrame { Alias = sub, Kind = subscription, Name = demo, Links = Dashboard.Health.LinkSet }",
            Full.ToString());

        // The part a caller may leave out is named and says nothing.
        Assert.Equal(
            "RuntimeFrame { Alias = sub, Kind = subscription, Name = demo, Links =  }",
            Bare.ToString());

        Assert.Equal(
            "RuntimeFrame { Alias = , Kind = , Name = , Links =  }",
            Empty.ToString());
        Assert.Equal(
            "RuntimeFrame { Alias = , Kind = , Name = , Links =  }",
            Nothing.ToString());

        // The links that hold nothing still say the type they stand on: the words tell an empty hold from no hold.
        Assert.Equal(
            "RuntimeFrame { Alias = sub, Kind = subscription, Name = demo, Links = Dashboard.Health.LinkSet }",
            (Bare with { Links = LinkSet.None }).ToString());

        // A word is said as it stands, its spaces and its markup all, and a word of a thousand letters is said whole.
        Assert.Equal(
            "RuntimeFrame { Alias =  sub , Kind =  subscription , Name =  demo , Links =  }",
            new RuntimeFrame(" sub ", " subscription ", " demo ").ToString());
        Assert.StartsWith(
            "RuntimeFrame { Alias = <img src=x>, ",
            Boundary("<img src=x>", "plan", "asp").ToString()!,
            StringComparison.Ordinal);
        Assert.Contains($"Name = {new string('x', 1_000)}", Boundary(Alias, Kind, new string('x', 1_000)).ToString(), StringComparison.Ordinal);
    }

    /// <summary>A frame's words are nothing but words, so no reader's culture turns them: a viewer of German is told
    /// the same frame as the invariant reader - the same alias, the same kind, the same name, the same type of a link
    /// set - which is what a log of one of these frames may count on whatever the reader of the machine happens to
    /// be.</summary>
    [Fact]
    public void TheFramesOwnWordsAreTheSameWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(() =>
        {
            foreach (var frame in new[] { Full, Bare, Empty, Nothing, Boundary("f", "cluster", "aks-demo") })
            {
                Assert.Equal(Words(frame), frame.ToString());
            }

            Assert.Equal(
                "RuntimeFrame { Alias = sub, Kind = subscription, Name = demo, Links = Dashboard.Health.LinkSet }",
                Full.ToString());
        });
    }

    /// <summary>The parts are weighed as words and not as a reader spells them: two frames whose words differ only in
    /// case are two frames whatever culture the thread runs under, and the words that are one word are one frame
    /// under another reader too - the comparison is ordinal, as the manifest's words are. The dotted and the dotless
    /// <c>i</c>, which one reader's capital of a small letter turns into the other's, are two kinds here.</summary>
    [Fact]
    public void TheWordsOfAFrameAreWeighedOrdinallyWhateverTheReaderSpeaks()
    {
        var upper = new RuntimeFrame("SUB", "SUBSCRIPTION", "DEMO", OneLink);

        ViewerCulture.German(() =>
        {
            Assert.NotEqual(Full, upper);
            Assert.True(Full != upper);
            Assert.False(Full.Equals((object)upper));
            Assert.Equal(Full, new RuntimeFrame(Alias, Kind, Name, OneLink));
        });

        Assert.NotEqual(Full, Full with { Alias = "ｓｕｂ" });
        Assert.NotEqual(Full, Full with { Kind = "Subscription" });

        var dotted = Boundary("ist", "istPlan", "ist");
        var dotless = Boundary("ist", "ıstPlan", "ist");

        Assert.NotEqual(dotted, dotless);
        Assert.NotEqual(dotted with { Alias = "İst" }, dotted);
    }

    // ---------------------------------------------------------------------------- the record's own shape

    /// <summary>The frame's whole public surface as it stands: its four parts, named in the order the constructor
    /// names them and of the type the constructor says, the three a caller must name and the one it may leave out as
    /// nothing, and the members every record is asked - falling apart, comparing, hashing and saying itself - and
    /// nothing else to call. There is no verdict on a frame to read and none to set: what a boundary is comes from
    /// the word of its kind and the links it stands on, and <see cref="RuntimeBoxLink" /> decides from them. A fifth
    /// part added to the record has to be written down here as well, which is the point: a frame of a diagram cannot
    /// carry a part nobody decided what it means.</summary>
    [Fact]
    public void TheFrameHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Alias", "Kind", "Name", "Links"],
            [.. typeof(RuntimeFrame)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Alias", "Deconstruct", "Equals", "GetHashCode", "Kind", "Links", "Name", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        var properties = typeof(RuntimeFrame)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Alias"].PropertyType);
        Assert.Equal(typeof(string), properties["Kind"].PropertyType);
        Assert.Equal(typeof(string), properties["Name"].PropertyType);
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
        var constructors = typeof(RuntimeFrame).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Alias", "Kind", "Name", "Links"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(LinkSet)],
            [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.Equal([false, false, false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.Null(parameters[3].DefaultValue);

        // The nullable annotations stand as the record declares them: the links may be nothing, the three words may
        // not - which is what the reader of the file leans on, and what the record itself does not enforce (a caller
        // may still hand it the nothing of a word, pinned above).
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        List<PropertyInfo> parts = [properties["Alias"], properties["Kind"], properties["Name"], properties["Links"]];

        Assert.Equal(
            [false, false, false, true],
            [.. parts.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        Assert.Equal([typeof(IEquatable<RuntimeFrame>)], typeof(RuntimeFrame).GetInterfaces());
        Assert.True(typeof(RuntimeFrame).IsSealed);
        Assert.Equal(typeof(object), typeof(RuntimeFrame).BaseType);
        Assert.Empty(typeof(RuntimeFrame).GetEvents());
        Assert.Empty(typeof(RuntimeFrame).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(RuntimeFrame).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The words a frame says about itself, taken in the invariant culture whatever the reader of this test
    /// happens to be: no part of a frame is a number a reader spells, so these are the words every reader is
    /// given.</summary>
    private static string Words(RuntimeFrame frame) => ViewerCulture.Invariant(() => frame.ToString()!);

    /// <summary>The public members the frame declares itself: its parts and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeFrame)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
