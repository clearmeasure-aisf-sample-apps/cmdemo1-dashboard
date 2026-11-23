using System.Collections.Immutable;

namespace Dashboard.Tests;

/// <summary>
/// The three ways a caller spells an environment's deployables list, and the two readings the page makes of it:
/// the record holds the very list instance it was handed (no copy, no normalize), compares by the instance it
/// stands on (not by its contents), and the page reads the deployables in the order they were handed - for the
/// array, the <see cref="List{T}" /> and the <see cref="ImmutableArray{T}" />. The same holds for the pages
/// list.
/// </summary>
public class EnvironmentListHandlingTests
{
    private static readonly NodeInfo West = new("ca-demo-uat-ui", "westus3", NodeInfo.PrimaryRole, new Uri("https://ui.uat.example.net"));
    private static readonly IReadOnlyList<NodeInfo> OneNode = [West];

    private static readonly DeployableInfo Ui = new("ui", null, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode);
    private static readonly DeployableInfo Api = new("api", null, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode);

    private static readonly PageLink Scorecard = new("Scorecard", new Uri("https://ca-demo-uat-scorecard.example.net/scorecard"), "scorecard");
    private static readonly PageLink Dashboard = new("Dashboard", new Uri("https://ca-demo-uat.example.net"), "dashboard");

    // ------------------------------------------------------------------ the three spellings of the deployables list

    /// <summary>The array spelling: the environment holds the array, the copy holds the array, the array is
    /// compared by instance, and the page reads its two deployables in order.</summary>
    [Fact]
    public void TheArraySpellingIsHeldComparedAndReadAsAnArray()
    {
        var array = new[] { Ui, Api };
        var environment = new EnvironmentInfo("uat", "nonprod", array);

        // The record holds the array, no copy made, no normalize done.
        Assert.Same(array, environment.Deployables);
        Assert.Equal(2, environment.Deployables.Count);
        Assert.Same(Ui, environment.Deployables[0]);
        Assert.Same(Api, environment.Deployables[1]);

        // The with copy holds the array instance, not a copy of it.
        var copy = environment with { };
        Assert.Same(array, copy.Deployables);

        // The array is compared as the hold it stands on: another array of the same two deployables is another
        // environment.
        var other = new EnvironmentInfo("uat", "nonprod", new[] { Ui, Api });
        Assert.NotEqual(environment, other);
        Assert.NotEqual(environment.GetHashCode(), other.GetHashCode());

        // The same array instance again is the same environment.
        var again = new EnvironmentInfo("uat", "nonprod", array);
        Assert.Equal(environment, again);
        Assert.Equal(environment.GetHashCode(), again.GetHashCode());
    }

    /// <summary>The List spelling: the environment holds the list, the copy holds the list, the list is compared
    /// by instance, and the page reads its two deployables in order.</summary>
    [Fact]
    public void TheListSpellingIsHeldComparedAndReadAsAList()
    {
        var list = new List<DeployableInfo> { Ui, Api };
        var environment = new EnvironmentInfo("uat", "nonprod", list);

        // The record holds the list, no copy made, no normalize done.
        Assert.Same(list, environment.Deployables);
        Assert.Equal(2, environment.Deployables.Count);
        Assert.Same(Ui, environment.Deployables[0]);
        Assert.Same(Api, environment.Deployables[1]);

        // The with copy holds the list instance, not a copy of it.
        var copy = environment with { };
        Assert.Same(list, copy.Deployables);

        // The list is compared as the hold it stands on: another list of the same two deployables is another
        // environment.
        var other = new EnvironmentInfo("uat", "nonprod", new List<DeployableInfo> { Ui, Api });
        Assert.NotEqual(environment, other);
        Assert.NotEqual(environment.GetHashCode(), other.GetHashCode());

        // The same list instance again is the same environment.
        var again = new EnvironmentInfo("uat", "nonprod", list);
        Assert.Equal(environment, again);
        Assert.Equal(environment.GetHashCode(), again.GetHashCode());
    }

