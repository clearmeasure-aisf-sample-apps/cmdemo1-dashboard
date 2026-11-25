using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The CPU trend's title says its unit in the one word <c>ClusterText.CoresUnit</c> names, so the trend and the
/// cluster's meters cannot drift apart. The constant is internal, so it is read by reflection.
/// </summary>
public class ClusterMonitorCoresUnitTests
{
    [Fact]
    public void TheCoresUnitIsAnInternalConstantOfClusterText()
    {
        var field = typeof(ClusterText).GetField("CoresUnit", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly);
        Assert.Equal("cores", field.GetRawConstantValue());
    }

    [Fact]
    public void TheCpuTrendTitleEndsInTheCoresUnitOfClusterText()
    {
        var unit = (string)typeof(ClusterText).GetField("CoresUnit", BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
        var monitor = new ClusterMonitor(new ClusterInfo("aks-demo"), null!, null!, null!, null!);
        monitor.Samples.Add(new ClusterSample(812, 9876543210));
        monitor.Samples.Add(new ClusterSample(1000, 9876543210));

        Assert.EndsWith($", now {ClusterText.Cores(1000)} {unit}", monitor.CpuTrend!.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void TheClusterMonitorSourceDoesNotSpellTheCoresWordByHand()
    {
        var source = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Dashboard", "Cluster", "ClusterMonitor.cs"));

        Assert.DoesNotContain("\" cores\"", source, StringComparison.Ordinal);
        Assert.Contains("ClusterText.CoresUnit", source, StringComparison.Ordinal);
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
