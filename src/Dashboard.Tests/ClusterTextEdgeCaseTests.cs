namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterText"/>: the words the cluster's view says
/// its numbers in. The ordinary values of every member are pinned where they are read - <see cref="ClusterTextTests"/>
/// for the cores, the cpu, the memory, the share, the percent, the age, the counts and the names,
/// <see cref="ClusterDeliveryHelperTests"/> for <see cref="ClusterText.PodIn"/>, <see cref="ClusterAssessmentTests"/>
/// and <see cref="ClusterHelperCoverageTests"/> for <see cref="ClusterText.PodWords"/>, and
/// <see cref="ClusterGroupsTests"/> for <see cref="ClusterText.Summary"/>, <see cref="ClusterText.PlatformSummary"/>
/// and the platform's <see cref="ClusterText.Volume"/> - and what they leave open is pinned here: the two ends of
/// each unit and the rounding that lands on the next one (<c>999 m</c> that rounds to <c>1000 m</c>, the byte that
/// is one short of a MiB still said in KiB, the core that <c>999</c> millicores round to), a number that is no number
/// and the two infinities, the zero and the negative where the view says nothing rather than a measurement, the
/// argument that is nothing and the name of the part that is missing, the node name exactly as long as it is cut,
/// the condition in a case the view does not know, the zone that is not UTC, the facts that are one second too old,
/// the pod that does not say when it started, the namespace that has nothing to say, the platform of no namespace,
/// the pool that says a count of one or none and the window of no minutes, and the cluster that is not there said as
/// the empty word in the title of the link.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every use, group, pod and service is one the
/// test builds, and the moment the facts are of is the fixed moment the test's fixture stands on.
/// </para>
/// </summary>
public class ClusterTextEdgeCaseTests
{
    private static readonly DateTimeOffset Now = ClusterFixture.Generated;

    // ----- the constant

    /// <summary>The meter and its number are marked from this share of a limit on: the value the constant stands
    /// on, which the page reads off the name rather than the ninety it types.</summary>
    [Fact]
    public void TheHighPercentIsNinety() => Assert.Equal(90.0, ClusterText.HighPercent);

    // ----- cores and cpu

    /// <summary>The millicores are two digits of a core, and the rounding lands on the next number: the
    /// millicore that rounds to a whole core, the one that stays a fraction, the two that the first digit
    /// drops, the nine digits of a core and the core the rounding reaches.</summary>
    [Theory]
    [InlineData(999, "1")]
    [InlineData(994, "0.99")]
    [InlineData(10, "0.01")]
    [InlineData(9, "0.01")]
    [InlineData(4, "0")]
    [InlineData(999999.999, "1000")]
    [InlineData(1000000000000, "1000000000")]
    public void CoresRoundTheMillicoresToTwoDigits(double millicores, string cores) =>
        Assert.Equal(cores, ClusterText.Cores(millicores));

    /// <summary>A number a node does not report is said as the number it is - not checked: the negative, the
    /// number that is no number and the two infinities stand as the invariant words they are.</summary>
    [Fact]
    public void CoresSayTheNegativeAndTheNumbersThatAreNotNumbers()
    {
        Assert.Equal("-0.5", ClusterText.Cores(-500));
        Assert.Equal("NaN", ClusterText.Cores(double.NaN));
        Assert.Equal("Infinity", ClusterText.Cores(double.PositiveInfinity));
        Assert.Equal("-Infinity", ClusterText.Cores(double.NegativeInfinity));
    }

    /// <summary>The cpu of a node or of the cluster against a whole that is not a core: the two zeros, the
    /// whole that is a core, and the numbers the collector may say that are no numbers at all.</summary>
    [Fact]
    public void TheCoresOfANumberThatIsNotACoreAreSaidAsTheNumbersTheyAre()
    {
        Assert.Equal("0 of 0 cores", ClusterText.CoresOf(0, 0));
        Assert.Equal("1.5 of 1 core", ClusterText.CoresOf(1500, 1000));
        Assert.Equal("-0.5 of 1 core", ClusterText.CoresOf(-500, 1000));
        Assert.Equal("NaN of 1 core", ClusterText.CoresOf(double.NaN, 1000));
        Assert.Equal("Infinity of 1 core", ClusterText.CoresOf(double.PositiveInfinity, 1000));
    }

