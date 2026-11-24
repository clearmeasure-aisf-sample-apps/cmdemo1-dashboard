namespace Dashboard.Tests;

/// <summary>
/// Each of the four words <see cref="ResourceHealthText.Of"/> says of a box, one rule at a time (#683): the badge is the
/// most urgent label and the first of equals, the line is the source for one and the counts for several, the tone turns
/// at exactly <see cref="HourlyCheckText.Recent"/>, and the tooltip names eight and counts the rest.
/// </summary>
public class ResourceHealthTextWordsTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    private static string Id(int number) => $"/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv{number}";

    private static ResourceHealthReport Report(params (string Id, ResourceHealthReading Reading)[] readings) =>
        new(Read, readings.ToDictionary(entry => entry.Id, entry => entry.Reading, StringComparer.OrdinalIgnoreCase));

    private static ResourceHealthReading Health(string status, DateTimeOffset? at = null) =>
        new(status, ResourceHealthReading.FromResourceHealth, at ?? Read);

    [Fact]
    public void BadgeIsTheMostUrgentLabelWhateverTheOrderOfTheIds()
    {
        var report = Report((Id(1), Health("available")), (Id(2), Health("degraded")), (Id(3), new ResourceHealthReading("failed", ResourceHealthReading.FromProvisioningState, Read)));

        Assert.Equal("Failed", ResourceHealthText.Of([Id(1), Id(2), Id(3)], report, Read, TimeZoneInfo.Utc)!.Label);
        Assert.Equal("Failed", ResourceHealthText.Of([Id(3), Id(2), Id(1)], report, Read, TimeZoneInfo.Utc)!.Label);
        Assert.Equal("Degraded", ResourceHealthText.Of([Id(1), Id(2)], report, Read, TimeZoneInfo.Utc)!.Label);
    }

    [Fact]
    public void BadgeOfTwoDifferentProvisioningStatesIsTheFirstInIdOrder()
    {
        var report = Report(
            (Id(1), new ResourceHealthReading("updating", ResourceHealthReading.FromProvisioningState, Read)),
            (Id(2), new ResourceHealthReading("failed", ResourceHealthReading.FromProvisioningState, Read)));

        Assert.Equal("Updating", ResourceHealthText.Of([Id(1), Id(2)], report, Read, TimeZoneInfo.Utc)!.Label);
        Assert.Equal("Failed", ResourceHealthText.Of([Id(2), Id(1)], report, Read, TimeZoneInfo.Utc)!.Label);
    }

    [Fact]
    public void LineOfOneIsItsSourceAndOfSeveralIsTheCountsMostUrgentFirstEachWithTheAgeOfTheOldest()
    {
        var report = Report((Id(1), Health("available", Read)), (Id(2), Health("degraded", Read.AddMinutes(30))), (Id(3), Health("available", Read.AddMinutes(30))));
        var now = Read.AddMinutes(40);

        Assert.Equal("Resource Health, 40 min ago", ResourceHealthText.Of([Id(1)], report, now, TimeZoneInfo.Utc)!.Line);
        Assert.Equal("1 degraded · 2 available, 40 min ago", ResourceHealthText.Of([Id(3), Id(1), Id(2)], report, now, TimeZoneInfo.Utc)!.Line);
        Assert.Equal("1 degraded · 1 available, 10 min ago", ResourceHealthText.Of([Id(2), Id(3)], report, now, TimeZoneInfo.Utc)!.Line);
        Assert.Equal("not in the hourly reading", ResourceHealthText.Of([Id(9)], report, now, TimeZoneInfo.Utc)!.Line);
        Assert.Equal("1 not known · 1 available, 40 min ago", ResourceHealthText.Of([Id(1), Id(9)], report, now, TimeZoneInfo.Utc)!.Line);
        Assert.Equal("Resource Health", ResourceHealthText.Of([Id(1)], report, null, TimeZoneInfo.Utc)!.Line);
    }

    [Fact]
    public void ToneIsPlainUpToExactlyRecentOfTheOldestReadingThenMuted()
    {
        var report = Report((Id(1), Health("available", Read)), (Id(2), Health("available", Read.AddHours(1))));

        Assert.Equal(HealthTone.Plain, ResourceHealthText.Of([Id(1), Id(2)], report, Read + HourlyCheckText.Recent, TimeZoneInfo.Utc)!.Tone);
        Assert.Equal(HealthTone.Muted, ResourceHealthText.Of([Id(1), Id(2)], report, Read + HourlyCheckText.Recent + TimeSpan.FromSeconds(1), TimeZoneInfo.Utc)!.Tone);
        Assert.Equal(HealthTone.Plain, ResourceHealthText.Of([Id(2)], report, Read + HourlyCheckText.Recent + TimeSpan.FromSeconds(1), TimeZoneInfo.Utc)!.Tone);
        Assert.Equal(HealthTone.Muted, ResourceHealthText.Of([Id(1)], report, null, TimeZoneInfo.Utc)!.Tone);
        Assert.Equal(HealthTone.Muted, ResourceHealthText.Of([Id(9)], report, Read, TimeZoneInfo.Utc)!.Tone);
    }

    [Fact]
    public void TitleNamesEightResourcesAndCountsTheRestOnlyPastEight()
    {
        var ids = Enumerable.Range(1, 10).Select(Id).ToList();
        var report = Report(ids.Select(id => (id, Health("available"))).ToArray());

        var eight = ResourceHealthText.Of(ids.Take(8).ToList(), report, null, TimeZoneInfo.Utc)!.Title.Split('\n');
        Assert.Equal(9, eight.Length);
        Assert.Equal(ResourceHealthText.Source, eight[0]);
        Assert.Equal("kv8: available (by Resource Health), read 2026-10-04 21:37 +00:00.", eight[^1]);

        var ten = ResourceHealthText.Of(ids, report, null, TimeZoneInfo.Utc)!.Title.Split('\n');
        Assert.Equal(10, ten.Length);
        Assert.Equal("kv8: available (by Resource Health), read 2026-10-04 21:37 +00:00.", ten[^2]);
        Assert.Equal("And 2 more.", ten[^1]);
    }
}
