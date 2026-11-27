namespace Dashboard.Tests;

/// <summary>
/// The boundaries of <see cref="DatabaseText" /> that <see cref="DatabaseViewTests" />, <see cref="RuntimeTextHelperTests" />,
/// <see cref="ViewLinksTests" /> and <see cref="PortalLabelTests" /> leave open: the null guards, the empty manifest and
/// live file, the ends of a count, ties between two moments and the fixed words of the view.
/// </summary>
public sealed class DatabaseTextEdgeCaseTests
{
    private static readonly DateTimeOffset Noon = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

    private static DatabaseTable Table(string schema, string name, long? rows = null, int? columns = null)
    {
        var alias = DatabaseText.Alias(schema, name);
        return new DatabaseTable(alias, "db." + alias, schema, name, rows, columns);
    }

    private static DatabaseManifest ManifestOf(params DatabaseTable[] tables) =>
        new("tdd", "sqldb-tdd", null, null, false, tables, tables.Length);

    private static DatabaseLive LiveOf(params DatabaseLiveTable[] tables) => new(null, tables, false);

    private static DatabaseLiveTable LiveTable(string schema, string name, long? rows = null) => new(schema, name, rows, []);

    private static LinkSet Links(string key, string address) => new([KeyValuePair.Create(key, new Uri(address))]);

    private static RuntimeNode Sql(string alias = "sqldb", string name = "sql-tdd", LinkSet? links = null) =>
        new(alias, RuntimeNodeKind.Sql, name, Links: links);

    [Fact]
    public void TheSchemaChangedLineIsFixed() =>
        Assert.Equal("The schema changed since this diagram was drawn: deploy the dashboard again.", DatabaseText.SchemaChanged);

    [Theory]
    [InlineData(-1L, "-1 rows")]
    [InlineData(-1234L, "-1,234 rows")]
    [InlineData(1000L, "1,000 rows")]
    [InlineData(long.MaxValue, "9,223,372,036,854,775,807 rows")]
    public void ARowCountAtTheEndsIsGroupedAndPlural(long rows, string text) =>
        Assert.Equal(text, DatabaseText.Rows(rows));

    [Theory]
    [InlineData("", "", "t__")]
    [InlineData("", "Orphan", "t__Orphan")]
    [InlineData("dbo", "", "t_dbo_")]
    [InlineData("my_schema", "Table_1", "t_my_schema_Table_1")]
    [InlineData("dbo", "a.b[c]", "t_dbo_a_b_c_")]
    [InlineData("dbo", "\U0001F600", "t_dbo___")]
    public void AnAliasKeepsDigitsAndUnderscoresAndReplacesEveryOtherCharacter(string schema, string name, string alias) =>
        Assert.Equal(alias, DatabaseText.Alias(schema, name));

    [Fact]
    public void ATableWithNeitherSchemaNorNameIsNamedByNothing() =>
        Assert.Equal(string.Empty, DatabaseText.QualifiedTable(string.Empty, string.Empty));

