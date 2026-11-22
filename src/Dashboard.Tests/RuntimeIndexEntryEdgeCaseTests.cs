using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeIndexEntry" />: one environment of
/// <c>runtime/index.json</c>, the three parts that say what it is called and which two files of <c>runtime/</c> hold
/// its diagram, and the fourth part the environment may name - the two files of its database view. The record decides
/// nothing of its own: no verdict, no words for the page, no moment, no address. What is pinned here is that each
/// part comes back as the part it went in as, that the record refuses nothing it is handed, and that the members
/// every record is asked compare, hash, fall apart, copy and say the four parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeManifestParserEdgeCaseTests" /> and
/// <see cref="RuntimeManifestParserTests" /> pin the read out of the file: that <c>name</c> is required and arrives
/// trimmed, that a <c>manifest</c> and an <c>svg</c> that are no text or an empty one fall back to the environment's
/// own name, that a file with a slash, a backslash, a colon or a leading dot stops the index, and that a
/// <c>database</c> member that names no two plain files of <c>runtime/</c> is left out rather than refused.
/// <see cref="RuntimeIndexFrameAndParseResultTests" /> pins the entry as one of the list of an index - the names read
/// back out of it, the database of the one environment and the absence of it of the next - and pins the index and the
/// frame as values in the same way the tests below pin an entry. <see cref="DatabaseViewTests" /> pins which entries
/// of the sample files have a database view and which have none, and
/// <see cref="RuntimeLoaderTests" /> pins that the loader asks the site for the entry's two files.
/// </para>
/// <para>
/// What an entry <em>means</em> is nobody's decision but the page's, and it is pinned there: that the name is the
/// environment's word in the topology, matched case-free to pick the diagram to show, that the two files are opened
/// under <c>runtime/</c> by the loader and never as addresses of their own, and that an entry with no database is a
/// box that leads to the Azure portal instead of into a view. What follows is the record on its own, at the ends the
/// tests above leave open: the four parts by the names of their own properties, the fourth standing absent when the
/// caller does not name it, the empty word that is a word and the spaces and the letters that only look alike, the
/// name never weighed against the two file names it is conventionally the stem of, the database held as the value it
/// is - equal by its two files, and the very reference it was handed - and the whole public surface, so that the day
/// a fifth part is added the list of them says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: an entry holds three words and, where the
/// deployment recorded a schema, two more, calls nothing at all, and every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimeIndexEntryEdgeCaseTests
{
    private const string Name = "tdd";
    private const string Manifest = "tdd.json";
    private const string Svg = "tdd.svg";

    /// <summary>The two files of the database view, in the order the record of them is declared: the diagram first,
    /// the manifest after - the other way round from the entry's own two files, so that a part landing on the wrong
    /// one of them is seen rather than quietly matching.</summary>
    private const string DatabaseSvg = "tdd-db.svg";
    private const string DatabaseManifest = "tdd-db.json";

    /// <summary>An entry with every part named, the three words apart from one another and a database view of its
    /// own, so that a part landing on another property is seen instead of quietly matching.</summary>
    private static RuntimeIndexEntry Full => new(Name, Manifest, Svg, Files);

    /// <summary>An entry named by nothing but the three a caller must name, the database standing as it stands when
    /// nobody names it.</summary>
    private static RuntimeIndexEntry Bare => new(Name, Manifest, Svg);

    /// <summary>The two files of a database view, as the deployment writes them.</summary>
    private static RuntimeDatabaseFiles Files => new(DatabaseSvg, DatabaseManifest);

    /// <summary>The entry of nothing at all: no words, and no database where the deployment recorded no schema.</summary>
    private static RuntimeIndexEntry Nothing => new(null!, null!, null!);

    /// <summary>The entry of empty words: a part said as nothing, which the record keeps as a word and not as the
    /// absence of one.</summary>
    private static RuntimeIndexEntry Empty => new(string.Empty, string.Empty, string.Empty);

    // -------------------------------------------------------- the four parts, by the names of their own properties

    /// <summary>The constructor's four parts, position by position, come back as the property of the same name - the
    /// order the reader of an index hands an entry over in, and the order the page writes the two files out of. Each
    /// part keeps the value it was given and nothing about it is worked out of the others: an entry named for one
    /// environment and pointing at another environment's files keeps both, and an entry of one word stands with the
    /// files unnamed.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var entry = Full;

        Assert.Equal(Name, entry.Name);
        Assert.Equal(Manifest, entry.Manifest);
        Assert.Equal(Svg, entry.Svg);
        Assert.Equal(Files, entry.Database);

        var uat = new RuntimeIndexEntry("uat", "uat-prod.json", "uat-prod.svg", new RuntimeDatabaseFiles("uat.database.svg", "uat.database.json"));

        Assert.Equal("uat", uat.Name);
        Assert.Equal("uat-prod.json", uat.Manifest);
        Assert.Equal("uat-prod.svg", uat.Svg);
        Assert.Equal("uat.database.svg", uat.Database!.Svg);
        Assert.Equal("uat.database.json", uat.Database.Manifest);

        // The name is no part of the two file names: the record reads nothing out of either.
        var crossed = new RuntimeIndexEntry("prod", "tdd.json", "tdd.svg");

        Assert.Equal("prod", crossed.Name);
        Assert.Equal(Manifest, crossed.Manifest);
        Assert.Equal(Svg, crossed.Svg);
        Assert.Null(crossed.Database);
    }

    /// <summary>The database is the one part a caller may leave out, and it stands absent when it does - absent, and
    /// not the two files of a view that names nothing, which the page could read as a database box to open. Naming it
    /// as nothing is the same as not naming it, and the declaration of that default is read off the type in the
    /// surface test at the end of this file.</summary>
    [Fact]
    public void TheDatabaseTheCallerDoesNotNameStandsAbsent()
    {
        Assert.Null(Bare.Database);
        Assert.Null(new RuntimeIndexEntry(Name, Manifest, Svg, null).Database);
        Assert.Null(Empty.Database);
        Assert.Null(Nothing.Database);

        // The absence of the part is the absence of a view, and the entry whose database is named as nothing is the
        // entry whose database is not named at all.
        Assert.Equal(Bare, Bare with { Database = null });
        Assert.Equal(Bare.GetHashCode(), (Bare with { Database = null }).GetHashCode());
        Assert.Single(new HashSet<RuntimeIndexEntry> { Bare, new RuntimeIndexEntry(Name, Manifest, Svg, null) });

        // The files of a view that names no two files are still a view the entry holds - the record of them decides
        // nothing, and it is the reader of the file that leaves such a member out (pinned with the reader).
        Assert.NotNull((Bare with { Database = new RuntimeDatabaseFiles(string.Empty, string.Empty) }).Database);
        Assert.NotEqual(Bare, Bare with { Database = new RuntimeDatabaseFiles(string.Empty, string.Empty) });
    }

    /// <summary>Every word is kept exactly as it is handed: the empty word is a word and not nothing, the spaces
    /// about a word stay about it, a word of one letter is a word, and a word of letters that only look like the
    /// Latin ones is kept letter for letter. (That the file's words arrive trimmed, and that an index may not name an
    /// environment without a word for it, is the reader's and is pinned with the reader.)</summary>
    [Fact]
    public void AnEntryKeepsItsWordsExactlyAsTheyAreGiven()
    {
        var empty = Empty;

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.Manifest);
        Assert.Equal(string.Empty, empty.Svg);

        var oneLetter = new RuntimeIndexEntry("u", "u", "u", new RuntimeDatabaseFiles("u", "u"));

        Assert.Equal("u", oneLetter.Name);
        Assert.Equal("u", oneLetter.Manifest);
        Assert.Equal("u", oneLetter.Svg);
        Assert.Equal(new RuntimeDatabaseFiles("u", "u"), oneLetter.Database);

        var spaced = new RuntimeIndexEntry("  tdd  ", " tdd.json ", " tdd.svg ", new RuntimeDatabaseFiles(" tdd-db.svg ", " tdd-db.json "));

        Assert.Equal("  tdd  ", spaced.Name);
        Assert.Equal(" tdd.json ", spaced.Manifest);
        Assert.Equal(" tdd.svg ", spaced.Svg);
        Assert.Equal(" tdd-db.svg ", spaced.Database!.Svg);
        Assert.Equal(" tdd-db.json ", spaced.Database.Manifest);

        var written = new RuntimeIndexEntry("ｐｒｏｄ", "prōd.json", "prod-日本.svg");

        Assert.Equal("ｐｒｏｄ", written.Name);
        Assert.Equal("prōd.json", written.Manifest);
        Assert.Equal("prod-日本.svg", written.Svg);

        // A name and a file of no extension at all, and a word of a thousand letters, are kept as they stand: the
        // shape of a file name is the reader's question, and the page asks the site for whatever it is told.
        var longName = new string('x', 1_000);
        var longEntry = new RuntimeIndexEntry(longName, "prod", "prod", new RuntimeDatabaseFiles(longName, longName));

        Assert.Equal(longName, longEntry.Name);
        Assert.Equal("prod", longEntry.Manifest);
        Assert.Equal(longName, longEntry.Database!.Manifest);
    }

    /// <summary>The record makes no decision of its own about what a part may be: an entry will hold no words at all,
    /// since it is the file that is read and the reader that refuses one. Only the database can be nothing; the three
    /// words are non-nullable in the type and the caller that hands the record a null of one finds it kept.</summary>
    [Fact]
    public void AnEntryRefusesNothingItIsGiven()
    {
        var nothing = Nothing;

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Manifest);
        Assert.Null(nothing.Svg);
        Assert.Null(nothing.Database);

        // Nothing it is not told, and empty words it is told, are two entries: the absence of a word is no word.
        Assert.NotEqual(nothing, Empty);
        Assert.True(nothing != Empty);
        Assert.False(nothing.Equals(Empty));

        // Two entries of nothing at all, made by two calls, are one entry.
        Assert.Equal(nothing, new RuntimeIndexEntry(null!, null!, null!, null));
        Assert.Single(new HashSet<RuntimeIndexEntry> { nothing, new RuntimeIndexEntry(null!, null!, null!, null) });

        // The name is the word the page looks an entry out by; the record still does not insist on one.
        Assert.Null(new RuntimeIndexEntry(null!, Manifest, Svg).Name);
    }

    // ------------------------------------------------------------------- the two files of the database view

    /// <summary>The database is the one part that is a value of its own rather than a word: the entry holds the very
    /// pair of files it is handed, both of them read back in the order the record of them declares - the diagram
    /// first, the manifest after, which is the other way round from the entry's own two files - and a pair of nothing
    /// is no pair at all.</summary>
    [Fact]
    public void TheDatabaseIsTheTwoFilesItWasHandedAndHoldsThemAsItsOwn()
    {
        var files = Files;
        var entry = new RuntimeIndexEntry(Name, Manifest, Svg, files);

        Assert.Same(files, entry.Database);
        Assert.Equal(DatabaseSvg, entry.Database!.Svg);
        Assert.Equal(DatabaseManifest, entry.Database.Manifest);

        // A copy names the part again and the entry holds that pair instead, by reference as it held the first.
        var other = new RuntimeDatabaseFiles("uat.database.svg", "uat.database.json");

        Assert.NotSame(files, other);
        Assert.Same(other, (entry with { Database = other }).Database);
        Assert.Same(files, entry.Database);

        // The pair of nothing is the absence of a view, and a view of two empty names is a view all the same.
        Assert.Null((entry with { Database = null! }).Database);
        Assert.Equal(new RuntimeDatabaseFiles(string.Empty, string.Empty), (entry with { Database = new RuntimeDatabaseFiles(string.Empty, string.Empty) }).Database);
    }

    /// <summary>Two entries whose databases are two calls on the same two words are one entry - the part is weighed
    /// as the record of it weighs itself, value by value, and not by the reference the call happened to bring - while
    /// a file exchanged for the other, a file added, or a file of another word is another entry.</summary>
    [Fact]
    public void AnEntryIsOneWithAnotherWhileTheSameTwoFilesAreNamed()
    {
        var one = Full;
        var two = new RuntimeIndexEntry(Name, Manifest, Svg, new RuntimeDatabaseFiles(DatabaseSvg, DatabaseManifest));

        Assert.NotSame(one.Database, two.Database);
        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Single(new HashSet<RuntimeIndexEntry> { one, two });

        // The two files exchanged is another view, and an empty one is neither that view nor no view.
        Assert.NotEqual(one, one with { Database = new RuntimeDatabaseFiles(DatabaseManifest, DatabaseSvg) });
        Assert.NotEqual(one, one with { Database = new RuntimeDatabaseFiles(DatabaseSvg, string.Empty) });
        Assert.NotEqual(one, one with { Database = new RuntimeDatabaseFiles(string.Empty, DatabaseManifest) });
        Assert.NotEqual(one, one with { Database = null });

        // The entry of no view and the entry of an unnamed one are two entries; the entry of no view and the entry of
        // the same two files twice over are two more.
        Assert.NotEqual(Bare, Full);
        Assert.Equal(2, new HashSet<RuntimeIndexEntry> { Bare, Full }.Count);
    }

    // ------------------------------------------------------------------------ the record of the four parts

    /// <summary>Two entries are one while their four parts are, whichever of the ways of asking is used: the record's
    /// own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, and the hash that follows them
    /// and stands from call to call. An entry is compared, not looked through: two calls on the same three words and
    /// the same two files are one entry, and a set of them holds them once.</summary>
    [Fact]
    public void TwoEntriesAreOneWhileTheirFourPartsAre()
    {
        var one = Full;
        var two = new RuntimeIndexEntry(Name, Manifest, Svg, Files);

        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.True(one.Equals(one));
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<RuntimeIndexEntry> { one, two });

        // The parts asked out and put back in make the same entry again: the equality is of the values, not of the
        // call that gathered them.
        one.Deconstruct(out var name, out var manifest, out var svg, out var database);

        Assert.Equal(one, new RuntimeIndexEntry(name, manifest, svg, database));
        Assert.Equal(one.GetHashCode(), new RuntimeIndexEntry(name, manifest, svg, database).GetHashCode());
    }

    /// <summary>Any one of the four parts tells two entries apart, and nothing is compared loosely: the words
    /// ordinally, so a name or a file in another case, or with a space on it, or with a letter that only looks like
    /// the one expected, is another word. The empty word, the word of spaces and the nothing that is no word are three
    /// parts of their own.</summary>
    [Fact]
    public void AnEntryDiffersInAnyOneOfItsParts()
    {
        var entry = Full;

        Assert.NotEqual(entry, entry with { Name = string.Empty });
        Assert.NotEqual(entry, entry with { Name = "  " });
        Assert.NotEqual(entry, entry with { Name = null! });
        Assert.NotEqual(entry, entry with { Name = "TDD" });
        Assert.NotEqual(entry, entry with { Name = "tdd " });
        Assert.NotEqual(entry, entry with { Name = "uat" });
        Assert.NotEqual(entry, entry with { Manifest = string.Empty });
        Assert.NotEqual(entry, entry with { Manifest = null! });
        Assert.NotEqual(entry, entry with { Manifest = "TDD.JSON" });
        Assert.NotEqual(entry, entry with { Manifest = "tdd.json.txt" });
        Assert.NotEqual(entry, entry with { Svg = string.Empty });
        Assert.NotEqual(entry, entry with { Svg = null! });
        Assert.NotEqual(entry, entry with { Svg = "tdd.json" });
        Assert.NotEqual(entry, entry with { Database = null });
        Assert.NotEqual(entry, entry with { Database = new RuntimeDatabaseFiles(DatabaseSvg, "tdd-db.json ") });

        // The manifest and the SVG are two parts: an entry whose two files are one and the same word is another
        // entry from the one that names them apart.
        Assert.NotEqual(entry, new RuntimeIndexEntry(Name, "tdd.svg", "tdd.svg", entry.Database));
        Assert.NotEqual(entry, entry with { Name = "ｔｄｄ" });

        // The empty word and the nothing are two parts, and a word of spaces is a third.
        Assert.NotEqual(new RuntimeIndexEntry(string.Empty, Manifest, Svg), new RuntimeIndexEntry(null!, Manifest, Svg));
        Assert.NotEqual(new RuntimeIndexEntry(" ", Manifest, Svg), new RuntimeIndexEntry(null!, Manifest, Svg));

        // The entries that differ from this one by one part are four apart.
        Assert.Equal(
            4,
            new HashSet<RuntimeIndexEntry>
            {
                entry,
                entry with { Name = "uat" },
                entry with { Manifest = "uat.json" },
                entry with { Svg = "uat.svg" },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two entries that are no entry
    /// at all are one, an entry and no entry are two - the equality and the inequality each saying it, from either
    /// side - and the entry asked about itself says no to the nothing, to the name it is called by and to the file it
    /// points at. The entry of nothing at all is still an entry.</summary>
    [Fact]
    public void NoEntryIsOneWithNoEntryAndTheEntryOfNothingIsAnEntry()
    {
        RuntimeIndexEntry? nothing = null;
        RuntimeIndexEntry? alsoNothing = null;
        var there = Full;

        Assert.Null(default(RuntimeIndexEntry));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((RuntimeIndexEntry?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(Name));
        Assert.False(there.Equals(Files));
        Assert.False(there.Equals(new RuntimeIndex([there], "1.2026.8")));
        Assert.False(there.Equals(new RuntimeManifest(Name, [], [], [])));
        Assert.False(there.Equals(new RuntimeEdge("a-to-b", "a", "b", RuntimeEdgeKind.Sql)));
        Assert.False(there.Equals(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sqldb-demo")));

        var alsoEmpty = Empty;

        Assert.Equal(Empty, alsoEmpty);
        Assert.NotSame(Empty, alsoEmpty);
        Assert.Equal(Empty.GetHashCode(), alsoEmpty.GetHashCode());
        Assert.True(Empty == alsoEmpty);
        Assert.Single(new HashSet<RuntimeIndexEntry> { Empty, alsoEmpty });
    }

    /// <summary>The name is held as it is written and is never weighed against anything: not against the two file
    /// names the deployment conventionally makes out of it, and not against the name of a manifest read out of the
    /// file it points at. Only the very word the constructor was given makes the copied entry the entry it was
    /// copied from - and a page that looks an entry out by its name asks case-free, which is the page's and is
    /// pinned there, not here.</summary>
    [Theory]
    [InlineData("tdd", true)]
    [InlineData("TDD", false)]
    [InlineData(" tdd", false)]
    [InlineData("uat", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void TheNameIsHeldAsItIsWrittenAndNeverCheckedAgainstTheTwoFiles(string? name, bool isTheVeryEntry)
    {
        var entry = Full with { Name = name! };

        Assert.Equal(name, entry.Name);
        Assert.Equal(Manifest, entry.Manifest);
        Assert.Equal(Svg, entry.Svg);
        Assert.Equal(Files, entry.Database);
        Assert.Contains($"Name = {name}", entry.ToString(), StringComparison.Ordinal);
        Assert.Equal(isTheVeryEntry, entry == Full);
    }

    // ------------------------------------------------------------------------------------------ copies

    /// <summary><c>with</c> names the part it changes and carries the other three: a copy that names nothing is one
    /// with its original in another reference, the original is left where it stood, the fourth part may be put on and
    /// taken off as the three words are, and a copy of the entry of a database view that names the files apart from
    /// one another keeps naming them apart.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var entry = Full;
        var copy = entry with { };

        Assert.Equal(entry, copy);
        Assert.NotSame(entry, copy);
        Assert.True(entry == copy);

        var withoutView = entry with { Database = null };

        Assert.Null(withoutView.Database);
        Assert.Equal(Name, withoutView.Name);
        Assert.Equal(Manifest, withoutView.Manifest);
        Assert.Equal(Svg, withoutView.Svg);
        Assert.Equal(Files, entry.Database);

        var ofAnotherEnvironment = entry with { Name = "uat", Manifest = "uat.json", Svg = "uat.svg" };

        Assert.Equal("uat", ofAnotherEnvironment.Name);
        Assert.Equal("uat.json", ofAnotherEnvironment.Manifest);
        Assert.Equal("uat.svg", ofAnotherEnvironment.Svg);
        Assert.Equal(Files, ofAnotherEnvironment.Database);
        Assert.NotEqual(entry, ofAnotherEnvironment);

        // A copy that names the parts the entry already holds is the entry, and a copy of the entry of nothing is the
        // entry of nothing.
        Assert.Equal(entry, entry with { Name = Name, Manifest = Manifest, Svg = Svg, Database = Files });
        Assert.Equal(Nothing, Nothing with { Name = null!, Manifest = null!, Svg = null!, Database = null });
    }

    /// <summary>The four parts fall out of an entry in the order they went in, each as the value it holds, and what
    /// came out goes back in the same way; the part the entry was not told falls out as the nothing it is, and the
    /// entry that was told nothing at all is an entry all the same, database and all.</summary>
    [Fact]
    public void TheFourPartsFallOutOfAnEntryInTheOrderTheyWentIn()
    {
        var (name, manifest, svg, database) = Full;

        Assert.Equal(Name, name);
        Assert.Equal(Manifest, manifest);
        Assert.Equal(Svg, svg);
        Assert.Equal(Files, database);

        var bare = Bare;
        bare.Deconstruct(out var bareName, out var bareManifest, out var bareSvg, out var bareDatabase);

        Assert.Equal(Name, bareName);
        Assert.Equal(Manifest, bareManifest);
        Assert.Equal(Svg, bareSvg);
        Assert.Null(bareDatabase);
        Assert.Equal(bare, new RuntimeIndexEntry(bareName, bareManifest, bareSvg, bareDatabase));

        var (nothingName, nothingManifest, nothingSvg, nothingDatabase) = Nothing;

        Assert.Null(nothingName);
        Assert.Null(nothingManifest);
        Assert.Null(nothingSvg);
        Assert.Null(nothingDatabase);

        // One part asked out of the four, the rest of them left where they are.
        var (only, _, _, _) = new RuntimeIndexEntry("prod", "prod.json", "prod.svg");

        Assert.Equal("prod", only);

        // The view falls out whole, its own two parts in the order its own record declares them.
        var (_, _, _, view) = new RuntimeIndexEntry("uat", "uat.json", "uat.svg", new RuntimeDatabaseFiles("uat.database.svg", "uat.database.json"));
        var (viewSvg, viewManifest) = view!;

        Assert.Equal("uat.database.svg", viewSvg);
        Assert.Equal("uat.database.json", viewManifest);
    }

    // --------------------------------------------------------------------------------------------- words

    /// <summary>The words an entry says about itself - its name, then its four parts in the order they are declared,
    /// each named and standing blank when there is nothing of it, the fourth saying its two files inside itself - are
    /// the record's own and not the page's: the page is given its words by the payload builder and the components. The
    /// whole entry stands here, so a part moved about in the record is a change the tests see.</summary>
    [Fact]
    public void AnEntrySaysItsFourPartsInTheirOrder()
    {
        Assert.Equal(
            "RuntimeIndexEntry { Name = tdd, Manifest = tdd.json, Svg = tdd.svg, Database = RuntimeDatabaseFiles { Svg = tdd-db.svg, Manifest = tdd-db.json } }",
            Full.ToString());

        // The part a caller may leave out is named and says nothing.
        Assert.Equal(
            "RuntimeIndexEntry { Name = tdd, Manifest = tdd.json, Svg = tdd.svg, Database =  }",
            Bare.ToString());

        Assert.Equal(
            "RuntimeIndexEntry { Name = , Manifest = , Svg = , Database =  }",
            Empty.ToString());
        Assert.Equal(
            "RuntimeIndexEntry { Name = , Manifest = , Svg = , Database =  }",
            Nothing.ToString());

        // A word is said as it stands, its spaces and all, and no part is put in quotes.
        Assert.Equal(
            "RuntimeIndexEntry { Name =  tdd , Manifest = tdd.json, Svg = tdd.svg, Database =  }",
            new RuntimeIndexEntry(" tdd ", Manifest, Svg).ToString());
        Assert.Equal(
            "RuntimeIndexEntry { Name = tdd, Manifest = tdd.json, Svg = tdd.svg, Database = RuntimeDatabaseFiles { Svg = , Manifest =  } }",
            new RuntimeIndexEntry(Name, Manifest, Svg, new RuntimeDatabaseFiles(string.Empty, string.Empty)).ToString());
    }

    /// <summary>An entry's words are nothing but words, so no reader's culture turns them: a viewer of German is told
    /// the same entry as the invariant reader - the same name, the same files, the same pair of a database view -
    /// which is what a log of one of these entries may count on whatever the reader of the machine happens to be.</summary>
    [Fact]
    public void TheEntrysOwnWordsAreTheSameWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "RuntimeIndexEntry { Name = tdd, Manifest = tdd.json, Svg = tdd.svg, Database = RuntimeDatabaseFiles { Svg = tdd-db.svg, Manifest = tdd-db.json } }",
                Full.ToString());
            Assert.Equal(
                "RuntimeIndexEntry { Name = tdd, Manifest = tdd.json, Svg = tdd.svg, Database =  }",
                Bare.ToString());
            Assert.Equal(Full, new RuntimeIndexEntry(Name, Manifest, Svg, Files));
        });
    }

    /// <summary>The parts are weighed as words and not as a reader spells them: two entries whose words differ only in
    /// case are two entries whatever culture the thread runs under, and the words that are one word are one entry
    /// under another reader too - the comparison is ordinal, as the manifest's words are. The dotted and the dotless
    /// <c>i</c>, which one reader's capital of a small letter turns into the other's, are two names here.</summary>
    [Fact]
    public void TheWordsOfAnEntryAreWeighedOrdinallyWhateverTheReaderSpeaks()
    {
        var upper = new RuntimeIndexEntry("TDD", "TDD.JSON", "TDD.SVG");

        ViewerCulture.German(() =>
        {
            Assert.NotEqual(Bare, upper);
            Assert.True(Bare != upper);
            Assert.False(Bare.Equals((object)upper));
        });

        Assert.NotEqual(Bare, Bare with { Name = "ＴＤＤ" });
        Assert.NotEqual(Bare, Bare with { Name = "Tdd" });
        Assert.Equal(Bare, new RuntimeIndexEntry(Name, Manifest, Svg));

        var dotted = new RuntimeIndexEntry("ist", "ist.json", "ist.svg");
        var dotless = new RuntimeIndexEntry("ıst", "ist.json", "ist.svg");

        Assert.NotEqual(dotted, dotless);
        Assert.NotEqual(dotted with { Manifest = "İst.json" }, dotted);
    }

    // ---------------------------------------------------------------------------- the record's own shape

    /// <summary>The entry's whole public surface as it stands: its four parts, named in the order the constructor
    /// names them and of the type the constructor says, the three a caller must name and the one it may leave out as
    /// nothing, and the members every record is asked - falling apart, comparing, hashing and saying itself - and
    /// nothing else to call. There is no verdict on an environment to read and none to set: what a diagram is comes
    /// from the two files, and the page decides from them. A fifth part added to the record has to be written down
    /// here as well, which is the point: an environment of a diagram cannot carry a part nobody decided what it
    /// means.</summary>
    [Fact]
    public void TheEntryHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Name", "Manifest", "Svg", "Database"],
            [.. typeof(RuntimeIndexEntry)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Database", "Deconstruct", "Equals", "GetHashCode", "Manifest", "Name", "Svg", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        var properties = typeof(RuntimeIndexEntry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(string), properties["Manifest"].PropertyType);
        Assert.Equal(typeof(string), properties["Svg"].PropertyType);
        Assert.Equal(typeof(RuntimeDatabaseFiles), properties["Database"].PropertyType);

        Assert.All(
            properties.Values,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.True(property.CanWrite);
            });

        // The one public constructor, naming its four parts as the properties are named; the copy constructor the
        // compiler writes for a record is not one a caller outside the record calls.
        var constructors = typeof(RuntimeIndexEntry).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Name", "Manifest", "Svg", "Database"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(RuntimeDatabaseFiles)],
            [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.Equal([false, false, false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.Null(parameters[3].DefaultValue);

        // The nullable annotations stand as the record declares them: the database may be nothing, the three words
        // may not - which is what the reader of the file leans on, and what the record itself does not enforce (a
        // caller may still hand it the nothing of a word, pinned above).
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        List<PropertyInfo> parts = [properties["Name"], properties["Manifest"], properties["Svg"], properties["Database"]];

        Assert.Equal(
            [false, false, false, true],
            [.. parts.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        Assert.Equal([typeof(IEquatable<RuntimeIndexEntry>)], typeof(RuntimeIndexEntry).GetInterfaces());
        Assert.True(typeof(RuntimeIndexEntry).IsSealed);
        Assert.Equal(typeof(object), typeof(RuntimeIndexEntry).BaseType);
        Assert.Empty(typeof(RuntimeIndexEntry).GetEvents());
        Assert.Empty(typeof(RuntimeIndexEntry).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the entry declares itself: its parts and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeIndexEntry)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
