using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The shared <see cref="Containers"/> ObjectMother (#686) - the one home for the <c>containers.json</c> the Resource
/// Health tests used to retype inline. It is pinned here the way the fixtures are: the fixed moment it reads at, the
/// id-prefix vocabulary it names, the three source words it reaches for, and the guarantees the app's own read gives
/// it - found by id whatever its case, a long summary cut to 200, a refused status left out - and that a report built
/// by <see cref="Containers.Report"/> reads back through the app's own words exactly as a hand-parsed file does, so
/// the builder cannot drift from the read.
/// </summary>
public class ContainersObjectMotherTests
{
    private static readonly DateTimeOffset Read = Containers.Read;

    [Fact]
    public void TheFixtureReadsAtItsFixedMomentAndAsksNoClock()
    {
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 21, 37, 0, TimeSpan.Zero), Read);
        Assert.Equal(TimeSpan.Zero, Read.Offset);
    }

    [Fact]
    public void TheIdPrefixVocabularyIsTheOneTheTestsShare()
    {
        Assert.Equal("/subscriptions/s/resourceGroups/", Containers.IdPrefix);
        Assert.Equal(Containers.IdPrefix + "rg", Containers.ResourceGroup);
        Assert.Equal("/subscriptions/s", Containers.Subscription);
        Assert.StartsWith(Containers.IdPrefix, Containers.KeyVault, StringComparison.Ordinal);
        Assert.StartsWith(Containers.IdPrefix, Containers.Identity, StringComparison.Ordinal);
        Assert.StartsWith(Containers.IdPrefix, Containers.App, StringComparison.Ordinal);
        Assert.EndsWith("kv", Containers.KeyVault, StringComparison.Ordinal);
        Assert.EndsWith("id", Containers.Identity, StringComparison.Ordinal);
        Assert.EndsWith("app", Containers.App, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadingIsFromResourceHealthAtTheReadMomentByDefaultAndKeepsWhatItIsSaid()
    {
        Assert.Equal(
            new ResourceHealthReading("available", ResourceHealthReading.FromResourceHealth, Read),
            Containers.AReading(Containers.KeyVault, "available"));

        Assert.Equal(
            new ResourceHealthReading("failed", ResourceHealthReading.FromProvisioningState, Read.AddMinutes(-5), "the deployment failed"),
            Containers.AReading(Containers.Identity, "failed", ResourceHealthReading.FromProvisioningState, Read.AddMinutes(-5), "the deployment failed"));
    }

    [Fact]
    public void AReportIsGeneratedAtItsReadMomentAndFindsAReadingWhateverTheCaseOfItsId()
    {
        var report = Containers.Report(
            Containers.Reading.Of(Containers.KeyVault, "available", summary: "No incidents reported."),
            Containers.Reading.Of(Containers.Identity, "succeeded", ResourceHealthReading.FromProvisioningState),
            Containers.Reading.Of(Containers.App, "unknown", ResourceHealthReading.None));

        Assert.Equal(Read, report.Generated);
        Assert.Equal(3, report.Readings.Count);
        Assert.Equal("available", report.Find(Containers.KeyVault)?.Status);
        Assert.Equal("available", report.Find(Containers.KeyVault.ToUpperInvariant())?.Status);
        Assert.Equal(ResourceHealthReading.FromProvisioningState, report.Find(Containers.Identity.ToUpperInvariant())?.Source);
        Assert.Equal(ResourceHealthReading.None, report.Find(Containers.App)?.Source);
        Assert.Equal("unknown", report.Find(Containers.App.ToLowerInvariant())?.Status);
        // Found whatever its case: the vault is there in a different lettering.
        Assert.NotNull(report.Find("/SUBSCRIPTIONS/S/RESOURCEGROUPS/RG/PROVIDERS/MICROSOFT.KEYVAULT/VAULTS/KV"));
        Assert.Null(report.Find("/subscriptions/s/resourceGroups/rg/providers/Microsoft.Web/serverFarms/none"));
    }

    [Fact]
    public void TheThreeSourceWordsReachForTheThreeWordsThePageUses()
    {
        var report = Containers.Report(
            Containers.Reading.Of(Containers.KeyVault, "available", ResourceHealthReading.FromResourceHealth),
            Containers.Reading.Of(Containers.Identity, "succeeded", ResourceHealthReading.FromProvisioningState),
            Containers.Reading.Of(Containers.App, "unknown", ResourceHealthReading.None));

        Assert.Equal(ResourceHealthReading.FromResourceHealth, report.Find(Containers.KeyVault)?.Source);
        Assert.Equal(ResourceHealthReading.FromProvisioningState, report.Find(Containers.Identity)?.Source);
        Assert.Equal(ResourceHealthReading.None, report.Find(Containers.App)?.Source);
    }

    [Fact]
    public void ALongSummaryIsCutToTwoHundredByTheReadAndARefusedStatusIsLeftOut()
    {
        var longReport = Containers.Report(Containers.Reading.Of(Containers.KeyVault, "available", summary: new string('x', 500)));
        Assert.Equal(200, longReport.Find(Containers.KeyVault)!.Summary!.Length);

        var refused = Containers.Report(Containers.Reading.Of(Containers.KeyVault, "<b>up</b>"));
        Assert.Empty(refused.Readings);
    }

    [Fact]
    public void AReportOfNoReadingsWasReadAtTheFixedMomentAndSaidNothing()
    {
        var report = Containers.Report();

        Assert.Equal(Read, report.Generated);
        Assert.Empty(report.Readings);
    }

    [Fact]
    public void AGeneratedDifferentFromTheReadMomentIsKept()
    {
        var report = Containers.Report(Read.AddMinutes(-40), Containers.Reading.Of(Containers.KeyVault, "available"));

        Assert.Equal(Read.AddMinutes(-40), report.Generated);
        Assert.Equal(Read, report.Find(Containers.KeyVault)?.CheckedAt);
    }

    [Fact]
    public void ABoxReadsTheBuiltReportAsItReadsTheHandParsedFile()
    {
        // The builder's report and the file the test parsed by hand are the same report to the page's own read.
        var built = Containers.Report(
            Containers.Reading.Of(Containers.KeyVault, "available", summary: "No incidents reported."),
            Containers.Reading.Of(Containers.Identity, "succeeded", ResourceHealthReading.FromProvisioningState),
            Containers.Reading.Of(Containers.App, "unknown", ResourceHealthReading.None));

        var parsed = ResourceHealthReport.Parse("""
            { "generated": "2026-10-04T21:37:00Z",
              "resources": { "nonprod": [
                { "id": "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv",
                  "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:37:00Z", "summary": "No incidents reported." } },
                { "id": "/subscriptions/s/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id",
                  "health": { "status": "succeeded", "source": "provisioningState", "checkedAt": "2026-10-04T21:37:00Z" } },
                { "id": "/subscriptions/s/resourceGroups/apps/providers/Microsoft.App/containerApps/app",
                  "health": { "status": "unknown", "source": "none", "checkedAt": "2026-10-04T21:37:00Z" } } ] } }
            """)!;

        var now = Read.AddMinutes(40);
        // Same readings, in the same words: the builder's report and the hand-parsed file agree reading for reading.
        Assert.Equal(parsed.Readings.Count, built.Readings.Count);
        Assert.Equal(parsed.Generated, built.Generated);
        Assert.Equal(parsed.Readings.OrderBy(r => r.Key, StringComparer.OrdinalIgnoreCase).Select(r => r.Value).ToList(),
            built.Readings.OrderBy(r => r.Key, StringComparer.OrdinalIgnoreCase).Select(r => r.Value).ToList());
        Assert.Equal(
            ResourceHealthText.Of([Containers.KeyVault], parsed, now, TimeZoneInfo.Utc),
            ResourceHealthText.Of([Containers.KeyVault], built, now, TimeZoneInfo.Utc));

        // The box the page makes of the built report says the words it says of the parsed one: same label, line and tone.
        var words = ResourceHealthText.Of([Containers.KeyVault], built, now, TimeZoneInfo.Utc)!;
        Assert.Equal(("Available", "Resource Health, 40 min ago", "plain"), (words.Label, words.Line, words.Tone));
    }
}
