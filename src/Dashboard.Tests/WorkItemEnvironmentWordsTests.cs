namespace Dashboard.Tests;

/// <summary>
/// The three words a work item's environments are named by, pinned in one place: the consts
/// <see cref="WorkItemEnvironments.TddWord" />, <see cref="WorkItemEnvironments.UatWord" /> and
/// <see cref="WorkItemEnvironments.ProdWord" />, which are both the property names
/// <see cref="WorkItemsReport.Parse" /> looks for in the work-items file and the place names
/// <see cref="WorkItemsText.Places" /> hands to the page. These tests pin that the words are what they are, that
/// the page's places come out in the page's own order under those words, and that the file reading finds a
/// deployment under each word whichever letters the file writes it in. The rest of the record's surface - the
/// parts, the copy, the equality - is pinned in <see cref="WorkItemEnvironmentsEdgeCaseTests" /> and is left alone.
/// </summary>
public class WorkItemEnvironmentWordsTests
{
    private static readonly DateTimeOffset TddAt = new(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset UatAt = new(2026, 11, 9, 17, 15, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ProdAt = new(2026, 11, 9, 18, 30, 0, TimeSpan.Zero);

    /// <summary>The three words are what the file and the page have always said them: tdd, uat, prod - in that
    /// order, the order the work runs through them and the order the page shows them.</summary>
    [Fact]
    public void TheThreeWordsAreTheFileNamesAndThePlaceNames()
    {
        Assert.Equal("tdd", WorkItemEnvironments.TddWord);
        Assert.Equal("uat", WorkItemEnvironments.UatWord);
        Assert.Equal("prod", WorkItemEnvironments.ProdWord);

        // The three words are three different words, and they are the order the page says them.
        Assert.Equal(3, new[] { WorkItemEnvironments.TddWord, WorkItemEnvironments.UatWord, WorkItemEnvironments.ProdWord }
            .Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            [WorkItemEnvironments.TddWord, WorkItemEnvironments.UatWord, WorkItemEnvironments.ProdWord],
            new[] { WorkItemEnvironments.TddWord, WorkItemEnvironments.UatWord, WorkItemEnvironments.ProdWord });
    }

    /// <summary>For an item the file says is deployed everywhere, the page's places come out under the three words,
    /// in the page's own order - tdd, uat, prod - and only the environments the item has reached: an item in tdd
    /// alone says one place, an item in tdd and prod says the two of them with uat absent between them.</summary>
    [Fact]
    public void ThePlacesOfAnEverywhereItemAreTheThreeWordsInOrder()
    {
        var item = Deployed(TddAt, UatAt, ProdAt);

        Assert.Equal(
            [WorkItemEnvironments.TddWord, WorkItemEnvironments.UatWord, WorkItemEnvironments.ProdWord],
            WorkItemsText.Places(item).Select(place => place.Name));
        Assert.Equal(
            ["tdd 2.4.80", "uat 2.4.81", "prod 2.4.82"],
            WorkItemsText.Places(item).Select(WorkItemsText.PlaceLabel));

        // Only the environments the item has reached: tdd alone is one place, tdd and prod are two with uat out.
        var onlyTdd = Deployed(TddAt, null, null);
        Assert.Equal([WorkItemEnvironments.TddWord], WorkItemsText.Places(onlyTdd).Select(place => place.Name));

        var tddAndProd = Deployed(TddAt, null, ProdAt);
        Assert.Equal(
            [WorkItemEnvironments.TddWord, WorkItemEnvironments.ProdWord],
            WorkItemsText.Places(tddAndProd).Select(place => place.Name));

        // No environment at all is no place: the shared nothing says nothing to the page.
        Assert.Empty(WorkItemsText.Places(Deployed(null, null, null)));
    }

    /// <summary>The title a place stands for is the page's own sentence over the word: <c>Deployed to prod as 2.4.82,
    /// 2 h ago</c> for a production arrival - the word in the middle is the const, not a word the title typed itself.
    /// A place with no arrival moment says the version and nothing else after it.</summary>
    [Fact]
    public void ThePlaceTitleSaysTheWordTheConstNames()
    {
        var item = Deployed(TddAt, UatAt, ProdAt);

        // Two hours is the smallest whole-unit the page says in hours (minutes roll up at two hours), so the ago is
        // said as <c>2 h ago</c> for each of the three places, one hour and a quarter apart.
        Assert.Equal("Deployed to tdd as 2.4.80, 2 h ago", WorkItemsText.PlaceTitle(WorkItemsText.Places(item)[0], TddAt.AddHours(2)));
        Assert.Equal("Deployed to uat as 2.4.81, 15 min ago", WorkItemsText.PlaceTitle(WorkItemsText.Places(item)[1], UatAt.AddMinutes(15)));
        Assert.Equal("Deployed to prod as 2.4.82, 2 h ago", WorkItemsText.PlaceTitle(WorkItemsText.Places(item)[2], ProdAt.AddHours(2)));

        // A place with no arrival moment says the version and nothing after it: the word is still the const's.
        var versionOnly = new WorkItem(
            new WorkItemIdentity(455, "Everywhere", null, null, null),
            [],
            null,
            null,
            [],
            [],
            null,
            new WorkItemEnvironments(
                new WorkDeployment("2.4.80", null),
                new WorkDeployment("2.4.81", null),
                new WorkDeployment("2.4.82", null)),
            null);

        Assert.Equal("Deployed to tdd as 2.4.80", WorkItemsText.PlaceTitle(WorkItemsText.Places(versionOnly)[0], TddAt));
        Assert.Equal("Deployed to uat as 2.4.81", WorkItemsText.PlaceTitle(WorkItemsText.Places(versionOnly)[1], TddAt));
        Assert.Equal("Deployed to prod as 2.4.82", WorkItemsText.PlaceTitle(WorkItemsText.Places(versionOnly)[2], TddAt));
    }

    /// <summary>The reading of the file finds a deployment under each of the three words whichever letters the file
    /// writes the word in - all upper case, all lower case, and mixed - and stands each on the property of its own
    /// name, so a deployment the file wrote as <c>TDD</c> is the tdd place and not the uat or the prod one.</summary>
    [Theory]
    [InlineData("TDD", "UAT", "PROD")]
    [InlineData("tdd", "uat", "prod")]
    [InlineData("Tdd", "Uat", "Prod")]
    [InlineData("tDd", "uAt", "pRoD")]
    public void TheReadingFindsADeploymentUnderEachWordInAnyLetters(string tdd, string uat, string prod)
    {
        var report = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 455, "title": "Everywhere", "environments": {
                "{{tdd}}": { "version": "2.4.80", "deployedAt": "2026-11-09T16:00:00Z" },
                "{{uat}}": { "version": "2.4.81", "deployedAt": "2026-11-09T17:15:00Z" },
                "{{prod}}": { "version": "2.4.82", "deployedAt": "2026-11-09T18:30:00Z" } } } ] }
            """)!;

        var environments = Assert.Single(report.Items).Environments;

        Assert.Equal("2.4.80", environments.Tdd!.Version);
        Assert.Equal(TddAt, environments.Tdd.DeployedAt);
        Assert.Equal("2.4.81", environments.Uat!.Version);
        Assert.Equal(UatAt, environments.Uat.DeployedAt);
        Assert.Equal("2.4.82", environments.Prod!.Version);
        Assert.Equal(ProdAt, environments.Prod.DeployedAt);

        // And the page's places come out under the three words in the page's order, whatever the file's letters.
        Assert.Equal(
            [WorkItemEnvironments.TddWord, WorkItemEnvironments.UatWord, WorkItemEnvironments.ProdWord],
            WorkItemsText.Places(Assert.Single(report.Items)).Select(place => place.Name));
    }

    /// <summary>The two copies of the words have not drifted apart: the words the reading looks for and the words the
    /// page shows are the same three words, so a file written in the page's own words still reads every place it has
    /// reached - the reader and the chips say the same vocabulary.</summary>
    [Fact]
    public void TheFileWrittenInThePagesOwnWordsStillReadsEveryPlace()
    {
        var report = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 470, "environments": {
                "{{WorkItemEnvironments.TddWord}}": { "version": "2.4.80", "deployedAt": "2026-11-09T16:00:00Z" },
                "{{WorkItemEnvironments.UatWord}}": { "version": "2.4.81", "deployedAt": "2026-11-09T17:15:00Z" },
                "{{WorkItemEnvironments.ProdWord}}": { "version": "2.4.82", "deployedAt": "2026-11-09T18:30:00Z" } } } ] }
            """)!;

        var environments = Assert.Single(report.Items).Environments;

        Assert.Equal("2.4.80", environments.Tdd!.Version);
        Assert.Equal("2.4.81", environments.Uat!.Version);
        Assert.Equal("2.4.82", environments.Prod!.Version);
    }

    /// <summary>A work item whose three environments are each a deployment of its own version and its own moment, or
    /// the nothing of an environment that has not got the item.</summary>
    private static WorkItem Deployed(DateTimeOffset? tdd, DateTimeOffset? uat, DateTimeOffset? prod) =>
        new(
            new WorkItemIdentity(455, "Everywhere", null, null, null),
            [],
            null,
            null,
            [],
            [],
            null,
            new WorkItemEnvironments(
                tdd is { } atTdd ? new WorkDeployment("2.4.80", atTdd) : null,
                uat is { } atUat ? new WorkDeployment("2.4.81", atUat) : null,
                prod is { } atProd ? new WorkDeployment("2.4.82", atProd) : null),
            null);
}
