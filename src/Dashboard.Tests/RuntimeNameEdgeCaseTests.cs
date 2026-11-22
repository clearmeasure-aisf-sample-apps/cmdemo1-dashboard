using System.Reflection;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeName" />: one box of the runtime diagram that
/// has a name and nothing else to update - a frame that is no region, or the browser - the word the script keys the box
/// by and the link its name leads along. The record decides nothing of its own: no address, no words, no verdict. What
/// is pinned here is that each of the two parts comes back as the part it went in as, that the record refuses nothing
/// it is handed - no alias, and no link although the type says it always has one - that the link is <em>held</em> as
/// the very reference it was handed and yet weighed as the value it is, and that the members every record is asked
/// compare, hash, fall apart, copy and say the two parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeTrendAndNameTests" /> pins that a name carries its
/// alias and the href and the title of the link of its name, that two names of the same box and the same link are one
/// name while another alias, another title or another href makes another name, the two property names, that a name of
/// no alias and no words stands as it is and is written <c>{"alias":"","nameLink":{"href":"","title":""}}</c>, the JSON
/// the script reads, that an empty list of names is written and no list of them left out, and that the builder names
/// every box of a link and leaves the ones without one out. <see cref="RuntimeBoxLinkEdgeCaseTests" /> pins what the
/// link a box is named by leads to and what it is called.
/// </para>
/// <para>
/// What follows is the record on its own, at the ends those tests leave open: the two parts by the names of their own
/// properties and position by position, the spaces and the letters that only look alike, the nothing of either part
/// kept as the nothing it is, the hash and the set and the dictionary that follow the value of the link rather than
/// its reference, the two ways a name is unlike every other name, <c>with</c>, the falling apart, the words a name says
/// about itself, the whole public surface, and the JSON at the ends where a part is nothing and where the words are
/// letters the writer escapes - so that the day a third part is added the list of them says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a name holds a word and a link, calls nothing at
/// all, and every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimeNameEdgeCaseTests
{
    private const string Alias = "sub";
    private const string Href = "https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000";
    private const string Title = "The Azure subscription in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)";

    /// <summary>The words the record says about a name that has both its parts: its own name, then the two of them in
    /// the order they are declared, each named - and the link saying itself as the link of the same two parts.</summary>
    private const string FullLine =
        "RuntimeName { Alias = sub, NameLink = RuntimeLink { Href = https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000, Title = The Azure subscription in the Azure portal (opens in a new tab; the Azure portal asks you to sign in) } }";

    /// <summary>The words of the name of nothing at all: both parts named, neither saying anything.</summary>
    private const string NothingLine = "RuntimeName { Alias = , NameLink =  }";

    /// <summary>The one link a box's name takes: its portal page. Every fixture below stands on it, so that the
    /// fixtures are copies of one name rather than names that only look alike - which is the part's behaviour, and is
    /// pinned for itself further down.</summary>
    private static readonly RuntimeLink Portal = new(Href, Title);

    /// <summary>A name with both its parts named, the two of them apart from one another, so that a part landing on
    /// the other property is seen instead of quietly matching.</summary>
    private static RuntimeName Full => new(Alias, Portal);

    /// <summary>The name of nothing at all: no word and no link - the two parts the record will hold although the
    /// manifest's reader refuses a box of either.</summary>
    private static RuntimeName Nothing => new(null!, null!);

    /// <summary>The name of empty parts: a word said as nothing, which the record keeps as a word and not as the
    /// absence of one, and a link of no address and no words, which is a link.</summary>
    private static RuntimeName Empty => new(string.Empty, new RuntimeLink(string.Empty, string.Empty));

    /// <summary>The payload of one box that is nothing but a name, as the script is handed it.</summary>
    private static string PayloadJson(RuntimeName name) => new RuntimePayload([], [], [], [name]).ToJson();

    /// <summary>The words the script is handed for one name - the one entry of the payload's list of them.</summary>
    private static string DrawnJson(RuntimeName name)
    {
        using var json = JsonDocument.Parse(PayloadJson(name));
        return json.RootElement.GetProperty("names")[0].GetRawText();
    }

    // -------------------------------------------------------- the two parts, by the names of their own properties

    /// <summary>The constructor's two parts, position by position, come back as the property of the same name - the
    /// order the builder writes a name over in. Each part keeps the value it was given and nothing about it is worked
    /// out of the other: a box named <c>rg_tier</c> whose name leads to a subscription's portal page keeps both words,
    /// and the record puts neither the alias into the address nor the address into the alias.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var name = Full;

        Assert.Equal(Alias, name.Alias);
        Assert.Same(Portal, name.NameLink);
        Assert.Equal(Href, name.NameLink.Href);
        Assert.Equal(Title, name.NameLink.Title);

        // The browser is named by the site it opens, and the frame by the portal page of the thing it is drawn about.
        var browser = new RuntimeName("browser", new RuntimeLink("https://demo.example.net", "Browser: demo.example.net (opens in a new tab)"));

        Assert.Equal("browser", browser.Alias);
        Assert.Equal("https://demo.example.net", browser.NameLink.Href);
        Assert.Equal("Browser: demo.example.net (opens in a new tab)", browser.NameLink.Title);

        // The alias is the box's word in the diagram, the href the address outside it: neither says the other.
        var crossed = new RuntimeName("rg_tier", Portal);

        Assert.Equal("rg_tier", crossed.Alias);
        Assert.Equal(Href, crossed.NameLink.Href);
        Assert.NotEqual(crossed.Alias, crossed.NameLink.Href);
        Assert.NotEqual(crossed, Full);
    }

    /// <summary>The alias is held as it is written and is never weighed loosely: another case, a space on either end,
    /// one letter instead of three, a letter that only looks like the Latin one, the empty word, the word of spaces and
    /// the nothing are each a part of their own, and only the very word makes the name the name it was copied from.
    /// That the manifest's reader refuses an alias that is empty, and trims the ones the file gives, is the reader's.</summary>
    [Theory]
    [InlineData("sub", true)]
    [InlineData("SUB", false)]
    [InlineData("Sub", false)]
    [InlineData(" sub", false)]
    [InlineData("sub ", false)]
    [InlineData("s", false)]
    [InlineData("sub_", false)]
    [InlineData("subscription", false)]
    [InlineData("ｓｕｂ", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void TheAliasIsHeldAsItIsWrittenAndNeverWeighedLoosely(string? alias, bool isTheVeryName)
    {
        var name = Full with { Alias = alias! };

        Assert.Equal(alias, name.Alias);
        Assert.Same(Portal, name.NameLink);
        Assert.Equal(isTheVeryName, name == Full);
        Assert.Equal(isTheVeryName, name.Equals(Full));
        Assert.Equal(isTheVeryName, name.Equals((object)Full));

        if (isTheVeryName)
        {
            Assert.Equal(Full.GetHashCode(), name.GetHashCode());
            Assert.Single(new HashSet<RuntimeName> { Full, name });
        }
        else
        {
            // A name that is another name is kept out of a set twice over, whether or not the two fall in one bucket.
            Assert.Equal(2, new HashSet<RuntimeName> { Full, name }.Count);
        }
    }

    /// <summary>Both parts are kept exactly as they are handed: the empty word is a word and not nothing, the spaces
    /// about a word stay about it, a word of one letter is a word, a word of letters that only look like the Latin ones
    /// is kept letter for letter, and a word of a thousand letters is kept at its length - the address and the words
    /// over it the same way, since the record trims nothing and shortens nothing.</summary>
    [Fact]
    public void ANameKeepsItsWordsExactlyAsTheyAreGiven()
    {
        var oneLetter = new RuntimeName("r", new RuntimeLink("h", "t"));

        Assert.Equal("r", oneLetter.Alias);
        Assert.Equal("h", oneLetter.NameLink.Href);
        Assert.Equal("t", oneLetter.NameLink.Title);

        var spaced = new RuntimeName("  sub  ", new RuntimeLink("  https://portal.example.net/  ", "  the subscription  "));

        Assert.Equal("  sub  ", spaced.Alias);
        Assert.Equal("  https://portal.example.net/  ", spaced.NameLink.Href);
        Assert.Equal("  the subscription  ", spaced.NameLink.Title);

        var written = new RuntimeName("rēgion_ｐｒｉｍａｒｙ", new RuntimeLink("https://portal.example.net/#subscriptions/ subscriptions /00000000", "café"));

        Assert.Equal("rēgion_ｐｒｉｍａｒｙ", written.Alias);
        Assert.Equal("https://portal.example.net/#subscriptions/ subscriptions /00000000", written.NameLink.Href);
        Assert.Equal("café", written.NameLink.Title);

        // The boundary of length: a thousand letters of alias, and an address as long as a page of them.
        var longAlias = new string('x', 1_000);
        var longHref = "https://portal.example.net/#" + new string('y', 4_096);
        var longTitle = new string('z', 4_096);
        var boundless = new RuntimeName(longAlias, new RuntimeLink(longHref, longTitle));

        Assert.Equal(1_000, boundless.Alias.Length);
        Assert.Equal(longHref, boundless.NameLink.Href);
        Assert.Equal(longTitle, boundless.NameLink.Title);
        Assert.Equal(boundless, new RuntimeName(longAlias, new RuntimeLink(longHref, longTitle)));
        Assert.NotEqual(boundless, boundless with { Alias = longAlias.Substring(0, 999) });
    }

    /// <summary>The record makes no decision of its own about what a part may be: a name will hold no word and no link
    /// at all, although the type says both are always there - it is the manifest's reader that refuses a box without an
    /// alias and the box link that leaves a box without an https address unnamed. The nothing of a link and the nothing
    /// of a word are two different names, and both are names.</summary>
    [Fact]
    public void ANameRefusesNothingItIsGiven()
    {
        var nothing = Nothing;

        Assert.Null(nothing.Alias);
        Assert.Null(nothing.NameLink);
        Assert.Equal(nothing, new RuntimeName(null!, null!));
        Assert.Single(new HashSet<RuntimeName> { nothing, new RuntimeName(null!, null!) });

        // The name of no word is another name from the name of no link, and both are another from the name of neither.
        var noAlias = new RuntimeName(null!, Portal);
        var noLink = new RuntimeName(Alias, null!);

        Assert.Null(noAlias.Alias);
        Assert.Same(Portal, noAlias.NameLink);
        Assert.Equal(Alias, noLink.Alias);
        Assert.Null(noLink.NameLink);
        Assert.NotEqual(noAlias, noLink);
        Assert.True(noAlias != noLink);
        Assert.Equal(3, new HashSet<RuntimeName> { noAlias, noLink, nothing }.Count);

        // The empty word is a word, and the empty link a link: none of them is the nothing above.
        Assert.NotEqual(Nothing, Empty);
        Assert.NotEqual(noAlias, Empty);
        Assert.NotEqual(noLink, Empty);
        Assert.Equal(string.Empty, Empty.Alias);
        Assert.Equal(string.Empty, Empty.NameLink.Href);
        Assert.Equal(string.Empty, Empty.NameLink.Title);

        // Two names of one empty word and one empty link are one name, although neither is a name the page can draw.
        Assert.Equal(Empty, new RuntimeName(string.Empty, new RuntimeLink(string.Empty, string.Empty)));
        Assert.Equal(Empty.GetHashCode(), new RuntimeName(string.Empty, new RuntimeLink(string.Empty, string.Empty)).GetHashCode());
    }

    // ------------------------------------------------------------------------------------------ the link

    /// <summary>The link is held as the very reference the caller handed - a copy copies the parts and does not remake
    /// them - and yet weighed as the value it is: two names standing on two links of one address and one title are one
    /// name, in the hash as in the set and in the dictionary, because a name is weighed part by part and a link weighs
    /// itself word by word. (That the two words of a name are one name is pinned with the other file; what follows is
    /// what it costs the hash, the set and the copy.)</summary>
    [Fact]
    public void TheLinkIsTheVeryOneTheCallerHandedAndIsWeighedAsTheValueItIs()
    {
        var link = new RuntimeLink(Href, Title);
        var name = new RuntimeName(Alias, link);

        Assert.Same(link, name.NameLink);
        Assert.Same(link, (name with { Alias = "rg_tier" }).NameLink);
        Assert.Same(link, (name with { NameLink = link }).NameLink);

        var written = new RuntimeName(Alias, new RuntimeLink(Href, Title));

        Assert.NotSame(name.NameLink, written.NameLink);
        Assert.Equal(name.GetHashCode(), written.GetHashCode());
        Assert.Single(new HashSet<RuntimeName> { name, written });

        // The same story told by a dictionary, which is how a page keys a box by its name: by the value, not the reference.
        var indexed = new Dictionary<RuntimeName, string> { [name] = "the subscription" };

        Assert.True(indexed.ContainsKey(written));
        Assert.Equal("the subscription", indexed[written]);
        Assert.False(indexed.ContainsKey(name with { Alias = "rg_tier" }));

        // A link that differs in either of its two words is another part: the address alone, and the words alone.
        Assert.Equal(2, new HashSet<RuntimeName> { name, new RuntimeName(Alias, new RuntimeLink("https://portal.example.net/#resourcegroups/rg", Title)) }.Count);
        Assert.Equal(2, new HashSet<RuntimeName> { name, new RuntimeName(Alias, new RuntimeLink(Href, "The resource group in the Azure portal")) }.Count);
        Assert.True(name != new RuntimeName(Alias, new RuntimeLink(Href, Title + ".")));
        Assert.False(name == new RuntimeName(Alias, new RuntimeLink(Href + "/", Title)));

        // The empty link is a link, and no link at all is another part - which is what the type forbids and the record keeps.
        Assert.True(name != new RuntimeName(Alias, new RuntimeLink(string.Empty, string.Empty)));
        Assert.True(name != new RuntimeName(Alias, null!));
    }

    // ---------------------------------------------------------------------------- the record of the two parts

    /// <summary>Two names are one while their two parts are, whichever of the ways of asking is used: the record's own
    /// <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, and the hash that follows them and
    /// stands from call to call. A name is compared, not looked through: two calls on the same word and the same link
    /// are one name in another reference.</summary>
    [Fact]
    public void TwoNamesAreOneWhileTheirTwoPartsAre()
    {
        var one = Full;
        var two = new RuntimeName(Alias, Portal);

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
        Assert.Single(new HashSet<RuntimeName> { one, two });

        // The parts asked out and put back in make the same name again: the equality is of the values, not of the call.
        one.Deconstruct(out var alias, out var link);

        Assert.Equal(one, new RuntimeName(alias, link));
        Assert.Equal(one.GetHashCode(), new RuntimeName(alias, link).GetHashCode());
    }

    /// <summary>Either part tells two names apart, and nothing is weighed loosely: an alias or an address in another
    /// case, or with a space on it, or with a letter that only looks like the one expected, is another word, and a
    /// missing part is another name from a part said as nothing. Neither part stands in for the other.</summary>
    [Fact]
    public void ANameDiffersInEitherOfItsTwoParts()
    {
        var name = Full;

        Assert.NotEqual(name, name with { Alias = string.Empty });
        Assert.NotEqual(name, name with { Alias = null! });
        Assert.NotEqual(name, name with { Alias = Alias.ToUpperInvariant() });
        Assert.NotEqual(name, name with { Alias = Alias + " " });
        Assert.NotEqual(name, name with { Alias = "sUb" });
        Assert.NotEqual(name, name with { Alias = "ｓｕｂ" });
        Assert.NotEqual(name, name with { NameLink = null! });
        Assert.NotEqual(name, name with { NameLink = new RuntimeLink(string.Empty, string.Empty) });
        Assert.NotEqual(name, name with { NameLink = new RuntimeLink(Href, Title.ToUpperInvariant()) });
        Assert.NotEqual(name, name with { NameLink = new RuntimeLink(Href.ToUpperInvariant(), Title) });

        // The two names of one box, one of them named by the other's address: no part is read as the other's.
        Assert.False(new RuntimeName(Href, Portal).Equals(new RuntimeName(Alias, new RuntimeLink(Href, Href))));
        Assert.NotEqual(new RuntimeName(Alias, new RuntimeLink(Alias, Title)), new RuntimeName(Href, Portal));

        // A name differs from another by one part, and the four of them are four apart: the name itself and one of each.
        Assert.Equal(
            4,
            new HashSet<RuntimeName>
            {
                name,
                name with { Alias = "rg_tier" },
                name with { NameLink = new RuntimeLink("https://portal.example.net/#resourcegroups/rg-cmdemo1", Title) },
                name with { NameLink = new RuntimeLink(Href, "The resource group rg-cmdemo1 in the Azure portal") },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two names that are no name at all
    /// are one, a name and no name are two - the equality and the inequality each saying it, from either side - and the
    /// name asked about itself says no to the nothing, to the word it is named by, to the link of its name and to the
    /// address of that link. The name of nothing at all is still a name.</summary>
    [Fact]
    public void NoNameIsOneWithNoNameAndTheNameOfNothingIsAName()
    {
        RuntimeName? nothing = null;
        RuntimeName? alsoNothing = null;
        var there = Full;

        Assert.Null(default(RuntimeName));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((RuntimeName?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(Alias));
        Assert.False(there.Equals(Portal));
        Assert.False(there.Equals(Href));
        Assert.False(there.Equals(Title));
        Assert.False(there.Equals(new RuntimeLink(Alias, Href)));
        Assert.False(there.Equals(new RuntimeRegionMark(Alias, "serving", "serving traffic")));
        Assert.False(there.Equals(new RuntimeRegion(Alias, "cmdemo1", [])));
        Assert.False(there.Equals(new RuntimeTile(Alias, "neutral", "not probed", null, [], null, "not checked")));

        var alsoEmpty = Empty;

        Assert.Equal(Empty, alsoEmpty);
        Assert.NotSame(Empty, alsoEmpty);
        Assert.Equal(Empty.GetHashCode(), alsoEmpty.GetHashCode());
        Assert.True(Empty == alsoEmpty);
        Assert.Single(new HashSet<RuntimeName> { Empty, alsoEmpty });
    }

    /// <summary>The two parts fall out of a name in the order they went in, each as the value it holds, and what came
    /// out goes back in the same way; the name that was told nothing falls out as the nothing it is.</summary>
    [Fact]
    public void TheTwoPartsFallOutOfANameInTheOrderTheyWentIn()
    {
        var (alias, link) = Full;

        Assert.Equal(Alias, alias);
        Assert.Same(Portal, link);
        Assert.Equal(Full, new RuntimeName(alias, link));

        var name = Full;
        name.Deconstruct(out var first, out var second);

        Assert.Equal(Alias, first);
        Assert.Same(Portal, second);

        // One part asked out of the two, the rest of them left where they are.
        var (only, _) = new RuntimeName("browser", new RuntimeLink("https://demo.example.net", Title));

        Assert.Equal("browser", only);

        var (nothingAlias, nothingLink) = Nothing;

        Assert.Null(nothingAlias);
        Assert.Null(nothingLink);
    }

    /// <summary><c>with</c> names the part it changes and carries the other: a copy that names nothing is one with its
    /// original in another reference - its link included, since a copy copies the parts and not what they are made
    /// of - the original is left where it stood, and the link may be put on and taken off as the word is.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var name = Full;
        var copy = name with { };

        Assert.Equal(name, copy);
        Assert.NotSame(name, copy);
        Assert.True(name == copy);
        Assert.Same(name.NameLink, copy.NameLink);

        var retitled = name with { Alias = "rg_tier" };

        Assert.Equal("rg_tier", retitled.Alias);
        Assert.Same(Portal, retitled.NameLink);
        Assert.Equal(Alias, name.Alias);

        var relinked = name with { NameLink = new RuntimeLink(Href, "The resource group rg-cmdemo1 in the Azure portal") };

        Assert.Equal(Alias, relinked.Alias);
        Assert.NotEqual(name, relinked);
        Assert.Equal(2, new HashSet<RuntimeName> { name, relinked }.Count);
        Assert.Equal(Portal, name.NameLink);

        // A copy that names the parts the name already holds is the name, and a copy of the name of nothing is that.
        Assert.Equal(name, name with { Alias = Alias, NameLink = Portal });
        Assert.Equal(Nothing, Nothing with { Alias = null!, NameLink = null! });
        Assert.Equal(Full, Nothing with { Alias = Alias, NameLink = Portal });
        Assert.Null((name with { NameLink = null! }).NameLink);
        Assert.Equal(string.Empty, (name with { Alias = string.Empty }).Alias);
    }

    // --------------------------------------------------------------------------------------------- words

    /// <summary>The words a name says about itself - its name, then its two parts in the order they are declared, each
    /// named and standing blank when there is nothing of it - are the record's own and not the page's: the page is
    /// given its words by the payload builder, and the script is given JSON. A link says itself as a link of the same
    /// two parts, and no part is put in quotes.</summary>
    [Fact]
    public void ANameSaysItsTwoPartsInTheirOrder()
    {
        Assert.Equal(FullLine, Full.ToString());

        // The part that is no link is named and says nothing, and so does the name of neither part.
        Assert.Equal("RuntimeName { Alias = sub, NameLink =  }", new RuntimeName(Alias, null!).ToString());
        Assert.Equal(NothingLine, Nothing.ToString());

        // The empty words stand blank, and the empty link is still a link to be said.
        Assert.Equal("RuntimeName { Alias = , NameLink = RuntimeLink { Href = , Title =  } }", Empty.ToString());

        // A word is said as it stands, its spaces and its quotes and all.
        Assert.Equal(
            """RuntimeName { Alias =   sub  , NameLink = RuntimeLink { Href = a b, Title = "the subscription" } }""",
            new RuntimeName("  sub  ", new RuntimeLink("a b", "\"the subscription\"")).ToString());
    }

    /// <summary>A name's own words are the same whatever the reader speaks, and its two words are weighed ordinally
    /// whatever the reader's culture turns a letter into: a viewer of German is told the same name, in the same words,
    /// and the dotted and the dotless <c>i</c> - which one reader's capital of a small letter turns into the other's -
    /// stay two letters of two aliases.</summary>
    [Fact]
    public void TheNameAndItsWordsAreTheSameWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal(FullLine, Full.ToString());
            Assert.Equal(NothingLine, Nothing.ToString());
            Assert.Equal(Full, new RuntimeName(Alias, new RuntimeLink(Href, Title)));
            Assert.Equal(Full.GetHashCode(), new RuntimeName(Alias, new RuntimeLink(Href, Title)).GetHashCode());

            Assert.NotEqual(Full, Full with { Alias = Alias.ToUpperInvariant() });
            Assert.NotEqual(Full, Full with { Alias = "Sub" });
            Assert.NotEqual(Full, new RuntimeName(Alias, new RuntimeLink(Href, Title.ToUpperInvariant())));
            Assert.True(Full != new RuntimeName(Alias, new RuntimeLink(Href.ToUpperInvariant(), Title)));
        });

        Assert.NotEqual(new RuntimeName("ist", Portal), new RuntimeName("ıst", Portal));
        Assert.NotEqual(new RuntimeName("ist", Portal), new RuntimeName("İst", Portal));
        Assert.Equal(new RuntimeName("ist", Portal), new RuntimeName("ist", Portal));
    }

    // ------------------------------------------------------------------------ JSON at the edges

    /// <summary>The three ways a part can be missing at the end the record allows and the type forbids: the alias is
    /// left out of the words the script is handed, the link of a name is left out, and a name of neither is written as
    /// an empty object. The empty word is written, since it is a word and not the absence of one - which is why the
    /// reader of the manifest, not the record, is what refuses a box of either.</summary>
    [Fact]
    public void ThePartThatIsNothingIsLeftOutOfTheWordsTheScriptReads()
    {
        Assert.Equal(
            """{"nameLink":{"href":"https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000","title":"The Azure subscription in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)"}}""",
            DrawnJson(new RuntimeName(null!, Portal)));
        Assert.Equal("""{"alias":"sub"}""", DrawnJson(new RuntimeName(Alias, null!)));
        Assert.Equal("{}", DrawnJson(Nothing));

        // The empty words stand in the JSON as the words they are.
        using var json = JsonDocument.Parse(PayloadJson(Empty));
        var drawn = json.RootElement.GetProperty("names")[0];

        Assert.Equal(["alias", "nameLink"], drawn.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["href", "title"], drawn.GetProperty("nameLink").EnumerateObject().Select(property => property.Name));
    }

    /// <summary>The words the script is handed are the words the record holds, through the writer's escapes: a letter
    /// outside ASCII, a quote, an angle bracket, an ampersand, a tab and a newline are written as the writer writes
    /// them and read back as they went in - and a name that needs them is one name with the name written plainly.</summary>
    [Fact]
    public void TheWordsAreWrittenAsTheWriterEscapesThemAndReadBackAsTheyWentIn()
    {
        var name = new RuntimeName("rēgion", new RuntimeLink("https://portal.example.net/#?a=1&b=2", "The <subscription> \"one\""));

        Assert.Equal(
            """{"alias":"r\u0113gion","nameLink":{"href":"https://portal.example.net/#?a=1\u0026b=2","title":"The \u003Csubscription\u003E \u0022one\u0022"}}""",
            DrawnJson(name));

        using var json = JsonDocument.Parse(PayloadJson(name));
        var drawn = json.RootElement.GetProperty("names")[0];

        Assert.Equal("rēgion", drawn.GetProperty("alias").GetString());
        Assert.Equal("https://portal.example.net/#?a=1&b=2", drawn.GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal("The <subscription> \"one\"", drawn.GetProperty("nameLink").GetProperty("title").GetString());

        // A word of a tab and a newline of its own is written escaped and read back whole.
        var control = new RuntimeName("a\tb\nc", new RuntimeLink("https://x.example/", "d"));

        Assert.Equal("""{"alias":"a\tb\nc","nameLink":{"href":"https://x.example/","title":"d"}}""", DrawnJson(control));

        using var written = JsonDocument.Parse(PayloadJson(control));

        Assert.Equal("a\tb\nc", written.RootElement.GetProperty("names")[0].GetProperty("alias").GetString());
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>The name's whole public surface as it stands: its two parts, named in the order the constructor names
    /// them and of the type the constructor says, both of them a part a caller must name and neither of them allowed to
    /// be nothing - which the record does not enforce and the reader does - and the members every record is asked:
    /// falling apart, comparing, hashing and saying itself, and nothing else to call. There is no factory of the
    /// manifest's parts here (a name is built straight out of an alias and a <see cref="RuntimeLink" />, where a
    /// <see cref="RuntimeTrend" /> copies a health trend and a <see cref="RuntimeLink" /> reads an address), and no
    /// verdict to read: what a box is doing is its tile's, and a box with a tile is drawn from the tile. A third part
    /// added to the record has to be written down here as well, which is the point: a name on a diagram cannot carry a
    /// part nobody decided what it means.</summary>
    [Fact]
    public void TheNameHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Alias", "NameLink"],
            [.. typeof(RuntimeName)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Alias", "Deconstruct", "Equals", "GetHashCode", "NameLink", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);
        Assert.DoesNotContain("PrintMembers", PublicSurface(), StringComparer.Ordinal);

        var properties = typeof(RuntimeName)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Alias"].PropertyType);
        Assert.Equal(typeof(RuntimeLink), properties["NameLink"].PropertyType);
        Assert.All(
            properties.Values,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.True(property.CanWrite);

                // Both parts are read once and written only as the record is made: a name on a payload cannot be edited.
                Assert.Contains(typeof(System.Runtime.CompilerServices.IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
            });

        // The one public constructor, naming its two parts as the properties are named; the copy constructor the
        // compiler writes for a record is not one a caller outside the record calls.
        var constructor = Assert.Single(typeof(RuntimeName).GetConstructors());
        var parameters = constructor.GetParameters();

        Assert.Equal(["Alias", "NameLink"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(string), typeof(RuntimeLink)], [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));

        // The nullable annotations stand as the record declares them: neither part may be nothing - which is what the
        // builder leans on when it hands the payload's names to the script, and what the record itself does not
        // enforce (a caller may still hand it the nothing of either, pinned above).
        var nullability = new NullabilityInfoContext();

        Assert.All(parameters, parameter => Assert.Equal(NullabilityState.NotNull, nullability.Create(parameter).WriteState));
        Assert.All(properties.Values, property => Assert.Equal(NullabilityState.NotNull, nullability.Create(property).ReadState));

        // The record's own two ways of comparing, the members every record is asked, and nothing else.
        Assert.Equal(2, typeof(RuntimeName).GetMethods().Count(method => method.Name == nameof(Equals)));
        Assert.NotNull(typeof(RuntimeName).GetMethod("Equals", [typeof(RuntimeName)]));
        Assert.NotNull(typeof(RuntimeName).GetMethod("Equals", [typeof(object)]));
        Assert.NotNull(typeof(RuntimeName).GetMethod("GetHashCode"));
        Assert.NotNull(typeof(RuntimeName).GetMethod("ToString", Type.EmptyTypes));
        Assert.NotNull(typeof(RuntimeName).GetMethod("Deconstruct"));
        Assert.Null(typeof(RuntimeName).GetMethod("Of"));
        Assert.Null(typeof(RuntimeName).GetMethod("To"));

        var equality = typeof(RuntimeName).GetMethod("op_Equality")!;

        Assert.True(equality.IsStatic);
        Assert.True(equality.IsPublic);
        Assert.Equal(typeof(bool), equality.ReturnType);
        Assert.Equal([typeof(RuntimeName), typeof(RuntimeName)], [.. equality.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal([true, true], [.. typeof(RuntimeName).GetMethods().Where(method => method.Name.StartsWith("op_", StringComparison.Ordinal)).Select(method => method.IsStatic)]);

        // The two members a record keeps to itself are there, and are not for a caller to ask.
        Assert.NotNull(typeof(RuntimeName).GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.NotNull(typeof(RuntimeName).GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance));

        Assert.Equal([typeof(IEquatable<RuntimeName>)], typeof(RuntimeName).GetInterfaces());
        Assert.True(typeof(RuntimeName).IsSealed);
        Assert.Equal(typeof(object), typeof(RuntimeName).BaseType);
        Assert.Empty(typeof(RuntimeName).GetEvents());
        Assert.Empty(typeof(RuntimeName).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.DoesNotContain(
            typeof(RuntimeName).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => !method.Name.StartsWith("op_", StringComparison.Ordinal));
    }

    // --------------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the name declares itself: its parts and the methods that are not the get and the set
    /// of one - the two operators kept although the compiler names them specially, since they are the <c>==</c> and the
    /// <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The constructor is
    /// no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeName)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
