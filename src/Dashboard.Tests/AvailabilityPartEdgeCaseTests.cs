using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="AvailabilityPart" /> is one part of the availability line: a word before, the number in the middle
/// that carries it, a word after, the tooltip the part gives itself and the moment it names. Five members and no
/// logic of its own - <see cref="AvailabilityText.Parts" /> settles the words and the part holds them, and
/// <c>AvailabilityLine.razor</c> says each with the number in bold, the tooltip on it and the moment inside a
/// <c>&lt;time&gt;</c> while it names one.
/// <para>
/// What is already pinned elsewhere is walked past here on purpose: <see cref="AvailabilityTests" /> pins the parts
/// of a day and a week with the last failure - the day's count and percent, the week's count and its percent
/// tooltip, the failure with its moment - as a whole list compared with hand-built parts, and pins the line's words,
/// the percents and the windows without a report; <see cref="DeliveryReportEdgeCaseTests" /> pins the week counted
/// alone and all healthy with its part of no failure and the last failure said with no window behind it;
/// <see cref="SharedCostAvailabilityTests" /> pins the same three parts through the shared card's line. The records
/// the part stands beside are pinned on their own: <see cref="HealthWindowEdgeCaseTests" />,
/// <see cref="HealthReportsEdgeCaseTests" />, <see cref="HourlyCheckEdgeCaseTests" /> and
/// <see cref="CostPartEdgeCaseTests" /> for the record with the same shape.
/// </para>
/// <para>
/// What follows is the record of the five members itself: the constructor handing each argument to the property of
/// its name and holding the very string it was given, the two optional members standing at their nothing by
/// default; each word kept exactly as it is given - empty, in its whitespace, in its case, of any length, in the
/// marks the page itself says its words with - and refused by nothing, so that a word the file never wrote stands as
/// the nothing that is not the empty word; the five members coming back out in the order they went in; a member
/// turned as the part is made and the copy that turns one member while it keeps the others; the equality of the
/// typed record, of the boxed object and of <c>==</c> and <c>!=</c>, the hash that follows them, and the moment
/// compared as the instant it is - the same instant spelled in another offset is the same part - while a second
/// away is another part; the words the record says itself, which say the four words in their own letters and the
/// moment in the reader's digits; and the public surface, so that the day a sixth member is added the list below
/// says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds five members, calls nothing
/// at all, and every value below is one the test brought. Where a part is taken from
/// <see cref="AvailabilityText.Parts" /> the moment it is asked is the test's own, fixed.
/// </para>
/// </summary>
public class AvailabilityPartEdgeCaseTests
{
    /// <summary>The moment the page is asked, and every "ago" below is counted from: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>A moment nine hours and seventeen minutes before it: the report the failure part names.</summary>
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>The part of the week as the page makes it: the count in the middle, its percent tooltip, and no
    /// moment of its own - the part <see cref="AvailabilityTests" /> pins as a value, held here as the part the
    /// tests turn.</summary>
    private static AvailabilityPart Week =>
        new(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days");

    /// <summary>The part of the last failure as the page makes it: no tooltip, and the moment the part names.</summary>
    private static AvailabilityPart Failure =>
        new("last failure ", "9 h ago", string.Empty, When: Failed);

    /// <summary>All five members named at once, two of them the words the page says and three of its own: the week's
    /// words with the failure's moment, so that a member landing on another's place is seen rather than quietly
    /// matching.</summary>
    private static AvailabilityPart Full =>
        new(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed);

    // ------------------------------------------------------- the five members, by the names of their own properties

    /// <summary>The constructor's five arguments, position by position, come back as the property of the same name:
    /// the word before, the number, the word after, the tooltip and the moment - the order the page says them in.
    /// Each word is held as the very string it was given, copied by nothing and turned by nothing, and the part that
    /// names no tooltip and no moment is still a part, with both of them at their nothing rather than at the empty
    /// word.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var before = Own("last failure ");
        var strong = Own("9 h ago");
        var after = Own(string.Empty);
        var title = Own("The report that failed");
        var part = new AvailabilityPart(before, strong, after, title, Failed);

        Assert.Same(before, part.Before);
        Assert.Same(strong, part.Strong);
        Assert.Same(after, part.After);
        Assert.Same(title, part.Title);
        Assert.Equal(Failed, part.When);
        Assert.Equal(Failed.Offset, part.When!.Value.Offset);

        // The two optional members default to their nothing: the page's three words make a part without naming them.
        var week = new AvailabilityPart(string.Empty, "164 of 166", " in 7 days");

        Assert.Equal(string.Empty, week.Before);
        Assert.Equal("164 of 166", week.Strong);
        Assert.Equal(" in 7 days", week.After);
        Assert.Null(week.Title);
        Assert.Null(week.When);

        // And the named form the page uses for the failure: the three words in their places and the moment named by
        // its member's name.
        var failure = Failure;

        Assert.Equal("last failure ", failure.Before);
        Assert.Equal("9 h ago", failure.Strong);
        Assert.Equal(string.Empty, failure.After);
        Assert.Null(failure.Title);
        Assert.Equal(Failed, failure.When);
    }

