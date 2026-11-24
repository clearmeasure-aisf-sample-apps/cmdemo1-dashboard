using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The <see cref="ProcessVitals" /> reading that holds no value: a <c>process</c> object that is present but has
/// nothing in it, or only JSON <c>null</c>s. That reading is turned into <see langword="null" /> by
/// <see cref="ProcessVitals.Read" /> (pinned through <see cref="TelemetrySnapshot.Parse" />, the public entry
/// point), so an app that sends an empty <c>process</c> object looks the same as one that sends none at all.
/// And a single measured value - any one of the seven, even <c>0</c> - keeps the reading.
/// </summary>
public class ProcessVitalsEmptyReadingTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>A process object that is present but has no known keys reads as no vitals.</summary>
    [Fact]
    public void AnEmptyProcessObjectIsNoVitals()
    {
        var snapshot = TelemetrySnapshot.Parse("""{ "requests": { }, "process": { } }""", Now)!;

        Assert.Null(snapshot.Process);
    }

    /// <summary>A process object whose known keys are all JSON null reads as no vitals.</summary>
    [Fact]
    public void AProcessObjectWhoseKeysAreAllNullIsNoVitals()
    {
        var snapshot = TelemetrySnapshot.Parse("""
            {
                "requests": { },
                "process": {
                    "cpuPercent": null,
                    "workingSetMb": null,
                    "gcHeapMb": null,
                    "threads": null,
                    "inFlight": null,
                    "exceptionsPerMinute": null,
                    "uptimeSeconds": null
                }
            }
        """, Now)!;

        Assert.Null(snapshot.Process);
    }

    /// <summary>A process object with exactly one measured value is kept.</summary>
    [Theory]
    [InlineData("""{ "requests": { }, "process": { "cpuPercent": 1.5 } }""", "CpuPercent", typeof(double))]
    [InlineData("""{ "requests": { }, "process": { "workingSetMb": 512 } }""", "WorkingSetMb", typeof(double))]
    [InlineData("""{ "requests": { }, "process": { "gcHeapMb": 128 } }""", "GcHeapMb", typeof(double))]
    [InlineData("""{ "requests": { }, "process": { "threads": 8 } }""", "Threads", typeof(int))]
    [InlineData("""{ "requests": { }, "process": { "inFlight": 3 } }""", "InFlight", typeof(int))]
    [InlineData("""{ "requests": { }, "process": { "exceptionsPerMinute": 2 } }""", "ExceptionsPerMinute", typeof(int))]
    [InlineData("""{ "requests": { }, "process": { "uptimeSeconds": 3600 } }""", "UptimeSeconds", typeof(long))]
    public void AProcessObjectWithOneMeasuredValueIsKept(string json, string property, Type type)
    {
        var snapshot = TelemetrySnapshot.Parse(json, Now)!;

        Assert.NotNull(snapshot.Process);

        var part = typeof(ProcessVitals).GetProperty(property)!;
        var actual = part.GetValue(snapshot.Process);
        Assert.IsType(type, actual);
        Assert.NotEqual(0, actual);
    }

    /// <summary>A value of 0 counts as measured and keeps the reading.</summary>
    [Theory]
    [InlineData("""{ "requests": { }, "process": { "threads": 0 } }""", "Threads")]
    [InlineData("""{ "requests": { }, "process": { "cpuPercent": 0 } }""", "CpuPercent")]
    [InlineData("""{ "requests": { }, "process": { "uptimeSeconds": 0 } }""", "UptimeSeconds")]
    public void AZeroValueKeepsTheReading(string json, string property)
    {
        var snapshot = TelemetrySnapshot.Parse(json, Now)!;

        Assert.NotNull(snapshot.Process);

        var part = typeof(ProcessVitals).GetProperty(property)!;
        var value = part.GetValue(snapshot.Process);

        // A zero is not null, and its numeric value is 0.
        Assert.NotNull(value);
        Assert.Equal(0, Convert.ToInt64(value, CultureInfo.InvariantCulture));
    }
}
