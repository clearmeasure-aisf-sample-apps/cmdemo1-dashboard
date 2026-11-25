using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkWip" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsText.cs</c> that is one column of the work-in-progress strip and how many items
/// the file counts there. It has two parts - the column's name and the count - and neither is optional.
/// <para>
/// What the other files already pin is left alone. The strip as a whole is <see cref="WorkWipTests" />: that the
/// file's counts win for the board's columns and the items are counted only without them, that Done is never work in
/// progress, that a column the board does not know is gathered under <see cref="WorkColumns.Unplaced" /> last, and
/// that the rows follow the board's order - all of it read out of <c>row.Column</c> and <c>row.Count</c>.
/// <see cref="WorkItemsTests" /> pins the same strip out of the sample file and of the live feed, and
/// <see cref="WorkItemsTextOpenEdgeCaseTests" /> pins a count the file gives below zero. None of those ever built a
/// row by hand, so the record's own members - the constructor and the property each part lands on, the equality and
/// the hash a row is asked for, the falling apart, the words, the copy, and the surface - were unpinned.
/// </para>
/// <para>
/// What is pinned here, and only that: the two parts held as the caller gave them, with no canonicalising and no
/// trimming of the column's spelling and none of the count, at both ends of an <see cref="int" /> and below zero; a
/// column named as nothing at all and a column named as the empty word, each held as it was given and neither turned
/// into <see cref="WorkColumns.Unplaced" />; two rows that are one row while their two parts are, and two rows that
/// are not - including the two spellings of one board column, which are one column to <see cref="WorkColumns" /> and
/// two rows to the record; the hash, the set, and the two operators; the copy that changes either part; the words the
/// row says and the reader it says them to; the rows <see cref="WorkItemsText.Wip(WorkItemsStats, IReadOnlyList{WorkItem})" />
/// returns being these very values; and the surface, so that the day a part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a row holds the column and the count it
/// was built of and calls nothing at all, so every value below is one the test brought, and every call says the same
/// thing twice.
/// </para>
/// </summary>
public class WorkWipEdgeCaseTests
{
    private static readonly IReadOnlyList<WorkItem> NoItems = [];

    /// <summary>A row as the strip builds one: the column's own spelling, and the count beside it.</summary>
    private static WorkWip Row(string column, int count) => new(column, count);

    /// <summary>A work item standing in a column of its own; nothing else about it is asked.</summary>
    private static WorkItem Item(int number, string? column) =>
        new(number, $"Item {number}", null, null, column, [], null, null, [], [], null, WorkItemEnvironments.None, null);

    /// <summary><c>stats.wipByColumn</c> as the file gives it: the column's own spelling, matched however it is cased.</summary>
    private static WorkItemsStats Stats(params (string Column, int Count)[] counts) =>
        new(counts.ToDictionary(pair => pair.Column, pair => pair.Count, StringComparer.OrdinalIgnoreCase), null, null, [], null, null);

    // --------------------------------- the two parts, by the names of their own properties

    /// <summary>The constructor's two arguments come back as the property of the same name: the column first and the
    /// count beside it, the order the strip shows them. Each is held as the very value the row was built of, and a
    /// second row built of the same two parts is the same row - a row is a value, not the strip's place in a list.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var row = new WorkWip(WorkColumns.Review, 3);

        Assert.Equal(WorkColumns.Review, row.Column);
        Assert.Equal(3, row.Count);

        Assert.Equal(Row(WorkColumns.Review, 3), row);
        Assert.NotSame(Row(WorkColumns.Review, 3), row);

        // A row written with the two parts the other way round about is still read the right way: the word "6" is the
        // column's name and the number 3 is the count, and neither is handed to the other's property.
        var writtenTheOtherWayAbout = new WorkWip("6", 3);

