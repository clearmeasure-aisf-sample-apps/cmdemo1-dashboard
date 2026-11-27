namespace Dashboard.Tests;

/// <summary>
/// <see cref="WorkColumnGroup"/> is one column of the board and the items that sit in it, in the file's order.
/// How <see cref="WorkItemsText.Active"/> fills the groups is pinned elsewhere (<see cref="WorkItemsTests"/>,
/// <see cref="WorkItemsTextEdgeCaseTests"/>, <see cref="WorkItemsTextBoundaryTests"/>); what is pinned here is the
/// record's own value semantics: the parts kept as given when blank, empty or absent, equality (the item list is
/// compared by reference, not by its contents), copies, deconstruction and its words.
/// </summary>
public class WorkColumnGroupEdgeCaseTests
{
    private static WorkItem Item(int? number, string? column = null) =>
        new(new WorkItemIdentity(number, $"Item {number}", null, null, column), [], null, null, [], [], null, WorkItemEnvironments.None, null);

    // ----- the parts, kept as given -----

    [Theory]
    [InlineData(WorkColumns.Todo)]
    [InlineData(WorkColumns.Unplaced)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" todo ")]
    public void TheColumnIsKeptExactlyAsGivenAndNotTidied(string column)
    {
        Assert.Equal(column, new WorkColumnGroup(column, []).Column);
    }

    [Fact]
    public void TheItemsAreTheSameListInstanceInTheGivenOrder()
    {
        IReadOnlyList<WorkItem> items = [Item(3, "Todo"), Item(1, "Todo"), Item(2, "Todo")];

        var group = new WorkColumnGroup(WorkColumns.Todo, items);

        Assert.Same(items, group.Items);
        Assert.Equal([3, 1, 2], group.Items.Select(item => item.Identity.Number));
    }

    [Fact]
    public void AnEmptyGroupIsKeptAndNotRefused()
    {
        Assert.Empty(new WorkColumnGroup(WorkColumns.Review, []).Items);
    }

    [Fact]
    public void ItemsOfAnotherColumnAreKeptBecauseTheGroupDoesNotCheckThem()
    {
        var group = new WorkColumnGroup(WorkColumns.Todo, [Item(1, WorkColumns.Done), Item(2, null)]);

        Assert.Equal([1, 2], group.Items.Select(item => item.Identity.Number));
    }

    [Fact]
    public void AbsentPartsAreKeptAbsentAndNotRefused()
    {
        var group = new WorkColumnGroup(null!, null!);

        Assert.Null(group.Column);
        Assert.Null(group.Items);
    }

    // ----- equality -----

    [Fact]
    public void TwoGroupsWithTheSameColumnAndTheSameListAreEqualAndHashAlike()
    {
        IReadOnlyList<WorkItem> items = [Item(1, "Todo")];
        var first = new WorkColumnGroup(WorkColumns.Todo, items);
        var second = new WorkColumnGroup(WorkColumns.Todo, items);

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void EqualItemsInADifferentListAreADifferentGroup()
    {
        var first = new WorkColumnGroup(WorkColumns.Todo, [Item(1, "Todo")]);
        var second = new WorkColumnGroup(WorkColumns.Todo, [Item(1, "Todo")]);

        Assert.Equal(first.Items, second.Items);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void TwoCallsOfActiveGiveGroupsThatAreNotEqual()
    {
        IReadOnlyList<WorkItem> items = [Item(1, WorkColumns.Todo)];

        Assert.NotEqual(WorkItemsText.Active(items)[0], WorkItemsText.Active(items)[0]);
    }

    [Fact]
    public void ADifferentColumnIsADifferentGroupAndCasingMatters()
    {
        IReadOnlyList<WorkItem> items = [];
        var group = new WorkColumnGroup(WorkColumns.Todo, items);

        Assert.NotEqual(group, new WorkColumnGroup(WorkColumns.Review, items));
        Assert.NotEqual(group, new WorkColumnGroup(WorkColumns.Todo.ToUpperInvariant(), items));
    }

    [Fact]
    public void AGroupIsNotEqualToNothingOrToAnotherKindOfObject()
    {
        var group = new WorkColumnGroup(WorkColumns.Todo, []);

        Assert.False(group.Equals(null));
        Assert.False(group.Equals((object)WorkColumns.Todo));
        Assert.False(group == null);
        Assert.True(group != null);
        Assert.True(null == (WorkColumnGroup?)null);
    }

    [Fact]
    public void GroupsWithAbsentPartsAreEqualToEachOther()
    {
        Assert.Equal(new WorkColumnGroup(null!, null!), new WorkColumnGroup(null!, null!));
        Assert.NotEqual(new WorkColumnGroup(null!, null!), new WorkColumnGroup(string.Empty, null!));
    }

    // ----- copies, deconstruction, words -----

    [Fact]
    public void ACopyThatTurnsTheColumnKeepsTheSameItemsAndLeavesTheOriginal()
    {
        var original = new WorkColumnGroup(WorkColumns.Todo, [Item(1)]);
        var copy = original with { Column = WorkColumns.Unplaced };

        Assert.Equal(WorkColumns.Unplaced, copy.Column);
        Assert.Same(original.Items, copy.Items);
        Assert.Equal(WorkColumns.Todo, original.Column);
    }

    [Fact]
    public void ACopyThatTurnsNothingIsEqualButNotTheSameInstance()
    {
        var original = new WorkColumnGroup(WorkColumns.Review, [Item(1)]);
        var copy = original with { };

        Assert.Equal(original, copy);
        Assert.NotSame(original, copy);
    }

    [Fact]
    public void AGroupFallsApartIntoItsColumnAndItems()
    {
        IReadOnlyList<WorkItem> items = [Item(7)];

        var (column, found) = new WorkColumnGroup(WorkColumns.Unplaced, items);

        Assert.Equal(WorkColumns.Unplaced, column);
        Assert.Same(items, found);
    }

    [Fact]
    public void AGroupSaysItsColumnInItsWords()
    {
        Assert.StartsWith("WorkColumnGroup { Column = Todo, Items = ", new WorkColumnGroup("Todo", []).ToString(), StringComparison.Ordinal);
        Assert.Equal("WorkColumnGroup { Column = , Items =  }", new WorkColumnGroup(null!, null!).ToString());
    }
}
