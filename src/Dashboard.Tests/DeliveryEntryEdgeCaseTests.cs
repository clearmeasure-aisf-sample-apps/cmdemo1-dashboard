using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeliveryEntry" />: the last deployment of one
/// deployable to one environment, which the page shows as a delivery card and compares with the same entry of the
/// other environments.
/// <para>
/// What the reading of the file makes of an entry is pinned where the reading happens, and is not pinned again
/// here: <see cref="DeliveryTests" /> reads the shipped delivery file entry by entry - the whole entry of the
/// sample's <c>ui</c> in <c>uat</c> falling out of thirteen keys, every part of an entry being optional, a
/// person's sign-off surviving the automatic one of the deployment shown, the four units of a lead time and the
/// counts of the last seven days; <see cref="DeliveryReportEdgeCaseTests" /> pins the same reading at its ends -
/// the names and the versions read trimmed and without their build metadata, the numbers a file cannot give (a
/// negative, a word, a count too big for a real number), the deployables of a kind that is no array, the lookups a
/// name answers and the names that answer nothing - and the words the page says of an entry, which are
/// <see cref="DeliveryText" />'s and stand in that file; <see cref="DeliveryEnvironmentTests" /> and
/// <see cref="DeployableFactsTests" /> pin the thirteen-part record as the entries of an environment and as the
/// cards of a row; and <see cref="ClusterDeliveryHelperTests" /> pins <c>DeliveryText.BehindHelp</c>.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself, which all of that walks past: the constructor handing
/// each of its thirteen arguments to the property of its name and holding the very <see cref="BehindFirst" />,
/// <see cref="PersonSignOff" /> and <see cref="Uri" /> it was built of; the name and the four words kept exactly as
/// they are given - empty, padded with whitespace, in their case, trimmed nowhere; the two moments of the entry and
/// the one of its sign-off kept as the instant and the hours it is written in, the ends of the clock among them;
/// the hours and the two counts at the
/// ends they run to, the count below nothing and the count above everything included; the address of a release the
/// page would not link to, kept and never asked; the sign-off the entry may do without; the thirteen parts coming
/// back out in the order they went in; two entries being one while their thirteen parts are - the name in its case,
/// unlike the lookup, the parts in the order they stand in, a moment spelled in another viewer's hours being one
/// entry of the same instant while its hours are kept, and <c>NaN</c> being the number a record is one with although
/// no comparison of two numbers says so - the hash following the equality, and the nothing on either
/// side of the members every record is asked with; the copy that turns one part and keeps the other twelve; the
/// words the record says its parts in, following whoever reads them; and the public surface, so that the day a
/// fourteenth part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the entry holds two moments and an
/// address, but it calls nothing of either - every moment below is one the test brought, and the address is held
/// and never fetched. The members that would ask a clock, and the release page that would be read, are the page's
/// words and the monitor's reading, and both are pinned on those words and in that reading.
/// </para>
/// </summary>
public class DeliveryEntryEdgeCaseTests
{
    /// <summary>The moment the sample's <c>ui</c> was deployed, and the moment of the commit that led to it.</summary>
    private static readonly DateTimeOffset Deployed = new(2026, 10, 4, 16, 37, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Committed = new(2026, 10, 4, 11, 25, 0, TimeSpan.Zero);

    /// <summary>The moment the card is asked, fixed and never the clock: five hours after the deployment above.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The hours five and a half east of the meridian, to spell one instant the other way round.</summary>
    private static readonly TimeSpan East = TimeSpan.FromMinutes(330);

    /// <summary>The entry of the shipped delivery file, part for part, brought here without reading the file.</summary>
    private static DeliveryEntry Full() =>
        new(
            "ui",
            "2.4.14",
            Deployed,
            "cm-ai-ops",
            "Acceptance tests passed in tdd",
            "9f8e7d6c5b4a",
            Committed,
            5.2,
            new BehindFirst(2, 3.4),
            4,
            0,
            new Uri("https://octopus.example.net/r/2.4.14"),
            new PersonSignOff(
                "2.4.11",
                "pat.morgan",
                new DateTimeOffset(2026, 10, 1, 15, 12, 41, TimeSpan.Zero),
                "Reviewed with the customer",
                new Uri("https://octopus.example.net/r/2.4.11")));

    /// <summary>An entry of nothing but a name: the twelve parts a file may leave out, left out.</summary>
    private static DeliveryEntry Bare(string name = "ui") =>
        new(name, null, null, null, null, null, null, null, null, null, null, null);

    // ----- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's thirteen arguments, position by position, come back as the property of the same
    /// name, and the entry holds the very distance, the very sign-off and the very address it was built of: the card
    /// is shown out of these and not out of copies of them. An entry built again of the same thirteen parts is the
    /// same entry and no longer the same reference.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var behind = new BehindFirst(2, 3.4);
        var url = new Uri("https://octopus.example.net/r/2.4.14");
        var signOff = new PersonSignOff("2.4.11", "pat.morgan", null, null, null);
        var entry = new DeliveryEntry(
            "ui", "2.4.14", Deployed, "cm-ai-ops", "Acceptance tests passed in tdd", "9f8e7d6c5b4a", Committed,
            5.2, behind, 4, 0, url, signOff);

        Assert.Equal("ui", entry.Name);
        Assert.Equal("2.4.14", entry.Version);
        Assert.Equal(Deployed, entry.DeployedAt);
        Assert.Equal("cm-ai-ops", entry.SignedOffBy);
        Assert.Equal("Acceptance tests passed in tdd", entry.Reason);
        Assert.Equal("9f8e7d6c5b4a", entry.Commit);
        Assert.Equal(Committed, entry.CommitAt);
        Assert.Equal(5.2, entry.LeadTimeHours);
        Assert.Same(behind, entry.Behind);
        Assert.Equal(4, entry.DeploymentsLast7Days);
        Assert.Equal(0, entry.FailedLast7Days);
        Assert.Same(url, entry.ReleaseUrl);
        Assert.Same(signOff, entry.LastPersonSignOff);
        Assert.Equal(2, entry.Behind!.Versions);
        Assert.Equal(3.4, entry.Behind.Days);
        Assert.Equal("pat.morgan", entry.LastPersonSignOff!.By);
        Assert.Null(entry.LastPersonSignOff.At);
        Assert.Equal("https://octopus.example.net/r/2.4.14", entry.ReleaseUrl!.AbsoluteUri);

        // A new entry of the same thirteen parts is another entry standing on the same facts.
        var again = new DeliveryEntry(
            "ui", "2.4.14", Deployed, "cm-ai-ops", "Acceptance tests passed in tdd", "9f8e7d6c5b4a", Committed,
            5.2, behind, 4, 0, url, signOff);
        Assert.Equal(entry, again);
        Assert.NotSame(entry, again);
    }

