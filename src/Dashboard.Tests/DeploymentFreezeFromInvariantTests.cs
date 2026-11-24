namespace Dashboard.Tests;

/// <summary>
/// The invariant the words of a freeze lean on (issue #1055): a freeze the page says of is in force or comes, a
/// freeze that comes has a known <see cref="DeploymentFreeze.From"/>, and a freeze in force without one is said by
/// its end alone. The record allows <see cref="DeploymentFreeze.From"/> to be null; the callers hold the rest, and
/// the record's documentation says so.
/// </summary>
public class DeploymentFreezeFromInvariantTests
{
    /// <summary>Friday noon, UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;
    private static readonly string[] Ui = ["ui"];

    private static DeploymentFreeze Freeze(DateTimeOffset? from, DateTimeOffset to, bool active) =>
        new("prod weekend", from, to, active, ["prod"], ["demo-ui"]);

    public static TheoryData<int> Hours => [-100, -73, -72, -1, 0, 1, 24, 72, 73, 100];

    [Theory]
    [MemberData(nameof(Hours))]
    public void AFreezeThatComesAlwaysHasAKnownStart(int hours)
    {
        var to = Now.AddHours(200);
        DeploymentFreeze[] freezes = [Freeze(null, to, true), Freeze(null, to, false), Freeze(Now.AddHours(hours), to, false), Freeze(Now.AddHours(hours), to, true)];

        foreach (var freeze in freezes)
        {
            if (freeze.ComesAt(Now))
            {
                Assert.NotNull(freeze.From);
                Assert.False(freeze.InForceAt(Now));
            }

            if (freeze.InForceAt(Now) && freeze.From is null)
            {
                Assert.True(freeze.Active);
            }
        }
    }

    [Fact]
    public void AFreezeInForceWithoutAStartIsSaidByItsEndAlone()
    {
        var report = DeploymentsReport.Parse("""
            { "deployments": [], "recent": [], "missing": [],
              "freezes": [ { "name": "prod weekend", "from": "not a time", "to": "2026-10-12T00:00:00Z", "active": true, "environments": [ "prod" ], "projects": [ "demo-ui" ] } ] }
            """)!;
        Assert.Null(Assert.Single(report.Freezes).From);

        var node = Assert.Single(DeploymentActivity.OfNode(report, "prod", "demo", "ui", Now, Utc));
        var environment = Assert.Single(DeploymentActivity.OfEnvironment(report, "prod", report.Marks("prod", "demo", Ui, Now), Now, Utc));

        Assert.Equal(ActivityKind.Frozen, node.Kind);
        Assert.StartsWith("frozen until ", node.Text, StringComparison.Ordinal);
        Assert.Equal(ActivityKind.Frozen, environment.Kind);
        Assert.DoesNotContain(" from ", environment.Text, StringComparison.Ordinal);
        Assert.Contains("In force now", environment.Title, StringComparison.Ordinal);
        Assert.DoesNotContain(" from ", environment.Title.Split('.')[0], StringComparison.Ordinal);
    }

    [Fact]
    public void AFreezeWithoutAStartThatIsNotActiveIsNotSaid()
    {
        var report = DeploymentsReport.Parse("""
            { "deployments": [], "recent": [], "missing": [],
              "freezes": [ { "name": "prod weekend", "to": "2026-10-12T00:00:00Z", "active": false, "environments": [ "prod" ], "projects": [ "demo-ui" ] } ] }
            """)!;

        Assert.Empty(DeploymentActivity.OfNode(report, "prod", "demo", "ui", Now, Utc));
        Assert.Empty(DeploymentActivity.OfEnvironment(report, "prod", report.Marks("prod", "demo", Ui, Now), Now, Utc));
    }

    [Fact]
    public void TheRecordSaysTheInvariantAndTheWordsAssertItInOnePlace()
    {
        var source = File.ReadAllText(Path.Combine(SourceRoot(), "Dashboard", "Health", "DeploymentActivity.cs"));
        var record = source[..source.IndexOf("public sealed record DeploymentFreeze(", StringComparison.Ordinal)];
        var docs = record[record.LastIndexOf("/// <summary>A deployment freeze", StringComparison.Ordinal)..];

        Assert.Contains("A freeze that <see cref=\"ComesAt\"/> a moment always has a", docs, StringComparison.Ordinal);
        Assert.Contains("record itself never refuses a null.", docs, StringComparison.Ordinal);
        Assert.Contains("True only where <see cref=\"From\"/> is known.", source, StringComparison.Ordinal);

        var words = source[source.IndexOf("public static class DeploymentActivity", StringComparison.Ordinal)..];
        Assert.Single(words.Split("From!.Value")[1..]);
        Assert.Contains("private static DateTimeOffset Begins(DeploymentFreeze freeze) => freeze.From!.Value;", words, StringComparison.Ordinal);
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
