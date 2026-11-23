namespace Dashboard.Tests;

/// <summary>
/// The edge cases of <see cref="TelemetrySnapshot.Parse" /> that the other snapshot tests leave unpinned: names and
/// moments written with JSON escapes, an answer that tells the SQL of its requests apart from the background without
/// saying the total, and an answer whose broken escapes make it no JSON at all. The record's own members (equality,
/// copies, <see cref="TelemetrySnapshot.SplitsSql" /> and <see cref="TelemetrySnapshot.SqlOfTraffic" /> on values a
/// caller hands it) are pinned by <see cref="TelemetrySnapshotEdgeCaseTests" />.
/// </summary>
public sealed class TelemetrySnapshotEscapeEdgeCaseTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static TelemetrySnapshot Reading(string json) =>
        TelemetrySnapshot.Parse(json, Read) ?? throw new InvalidOperationException("The answer was not a reading.");

    /// <summary>A name written with JSON escapes is the name it stands for: every section and every counter is
    /// found under its escaped spelling just as under its plain one.</summary>
    [Fact]
    public void ANameWrittenWithEscapesIsTheNameItStandsFor()
    {
        var read = Reading("""
            { "requests": { "perMinute": 11, "frontDoor": 7, "direct": 3, "errors": 5, "p95Ms": 13 },
              "probes": { "perMinute": 17, "frontDoor": 19 },
              "sql": { "perMinute": 10, "p95Ms": 29, "requests": 6, "background": 4 },
              "http": { "perMinute": 31 } }
            """);

        Assert.Equal(new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read), read with { SqlRequests = null, SqlBackground = null });
        Assert.Equal(6, read.SqlRequests);
        Assert.Equal(4, read.SqlBackground);
        Assert.True(read.SplitsSql);
        Assert.Equal(6, read.SqlOfTraffic);
    }

    /// <summary>An escaped character that changes the name makes it another name: a traffic section whose name
    /// carries an escaped capital is no traffic, so the answer is no reading.</summary>
    [Fact]
    public void AnEscapeThatChangesTheNameIsAnotherName() =>
        Assert.Null(TelemetrySnapshot.Parse("""{ "Requests": { "perMinute": 11 } }""", Read));

    /// <summary>The moment the app started at is read through its escapes too: a plus sign written as an escape
    /// is the offset it stands for, and the instant is the one the plain spelling says.</summary>
    [Fact]
    public void AMomentWrittenWithEscapesIsTheMomentItStandsFor()
    {
        var read = Reading("""{ "requests": { }, "startedAt": "2026-10-04T22:31:19+02:00" }""");

        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 31, 19, TimeSpan.FromHours(2)), read.StartedAt);
        Assert.Equal(TimeSpan.FromHours(2), read.StartedAt!.Value.Offset);
    }

    /// <summary>An app that tells the SQL of its requests from the background but leaves out the total still
    /// tells them apart: the total is the zero of a name never said, and the traffic's SQL is the requests' count,
    /// larger than that total - the reader does not add the halves up into a total of its own.</summary>
    [Fact]
    public void HalvesWithoutATotalStillTellTheSqlApart()
    {
        var read = Reading("""{ "requests": { "perMinute": 11 }, "sql": { "requests": 6, "background": 4 } }""");

        Assert.Equal(0, read.Sql);
        Assert.Null(read.SqlP95Ms);
        Assert.Equal(6, read.SqlRequests);
        Assert.Equal(4, read.SqlBackground);
        Assert.True(read.SplitsSql);
        Assert.Equal(6, read.SqlOfTraffic);
    }

    /// <summary>An escape that is not a JSON escape, or one cut short, makes the whole answer no JSON, and so no
    /// reading - wherever in the answer it stands.</summary>
    [Theory]
    [InlineData("""{ "\q": 1, "requests": { "perMinute": 11 } }""")]
    [InlineData("""{ "requests": { "perMinute": 11 }, "startedAt": "\u12" }""")]
    [InlineData("""{ "requests": { "perMinute": 11, "\x41": 1 } }""")]
    [InlineData("{ \"requests\": { \"perMinute\": 11 }, \"note\": \"line\nbreak\" }")]
    public void ABrokenEscapeIsNoReading(string body) => Assert.Null(TelemetrySnapshot.Parse(body, Read));

    /// <summary>Two parses of one answer are two equal readings and not one shared object: the reader keeps nothing
    /// between calls.</summary>
    [Fact]
    public void EachParseIsAReadingOfItsOwn()
    {
        const string body = """{ "requests": { "perMinute": 11 }, "sql": { "perMinute": 10, "requests": 6, "background": 4 } }""";

        var first = Reading(body);
        var second = Reading(body);

        Assert.Equal(first, second);
        Assert.NotSame(first, second);
    }
}
