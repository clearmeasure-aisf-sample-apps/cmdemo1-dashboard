namespace Dashboard.Tests;

/// <summary>
/// Whose the two name lists of a <see cref="DeploymentFreeze" /> are (#1204): the caller's, kept as the very
/// references handed over and never copied, as the record says in its own words. A caller that changes its list
/// changes what the freeze covers; the file is safe because every entry it reads is given two lists of its own.
/// </summary>
public class DeploymentFreezeListOwnershipTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 10, 19, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2026, 10, 11, 19, 0, 0, TimeSpan.Zero);

    private const string TwoAlikeEntries = """
        { "deployments": [], "recent": [], "missing": [],
          "freezes": [
            { "name": "weekend", "from": "2026-10-10T19:00:00Z", "to": "2026-10-11T19:00:00Z", "active": false, "environments": [ "prod" ], "projects": [ "demo-ui" ] },
            { "name": "weekend", "from": "2026-10-10T19:00:00Z", "to": "2026-10-11T19:00:00Z", "active": false, "environments": [ "prod" ], "projects": [ "demo-ui" ] } ] }
        """;

    /// <summary>The lists are the caller's: what the freeze covers follows the caller's own list as it grows,
    /// changes and empties, with no new freeze in between.</summary>
    [Fact]
    public void WhatAFreezeCoversFollowsTheCallersListsBecauseTheyAreNeverCopied()
    {
        List<string> environments = ["prod"];
        List<string> projects = ["demo-ui"];
        var freeze = new DeploymentFreeze("weekend", Start, End, false, environments, projects);

        Assert.False(freeze.Covers("uat"));
        Assert.False(freeze.Covers("prod", "demo-system"));

        environments.Add("uat");
        projects[0] = "demo-system";

        Assert.True(freeze.Covers("uat"));
        Assert.True(freeze.Covers("prod", "demo-system"));
        Assert.False(freeze.Covers("prod", "demo-ui"));

        environments.Clear();

        Assert.False(freeze.Covers("prod"));
        Assert.False(freeze.Covers("uat"));
        Assert.Same(environments, freeze.Environments);
        Assert.Same(projects, freeze.Projects);
    }

    /// <summary>The file never shares a list: every entry read is given two lists of its own, so no two freezes of
    /// one file, and no two readings of the same text, hold the same list, and changing what one holds leaves what
    /// the other covers as it was.</summary>
    [Fact]
    public void EveryFreezeReadFromTheFileHoldsListsNothingElseHolds()
    {
        var first = DeploymentsReport.Parse(TwoAlikeEntries)!;
        var second = DeploymentsReport.Parse(TwoAlikeEntries)!;

        DeploymentFreeze[] freezes = [.. first.Freezes, .. second.Freezes];

        Assert.Equal(4, freezes.Length);

        var environmentLists = freezes.Select(freeze => freeze.Environments).ToList();
        var projectLists = freezes.Select(freeze => freeze.Projects).ToList();

        Assert.Equal(4, environmentLists.Distinct(ReferenceEqualityComparer.Instance).Count());
        Assert.Equal(4, projectLists.Distinct(ReferenceEqualityComparer.Instance).Count());
        Assert.Empty(environmentLists.Intersect(projectLists.Cast<object>(), ReferenceEqualityComparer.Instance));

        Assert.All(freezes, freeze => Assert.Equal(["prod"], freeze.Environments));
        Assert.All(freezes, freeze => Assert.Equal(["demo-ui"], freeze.Projects));

        // Changing the list one freeze holds changes no other freeze of the file.
        ((List<string>)freezes[0].Environments).Clear();

        Assert.False(freezes[0].Covers("prod"));
        Assert.All(freezes[1..], freeze => Assert.True(freeze.Covers("prod", "demo-ui")));
    }

    /// <summary>The record's own words say the lists are the caller's and never copied, so the reading the tests
    /// pin is the one the code documents rather than an accident of it.</summary>
    [Fact]
    public void TheRecordSaysItsListsAreTheCallersAndNeverCopied()
    {
        var source = File.ReadAllText(Path.Combine(SourceRoot(), "Dashboard", "Health", "DeploymentActivity.cs"));
        var record = source[..source.IndexOf("public sealed record DeploymentFreeze(", StringComparison.Ordinal)];
        var docs = record[record.LastIndexOf("/// <summary>A deployment freeze", StringComparison.Ordinal)..];

        Assert.Contains("The two name lists are the caller's", docs, StringComparison.Ordinal);
        Assert.Contains("copies neither", docs, StringComparison.Ordinal);
        Assert.Contains("<param name=\"Environments\">The environments it covers, by name: the caller's list itself, never copied.</param>", docs, StringComparison.Ordinal);
        Assert.Contains("<param name=\"Projects\">The Octopus projects of the system it covers, by name: the caller's list itself, never copied.</param>", docs, StringComparison.Ordinal);
    }

    private static string SourceRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Dashboard", "Health", "DeploymentActivity.cs")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("src/Dashboard/Health/DeploymentActivity.cs was not found above the test's output.");
    }
}
