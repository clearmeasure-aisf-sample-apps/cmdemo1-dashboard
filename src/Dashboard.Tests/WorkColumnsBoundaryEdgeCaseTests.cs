using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkColumns" /> that the two files beside it leave
/// open, declared in <c>src/Dashboard/Work/WorkColumns.cs</c>.
/// <para>
/// <see cref="WorkColumnsTests" /> pins the board itself: the seven names in their order and the six that are active,
/// which names <see cref="WorkColumns.IsKnown" /> and <see cref="WorkColumns.IsDone" /> accept, the casing
/// <see cref="WorkColumns.Canonical" /> restores, the blank column shown as <see cref="WorkColumns.Unplaced" /> and
/// the unknown column trimmed but otherwise kept. <see cref="WorkColumnsEdgeCaseTests" /> - which already holds the
/// edge cases under the name this issue asked for, so what is left over is set down under a name of its own rather
/// than over that file, whose every test stands - pins the letters of each constant, the sizes of the two lists and
/// their refusal of an <c>Add</c> and a <c>Clear</c>, <see cref="WorkColumns.Same" /> with nothing on its left and
/// with letters that only look alike, and <see cref="WorkColumns.Canonical" /> leaving a padded name of the board in
/// its own letters.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the surface as reflection reads it, the eight names standing as
/// literals, the two lists as fields of their own, and the four members being the whole of what there is to call;
/// <see cref="WorkColumns.Same" /> handed nothing on its right, and nothing on either side, which the signature
/// promises against but which the method answers for itself rather than throwing over; that neither list yields to a
/// writer through <see cref="IList{T}" /> either - the index, the insert, the removal, and the read that still works -
/// and that the board and the active work are two lists, each of them the very same one at every asking;
/// <see cref="WorkColumns.Canonical" /> handing back the board's own string for all seven names and the very word
/// <see cref="WorkColumns.Unplaced" /> for every blank, while a name worn under spaces comes back a copy that reads
/// as the board's word and is not it, which is what lets a column of <c>"Done "</c> say "not done" to one question
/// and "done" to the next; spacing nobody can see, which is a blank in one code point and part of a name in the next;
/// a reader whose own alphabet turns an <c>i</c>, which moves nothing here; a column of any length, kept whole, never
/// made known and never left blank; and the name a column is filed under being neither blank nor padded, however the
/// column came.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every name, code point, length and
/// culture below is one the test brought.
/// </para>
/// </summary>
public class WorkColumnsBoundaryEdgeCaseTests
{
    // ----- the surface, as reflection reads it

