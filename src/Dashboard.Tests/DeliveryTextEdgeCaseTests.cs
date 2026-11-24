using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeliveryText" />, the words of the delivery card:
/// the hours of a lead time, whose delivery a card tells itself, the two tooltips, the distance behind the first
/// environment, the counts of the last seven days, and the last failover test.
/// <para>
/// What is pinned where it belongs is walked past here on purpose: <see cref="DeliveryTests" /> says the four units
/// of a lead time, the nine sayings of the distance behind, the counts of the seven days with what failed among them,
/// the failover test in its three parts and whose delivery the shipped file's entries are - and pins the whole text
/// of <see cref="DeliveryText.PersonSignOffHelp" /> to the letter; <see cref="DeliveryReportEdgeCaseTests" /> pins
/// the same members at their ends - a lead time changing its unit at the hour and at the two days, the days behind
/// being whole before they are counted, the distance refusing the comparison it has no ground for, the failures among
/// one deployment and among none, the failover test that says nothing and the empty name saying no more than no name,
/// whose delivery told by the whole name of an entry including the empty environment, and the invariant point a
/// German reader sees on the hours and the seconds; <see cref="DeliveryEntryEdgeCaseTests" /> pins the words of the
/// entry's own numbers - no lead time at all, the smallest a real number holds, the largest a count holds and a count
/// below nothing - and the two counts standing where the file left them; <see cref="DeployableFactsTests" /> and
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pin the words the cards are drawn with; and
/// <see cref="ClusterDeliveryHelperTests" /> pins the tooltip of the comparison in the environment it names.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the hours a file cannot give, said in the negative minutes they are
/// and never taken for their own size; the half minute going to the even whole, and the tenth of an hour that is kept
/// or dropped by what the number really is rather than by what it was written as; the days above two of them counting
/// themselves in days to the tenth; the card whose entry or environment is not there at all, saying the words round
/// an empty place; the tooltip naming nobody, the empty one and the one of nothing but a space; the distance that
/// keeps its two numbers apart while one of them is nothing, the name that is only a space not being the first
/// environment, the first environment named with a space before it, and a distance of no versions and no days that
/// are numbers at the ends of the count; the failures above the deployments they were among and below nothing, the
/// deployments refused while the failures are given, and the words of an entry that hold nothing but the two counts;
/// the failover test of an environment whose name is nothing but spaces, its moment ahead of the one asked of it, its
/// seconds negative and its seconds rounded to the tenth the stopwatch resolves; and the seven members the class
/// holds, so that the day a word is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <c>DeliveryText</c> is words over the
/// numbers and the moments it is handed, and every moment below is one the test brought.
/// </para>
/// </summary>
public class DeliveryTextEdgeCaseTests
{
    /// <summary>The moment the card is asked, fixed and never the clock.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>Nine hours and seventeen minutes before it: <c>9 h ago</c>.</summary>
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>An entry of nothing but the two counts of the last seven days; the other eleven parts stand empty.</summary>
    private static DeliveryEntry Entry(int? deployments, int? failed) =>
        new("ui", null, null, null, null, null, null, null, null, deployments, failed, null);

    // ---------------------------------------------------------------- DeliveryText.LeadTime