    /// <summary>Every word is kept in its exact letters: the empty word, the word that is only whitespace, the word
    /// with its whitespace around it, a word in another case, a word of digits, a word of the marks the page itself
    /// says its words with - the dash, the dot, the percent and a letter outside the ASCII. Nothing here trims,
    /// nothing here changes a case, nothing here replaces an empty word with another - the part says back what it
    /// was handed, and the tooltip, which may be empty where the page gives one, is kept as the words are.</summary>
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " \t ")]
    [InlineData("  ", "last failure ")]
    [InlineData("last failure ", "")]
    [InlineData("9 h ago", " in 7 days")]
    [InlineData("–", "·")]
    [InlineData("95.8 %", "164 of 166")]
    [InlineData("  9 h ago  ", "  in 7 days  ")]
    [InlineData("uAt", "Shared")]
    [InlineData("95,8 %", "in the last full day")]
    [InlineData("10000", "0")]
    [InlineData("·", "95.8 %")]
    public void EachWordIsKeptExactlyAsItIsGiven(string before, string after)
    {
        var title = Own("98.8 % of the hourly checks of the last 7 days");
        var part = new AvailabilityPart(before, "164 of 166", after, title, Failed);

        Assert.Equal(before, part.Before);
        Assert.Equal("164 of 166", part.Strong);
        Assert.Equal(after, part.After);
        Assert.Equal("98.8 % of the hourly checks of the last 7 days", part.Title);
        Assert.Same(before, part.Before);
        Assert.Same(after, part.After);
        Assert.Same(title, part.Title);
        Assert.Equal((before, after, title), (part.Before, part.After, part.Title));
    }

    /// <summary>A word of any length is held whole and said whole - a word one letter short is another part, told
    /// apart - and a word built at runtime from the same letters is the word it spells: the part keeps the reference
    /// it was given, and two parts are one when their letters are one.</summary>
    [Fact]
    public void AWordOfAnyLengthIsHeldWhole()
    {
        var longStrong = new string('1', 10_000);
        var longTitle = new string('y', 5_000);
        var part = new AvailabilityPart(string.Empty, longStrong, " in 7 days", longTitle);

        Assert.Same(longStrong, part.Strong);
        Assert.Same(longTitle, part.Title);
        Assert.Equal(10_000, part.Strong.Length);
        Assert.Equal(5_000, longTitle.Length);

        // A word one letter shorter is another part, and it is said apart: the whole word is held, not a prefix of it.
        var shorter = new AvailabilityPart(string.Empty, new string('1', 9_999), " in 7 days", longTitle);

        Assert.NotEqual(part, shorter);
        Assert.NotEqual(Words(part), Words(shorter));

        // The same letters, gathered into other strings: another pair of references, and one part.
        var again = new AvailabilityPart(Own(string.Empty), Own(longStrong), Own(" in 7 days"), Own(longTitle));

        Assert.NotSame(part.Strong, again.Strong);
        Assert.NotSame(part.Title, again.Title);
        Assert.Equal(part, again);
        Assert.Equal(part.GetHashCode(), again.GetHashCode());
    }

    /// <summary>The record refuses nothing: a word that is not there at all stands as that, and it is not the empty
    /// word - which in turn is not the word the page says for a part it does not have. A part whose five members are
    /// nothing is still a part, and the reference that is no part at all is another nothing beside them.</summary>
    [Fact]
    public void AWordThatIsNotGivenIsKeptAsTheNothingItIs()
    {
        var noBefore = new AvailabilityPart(null!, "9 h ago", string.Empty);

        Assert.NotNull(noBefore);
        Assert.Null(noBefore.Before);
        Assert.Equal("9 h ago", noBefore.Strong);

        var noStrong = new AvailabilityPart("last failure ", null!, string.Empty);

        Assert.Equal("last failure ", noStrong.Before);
        Assert.Null(noStrong.Strong);

        var noAfter = new AvailabilityPart("last failure ", "9 h ago", null!);

        Assert.Null(noAfter.After);

        var noTitle = new AvailabilityPart("last failure ", "9 h ago", string.Empty, null, Failed);

        Assert.Null(noTitle.Title);
        Assert.Equal(Failed, noTitle.When);

        var noWhen = new AvailabilityPart("last failure ", "9 h ago", string.Empty, "The report that failed", null);

        Assert.Equal("The report that failed", noWhen.Title);
        Assert.Null(noWhen.When);

        var neither = new AvailabilityPart(null!, null!, null!);

        Assert.Null(neither.Before);
        Assert.Null(neither.Strong);
        Assert.Null(neither.After);
        Assert.Null(neither.Title);
        Assert.Null(neither.When);
        Assert.Equal("AvailabilityPart { Before = , Strong = , After = , Title = , When =  }", Words(neither));

        // Nothing, the empty word and the page's words are three different parts to this record, in each of the five
        // places at once.
        Assert.NotEqual(neither, new AvailabilityPart(string.Empty, string.Empty, string.Empty, null, null));
        Assert.NotEqual(noBefore, new AvailabilityPart(string.Empty, "9 h ago", string.Empty));
        Assert.NotEqual(noTitle, new AvailabilityPart("last failure ", "9 h ago", string.Empty, string.Empty, Failed));
        Assert.NotEqual(noWhen, new AvailabilityPart("last failure ", "9 h ago", string.Empty, "The report that failed", Failed));
        Assert.NotEqual(neither, noBefore);
        Assert.True(neither != new AvailabilityPart(string.Empty, string.Empty, string.Empty, null, null));
        Assert.True(neither.Equals(new AvailabilityPart(null!, null!, null!)));

        // And all three of them are a part, which is not the same as no part.
        Assert.Null(default(AvailabilityPart));
        Assert.NotNull(neither);
    }

    // ----------------------------------------------- the five members coming out again, and the copy

    /// <summary>The five members come back out in the order they went in - the word before, the number, the word
    /// after, the tooltip and the moment - whether they are asked for at once or one after the other, and what came
    /// out goes back in the same way: the part the five members fall apart into is the part they were. The empty
    /// words and the nothing fall apart as they stand, turned to nothing by nobody.</summary>
    [Fact]
    public void ThePartFallsApartIntoItsFiveMembersInTheOrderTheyWentIn()
    {
        var (before, strong, after, title, when) = Full;

        Assert.Equal(string.Empty, before);
        Assert.Equal("164 of 166", strong);
        Assert.Equal(" in 7 days", after);
        Assert.Equal("98.8 % of the hourly checks of the last 7 days", title);
        Assert.Equal(Failed, when);
        Assert.Equal(Full, new AvailabilityPart(before, strong, after, title, when));

        Full.Deconstruct(out var wordBefore, out var wordStrong, out var wordAfter, out var wordTitle, out var wordWhen);

        Assert.Equal(before, wordBefore);
        Assert.Equal(strong, wordStrong);
        Assert.Equal(after, wordAfter);
        Assert.Equal(title, wordTitle);
        Assert.Equal(when, wordWhen);

        var (nothingBefore, nothingStrong, nothingAfter, noTitle, noWhen) = new AvailabilityPart(null!, null!, null!);

        Assert.Null(nothingBefore);
        Assert.Null(nothingStrong);
        Assert.Null(nothingAfter);
        Assert.Null(noTitle);
        Assert.Null(noWhen);
        Assert.Equal(new AvailabilityPart(null!, null!, null!), new AvailabilityPart(nothingBefore!, nothingStrong!, nothingAfter!));

        var blanks = new AvailabilityPart("", "   ", "  ");
        var (blank, middle, space, blankTitle, blankWhen) = blanks;

        Assert.Equal("", blank);
        Assert.Equal("   ", middle);
        Assert.Null(blankTitle);
        Assert.Null(blankWhen);
        Assert.Equal("  ", space);
        Assert.NotEqual(blank, space);
    }

    /// <summary>Either member may be turned as the part is made, and either may be turned in a copy: the copy that
    /// names nothing is the part it started from in another reference, the copy that names one member keeps the
    /// others exactly as they stood, the original is left where it stood, and a word turned to nothing is the
    /// nothing and not the empty word.</summary>
    [Fact]
    public void ACopyTurnsTheMemberItNamesAndKeepsTheOthers()
    {
        var original = Full;

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);

        var withNoTooltip = original with { Title = null };

        Assert.Equal(string.Empty, withNoTooltip.Before);
        Assert.Equal("164 of 166", withNoTooltip.Strong);
        Assert.Equal(" in 7 days", withNoTooltip.After);
        Assert.Null(withNoTooltip.Title);
        Assert.Equal(Failed, withNoTooltip.When);
        Assert.Equal(Failed, original.When);
        Assert.NotSame(original, withNoTooltip);
        Assert.NotEqual(original, withNoTooltip);

        var withNoMoment = original with { When = null };

        Assert.Equal("98.8 % of the hourly checks of the last 7 days", withNoMoment.Title);
        Assert.Null(withNoMoment.When);
        Assert.Equal(Week, withNoMoment);

        var momentTurned = original with { When = Now };

        Assert.Equal(Now, momentTurned.When);
        Assert.Equal(Failed, original.When);
        Assert.NotEqual(original, momentTurned);

        var wordTurnedToNothing = original with { Before = null! };

        Assert.Null(wordTurnedToNothing.Before);
        Assert.Equal("164 of 166", wordTurnedToNothing.Strong);
        Assert.NotEqual(wordTurnedToNothing, original with { Before = string.Empty });

        // The members are turned as the part is made too, and the part made that way is a part like any other.
        var made = new AvailabilityPart(string.Empty, "164 of 166", " in 7 days") { After = " in 24 hours" };

        Assert.Equal(new AvailabilityPart(string.Empty, "164 of 166", " in 24 hours"), made);
        Assert.NotSame(original, made);
    }

    // ------------------------------------------------------------ the equality and the hash

    /// <summary>A part is one with another while its five members are one, whichever of the four ways of asking is
    /// used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash
    /// follows the equality, boxed or unboxed and from one call to the next, and the letters are compared rather
    /// than the references - so the parts the page makes of one file are the parts the test writes out of the same
    /// words, one pair and one entry of a set.</summary>
    [Fact]
    public void TwoPartsAreOneWhileTheirFiveMembersAre()
    {
        var one = Full;
        var two = new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<AvailabilityPart> { one, two });

        var writtenAgain = new AvailabilityPart(string.Empty, Own("164 of 166"), Own(" in 7 days"), Own("98.8 % of the hourly checks of the last 7 days"), Failed);

        Assert.NotSame(one.Strong, writtenAgain.Strong);
        Assert.NotSame(one.Title, writtenAgain.Title);

        // The empty word gathered character by character is the one empty word there is: the record says back the
        // reference it was given, which for the empty word is the very reference the other part holds.
        Assert.Same(one.Before, writtenAgain.Before);
        Assert.Equal(one, writtenAgain);
        Assert.True(one == writtenAgain);
        Assert.Equal(one.GetHashCode(), writtenAgain.GetHashCode());
        Assert.Single(new HashSet<AvailabilityPart> { one, two, writtenAgain });

        // The parts the page makes of one set of reports are parts like these: the same words the tests write out,
        // and the failure's moment as the page carries it.
        var parts = AvailabilityText.Parts(new HealthReports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed), Now);

        Assert.Equal(3, parts.Count);
        Assert.Equal(Week, parts[1]);
        Assert.Equal(Failure, parts[2]);
        Assert.Equal(Week.GetHashCode(), parts[1].GetHashCode());
        Assert.Equal(Failure.GetHashCode(), parts[2].GetHashCode());
        Assert.Equal(2, new HashSet<AvailabilityPart> { Week, Failure, parts[1], parts[2] }.Count);

        // Two parts whose five members are all nothing are one part too, and so are two empty ones.
        Assert.Equal(new AvailabilityPart(null!, null!, null!), new AvailabilityPart(null!, null!, null!));
        Assert.Equal(new AvailabilityPart(null!, null!, null!).GetHashCode(), new AvailabilityPart(null!, null!, null!).GetHashCode());
        Assert.Equal(new AvailabilityPart("", "", "", null, null), new AvailabilityPart(Own(""), Own(""), Own(""), null, null));
    }

    /// <summary>A difference in any member is another part: a letter turned in the number, the tooltip added where
    /// the part had none and dropped where it had one, the moment added or dropped, the two words exchanged between
    /// the properties, the whitespace added at either end and the case turned - the comparison is of the letters one
    /// after another, so neither a trimmed word nor a word of another case is the word it stands beside. A part that
    /// is nothing in a word is another part than the one whose word is empty.</summary>
    [Fact]
    public void AMemberTurnedOrTurnedAboutIsAnotherPart()
    {
        var one = Full;

        Assert.NotEqual(one, new AvailabilityPart("last failure ", "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "165 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 day", Failed));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days"));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed.AddSeconds(1)));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed.AddSeconds(-1)));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", null, Failed));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days ", Failed));
        Assert.NotEqual(one, new AvailabilityPart(" ", "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, " 164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days ", "98.8 % of the hourly checks of the last 7 days", Failed));
        Assert.NotEqual(one, new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the LAST 7 days", Failed));
        Assert.NotEqual(one, new AvailabilityPart("98.8 % of the hourly checks of the last 7 days", "164 of 166", " in 7 days", string.Empty, Failed));
        Assert.NotEqual(one, new AvailabilityPart(null!, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed));

        Assert.True(one != new AvailabilityPart(string.Empty, "165 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed));
        Assert.False(one == new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", null, Failed));
        Assert.False(one.Equals((object)new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed.AddMinutes(1))));
        Assert.False(one.Equals(one with { Title = null }));
        Assert.False(one.Equals(one with { When = null }));

        // The five members are five places: turning any one of them alone is another part, and all six stand apart.
        Assert.Equal(
            6,
            new HashSet<AvailabilityPart>
            {
                one,
                one with { Before = "last failure " },
                one with { Strong = "165 of 166" },
                one with { After = " in 24 hours" },
                one with { Title = null },
                one with { When = Now },
            }.Count);
    }

    /// <summary>
    /// The moment is compared as the instant it is, and so is it hashed: the same moment spelled in the hours of
    /// another offset is the same <c>When</c>, the same part and the same entry of a set - the part says the moment
    /// wherever the viewer stands, and the equality does not care which of the viewer's hours it was written in. A
    /// different instant is another part, in whatever hours it is spelled.
    /// </summary>
    [Fact]
    public void TheSameInstantIsTheSameMomentHoweverItIsSpelled()
    {
        var utc = new DateTimeOffset(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);
        var eastTwo = utc.ToOffset(TimeSpan.FromHours(2));

        var inUtc = new AvailabilityPart("last failure ", "9 h ago", string.Empty, When: utc);
        var inEastTwo = new AvailabilityPart("last failure ", "9 h ago", string.Empty, When: eastTwo);

        Assert.Equal(utc, inEastTwo.When);
        Assert.Equal(TimeSpan.FromHours(2), inEastTwo.When!.Value.Offset);
        Assert.Equal(inUtc, inEastTwo);
        Assert.True(inUtc == inEastTwo);
        Assert.Equal(inUtc.GetHashCode(), inEastTwo.GetHashCode());
        Assert.Single(new HashSet<AvailabilityPart> { inUtc, inEastTwo });

        // A second away is another moment, and a different instant is another part in whatever hours it is spelled.
        var aSecondOn = new AvailabilityPart("last failure ", "9 h ago", string.Empty, When: utc.AddSeconds(1).ToOffset(TimeSpan.FromHours(2)));

        Assert.NotEqual(inUtc, aSecondOn);
        Assert.True(inUtc != aSecondOn);
        Assert.NotEqual(inUtc, new AvailabilityPart("last failure ", "9 h ago", string.Empty, When: utc.AddMinutes(-1)));

        // The ends of the moment line are moments the part holds and says: the first and the last, each in its own
        // offset, apart from one another and from a part that names no moment at all.
        var first = new AvailabilityPart(string.Empty, string.Empty, string.Empty, When: DateTimeOffset.MinValue);
        var last = new AvailabilityPart(string.Empty, string.Empty, string.Empty, When: DateTimeOffset.MaxValue);

        Assert.Equal(DateTimeOffset.MinValue, first.When);
        Assert.Equal(DateTimeOffset.MaxValue, last.When);
        Assert.NotEqual(first, last);
        Assert.NotEqual(first, new AvailabilityPart(string.Empty, string.Empty, string.Empty));
        Assert.Equal("AvailabilityPart { Before = , Strong = , After = , Title = , When = 01/01/0001 00:00:00 +00:00 }", Words(first));
        Assert.Equal("AvailabilityPart { Before = , Strong = , After = , Title = , When = 12/31/9999 23:59:59 +00:00 }", Words(last));
    }

    /// <summary>The nothing on either side of the members every record is asked with: two parts that are no part at
    /// all are one, a part and no part are two - the equality and the inequality each saying it, from either side -
    /// and the part asked about itself says no to the nothing, to the words it was made of, to the window it counts
    /// and to the anonymous thing that happens to carry its five members.</summary>
    [Fact]
    public void NoPartIsOneWithNoPartAndNotWithAnyOtherThing()
    {
        AvailabilityPart? nothing = null;
        AvailabilityPart? alsoNothing = null;
        var there = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there!.Equals(nothing));
        Assert.True(there.Equals(there));
        Assert.Null(default(AvailabilityPart));

        Assert.False(Full.Equals(null));
        Assert.False(Full.Equals((AvailabilityPart?)null));
        Assert.False(Full.Equals((object?)null));
        Assert.False(Full.Equals("164 of 166 in 7 days"));
        Assert.False(Full.Equals(new { Before = string.Empty, Strong = "164 of 166", After = " in 7 days", Title = "98.8 % of the hourly checks of the last 7 days", When = (DateTimeOffset?)Failed }));
        Assert.False(Full.Equals(new HealthWindow(166, 164)));
        Assert.False(Full.Equals(new CostPart("$1.52", "yesterday")));
        Assert.False(Full.Equals(Week));
        Assert.False(Full.Equals(Failure));
    }

    // ------------------------------------------------------------ the words the record says itself

    /// <summary>The words the part says its five members in: its own name at the front, the word before, the number,
    /// the word after, the tooltip and the moment as they are named, each said as it stands and none of them
    /// quoted - and a member that is nothing, or empty, as the empty place between the mark and the comma. These are
    /// the record's words and not the page's: the page puts the number in bold, hangs the word after it and keeps
    /// the tooltip and the moment for their own elements.</summary>
    [Fact]
    public void TheRecordSaysItsFiveMembersInTheOrderTheyAreNamed()
    {
        Assert.Equal(
            "AvailabilityPart { Before = , Strong = 164 of 166, After =  in 7 days, Title = 98.8 % of the hourly checks of the last 7 days, When = 10/06/2026 16:13:00 +00:00 }",
            Words(Full));
        Assert.Equal(
            "AvailabilityPart { Before = , Strong = 164 of 166, After =  in 7 days, Title = 98.8 % of the hourly checks of the last 7 days, When =  }",
            Words(Week));
        Assert.Equal(
            "AvailabilityPart { Before = last failure , Strong = 9 h ago, After = , Title = , When = 10/06/2026 16:13:00 +00:00 }",
            Words(Failure));
        Assert.Equal("AvailabilityPart { Before = , Strong = , After = , Title = , When =  }", Words(new AvailabilityPart(null!, null!, null!)));
        Assert.Equal("AvailabilityPart { Before = , Strong = , After = , Title = , When =  }", Words(new AvailabilityPart(string.Empty, string.Empty, string.Empty, null, null)));
        Assert.Equal(
            "AvailabilityPart { Before =   , Strong = 9 h ago, After = , Title = , When =  }",
            Words(new AvailabilityPart("  ", "9 h ago", string.Empty)));

        // The words are no measure of the part: two parts that differ by a letter's case are said apart, and the
        // empty word is not said as the moment is.
        Assert.NotEqual(Words(Full), Words(new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 DAYS", Failed)));
        Assert.NotEqual(Words(new AvailabilityPart(string.Empty, string.Empty, string.Empty)), Words(new AvailabilityPart(string.Empty, string.Empty, string.Empty, When: Failed)));
        Assert.DoesNotContain("·", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>
    /// The four words are letters, so no reader's culture turns them - and neither does it turn the equality, which
    /// is of the words and of the instant. The moment is the one member the reader's culture writes, and it writes
    /// it in its own digits: the same part a reader of German says in the German day, equal to the part the invariant
    /// reader says and one hash with it.
    /// </summary>
    [Fact]
    public void TheWordsAreTheSameWhateverTheReaderSpeaksAndSoIsTheEquality()
    {
        var german = new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days", Failed);

        Assert.Equal(
            "AvailabilityPart { Before = , Strong = 164 of 166, After =  in 7 days, Title = 98.8 % of the hourly checks of the last 7 days, When = 10/06/2026 16:13:00 +00:00 }",
            Words(Full));

        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "AvailabilityPart { Before = , Strong = 164 of 166, After =  in 7 days, Title = 98.8 % of the hourly checks of the last 7 days, When = 06.10.2026 16:13:00 +00:00 }",
                Full.ToString());
            Assert.Equal(
                "AvailabilityPart { Before = , Strong = 164 of 166, After =  in 7 days, Title = 98.8 % of the hourly checks of the last 7 days, When =  }",
                Week.ToString());

            // The words and the instant are nothing for the reader to turn: the part made under the German reader
            // is the part made under any reader.
            Assert.Equal(Full, german);
            Assert.True(Full == german);
            Assert.Equal(Full.GetHashCode(), german.GetHashCode());
        });

        Assert.Equal(Words(Full), Words(german));
    }

    // --------------------------------------------------- the parts the page makes, as the parts themselves

    /// <summary>
    /// The parts of the reports that counted nothing, read off the record rather than off the line: a window that
    /// ended no check is said with its own words and an empty word where the count would stand, the week alone says
    /// its seven days and the day and week together leave the day first - the line's words are pinned where they
    /// are said, and this pins the parts the line is made of.
    /// </summary>
    [Fact]
    public void TheWindowThatCountedNothingIsSaidAsParts()
    {
        Assert.Equal(
            [
                new AvailabilityPart("No hourly check ended in ", "24 hours", string.Empty),
                new AvailabilityPart(string.Empty, "none", " in 7 days"),
            ],
            AvailabilityText.Parts(new HealthReports(new HealthWindow(0, 0), new HealthWindow(0, 0), null), Now));

        Assert.Equal(
            [new AvailabilityPart("No hourly check ended in ", "24 hours", string.Empty)],
            AvailabilityText.Parts(new HealthReports(new HealthWindow(0, 0), null, null), Now));

        Assert.Equal(
            [new AvailabilityPart("No hourly check ended in ", "7 days", string.Empty)],
            AvailabilityText.Parts(new HealthReports(null, new HealthWindow(0, 0), null), Now));
    }

    // -------------------------------------------------------------------- the record's own shape

    /// <summary>The part's whole public surface, as it stands: the five members named in the order the constructor
    /// names them - the word before, the number, the word after, the tooltip and the moment - and the members every
    /// record is asked, falling apart, comparing, hashing and saying itself, and nothing else to call. The two
    /// optional members are optional at the call, and a sixth member added to the record has to be written here as
    /// well, which is the point: the page cannot be shown a member nobody decided what it means.</summary>
    [Fact]
    public void ThePartHasFiveMembersAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Before", "Strong", "After", "Title", "When"],
            [.. typeof(AvailabilityPart)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["After", "Before", "Deconstruct", "Equals", "GetHashCode", "Strong", "Title", "ToString", "When", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(AvailabilityPart)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Before"].PropertyType);
        Assert.Equal(typeof(string), properties["Strong"].PropertyType);
        Assert.Equal(typeof(string), properties["After"].PropertyType);
        Assert.Equal(typeof(string), properties["Title"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["When"].PropertyType);
        Assert.All(properties.Values, property => Assert.True(property.CanWrite));

        // Five members, and the constructor that takes them in that order: the first three given at the call and
        // the last two of them optional.
        var constructor = typeof(AvailabilityPart).GetConstructors().Single();
        var parameters = constructor.GetParameters();

        Assert.Equal(["Before", "Strong", "After", "Title", "When"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(string), parameters[1].ParameterType);
        Assert.Equal(typeof(string), parameters[2].ParameterType);
        Assert.Equal(typeof(string), parameters[3].ParameterType);
        Assert.Equal(typeof(DateTimeOffset?), parameters[4].ParameterType);
        Assert.False(parameters[0].IsOptional);
        Assert.False(parameters[1].IsOptional);
        Assert.False(parameters[2].IsOptional);
        Assert.True(parameters[3].IsOptional);
        Assert.True(parameters[4].IsOptional);

        // The five members come out as the five members they went in, in their own types: the out parameters of the
        // deconstruction are the members' types themselves, read off the reference they hand back.
        var deconstruct = typeof(AvailabilityPart).GetMethod("Deconstruct")!;

        Assert.True(deconstruct.IsPublic);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(string), typeof(DateTimeOffset?)],
            [.. deconstruct.GetParameters()
                .Select(parameter => parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType)]);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the part declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the tests above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(AvailabilityPart)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a part says its five members in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the four words are letters and outlast any reader, and the moment is said in the
    /// invariant digits here so that the string is the point and the culture only the way to say it once.</summary>
    private static string Words(AvailabilityPart part) => ViewerCulture.Invariant(() => part.ToString()!);

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the record does with a word it is given - keeping the one and not another that spells
    /// the same - is only seen when the two are apart.</summary>
    private static string Own(string word) => new(word.ToCharArray());
}
