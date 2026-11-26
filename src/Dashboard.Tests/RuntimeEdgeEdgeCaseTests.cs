using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeEdge" />: one drawn line of an environment's
/// runtime diagram, the four parts it is made of and the one number an origin may carry. The record decides nothing
/// of its own - no verdict, no words for the page, no moment - so what is pinned here is that each part comes back
/// as the part it went in as, that the record refuses nothing it is handed, and that the members every record is
/// asked compare, hash, fall apart, copy and say the five parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeManifestParserEdgeCaseTests" /> pins the read
/// out of a file: that <c>id</c>, <c>from</c> and <c>to</c> are required, that every word of a kind - in any case,
/// trimmed, and any word the dashboard has never heard as the kind it does not know - becomes a
/// <see cref="RuntimeEdgeKind" />, and that a priority is kept only when the number is a whole one of an
/// <see cref="int" /> (the two ends of it included) and left out when it is a fraction or too big, a priority written
/// as no number at all being the one optional member that stops the manifest being read.
/// <see cref="DependencyTests" /> pins that an edge the reader built is <em>equal</em> to one built by hand, and
/// <see cref="LinkTests" />, <see cref="RotationTests" />, <see cref="TrendTests" /> and
/// <see cref="RuntimePayloadBuilderTests" /> build edges and read their parts back through the payload. What an edge
/// <em>means</em> - that the priority <c>1</c> is the origin the page says <c>first, while healthy</c> about, that an
/// <see cref="RuntimeEdgeKind.Sql" /> line leads to the database's box, that an id is how the page finds its line
/// again - is the builder's and is pinned there, and the words of a line of the diagram are the payload's.
/// </para>
/// <para>
/// What follows is the record on its own, at the ends the tests above leave open: the five parts by the names of
/// their own properties, the fifth standing absent when the caller does not name it, the ends of an
/// <see cref="int" /> and the nothing that is not the zero, the empty word that is a word and the spaces and the
/// letters that only look alike, the direction a line has - <c>a-to-b</c> from <c>a</c> to <c>b</c> being another
/// line from <c>b</c> to <c>a</c> - the six kinds the part may hold and the unset one, the id the record neither
/// requires nor checks against the two aliases it says are joined, and the whole public surface, so that the day a
/// sixth part is added the list of them says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: an edge holds three words, a kind and a number,
/// calls nothing at all, and every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimeEdgeEdgeCaseTests
{
    private const string AnId = "browser-to-fd_ui";
    private const string From = "browser";
    private const string To = "fd_ui";

    /// <summary>An edge of the manifest's own convention: the id is the two aliases with <c>-to-</c> between them,
    /// which is how the deployment writes it and how these tests name a line. A test that means an edge of another
    /// id - a spaced one, an upper-case one, the two aliases the other way round - says all five parts to the
    /// constructor, since the record checks nothing of the convention (pinned below).</summary>
    private static RuntimeEdge Line(string from, string to, RuntimeEdgeKind kind, int? priority = null) =>
        ManifestEdges.Between(from, to, kind, priority);

    /// <summary>An edge with every part named, the three words apart from one another and the kind and the priority
    /// of a Front Door origin, so that a part landing on another property is seen instead of quietly matching.</summary>
    private static RuntimeEdge Full => Line(From, To, RuntimeEdgeKind.Origin, 1);

    /// <summary>An edge named by nothing but the four a caller must name, the priority standing as it stands when
    /// nobody names it.</summary>
    private static RuntimeEdge Bare => Line(From, To, RuntimeEdgeKind.Public);

    /// <summary>The edge of nothing at all: no words, no kind named and no priority - the parts the record will hold
    /// although the file may not give them.</summary>
    private static RuntimeEdge Nothing => new(null!, null!, null!, default);

    /// <summary>The edge of empty words: a part said as nothing, which the record keeps as a word and not as the
    /// absence of one.</summary>
    private static RuntimeEdge Empty => new(string.Empty, string.Empty, string.Empty, RuntimeEdgeKind.Other);

    // ------------------------------------------------------- the five parts, by the names of their own properties

    /// <summary>The constructor's five parts, position by position, come back as the property of the same name - the
    /// order the reader of a manifest hands an edge over in, and the order the page writes the id out of. Each part
    /// keeps the value it was given and nothing about it is worked out of the others: an edge from a node to itself
    /// keeps the one alias in both places.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var edge = Full;

        Assert.Equal(AnId, edge.Id);
        Assert.Equal(From, edge.From);
        Assert.Equal(To, edge.To);
        Assert.Equal(RuntimeEdgeKind.Origin, edge.Kind);
        Assert.Equal(1, edge.Priority);

        var sql = Line("app_ui_primary", "sqldb", RuntimeEdgeKind.Sql);

        Assert.Equal("app_ui_primary-to-sqldb", sql.Id);
        Assert.Equal("app_ui_primary", sql.From);
        Assert.Equal("sqldb", sql.To);
        Assert.Equal(RuntimeEdgeKind.Sql, sql.Kind);
        Assert.Null(sql.Priority);

        // The line of a node to itself: the record puts no alias in the other's place and finds nothing in it.
        var itself = Line("sqldb", "sqldb", RuntimeEdgeKind.Other, 0);

        Assert.Equal("sqldb", itself.From);
        Assert.Equal("sqldb", itself.To);
        Assert.Equal(0, itself.Priority);
    }

    /// <summary>The priority is the one part a caller may leave out, and it stands absent when it does - absent, and
    /// not the zero a page could read as a priority Front Door actually gave. Naming it as nothing is the same as
    /// not naming it, and the declaration of that default - five parts, one of them defaulting to the nothing - is
    /// read off the type in the surface test at the end of this file.</summary>
    [Fact]
    public void ThePriorityTheCallerDoesNotNameStandsAbsent()
    {
        Assert.Null(Bare.Priority);
        Assert.Null(Line(From, To, RuntimeEdgeKind.Dashboard, null).Priority);
        Assert.Null(Empty.Priority);
        Assert.Null(Nothing.Priority);

        // The absence of the part is the absence of a number, and the one a caller names as nothing is the same
        // edge as the one it does not name at all.
        Assert.Equal(Bare, Bare with { Priority = null });
        Assert.False(Bare.Priority.HasValue);
    }

    /// <summary>Every word is kept exactly as it is handed: the empty word is a word and not nothing, the spaces
    /// about a word stay about it, a word of one letter is a word, and a word of letters that only look like the
    /// Latin ones is kept letter for letter. (That the file's words arrive trimmed, and that a manifest may not name
    /// an edge without its three words, is the reader's and is pinned with the reader.)</summary>
    [Fact]
    public void AnEdgeKeepsItsWordsExactlyAsTheyAreGiven()
    {
        var empty = Empty;

        Assert.Equal(string.Empty, empty.Id);
        Assert.Equal(string.Empty, empty.From);
        Assert.Equal(string.Empty, empty.To);

        var oneLetter = new RuntimeEdge("i", "a", "b", RuntimeEdgeKind.Dependency, 1);

        Assert.Equal("i", oneLetter.Id);
        Assert.Equal("a", oneLetter.From);
        Assert.Equal("b", oneLetter.To);

        var spaced = new RuntimeEdge("  browser-to-fd_ui  ", " browser ", " fd_ui ", RuntimeEdgeKind.Origin, 2);

        Assert.Equal("  browser-to-fd_ui  ", spaced.Id);
        Assert.Equal(" browser ", spaced.From);
        Assert.Equal(" fd_ui ", spaced.To);

        var written = new RuntimeEdge("brōwser-to-fd_ui", "brōwser", "ｆｄ_ｕｉ", RuntimeEdgeKind.Sql, -1);

        Assert.Equal("brōwser", written.From);
        Assert.Equal("ｆｄ_ｕｉ", written.To);

        // The id the page looks a line up by is kept at whatever length the file gives it.
        var longId = new string('x', 1_000);

        Assert.Equal(longId, new RuntimeEdge(longId, From, To, RuntimeEdgeKind.Other, null).Id);
    }

    /// <summary>The record makes no decision of its own about what a part may be: an edge will hold no words at all,
    /// since it is the file that is read and the reader that refuses one. Only the kind cannot be nothing - it is a
    /// value type, and the nothing it is handed is the kind the dashboard does not know.</summary>
    [Fact]
    public void AnEdgeRefusesNothingItIsGiven()
    {
        var nothing = Nothing;

        Assert.Null(nothing.Id);
        Assert.Null(nothing.From);
        Assert.Null(nothing.To);
        Assert.Equal(RuntimeEdgeKind.Other, nothing.Kind);
        Assert.Null(nothing.Priority);

        // Nothing it is not told, and empty words it is told, are two edges: the absence of a word is no word.
        Assert.NotEqual(nothing, Empty);
        Assert.True(nothing != Empty);
        Assert.False(nothing.Equals(Empty));

        // Two edges of nothing at all, made by two calls, are one edge.
        Assert.Equal(nothing, new RuntimeEdge(null!, null!, null!, default, null));
        Assert.Single(new HashSet<RuntimeEdge> { nothing, new RuntimeEdge(null!, null!, null!, default, null) });
    }

    // ---------------------------------------------------------------------------- the kind a line is drawn as

    /// <summary>The six kinds an edge may be told, each held as it is given: the record compares them apart from one
    /// another and says each by its own word, and a line of one kind is no line of another.</summary>
    [Theory]
    [InlineData(RuntimeEdgeKind.Other)]
    [InlineData(RuntimeEdgeKind.Public)]
    [InlineData(RuntimeEdgeKind.Origin)]
    [InlineData(RuntimeEdgeKind.Sql)]
    [InlineData(RuntimeEdgeKind.Dashboard)]
    [InlineData(RuntimeEdgeKind.Dependency)]
    public void EveryKindOfLineIsHeldAndSaidAsItIsGiven(RuntimeEdgeKind kind)
    {
        var edge = Line(From, To, kind, 1);

        Assert.Equal(kind, edge.Kind);
        Assert.Equal(edge, Line(From, To, kind, 1));
        Assert.Equal(edge.GetHashCode(), Line(From, To, kind, 1).GetHashCode());
        Assert.Contains($"Kind = {kind}", edge.ToString(), StringComparison.Ordinal);
    }

    /// <summary>The six kinds are the whole vocabulary the part may hold, in the order they are declared, the first
    /// of them what an unset kind is: a line the dashboard does not know is drawn and not updated, so the nothing of
    /// a kind and the word it does not know are one and the same kind. The word a file spells for each of the six is
    /// the reader's and is pinned with the reader.</summary>
    [Fact]
    public void TheSixKindsAreTheWholeVocabularyAndTheUnsetOneIsTheOneUnknown()
    {
        Assert.Equal(
            ["Other", "Public", "Origin", "Sql", "Dashboard", "Dependency"],
            [.. Enum.GetValues<RuntimeEdgeKind>().Select(kind => kind.ToString())]);
        Assert.Equal([0, 1, 2, 3, 4, 5], [.. Enum.GetValues<RuntimeEdgeKind>().Select(kind => (int)kind)]);
        Assert.Equal(6, Enum.GetValues<RuntimeEdgeKind>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(RuntimeEdgeKind)));
        Assert.Equal(RuntimeEdgeKind.Other, default);
        Assert.Equal(RuntimeEdgeKind.Other, (RuntimeEdgeKind)0);
    }

    /// <summary>A kind outside the six is held as it is handed and said as the number it is, and the ends of an
    /// <see cref="int" /> are two kinds apart from one another and from every one of the six - the record weighing
    /// the number and knowing nothing of the vocabulary, which is the file's question and is pinned with the
    /// reader.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(99)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void AKindOutsideTheSixIsHeldAsTheNumberItIs(int number)
    {
        var kind = (RuntimeEdgeKind)number;
        var edge = Line(From, To, kind, 1);

        Assert.Equal(kind, edge.Kind);
        Assert.False(Enum.IsDefined(kind));
        Assert.Contains("Kind = " + number.ToString(CultureInfo.InvariantCulture), edge.ToString(), StringComparison.Ordinal);
        Assert.NotEqual(edge, Full);
        Assert.NotEqual(edge, Bare);
        Assert.All(Enum.GetValues<RuntimeEdgeKind>(), known => Assert.NotEqual(known, kind));
    }

    // --------------------------------------------------------------------------- the priority, at the ends of one

    /// <summary>Any whole number of an <see cref="int" /> is kept exactly - the two ends of it, one beside the other,
    /// the negative numbers a manifest never gives, zero and one - and each is a part of its own, so that two edges
    /// a priority apart are two edges at either end. (That the reader keeps only a whole number of an <c>int</c> and
    /// leaves the rest out is pinned with the reader; this is the record holding what it is handed.)</summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MinValue + 1)]
    [InlineData(-2)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue - 1)]
    [InlineData(int.MaxValue)]
    public void EveryPriorityIsHeldExactlyAndApart(int priority)
    {
        var edge = Line(From, To, RuntimeEdgeKind.Origin, priority);

        Assert.Equal(priority, edge.Priority);
        Assert.True(edge.Priority.HasValue);
        Assert.Contains("Priority = " + priority.ToString(CultureInfo.InvariantCulture), edge.ToString(), StringComparison.Ordinal);
        Assert.NotEqual(edge, Bare);

        // One priority more is another edge, at either end as anywhere else - and the value is read back out of the
        // nullable, not guessed at from the number beside it.
        if (priority != int.MaxValue)
        {
            Assert.NotEqual(edge, edge with { Priority = priority + 1 });
            Assert.Equal(2, new HashSet<RuntimeEdge> { edge, edge with { Priority = priority + 1 } }.Count);
        }

        Assert.Equal(edge, new RuntimeEdge(edge.Id, edge.From, edge.To, edge.Kind, edge.Priority!.Value));
    }

    /// <summary>The one word the record works out of its kind and its priority: an edge is the first origin only
    /// when it is an origin and its priority is one - the origin Front Door serves before the others. Every other
    /// kind, whatever number it carries, is no first origin; an origin with no priority or one not one is no first
    /// origin; and the word is read from the kind and the number together, not from the number alone, so a <c>sql</c>
    /// line with priority one and an <c>origin</c> line with priority one are not the same edge in that respect.
    /// (That the page says <c>first, while healthy</c> of it is the builder's and is pinned with the builder.)</summary>
    [Theory]
    [InlineData(RuntimeEdgeKind.Origin, 1, true)]
    [InlineData(RuntimeEdgeKind.Origin, 0, false)]
    [InlineData(RuntimeEdgeKind.Origin, 2, false)]
    [InlineData(RuntimeEdgeKind.Origin, -1, false)]
    [InlineData(RuntimeEdgeKind.Origin, null, false)]
    [InlineData(RuntimeEdgeKind.Public, 1, false)]
    [InlineData(RuntimeEdgeKind.Sql, 1, false)]
    [InlineData(RuntimeEdgeKind.Dashboard, 1, false)]
    [InlineData(RuntimeEdgeKind.Dependency, 1, false)]
    [InlineData(RuntimeEdgeKind.Other, 1, false)]
    [InlineData(RuntimeEdgeKind.Other, null, false)]
    public void TheWordIsFirstOriginStandsForAnOriginOfPriorityOne(RuntimeEdgeKind kind, int? priority, bool isFirstOrigin)
    {
        var edge = Line(From, To, kind, priority);

        Assert.Equal(isFirstOrigin, edge.IsFirstOrigin);

        // The word is a word of the record: it is not one of the five parts it compares apart, so two edges that
        // agree on every part and differ in nothing are one edge with one word.
        var same = new RuntimeEdge(edge.Id, edge.From, edge.To, edge.Kind, edge.Priority);
        Assert.Equal(edge.IsFirstOrigin, same.IsFirstOrigin);
    }

    /// <summary>The priority that is nothing is not the priority that is zero, and neither is the one that is one:
    /// Front Door's priority <c>1</c> is the first origin, and a line that says no number is not a line that says
    /// zero - the three of them are three edges, three entries of a set, and the nothing says so in its own words by
    /// naming the part and leaving it blank.</summary>
    [Fact]
    public void ThePriorityThatIsNothingIsNotTheOneThatIsZero()
    {
        var none = Bare;
        var zero = Bare with { Priority = 0 };
        var one = Bare with { Priority = 1 };

        Assert.Null(none.Priority);
        Assert.Equal(0, zero.Priority);
        Assert.Equal(1, one.Priority);

        Assert.NotEqual(none, zero);
        Assert.NotEqual(zero, one);
        Assert.NotEqual(none, one);
        Assert.True(none != zero);
        Assert.False(none == zero);
        Assert.Equal(3, new HashSet<RuntimeEdge> { none, zero, one }.Count);

        // A line of no kind and no priority at all: the nothing of the one part and the zero of the other are still
        // two lines.
        Assert.NotEqual(Nothing, Nothing with { Priority = 0 });
        Assert.NotEqual(Empty, Empty with { Priority = 0 });
        Assert.Equal(Empty, Empty with { Priority = null });
    }

    // ---------------------------------------------------------------------------- the record of the five parts

    /// <summary>Two edges are one while their five parts are, whichever of the ways of asking is used: the record's
    /// own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, and the hash that follows them
    /// and stands from call to call. An edge is compared, not looked through: two calls on the same three words, the
    /// same kind and the same number are one edge, and a set of them holds them once.</summary>
    [Fact]
    public void TwoEdgesAreOneWhileTheirFivePartsAre()
    {
        var one = Full;
        var two = Line(From, To, RuntimeEdgeKind.Origin, 1);

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
        Assert.Single(new HashSet<RuntimeEdge> { one, two });

        // The parts asked out and put back in make the same edge again: the equality is of the values, not of the
        // call that gathered them.
        one.Deconstruct(out var id, out var from, out var to, out var kind, out var priority);

        Assert.Equal(one, new RuntimeEdge(id, from, to, kind, priority));
        Assert.Equal(one.GetHashCode(), new RuntimeEdge(id, from, to, kind, priority).GetHashCode());
    }

    /// <summary>Any one of the five parts tells two edges apart, and nothing is compared loosely: the words
    /// ordinally, so an id or an alias in another case or with a space on it is another word, and the direction of a
    /// line belongs to it - <c>a-to-b</c> from <c>a</c> to <c>b</c> is not the same line from <c>b</c> to <c>a</c>.
    /// The empty word, the word of spaces and the nothing that is no word are three parts of their own.</summary>
    [Fact]
    public void AnEdgeDiffersInAnyOneOfItsParts()
    {
        var edge = Full;

        Assert.NotEqual(edge, edge with { Id = string.Empty });
        Assert.NotEqual(edge, edge with { Id = "  " });
        Assert.NotEqual(edge, edge with { Id = null! });
        Assert.NotEqual(edge, edge with { Id = "BROWSER-TO-FD_UI" });
        Assert.NotEqual(edge, edge with { Id = "browser-to-fd_ui " });
        Assert.NotEqual(edge, edge with { Id = "fd_ui-to-browser" });
        Assert.NotEqual(edge, edge with { From = string.Empty });
        Assert.NotEqual(edge, edge with { From = null! });
        Assert.NotEqual(edge, edge with { From = "Browser" });
        Assert.NotEqual(edge, edge with { From = To, To = From });
        Assert.NotEqual(edge, edge with { To = string.Empty });
        Assert.NotEqual(edge, edge with { To = null! });
        Assert.NotEqual(edge, edge with { To = "fd_ui-2" });
        Assert.NotEqual(edge, edge with { Kind = RuntimeEdgeKind.Public });
        Assert.NotEqual(edge, edge with { Kind = RuntimeEdgeKind.Other });
        Assert.NotEqual(edge, edge with { Priority = null });
        Assert.NotEqual(edge, edge with { Priority = 2 });
        Assert.NotEqual(edge, edge with { Priority = -1 });

        // The empty word and the nothing are two parts, and a word of spaces is a third.
        Assert.NotEqual(new RuntimeEdge(string.Empty, From, To, RuntimeEdgeKind.Other, null), new RuntimeEdge(null!, From, To, RuntimeEdgeKind.Other, null));
        Assert.NotEqual(new RuntimeEdge(" ", From, To, RuntimeEdgeKind.Other, null), new RuntimeEdge(null!, From, To, RuntimeEdgeKind.Other, null));

        // The edges that differ from this one by one part are five apart.
        Assert.Equal(
            5,
            new HashSet<RuntimeEdge>
            {
                edge,
                edge with { Id = "browser-to-fd_ui-2" },
                edge with { From = "swa_dashboard" },
                edge with { To = "app_ui_primary" },
                edge with { Kind = RuntimeEdgeKind.Sql },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two edges that are no edge at
    /// all are one, an edge and no edge are two - the equality and the inequality each saying it, from either side -
    /// and the edge asked about itself says no to the nothing, to the id it is named by and to the mark the page is
    /// drawn from. The edge of nothing at all is still an edge.</summary>
    [Fact]
    public void NoEdgeIsOneWithNoEdgeAndTheEdgeOfNothingIsAnEdge()
    {
        RuntimeEdge? nothing = null;
        RuntimeEdge? alsoNothing = null;
        var there = Full;

        Assert.Null(default(RuntimeEdge));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((RuntimeEdge?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(AnId));
        Assert.False(there.Equals(1));
        Assert.False(there.Equals(RuntimeEdgeKind.Origin));
        Assert.False(there.Equals(new RuntimeEdgeMark(AnId, "neutral", null, null, null, "Front Door to fd_ui.")));
        Assert.False(there.Equals(new RuntimeNode(From, RuntimeNodeKind.Person, "browser")));

        var alsoEmpty = Empty;

        Assert.Equal(Empty, alsoEmpty);
        Assert.NotSame(Empty, alsoEmpty);
        Assert.Equal(Empty.GetHashCode(), alsoEmpty.GetHashCode());
        Assert.True(Empty == alsoEmpty);
        Assert.Single(new HashSet<RuntimeEdge> { Empty, alsoEmpty });
    }

    /// <summary>An edge is one with itself only while its id is: an id that follows the manifest's convention, one
    /// that names the two aliases the other way round, one of another word altogether, the empty word, a word of
    /// spaces and no word at all are six ids the record holds without a word of objection, and only the one that
    /// repeats what the constructor was given makes the very edge it was copied from - the convention is the file's
    /// and the page looks a line up by its id, so nothing here checks one against the other.</summary>
    [Theory]
    [InlineData("browser-to-fd_ui", true)]
    [InlineData("fd_ui-to-browser", false)]
    [InlineData("a line the manifest made up", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void TheIdIsHeldAsItIsWrittenAndNeverCheckedAgainstTheTwoAliases(string? id, bool isTheVeryEdge)
    {
        var edge = Full with { Id = id! };

        Assert.Equal(id, edge.Id);
        Assert.Equal(From, edge.From);
        Assert.Equal(To, edge.To);
        Assert.Equal(RuntimeEdgeKind.Origin, edge.Kind);
        Assert.Equal(1, edge.Priority);
        Assert.Contains($"Id = {id}", edge.ToString(), StringComparison.Ordinal);
        Assert.Equal(isTheVeryEdge, edge == Full);
    }

    /// <summary><c>with</c> names the part it changes and carries the other four: a copy that names nothing is one
    /// with its original in another reference, the original is left where it stood, the fifth part may be put on and
    /// taken off as the other four are, and turning the direction of a line turns the edge into another one while
    /// its id stands untouched - which is the record's point, and the reason the id is checked by nobody.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var edge = Full;
        var copy = edge with { };

        Assert.Equal(edge, copy);
        Assert.NotSame(edge, copy);
        Assert.True(edge == copy);

        var turnedAbout = edge with { From = To, To = From };

        Assert.Equal(To, turnedAbout.From);
        Assert.Equal(From, turnedAbout.To);
        Assert.Equal(AnId, turnedAbout.Id);
        Assert.Equal(RuntimeEdgeKind.Origin, turnedAbout.Kind);
        Assert.Equal(1, turnedAbout.Priority);
        Assert.NotEqual(edge, turnedAbout);

        var silent = edge with { Priority = null, Kind = RuntimeEdgeKind.Other };

        Assert.Null(silent.Priority);
        Assert.Equal(RuntimeEdgeKind.Other, silent.Kind);
        Assert.Equal(AnId, silent.Id);
        Assert.Equal(1, edge.Priority);
        Assert.Equal(RuntimeEdgeKind.Origin, edge.Kind);

        // A copy that names the parts the edge already holds is the edge, and a copy of the edge of nothing is the
        // edge of nothing.
        Assert.Equal(edge, edge with { Id = AnId, From = From, To = To, Kind = RuntimeEdgeKind.Origin, Priority = 1 });
        Assert.Equal(Nothing, Nothing with { Id = null!, From = null!, To = null!, Kind = RuntimeEdgeKind.Other, Priority = null });
    }

    /// <summary>The five parts fall out of an edge in the order they went in, each as the value it holds, and what
    /// came out goes back in the same way; the parts the edge was not told fall out as the nothing they are, and the
    /// edge that was told nothing at all is an edge all the same, at either end of a priority as anywhere.</summary>
    [Fact]
    public void TheFivePartsFallOutOfAnEdgeInTheOrderTheyWentIn()
    {
        var (id, from, to, kind, priority) = Full;

        Assert.Equal(AnId, id);
        Assert.Equal(From, from);
        Assert.Equal(To, to);
        Assert.Equal(RuntimeEdgeKind.Origin, kind);
        Assert.Equal(1, priority);

        var bare = Bare;
        bare.Deconstruct(out var bareId, out var bareFrom, out var bareTo, out var bareKind, out var barePriority);

        Assert.Equal(AnId, bareId);
        Assert.Equal(From, bareFrom);
        Assert.Equal(To, bareTo);
        Assert.Equal(RuntimeEdgeKind.Public, bareKind);
        Assert.Null(barePriority);
        Assert.Equal(bare, new RuntimeEdge(bareId, bareFrom, bareTo, bareKind, barePriority));

        var (nothingId, nothingFrom, nothingTo, nothingKind, nothingPriority) = Nothing;

        Assert.Null(nothingId);
        Assert.Null(nothingFrom);
        Assert.Null(nothingTo);
        Assert.Equal(RuntimeEdgeKind.Other, nothingKind);
        Assert.Null(nothingPriority);

        // One part asked out of the five, the rest of them left where they are.
        var (only, _, _, _, _) = Line("ca_ui", "sqldb", RuntimeEdgeKind.Sql, int.MinValue);

        Assert.Equal("ca_ui-to-sqldb", only);

        var ends = Line(From, To, RuntimeEdgeKind.Dependency, int.MaxValue);
        ends.Deconstruct(out _, out _, out _, out var endsKind, out var endsPriority);

        Assert.Equal(RuntimeEdgeKind.Dependency, endsKind);
        Assert.Equal(int.MaxValue, endsPriority);
    }

    /// <summary>The words an edge says about itself - its name, then its five parts in the order they are declared,
    /// each named and standing blank when there is nothing of it, then the one word it works out of them,
    /// <see cref="RuntimeEdge.IsFirstOrigin" />, named and stood as its true and false - are the record's own and not
    /// the page's: the page is given its words by the payload builder, and an id, an alias, a kind and a number are
    /// what these words carry. The whole line stands here, so a part moved about in the record is a change the tests
    /// see.</summary>
    [Fact]
    public void AnEdgeSaysItsFivePartsInTheirOrder()
    {
        Assert.Equal(
            "RuntimeEdge { Id = browser-to-fd_ui, From = browser, To = fd_ui, Kind = Origin, Priority = 1, IsFirstOrigin = True }",
            Full.ToString());

        // The part a caller may leave out is named and says nothing, and the word worked out of it says false.
        Assert.Equal(
            "RuntimeEdge { Id = browser-to-fd_ui, From = browser, To = fd_ui, Kind = Public, Priority = , IsFirstOrigin = False }",
            Bare.ToString());

        Assert.Equal(
            "RuntimeEdge { Id = , From = , To = , Kind = Other, Priority = , IsFirstOrigin = False }",
            Empty.ToString());
        Assert.Equal(
            "RuntimeEdge { Id = , From = , To = , Kind = Other, Priority = , IsFirstOrigin = False }",
            Nothing.ToString());

        // A number is said as it stands, its sign and all, and a kind outside the six says its number; the word is
        // false wherever the kind is no origin or the priority is no one.
        Assert.Equal(
            "RuntimeEdge { Id = a-to-b, From = a, To = b, Kind = Sql, Priority = -2147483648, IsFirstOrigin = False }",
            Line("a", "b", RuntimeEdgeKind.Sql, int.MinValue).ToString());
        Assert.Equal(
            "RuntimeEdge { Id = a-to-b, From = a, To = b, Kind = Dependency, Priority = 2147483647, IsFirstOrigin = False }",
            Line("a", "b", RuntimeEdgeKind.Dependency, int.MaxValue).ToString());
        Assert.Equal(
            "RuntimeEdge { Id = a-to-b, From = a, To = b, Kind = 99, Priority = , IsFirstOrigin = False }",
            Line("a", "b", (RuntimeEdgeKind)99).ToString());

        // The words of the six kinds are the words of the enum, and no part is put in quotes.
        Assert.Contains("Kind = Dashboard", Line(From, To, RuntimeEdgeKind.Dashboard).ToString(), StringComparison.Ordinal);
        Assert.Contains("IsFirstOrigin = False", Line(From, To, RuntimeEdgeKind.Dashboard).ToString(), StringComparison.Ordinal);
    }

    /// <summary>An edge's words are three words, a kind and a number, so no reader's culture turns them: a reader of German is
    /// told the same line as the invariant reader - the negative priority with its sign, the positive one with
    /// nothing before it, and the number of a kind outside the six - which is what a log of one of these edges may
    /// count on whatever the reader of the machine happens to be.</summary>
    [Fact]
    public void TheEdgesOwnWordsAreTheSameWhateverTheReaderSpeaks()
    {
        var minus = Line("a", "b", RuntimeEdgeKind.Origin, -1);
        var million = Line("a", "b", RuntimeEdgeKind.Origin, 1_000_000);

        ViewerCulture.German(() =>
        {
            Assert.Equal("RuntimeEdge { Id = a-to-b, From = a, To = b, Kind = Origin, Priority = -1, IsFirstOrigin = False }", minus.ToString());
            Assert.Equal("RuntimeEdge { Id = a-to-b, From = a, To = b, Kind = Origin, Priority = 1000000, IsFirstOrigin = False }", million.ToString());
            Assert.Equal("RuntimeEdge { Id = browser-to-fd_ui, From = browser, To = fd_ui, Kind = Origin, Priority = 1, IsFirstOrigin = True }", Full.ToString());
        });

        // Under the reader the test happens to run as, the words are the words the invariant reader gets.
        Assert.Equal(Words(minus), minus.ToString());
        Assert.Equal(Words(million), million.ToString());
        Assert.Equal(Words(Bare), Bare.ToString());
    }

    /// <summary>The parts are weighed as words and not as a reader spells them: two edges whose words differ only in
    /// case are two edges whatever culture the thread runs under, and the words that are one word are one edge under
    /// another reader too - the comparison is ordinal, as the manifest's words are.</summary>
    [Fact]
    public void TheWordsOfAnEdgeAreWeighedOrdinallyWhateverTheReaderSpeaks()
    {
        var upper = new RuntimeEdge("BROWSER-TO-FD_UI", From, To, RuntimeEdgeKind.Origin, 1);

        ViewerCulture.German(() =>
        {
            Assert.NotEqual(Full, upper);
            Assert.True(Full != upper);
            Assert.False(Full.Equals((object)upper));
        });

        Assert.NotEqual(Full, Full with { From = "ＢＲＯＷＳＥＲ" });
        Assert.Equal(Full, Line(From, To, RuntimeEdgeKind.Origin, 1));
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>The edge's whole public surface as it stands: its five parts, named in the order the constructor
    /// names them and of the type the constructor says, the four a caller must name and the one it may leave out as
    /// nothing, the one read-only word of its own it says about them - <see cref="RuntimeEdge.IsFirstOrigin" /> -
    /// and the members every record is asked - falling apart, comparing, hashing and saying itself - and nothing
    /// else to call. There is no verdict on an edge to set: the five parts are read and written by the parts of the
    /// record, and the one computed word the record works out of its kind and its priority stands apart from them
    /// and has no setter. A sixth part added to the record has to be written down here as well, which is the point:
    /// a line of a diagram cannot carry a part nobody decided what it means.</summary>
    [Fact]
    public void TheEdgeHasFivePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Id", "From", "To", "Kind", "Priority", "IsFirstOrigin"],
            [.. typeof(RuntimeEdge)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Equals", "From", "GetHashCode", "Id", "IsFirstOrigin", "Kind", "Priority", "To", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(RuntimeEdge)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Id"].PropertyType);
        Assert.Equal(typeof(string), properties["From"].PropertyType);
        Assert.Equal(typeof(string), properties["To"].PropertyType);
        Assert.Equal(typeof(RuntimeEdgeKind), properties["Kind"].PropertyType);
        Assert.Equal(typeof(int?), properties["Priority"].PropertyType);
        Assert.Equal(typeof(bool), properties["IsFirstOrigin"].PropertyType);

        // The five parts a caller may name stand read and write; the one word the record works out of them stands
        // read only, with no setter to put another verdict on the edge.
        Assert.All(
            ["Id", "From", "To", "Kind", "Priority"],
            name =>
            {
                Assert.True(properties[name].CanRead);
                Assert.True(properties[name].CanWrite);
            });

        Assert.True(properties["IsFirstOrigin"].CanRead);
        Assert.False(properties["IsFirstOrigin"].CanWrite);

        // The one public constructor, naming its five parts as the properties are named; the copy constructor the
        // compiler writes for a record is not one a caller outside the record calls.
        var constructors = typeof(RuntimeEdge).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Id", "From", "To", "Kind", "Priority"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(string), typeof(string), typeof(string), typeof(RuntimeEdgeKind), typeof(int?)], [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.Equal([false, false, false, false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(4), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.Null(parameters[4].DefaultValue);

        // The nullable annotations stand as the record declares them: the priority may be nothing, the three words
        // and the kind may not - which is what the reader of the file leans on, and what the record itself does not
        // enforce (a caller may still hand it the nothing of a word, pinned above).
        var nullability = new NullabilityInfoContext();

        // The default of the fifth part is the nothing, and the four before it are named by every caller.
        Assert.Equal(
            [false, false, false, false, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        List<PropertyInfo> parts = [properties["Id"], properties["From"], properties["To"], properties["Kind"], properties["Priority"]];

        Assert.Equal(
            [false, false, false, false, true],
            [.. parts.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        // The computed word is a plain value type: it may be nothing in no sense, so it is not nullable in its
        // read state, and it is not one of the five parts a constructor names.
        Assert.False(nullability.Create(properties["IsFirstOrigin"]).ReadState == NullabilityState.Nullable);

        Assert.Equal([typeof(IEquatable<RuntimeEdge>)], typeof(RuntimeEdge).GetInterfaces());
        Assert.True(typeof(RuntimeEdge).IsSealed);
        Assert.Equal(typeof(object), typeof(RuntimeEdge).BaseType);
        Assert.Empty(typeof(RuntimeEdge).GetEvents());
        Assert.Empty(typeof(RuntimeEdge).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the edge declares itself: its parts and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeEdge)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words an edge says about itself, taken in the invariant culture whatever the reader of this test
    /// happens to be: the parts are words and a number, and the culture is only the way the number is said once.</summary>
    private static string Words(RuntimeEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        var reader = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            return edge.ToString()!;
        }
        finally
        {
            CultureInfo.CurrentCulture = reader;
        }
    }
}
