namespace Dashboard.Tests;

/// <summary>
/// The edge cases of how <see cref="TelemetrySnapshot.Parse" /> matches the names in an answer and which JSON it will
/// read: the parts <see cref="TelemetrySnapshotEdgeCaseTests" /> and <see cref="TelemetryTests" /> leave unpinned.
/// Names are matched exactly (case counts), a name said twice is read as its last saying, names the reader does not
/// know are passed over, a negative zero is the zero it equals, a number in exponent form is the number it stands
/// for, and JSON written loosely (comments, trailing commas, single quotes) is not an answer at all.
/// </summary>
public sealed class TelemetrySnapshotNameEdgeCaseTests
{
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static TelemetrySnapshot Reading(string json) =>
        TelemetrySnapshot.Parse(json, Read) ?? throw new InvalidOperationException("The answer was not a reading.");

    /// <summary>The traffic section is found only under its exact lower-case name: an answer that capitalises it
    /// has no traffic and so is no reading.</summary>
    [Theory]
    [InlineData("""{ "Requests": { "perMinute": 11 } }""")]
    [InlineData("""{ "REQUESTS": { "perMinute": 11 } }""")]
    [InlineData("""{ "requests ": { "perMinute": 11 } }""")]
    public void TheTrafficIsFoundOnlyUnderItsExactName(string body) => Assert.Null(TelemetrySnapshot.Parse(body, Read));

    /// <summary>A counter or a section under a name of the wrong case is a name the app never said: it counts
    /// nothing, and the percentiles and SQL halves stay unsaid.</summary>
    [Fact]
    public void ANameOfTheWrongCaseIsANameNeverSaid()
    {
        var read = Reading("""
            { "requests": { "PerMinute": 11, "FrontDoor": 7, "Direct": 3, "Errors": 5, "P95Ms": 13 },
              "Probes": { "perMinute": 17, "frontDoor": 19 },
              "SQL": { "perMinute": 10, "p95Ms": 29, "requests": 6, "background": 4 },
              "sql": { "Requests": 6, "Background": 4 },
              "HTTP": { "perMinute": 31 } }
            """);

        Assert.Equal(new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }

    /// <summary>A name said twice in the same object is read as its last saying, for a counter, a percentile, a SQL
    /// half and a whole section alike.</summary>
    [Fact]
    public void ANameSaidTwiceIsReadAsItsLastSaying()
    {
        var read = Reading("""
            { "requests": { "perMinute": 1, "perMinute": 11, "p95Ms": 2, "p95Ms": 13 },
              "probes": { "perMinute": 99 },
              "probes": { "perMinute": 17 },
              "sql": { "perMinute": 10, "requests": 1, "requests": 6, "background": 4 } }
            """);

        Assert.Equal(11, read.Requests);
        Assert.Equal(13, read.P95Ms);
        Assert.Equal(17, read.Probes);
        Assert.Equal(6, read.SqlRequests);
        Assert.Equal(6, read.SqlOfTraffic);
    }

    /// <summary>A traffic section said first as an object and last as something else is no traffic: the last
    /// saying is the one the shape rule is held to.</summary>
    [Fact]
    public void TrafficSaidLastAsNoObjectIsNoReading() =>
        Assert.Null(TelemetrySnapshot.Parse("""{ "requests": { "perMinute": 11 }, "requests": 11 }""", Read));

    /// <summary>Names the reader does not know, at the top and inside each section, are passed over and leave the
    /// reading exactly what the known names make it.</summary>
    [Fact]
    public void NamesTheReaderDoesNotKnowArePassedOver()
    {
        var read = Reading("""
            { "version": "9.9", "extra": [1, 2, 3],
              "requests": { "perMinute": 11, "p99Ms": 400, "nested": { "perMinute": 999 } },
              "probes": { "perMinute": 17, "other": 5 },
              "sql": { "perMinute": 10, "p99Ms": 80 },
              "http": { "perMinute": 31, "errors": 2 },
              "queue": { "perMinute": 77 } }
            """);

        Assert.Equal(new TelemetrySnapshot(11, 0, 0, 0, null, 17, 0, 10, null, 31, Read), read);
    }

    /// <summary>A negative zero is not below zero: it is read as the zero it equals, a number the app did say, not
    /// the nothing of a percentile or SQL half it did not.</summary>
    [Fact]
    public void ANegativeZeroIsTheZeroItEquals()
    {
        var read = Reading("""
            { "requests": { "perMinute": -0, "p95Ms": -0.0 },
              "sql": { "perMinute": -0, "p95Ms": -0, "requests": -0.0, "background": -0 } }
            """);

        Assert.Equal(0, read.Requests);
        Assert.Equal(0, read.P95Ms);
        Assert.Equal(0, read.SqlP95Ms);
        Assert.Equal(0, read.SqlRequests);
        Assert.Equal(0, read.SqlBackground);
        Assert.True(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }

    /// <summary>A number written in exponent form is the number it stands for, rounded like any other.</summary>
    [Theory]
    [InlineData("1.1e1", 11)]
    [InlineData("1.1E1", 11)]
    [InlineData("1.1e+1", 11)]
    [InlineData("110e-1", 11)]
    [InlineData("1.25e1", 12)]
    [InlineData("1.35e1", 14)]
    [InlineData("4e-1", 0)]
    [InlineData("0e0", 0)]
    public void ANumberInExponentFormIsTheNumberItStandsFor(string number, int expected)
    {
        Assert.Equal(expected, Reading($$"""{ "requests": { "perMinute": {{number}} } }""").Requests);
        Assert.Equal(expected, Reading($$"""{ "requests": { }, "sql": { "p95Ms": {{number}} } }""").SqlP95Ms);
    }

    /// <summary>JSON written loosely - with a comment, a trailing comma or single quotes - is not an answer the
    /// endpoint writes, and is no reading.</summary>
    [Theory]
    [InlineData("""{ "requests": { "perMinute": 11 } // note""")]
    [InlineData("""{ /* note */ "requests": { "perMinute": 11 } }""")]
    [InlineData("""{ "requests": { "perMinute": 11, } }""")]
    [InlineData("""{ "requests": { "perMinute": 11 }, }""")]
    [InlineData("{ 'requests': { 'perMinute': 11 } }")]
    [InlineData("{ requests: { perMinute: 11 } }")]
    [InlineData("""{ "requests": { "perMinute": NaN } }""")]
    [InlineData("""{ "requests": { "perMinute": 011 } }""")]
    public void LooseJsonIsNoReading(string body) => Assert.Null(TelemetrySnapshot.Parse(body, Read));

    /// <summary>Whitespace around and inside a good answer changes nothing, and the moment the reader brings is the
    /// moment the reading holds, even at the ends of <see cref="DateTimeOffset" />.</summary>
    [Fact]
    public void WhitespaceChangesNothingAndTheMomentIsTheReadersAtEitherEnd()
    {
        const string body = "\r\n\t {\n\"requests\"\t:\r\n{ \"perMinute\" : 11 }\n}\t\r\n ";

        var atMin = TelemetrySnapshot.Parse(body, DateTimeOffset.MinValue);
        var atMax = TelemetrySnapshot.Parse(body, DateTimeOffset.MaxValue);

        Assert.NotNull(atMin);
        Assert.NotNull(atMax);
        Assert.Equal(11, atMin.Requests);
        Assert.Equal(DateTimeOffset.MinValue, atMin.ReadAt);
        Assert.Equal(DateTimeOffset.MaxValue, atMax.ReadAt);
    }
}
