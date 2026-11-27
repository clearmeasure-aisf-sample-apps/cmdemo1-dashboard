using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The badge word <see cref="ResourceHealthText.Label" /> makes for each word the file can say, named for the file
/// words it maps from: every Resource Health status maps to its badge word, <c>succeeded</c> from a provisioning state
/// maps to <c>Provisioned</c>, and a word that belongs to neither vocabulary says what it has always said. These are
/// the words whose match used to ride on the capitalising landing on the badge's private word; they are pinned here
/// one mapping at a time, and a box of several proves the badge order still recognizes every word <c>Label</c> makes.
/// </summary>
public class ResourceHealthTextBadgeWordTests
{
    private static readonly ResourceHealthReports Health = new(new DateTimeOffset(2026, 10, 4, 21, 37, 0, TimeSpan.Zero));

    /// <summary>Each word Resource Health can say in containers.json gives the badge of the same name, and only from
    /// its own source: a Resource Health <c>succeeded</c> is no badge word at all and a provisioning state's
    /// <c>available</c> is a provisioning state, capitalised.</summary>
    [Theory]
    [InlineData("available", "Available")]
    [InlineData("degraded", "Degraded")]
    [InlineData("unavailable", "Unavailable")]
    public void AResourceHealthStatusSaysItsBadgeWord(string status, string badge) =>
        Assert.Equal(badge, ResourceHealthText.Label(Health.Said(status)));

    /// <summary>A provisioning state of <c>succeeded</c> is the badge word <c>Provisioned</c>, and the other
    /// provisioning states are said as the file says them, the first letter capitalised: <c>failed</c> is
    /// <c>Failed</c>.</summary>
    [Theory]
    [InlineData("succeeded", "Provisioned")]
    [InlineData("failed", "Failed")]
    public void AProvisioningStateSaysItsBadgeOrItsOwnWord(string status, string badge) =>
        Assert.Equal(badge, ResourceHealthText.Label(Health.Said(status, ResourceHealthReading.FromProvisioningState)));

    /// <summary>The words follow their source, not just their letters: Resource Health does not say
    /// <c>succeeded</c> (that is a provisioning state's word) and a provisioning state that says <c>available</c>
    /// is not the badge word <c>Available</c> but a state, capitalised. Each of them lands on its old arm:
    /// <c>Not known</c> and <c>Available</c> respectively.</summary>
    [Fact]
    public void AWordThatIsNotThisVocabularySaysWhatTheArmsHaveAlwaysSaid()
    {
        Assert.Equal("Not known", ResourceHealthText.Label(Health.Said("succeeded")));
        Assert.Equal("Available", ResourceHealthText.Label(Health.Said("available", ResourceHealthReading.FromProvisioningState)));
    }

    /// <summary>A box of one resource in each of the three Resource Health states: the badge is the most urgent,
    /// <c>Unavailable</c>, and the line counts every state the badge's word is made from - which is the proof that
    /// the order still recognizes every word the badge says.</summary>
    [Fact]
    public void ABoxOfTheThreeResourceHealthStatesSaysUnavailableAndCountsEachState()
    {
        var report = Health.Report(
            ("/s/down", Health.Said("unavailable")),
            ("/s/slow", Health.Said("degraded")),
            ("/s/up", Health.Said("available")));

        var words = ResourceHealthText.Of(["/s/down", "/s/slow", "/s/up"], report, Health.Read, TimeZoneInfo.Utc)!;

        Assert.Equal(("Unavailable", "1 unavailable · 1 degraded · 1 available, just now", "plain"), (words.Label, words.Line, words.Tone));
    }
}