    /// <summary>The pod stays in millicores below one core, and the rounding of the millicores may reach the
    /// whole core it is under: the last millicore, the one that rounds up, the one that rounds down, and the
    /// numbers the collector may say that are no numbers at all.</summary>
    [Fact]
    public void TheCpuOfAPodRoundsTheMillicoresAndSaysTheNumbersThatAreNotNumbers()
    {
        Assert.Equal("999 m", ClusterText.Cpu(999));
        Assert.Equal("1000 m", ClusterText.Cpu(999.6));
        Assert.Equal("999 m", ClusterText.Cpu(999.4));
        Assert.Equal("-5 m", ClusterText.Cpu(-5));
        Assert.Equal("NaN cores", ClusterText.Cpu(double.NaN));
        Assert.Equal("Infinity cores", ClusterText.Cpu(double.PositiveInfinity));
        Assert.Equal("-Infinity m", ClusterText.Cpu(double.NegativeInfinity));
    }

    /// <summary>The pod against its limit at the ends of the two units: the two zeros, the two that meet at
    /// the core, the one that stays in millicores against a limit of a core and the other way round, and the
    /// numbers that are no numbers at all.</summary>
    [Fact]
    public void TheCpuOfAPodMeetsItsLimitAtTheEndsOfTheTwoUnits()
    {
        Assert.Equal("0 of 0 m", ClusterText.CpuOf(0, 0));
        Assert.Equal("999 m of 1 core", ClusterText.CpuOf(999, 1000));
        Assert.Equal("1 core of 999 m", ClusterText.CpuOf(1000, 999));
        Assert.Equal("1 of 1 core", ClusterText.CpuOf(1000, 1000));
        Assert.Equal("14 of 14 m", ClusterText.CpuOf(14, 14));
        Assert.Equal("-14 of -500 m", ClusterText.CpuOf(-14, -500));
        Assert.Equal("NaN cores of 14 m", ClusterText.CpuOf(double.NaN, 14));
        Assert.Equal("14 m of NaN cores", ClusterText.CpuOf(14, double.NaN));
    }

    /// <summary>The cpu of the pod's row: the limit that is there but not more than zero is no limit, the
    /// usage that is there as zero is a usage, and the use that is nothing is no row at all.</summary>
    [Fact]
    public void TheCpuUseOfALimitThatIsNotMoreThanZeroIsThePlainUsage()
    {
        Assert.Equal("61 m", ClusterText.CpuUse(new ResourceUse(61, 250, 0)));
        Assert.Equal("61 m", ClusterText.CpuUse(new ResourceUse(61, 250, -5)));
        Assert.Equal("0 of 100 m", ClusterText.CpuUse(new ResourceUse(0, 0, 100)));
        Assert.Equal("0 m", ClusterText.CpuUse(new ResourceUse(0, 0, 0)));
        Assert.Equal("NaN cores of 500 m", ClusterText.CpuUse(new ResourceUse(double.NaN, 0, 500)));
        Assert.Equal("Infinity cores of 500 m", ClusterText.CpuUse(new ResourceUse(double.PositiveInfinity, 0, 500)));
    }

    /// <summary>The use that is nothing is no row at all: the rule says so before it says anything else, each
    /// with the name of the part that is not there.</summary>
    [Fact]
    public void AUseThatIsNotThereIsNoUse()
    {
        Assert.Equal("cpu", Assert.Throws<ArgumentNullException>(() => ClusterText.CpuUse(null!)).ParamName);
        Assert.Equal("memory", Assert.Throws<ArgumentNullException>(() => ClusterText.MemoryUse(null!)).ParamName);
        Assert.Equal("cpu", Assert.Throws<ArgumentNullException>(() => ClusterText.NodeCpu(null!)).ParamName);
        Assert.Equal("memory", Assert.Throws<ArgumentNullException>(() => ClusterText.NodeMemory(null!)).ParamName);
        Assert.Equal("use", Assert.Throws<ArgumentNullException>(() => ClusterText.Requested(null!)).ParamName);
    }

