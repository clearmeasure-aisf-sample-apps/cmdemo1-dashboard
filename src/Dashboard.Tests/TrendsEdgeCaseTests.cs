namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="Trends" />, the trends the page draws from the
/// readings a node keeps: the node and the origins no member takes, the one reading that is not yet a line, the
/// reading that is there but says nothing of the number, and the words and the numbers of a line the existing
/// tests leave open.
/// <para>
/// What is pinned where it belongs is left alone: <see cref="TrendTests" /> pins the scale from zero to the
/// largest, the unit after the numbers, the flat zero line, the fewer-than-two-is-no-line rule of
/// <see cref="Trend.Of" />, the sixty readings a node keeps and the <see cref="Trends.Requests" /> title of
/// them, the gap a round without a reading leaves, the title of <see cref="Trends.FromFrontDoor" />, the zero
/// line of <see cref="Trends.Direct" />, the titles of <see cref="Trends.Sql" /> and <see cref="Trends.Cpu" />,
/// and the adding of the origins of a <see cref="Trends.Sum" /> counted back from the last, the empty set
/// included; <see cref="TrendsHelperTests" /> pins <see cref="Trends.Http" /> in full - its line, its gap, its
/// flat zero, the words of its title, its nothing and its refusal of no node; and <see cref="TelemetryTests" />
/// pins the number <see cref="Trends.Sql" /> shows: the requests' commands once the node tells them from the
/// background ones, the older app's total and the readings from before the split left out.
/// </para>
/// <para>
/// What all of that leaves open, and what follows: the refusal of no node by the name of its parameter in every
/// member and the refusal of no origins in <see cref="Trends.Sum" />; the one reading that is no line of
/// <see cref="Trends.Requests" /> and of <see cref="Trends.FromFrontDoor" />; the words and the line of
/// <see cref="Trends.Direct" /> that its zero line never said; the number <see cref="Trends.Sql" /> shows when
/// the last reading no longer tells the commands apart, and the split commands that stay at zero; the reading
/// where the process is there but says no CPU share, the process that says no CPU at all, and the decimal the
/// title keeps; and the <see cref="Trends.Sum" /> where a check no origin reported is left out, where an origin
/// that never reported adds nothing, where nothing was ever reported is no sum, and where the one origin is its
/// own Front Door number.
/// </para>
/// </summary>
public class TrendsEdgeCaseTests
{
    /// <summary>The moment every reading below was taken, so that no test asks the clock.</summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// A reading of the numbers named, every other number of it zero: the least a round can report, the Front
    /// Door having forwarded the requests the node counted. The process is there when a CPU share is named or the
    /// process is said to be, and it says no share when the process is there and no share is named.
    /// </summary>
    private static TelemetrySnapshot Sample(
        int requests,
        int direct = 0,
        int sql = 0,
        int? sqlRequests = null,
        int? sqlBackground = null,
        double? cpu = null,
        bool process = false) => new(requests, requests, direct, 0, null, 0, 0, sql, null, 0, ReadAt)
    {
        Process = cpu is not null || process
            ? new ProcessVitals(cpu, null, null, null, null, null, 600)
            : null,
        SqlRequests = sqlRequests,
        SqlBackground = sqlBackground,
    };

    // ----- the node and the origins no member takes