        Assert.Equal("6", writtenTheOtherWayAbout.Column);
        Assert.Equal(3, writtenTheOtherWayAbout.Count);
    }

    /// <summary>Every name the board has - the seven of <see cref="WorkColumns.Order" /> and
    /// <see cref="WorkColumns.Unplaced" /> - is held by a row exactly as the caller spelled it. The row canonicalises
    /// nothing: <see cref="WorkColumns.Canonical" /> is the one that turns a spelling into the board's word, and no
    /// part of <see cref="WorkWip" /> calls it.</summary>
    [Theory]
    [InlineData(WorkColumns.Todo)]
    [InlineData(WorkColumns.InProgress)]
    [InlineData(WorkColumns.Review)]
    [InlineData(WorkColumns.Merged)]
    [InlineData(WorkColumns.Tdd)]
    [InlineData(WorkColumns.Uat)]
    [InlineData(WorkColumns.Done)]
    [InlineData(WorkColumns.Unplaced)]
    public void EveryNameTheBoardHasIsHeldAsTheCallerGaveIt(string column)
    {
        var row = new WorkWip(column, 7);

        Assert.Equal(column, row.Column);
        Assert.Equal(7, row.Count);
        Assert.Equal(Row(column, 7), row);
        Assert.NotEqual(Row(column, 6), row);
    }

    /// <summary>The count is held as it was given, at the ends an <see cref="int" /> runs to and below zero: nothing
    /// is clamped to zero, nothing is saturated to the top, and nothing is taken as an absent count - a row has no
    /// part that can be absent, so a count the file gave below zero is shown as the file gave it.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(1_000_000)]
    [InlineData(int.MaxValue - 1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void TheCountIsHeldAtBothEndsOfAnIntAndBelowZero(int count)
    {
        var row = new WorkWip(WorkColumns.Todo, count);

        Assert.Equal(count, row.Count);
        Assert.Equal(Row(WorkColumns.Todo, count), row);
        Assert.Equal(Row(WorkColumns.Todo, count).GetHashCode(), row.GetHashCode());
        Assert.Contains($"Count = {count.ToString(CultureInfo.InvariantCulture)}", CultureScope.Words(row), StringComparison.Ordinal);
    }

    /// <summary>A row counting nothing is a row, and not the absence of one: it holds a column, it says both of its
    /// parts, it is one with another row counting nothing in the same column, and it is not one with a row of no
    /// column at all. The strip leans on this - a column the file counts nothing in is shown at zero rather than
    /// dropped - and the dropping is <see cref="WorkItemsText.Wip(WorkItemsStats, IReadOnlyList{WorkItem})" />'s
    /// doing, never the record's.</summary>
    [Fact]
    public void ARowCountingNothingIsARowAndNotTheAbsenceOfOne()
    {
        var zero = new WorkWip(WorkColumns.Merged, 0);

        Assert.NotNull(zero);
        Assert.Equal(WorkColumns.Merged, zero.Column);
        Assert.Equal(0, zero.Count);
        Assert.Equal(Row(WorkColumns.Merged, 0), zero);
        Assert.NotEqual(Row(WorkColumns.Merged, 1), zero);
        Assert.True(zero.ToString().Length > 0);

        var set = new HashSet<WorkWip>([zero, Row(WorkColumns.Merged, 0), Row(WorkColumns.Merged, 1)]);

        Assert.Equal(2, set.Count);
    }

    /// <summary>A column named as the empty word, or as nothing but spaces, is held as it was given: the record trims
    /// nothing and refuses nothing. Turning a blank column into <see cref="WorkColumns.Unplaced" /> is
    /// <see cref="WorkColumns.Canonical" /> and the strip's job, so the two blanks are two rows to the record even
    /// though the board puts both of them under one name.</summary>
    [Fact]
    public void AColumnNamedWithNothingOrWithSpacesIsHeldAsItWasGiven()
    {
        var empty = new WorkWip(string.Empty, 2);
        var spaces = new WorkWip("   ", 2);

        Assert.Equal(string.Empty, empty.Column);
        Assert.Equal("   ", spaces.Column);

        Assert.NotEqual(empty, spaces);
        Assert.NotEqual(empty, Row(WorkColumns.Unplaced, 2));
        Assert.NotEqual(spaces, Row(WorkColumns.Unplaced, 2));
        Assert.Contains("Column = ,", CultureScope.Words(empty), StringComparison.Ordinal);

        // And a name with spaces around the board's word is no board column at all - which is why the strip puts such
        // an item under Unplaced - and it is a third name to the row.
        Assert.False(WorkColumns.IsKnown(" todo "));
        Assert.NotEqual(Row(" todo ", 2), Row(WorkColumns.Todo, 2));
    }

    /// <summary>A row built with no column at all is built: the record guards nothing, so <c>Column</c> comes back as
    /// nothing, the words say a column of nothing, and the row is one with another row of nothing and not one with a
    /// row of the empty word. Nothing on the page builds one - the strip hands every row a name, and
    /// <see cref="WorkItemsText.ColumnId" /> is the member that refuses a blank name - so this is the record's own
    /// boundary, pinned so that the day a guard is added to it is a decided thing and not a surprise.</summary>
    [Fact]
    public void AColumnThatIsNothingAtAllIsHeldAsNothingAndNotRefused()
    {
        var noColumn = new WorkWip(null!, 2);

        Assert.Null(noColumn.Column);
        Assert.Equal(2, noColumn.Count);
        Assert.Equal(new WorkWip(null!, 2), noColumn);
        Assert.NotEqual(new WorkWip(string.Empty, 2), noColumn);
        Assert.Contains("Column = ,", CultureScope.Words(noColumn), StringComparison.Ordinal);

        // The blank the strip does use is a name, so it is a row of its own beside the row of nothing.
        Assert.NotEqual(Row(WorkColumns.Unplaced, 2), noColumn);
    }

    // --------------------------------- the equality, the hash, the operators

    /// <summary>Two rows are one row while their two parts are one, and are not one the moment either part differs:
    /// another column is another row, and so is another count. The hash follows, and stands still from one call to the
    /// next, which is what lets a row be a key as well as a cell.</summary>
    [Fact]
    public void TwoRowsAreOneRowWhileTheirTwoPartsAre()
    {
        var row = new WorkWip(WorkColumns.Todo, 3);
        var again = new WorkWip(WorkColumns.Todo, 3);

        Assert.Equal(row, again);
        Assert.True(row == again);
        Assert.False(row != again);
        Assert.Equal(row.GetHashCode(), again.GetHashCode());
        Assert.Equal(row.GetHashCode(), row.GetHashCode());

        var anotherCount = new WorkWip(WorkColumns.Todo, 4);
        var anotherColumn = new WorkWip(WorkColumns.InProgress, 3);
        var neither = new WorkWip(WorkColumns.Uat, 0);

        Assert.NotEqual(row, anotherCount);
        Assert.NotEqual(row, anotherColumn);
        Assert.NotEqual(row, neither);
        Assert.True(row != anotherCount);
        Assert.False(row == anotherCount);
        Assert.NotEqual(row.GetHashCode(), anotherColumn.GetHashCode());
    }

    /// <summary>One board column spelled two ways is one column to the board and two rows to the record: the column
    /// name is compared as the word it is, letter for letter and case for case, because the record calls nothing on
    /// the board's behalf. This is the trap the strip avoids by writing every row with
    /// <see cref="WorkColumns.Active" />'s own names, and it is pinned here so that a change to it - a column typed
    /// value, or a canonicalising constructor - is made on purpose.</summary>
    [Fact]
    public void OneColumnSpelledTwoWaysIsTwoRowsToTheRecordAndOneColumnToTheBoard()
    {
        Assert.NotEqual(Row("todo", 3), Row(WorkColumns.Todo, 3));
        Assert.NotEqual(Row("TODO", 3), Row(WorkColumns.Todo, 3));
        Assert.NotEqual(Row("todo ", 3), Row(" todo", 3));

        var set = new HashSet<WorkWip>([Row("todo", 3), Row(WorkColumns.Todo, 3), Row("TODO", 3)]);

        Assert.Equal(3, set.Count);

        // The board itself knows the three as one column, however: the row is not where a spelling is settled.
        Assert.True(WorkColumns.IsKnown("todo"));
        Assert.Equal(WorkColumns.Todo, WorkColumns.Canonical("todo"));
        Assert.True(WorkColumns.Same("TODO", WorkColumns.Todo));
    }

    /// <summary>No row is one with no row, and no row is one with a thing that is not a row - not the empty word, not
    /// the count on its own, not another record of two parts that happen to be the same two parts. A set may hold a
    /// row of nothing beside a row of something without confusing the two.</summary>
    [Fact]
    public void NoRowIsOneWithNoRowAndNotWithAnyOtherThing()
    {
        var row = Row(WorkColumns.Todo, 3);

        Assert.NotNull(row);
        Assert.False(row.Equals(null));
        Assert.False(row.Equals((object?)null));
        Assert.NotEqual(row, (object)"Todo");
        Assert.NotEqual(row, (object)3);
        Assert.NotEqual(row, (object)new WorkColumnGroup(WorkColumns.Todo, []));

        var set = new HashSet<WorkWip?>([null, row, Row(WorkColumns.Todo, 3)]);

        Assert.Equal(2, set.Count);
        Assert.Contains(row, set);
    }

    /// <summary>A copy that changes neither part is the same row, and a copy that changes either one is another row:
    /// the <c>with</c> of the record turns the two parts and nothing else, and the untouched copy keeps both.</summary>
    [Fact]
    public void ACopyThatChangesEitherPartIsAnotherRowAndOneThatChangesNeitherIsTheSameRow()
    {
        var row = Row(WorkColumns.Review, 2);

        Assert.Equal(row, row with { Count = 2 });
        Assert.NotSame(row, row with { Count = 2 });

        Assert.Equal(Row(WorkColumns.Merged, 2), row with { Column = WorkColumns.Merged });
        Assert.Equal(Row(WorkColumns.Review, 5), row with { Count = 5 });
        Assert.NotEqual(row, row with { Column = WorkColumns.Merged });
        Assert.NotEqual(row, row with { Count = 5 });

        // The ends of an int turn as easily as any count, and the copy keeps the column while it turns them.
        Assert.Equal(Row(WorkColumns.Review, int.MinValue), row with { Count = int.MinValue });
        Assert.Equal(WorkColumns.Review, (row with { Count = int.MinValue }).Column);
    }

    /// <summary>The row falls apart into its two parts, in the order they were built: the column out first and the
    /// count after it, the ends of an int and a column of nothing included.</summary>
    [Fact]
    public void TheRowFallsApartIntoItsTwoPartsInTheOrderTheyWereBuilt()
    {
        var (column, count) = Row(WorkColumns.Tdd, 8);

        Assert.Equal(WorkColumns.Tdd, column);
        Assert.Equal(8, count);

        string nothing;
        int atTheBottom;
        new WorkWip(null!, int.MinValue).Deconstruct(out nothing, out atTheBottom);

        Assert.Null(nothing);
        Assert.Equal(int.MinValue, atTheBottom);

        // The two parts come back as the two parts they went in, in the order the constructor names them.
        new WorkWip(WorkColumns.Uat, int.MaxValue).Deconstruct(out var last, out var top);
        Assert.Equal(WorkColumns.Uat, last);
        Assert.Equal(int.MaxValue, top);
    }

    // --------------------------------- the words the row says

    /// <summary>The row says its two parts in the order they are named, in the words of the record: the column's own
    /// spelling, and the count as the file gave it, below zero included. This is the voice of the debugger and of the
    /// log, not the voice of the page - the page has its own words in <see cref="WorkItemsText" />.</summary>
    [Fact]
    public void TheRowSaysItsTwoPartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("WorkWip { Column = Todo, Count = 3 }", CultureScope.Words(Row(WorkColumns.Todo, 3)));
        Assert.Equal("WorkWip { Column = In Progress, Count = 0 }", CultureScope.Words(Row(WorkColumns.InProgress, 0)));
        Assert.Equal("WorkWip { Column = Unplaced, Count = -2 }", CultureScope.Words(Row(WorkColumns.Unplaced, -2)));
    }

    /// <summary>The row says the same words to a reader of German, of French or of the invariant culture as to a
    /// reader of English: a column name is a name and a count here is written in plain digits, with no thousands
    /// separator and no other numeral for any reader, so no strip of work in progress depends on who reads it.</summary>
    [Fact]
    public void TheRowSaysTheSameWordsToEveryReader()
    {
        var row = Row(WorkColumns.Uat, int.MinValue);
        var english = CultureScope.Says("en-US", () => row.ToString());

        Assert.Equal(english, CultureScope.Says("de-DE", () => row.ToString()));
        Assert.Equal(english, CultureScope.Says("fr-FR", () => row.ToString()));
        Assert.Equal(english, CultureScope.Words(row));
    }

    // --------------------------------- the rows the strip returns are these rows

    /// <summary>The strip's rows are these very values: a whole strip read from the file's counts, and a whole strip
    /// counted from the items, each compares equal to the rows the test builds by hand: the record's equality, not the
    /// reference, is what comparing the two strips comes down to. The top of an int and a count below zero pass
    /// through the strip as they pass through the record, and the strip's own rows are written with the board's own
    /// names, so no row of the strip is one of the two-spellings rows below.</summary>
    [Fact]
    public void TheRowsTheStripReturnsAreTheseVeryRows()
    {
        var fromFile = WorkItemsText.Wip(Stats(("todo", int.MaxValue), ("Release Queue", 1), ("Blocked", -2)), NoItems);

        Assert.Equal(
            [
                Row(WorkColumns.Todo, int.MaxValue),
                Row(WorkColumns.InProgress, 0),
                Row(WorkColumns.Review, 0),
                Row(WorkColumns.Merged, 0),
                Row(WorkColumns.Tdd, 0),
                Row(WorkColumns.Uat, 0),
            ],
            fromFile);

        var fromItems = WorkItemsText.Wip(WorkItemsStats.Empty, [Item(1, "Todo"), Item(2, "Todo"), Item(3, "Release Queue"), Item(4, WorkColumns.Done)]);

        Assert.Equal(
            [
                Row(WorkColumns.Todo, 2),
                Row(WorkColumns.InProgress, 0),
                Row(WorkColumns.Review, 0),
                Row(WorkColumns.Merged, 0),
                Row(WorkColumns.Tdd, 0),
                Row(WorkColumns.Uat, 0),
                Row(WorkColumns.Unplaced, 1),
            ],
            fromItems);

        // Every row of the strip is a row of the board's own name: none of them was built with a spelling the record
        // would hold and the board would not, so a set of them holds one row per column.
        Assert.Equal(fromItems.Count, new HashSet<WorkWip>(fromItems).Count);
    }

    // --------------------------------- the record's own shape

    /// <summary>The row's whole public surface as it stands: the two parts named in the order the constructor names
    /// them - the column, then the count - and the members every record is asked, falling apart, comparing, hashing
    /// and saying itself, and nothing else to call. Neither part is optional, neither is written after the row is
    /// built, and a third part added to the record has to be written here as well, which is the point: the strip
    /// cannot be shown a part nobody decided what it means.</summary>
    [Fact]
    public void TheRowHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkWip);

        Assert.True(type.IsSealed);

        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(["Column", "Count"], [.. properties.Keys]);
        Assert.Equal(typeof(string), properties["Column"].PropertyType);
        Assert.Equal(typeof(int), properties["Count"].PropertyType);
        Assert.All(properties.Values, property => Assert.True(property.CanWrite));

        Assert.Equal(
            ["Column", "Count", "Deconstruct", "Equals", "GetHashCode", "ToString", "op_Equality", "op_Inequality"],
            [.. PublicSurface(type)]);

        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
        var parameters = constructor.GetParameters();

        Assert.Equal(["Column", "Count"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(int), parameters[1].ParameterType);
        Assert.False(parameters[0].IsOptional);
        Assert.False(parameters[1].IsOptional);

        var deconstruct = type.GetMethod("Deconstruct")!;

        Assert.True(deconstruct.IsPublic);
        Assert.Equal(
            [typeof(string), typeof(int)],
            [.. deconstruct.GetParameters()
                .Select(parameter => parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType)]);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the row declares itself: its two properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