    // ----- memory

    /// <summary>The memory of the pod's row, by the same rule: the limit that is there but not more than zero
    /// is no limit, the usage that is there as zero is a usage, and the bytes the collector may say that are
    /// no bytes at all.</summary>
    [Fact]
    public void TheMemoryUseOfALimitThatIsNotMoreThanZeroIsThePlainUsage()
    {
        Assert.Equal("512 KiB", ClusterText.MemoryUse(new ResourceUse(524288, 0, 0)));
        Assert.Equal("512 KiB", ClusterText.MemoryUse(new ResourceUse(524288, 0, -1)));
        Assert.Equal("0 KiB of 512 MiB", ClusterText.MemoryUse(new ResourceUse(0, 0, 536870912)));
        Assert.Equal("NaN KiB", ClusterText.MemoryUse(new ResourceUse(double.NaN, 0, null)));
        Assert.Equal("Infinity GiB of 512 MiB", ClusterText.MemoryUse(new ResourceUse(double.PositiveInfinity, 0, 536870912)));
    }

    /// <summary>The bytes of the node or of all nodes against what they offer: the offer that is not there,
    /// the offer that is there but not more than zero, and the usage that is a step of a unit below the offer.</summary>
    [Fact]
    public void TheMemoryOfANodeAgainstItsOfferSaysTheUnitsApart()
    {
        Assert.Equal("9.2 GiB", ClusterText.NodeMemory(new ResourceUse(9876543210, null, null, null)));
        Assert.Equal("9.2 GiB", ClusterText.NodeMemory(new ResourceUse(9876543210, 0, null, 0)));
        Assert.Equal("512 KiB of 2 GiB", ClusterText.NodeMemory(new ResourceUse(524288, 0, null, 2147483648)));
    }

    /// <summary>The cpu of the node or of all nodes against what they offer: the offer that is there but not
    /// more than zero is no offer, the offer that is a core is said as the core it is, and the usage that is
    /// there as zero is a usage.</summary>
    [Fact]
    public void TheCpuOfANodeAgainstItsOfferSaysTheOfferItHas()
    {
        Assert.Equal("0.81 cores", ClusterText.NodeCpu(new ResourceUse(812, 0, null, 0)));
        Assert.Equal("0.81 cores", ClusterText.NodeCpu(new ResourceUse(812, 0, null, -1)));
        Assert.Equal("0.81 of 1 core", ClusterText.NodeCpu(new ResourceUse(812, null, null, 1000)));
        Assert.Equal("0 of 3.86 cores", ClusterText.NodeCpu(new ResourceUse(0, null, null, 3860)));
    }

    /// <summary>The share the pods ask for: the ask that is there as zero, the offer that is there but not
    /// more than zero, and the numbers the collector may say that are no numbers at all.</summary>
    [Fact]
    public void TheRequestedOfANumberThatIsNotANumberIsSaidAsTheNumberItIs()
    {
        Assert.Equal("0 % requested", ClusterText.Requested(new ResourceUse(0, 0, null, 1000)));
        Assert.Null(ClusterText.Requested(new ResourceUse(10, 10, null, 0)));
        Assert.Null(ClusterText.Requested(new ResourceUse(10, 10, null, -5)));
        Assert.Equal("NaN % requested", ClusterText.Requested(new ResourceUse(0, double.NaN, null, 1000)));
        Assert.Equal("Infinity % requested", ClusterText.Requested(new ResourceUse(0, double.PositiveInfinity, null, 1000)));
        Assert.Equal("-10 % requested", ClusterText.Requested(new ResourceUse(0, -10, null, 100)));
    }

