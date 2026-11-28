namespace Dashboard.Tests;

/// <summary>
/// The tooltip of the schema-read line (<see cref="DatabaseText.ReadTitle"/>): the moments in full, in the viewer's
/// zone, and what is how old. The fixtures are the same as in <see cref="DatabaseViewTests"/>, with a fixed
/// <see cref="TimeZoneInfo.Utc"/> so the expected strings are stable.
/// </summary>
public sealed class DatabaseTextReadTitleTests
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.Utc;

    private const string ManifestJson = """
        {
          "environment": "tdd",
          "database": "sqldb-cmdemo1-tdd",
          "readAt": "2026-10-12T10:00:00Z",
          "source": "https://raw.githubusercontent.com/org/repo/main/environments/tdd/database.json",
          "truncated": false,
          "tables": [
            { "alias": "t_dbo_Employee", "qualifiedName": "db.t_dbo_Employee", "schema": "dbo", "name": "Employee", "rows": 123, "columns": 7 },
            { "alias": "t_dbo_WorkOrder", "qualifiedName": "db.t_dbo_WorkOrder", "schema": "dbo", "name": "WorkOrder", "rows": 1234, "columns": 12 }
          ]
        }
        """;

    private const string LiveJson = """
        {
          "generated": "2026-10-12T11:00:00Z",
          "database": "sqldb-cmdemo1-tdd",
          "tables": [
            { "schema": "dbo", "name": "Employee", "rows": 130, "columns": [] },
            { "schema": "dbo", "name": "WorkOrder", "rows": 1, "columns": [] }
          ],
          "truncated": false
        }
        """;

    private static readonly DateTimeOffset ReadAt = new(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Generated = new(2026, 10, 12, 11, 0, 0, TimeSpan.Zero);

    private static DatabaseManifest Manifest(string json = ManifestJson) => DatabaseParser.ParseManifest(json).Value!;

    private static DatabaseLive Live(string json = LiveJson) => DatabaseParser.ParseLive(json)!;

    [Fact]
    public void NoMomentInEitherFileIsNoTooltip()
    {
        var manifest = Manifest("""{ "environment": "tdd", "database": "db", "tables": [] }""");

        Assert.Null(DatabaseText.ReadTitle(manifest, null, Zone));
        Assert.Null(DatabaseText.ReadTitle(manifest, new DatabaseLive(null, [], false), Zone));
    }

    [Fact]
    public void ALiveFileNewerThanTheDeploymentSaysTheRowCountsAreOfItsMomentWithTheDrawnClause()
    {
        var title = DatabaseText.ReadTitle(Manifest(), Live(), Zone);

        Assert.Equal(
            $"The row counts and the list are of {TimeText.DateAndTime(Generated, Zone)}. The diagram was drawn at the environment's last deployment, {TimeText.DateAndTime(ReadAt, Zone)}.",
            title);
    }

    [Fact]
    public void ALiveFileWithoutADeploymentMomentSaysTheRowCountsAreOfItsMomentWithoutTheDrawnClause()
    {
        var manifest = Manifest() with { ReadAt = null };

        Assert.Equal(
            $"The row counts and the list are of {TimeText.DateAndTime(Generated, Zone)}.",
            DatabaseText.ReadTitle(manifest, Live(), Zone));
    }

    [Fact]
    public void AManifestMomentNewerThanTheLiveFileSaysTheDeploymentReadTheSchema()
    {
        var manifest = Manifest() with { ReadAt = Generated.AddHours(1) };

        Assert.Equal(
            $"The deployment read the schema {TimeText.DateAndTime(Generated.AddHours(1), Zone)}: the diagram is as old as the environment's last deployment of the dashboard.",
            DatabaseText.ReadTitle(manifest, Live(), Zone));
    }

    [Fact]
    public void AManifestMomentWithNoLiveFileSaysTheDeploymentReadTheSchema()
    {
        Assert.Equal(
            $"The deployment read the schema {TimeText.DateAndTime(ReadAt, Zone)}: the diagram is as old as the environment's last deployment of the dashboard.",
            DatabaseText.ReadTitle(Manifest(), null, Zone));
    }

    [Fact]
    public void ANullManifestOrZoneIsRefused()
    {
        Assert.Throws<ArgumentNullException>(() => DatabaseText.ReadTitle(null!, null, Zone));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.ReadTitle(Manifest(), null, null!));
    }
}
