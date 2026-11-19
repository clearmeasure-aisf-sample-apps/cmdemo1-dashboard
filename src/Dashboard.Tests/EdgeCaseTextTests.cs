using System.Globalization;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The three classes that turn a fact into a word - <see cref="TimeText" /> for a moment, <see cref="LinkText" /> for
/// where a link leads, <see cref="CostText" /> for a sum of money and the days it covers - all settle on their words
/// by comparing a number with a threshold: a length of time is said in seconds until it reaches sixty of them, in
/// minutes until it reaches two hours, in hours until it reaches two days; "yesterday" is true of exactly one day and
/// of nothing else; a sum keeps its minus sign only when it still rounds to something. Every one of those edges is a
/// sentence the page can get wrong in a way no reader notices until the day the app starts writing it - one second
/// too early, one cent too round, one duration too negative.
/// <para>
/// So the edges are pinned here, at zero and one, one step under and one step over each threshold, with the empty and
/// the absent argument where the code takes one and the negative duration where the clock has drifted: the moment a
/// threshold moves, the value it produces moves with it, and the assertion says which is which. Every case names the
/// words or the number the code actually hands back - the odd ones it has always handed back included - and the only
/// refusals asserted are the arguments a method turns down by name, which it does rather than invent a word.
/// </para>
/// <para>
/// The second half is the same rule seen from the other side. A word the page says about a moment or a sum is owned
/// by the code that decides when to say it, and a component that writes "as of", "yesterday", "min ago" or a sum of
/// its own is a copy that drifts: the class moves a threshold and the markup keeps on saying the old words,
/// confidently, forever. A guard therefore walks every <c>.razor</c> file of the app the way
/// <see cref="TimeElementGuardTests" />, <see cref="NewTabLinkGuardTests" /> and <see cref="InlineStyleGuardTests" />
/// walk them - reading each file whole, passing over the comments that quote the words on purpose, and reporting the
/// file and the line a wording stands on - and the wording it looks for is not typed out here but taken from the real
/// returns of the methods above, so the guard follows a rewording instead of arguing with it. What the markup says
/// today anyway is written down, exactly, in <see cref="AllowedSpeech" />: three lines that name a day as plainly as
/// <see cref="CostText" /> does, kept because taking them out would mean editing markup this test has no business
/// editing.
/// </para>
/// </summary>
public class EdgeCaseTextTests
{
    /// <summary>The moment this rendering happens, and the instant every "ago" is counted from.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The last complete UTC day a cost file covers: the day before <see cref="Now" />.</summary>
    private static readonly DateOnly Day = new(2026, 10, 3);

    private static readonly TimeZoneInfo WestFive = TimeZoneInfo.CreateCustomTimeZone("west-five", TimeSpan.FromHours(-5), "West Five", "West Five");
    private static readonly TimeZoneInfo EastFiveThirty = TimeZoneInfo.CreateCustomTimeZone("east-five-thirty", TimeSpan.FromMinutes(330), "East Five Thirty", "East Five Thirty");

    private static readonly CostAmounts Amounts = new(1.52, 9.8, 11.02);
    private static readonly CostReport Report = new(Now, "USD", Day, Amounts, []);

    // ------------------------------------------------------------------------------------------- TimeText

    /// <summary>
    /// <c>TimeText.Span</c> changes unit at two of the next, not at one: seconds to sixty, minutes to two hours,
    /// hours to two days - and it truncates on the way, so 59.999 s is still "59 s" and never "1 min". The last two
    /// rows are the negative half of the number line: a duration below zero has no words of its own, because a
    /// "minus 3 minutes" on a page that says "built" or "deployed" is a clock out by an hour rather than a length of
    /// time, and the clamp says the shortest true thing instead.
    /// </summary>
    [Theory]
    [InlineData("-10675199.02:48:05.4775808", "0 s")]
    [InlineData("-1.00:00:00", "0 s")]
    [InlineData("-00:00:00.0000001", "0 s")]
    [InlineData("00:00:00", "0 s")]
    [InlineData("00:00:00.999", "0 s")]
    [InlineData("00:00:01", "1 s")]
    [InlineData("00:00:59", "59 s")]
    [InlineData("00:00:59.999", "59 s")]
    [InlineData("00:01:00", "1 min")]
    [InlineData("01:59:00", "119 min")]
    [InlineData("02:00:00", "2 h")]
    [InlineData("1.23:59:59", "47 h")]
    [InlineData("2.00:00:00", "2 d")]
    [InlineData("47.12:00:00", "47 d")]
    [InlineData("3650.00:00:00", "3650 d")]
    public void ASpanIsSaidInWholeUnitsAndNeverBelowZero(string span, string expected) =>
        Assert.Equal(expected, TimeText.Span(TimeSpan.Parse(span, CultureInfo.InvariantCulture)));

    /// <summary>The two ends of <see cref="TimeSpan" /> itself, taken as the values rather than as text: the
    /// smallest says the shortest true thing, the largest is still counted in days and not in years.</summary>
    [Fact]
    public void TheEndsOfTimeSpanAreStillSaid()
    {
        Assert.Equal("0 s", TimeText.Span(TimeSpan.MinValue));
        Assert.Equal("0 s", TimeText.Span(TimeSpan.Zero));
        Assert.Equal("1 min", TimeText.Span(TimeSpan.FromMinutes(1)));
        Assert.Equal("10675199 d", TimeText.Span(TimeSpan.MaxValue));
    }

    /// <summary>
    /// <c>TimeText.Ago</c> is the one wording of the page that a viewer reads as a fact about the clock rather than
    /// about arithmetic, so its edges matter twice: a moment under a minute old is "just now" - including the exact
    /// zero, and including a moment ahead of the clock, whose negative duration would otherwise be the only span the
    /// app refuses to say at all. One minute to the second it stops being "just now".
    /// </summary>
    [Theory]
    [InlineData(-86400, "just now")]
    [InlineData(-1, "just now")]
    [InlineData(0, "just now")]
    [InlineData(1, "just now")]
    [InlineData(59, "just now")]
    [InlineData(60, "1 min ago")]
    [InlineData(7199, "119 min ago")]
    [InlineData(7200, "2 h ago")]
    [InlineData(86399, "23 h ago")]
    [InlineData(86400, "24 h ago")]
    [InlineData(172800, "2 d ago")]
    public void TheAgeOfAMomentTurnsOnAMinute(int secondsInThePast, string expected) =>
        Assert.Equal(expected, TimeText.Ago(Now.AddSeconds(-secondsInThePast), Now));