    /// <summary>The ImmutableList spelling: the environment holds the list, the copy holds the list, the
    /// ImmutableList is compared by instance, and the page reads its two deployables in order.</summary>
    [Fact]
    public void TheImmutableListSpellingIsHeldComparedAndReadAsAnImmutableList()
    {
        var immutable = ImmutableList.Create(Ui, Api);
        var environment = new EnvironmentInfo("uat", "nonprod", immutable);

        // The record holds the ImmutableList, no copy made, no normalize done.
        Assert.Same(immutable, environment.Deployables);
        Assert.Equal(2, environment.Deployables.Count);
        Assert.Same(Ui, environment.Deployables[0]);
        Assert.Same(Api, environment.Deployables[1]);

        // The with copy holds the same ImmutableList instance, not a copy of it.
        var copy = environment with { };
        Assert.Same(immutable, copy.Deployables);

        // The ImmutableList is compared as the hold it stands on: another ImmutableList of the same two
        // deployables is another environment.
        var other = new EnvironmentInfo("uat", "nonprod", ImmutableList.Create(Ui, Api));
        Assert.NotEqual(environment, other);
        Assert.NotEqual(environment.GetHashCode(), other.GetHashCode());

        // The same ImmutableList instance again is the same environment.
        var again = new EnvironmentInfo("uat", "nonprod", immutable);
        Assert.Equal(environment, again);
        Assert.Equal(environment.GetHashCode(), again.GetHashCode());
    }

    // ------------------------------------------------------------------ the three spellings of the pages list

    /// <summary>The array spelling for pages: the environment holds the array, the copy holds the array, the array
    /// is compared by instance, and the page reads its two pages in order.</summary>
    [Fact]
    public void ThePagesArraySpellingIsHeldComparedAndReadAsAnArray()
    {
        var deployables = new[] { Ui };
        var pages = new[] { Scorecard, Dashboard };
        var environment = new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages);

        Assert.Same(pages, environment.Pages);
        Assert.Equal(2, environment.Pages!.Count);
        Assert.Same(Scorecard, environment.Pages[0]);
        Assert.Same(Dashboard, environment.Pages[1]);

        var copy = environment with { };
        Assert.Same(pages, copy.Pages);

        var other = new EnvironmentInfo("uat", "nonprod", deployables, Pages: new[] { Scorecard, Dashboard });
        Assert.NotEqual(environment, other);
        Assert.NotEqual(environment.GetHashCode(), other.GetHashCode());

