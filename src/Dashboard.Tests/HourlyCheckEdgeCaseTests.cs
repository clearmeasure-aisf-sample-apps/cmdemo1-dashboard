using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="HourlyCheck" />, what one hourly health report of
/// the pipeline found for one container app: the word it found and the moment that report ended, two parts and no
/// rule of its own. The page shows both only through <see cref="HourlyCheckText" /> - the box's line
/// <c>hourly: healthy, 40 min ago</c> and the tooltip under it - and the record is what the box echoes.
/// <para>
/// Which readings make a check at all is already pinned where the file is read: <see cref="AvailabilityTests" />
/// reads a <c>last</c> of <c>delivery.json</c> and says that the word <c>healthy</c> and the word
/// <c>Unreachable</c> become the checks they do, that a check without its moment is no check, that a moment of
/// <c>an hour ago</c> is no moment, that a <c>last</c> with no word is no check and that a <c>last</c> that is a
/// word rather than an object is no check; <see cref="RuntimePayloadBuilderTests" /> reads all three words as the
/// page writes them. Which words the box then says, how old a check may be and still be said (the two hours of
/// <see cref="HourlyCheckText.Recent" />, at the edge and past it) and what the tooltip makes of a check are
/// pinned there and in <see cref="HealthHelperCoverageTests" /> and <see cref="AvailabilityTests" /> too, so none
/// of that is said again here.
/// </para>
/// <para>
/// What follows is the record and the boundaries the reading left open. First the three words as the fixed values
/// they are - the page compares them, the box says them, and nothing but this file says them. Then the two parts
/// as the record holds them: the constructor's arguments landing on the property of the same name, nothing lowered
/// and nothing trimmed and nothing refused - the reader owns all three of those - and the moment held to the tick
/// at the ends of its own number. Then the reading, which is internal to an app that declares no
/// <c>InternalsVisibleTo</c> (as <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>) and is therefore pinned
/// through <see cref="DeliveryReport.Parse" /> here as <see cref="AksServiceEdgeCaseTests" /> pins
/// <c>AksPool.Read</c> through <see cref="AksService.Parse" />: every one of the three words in the spellings a
/// runbook's log may give it, the words that are beside the three rather than among them, a result and a moment of
/// another kind than the file says, the same instant written at another offset, the fraction of a second the file
/// may write and the box never says, and the words the entry carries besides its own. Then the members every
/// assertion above leans on without ever saying a word about them: that two checks are one when the word and the
/// instant are, with the hash that goes with it, and that either part alone tells them apart - the empty word and
/// no word at all among them; the copy that changes the part it names and nothing else; the two parts coming back
/// out in the order they went in; and the line the record says about itself.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test wrote into the
/// file or brought itself, and a moment written without an offset is read as the universal one, so the time zone
/// of the machine that runs this never enters. The one member that does look at the clock is
/// <see cref="HourlyCheckText.RecentResult" />, and it is pinned elsewhere with moments the test brought.
/// </para>
/// </summary>
public class HourlyCheckEdgeCaseTests
{
    /// <summary>The moment every file below writes, and the <see cref="HourlyCheck.At" /> it is read as.</summary>
    private const string Written = "2026-10-07T01:02:13Z";

    private static readonly DateTimeOffset Reported = new(2026, 10, 7, 1, 2, 13, TimeSpan.Zero);

    /// <summary>The very instant of <see cref="Reported" />, written five hours west of the meridian.</summary>
    private static readonly DateTimeOffset SameInstantWest = new(2026, 10, 6, 20, 2, 13, TimeSpan.FromHours(-5));

    /// <summary>
    /// The <c>last</c> of the app of a file written this way: the reading the page does, with nothing of the app
    /// held back that could change what it makes of its check.
    /// </summary>
    private static HourlyCheck? Last(string last) =>
        DeliveryReport.Parse($$"""
            { "environments": [ { "name": "tdd", "deployables": [],
                "health": { "last24Hours": { "reports": 24, "healthy": 23 }, "last7Days": { "reports": 168, "healthy": 166 }, "lastFailure": null,
                            "containerApps": [ { "name": "ui", "last": {{last}} } ] } } ] }
            """)!.ContainerAppsOf("tdd")![0].Last;