    [Fact]
    public void TheBoxIsTheFirstSqlNodeAndNoneWithoutNodes()
    {
        var first = Sql("first");
        var manifest = new RuntimeManifest("tdd", [new RuntimeNode("web", RuntimeNodeKind.WebApp, "app-tdd"), first, Sql("second")], [], []);

        Assert.Same(first, DatabaseText.Box(manifest));
        Assert.Null(DatabaseText.Box(new RuntimeManifest("tdd", [], [], [])));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.Box(null!));
    }

    [Fact]
    public void TheBoxLabelIsFixedWordsAroundTheNameAsGiven()
    {
        Assert.Equal("Database : open its tables", DatabaseText.BoxLabel(Sql(name: string.Empty)));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.BoxLabel(null!));
    }

    [Fact]
    public void TheTopologysDatabaseLinkWinsOverTheNodesOwn()
    {
        var box = Sql(links: Links(LinkSet.Portal, "https://portal.example.net/#own"));

        var link = Assert.Single(DatabaseText.Links(box, Links(LinkSet.Database, "https://portal.example.net/#topology")));

        Assert.Equal("https://portal.example.net/#topology", link.Href);
        Assert.Equal("Open in the Azure portal", link.Text);
    }

    [Fact]
    public void ATopologyWithoutADatabaseLinkFallsBackToTheNodesOwn()
    {
        var box = Sql(links: Links(LinkSet.Portal, "https://portal.example.net/#own"));

        var link = Assert.Single(DatabaseText.Links(box, Links(LinkSet.Logs, "https://portal.example.net/#logs")));

        Assert.Equal("https://portal.example.net/#own", link.Href);
        Assert.Equal("Open in the Azure portal", link.Text);
    }

    [Fact]
    public void ALinkOutsideThePortalOpensTheDatabaseByName()
    {
        var box = Sql(links: Links(RuntimeBoxLink.Site, "https://db.example.net/"));

        var link = Assert.Single(DatabaseText.Links(box, null));

        Assert.Equal("Open sql-tdd", link.Text);
        Assert.Equal("https://db.example.net/", link.Href);
        Assert.Throws<ArgumentNullException>(() => DatabaseText.Links(null!, null));
    }

    [Fact]
    public void AnEmptyManifestHasNoCountsNoListAndNoChangeAgainstAnEmptyLiveFile()
    {
        var empty = ManifestOf();

        Assert.Empty(DatabaseText.Counts(empty, null));
        Assert.Empty(DatabaseText.Counts(empty, LiveOf()));
        Assert.Empty(DatabaseText.List(empty, null));
        Assert.Empty(DatabaseText.List(empty, LiveOf()));
        Assert.False(DatabaseText.HasSchemaChanged(empty, LiveOf()));
    }

    [Fact]
    public void EveryManifestMemberRefusesNoManifest()
    {
        Assert.Throws<ArgumentNullException>(() => DatabaseText.Counts(null!, null));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.HasSchemaChanged(null!, null));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.ReadAt(null!, null));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.ReadLine(null!, null, Noon));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.TruncatedLine(null!));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.List(null!, null));
        Assert.Throws<ArgumentNullException>(() => DatabaseText.Key(null!));
    }

    [Fact]
    public void ALiveCountOfZeroReplacesTheDrawnCountAndALiveTableWithoutOneKeepsIt()
    {
        var manifest = ManifestOf(Table("dbo", "Employee", rows: 10), Table("dbo", "WorkOrder", rows: 20));

        Assert.Equal(
            [new DatabaseCount("db.t_dbo_Employee", "0 rows"), new DatabaseCount("db.t_dbo_WorkOrder", "20 rows")],
            DatabaseText.Counts(manifest, LiveOf(LiveTable("dbo", "Employee", 0), LiveTable("dbo", "WorkOrder"))));
    }

    [Fact]
    public void ALiveTableIsMatchedByItsAliasAndTheFirstOfTwoWithOneAliasWins()
    {
        var manifest = ManifestOf(Table("sales", "Order Line", rows: 1));

        Assert.Equal(
            [new DatabaseCount("db.t_sales_Order_Line", "7 rows")],
            DatabaseText.Counts(manifest, LiveOf(LiveTable("sales", "Order Line", 7), LiveTable("sales", "Order-Line", 9))));
        Assert.False(DatabaseText.HasSchemaChanged(manifest, LiveOf(LiveTable("sales", "Order-Line", 9))));
    }

    [Fact]
    public void AnEmptyDiagramMeetsAnyLiveTableAsAChangeUnlessItIsTruncated()
    {
        var live = LiveOf(LiveTable("dbo", "Employee"));

        Assert.True(DatabaseText.HasSchemaChanged(ManifestOf(), live));
        Assert.False(DatabaseText.HasSchemaChanged(ManifestOf() with { Truncated = true }, live));
    }

    [Fact]
    public void ALiveFileOfTheSameMomentIsNotNewerThanTheDeployment()
    {
        var manifest = ManifestOf() with { ReadAt = Noon };

        Assert.Equal((Noon, false), DatabaseText.ReadAt(manifest, LiveOf() with { Generated = Noon }));
        Assert.Equal((Noon.AddTicks(1), true), DatabaseText.ReadAt(manifest, LiveOf() with { Generated = Noon.AddTicks(1) }));
    }

    [Fact]
    public void NeitherMomentIsNoMomentAndNoLine()
    {
        Assert.Equal(((DateTimeOffset?)null, false), DatabaseText.ReadAt(ManifestOf(), null));
        Assert.Equal(((DateTimeOffset?)null, false), DatabaseText.ReadAt(ManifestOf(), LiveOf()));
        Assert.Null(DatabaseText.ReadLine(ManifestOf(), LiveOf(), Noon));
    }

    [Fact]
    public void TheReadLineSaysTheAgoOfTheMomentItChose()
    {
        var manifest = ManifestOf() with { ReadAt = Noon.AddHours(-5) };

        Assert.Equal(
            $"Schema read {TimeText.Ago(Noon.AddHours(-5), Noon)} (at the environment's last deployment)",
            DatabaseText.ReadLine(manifest, LiveOf(), Noon));
        Assert.Equal(
            $"Schema read {TimeText.Ago(Noon.AddHours(-1), Noon)}",
            DatabaseText.ReadLine(manifest, LiveOf() with { Generated = Noon.AddHours(-1) }, Noon));
    }

    [Theory]
    [InlineData(0, "The diagram draws the first 0 tables.")]
    [InlineData(1, "The diagram draws the first 1 tables.")]
    [InlineData(2000, "The diagram draws the first 2000 tables.")]
    public void TheTruncatedLineWritesTheDrawnCountDigitByDigit(int drawn, string line) =>
        Assert.Equal(line, DatabaseText.TruncatedLine(ManifestOf() with { Truncated = true, Drawn = drawn }));

    [Fact]
    public void AnEmptyLiveFileIsAnEmptyListEvenWhenTheDiagramHasTables()
    {
        var manifest = ManifestOf(Table("dbo", "Employee", rows: 3, columns: 4));

        Assert.Empty(DatabaseText.List(manifest, LiveOf()));
        Assert.Equal([new DatabaseListRow("dbo.Employee", 4, 3)], DatabaseText.List(manifest, null));
    }

    [Fact]
    public void TheListOfTheLiveFileCountsItsColumnsAndKeepsAnUncountedTable()
    {
        var live = LiveOf(new DatabaseLiveTable("", "Orphan", null, [new DatabaseColumn("Id", "int", false, "pk")]));

        Assert.Equal([new DatabaseListRow("Orphan", 1, null)], DatabaseText.List(ManifestOf(Table("dbo", "Employee")), live));
        Assert.Equal([new DatabaseListRow("dbo.Employee", null, null)], DatabaseText.List(ManifestOf(Table("dbo", "Employee")), null));
    }

    [Theory]
    [InlineData(",", "")]
    [InlineData(" pk , fk ", "primary key, foreign key")]
    [InlineData("pk,,fk", "primary key, foreign key")]
    [InlineData("fk,pk", "foreign key, primary key")]
    [InlineData("PK", "PK")]
    [InlineData("pk,unique", "primary key, unique")]
    public void AKeyIsTrimmedSplitAndOnlyLowerCaseKeysAreSaidInWords(string key, string words) =>
        Assert.Equal(words, DatabaseText.Key(key));

    [Theory]
    [InlineData(0L, "0")]
    [InlineData(999L, "999")]
    [InlineData(1000L, "1,000")]
    [InlineData(-1234L, "-1,234")]
    [InlineData(long.MaxValue, "9,223,372,036,854,775,807")]
    [InlineData(long.MinValue, "-9,223,372,036,854,775,808")]
    public void ANumberAtTheEndsIsGroupedInThousands(long number, string text) =>
        Assert.Equal(text, DatabaseText.Number(number));

    [Fact]
    public void NotSaidIsTheEnDashAndNotAHyphen()
    {
        Assert.Equal("\u2013", DatabaseText.NotSaid);
        Assert.NotEqual("-", DatabaseText.NotSaid);
    }

    [Fact]
    public void NullableAndNumberAgreeOnNotSaid()
    {
        Assert.Equal(DatabaseText.NotSaid, DatabaseText.Nullable(null));
        Assert.Equal(DatabaseText.NotSaid, DatabaseText.Number(null));
    }
}