    /// <summary>
    /// Every trend of a node is refused for the node that is not there, and the refusal is by the name of the
    /// parameter - the one <see cref="Trends.Http" />'s existing refusal does not say - so a caller that mixed up
    /// its two arguments finds out which one it mixed up.
    /// </summary>
    [Fact]
    public void EveryTrendOfANodeRefusesANodeThatIsNotThereByItsName()
    {
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => Trends.Requests(null!)).ParamName);
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => Trends.FromFrontDoor(null!)).ParamName);
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => Trends.Direct(null!)).ParamName);
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => Trends.Sql(null!)).ParamName);
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => Trends.Http(null!)).ParamName);
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => Trends.Cpu(null!)).ParamName);
    }

    /// <summary>The sum of the origins is refused for the origins that are not there, by the name of its own
    /// parameter rather than the node's.</summary>
    [Fact]
    public void TheSumOfTheOriginsRefusesOriginsThatAreNotThereByTheirName() =>
        Assert.Equal("origins", Assert.Throws<ArgumentNullException>(() => Trends.Sum(null!)).ParamName);

    // ----- the requests a node counted

    /// <summary>
    /// The boundary of the requests' line is the existing rule - no line below two readings, the readings a node
    /// never reported not counted - and the line of the two readings the existing title of the sixty never said:
    /// the smallest, the largest and the last in the words the tile shows.
    /// </summary>
    [Fact]
    public void OneReadingIsNoLineOfRequestsAndTwoAreItsLineInWords()
    {
        Assert.Null(Trends.Requests(Targets.NodeWithTelemetry(Sample(4))));

        var trend = Trends.Requests(Targets.NodeWithTelemetry(Sample(3), Sample(10)))!;

        Assert.Equal([0.3, 1], trend.Points);
        Assert.Equal("Requests per minute, last 2 checks: 3 to 10, now 10", trend.Title);
    }

    /// <summary>The requests Front Door forwarded follow the same boundary: the one reading the node counted is
    /// no line yet, and the readings it is counted from are its own rather than the requests it carries.</summary>
    [Fact]
    public void OneReadingIsNoLineFromFrontDoor() =>
        Assert.Null(Trends.FromFrontDoor(Targets.NodeWithTelemetry(Sample(9))));

    /// <summary>
    /// The requests to the web app's own address keep their own words and their own line - the words the tile
    /// shows under the arrow to the node, the numbers the node counted direct - and the same boundary: the one
    /// reading is no line yet.
    /// </summary>
    [Fact]
    public void TheDirectReadingsKeepTheirOwnWordsAndLine()
    {
        Assert.Null(Trends.Direct(Targets.NodeWithTelemetry(Sample(6, direct: 2))));

        var trend = Trends.Direct(Targets.NodeWithTelemetry(Sample(6, direct: 2), Sample(10, direct: 5)))!;

        Assert.Equal([0.4, 1], trend.Points);
        Assert.Equal("Requests per minute to the web app's own address, last 2 checks: 2 to 5, now 5", trend.Title);
    }

    // ----- the SQL number the tile shows

    /// <summary>
    /// The number <see cref="Trends.Sql" /> shows is the split one while the node's last reading tells the
    /// commands apart - and the total one the moment it does not: a node that told them apart for two rounds and
    /// then went silent in a round reports no split in that round, so the trend goes back to all of its commands,
    /// the split ones counted as the totals they were.
    /// </summary>
    [Fact]
    public void TheShownSqlNumberFollowsTheLastReadingAndNotTheOnesBefore()
    {
        var node = Targets.NodeWithTelemetry(
            Sample(4, sql: 75, sqlRequests: 20, sqlBackground: 55),
            Sample(9, sql: 70, sqlRequests: 30, sqlBackground: 40),
            null);

        Assert.Null(node.Telemetry);
        Assert.Equal("SQL commands per minute, last 2 checks: 70 to 75, now 70", Trends.Sql(node)!.Title);
    }

    /// <summary>The split commands that stay at zero are the flat line at the bottom under the split's own words,
    /// the background ones never counted: a node that runs no queries in its requests while it polls its message
    /// bus.</summary>
    [Fact]
    public void SplitSqlCommandsThatStayAtZeroAreAFlatLineUnderTheSplitWords()
    {
        var node = Targets.NodeWithTelemetry(
            Sample(4, sql: 3, sqlRequests: 0, sqlBackground: 3),
            Sample(9, sql: 7, sqlRequests: 0, sqlBackground: 7));

        var trend = Trends.Sql(node)!;

        Assert.Equal([0, 0], trend.Points);
        Assert.Equal("SQL commands per minute while handling requests, last 2 checks: 0 to 0, now 0", trend.Title);
    }

    // ----- the CPU the process reports

    /// <summary>
    /// A reading where the process is there but says no CPU share is a gap in the line, not a zero: the process
    /// measured its memory and its threads and not its CPU, and the line of the two shares it did say keeps its
    /// count of the checks it was drawn over.
    /// </summary>
    [Fact]
    public void AProcessThatSaysNoCpuShareIsAGapInTheLine()
    {
        var node = Targets.NodeWithTelemetry(Sample(4, cpu: 3), Sample(8, process: true), Sample(9, cpu: 9));

        var trend = Trends.Cpu(node)!;

        Assert.Equal([0.333, 1], trend.Points);
        Assert.Equal("CPU of the process, last 2 checks: 3 to 9 %, now 9 %", trend.Title);
    }

    /// <summary>
    /// The unit the CPU trend says after its numbers is the same word the tile says after the same reading:
    /// the trend's title ends in the sign <see cref="VitalsText.Cpu" /> prints, which the trend takes from the
    /// named <see cref="VitalsText.PercentSign" /> rather than a second copy of the sign the tile writes - the
    /// two agree because they say the one const, not because two literals happen to match.
    /// </summary>
    [Fact]
    public void TheCpuTrendSaysTheUnitTheTileSaysAfterTheSameReading()
    {
        var node = Targets.NodeWithTelemetry(Sample(4, cpu: 3), Sample(8, cpu: 9));

        var trend = Trends.Cpu(node)!;

        // The words the tile prints after the same reading end in the sign the trend's title ends in: the tile and
        // the trend say the one unit, so they agree rather than by two literals happening to match.
        Assert.EndsWith(" %", VitalsText.Cpu(9), StringComparison.Ordinal);
        Assert.EndsWith(" %", trend.Title, StringComparison.Ordinal);
    }

    /// <summary>
    /// A process that is there in every reading and says no CPU share in any of them is no line, and a share that
    /// is not a number is no share: the line is drawn over the numbers the process said, and it said none.
    /// </summary>
    [Fact]
    public void AProcessThatSaysNoCpuAtAllIsNoLine()
    {
        Assert.Null(Trends.Cpu(Targets.NodeWithTelemetry(Sample(4, process: true), Sample(8, process: true))));
        Assert.Null(Trends.Cpu(Targets.NodeWithTelemetry(Sample(4, cpu: double.NaN), Sample(8, cpu: double.NaN))));
    }

    /// <summary>The CPU share the title keeps is the share the process said, with its one decimal where it has
    /// one: the number of the line and the number of its words are the same number.</summary>
    [Fact]
    public void ACpuShareWithADecimalKeepsOneInTheTitle()
    {
        var trend = Trends.Cpu(Targets.NodeWithTelemetry(Sample(4, cpu: 2.5), Sample(8, cpu: 3.4)))!;

        Assert.Equal("CPU of the process, last 2 checks: 2.5 to 3.4 %, now 3.4 %", trend.Title);
    }

    // ----- the origins of a Front Door added up

    /// <summary>
    /// A check where no origin reported is left out of the sum, not counted as zero: one origin reported nothing
    /// for the check and the other had no reading that far back, so the sum has no number for the check and the
    /// line is drawn over the checks it has, counted back from the last.
    /// </summary>
    [Fact]
    public void ACheckWhereNoOriginReportedIsLeftOutOfTheSum()
    {
        var reporting = Targets.NodeWithTelemetry(Sample(5), null, Sample(30));
        var oneReading = Targets.NodeWithTelemetry(Sample(1));

        var trend = Trends.Sum([reporting, oneReading])!;

        Assert.Equal([0.161, 1], trend.Points);
        Assert.Equal("Requests per minute through Front Door, last 2 checks: 5 to 31, now 31", trend.Title);
    }

    /// <summary>An origin that never reported has no readings to add, check by check: the sum is the number of
    /// the origins that did, at the checks they did, and the origin that kept its own silence adds nothing to
    /// none of them.</summary>
    [Fact]
    public void AnOriginThatNeverReportedAddsNothingToTheSum()
    {
        var reporting = Targets.NodeWithTelemetry(Sample(5), Sample(10));
        var silent = Targets.PrimaryNode();

        var trend = Trends.Sum([reporting, silent])!;

        Assert.Equal([0.5, 1], trend.Points);
        Assert.Equal("Requests per minute through Front Door, last 2 checks: 5 to 10, now 10", trend.Title);
    }

    /// <summary>Origins that never reported are no sum: there is no check with a number in it, the set of
    /// origins that all kept silent the same nothing the empty set of origins is.</summary>
    [Fact]
    public void OriginsThatNeverReportedAreNoSum() =>
        Assert.Null(Trends.Sum([Targets.PrimaryNode(), Targets.StandbyNode()]));

    /// <summary>
    /// The one origin is its own Front Door number: the sum adds nothing to it and takes nothing from it, so its
    /// line is the line the arrow to it already drew, under the words of the sum rather than the words of the
    /// arrow.
    /// </summary>
    [Fact]
    public void ASingleOriginIsItsOwnFrontDoorNumber()
    {
        var only = Targets.NodeWithTelemetry(Sample(4), Sample(10));

        var sum = Trends.Sum([only])!;

        Assert.Equal(Trends.FromFrontDoor(only)!.Points, sum.Points);
        Assert.Equal("Requests per minute through Front Door, last 2 checks: 4 to 10, now 10", sum.Title);
    }
}
