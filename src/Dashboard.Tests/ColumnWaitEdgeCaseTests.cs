using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ColumnWait" />: one column's wait, the record a
/// column's timed stays or the file's <c>timePerColumnHours</c> lands in - the column it is of, the hours it stands
/// on, and the mark saying it is the one the factory should look at first.
/// <para>
/// What is already pinned where it belongs: <see cref="ColumnWaitTests" /> pins what
/// <see cref="WorkBotText.ColumnWaits" /> puts in the three parts and in which order - the file's column times
/// winning over the items' own stays, the longest first and the name breaking a tie, the blank column coming out as
/// <see cref="WorkColumns.Unplaced" /> while a column the board does not know keeps the name it was given, a stay
/// that started after it ended timing nothing, and a list with nothing timed standing empty; it pins the mark on the
/// wait the list is asked for through <see cref="WorkBotText.Longest" />, both when exactly one wait carries it and
/// when a tie gives it to all of them, and on none; and it pins the words of the waits and of their hours, which are
/// <see cref="WorkItemsText.Duration" />'s and not the record's.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the constructor handing each of its three arguments to the property of
/// its name and holding the very <see cref="WorkDuration" /> it was given - refusing nothing, the column that is not
/// there and the hours that are not there both kept as the nothing they are; the column kept in its exact letters,
/// neither trimmed, turned to the board's canonical name nor cut, which is the saying of a wait rather than the
/// making of one; the hours at the ends a number runs to, the day stands unsaid and is no day at zero; the mark said
/// as it is given and never worked out of the hours; the three parts coming back out in the order they went in; two
/// waits being one while their three parts are - the mark one of them, so a wait marked and the same wait unmarked
/// are two waits - and the hash following the equality; the copy that turns one part and keeps the other two; the
/// words the record says itself, as distinct from the page's; and the public surface, so that the day a fourth part
/// is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a wait holds a name, three numbers and a mark,
/// no moment and no address - the clock belongs to <see cref="WorkBotText.ColumnWaits" />, which timed waits are
/// pinned in <see cref="ColumnWaitTests" /> with the moments the tests bring - and every value below is one the test
/// brought.
/// </para>
/// </summary>
public class ColumnWaitEdgeCaseTests
{
    private static WorkDuration Hours(double? median = 6, double? p85 = 10, int? n = 3) => new(median, p85, n);

    // ----- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same
    /// name: the column the wait is of, the hours it stands on and the mark. The wait holds the very
    /// <see cref="WorkDuration" /> it was built of, not a copy of it, and a new wait of the same three parts is
    /// another wait rather than the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var duration = Hours(6, 10, 3);
        var wait = new ColumnWait(WorkColumns.Uat, duration, true);

        Assert.Equal(WorkColumns.Uat, wait.Column);
        Assert.Same(duration, wait.Duration);
        Assert.True(wait.Longest);

        var again = new ColumnWait(WorkColumns.Uat, duration, true);
        Assert.Equal(wait, again);
        Assert.NotSame(wait, again);