    /// <summary>The whole public surface of the class, as it stands: the eight names, the two lists and the four
    /// members - and nothing else to call. A column, a list or a test added to the class has to be written here as
    /// well, which is the point: the board every page of work is built on cannot quietly grow a name nobody filed an
    /// item under. The class is a static one, sealed and never to be built, so there is no instance of the board for
    /// a caller to hold and no property to read the two lists through.</summary>
    [Fact]
    public void TheSurfaceIsTheEightNamesTheTwoListsAndTheFourMembers()
    {
        Assert.Equal(
            [
                "Active", "Canonical", "Done", "InProgress", "IsDone", "IsKnown", "Merged",
                "Order", "Review", "Same", "Tdd", "Todo", "Uat", "Unplaced",
            ],
            PublicSurface());

        var board = typeof(WorkColumns);

        Assert.True(board.IsAbstract);
        Assert.True(board.IsSealed);
        Assert.Empty(board.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(board.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Empty(board.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    /// <summary>The eight names as the fixed values they are: public, static literals of <see cref="string" />, each
    /// one given once in the declaration and read out of the running assembly as the word it spells - words the page
    /// compares against, and not fields a caller could be handed a different copy of.</summary>
    [Theory]
    [InlineData(nameof(WorkColumns.Todo), "Todo")]
    [InlineData(nameof(WorkColumns.InProgress), "In Progress")]
    [InlineData(nameof(WorkColumns.Review), "Review")]
    [InlineData(nameof(WorkColumns.Merged), "Merged")]
    [InlineData(nameof(WorkColumns.Tdd), "TDD")]
    [InlineData(nameof(WorkColumns.Uat), "UAT")]
    [InlineData(nameof(WorkColumns.Done), "Done")]
    [InlineData(nameof(WorkColumns.Unplaced), "Unplaced")]
    public void EachColumnNameIsAPublicLiteralOfText(string field, string value)
    {
        var name = typeof(WorkColumns).GetField(field, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

        Assert.Equal(field, name.Name);
        Assert.True(name.IsPublic);
        Assert.True(name.IsStatic);
        Assert.True(name.IsLiteral);
        Assert.False(name.IsInitOnly);
        Assert.Equal(typeof(string), name.FieldType);
        Assert.Equal(value, (string)name.GetRawConstantValue()!);
        Assert.Equal(value, name.GetValue(null));
    }

    /// <summary>The two lists as the fields they are and not as properties rebuilt at every asking: public, static,
    /// <see langword="readonly" /> and of <see cref="IReadOnlyList{T}" /> - the shape that hands a reader the board to
    /// walk and nothing to write through, and that no caller can replace once the class is loaded.</summary>
    [Theory]
    [InlineData(nameof(WorkColumns.Order))]
    [InlineData(nameof(WorkColumns.Active))]
    public void EachListOfTheBoardIsAReadOnlyFieldOfText(string field)
    {
        var list = typeof(WorkColumns).GetField(field, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

        Assert.True(list.IsPublic);
        Assert.True(list.IsStatic);
        Assert.False(list.IsLiteral);
        Assert.True(list.IsInitOnly);
        Assert.Equal(typeof(IReadOnlyList<string>), list.FieldType);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(list.GetValue(null));
    }

    /// <summary>The four members as the whole of what there is to call, and each as it is called: static, on text,
    /// the three questions answering <see cref="bool" /> and the one spelling answering <see cref="string" />.
    /// Nothing is generic, nothing is virtual, and nothing takes a second argument but <see cref="WorkColumns.Same" />,
    /// which is the only member a caller stands two columns in front of.</summary>
    [Fact]
    public void TheFourMembersAreTheWholeOfWhatCanBeCalledAndEachTakesText()
    {
        var methods = typeof(WorkColumns).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        var called = methods.Select(method => method.Name).Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(["Canonical", "IsDone", "IsKnown", "Same"], called);

        foreach (var method in methods)
        {
            Assert.True(method.IsStatic, $"{method.Name} is not static");
            Assert.True(method.IsPublic, $"{method.Name} is not public");
            Assert.False(method.IsGenericMethod, $"{method.Name} takes a type");
            Assert.False(method.IsVirtual, $"{method.Name} is virtual");
            Assert.False(method.IsAbstract, $"{method.Name} is left to another class to answer");
            Assert.NotEmpty(method.GetParameters());
            Assert.All(method.GetParameters(), parameter => Assert.Equal(typeof(string), parameter.ParameterType));
        }

        Assert.Equal(typeof(bool), Member("Same").ReturnType);
        Assert.Equal(2, Member("Same").GetParameters().Length);
        Assert.Equal(typeof(bool), Member("IsDone").ReturnType);
        Assert.Single(Member("IsDone").GetParameters());
        Assert.Equal(typeof(bool), Member("IsKnown").ReturnType);
        Assert.Single(Member("IsKnown").GetParameters());
        Assert.Equal(typeof(string), Member("Canonical").ReturnType);
        Assert.Single(Member("Canonical").GetParameters());
    }

    // ----- Same, handed nothing to stand on

    /// <summary>The right hand of <see cref="WorkColumns.Same" /> is written as text and not as text that may be
    /// missing - a promise the method does not enforce, and does not need enforcing to answer well: a name it was
    /// never given is answered rather than thrown over, and it never matches a name that is there, not even the
    /// nothing a blank column is. With nothing on either side there is nothing left to tell apart, and the two stand
    /// as the same; that is the answer a blank column and a column nobody asked about agree on.</summary>
    [Fact]
    public void SameWithNothingToCompareAgainstAnswersInsteadOfThrowing()
    {
        Assert.False(WorkColumns.Same("Review", null!));
        Assert.False(WorkColumns.Same(WorkColumns.Done, null!));
        Assert.False(WorkColumns.Same(string.Empty, null!));
        Assert.False(WorkColumns.Same(" ", null!));

        Assert.True(WorkColumns.Same(null, null!));
    }

    // ----- the two lists, as far as a writer gets

    /// <summary>Neither list yields to a writer, however the write is asked for. The file beside pins the
    /// <c>Add</c> and the <c>Clear</c>; refused here as well, on the board and on the active work alike, are the index
    /// written to, the insert, the removal by name and the removal by place - each with
    /// <see cref="NotSupportedException" />, and each after the read has been shown unhindered: the count, the index,
    /// the name at the place it stands at. The refusal is about a list being kept, not about one being empty.</summary>
    [Fact]
    public void NeitherListYieldsToAWriterThroughTheListInterfaceEither()
    {
        foreach (var board in new IReadOnlyList<string>[] { WorkColumns.Order, WorkColumns.Active })
        {
            var writable = (IList<string>)board;

            Assert.True(writable.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => writable[0] = "Blocked");
            Assert.Throws<NotSupportedException>(() => writable.Insert(0, "Blocked"));
            Assert.Throws<NotSupportedException>(() => _ = writable.Remove(WorkColumns.Review));
            Assert.Throws<NotSupportedException>(() => writable.RemoveAt(writable.Count - 1));

            Assert.Equal(board.Count, writable.Count);
            Assert.Equal(board[2], writable[2]);
            Assert.Equal(WorkColumns.Tdd, writable[4]);
        }

        Assert.Equal(6, ((IList<string>)WorkColumns.Order).IndexOf(WorkColumns.Done));
        Assert.Equal(-1, ((IList<string>)WorkColumns.Active).IndexOf(WorkColumns.Done));
    }

    /// <summary>The board and the active work are two lists and not one list wearing a different hat: neither is the
    /// other, and each of them is the very same list at every asking, since both are read off a field of the class
    /// rather than built again for whoever looks - which is what lets a page hold the board between two renders and
    /// mean the same seven columns.</summary>
    [Fact]
    public void TheBoardAndTheActiveWorkAreTwoListsAndEachIsItselfAtEveryAsking()
    {
        IReadOnlyList<string> asked = WorkColumns.Order;
        IReadOnlyList<string> again = WorkColumns.Order;

        Assert.Same(asked, again);

        var active = WorkColumns.Active;
        var alsoActive = WorkColumns.Active;

        Assert.Same(active, alsoActive);
        Assert.NotSame(WorkColumns.Order, WorkColumns.Active);
    }

    // ----- Canonical, and whose string it hands back

    /// <summary>Every name of the board - all seven, where the file beside samples two - comes back as the board's own
    /// string: the very object the declaration wrote, whether the column was spelled as it stands, in lower case or
    /// in upper. Every blank answers with the one word of <see cref="WorkColumns.Unplaced" /> it is shown under,
    /// whatever kind of space it was left as. And a column that only reads like that word, in the file's own lower
    /// case, is kept as the feed wrote it: the same letters, a different string, and no seat on the board.</summary>
    [Fact]
    public void EveryNameOfTheBoardComesBackAsTheStringTheBoardWears()
    {
        foreach (var name in WorkColumns.Order)
        {
            Assert.Same(name, WorkColumns.Canonical(name));
            Assert.Same(name, WorkColumns.Canonical(name.ToLowerInvariant()));
            Assert.Same(name, WorkColumns.Canonical(name.ToUpperInvariant()));
        }

        foreach (var blank in new string?[] { null, "", " ", "\t", "\r\n", "\u00A0", " \t\r\n\u00A0 " })
        {
            Assert.Same(WorkColumns.Unplaced, WorkColumns.Canonical(blank));
        }

        Assert.Equal("unplaced", WorkColumns.Canonical("unplaced"));
        Assert.NotSame(WorkColumns.Unplaced, WorkColumns.Canonical("unplaced"));
    }

    /// <summary>The trap the board's spelling springs. A column that is one of the seven under spaces is not one of
    /// the seven: it is not known, not done and not the same as the word, so it is trimmed and kept - a copy that
    /// reads as the board's word, is not the same string as it, and answers the second asking what the first refused.
    /// <c>"Done "</c> is not done, and the name it is filed under is; four thousand spaces of the word behave
    /// exactly as one does, and none of this is a way of settling a column, only of saying it to a reader.</summary>
    [Fact]
    public void ANameWornUnderSpacesReadsAsTheBoardAndIsNotTheBoard()
    {
        Assert.False(WorkColumns.IsDone("Done "));
        Assert.False(WorkColumns.IsKnown(" In Progress "));

        Assert.Equal(WorkColumns.Done, WorkColumns.Canonical("Done "));
        Assert.NotSame(WorkColumns.Done, WorkColumns.Canonical("Done "));
        Assert.True(WorkColumns.IsDone(WorkColumns.Canonical("Done ")));
        Assert.True(WorkColumns.IsKnown(WorkColumns.Canonical(" In Progress ")));

        var underFourThousandSpaces = WorkColumns.Done + new string(' ', 4096);

        Assert.False(WorkColumns.IsDone(underFourThousandSpaces));
        Assert.Equal(WorkColumns.Done, WorkColumns.Canonical(underFourThousandSpaces));
        Assert.Equal(WorkColumns.Done.Length, WorkColumns.Canonical(underFourThousandSpaces).Length);
    }

    // ----- spacing nobody can see

    /// <summary>A blank is whatever the board says a blank is, code point by code point: a no-break space and an em
    /// space are blanks, and a column of nothing but them is shown as <see cref="WorkColumns.Unplaced" /> - while a
    /// zero width space and a byte order mark are no space at all to the board, so a column wearing one of those is an
    /// unknown name kept whole, shown under the invisible word the feed wrote for it. None of it is known, none of it
    /// done, and none of it the empty column the board matches against nothing.</summary>
    [Theory]
    [InlineData("\u00A0", "Unplaced")]
    [InlineData("\u2003", "Unplaced")]
    [InlineData("\u00A0 \u2003\t", "Unplaced")]
    [InlineData("\u200B", "\u200B")]
    [InlineData("\uFEFF", "\uFEFF")]
    [InlineData("\u200B\u200B", "\u200B\u200B")]
    public void SpacingNobodyCanSeeIsABlankInOneCodePointAndANameInTheNext(string column, string expected)
    {
        Assert.Equal(expected, WorkColumns.Canonical(column));
        Assert.False(WorkColumns.IsKnown(column));
        Assert.False(WorkColumns.IsDone(column));
        Assert.False(WorkColumns.Same(column, string.Empty));
    }

    /// <summary>Invisible spacing is walked past on the way out and not forgiven on the way in: a name of the board
    /// under no-break spaces is trimmed down to the board's word and its letters, but it is not known, not done and
    /// not the same as that word, and the board's spelling is only ever restored to a column that matched it whole.
    /// A blank that starts with a zero width space is no blank at all, and the no-break space behind it is walked off
    /// the end like any other: what is left is the one letter the feed wrote, and no word of the board's.</summary>
    [Fact]
    public void InvisibleSpacingIsWalkedPastOnTheWayOutAndNotForgivenOnTheWayIn()
    {
        Assert.Equal("Blocked", WorkColumns.Canonical("\u00A0Blocked\u00A0"));
        Assert.Equal(WorkColumns.Review, WorkColumns.Canonical("\u00A0Review"));
        Assert.Equal(WorkColumns.Unplaced, WorkColumns.Canonical("\u00A0\u2003 \t"));

        Assert.False(WorkColumns.IsKnown("\u00A0Done"));
        Assert.False(WorkColumns.IsDone("\u00A0Done"));
        Assert.False(WorkColumns.Same("Done\u00A0", WorkColumns.Done));
        Assert.Equal("\u200B", WorkColumns.Canonical("\u200B\u00A0 "));
        Assert.NotEqual(WorkColumns.Unplaced, WorkColumns.Canonical("\u200B\u00A0 "));
    }

    // ----- a reader whose alphabet is not the board's

    /// <summary>The board is matched by the letters and by nothing else, so no reader moves it: a Turkish one, whose
    /// own rules make a dotless <c>ı</c> the lower case of <c>I</c> and an <c>i</c> the lower case of a dotted
    /// <c>İ</c>, is told the same names, the same "done" and the same <see cref="WorkColumns.Unplaced" /> as everyone
    /// else - a German, a Russian, an American and the invariant reader with it. Nothing of the board is read out of
    /// an alphabet; the seven words are matched letter for letter, case forgiven and nothing else.</summary>
    [Theory]
    [InlineData("tr-TR")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    [InlineData("en-US")]
    [InlineData("")]
    public void TheBoardDoesNotTurnForWhoeverReadsIt(string culture)
    {
        AsAReaderOf(culture, () =>
        {
            Assert.True(WorkColumns.Same("in progress", "IN PROGRESS"));
            Assert.True(WorkColumns.IsKnown("tdd"));
            Assert.True(WorkColumns.IsDone("dOnE"));
            Assert.False(WorkColumns.IsKnown("ın progress"));
            Assert.False(WorkColumns.Same("İn Progress", WorkColumns.InProgress));
            Assert.Equal(WorkColumns.Unplaced, WorkColumns.Canonical(" "));
            Assert.Same(WorkColumns.Merged, WorkColumns.Canonical("MERGED"));
            Assert.Same(WorkColumns.Review, WorkColumns.Canonical("rEvIeW"));
        });
    }

    /// <summary>The same column matched two ways by the same Turkish reader, which is what keeps the test above from
    /// saying nothing: by that reader's own rules, <c>ın progress</c> is <c>In Progress</c> and <c>İ</c> is <c>i</c>;
    /// by the board's, neither pair is one name and the column is a name of the feed. A spelling of
    /// <see cref="WorkColumns.Same" /> that asked the reader instead of the letters would show the same column of
    /// <c>In Progress</c> under a name of its own to one reader and under the board's to another - and the work items
    /// grouped by it would not add up the same twice.</summary>
    [Fact]
    public void AReadersOwnRulesWouldForgiveWhatTheBoardDoesNot()
    {
        AsAReaderOf("tr-TR", () =>
        {
            var alphabet = CultureInfo.CurrentCulture.CompareInfo;

            Assert.Equal(0, alphabet.Compare("ın progress", "In Progress", CompareOptions.IgnoreCase));
            Assert.Equal(0, alphabet.Compare("İ", "i", CompareOptions.IgnoreCase));
            Assert.NotEqual(0, string.CompareOrdinal("ın progress", "In Progress"));
            Assert.False(WorkColumns.Same("ın progress", WorkColumns.InProgress));
            Assert.False(WorkColumns.IsKnown("İn Progress"));
            Assert.Equal("ın progress", WorkColumns.Canonical("ın progress"));
        });
    }

    // ----- a column of any length

    /// <summary>No length makes a column one of the board and no length makes it a blank: a name of one letter and a
    /// name of four thousand are both unknown, both kept whole - the copy handed back as long as the column it was
    /// made from, nothing cut, spelled out or shortened to fit - and neither done nor the same as one of the seven. A
    /// column of spaces is <see cref="WorkColumns.Unplaced" /> however long it is, and the word given back is the
    /// eight letters of that name rather than the length that was asked.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(11)]
    [InlineData(64)]
    [InlineData(4096)]
    public void AColumnOfAnyLengthIsKeptWholeAndIsStillNoColumnOfTheBoard(int length)
    {
        var column = new string('x', length);

        Assert.False(WorkColumns.IsKnown(column));
        Assert.False(WorkColumns.IsDone(column));
        Assert.False(WorkColumns.Same(column, WorkColumns.Todo));
        Assert.Equal(column, WorkColumns.Canonical(column));
        Assert.Equal(length, WorkColumns.Canonical(column).Length);

        var blank = new string(' ', length);

        Assert.False(WorkColumns.IsKnown(blank));
        Assert.False(WorkColumns.IsDone(blank));
        Assert.Equal(WorkColumns.Unplaced, WorkColumns.Canonical(blank));
        Assert.Equal(WorkColumns.Unplaced.Length, WorkColumns.Canonical(blank).Length);
    }

    /// <summary>The name a column is filed under is neither nothing nor padded, however the column came: never the
    /// empty text, never all spaces, never a name with something left to walk past at either end - and never longer
    /// than the column it was made from, since the only way the board gives anything back is by taking letters off.
    /// The one word that can outlive the column it answers is <see cref="WorkColumns.Unplaced" />, and it is reached
    /// only by a column that said nothing at all. What one asking gives, a second asking agrees to: every column below
    /// is answered the same twice, the ones that reach the board's own spelling by their letters as well as those the
    /// feed wrote as they stand.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    [InlineData("\u00A0")]
    [InlineData("\u200B")]
    [InlineData("Unplaced")]
    [InlineData("unplaced")]
    [InlineData("  Review  ")]
    [InlineData("In  Progress")]
    [InlineData("Blocked")]
    [InlineData("done")]
    [InlineData("DONE")]
    [InlineData("Todo\n")]
    public void TheNameAColumnIsFiledUnderIsNeitherBlankNorPadded(string column)
    {
        var filed = WorkColumns.Canonical(column);

        Assert.NotEmpty(filed);
        Assert.False(string.IsNullOrWhiteSpace(filed));
        Assert.Equal(filed, filed.Trim());
        Assert.True(
            filed.Length <= column.Length || filed == WorkColumns.Unplaced,
            $"'{filed}' outlived the {column.Length}-letter column it was made from and stands for no blank");
        Assert.Equal(filed, WorkColumns.Canonical(filed));
    }

    // ----- the reading of it

    /// <summary>Asks the board as a reader of the culture named would - the invariant one for a name of no culture at
    /// all - and puts the thread back as it found it, the way <see cref="ViewerCulture" /> does for a German one. The
    /// board takes no culture to work with, so this says nothing about how an answer is reached and everything about
    /// who is left holding it.</summary>
    private static void AsAReaderOf(string culture, Action what)
    {
        ArgumentNullException.ThrowIfNull(what);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = culture.Length == 0
                ? CultureInfo.InvariantCulture
                : CultureInfo.GetCultureInfo(culture);
            what();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }

    /// <summary>The public members the class declares itself: the eight names and the two lists as the fields they
    /// are, and the four members as the methods they are - the getters and setters that come with a member left out,
    /// the names deduplicated so that an overload says one name, and all of it in the order the names are said.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkColumns)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>One of the four members, by name, as the running assembly holds it.</summary>
    private static MethodInfo Member(string name) =>
        typeof(WorkColumns).GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;
}
