using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="HealthCheckEntry" />: one thing the app depends on
/// and checks, as the detailed health check named it - the four parts <c>src/Dashboard/Health/HealthDetail.cs</c>
/// gives it (the name, the state, the entry's own words and the milliseconds it took) and the one word it works out
/// of them, <see cref="HealthCheckEntry.IsHealthy" />.
/// <para>
/// What the other files already pin is left alone. <see cref="HealthDetailTests" /> pins the four parts as they come
/// out of the endpoint's answer - an entry read with its status in whatever case the answer wrote it, a status the
/// dashboard does not know and a status left out both landing on <see cref="CheckState.Unknown" />, an answer that
/// names no entry being no answer at all - and, with it, that an entry built by hand of the same four parts is one
/// with the entry the answer gave. <see cref="CheckStateEdgeCaseTests" /> pins the four states themselves, the
/// numbers and the words and the order they are declared in. The words the page makes of an entry are
/// <see cref="HealthDetailText" />'s and are pinned there and on the tiles: <see cref="HealthDetailTests" /> the
/// tooltip of a mark, <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> that a healthy entry's word is
/// visually hidden, <see cref="DependencyTests" /> a long description cut for the line and whole in the tooltip, and
/// <see cref="EventDetectorEdgeCaseTests" /> that an entry's description of no characters is said as the blank it is
/// and that a change of an entry's state is an event of its own.
/// </para>
/// <para>
/// What is left, and is pinned here, is the entry as the record it is: the constructor handing each of its four
/// arguments to the property of its name and the last two falling to nothing when the caller does not name them,
/// nameable out of their order as well; the name kept in its exact letters - nothing, the empty name, a name of
/// whitespace, a name padded, a name a thousand characters long - and never compared as a reader hears it, whatever
/// culture is reading, which is what sets the entry apart from the lookup that finds it whatever case it is named
/// in; the state stood on as it was given, the state nobody set standing on <see cref="CheckState.Healthy" /> and a
/// number the enum does not name kept as the number it is; a description of no characters being a description and
/// not the absence of one; the milliseconds at the ends a real number runs to, the negative and the number that is
/// no number, the nothing below a tenth of a millisecond and the nothing that was never said; <see cref="IsHealthy" />
/// read out of the state and out of nothing else; the two entries being one while their four parts are, and the
/// hash, the set and the lookup following that; the four parts coming back out in the order they went in; the copy
/// that turns one part and keeps the other three; the words the entry says its four parts in, the healthy of them
/// said with them, and those words following whoever reads a number; and the public surface, so that the day a fifth
/// part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: an entry holds no moment and calls
/// nothing, every state, word and number below is one the test brings, and the one moment named here is fixed and
/// only ever stands for the instant the entry was read at the door of the record above it.
/// </para>
/// </summary>
public class HealthCheckEntryEdgeCaseTests
{
    /// <summary>A fixed moment, never the clock: an entry holds none, and this is here only to make a detailed
    /// answer for the tests that ask what an entry is not.</summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>An entry with every part said: the name the app registered the check by, the state it ended in, its
    /// own words and the milliseconds it took.</summary>
    private static HealthCheckEntry Full() =>
        new("DataAccess", CheckState.Unhealthy, "Database connection failed: login timeout expired", 15002.3);

    /// <summary>An entry of nothing but a name and a state: the two parts a check may leave unsaid, left unsaid.</summary>
    private static HealthCheckEntry Bare(string name = "Queue", CheckState state = CheckState.Unknown) =>
        new(name, state);

    // ----- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same name,
    /// and the entry holds the words it was given as the very text it stands in: an entry built again of the same
    /// four parts is the same entry and no longer the same reference.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var description = "Database connection failed: login timeout expired".AsSpan().ToString();
        var entry = new HealthCheckEntry("DataAccess", CheckState.Unhealthy, description, 15002.3);

        Assert.Equal("DataAccess", entry.Name);
        Assert.Equal(CheckState.Unhealthy, entry.State);
        Assert.Equal(description, entry.Description);
        Assert.Same(description, entry.Description);
        Assert.Equal(15002.3, entry.DurationMs!.Value);
        Assert.True(entry.Description!.Contains("login timeout expired", StringComparison.Ordinal));

        var again = Full();

