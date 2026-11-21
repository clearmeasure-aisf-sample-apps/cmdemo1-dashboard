using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests that <see cref="AvailabilityPart" /> still owed: the shape of the
/// record itself, the part as the key of the lookups a line is gathered into, the moment at the ends of the hours it
/// may be spelled in and a tick on from itself, the words that only look alike, and the one member a reader turns.
/// <see cref="AvailabilityPart" /> is one part of the availability line - the word before, the number that carries it,
/// the word after, the tooltip the part gives itself and the moment it names - and it settles nothing of its own:
/// <see cref="AvailabilityText.Parts" /> chooses the words, <c>AvailabilityLine.razor</c> says them, and the record
/// holds what it was handed and nothing more.
/// </summary>
/// <remarks>
/// <see cref="AvailabilityPartEdgeCaseTests" /> - the file this one finishes rather than repeats, added for the same
/// record under an earlier issue - already pins the five members as the constructor hands them out (each argument on
/// the property of its name, the very string it was given, the two optional members standing at their nothing by
/// default), every word kept exactly as it is given and refused by nothing, a word of any length held whole, the five
/// members fallen apart in the order they went in, the copy turning the member it names and leaving the original
/// standing, the equality of the typed record, of the boxed <see cref="object" /> and of <c>==</c> and <c>!=</c> with
/// the hash following them, the moment compared as the instant it is and a second away another part, no part one with
/// no part and none with any other thing, the record's own words in the order its members are named and under a
/// German reader, the parts of the windows that counted nothing, and the five members with their types by name.
/// <para>
/// Left open by all of that, and pinned here: that the record is sealed, that <c>IEquatable&lt;AvailabilityPart&gt;</c>
/// is the one interface it answers through, and that the comparison a caller gets through it is the comparison of the
/// five members; that the five are held as the type declares them - the three words always there, the tooltip and the
/// moment sometimes not - which is what lets the markup say <c>@part.Before</c> plainly and write a
/// <c>&lt;time&gt;</c> only while the part names a moment; that every one of them is written only while the part is
/// being made, which leaves a copy the one way to turn a part; that the two optional members are optional at the call
/// and the three words are not; that the part is one key in a set, in a lookup and boxed among objects, whether it
/// came off the page or out of the test's own letters; that the moment keeps the hours it was spelled in while
/// standing for one instant - at the fourteenth hour east of the meridian, in the half hours of a zone thirty minutes
/// off and on the twelfth hour west of it alike - and that a part one tick older is another part saying one line;
/// that words which only look alike are held and told apart - an accent composed and the same accent spelled with its
/// mark, a letter beyond the plane the UTF-16 units are counted in, the whitespace a line is made of, and the marks
/// of the page itself in a word, which are escaped by nobody here; and that a reader who turns an <c>I</c> into a
/// dotless one turns no word of a part, no equality of it and no hash of it, only the digits of its moment.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the record calls nothing at all, every
/// moment below is one the test brought, and where a part is taken from <see cref="AvailabilityText.Parts" /> the
/// moment it is asked is the test's own, fixed.
/// </para>
/// </remarks>
public class AvailabilityPartSurfaceEdgeCaseTests
{
    /// <summary>The readers whose hours a part is said in here: the one that turns an <c>I</c> into a dotless one and
    /// the one that puts the day before the month. Each writes the fixture's moment in digits of its own, which is
    /// what the loop that looks at the words looks for; the English reader stands lower down, for the ask that is not
    /// about the moment at all but about the case of a letter.</summary>
    private static readonly string[] Readers = ["tr-TR", "de-DE"];

    /// <summary>The moment the page is asked, and every "ago" below is counted from: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>A moment nine hours and seventeen minutes before <see cref="Now" />, on the meridian: the report the failure part names.</summary>
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>The part of the week as the page makes it: three words, a tooltip of its own and no moment.</summary>
    private static AvailabilityPart Week =>
        new(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days");

    /// <summary>The part of the last failure as the page makes it: no tooltip, and the moment the part names.</summary>
    private static AvailabilityPart Failure =>
        new("last failure ", "9 h ago", string.Empty, When: Failed);

    /// <summary>All five members at once: the week's three words and its tooltip, with the failure's moment under it.</summary>
    private static AvailabilityPart Full => Week with { When = Failed };

    /// <summary>The three parts of one line, as the page makes them of one set of reports and one fixed moment.</summary>
    private static IReadOnlyList<AvailabilityPart> Line =>
        AvailabilityText.Parts(new HealthReports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed), Now);