    // ----- the name and the words, kept exactly as they are given

    /// <summary>The name is kept in its exact letters: the empty name, the name that is only whitespace, the name
    /// padded with it and the name in another case stand where they stand. That a name padded is not the system and
    /// that an entry is found whatever case it is named in are the lookups, pinned where they happen; the record
    /// turns nothing.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" ui ")]
    [InlineData("UI")]
    [InlineData("uAt")]
    [InlineData("system")]
    [InlineData("SYSTEM")]
    public void TheNameIsKeptExactlyAsItIsGiven(string name)
    {
        var entry = Bare(name);

        Assert.Equal(name, entry.Name);
        Assert.Equal(entry, Bare(name));
        Assert.Equal(entry.GetHashCode(), Bare(name).GetHashCode());
        Assert.NotEqual(entry, Bare($"{name} "));
        Assert.Equal(DeliveryReport.IsSystem(name), DeliveryReport.IsSystem(entry.Name));
    }

    /// <summary>The four words an entry carries - the version, who signed it off, the reason given with that, and
    /// the commit - are kept as letters and nothing more: the empty word is said and not left unsaid, the word of
    /// nothing but whitespace is a word of its own, and a version padded or carrying its build metadata stands
    /// whole, since it is the file's reading that trims and cuts and not the entry.</summary>
    [Fact]
    public void TheWordsAreKeptAsTheyAreGivenAndNotTrimmed()
    {
        var entry = new DeliveryEntry(
            "ui", " 2.4.14+abc123 ", null, " ", " pat.morgan ", " 0a1b2c3d4e5f ", null, null, null, null, null, null);

        Assert.Equal(" 2.4.14+abc123 ", entry.Version);
        Assert.Equal(" ", entry.SignedOffBy);
        Assert.Equal(" pat.morgan ", entry.Reason);
        Assert.Equal(" 0a1b2c3d4e5f ", entry.Commit);
        Assert.NotEqual("2.4.14", entry.Version);
        Assert.NotEqual("pat.morgan", entry.Reason);
        Assert.NotEqual("cm-ai-ops", entry.SignedOffBy);

        var empty = new DeliveryEntry(
            "ui", string.Empty, null, string.Empty, string.Empty, string.Empty, null, null, null, null, null, null);

        Assert.Equal(string.Empty, empty.Version);
        Assert.Equal(string.Empty, empty.SignedOffBy);
        Assert.Equal(string.Empty, empty.Reason);
        Assert.Equal(string.Empty, empty.Commit);
        Assert.NotEqual(Bare(), empty);
        Assert.NotEqual(empty with { Version = null }, empty);
    }

    // ----- the three moments, kept as the moment and the hours they are written in

    /// <summary>What the file does not date stands undated, and no moment is invented for it: the two moments of an
    /// entry are none at the entry that is only a name, and the entry with one of them is another entry than the
    /// one without - which is why a card leaves a line out rather than saying a deployment ended at some unknown
    /// time.</summary>
    [Fact]
    public void TheEntryWithoutItsMomentsKeepsNoMoment()
    {
        var bare = Bare();

        Assert.Null(bare.DeployedAt);
        Assert.Null(bare.CommitAt);
        Assert.Null(bare.LastPersonSignOff);
        Assert.NotEqual(bare, bare with { DeployedAt = Deployed });
        Assert.NotEqual(bare, bare with { CommitAt = Committed });
        Assert.NotEqual(bare with { DeployedAt = Deployed }, bare with { CommitAt = Deployed });
    }