    /// <summary>The clock, the date and the machine-readable moment at the edges of a day and of a year: a moment a
    /// quarter past midnight in one zone is still the day before in another, and the <c>datetime</c> a
    /// <c>&lt;time&gt;</c> writes is UTC whoever looks at it - the year changing underneath on the way back.</summary>
    [Fact]
    public void AInstantCrossedIntoAnotherZoneKeepsItsDayAndItsOffset()
    {
        var midnight = new DateTimeOffset(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal("00:00:00", TimeText.Clock(midnight, TimeZoneInfo.Utc));
        Assert.Equal("19:00:00", TimeText.Clock(midnight, WestFive));
        Assert.Equal("2026-10-04 19:00 -05:00", TimeText.DateAndTime(midnight, WestFive));
        Assert.Equal("2026-10-05 05:30 +05:30", TimeText.DateAndTime(midnight, EastFiveThirty));
        Assert.Equal("2026-10-05 00:00 +00:00", TimeText.DateAndTime(midnight, TimeZoneInfo.Utc));

        // Iso says the same instant to a machine: the offset is taken out, the day it belongs to follows, and the
        // fraction of the second is cut rather than rounded up into a minute the instant never reached.
        Assert.Equal("2026-10-05T00:00:00Z", TimeText.Iso(midnight));
        Assert.Equal("2025-12-31T22:30:00Z", TimeText.Iso(new DateTimeOffset(2026, 1, 1, 0, 30, 0, TimeSpan.FromHours(2))));
        Assert.Equal("2026-10-04T22:00:05Z", TimeText.Iso(new DateTimeOffset(2026, 10, 4, 22, 0, 5, 999, TimeSpan.Zero)));
        Assert.Equal("0001-01-01T00:00:00Z", TimeText.Iso(DateTimeOffset.MinValue));
        Assert.Equal("9999-12-31T23:59:59Z", TimeText.Iso(DateTimeOffset.MaxValue));
    }

    /// <summary>An offset the viewer stands in changes no instant: the same moment at +14 and at -12 is the same
    /// <c>datetime</c>, which is the whole point of writing one.</summary>
    [Theory]
    [InlineData(14)]
    [InlineData(0)]
    [InlineData(-12)]
    public void TheMachineReadableMomentIgnoresTheOffsetItWasWrittenIn(int hours) =>
        Assert.Equal("2026-10-04T22:00:00Z", TimeText.Iso(Now.ToOffset(TimeSpan.FromHours(hours))));

    // ------------------------------------------------------------------------------------------- LinkText

    /// <summary>
    /// <c>LinkText.Portal</c> and <c>LinkText.For</c> glue a name onto a suffix, and a name of nothing is still a
    /// name it has to accept: the subject comes from the topology, and a topology that names nothing leaves the
    /// tooltip reading " of uat" rather than throwing at the middle of a render. What the suffix is stays the
    /// constant's, so an empty subject still promises the new tab and the sign-in.
    /// </summary>
    [Fact]
    public void ALinkTitleOfNothingStillCarriesItsSuffix()
    {
        Assert.Equal(LinkText.PortalSuffix, LinkText.Portal(""));
        Assert.Equal($"uat{LinkText.PortalSuffix}", LinkText.Portal("uat"));
        Assert.Equal($" of {LinkText.PortalSuffix}", LinkText.For("", ""));
        Assert.Equal($" of uat{LinkText.PortalSuffix}", LinkText.For(null!, "uat"));
        Assert.Equal($"Release  of  in Octopus Deploy{LinkText.OctopusSuffix}", LinkText.ReleaseTitle("", ""));
        Assert.Equal($"Commit  on GitHub{LinkText.GitHubSuffix}", LinkText.CommitTitle(""));

        // The suffixes themselves, one for the portal and Octopus, one for a place that asks for nothing.
        Assert.Equal(" (opens in a new tab; the Azure portal asks you to sign in)", LinkText.PortalSuffix);
        Assert.Equal(" (opens in a new tab; Octopus Deploy asks you to sign in)", LinkText.OctopusSuffix);
        Assert.Equal(" (opens in a new tab)", LinkText.GitHubSuffix);
    }

    /// <summary>
    /// A link's key is looked up by <see cref="StringComparer.Ordinal" />, so the one that names no link of
    /// <see cref="LinkSet" /> - a key the topology invented, the same key with its first letter in the wrong case,
    /// or no key at all - falls through to the words built out of it. It is answered rather than thrown, and
    /// answered out of the key it was handed: a key of nothing says " of uat", and <c>Portal</c> is not <c>portal</c>.
    /// </summary>
    [Theory]
    [InlineData("", " of uat")]
    [InlineData("portal", "The web app uat in the Azure portal")]
    [InlineData("Portal", "Portal of uat")]
    [InlineData("PORTAL", "PORTAL of uat")]
    [InlineData("workloads", "workloads of uat")]
    [InlineData("liveMetrics", "Live Metrics of Application Insights for uat: requests, failures and servers as they happen")]
    [InlineData("dependencies", "Dependency calls of uat (SQL and HTTP) of the last hour, a query in Application Insights Logs")]
    [InlineData("0", "0 of uat")]
    public void ALinkKeyIsLookedUpByItsExactSpelling(string key, string expected) =>
        Assert.Equal($"{expected}{LinkText.PortalSuffix}", LinkText.For(key, "uat"));

    /// <summary>
    /// The release page needs both of its halves: without the project's page, or without a version, or with a
    /// version of nothing - the empty string and the whitespace a JSON field drift into included - there is no
    /// release to lead to and the link is absent rather than a guess. The project's trailing slash is taken off
    /// before the path is added, so the address has one separator and not two, and the version is escaped because it
    /// is a string a repository wrote, not a path segment the app chose.
    /// </summary>
    [Theory]
    [InlineData(null, "2.4.15", null)]
    [InlineData("https://a.example.net/app", null, null)]
    [InlineData("https://a.example.net/app", "", null)]
    [InlineData("https://a.example.net/app", "   ", null)]
    [InlineData("https://a.example.net/app/", "2.4.15", "https://a.example.net/app/deployments/releases/2.4.15")]
    [InlineData("https://a.example.net/app", "0", "https://a.example.net/app/deployments/releases/0")]
    [InlineData("https://a.example.net/app", "1.0 / beta+2", "https://a.example.net/app/deployments/releases/1.0%20%2F%20beta%2B2")]
    [InlineData("https://a.example.net/app", "?#", "https://a.example.net/app/deployments/releases/%3F%23")]
    public void AReleasePageWantsItsProjectAndItsVersion(string? project, string? version, string? expected)
    {
        var release = LinkText.Release(project is null ? null : new Uri(project), version);

        Assert.Equal(expected, release?.AbsoluteUri);
    }

    /// <summary>The commit a title names is cut to seven characters when it is longer than seven and written as it
    /// stands when it is not: the length exactly on the cut, one over it, one under it, one character, and the empty
    /// string that cuts to nothing.</summary>
    [Theory]
    [InlineData("", "Commit  on GitHub")]
    [InlineData("0", "Commit 0 on GitHub")]
    [InlineData("012345", "Commit 012345 on GitHub")]
    [InlineData("0123456", "Commit 0123456 on GitHub")]
    [InlineData("01234567", "Commit 0123456 on GitHub")]
    [InlineData("0123456789abcdef", "Commit 0123456 on GitHub")]
    public void ACommitIsNamedByTheSevenCharactersThatDistinguishIt(string commit, string expected) =>
        Assert.Equal($"{expected}{LinkText.GitHubSuffix}", LinkText.CommitTitle(commit));

    /// <summary>A commit of nothing at all - not the empty string, which is a SHA of no characters, but no string -
    /// is refused by name rather than turned into a tooltip that points nowhere.</summary>
    [Fact]
    public void NoCommitOfNothingIsNamed()
    {
        var error = Assert.Throws<ArgumentNullException>(() => LinkText.CommitTitle(null!));

        Assert.Equal("commit", error.ParamName);
    }

    // ------------------------------------------------------------------------------------------- CostText

    /// <summary>
    /// A sum is said to the cent, away from zero, with the sign only while there is something to put a sign in
    /// front of: a refund of less than half a cent rounds to nothing and is said without the minus, which is the
    /// difference between "the bill was corrected" and "you owe minus nothing". The thousands separator and the
    /// decimal point are the invariant ones whoever reads the page, and the currency is what Azure billed in - the
    /// dollar for USD however the code spells it, the code after the number for anything else, nothing at all for a
    /// file that named none, and a dash for a number the file never gave.
    /// </summary>
    [Theory]
    [InlineData(null, "USD", CostText.NoNumber)]
    [InlineData(null, null, CostText.NoNumber)]
    [InlineData(0d, "USD", "$0.00")]
    [InlineData(0d, null, "0.00")]
    [InlineData(0.001, "USD", "$0.00")]
    [InlineData(0.004, "USD", "$0.00")]
    [InlineData(0.005, "USD", "$0.01")]
    [InlineData(0.0051, "USD", "$0.01")]
    [InlineData(-0.0049, "USD", "$0.00")]
    [InlineData(-0.005, "USD", "-$0.01")]
    [InlineData(-0.3, "usd", "-$0.30")]
    [InlineData(999.994, "USD", "$999.99")]
    [InlineData(999.995, "USD", "$1,000.00")]
    [InlineData(-1000.005, "USD", "-$1,000.01")]
    [InlineData(1e21, "USD", "$1,000,000,000,000,000,000,000.00")]
    [InlineData(1.5, "EUR", "1.50 EUR")]
    [InlineData(1.5, "eur", "1.50 eur")]
    [InlineData(1.5, "usd ", "1.50 usd ")]
    [InlineData(1.5, "", "1.50 ")]
    public void ASumIsRoundedAwayFromZeroAndSignedOnlyWhenItRoundsToSomething(double? amount, string? currency, string expected) =>
        Assert.Equal(expected, CostText.Money(amount, currency));

    /// <summary>A zero written with a minus on it is still zero: the sign is only ever put in front of an amount
    /// that rounds to something, so the negative of nothing reads as nothing.</summary>
    [Fact]
    public void ANegativeZeroIsSaidWithoutASign()
    {
        Assert.Equal("$0.00", CostText.Money(-0.0, "USD"));
        Assert.Equal("0.00", CostText.Money(-0.0, null));
        Assert.Equal("0.00 EUR", CostText.Money(-0.0, "EUR"));
    }

    /// <summary>A number Azure did not give is a dash; a number that is not a number at all is said as the words the
    /// invariant format has for it. The cost monitor reads only finite numbers out of the file, so this is what the
    /// public method owes a caller that hands it the rest of the number line - a value pinned here rather than one
    /// quietly allowed to change.</summary>
    [Fact]
    public void ASumThatIsNoNumberIsSaidAsOne()
    {
        Assert.Equal("$NaN", CostText.Money(double.NaN, "USD"));
        Assert.Equal("$Infinity", CostText.Money(double.PositiveInfinity, "USD"));
        Assert.Equal("-$Infinity", CostText.Money(double.NegativeInfinity, "USD"));
        Assert.Equal("NaN", CostText.Money(double.NaN, null));
    }

    /// <summary>The currency is compared with <c>USD</c> by sense of case and by nothing else, and an empty code is
    /// a code that is not <c>USD</c>: the number stands with a trailing space and no symbol. Awkward, and the shape
    /// the page would print today - pinned so that a change of mind here is a change of the page's words.</summary>
    [Fact]
    public void ACurrencyOfNothingNamesNoSymbol()
    {
        Assert.Equal("0.00", CostText.Money(0, null));
        Assert.Equal("0.00 ", CostText.Money(0, string.Empty));
        Assert.Equal("$0.00", CostText.Money(0, "UsD"));
    }

    /// <summary>
    /// "yesterday" is true of exactly one day: the day the file covers, the UTC day before the rendering. The day
    /// itself, the day before that, a day still to come and the first day there is are all named by their date, so a
    /// cost file nobody renewed cannot read as though it were this morning's.
    /// </summary>
    [Theory]
    [InlineData(null, null, null, "in the last full day")]
    [InlineData(2026, 10, 4, "on 2026-10-04")]
    [InlineData(2026, 10, 3, "yesterday")]
    [InlineData(2026, 10, 2, "on 2026-10-02")]
    [InlineData(2026, 10, 5, "on 2026-10-05")]
    [InlineData(2026, 1, 1, "on 2026-01-01")]
    [InlineData(1, 1, 1, "on 0001-01-01")]
    [InlineData(9999, 12, 31, "on 9999-12-31")]
    public void TheDayOfASumIsYesterdayOnlyTheDayAfterIt(int? year, int? month, int? day, string expected) =>
        Assert.Equal(expected, CostText.Day(year is null ? null : new DateOnly(year.Value, month!.Value, day!.Value), Now));

    /// <summary>The day is the day in UTC, wherever the viewer stands: 22:00 on the 4th in UTC is already the 5th in
    /// Tokyo, and the file's "yesterday" is still the 3rd because its days are UTC days.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(-8)]
    public void YesterdayIsTheUtcDayBeforeWhateverTheViewersOffset(int hours) =>
        Assert.Equal("yesterday", CostText.Day(Day, Now.ToOffset(TimeSpan.FromHours(hours))));

    /// <summary>The month a sum covers is "this month" only while the rendering stands in the month the day does -
    /// the same month in another year is not this one, December seen from January has a year to name, and a day that
    /// is not there names no month at all.</summary>
    [Theory]
    [InlineData(null, null, null, "this month")]
    [InlineData(2026, 10, 3, "this month")]
    [InlineData(2026, 10, 31, "this month")]
    [InlineData(2026, 10, 1, "this month")]
    [InlineData(2026, 9, 30, "in September")]
    [InlineData(2026, 11, 1, "in November")]
    [InlineData(2026, 12, 31, "in December")]
    [InlineData(2025, 10, 3, "in October 2025")]
    [InlineData(2025, 12, 31, "in December 2025")]
    public void TheMonthOfASumIsNamedWhenItIsNoLongerThisOne(int? year, int? month, int? day, string expected) =>
        Assert.Equal(expected, CostText.Month(year is null ? null : new DateOnly(year.Value, month!.Value, day!.Value), Now));

    /// <summary>A date of the page is written the same in whatever culture the server's thread happens to carry:
    /// four digits, a month, a day, and the year never cut to two.</summary>
    [Theory]
    [InlineData(1, 1, 1, "0001-01-01")]
    [InlineData(2026, 1, 9, "2026-01-09")]
    [InlineData(2026, 10, 3, "2026-10-03")]
    [InlineData(9999, 12, 31, "9999-12-31")]
    public void ADayIsWrittenTheSameWhereverTheServerRuns(int year, int month, int day, string expected) =>
        Assert.Equal(expected, CostText.Date(new DateOnly(year, month, day)));

    /// <summary>The day the numbers stop at, or nothing when the file never said: the words carry the date and the
    /// zone the days are counted in.</summary>
    [Fact]
    public void TheAsOfNamesItsDayOrSaysNothing()
    {
        Assert.Null(CostText.AsOf(null));
        Assert.Equal("as of 2026-10-03 (UTC)", CostText.AsOf(Day));
        Assert.Equal("as of 0001-01-01 (UTC)", CostText.AsOf(DateOnly.MinValue));
        Assert.Equal("UTC", CostText.DayZone);
    }

    /// <summary>
    /// The three sums always come back as three parts, whether Azure gave the numbers or not: a period is never
    /// left out because its amount is missing, so the line keeps its shape from one refresh to the next and a reader
    /// learns the places rather than the words. The absent amounts are the dash of <see cref="CostText.NoNumber" />.
    /// </summary>
    [Fact]
    public void TheThreePartsAreAlwaysThree()
    {
        Assert.Equal(
            [new CostPart("$1.52", "yesterday"), new CostPart("$9.80", "in 7 days"), new CostPart("$11.02", "this month")],
            CostText.Parts(Amounts, "USD", Day, Now));

        Assert.Equal(
            [new CostPart(CostText.NoNumber, "yesterday"), new CostPart(CostText.NoNumber, "in 7 days"), new CostPart(CostText.NoNumber, "this month")],
            CostText.Parts(new CostAmounts(null, null, null), "USD", Day, Now));

        // A zero is a number Azure gave: it is said, and it is not the dash.
        Assert.Equal(
            [new CostPart("$0.00", "yesterday"), new CostPart("$0.00", "in 7 days"), new CostPart("$0.00", "this month")],
            CostText.Parts(new CostAmounts(0, 0, 0), "USD", Day, Now));

        // An undated file is of "the last full day" and of "this month", which are true whatever the day.
        Assert.Equal(
            [new CostPart("1.52", "in the last full day"), new CostPart("9.80", "in 7 days"), new CostPart("11.02", "this month")],
            CostText.Parts(Amounts, null, null, Now));
    }

    /// <summary>A whole line: the three parts, joined, and the day they stop at only when the file named one.</summary>
    [Fact]
    public void ALineJoinsItsPartsAndAppendsItsDayWhenItHasOne()
    {
        Assert.Equal(
            "$1.52 yesterday · $9.80 in 7 days · $11.02 this month · as of 2026-10-03 (UTC)",
            CostText.Line(Amounts, Report, Now));

        Assert.Equal(
            "— in the last full day · — in 7 days · — this month",
            CostText.Line(new CostAmounts(null, null, null), Report with { AsOf = null }, Now));
    }

    /// <summary>One service is a list of one and two are joined with a comma: the page is handed the whole list and
    /// says it in one run of words, the amounts following the code that billed them.</summary>
    [Theory]
    [InlineData(1, "Most this month: One $1.00")]
    [InlineData(2, "Most this month: One $1.00, Two $2.00")]
    public void ServicesAreListedWhileThereIsOne(int count, string expected)
    {
        CostService[] services =
        [
            new CostService("One", 1),
            new CostService("Two", 2),
        ];

        Assert.Equal(expected, CostText.Services(services[..count], Report, Now));
    }

    /// <summary>No service, an empty list and a service whose amount is missing: the first two say nothing at all,
    /// the third is still a service and its amount is the dash.</summary>
    [Fact]
    public void NoServiceSaysNothingAndAServiceOfNoAmountIsStillListed()
    {
        Assert.Null(CostText.Services(null, Report, Now));
        Assert.Null(CostText.Services([], Report, Now));
        Assert.Equal("Most this month: SQL Database —", CostText.Services([new CostService("SQL Database", null)], Report, Now));
    }

    /// <summary>An estimate is said when it has a month to be of: zero is an amount and is said, no amount at all is
    /// silent, and the share is rounded to whole percent away from zero - half a percent of what all pods request
    /// reads as 1 %, and a tenth of that reads as 0 %.</summary>
    [Theory]
    [InlineData(null, "plus about $0.50 this month of what the environments share")]
    [InlineData(0d, "plus about $0.50 this month of what the environments share (0 % of what all pods request)")]
    [InlineData(0.0049, "plus about $0.50 this month of what the environments share (0 % of what all pods request)")]
    [InlineData(0.005, "plus about $0.50 this month of what the environments share (1 % of what all pods request)")]
    [InlineData(0.2048, "plus about $0.50 this month of what the environments share (20 % of what all pods request)")]
    [InlineData(0.995, "plus about $0.50 this month of what the environments share (100 % of what all pods request)")]
    [InlineData(1d, "plus about $0.50 this month of what the environments share (100 % of what all pods request)")]
    public void AnEstimateSaysItsShareInWholePercent(double? share, string expected) =>
        Assert.Equal(expected, CostText.Estimate(new CostEstimate(share, new CostAmounts(0, 0, 0.5)), Report, Now));

    /// <summary>Nothing to estimate and an estimate with no month in it both say nothing; an estimate of zero for
    /// the month is said, because zero is a number Azure gave and not a number that is missing.</summary>
    [Fact]
    public void AnEstimateOfNoMonthSaysNothing()
    {
        Assert.Null(CostText.Estimate(null, Report, Now));
        Assert.Null(CostText.Estimate(new CostEstimate(0.2, new CostAmounts(1.1, 2.2, null)), Report, Now));
        Assert.Equal(
            "plus about $0.00 this month of what the environments share (20 % of what all pods request)",
            CostText.Estimate(new CostEstimate(0.2048, new CostAmounts(0, 0, 0)), Report, Now));
    }

    /// <summary>The help and the tooltips take the day they name out of the file, and say the shorter true thing
    /// when the file named none. A name of nothing is quoted all the same.</summary>
    [Fact]
    public void TheHelpNamesTheDaysItExplains()
    {
        Assert.StartsWith("Not live: what Azure Cost Management reported for complete UTC days up to 2026-10-03, by", CostText.Help(Report), StringComparison.Ordinal);
        Assert.StartsWith("Not live: what Azure Cost Management reported for complete UTC days, by", CostText.Help(Report with { AsOf = null }), StringComparison.Ordinal);
        Assert.Equal(
            """Azure bills resources with the tag "environment" = "", which is no environment of this topology.""",
            CostText.OtherHelp(string.Empty));
        Assert.Equal(
            """Azure bills resources with the tag "environment" = "retired", which is no environment of this topology.""",
            CostText.OtherHelp("retired"));
    }

    /// <summary>The methods that read a report refuse the one that is not there, by name, rather than answering a
    /// viewer with the words of an empty file: a missing amount is the dash, a missing report is a bug.</summary>
    [Fact]
    public void AReportOfNothingIsRefusedByName()
    {
        Assert.Equal("amounts", Assert.Throws<ArgumentNullException>(() => CostText.Parts(null!, "USD", Day, Now)).ParamName);
        Assert.Equal("amounts", Assert.Throws<ArgumentNullException>(() => CostText.Line(null!, Report, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Line(Amounts, null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Services([], null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Estimate(null, null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Help(null!)).ParamName);
    }

    // -------------------------------------------------------------------------------- the wording the code owns

    /// <summary>How a wording a component must not write is looked for.</summary>
    private enum Kind
    {
        /// <summary>The words themselves, with no letter glued in front of them and no letter or digit glued to
        /// their back - a line that says "was of" or "this monthly" says something else.</summary>
        Words,

        /// <summary>The words with a digit straight after them - the sign <c>CostText.Money</c> puts in front of a number.</summary>
        Lead,

        /// <summary>The words as a shape, every digit of the sample return standing for any digit at all - a day of
        /// the same length whatever day it is.</summary>
        Shape,
    }

    /// <summary>
    /// One wording the page says about a moment, a link or a sum, and the code that decides when it is true. The
    /// words are not typed out below: they are read back out of the real returns of <see cref="TimeText" />,
    /// <see cref="LinkText" /> and <see cref="CostText" />, so a rewording moves the guard with it instead of leaving
    /// a list of the old words behind, and a threshold moved is caught by the tests above rather than quietly
    /// tolerated here.
    /// </summary>
    private sealed record OwnedWord(string Words, string Owner, Kind Kind);

    /// <summary>The unit words <c>TimeText.Span</c> settles on, read off one span of each band.</summary>
    private static readonly string[] Units =
    [
        LastWord(TimeText.Span(TimeSpan.FromSeconds(45))),
        LastWord(TimeText.Span(TimeSpan.FromMinutes(3))),
        LastWord(TimeText.Span(TimeSpan.FromHours(2))),
        LastWord(TimeText.Span(TimeSpan.FromDays(4))),
    ];

    /// <summary>The word <c>TimeText.Ago</c> hangs on the end of a span.</summary>
    private static readonly string Ago = LastWord(TimeText.Ago(Now.AddMinutes(-5), Now));

    /// <summary>The day part of "as of 2026-10-03 (UTC)": the words before the date.</summary>
    private static readonly string AsOfWords = BeforeFirstDigit(CostText.AsOf(Day)!);

    /// <summary>The sign <c>CostText.Money</c> puts in front of a dollar amount.</summary>
    private static readonly string DollarSign = BeforeFirstDigit(CostText.Money(0, "USD"));

    /// <summary>The shape of a day as <c>CostText.Date</c> writes it - <c>####-##-##</c> for the file that writes
    /// <c>2026-10-03</c>, and the shape of whatever it writes on any other day.</summary>
    private static readonly string DayShape = Shape(CostText.Date(Day));

    /// <summary>The words a link's suffix says beyond "opens in a new tab", which the markup is required to write
    /// itself by <see cref="NewTabLinkGuardTests" />: the part that names who asks for a sign-in.</summary>
    private static readonly string[] SignIn = [Between(LinkText.PortalSuffix, "; ", ")"), Between(LinkText.OctopusSuffix, "; ", ")")];

    /// <summary>Every wording the text classes own, as the code returns it today.</summary>
    private static readonly OwnedWord[] Owned =
    [
        .. Units.Select(unit => new OwnedWord($"{unit} {Ago}", $"TimeText.Span counts an age out in {unit}s and TimeText.Ago hangs the word {Ago} on the end of it", Kind.Words)),
        new OwnedWord(TimeText.Ago(Now, Now), "TimeText.Ago says it of a moment under a minute old, and of one ahead of the clock", Kind.Words),
        new OwnedWord(CostText.Day(null, Now), "CostText.Day says it when the file names no day at all", Kind.Words),
        new OwnedWord(CostText.Day(Day, Now), "CostText.Day says it only of the UTC day before the rendering", Kind.Words),
        new OwnedWord(CostText.Month(null, Now), "CostText.Month says it only while the day named stands in this month", Kind.Words),
        new OwnedWord(CostText.Parts(Amounts, "USD", Day, Now)[1].Period, "CostText.Parts names the seven days a sum covers with it", Kind.Words),
        new OwnedWord(AsOfWords, "CostText.AsOf puts it in front of the day its numbers stop at", Kind.Words),
        new OwnedWord(DollarSign, "CostText.Money signs a US dollar amount with it", Kind.Lead),
        new OwnedWord(DayShape, "CostText.Date and TimeText.Iso write a day shaped like it", Kind.Shape),
        .. SignIn.Select(words => new OwnedWord(words, "LinkText ends a portal and an Octopus tooltip with it", Kind.Words)),
    ];

    /// <summary>
    /// The wording the markup writes for itself today, exactly: the file as the walk prints it and the words it
    /// speaks. Three lines name the day a card is of in their own words - <c>CostLine.razor</c> and
    /// <c>WorkItemsView.razor</c> so that the day stands inside a real <c>&lt;time&gt;</c> element that
    /// <see cref="TimeElementGuardTests" /> insists carries a <c>datetime</c>, and <c>DeliveryCard.razor</c> inside a
    /// sentence of its own about when the delivery facts changed. Each is a copy of <see cref="CostText.AsOf" />'s
    /// words, and each is listed rather than rewritten here: this test reads the markup, it does not edit it.
    /// Nothing else is on the list, and nothing joins it without a decision written down next to it.
    /// </summary>
    private static readonly (string File, string Words)[] AllowedSpeech =
    [
        (Path.Combine("Components", "CostLine.razor"), AsOfWords),
        (Path.Combine("Components", "DeliveryCard.razor"), AsOfWords),
        (Path.Combine("Components", "WorkItemsView.razor"), AsOfWords),
    ];

    /// <summary>One place the markup speaks a wording the code owns: the file to print, the line it stands on, the
    /// wording, and the line as the file wrote it.</summary>
    private sealed record SpokenWord(string File, int Line, OwnedWord Word, string Source)
    {
        /// <summary>The allowlist's pair: the file and the words, with no line number to drift.</summary>
        public (string File, string Words) Key => (File, Word.Words);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the words stand on, the words
        /// as the markup wrote them, and the code that owns them.</summary>
        public string Report =>
            $"{File}:{Line} the markup writes \"{Source.Trim()}\": {Word.Owner}, which settles it against a number";
    }

    /// <summary>
    /// No component of the app says for itself what the text classes settle on: no age spelled out of a unit and
    /// "ago", no "yesterday", "this month" or "as of" that no comparison with the clock has earned, no dollar sign
    /// in front of a hand-written number, no day typed out in the shape <c>CostText.Date</c> writes and no
    /// promise of a sign-in that <c>LinkText</c> does not make. Anything the markup does say today is on the list
    /// above, file and words; a wording added without a line here fails, and the failure prints the line to look at.
    /// </summary>
    [Fact]
    public void NoComponentSpeaksWhatTheTextClassesDecide()
    {
        var offenders = OffendersOf(SpokenInApp());

        Assert.Empty(offenders);
    }

    /// <summary>The other direction of the same list: a wording the allowlist approves and the markup no longer
    /// speaks approves nothing, and is reported so that the person who fixes one of these three lines takes it off
    /// rather than leaves a permission standing for words nobody writes. Compared as the multiset it is, so a fourth
    /// "as of" in a file the list already names is refused and one entry less in the list than in the markup is too.</summary>
    [Fact]
    public void EveryAllowedWordingStillStandsInTheMarkup()
    {
        var spoken = SpokenInApp()
            .Select(spoken => spoken.Key)
            .OrderBy(key => key.File, StringComparer.Ordinal)
            .ThenBy(key => key.Words, StringComparer.Ordinal)
            .ToList();

        var allowed = AllowedSpeech
            .OrderBy(entry => entry.File, StringComparer.Ordinal)
            .ThenBy(entry => entry.Words, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(allowed, spoken);
    }

    /// <summary>
    /// A guard that found nothing has read nothing. The walk reaches forty-four files of markup; the wordings above
    /// are fourteen, every one of them read out of what the code returns; and those words do stand in the app -
    /// fourteen times as the files are written, three of them in the markup this rule is about and eleven inside the
    /// comments that quote what a card will read ("5 min ago", "yesterday" and "this month" are said only while they
    /// are true), which is where they belong and where this guard has to be blind. So the check is that the raw files
    /// speak far more often than the comment-blanked ones do: a walk that reached no file fails the counts, and one
    /// that stripped nothing fails the difference.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheMarkupAndTheCommentsItPassesOver()
    {
        var markup = RazorMarkup().ToList();
        var files = markup.Select(part => part.File).ToList();

        Assert.True(files.Count >= 20, $"only {files.Count} .razor file(s) were read under {DashboardRoot()}: the walk does not reach the app");

        Assert.Contains(Path.Combine("Components", "CostLine.razor"), files, StringComparer.Ordinal);
        Assert.Contains(Path.Combine("Components", "WorkItemsView.razor"), files, StringComparer.Ordinal);
        Assert.Contains("App.razor", files, StringComparer.Ordinal);

        Assert.True(Owned.Length >= 12, $"only {Owned.Length} wording(s) are owned: the list stopped reading what the code returns");
        Assert.All(Owned, word => Assert.False(string.IsNullOrWhiteSpace(word.Words), $"{word.Owner}: the wording owns nothing at all"));

        // The list really is what the code returns today rather than an empty one left standing: the words the tests
        // above assert are the words the guard looks for, and the shape is the real day with its digits stood in for.
        // Reword a method and these fail - which is the day this guard has to be read again, not quietly ignored.
        Assert.Contains(Owned, word => word.Words == TimeText.Ago(Now, Now) && word.Words == "just now");
        Assert.Contains(Owned, word => word.Words == CostText.Day(Day, Now) && word.Words == "yesterday");
        Assert.Contains(Owned, word => word.Words == CostText.Month(null, Now) && word.Words == "this month");
        Assert.Contains(Owned, word => word.Words == AsOfWords && word.Words == "as of");
        Assert.Contains(Owned, word => word.Kind == Kind.Shape && word.Words == DayShape && DayShape == "####-##-##");
        Assert.Contains(Owned, word => word.Kind == Kind.Lead && word.Words == DollarSign);

        var raw = markup.Sum(part => CountOf(part.Text));
        var spoken = SpokenInApp();

        Assert.True(raw > spoken.Count, $"the comments held {raw - spoken.Count} of the {raw} occurrence(s): the scan read {spoken.Count} and stripped nothing");
        Assert.True(raw >= 10, $"only {raw} occurrence(s) of an owned wording were seen in {files.Count} file(s): the patterns match nothing");

        // And the three lines the allowlist approves really are the three the app writes.
        Assert.Equal(AllowedSpeech.Length, spoken.Count);
        Assert.All(spoken, word => Assert.Contains("as of", word.Report, StringComparison.Ordinal));
    }

    /// <summary>The rule refuses each kind of wording it owns, on the line the words stand on: an age spelled by
    /// hand, a "yesterday" no comparison with the clock earned, a sum signed in the markup, a day typed out, and a
    /// promise of a sign-in the tooltip does not carry from <c>LinkText</c>.</summary>
    [Fact]
    public void TheRuleRefusesWordsTheMarkupWritesForItself()
    {
        var text = """
            <main>
                <span>built 5 min ago</span>
                <span title="The Azure portal asks you to sign in">portal</span>
                <p>as of yesterday</p>
                <dd>$1.52</dd>
                <dd>2026-10-03</dd>
                <span class="cost__part cost__as-of">no wording here</span>
            </main>
            """;

        var spoken = Scan(Path.Combine("Components", "Sample.razor"), text);
        var lines = spoken.Select(word => word.Line).ToList();

        Assert.Equal([2, 3, 4, 4, 5, 6], lines);

        Assert.Contains(spoken, word => word.Word.Words == "min ago");
        Assert.Contains(spoken, word => word.Word.Words == "the Azure portal asks you to sign in");
        Assert.Contains(spoken, word => word.Word.Words == "as of");
        Assert.Contains(spoken, word => word.Word.Words == "yesterday");
        Assert.Contains(spoken, word => word.Word.Kind == Kind.Lead);
        Assert.Contains(spoken, word => word.Word.Kind == Kind.Shape);

        // The class name that carries the wording's own letters is not the wording: hyphens are not spaces.
        Assert.DoesNotContain(spoken, word => word.Line == 7);

        // Every report prints the file the way the walk prints it and the line the words stand on.
        Assert.All(spoken, word => Assert.StartsWith($"{Path.Combine("Components", "Sample.razor")}:{word.Line} ", word.Report, StringComparison.Ordinal));
    }

    /// <summary>The rule passes over what the code says and over the words of a comment: a Razor call writes the
    /// words at runtime and owns them there, and the doc comments of these very components quote what a card will
    /// read - which is the only way to explain a threshold to the next person without repeating it in markup.</summary>
    [Fact]
    public void TheRuleReadsWhatTheCodeSaysAndSkipsWhatACommentQuotes()
    {
        var text = """
            @* The card reads "5 min ago" and "as of yesterday" - words TimeText and CostText settle on. *@
            <div class="cost">
                <span>@CostText.AsOf(day)</span>
                <time datetime="@TimeText.Iso(at)">@TimeText.Ago(at, Now)</time>
                <span>@CostText.Money(amount, Report.Currency)</span>
                <!-- a note about "this month" that no viewer is shown -->
            </div>
            <!--
                <p>$1.52 as of 2026-10-03</p>
            -->
            @code {
                /// <summary>The moment of this rendering: "yesterday" is counted from it.</summary>
                private DateTimeOffset Now { get; set; }
            }
            """;

        Assert.Empty(Scan(Path.Combine("Components", "Sample.razor"), text));

        // The same text read with its comments left in speaks these words over and over: the sample really carries
        // them, the scan really passed them by, and neither half of the verdict above was reached by matching nothing.
        Assert.True(
            CountOf(text) >= 5,
            $"the sample writes only {CountOf(text)} owned wording(s), so the clean verdict above proves nothing");
    }

    /// <summary>A wording that stands in a file the list does not name is refused all the same, and one that stands
    /// on the list in its own file is not: the pair is what is approved.</summary>
    [Fact]
    public void AnAllowlistEntryApprovesItsOwnFileOnly()
    {
        var allowed = Scan(Path.Combine("Components", "CostLine.razor"), "<span>as of the last change</span>");
        var elsewhere = Scan(Path.Combine("Components", "OtherCard.razor"), "<span>as of the last change</span>");

        Assert.Single(allowed);
        Assert.Single(elsewhere);
        Assert.Empty(OffendersOf(allowed));
        Assert.Single(OffendersOf(elsewhere));

        // The line of the file is the line of the words, and the source is kept for the report.
        var words = Scan(Path.Combine("Components", "Sample.razor"), "one\n  two as of three\n");

        Assert.Single(words);
        Assert.Equal(2, words[0].Line);
        Assert.Contains("two as of three", words[0].Source, StringComparison.Ordinal);
    }

    // --------------------------------------------------------------------------------------------- how it reads

    /// <summary>Every wording the markup speaks, in every component, in the order the files walk.</summary>
    private static List<SpokenWord> SpokenInApp() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>
    /// The words the markup writes that no line of <see cref="AllowedSpeech" /> approves: an allowance is spent by
    /// one occurrence, so a wording the list names once is refused the second time it stands in the same file, and
    /// the message prints the file and the line to look at.
    /// </summary>
    private static List<string> OffendersOf(IEnumerable<SpokenWord> spoken)
    {
        var left = new Dictionary<(string File, string Words), int>();

        foreach (var entry in AllowedSpeech)
        {
            left.TryGetValue(entry, out var allowance);
            left[entry] = allowance + 1;
        }

        var offenders = new List<string>();

        foreach (var word in spoken)
        {
            if (left.TryGetValue(word.Key, out var allowance) && allowance > 0)
            {
                left[word.Key] = allowance - 1;
                continue;
            }

            offenders.Add(word.Report);
        }

        return offenders;
    }

    /// <summary>The owned wordings a piece of markup speaks, with the file to print and the line each stands on. A
    /// whole component and a sample the tests above read the rule through are scanned the same way, and the same way
    /// round: the comments go first, then the words are looked for, so a quoted "5 min ago" in a doc comment is as
    /// passed over in a sample as it is in the app.</summary>
    private static List<SpokenWord> Scan(string file, string text)
    {
        var written = text.Split('\n');
        var spoken = SpokenWordsOnly(text).Split('\n');
        var found = new List<SpokenWord>();

        for (var line = 0; line < spoken.Length; line++)
        {
            foreach (var word in Owned)
            {
                var at = At(spoken[line], word);

                for (var occurrence = 0; occurrence < at.Count; occurrence++)
                {
                    found.Add(new SpokenWord(file, line + 1, word, written[line].TrimEnd('\r')));
                }
            }
        }

        return found;
    }

    /// <summary>How often a piece of text speaks an owned wording, comments included - the count a scan that read no
    /// file, or matched nothing, cannot reach.</summary>
    private static int CountOf(string text)
    {
        var count = 0;

        foreach (var line in text.Split('\n'))
        {
            foreach (var word in Owned)
            {
                count += At(line, word).Count;
            }
        }

        return count;
    }

    /// <summary>Where a wording stands in one line: the index of every occurrence, read case-free because HTML prose
    /// starts a sentence with a capital and a tooltip may hold the same words in the middle of one. A wording is a
    /// wording and not the tail of a longer one, so a letter glued to either end means the line says something else:
    /// "was of" is not "as of" and "this monthly" is not "this month". A shape is the one exception at its own ends,
    /// where a digit outside it would only say the number is longer.</summary>
    private static List<int> At(string line, OwnedWord word)
    {
        var at = new List<int>();

        if (word.Kind == Kind.Shape)
        {
            for (var start = 0; start + word.Words.Length <= line.Length; start++)
            {
                if (Fits(line, start, word.Words)
                    && (start == 0 || !IsDigit(line[start - 1]))
                    && (start + word.Words.Length >= line.Length || !IsDigit(line[start + word.Words.Length])))
                {
                    at.Add(start);
                }
            }

            return at;
        }

        for (var start = line.IndexOf(word.Words, StringComparison.OrdinalIgnoreCase);
             start >= 0;
             start = start + 1 < line.Length ? line.IndexOf(word.Words, start + 1, StringComparison.OrdinalIgnoreCase) : -1)
        {
            var end = start + word.Words.Length;
            var alone = (start == 0 || !char.IsLetter(line[start - 1]))
                        && (word.Kind == Kind.Lead
                            ? end < line.Length && IsDigit(line[end])
                            : end >= line.Length || !(char.IsLetter(line[end]) || IsDigit(line[end])));

            if (alone)
            {
                at.Add(start);
            }
        }

        return at;
    }

    /// <summary>True when the line reads the shape from <paramref name="start"/> on: every <c>#</c> a digit, every
    /// other character itself - the dashes of a day, which no digit stands for.</summary>
    private static bool Fits(string line, int start, string shape)
    {
        for (var i = 0; i < shape.Length; i++)
        {
            if (shape[i] == '#')
            {
                if (!IsDigit(line[start + i]))
                {
                    return false;
                }
            }
            else if (line[start + i] != shape[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    /// <summary>The text a component writes for a viewer: its <c>@* Razor comments *@</c>, its HTML comments and the
    /// <c>//</c> and <c>///</c> lines of its own code blanked out. Everything else keeps its place in the file - the
    /// blanks hold their line breaks, so a wording is reported on the line the file wrote it on.</summary>
    private static string SpokenWordsOnly(string text)
    {
        var chars = Blank(Blank(text, "@*", "*@"), "<!--", "-->").ToCharArray();
        var start = 0;

        while (start < chars.Length)
        {
            var end = Array.IndexOf(chars, '\n', start);
            end = end < 0 ? chars.Length : end;

            if (new string(chars, start, end - start).TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                for (var at = start; at < end; at++)
                {
                    chars[at] = ' ';
                }
            }

            start = end + 1;
        }

        return new string(chars);
    }

    /// <summary>Every character between the marker that opens a comment and the one that closes it - to the end of
    /// the file when nothing does - turned into a space, its line breaks left where they were.</summary>
    private static string Blank(string text, string open, string close)
    {
        var chars = text.ToCharArray();
        var at = 0;

        while (true)
        {
            var from = text.IndexOf(open, at, StringComparison.Ordinal);

            if (from < 0)
            {
                break;
            }

            var closing = text.IndexOf(close, from + open.Length, StringComparison.Ordinal);
            var to = closing < 0 ? text.Length : closing + close.Length;

            for (var i = from; i < to; i++)
            {
                if (chars[i] != '\n')
                {
                    chars[i] = ' ';
                }
            }

            at = to;
        }

        return new string(chars);
    }

    // ------------------------------------------------------------------------------------- what the code returns

    /// <summary>The words a return puts in front of its first digit: <c>as of</c> out of <c>as of 2026-10-03 (UTC)</c>,
    /// <c>$</c> out of <c>$0.00</c>.</summary>
    private static string BeforeFirstDigit(string text)
    {
        var at = text.AsSpan().IndexOfAnyInRange('0', '9');

        return (at < 0 ? text.AsSpan() : text.AsSpan(0, at)).Trim().ToString();
    }

    /// <summary>The last word a return puts after its number: <c>min</c> out of <c>3 min</c>, <c>ago</c> out of
    /// <c>5 min ago</c>.</summary>
    private static string LastWord(string text) =>
        text.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[^1];

    /// <summary>The same words with every digit stood in for by a <c>#</c>: the shape the code writes, whatever
    /// number it writes.</summary>
    private static string Shape(string text) =>
        string.Concat([.. text.Select(c => IsDigit(c) ? '#' : c)]);

    /// <summary>The words between two marks of a return: the sign-in a link's suffix promises, which stands between
    /// the <c>; </c> that opens it and the <c>)</c> that ends the whole parenthesis.</summary>
    private static string Between(string text, string open, string close)
    {
        var from = text.IndexOf(open, StringComparison.Ordinal);
        var to = text.IndexOf(close, from + open.Length, StringComparison.Ordinal);

        return text[(from + open.Length)..to];
    }

    // ------------------------------------------------------------------------------------------------- the walk

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    /// <summary>Every <c>.razor</c> file of the app, as the path the assertion prints and its text. The build's
    /// copies under <c>bin</c> and <c>obj</c> hold the same markup twice, so those directories are walked past.</summary>
    private static IEnumerable<(string File, string Text)> RazorMarkup()
    {
        var root = DashboardRoot();

        foreach (var path in Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);

            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return (relative, File.ReadAllText(path));
        }
    }
}
