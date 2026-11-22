namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkColumns" />.
/// <para>
/// What is already pinned where it belongs: <see cref="WorkColumnsTests" /> pins the board order and the active
/// columns, which names <see cref="WorkColumns.IsKnown" /> and <see cref="WorkColumns.IsDone" /> accept, the casing
/// <see cref="WorkColumns.Canonical" /> restores, the blank column shown as <see cref="WorkColumns.Unplaced" /> and the
/// unknown column trimmed but otherwise kept.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the exact letters of each constant; <see cref="WorkColumns.Same" />
/// called directly, with nothing on the left, with blank text, with spacing and with letters that only look alike;
/// the sizes of <see cref="WorkColumns.Order" /> and <see cref="WorkColumns.Active" /> and that the lists cannot be
/// written to; and <see cref="WorkColumns.Canonical" /> not restoring the board's spelling to a known name that came
/// with spaces around it, only trimming it.
/// </para>
/// <para>
/// Nothing here reaches the network or the clock: every value below is one the test brought.
/// </para>
/// </summary>
public class WorkColumnsEdgeCaseTests
{
    // ----- the constants, letter for letter

    [Fact]
    public void EachColumnConstantIsSpelledExactly()
    {
        Assert.Equal("Todo", WorkColumns.Todo);
        Assert.Equal("In Progress", WorkColumns.InProgress);
        Assert.Equal("Review", WorkColumns.Review);
        Assert.Equal("Merged", WorkColumns.Merged);
        Assert.Equal("TDD", WorkColumns.Tdd);
        Assert.Equal("UAT", WorkColumns.Uat);
        Assert.Equal("Done", WorkColumns.Done);
        Assert.Equal("Unplaced", WorkColumns.Unplaced);
    }

    // ----- the two lists

    [Fact]
    public void OrderHoldsSevenColumnsAndActiveSixWithDoneLast()
    {
        Assert.Equal(7, WorkColumns.Order.Count);
        Assert.Equal(6, WorkColumns.Active.Count);
        Assert.Equal(WorkColumns.Todo, WorkColumns.Order[0]);
        Assert.Equal(WorkColumns.Done, WorkColumns.Order[^1]);
        Assert.Equal(WorkColumns.Uat, WorkColumns.Active[^1]);
        Assert.Equal(WorkColumns.Order.Take(6), WorkColumns.Active);
        Assert.Equal(WorkColumns.Order.Count, WorkColumns.Order.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void NeitherListCanBeWrittenTo()
    {
        Assert.Throws<NotSupportedException>(() => ((ICollection<string>)WorkColumns.Order).Add("Blocked"));
        Assert.Throws<NotSupportedException>(() => ((ICollection<string>)WorkColumns.Active).Clear());
    }

    [Fact]
    public void EveryActiveColumnIsKnownAndNotDone()
    {
        foreach (var column in WorkColumns.Active)
        {
            Assert.True(WorkColumns.IsKnown(column));
            Assert.False(WorkColumns.IsDone(column));
        }
    }

    // ----- Same, called directly

    [Theory]
    [InlineData("Review", "Review", true)]
    [InlineData("review", "REVIEW", true)]
    [InlineData("In progress", "in PROGRESS", true)]
    [InlineData("", "", true)]
    [InlineData(" ", " ", true)]
    [InlineData("", " ", false)]
    [InlineData(" Review", "Review", false)]
    [InlineData("Review ", "Review", false)]
    [InlineData("InProgress", "In Progress", false)]
    [InlineData("In\tProgress", "In Progress", false)]
    [InlineData("Todo", "To do", false)]
    [InlineData("TДD", "TDD", false)]
    [InlineData(null, "", false)]
    [InlineData(null, "Done", false)]
    public void SameForgivesCasingOnlyAndNothingElse(string? left, string right, bool same) =>
        Assert.Equal(same, WorkColumns.Same(left, right));

    [Fact]
    public void SameIsSymmetricForTwoNonNullNames()
    {
        Assert.True(WorkColumns.Same("uat", "UAT"));
        Assert.True(WorkColumns.Same("UAT", "uat"));
        Assert.False(WorkColumns.Same("Done", "Done "));
        Assert.False(WorkColumns.Same("Done ", "Done"));
    }

    // ----- IsDone and IsKnown at the edges left open

    [Theory]
    [InlineData("dOnE", true)]
    [InlineData("\tDone", false)]
    [InlineData("Done.", false)]
    [InlineData("Don", false)]
    [InlineData("Merged", false)]
    [InlineData("UAT", false)]
    public void IsDoneAtTheEdges(string column, bool done) =>
        Assert.Equal(done, WorkColumns.IsDone(column));

    [Theory]
    [InlineData("IN PROGRESS", true)]
    [InlineData("in progress", true)]
    [InlineData("InProgress", false)]
    [InlineData("\tTodo", false)]
    [InlineData("\n", false)]
    [InlineData("TDD UAT", false)]
    public void IsKnownAtTheEdges(string column, bool known) =>
        Assert.Equal(known, WorkColumns.IsKnown(column));

    // ----- Canonical at the edges left open

    [Theory]
    [InlineData(" todo ", "todo")]
    [InlineData("\tDONE\n", "DONE")]
    [InlineData(" UAT", "UAT")]
    public void AKnownNameWithSpacesAroundItIsTrimmedButNotRespelled(string column, string expected)
    {
        Assert.Equal(expected, WorkColumns.Canonical(column));
        Assert.False(WorkColumns.IsKnown(column));
    }

    [Theory]
    [InlineData("\n", "Unplaced")]
    [InlineData("\r\n\t ", "Unplaced")]
    [InlineData(" ", "Unplaced")]
    public void AnyWhitespaceAloneIsUnplaced(string column, string expected) =>
        Assert.Equal(expected, WorkColumns.Canonical(column));

    [Theory]
    [InlineData("x", "x")]
    [InlineData("Blocked", "Blocked")]
    [InlineData("unplaced", "unplaced")]
    public void AnUnknownNameIsKeptInItsOwnLetters(string column, string expected) =>
        Assert.Equal(expected, WorkColumns.Canonical(column));

    [Fact]
    public void CanonicalIsStableWhenAppliedTwice()
    {
        foreach (var column in new string?[] { null, "", "tdd", "Blocked ", "Unplaced" })
        {
            var once = WorkColumns.Canonical(column);
            Assert.Equal(once, WorkColumns.Canonical(once));
        }
    }

    [Fact]
    public void APaddedKnownNameOnlyReachesTheBoardsSpellingOnTheSecondPass()
    {
        var once = WorkColumns.Canonical(" review ");
        Assert.Equal("review", once);
        Assert.Equal(WorkColumns.Review, WorkColumns.Canonical(once));
    }

    [Fact]
    public void ACanonicalKnownNameReturnsTheVeryStringOfTheBoard()
    {
        Assert.Same(WorkColumns.InProgress, WorkColumns.Canonical("in progress"));
        Assert.Same(WorkColumns.Done, WorkColumns.Canonical("DONE"));
    }
}
