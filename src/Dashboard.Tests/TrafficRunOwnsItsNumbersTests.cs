using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The three numbers of a press - how many requests a second, for how long, and how long each request waits for
/// its answer - are declared on the run itself, in <see cref="TrafficRun.Values" /> (#1777), and the run is the
/// only way a caller gets them. The guards that stand on the other side of the move keep standing: the plan's
/// <see cref="TrafficPlan.AnswerWithinSeconds" /> stays the public const the shared wait was pinned as (#1724),
/// and it now names the const the run declares, so the two say the one wait; and the run is still the three
/// numbers in the order the browser call takes them (#1742). What this guard pins is the move itself: the numbers
/// declared on the run, the run written from its own numbers, the plan keeping no second copy of them, and no
/// file of the app reading them from anywhere but the run.
/// </summary>
public class TrafficRunOwnsItsNumbersTests
{
    /// <summary>The three numbers of a press, in the order a run carries them and the browser call takes them.</summary>
    private static readonly string[] NumbersOfARun = ["PerSecond", "Seconds", "AnswerWithinSeconds"];

    /// <summary>
    /// The three numbers are declared on the run itself: public consts of the run's <see cref="TrafficRun.Values"
    /// />, named one for one, with the values the button always sent - two requests a second for a minute, every
    /// one of them given up after the shared ten seconds of a probe. Consts, so no caller keeps a copy of the
    /// numbers.
    /// </summary>
    [Fact]
    public void TheRunDeclaresTheThreeNumbersOfAPressOnItself()
    {
        foreach (var number in NumbersOfARun)
        {
            var field = typeof(TrafficRun.Values).GetField(number, BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(field);
            Assert.True(field!.IsLiteral, $"{number} of the run is a const, so no caller keeps a copy of the number.");
            Assert.True(field.IsPublic);
            Assert.Equal(typeof(int), field.FieldType);
        }

        Assert.Equal(2, TrafficRun.Values.PerSecond);
        Assert.Equal(60, TrafficRun.Values.Seconds);
        Assert.Equal(10, TrafficRun.Values.AnswerWithinSeconds);
    }

    /// <summary>
    /// The run of the button is written from the run's own numbers: the value a press sends is the value the run
    /// declares, and not the numbers read back out of the plan they once came from.
    /// </summary>
    [Fact]
    public void TheRunIsWrittenFromItsOwnNumbers() =>
        Assert.Equal(
            new TrafficRun(TrafficRun.Values.PerSecond, TrafficRun.Values.Seconds, TrafficRun.Values.AnswerWithinSeconds),
            TrafficRun.Default);

    /// <summary>
    /// The plan keeps no second copy of the run's numbers: where the guard still reads them from the plan, the
    /// plan names the const the run declares - <c>PerSecond</c> and <c>Seconds</c> are no literals of the plan at
    /// all, and the plan's <c>AnswerWithinSeconds</c>, the const the shared-wait guard pins on the plan, is the
    /// run's own wait.
    /// </summary>
    [Fact]
    public void ThePlanNamesTheRunsNumbersAndKeepsNoSecondCopyOfThem()
    {
        Assert.DoesNotContain(
            typeof(TrafficPlan).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(field => field.Name is "PerSecond" or "Seconds"),
            field => field.IsLiteral);

        Assert.Equal(TrafficRun.Values.PerSecond, TrafficPlan.PerSecond);
        Assert.Equal(TrafficRun.Values.Seconds, TrafficPlan.Seconds);
        Assert.Equal(TrafficRun.Values.AnswerWithinSeconds, TrafficPlan.AnswerWithinSeconds);
    }

    /// <summary>
    /// The run is the only way a caller gets the numbers: no source file of the app reads <c>PerSecond</c>,
    /// <c>Seconds</c> or <c>AnswerWithinSeconds</c> from the plan - not the panel's call, countdown and sentence,
    /// not the count's clause about a request given up. The plan's own file excepted, where the members the guard
    /// pins are declared, naming the run's numbers.
    /// </summary>
    [Fact]
    public void NoFileOfTheAppReadsTheNumbersFromThePlan()
    {
        var offenders = SourceFilesOfTheApp()
            .Where(pair => !pair.File.EndsWith("TrafficPlan.cs", StringComparison.Ordinal))
            .Select(pair => (pair.File, Offence: NumbersOfARun
                .Where(number => pair.Text.Contains($"TrafficPlan.{number}", StringComparison.Ordinal))
                .Select(number => $"TrafficPlan.{number}")
                .ToList()))
            .Where(pair => pair.Offence.Count > 0)
            .Select(pair => $"{pair.File} reads {string.Join(", ", pair.Offence)} from the plan")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// No file of the app writes <c>new TrafficRun(</c>: a press takes the run's own <c>Default</c>, written where
    /// the run is declared from the run's own numbers, so no caller passes three ints to a run it made itself.
    /// </summary>
    [Fact]
    public void NoFileOfTheAppPassesThreeIntsToARunItMade()
    {
        var offenders = SourceFilesOfTheApp()
            .Where(pair => pair.Text.Contains("new TrafficRun(", StringComparison.Ordinal))
            .Select(pair => pair.File)
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// The source files of the app - every <c>.cs</c> and <c>.razor</c> under <c>src/Dashboard</c>, the build's
    /// output excluded, the way the guards that walk the app walk it - each as its path from the app's root and
    /// its text.
    /// </summary>
    private static IEnumerable<(string File, string Text)> SourceFilesOfTheApp()
    {
        var root = MarkupGuards.DashboardRoot();

        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".razor", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);
            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return (relative, File.ReadAllText(path));
        }
    }
}
