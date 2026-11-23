namespace Dashboard.Tests;

/// <summary>
/// The edge cases of where <see cref="TelemetrySnapshot.Parse" /> looks for each name, plus a refusal that happens
/// before rounding. None of the other files pins these: <see cref="TelemetrySnapshotEdgeCaseTests" />,
/// <see cref="TelemetrySnapshotBoundaryEdgeCaseTests" />, <see cref="TelemetrySnapshotNameEdgeCaseTests" /> and
/// <see cref="TelemetryTests" />. The four sections (<c>requests</c>, <c>probes</c>, <c>sql</c>, <c>http</c>) are
/// read only at the top of an answer. Each name is read only in the section it belongs to, so a name the reader knows
/// in one section counts nothing when it turns up in another. A fraction below zero is refused as below zero instead
/// of being rounded to the zero it lies next to. Every answer and moment here is fixed by the test, so nothing reaches
/// the network or the clock.
/// </summary>
public sealed class TelemetrySnapshotSectionEdgeCaseTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static TelemetrySnapshot Reading(string json) =>
        TelemetrySnapshot.Parse(json, Read) ?? throw new InvalidOperationException("The answer was not a reading.");

    /// <summary>Sections nested inside the traffic are not the sections of the answer: probes, SQL and HTTP said
    /// under <c>requests</c> count nothing, and the SQL halves said there tell nothing apart.</summary>
    [Fact]
    public void SectionsAreReadOnlyAtTheTopOfTheAnswer()
    {
        var read = Reading("""
            { "requests": { "perMinute": 11,
                "probes": { "perMinute": 17, "frontDoor": 2 },
                "sql": { "perMinute": 10, "p95Ms": 29, "requests": 6, "background": 4 },
                "http": { "perMinute": 31 } } }
            """);

        Assert.Equal(new TelemetrySnapshot(11, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }

    /// <summary>The counters said at the top of an answer, outside any section, count nothing: only the traffic
    /// object makes it a reading, and the numbers beside it are not read as the traffic.</summary>
    [Fact]
    public void CountersSaidOutsideTheirSectionCountNothing()
    {
        var read = Reading("""
            { "perMinute": 11, "frontDoor": 7, "direct": 3, "errors": 5, "p95Ms": 13, "background": 4,
              "requests": { } }
            """);

        Assert.Equal(new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
    }

    /// <summary>Each section answers only for its own names: a percentile or error count under <c>probes</c> or
    /// <c>http</c>, a front-door count under <c>sql</c> and SQL halves under <c>requests</c> are not moved into the
    /// part of the same name in another section.</summary>
    [Fact]
    public void ANameIsReadOnlyInTheSectionItBelongsTo()
    {
        var read = Reading("""
            { "requests": { "perMinute": 11, "requests": 6, "background": 4 },
              "probes": { "perMinute": 17, "errors": 3, "p95Ms": 9, "direct": 2 },
              "sql": { "perMinute": 10, "frontDoor": 8, "errors": 1 },
              "http": { "perMinute": 31, "p95Ms": 7, "frontDoor": 5 } }
            """);

        Assert.Equal(new TelemetrySnapshot(11, 0, 0, 0, null, 17, 0, 10, null, 31, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Equal(10, read.SqlOfTraffic);
    }

    /// <summary>The traffic's <c>requests</c> section and the SQL's <c>requests</c> count share a name but not a
    /// place: the SQL's half is the number under <c>sql</c>, and the traffic's object is not taken for it.</summary>
    [Fact]
    public void TheTwoRequestsAreToldApartByWhereTheyAreSaid()
    {
        var read = Reading("""
            { "requests": { "perMinute": 22 },
              "sql": { "perMinute": 10, "requests": 6, "background": 4 } }
            """);

        Assert.Equal(22, read.Requests);
        Assert.Equal(6, read.SqlRequests);
        Assert.Equal(4, read.SqlBackground);
        Assert.True(read.SplitsSql);
        Assert.Equal(6, read.SqlOfTraffic);
    }

    /// <summary>A fraction below zero is refused before it is rounded: <c>-0.4</c> would round to zero, but the
    /// reader refuses it as below zero. Counters said that way count nothing, percentiles are unsaid, and the SQL half
    /// said that way is unsaid, which ends the telling apart, so the traffic's SQL falls back on the total.</summary>
    [Theory]
    [InlineData("-0.4")]
    [InlineData("-0.0001")]
    [InlineData("-1e-300")]
    public void AFractionBelowZeroIsRefusedAndNotRoundedToZero(string number)
    {
        var read = Reading($$"""
            { "requests": { "perMinute": {{number}}, "p95Ms": {{number}} },
              "sql": { "perMinute": 10, "p95Ms": {{number}}, "requests": {{number}}, "background": 4 } }
            """);

        Assert.Equal(0, read.Requests);
        Assert.Null(read.P95Ms);
        Assert.Null(read.SqlP95Ms);
        Assert.Null(read.SqlRequests);
        Assert.Equal(4, read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Equal(10, read.SqlOfTraffic);
    }

    /// <summary>The same fraction above zero is rounded to the zero beside it and is a number said: the percentile
    /// is zero rather than unsaid, and the SQL half keeps the telling apart.</summary>
    [Fact]
    public void AFractionAboveZeroIsTheZeroItRoundsTo()
    {
        var read = Reading("""
            { "requests": { "perMinute": 0.4, "p95Ms": 0.4 },
              "sql": { "perMinute": 10, "p95Ms": 0.4, "requests": 0.4, "background": 4 } }
            """);

        Assert.Equal(0, read.Requests);
        Assert.Equal(0, read.P95Ms);
        Assert.Equal(0, read.SqlP95Ms);
        Assert.Equal(0, read.SqlRequests);
        Assert.True(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }
}
