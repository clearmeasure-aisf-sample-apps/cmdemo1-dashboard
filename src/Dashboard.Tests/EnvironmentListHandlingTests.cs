using System.Collections.Immutable;

namespace Dashboard.Tests;

/// <summary>
/// The three ways a caller spells an environment's deployables list and its pages list - an array, a
/// <see cref="List{T}" /> and an <see cref="ImmutableList{T}" />: the record holds the very list instance it was
/// handed (no copy, no normalize), keeps it in the order it was handed, and compares by the instance it stands on
/// (not by its contents). Its two boundaries are the lists that are absent or empty and a list of a few items.
/// </summary>
public class EnvironmentListHandlingTests
{
    private static readonly NodeInfo West = new("ca-demo-uat-ui", "westus3", NodeInfo.PrimaryRole, new Uri("https://ui.uat.example.net"));
    private static readonly IReadOnlyList<NodeInfo> OneNode = [West];

    private static readonly DeployableInfo Ui = DeployableNamed("ui");
    private static readonly DeployableInfo Api = DeployableNamed("api");

    private static readonly PageLink Scorecard = new("Scorecard", new Uri("https://ca-demo-uat-scorecard.example.net/scorecard"), "scorecard");
    private static readonly PageLink Dashboard = new("Dashboard", new Uri("https://ca-demo-uat.example.net"), "dashboard");

    private static DeployableInfo DeployableNamed(string name) =>
        new(name, null, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode);

    private static IReadOnlyList<T> Spell<T>(string spelling, params T[] items) => spelling switch
    {
        "array" => items,
        "list" => new List<T>(items),
        "immutable" => ImmutableList.CreateRange(items),
        _ => throw new ArgumentOutOfRangeException(nameof(spelling)),
    };

    /// <summary>For each spelling, the deployables list and the pages list are held as the very instance handed,
    /// stay in the order handed, are held by a <c>with</c> copy as the same instance, and are compared by the
    /// instance they stand on: another list of the same items is another environment, the same instance again is
    /// the same environment.</summary>
    [Theory]
    [InlineData("array")]
    [InlineData("list")]
    [InlineData("immutable")]
    public void AListIsHeldInTheOrderAndAsTheInstanceItWasHandedWhateverItsSpelling(string spelling)
    {
        var deployables = Spell(spelling, Ui, Api);
        var pages = Spell(spelling, Scorecard, Dashboard);
        var environment = new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages);

        Assert.Same(deployables, environment.Deployables);
        Assert.Same(Ui, environment.Deployables[0]);
        Assert.Same(Api, environment.Deployables[1]);
        Assert.Same(pages, environment.Pages);
        Assert.Same(Scorecard, environment.Pages![0]);
        Assert.Same(Dashboard, environment.Pages[1]);

        var copy = environment with { };
        Assert.Same(deployables, copy.Deployables);
        Assert.Same(pages, copy.Pages);

        Assert.NotEqual(environment, new EnvironmentInfo("uat", "nonprod", Spell(spelling, Ui, Api), Pages: pages));
        Assert.NotEqual(environment, new EnvironmentInfo("uat", "nonprod", deployables, Pages: Spell(spelling, Scorecard, Dashboard)));
        Assert.Equal(environment, new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages));
    }

    /// <summary>The deployables list is held as handed at the boundaries of its length - none, one and two - with
    /// the first and the last of it the ones handed; when there is any, another list instance of the same
    /// deployables is another environment.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TheDeployablesCountBoundaryIsHeldAndCompared(int count)
    {
        var deployables = Enumerable.Range(0, count).Select(i => DeployableNamed($"deployable {i}")).ToArray();
        var environment = new EnvironmentInfo("uat", "nonprod", deployables);

        Assert.Same(deployables, environment.Deployables);
        Assert.Equal(count, environment.Deployables.Count);

        if (count > 0)
        {
            Assert.Equal("deployable 0", environment.Deployables[0].Name);
            Assert.Equal($"deployable {count - 1}", environment.Deployables[count - 1].Name);
            Assert.NotEqual(environment, new EnvironmentInfo("uat", "nonprod", deployables.ToArray()));
        }
    }

    /// <summary>The pages list is held as handed when it is absent (null) and when it is empty: the absent list and
    /// the empty one are two different environments.</summary>
    [Fact]
    public void ThePagesAbsentAndEmptyBoundariesAreHeldAndCompared()
    {
        var deployables = new[] { Ui };

        var absent = new EnvironmentInfo("uat", "nonprod", deployables);
        Assert.Null(absent.Pages);

        var emptyPages = Array.Empty<PageLink>();
        var empty = new EnvironmentInfo("uat", "nonprod", deployables, Pages: emptyPages);
        Assert.Same(emptyPages, empty.Pages);
        Assert.Empty(empty.Pages!);

        Assert.NotEqual(absent, empty);
    }
}
