using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The traffic sentence's "given up" part reads the probe's own clause (<c>ProbeDefaults.NoAnswerWithin</c>)
/// for the traffic's wait, so a rewording of that clause reaches the traffic button too (#1740). <c>ProbeDefaults</c>
/// is internal, so the clause is reached by reflection, as <see cref="ProbeDefaultsEdgeCaseTests" /> does.
/// </summary>
public class TrafficCountProbeClauseTests
{
    [Fact]
    public void TheGivenUpPartIsTheProbeClauseForTheTrafficWait()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
        var method = type.GetMethod("NoAnswerWithin", BindingFlags.Public | BindingFlags.Static, [typeof(TimeSpan)])!;
        var clause = (string)method.Invoke(null, [TimeSpan.FromSeconds(TrafficPlan.AnswerWithinSeconds)])!;

        Assert.Equal($"119 sent, 109 answered, 10 with {clause}", TrafficCount.From([119, 109, 0, 10, 0]).Counts);
    }

    [Fact]
    public void TheWordsNoAnswerWithinAreWrittenOnceInTheApp()
    {
        var src = Path.Combine(RepoRoot(), "src", "Dashboard");
        var writers = Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => File.ReadLines(file).Any(line => !line.TrimStart().StartsWith("///", StringComparison.Ordinal)
                && line.Contains("no answer within", StringComparison.Ordinal)))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Equal(["ProbeDefaults.cs"], writers);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Dashboard.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Dashboard.sln not found above the test output.");
    }
}