    /// <summary>The bytes at every end of a unit: the byte that is one short of the next unit still in the
    /// old one, the byte that is the unit it is, the negative that rounds to no byte, and the numbers that
    /// are no numbers at all.</summary>
    [Fact]
    public void TheBytesAtTheEndsOfTheUnitsAreSaidInTheUnitTheyStandIn()
    {
        Assert.Equal("0 KiB", ClusterText.Memory(0));
        Assert.Equal("1 KiB", ClusterText.Memory(1023));
        Assert.Equal("1 KiB", ClusterText.Memory(1024));
        Assert.Equal("1024 KiB", ClusterText.Memory(1048575));
        Assert.Equal("1 MiB", ClusterText.Memory(1048576));
        Assert.Equal("1024 MiB", ClusterText.Memory(1073741823));
        Assert.Equal("-0 KiB", ClusterText.Memory(-100));
        Assert.Equal("NaN KiB", ClusterText.Memory(double.NaN));
        Assert.Equal("Infinity GiB", ClusterText.Memory(double.PositiveInfinity));
        Assert.Equal("-Infinity KiB", ClusterText.Memory(double.NegativeInfinity));
    }

    /// <summary>The memory against a limit or a capacity at the ends of the units: the two that share a unit,
    /// the two zeros, the two that do not share one, and the numbers that are no numbers at all.</summary>
    [Fact]
    public void TheMemoryOfANumberThatIsNotAByteIsSaidAsTheNumberItIs()
    {
        Assert.Equal("1 of 2 KiB", ClusterText.MemoryOf(1024, 2048));
        Assert.Equal("9 of 17 MiB", ClusterText.MemoryOf(9000000, 18000000));
        Assert.Equal("0 of 0 KiB", ClusterText.MemoryOf(0, 0));
        Assert.Equal("512 KiB of 9.2 GiB", ClusterText.MemoryOf(524288, 9876543210));
        Assert.Equal("NaN KiB of 1 GiB", ClusterText.MemoryOf(double.NaN, 1073741824));
        Assert.Equal("Infinity GiB of 1 KiB", ClusterText.MemoryOf(double.PositiveInfinity, 1024));
    }

    // ----- the share and the percent

    /// <summary>The share at the ends the reading may stand on: the part that is there as zero, the whole that
    /// is there but not more than zero, the part that is no number, the whole that is no number, and the third
    /// that is no whole number of a percent.</summary>
    [Fact]
    public void AShareAtTheEndsOfTheNumbersIsSaidAsTheNumberItIs()
    {
        Assert.Equal(0.0, ClusterText.Share(0, 100)!.Value);
        Assert.Equal(100.0, ClusterText.Share(100, 100)!.Value);
        Assert.Equal(150.0, ClusterText.Share(150, 100)!.Value);
        Assert.Equal(-50.0, ClusterText.Share(-50, 100)!.Value);
        Assert.Null(ClusterText.Share(10, -100));
        Assert.Null(ClusterText.Share(10, double.NaN));
        Assert.True(double.IsNaN(ClusterText.Share(double.NaN, 100)!.Value));
        Assert.Equal(double.PositiveInfinity, ClusterText.Share(double.PositiveInfinity, 100)!.Value);
        Assert.Equal(33.33, ClusterText.Share(1, 3)!.Value, 2);
    }

    /// <summary>The whole percentage at the ends: the share that is there as zero, the one that rounds to a
    /// whole hundred, the halves the rounding turns to the even side, the negative that rounds to no percent,
    /// and the numbers that are no numbers at all.</summary>
    [Fact]
    public void APercentageAtTheEndsOfTheNumbersIsSaidAsTheNumberItIs()
    {
        Assert.Equal("0 %", ClusterText.Percent(0));
        Assert.Equal("100 %", ClusterText.Percent(99.9));
        Assert.Equal("2 %", ClusterText.Percent(2.5));
        Assert.Equal("4 %", ClusterText.Percent(3.5));
        Assert.Equal("-0 %", ClusterText.Percent(-0.4));
        Assert.Equal("NaN %", ClusterText.Percent(double.NaN));
        Assert.Equal("Infinity %", ClusterText.Percent(double.PositiveInfinity));
        Assert.Equal("1000 %", ClusterText.Percent(1000.4));
    }

