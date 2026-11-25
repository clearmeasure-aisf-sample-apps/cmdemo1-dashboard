using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The file's bot and team words, read in one place: an item's <c>worker</c> (read by <see cref="WorkItemsReport.Parse" />)
/// and a filter's audience (<see cref="WorkItemsText.AudienceOf" />) come from the same words, and each keeps its own
/// default - Unknown for a worker, All for an audience. The shared reader is internal to an app that declares no
/// <c>InternalsVisibleTo</c>, so it is pinned through the file.
/// </summary>
public class WorkWorkerWordTests
{
    private static WorkWorker WorkerOf(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "title": "One", "worker": {{JsonSerializer.Serialize(word)}} } ] }""";
        return Assert.Single(WorkItemsReport.Parse(json)!.Items).Worker;
    }

    [Theory]
    [InlineData("bot", WorkWorker.Bot)]
    [InlineData("team", WorkWorker.Team)]
    [InlineData("BOT", WorkWorker.Bot)]
    [InlineData("Team", WorkWorker.Team)]
    [InlineData("  bot  ", WorkWorker.Bot)]
    [InlineData("human", WorkWorker.Unknown)]
    [InlineData("all", WorkWorker.Unknown)]
    [InlineData("bots", WorkWorker.Unknown)]
    [InlineData("", WorkWorker.Unknown)]
    [InlineData("   ", WorkWorker.Unknown)]
    public void AWordReadsAsItsWorkerAndAnythingElseIsUnknown(string word, WorkWorker expected) =>
        Assert.Equal(expected, WorkerOf(word));

    [Theory]
    [InlineData("bot", WorkAudience.Bot)]
    [InlineData("team", WorkAudience.Team)]
    [InlineData("BOT", WorkAudience.Bot)]
    [InlineData("  team  ", WorkAudience.Team)]
    [InlineData("human", WorkAudience.All)]
    [InlineData("", WorkAudience.All)]
    [InlineData(null, WorkAudience.All)]
    public void TheSameWordReadsAsItsAudienceAndAnythingElseIsAll(string? word, WorkAudience expected) =>
        Assert.Equal(expected, WorkItemsText.AudienceOf(word));

    /// <summary>The word a worker is said with on a row reads back as that worker, and as the audience that holds it.</summary>
    [Theory]
    [InlineData(WorkWorker.Bot, WorkAudience.Bot)]
    [InlineData(WorkWorker.Team, WorkAudience.Team)]
    public void AWorkersWordRoundTripsToTheWorkerAndItsAudience(WorkWorker worker, WorkAudience audience)
    {
        var word = WorkItemsText.WorkerLabel(worker)!;
        Assert.Equal(worker, WorkerOf(word));
        Assert.Equal(audience, WorkItemsText.AudienceOf(word));
        Assert.Equal(word, WorkItemsText.AudienceHash(audience));
    }

    /// <summary>The item reader and the filter agree word for word: the audience a word names holds the item that word was read into.</summary>
    [Theory]
    [InlineData("bot")]
    [InlineData("TEAM")]
    [InlineData(" Bot ")]
    [InlineData("robot")]
    [InlineData("")]
    public void AnItemsWorkerAndTheFiltersAudienceComeFromTheSameWord(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "title": "One", "worker": {{JsonSerializer.Serialize(word)}} } ] }""";
        var item = Assert.Single(WorkItemsReport.Parse(json)!.Items);

        Assert.Contains(item, WorkItemsText.Select([item], WorkItemsText.AudienceOf(word)));
    }
}