        Assert.Equal(entry, again);
        Assert.NotSame(entry, again);
        Assert.Equal(entry.GetHashCode(), again.GetHashCode());
    }

    /// <summary>The last two parts are the two a caller need not name: the entry built of a name and a state says no
    /// words and took no measurable time, which is the entry the answer of a check that leaves both out lands on -
    /// and naming the nothings explicitly makes the same entry, not another.</summary>
    [Fact]
    public void TheTwoLastPartsFallToNothingWhenTheCallerDoesNotNameThem()
    {
        var bare = Bare();

        Assert.Null(bare.Description);
        Assert.Null(bare.DurationMs);
        Assert.Equal("Queue", bare.Name);
        Assert.Equal(CheckState.Unknown, bare.State);

        Assert.Equal(bare, new HealthCheckEntry("Queue", CheckState.Unknown, null, null));
        Assert.Equal(bare.GetHashCode(), new HealthCheckEntry("Queue", CheckState.Unknown, null, null).GetHashCode());
        Assert.NotEqual(bare, new HealthCheckEntry("Queue", CheckState.Unknown, null, 0));
        Assert.NotEqual(bare, new HealthCheckEntry("Queue", CheckState.Unknown, string.Empty, null));

        // Named out of their order, the parts land where they stand and the one left unnamed is nothing.
        var slow = new HealthCheckEntry("LlmGateway", CheckState.Degraded, DurationMs: 42.07);

        Assert.Null(slow.Description);
        Assert.Equal(42.07, slow.DurationMs!.Value);

        var worded = new HealthCheckEntry("LlmGateway", CheckState.Degraded, Description: "Chat client answered slowly");

        Assert.Null(worded.DurationMs);
        Assert.Equal("Chat client answered slowly", worded.Description);
        Assert.NotEqual(slow, worded);
    }

    // ----- the name, kept exactly as the app registered the check

    /// <summary>The name is kept in its exact letters: the empty name, the name that is only whitespace, a name
    /// padded, a name in another case, a name with the full stop the answer's own words end in and a name of letters
    /// no reader expects all stand where they stand, and are one entry with another of the very same letters and of
    /// one hash. That the lookup finds an entry whatever case it is named in, and that the parse leaves an entry with
    /// no name out, are pinned where they happen; the entry turns nothing.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(" API ")]
    [InlineData("API")]
    [InlineData("api")]
    [InlineData("aPi")]
    [InlineData("DataAccess")]
    [InlineData("DataAccess.")]
    [InlineData("check 1")]
    [InlineData("ÆØÅ-ø")]
    public void TheNameIsKeptExactlyAsTheAppRegisteredTheCheck(string name)
    {
        var entry = Bare(name);

        Assert.Equal(name, entry.Name);
        Assert.Equal(entry, Bare(name));
        Assert.Equal(entry.GetHashCode(), Bare(name).GetHashCode());
        Assert.NotEqual(entry, Bare($"{name} "));
        Assert.Contains($"Name = {name},", Words(entry), StringComparison.Ordinal);
    }

    /// <summary>A name of a thousand characters is kept at its full length, in the entry and in its words: nothing
    /// here is cut to what a line of the diagram holds, which is the page's doing and pinned on the page's own
    /// words.</summary>
    [Fact]
    public void ANameOfAThousandCharactersIsKeptWhole()
    {
        var name = new string('A', 1_000);

        var entry = Bare(name);

        Assert.Equal(1_000, entry.Name!.Length);
        Assert.Equal(name, entry.Name);
        Assert.Equal(entry, Bare(name));
        Assert.Contains(name, Words(entry), StringComparison.Ordinal);
        Assert.StartsWith("HealthCheckEntry { Name = AAAA", Words(entry), StringComparison.Ordinal);
    }

    /// <summary>An entry named nothing at all is an entry: the record asks nothing of its name, neither a text nor a
    /// length, so the <see langword="null" /> a caller forces past the compiler's warning is kept as the nothing it
    /// is, says the empty place in the words, is one with another entry named nothing and is not the entry named
    /// with no characters. That an answer naming no entry is left out of the answer is the reading's, pinned
    /// there.</summary>
    [Fact]
    public void AnEntryNamedNothingIsStillAnEntryAndNotTheOneNamedWithNoCharacters()
    {
        var none = new HealthCheckEntry(null!, CheckState.Healthy);
        var empty = Bare(string.Empty);

        Assert.Null(none.Name);
        Assert.Equal(string.Empty, empty.Name);
        Assert.NotEqual(none, empty);
        Assert.NotEqual(none.GetHashCode(), empty.GetHashCode());
        Assert.Equal(none, new HealthCheckEntry(null!, CheckState.Healthy));
        Assert.Equal(none.GetHashCode(), new HealthCheckEntry(null!, CheckState.Healthy).GetHashCode());
        Assert.StartsWith("HealthCheckEntry { Name = , State = Healthy", Words(none), StringComparison.Ordinal);
        Assert.True(none.IsHealthy);
        Assert.Single(new HashSet<HealthCheckEntry> { none, new HealthCheckEntry(null!, CheckState.Healthy) });
    }

    /// <summary>Two entries are the same entry only while their names are the same letters in the same order: the
    /// case is part of the name, one letter more is another check, and the comparison does not move with whoever
    /// reads - a German reader of the two entries is shown two entries, since it is the entry's own letters that are
    /// compared and not a reader's ear. The lookup that forgives the case is the answer's, pinned there.</summary>
    [Fact]
    public void TwoNamesThatDifferInALetterOrACaseAreTwoNames()
    {
        var api = Bare("API");

        Assert.NotEqual(api, Bare("api"));
        Assert.NotEqual(api, Bare("aPI"));
        Assert.NotEqual(api, Bare("APIs"));
        Assert.NotEqual(api, Bare("AP I"));
        Assert.NotEqual(api.GetHashCode(), Bare("api").GetHashCode());
        Assert.Equal(2, new HashSet<HealthCheckEntry> { api, Bare("api") }.Count);

        ViewerCulture.German(() =>
        {
            Assert.False(api.Equals(Bare("api")));
            Assert.False(api == Bare("api"));
            Assert.True(api == Bare("API"));
        });
    }

    // ----- the state, and the healthy an entry says out of it

    /// <summary>The state stands as it was given and the entry is healthy of it and of nothing else: of the four
    /// states only <see cref="CheckState.Healthy" /> says so, the two that are not healthy and the one the dashboard
    /// does not know saying no as plainly as each other.</summary>
    [Theory]
    [InlineData(CheckState.Healthy, true)]
    [InlineData(CheckState.Degraded, false)]
    [InlineData(CheckState.Unhealthy, false)]
    [InlineData(CheckState.Unknown, false)]
    public void TheStateStandsAsGivenAndSaysHealthyOfHealthyAlone(CheckState state, bool healthy)
    {
        var entry = new HealthCheckEntry("DataAccess", state, "Database connection failed", 12.3);

        Assert.Equal(state, entry.State);
        Assert.Equal(healthy, entry.IsHealthy);
        Assert.Equal(healthy, (entry with { }).IsHealthy);
        Assert.EndsWith(healthy ? "IsHealthy = True }" : "IsHealthy = False }", Words(entry), StringComparison.Ordinal);
    }

    /// <summary>The state a caller never settled stands on the first of the four, which is the healthy one: an entry
    /// built of <see cref="default" /> says it is healthy, and nothing in the entry classifies an answer the way the
    /// reading of one does - the reading settles a status it does not know on the state that is not known, pinned in
    /// <see cref="HealthDetailTests" />.</summary>
    [Fact]
    public void AnEntryNobodySetTheStateOnSaysItIsHealthy()
    {
        var forgotten = new HealthCheckEntry("Queue", default);

        Assert.Equal(CheckState.Healthy, forgotten.State);
        Assert.True(forgotten.IsHealthy);
        Assert.Equal(Bare("Queue", CheckState.Healthy), forgotten);
        Assert.Contains("IsHealthy = True", Words(forgotten), StringComparison.Ordinal);
    }

    /// <summary>A state the enum does not name is kept as the number it stands on: it is neither one of the four nor
    /// nothing, it says no healthy, it is said in the words as its number where the four are said as their word, and
    /// it is another entry than every state a name belongs to - the entry does not classify what it is told, it
    /// keeps it.</summary>
    [Theory]
    [InlineData((CheckState)(-1))]
    [InlineData((CheckState)4)]
    [InlineData((CheckState)int.MaxValue)]
    [InlineData((CheckState)int.MinValue)]
    public void AStateTheEnumDoesNotNameIsKeptAsTheNumberItStandsOn(CheckState state)
    {
        var entry = new HealthCheckEntry("Queue", state, "Queue says nothing", 3.5);

        Assert.Equal(state, entry.State);
        Assert.False(entry.IsHealthy);
        Assert.StartsWith($"HealthCheckEntry {{ Name = Queue, State = {(int)state},", Words(entry), StringComparison.Ordinal);
        Assert.Contains("IsHealthy = False", Words(entry), StringComparison.Ordinal);

        foreach (var named in Enum.GetValues<CheckState>())
        {
            Assert.NotEqual(entry, Bare("Queue", named));
        }

        Assert.Equal(entry, new HealthCheckEntry("Queue", state, "Queue says nothing", 3.5));
        Assert.NotEqual(entry.GetHashCode(), Bare("Queue", CheckState.Healthy).GetHashCode());
    }

    /// <summary>The word for healthy is read out of the state and out of nothing else: the entry that took fifteen
    /// seconds and said its failure in words is healthy while its state says so, and the very same name, the very
    /// same text and the very same number are not healthy the moment the state is turned. One part turned is two
    /// entries.</summary>
    [Fact]
    public void IsHealthyReadsTheStateAndNothingElse()
    {
        var words = "Database connection failed: login timeout expired";
        var wellButSlow = new HealthCheckEntry("DataAccess", CheckState.Healthy, words, 15002.3);
        var illButNoQuicker = new HealthCheckEntry("DataAccess", CheckState.Degraded, words, 15002.3);

        Assert.True(wellButSlow.IsHealthy);
        Assert.False(illButNoQuicker.IsHealthy);
        Assert.Same(wellButSlow.Description, illButNoQuicker.Description);
        Assert.Equal(wellButSlow.Name, illButNoQuicker.Name);
        Assert.Equal(wellButSlow.DurationMs, illButNoQuicker.DurationMs);

        // The state turned is another word and another entry; the three parts it shares stayed where they stood.
        Assert.NotEqual(wellButSlow, illButNoQuicker);
        Assert.True(illButNoQuicker with { State = CheckState.Healthy } == wellButSlow);
        Assert.False((wellButSlow with { State = CheckState.Unknown }).IsHealthy);

        // Neither the words nor the milliseconds can say a healthy on their own, nor take one back.
        foreach (var description in new[] { null, string.Empty, " ", "unhealthy" })
        {
            foreach (var duration in new double?[] { null, 0, -1, double.NaN, 15002.3 })
            {
                var entry = new HealthCheckEntry("API", CheckState.Healthy, description, duration);

                Assert.True(entry.IsHealthy);
                Assert.False((entry with { State = CheckState.Degraded }).IsHealthy);
            }
        }
    }

    // ----- the entry's own words

    /// <summary>The words a check gives are kept as letters and nothing more: the empty word is said and not left
    /// unsaid, a word of nothing but whitespace is a word of its own, and a description padded, in its case, ending
    /// in a full stop or running over the line the page would cut it to stands whole - it is the page that trims and
    /// cuts, and the entry that keeps.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  Database connection successful  ")]
    [InlineData("Database connection successful")]
    [InlineData("database connection successful")]
    [InlineData("Chat client answered slowly: 4.2 s (threshold 2 s).")]
    [InlineData("Login failed.\nTry again.")]
    public void TheDescriptionIsKeptAsTheCheckWroteIt(string description)
    {
        var entry = new HealthCheckEntry("DataAccess", CheckState.Unhealthy, description, 12.3);

        Assert.Equal(description, entry.Description);
        Assert.Equal(entry, new HealthCheckEntry("DataAccess", CheckState.Unhealthy, description, 12.3));
        Assert.Equal(entry.GetHashCode(), new HealthCheckEntry("DataAccess", CheckState.Unhealthy, description, 12.3).GetHashCode());
    }

    /// <summary>The empty description, a description of whitespace and no description at all are three entries
    /// apart: the check that answered with words of no characters is not the check that answered with none, and the
    /// mark and the line the page makes of them differ accordingly - which is the page's doing, pinned there. Here
    /// only the parts being apart is.</summary>
    [Fact]
    public void ADescriptionOfNoCharactersIsNotADescriptionOfNothing()
    {
        var none = Bare();
        var empty = new HealthCheckEntry("Queue", CheckState.Unknown, string.Empty);
        var blank = new HealthCheckEntry("Queue", CheckState.Unknown, "   ");

        Assert.Null(none.Description);
        Assert.Equal(string.Empty, empty.Description);
        Assert.Equal("   ", blank.Description);
        Assert.Equal(3, blank.Description!.Length);

        Assert.NotEqual(none, empty);
        Assert.NotEqual(empty, blank);
        Assert.NotEqual(none, blank);
        Assert.Equal(3, new HashSet<HealthCheckEntry> { none, empty, blank }.Count);
        Assert.NotEqual(none.GetHashCode(), empty.GetHashCode());
        Assert.NotEqual(empty.GetHashCode(), blank.GetHashCode());
    }

    // ----- the milliseconds, at the ends a number runs to

    /// <summary>The milliseconds are the number the answer gave, kept as it is: nothing at all, the zero that is a
    /// number, the nothing below a tenth of a millisecond the page says <c>under 0.1 ms</c> of, a tenth of a tenth,
    /// a negative the answer cannot mean, the largest and the smallest a real number holds, and both the infinities.
    /// The words the page makes of them are pinned where they are said; the entry keeps what it is given.</summary>
    [Theory]
    [InlineData(0d)]
    [InlineData(0.0006)]
    [InlineData(0.04)]
    [InlineData(1.5)]
    [InlineData(42.07)]
    [InlineData(999.9)]
    [InlineData(1000d)]
    [InlineData(15002.3)]
    [InlineData(-15002.3)]
    [InlineData(double.Epsilon)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void TheDurationIsTheNumberTheAnswerGaveAndNoOther(double milliseconds)
    {
        var entry = new HealthCheckEntry("API", CheckState.Healthy, "API layer is healthy", milliseconds);

        Assert.Equal(milliseconds, entry.DurationMs!.Value);
        Assert.True(entry.IsHealthy);
        Assert.Equal(entry, new HealthCheckEntry("API", CheckState.Healthy, "API layer is healthy", milliseconds));
        Assert.Equal(entry.GetHashCode(), new HealthCheckEntry("API", CheckState.Healthy, "API layer is healthy", milliseconds).GetHashCode());
    }

    /// <summary>The number that is no number is a duration all the same, and a record holds it to be one with itself
    /// although no comparison of two numbers says so: an entry of it is one entry of a set, of one hash, while the
    /// same entry of no number at all, or of the zero, is another. The reading of an answer cannot hand one out,
    /// because it takes the number as the answer wrote it - what is pinned here is that the entry asks nothing of
    /// the number it holds.</summary>
    [Fact]
    public void ANumberThatIsNoNumberIsADurationAndOneWithItself()
    {
        var notANumber = new HealthCheckEntry("API", CheckState.Healthy, null, double.NaN);
        var another = new HealthCheckEntry("API", CheckState.Healthy, null, double.NaN);

        Assert.True(double.IsNaN(notANumber.DurationMs!.Value));
        Assert.False(notANumber.DurationMs!.Value > 0);
        Assert.False(notANumber.DurationMs!.Value < 0);
        Assert.False(notANumber.DurationMs!.Value == notANumber.DurationMs!.Value);
        Assert.Equal(notANumber, another);
        Assert.True(notANumber == another);
        Assert.Equal(notANumber.GetHashCode(), another.GetHashCode());
        Assert.Single(new HashSet<HealthCheckEntry> { notANumber, another });
        Assert.Contains("DurationMs = NaN", Words(notANumber), StringComparison.Ordinal);

        Assert.NotEqual(notANumber, new HealthCheckEntry("API", CheckState.Healthy, null, null));
        Assert.NotEqual(notANumber, new HealthCheckEntry("API", CheckState.Healthy, null, 0));
        Assert.NotEqual(notANumber.GetHashCode(), new HealthCheckEntry("API", CheckState.Healthy, null, 0).GetHashCode());
    }

    /// <summary>The negative zero and the zero the answer wrote are one duration and one entry, whichever of the two
    /// a caller happened to hold - the entry compares numbers as numbers, not as the bits that spell them, although
    /// it says the sign it was given in its own words - while the duration that was never said is a third thing apart
    /// from both.</summary>
    [Fact]
    public void TheNegativeZeroAndTheZeroAreOneDurationAndOneEntry()
    {
        var below = new HealthCheckEntry("API", CheckState.Healthy, null, -0d);
        var zero = new HealthCheckEntry("API", CheckState.Healthy, null, 0d);
        var unsaid = Bare("API", CheckState.Healthy);

        Assert.Equal(below, zero);
        Assert.Equal(below.GetHashCode(), zero.GetHashCode());
        Assert.Single(new HashSet<HealthCheckEntry> { below, zero });
        Assert.NotEqual(below, unsaid);
        Assert.NotEqual(zero, unsaid);
        Assert.Contains("DurationMs = -0,", Words(below), StringComparison.Ordinal);
        Assert.Contains("DurationMs = 0,", Words(zero), StringComparison.Ordinal);
        Assert.Contains("DurationMs = ,", Words(unsaid), StringComparison.Ordinal);
    }

    // ----- two entries being one entry or two

    /// <summary>Two entries are one while their four parts are, each part by its own equality and none of them by
    /// reference: two entries of the same name, state, words and number are one entry however many instances carry
    /// them, of one hash from call to call, boxed or unboxed, and one entry of a set - which is what lets the page
    /// meet the entry it read again after it was read a second time.</summary>
    [Fact]
    public void TwoEntriesAreOneWhileTheirFourPartsAre()
    {
        var one = Full();
        var two = Full();

        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals(one with { }));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.False(one == (HealthCheckEntry?)null);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<HealthCheckEntry> { one, two });
        Assert.Contains(one, new HashSet<HealthCheckEntry> { two });
        Assert.Equal("said", new Dictionary<HealthCheckEntry, string> { [two] = "said" }[one]);

        // The words are compared as the text they are, not as the instance they stand in.
        var sameWords = new HealthCheckEntry("DataAccess", CheckState.Unhealthy, "Database connection failed: login timeout expired".AsSpan().ToString(), 15002.3);
        Assert.Equal(one, sameWords);
        Assert.Equal(one.GetHashCode(), sameWords.GetHashCode());
        Assert.NotSame(one.Description, sameWords.Description);
    }

    /// <summary>A difference in one part is another entry, and so is the same part in another place: the name in its
    /// case, another state, the words left unsaid that were said, the number by a tenth of a millisecond, a name
    /// turned into a description and a description into a name. A set of the entries that differ in one part each
    /// holds them all apart, and each part turned moves the hash with it.</summary>
    [Fact]
    public void AnEntryThatDiffersInOnePartIsAnotherEntry()
    {
        var one = Full();

        Assert.NotEqual(one, one with { Name = "dataaccess" });
        Assert.NotEqual(one, one with { Name = "Data Access" });
        Assert.NotEqual(one, one with { Name = string.Empty });
        Assert.NotEqual(one, one with { State = CheckState.Healthy });
        Assert.NotEqual(one, one with { State = CheckState.Degraded });
        Assert.NotEqual(one, one with { State = CheckState.Unknown });
        Assert.NotEqual(one, one with { Description = null });
        Assert.NotEqual(one, one with { Description = string.Empty });
        Assert.NotEqual(one, one with { Description = "Database connection failed: login timeout expired." });
        Assert.NotEqual(one, one with { DurationMs = null });
        Assert.NotEqual(one, one with { DurationMs = 0 });
        Assert.NotEqual(one, one with { DurationMs = 15002.4 });
        Assert.NotEqual(one, new HealthCheckEntry("Database connection failed: login timeout expired", CheckState.Unhealthy, "DataAccess", 15002.3));

        Assert.Equal(
            6,
            new HashSet<HealthCheckEntry>
            {
                one,
                one with { Name = "dataaccess" },
                one with { State = CheckState.Healthy },
                one with { Description = null },
                one with { DurationMs = null },
                one with { DurationMs = 15002.4 },
            }.Count);

        Assert.NotEqual(one.GetHashCode(), (one with { Name = "dataaccess" }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { State = CheckState.Healthy }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Description = null }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { DurationMs = null }).GetHashCode());
    }

    /// <summary>The nothing on either side of the members every record is asked with: two entries that are no entry
    /// at all are one, an entry and no entry are two - the equality and the inequality each saying it, from either
    /// side - and the entry asked about itself says no to the things it is not: a word, a state, a number, the
    /// answer the entries stand in, another entry of the same name and the anonymous thing that happens to carry two
    /// of its parts.</summary>
    [Fact]
    public void NoEntryIsOneWithNoEntryAndNotWithAnythingThatIsNotAnEntry()
    {
        HealthCheckEntry? nothing = null;
        HealthCheckEntry? alsoNothing = null;
        var there = Bare();
        var answer = new HealthDetail([there], ReadAt);

        Assert.Null(default(HealthCheckEntry));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("Queue"));
        Assert.False(there.Equals(CheckState.Unknown));
        Assert.False(there.Equals(15002.3));
        Assert.False(there.Equals(answer));
        Assert.False(there.Equals(new { Name = "Queue", State = CheckState.Unknown }));
        Assert.False(there.Equals(new HealthCheckEntry("Queue", CheckState.Healthy)));
        Assert.False(there.Equals(new HealthCheckEntry("Queue", CheckState.Unknown, string.Empty)));
        Assert.False(answer.Equals(there));
    }

    // ----- the entry falling apart, and being copied

    /// <summary>The four parts fall out in the order the constructor names them - the name, the state, the words and
    /// the milliseconds - as the very text the entry was made of, and what came out goes back in the same way. The
    /// entry that is only a name and a state falls apart to the two parts a check may leave unsaid.</summary>
    [Fact]
    public void TheFourPartsReadOutOfTheEntryInTheOrderTheyStand()
    {
        var description = "Database connection failed: login timeout expired";
        var entry = new HealthCheckEntry("DataAccess", CheckState.Unhealthy, description, 15002.3);

        var (name, state, words, duration) = entry;

        Assert.Equal("DataAccess", name);
        Assert.Equal(CheckState.Unhealthy, state);
        Assert.Same(description, words);
        Assert.Equal(15002.3, duration);
        Assert.Equal(entry, new HealthCheckEntry(name, state, words, duration));

        var (bareName, bareState, bareWords, bareDuration) = Bare();

        Assert.Equal("Queue", bareName);
        Assert.Equal(CheckState.Unknown, bareState);
        Assert.Null(bareWords);
        Assert.Null(bareDuration);
        Assert.Equal(Bare(), new HealthCheckEntry(bareName, bareState, bareWords, bareDuration));
    }

    /// <summary>Any of the four parts may be turned as the entry is copied - the name to another name, the state to
    /// another state, the words to none, the milliseconds to a tenth more - and stands where it is turned to, while
    /// the three the turn does not name stay the very text and numbers the entry was built of. The copy is another
    /// entry and never the same reference, and the entry it was copied from stands as it stood.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherThree()
    {
        var description = "Database connection failed: login timeout expired";
        var entry = new HealthCheckEntry("DataAccess", CheckState.Unhealthy, description, 15002.3);

        var renamed = entry with { Name = "DataAccess2" };

        Assert.Equal("DataAccess2", renamed.Name);
        Assert.Equal(CheckState.Unhealthy, renamed.State);
        Assert.Same(description, renamed.Description);
        Assert.Equal(15002.3, renamed.DurationMs!.Value);
        Assert.NotSame(entry, renamed);
        Assert.NotEqual(entry, renamed);

        var recovered = entry with { State = CheckState.Healthy };

        Assert.True(recovered.IsHealthy);
        Assert.Contains("IsHealthy = True", Words(recovered), StringComparison.Ordinal);
        Assert.Same(description, recovered.Description);
        Assert.Equal(CheckState.Unhealthy, entry.State);

        var silent = entry with { Description = null, DurationMs = null };

        Assert.Null(silent.Description);
        Assert.Null(silent.DurationMs);
        Assert.Equal(entry.Name, silent.Name);
        Assert.Equal(entry.State, silent.State);
        Assert.Equal(Bare("DataAccess", CheckState.Unhealthy), silent);

        var unchanged = entry with { };

        Assert.Equal(entry, unchanged);
        Assert.NotSame(entry, unchanged);
        Assert.Equal(entry.GetHashCode(), unchanged.GetHashCode());
        Assert.Same(description, unchanged.Description);
        Assert.Equal("DataAccess", entry.Name);
    }

    // ----- the words the entry says itself

    /// <summary>The words the entry says its four parts in, in the order the constructor names them, and a fifth
    /// thing with them: the <c>IsHealthy</c> the state works out is said in the words too, since it is a public part
    /// of the entry like the four. The parts a caller left out are the empty places between the mark and the comma,
    /// a state the enum does not name says its number, and no word here is the page's: no colon after a name, no
    /// full stop, no unit and no <c>Not known</c>, which are the words the page says of an entry it is shown.</summary>
    [Fact]
    public void TheEntrySaysItsFourPartsAndTheHealthyTheyMake()
    {
        Assert.Equal(
            "HealthCheckEntry { Name = DataAccess, State = Unhealthy, Description = Database connection failed: login timeout expired, DurationMs = 15002.3, IsHealthy = False }",
            Words(Full()));
        Assert.Equal(
            "HealthCheckEntry { Name = Queue, State = Unknown, Description = , DurationMs = , IsHealthy = False }",
            Words(Bare()));
        Assert.Equal(
            "HealthCheckEntry { Name = , State = Healthy, Description = , DurationMs = 0, IsHealthy = True }",
            Words(new HealthCheckEntry(string.Empty, CheckState.Healthy, null, 0)));
        Assert.Equal(
            "HealthCheckEntry { Name = Sqlite, State = Healthy, Description = , DurationMs = 0.04, IsHealthy = True }",
            Words(new HealthCheckEntry("Sqlite", CheckState.Healthy, null, 0.04)));

        foreach (var word in new[] { "Not known", "Took ", " ms", " ago", ": Healthy." })
        {
            Assert.DoesNotContain(word, Words(Full()), StringComparison.Ordinal);
        }

        // The fifth word of the words is the only one a caller did not name, and it is said whichever way it falls.
        Assert.EndsWith("IsHealthy = False }", Words(Bare()), StringComparison.Ordinal);
        Assert.EndsWith("IsHealthy = True }", Words(Bare("Queue", CheckState.Healthy)), StringComparison.Ordinal);
    }

    /// <summary>The number in the entry's own words follows whoever reads them, which is what sets them apart from
    /// the page's: a German reader of the record is told <c>DurationMs = 12,3</c> and of the tooltip
    /// <c>Took 12.3 ms</c>, since the words of the tiles are written for the dashboard and not for whoever looks -
    /// and a name, a state and a text are the same letters to whichever reader, so only the number moves.</summary>
    [Fact]
    public void TheWordsOfANumberFollowTheReaderWhileThePagesWordsDoNot()
    {
        var entry = new HealthCheckEntry("Api", CheckState.Healthy, "API layer is healthy", 12.3);

        Assert.Equal(
            "HealthCheckEntry { Name = Api, State = Healthy, Description = API layer is healthy, DurationMs = 12.3, IsHealthy = True }",
            Words(entry));

        ViewerCulture.German(() =>
        {
            Assert.Contains("DurationMs = 12,3", entry.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain("DurationMs = 12.3", entry.ToString(), StringComparison.Ordinal);
            Assert.Equal(entry, new HealthCheckEntry("Api", CheckState.Healthy, "API layer is healthy", 12.3));

            // The page's words are the page's, whichever reader looks: invariant, and in the unit it chose.
            Assert.Equal("Api: Healthy. API layer is healthy. Took 12.3 ms.", HealthDetailText.Title(entry));
            Assert.Equal("1.2 s", HealthDetailText.Duration(1234.5));
        });
    }

    /// <summary>The words at the ends a number and a state run to, said as the entry says them: the largest and the
    /// smallest real number in the exponent the reader of a log looks for, both infinities by their word, the number
    /// that is no number, a fraction of a millisecond, and the state that is no state by its number.</summary>
    [Theory]
    [InlineData(double.MaxValue, "DurationMs = 1.7976931348623157E+308")]
    [InlineData(double.MinValue, "DurationMs = -1.7976931348623157E+308")]
    [InlineData(double.PositiveInfinity, "DurationMs = Infinity")]
    [InlineData(double.NegativeInfinity, "DurationMs = -Infinity")]
    [InlineData(double.Epsilon, "DurationMs = 5E-324")]
    [InlineData(0.0006, "DurationMs = 0.0006")]
    [InlineData(15002.3, "DurationMs = 15002.3")]
    public void TheWordsSayANumberAtWhateverEndItStandsOn(double milliseconds, string said)
    {
        Assert.Contains(said, Words(new HealthCheckEntry("API", CheckState.Healthy, null, milliseconds)), StringComparison.Ordinal);
    }

    // ----- the surface the entry declares

    /// <summary>The entry's whole public surface, as the running assembly holds it: its four parts named in the
    /// order the constructor names them, of the type the constructor says, and the one word worked out of them
    /// after the four - the members every record is asked, comparing, hashing, saying itself, falling apart and
    /// copying, the two operators kept although the compiler names them specially, since they are the <c>==</c> and
    /// the <c>!=</c> a caller writes - and nothing else to call. A fifth part added to the entry has to be written
    /// here as well, which is the point: a mark cannot be drawn out of a part nobody decided what it means. The way
    /// in is one - the four parts, the last two only for a caller who cares - and the copy the <c>with</c> walks is
    /// the compiler's private one.</summary>
    [Fact]
    public void TheEntryHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(HealthCheckEntry);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal([typeof(IEquatable<HealthCheckEntry>)], type.GetInterfaces());

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Name", "State", "Description", "DurationMs", "IsHealthy"], properties.Select(property => property.Name));
        Assert.Equal(typeof(string), properties[0].PropertyType);
        Assert.Equal(typeof(CheckState), properties[1].PropertyType);
        Assert.Equal(typeof(string), properties[2].PropertyType);
        Assert.Equal(typeof(double?), properties[3].PropertyType);
        Assert.Equal(typeof(bool), properties[4].PropertyType);
        Assert.All(properties, property => Assert.True(property.CanRead));
        Assert.All(properties.Take(4), property => Assert.NotNull(property.SetMethod));
        Assert.Null(properties[4].SetMethod);

        // The word the state works out is no contract the entry could be compared by: only the four parts are.
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));

        Assert.Equal(
            ["Deconstruct", "Description", "DurationMs", "Equals", "GetHashCode", "IsHealthy", "Name", "State", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Equal(4, arguments.Length);
        Assert.Equal(["Name", "State", "Description", "DurationMs"], arguments.Select(argument => argument.Name));
        Assert.False(arguments[0].HasDefaultValue);
        Assert.False(arguments[1].HasDefaultValue);
        Assert.True(arguments[2].HasDefaultValue);
        Assert.True(arguments[3].HasDefaultValue);
        Assert.Null(arguments[2].DefaultValue);
        Assert.Null(arguments[3].DefaultValue);

        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);

        // The two overloads of the equality every record is asked with, and the one Deconstruct.
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Name", "State", "Description", "DurationMs"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));
    }

    /// <summary>The public members the entry declares itself: its five properties and the methods that are not the
    /// get and the set of one, with the record's compiler-named clone walked past - all of it the list a caller can
    /// write against, and nothing on it that nobody decided what it means.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(HealthCheckEntry)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words an entry says its parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own further up
    /// this file, so the strings here are the point and the culture only the way to say them once.</summary>
    private static string Words(HealthCheckEntry entry) => ViewerCulture.Invariant(() => entry.ToString()!);
}