    // ---------------------------------------------------------------- the record's own shape

    /// <summary>
    /// The part is what a record of five members is: sealed, so that nothing written below it can be compared with it
    /// by a caller that means the other thing, and <c>IEquatable&lt;AvailabilityPart&gt;</c> the one interface it
    /// answers through - the comparison that interface gives a caller being the comparison of the five members, twin
    /// to twin, another part apart, and the nothing answered as no. The interface asks the same questions as the
    /// record itself and answers them the same way, which is what lets a line of parts be gathered, counted and
    /// looked up through whichever name a caller happens to hold it by.
    /// </summary>
    [Fact]
    public void ThePartIsSealedAndComparedThroughTheInterfaceOfItsOwnType()
    {
        Assert.True(typeof(AvailabilityPart).IsSealed);
        Assert.Equal([typeof(IEquatable<AvailabilityPart>)], typeof(AvailabilityPart).GetInterfaces());

        IEquatable<AvailabilityPart> asInterface = Full;

        Assert.True(asInterface.Equals(Week with { When = Failed }));
        Assert.True(asInterface.Equals(Full with { }));
        Assert.True(asInterface.Equals(Full));
        Assert.False(asInterface.Equals(Week));
        Assert.False(asInterface.Equals(Failure));
        Assert.False(asInterface.Equals((AvailabilityPart?)null));

        // The page's own part of the week, told the moment the failure's part carries, is the part the test wrote out
        // of the same five members: the interface is asked with what the page made and answers with the record's own
        // five, and the page's part without that moment is another part.
        Assert.True(asInterface.Equals(Line[1] with { When = Failed }));
        Assert.Equal(Full, Line[1] with { When = Failed });

        // The same five members in the places of two of the others is another part, and the interface says so too.
        Assert.False(asInterface.Equals(new AvailabilityPart("164 of 166", string.Empty, " in 7 days", Full.Title, Failed)));

        // The interface's answer and the record's own answer are one answer, told either way.
        Assert.Equal(Full.Equals(Week with { When = Failed }), asInterface.Equals(Week with { When = Failed }));
        Assert.Equal(Full.Equals(Failure), asInterface.Equals(Failure));
        Assert.Equal(Full.GetHashCode(), asInterface.GetHashCode());
    }

