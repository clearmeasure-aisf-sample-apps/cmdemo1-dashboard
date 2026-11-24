namespace Dashboard.Tests;

/// <summary>
/// The readings a tile and the cluster keep are named where they are kept (<see cref="TargetStatus.SampleLength" />,
/// <see cref="ClusterMonitor.SampleLength" />), apart from the length of a drawn <see cref="Trend" />. They have the same
/// value today: every buffer keeps what it kept before the names were added, and the drawing keeps its own number.
/// </summary>
public class SampleWindowIsNamedTests
{
    [Fact]
    public void EveryWindowKeepsTheNumberItKeptBefore()
    {
        Assert.Equal(60, Trend.Length);
        Assert.Equal(60, TargetStatus.SampleLength);
        Assert.Equal(60, ClusterMonitor.SampleLength);
        Assert.Equal(30, TargetStatus.HistoryLength);
        Assert.Equal(50, EventLog.DefaultCapacity);
    }

    [Fact]
    public void ATilesSamplesAreSizedByItsSampleLength()
    {
        var tile = new TargetStatus(TargetKind.Node, "uat-east", new Uri("https://uat-east.example.net"));

        Assert.Equal(TargetStatus.SampleLength, tile.Samples.Capacity);
        Assert.Equal(TargetStatus.HistoryLength, tile.History.Capacity);
    }

    [Fact]
    public void TheClustersSamplesAreSizedByItsSampleLength()
    {
        var monitor = new ClusterMonitor(new ClusterInfo("aks-demo"), [], null!, null!, null!);

        Assert.Equal(ClusterMonitor.SampleLength, monitor.Samples.Capacity);
    }

    [Fact]
    public void ATileKeepsNoMoreThanItsSampleLength()
    {
        var tile = new TargetStatus(TargetKind.Node, "uat-east", new Uri("https://uat-east.example.net"));
        var snapshot = TelemetrySnapshots.Quiet(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero));

        for (var check = 0; check < TargetStatus.SampleLength + 5; check++)
        {
            tile.RecordTelemetry(snapshot);
        }

        Assert.Equal(TargetStatus.SampleLength, tile.Samples.Count);
    }
}
