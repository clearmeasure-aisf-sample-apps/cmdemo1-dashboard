namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="AksAvailability" />, the verdict Azure Resource
/// Health gives the AKS service. <see cref="ClusterParserTests" /> pins that the sample file reads as one exact
/// verdict, and <see cref="ClusterAssessmentTests" /> and <see cref="ClusterEventDetectorTests" /> pin how a
/// verdict of each state is treated; neither pins the verdict itself. What is pinned here: the four words the
/// constants own; that the verdict keeps what it was given - the empty and the absent as well as the full, and
/// the two ends of the clock; and how it compares, copies and reads out - equal while all four parts are,
/// different in any one of them, and its state as the file spelled it, whatever the case.
/// </summary>
public class AksAvailabilityEdgeCaseTests
{
    /// <summary>The moment the sample's verdict occurred: the one <see cref="ClusterParserTests" /> reads.</summary>
    private static readonly DateTimeOffset Occurred = new(2026, 10, 3, 14, 5, 0, TimeSpan.Zero);

    /// <summary>
    /// The constants are the only place the file's spelling meets the code's: the page reads the state the file
    /// writes, and the code decides when it is one of the four. The four words, as Azure spells them, stand here
    /// so a change of spelling is a change the tests see.
    /// </summary>
    [Fact]
    public void TheFourStatesAreTheFourWordsAzureUses()
    {
        Assert.Equal("Available", AksAvailability.Available);
        Assert.Equal("Unavailable", AksAvailability.Unavailable);
        Assert.Equal("Degraded", AksAvailability.Degraded);
        Assert.Equal("Unknown", AksAvailability.Unknown);
    }

    /// <summary>
    /// A full verdict keeps its four parts as given: a state, a sentence, a reason - the reason the sample file
    /// never names - and the moment it occurred.
    /// </summary>
    [Fact]
    public void TheVerdictKeepsWhatItWasGiven()
    {
        var verdict = new AksAvailability(AksAvailability.Degraded, "The API server is slow.", "An upgrade is in progress.", Occurred);

        Assert.Equal((AksAvailability.Degraded, "The API server is slow.", "An upgrade is in progress.", Occurred), (verdict.State, verdict.Summary, verdict.Reason, verdict.OccurredAt));
    }

    /// <summary>
    /// The non-nullable state is a string the file wrote, not a word the code chose: the empty one, the
    /// whitespace one and the one in a case of its own are kept as they stand, and the parts the file does not
    /// name are null and the empty string and the whitespace, apart.
    /// </summary>
    [Theory]
    [InlineData("", null, null)]
    [InlineData("   ", "", "")]
    [InlineData("available", "  ", null)]
    [InlineData("UnAvailable", null, " ")]
    public void AnEmptyOrAbsentPartIsKeptAsItStood(string state, string? summary, string? reason)
    {
        var verdict = new AksAvailability(state, summary, reason, null);

        Assert.Null(verdict.OccurredAt);
        Assert.Equal((state, summary, reason), (verdict.State, verdict.Summary, verdict.Reason));
    }

    /// <summary>The two ends of <see cref="DateTimeOffset" /> itself are kept, and a moment stands in the offset
    /// the file wrote it in: the verdict keeps the offset with the moment.</summary>
    [Fact]
    public void TheEndsOfTheClockAndTheOffsetTheMomentStoodInAreKept()
    {
        Assert.Equal(DateTimeOffset.MinValue, new AksAvailability(AksAvailability.Unknown, null, null, DateTimeOffset.MinValue).OccurredAt);
        Assert.Equal(DateTimeOffset.MaxValue, new AksAvailability(AksAvailability.Unknown, null, null, DateTimeOffset.MaxValue).OccurredAt);

        var at = new DateTimeOffset(2026, 10, 3, 4, 5, 0, TimeSpan.FromHours(-10));
        Assert.Equal(at, new AksAvailability(AksAvailability.Available, null, null, at).OccurredAt);
    }

    /// <summary>Two verdicts are the same one while all four parts are: the equality of the operator and of
    /// <c>Equals</c>, and the hash code that follows them, standing where it stands from call to call.</summary>
    [Fact]
    public void TwoVerdictsAreEqualWhenAllFourPartsAre()
    {
        var one = new AksAvailability(AksAvailability.Available, "No known problems.", "None", Occurred);
        var two = new AksAvailability(AksAvailability.Available, "No known problems.", "None", Occurred);

        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
    }

