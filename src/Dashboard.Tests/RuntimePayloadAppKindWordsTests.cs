namespace Dashboard.Tests;

/// <summary>
/// The nouns naming which apps a tile waits for or was reached through (#1708): <c>WaitingTile</c> and
/// <c>PassedLine</c> in <c>RuntimePayloadBuilder</c> say the same pair, "web apps" and "container apps", so each is
/// named once (<c>WebApps</c>, <c>ContainerApps</c>) and no call site of either helper writes it as a literal again.
/// </summary>
public class RuntimePayloadAppKindWordsTests
{
    private static readonly string[] Nouns = ["web apps", "container apps"];

    private static readonly string[] Helpers = ["WaitingTile(", "PassedLine("];

    /// <summary>Each app-kind noun has exactly one home in the builder: one <c>private const</c>.</summary>
    [Fact]
    public void EveryAppKindNounIsNamedOnce()
    {
        var lines = File.ReadAllLines(BuilderSource);

        foreach (var noun in Nouns)
        {
            Assert.Equal(1, lines.Count(line =>
                line.TrimStart().StartsWith("private const string", StringComparison.Ordinal)
                && line.Contains($"= \"{noun}\";", StringComparison.Ordinal)));
        }
    }

    /// <summary>No call of <c>WaitingTile</c> or <c>PassedLine</c> passes an app-kind noun (or a typo of one) as a quoted literal.</summary>
    [Fact]
    public void NoHelperCallWritesAnAppKindNounAsALiteral()
    {
        var lines = File.ReadAllLines(BuilderSource);
        Assert.True(lines.Length > 500, $"only {lines.Length} line(s) read from {BuilderSource}: the guard is checking nothing");

        var calls = lines
            .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal) && !line.TrimStart().StartsWith("private static", StringComparison.Ordinal))
            .Where(line => Helpers.Any(helper => line.Contains(helper, StringComparison.Ordinal)))
            .ToList();

        // Three calls of WaitingTile and two of PassedLine: the five sites the issue counts.
        Assert.True(calls.Count >= 5, $"only {calls.Count} helper call(s) found: the guard is checking nothing");

        var offenders = calls.Where(line => line.Contains("app\"", StringComparison.Ordinal) || line.Contains("apps\"", StringComparison.Ordinal));
        Assert.Empty(offenders);
    }

    private static string BuilderSource => MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs"));
}