        var again = new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages);
        Assert.Equal(environment, again);
        Assert.Equal(environment.GetHashCode(), again.GetHashCode());
    }

    /// <summary>The List spelling for pages: the environment holds the list, the copy holds the list, the list is
    /// compared by instance, and the page reads its two pages in order.</summary>
    [Fact]
    public void ThePagesListSpellingIsHeldComparedAndReadAsAList()
    {
        var deployables = new[] { Ui };
        var pages = new List<PageLink> { Scorecard, Dashboard };
        var environment = new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages);

        Assert.Same(pages, environment.Pages);
        Assert.Equal(2, environment.Pages!.Count);
        Assert.Same(Scorecard, environment.Pages[0]);
        Assert.Same(Dashboard, environment.Pages[1]);

        var copy = environment with { };
        Assert.Same(pages, copy.Pages);

        var other = new EnvironmentInfo("uat", "nonprod", deployables, Pages: new List<PageLink> { Scorecard, Dashboard });
        Assert.NotEqual(environment, other);
        Assert.NotEqual(environment.GetHashCode(), other.GetHashCode());

        var again = new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages);
        Assert.Equal(environment, again);
        Assert.Equal(environment.GetHashCode(), again.GetHashCode());
    }

    /// <summary>The ImmutableList spelling for pages: the environment holds the list, the copy holds the list,
    /// the ImmutableList is compared by instance, and the page reads its two pages in order.</summary>
    [Fact]
    public void ThePagesImmutableListSpellingIsHeldComparedAndReadAsAnImmutableList()
    {
        var deployables = new[] { Ui };
        var pages = ImmutableList.Create(Scorecard, Dashboard);
        var environment = new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages);

        Assert.Same(pages, environment.Pages);
        Assert.Equal(2, environment.Pages!.Count);
        Assert.Same(Scorecard, environment.Pages[0]);
        Assert.Same(Dashboard, environment.Pages[1]);

        var copy = environment with { };
        Assert.Same(pages, copy.Pages);

        var other = new EnvironmentInfo("uat", "nonprod", deployables, Pages: ImmutableList.Create(Scorecard, Dashboard));
        Assert.NotEqual(environment, other);
        Assert.NotEqual(environment.GetHashCode(), other.GetHashCode());

        var again = new EnvironmentInfo("uat", "nonprod", deployables, Pages: pages);
        Assert.Equal(environment, again);
        Assert.Equal(environment.GetHashCode(), again.GetHashCode());
    }

    // ------------------------------------------------------------------ the boundary of the deployables count

    /// <summary>
    /// The environment holds the deployables list as handed at every boundary of its length: zero, one, two, a
    /// few hundred and a thousand. The record compares by the list instance it stands on: another list of the
    /// same deployables is another environment, and an environment of a thousand deployables is not an
    /// environment of nine hundred ninety-nine, nor of none.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(251)]
    [InlineData(1000)]
    public void TheDeployablesCountBoundaryIsHeldAndCompared(int count)
    {
        var deployables = Enumerable.Range(0, count)
            .Select(i => new DeployableInfo($"deployable {i}", null, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode))
            .ToArray();
        var environment = new EnvironmentInfo("uat", "nonprod", deployables);

        // The record holds the list as handed, no copy made, no normalize done.
        Assert.Same(deployables, environment.Deployables);
        Assert.Equal(count, environment.Deployables.Count);

        // The first and the last of it are the ones handed.
        if (count > 0)
        {
            Assert.Equal("deployable 0", environment.Deployables[0].Name);
        }
        if (count > 1)
        {
            Assert.Equal($"deployable {count - 1}", environment.Deployables[count - 1].Name);
        }

        // The environment of a thousand deployables is nobody else's equal: not the one with nine hundred
        // ninety-nine, not the one with the same thousand in another list instance, and not the one of none.
        if (count == 1000)
        {
            var fewer = Enumerable.Range(0, 999)
                .Select(i => new DeployableInfo($"deployable {i}", null, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode))
                .ToArray();
            var lessEnvironment = new EnvironmentInfo("uat", "nonprod", fewer);
            Assert.NotEqual(environment, lessEnvironment);

            var sameCountDifferentList = Enumerable.Range(0, 1000)
                .Select(i => new DeployableInfo($"deployable {i}", null, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode))
                .ToArray();
            var sameCountEnvironment = new EnvironmentInfo("uat", "nonprod", sameCountDifferentList);
            Assert.NotEqual(environment, sameCountEnvironment);

            var noDeployables = Array.Empty<DeployableInfo>();
            var emptyEnvironment = new EnvironmentInfo("uat", "nonprod", noDeployables);
            Assert.NotEqual(environment, emptyEnvironment);
        }
    }

    // ------------------------------------------------------------------ the boundary of the pages count

    /// <summary>
    /// The environment holds the pages list as handed at every boundary of its length: the absence of a pages
    /// list (null), an empty pages list (a list of none), and a pages list of a thousand. The record compares
    /// by the list instance it stands on, and the page reads the thousandth page as the one handed.
    /// </summary>
    [Fact]
    public void ThePagesCountBoundaryIsHeldAndCompared()
    {
        var deployables = new[] { Ui };

        // The absence of a pages list: the environment holds null, the page reads no pages at all.
        var noPagesEnvironment = new EnvironmentInfo("uat", "nonprod", deployables);
        Assert.Null(noPagesEnvironment.Pages);

        // An empty pages list: the environment holds an empty list, not null - the page reads a pages list of
        // none, not the absence of a pages list.
        var emptyPages = Array.Empty<PageLink>();
        var emptyPagesEnvironment = new EnvironmentInfo("uat", "nonprod", deployables, Pages: emptyPages);
        Assert.NotNull(emptyPagesEnvironment.Pages);
        Assert.Empty(emptyPagesEnvironment.Pages);

        // A pages list of a thousand: the environment holds the list as handed, and the page reads the
        // thousandth page as the one handed.
        var thousandPages = Enumerable.Range(0, 1000)
            .Select(i => new PageLink($"page {i}", new Uri($"https://page{i}.example.net")))
            .ToArray();
        var thousandPagesEnvironment = new EnvironmentInfo("uat", "nonprod", deployables, Pages: thousandPages);
        Assert.Same(thousandPages, thousandPagesEnvironment.Pages);
        Assert.Equal(1000, thousandPagesEnvironment.Pages!.Count);
        Assert.Same(thousandPages[999], thousandPagesEnvironment.Pages[999]);
        Assert.Equal("page 999", thousandPagesEnvironment.Pages[999].Name);
    }
}