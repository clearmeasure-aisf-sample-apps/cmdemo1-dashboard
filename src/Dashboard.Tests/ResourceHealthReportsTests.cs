using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The shared test builder <see cref="ResourceHealthReports" /> (#1251) builds exactly what the private <c>Said</c> and
/// <c>Report</c> helpers of the resource-health and unplaced tests build: a reading defaulting to the read moment, and a
/// report of readings found whatever the case of the id.
/// </summary>
public class ResourceHealthReportsTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

    private const string Kv = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv";

    private const string RegistryId = "/subscriptions/s/resourceGroups/rg-demo1-prod/providers/Microsoft.ContainerRegistry/registries/acrdemo1";

    private static readonly ResourceHealthReports Health = new(Read);

    [Fact]
    public void TheBuilderRecordsTheMomentTheFileWasRead()
    {
        Assert.Equal(Read, Health.Read);
        Assert.Equal(Read.AddDays(-8), new ResourceHealthReports(Read.AddDays(-8)).Read);
    }

    [Fact]
    public void AReadingSaidWithOnlyAStatusIsFromResourceHealthAtTheReadMomentWithNoSummary()
    {
        Assert.Equal(new ResourceHealthReading("available", ResourceHealthReading.FromResourceHealth, Read), Health.Said("available"));
        Assert.Null(Health.Said("available").Summary);
    }

    [Fact]
    public void AReadingKeepsEverythingTheTestSays()
    {
        var at = Read.AddHours(-3);

        var reading = Health.Said("failed", ResourceHealthReading.FromProvisioningState, at, "the deployment failed");

        Assert.Equal(new ResourceHealthReading("failed", ResourceHealthReading.FromProvisioningState, at, "the deployment failed"), reading);
    }

    [Fact]
    public void AReportIsGeneratedAtTheReadMomentAndHoldsEveryReadingByItsId()
    {
        var available = Health.Said("available");
        var degraded = Health.Said("degraded", at: Read.AddMinutes(-5));

        var report = Health.Report((Kv, available), (RegistryId, degraded));

        Assert.Equal(Read, report.Generated);
        Assert.Equal(2, report.Readings.Count);
        Assert.Same(available, report.Find(Kv));
        Assert.Same(degraded, report.Find(RegistryId));
    }

    [Fact]
    public void AReportFindsAReadingWhateverTheCaseOfTheIdAsked()
    {
        var reading = Health.Said("available");

        var report = Health.Report((Kv, reading));

        Assert.Same(reading, report.Find(Kv.ToUpperInvariant()));
        Assert.Same(reading, report.Find(Kv.ToLowerInvariant()));
        Assert.Null(report.Find(RegistryId));
    }

    [Fact]
    public void AReportOfNoReadingsWasReadAndSaidNothing()
    {
        var report = Health.Report();

        Assert.Equal(Read, report.Generated);
        Assert.Empty(report.Readings);
        Assert.Null(report.Find(Kv));
    }

    [Fact]
    public void TwoReadingsForTheSameIdInAnyCaseAreRefusedAsTheFileCannotSayBoth()
    {
        Assert.Throws<ArgumentException>(() => Health.Report((Kv, Health.Said("available")), (Kv.ToUpperInvariant(), Health.Said("degraded"))));
    }

    [Fact]
    public void OneProdReadingIsTheParsedFileWithItsCheckedAtMoment()
    {
        var report = ResourceHealthReports.OneProdReading(RegistryId, "degraded", ResourceHealthReading.FromResourceHealth);

        var reading = Assert.Single(report.Readings).Value;
        Assert.Equal(new ResourceHealthReading("degraded", ResourceHealthReading.FromResourceHealth, Read), reading);
        Assert.Same(reading, report.Find(RegistryId.ToUpperInvariant()));
    }

    [Fact]
    public void ABoxReadsTheBuiltReportAsItReadsTheParsedOne()
    {
        var built = Health.Report((RegistryId, Health.Said("degraded")));
        var parsed = ResourceHealthReports.OneProdReading(RegistryId, "degraded", ResourceHealthReading.FromResourceHealth);

        var fromBuilt = ResourceHealthText.Of([RegistryId], built, Read.AddMinutes(40), TimeZoneInfo.Utc);
        var fromParsed = ResourceHealthText.Of([RegistryId], parsed, Read.AddMinutes(40), TimeZoneInfo.Utc);

        Assert.NotNull(fromBuilt);
        Assert.Equal(fromParsed, fromBuilt);
        Assert.Equal("Degraded", fromBuilt.Label);
    }
}