    /// <summary>The high mark at its own end: the largest number under it, the smallest above it, the
    /// negative, and the numbers that are no numbers at all.</summary>
    [Fact]
    public void TheHighMarkIsHighFromItsOwnEndOn()
    {
        Assert.False(ClusterText.IsHigh(89.99999999999999));
        Assert.True(ClusterText.IsHigh(90.0000001));
        Assert.False(ClusterText.IsHigh(-100));
        Assert.True(ClusterText.IsHigh(double.PositiveInfinity));
        Assert.False(ClusterText.IsHigh(double.NegativeInfinity));
        Assert.False(ClusterText.IsHigh(double.NaN));
    }

    // ----- the counts and the names

    /// <summary>The restarts at the ends of the count: the plural that is not the one it counts, the negative
    /// that is not the one it counts, and the two ends of the number it is held in.</summary>
    [Fact]
    public void TheRestartsAtTheEndsOfTheCountAreSaidAsTheNumberTheyAre()
    {
        Assert.Equal("2 restarts", ClusterText.Restarts(2));
        Assert.Equal("-1 restarts", ClusterText.Restarts(-1));
        Assert.Equal("2147483647 restarts", ClusterText.Restarts(int.MaxValue));
        Assert.Equal("-2147483648 restarts", ClusterText.Restarts(int.MinValue));
    }

    /// <summary>The parts of a whole at the ends of the counts: the two zeros, the one that is not the whole,
    /// and the two ends of the number a count is held in.</summary>
    [Fact]
    public void ThePartOfAWholeAtTheEndsOfTheCountsIsSaidAsTheNumbersTheyAre()
    {
        Assert.Equal("0 of 0", ClusterText.Of(0, 0));
        Assert.Equal("1 of 2", ClusterText.Of(1, 2));
        Assert.Equal("2147483647 of -2147483648", ClusterText.Of(int.MaxValue, int.MinValue));
    }

    /// <summary>The age at every end of a unit: the span that is there as zero, the second that is one short
    /// of a minute, the two minutes that are two hours, and the two hours that are two days.</summary>
    [Fact]
    public void TheAgeAtTheEndsOfTheUnitsIsSaidInTheUnitItStandsIn()
    {
        Assert.Equal("0 s", ClusterText.Age(Now, Now));
        Assert.Equal("59 s", ClusterText.Age(Now.AddSeconds(-59.999), Now));
        Assert.Equal("1 min", ClusterText.Age(Now.AddSeconds(-60), Now));
        Assert.Equal("119 min", ClusterText.Age(Now.AddSeconds(-(119 * 60 + 59.999)), Now));
        Assert.Equal("2 h", ClusterText.Age(Now.AddSeconds(-120 * 60), Now));
        Assert.Equal("47 h", ClusterText.Age(Now.AddSeconds(-(47 * 3600 + 59 * 60 + 59.999)), Now));
        Assert.Equal("2 d", ClusterText.Age(Now.AddSeconds(-48 * 3600), Now));
        Assert.Equal("24 h", ClusterText.Age(Now.AddDays(-1), Now));
    }

    /// <summary>The node's name at the end the cut lands: the name that is exactly as long as the cut, the
    /// one the cut reaches, the one whose dash is not a part, and the one that says nothing at all.</summary>
    [Fact]
    public void TheShortNodeAtTheEndOfTheCutIsSaidAsTheCutSaysIt()
    {
        Assert.Equal(string.Empty, ClusterText.ShortNode(string.Empty));
        Assert.Equal("-", ClusterText.ShortNode("-"));
        Assert.Equal("-abc", ClusterText.ShortNode("-abc"));
        Assert.Equal("abc-", ClusterText.ShortNode("abc-"));
        Assert.Equal("a-1234567", ClusterText.ShortNode("a-1234567"));
        Assert.Equal("a-…345678", ClusterText.ShortNode("a-12345678"));
        Assert.Equal("a-…efghij", ClusterText.ShortNode("a-b-cdefghij"));
    }

