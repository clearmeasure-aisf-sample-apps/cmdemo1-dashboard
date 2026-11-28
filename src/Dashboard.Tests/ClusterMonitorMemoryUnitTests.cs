using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The memory trend's title says its unit in the one word <c>ClusterText.GibibyteTrendUnit</c> names, so the trend
/// and the cluster's meters cannot drift apart. The constant is internal, so it is read by reflection.
/// </summary>
public class ClusterMonitorMemoryUnitTests
{
    [Fact]
    public void TheGibibyteTrendUnitIsAnInternalLiteralOfClusterText()
    {
        var field = typeof(ClusterText).GetField("GibibyteTrendUnit", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly);
        Assert.Equal(" GiB", field.GetRawConstantValue());
    }

    [Fact]
    public void TheMemoryTrendTitleEndsInTheGibibyteTrendUnitOfClusterText()
    {
        var unit = (string)typeof(ClusterText).GetField("GibibyteTrendUnit", BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
        var monitor = new ClusterMonitor(new ClusterInfo("aks-demo"), null!, null!, null!, null!);
        monitor.Samples.Add(new ClusterSample(812, 9876543210));
        monitor.Samples.Add(new ClusterSample(1000, 9876543210));

        // The title ends with the reflected unit.
        Assert.EndsWith(unit, monitor.MemoryTrend!.Title, StringComparison.Ordinal);

        // The unit is the same word the memory meter writes after its number.
        var meterText = ClusterText.Memory(9876543210);
        Assert.EndsWith(unit, meterText, StringComparison.Ordinal);
    }

    [Fact]
    public void TheClusterMonitorSourceDoesNotSpellTheGibibyteUnitByHand()
    {
        var source = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Dashboard", "Cluster", "ClusterMonitor.cs"));

        Assert.DoesNotContain("\" GiB\"", source, StringComparison.Ordinal);
        Assert.Contains("ClusterText.GibibyteTrendUnit", source, StringComparison.Ordinal);
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Dashboard.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Dashboard.sln not found above the test output.");
    }
}
