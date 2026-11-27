namespace Dashboard.Tests;

/// <summary>
/// The words <see cref="WorkItemsText.Rework" /> gives a rework measure that has every part at once: failures,
/// reverts and a fractional rate, joined as <c>a · b · c</c>. Fixed inputs only: no network, no clock.
/// </summary>
public class WorkMeasureTextSeparatorTests
{
    [Fact]
    public void FailuresRevertsAndAFractionalRateAreJoinedByAMiddleDot() =>
        Assert.Equal("3 functional-test failures · 2 reverts · 12.5 %", WorkItemsText.Rework(new WorkRework(3, 2, 0.125)));

    [Fact]
    public void ARateAtOneIsShownAsAPercent() =>
        Assert.Equal("100 %", WorkItemsText.Rework(new WorkRework(null, null, 1.0)));

    [Fact]
    public void ARateAboveOneIsKeptAsANumber() =>
        Assert.Equal("1.5", WorkItemsText.Rework(new WorkRework(null, null, 1.5)));
}
