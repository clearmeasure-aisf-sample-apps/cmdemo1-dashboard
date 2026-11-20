using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RecentDeployment" /> is one entry of the deployments file's <c>recent</c>: the last deployment of one
/// project to one environment that ended, in eight parts - the project, the environment, the release, the result as
/// the page knows it and as the file wrote it, the moment it ended, who started it, and the task in Octopus Deploy.
/// <para>
/// What a file makes of an entry is pinned where the reading happens and is not pinned again here:
/// <see cref="DeploymentActivityTests" /> reads the sample's <c>recent</c> - an entry of it falling out of the eight
/// parts as the file writes them, the entry a file gives no <c>startedBy</c> for standing with nobody named, an entry
/// that is a word rather than an object and an entry without its five necessary parts being left out of the list
/// altogether, the newest entry first however the file orders them - and pins the words the page makes of every part
/// (<see cref="DeploymentActivity.OfNode" />, <see cref="DeploymentActivity.OfEnvironment" />). The app declares no
/// <c>InternalsVisibleTo</c>, so the internal <c>Read</c> is pinned through <see cref="DeploymentsReport.Parse" />
/// there rather than here.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself, which all of that walks past: the constructor handing each
/// of its eight arguments to the property of its name and holding the very <see cref="Uri" /> it was built of; the
/// five words kept exactly as they are given - empty, padded with whitespace, in their case, trimmed nowhere, and
/// given as nothing where a part is required; the result as the state it is, including the three a recent entry never
/// takes, since it is the reading that settles an entry on <see cref="DeploymentState.Unknown" /> and not the record;
/// the moment it ended kept as the instant and the hours it is written in, the two ends of the clock and the fraction
/// of a second among them; the two parts an entry may do without being nothing rather than the empty word, and the
/// address held, never asked, and compared as an address is compared - one task marked at another place being one
/// entry and another path being another; the eight parts coming back out in the order they went in; two entries being one
/// while their eight parts are - one instant spelled in another hour being one entry, its hours kept for whoever
/// prints it - the hash following the equality, and the nothing on either side of the members every record is asked
/// with; the copy that turns the part it names and keeps the other seven; the words the record says its parts in,
/// following whoever reads them; and the public surface, so that the day a ninth part is added the list says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the entry holds a moment and an address
/// but calls nothing of either - every moment below is one the test brought, and the address is held and never
/// fetched. What a moment means to the page - how long ago, and in whose hours - is
/// <see cref="DeploymentText" /> and <see cref="TimeText" />, and is pinned on those words.
/// </para>
/// </summary>
public class RecentDeploymentEdgeCaseTests
{
    /// <summary>The moment the sample's <c>demo-ui 2.4.43</c> reached <c>uat</c>: Friday seven in the morning, UTC.</summary>
    private static readonly DateTimeOffset Finished = new(2026, 10, 9, 7, 0, 0, TimeSpan.Zero);

    /// <summary>The hours five and a half east of the meridian, to spell one instant the other way round.</summary>
    private static readonly TimeSpan East = TimeSpan.FromMinutes(330);

    private const string Tasks = "https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-";

    /// <summary>The second entry of the shipped sample, part for part, brought here without reading the file.</summary>
    private static RecentDeployment Full() =>
        new("demo-ui", "uat", "2.4.43", DeploymentState.Succeeded, "succeeded", Finished, "ai-ops", new Uri($"{Tasks}2"));

    /// <summary>An entry that names nobody and nowhere: what a file must give, with a result the page does not know.</summary>
    private static RecentDeployment Bare() =>
        new("demo-ui", "uat", "2.4.43", DeploymentState.Unknown, "timed out", Finished, null, null);

    // ----- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's eight arguments, position by position, come back as the property of the same name,
    /// and the entry holds the very address it was built of. An entry built again of the same eight parts is the same
    /// entry and no longer the same reference - which is what lets a test name an entry read from a file by writing
    /// it out, as <see cref="DeploymentActivityTests" /> does.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var url = new Uri($"{Tasks}2");
        var entry = new RecentDeployment("demo-ui", "uat", "2.4.43", DeploymentState.Failed, "failed", Finished, "ai-ops", url);

