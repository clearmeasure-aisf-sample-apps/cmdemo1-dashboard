using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// What a deployable says about the build it runs, at its build endpoint (<c>buildPath</c> of the topology, for
/// example <c>/_build</c>): the commit, and the facts the build measured about the code. Every part is optional: an
/// app reports what its build measured, and the card shows what is there.
/// </summary>
/// <param name="Version">The version of the build, without build metadata.</param>
/// <param name="Commit">The commit the build was made from.</param>
/// <param name="CommitUrl">The commit's page.</param>
/// <param name="BuiltAt">When the build ran.</param>
/// <param name="BuildUrl">The build's run.</param>
public sealed record BuildInfo(
    string? Version,
    string? Commit,
    Uri? CommitUrl,
    DateTimeOffset? BuiltAt,
    Uri? BuildUrl,
    CodeSize? Code,
    TestCounts? Tests,
    CoverageFacts? Coverage,
    ComplexityFacts? Complexity,
    CrapFacts? Crap,
    int? QodanaProblems)
{
    /// <summary>The endpoint's answer; null when it is not a JSON object or holds none of the parts (an error page).</summary>
    public static BuildInfo? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var build = new BuildInfo(
                VersionText.Display(JsonRead.Text(root, "version")),
                JsonRead.Text(root, "commit"),
                JsonRead.Address(root, "commitUrl"),
                JsonRead.Time(root, "builtAt"),
                JsonRead.Address(root, "buildUrl"),
                CodeSize.Read(JsonRead.Section(root, "code")),
                TestCounts.Read(JsonRead.Section(root, "tests")),
                CoverageFacts.Read(JsonRead.Section(root, "coverage")),
                ComplexityFacts.Read(JsonRead.Section(root, "complexity")),
                CrapFacts.Read(JsonRead.Section(root, "crap")),
                JsonRead.Count(JsonRead.Section(root, "analysis"), "qodanaProblems"));
            return MeasuredFacts.OrNone(build, build.MeasuredNothing);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// True when the answer holds none of the parts: no build. The commit's page is not one of them - a page without
    /// its commit says no build.
    /// </summary>
    private bool MeasuredNothing =>
        MeasuredFacts.None(Version, Commit, BuiltAt, BuildUrl, Code, Tests, Coverage, Complexity, Crap, QodanaProblems);
}