    /// <summary>A <c>last</c> whose result stands in the file as written here, at the one moment of the line above.</summary>
    private static HourlyCheck? Result(string result) => Last($$"""{ "result": {{result}}, "at": "{{Written}}" }""");

    /// <summary>A <c>last</c> whose moment stands in the file as written here, with the word <c>healthy</c>.</summary>
    private static HourlyCheck? Moment(string at) => Last($$"""{ "result": "healthy", "at": {{at}} }""");

    /// <summary>A check of the one moment, under whichever word the test names - so that a comparison of two
    /// checks is a comparison of their words and not of the moment they share.</summary>
    private static HourlyCheck ReportedAs(HourlyCheckResult result) => new(result, Reported);

    // ------------------------------------------------------------------ the three words

    /// <summary>
    /// The three words the report knows, as the characters they are: the box says the word it holds, the reading
    /// compares what it read against these three, and the page's own words (<see cref="HourlyCheckText.Result" />,
    /// <see cref="HourlyCheckText.Title" />) carry whichever one it found - so a change to one of these three is a
    /// change of the contract between the runbook's log and the page, and nothing but this test says the three
    /// characters themselves.
    /// </summary>
    [Fact]
    public void TheThreeWordsAreTheseThreeWords()
    {
        Assert.Equal("healthy", HourlyCheckResult.Healthy.Word);
        Assert.Equal("unhealthy", HourlyCheckResult.Unhealthy.Word);
        Assert.Equal("unreachable", HourlyCheckResult.Unreachable.Word);
    }

    /// <summary>The three words are three, each already in its own lower case, trimmed and of no spaces: the
    /// reading lowers and trims what the file wrote before it compares the whole word against these three, so a
    /// word that stood for two of them would make one report say two things.</summary>
    [Fact]
    public void TheThreeWordsDifferFromOneAnotherAndSayThemselves()
    {
        var words = new[] { HourlyCheckResult.Healthy.Word, HourlyCheckResult.Unhealthy.Word, HourlyCheckResult.Unreachable.Word };

        Assert.Equal(3, words.Distinct(StringComparer.Ordinal).Count());
        Assert.All(words, word =>
        {
            Assert.Equal(word, word.Trim());
            Assert.Equal(word, word.ToLowerInvariant());
            Assert.DoesNotContain(" ", word, StringComparison.Ordinal);
        });
    }

    // ------------------------------------------------------------------ the two parts as the record holds them

    /// <summary>The constructor's two arguments, each come back as the property of the same name: the word the
    /// report found and the moment it ended, the latter with the offset it was given rather than the one the page
    /// shows it in.</summary>
    [Fact]
    public void TheCheckCarriesTheWordAndTheMomentItWasGiven()
    {
        var found = new HourlyCheck(HourlyCheckResult.Unreachable, Reported);
        var western = new HourlyCheck(HourlyCheckResult.Healthy, SameInstantWest);

        Assert.Equal(HourlyCheckResult.Unreachable, found.Result);
        Assert.Equal(Reported, found.At);
        Assert.Equal(TimeSpan.Zero, found.At.Offset);

        Assert.Equal(HourlyCheckResult.Healthy, western.Result);
        Assert.Equal(SameInstantWest, western.At);
        Assert.Equal(TimeSpan.FromHours(-5), western.At.Offset);
        Assert.Equal(Reported.UtcDateTime, western.At.UtcDateTime);
    }

