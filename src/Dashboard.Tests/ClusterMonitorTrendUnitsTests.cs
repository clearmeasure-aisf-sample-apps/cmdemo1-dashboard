using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The cluster's trends and its meters say one number in one unit: <see cref="ClusterMonitor.CpuTrend" /> and
/// <see cref="ClusterMonitor.MemoryTrend" /> divide by the same <c>ClusterText.MillicoresPerCore</c> and
/// <c>ClusterText.Gibibyte</c> that <see cref="ClusterText.Cores" /> and <see cref="ClusterText.Memory" /> divide by.
/// The constants are internal and the app declares no <c>InternalsVisibleTo</c>, so they are read by reflection.
/// </summary>
public class ClusterMonitorTrendUnitsTests
{
    [Theory]
    [InlineData("MillicoresPerCore", 1000d)]
    [InlineData("Gibibyte", 1073741824d)]
    public void TheClusterUnitIsAnInternalConstantOfClusterText(string name, double value)
    {
        var field = typeof(ClusterText).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly);
        Assert.Equal(value, field.GetRawConstantValue());
        Assert.Null(typeof(ClusterText).GetField(name, BindingFlags.Public | BindingFlags.Static));
    }

    [Fact]
    public void TheCpuTrendSaysNowInTheNumberTheCoresMeterWrites()
    {
        var monitor = Monitor(new ClusterSample(812, 9876543210), new ClusterSample(1500, 9876543210));

        Assert.EndsWith($", now {ClusterText.Cores(1500)} cores", monitor.CpuTrend!.Title, StringComparison.Ordinal);
        Assert.Equal("1.5", ClusterText.Cores(1500));
    }

    [Fact]
    public void TheMemoryTrendSaysNowInTheNumberTheMemoryMeterWrites()
    {
        var monitor = Monitor(new ClusterSample(812, 1073741824), new ClusterSample(812, 9876543210));

        Assert.EndsWith($", now {ClusterText.Memory(9876543210)}", monitor.MemoryTrend!.Title, StringComparison.Ordinal);
        Assert.Equal("9.2 GiB", ClusterText.Memory(9876543210));
    }

    private static ClusterMonitor Monitor(params ClusterSample[] samples)
    {
        var monitor = new ClusterMonitor(new ClusterInfo("aks-demo"), null!, null!, null!, null!);
        foreach (var sample in samples)
        {
            monitor.Samples.Add(sample);
        }

        return monitor;
    }
}