    /// <summary>
    /// Hours a file cannot give are said for the number they are and not for their size: the branch of a lead time
    /// asks <c>under an hour</c> first and <c>under two days</c> second, so a negative - which no pipeline can have
    /// measured - never reaches the hours or the days but is said in minutes, as big as the number it was made of.
    /// Nothing takes an absolute value, rounds a lead of nothing up, or hides the mark in front of it.
    /// </summary>
    [Theory]
    [InlineData(0, "0 min")]
    [InlineData(-0.4, "-24 min")]
    [InlineData(-1, "-60 min")]
    [InlineData(-2.5, "-150 min")]
    [InlineData(-48, "-2880 min")]
    [InlineData(-100, "-6000 min")]
    [InlineData(-720, "-43200 min")]
    public void ALeadTimeOfHoursNoFileGivesIsSaidInMinutes(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    /// <summary>
    /// The minutes under an hour are rounded to the whole, and a half goes to the even whole beside it rather than
    /// always up: half a minute is no minute, one and a half is two, two and a half is two again, and three and a
    /// half is four - which is the rounding of the framework, kept as it is so that the same half minute is never one
    /// minute on one card and two on another. What is not a half is rounded the way anybody expects.
    /// </summary>
    [Theory]
    [InlineData(0.008333333333333333, "0 min")]
    [InlineData(0.0125, "1 min")]
    [InlineData(0.025, "2 min")]
    [InlineData(0.0375, "2 min")]
    [InlineData(0.041666666666666664, "2 min")]
    [InlineData(0.058333333333333334, "4 min")]
    [InlineData(0.075, "4 min")]
    [InlineData(0.0999999, "6 min")]
    [InlineData(0.1, "6 min")]
    public void TheMinutesOfALeadTimeGoToTheEvenWhole(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    /// <summary>
    /// The tenth of an hour is kept when the number really has one and dropped when it has none: and a number under
    /// the unit's change of hands can be said in the unit above it, since the words are rounded after the branch was
    /// taken - <c>1.95</c> is hours and is said as <c>2 h</c>, and what is still under forty-eight of them is said as
    /// the forty-eighth hour. Nothing is carried over into the next unit, which is why <c>47.95</c> hours are
    /// <c>48 h</c> and <c>48.04</c> of them are already <c>2 d</c>.
    /// </summary>
    [Theory]
    [InlineData(1.05, "1.1 h")]
    [InlineData(1.95, "2 h")]
    [InlineData(2.4499, "2.4 h")]
    [InlineData(2.45, "2.5 h")]
    [InlineData(24, "24 h")]
    [InlineData(23.9499999, "23.9 h")]
    [InlineData(47.9499, "47.9 h")]
    [InlineData(47.95, "48 h")]
    [InlineData(48.04, "2 d")]
    public void ATenthOfAnHourIsSaidAsTheNumberReallyIs(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    /// <summary>
    /// From two days on the lead time counts whole days with the tenth of one of them when the file wrote one, and
    /// stops at no upper end: a week is <c>7 d</c>, a year is <c>365 d</c>, and a million hours are said as the days
    /// they are and never rolled back into hours or minutes.
    /// </summary>
    [Theory]
    [InlineData(48, "2 d")]
    [InlineData(71.9, "3 d")]
    [InlineData(72.04, "3 d")]
    [InlineData(168, "7 d")]
    [InlineData(8760, "365 d")]
    [InlineData(87600, "3650 d")]
    [InlineData(1000000, "41666.7 d")]
    public void ALeadTimeOfTwoDaysOnCountsItselfInDays(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    /// <summary>
    /// The two unit boundaries of a lead time, each at its edge: just under one hour is still minutes, one hour
    /// exactly is already hours; just under forty-eight hours is still hours, forty-eight hours exactly is already
    /// days. The thresholds are the documented constants of <see cref="DeliveryText" /> and this pins them at their
    /// edges.
    /// </summary>
    [Theory]
    [InlineData(0.9999999, "60 min")]
    [InlineData(1, "1 h")]
    [InlineData(47.9999, "48 h")]
    [InlineData(48, "2 d")]
    public void TheUnitBoundariesOfALeadTimeArePinned(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    // ---------------------------------------------------------------- DeliveryText.Context

    /// <summary>
    /// Whose delivery a card is said round whatever the two names are, and neither of them is asked for: the entry
    /// that is not there and the environment that is not there leave the empty place the words stand in, so that a
    /// card built of nothing says <c> in </c> rather than refusing to be drawn - and the empty name is no name at
    /// all, which is not the system's, while the system's in any case is.
    /// </summary>
    [Theory]
    [InlineData(null, "uat", " in uat")]
    [InlineData(null, null, " in ")]
    [InlineData("", "", " in ")]
    [InlineData(" ", " ", "  in  ")]
    [InlineData("ui", "", "ui in ")]
    [InlineData("ui", null, "ui in ")]
    [InlineData("system", null, "the system (infrastructure and pipeline) in ")]
    [InlineData(null, "system", " in system")]
    [InlineData("system", "system", "the system (infrastructure and pipeline) in system")]
    public void WhoseDeliveryIsSaidRoundANameThatIsNotThere(string? name, string? environment, string expected) =>
        Assert.Equal(expected, DeliveryText.Context(name!, environment!));

    // ---------------------------------------------------------------- DeliveryText.BehindHelp

    /// <summary>
    /// The tooltip of the comparison names the first environment and asks nothing of the name it is given: nobody
    /// named, the empty name and the name of nothing but a space all stand in the sentence, each leaving the place
    /// where the first environment would be read. The rest of the words never change, so the tooltip cannot be shown
    /// for one environment and say something else for another.
    /// </summary>
    [Theory]
    [InlineData("", "Versions: the distance in the project's list of releases. Days: since this environment and  last ran the same release.")]
    [InlineData(null, "Versions: the distance in the project's list of releases. Days: since this environment and  last ran the same release.")]
    [InlineData(" ", "Versions: the distance in the project's list of releases. Days: since this environment and   last ran the same release.")]
    [InlineData("tdd", "Versions: the distance in the project's list of releases. Days: since this environment and tdd last ran the same release.")]
    [InlineData("The first one, named at length", "Versions: the distance in the project's list of releases. Days: since this environment and The first one, named at length last ran the same release.")]
    public void TheTooltipOfTheComparisonNamesWhomeverItIsGiven(string? first, string expected) =>
        Assert.Equal(expected, DeliveryText.BehindHelp(first!));

    // ---------------------------------------------------------------- DeliveryText.Behind

    /// <summary>
    /// The distance keeps its two numbers apart to the end: nothing behind is said before anything is counted, so
    /// the environment that runs the same release as the first one is <c>same as tdd</c> even when the file counts
    /// five days since they last did - the days only ever join a distance of versions, and a distance of none is no
    /// distance at all. The name of the first environment is written where the file's order put it, and the empty one
    /// names nobody rather than stopping the sentence.
    /// </summary>
    [Theory]
    [InlineData(0, 5d, "tdd", "same as tdd")]
    [InlineData(0, 47.9, "tdd", "same as tdd")]
    [InlineData(0, -5d, "tdd", "same as tdd")]
    [InlineData(0, null, "tdd", "same as tdd")]
    [InlineData(0, 5d, "", "same as ")]
    [InlineData(0, null, "", "same as ")]
    [InlineData(2, 3d, "", "2 versions, 3 days behind ")]
    [InlineData(2, 3d, " ", "2 versions, 3 days behind  ")]
    public void TheDistanceOfNothingBehindIsSaidBeforeItIsCounted(int? versions, double? days, string first, string expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(versions, days), "uat", first));

    /// <summary>
    /// The environment asked for is compared with the first one by its whole name, in any case and nowhere else: the
    /// name with a space in front of it is another environment, which is behind, while the first environment named
    /// with a space before it is only ever written at the end of the sentence - the comparison never trims, never
    /// shortens and never asks the current culture what a letter is.
    /// </summary>
    [Theory]
    [InlineData("tdd ", "tdd", "2 versions, 3 days behind tdd")]
    [InlineData(" tdd", "tdd", "2 versions, 3 days behind tdd")]
    [InlineData("tdd", " tdd", "2 versions, 3 days behind  tdd")]
    [InlineData("tdd", "tdd ", "2 versions, 3 days behind tdd ")]
    [InlineData("", "tdd", "2 versions, 3 days behind tdd")]
    [InlineData(null, "tdd", "2 versions, 3 days behind tdd")]
    [InlineData("tdd", "Tdd", null)]
    public void TheFirstEnvironmentIsComparedWithByItsWholeName(string? environment, string? first, string? expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(2, 3), environment!, first));

    /// <summary>
    /// The two numbers at the ends they run to are said as they are counted: the largest a count holds joined by the
    /// days rounded whole from a million of them, a thousand days keeping the singular of the one version before
    /// them, and the smallest a count holds - nothing is skipped, nothing is taken for its own size, and a number no
    /// file can give says no distance rather than a negative one.
    /// </summary>
    [Theory]
    [InlineData(int.MaxValue, 1000000d, "2147483647 versions, 1000000 days behind tdd")]
    [InlineData(int.MinValue, null, null)]
    [InlineData(int.MinValue, 0.5, "same as tdd")]
    [InlineData(-2, null, null)]
    [InlineData(1, 1000.4, "1 version, 1000 days behind tdd")]
    [InlineData(1, 0.5, "1 version behind tdd")]
    [InlineData(2, 47.9, "2 versions, 48 days behind tdd")]
    public void TheDistanceAtTheEndsOfItsTwoNumbers(int? versions, double? days, string? expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(versions, days), "uat", "tdd"));

    /// <summary>
    /// The days that are no number at all are no days: the distance of nothing but them is read as the two
    /// environments running one release, since the count of them asks a number of what is not one, while the distance
    /// that also counts versions says those and holds its peace about the rest - a value no file can give, said as
    /// the absence of the days rather than as anything they might be read for.
    /// </summary>
    [Fact]
    public void TheDistanceOfDaysThatAreNoNumberIsSaidWithoutThem()
    {
        Assert.Equal("same as tdd", DeliveryText.Behind(new BehindFirst(null, double.NaN), "uat", "tdd"));
        Assert.Equal("2 versions behind tdd", DeliveryText.Behind(new BehindFirst(2, double.NaN), "uat", "tdd"));
        Assert.Equal("1 version behind tdd", DeliveryText.Behind(new BehindFirst(1, double.NaN), "uat", "tdd"));
    }

    // ---------------------------------------------------------------- DeliveryText.Frequency

    /// <summary>
    /// The failures are counted among the deployments they were, whether or not that is possible: more failed than
    /// there were deployments is said as the two counts stand, a single deployment among five failures keeps its
    /// singular, and a count below nothing keeps its mark - the words never check the file's arithmetic, which is
    /// where a count the pipeline could not have meant is seen rather than smoothed away.
    /// </summary>
    [Theory]
    [InlineData(2, 5, "2 deployments, 5 failed")]
    [InlineData(1, 5, "1 deployment, 5 failed")]
    [InlineData(100, 100, "100 deployments, 100 failed")]
    [InlineData(1, -2, "1 deployment, -2 failed")]
    [InlineData(0, -1, "0 deployments, -1 failed")]
    [InlineData(3, int.MinValue, "3 deployments, -2147483648 failed")]
    [InlineData(int.MinValue, null, "-2147483648 deployments")]
    [InlineData(int.MinValue, 0, "-2147483648 deployments, none failed")]
    public void TheFailuresAmongTheDeploymentsAreSaidAsTheFileCountsThem(int? deployments, int? failed, string expected) =>
        Assert.Equal(expected, DeliveryText.Frequency(Entry(deployments, failed)));

    /// <summary>
    /// The deployments are what the words are built on, and the failures only ever join them: an entry whose count of
    /// deployments the file never wrote is said as nothing at all even when it names failures, and the zero of them is
    /// a count while the absence of it is not.
    /// </summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, 0)]
    [InlineData(null, 5)]
    [InlineData(null, -3)]
    public void TheDeploymentsTheFileNeverCountedLeaveTheWordsUnsaid(int? deployments, int? failed) =>
        Assert.Null(DeliveryText.Frequency(Entry(deployments, failed)));

    /// <summary>
    /// The frequency says the two counts and nothing else of an entry: the twelve parts a card is built of - the
    /// name, the version, the two moments, the person who signed off, the distance behind - change no word of it, and
    /// the empty entry speaks the same sentence as the full one while its two counts are one. What is refused is the
    /// entry that is not there, by the name of the parameter the guard keeps.
    /// </summary>
    [Fact]
    public void TheFrequencySaysTheTwoCountsOfAnEntryAndNothingElse()
    {
        var full = new DeliveryEntry(
            "ui", "2.4.14", Now, "cm-ai-ops", "Acceptance tests passed in tdd", "9f8e7d6c5b4a", Failed,
            5.2, new BehindFirst(2, 3.4), 4, 0, new Uri("https://octopus.example.net/r/2.4.14"),
            new PersonSignOff("2.4.11", "pat.morgan", Failed, "Reviewed with the customer", null));

        Assert.Equal("4 deployments, none failed", DeliveryText.Frequency(full));
        Assert.Equal(DeliveryText.Frequency(full), DeliveryText.Frequency(Entry(4, 0)));
        Assert.Equal(DeliveryText.Frequency(full with { Name = string.Empty }), DeliveryText.Frequency(Entry(4, 0)));
        Assert.Equal("4 deployments", DeliveryText.Frequency(full with { FailedLast7Days = null }));
        Assert.Equal("entry", Assert.Throws<ArgumentNullException>(() => DeliveryText.Frequency(null!)).ParamName);
    }

    // ---------------------------------------------------------------- DeliveryText.Failover

    /// <summary>
    /// The environment of the failover test is said as the file wrote it and asked for nothing: a name of nothing but
    /// spaces is a name, standing where a name stands and leaving the space between it and the colon that follows -
    /// which is what a card shows when the file names an environment by a blank. The empty name says no more than no
    /// name does, and the name of nothing but a space says that space.
    /// </summary>
    [Theory]
    [InlineData(" ", null, " ")]
    [InlineData("  ", null, "  ")]
    [InlineData("", null, "")]
    [InlineData(" ", 44d, " : the standby answered after 44 s")]
    [InlineData("  ", 44d, "  : the standby answered after 44 s")]
    public void TheEnvironmentOfAFailoverTestIsSaidAsTheFileWroteIt(string environment, double? seconds, string expected) =>
        Assert.Equal(expected, DeliveryText.Failover(new FailoverTest(environment, null, seconds), Now));

    /// <summary>
    /// The moment a failover test stands on is counted to the moment the card is asked and to no clock: a test of the
    /// very moment asked is <c>just now</c>, one ahead of it is <c>just now</c> too rather than a time to come, and
    /// what is forty-eight hours gone on is already said in days - the age standing alone before the seconds, with no
    /// comma in front of it since no environment names itself here.
    /// </summary>
    [Theory]
    [InlineData(0d, "just now: the standby answered after 44 s")]
    [InlineData(-3d, "just now: the standby answered after 44 s")]
    [InlineData(0.5d, "30 min ago: the standby answered after 44 s")]
    [InlineData(1d, "60 min ago: the standby answered after 44 s")]
    [InlineData(2d, "2 h ago: the standby answered after 44 s")]
    [InlineData(1.5d, "90 min ago: the standby answered after 44 s")]
    [InlineData(47.9d, "47 h ago: the standby answered after 44 s")]
    [InlineData(48d, "2 d ago: the standby answered after 44 s")]
    [InlineData(72d, "3 d ago: the standby answered after 44 s")]
    [InlineData(240d, "10 d ago: the standby answered after 44 s")]
    public void TheFailoverTestIsAgedToTheMomentTheCardIsAsked(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.Failover(new FailoverTest(null, Now.AddHours(-hours), 44), Now));

    /// <summary>
    /// The seconds of a failover test are seconds and are never rolled up: an hour of them is <c>3600 s</c> and a day
    /// of them <c>86400 s</c>, the hundredth under a whole one is the whole, and what is under a hundredth of a second
    /// is said as no second at all with its mark in front of it - while a negative duration, which no stopwatch
    /// gives, keeps the mark in front of it too.
    /// </summary>
    [Theory]
    [InlineData(44, "the standby answered after 44 s")]
    [InlineData(44.44, "the standby answered after 44.4 s")]
    [InlineData(100.05, "the standby answered after 100.1 s")]
    [InlineData(0.99, "the standby answered after 1 s")]
    [InlineData(3600, "the standby answered after 3600 s")]
    [InlineData(86400, "the standby answered after 86400 s")]
    [InlineData(-5, "the standby answered after -5 s")]
    [InlineData(-0.04, "the standby answered after -0 s")]
    public void TheSecondsOfAFailoverTestStaySeconds(double seconds, string expected) =>
        Assert.Equal(expected, DeliveryText.Failover(new FailoverTest(null, null, seconds), Now));

    /// <summary>
    /// The three parts of a failover test are joined in the order the card says them and each of them is left out
    /// when the file left it out: the environment and the age with a comma between them, the seconds after a colon,
    /// and the age counted from the moment the card was asked - the same test said at four different moments is four
    /// sentences, none of them asking a clock.
    /// </summary>
    [Fact]
    public void TheThreePartsOfAFailoverTestAreJoinedInTheOrderTheCardSaysThem()
    {
        var test = new FailoverTest("uat", Failed, 44.5);

        Assert.Equal("uat, 9 h ago: the standby answered after 44.5 s", DeliveryText.Failover(test, Now));
        Assert.Equal("uat, 3 d ago: the standby answered after 44.5 s", DeliveryText.Failover(test, Now.AddDays(3)));
        Assert.Equal("uat, just now: the standby answered after 44.5 s", DeliveryText.Failover(test, Failed));
        Assert.Equal("uat, 17 h ago: the standby answered after 44.5 s", DeliveryText.Failover(test, Now.AddHours(8)));
        Assert.Equal(
            "the standby answered after 44.5 s",
            DeliveryText.Failover(test with { Environment = null, At = null }, Now));
        Assert.Equal("uat", DeliveryText.Failover(test with { At = null, Seconds = null }, Now));
        Assert.Equal(string.Empty, DeliveryText.Failover(new FailoverTest(null, null, null), Now.AddMinutes(-45)));
        Assert.Equal("test", Assert.Throws<ArgumentNullException>(() => DeliveryText.Failover(null!, Now)).ParamName);
    }

    // ---------------------------------------------------------------- what the class holds

    /// <summary>
    /// The whole public surface of the words of the delivery card: six methods and the one tooltip the page quotes
    /// as a constant, every one of them static because the class holds nothing, and nothing else to call. A word
    /// added to <see cref="DeliveryText" /> has to be written here as well, which is the point: the list is what a
    /// reader of the card checks the file against, and <see cref="DeliveryText.PersonSignOffHelp" /> is the member
    /// whose text is pinned where the row that shows it is - <see cref="DeliveryTests" /> says it to the letter, and
    /// this says that it is a constant string of the class and one of these seven.
    /// </summary>
    [Fact]
    public void TheWordsOfTheDeliveryCardAreSixMethodsAndOneConstant()
    {
        Assert.Equal(
            ["Behind", "BehindHelp", "Context", "Failover", "Frequency", "LeadTime", "PersonSignOffHelp"],
            [.. typeof(DeliveryText)
                .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(member => member is not MethodInfo method || !method.IsSpecialName)
                .Select(member => member.Name)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)]);

        var help = typeof(DeliveryText).GetField(nameof(DeliveryText.PersonSignOffHelp))!;

        Assert.True(help.IsLiteral);
        Assert.True(help.IsStatic);
        Assert.Equal(typeof(string), help.FieldType);
        Assert.Equal(DeliveryText.PersonSignOffHelp, help.GetRawConstantValue());
        Assert.Empty(typeof(DeliveryText).GetConstructors());
        Assert.Empty(typeof(DeliveryText).GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }
}
