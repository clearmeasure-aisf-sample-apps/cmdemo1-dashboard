using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The remaining member tests of <c>src/Dashboard/Runtime/RuntimeManifest.cs</c> (cmdemo1-dashboard#1347): the two
/// records that file declares and no file of its own pins - <see cref="RuntimeIndex" />, the
/// <c>runtime/index.json</c> that lists the environments whose diagram the deployment rendered and the PlantUML
/// version that rendered them, and
/// <see cref="RuntimeDatabaseFiles" />, the two files of one environment's database view. Neither record decides
/// anything of its own: no verdict, no words for the page, no moment, no address, no request. What is pinned here is
/// that each part comes back as the part it went in as, that the part a caller may leave out stands as nothing rather
/// than as the empty word, that the list of an index is held and never copied nor opened, that the records refuse
/// nothing they are handed, and that the members every record is asked compare, hash, fall apart, copy and say the
/// parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeIndexFrameAndParseResultTests" /> pins an index
/// read out of a file the test brought - the two names coming back in the order of <c>system.json</c>, one entry's
/// database and the next entry's absence of one, the version, and that an index is its values - and pins the frames of
/// a manifest and the <see cref="RuntimeParseResult{T}" /> beside them.
/// <see cref="RuntimeIndexEntryEdgeCaseTests" /> pins the entry the index's list holds, and pins the two files of a
/// database view only as far as an entry carries them: as a part of its own standing absent when nobody names it,
/// equal by its two files, another entry when the two are swapped, and said inside an entry's line.
/// <see cref="RuntimeManifestParserEdgeCaseTests" />, <see cref="RuntimeManifestParserBoundaryEdgeCaseTests" /> and
/// <see cref="RuntimeManifestParserJsonEdgeCaseTests" /> pin the reading of both records out of JSON, which is where
/// the trimming, the blank-as-nothing and the plain-file-of-<c>runtime/</c> rules live and not in the records.
/// <see cref="RuntimeManifestEdgeCaseTests" />, <see cref="RuntimeManifestSurfaceEdgeCaseTests" /> and
/// <see cref="RuntimeManifestBoundaryEdgeCaseTests" /> pin <see cref="RuntimeManifest" /> and
/// <see cref="RuntimeUnplaced" /> to the ends of their members, so nothing of those two is repeated here.
/// </para>
/// <para>
/// What stands open, and stands here, is each record on its own: the index's two parts by the names of their own
/// properties and the version's default, the version kept exactly as it was written - spaces, commas and letters that
/// only look alike included, the reader's trimming being another question - the list held so that the entries a page
/// reads back are the very entries it handed over, an index of no environments and an index of none at all being two
/// indexes, the words each record says of itself at every end of its parts, the hash standing from call to call while
/// each part sets one record apart, the deconstruction of both, and the whole public surface of both so that the day a
/// part is added the list of them says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: an index holds a list and a word, a pair of
/// files holds two words, neither calls anything, and every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimeManifestRecordsEdgeCaseTests
{
    private const string Svg = "uat.database.svg";
    private const string Manifest = "uat.database.json";
    private const string PlantUml = "1.2026.8";

    /// <summary>The line a whole index says of itself: the environment list named and never walked, which is why the
    /// same words serve an index of two environments and an index of none.</summary>
    private const string IndexWords =
        "RuntimeIndex { Environments = Dashboard.Runtime.RuntimeIndexEntry[], PlantUml = 1.2026.8 }";

    /// <summary>The line an index of no part at all says of itself: both parts named, both blank.</summary>
    private const string IndexBlanks = "RuntimeIndex { Environments = , PlantUml =  }";

    /// <summary>The two environments of an index, in the order <c>system.json</c> gives them, the second carrying a
    /// database view and the first standing without one.</summary>
    private static readonly RuntimeIndexEntry[] Environments =
    [
        new("tdd", "tdd.json", "tdd.svg"),
        new("uat", "uat-prod.json", "uat-prod.svg", Files),
    ];

    private static readonly RuntimeIndexEntry[] NoEnvironments = [];

    /// <summary>The two files of a database view as the deployment writes them: the diagram first, the manifest after
    /// - the other way round from the entry's own two files, so that a part landing on the wrong one is seen.</summary>
    private static RuntimeDatabaseFiles Files => new(Svg, Manifest);

    /// <summary>The pair of nothing at all: neither file named - the pair the record will hold although no file may
    /// say it.</summary>
    private static RuntimeDatabaseFiles NoPair => new(null!, null!);

    /// <summary>An index with both parts named.</summary>
    private static RuntimeIndex Full => new(Environments, PlantUml);

    /// <summary>An index of the part a caller must name: one from before the file recorded the version that rendered
    /// the diagrams.</summary>
    private static RuntimeIndex NoVersion => new(Environments);

    /// <summary>The index of no part at all: no list, no version - the index the record will hold although no file may
    /// say it.</summary>
    private static RuntimeIndex NoParts => new(null!, null!);

    // ------------------------------------------------------------------------ RuntimeDatabaseFiles, the two parts

    /// <summary>The constructor's two parts, position by position, come back as the property of the same name, and the
    /// two are never weighed against one another: the record of a view whose two files carry the same word holds both
    /// parts as given and derives nothing - no stem, no extension, no <c>runtime/</c> - from either.</summary>
    [Fact]
    public void TheConstructorHandsEachFileToThePropertyOfItsName()
    {
        var files = Files;

        Assert.Equal(Svg, files.Svg);
        Assert.Equal(Manifest, files.Manifest);

        // The order the record declares: the diagram of the tables first, the manifest of them after.
        var oneWord = new RuntimeDatabaseFiles("db", "db");

        Assert.Equal("db", oneWord.Svg);
        Assert.Equal("db", oneWord.Manifest);
        Assert.Equal(oneWord, oneWord with { Svg = "db", Manifest = "db" });

        // The other way round is another pair of files, and not the same view named twice.
        Assert.NotEqual(files, new RuntimeDatabaseFiles(Manifest, Svg));
        Assert.NotEqual(files.GetHashCode(), new RuntimeDatabaseFiles(Manifest, Svg).GetHashCode());
    }

    /// <summary>Both parts at their ends: the empty word, which the record keeps as a word and not as the absence of
    /// one, the word of spaces, the word of markup, the long name and the nothing that is no word at all - which is
    /// what the record holds although no reader of a file will ever hand it. Whether a file of <c>runtime/</c> is a
    /// plain name is the reader's question and is pinned with the reader; the record refuses nothing.</summary>
    [Fact]
    public void TheTwoFilesAreKeptAsWrittenAndNothingIsRefused()
    {
        Assert.Equal(string.Empty, new RuntimeDatabaseFiles(string.Empty, string.Empty).Svg);
        Assert.Equal(string.Empty, new RuntimeDatabaseFiles(string.Empty, string.Empty).Manifest);
        Assert.Equal("  ", new RuntimeDatabaseFiles("  ", " ").Svg);
        Assert.Equal(" ", new RuntimeDatabaseFiles("  ", " ").Manifest);
        Assert.Equal("<b>db</b>.svg", new RuntimeDatabaseFiles("<b>db</b>.svg", Manifest).Svg);
        Assert.Equal("../db.svg", new RuntimeDatabaseFiles("../db.svg", Manifest).Svg);

        var longName = new string('d', 300) + ".svg";

        Assert.Equal(longName, new RuntimeDatabaseFiles(longName, longName).Svg);

        var nothing = new RuntimeDatabaseFiles(null!, null!);

        Assert.Null(nothing.Svg);
        Assert.Null(nothing.Manifest);
        Assert.Equal(nothing, new RuntimeDatabaseFiles(null!, null!));
        Assert.Equal(nothing.GetHashCode(), new RuntimeDatabaseFiles(null!, null!).GetHashCode());
        Assert.False(nothing.Equals(Files));
    }

    /// <summary>The pair is both its words together and neither of them alone: a pair of one word changed is another
    /// pair, the empty word is not the nothing of it, and the difference is weighed ordinally whatever the reader of
    /// the page speaks - so an entry's database view is never the view of the environment whose name reads alike in
    /// another alphabet.</summary>
    [Fact]
    public void APairOfFilesIsItsTwoWordsTogetherAndNeitherAlone()
    {
        var files = Files;

        Assert.True(files.Equals(Files));
        Assert.True(files == Files);
        Assert.False(files != Files);
        Assert.Equal(files.GetHashCode(), Files.GetHashCode());

        Assert.NotEqual(files, files with { Svg = string.Empty });
        Assert.NotEqual(files, files with { Manifest = string.Empty });
        Assert.NotEqual(files, files with { Svg = null! });
        Assert.NotEqual(files, files with { Manifest = null! });
        Assert.NotEqual(files, files with { Svg = Svg.ToUpperInvariant() });
        Assert.NotEqual(files, files with { Manifest = "uat.database.json " });

        // The empty word and no word are two pairs, part by part.
        Assert.NotEqual(new RuntimeDatabaseFiles(string.Empty, Manifest), new RuntimeDatabaseFiles(null!, Manifest));
        Assert.NotEqual(new RuntimeDatabaseFiles(Svg, string.Empty), new RuntimeDatabaseFiles(Svg, null!));

        ViewerCulture.German(
            () =>
            {
                Assert.Equal(files, Files);
                Assert.NotEqual(files, files with { Svg = "uat.database.svg\u0301" });
                Assert.NotEqual(new RuntimeDatabaseFiles("ist", Manifest), new RuntimeDatabaseFiles("ıst", Manifest));
            });
    }

    /// <summary>A pair copied changes only the word it names and shares the other: the copy of nothing named is the
    /// same pair in another object, which is how a page hands the two files of a view on without owning them.</summary>
    [Fact]
    public void WithChangesOneFileAndSharesTheOther()
    {
        var files = Files;
        var copy = files with { Svg = "prod.database.svg" };

        Assert.Equal("prod.database.svg", copy.Svg);
        Assert.Equal(Manifest, copy.Manifest);
        Assert.Equal(Svg, files.Svg);

        var same = files with { };

        Assert.Equal(files, same);
        Assert.NotSame(files, same);
        Assert.Equal(files.GetHashCode(), same.GetHashCode());
    }

    /// <summary>The two words fall out in the order they went in - the diagram first, the manifest after - by the
    /// deconstruction the page may write as an assignment, both at once and one of the two alone.</summary>
    [Fact]
    public void TheTwoFilesFallOutInTheOrderTheyWentIn()
    {
        var (svg, manifest) = Files;

        Assert.Equal(Svg, svg);
        Assert.Equal(Manifest, manifest);

        var nothing = NoPair;
        nothing.Deconstruct(out var nothingSvg, out var nothingManifest);

        Assert.Null(nothingSvg);
        Assert.Null(nothingManifest);

        // One word asked out of the two, the other left where it stands.
        var (only, _) = Files;

        Assert.Equal(Svg, only);
    }

    /// <summary>The words a pair says of itself: its two parts in the order they are declared, each named and standing
    /// blank where there is nothing of it, the words neither quoted nor escaped nor trimmed. No part of a pair is a
    /// number a reader spells, so a reader of German is told the very line the invariant reader is told.</summary>
    [Fact]
    public void APairSaysItsTwoFilesInTheirOrder()
    {
        Assert.Equal(
            "RuntimeDatabaseFiles { Svg = uat.database.svg, Manifest = uat.database.json }",
            Files.ToString());
        Assert.Equal(
            "RuntimeDatabaseFiles { Svg = , Manifest =  }",
            new RuntimeDatabaseFiles(string.Empty, string.Empty).ToString());
        Assert.Equal(
            "RuntimeDatabaseFiles { Svg = , Manifest =  }",
            NoPair.ToString());
        Assert.Equal(
            "RuntimeDatabaseFiles { Svg =  db , Manifest = uat.database.json }",
            new RuntimeDatabaseFiles(" db ", Manifest).ToString());

        ViewerCulture.German(() => Assert.Equal(
            "RuntimeDatabaseFiles { Svg = uat.database.svg, Manifest = uat.database.json }",
            Files.ToString()));
    }

    /// <summary>The hash of a pair stands from call to call, is one hash for every pair of the two same words
    /// whichever of the three ways of making one reached it, and follows the two words: eight pairs - the whole one,
    /// the copy that names nothing, the pair of one word blanked and the pair of the same word nothing, each of them
    /// twice over, the swapped two and the two of words nobody gave - are seven in a set, the copy standing with the
    /// pair it came from.</summary>
    [Fact]
    public void TheHashOfAPairStandsAndFollowsItsTwoWords()
    {
        var files = Files;
        var hash = files.GetHashCode();

        Assert.Equal(hash, files.GetHashCode());
        Assert.Equal(hash, ((object)files).GetHashCode());
        Assert.Equal(hash, (files with { }).GetHashCode());
        Assert.Equal(hash, new RuntimeDatabaseFiles(Svg, Manifest).GetHashCode());

        RuntimeDatabaseFiles[] variants =
        [
            files,
            files with { },
            files with { Svg = string.Empty },
            files with { Svg = null! },
            files with { Manifest = string.Empty },
            files with { Manifest = null! },
            new RuntimeDatabaseFiles(Manifest, Svg),
            new RuntimeDatabaseFiles("db.svg", "db.json"),
        ];

        Assert.Equal(7, new HashSet<RuntimeDatabaseFiles>(variants).Count);
        Assert.Equal(7, variants.Distinct().Count());
    }

    // --------------------------------------------------------------------------------------- RuntimeIndex, the two parts

    /// <summary>The constructor's two parts, position by position, come back as the property of the same name: the
    /// environment list held as the very list it was handed, and the version standing as nothing when the caller does
    /// not name it - the index of a deployment that recorded no PlantUML version, which is not the index that recorded
    /// the empty word for it.</summary>
    [Fact]
    public void TheIndexHandsEachPartToThePropertyOfItsName()
    {
        var index = Full;

        Assert.Same(Environments, index.Environments);
        Assert.Equal(PlantUml, index.PlantUml);
        Assert.Equal(2, index.Environments.Count);
        Assert.Same(Environments[0], index.Environments[0]);
        Assert.Same(Environments[1], index.Environments[1]);

        var noVersion = NoVersion;

        Assert.Null(noVersion.PlantUml);
        Assert.Same(Environments, noVersion.Environments);
        Assert.Equal(noVersion, new RuntimeIndex(Environments, null));

        // The nothing of a version is not its empty word: a file that named no PlantUML and one that named nothing.
        Assert.NotEqual(noVersion, index with { PlantUml = string.Empty });
        Assert.Equal(string.Empty, (index with { PlantUml = string.Empty }).PlantUml);
    }

    /// <summary>The index holds its list and copies nothing out of it, and never opens it: the environment read back is
    /// the very object handed over, a list grown after the index was made is a list the index now stands on while the
    /// index itself stays one index in equality and in hash, and a second list of the same environments is a second
    /// index - which is why two reads of one file are two indexes. That the version is kept as written is the test
    /// below; what stands here is that the record asks nothing of the environments it holds, not even whether the
    /// entries in it are entries.</summary>
    [Fact]
    public void TheIndexHoldsItsListAndOpensNothingOfIt()
    {
        var environments = new List<RuntimeIndexEntry> { new("tdd", "tdd.json", "tdd.svg") };
        var theEnvironment = environments[0];
        var index = new RuntimeIndex(environments, PlantUml);
        var askedBefore = index.GetHashCode();
        var alsoIndex = new RuntimeIndex(environments, PlantUml);

        Assert.Same(environments, index.Environments);
        Assert.Same(theEnvironment, Assert.Single(index.Environments));
        Assert.Same(theEnvironment, (index with { }).Environments[0]);
        Assert.Same(environments, (index with { PlantUml = null }).Environments);

        environments.Add(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"));

        Assert.Equal(2, index.Environments.Count);
        Assert.Equal("uat", index.Environments[1].Name);
        Assert.True(index == alsoIndex);
        Assert.Equal(index, alsoIndex);
        Assert.Equal(askedBefore, index.GetHashCode());
        Assert.Single(new HashSet<RuntimeIndex> { index, alsoIndex });

        Assert.NotEqual(index, new RuntimeIndex([.. environments], PlantUml));

        // A list of entries that are no entry is a list, and the record neither reads nor refuses it - and weighs the
        // hold, so two arrays of the one nothing are two indexes while one array handed twice is one index.
        var listless = new RuntimeIndex([null!], PlantUml);
        var alsoListless = new RuntimeIndex([null!], PlantUml);

        Assert.Null(Assert.Single(listless.Environments));
        Assert.NotNull(listless.ToString());
        Assert.NotEqual(listless, alsoListless);
        Assert.Equal(listless, new RuntimeIndex(listless.Environments, PlantUml));
        Assert.Equal(listless.GetHashCode(), new RuntimeIndex(listless.Environments, PlantUml).GetHashCode());
    }

    /// <summary>The version is kept exactly as it was written: no trim, no lower-case, no fall-back from the empty
    /// word to the nothing of it, and nothing parsed out of the number-dots-number the file usually says - the record
    /// holds a word. That a file's blank or whitespace-only version arrives as nothing, and its padded one trimmed,
    /// is the reader's and is pinned with the reader.</summary>
    [Theory]
    [InlineData("1.2026.8")]
    [InlineData(" 1.2026.8 ")]
    [InlineData("1.2026,8")]
    [InlineData("1.2026.8-beta")]
    [InlineData("1")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("V1.2026.8")]
    [InlineData("plantuml 1.2026.4beta")]
    [InlineData("démo 日本")]
    public void TheVersionIsKeptExactlyAsGiven(string plantUml)
    {
        var index = NoVersion with { PlantUml = plantUml };

        Assert.Equal(plantUml, index.PlantUml);
        Assert.Equal(plantUml.Length, index.PlantUml!.Length);

        // The record trims nothing, which is the whole of the difference between a word and the same word with its
        // padding: two indexes whenever the two words differ, and the padding standing in the record's line too.
        Assert.Equal(plantUml.Trim() == plantUml, index == NoVersion with { PlantUml = plantUml.Trim() });
        Assert.Contains($"PlantUml = {plantUml} }}", index.ToString(), StringComparison.Ordinal);
        Assert.Equal(index, NoVersion with { PlantUml = plantUml });
        Assert.Equal(index.GetHashCode(), (NoVersion with { PlantUml = plantUml }).GetHashCode());
    }

    /// <summary>The three states a version is in: the word the file said, the empty word and the nothing of a file
    /// that named no version. They are three indexes, and the page tells the last two apart - the empty word being a
    /// version it has something to say, the nothing of it being no version at all to say.</summary>
    [Fact]
    public void TheEmptyVersionIsAnotherThanNoVersionAtAll()
    {
        var said = Full;
        var blank = said with { PlantUml = string.Empty };
        var nothing = said with { PlantUml = null };

        Assert.Equal(PlantUml, said.PlantUml);
        Assert.Equal(string.Empty, blank.PlantUml);
        Assert.Null(nothing.PlantUml);

        Assert.False(said.Equals(blank));
        Assert.False(blank.Equals(nothing));
        Assert.False(said.Equals(nothing));
        Assert.Equal(3, new HashSet<RuntimeIndex> { said, blank, nothing }.Count);

        // The version a caller leaves out is the version a copy blanks to nothing, and neither is an index of no
        // environments.
        Assert.Equal(nothing, NoVersion);
        Assert.NotEqual(nothing, NoParts);
    }

    /// <summary>An index of no environments is an index of an environment list, and an index handed no list at all is
    /// another index: the file that says <c>"environments": []</c> - which the reader accepts and the page shows as no
    /// diagram to draw - being one index, and the record of nothing being another. The line tells them apart by
    /// nothing: an empty array is said in the very words of a full one, the record naming its hold and never walking
    /// it, and only the nothing of a list stands blank, which is why the two are told apart by asking the part.</summary>
    [Fact]
    public void TheIndexOfNoEnvironmentsIsAnotherThanTheIndexOfNoParts()
    {
        var drawn = new RuntimeIndex(NoEnvironments, PlantUml);

        Assert.Empty(drawn.Environments);
        Assert.Same(NoEnvironments, drawn.Environments);
        Assert.Equal(PlantUml, drawn.PlantUml);

        Assert.NotEqual(drawn, NoParts);
        Assert.False(drawn.Equals(NoParts));
        Assert.False(NoParts.Equals(drawn));
        Assert.True(drawn != NoParts);
        Assert.Equal(2, new HashSet<RuntimeIndex> { drawn, NoParts }.Count);

        Assert.Equal(IndexWords, drawn.ToString());
        Assert.Equal(IndexBlanks, NoParts.ToString());

        // The nothing reached by the copy is the nothing reached by the constructor.
        Assert.Equal(NoParts, drawn with { Environments = null!, PlantUml = null });
        Assert.Equal(NoParts.GetHashCode(), (drawn with { Environments = null!, PlantUml = null }).GetHashCode());

        // The empty array the record stands on is one hold, so the copy that empties the list is the same index; a
        // second empty list written out of the first is another hold, which the record - weighing the hold and never
        // walking it - tells from the one it holds.
        Assert.Equal(drawn, drawn with { Environments = [] });
        Assert.Same(NoEnvironments, (drawn with { Environments = [] }).Environments);
        Assert.NotEqual(drawn, drawn with { Environments = [.. NoEnvironments] });
        Assert.Equal(2, new HashSet<RuntimeIndex> { drawn, drawn with { Environments = [.. NoEnvironments] } }.Count);
    }

    /// <summary>An index is one with itself and with no other record of the runtime files: never with the list of
    /// environments it stands on, nor with one of them, nor with the pair of files an environment names, nor with a
    /// manifest, a box, a kind or the version's own word. That is what keeps an index and an entry of it out of one
    /// another in a page that reads both.</summary>
    [Fact]
    public void AnIndexIsOneWithItselfAndWithNoOtherRecordOfTheRuntimeFiles()
    {
        var index = Full;
        var itself = index;
        RuntimeIndex? gone = null;
        RuntimeIndex? alsoGone = null;

        Assert.True(index.Equals(index));
        Assert.True(index.Equals((object)index));
        Assert.True(index == itself);
        Assert.False(index != itself);

        Assert.True(gone == alsoGone);
        Assert.False(gone != alsoGone);
        Assert.Null(default(RuntimeIndex));
        Assert.False(index == gone);
        Assert.False(gone == index);
        Assert.True(index != gone);
        Assert.False(index.Equals(gone));
        Assert.False(index.Equals((object?)null));
        Assert.False(index.Equals((object)"1.2026.8"));

        Assert.False(index.Equals(Environments));
        Assert.False(index.Equals(Environments[0]));
        Assert.False(index.Equals(Files));
        Assert.False(index.Equals(Environments[1].Database));
        Assert.False(index.Equals(PlantUml));
        Assert.False(index.Equals(new RuntimeManifest("uat", [], [], [])));
        Assert.False(index.Equals(new RuntimeNode("web_primary", RuntimeNodeKind.WebApp, "app-uat")));
        Assert.False(index.Equals(new RuntimeRegion("region_primary", "southcentralus", ["primary"])));
        Assert.False(index.Equals(new RuntimeEdge("browser-to-web_primary", "browser", "web_primary", RuntimeEdgeKind.Public)));
        Assert.False(index.Equals(new RuntimeFrame("sub", "subscription", "demo")));
        Assert.False(index.Equals(new RuntimeUnplaced("stray")));
        Assert.False(index.Equals(RuntimeNodeKind.WebApp));
        Assert.False(index.Equals(RuntimeEdgeKind.Public));

        // The index of one environment is no environment either.
        Assert.False(Files.Equals(Environments[1]));
    }

    /// <summary>The index's version, weighed ordinally whatever the reader speaks: two indexes of one version are one
    /// index to a German reader as to an invariant one, and a version that only looks alike in another alphabet, in
    /// another case or with one space more is another index - which is what keeps an index of one file from matching
    /// an index of another in a dictionary keyed by the record.</summary>
    [Fact]
    public void TheVersionIsComparedOrdinallyWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(
            () =>
            {
                Assert.Equal(Full, new RuntimeIndex(Environments, PlantUml));
                Assert.NotEqual(Full, Full with { PlantUml = "1.2026.8 " });
                Assert.NotEqual(Full, Full with { PlantUml = "1.2026.8\n" });
                Assert.NotEqual(Full, Full with { PlantUml = "1.2026.B" });
                Assert.NotEqual(new RuntimeIndex(Environments, "ist"), new RuntimeIndex(Environments, "ıst"));
            });
    }

    /// <summary>The two parts fall out in the order they went in - the environments, then the version - by the
    /// deconstruction the page may write as an assignment, by the call of the same name, and with the parts asked back
    /// out rebuilding the very index they came from. The nothing of both parts falls out as two nothings, and the
    /// empty list falls out as the hold it stands on.</summary>
    [Fact]
    public void TheTwoPartsFallOutInTheOrderTheyWentIn()
    {
        var (environments, plantUml) = Full;

        Assert.Same(Environments, environments);
        Assert.Equal(PlantUml, plantUml);
        Assert.Equal(Full, new RuntimeIndex(environments, plantUml));

        var index = Full;
        index.Deconstruct(out var held, out var version);

        Assert.Same(Environments, held);
        Assert.Equal(PlantUml, version);

        var (nothingEnvironments, nothingVersion) = NoParts;

        Assert.Null(nothingEnvironments);
        Assert.Null(nothingVersion);

        var (drawn, _) = new RuntimeIndex(NoEnvironments);

        Assert.Same(NoEnvironments, drawn);

        // The version left out of the construction is the version that falls out.
        var (_, theVersion) = NoVersion;

        Assert.Null(theVersion);
    }

    /// <summary>The words an index says of itself: its two parts in the order they are declared, the list named and
    /// never walked - so the index of two environments and the index of none are said in one and the same words - and
    /// the version said exactly as it stands, quoted by nothing and trimmed by nothing. The page is given its words
    /// elsewhere, and they are pinned with the builder.</summary>
    [Fact]
    public void AnIndexSaysItsTwoPartsInTheirOrder()
    {
        RuntimeIndexEntry[] again = [.. Environments];
        var environments = new List<RuntimeIndexEntry>(Environments);

        Assert.Equal(IndexWords, Full.ToString());
        Assert.Equal(
            "RuntimeIndex { Environments = Dashboard.Runtime.RuntimeIndexEntry[], PlantUml =  }",
            NoVersion.ToString());
        Assert.Equal(IndexBlanks, NoParts.ToString());
        Assert.Equal(
            "RuntimeIndex { Environments = , PlantUml =   plantuml   }",
            new RuntimeIndex(null!, "  plantuml  ").ToString());

        // The line names the hold and asks nothing of it, so an empty array of environments is said in the very words
        // of a full one, and so is a second array of the same environments - while a list says the list it stands on.
        Assert.Equal(IndexWords, new RuntimeIndex(NoEnvironments, PlantUml).ToString());
        Assert.Equal(IndexWords, new RuntimeIndex(again, PlantUml).ToString());
        Assert.Equal(
            "RuntimeIndex { Environments = System.Collections.Generic.List`1[Dashboard.Runtime.RuntimeIndexEntry], "
            + "PlantUml = 1.2026.8 }",
            new RuntimeIndex(environments, PlantUml).ToString());

        ViewerCulture.German(
            () =>
            {
                Assert.Equal(IndexWords, Full.ToString());
                Assert.Equal(IndexBlanks, NoParts.ToString());
            });
    }

    /// <summary>The hash of an index stands from call to call and follows its two parts: eight indexes - the whole
    /// one, the copy that names nothing, the version changed, the version blanked, the version nothing, the list
    /// emptied, the list a second hold of the same environments and the list nothing - are seven in a set,
    /// the copy standing with the index it came from.</summary>
    [Fact]
    public void TheHashOfAnIndexStandsAndFollowsItsTwoParts()
    {
        foreach (var index in new[] { Full, NoVersion, NoParts })
        {
            var hash = index.GetHashCode();

            Assert.Equal(hash, index.GetHashCode());
            Assert.Equal(hash, (index with { }).GetHashCode());
            Assert.Equal(hash, ((object)index).GetHashCode());
            Assert.Equal(hash, new RuntimeIndex(index.Environments, index.PlantUml).GetHashCode());
        }

        var full = Full;
        RuntimeIndex[] variants =
        [
            full,
            full with { },
            full with { PlantUml = "1.2026.9" },
            full with { PlantUml = string.Empty },
            full with { PlantUml = null },
            full with { Environments = NoEnvironments },
            full with { Environments = [.. Environments] },
            full with { Environments = null! },
        ];

        Assert.Equal(7, variants.Distinct().Count());
        Assert.Equal(7, new HashSet<RuntimeIndex>(variants).Count);
    }

    /// <summary>Equality as a caller asks it, at either door and through the two operators the record gives: the
    /// constructor, the copy that names nothing and the parts read back out all make one index of one set of parts,
    /// the record is symmetric and transitive across them, and <see cref="EqualityComparer{T}" /> - the door a generic
    /// dictionary knocks at - says the same as the record itself, nothings included.</summary>
    [Fact]
    public void AnIndexIsEqualWhicheverDoorTheCallerAsksThrough()
    {
        var one = Full;
        var twin = new RuntimeIndex(Environments, PlantUml);
        var other = Full with { PlantUml = "1.2026.9" };
        var copy = one with { };
        var (environments, plantUml) = one;
        var rebuilt = new RuntimeIndex(environments, plantUml);

        var comparer = EqualityComparer<RuntimeIndex>.Default;

        Assert.True(comparer.Equals(one, twin));
        Assert.True(comparer.Equals(one, copy));
        Assert.True(comparer.Equals(copy, rebuilt));
        Assert.True(comparer.Equals(one, rebuilt));
        Assert.False(comparer.Equals(one, other));
        Assert.False(comparer.Equals(one, null));
        Assert.True(comparer.Equals(null, null));
        Assert.Equal(one.GetHashCode(), comparer.GetHashCode(twin));
        Assert.True(one.Equals(copy) && copy.Equals(one));

        // And the record's own door says it the same way, from either side of the operator.
        Assert.True(one == twin);
        Assert.False(one != twin);
        Assert.True(one != other);
        Assert.False(one == other);
    }

    /// <summary>An index finds its entry in a dictionary keyed by the record - by the constructor's parts, by the copy
    /// that names nothing and by the parts asked back out - and misses it for one word of the version changed, for one
    /// space of it, and for the very same environments in a second list.</summary>
    [Fact]
    public void AnIndexFindsItsEntryWhenAskedByAnEqualIndex()
    {
        var svgByIndex = new Dictionary<RuntimeIndex, string> { [Full] = "index.svg", [NoVersion] = "index-old.svg" };

        Assert.Equal("index.svg", svgByIndex[new RuntimeIndex(Environments, PlantUml)]);
        Assert.Equal("index.svg", svgByIndex[Full with { }]);
        Assert.Equal("index-old.svg", svgByIndex[Full with { PlantUml = null }]);
        Assert.False(svgByIndex.ContainsKey(Full with { PlantUml = "1.2026.8 " }));
        Assert.False(svgByIndex.ContainsKey(Full with { Environments = [.. Environments] }));
        Assert.False(svgByIndex.ContainsKey(NoParts));
    }

    // --------------------------------------------------------------------- the two records' own surfaces

    /// <summary>The whole public surface of <see cref="RuntimeIndex" />: its two parts as the record gives them - each
    /// read at will and written only as the index is made or copied, the list annotated non-nullable and the version
    /// nullable at exactly the part a caller may leave out - and the members every record is asked, with nothing else
    /// to call, nothing to subscribe to, no static part, no field and no base but <see cref="object" />. A part added
    /// to the record has to be written here as well, which is the point: the page cannot be shown an index part nobody
    /// decided what means.</summary>
    [Fact]
    public void TheIndexHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(RuntimeIndex);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Environments", "PlantUml"], [.. properties.Select(property => property.Name)]);
        Assert.Equal(
            [typeof(IReadOnlyList<RuntimeIndexEntry>), typeof(string)],
            [.. properties.Select(property => property.PropertyType)]);

        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, true],
            [.. properties.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);
        Assert.Equal(
            [false, true],
            [.. properties.Select(property => nullability.Create(property).WriteState == NullabilityState.Nullable)]);
        Assert.All(
            properties,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.True(property.CanWrite);
                Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
            });

        Assert.Equal(
            ["Deconstruct", "Environments", "Equals", "GetHashCode", "PlantUml", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface(type));

        var parameters = Assert.Single(type.GetConstructors()).GetParameters();

        Assert.Equal(["Environments", "PlantUml"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.Null(parameters[1].DefaultValue);
        Assert.Equal(
            [false, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        Assert.True(type.IsSealed);
        Assert.False(type.IsAbstract);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<RuntimeIndex>)], type.GetInterfaces());
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetEvents());
    }

    /// <summary>The whole public surface of <see cref="RuntimeDatabaseFiles" />: its two words, both of them parts a
    /// caller must name and both of them annotated as something to read, set only as the pair is made or copied - and
    /// the members every record is asked, with nothing else to call and no part of the runtime file to compute.</summary>
    [Fact]
    public void ThePairOfFilesHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(RuntimeDatabaseFiles);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Svg", "Manifest"], [.. properties.Select(property => property.Name)]);
        Assert.Equal([typeof(string), typeof(string)], [.. properties.Select(property => property.PropertyType)]);

        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false],
            [.. properties.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);
        Assert.All(
            properties,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.True(property.CanWrite);
                Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
            });

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Manifest", "Svg", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface(type));

        var parameters = Assert.Single(type.GetConstructors()).GetParameters();

        Assert.Equal(["Svg", "Manifest"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.Equal(
            [false, false],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        Assert.True(type.IsSealed);
        Assert.False(type.IsAbstract);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<RuntimeDatabaseFiles>)], type.GetInterfaces());
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetEvents());
    }

    // ------------------------------------------------------------------------------------------ the machinery

    /// <summary>The public members a type declares itself: its properties and its methods that are not a property's
    /// get or set, the two operators kept although the compiler names them specially since they are the <c>==</c> and
    /// the <c>!=</c> a caller writes, the record's compiler-named clone and its print walk past.</summary>
    private static List<string> PublicSurface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