        // The mark falls both ways, and the wait says whichever way it was given.
        Assert.False(new ColumnWait(WorkColumns.Uat, duration, false).Longest);
        Assert.True(new ColumnWait(WorkColumns.Uat, duration, true).Longest);
    }

    /// <summary>The record guards nothing: a wait is built on a column that is not there and on hours that are not
    /// there, both kept as the <see langword="null" /> they are rather than turned to a name or to zero, and said
    /// back by every one of the record's own members - the parts, the mark, the equality, the hash and the words -
    /// without a word of complaint. What the page does with such a wait is the page's, and the waits the board's own
    /// numbers are made of are never like it: <see cref="WorkBotText.ColumnWaits" /> names a column for every wait it
    /// makes and times hours for it, pinned in <see cref="ColumnWaitTests" />.</summary>
    [Fact]
    public void TheWaitHoldsTheNothingItIsGivenAndRefusesNothing()
    {
        var nothing = new ColumnWait(null!, null!, false);

        Assert.Null(nothing.Column);
        Assert.Null(nothing.Duration);
        Assert.False(nothing.Longest);

        var (column, duration, longest) = nothing;
        Assert.Null(column);
        Assert.Null(duration);
        Assert.False(longest);

        var alsoNothing = new ColumnWait(null!, null!, false);
        Assert.Equal(nothing, alsoNothing);
        Assert.True(nothing == alsoNothing);
        Assert.Equal(nothing.GetHashCode(), alsoNothing.GetHashCode());
        Assert.Equal("ColumnWait { Column = , Duration = , Longest = False }", Words(nothing));

        // The mark stands on its own: a wait with no hours in it can carry it, and the page is left to say what it
        // is worth - the hours it has none of are the unsaid thing, not zero hours.
        var markedButUnmetered = new ColumnWait(WorkColumns.Review, null!, true);
        Assert.True(markedButUnmetered.Longest);
        Assert.Null(markedButUnmetered.Duration);
        Assert.NotEqual(nothing, markedButUnmetered);
    }

    // ----- the column, kept exactly as it is given

    /// <summary>The column of a wait is the letters the wait was built on: the empty name, the name that is only
    /// whitespace, the name padded with spaces, the name in the case the file writes it in and the name the board
    /// spells another way all stand where they stand. The trimming, the canonical name and the empty name turned to
    /// <see cref="WorkColumns.Unplaced" /> are the making of the waits, pinned in
    /// <see cref="ColumnWaitTests" />; the record repeats what it was told.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("  Review  ")]
    [InlineData("review")]
    [InlineData("uAT")]
    [InlineData("Blocked")]
    [InlineData("Unplaced")]
    public void TheColumnIsKeptExactlyAsItIsGiven(string column) =>
        Assert.Equal(column, new ColumnWait(column, Hours(1, 1, 1), false).Column);

    /// <summary>The column is kept whole and as it is written, at the ends a name runs to: five hundred letters, a
    /// name of accents and a tick, and the name that differs from the board's own only in its case or only in the
    /// spaces around it is another column to the record, and so another wait - which is why two waits may be named
    /// <c>UAT</c> and <c>uat</c> in one list only because whoever made them said so, and nothing in the record stops
    /// them.</summary>
    [Fact]
    public void AColumnOfFiveHundredLettersOrOfAccentsIsKeptWhole()
    {
        var longColumn = new string('W', 500);
        var accented = new ColumnWait("Ünknown ✓", Hours(2, 3, 1), false);
        var longWait = new ColumnWait(longColumn, Hours(2, 3, 1), false);

        Assert.Equal(500, longWait.Column.Length);
        Assert.StartsWith("WWWW", longWait.Column, StringComparison.Ordinal);
        Assert.EndsWith("WWWW", longWait.Column, StringComparison.Ordinal);
        Assert.Contains(longColumn, Words(longWait), StringComparison.Ordinal);

        Assert.Equal("Ünknown ✓", accented.Column);
        Assert.Contains("Column = Ünknown ✓,", Words(accented), StringComparison.Ordinal);

        // Case and padding are no news to the record: each of these is another column, and so another wait.
        var board = new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), true);
        Assert.NotEqual(board, new ColumnWait("uat", Hours(6, 10, 3), true));
        Assert.NotEqual(board, new ColumnWait(" UAT", Hours(6, 10, 3), true));
        Assert.NotEqual(WorkColumns.Uat, new ColumnWait("uat", Hours(6, 10, 3), true).Column);
    }

    // ----- the hours, held as the very record and at the ends the numbers run to

    /// <summary>The hours a wait stands on are the very <see cref="WorkDuration" /> the wait was built of, held at
    /// the ends a number runs to: the day that was not metered standing unsaid rather than at zero, the zero and an
    /// hour below it, the largest and the least number there are, the number that is no number, and the count of
    /// items at nought, at one and at the top of an <see cref="int" /> - or unsaid, which is not nought.</summary>
    [Fact]
    public void TheWaitHoldsTheHoursItIsGivenAtTheEndsANumberRunsTo()
    {
        var duration = Hours(6, 10, 3);
        var wait = new ColumnWait(WorkColumns.Review, duration, true);

        Assert.Same(duration, wait.Duration);
        Assert.Equal(6, wait.Duration.Median);
        Assert.Equal(10, wait.Duration.P85);
        Assert.Equal(3, wait.Duration.N);

        var unsaid = new ColumnWait(WorkColumns.Review, Hours(null, null, null), false);
        Assert.Null(unsaid.Duration.Median);
        Assert.Null(unsaid.Duration.P85);
        Assert.Null(unsaid.Duration.N);
        Assert.NotEqual(unsaid, new ColumnWait(WorkColumns.Review, Hours(0, 0, 0), false));

        var nought = new ColumnWait(WorkColumns.Review, Hours(0, 0, 0), false);
        Assert.Equal(0d, nought.Duration.Median);
        Assert.Equal(0, nought.Duration.N);

        var negative = new ColumnWait(WorkColumns.Review, Hours(-0.5, -0.25, 2), false);
        Assert.Equal(-0.5, negative.Duration.Median);
        Assert.Equal(-0.25, negative.Duration.P85);

        var ends = new ColumnWait(WorkColumns.Review, Hours(double.MinValue, double.MaxValue, int.MaxValue), false);
        Assert.Equal(double.MinValue, ends.Duration.Median);
        Assert.Equal(double.MaxValue, ends.Duration.P85);
        Assert.Equal(int.MaxValue, ends.Duration.N);

        var notANumber = new ColumnWait(WorkColumns.Review, Hours(double.NaN, double.NaN, 1), false);
        Assert.True(double.IsNaN(notANumber.Duration.Median!.Value));
        Assert.True(double.IsNaN(notANumber.Duration.P85!.Value));
    }

    /// <summary>The mark is said as it was given and is never worked out of the hours: the shortest wait on the
    /// board may carry it and the longest may do without, and a wait whose hours say nothing at all may carry it too.
    /// Whoever marks the waits is <see cref="WorkBotText.ColumnWaits" />, pinned in
    /// <see cref="ColumnWaitTests" /> marking the longest and, when two tie, marking both.</summary>
    [Theory]
    [InlineData(0.0, 0.0, 1, true)]
    [InlineData(1000.0, 2000.0, 9, false)]
    [InlineData(6.0, 10.0, 3, true)]
    [InlineData(6.0, 10.0, 3, false)]
    [InlineData(null, null, 0, true)]
    public void TheMarkIsSaidAsItIsGivenAndNeverWorkedOut(double? median, double? p85, int n, bool longest)
    {
        var wait = new ColumnWait(WorkColumns.Tdd, Hours(median, p85, n), longest);

        Assert.Equal(longest, wait.Longest);
        Assert.Equal(median, wait.Duration.Median);
        Assert.Equal(p85, wait.Duration.P85);
        Assert.Equal(n, wait.Duration.N);
    }

    // ----- the three parts coming back out, in the order they went in

    /// <summary>The three parts of a wait come back out in the order they went in - the column, the hours and the
    /// mark - the hours as the very record they were, and what came out goes back in the same way: the wait the three
    /// parts fall apart into is the wait they were. A wait built on nothing falls apart to the nothing it was built
    /// on, the mark the only part that cannot be nothing.</summary>
    [Fact]
    public void TheWaitFallsApartInTheOrderItWasMade()
    {
        var duration = Hours(6, 10, 3);
        var wait = new ColumnWait(WorkColumns.Uat, duration, true);

        var (column, hours, longest) = wait;

        Assert.Equal(WorkColumns.Uat, column);
        Assert.Same(duration, hours);
        Assert.True(longest);
        Assert.Equal(wait, new ColumnWait(column, hours, longest));

        var (noColumn, noHours, noMark) = new ColumnWait(null!, null!, false);
        Assert.Null(noColumn);
        Assert.Null(noHours);
        Assert.False(noMark);
    }

    // ----- the equality, the hash, the two

    /// <summary>Two waits are one while their three parts are - the column as the letters it is, the hours as the
    /// three numbers they stand on in whatever <see cref="WorkDuration" /> they stand in, and the mark as the
    /// <see cref="bool" /> it is - whichever of the ways of asking is used, from either side. The hash follows the
    /// equality, boxed or unboxed and from one call to the next, so the waits that are one are one wait of a set:
    /// which is how a wait the test writes is found again in the list the board's numbers made.</summary>
    [Fact]
    public void TwoWaitsAreOneWhileTheirThreePartsAre()
    {
        var one = new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), true);
        var two = new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), true);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<ColumnWait> { one, two });
        Assert.Contains(two, new HashSet<ColumnWait> { one, two });

        // The hours are compared as the record they are and not as the reference they stand on.
        Assert.NotSame(one.Duration, two.Duration);
        Assert.Equal(Hours(6, 10, 3), one.Duration);
    }

    /// <summary>A difference in one part is another wait, and the mark is one of the parts: the same column and the
    /// same hours marked and unmarked are two waits with two hashes, which is what lets a list hold exactly one
    /// longest wait and still be a list of the waits it was made of. A column in another case or padded with a
    /// space, a median turned by an hour, the p85 the file did not give turned to the one it did, the count of items
    /// at nought where it was unsaid - each is another wait, and a set of waits that differ in one part each holds
    /// them all apart.</summary>
    [Fact]
    public void AWaitThatDiffersInOnePartIsAnotherWait()
    {
        var one = new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), true);

        Assert.NotEqual(one, new ColumnWait(WorkColumns.Review, Hours(6, 10, 3), true));
        Assert.NotEqual(one, new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), false));
        Assert.NotEqual(one, new ColumnWait(WorkColumns.Uat, Hours(7, 10, 3), true));
        Assert.NotEqual(one, new ColumnWait(WorkColumns.Uat, Hours(6, null, 3), true));
        Assert.NotEqual(one, new ColumnWait(WorkColumns.Uat, Hours(6, 10, null), true));
        Assert.NotEqual(one, new ColumnWait(WorkColumns.Uat, Hours(6, 10, 0), true));
        Assert.NotEqual(one, new ColumnWait(WorkColumns.Uat, null!, true));

        // The mark alone is enough to part two waits, and it parts their hashes too.
        var unmarked = new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), false);
        Assert.False(one.Equals(unmarked));
        Assert.True(one != unmarked);
        Assert.Equal(2, new HashSet<ColumnWait> { one, unmarked }.Count);

        Assert.Equal(
            4,
            new HashSet<ColumnWait>
            {
                one,
                new ColumnWait("uat", Hours(6, 10, 3), true),
                new ColumnWait(WorkColumns.Uat, Hours(7, 10, 3), true),
                new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), false),
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two waits that are no wait at
    /// all are one, a wait and no wait are two - the equality and the inequality each saying it, from either side -
    /// and the wait asked about itself says no to the things it is not: its own words as a name, the hours it stands
    /// on as the record they are, the board's column word, and the anonymous thing that happens to carry all three
    /// of its parts.</summary>
    [Fact]
    public void NoWaitIsOneWithNoWaitAndNotWithAnyOtherThing()
    {
        ColumnWait? nothing = null;
        ColumnWait? alsoNothing = null;
        var there = new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), true);

        Assert.Null(default(ColumnWait));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (ColumnWait?)null);
        Assert.True(there != (ColumnWait?)null);
        Assert.True((ColumnWait?)null == nothing);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("ColumnWait { Column = UAT, Duration = , Longest = True }"));
        Assert.False(there.Equals(Hours(6, 10, 3)));
        Assert.False(there.Equals(WorkColumns.Uat));
        Assert.False(there.Equals(new { Column = WorkColumns.Uat, Duration = Hours(6, 10, 3), Longest = true }));
    }

    // ----- the copy, turning the part it names

    /// <summary>Each part may be turned as the wait is copied - the column to another column, the hours to another
    /// record or to none, the mark to the one it was not - and stands where it is turned to, while the parts the
    /// turn does not name stay the very instances the wait was built of: the copy is another wait and never the same
    /// reference, and the original stands as it stood. A copy that names no part at all is the wait it was copied
    /// from, in its parts and in its hash, and another reference.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherTwo()
    {
        var duration = Hours(6, 10, 3);
        var wait = new ColumnWait(WorkColumns.Uat, duration, true);

        var renamed = wait with { Column = WorkColumns.Review };
        Assert.Equal(WorkColumns.Review, renamed.Column);
        Assert.Same(duration, renamed.Duration);
        Assert.True(renamed.Longest);
        Assert.NotSame(wait, renamed);
        Assert.NotEqual(wait, renamed);

        var marked = new ColumnWait(WorkColumns.Uat, duration, false) with { Longest = true };
        Assert.True(marked.Longest);
        Assert.Same(duration, marked.Duration);
        Assert.Equal(wait, marked);

        var hoursTurned = wait with { Duration = Hours(1, 2, 2) };
        Assert.NotSame(duration, hoursTurned.Duration);
        Assert.Equal(Hours(1, 2, 2), hoursTurned.Duration);
        Assert.NotEqual(wait, hoursTurned);

        var hoursTurnedOff = wait with { Duration = null! };
        Assert.Null(hoursTurnedOff.Duration);
        Assert.Equal(WorkColumns.Uat, hoursTurnedOff.Column);
        Assert.True(hoursTurnedOff.Longest);

        var unchanged = wait with { };
        Assert.Equal(wait, unchanged);
        Assert.NotSame(wait, unchanged);
        Assert.Same(duration, unchanged.Duration);
        Assert.Equal(wait.GetHashCode(), unchanged.GetHashCode());

        // The original is as it was, whichever way the copies went.
        Assert.Equal(WorkColumns.Uat, wait.Column);
        Assert.Same(duration, wait.Duration);
        Assert.True(wait.Longest);
    }

    // ----- the words the wait says itself

    /// <summary>The words the wait says its three parts in: its own name at the front, the parts in the order the
    /// constructor names them, the hours as the record they stand in - and the part nobody said as the empty place
    /// between the mark and the comma. No word of the page is among them: the tile says <c>median</c> and <c>p85</c>
    /// in its own lower case, with its <c>·</c>, its <c>so far</c> and its <c>Longest wait:</c> - the record spells
    /// both names of the hours with the capital and says no unit at all. Those page words are pinned in
    /// <see cref="ColumnWaitTests" />.</summary>
    [Fact]
    public void TheWaitSaysItsThreePartsInTheOrderTheyAreNamed()
    {
        Assert.Equal(
            "ColumnWait { Column = UAT, Duration = WorkDuration { Median = 6, P85 = 10, N = 3 }, Longest = True }",
            Words(new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), true)));
        Assert.Equal(
            "ColumnWait { Column = Review, Duration = WorkDuration { Median = 1, P85 = 2, N = 2 }, Longest = False }",
            Words(new ColumnWait(WorkColumns.Review, Hours(1, 2, 2), false)));
        Assert.Equal(
            "ColumnWait { Column = , Duration = WorkDuration { Median = , P85 = , N =  }, Longest = False }",
            Words(new ColumnWait(string.Empty, Hours(null, null, null), false)));
        Assert.Equal(
            "ColumnWait { Column = , Duration = , Longest = False }",
            Words(new ColumnWait(null!, null!, false)));

        var full = Words(new ColumnWait(WorkColumns.Uat, Hours(6, 10, 3), true));
        foreach (var word in new[] { "median ", "p85 ", " items", " item", " · ", " so far", "Longest wait:", "—" })
        {
            Assert.DoesNotContain(word, full, StringComparison.Ordinal);
        }
    }

    /// <summary>The words at the ends the hours run to, said as the wait says them: the whole hour and the ten,
    /// a thousandth of an hour, and the two ends of the exponent a reader of a log looks for - the record saying
    /// each as the plain number it stands on, with no unit and no dash.</summary>
    [Theory]
    [InlineData(6, 10, "Median = 6, P85 = 10")]
    [InlineData(0.001, 0.002, "Median = 0.001, P85 = 0.002")]
    [InlineData(1.7976931348623157e+308, 1e+300, "Median = 1.7976931348623157E+308, P85 = 1E+300")]
    public void TheWordsSayTheHoursAtWhateverEndTheyStandOn(double median, double p85, string said) =>
        Assert.Contains(said, Words(new ColumnWait(WorkColumns.Uat, Hours(median, p85, 1), true)), StringComparison.Ordinal);

    /// <summary>The words of the hours that are no number, and of the count at either end of an <see cref="int" />:
    /// <c>NaN</c> by its word, and the number that is no number one wait's equal to itself, since the record's words
    /// and the record's equality say the same thing about it.</summary>
    [Fact]
    public void TheWordsSayTheHoursThatAreNoNumberAndTheCountAtEitherEnd()
    {
        Assert.Equal(
            "ColumnWait { Column = UAT, Duration = WorkDuration { Median = NaN, P85 = NaN, N =  }, Longest = False }",
            Words(new ColumnWait(WorkColumns.Uat, Hours(double.NaN, double.NaN, null), false)));
        Assert.Equal(
            "ColumnWait { Column = UAT, Duration = WorkDuration { Median = -2147483648, P85 = 0, N = 2147483647 }, Longest = True }",
            Words(new ColumnWait(WorkColumns.Uat, new WorkDuration(int.MinValue, 0, int.MaxValue), true)));

        var nan = new ColumnWait(WorkColumns.Uat, Hours(double.NaN, null, null), false);
        Assert.Equal(nan, new ColumnWait(WorkColumns.Uat, Hours(double.NaN, null, null), false));
    }

    /// <summary>The numbers in the wait's own words follow whoever reads them, which is what sets them apart from
    /// the page's: a German reader of the record is told <c>P85 = 10,5</c> while the column, the part names and the
    /// mark's <c>True</c> are the same letters to whichever reader - and two waits the two readers see differently
    /// worded are one wait still, for the words are no part of the equality.</summary>
    [Fact]
    public void TheWordsOfTheHoursFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var wait = new ColumnWait(WorkColumns.Tdd, Hours(6, 10.5, 3), true);

        Assert.Equal(
            "ColumnWait { Column = TDD, Duration = WorkDuration { Median = 6, P85 = 10.5, N = 3 }, Longest = True }",
            Words(wait));

        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "ColumnWait { Column = TDD, Duration = WorkDuration { Median = 6, P85 = 10,5, N = 3 }, Longest = True }",
                wait.ToString());
            Assert.Equal(wait, new ColumnWait(WorkColumns.Tdd, Hours(6, 10.5, 3), true));
        });

        Assert.Equal(wait, new ColumnWait(WorkColumns.Tdd, Hours(6, 10.5, 3), true));
    }

    // ----- the record's own shape

    /// <summary>The wait's whole public surface, as the running assembly holds it: its three parts named in the
    /// order the constructor names them - the column, the hours and the mark - and the members every record is
    /// asked, comparing, hashing, saying itself, falling apart and copying, the two operators kept although the
    /// compiler names them specially, since they are the <c>==</c> and the <c>!=</c> a caller writes - and nothing
    /// else to call. A fourth part added to the wait has to be written here as well, which is the point: the tile
    /// cannot be shown a part nobody decided what it means. The way in is one - the three parts, none of them left
    /// out - and the copy the <c>with</c> walks is the compiler's private one.</summary>
    [Fact]
    public void TheWaitHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(ColumnWait);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal([typeof(IEquatable<ColumnWait>)], type.GetInterfaces());
        Assert.Equal(typeof(object), type.BaseType);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Column", "Duration", "Longest"], properties.Select(property => property.Name));
        Assert.Equal(typeof(string), properties[0].PropertyType);
        Assert.Equal(typeof(WorkDuration), properties[1].PropertyType);
        Assert.Equal(typeof(bool), properties[2].PropertyType);
        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // The parts are init-only: they are said once, at the making or at the copy, and never turned after.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });

        // The contract every record compares by is the compiler's private one: no public part of the wait.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        Assert.Equal(
            ["Column", "Deconstruct", "Duration", "Equals", "GetHashCode", "Longest", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Equal(3, arguments.Length);
        Assert.Equal(["Column", "Duration", "Longest"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(string), typeof(WorkDuration), typeof(bool)], arguments.Select(argument => argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));

        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);

        // The two overloads of the equality every record is asked with, and the one Deconstruct.
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Column", "Duration", "Longest"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // And nothing of a moment or an address on it: a wait carries a name, three numbers and a mark, no clock.
        var other = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Where(parameter => parameter != type)
            .ToList();
        Assert.DoesNotContain(typeof(DateTimeOffset), other);
        Assert.DoesNotContain(typeof(DateTime), other);
        Assert.DoesNotContain(typeof(Uri), other);
        Assert.DoesNotContain(typeof(TimeSpan), other);
    }

    /// <summary>The public members the wait declares itself: its three properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ColumnWait)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a wait says its three parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own further up
    /// this file, so the strings here are the point and the culture only the way to say them once.</summary>
    private static string Words(ColumnWait wait) => ViewerCulture.Invariant(() => wait.ToString()!);
}
