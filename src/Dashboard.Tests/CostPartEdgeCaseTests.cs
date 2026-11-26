using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CostPart" /> is one word of a cost line and the days that word covers: <c>$1.52</c> and
/// <c>yesterday</c>. Two words and nothing else - no number, no moment, no address - and the last shape the page
/// takes a sum through: <see cref="CostText.Parts" /> hands over three of them, and <c>CostLine.razor</c> says each
/// with the amount in bold and the period beside it.
/// <para>
/// What is already pinned elsewhere is walked past here on purpose: <see cref="EdgeCaseTextTests" /> says that the
/// three sums always come back as three parts - the numbers given, the numbers not given as
/// <see cref="CostText.NoNumber" />, the zero and the file with no day - and that a line joins them and appends its
/// day, each of them a whole list of parts compared with a hand-built one, which is this record's equality doing
/// its work without ever being named; the same test reads one part's <c>Period</c> to hold the words <c>in 7 days</c>
/// to the page that owns them, and <see cref="CostTests" /> pins the three parts of the file the optics read. The
/// records the words are made from are pinned beside this: <see cref="CostAmountsEdgeCaseTests" />,
/// <see cref="CostEntryEdgeCaseTests" /> and <see cref="CostEstimateEdgeCaseTests" />.
/// </para>
/// <para>
/// What follows is the record of the two words itself: the constructor handing each argument to the property of its
/// name and holding the very string it was given, both words kept exactly as they are given - empty, in their
/// whitespace, in their case, of any length, in any letters - and refused by nothing, so that a word the file never
/// wrote stands as the nothing that is not the empty word; the two words coming back out in the order they went in;
/// a word turned as the part is made and the copy that turns one word while it keeps the other; the equality of the
/// typed record, of the boxed object and of <c>==</c> and <c>!=</c>, the hash that follows them, the null on either
/// side of each, and the two words being compared for their letters rather than for their places or their
/// references; the words the record says itself, which - the parts being letters and not numbers - no reader's
/// culture turns; and the public surface, so that the day a third word is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds two words, calls nothing at
/// all, and every value below is one the test brought. Where a part is taken from
/// <see cref="CostText.Parts" /> the moment is the test's own, fixed; the members that would need a clock of their
/// own are <see cref="CostText.Parts" /> and <see cref="CostText.Line" />, and those are pinned on the words they
/// say, not on the record they hand back.
/// </para>
/// </summary>
public class CostPartEdgeCaseTests
{
    /// <summary>The day the numbers stop at and the moment the page is drawn at, both of them brought here and neither
    /// of them read off a clock: <c>2026-10-03</c> is the day before the rendering, so the first part is
    /// <c>yesterday</c> and the last is <c>this month</c>.</summary>
    private static readonly DateOnly Day = new(2026, 10, 3);

    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>A part of the line as the page makes it: the money in one word, the days in the other - two words
    /// that are not alike, so that a word landing on the other's place is seen rather than quietly matching.</summary>
    private static CostPart Full => new("$1.52", "yesterday");

    // ---------------------------------- the two words, by the names of their own properties

    /// <summary>The constructor's two arguments, position by position, come back as the property of the same name:
    /// the amount first and the days it covers last - the order the page says them in, the amount in bold and the
    /// period beside it. Each word is held as the very string it was given, copied by nothing and turned by
    /// nothing.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var amount = Own("$1.52");
        var period = Own("yesterday");
        var part = new CostPart(amount, period);

        Assert.Same(amount, part.Amount);
        Assert.Same(period, part.Period);
        Assert.Equal("$1.52", part.Amount);
        Assert.Equal("yesterday", part.Period);
        Assert.NotSame(amount, period);

        // The other two parts of the same line: the seven days, and the month so far.
        var week = new CostPart("$9.80", "in 7 days");
        var month = new CostPart("1,204.80 EUR", "in October 2025");