    /// <summary>The name that is not there is no name at all: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void ANameThatIsNotThereIsNoName() =>
        Assert.Equal("name", Assert.Throws<ArgumentNullException>(() => ClusterText.ShortNode(null!)).ParamName);

    /// <summary>The condition the view does not know: the one in a case it does not say, the one that says
    /// nothing at all, and the one that is nothing.</summary>
    [Fact]
    public void APressureTheViewDoesNotKnowIsSaidAsItWasGiven()
    {
        Assert.Equal("MEMORYPRESSURE", ClusterText.Pressure("MEMORYPRESSURE"));
        Assert.Equal("memorypressure", ClusterText.Pressure("memorypressure"));
        Assert.Equal(string.Empty, ClusterText.Pressure(string.Empty));
        Assert.Null(ClusterText.Pressure(null!));
    }

    // ----- the moment

    /// <summary>The facts are said in the time zone of the viewer: the zone that is two hours ahead of the
    /// world, the one that is five behind it, and the moment that is exactly a minute ago.</summary>
    [Fact]
    public void TheAsOfSaysTheTimeInTheZoneOfTheViewer()
    {
        var generated = new DateTimeOffset(2026, 10, 6, 20, 10, 4, TimeSpan.Zero);
        var plusTwo = TimeZoneInfo.CreateCustomTimeZone("PlusTwo", TimeSpan.FromHours(2), "Plus Two", "P2");
        var minusFive = TimeZoneInfo.CreateCustomTimeZone("MinusFive", TimeSpan.FromHours(-5), "Minus Five", "M5");

        Assert.Equal("as of 22:10:04 (5 min ago)", ClusterText.AsOf(generated, generated.AddMinutes(5), plusTwo));
        Assert.Equal("as of 15:10:04 (5 min ago)", ClusterText.AsOf(generated, generated.AddMinutes(5), minusFive));
        Assert.Equal("as of 20:10:04 (1 min ago)", ClusterText.AsOf(generated, generated.AddSeconds(60), TimeZoneInfo.Utc));
    }

    /// <summary>The note under Azure's facts at the ends of its age: the second that makes it old, the
    /// minute that is one over the old mark, and the ages the note says in hours and days.</summary>
    [Fact]
    public void AzuresFactsAtTheEndsOfTheirAgeAreSaidInTheAgeTheyStandIn()
    {
        var service = ClusterFixture.SampleService;
        var read = service.Generated!.Value;

        Assert.Equal(
            "Azure's facts are 90 min old: the workflow that publishes them may not be running.",
            ClusterText.OldFacts(service, read.AddMinutes(90).AddSeconds(1)));
        Assert.Equal(
            "Azure's facts are 91 min old: the workflow that publishes them may not be running.",
            ClusterText.OldFacts(service, read.AddMinutes(91)));
        Assert.Equal(
            "Azure's facts are 2 h old: the workflow that publishes them may not be running.",
            ClusterText.OldFacts(service, read.AddHours(2)));
        Assert.Equal(
            "Azure's facts are 2 d old: the workflow that publishes them may not be running.",
            ClusterText.OldFacts(service, read.AddHours(48)));
    }

    /// <summary>The facts that are not there are no facts at all: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void FactsThatAreNotThereAreNoFacts() =>
        Assert.Equal("service", Assert.Throws<ArgumentNullException>(() => ClusterText.OldFacts(null!, Now)).ParamName);

    // ----- the pod and the namespace

    /// <summary>The pod in a space that says nothing at all: the name is said with the space it is said in,
    /// empty or a word of no letters.</summary>
    [Fact]
    public void APodInASpaceThatSaysNothingIsSaidInTheSpaceItIsSaidIn()
    {
        Assert.Equal("ui in ", ClusterText.PodIn(ClusterFixture.Pod("ui"), string.Empty));
        Assert.Equal("ui in  ", ClusterText.PodIn(ClusterFixture.Pod("ui"), " "));
    }