    /// <summary>A verdict differs in any one of its four parts: a state in another case is another state, for
    /// the record compares as the file spells it - the case-free comparison is <c>AksService.Is</c>'s, not the
    /// record's - and a moment that stands one tick later or in another offset is another moment.</summary>
    [Fact]
    public void AVerdictDiffersInAnyOnePart()
    {
        var verdict = new AksAvailability(AksAvailability.Available, "No known problems.", "None", Occurred);

        Assert.True(verdict != verdict with { State = "Degraded" });
        Assert.NotEqual(verdict, verdict with { State = "available" });
        Assert.NotEqual(verdict, verdict with { State = " " });
        Assert.NotEqual(verdict, verdict with { Summary = null });
        Assert.NotEqual(verdict, verdict with { Summary = "" });
        Assert.NotEqual(verdict, verdict with { Reason = null });
        Assert.NotEqual(verdict, verdict with { Reason = " " });
        Assert.NotEqual(verdict, verdict with { OccurredAt = null });
        Assert.NotEqual(verdict, verdict with { OccurredAt = Occurred.AddTicks(1) });
        Assert.NotEqual(verdict, verdict with { OccurredAt = new DateTimeOffset(Occurred.UtcTicks, TimeSpan.FromHours(2)) });
    }

    /// <summary><c>with</c> names the parts it changes and leaves the rest, and the verdict it stood in stands
    /// as it stood: a copy is equal and not the same, and a copy with no part named is the same verdict again.</summary>
    [Fact]
    public void WithChangesOnlyThePartsItNames()
    {
        var verdict = new AksAvailability(AksAvailability.Available, "No known problems.", "None", Occurred);
        var copy = verdict with { };

        Assert.Equal(verdict, copy);
        Assert.NotSame(verdict, copy);

        var changed = verdict with { State = AksAvailability.Degraded };
        Assert.Equal((AksAvailability.Degraded, "No known problems.", "None", Occurred), (changed.State, changed.Summary, changed.Reason, changed.OccurredAt));
        Assert.Equal(verdict, verdict with { });
        Assert.Equal(AksAvailability.Available, verdict.State);

        Assert.Equal(
            new AksAvailability(AksAvailability.Unknown, "No sentence.", "No reason.", null),
            verdict with { State = AksAvailability.Unknown, Summary = "No sentence.", Reason = "No reason.", OccurredAt = null });
    }

    /// <summary>The four parts read out of the verdict in the order they stand in it.</summary>
    [Fact]
    public void TheFourPartsReadOutOfTheVerdict()
    {
        var (state, summary, reason, at) = new AksAvailability(AksAvailability.Degraded, "Slow.", "Upgrading.", Occurred);

        Assert.Equal((AksAvailability.Degraded, "Slow.", "Upgrading.", Occurred), (state, summary, reason, at));
    }

    /// <summary>The verdict says its parts in their order, the name of each with the value it stands on - the
    /// part the file did not name says its name and nothing else. (The value is written out as it is, so a
    /// rewording of a part is a change the tests see.)</summary>
    [Fact]
    public void TheVerdictSaysItsPartsInTheirOrder()
    {
        Assert.Equal(
            "AksAvailability { State = Available, Summary = No known problems., Reason = , OccurredAt =  }",
            new AksAvailability(AksAvailability.Available, "No known problems.", null, null).ToString());
    }

    // ------------------------------------------------------------------ the record's own shape

    /// <summary>
    /// The whole public surface of <see cref="AksAvailability" /> as it stands: its four parts in the order the
    /// constructor names them, the four state words the constants own, and the members every record is asked -
    /// falling apart, comparing, hashing and saying itself - and nothing else. A fifth part, a fifth state word,
    /// or a computed property added to the record without a test is refused here by name.
    /// </summary>
    [Fact]
    public void TheAvailabilityHasFourPartsAndTheFourStateWordsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["State", "Summary", "Reason", "OccurredAt"],
            RecordSurface.PropertyNamesOf<AksAvailability>());

        Assert.Equal(
            [
                "Available", "Deconstruct", "Degraded", "Equals", "GetHashCode", "OccurredAt", "Reason",
                "State", "Summary", "ToString", "Unavailable", "Unknown", "op_Equality", "op_Inequality",
            ],
            RecordSurface.WithFieldsOf(typeof(AksAvailability)));

        Assert.Equal(
            ["Available", "Degraded", "Unavailable", "Unknown"],
            RecordSurface.FieldNamesOf<AksAvailability>());

        var parameters = RecordSurface.ParametersOf<AksAvailability>();
        Assert.Equal(["State", "Summary", "Reason", "OccurredAt"], [.. parameters.Select(p => p.Name!)]);
        Assert.Equal([false, true, true, true], RecordSurface.NullableParts<AksAvailability>());
    }
}