    /// <summary>
    /// What the five members are declared to be, as the page reads them: the word before, the number and the word
    /// after are declared as there - which is why the markup says <c>@part.Before</c> and <c>@part.After</c> without
    /// asking whether a part has them - while the tooltip and the moment are declared as sometimes not, which is why
    /// the same markup writes the number inside a <c>&lt;time&gt;</c> only while the part names a moment and lets
    /// <c>title="@part.Title"</c> fall off the element when it carries no tooltip. Read and written both, in each of
    /// the five places; and each written only while the part is being made, the accessor marked as a copy's and no
    /// assignment's, which is what leaves a <c>with</c> the only way to turn a part and keeps the parts of a line
    /// standing still while the page shows them. One constructor takes the five in that order, the last two of them
    /// the ones a call may leave out, and left out they stand at the nothing the page reads as no tooltip and no
    /// moment rather than as the empty word.
    /// </summary>
    [Fact]
    public void TheFiveMembersAreDeclaredAsThePageReadsThemAndWrittenOnlyAtTheMaking()
    {
        var properties = typeof(AvailabilityPart)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(["Before", "Strong", "After", "Title", "When"], [.. properties.Keys]);

        var declared = new NullabilityInfoContext();

        Assert.Equal(NullabilityState.NotNull, declared.Create(properties["Before"]).ReadState);
        Assert.Equal(NullabilityState.NotNull, declared.Create(properties["Strong"]).ReadState);
        Assert.Equal(NullabilityState.NotNull, declared.Create(properties["After"]).ReadState);
        Assert.Equal(NullabilityState.Nullable, declared.Create(properties["Title"]).ReadState);
        Assert.Equal(NullabilityState.Nullable, declared.Create(properties["When"]).ReadState);

        Assert.All(properties.Values, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);

            // The setter of a record's member is an init: a caller may tell it at the making and in a copy, and the
            // compiler refuses it on a part that already stands.
            Assert.True(IsWrittenOnlyAtTheMaking(property), $"{property.Name} may be written after the part was made");
        });

        var constructor = Assert.Single(typeof(AvailabilityPart).GetConstructors());
        var parameters = constructor.GetParameters();

        Assert.Equal(["Before", "Strong", "After", "Title", "When"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([0, 1, 2, 3, 4], [.. parameters.Select(parameter => parameter.Position)]);
        Assert.All(parameters, parameter => Assert.False(parameter.IsOut));

        Assert.False(parameters[0].HasDefaultValue);
        Assert.False(parameters[1].HasDefaultValue);
        Assert.False(parameters[2].HasDefaultValue);
        Assert.True(parameters[3].HasDefaultValue);
        Assert.True(parameters[4].HasDefaultValue);
        Assert.Null(parameters[3].DefaultValue);
        Assert.Null(parameters[4].DefaultValue);

        // Written as read: a copy may tell the three words and tell the two optional members nothing, and no member
        // is a place a caller reaches back into once the part stands.
        Assert.Equal(NullabilityState.NotNull, declared.Create(properties["Before"]).WriteState);
        Assert.Equal(NullabilityState.NotNull, declared.Create(properties["Strong"]).WriteState);
        Assert.Equal(NullabilityState.NotNull, declared.Create(properties["After"]).WriteState);
        Assert.Equal(NullabilityState.Nullable, declared.Create(properties["Title"]).WriteState);
        Assert.Equal(NullabilityState.Nullable, declared.Create(properties["When"]).WriteState);

        // The five declared at the call as well: the words given, the two optional members the ones whose type says
        // they may be missing.
        Assert.Equal(NullabilityState.NotNull, declared.Create(parameters[0]).ReadState);
        Assert.Equal(NullabilityState.NotNull, declared.Create(parameters[1]).ReadState);
        Assert.Equal(NullabilityState.NotNull, declared.Create(parameters[2]).ReadState);
        Assert.Equal(NullabilityState.Nullable, declared.Create(parameters[3]).ReadState);
        Assert.Equal(NullabilityState.Nullable, declared.Create(parameters[4]).ReadState);
    }

    // --------------------------------------------------------- the part as the key of a lookup

    /// <summary>
    /// The equality and the hash a lookup asks with are the record's own five members, so a part is one key however it
    /// was reached: the page's three parts are three keys of one line, the twin a test wrote out meets the part the
    /// page made, a copy of no member named is the key that was already there, and the boxed part is found among
    /// objects as it is found among parts. A member turned is another key, in a set and in a lookup alike; and the
    /// part of no letters and no moment at all is a key like any other - the empty words and the nothings being a
    /// place a lookup can be asked for, which is what a page that counts the parts it has already said leans on.
    /// </summary>
    [Fact]
    public void APartIsOneKeyOfASetAndOfALookupHoweverItWasReached()
    {
        var parts = Line;

        Assert.Equal(3, parts.Count);
        Assert.Equal(3, parts.Distinct().Count());

        var byPart = parts.ToDictionary(part => part, part => part.Strong);

        Assert.Equal(3, byPart.Count);
        Assert.Equal("23 of 24", byPart[parts[0]]);
        Assert.Equal("164 of 166", byPart[Week]);
        Assert.Equal("9 h ago", byPart[Failure]);
        Assert.Equal("9 h ago", byPart[Failure with { }]);
        Assert.False(byPart.ContainsKey(Full with { Title = null }));
        Assert.False(byPart.ContainsKey(Full with { When = Failed.AddSeconds(1) }));

        var written = new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed);
        var keys = new HashSet<AvailabilityPart> { Week with { When = Failed } };

        Assert.Contains(written, keys);
        Assert.Contains(Full, keys);
        Assert.Contains(Full with { Title = Week.Title }, keys);
        Assert.False(keys.Add(Full));
        Assert.Single(keys);
        Assert.Contains((object)written, new HashSet<object> { Full });
        Assert.Single(new HashSet<object> { Full, written, Week with { When = Failed } });

        // A member turned is another key: six parts, six entries, one for each of the five places and the part they
        // all stand in.
        Assert.Equal(
            6,
            new HashSet<AvailabilityPart>
            {
                Full,
                Full with { Before = "Healthy in " },
                Full with { Strong = "165 of 166" },
                Full with { After = " in 24 hours" },
                Full with { Title = null },
                Full with { When = null },
            }.Count);

        var nothing = new AvailabilityPart(string.Empty, string.Empty, string.Empty);
        var absent = new Dictionary<AvailabilityPart, string> { [nothing] = "nothing to say" };

        Assert.Equal("nothing to say", absent[new AvailabilityPart(string.Empty, string.Empty, string.Empty)]);
        Assert.True(absent.TryGetValue(Full with { Strong = string.Empty, After = string.Empty, Title = null, When = null }, out var said));
        Assert.Equal("nothing to say", said);
        Assert.False(absent.ContainsKey(new AvailabilityPart(null!, string.Empty, string.Empty)));
        Assert.False(absent.ContainsKey(nothing with { Before = " " }));
    }

    // ---------------------------------------------------------------------- the moment's boundaries

    /// <summary>
    /// The moment is held as the instant it is and spelled in whichever hours it was written in: one instant on the
    /// meridian, at the fourteenth hour east of it, in the half hours of a zone that stands thirty minutes off and at
    /// the twelfth hour west of it is one part, one hash and one entry - and each of the four still says the hours it
    /// arrived in, which is the half the record's words keep for the viewer. The day inside those words moves with the
    /// hours, so the words are no way of telling two spellings of one part apart; a tick on is another part and
    /// another key while it still says one line, since the record writes no fraction of a second, which is why a
    /// lookup and not a printed word settles whether two parts are one.
    /// </summary>
    [Fact]
    public void TheMomentIsOneInstantInFourSpellingsAndATickOnIsAnotherPartSayingOneLine()
    {
        var east = Failed.ToOffset(TimeSpan.FromHours(14));
        var west = Failed.ToOffset(TimeSpan.FromHours(-12));
        var half = Failed.ToOffset(TimeSpan.FromMinutes(330));

        // The moment keeps the hours it was spelled in, which the record's own words say back and its equality
        // walks past.
        Assert.Equal(Failed, east);
        Assert.Equal(Failed, west);
        Assert.Equal(Failed, half);

        var spelled = new[]
        {
            Failure,
            Failure with { When = east },
            Failure with { When = west },
            Failure with { When = half },
        };

        Assert.All(spelled, part => Assert.Equal(Failure, part));
        Assert.Single(spelled.Distinct());
        Assert.Single(new HashSet<AvailabilityPart>(spelled));
        Assert.All(spelled, part => Assert.Equal(Failure.GetHashCode(), part.GetHashCode()));

        // Each spelling keeps its own hours, which the equality ignores and the record says back.
        Assert.Equal(TimeSpan.Zero, spelled[0].When!.Value.Offset);
        Assert.Equal(TimeSpan.FromHours(14), spelled[1].When!.Value.Offset);
        Assert.Equal(TimeSpan.FromHours(-12), spelled[2].When!.Value.Offset);
        Assert.Equal(TimeSpan.FromMinutes(330), spelled[3].When!.Value.Offset);

        Assert.Contains("When = 10/06/2026 16:13:00 +00:00", Words(spelled[0]), StringComparison.Ordinal);
        Assert.Contains("When = 10/07/2026 06:13:00 +14:00", Words(spelled[1]), StringComparison.Ordinal);
        Assert.Contains("When = 10/06/2026 04:13:00 -12:00", Words(spelled[2]), StringComparison.Ordinal);
        Assert.Contains("When = 10/06/2026 21:43:00 +05:30", Words(spelled[3]), StringComparison.Ordinal);
        Assert.NotEqual(Words(spelled[0]), Words(spelled[1]));
        Assert.NotEqual(Words(spelled[2]), Words(spelled[3]));

        // A tick on is another part, another key and another entry - and the one line, the record saying no fraction
        // of a second. A second on is the same distance and does say itself.
        var atTheTick = Failure with { When = Failed.AddTicks(1) };

        Assert.NotEqual(Failure, atTheTick);
        Assert.True(Failure != atTheTick);
        Assert.Equal(2, new HashSet<AvailabilityPart> { Failure, atTheTick }.Count);
        Assert.Equal(Words(Failure), Words(atTheTick));

        var atTheSecond = Failure with { When = Failed.AddSeconds(1) };

        Assert.NotEqual(Failure, atTheSecond);
        Assert.NotEqual(Words(Failure), Words(atTheSecond));
        Assert.Equal(3, new HashSet<AvailabilityPart> { Failure, atTheTick, atTheSecond }.Count);
        Assert.False(new Dictionary<AvailabilityPart, string> { [Failure] = "the failure" }.ContainsKey(atTheTick));

        // The ends of the number, and the part that names no moment at all: three parts, three entries, none of them
        // the part of the day the failure is of.
        var first = Failure with { When = DateTimeOffset.MinValue };
        var last = Failure with { When = DateTimeOffset.MaxValue };

        Assert.NotEqual(first, last);
        Assert.NotEqual(first, Failure with { When = null });
        Assert.NotEqual(last, Failure with { When = null });
        Assert.Equal(3, new HashSet<AvailabilityPart> { first, last, Failure with { When = null } }.Count);
    }

    // ------------------------------------------------------------- the words that only look alike

    /// <summary>
    /// Five members of letters, kept as letters and compared as letters, which is what the page's own words lean on:
    /// an accent composed and the same accent spelled as a letter with its mark are two words and two parts, nothing
    /// here normalizing them; a letter beyond the plane the UTF-16 units are counted in is held whole - both of its
    /// units - and is another part with one of them left standing, as it is with the pair gone; the
    /// whitespace a word is made of is part of the word, so the same letters over a line break, over a carriage return
    /// and over a space are three parts, said apart; and a word that arrives with the marks of the page in it is said
    /// as it arrived, because the record escapes nothing - the escaping a viewer needs belongs to the markup, and the
    /// record's words are no way of reading a part back. The five places stay five places whatever the letters in one
    /// of them say about another.
    /// </summary>
    [Fact]
    public void WordsThatOnlyLookAlikeAreHeldAndToldApart()
    {
        // An accented word, composed and spelled out of a letter and its mark: one word to anyone who folds the two
        // together, four units against five to this record, two parts, two entries and two lines of its own saying.
        var composed = new AvailabilityPart(string.Empty, Own("caf\u00E9"), string.Empty);
        var accented = new AvailabilityPart(string.Empty, Own("cafe\u0301"), string.Empty);

        Assert.Equal(4, composed.Strong.Length);
        Assert.Equal(5, accented.Strong.Length);
        Assert.Equal(composed.Strong.Normalize(NormalizationForm.FormKC), accented.Strong.Normalize(NormalizationForm.FormKC));
        Assert.False(string.Equals(composed.Strong, accented.Strong, StringComparison.Ordinal));

        // The record compares as letters, which is the same thing the two lines above say of its own words: one
        // accent spelled two ways is two parts and two sayings of them, and each spelling is the part it is.
        Assert.NotEqual(composed, accented);
        Assert.Equal(2, new HashSet<AvailabilityPart> { composed, accented }.Count);
        Assert.NotEqual(Words(composed), Words(accented));
        Assert.Equal(composed, new AvailabilityPart(string.Empty, Own("caf\u00E9"), string.Empty));
        Assert.Equal(accented, new AvailabilityPart(string.Empty, Own("cafe\u0301"), string.Empty));

        // A letter beyond the counted units: held whole, two units of it, one rune - and another part with only the
        // second of its two units left standing, or with the pair gone and the plain letter back on its own.
        var grinning = new AvailabilityPart(string.Empty, Own("1\U0001F600 h ago"), string.Empty);

        Assert.Equal(9, grinning.Strong.Length);
        Assert.Equal(8, grinning.Strong.EnumerateRunes().Count());
        Assert.Contains("\U0001F600", grinning.Strong, StringComparison.Ordinal);
        Assert.Contains("\uD83D", grinning.Strong, StringComparison.Ordinal);
        Assert.Equal(grinning, new AvailabilityPart(string.Empty, Own("1\U0001F600 h ago"), string.Empty));
        Assert.NotEqual(grinning, new AvailabilityPart(string.Empty, Own("1 h ago"), string.Empty));
        Assert.NotEqual(grinning, new AvailabilityPart(string.Empty, Own("\uDE00 h ago"), string.Empty));

        // The whitespace of a word is its letters: three words, three parts, three lines.
        var broken = new AvailabilityPart("last\nfailure ", "9 h ago", "\t");
        var returned = new AvailabilityPart("last\r\nfailure ", "9 h ago", " ");
        var spaced = new AvailabilityPart("last failure ", "9 h ago", " ");

        Assert.NotEqual(broken, returned);
        Assert.NotEqual(broken, spaced);
        Assert.NotEqual(returned, spaced);
        Assert.Equal(3, new HashSet<AvailabilityPart> { broken, returned, spaced }.Count);
        Assert.Contains("Before = last\nfailure , Strong = 9 h ago, After = \t", Words(broken), StringComparison.Ordinal);
        Assert.DoesNotContain("last\nfailure ", Words(spaced), StringComparison.Ordinal);
        Assert.NotEqual(broken.After, spaced.After);
        Assert.Equal("\t", broken.After);
        Assert.Equal(" ", spaced.After);

        // A word that arrives with the page's own marks in it is said as it arrived: nothing here escapes, and so
        // nothing here can be read back as markup by anybody.
        var marked = new AvailabilityPart("<strong>", "9 h ago", "</strong>", "a \"quoted\" title", Failed);

        Assert.Equal("<strong>", marked.Before);
        Assert.Contains("Before = <strong>", Words(marked), StringComparison.Ordinal);
        Assert.Contains("""Title = a "quoted" title""", Words(marked), StringComparison.Ordinal);
        Assert.NotEqual(marked, new AvailabilityPart("9 h ago", "9 h ago", "9 h ago", "a quoted title", Failed));

        // And a word that spells another member's name stays the word it is.
        var forged = new AvailabilityPart("Strong = 9 h ago, After = ", "164 of 166", string.Empty);

        Assert.Equal("Strong = 9 h ago, After = ", forged.Before);
        Assert.Equal("164 of 166", forged.Strong);
        Assert.DoesNotContain("Before = , Strong = 9 h ago", Words(forged), StringComparison.Ordinal);
        Assert.NotEqual(forged, new AvailabilityPart(string.Empty, "9 h ago", string.Empty));
        Assert.NotEqual(forged, Failure);
    }

    // ----------------------------------------------------------------------- the reader's own half

    /// <summary>
    /// A reader whose language turns an <c>I</c> into a dotless one has nothing to turn here: the record folds no
    /// case, sorts no letters and asks no calendar, so the five members are the five in whatever hours the page is
    /// read - equal, one hash, one key and one line as far as the words go - while the moment is the member that
    /// reader's digits write, in that reader's order of the day's parts and in none other. Which is why two parts
    /// that differ only in the case of one letter are two parts for every reader: the record compares letters rather
    /// than what a language makes of them, and the parts a page gathered in one reader's hours are the parts a test
    /// wrote out in another's.
    /// </summary>
    [Fact]
    public void ADotlessReaderTurnsNoWordAndOnlyTheHoursOfTheMoment()
    {
        var dotted = new AvailabilityPart("In 7 days", "164 of I66", " in 7 days", "Idle IP of the last 7 days", Failed);
        var written = new AvailabilityPart("In 7 days", "164 of I66", " in 7 days", "Idle IP of the last 7 days", Failed);

        Assert.Equal(dotted, written);
        Assert.Equal(dotted.GetHashCode(), written.GetHashCode());

        var invariant = Words(dotted);
        var words = invariant[..invariant.IndexOf("When = ", StringComparison.Ordinal)];

        // The four words are the words under every reader, and only what follows them is the reader's own.
        foreach (var language in Readers)
        {
            var said = SaidIn(language, dotted);
            var at = said.IndexOf("When = ", StringComparison.Ordinal);

            Assert.True(at > 0, $"{language}: the words say no moment at all");
            Assert.Equal(words, said[..at]);
            Assert.NotEqual(invariant[at..], said[at..]);
        }

        Assert.Contains("When = 10/06/2026 16:13:00 +00:00 }", invariant, StringComparison.Ordinal);
        Assert.Contains("When = 06.10.2026 16:13:00 +00:00 }", SaidIn("de-DE", dotted), StringComparison.Ordinal);

        // Two members that differ only in the case of one letter: one word to a reader who folds case, two letters to
        // this record, and two parts whoever reads them.
        Assert.NotEqual(dotted, dotted with { Strong = "164 of i66" });
        Assert.NotEqual(dotted, dotted with { Before = "in 7 days" });
        Assert.Equal(2, new HashSet<AvailabilityPart> { dotted, dotted with { Strong = "164 of i66" } }.Count);
        Assert.False(dotted.Equals(dotted with { Before = "in 7 days" }));

        // The part made under one reader is the part made under the next, and the letters are the letters: what the
        // reader's own comparison makes of them is the reader's business, not the record's.
        SaidIn("tr-TR", () =>
        {
            Assert.True(dotted == written);
            Assert.Equal(dotted.GetHashCode(), written.GetHashCode());
            Assert.Single(new HashSet<AvailabilityPart> { dotted, written });
            Assert.True(dotted.Equals(written));

            // The reader's own comparison is the reader's business: this language tells an I and an i apart where
            // English folds them together, and the record tells the two parts apart whichever of the two is asking.
            Assert.False(StringComparer.CurrentCultureIgnoreCase.Equals(dotted.Strong, "164 of i66"));
            Assert.True(dotted.Strong.Contains("I66", StringComparison.Ordinal));
        });

        SaidIn("en-US", () => Assert.True(StringComparer.CurrentCultureIgnoreCase.Equals("164 of I66", "164 of i66")));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>Whether a member may be written only while the part is being made: the record's init-only accessor,
    /// which the compiler marks with the modreq that a <c>with</c> carries and no assignment does.</summary>
    private static bool IsWrittenOnlyAtTheMaking(PropertyInfo property) =>
        property.SetMethod is { } setter
        && setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier =>
            modifier.FullName is { } name && name.Equals("System.Runtime.CompilerServices.IsExternalInit", StringComparison.Ordinal));

    /// <summary>The words a part says its five members in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the four words are letters and outlast any reader, and the moment is said in the invariant
    /// digits here so that the string is the point and the culture only the way to say it once.</summary>
    private static string Words(AvailabilityPart part) => SaidIn(CultureInfo.InvariantCulture, part);

    /// <summary>The words a part says to a reader of the named language.</summary>
    private static string SaidIn(string language, AvailabilityPart part) => SaidIn(new CultureInfo(language), part);

    /// <summary>The words a part says stood in one reader's hours, that reader's culture put on the thread for the
    /// asking and taken off again. The home of these three helpers is #530 (one test support file for the shared
    /// edge-case machinery), which this file reuses rather than widens.</summary>
    private static string SaidIn(CultureInfo reader, AvailabilityPart part)
    {
        ArgumentNullException.ThrowIfNull(part);

        var standing = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = reader;
            return part.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = standing;
        }
    }

    /// <summary>Something asked while the thread stands in a reader's hours: the asks below are not strings, so they
    /// take the culture rather than the words back.</summary>
    private static void SaidIn(string language, Action what)
    {
        ArgumentNullException.ThrowIfNull(what);

        var standing = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            what();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = standing;
        }
    }

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the record does with the word it is given is only seen when two are apart.</summary>
    private static string Own(string word) => new(word.ToCharArray());
}
