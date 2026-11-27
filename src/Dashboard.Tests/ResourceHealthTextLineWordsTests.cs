namespace Dashboard.Tests;

/// <summary>
/// The line words <see cref="ResourceHealthText.Of"/> says of a box, for the cases the word tests in
/// <see cref="ResourceHealthTextWordsTests"/> do not pin: a box of several with mixed readings puts the part separator
/// between the counts, and a single-resource box for each of the four <c>Origin</c> branches says its phrase.
/// </summary>
public class ResourceHealthTextLineWordsTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    private static string Id(int number) => $"/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv{number}";

    private static ResourceHealthReport Report(params (string Id, ResourceHealthReading Reading)[] readings) =>
        new(Read, readings.ToDictionary(entry => entry.Id, entry => entry.Reading, StringComparer.OrdinalIgnoreCase));

    private static ResourceHealthReading Health(string status, DateTimeOffset? at = null) =>
        new(status, ResourceHealthReading.FromResourceHealth, at ?? Read);

    [Fact]
    public void MixedReadingsOfSeveralResourcesPutThePartSeparatorBetweenTheCounts()
    {
        var report = Report((Id(1), Health("degraded")), (Id(2), Health("available")), (Id(3), Health("available")));
        var now = Read.AddMinutes(40);

        var line = ResourceHealthText.Of([Id(1), Id(2), Id(3)], report, now, TimeZoneInfo.Utc)!.Line;

        Assert.Equal("1 degraded · 2 available, 40 min ago", line);
        Assert.Contains(" · ", line);
    }

    [Fact]
    public void SingleResourceBoxSaysThePhraseOfItsSourceBranch()
    {
        var now = Read.AddMinutes(40);

        var fromHealth = Report((Id(1), Health("available")));
        Assert.Equal("Resource Health", ResourceHealthText.Of([Id(1)], fromHealth, null, TimeZoneInfo.Utc)!.Line);

        var fromProvisioning = Report((Id(1), new ResourceHealthReading("succeeded", ResourceHealthReading.FromProvisioningState, Read)));
        Assert.Equal("provisioning state", ResourceHealthText.Of([Id(1)], fromProvisioning, null, TimeZoneInfo.Utc)!.Line);

        var noSource = Report((Id(1), new ResourceHealthReading("available", ResourceHealthReading.None, Read)));
        Assert.Equal("no Resource Health", ResourceHealthText.Of([Id(1)], noSource, null, TimeZoneInfo.Utc)!.Line);

        var notInReading = Report((Id(2), Health("available")));
        Assert.Equal("not in the hourly reading", ResourceHealthText.Of([Id(1)], notInReading, null, TimeZoneInfo.Utc)!.Line);
    }

    [Fact]
    public void NoReportSaysNotReadYet()
    {
        Assert.Equal("no Resource Health read yet", ResourceHealthText.Of([Id(1)], null, null, TimeZoneInfo.Utc)!.Line);
    }
}
