namespace Dashboard.Tests;

/// <summary>
/// The weeks <see cref="WorkBotText.FromItems"/> counts merged pull requests and production deployments into, pinned
/// at the boundaries the week's rule draws: the Monday 00:00 UTC a week starts on, the Sunday 23:59 UTC that belongs
/// to the week of the Monday before it, one week holding both a merge and a production deployment, and the UTC date
/// a time with another offset is taken from.
/// </summary>
public class WorkBotTextWeekEdgeCaseTests
{
    // 2026-11-02 is a Monday and 2026-11-08 its Sunday; both belong to the week that starts 2026-11-02 00:00 UTC.
    private static readonly DateTimeOffset Monday = new(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset SundayNight = new(2026, 11, 8, 23, 59, 0, TimeSpan.Zero);

    private static WorkItem Item(int number, IReadOnlyList<WorkPullRequest> pulls, WorkItemEnvironments? environments = null) =>
        new(new WorkItemIdentity(number, $"Item {number}", null, null, "Done"), [], null, null, [], pulls, null, environments ?? WorkItemEnvironments.None, null, WorkWorker.Unknown, null);

    private static WorkPullRequest Merged(DateTimeOffset at) => new(1, null, "merged", at, null);

    [Fact]
    public void APullMergedAtTheFirstMomentOfAMondaysWeekUtcIsInTheWeekOfThatMonday()
    {
        var stats = WorkBotText.FromItems([Item(1, [Merged(Monday)])]);

        Assert.Equal([new WorkWeek(Monday, 1, 0)], stats.PerWeek);
    }

    [Fact]
    public void APullMergedOnTheLastMinuteOfASundaysDayUtcIsInTheWeekOfTheMondaySixDaysBefore()
    {
        var stats = WorkBotText.FromItems([Item(1, [Merged(SundayNight)])]);

        Assert.Equal([new WorkWeek(Monday, 1, 0)], stats.PerWeek);
    }

    [Fact]
    public void AMergeOnTheMondayAndAProductionDeploymentOnTheSundayAreCountedInOneWeek()
    {
        var stats = WorkBotText.FromItems(
        [
            Item(
                1,
                [Merged(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero))],
                new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", new DateTimeOffset(2026, 11, 8, 18, 0, 0, TimeSpan.Zero)))),
        ]);

        Assert.Equal([new WorkWeek(Monday, 1, 1)], stats.PerWeek);
    }

    [Fact]
    public void ATimedMondayInAnotherOffsetButASundayInUtcIsInThePreviousWeek()
    {
        // 2026-11-09 01:30 at +02:00 is 2026-11-08 23:30 UTC: a Sunday, in the week of 2026-11-02, because the week is
        // taken from UtcDateTime.
        var mondayLocally = new DateTimeOffset(2026, 11, 9, 1, 30, 0, TimeSpan.FromHours(2));
        var stats = WorkBotText.FromItems([Item(1, [Merged(mondayLocally)])]);

        Assert.Equal([new WorkWeek(Monday, 1, 0)], stats.PerWeek);
    }
}