    /// <summary>
    /// Retired deliberately with the closed vocabulary of issue #897: the record used to lower nothing, trim
    /// nothing and refuse nothing - the word it was handed was the word it said - and this test pinned that. A
    /// <see cref="HourlyCheckResult" /> is one of the three words the reading makes and nothing else, so the word a
    /// file writes is said as the reading says it: <see cref="HourlyCheckResult.Read" /> turns the other case and
    /// the padding into the word they are, and a word beside the three - the empty word, the word of spaces and a
    /// whole phrase - into no word at all.
    /// </summary>
    [Theory]
    [InlineData("HEALTHY", true)]
    [InlineData("Healthy", true)]
    [InlineData(" healthy ", true)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("healthy and quick", false)]
    [InlineData("hourly: healthy", false)]
    public void TheWordIsSaidAsTheReadingSaysItAndBesideTheThreeIsNoWord(string result, bool oneOfTheThree)
    {
        var word = HourlyCheckResult.Read(result);

        Assert.Equal(oneOfTheThree, word is not null);
        Assert.Equal(oneOfTheThree ? HourlyCheckResult.Healthy : null, word);
    }

    /// <summary>
    /// Retired deliberately with the closed vocabulary of issue #897: the record used to keep a word of
    /// <see langword="null" /> and say it as nothing. No <see cref="HourlyCheckResult" /> is no word at all - the
    /// reading refuses no word the same way it refuses the empty word - and a check always holds a word: the
    /// result it holds is one of the three, never nothing.
    /// </summary>
    [Fact]
    public void NoWordIsNoWordAndACheckAlwaysHoldsOne()
    {
        Assert.Null(HourlyCheckResult.Read(null));
        Assert.Null(HourlyCheckResult.Read(""));
        Assert.NotNull(new HourlyCheck(HourlyCheckResult.Healthy, Reported).Result);
    }

    /// <summary>The moment is held to the tick, at the ends of its own number and beyond the second the box says:
    /// the first and the last moment there are, a tick past the report's second and a tick before it, each standing
    /// as it was given - the record compares the tick, though it prints the second.</summary>
    [Fact]
    public void TheMomentIsHeldToTheTickAtTheEndsOfItsNumber()
    {
        Assert.Equal(DateTimeOffset.MinValue, new HourlyCheck(HourlyCheckResult.Healthy, DateTimeOffset.MinValue).At);
        Assert.Equal(DateTimeOffset.MaxValue, new HourlyCheck(HourlyCheckResult.Healthy, DateTimeOffset.MaxValue).At);

        var tickAfter = Reported.AddTicks(1);
        var tickBefore = Reported.AddTicks(-1);

        Assert.Equal(tickAfter, new HourlyCheck(HourlyCheckResult.Healthy, tickAfter).At);
        Assert.NotEqual(ReportedAs(HourlyCheckResult.Healthy), new HourlyCheck(HourlyCheckResult.Healthy, tickAfter));
        Assert.NotEqual(ReportedAs(HourlyCheckResult.Healthy), new HourlyCheck(HourlyCheckResult.Healthy, tickBefore));
        Assert.NotEqual(new HourlyCheck(HourlyCheckResult.Healthy, tickAfter), new HourlyCheck(HourlyCheckResult.Healthy, tickBefore));
    }

    // ------------------------------------------------------------------ the reading, through the file

    /// <summary>
    /// Every one of the three words the reading accepts, in the spellings a log may give it: itself, all in caps,
    /// padded with spaces and with the escapes a JSON string carries, and with a word of its own to keep - each
    /// coming back as the one word the check holds. The two words <see cref="AvailabilityTests" /> reads from the
    /// file are <c>healthy</c> and <c>Unreachable</c>; what is pinned here are the other spellings, the third word
    /// and the trimming.
    /// </summary>
    [Theory]
    [InlineData(""" "healthy" """, "healthy")]
    [InlineData(""" "HEALTHY" """, "healthy")]
    [InlineData(""" " Healthy " """, "healthy")]
    [InlineData(""" "unhealthy" """, "unhealthy")]
    [InlineData(""" "UNHEALTHY" """, "unhealthy")]
    [InlineData(""" "  Unreachable  " """, "unreachable")]
    [InlineData(""" "unreachable\n" """, "unreachable")]
    [InlineData(""" "\tunreachable\t" """, "unreachable")]
    public void TheWordTheFileWritesComesBackAsOneOfTheThree(string result, string word)
    {
        var found = Result(result);

        Assert.NotNull(found);
        Assert.Equal(word, found!.Result.Word);
        Assert.Equal(Reported, found.At);
    }