    /// <summary>The moments are kept whole, at the ends the clock runs to and in the hours they are written in:
    /// neither end is the other, the moment at the meridian keeps the hours it was spelled in when it is told from
    /// the one five and a half hours east of it, and the moment that carries a fraction of a second keeps that
    /// fraction. What the entry holds is the moment as the file spelled it, and no reading of it into the viewer's
    /// hours - which the two entries compare as one instant, as the test of that name says.</summary>
    [Fact]
    public void TheMomentsAreKeptAtTheEndsTheClockRunsTo()
    {
        var entry = Full() with { DeployedAt = DateTimeOffset.MinValue, CommitAt = DateTimeOffset.MaxValue };

        Assert.Equal(DateTimeOffset.MinValue, entry.DeployedAt);
        Assert.Equal(DateTimeOffset.MaxValue, entry.CommitAt);
        Assert.NotEqual(entry.DeployedAt, entry.CommitAt);
        Assert.NotEqual(entry, Full());

        var west = Full() with { CommitAt = new DateTimeOffset(2026, 10, 4, 11, 25, 0, TimeSpan.FromHours(-14)) };
        Assert.Equal(TimeSpan.FromHours(-14), west.CommitAt!.Value.Offset);
        Assert.NotEqual(Committed, west.CommitAt);

        var spelled = Full() with { DeployedAt = Deployed.ToOffset(East) };
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 7, 0, East), spelled.DeployedAt);
        Assert.Equal(East, spelled.DeployedAt!.Value.Offset);
        Assert.Equal("5 h ago", TimeText.Ago(Deployed, Now));
        Assert.Equal("5 h ago", TimeText.Ago(spelled.DeployedAt!.Value, Now));

        var exact = Full() with { DeployedAt = new DateTimeOffset(2026, 10, 4, 16, 37, 0, 123, TimeSpan.Zero) };
        Assert.Equal(123, exact.DeployedAt!.Value.Millisecond);
        Assert.NotEqual(exact, Full());
    }

    // ----- the hours of the lead time, at the ends they run to

    /// <summary>The hours from the commit to the deployment are the number the file wrote, kept as it is: nothing
    /// at all, the zero, the smallest a real number holds, a lead the pipeline cannot have meant, the largest there
    /// are, and the number that is no number. The words the card makes of them - <c>45 min</c>, <c>5.2 h</c>,
    /// <c>3.5 d</c> - and the reading's refusal of what no file can give are pinned where they are said.</summary>
    [Fact]
    public void TheLeadTimeIsTheHoursTheFileWroteAndNoOther()
    {
        Assert.Null(Bare().LeadTimeHours);

        var none = Bare() with { LeadTimeHours = 0 };
        Assert.Equal(0d, none.LeadTimeHours!.Value);
        Assert.Equal("0 min", DeliveryText.LeadTime(none.LeadTimeHours!.Value));
        Assert.NotEqual(Bare(), none);

        var smallest = Bare() with { LeadTimeHours = double.Epsilon };
        Assert.Equal(double.Epsilon, smallest.LeadTimeHours);
        Assert.Equal("0 min", DeliveryText.LeadTime(smallest.LeadTimeHours!.Value));

        var negative = Bare() with { LeadTimeHours = -2.5 };
        Assert.Equal(-2.5, negative.LeadTimeHours);
        Assert.True(negative.LeadTimeHours < 0);

        var largest = Bare() with { LeadTimeHours = double.MaxValue };
        Assert.Equal(double.MaxValue, largest.LeadTimeHours);

        var notANumber = Bare() with { LeadTimeHours = double.NaN };
        Assert.True(double.IsNaN(notANumber.LeadTimeHours!.Value));
        Assert.False(notANumber.LeadTimeHours!.Value > 0);
        Assert.False(notANumber.LeadTimeHours!.Value == notANumber.LeadTimeHours!.Value);
    }

    /// <summary>The lead time of an entry is its own number and no other entry's: two entries that differ in this
    /// part alone are two entries, and the hash keeps them apart as the equality does - except of the number that
    /// is no number, which a record holds to be one with itself, although no comparison of two numbers says
    /// so.</summary>
    [Fact]
    public void TheLeadTimeOfOneEntryIsNoLeadTimeOfAnother()
    {
        var five = Bare() with { LeadTimeHours = 5.2 };
        var other = Bare() with { LeadTimeHours = 5.3 };

        Assert.Equal(five, Bare() with { LeadTimeHours = 5.2 });
        Assert.Equal(five.GetHashCode(), (Bare() with { LeadTimeHours = 5.2 }).GetHashCode());
        Assert.NotEqual(five, other);
        Assert.NotEqual(five.GetHashCode(), other.GetHashCode());
        Assert.NotEqual(five, Bare() with { LeadTimeHours = null });

        var notANumber = Bare() with { LeadTimeHours = double.NaN };
        Assert.Equal(notANumber, Bare() with { LeadTimeHours = double.NaN });
        Assert.Equal(notANumber.GetHashCode(), (Bare() with { LeadTimeHours = double.NaN }).GetHashCode());
        Assert.NotEqual(notANumber, five);
    }

    // ----- the distance behind, held as it is given

    /// <summary>The distance to the first environment is held as the very record it stands in, and a record of no
    /// numbers is a distance still: an entry without it is not the entry that holds the distance of nothing said,
    /// which is the difference between a file that does not compare this environment and one that compares it and
    /// has nothing to tell. Both say nothing, and the page says <c>same as tdd</c> of the one and nothing at all of
    /// the other.</summary>
    [Fact]
    public void TheDistanceBehindIsHeldAsTheRecordItStandsIn()
    {
        var behind = new BehindFirst(2, 3.4);
        var entry = Bare() with { Behind = behind };

        Assert.Same(behind, entry.Behind);
        Assert.Equal(2, entry.Behind!.Versions);
        Assert.Equal(3.4, entry.Behind.Days);
        Assert.Equal("2 versions, 3 days behind tdd", DeliveryText.Behind(entry.Behind, "uat", "tdd"));

        var said = Bare() with { Behind = new BehindFirst(null, null) };
        Assert.Null(said.Behind!.Versions);
        Assert.Null(said.Behind.Days);
        Assert.Null(DeliveryText.Behind(said.Behind, "uat", "tdd"));
        Assert.Null(DeliveryText.Behind(entry.Behind, "tdd", "tdd"));
        Assert.NotEqual(Bare(), said);
        Assert.NotEqual(entry, said);
        Assert.Equal(said, Bare() with { Behind = new BehindFirst(null, null) });
    }

    // ----- the two counts, at the ends they run to

    /// <summary>The two counts of the last seven days stand where the file left them, at the ends a count runs to:
    /// the none deployed, the none failed among them, the one deployment of which none failed, the largest a count
    /// holds, and a count below nothing or with more failures than deployments - which no file gives and no word of
    /// the page hides. The refusal of a negative count is the reading's, pinned there; the entry keeps what it is
    /// given.</summary>
    [Fact]
    public void TheTwoCountsStandWhereTheFileLeftThem()
    {
        var none = Bare() with { DeploymentsLast7Days = 0, FailedLast7Days = 0 };
        Assert.Equal(0, none.DeploymentsLast7Days);
        Assert.Equal(0, none.FailedLast7Days);
        Assert.Equal("0 deployments", DeliveryText.Frequency(none));

        var one = Bare() with { DeploymentsLast7Days = 1, FailedLast7Days = 0 };
        Assert.Equal("1 deployment, none failed", DeliveryText.Frequency(one));

        var all = Bare() with { DeploymentsLast7Days = int.MaxValue, FailedLast7Days = int.MaxValue };
        Assert.Equal(int.MaxValue, all.DeploymentsLast7Days);
        Assert.Equal(int.MaxValue, all.FailedLast7Days);
        Assert.Equal("2147483647 deployments, 2147483647 failed", DeliveryText.Frequency(all));

        var impossible = Bare() with { DeploymentsLast7Days = -1, FailedLast7Days = 9 };
        Assert.Equal(-1, impossible.DeploymentsLast7Days);
        Assert.Equal(9, impossible.FailedLast7Days);
        Assert.Equal("-1 deployments, 9 failed", DeliveryText.Frequency(impossible));

        Assert.Null(Bare().DeploymentsLast7Days);
        Assert.Null(Bare().FailedLast7Days);
        Assert.Null(DeliveryText.Frequency(Bare()));
        Assert.NotEqual(none, Bare());
    }

    // ----- the address of the release, held as it is given

    /// <summary>The address is held as the very instance it was given, and the entry asks nothing of it: a page the
    /// reading would refuse, because a topology names absolute http and https addresses only, is kept here all the
    /// same, since it is the reading that decides what may be linked and not the entry. Two addresses written the
    /// same are one address, however many instances carry them, and one written otherwise is another entry.</summary>
    [Fact]
    public void TheReleaseAddressIsHeldAndNeverAsked()
    {
        var url = new Uri("https://octopus.example.net/r/2.4.14");
        var entry = Bare() with { ReleaseUrl = url };

        Assert.Same(url, entry.ReleaseUrl);
        Assert.True(entry.ReleaseUrl!.IsAbsoluteUri);
        Assert.Equal(new Uri("https://octopus.example.net/r/2.4.14"), entry.ReleaseUrl);
        Assert.Equal(entry, Bare() with { ReleaseUrl = new Uri("https://octopus.example.net/r/2.4.14") });
        Assert.Equal(entry.GetHashCode(), (Bare() with { ReleaseUrl = new Uri("https://octopus.example.net/r/2.4.14") }).GetHashCode());

        var relative = Bare() with { ReleaseUrl = new Uri("releases/1.0.30", UriKind.Relative) };
        Assert.False(relative.ReleaseUrl!.IsAbsoluteUri);
        Assert.Equal("releases/1.0.30", relative.ReleaseUrl!.ToString());
        Assert.NotEqual(entry, relative);
        Assert.NotEqual(Bare(), relative);

        Assert.NotEqual(entry, Bare() with { ReleaseUrl = new Uri("https://octopus.example.net/r/2.4.15") });
        Assert.NotEqual(entry, Bare());
    }

    // ----- the sign-off by a person, the part the entry may do without

    /// <summary>The sign-off is the part the entry does without: the entry built without it is the entry built with
    /// the one that is none, one entry and one hash, and it is no entry that names a person. The newest deployment
    /// a person signed off - which the card keeps while the automation signs the shown one off - is a part the
    /// entry either holds as the record it stands on or does not hold at all.</summary>
    [Fact]
    public void TheEntryWithoutASignOffIsTheOneItsSignOffIsNoneOn()
    {
        var without = Bare();
        var withNone = new DeliveryEntry("ui", null, null, null, null, null, null, null, null, null, null, null, null);
        var withOne = Bare() with { LastPersonSignOff = new PersonSignOff("2.4.11", "pat.morgan", null, null, null) };

        Assert.Null(without.LastPersonSignOff);
        Assert.Equal(without, withNone);
        Assert.True(without == withNone);
        Assert.False(without != withNone);
        Assert.Equal(without.GetHashCode(), withNone.GetHashCode());
        Assert.NotEqual(without, withOne);
        Assert.NotEqual(withOne, withOne with { LastPersonSignOff = withOne.LastPersonSignOff with { By = "ami-robot" } });
        Assert.NotEqual(withOne, withOne with { LastPersonSignOff = null });

        var automated = Bare() with { SignedOffBy = "cm-ai-ops", Reason = "tdd applied" };
        Assert.Equal("cm-ai-ops", automated.SignedOffBy);
        Assert.Equal("tdd applied", automated.Reason);
        Assert.Null(automated.LastPersonSignOff);
        Assert.NotEqual(automated, withOne);
    }

    // ----- the thirteen parts coming back out, in the order they went in

    /// <summary>The thirteen parts of an entry fall out in the order the constructor names them - the name, the
    /// version, the moment it ended, who signed it off and why, the commit and its moment, the hours between them,
    /// the distance behind, the two counts, the release and the newest sign-off by a person - as the very instances
    /// they were, and what came out goes back in the same way. The entry that is only a name falls apart to the
    /// twelve parts a file leaves out.</summary>
    [Fact]
    public void TheEntryFallsApartInTheOrderItWasMade()
    {
        var behind = new BehindFirst(2, 3.4);
        var url = new Uri("https://octopus.example.net/r/2.4.14");
        var signOff = new PersonSignOff("2.4.11", "pat.morgan", null, null, null);
        var entry = new DeliveryEntry(
            "ui", "2.4.14", Deployed, "cm-ai-ops", "Acceptance tests passed in tdd", "9f8e7d6c5b4a", Committed,
            5.2, behind, 4, 0, url, signOff);

        var (name, version, deployedAt, signedOffBy, reason, commit, commitAt, leadTime, outBehind, deployments, failed, releaseUrl, lastPersonSignOff) = entry;

        Assert.Equal("ui", name);
        Assert.Equal("2.4.14", version);
        Assert.Equal(Deployed, deployedAt);
        Assert.Equal("cm-ai-ops", signedOffBy);
        Assert.Equal("Acceptance tests passed in tdd", reason);
        Assert.Equal("9f8e7d6c5b4a", commit);
        Assert.Equal(Committed, commitAt);
        Assert.Equal(5.2, leadTime);
        Assert.Same(behind, outBehind);
        Assert.Equal(4, deployments);
        Assert.Equal(0, failed);
        Assert.Same(url, releaseUrl);
        Assert.Same(signOff, lastPersonSignOff);
        Assert.Equal(entry, new DeliveryEntry(
            name, version, deployedAt, signedOffBy, reason, commit, commitAt, leadTime, outBehind, deployments,
            failed, releaseUrl, lastPersonSignOff));

        var (bareName, bareVersion, bareDeployedAt, bareSignedOffBy, bareReason, bareCommit, bareCommitAt, bareLeadTime, bareBehind, bareDeployments, bareFailed, bareReleaseUrl, bareSignOff) = Bare();

        Assert.Equal("ui", bareName);
        Assert.Null(bareVersion);
        Assert.Null(bareDeployedAt);
        Assert.Null(bareSignedOffBy);
        Assert.Null(bareReason);
        Assert.Null(bareCommit);
        Assert.Null(bareCommitAt);
        Assert.Null(bareLeadTime);
        Assert.Null(bareBehind);
        Assert.Null(bareDeployments);
        Assert.Null(bareFailed);
        Assert.Null(bareReleaseUrl);
        Assert.Null(bareSignOff);
        Assert.Equal(Bare(), new DeliveryEntry(
            bareName, bareVersion, bareDeployedAt, bareSignedOffBy, bareReason, bareCommit, bareCommitAt,
            bareLeadTime, bareBehind, bareDeployments, bareFailed, bareReleaseUrl, bareSignOff));
    }

    // ----- the equality, the hash, the two

    /// <summary>Two entries are one while their thirteen parts are - each part by its own equality, the name with
    /// the case it is written in, the distance and the sign-off as the records they stand in and the address as the
    /// address it is, down to the parts no card ever shows - and the hash follows the equality, boxed or unboxed and
    /// from one call to the next, so that the entries that are one are one entry of a set and a lookup meets
    /// them.</summary>
    [Fact]
    public void TwoEntriesAreOneWhileTheirThirteenPartsAre()
    {
        var one = Full();
        var two = Full();

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
        Assert.Single(new HashSet<DeliveryEntry> { one, two });
        Assert.Contains(one, new HashSet<DeliveryEntry> { two });
    }

    /// <summary>A difference in one part is another entry, and so is the same part in another place: the name in its
    /// case, which the lookups of a report are not, a version with its build metadata on, a moment a second later,
    /// a person for the one who signed off, no reason at all, a commit of another deployment, a tenth of an hour,
    /// the distance with one version more, one deployment more, the failure the other does not name, an address of
    /// another release, and a sign-off with no note. A set of the entries that differ in one part each holds them
    /// all apart.</summary>
    [Fact]
    public void AnEntryThatDiffersInOnePartIsAnotherEntry()
    {
        var one = Full();

        Assert.NotEqual(one, one with { Name = "UI" });
        Assert.NotEqual(one, one with { Name = string.Empty });
        Assert.NotEqual(one, one with { Version = "2.4.14+abc123" });
        Assert.NotEqual(one, one with { Version = null });
        Assert.NotEqual(one, one with { DeployedAt = Deployed.AddSeconds(1) });
        Assert.NotEqual(one, one with { DeployedAt = null });
        Assert.NotEqual(one, one with { SignedOffBy = "pat.morgan" });
        Assert.NotEqual(one, one with { SignedOffBy = null });
        Assert.NotEqual(one, one with { Reason = null });
        Assert.NotEqual(one, one with { Commit = "0a1b2c3d4e5f" });
        Assert.NotEqual(one, one with { CommitAt = Committed.AddMilliseconds(1) });
        Assert.NotEqual(one, one with { LeadTimeHours = 5.21 });
        Assert.NotEqual(one, one with { Behind = new BehindFirst(3, 3.4) });
        Assert.NotEqual(one, one with { Behind = null });
        Assert.NotEqual(one, one with { DeploymentsLast7Days = 5 });
        Assert.NotEqual(one, one with { DeploymentsLast7Days = null });
        Assert.NotEqual(one, one with { FailedLast7Days = 1 });
        Assert.NotEqual(one, one with { FailedLast7Days = null });
        Assert.NotEqual(one, one with { ReleaseUrl = new Uri("https://octopus.example.net/r/2.4.11") });
        Assert.NotEqual(one, one with { LastPersonSignOff = null });
        var signOff = one.LastPersonSignOff!;
        Assert.NotEqual(one, one with { LastPersonSignOff = signOff with { Reason = null } });

        // The same thirteen parts in another order are thirteen other parts: a name is a name and no version.
        Assert.NotEqual(one, new DeliveryEntry(
            "2.4.14", "ui", Committed, "cm-ai-ops", "Acceptance tests passed in tdd", "9f8e7d6c5b4a", Deployed,
            5.2, new BehindFirst(2, 3.4), 4, 0, new Uri("https://octopus.example.net/r/2.4.14"), one.LastPersonSignOff));

        Assert.Equal(6, new HashSet<DeliveryEntry>
        {
            one,
            one with { Name = "UI" },
            one with { DeployedAt = Deployed.AddSeconds(1) },
            one with { LeadTimeHours = 5.21 },
            one with { FailedLast7Days = 1 },
            one with { LastPersonSignOff = null },
        }.Count);
    }

    /// <summary>The moment an entry stands on is the instant and not the hours it is spelled in: the entry of the
    /// deployment at <c>16:37 +00:00</c> and the entry of that very instant written as <c>22:07 +05:30</c> are one
    /// entry, of one hash and one of a set - while the moment one second later, whichever hours it is spelled in, is
    /// another entry. The hours the file wrote are kept for whoever prints the entry; the equality of the moment is
    /// what the entry compares, so that the same reading of one deployment in two time zones is one card.</summary>
    [Fact]
    public void TheSameInstantSpelledInAnotherHourIsTheSameEntry()
    {
        var utc = Bare() with { DeployedAt = Deployed };
        var east = Bare() with { DeployedAt = Deployed.ToOffset(East) };

        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 7, 0, East), east.DeployedAt);
        Assert.Equal(Deployed, east.DeployedAt!.Value.ToUniversalTime());
        Assert.True(utc.Equals(east));
        Assert.True(utc.Equals((object)east));
        Assert.True(utc == east);
        Assert.False(utc != east);
        Assert.Equal(utc.GetHashCode(), east.GetHashCode());
        Assert.Single(new HashSet<DeliveryEntry> { utc, east });
        Assert.Equal("5 h ago", TimeText.Ago(utc.DeployedAt!.Value, Now));
        Assert.Equal("5 h ago", TimeText.Ago(east.DeployedAt!.Value, Now));

        var aSecondLater = Bare() with { DeployedAt = Deployed.AddSeconds(1).ToOffset(East) };
        Assert.NotEqual(utc, aSecondLater);
        Assert.True(utc != aSecondLater);
        Assert.Equal(2, new HashSet<DeliveryEntry> { utc, aSecondLater }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two entries that are no entry
    /// at all are one, an entry and no entry are two - the equality and the inequality each saying it, from either
    /// side - and the entry asked about itself says no to the things it is not: a word, a moment, a distance, a
    /// sign-off, an address, another entry of the same name and the anonymous thing that happens to carry two of its
    /// parts.</summary>
    [Fact]
    public void NoEntryIsOneWithNoEntryAndNotWithAnyOtherThing()
    {
        DeliveryEntry? nothing = null;
        DeliveryEntry? alsoNothing = null;
        var there = Bare();

        Assert.Null(default(DeliveryEntry));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (DeliveryEntry?)null);
        Assert.True(there != (DeliveryEntry?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("ui"));
        Assert.False(there.Equals(Deployed));
        Assert.False(there.Equals(new BehindFirst(null, null)));
        Assert.False(there.Equals(new PersonSignOff(null, null, null, null, null)));
        Assert.False(there.Equals(new Uri("https://octopus.example.net/r/2.4.14")));
        Assert.False(there.Equals(new { Name = "ui", Version = (string?)null }));
        Assert.False(there.Equals(Bare() with { Version = " " }));
        Assert.False(there.Equals(new DeliveryEnvironment("ui", [there])));
    }

    // ----- the copy, turning the part it names

    /// <summary>Any of the thirteen parts may be turned as the entry is copied - the name to another name, the two
    /// moments to none, the counts to another pair, the sign-off to another person's - and stands where it is turned
    /// to, while the twelve the turn does not name stay the very instances the entry was built of. The copy is
    /// another entry and never the same reference, and the original stands as it stood.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherTwelve()
    {
        var behind = new BehindFirst(2, 3.4);
        var url = new Uri("https://octopus.example.net/r/2.4.14");
        var signOff = Full().LastPersonSignOff!;
        var entry = new DeliveryEntry(
            "ui", "2.4.14", Deployed, "cm-ai-ops", "Acceptance tests passed in tdd", "9f8e7d6c5b4a", Committed,
            5.2, behind, 4, 0, url, signOff);

        var renamed = entry with { Name = "system" };
        Assert.Equal("system", renamed.Name);
        Assert.Same(behind, renamed.Behind);
        Assert.Same(url, renamed.ReleaseUrl);
        Assert.Same(signOff, renamed.LastPersonSignOff);
        Assert.NotSame(entry, renamed);
        Assert.NotEqual(entry, renamed);
        Assert.True(DeliveryReport.IsSystem(renamed.Name));
        Assert.False(DeliveryReport.IsSystem(entry.Name));

        var undated = entry with { DeployedAt = null, CommitAt = null };
        Assert.Null(undated.DeployedAt);
        Assert.Null(undated.CommitAt);
        Assert.Equal("ui", undated.Name);
        Assert.Equal("2.4.14", undated.Version);
        Assert.Equal("cm-ai-ops", undated.SignedOffBy);
        Assert.Same(behind, undated.Behind);

        var countsTurned = entry with { DeploymentsLast7Days = 9, FailedLast7Days = 1 };
        Assert.Equal("9 deployments, 1 failed", DeliveryText.Frequency(countsTurned));
        Assert.Equal(5.2, countsTurned.LeadTimeHours);
        Assert.NotEqual(entry, countsTurned);

        var signOffTurned = entry with { LastPersonSignOff = signOff with { By = "ami-robot" } };
        Assert.Equal("ami-robot", signOffTurned.LastPersonSignOff!.By);
        Assert.Equal("2.4.11", signOffTurned.LastPersonSignOff.Version);
        Assert.Equal("pat.morgan", entry.LastPersonSignOff!.By);

        var unchanged = entry with { };
        Assert.Equal(entry, unchanged);
        Assert.NotSame(entry, unchanged);
        Assert.Equal(entry.GetHashCode(), unchanged.GetHashCode());
        Assert.Same(behind, unchanged.Behind);

        Assert.Equal("ui", entry.Name);
        Assert.Equal("2.4.14", entry.Version);
        Assert.Equal(Deployed, entry.DeployedAt);
        Assert.Same(behind, entry.Behind);
    }

    // ----- the words the record says itself

    /// <summary>The words the entry says its thirteen parts in: its own name at the front, the parts in the order
    /// the constructor names them, the moments as the clock of whoever reads them says them, the distance and the
    /// sign-off as the records they stand in, and the address as the address it is - the part nobody said being the
    /// empty place between the mark and the comma or the closing brace. These are the record's words and not the
    /// page's: no dash, no unit, no "behind" and no "ago", which are the words the page says of the parts it is
    /// shown.</summary>
    [Fact]
    public void TheEntrySaysItsThirteenPartsInTheOrderTheyAreNamed()
    {
        const string Spoken =
            "DeliveryEntry { Name = ui, Version = 2.4.14, DeployedAt = 10/04/2026 16:37:00 +00:00, "
                + "SignedOffBy = cm-ai-ops, Reason = Acceptance tests passed in tdd, Commit = 9f8e7d6c5b4a, "
                + "CommitAt = 10/04/2026 11:25:00 +00:00, LeadTimeHours = 5.2, "
                + "Behind = BehindFirst { Versions = 2, Days = 3.4 }, DeploymentsLast7Days = 4, FailedLast7Days = 0, "
                + "ReleaseUrl = https://octopus.example.net/r/2.4.14, "
                + "LastPersonSignOff = PersonSignOff { Version = 2.4.11, By = pat.morgan, At = 10/01/2026 15:12:41 +00:00, "
                + "Reason = Reviewed with the customer, ReleaseUrl = https://octopus.example.net/r/2.4.11 } }";

        Assert.Equal(Spoken, Words(Full()));
        Assert.StartsWith("DeliveryEntry { Name = ui, ", Words(Full()), StringComparison.Ordinal);
        Assert.EndsWith("ReleaseUrl = https://octopus.example.net/r/2.4.11 } }", Words(Full()), StringComparison.Ordinal);

        // The parts a file leaves out are the empty places, and the words say nothing the entry does not hold.
        Assert.Equal(
            "DeliveryEntry { Name =  ui , Version =  , DeployedAt = , SignedOffBy = , Reason = , Commit = , "
                + "CommitAt = , LeadTimeHours = , Behind = , DeploymentsLast7Days = , FailedLast7Days = , "
                + "ReleaseUrl = , LastPersonSignOff =  }",
            Words(new DeliveryEntry(" ui ", " ", null, null, null, null, null, null, null, null, null, null)));

        foreach (var word in new[] { "—", "%", " ago", " behind", " min", " deployments", " healthy", " same as" })
        {
            Assert.DoesNotContain(word, Words(Full()), StringComparison.Ordinal);
        }
    }

    /// <summary>The numbers and the moments of these words follow whoever reads them, which is what sets the
    /// record's own words apart from the page's: a reader of German is told <c>5,2</c> and <c>04.10.2026</c> by the
    /// entry and <c>5.2 h</c> and <c>2026-10-04 16:37 +00:00</c> by <see cref="DeliveryText.LeadTime" /> and
    /// <see cref="TimeText" />, whose words are written for the dashboard and not for whoever looks.</summary>
    [Fact]
    public void TheEntryOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        var ui = Full();

        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "DeliveryEntry { Name = ui, Version = 2.4.14, DeployedAt = 04.10.2026 16:37:00 +00:00, "
                    + "SignedOffBy = cm-ai-ops, Reason = Acceptance tests passed in tdd, Commit = 9f8e7d6c5b4a, "
                    + "CommitAt = 04.10.2026 11:25:00 +00:00, LeadTimeHours = 5,2, "
                    + "Behind = BehindFirst { Versions = 2, Days = 3,4 }, DeploymentsLast7Days = 4, FailedLast7Days = 0, "
                    + "ReleaseUrl = https://octopus.example.net/r/2.4.14, "
                    + "LastPersonSignOff = PersonSignOff { Version = 2.4.11, By = pat.morgan, At = 01.10.2026 15:12:41 +00:00, "
                    + "Reason = Reviewed with the customer, ReleaseUrl = https://octopus.example.net/r/2.4.11 } }",
                ui.ToString());
            Assert.Equal("5.2 h", DeliveryText.LeadTime(ui.LeadTimeHours!.Value));
            Assert.Equal("4 deployments, none failed", DeliveryText.Frequency(ui));
            Assert.Equal("2026-10-04 16:37 +00:00", TimeText.DateAndTime(ui.DeployedAt!.Value, TimeZoneInfo.Utc));
        });

        Assert.DoesNotContain("5,2", Words(ui), StringComparison.Ordinal);
        Assert.Contains("LeadTimeHours = 5.2", Words(ui), StringComparison.Ordinal);
    }

    // ----- the record's own shape

    /// <summary>The entry's whole public surface, as it stands: its thirteen parts, named in the order the
    /// constructor names them and of the type the constructor says, the entry falling apart into them, and the
    /// members every record is asked - comparing, hashing and saying itself - and nothing else to call. A fourteenth
    /// part added to the entry has to be written here as well, which is the point: the page cannot be shown a part
    /// nobody decided what it means.</summary>
    [Fact]
    public void TheEntryHasThirteenPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Name", "Version", "DeployedAt", "SignedOffBy", "Reason", "Commit", "CommitAt", "LeadTimeHours", "Behind", "DeploymentsLast7Days", "FailedLast7Days", "ReleaseUrl", "LastPersonSignOff"],
            [.. typeof(DeliveryEntry)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Behind", "Commit", "CommitAt", "Deconstruct", "DeployedAt", "DeploymentsLast7Days", "Equals", "FailedLast7Days", "GetHashCode", "LastPersonSignOff", "LeadTimeHours", "Name", "Reason", "ReleaseUrl", "SignedOffBy", "ToString", "Version", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal([typeof(IEquatable<DeliveryEntry>)], typeof(DeliveryEntry).GetInterfaces());

        var properties = typeof(DeliveryEntry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(string), properties["Version"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["DeployedAt"].PropertyType);
        Assert.Equal(typeof(string), properties["SignedOffBy"].PropertyType);
        Assert.Equal(typeof(string), properties["Reason"].PropertyType);
        Assert.Equal(typeof(string), properties["Commit"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["CommitAt"].PropertyType);
        Assert.Equal(typeof(double?), properties["LeadTimeHours"].PropertyType);
        Assert.Equal(typeof(BehindFirst), properties["Behind"].PropertyType);
        Assert.Equal(typeof(int?), properties["DeploymentsLast7Days"].PropertyType);
        Assert.Equal(typeof(int?), properties["FailedLast7Days"].PropertyType);
        Assert.Equal(typeof(Uri), properties["ReleaseUrl"].PropertyType);
        Assert.Equal(typeof(PersonSignOff), properties["LastPersonSignOff"].PropertyType);

        var arguments = typeof(DeliveryEntry).GetConstructors().Single().GetParameters();

        Assert.Equal(13, arguments.Length);
        Assert.False(arguments[0].HasDefaultValue);
        Assert.All(arguments.Skip(1).Take(11), argument => Assert.False(argument.HasDefaultValue));
        Assert.True(arguments[12].HasDefaultValue);
        Assert.Null(arguments[12].DefaultValue);
    }

    /// <summary>The public members the entry declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(DeliveryEntry)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words an entry says its thirteen parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own further
    /// up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(DeliveryEntry entry) => ViewerCulture.Invariant(() => entry.ToString()!);
}