        Assert.Equal("$9.80", week.Amount);
        Assert.Equal("in 7 days", week.Period);
        Assert.Equal("1,204.80 EUR", month.Amount);
        Assert.Equal("in October 2025", month.Period);
    }

    /// <summary>The words the page hands out stand in their own names: the amount carries the money as
    /// <see cref="CostText.Money" /> says it and the period carries the days as <see cref="CostText.Day" /> and
    /// <see cref="CostText.Month" /> name them - the sums in the first word and never in the second, a day Azure did
    /// not bill being <see cref="CostText.NoNumber" /> in the first while the second keeps its own place.</summary>
    [Fact]
    public void TheWordsThePageHandsOutStandInTheirOwnNames()
    {
        var parts = CostText.Parts(new CostAmounts(1.52, 9.8, 11.02), "USD", Day, Now);

        Assert.Equal(3, parts.Count);
        Assert.Equal("$1.52", parts[0].Amount);
        Assert.Equal("yesterday", parts[0].Period);
        Assert.Equal("$9.80", parts[1].Amount);
        Assert.Equal("in 7 days", parts[1].Period);
        Assert.Equal("$11.02", parts[2].Amount);
        Assert.Equal("this month", parts[2].Period);

        // The file that gave no number and named no day: the dash is the amount's word, the days are still named.
        var idle = CostText.Parts(new CostAmounts(null, null, null), null, null, Now);

        Assert.Equal(CostText.NoNumber, idle[0].Amount);
        Assert.Equal("in the last full day", idle[0].Period);
        Assert.Equal(CostText.NoNumber, idle[1].Amount);
        Assert.Equal("in 7 days", idle[1].Period);
        Assert.Equal(CostText.NoNumber, idle[2].Amount);
        Assert.Equal("this month", idle[2].Period);
    }

    /// <summary>Both words are kept in their exact letters: the empty word, the word that is only whitespace, the
    /// word with its whitespace around it, a word in another case, a word of the marks the page itself says its
    /// words with. Nothing here
    /// trims, nothing here changes a case, nothing here replaces an empty word with the dash - the trimming and the
    /// dashing are the page's words, pinned where those words are said, and the part says back what it was handed.</summary>
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " \t ")]
    [InlineData("  ", "yesterday")]
    [InlineData("$1.52", "")]
    [InlineData("—", "in 7 days")]
    [InlineData("·", "$0.00")]
    [InlineData("  $1.52  ", "  yesterday  ")]
    [InlineData("uAt", "Shared")]
    [InlineData("$1,204.00", "on 2026-10-03")]
    [InlineData("-$0.30", "in October 2025")]
    [InlineData("1.52 EUR", "this month")]
    public void EachWordIsKeptExactlyAsItIsGiven(string amount, string period)
    {
        var part = new CostPart(amount, period);

        Assert.Equal(amount, part.Amount);
        Assert.Equal(period, part.Period);
        Assert.Same(amount, part.Amount);
        Assert.Same(period, part.Period);
        Assert.Equal((amount, period), (part.Amount, part.Period));
    }

    /// <summary>A word of any length is held whole and said whole - a word one letter short is another part, told
    /// apart - and a word built at runtime from the same letters is the word it spells: the part keeps the reference
    /// it was given, and two parts are one when their letters are one.</summary>
    [Fact]
    public void AWordOfAnyLengthIsHeldWhole()
    {
        var longAmount = new string('$', 10_000);
        var longPeriod = new string('y', 5_000);
        var part = new CostPart(longAmount, longPeriod);

        Assert.Same(longAmount, part.Amount);
        Assert.Same(longPeriod, part.Period);
        Assert.Equal(10_000, part.Amount.Length);
        Assert.Equal(5_000, part.Period.Length);
        Assert.Equal($"CostPart {{ Amount = {longAmount}, Period = {longPeriod} }}", Words(part));

        // A word one letter shorter is another part, and it is said apart: the whole word is held, not a prefix of it.
        var shorter = new CostPart(new string('$', 9_999), longPeriod);

        Assert.NotEqual(part, shorter);
        Assert.NotEqual(Words(part), Words(shorter));

        // The same letters, gathered into other strings: another pair of references, and one part.
        var again = new CostPart(Own(longAmount), Own(longPeriod));

        Assert.NotSame(part.Amount, again.Amount);
        Assert.Equal(part, again);
        Assert.Equal(part.GetHashCode(), again.GetHashCode());
    }

    // ------------------------------------------------------ the word that is not given, and the empty one

    /// <summary>The record refuses nothing: a word that is not there at all stands as that, and it is not the empty
    /// word - which in turn is not the dash the page says for a sum Azure did not give. A part whose two words are
    /// nothing is still a part, and the reference that is no part at all is another nothing beside them.</summary>
    [Fact]
    public void AWordThatIsNotGivenIsKeptAsTheNothingItIs()
    {
        var noAmount = new CostPart(null!, "yesterday");

        Assert.NotNull(noAmount);
        Assert.Null(noAmount.Amount);
        Assert.Equal("yesterday", noAmount.Period);

        var noPeriod = new CostPart("$1.52", null!);

        Assert.Equal("$1.52", noPeriod.Amount);
        Assert.Null(noPeriod.Period);

        var neither = new CostPart(null!, null!);

        Assert.Null(neither.Amount);
        Assert.Null(neither.Period);
        Assert.Equal("CostPart { Amount = , Period =  }", Words(neither));

        // Nothing, the empty word and the dash are three different words to this record.
        Assert.NotEqual(neither, new CostPart("", ""));
        Assert.NotEqual(noAmount, new CostPart("", "yesterday"));
        Assert.NotEqual(noPeriod, new CostPart("$1.52", ""));
        Assert.NotEqual(new CostPart("", ""), new CostPart(CostText.NoNumber, CostText.NoNumber));
        Assert.NotEqual(neither, noAmount);
        Assert.True(neither != new CostPart("", ""));
        Assert.True(neither.Equals(new CostPart(null!, null!)));

        // And all three of them are a part, which is not the same as no part.
        Assert.Null(default(CostPart));
        Assert.NotNull(neither);
    }

    // --------------------------------------------------- the two words coming out again, and the copy

    /// <summary>The two words come back out in the order they went in - the amount, then the days - whether they are
    /// asked for at once or one after the other, and what came out goes back in the same way: the part the two words
    /// fall apart into is the part they were. The empty words, the whitespace and the nothing fall apart as they
    /// stand, turned to nothing by nobody.</summary>
    [Fact]
    public void ThePartFallsApartIntoItsTwoWordsInTheOrderTheyWentIn()
    {
        var (amount, period) = Full;

        Assert.Equal("$1.52", amount);
        Assert.Equal("yesterday", period);
        Assert.Equal(Full, new CostPart(amount, period));

        var first = Full;
        first.Deconstruct(out var money, out var days);

        Assert.Equal("$1.52", money);
        Assert.Equal("yesterday", days);
        Assert.Equal(amount, money);

        var (nothing, nowhere) = new CostPart(null!, null!);

        Assert.Null(nothing);
        Assert.Null(nowhere);
        Assert.Equal(new CostPart(null!, null!), new CostPart(nothing!, nowhere!));

        var (blank, space) = new CostPart("", "   ");

        Assert.Equal("", blank);
        Assert.Equal("   ", space);
        Assert.NotEqual(blank, space);
    }

    /// <summary>Either word may be turned as the part is made, and either may be turned in a copy: the copy that
    /// names nothing is the part it started from in another reference, the copy that names one word keeps the other
    /// exactly as it stood, the original is left where it stood, and a word turned to nothing is the nothing and not
    /// the empty word.</summary>
    [Fact]
    public void ACopyTurnsTheWordItNamesAndKeepsTheOther()
    {
        var original = Full;

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);

        var aged = original with { Period = "on 2026-10-03" };

        Assert.Equal("$1.52", aged.Amount);
        Assert.Equal("on 2026-10-03", aged.Period);
        Assert.Equal("yesterday", original.Period);
        Assert.NotSame(original, aged);
        Assert.NotEqual(original, aged);

        var notGiven = original with { Amount = CostText.NoNumber };

        Assert.Equal("—", notGiven.Amount);
        Assert.Equal("yesterday", notGiven.Period);
        Assert.Equal(new CostPart("—", "yesterday"), notGiven);

        var bothTurned = original with { Amount = "", Period = "" };

        Assert.Equal(new CostPart("", ""), bothTurned);
        Assert.NotEqual(original, bothTurned);

        var turnedToNothing = original with { Amount = null! };

        Assert.Null(turnedToNothing.Amount);
        Assert.Equal("yesterday", turnedToNothing.Period);
        Assert.NotEqual(turnedToNothing, original with { Amount = "" });

        // The words of a part are turned as the part is made too, and the part made that way is a part like any other.
        var made = new CostPart("—", "in 7 days") { Amount = "$9.80" };

        Assert.Equal(new CostPart("$9.80", "in 7 days"), made);
        Assert.NotSame(original, made);
    }

    // --------------------------------------------------------- the two words, the equality and the hash

    /// <summary>A part is one with another while its two words are one, whichever of the four ways of asking is
    /// used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash
    /// follows the equality, boxed or unboxed and from one call to the next, and the letters are compared rather
    /// than the references - a word gathered at runtime from the same letters is the word it spells, so parts that
    /// come from two readings of one file are one part and one entry of a set.</summary>
    [Fact]
    public void TwoPartsAreOneWhileTheirTwoWordsAre()
    {
        var one = Full;
        var two = new CostPart("$1.52", "yesterday");

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
        Assert.Single(new HashSet<CostPart> { one, two });

        var writtenAgain = new CostPart(Own("$1.52"), Own("yesterday"));

        Assert.NotSame(one.Amount, writtenAgain.Amount);
        Assert.NotSame(one.Period, writtenAgain.Period);
        Assert.Equal(one, writtenAgain);
        Assert.True(one == writtenAgain);
        Assert.Equal(one.GetHashCode(), writtenAgain.GetHashCode());
        Assert.Single(new HashSet<CostPart> { one, two, writtenAgain });

        // The parts the line is made of are parts like these: the page's own words match the same words written out.
        var fromThePage = CostText.Parts(new CostAmounts(1.52, 9.8, 11.02), "USD", Day, Now)[0];

        Assert.Equal(one, fromThePage);
        Assert.Equal(one.GetHashCode(), fromThePage.GetHashCode());
        Assert.Single(new HashSet<CostPart> { one, fromThePage });

        // Two parts whose words are both nothing are one part too, and so are two empty ones.
        Assert.Equal(new CostPart(null!, null!), new CostPart(null!, null!));
        Assert.Equal(new CostPart(null!, null!).GetHashCode(), new CostPart(null!, null!).GetHashCode());
        Assert.Equal(new CostPart("", ""), new CostPart(Own(""), ""));
    }

    /// <summary>A difference in either word is another part, and so is the same pair of words told in another order:
    /// a cent turned in the amount, another day named in the period, the two words exchanged between the properties,
    /// the whitespace added at either end and the case turned - the comparison is of the letters one after another,
    /// so neither a trimmed word nor a word of another case is the word it stands beside. A part that is nothing in
    /// a word is another part than the one whose word is empty.</summary>
    [Fact]
    public void AWordTurnedOrTurnedAboutIsAnotherPart()
    {
        var one = Full;

        Assert.NotEqual(one, new CostPart("$1.53", "yesterday"));
        Assert.NotEqual(one, new CostPart("$1.52", "today"));
        Assert.NotEqual(one, new CostPart("", "yesterday"));
        Assert.NotEqual(one, new CostPart(null!, "yesterday"));
        Assert.NotEqual(one, new CostPart("$1.52", ""));
        Assert.NotEqual(one, new CostPart("$1.52", null!));
        Assert.NotEqual(one, new CostPart(" $1.52", "yesterday"));
        Assert.NotEqual(one, new CostPart("$1.52 ", "yesterday"));
        Assert.NotEqual(one, new CostPart("$1.52", " yesterday"));
        Assert.NotEqual(one, new CostPart("$1.52", "Yesterday"));
        Assert.NotEqual(one, new CostPart("$1.52", "YESTERDAY"));
        Assert.NotEqual(one, new CostPart("yesterday", "$1.52"));
        Assert.NotEqual(one, new CostPart("—", "yesterday"));

        Assert.True(one != new CostPart("$1.52", "today"));
        Assert.False(one == new CostPart("$1.52", "today"));
        Assert.False(one.Equals((object)new CostPart("", "yesterday")));
        Assert.False(one.Equals(one with { Amount = null! }));
        Assert.False(one.Equals(one with { Period = "yesterdays" }));

        // The two words are two places: turning either of them alone is another part, and all four stand apart.
        Assert.Equal(
            4,
            new HashSet<CostPart>
            {
                one,
                one with { Amount = "$1.53" },
                one with { Period = "today" },
                one with { Period = "Today" },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two parts that are no part at
    /// all are one, a part and no part are two - the equality and the inequality each saying it, from either side -
    /// and the part asked about itself says no to the nothing, to the whole line it was joined into, to the sums it
    /// was made from, to a service, to an estimate and to the anonymous thing that happens to carry its two words.</summary>
    [Fact]
    public void NoPartIsOneWithNoPartAndNotWithAnyOtherThing()
    {
        CostPart? nothing = null;
        CostPart? alsoNothing = null;
        var there = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there!.Equals(nothing));
        Assert.True(there.Equals(there));
        Assert.Null(default(CostPart));

        Assert.False(Full.Equals(null));
        Assert.False(Full.Equals((CostPart?)null));
        Assert.False(Full.Equals((object?)null));
        Assert.False(Full.Equals("$1.52 yesterday"));
        Assert.False(Full.Equals(new { Amount = "$1.52", Period = "yesterday" }));
        Assert.False(Full.Equals(new CostAmounts(1.52, 9.8, 11.02)));
        Assert.False(Full.Equals(new CostService("SQL Database", 1.52)));
        Assert.False(Full.Equals(new CostEstimate(0.2048, new CostAmounts(1.17, 2.69, 2.69))));
    }

    // ------------------------------------------------------------- the words the record says itself

    /// <summary>The words the part says its two words in: its own name at the front, the amount first and the days
    /// last as they are named, each said as it stands and none of them quoted - and a word that is nothing, or empty,
    /// as the empty place between the mark and the comma. These are the record's words and not the page's: the page
    /// puts the amount in bold and hangs the days after it, and turns either word to nothing of its own.</summary>
    [Fact]
    public void TheRecordSaysItsTwoWordsInTheOrderTheyAreNamed()
    {
        Assert.Equal("CostPart { Amount = $1.52, Period = yesterday }", Words(Full));
        Assert.Equal("CostPart { Amount = —, Period = in 7 days }", Words(new CostPart("—", "in 7 days")));
        Assert.Equal("CostPart { Amount = 1,204.80 EUR, Period = in October 2025 }", Words(new CostPart("1,204.80 EUR", "in October 2025")));
        Assert.Equal("CostPart { Amount = -$0.30, Period = on 2026-10-03 }", Words(new CostPart("-$0.30", "on 2026-10-03")));
        Assert.Equal("CostPart { Amount = , Period =  }", Words(new CostPart("", "")));
        Assert.Equal("CostPart { Amount = $1.52, Period =  }", Words(new CostPart("$1.52", "")));
        Assert.Equal("CostPart { Amount =   , Period = yesterday }", Words(new CostPart("  ", "yesterday")));

        // The words are no measure of the part: two parts that differ by a letter's case are said apart, and the
        // empty word is not said as the dash is.
        Assert.NotEqual(Words(Full), Words(new CostPart("$1.52", "Yesterday")));
        Assert.NotEqual(Words(new CostPart("", "")), Words(new CostPart(CostText.NoNumber, "in 7 days")));
        Assert.DoesNotContain("USD", Words(Full), StringComparison.Ordinal);
        Assert.DoesNotContain("·", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The part's own words are letters, so no reader's culture turns them: a reader of German is told the
    /// same words as the invariant one - which is what sets this record apart from the sums beneath it, whose words
    /// follow whoever reads them. And the money a part carries is the page's <see cref="CostText.Money" />, written
    /// for the dashboard in the currency the file names and not for whoever looks either, so the part made of it
    /// under one reader is the part made of it under the other, equal and one hash.</summary>
    [Fact]
    public void ThePartsOwnWordsAreTheSameWhateverTheReaderSpeaks()
    {
        var part = new CostPart("1,204.80 EUR", "in October 2025");
        var sums = new CostAmounts(1204.8, 9.8, 11.02);

        Assert.Equal("CostPart { Amount = 1,204.80 EUR, Period = in October 2025 }", Words(part));
        Assert.Equal("CostPart { Amount = 1,204.80 EUR, Period = yesterday }", Words(CostText.Parts(sums, "EUR", Day, Now)[0]));

        ViewerCulture.German(() =>
        {
            Assert.Equal("CostPart { Amount = 1,204.80 EUR, Period = in October 2025 }", part.ToString());

            var german = CostText.Parts(sums, "EUR", Day, Now)[0];

            Assert.Equal("1,204.80 EUR", german.Amount);
            Assert.Equal("yesterday", german.Period);
            Assert.Equal("CostPart { Amount = 1,204.80 EUR, Period = yesterday }", german.ToString());
            Assert.Equal(part, new CostPart(german.Amount, part.Period));
            Assert.Equal(part.GetHashCode(), new CostPart(german.Amount, part.Period).GetHashCode());

            // The sums themselves are said the reader's way, which is why the part - the page's shape - is said the same.
            Assert.Equal(
                "CostAmounts { Yesterday = 1204,8, Last7Days = 9,8, MonthToDate = 11,02 }",
                sums.ToString());
        });

        Assert.Equal(Words(part), part.ToString());
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>The part's whole public surface, as it stands: the two words named in the order the constructor names
    /// them - the amount, then the days - and the members every record is asked, falling apart, comparing, hashing
    /// and saying itself, and nothing else to call. Both words are words and neither may be missing from a call. A
    /// third word added to the record has to be written here as well, which is the point: the page cannot be shown a
    /// word nobody decided what it means.</summary>
    [Fact]
    public void ThePartHasTwoWordsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Amount", "Period"],
            [.. typeof(CostPart)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Amount", "Deconstruct", "Equals", "GetHashCode", "Period", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(CostPart)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Amount"].PropertyType);
        Assert.Equal(typeof(string), properties["Period"].PropertyType);

        // Two words, and the constructor that takes them in that order.
        var constructor = typeof(CostPart).GetConstructors().Single();

        Assert.Equal(["Amount", "Period"], [.. constructor.GetParameters().Select(parameter => parameter.Name!)]);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the part declares itself: its properties and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the tests above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(CostPart)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a part says its two words in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the words themselves are letters and outlast any reader, which is pinned on its own
    /// further up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(CostPart part) => ViewerCulture.Invariant(() => part.ToString()!);

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the record does with a word it is given - keeping the one and not another that spells
    /// the same - is only seen when the two are apart.</summary>
    private static string Own(string word) => new(word.ToCharArray());
}
