namespace Dashboard.Tests;

/// <summary>
/// One row of the "In no environment" list: <see cref="UnplacedRow"/>, the record <see cref="UnplacedText.Rows"/>
/// builds for each entry — its entry, and the health words of the diagram's resource boxes.
/// </summary>
public class UnplacedRowTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

    private const string RegistryId = "/subscriptions/s/resourceGroups/rg-demo1-prod/providers/Microsoft.ContainerRegistry/registries/acrdemo1";

    private const string VaultId = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv-b";

    private static RuntimeManifest Manifest(string unplaced) =>
        RuntimeManifestParser.ParseManifest($$"""{ "environment": "tdd", "nodes": [], "unplaced": {{unplaced}} }""").Value!;

    private static ResourceHealthReport Report(string id, string status, string source) =>
        ResourceHealthReport.Parse($$"""
            { "resources": { "prod": [ { "id": "{{id}}", "health": { "status": "{{status}}", "source": "{{source}}", "checkedAt": "2026-10-12T09:00:00Z" } } ] } }
            """)!;

    private static UnplacedRow TheRow(RuntimeManifest manifest, ResourceHealthReport? report, DateTimeOffset? now) =>
        Assert.Single(UnplacedText.Rows(manifest, report, now, TimeZoneInfo.Utc));

    [Fact]
    public void ARecentReadingExposesTheEntryAndEveryWordOfTheRow()
    {
        var manifest = Manifest($$"""
            [ { "name": "acrdemo1", "type": "Container registry", "group": "rg-demo1-prod", "words": "no environment tag", "ids": ["{{RegistryId}}"] } ]
            """);
        var entry = Assert.Single(manifest.Unplaced!);

        var row = TheRow(manifest, Report(RegistryId, "available", "resourceHealth"), Read.AddMinutes(40));

        Assert.Same(entry, row.Entry);
        Assert.Equal("acrdemo1", row.Entry.Name);
        Assert.Equal("Container registry", row.Entry.Type);
        Assert.Equal("rg-demo1-prod", row.Entry.Group);
        Assert.Equal("no environment tag", row.Entry.Words);
        Assert.Equal([RegistryId], row.Entry.Ids);
        Assert.Equal("Available · Resource Health, 40 min ago", row.Health);
        Assert.Equal("plain", row.HealthTone);
        Assert.Equal(
            $"{ResourceHealthText.Source}\nacrdemo1: available (by Resource Health), read 2026-10-12 09:00 +00:00 (40 min ago).",
            row.HealthTitle);
    }

    [Fact]
    public void AnOldReadingSaysHowLongAgoAndIsMuted()
    {
        var manifest = Manifest($$"""[ { "name": "acrdemo1", "ids": ["{{RegistryId}}"] } ]""");

        var row = TheRow(manifest, Report(RegistryId, "degraded", "resourceHealth"), Read.AddHours(5));

        Assert.Same(manifest.Unplaced![0], row.Entry);
        Assert.Equal("Degraded · Resource Health, 5 h ago", row.Health);
        Assert.Equal("muted", row.HealthTone);
        Assert.Equal(
            $"{ResourceHealthText.Source}\nacrdemo1: degraded (by Resource Health), read 2026-10-12 09:00 +00:00 (5 h ago).",
            row.HealthTitle);
    }

    [Fact]
    public void WithoutATimeTheLineHasNoAgeAndIsMuted()
    {
        var manifest = Manifest($$"""[ { "name": "acrdemo1", "ids": ["{{RegistryId}}"] } ]""");

        var row = TheRow(manifest, Report(RegistryId, "available", "resourceHealth"), null);

        Assert.Equal("Available · Resource Health", row.Health);
        Assert.Equal("muted", row.HealthTone);
        Assert.Equal(
            $"{ResourceHealthText.Source}\nacrdemo1: available (by Resource Health), read 2026-10-12 09:00 +00:00.",
            row.HealthTitle);
    }

    [Fact]
    public void AnIdNotInTheReadingSaysNotKnown()
    {
        var manifest = Manifest($$"""[ { "name": "kv-b", "ids": ["{{VaultId}}"] } ]""");

        var row = TheRow(manifest, Report(RegistryId, "available", "resourceHealth"), Read.AddMinutes(40));

        Assert.Equal("kv-b", row.Entry.Name);
        Assert.Equal("Not known · not in the hourly reading", row.Health);
        Assert.Equal("muted", row.HealthTone);
        Assert.Equal($"{ResourceHealthText.Source}\nkv-b: not in the hourly reading.", row.HealthTitle);
    }

    [Fact]
    public void BeforeThePageReadTheFileTheRowSaysNotKnown()
    {
        var manifest = Manifest($$"""[ { "name": "acrdemo1", "ids": ["{{RegistryId}}"] } ]""");

        var row = TheRow(manifest, null, Read);

        Assert.Equal("Not known · no Resource Health read yet", row.Health);
        Assert.Equal("muted", row.HealthTone);
        Assert.Equal(
            $"{ResourceHealthText.Source} The page has not read it: containers.json on branch status, next to delivery.json, is not there or not read yet.",
            row.HealthTitle);
    }

    [Fact]
    public void AnEntryWithoutIdsSaysNotReadAndAnEmptyIdListIsTheSame()
    {
        var bare = TheRow(Manifest("""[ { "name": "kv-a", "type": "Key vault" } ]"""),
            Report(RegistryId, "available", "resourceHealth"), Read.AddMinutes(40));
        var empty = TheRow(Manifest("""[ { "name": "kv-b", "ids": [] } ]"""),
            Report(RegistryId, "available", "resourceHealth"), Read.AddMinutes(40));

        foreach (var row in new UnplacedRow[] { bare, empty })
        {
            Assert.Equal(UnplacedText.NotRead, row.Health);
            Assert.Equal("not read", row.Health);
            Assert.Equal("muted", row.HealthTone);
            Assert.Equal(UnplacedText.NotReadTitle, row.HealthTitle);
            Assert.Equal(
                "The manifest has no resource id for it, so the page cannot say what Azure Resource Health said of it.",
                row.HealthTitle);
            Assert.Null(row.Entry.Ids);
        }

        Assert.Equal("kv-a", bare.Entry.Name);
        Assert.Equal("Key vault", bare.Entry.Type);
        Assert.Equal("kv-b", empty.Entry.Name);
    }

    [Fact]
    public void TheRowIsARecordOfItsFourMembers()
    {
        var manifest = Manifest($$"""[ { "name": "acrdemo1", "ids": ["{{RegistryId}}"] } ]""");
        var row = TheRow(manifest, Report(RegistryId, "available", "resourceHealth"), Read.AddMinutes(40));

        // with replaces one member at a time; equality looks at all four.
        var muted = row with { HealthTone = "muted" };
        Assert.Equal("muted", muted.HealthTone);
        Assert.Equal(row.Entry, muted.Entry);
        Assert.Equal(row.Health, muted.Health);
        Assert.Equal(row.HealthTitle, muted.HealthTitle);
        Assert.NotEqual(row, muted);

        Assert.Equal(row, row with { Entry = row.Entry, Health = row.Health, HealthTone = row.HealthTone, HealthTitle = row.HealthTitle });
        Assert.Contains("acrdemo1", row.ToString());
        Assert.Contains("Available · Resource Health, 40 min ago", row.ToString());
        Assert.Contains("plain", row.ToString());
    }
}