    /// <summary>
    /// A word beside the three is no check at all, whichever way it is beside them: the empty word and a word of
    /// spaces, which the reading turns into nothing before it compares, the shorter word and the longer one and the
    /// word with the rest of the sentence after it, and a result of another kind than text - a number, a flag, no
    /// value, an array, an object. The unknown word of the sample and the missing word are
    /// <see cref="AvailabilityTests" />; what is pinned here is that the comparison is the whole word, and that a
    /// blank word is as much no check as no word.
    /// </summary>
    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"  \"")]
    [InlineData("\"health\"")]
    [InlineData("\"healthyy\"")]
    [InlineData("\"unreachabl\"")]
    [InlineData("\"unreachable!\"")]
    [InlineData("\"healthy and quick\"")]
    [InlineData("\"dead\"")]
    [InlineData("\"ok\"")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("""["healthy"]""")]
    [InlineData("""{ "word": "healthy" }""")]
    public void AWordBesideTheThreeIsNoCheck(string result) =>
        Assert.Null(Result(result));

    /// <summary>The same instant, whichever offset writes it: the file's <c>at</c> may stand on the meridian, two
    /// hours east or five and fourteen hours west of it, and the check the page leans on is one check - the same
    /// instant to the tick, with the offset the file wrote held rather than the one the viewer reads it in. A
    /// moment with no offset at all is read as the universal one, so no machine's zone decides what the check
    /// stands on.</summary>
    [Theory]
    [InlineData("2026-10-07T01:02:13Z", 0)]
    [InlineData("2026-10-07T01:02:13+00:00", 0)]
    [InlineData("2026-10-07T01:02:13", 0)]
    [InlineData("2026-10-07T03:02:13+02:00", 2)]
    [InlineData("2026-10-06T20:02:13-05:00", -5)]
    [InlineData("2026-10-06T11:02:13-14:00", -14)]
    [InlineData("2026-10-07T02:02:13+01:00", 1)]
    public void TheSameInstantWrittenApartIsTheSameCheck(string at, int offset)
    {
        var found = Moment($@"""{at}""");

        Assert.NotNull(found);
        Assert.Equal(Reported, found!.At);
        Assert.Equal(TimeSpan.FromHours(offset), found.At.Offset);
        Assert.Equal(HourlyCheckResult.Healthy, found.Result);
    }

    /// <summary>The fraction of the second the file may write is held, though the box says only the whole second
    /// it stands in: the reading takes the moment to the tick, and a check of the same second written with a
    /// fraction is no check of the second alone - the two hours the box counts from are counted off the tick the
    /// check holds, not off the second it is said in.</summary>
    [Fact]
    public void TheMomentTheFileWritesIsHeldToTheTickItWrites()
    {
        var exact = Moment("\"2026-10-07T01:02:13.4567890Z\"");

        Assert.NotNull(exact);
        Assert.Equal(Reported.Ticks + 4_567_890, exact!.At.Ticks);
        Assert.Equal(13, exact.At.Second);
        Assert.Equal(456, exact.At.Millisecond);
        Assert.NotEqual(Reported, exact.At);

        var half = Moment("\"2026-10-07T01:02:13.5Z\"");

        Assert.NotNull(half);
        Assert.Equal(500, half!.At.Millisecond);
    }

    /// <summary>A moment the file cannot read is no check, whichever way it fails: the empty text and a text of
    /// spaces, a day that is no day, an hour that is no hour, an offset that is no offset, a moment before the
    /// first moment there is once its own offset is counted, a year before the first year, a bare number, a flag,
    /// no value, an array and an object. The moment of <c>an hour ago</c> and the moment the entry does not carry
    /// are <see cref="AvailabilityTests" />; what is pinned here are the blanks, the kinds and the ends.</summary>
    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"  \"")]
    [InlineData("\"2026-13-45T01:02:13Z\"")]
    [InlineData("\"2026-10-07T25:02:13Z\"")]
    [InlineData("\"2026-10-07T01:02:13+00:60\"")]
    [InlineData("\"0001-01-01T00:00:00+00:01\"")]
    [InlineData("\"0000-01-01T00:00:00Z\"")]
    [InlineData("\"9999-99-99T99:99:99Z\"")]
    [InlineData("1759827733")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("""["2026-10-07T01:02:13Z"]""")]
    [InlineData("""{ "utc": "2026-10-07T01:02:13Z" }""")]
    public void AMomentTheFileCannotReadIsNoCheck(string at) => Assert.Null(Moment(at));

    /// <summary>The word and the moment in any order and beside any other words the log carries: the reading asks
    /// its two parts of the object by name, so the instant and the word come back the same check whichever stands
    /// first and whatever else stands with them.</summary>
    [Fact]
    public void TheTwoPartsAreAskedByNameAndWhateverStandsBesideThemIsLeftAlone()
    {
        var found = Last($$"""{ "at": "{{Written}}", "result": " Unreachable ", "note": "the runbook said nothing more", "seconds": 3 }""");

        Assert.Equal(new HourlyCheck(HourlyCheckResult.Unreachable, Reported), found);
    }

    /// <summary>A <c>last</c> that is no object at all is no check: the empty object, which says neither part, the
    /// array and the number and the flag that say nothing at all. The <c>last</c> that is <see langword="null" />
    /// and the <c>last</c> that is a bare word are <see cref="AvailabilityTests" />.</summary>
    [Theory]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("0")]
    [InlineData("true")]
    public void ALastThatIsNoObjectIsNoCheck(string last) => Assert.Null(Last(last));

    // ------------------------------------------------------------------ the record itself

    /// <summary>Two checks are one while the word and the instant are: the equality of the operator, of
    /// <c>Equals</c> boxed and unboxed, and the hash that follows them and stands from call to call - so a check is
    /// usable as the key of a row. The instant counts as the instant it is, not as the offset that wrote it, which
    /// is why the report five hours west is the very check of the report on the meridian.</summary>
    [Fact]
    public void TwoChecksAreOneWhenTheWordAndTheInstantAre()
    {
        var one = ReportedAs(HourlyCheckResult.Healthy);
        var other = new HourlyCheck(HourlyCheckResult.Healthy, SameInstantWest);

        Assert.True(one == other);
        Assert.True(one.Equals(other));
        Assert.True(one.Equals((object)other));
        Assert.True(other.Equals((object)one));
        Assert.Equal(one, other);
        Assert.Equal(one.GetHashCode(), other.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("healthy"));
        Assert.False(one.Equals(Reported));
    }

    /// <summary>
    /// A check differs in either of its parts, and in one of its words: the moment one second later, and the word
    /// turned to either of the other two. Retired deliberately with the closed vocabulary of issue #897: the check
    /// used to differ in a word of another case, the empty word, the word of spaces and no word at all - now the
    /// word is one of the three, and the three tell themselves apart one from another, the other case and the
    /// padding the reading says of them alike, and nothing beside the three a word at all.
    /// </summary>
    [Fact]
    public void ACheckDiffersInEitherOfItsParts()
    {
        var check = ReportedAs(HourlyCheckResult.Healthy);

        Assert.True(check != check with { At = Reported.AddSeconds(1) });
        Assert.NotEqual(check, check with { At = Reported.AddSeconds(1) });
        Assert.NotEqual(check, check with { Result = HourlyCheckResult.Unhealthy });
        Assert.NotEqual(check, check with { Result = HourlyCheckResult.Unreachable });
        Assert.NotEqual(HourlyCheckResult.Healthy, HourlyCheckResult.Unhealthy);
        Assert.NotEqual(HourlyCheckResult.Healthy, HourlyCheckResult.Unreachable);
        Assert.NotEqual(HourlyCheckResult.Unhealthy, HourlyCheckResult.Unreachable);
        Assert.Equal(HourlyCheckResult.Read("HEALTHY"), HourlyCheckResult.Read(" healthy"));
        Assert.Equal(HourlyCheckResult.Healthy, HourlyCheckResult.Read("healthy"));
        Assert.Null(HourlyCheckResult.Read(""));
        Assert.Null(HourlyCheckResult.Read(" "));
    }

    /// <summary><c>with</c> names the part it changes and leaves the rest: the copy that names nothing is the check
    /// it was and not the same object, the copied word leaves the moment where it stood, and the moment copied onto
    /// another offset is the same check because it is the same instant.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var check = new HourlyCheck(HourlyCheckResult.Unreachable, Reported);
        var copy = check with { };

        Assert.Equal(check, copy);
        Assert.NotSame(check, copy);

        var word = check with { Result = HourlyCheckResult.Healthy };
        Assert.Equal(HourlyCheckResult.Healthy, word.Result);
        Assert.Equal(Reported, word.At);

        var moved = check with { At = SameInstantWest };
        Assert.Equal(HourlyCheckResult.Unreachable, moved.Result);
        Assert.Equal(TimeSpan.FromHours(-5), moved.At.Offset);
        Assert.Equal(check, moved);
    }

    /// <summary>The two parts read out of the check in the order they stand in it - the word the check holds, one
    /// of the three the reading makes, and the moment at the end of its number.</summary>
    [Fact]
    public void TheTwoPartsReadOutOfTheCheck()
    {
        var (result, at) = new HourlyCheck(HourlyCheckResult.Unhealthy, SameInstantWest);

        Assert.Equal(HourlyCheckResult.Unhealthy, result);
        Assert.Equal(SameInstantWest, at);

        var (firstWord, never) = new HourlyCheck(HourlyCheckResult.Healthy, DateTimeOffset.MinValue);

        Assert.Equal(HourlyCheckResult.Healthy, firstWord);
        Assert.Equal(DateTimeOffset.MinValue, never);
    }

    /// <summary>The check says its two parts in their order, the name of each with the value it stands on: the word
    /// the check holds - one of the three, in the lowercase spelling the runbook writes it - and the moment in the
    /// hours the viewer reads, with the offset it stands on, the fraction of the second it holds left unsaid. (The
    /// words the moment says are the viewer's, so they are checked in the hours every viewer shares.)</summary>
    [Fact]
    public void TheCheckSaysItsTwoPartsInTheirOrder()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Assert.Equal(
                "HourlyCheck { Result = healthy, At = 10/07/2026 01:02:13 +00:00 }",
                ReportedAs(HourlyCheckResult.Healthy).ToString());
            Assert.Equal(
                "HourlyCheck { Result = unreachable, At = 10/06/2026 20:02:13 -05:00 }",
                new HourlyCheck(HourlyCheckResult.Unreachable, SameInstantWest).ToString());
            Assert.Equal(
                "HourlyCheck { Result = healthy, At = 01/01/0001 00:00:00 +00:00 }",
                new HourlyCheck(HourlyCheckResult.Healthy, DateTimeOffset.MinValue).ToString());
            Assert.Equal(
                "HourlyCheck { Result = healthy, At = 12/31/9999 23:59:59 +00:00 }",
                new HourlyCheck(HourlyCheckResult.Healthy, DateTimeOffset.MaxValue).ToString());
            Assert.Equal(
                "HourlyCheck { Result = healthy, At = 10/07/2026 01:02:13 +00:00 }",
                Moment("\"2026-10-07T01:02:13.4567890Z\"")!.ToString());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}
