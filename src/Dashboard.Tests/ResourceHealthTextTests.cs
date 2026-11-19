namespace Dashboard.Tests;

public class ResourceHealthTextTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = Read + TimeSpan.FromMinutes(40);

    private const string KeyVault = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv";
    private const string Identity = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id";
    private const string App = "/subscriptions/s/resourceGroups/apps/providers/Microsoft.App/containerApps/app";

    /// <summary>One reading per source, all read at <see cref="Read"/>.</summary>
    private static ResourceHealthReport Report() => ResourceHealthReport.Parse("""
        { "generated": "2026-10-04T21:37:00Z",
          "resources": { "nonprod": [
            { "id": "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv",
              "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:37:00Z", "summary": "No incidents reported." } },
            { "id": "/subscriptions/s/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id",
              "health": { "status": "succeeded", "source": "provisioningState", "checkedAt": "2026-10-04T21:37:00Z" } },
            { "id": "/subscriptions/s/resourceGroups/apps/providers/Microsoft.App/containerApps/app",
              "health": { "status": "unknown", "source": "none", "checkedAt": "2026-10-04T21:37:00Z" } } ] } }
        """)!;

    /// <summary>The same box, three ways to be unwell, each read at its own time.</summary>
    private static ResourceHealthReport MixedReport() => ResourceHealthReport.Parse("""
        { "resources": { "group": [
          { "id": "/s/apps/web", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:00:00Z" } },
          { "id": "/s/identities/id", "health": { "status": "failed", "source": "provisioningState", "checkedAt": "2026-10-04T21:37:00Z" } },
          { "id": "/s/vaults/kv", "health": { "status": "degraded", "source": "resourceHealth", "checkedAt": "2026-10-04T21:20:00Z" } } ] } }
        """)!;

    [Fact]
    public void ANullOrEmptyListOfIdsHasNoWords()
    {
        var report = Report();

        Assert.Null(ResourceHealthText.Of(null, report, Now, TimeZoneInfo.Utc));
        Assert.Null(ResourceHealthText.Of([], report, Now, TimeZoneInfo.Utc));
    }

    [Fact]
    public void ABoxWithoutAReportSaysNotKnown()
    {
        var words = ResourceHealthText.Of([KeyVault], null, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Not known", "no Resource Health read yet", "muted"), (words.Label, words.Line, words.Tone));
        Assert.Equal(
            $"{ResourceHealthText.Source} The page has not read it: containers.json on branch status, next to delivery.json, is not there or not read yet.",
            words.Title);
    }

    [Theory]
    [InlineData(KeyVault, "Available", "Resource Health, 40 min ago", "plain")]
    [InlineData(Identity, "Provisioned", "provisioning state, 40 min ago", "plain")]
    [InlineData(App, "Not known", "no Resource Health, 40 min ago", "plain")]
    [InlineData("/subscriptions/s/resourceGroups/apps/providers/Microsoft.App/containerApps/other", "Not known", "not in the hourly reading", "muted")]
    public void ABoxOfOneResourceHasItsWordItsSourceAndItsAge(string id, string label, string line, string tone)
    {
        var words = ResourceHealthText.Of([id], Report(), Now, TimeZoneInfo.Utc)!;

        Assert.Equal((label, line, tone), (words.Label, words.Line, words.Tone));
    }

    [Fact]
    public void TheTooltipNamesItsResourceItsStateItsSourceAndItsWhen()
    {
        var words = ResourceHealthText.Of([KeyVault], Report(), Now, TimeZoneInfo.Utc)!;

        Assert.Equal(
            $"{ResourceHealthText.Source}\nkv: available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago). No incidents reported.",
            words.Title);
    }

    [Fact]
    public void ABoxOfSeveralSaysItsWorstStateAndCountsTheRest()
    {
        var words = ResourceHealthText.Of(["/s/apps/web", "/s/identities/id", "/s/vaults/kv"], MixedReport(), Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Failed", "1 failed · 1 degraded · 1 available, 77 min ago", "plain"), (words.Label, words.Line, words.Tone));
    }

    [Fact]
    public void AnUnreadResourceIsWorseThanAProvisionedOne()
    {
        var words = ResourceHealthText.Of([Identity, "/s/missing"], Report(), Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Not known", "1 not known · 1 provisioned, 40 min ago", "plain"), (words.Label, words.Line, words.Tone));
    }

    [Fact]
    public void TheAgeIsHowLongAgoTheOldestReadingCameInAndAnOldBoxIsMuted()
    {
        var words = ResourceHealthText.Of(["/s/apps/web", "/s/identities/id", "/s/vaults/kv"], MixedReport(), Read + TimeSpan.FromMinutes(121), TimeZoneInfo.Utc)!;

        Assert.Equal(("Failed", "1 failed · 1 degraded · 1 available, 2 h ago", "muted"), (words.Label, words.Line, words.Tone));
    }

    [Fact]
    public void WithoutATimeTheLineHasNoAgeAndTheBoxIsMuted()
    {
        var words = ResourceHealthText.Of([KeyVault], Report(), null, TimeZoneInfo.Utc)!;

        Assert.Equal(("Available", "Resource Health", "muted"), (words.Label, words.Line, words.Tone));
        Assert.Equal(
            $"{ResourceHealthText.Source}\nkv: available (by Resource Health), read 2026-10-04 21:37 +00:00. No incidents reported.",
            words.Title);
    }
}