        Assert.Equal("demo-ui", entry.Project);
        Assert.Equal("uat", entry.Environment);
        Assert.Equal("2.4.43", entry.Release);
        Assert.Equal(DeploymentState.Failed, entry.Result);
        Assert.Equal("failed", entry.ResultText);
        Assert.Equal(Finished, entry.Finished);
        Assert.Equal("ai-ops", entry.StartedBy);
        Assert.Same(url, entry.Url);
        Assert.Equal($"{Tasks}2", entry.Url!.AbsoluteUri);

        // The word the file wrote and the state the page makes of it are two parts, and one does not follow the other.
        Assert.NotEqual(entry.ResultText, entry.Result.ToString());

        var again = new RecentDeployment("demo-ui", "uat", "2.4.43", DeploymentState.Failed, "failed", Finished, "ai-ops", url);

        Assert.Equal(entry, again);
        Assert.NotSame(entry, again);
        Assert.Equal(entry.GetHashCode(), again.GetHashCode());
    }

    // ----- the five words, kept exactly as they are given

    /// <summary>Every word an entry carries - the project, the environment, the release, the result in the file's own
    /// word and who started it - is kept as letters and nothing more: the empty word is said and not left unsaid, and
    /// a word of nothing but whitespace is a word of its own.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(" demo-ui ")]
    [InlineData("DEMO-UI")]
    [InlineData("demo-ui")]
    public void EachWordIsKeptExactlyAsItIsGiven(string word)
    {
        var entry = new RecentDeployment(word, word, word, DeploymentState.Succeeded, word, Finished, word, null);

        Assert.Equal(word, entry.Project);
        Assert.Equal(word, entry.Environment);
        Assert.Equal(word, entry.Release);
        Assert.Equal(word, entry.ResultText);
        Assert.Equal(word, entry.StartedBy);

        var same = new RecentDeployment(word, word, word, DeploymentState.Succeeded, word, Finished, word, null);

        Assert.Equal(entry, same);
        Assert.Equal(entry.GetHashCode(), same.GetHashCode());
        Assert.True(entry == same);
        Assert.False(entry != same);
    }

    /// <summary>Trimming, casing and leaving a word out are the reading's and not the entry's: an entry padded is no
    /// entry trimmed, one in another case is no entry in this, and a word said empty is a word said - which is the
    /// difference between a file that names an empty <c>startedBy</c> and one that names nobody, and why the page
    /// compares a project's and an environment's name by ignoring case while the entry keeps the letters it is given.</summary>
    [Fact]
    public void TheEntryTrimsNothingAndCasesNothing()
    {
        var padded = new RecentDeployment(" demo-ui ", " uat ", " 2.4.43 ", DeploymentState.Succeeded, " Succeeded ", Finished, " ai-ops ", null);

        Assert.NotEqual(padded, padded with { Project = "demo-ui" });
        Assert.NotEqual(padded, padded with { Environment = "uat" });
        Assert.NotEqual(padded, padded with { Release = "2.4.43" });
        Assert.NotEqual(padded, padded with { ResultText = " Succeeded" });
        Assert.NotEqual(padded, padded with { StartedBy = "ai-ops " });
        Assert.Equal(padded, padded with { Project = " demo-ui " });

        var cased = Full() with { ResultText = "SUCCEEDED" };
        Assert.NotEqual(Full(), cased);
        Assert.Equal(DeploymentState.Succeeded, cased.Result);

        // A word said empty stands; the entry that says nobody is another entry.
        var empty = Full() with { StartedBy = string.Empty };
        Assert.Equal(string.Empty, empty.StartedBy);
        Assert.NotEqual(empty, Full() with { StartedBy = null });
        Assert.NotEqual(empty, Full());

        // And the record asks nothing of its own parts: the five a file must give may still be given as nothing,
        // since it is the reading that leaves such an entry out, not the entry.
        var nothing = new RecentDeployment(null!, null!, null!, DeploymentState.Unknown, null!, Finished, null, null);

        Assert.Null(nothing.Project);
        Assert.Null(nothing.Environment);
        Assert.Null(nothing.Release);
        Assert.Null(nothing.ResultText);
        Assert.Equal(DeploymentState.Unknown, nothing.Result);
        Assert.NotEqual(Bare(), nothing);
    }

    // ----- the result, at the ends the state runs to

    /// <summary>The result is the state it is given, whichever of the seven it is - including the three a recent entry
    /// never takes, since a deployment that is still <c>executing</c> has not ended: it is the reading that settles an
    /// entry on <see cref="DeploymentState.Unknown" /> and not the record, which keeps what it is handed.</summary>
    [Theory]
    [InlineData(DeploymentState.Unknown)]
    [InlineData(DeploymentState.Queued)]
    [InlineData(DeploymentState.Executing)]
    [InlineData(DeploymentState.Waiting)]
    [InlineData(DeploymentState.Succeeded)]
    [InlineData(DeploymentState.Failed)]
    [InlineData(DeploymentState.Canceled)]
    public void TheResultStandsAsTheStateItIsGiven(DeploymentState state)
    {
        var entry = Bare() with { Result = state };

        Assert.Equal(state, entry.Result);
        Assert.Equal(entry, Bare() with { Result = state });
        Assert.Equal(entry.GetHashCode(), (Bare() with { Result = state }).GetHashCode());
        Assert.True(entry == Bare() with { Result = state });
        Assert.Equal(entry with { }, entry);

        // The word the file wrote is a part of its own and stays as it was given with it.
        Assert.Equal("timed out", entry.ResultText);
    }

    /// <summary>The seven states are seven entries and the record does not stop at the last of them: a result beyond
    /// every word <see cref="Deployment" /> knows is kept as the number it is, said as that number, and told apart
    /// from its neighbours - the page's words for a result it does not know are the file's own word, pinned there.</summary>
    [Fact]
    public void EveryStateIsADifferentEntry()
    {
        var states = new List<DeploymentState>(Enum.GetValues<DeploymentState>()) { (DeploymentState)7 };
        var entries = states.Select(state => Full() with { Result = state }).ToArray();

        Assert.Equal(8, states.Count);
        Assert.Equal(8, entries.Length);
        Assert.Equal(8, new HashSet<RecentDeployment>(entries).Count);

        for (var first = 0; first < entries.Length; first++)
        {
            for (var second = first + 1; second < entries.Length; second++)
            {
                Assert.NotEqual(entries[first], entries[second]);
                Assert.True(entries[first] != entries[second]);
            }
        }

        var beyond = Full() with { Result = (DeploymentState)7 };
        Assert.Equal((DeploymentState)7, beyond.Result);
        Assert.False(Enum.IsDefined(beyond.Result));
        Assert.Contains("Result = 7", Words(beyond), StringComparison.Ordinal);
        Assert.NotEqual(Full() with { Result = DeploymentState.Unknown }, beyond);
    }

    // ----- the moment it ended, kept as the instant and the hours it is written in

    /// <summary>The moment is kept whole, at the ends the clock runs to and in the hours it was spelled in: neither
    /// end is the other, the fraction of a second is no rounding, and the hours the file wrote stand while the
    /// instant behind them is what two entries compare.</summary>
    [Fact]
    public void TheMomentIsKeptAtTheEndsTheClockRunsTo()
    {
        var first = Full() with { Finished = DateTimeOffset.MinValue };
        var last = Full() with { Finished = DateTimeOffset.MaxValue };

        Assert.Equal(DateTimeOffset.MinValue, first.Finished);
        Assert.Equal(DateTimeOffset.MaxValue, last.Finished);
        Assert.NotEqual(first, last);
        Assert.True(first.Finished < Finished && last.Finished > Finished);

        var spelled = Full() with { Finished = new DateTimeOffset(2026, 10, 9, 12, 30, 0, East) };
        Assert.Equal(East, spelled.Finished.Offset);
        Assert.Equal(Finished, spelled.Finished.ToUniversalTime());
        Assert.NotEqual(Finished.DateTime, spelled.Finished.DateTime);
        Assert.True(Finished == spelled.Finished);

        var exact = Full() with { Finished = new DateTimeOffset(2026, 10, 9, 7, 0, 0, 123, TimeSpan.Zero) };
        Assert.Equal(123, exact.Finished.Millisecond);
        Assert.NotEqual(exact, Full());
        Assert.NotEqual(exact.Finished, Full().Finished);

        // A moment a second on is another entry, and the order the report puts its entries in stands on this part.
        var later = Full() with { Finished = Finished.AddSeconds(1) };
        var laterStill = Full() with { Finished = Finished.AddSeconds(2) };

        Assert.NotEqual(Full(), later);
        Assert.True(Full().Finished < later.Finished && later.Finished < laterStill.Finished);
        Assert.Equal(3, new HashSet<RecentDeployment> { Full(), laterStill, later }.Count);
    }

    /// <summary>The moment an entry stands on is the instant and not the hours it is spelled in: the entry of the
    /// deployment that ended at <c>07:00 +00:00</c> and the entry of that very instant written as <c>12:30 +05:30</c>
    /// are one entry, of one hash and one of a set - while the words the entry says itself keep the hours it was
    /// spelled in, since those are the file's letters and not a comparison.</summary>
    [Fact]
    public void TheSameInstantSpelledInAnotherHourIsTheSameEntry()
    {
        var utc = Full();
        var east = Full() with { Finished = Finished.ToOffset(East) };

        Assert.Equal(new DateTimeOffset(2026, 10, 9, 12, 30, 0, East), east.Finished);
        Assert.True(utc.Equals(east));
        Assert.True(utc.Equals((object)east));
        Assert.True(utc == east);
        Assert.False(utc != east);
        Assert.Equal(utc.GetHashCode(), east.GetHashCode());
        Assert.Single(new HashSet<RecentDeployment> { utc, east });

        Assert.Contains("Finished = 10/09/2026 07:00:00 +00:00", Words(utc), StringComparison.Ordinal);
        Assert.Contains("Finished = 10/09/2026 12:30:00 +05:30", Words(east), StringComparison.Ordinal);

        var aSecondLater = Full() with { Finished = Finished.AddSeconds(1).ToOffset(East) };
        Assert.NotEqual(utc, aSecondLater);
        Assert.True(utc != aSecondLater);
        Assert.Equal(2, new HashSet<RecentDeployment> { utc, aSecondLater }.Count);
    }

    // ----- the two parts an entry may do without

    /// <summary>The person who started a deployment and the task in Octopus Deploy are the two parts an entry does
    /// without: nothing, and never the empty word or an address of no task - which is why a line of the page links
    /// where the file gives an address and says nothing of who started what it names.</summary>
    [Fact]
    public void TheTwoPartsAnEntryMayDoWithoutAreNothing()
    {
        var without = Bare();
        var withBoth = Full();

        Assert.Null(without.StartedBy);
        Assert.Null(without.Url);
        Assert.Equal(without, new RecentDeployment("demo-ui", "uat", "2.4.43", DeploymentState.Unknown, "timed out", Finished, null, null));
        Assert.Equal(without.GetHashCode(), Bare().GetHashCode());

        Assert.NotEqual(without, without with { StartedBy = string.Empty });
        Assert.NotEqual(without, without with { Url = new Uri($"{Tasks}2") });
        Assert.NotEqual(withBoth, withBoth with { StartedBy = null });
        Assert.NotEqual(withBoth, withBoth with { StartedBy = "jeffrey" });
        Assert.NotEqual(withBoth, withBoth with { Url = null });
    }

    /// <summary>The address is held as the very instance it was given and asked nothing of: no lookup, no request, and
    /// no refusal of an address the reading would never let through - a scheme other than http or https, and an
    /// address that is no address but a path - since it is <c>JsonRead.Address</c> that decides what the page may link
    /// to and not the entry. Two spellings of one task are one address, whatever comes after the mark being a place in
    /// the page rather than another task, and the entry is one entry with them; another path is another address and
    /// another entry. The words the entry says itself keep the mark and what follows it.</summary>
    [Fact]
    public void TheAddressIsHeldAndNeverAsked()
    {
        var url = new Uri($"{Tasks}2");
        var entry = Bare() with { Url = url };

        Assert.Same(url, entry.Url);
        Assert.True(entry.Url!.IsAbsoluteUri);
        Assert.Equal("https", entry.Url.Scheme);
        Assert.Equal(entry, Bare() with { Url = new Uri($"{Tasks}2") });
        Assert.Equal(entry.GetHashCode(), (Bare() with { Url = new Uri($"{Tasks}2") }).GetHashCode());
        Assert.NotEqual(entry, Bare());

        // The same task, marked at another place in it: one address, one entry, one hash - and other words.
        var marked = Bare() with { Url = new Uri($"{Tasks}1") };
        Assert.True(entry == marked);
        Assert.Equal(entry.GetHashCode(), marked.GetHashCode());
        Assert.Single(new HashSet<RecentDeployment> { entry, marked });
        Assert.NotEqual(Words(entry), Words(marked));

        // Another task, in another space of the same instance, is another address and another entry.
        var elsewhere = Bare() with { Url = new Uri("https://octopus.example.net/spaces/356/tasks/ServerTasks-2") };
        Assert.NotEqual(entry, elsewhere);
        Assert.True(entry != elsewhere);

        // An address of a host nothing could ever answer for is held the same way, host and all: holding an address
        // asks nothing of it, and no request is made of any of these.
        var far = Bare() with { Url = new Uri("https://octopus.invalid/app#/task/2") };
        Assert.Equal("octopus.invalid", far.Url!.DnsSafeHost);
        Assert.Contains("Url = https://octopus.invalid/app#/task/2", Words(far), StringComparison.Ordinal);

        // What the page would not link to is a part like any other: the record knows no scheme and no shape.
        var relative = Bare() with { Url = new Uri("ServerTasks-2", UriKind.Relative) };
        Assert.False(relative.Url!.IsAbsoluteUri);
        Assert.NotEqual(entry, relative);
        Assert.NotEqual(Bare(), relative);

        var otherScheme = Bare() with { Url = new Uri("ftp://octopus.example.net/task/2") };
        Assert.Equal("ftp", otherScheme.Url!.Scheme);
        Assert.NotEqual(entry, otherScheme);
    }

    // ----- the eight parts coming back out, in the order they went in

    /// <summary>The eight parts of an entry fall out in the order the constructor names them - the project, the
    /// environment, the release, the result, the result in the file's word, the moment it ended, who started it and the
    /// task - as the very address it was, and what came out goes back in the same way. The entry that names nobody and
    /// nowhere falls apart to the two parts a file leaves out.</summary>
    [Fact]
    public void TheEntryFallsApartInTheOrderItWasMade()
    {
        var url = new Uri($"{Tasks}2");
        var entry = new RecentDeployment("demo-ui", "uat", "2.4.43", DeploymentState.Canceled, "canceled", Finished, "jeffrey", url);

        var (project, environment, release, result, resultText, finished, startedBy, task) = entry;

        Assert.Equal("demo-ui", project);
        Assert.Equal("uat", environment);
        Assert.Equal("2.4.43", release);
        Assert.Equal(DeploymentState.Canceled, result);
        Assert.Equal("canceled", resultText);
        Assert.Equal(Finished, finished);
        Assert.Equal("jeffrey", startedBy);
        Assert.Same(url, task);
        Assert.Equal(entry, new RecentDeployment(project, environment, release, result, resultText, finished, startedBy, task));

        var (bareProject, bareEnvironment, bareRelease, bareResult, bareResultText, bareFinished, bareStartedBy, bareTask) = Bare();

        Assert.Equal("demo-ui", bareProject);
        Assert.Equal("uat", bareEnvironment);
        Assert.Equal("2.4.43", bareRelease);
        Assert.Equal(DeploymentState.Unknown, bareResult);
        Assert.Equal("timed out", bareResultText);
        Assert.Equal(Finished, bareFinished);
        Assert.Null(bareStartedBy);
        Assert.Null(bareTask);
        Assert.Equal(Bare(), new RecentDeployment(bareProject, bareEnvironment, bareRelease, bareResult, bareResultText, bareFinished, bareStartedBy, bareTask));
    }

    // ----- the equality, the hash, the two operators

    /// <summary>Two entries are one while their eight parts are - each part by its own equality, the three words in the
    /// case they are written in, the result as the state it is, the moment as the instant it stands on and the address
    /// as the address it is, down to the parts no line of the page ever shows - and the hash follows the equality,
    /// boxed or unboxed and from one call to the next, so that the entries that are one are one entry of a set and a
    /// lookup meets them.</summary>
    [Fact]
    public void TwoEntriesAreOneWhileTheirEightPartsAre()
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
        Assert.Single(new HashSet<RecentDeployment> { one, two });
        Assert.Contains(one, new HashSet<RecentDeployment> { two });

        var keyed = new Dictionary<RecentDeployment, string> { [one] = "the last one" };
        keyed[two] = "still the last one";

        Assert.Single(keyed);
        Assert.Equal("still the last one", keyed[Bare() with { Result = DeploymentState.Succeeded, ResultText = "succeeded", StartedBy = "ai-ops", Url = new Uri($"{Tasks}2") }]);
    }

    /// <summary>A difference in one part is another entry, and so is the same part in another place: the project of
    /// another deployable, the environment in another case, a release one number on, the result in another state, the
    /// word of that result capitalized, a moment a second on, another person, no person, an address of another task and
    /// no address at all. A set of the entries that differ in one part each holds them all apart, and the eight parts
    /// handed over in another order are eight other parts.</summary>
    [Fact]
    public void AnEntryThatDiffersInOnePartIsAnotherEntry()
    {
        var one = Full();

        RecentDeployment[] different =
        [
            one with { Project = "demo-system" },
            one with { Project = "DEMO-UI" },
            one with { Project = string.Empty },
            one with { Environment = "prod" },
            one with { Environment = "UAT" },
            one with { Release = "2.4.44" },
            one with { Result = DeploymentState.Failed },
            one with { Result = DeploymentState.Unknown },
            one with { ResultText = "Succeeded" },
            one with { ResultText = "succeeded " },
            one with { Finished = Finished.AddSeconds(1) },
            one with { Finished = DateTimeOffset.MinValue },
            one with { StartedBy = "jeffrey" },
            one with { StartedBy = null },
            one with { Url = new Uri("https://octopus.example.net/spaces/356/tasks/ServerTasks-2") },
            one with { Url = null },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.False(one == other);
            Assert.True(one != other);
        }

        Assert.Equal(1 + different.Length, new HashSet<RecentDeployment>([one, .. different]).Count);

        // The environment is no project and the word of a result is no release: the parts stand in their places.
        Assert.NotEqual(one, new RecentDeployment("uat", "demo-ui", "2.4.43", one.Result, one.ResultText, one.Finished, one.StartedBy, one.Url));
        Assert.NotEqual(one, new RecentDeployment(one.Project, one.Environment, one.ResultText, one.Result, one.Release, one.Finished, one.StartedBy, one.Url));
    }

    /// <summary>The nothing on either side of the members every record is asked with: two entries that are no entry at
    /// all are one, an entry and no entry are two - the equality and the inequality each saying it, from either side -
    /// and the entry asked about itself says no to the things it is not: a word, a state, a moment, an address, another
    /// entry that differs in a single part, a wait of the same release and the anonymous thing that happens to carry
    /// two of its parts.</summary>
    [Fact]
    public void NoEntryIsOneWithNoEntryAndNotWithAnyOtherThing()
    {
        RecentDeployment? nothing = null;
        RecentDeployment? alsoNothing = null;
        var there = Bare();

        Assert.Null(default(RecentDeployment));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (RecentDeployment?)null);
        Assert.True(there != (RecentDeployment?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("demo-ui"));
        Assert.False(there.Equals(DeploymentState.Unknown));
        Assert.False(there.Equals(Finished));
        Assert.False(there.Equals(new Uri($"{Tasks}2")));
        Assert.False(there.Equals(new DeploymentWait("sign-off", null, null, null)));
        Assert.False(there.Equals(new { Project = "demo-ui", Environment = "uat" }));
        Assert.False(there.Equals(Bare() with { Release = "2.4.44" }));
        Assert.False(there.Equals(Full()));
    }

    // ----- the copy, turning the part it names

    /// <summary>Any of the eight parts may be turned as the entry is copied - the environment to another one, the
    /// result to another state, the moment to the other end of the clock, the person and the address to nothing - and
    /// stands where it is turned to, while the seven the turn does not name stay the very values the entry was built
    /// of. The copy is another entry and never the same reference, the original stands as it stood, and a copy that
    /// names every part its own value is the entry it was copied from.</summary>
    [Fact]
    public void AnEntryCopiedTurnsThePartItNamesAndKeepsTheOtherSeven()
    {
        var url = new Uri($"{Tasks}2");
        var entry = new RecentDeployment("demo-ui", "uat", "2.4.43", DeploymentState.Succeeded, "succeeded", Finished, "ai-ops", url);

        var moved = entry with { Environment = "prod" };

        Assert.Equal("prod", moved.Environment);
        Assert.Equal("demo-ui", moved.Project);
        Assert.Equal("2.4.43", moved.Release);
        Assert.Equal(DeploymentState.Succeeded, moved.Result);
        Assert.Equal("succeeded", moved.ResultText);
        Assert.Equal(Finished, moved.Finished);
        Assert.Equal("ai-ops", moved.StartedBy);
        Assert.Same(url, moved.Url);
        Assert.NotSame(entry, moved);
        Assert.NotEqual(entry, moved);

        var worded = entry with { Result = DeploymentState.Canceled, ResultText = "aborted" };

        Assert.Equal(DeploymentState.Canceled, worded.Result);
        Assert.Equal("aborted", worded.ResultText);
        Assert.Equal(DeploymentState.Succeeded, entry.Result);
        Assert.Equal("succeeded", entry.ResultText);

        var silent = entry with { StartedBy = null, Url = null };

        Assert.Null(silent.StartedBy);
        Assert.Null(silent.Url);
        Assert.Equal(Bare() with { Result = DeploymentState.Succeeded, ResultText = "succeeded" }, silent);
        Assert.NotEqual(entry, silent);

        var unchanged = entry with { };

        Assert.Equal(entry, unchanged);
        Assert.NotSame(entry, unchanged);
        Assert.Equal(entry.GetHashCode(), unchanged.GetHashCode());
        Assert.Same(url, unchanged.Url);

        var fromBare = Bare() with { StartedBy = "jeffrey" };

        Assert.Equal("jeffrey", fromBare.StartedBy);
        Assert.Null(fromBare.Url);
        Assert.Equal(DeploymentState.Unknown, fromBare.Result);
        Assert.Equal("timed out", fromBare.ResultText);
        Assert.NotEqual(Bare(), fromBare);

        Assert.Equal("uat", entry.Environment);
        Assert.Same(url, entry.Url);
    }

    // ----- the words the record says itself

    /// <summary>The words the entry says its eight parts in: its own name at the front, the parts in the order the
    /// constructor names them, the result as the state it is, the moment as the clock of whoever reads them says it,
    /// and the address as the address it is - the part nobody gave being the empty place between the mark and the
    /// comma. These are the record's words and not the page's: no <c>ago</c>, no <c>reached</c>, no
    /// <c>deployed</c> and no sentence, which are the words <see cref="DeploymentActivity" /> makes of the parts.</summary>
    [Fact]
    public void TheEntrySaysItsEightPartsInTheOrderTheyAreNamed()
    {
        const string Spoken =
            "RecentDeployment { Project = demo-ui, Environment = uat, Release = 2.4.43, Result = Succeeded, "
                + "ResultText = succeeded, Finished = 10/09/2026 07:00:00 +00:00, StartedBy = ai-ops, "
                + "Url = https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-2 }";

        Assert.Equal(Spoken, Words(Full()));
        Assert.StartsWith("RecentDeployment { Project = demo-ui, ", Words(Full()), StringComparison.Ordinal);
        Assert.EndsWith("ServerTasks-2 }", Words(Full()), StringComparison.Ordinal);

        // The two parts a file leaves out are the empty places, and an entry of nothing says nothing.
        Assert.Equal(
            "RecentDeployment { Project = demo-ui, Environment = uat, Release = 2.4.43, Result = Unknown, "
                + "ResultText = timed out, Finished = 10/09/2026 07:00:00 +00:00, StartedBy = , Url =  }",
            Words(Bare()));
        Assert.Equal(
            "RecentDeployment { Project = , Environment = , Release = , Result = Unknown, ResultText = , "
                + "Finished = 01/01/0001 00:00:00 +00:00, StartedBy = , Url =  }",
            Words(new RecentDeployment(null!, null!, null!, DeploymentState.Unknown, null!, default, null, null)));

        foreach (var word in new[] { "—", " ago", "deployed", "reached", "The last deployment", "canceled" })
        {
            Assert.DoesNotContain(word, Words(Full()), StringComparison.Ordinal);
        }
    }

    /// <summary>Each state is said by the name the page gives it and no other word - even the one beyond every name,
    /// which the record says as the number it is - while the word the file wrote is said in its own letters, whatever
    /// state stands beside it: the two parts are two parts, in the words as everywhere else.</summary>
    [Theory]
    [InlineData(DeploymentState.Unknown, "Unknown")]
    [InlineData(DeploymentState.Queued, "Queued")]
    [InlineData(DeploymentState.Executing, "Executing")]
    [InlineData(DeploymentState.Waiting, "Waiting")]
    [InlineData(DeploymentState.Succeeded, "Succeeded")]
    [InlineData(DeploymentState.Failed, "Failed")]
    [InlineData(DeploymentState.Canceled, "Canceled")]
    public void TheResultIsSaidByItsNameAndTheWordByItsLetters(DeploymentState state, string name)
    {
        var entry = Full() with { Result = state };

        Assert.Contains($"Result = {name}, ", Words(entry), StringComparison.Ordinal);
        Assert.Contains("ResultText = succeeded, ", Words(entry), StringComparison.Ordinal);
        Assert.StartsWith("RecentDeployment { Project = demo-ui, ", Words(entry), StringComparison.Ordinal);
    }

    /// <summary>The moment of these words follows whoever reads them, which is what sets the record's own words apart
    /// from the page's: a reader of German is told <c>09.10.2026 07:00:00 +00:00</c> by the entry and
    /// <c>2026-10-09 07:00 +00:00</c> by <see cref="TimeText" />, whose words are written for the dashboard and not for
    /// whoever looks. The result's name and the file's word are the same letters to both readers.</summary>
    [Fact]
    public void TheEntryOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        var ui = Full();

        ViewerCulture.German(() =>
        {
            Assert.Contains("Finished = 09.10.2026 07:00:00 +00:00", ui.ToString(), StringComparison.Ordinal);
            Assert.Contains("Result = Succeeded, ResultText = succeeded", ui.ToString(), StringComparison.Ordinal);
            Assert.Equal("2026-10-09 07:00 +00:00", TimeText.DateAndTime(ui.Finished, TimeZoneInfo.Utc));
        });

        Assert.Contains("Finished = 10/09/2026 07:00:00 +00:00", Words(ui), StringComparison.Ordinal);
        Assert.DoesNotContain("09.10.2026", Words(ui), StringComparison.Ordinal);
    }

    // ----- the record's own shape

    /// <summary>The entry's whole public surface, as it stands: its eight parts, named in the order the constructor
    /// names them and of the type the constructor says, the entry falling apart into them, and the members every
    /// record is asked - comparing, hashing and saying itself - and nothing else to call. A ninth part added to the
    /// entry has to be written here as well, which is the point: the page cannot be shown a part nobody decided what
    /// it means.</summary>
    [Fact]
    public void TheEntryHasEightPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Project", "Environment", "Release", "Result", "ResultText", "Finished", "StartedBy", "Url"],
            [.. typeof(RecentDeployment)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Environment", "Equals", "Finished", "GetHashCode", "Project", "Release", "Result", "ResultText", "StartedBy", "ToString", "Url", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal([typeof(IEquatable<RecentDeployment>)], typeof(RecentDeployment).GetInterfaces());

        var properties = typeof(RecentDeployment)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Project"].PropertyType);
        Assert.Equal(typeof(string), properties["Environment"].PropertyType);
        Assert.Equal(typeof(string), properties["Release"].PropertyType);
        Assert.Equal(typeof(DeploymentState), properties["Result"].PropertyType);
        Assert.Equal(typeof(string), properties["ResultText"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset), properties["Finished"].PropertyType);
        Assert.Equal(typeof(string), properties["StartedBy"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Url"].PropertyType);

        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.GetMethod!.IsPublic));
        Assert.All(properties.Values, property => Assert.Empty(property.GetIndexParameters()));

        var arguments = typeof(RecentDeployment).GetConstructors().Single().GetParameters();

        Assert.Equal(8, arguments.Length);
        Assert.Equal(
            ["Project", "Environment", "Release", "Result", "ResultText", "Finished", "StartedBy", "Url"],
            [.. arguments.Select(argument => argument.Name!)]);
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(DeploymentState), typeof(string), typeof(DateTimeOffset), typeof(string), typeof(Uri)],
            [.. arguments.Select(argument => argument.ParameterType)]);
    }

    /// <summary>The public members the entry declares itself: its properties and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list, and the reading of a file
    /// is no public member at all.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RecentDeployment)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words an entry says its eight parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own further up
    /// this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(RecentDeployment entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return entry.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
