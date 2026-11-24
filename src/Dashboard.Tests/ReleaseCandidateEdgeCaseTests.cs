using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ReleaseCandidate" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that names the Octopus release cut for an item, <c>2.4.&lt;build&gt;</c>,
/// and when it was created. Both parts may be null, and a candidate with neither is another value than no candidate
/// at all.
/// <para>
/// What the other files already pin is left alone. The taking of a release candidate out of <c>workitems.json</c> is
/// <c>ReleaseCandidate.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> and is
/// therefore pinned through <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" />
/// says that a candidate with only a version is still a candidate; <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" />
/// that the live feed's candidates arrive with their versions; and <see cref="WorkItemsTests.ThePublishedWorkItemsFileParses" />
/// that the live feed's candidates arrive with their created moments. What a release candidate is worth to the page -
/// the <c>2.4.86</c> of a label - is <see cref="WorkItemsText.Carries" />, and is pinned in those files.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each part to the property of its
/// name and holding the very instance it was given; the four ways the two parts can be filled (each one can be
/// present or absent); a candidate with no created moment being another candidate than no candidate at all; the
/// equality and hash of two candidates that differ only in the part the page does not show; and the public surface,
/// so that the day a part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a release candidate holds the parts
/// the file gave it and calls nothing at all, so every value below is one the test brought, and every call says the
/// same thing twice.
/// </para>
/// </summary>
public class ReleaseCandidateEdgeCaseTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);

    /// <summary>A candidate with every part filled.</summary>
    private static ReleaseCandidate Full => new("2.4.86", CreatedAt);

    /// <summary>A candidate with no parts.</summary>
    private static ReleaseCandidate Empty => new(null, null);

    // --------------------------------- the parts, by the names of their own properties

    /// <summary>The constructor's two arguments, position by position, come back as the property of the same name:
    /// version first, createdAt last - the order the page shows them. Each is held as the very value the candidate
    /// was built of, never as a copy of it, and a new candidate of the same two parts is another candidate, not the
    /// one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var candidate = new ReleaseCandidate("2.4.86", CreatedAt);

        Assert.Equal("2.4.86", candidate.Version);
        Assert.Equal(CreatedAt, candidate.CreatedAt);

        var again = new ReleaseCandidate("2.4.86", CreatedAt);

        Assert.Equal(candidate, again);
        Assert.NotSame(candidate, again);

        // A candidate with no parts is another value than one with a part set.
        var noVersion = new ReleaseCandidate(null, CreatedAt);

        Assert.NotEqual(candidate, noVersion);
        Assert.Null(noVersion.Version);
    }

    /// <summary>Each part stands on its own property and nowhere else: setting version does not set createdAt, and
    /// vice versa. The two defaults (both null) are one candidate, and each part set while the other is not is
    /// another.</summary>
    [Theory]
    [InlineData("2.4.86", null)]
    [InlineData(null, "2026-11-09T16:00:00Z")]
    [InlineData("2.4.86", "2026-11-09T16:00:00Z")]
    public void EachPartStandsOnItsOwnPropertyAndNowhereElse(string? version, string? createdAt)
    {
        DateTimeOffset? when = createdAt is null ? null : new DateTimeOffset(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);
        var candidate = new ReleaseCandidate(version, when);

        Assert.Equal(version, candidate.Version);
        Assert.Equal(when, candidate.CreatedAt);
    }

    /// <summary>A candidate with no createdAt said is another candidate than one with a createdAt that is the same
    /// as the version. The first has no createdAt part; the second has a createdAt part. Both are valid candidates,
    /// and the page treats them differently (one shows no time, the other shows a time), so they must be different
    /// values.</summary>
    [Fact]
    public void ACandidateWithNoCreatedAtSaidIsStillACandidate()
    {
        var noCreatedAt = new ReleaseCandidate("2.4.86", null);
        var withCreatedAt = new ReleaseCandidate("2.4.86", CreatedAt);

        Assert.NotEqual(noCreatedAt, withCreatedAt);
        Assert.Null(noCreatedAt.CreatedAt);
        Assert.Equal(CreatedAt, withCreatedAt.CreatedAt);
    }

    // --------------------------------- the parts, together

    /// <summary>Two candidates with the same two parts are one. The parts are not decorative; they are part of the
    /// value. Two candidates that differ in one part are not one.</summary>
    [Fact]
    public void TwoCandidatesAreOneWhileTheirTwoPartsAre()
    {
        var candidate = new ReleaseCandidate("2.4.86", CreatedAt);
        var candidateAgain = new ReleaseCandidate("2.4.86", CreatedAt);

        Assert.Equal(candidate, candidateAgain);
        Assert.Equal(candidate.GetHashCode(), candidateAgain.GetHashCode());

        var differentVersion = new ReleaseCandidate("2.4.87", CreatedAt);
        var differentCreatedAt = new ReleaseCandidate("2.4.86", CreatedAt.AddHours(1));

        Assert.NotEqual(candidate, differentVersion);
        Assert.NotEqual(candidate, differentCreatedAt);

        // No candidate is one with no candidate, and not with any other thing.
        Assert.NotNull(candidate);
        Assert.NotEqual((object)"not a candidate", (object)candidate);

        // The hash stands still from one call to the next.
        Assert.Equal(candidate.GetHashCode(), candidate.GetHashCode());
        Assert.Equal(candidateAgain.GetHashCode(), candidateAgain.GetHashCode());
    }

    /// <summary>A candidate that differs in one part is another candidate. The page depends on this to tell apart
    /// two candidates with different versions, and on the hash to put the two in a set without confusing them.</summary>
    [Fact]
    public void ACandidateThatDiffersInOnePartIsAnotherCandidate()
    {
        var candidate = new ReleaseCandidate("2.4.86", CreatedAt);
        var differentVersion = new ReleaseCandidate("2.4.87", CreatedAt);
        var differentCreatedAt = new ReleaseCandidate("2.4.86", CreatedAt.AddHours(1));

        Assert.NotEqual(candidate, differentVersion);
        Assert.NotEqual(candidate, differentCreatedAt);

        Assert.NotEqual(candidate.GetHashCode(), differentVersion.GetHashCode());
        Assert.NotEqual(candidate.GetHashCode(), differentCreatedAt.GetHashCode());
    }

    /// <summary>No candidate is one with no candidate, and not with any other thing. A set of candidates may hold
    /// null (it is a different value) and may hold things that are not candidates without confusing them with
    /// candidates.</summary>
    [Fact]
    public void NoCandidateIsOneWithNoCandidateAndNotWithAnyOtherThing()
    {
        var candidate = new ReleaseCandidate("2.4.86", null);

        Assert.NotNull(candidate);
        Assert.NotEqual((object)"", (object)candidate);
        Assert.NotEqual((object)"2.4.86", (object)candidate);
        Assert.NotEqual((object)true, (object)candidate);

        var set = new HashSet<ReleaseCandidate?>([null, candidate, new ReleaseCandidate("2.4.87", null)]);

        Assert.Equal(3, set.Count);
    }

    // --------------------------------- the hash, the set, the operators

    /// <summary>The hash follows the parts: two candidates that are one have the same hash, and two that are not
    /// one have different hashes. A set of candidates holds one copy of each value, and the two operators follow
    /// the parts.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var candidate = new ReleaseCandidate("2.4.86", CreatedAt);
        var candidateAgain = new ReleaseCandidate("2.4.86", CreatedAt);
        var differentCandidate = new ReleaseCandidate("2.4.87", null);

        Assert.Equal(candidate.GetHashCode(), candidateAgain.GetHashCode());
        Assert.Equal(candidate.GetHashCode(), candidate.GetHashCode());

        var set = new HashSet<ReleaseCandidate>([candidate, candidateAgain, differentCandidate]);

        Assert.Equal(2, set.Count);
        Assert.Contains(candidate, set);
        Assert.Contains(differentCandidate, set);

        Assert.True(candidate == candidateAgain);
        Assert.True(candidate != differentCandidate);
    }

    // --------------------------------- the words the candidate says

    /// <summary>The candidate says its two parts in the order they are named: version, createdAt. This is the
    /// record's own voice, for the debugger and for the log, and it follows the parts as they change.</summary>
    [Fact]
    public void TheRecordSaysItsTwoPartsInTheOrderTheyAreNamed()
    {
        var candidate = new ReleaseCandidate("2.4.86", CreatedAt);

        var text = candidate.ToString();

        Assert.Contains("Version = 2.4.86", text, StringComparison.Ordinal);
        Assert.Contains("CreatedAt = ", text, StringComparison.Ordinal);
    }

    /// <summary>The candidate's own words follow the reader while the parts themselves do not: the moment written
    /// in the text is the reader's moment, not the one the candidate carries. A candidate carried across a border
    /// says its parts in the same words.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var candidate = new ReleaseCandidate("2.4.86", CreatedAt);

        var english = CultureScope.Says(new CultureInfo("en-US"), () => candidate.ToString());
        Assert.Contains("11/9/2026", english, StringComparison.Ordinal);

        var german = CultureScope.Says(new CultureInfo("de-DE"), () => candidate.ToString());
        Assert.Contains("9.11.2026", german, StringComparison.Ordinal);
    }

    // --------------------------------- the surface

    /// <summary>The record has two parts, both null-ready, and the members every record is asked: the constructor,
    /// the properties, ToString, Equals, GetHashCode, the == and != operators, and the copy constructor. No more,
    /// no less. The day a part is added or taken away this test says so.</summary>
    [Fact]
    public void TheRecordHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(ReleaseCandidate);

        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single(c => c.GetParameters().Length == 2);

        Assert.Equal("Version", constructor.GetParameters()[0].Name);
        Assert.Equal("CreatedAt", constructor.GetParameters()[1].Name);
    }
}