    /// <summary>The pod's state where the file does not say when it started: the pending pod that does not
    /// say is the pending it is, with the reason it has where it has one; the unknown pod that is not ready
    /// is the unknown it is; and the pending that has started is the time it has been pending.</summary>
    [Fact]
    public void TheWordsOfAPodThatDoesNotSayWhenItStartedAreTheWordsTheFileSays()
    {
        var noStart = ClusterFixture.Pod("ui", ClusterPod.Pending).NotReady().StartedAt(null);
        var noStartEvicted = ClusterFixture.Pod("ui", ClusterPod.Pending, reason: "Evicted").NotReady().StartedAt(null);
        var unknown = ClusterFixture.Pod("ui", ClusterPod.Unknown).NotReady();
        var pendingAWhile = ClusterFixture.Pod("ui", ClusterPod.Pending).NotReady().Started(TimeSpan.FromSeconds(301));

        Assert.Equal("Pending", ClusterText.PodWords(noStart, PodState.Unhealthy, Now));
        Assert.Equal("Evicted", ClusterText.PodWords(noStartEvicted, PodState.Unhealthy, Now));
        Assert.Equal("Unknown", ClusterText.PodWords(unknown, PodState.Unhealthy, Now));
        Assert.Equal("Pending for 5 min", ClusterText.PodWords(pendingAWhile, PodState.Unhealthy, Now));
    }

    /// <summary>The pod that is not there is no pod at all: the rule says so before it says anything
    /// else, with the name of the part that is missing.</summary>
    [Fact]
    public void APodThatIsNotThereIsNoPod() =>
        Assert.Equal("pod", Assert.Throws<ArgumentNullException>(() => ClusterText.PodWords(null!, PodState.Ready, Now)).ParamName);

    /// <summary>The line over a namespace that has nothing to say: no pod is no ready, no finished, no
    /// restart, no cpu and no memory; one pod that reports a cpu of its own says the cpu it reports; and
    /// the group that is nothing is no line at all.</summary>
    [Fact]
    public void TheSummaryOfANamespaceThatHasNothingToSaySaysOnlyWhatItHas()
    {
        Assert.Equal("0 of 0 ready · 0 restarts", ClusterText.Summary(new NamespaceGroup("apps", null, true, [], [])));

        var cpuOnly = ClusterFixture.Pod("ui") with { Cpu = new ResourceUse(53, null, null), Memory = ResourceUse.None };
        Assert.Equal("1 of 1 ready · 0 restarts · CPU 53 m",
            ClusterText.Summary(new NamespaceGroup("apps", null, true, [new PodRow(cpuOnly, PodState.Ready)], [])));

        var memoryOnly = ClusterFixture.Pod("ui") with { Cpu = ResourceUse.None, Memory = new ResourceUse(1572864, null, null) };
        Assert.Equal("1 of 1 ready · 0 restarts · memory 2 MiB",
            ClusterText.Summary(new NamespaceGroup("apps", null, true, [new PodRow(memoryOnly, PodState.Ready)], [])));
    }

    /// <summary>The group that is nothing is no line at all: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void AGroupThatIsNotThereIsNoLine()
    {
        Assert.Equal("group", Assert.Throws<ArgumentNullException>(() => ClusterText.Summary(null!)).ParamName);
        Assert.Equal("groups", Assert.Throws<ArgumentNullException>(() => ClusterText.PlatformSummary(null!)).ParamName);
    }

    /// <summary>The line of the platform of no namespace: the count that is not one is the plural it is,
    /// and the pods that are not there are the zeros they are.</summary>
    [Fact]
    public void ThePlatformOfNoNamespaceIsSaidAsThePlatformItIs() =>
        Assert.Equal("Platform: 0 namespaces, 0 of 0 pods ready", ClusterText.PlatformSummary(new ClusterGroups([], [])));

    /// <summary>The volume claim at the ends: the size that is there as zero, the one a step of a unit below
    /// the GiB, the size without a phase, and the name that says nothing at all.</summary>
    [Fact]
    public void TheVolumeAtTheEndsOfTheUnitsIsSaidAsTheUnitsSayIt()
    {
        Assert.Equal("data 0 KiB, Bound", ClusterText.Volume(new ClusterVolume("data", 0, "Bound")));
        Assert.Equal("data 1 GiB", ClusterText.Volume(new ClusterVolume("data", 1073741824, null)));
        Assert.Equal("cache 1024 MiB, Bound", ClusterText.Volume(new ClusterVolume("cache", 1073741823, "Bound")));
        Assert.Equal(string.Empty, ClusterText.Volume(new ClusterVolume(string.Empty, null, null)));
    }

