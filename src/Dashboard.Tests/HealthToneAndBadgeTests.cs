namespace Dashboard.Tests;

/// <summary>
/// The two fixed sets of words of Resource Health on a box: the tone (<see cref="HealthTone"/>) and the order of the
/// badges (<see cref="HealthBadge"/>, as <see cref="ResourceHealthText.Of"/> shows the most urgent). The words are written out here
/// rather than taken from the app: a word the page shows a reader is a word a change to has to break.
/// </summary>
public class HealthToneAndBadgeTests
{
    [Fact]
    public void TheTonesAreThePagesTwoWordsAndNeverAColour()
    {
        Assert.Equal("plain", HealthTone.Plain);
        Assert.Equal("muted", HealthTone.Muted);
        Assert.Equal("plain", HealthTone.Of(recent: true));
        Assert.Equal("muted", HealthTone.Of(recent: false));
    }

    /// <summary>The order the boxes have always shown: a resource that is down, then a provisioning state that did not
    /// succeed, then degraded, not known, provisioned and available.</summary>
    [Fact]
    public void TheBadgesAreDeclaredMostUrgentFirst() =>
        Assert.Equal(
            [
                HealthBadge.Unavailable,
                HealthBadge.OtherProvisioningState,
                HealthBadge.Degraded,
                HealthBadge.NotKnown,
                HealthBadge.Provisioned,
                HealthBadge.Available,
            ],
            Enum.GetValues<HealthBadge>().Order());

    [Theory]
    [InlineData("unavailable", "resourceHealth", "failed", "Unavailable")]
    [InlineData("degraded", "resourceHealth", "failed", "Failed")]
    [InlineData("updating", "provisioningState", "unknown", "Updating")]
    [InlineData("succeeded", "provisioningState", "unknown", "Not known")]
    [InlineData("available", "resourceHealth", "succeeded", "Provisioned")]
    [InlineData("degraded", "resourceHealth", "unknown", "Degraded")]
    [InlineData("deleting", "provisioningState", "failed", "Deleting")]
    public void ABoxOfTwoShowsTheMoreUrgentBadge(string first, string firstSource, string second, string expected)
    {
        var report = ResourceHealthReport.Parse($$"""
            { "resources": { "group": [
              { "id": "/s/a", "health": { "status": "{{first}}", "source": "{{firstSource}}", "checkedAt": "2026-10-04T21:00:00Z" } },
              { "id": "/s/b", "health": { "status": "{{second}}", "source": "{{(second == "unknown" ? "none" : "provisioningState")}}", "checkedAt": "2026-10-04T21:00:00Z" } } ] } }
            """);

        var words = ResourceHealthText.Of(["/s/a", "/s/b"], report, null, TimeZoneInfo.Utc);

        Assert.Equal(expected, words!.Label);
        Assert.Equal("muted", words.Tone);
    }

    [Fact]
    public void AnUnplacedEntryWithoutAnIdIsMuted()
    {
        var manifest = new RuntimeManifest("nonprod", [], [], [], Unplaced: [new RuntimeUnplaced("kv")]);

        Assert.Equal("muted", UnplacedText.Rows(manifest, null, null, TimeZoneInfo.Utc).Single().HealthTone);
    }
}
