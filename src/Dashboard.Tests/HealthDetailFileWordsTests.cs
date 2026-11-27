namespace Dashboard.Tests;

/// <summary>
/// Pins the words <see cref="HealthDetail.Parse" /> reads from ASP.NET Core's detailed health check: the property
/// names are matched exactly, the status words in any case but otherwise letter for letter.
/// </summary>
public class HealthDetailFileWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 27, 7, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StatusesAreMatchedInAnyCase()
    {
        var detail = HealthDetail.Parse(
            """{ "entries": [ { "name": "A", "status": "Healthy" }, { "name": "B", "status": "DEGRADED" }, { "name": "C", "status": "unhealthy" } ] }""",
            Now)!;

        Assert.Equal([CheckState.Healthy, CheckState.Degraded, CheckState.Unhealthy], detail.Entries.Select(entry => entry.State));
        Assert.Equal(["A", "B", "C"], detail.Entries.Select(entry => entry.Name));
    }

    [Fact]
    public void AMisspelledOrAbsentStatusIsUnknownWhileAPaddedOneIsTrimmed()
    {
        var detail = HealthDetail.Parse(
            """{ "entries": [ { "name": "Padded", "status": "Unhealthy " }, { "name": "Misspelled", "status": "Un healthy" }, { "name": "Silent" } ] }""",
            Now)!;

        // JsonRead.Text trims every word of a file, so the padding never reaches the status match.
        Assert.Equal(CheckState.Unhealthy, detail.Find("Padded")!.State);
        Assert.Equal(CheckState.Unknown, detail.Find("Misspelled")!.State);
        Assert.Equal(CheckState.Unknown, detail.Find("Silent")!.State);
    }

    [Fact]
    public void AnEntryWithoutANameIsLeftOut()
    {
        var detail = HealthDetail.Parse(
            """{ "entries": [ { "status": "Healthy" }, { "Name": "Cased", "status": "Healthy" }, { "name": "Kept", "status": "Healthy" } ] }""",
            Now)!;

        var entry = Assert.Single(detail.Entries);
        Assert.Equal("Kept", entry.Name);
    }

    [Fact]
    public void TheDescriptionAndTheDurationAreCarriedThrough()
    {
        var detail = HealthDetail.Parse(
            """{ "entries": [ { "name": "DataAccess", "status": "Healthy", "description": "Database connection successful", "durationMs": 42.1 } ] }""",
            Now)!;

        var entry = Assert.Single(detail.Entries);
        Assert.Equal("Database connection successful", entry.Description);
        Assert.Equal(42.1, entry.DurationMs);
        Assert.Equal(Now, detail.ReadAt);
    }

    [Fact]
    public void PropertyNamesAreCaseSensitive()
    {
        Assert.Null(HealthDetail.Parse("""{ "Entries": [ { "name": "A", "status": "Healthy" } ] }""", Now));

        var detail = HealthDetail.Parse(
            """{ "entries": [ { "name": "A", "Status": "Healthy", "Description": "words", "DurationMs": 3 } ] }""",
            Now)!;
        var entry = Assert.Single(detail.Entries);
        Assert.Equal(CheckState.Unknown, entry.State);
        Assert.Null(entry.Description);
        Assert.Null(entry.DurationMs);
    }
}