    /// <summary>The volume that is not there is no volume at all: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void AVolumeThatIsNotThereIsNoVolume() =>
        Assert.Equal("volume", Assert.Throws<ArgumentNullException>(() => ClusterText.Volume(null!)).ParamName);

    // ----- azure

    /// <summary>The node pool at the ends of its count: the count that is one, the one that is none, the one
    /// that is negative, the count with a size it multiplies, and the size that says nothing at all.</summary>
    [Fact]
    public void ThePoolSizeAtTheEndsOfTheCountIsSaidAsTheCountSaysIt()
    {
        Assert.Equal("1 node", ClusterText.PoolSize(new AksPool("user", null, 1, null, null, null, null, null)));
        Assert.Equal("0 nodes", ClusterText.PoolSize(new AksPool("user", null, 0, null, null, null, null, null)));
        Assert.Equal("2 nodes", ClusterText.PoolSize(new AksPool("user", null, 2, null, null, null, null, null)));
        Assert.Equal("-1 nodes", ClusterText.PoolSize(new AksPool("user", null, -1, null, null, null, null, null)));
        Assert.Equal("0 × X", ClusterText.PoolSize(new AksPool("user", null, 0, "X", null, null, null, null)));
        Assert.Equal(string.Empty, ClusterText.PoolSize(new AksPool("user", null, null, string.Empty, null, null, null, null)));
    }

    /// <summary>The pool that is not there is no pool at all: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void APoolThatIsNotThereIsNoPool() =>
        Assert.Equal("pool", Assert.Throws<ArgumentNullException>(() => ClusterText.PoolSize(null!)).ParamName);

    /// <summary>The window the metrics are an average of, at its ends: the minutes that are none, the two,
    /// the negative, and the most a window of minutes can be.</summary>
    [Fact]
    public void TheMetricsWindowAtTheEndsOfTheMinutesIsSaidAsTheMinutesSayIt()
    {
        Assert.Equal("average of the last 0 minutes", ClusterText.MetricsWindow(new AksMetrics(0, 1, 2, 3)));
        Assert.Equal("average of the last 2 minutes", ClusterText.MetricsWindow(new AksMetrics(2, 1, 2, 3)));
        Assert.Equal("average of the last -5 minutes", ClusterText.MetricsWindow(new AksMetrics(-5, 1, 2, 3)));
        Assert.Equal("average of the last 2147483647 minutes", ClusterText.MetricsWindow(new AksMetrics(int.MaxValue, 1, 2, 3)));
    }

    /// <summary>The metrics that are not there are no metrics at all: the rule says so before it says
    /// anything else.</summary>
    [Fact]
    public void MetricsThatAreNotThereAreNoMetrics() =>
        Assert.Equal("metrics", Assert.Throws<ArgumentNullException>(() => ClusterText.MetricsWindow(null!)).ParamName);

    // ----- the portal links

    /// <summary>The titles of the links to the portal where the cluster is not named: the name that is the
    /// empty word, and the one that is a word of no letters - the words of the title stand as the words
    /// they are said with.</summary>
    [Fact]
    public void TheTitlesOfThePortalLinksOfAClusterThatIsNotNamedSayTheWordTheyAreSaidWith()
    {
        Assert.Equal(
            "The AKS cluster  in the Azure portal" + LinkText.PortalSuffix,
            ClusterText.PortalTitle(string.Empty));
        Assert.Equal(
            "The AKS cluster   in the Azure portal" + LinkText.PortalSuffix,
            ClusterText.PortalTitle(" "));
        Assert.Equal(
            "The workloads of  in the Azure portal: deployments, pods and their logs" + LinkText.PortalSuffix,
            ClusterText.WorkloadsTitle(string.Empty));
    }
}
