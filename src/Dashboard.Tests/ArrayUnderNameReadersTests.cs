using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// The three readers that ask <see cref="JsonRead.HasArray" /> and <see cref="JsonRead.Items" /> whether there is a
/// list under a name (#2158): the <c>containerApps</c> of an environment's <c>health</c> (<see cref="DeliveryReport.ContainerAppsOf" />), <see cref="DatabaseParser.ParseLive" /> and
/// the <c>pages</c> of an environment of <see cref="TopologyParser.Parse" />. Pinned here: what is no list (absent,
/// <c>null</c>, an object, a string, a parent that is no object) is no answer, an empty list is an empty answer where
/// the reader keeps one, and the entries that are read keep the order of the file.
/// </summary>
public class ArrayUnderNameReadersTests
{
    /// <summary>The container apps of an environment whose <c>health</c> is <paramref name="health"/>, read from the file.</summary>
    private static IReadOnlyList<ContainerAppReports>? AppsOfHealth(string health) =>
        DeliveryReport.Parse($$"""{ "environments": [ { "name": "tdd", "deployables": [] {{health}} } ] }""")!.ContainerAppsOf("tdd");

    /// <summary>The container apps of a <c>health</c> that counts both windows and says <paramref name="containerApps"/>.</summary>
    private static IReadOnlyList<ContainerAppReports>? ReadAll(string containerApps) =>
        AppsOfHealth($$""", "health": { "last24Hours": { "reports": 24, "healthy": 24 }, "last7Days": { "reports": 168, "healthy": 168 }{{containerApps}} }""");

    [Theory]
    [InlineData("")]
    [InlineData(", \"health\": null")]
    [InlineData(", \"health\": [ { \"containerApps\": [ { \"name\": \"demo-ui\" } ] } ]")]
    [InlineData(", \"health\": \"containerApps\"")]
    public void ContainerAppsOfNoHealthOrAHealthThatIsNoObjectAreNone(string health)
    {
        Assert.Null(AppsOfHealth(health));
    }

    [Theory]
    [InlineData("")]
    [InlineData(", \"containerApps\": \"demo-ui\"")]
    [InlineData(", \"containerApps\": null")]
    [InlineData(", \"containerApps\": { \"name\": \"demo-ui\" }")]
    public void ContainerAppsThatAreNoListAreNone(string containerApps)
    {
        Assert.Null(ReadAll(containerApps));
    }

    [Fact]
    public void AnEmptyListOfContainerAppsIsAnEmptyAnswerNotNone()
    {
        var apps = ReadAll(""", "containerApps": [] """);

        Assert.NotNull(apps);
        Assert.Empty(apps);
    }

    [Fact]
    public void ContainerAppsWithoutANameAreLeftOutAndTheRestKeepTheirOrder()
    {
        var apps = ReadAll(""", "containerApps": [ { "name": "demo-ui" }, { }, { "name": "" }, 7, { "name": "demo-api" } ] """);

        Assert.NotNull(apps);
        Assert.Equal(["demo-ui", "demo-api"], apps.Select(app => app.Name));
    }

    [Theory]
    [InlineData("{ \"generated\": \"2026-10-07T01:30:00Z\" }")]
    [InlineData("{ \"tables\": null }")]
    [InlineData("{ \"tables\": \"A\" }")]
    [InlineData("{ \"tables\": { \"name\": \"A\" } }")]
    public void ALiveFileWhoseTablesAreNoListIsNoFile(string json)
    {
        Assert.Null(DatabaseParser.ParseLive(json));
    }

    [Fact]
    public void ALiveFileWithAnEmptyListOfTablesIsAFileWithNoTables()
    {
        var live = DatabaseParser.ParseLive("""{ "tables": [] }""");

        Assert.NotNull(live);
        Assert.Empty(live.Tables);
        Assert.False(live.Truncated);
    }

    [Fact]
    public void TheTablesOfALiveFileKeepTheOrderOfTheFile()
    {
        var live = DatabaseParser.ParseLive("""{ "tables": [ { "name": "Zeta" }, { "name": "Alpha" }, { "name": "Mid" } ] }""");

        Assert.NotNull(live);
        Assert.Equal(["Zeta", "Alpha", "Mid"], live.Tables.Select(table => table.Name));
    }

    private static IReadOnlyList<PageLink>? PagesOf(string pages) =>
        TopologyReading.Valid($$"""{ "environments": [ { "name": "tdd", "deployables": [] {{pages}} } ] }""").Environments[0].Pages;

    [Theory]
    [InlineData("")]
    [InlineData(", \"pages\": null")]
    [InlineData(", \"pages\": { \"name\": \"Scorecard\", \"url\": \"https://scorecard.example.net/\" }")]
    [InlineData(", \"pages\": \"https://scorecard.example.net/\"")]
    public void PagesThatAreNoListAreNone(string pages)
    {
        Assert.Null(PagesOf(pages));
    }

    [Fact]
    public void AListOfOnlyPagesThatCannotBeReadIsNone()
    {
        Assert.Null(PagesOf(""", "pages": [ { "name": "No address" }, { "url": "https://no-name.example.net/" }, 7, null ]"""));
    }

    [Fact]
    public void ThePagesThatCanBeReadKeepTheOrderOfTheFile()
    {
        var pages = PagesOf(""", "pages": [ { "name": "Zeta", "url": "https://zeta.example.net/" }, { "name": "Skipped" }, { "name": "Alpha", "url": "https://alpha.example.net/" } ]""");

        Assert.NotNull(pages);
        Assert.Equal(["Zeta", "Alpha"], pages.Select(page => page.Name));
    }
}
