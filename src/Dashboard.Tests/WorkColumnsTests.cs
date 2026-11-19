namespace Dashboard.Tests;

/// <summary>
/// The board's columns as <see cref="WorkColumns"/> names them: the seven the file may give, in board order, plus the
/// name a blank or unknown column is shown under. Casing is forgiven, spacing is not.
/// </summary>
public class WorkColumnsTests
{
    [Fact]
    public void TheOrderIsTheBoardLeftToRightWithDoneLast()
    {
        Assert.Equal(["Todo", "In Progress", "Review", "Merged", "TDD", "UAT", "Done"], WorkColumns.Order);
        Assert.Equal(["Todo", "In Progress", "Review", "Merged", "TDD", "UAT"], WorkColumns.Active);
        Assert.DoesNotContain(WorkColumns.Done, WorkColumns.Active);
        Assert.DoesNotContain(WorkColumns.Unplaced, WorkColumns.Active);
    }

    [Theory]
    [InlineData("Todo", true)]
    [InlineData("In Progress", true)]
    [InlineData("Review", true)]
    [InlineData("Merged", true)]
    [InlineData("TDD", true)]
    [InlineData("UAT", true)]
    [InlineData("Done", true)]
    [InlineData("tdd", true)]
    [InlineData("u a t", false)]
    [InlineData("todo ", false)]
    [InlineData("Unplaced", false)]
    [InlineData("Blocked", false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData(null, false)]
    public void OnlyTheSevenNamesOfTheBoardAreKnown(string? column, bool known) =>
        Assert.Equal(known, WorkColumns.IsKnown(column));

    [Theory]
    [InlineData("Done", true)]
    [InlineData("done", true)]
    [InlineData("DONE", true)]
    [InlineData("Done ", false)]
    [InlineData(" In Progress", false)]
    [InlineData("Unplaced", false)]
    [InlineData("Review", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void DoneIsTheOnlyColumnThatIsFinished(string? column, bool done) =>
        Assert.Equal(done, WorkColumns.IsDone(column));

    [Fact]
    public void AColumnOfTheBoardKeepsItsOwnSpelling()
    {
        foreach (var name in WorkColumns.Order)
        {
            Assert.Equal(name, WorkColumns.Canonical(name));
            Assert.True(WorkColumns.IsKnown(name));
        }
    }

    [Theory]
    [InlineData("todo", "Todo")]
    [InlineData("IN PROGRESS", "In Progress")]
    [InlineData("reViEw", "Review")]
    [InlineData("merged", "Merged")]
    [InlineData("tdd", "TDD")]
    [InlineData("uat", "UAT")]
    [InlineData("done", "Done")]
    public void CasingIsNormalisedToTheBoardsOwnSpelling(string column, string expected) =>
        Assert.Equal(expected, WorkColumns.Canonical(column));

    [Theory]
    [InlineData(null, "Unplaced")]
    [InlineData("", "Unplaced")]
    [InlineData(" ", "Unplaced")]
    [InlineData("\t ", "Unplaced")]
    public void NothingInTheColumnFieldIsShownAsUnplaced(string? column, string expected) =>
        Assert.Equal(expected, WorkColumns.Canonical(column));

    [Theory]
    [InlineData("  Review  ", "Review")]
    [InlineData("Blocked  ", "Blocked")]
    [InlineData("  in progress", "in progress")]
    [InlineData("In  Progress", "In  Progress")]
    [InlineData("Unplaced", "Unplaced")]
    public void AColumnThatIsNotOnTheBoardKeepsTheFilesOwnSpelling(string column, string expected) =>
        Assert.Equal(expected, WorkColumns.Canonical(column));

    [Fact]
    public void UnplacedIsActiveLookingWorkThatNobodyHasFiledYet()
    {
        Assert.False(WorkColumns.IsDone(WorkColumns.Unplaced));
        Assert.False(WorkColumns.IsKnown(WorkColumns.Unplaced));
        Assert.Equal(WorkColumns.Unplaced, WorkColumns.Canonical(WorkColumns.Unplaced));
        Assert.True(WorkColumns.Same(WorkColumns.Unplaced, "unplaced"));
    }
}
