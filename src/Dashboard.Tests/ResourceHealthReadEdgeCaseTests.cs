namespace Dashboard.Tests;

/// <summary>
/// Boundary tests of <c>ResourceHealthReading.Read</c>, the rule that turns one record's <c>health</c> of
/// <c>containers.json</c> into a reading. <c>Read</c> is internal and the app declares no <c>InternalsVisibleTo</c>,
/// so every case here is a one-resource document driven through the public <see cref="ResourceHealthReport.Parse" />.
/// Already pinned in <see cref="ResourceHealthTests" /> and not said again: a missing time, an empty status, markup,
/// a number for a status, a null <c>health</c>, a status lowered from <c>Degraded</c>, an unknown source read as
/// <c>none</c> and a summary cut to 200 characters. Nothing here reaches the network or asks a clock.
/// </summary>
public class ResourceHealthReadEdgeCaseTests
{
    private const string Id = "/s/kv";
    private const string At = "\"2026-10-04T21:37:00Z\"";

    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    /// <summary>The report of a file with one resource whose <c>health</c> is <paramref name="health" />.</summary>
    private static ResourceHealthReport Parse(string health) =>
        ResourceHealthReport.Parse($$"""{ "resources": { "nonprod": [ { "id": "{{Id}}", "health": {{health}} } ] } }""")!;

    private static string Health(string status, string checkedAt = At) =>
        $$"""{ "status": "{{status}}", "source": "resourceHealth", "checkedAt": {{checkedAt}} }""";

    [Fact]
    public void AStatusOfExactlyFortyCharactersIsKept()
    {
        var status = new string('a', 40);

        Assert.Equal(status, Parse(Health(status)).Find(Id)?.Status);
    }

    [Fact]
    public void AStatusOfFortyCharactersWithSpacesInsideIsKept()
    {
        var status = new string('a', 19) + "  " + new string('b', 19);

        Assert.Equal(status, Parse(Health(status)).Find(Id)?.Status);
    }

    [Fact]
    public void AStatusOfFortyOneCharactersIsDropped()
    {
        var report = Parse(Health(new string('a', 41)));

        Assert.Null(report.Find(Id));
        Assert.Empty(report.Readings);
    }

    [Theory]
    [InlineData("available1")]
    [InlineData("2xx")]
    [InlineData("up-ish")]
    [InlineData("not_known")]
    [InlineData("ok.")]
    [InlineData("ok!")]
    [InlineData("état")]
    [InlineData("ÉTAT")]
    [InlineData("up\\tdown")]
    public void AStatusOutsideLowercaseAsciiLettersAndSpacesIsDropped(string status)
    {
        Assert.Empty(Parse(Health(status)).Readings);
    }

    [Fact]
    public void AnUppercaseAsciiStatusIsLoweredAndKept()
    {
        Assert.Equal("partially available", Parse(Health("PARTIALLY AVAILABLE")).Find(Id)?.Status);
    }

    [Fact]
    public void ACheckedAtWithoutAnOffsetIsReadAsUniversalTime()
    {
        var reading = Parse(Health("available", "\"2026-10-04T21:37:00\"")).Find(Id)!;

        Assert.Equal(TimeSpan.Zero, reading.CheckedAt.Offset);
        Assert.Equal(Read, reading.CheckedAt);
        Assert.Equal(Read.UtcTicks, reading.CheckedAt.UtcTicks);
    }

    [Fact]
    public void ACheckedAtWithAnOffsetKeepsItsInstant()
    {
        var reading = Parse(Health("available", "\"2026-10-04T23:37:00+02:00\"")).Find(Id)!;

        Assert.Equal(Read, reading.CheckedAt);
        Assert.Equal(TimeSpan.FromHours(2), reading.CheckedAt.Offset);
    }

    [Theory]
    [InlineData("1791157020")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("""{ "at": "2026-10-04T21:37:00Z" }""")]
    [InlineData("""[ "2026-10-04T21:37:00Z" ]""")]
    [InlineData("\"not a time\"")]
    [InlineData("\"  \"")]
    public void ACheckedAtThatIsNotATimeInTextIsDropped(string checkedAt)
    {
        Assert.Empty(Parse(Health("available", checkedAt)).Readings);
    }

    [Fact]
    public void DuplicateIdsInOneReportLeaveOneReadingTheLastOne()
    {
        var report = ResourceHealthReport.Parse("""
            { "resources": {
                "nonprod": [ { "id": "/s/kv", "health": { "status": "degraded", "source": "resourceHealth", "checkedAt": "2026-10-04T20:37:00Z" } } ],
                "apps": [ { "id": "/S/KV", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:37:00Z" } } ] } }
            """)!;

        Assert.Single(report.Readings);
        Assert.Equal(new ResourceHealthReading("available", ResourceHealthReading.FromResourceHealth, Read), report.Find(Id));
    }

    [Fact]
    public void ADuplicateIdWithoutAReadingDoesNotEraseTheEarlierOne()
    {
        var report = ResourceHealthReport.Parse("""
            { "resources": { "nonprod": [
                { "id": "/s/kv", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:37:00Z" } },
                { "id": "/s/kv", "health": { "status": "up-ish", "source": "resourceHealth", "checkedAt": "2026-10-04T22:37:00Z" } } ] } }
            """)!;

        Assert.Single(report.Readings);
        Assert.Equal("available", report.Find(Id)?.Status);
    }
}
