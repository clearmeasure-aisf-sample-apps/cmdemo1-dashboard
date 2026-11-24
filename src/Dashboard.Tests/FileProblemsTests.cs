using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The three sentences every reader of a file the dashboard does not own says when what it was pointed at is no file,
/// now held once in <see cref="FileProblems" /> instead of typed out by each reader (#1330).
/// <see cref="ClusterStatus.Parse" /> and <see cref="AksService.Parse" /> handed back
/// <c>"the file is empty"</c>, <c>"the file does not contain a JSON object"</c> and <c>"the file is not valid JSON"</c>
/// word for word, and so did <see cref="PinnedVersions.Parse" /> and <see cref="PinnedVersions.ParseKustomization" /> -
/// nine copies of three sentences, each able to be reworded on its own while the other two stayed behind.
/// <para>
/// The sentences are copy a viewer reads: a <see cref="Parsed{T}.Problem" /> becomes a
/// <see cref="SourceReading{T}.Detail" /> and the line under a card, and a <see cref="PinnedVersions.Detail" /> the
/// version line. So the tests below are of two kinds. The first pins the words themselves - a holder nobody pins can be
/// reworded as quietly as nine readers could before - and then walks each refusal through each reader, so the holder is
/// named as the answer and not merely defined: an empty file, a file that is JSON of another shape, a file that is no
/// JSON at all, for the cluster's status, for Azure's facts, for <c>versions.json</c> and for a Kustomize pin.
/// </para>
/// <para>
/// The second kind is the guard the issue is really about. It reads the app's own source the way
/// <see cref="HealthHelperCoverageTests" /> and <see cref="ClusterHelperCoverageTests" /> read it: each of the three
/// sentences may be typed out in exactly one file of <c>src/Dashboard</c>, the holder, and every reader that refuses
/// must do it by the name. A rewording is then one edit that every test here follows, and a fourth reader inventing a
/// wording of its own is a failing test naming the file and the line. The page-facing expectations of
/// <c>ClusterReaderEdgeCaseTests</c> and <c>ClusterMonitorEdgeCaseTests</c> are held to the other end of the same
/// rule: they keep the words themselves, because they are the ones saying what the card shows.
/// </para>
/// <para>
/// The topology's reader and the runtime manifest's are left with the wordings they have (<c>"The file is empty."</c>,
/// <c>"The file must contain a JSON object."</c>, and a broken file that says which line and position it broke at):
/// they report a file the operator edits rather than a file a collector publishes, they carry several faults at once
/// instead of one refusal, and unifying them would change what the page shows - which is more than this issue asked
/// for.
/// </para>
/// </summary>
public class FileProblemsTests
{
    // --------------------------------------------------------------------------- the words themselves

    /// <summary>The three sentences, word for word, as the page has always shown them: the holder is a place to keep
    /// them, not a licence to reword them. Lower case and with no full stop, like every other refusal - each is the
    /// end of a sentence, which is how it reaches the card.</summary>
    [Fact]
    public void TheHolderSaysWhatTheReadersHaveAlwaysSaid()
    {
        Assert.Equal("the file is empty", FileProblems.Empty);
        Assert.Equal("the file does not contain a JSON object", FileProblems.NotAnObject);
        Assert.Equal("the file is not valid JSON", FileProblems.Broken);
    }

    /// <summary>Three refusals and not three ways of saying one thing: a viewer, and a test, can tell them apart. None
    /// of them is empty, begins with a capital or ends in punctuation, so the card can put one after its own words as
    /// the readers did before the holder existed.</summary>
    [Fact]
    public void TheThreeSentencesAreThreeAndEndASentence()
    {
        string[] sentences = [FileProblems.Empty, FileProblems.NotAnObject, FileProblems.Broken];

        Assert.Equal(3, sentences.Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            sentences,
            sentence =>
            {
                Assert.False(string.IsNullOrWhiteSpace(sentence));
                Assert.False(char.IsUpper(sentence[0]), $"\"{sentence}\" starts with a capital and cannot end a sentence");
                Assert.False(sentence.EndsWith('.'), $"\"{sentence}\" ends with a full stop the card would show twice");
            });
    }

    // --------------------------------------------------------------------------------- an empty file

    /// <summary>A file with nothing in it, whichever nothing: no content at all, the empty string, a blank, and the
    /// whitespace a collector that died mid-write leaves behind. All four readers refuse with the one sentence, and
    /// none of them hands back a value to go with the refusal.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t\r\n  ")]
    public void EveryReaderRefusesAnEmptyFileInTheSameWords(string? json)
    {
        Assert.Equal(FileProblems.Empty, ClusterStatus.Parse(json).Problem);
        Assert.Null(ClusterStatus.Parse(json).Value);

        Assert.Equal(FileProblems.Empty, AksService.Parse(json).Problem);
        Assert.Null(AksService.Parse(json).Value);

        AssertRefused(PinnedVersions.Parse(json));
        Assert.Equal(FileProblems.Empty, PinnedVersions.Parse(json).Detail);

        AssertRefused(PinnedVersions.ParseKustomization(json, "ui"));
        Assert.Equal(FileProblems.Empty, PinnedVersions.ParseKustomization(json, "ui").Detail);
    }

    // ------------------------------------------------------------------- a file that is JSON of the wrong shape

    /// <summary>A file that parses and is not an object - the array, the number, the text, the two bools and the JSON
    /// <c>null</c> - is no file, whatever it holds inside it. The status, Azure's facts and the map of pinned versions
    /// are all objects, so the shape is refused before its contents are looked at: the top of an array is not examined
    /// for a <c>nodes</c> any more than the top of a number is.</summary>
    [Theory]
    [InlineData("[]")]
    [InlineData("[ 1, 2, 3 ]")]
    [InlineData("[ { \"name\": \"node-1\" } ]")]
    [InlineData("3")]
    [InlineData("-1.5")]
    [InlineData("\"text\"")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    public void EveryReaderRefusesAFileThatIsNoJsonObject(string json)
    {
        Assert.Equal(FileProblems.NotAnObject, ClusterStatus.Parse(json).Problem);
        Assert.Null(ClusterStatus.Parse(json).Value);

        Assert.Equal(FileProblems.NotAnObject, AksService.Parse(json).Problem);
        Assert.Null(AksService.Parse(json).Value);

        AssertRefused(PinnedVersions.Parse(json));
        Assert.Equal(FileProblems.NotAnObject, PinnedVersions.Parse(json).Detail);
    }

    // ------------------------------------------------------------------------ a file that is no JSON at all

    /// <summary>A file that is no JSON: opened and never closed, closed without opening, a value that never came,
    /// words after the object ended, quotes of the kind JSON does not have, and text that was never meant to be a
    /// file. Every one of them is the third sentence - the reader does not report which byte broke, because what a
    /// viewer can do about a collector's half-written file is nothing.</summary>
    [Theory]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("[")]
    [InlineData("{ \"nodes\": []")]
    [InlineData("{ \"nodes\": [ ] } }")]
    [InlineData("{ \"a\": }")]
    [InlineData("{ \"generated\": }")]
    [InlineData("not json at all")]
    [InlineData("{\"a\": 1} trailing words")]
    [InlineData("{'name': 'ui'}")]
    [InlineData("07")]
    [InlineData("{,}")]
    public void EveryReaderRefusesAFileThatIsNoJsonInTheSameWords(string json)
    {
        Assert.Equal(FileProblems.Broken, ClusterStatus.Parse(json).Problem);
        Assert.Null(ClusterStatus.Parse(json).Value);

        Assert.Equal(FileProblems.Broken, AksService.Parse(json).Problem);
        Assert.Null(AksService.Parse(json).Value);

        AssertRefused(PinnedVersions.Parse(json));
        Assert.Equal(FileProblems.Broken, PinnedVersions.Parse(json).Detail);
    }

    // --------------------------------------------------------------------------- the refusal reaches the card

    /// <summary>The refusal a reader hands back is the line the card shows, unchanged: what the reader makes of a
    /// <see cref="Parsed{T}" /> that carries a <see cref="Parsed{T}.Problem" /> is a <see cref="SourceReading{T}" />
    /// whose <see cref="SourceReading{T}.Detail" /> is that very sentence, the three refusals quoted here by name so
    /// the card and the holder are pinned to each other rather than to a string that happens to match today.</summary>
    [Fact]
    public void TheRefusalOfAReaderIsTheDetailOfTheReading()
    {
        var empty = AksService.Parse("   ");
        var notAnObject = ClusterStatus.Parse("[]");
        var broken = ClusterStatus.Parse("{");

        Assert.Null(empty.Value);
        Assert.Null(notAnObject.Value);
        Assert.Null(broken.Value);

        Assert.Equal(
            new SourceReading<AksService>(SourceState.Malformed, Detail: FileProblems.Empty),
            new SourceReading<AksService>(SourceState.Malformed, empty.Value, empty.Problem));

        Assert.Equal(
            new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: FileProblems.NotAnObject),
            new SourceReading<ClusterStatus>(SourceState.Malformed, notAnObject.Value, notAnObject.Problem));

        Assert.Equal(
            new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: FileProblems.Broken),
            new SourceReading<ClusterStatus>(SourceState.Malformed, broken.Value, broken.Problem));
    }

    // -------------------------------------------------------------------------------- the holder is the only copy

    /// <summary>The rule the issue asked for, read from the app's own source: each of the three sentences is typed out
    /// in exactly one file of <c>src/Dashboard</c>, the holder. A reader that words a refusal its own way fails here
    /// with the file named, which is the drift - nine copies rewordable one at a time - that made three sentences a
    /// bug.</summary>
    [Fact]
    public void TheAppTypesEachRefusalInOneFileOnly()
    {
        var sources = AppSources();

        foreach (var sentence in new[] { FileProblems.Empty, FileProblems.NotAnObject, FileProblems.Broken })
        {
            var quoted = "\"" + sentence + "\"";
            List<string> typed = [.. sources.Where(source => source.Text.Contains(quoted, StringComparison.Ordinal)).Select(source => source.File)];

            Assert.Equal(new[] { Holder }, typed);
        }
    }

    /// <summary>And the readers that refuse do it by the name: each of the three files that turned its own sentences
    /// into copy quotes the holder for all three of them, so none keeps a private wording the guard above could miss -
    /// and all three of them are really in the walk, so neither loop passes on an empty one.</summary>
    [Fact]
    public void TheReadersRefuseByTheHoldersName()
    {
        List<string> files = [.. AppSources().Select(source => source.File).Where(file => Readers.Contains(file, StringComparer.Ordinal)).Distinct(StringComparer.Ordinal)];

        Assert.Equal(Readers, files);

        foreach (var (file, text) in AppSources().Where(source => Readers.Contains(source.File, StringComparer.Ordinal)))
        {
            Assert.Contains("FileProblems.Empty", text, StringComparison.Ordinal);
            Assert.Contains("FileProblems.NotAnObject", text, StringComparison.Ordinal);
            Assert.Contains("FileProblems.Broken", text, StringComparison.Ordinal);
        }
    }

    /// <summary>The page-facing expectations keep the words themselves: <c>ClusterReaderEdgeCaseTests</c> and
    /// <c>ClusterMonitorEdgeCaseTests</c> name what the card shows rather than quoting the holder, so a change to the
    /// wording is a change they see and not a change they follow. This pins that they still do.</summary>
    [Fact]
    public void ThePageFacingExpectationsStillNameTheWordsTheCardShows()
    {
        AssertSentencesNamed("ClusterReaderEdgeCaseTests.cs", FileProblems.Empty, FileProblems.NotAnObject);
        AssertSentencesNamed("ClusterMonitorEdgeCaseTests.cs", FileProblems.Empty, FileProblems.Broken);
        AssertSentencesNamed("SourceReadingEdgeCaseTests.cs", FileProblems.Broken);
    }

    // ------------------------------------------------------------------------------------------------ helpers

    /// <summary>The one file of the app allowed to type the three sentences out.</summary>
    private const string Holder = "Health/FileProblems.cs";

    /// <summary>The three files whose readers the issue found typing the sentences out, in the order the walk of
    /// <c>src/Dashboard</c> prints them.</summary>
    private static readonly string[] Readers = ["Cluster/AksService.cs", "Cluster/ClusterStatus.cs", "Health/PinnedVersions.cs"];

    /// <summary>A reading of a file the dashboard does not own that came back with a refusal and no value.</summary>
    private static void AssertRefused(PinnedVersions versions)
    {
        Assert.Equal(PinnedVersionsState.Unavailable, versions.State);
        Assert.Empty(versions.Versions);
        Assert.NotNull(versions.Detail);
    }

    /// <summary>That a test file of this project holds these sentences as words of its own.</summary>
    private static void AssertSentencesNamed(string name, params string[] sentences)
    {
        var text = File.ReadAllText(Path.Combine(TestsRoot(), name));

        Assert.All(sentences, sentence => Assert.Contains("\"" + sentence + "\"", text, StringComparison.Ordinal));
    }

    /// <summary>Every <c>.cs</c> file of the app, as the path the assertion prints relative to <c>src/Dashboard</c>
    /// with a forward slash, and its text. The build's copies under <c>bin</c> and <c>obj</c> hold the same source
    /// twice, so they are walked past, the way the other source guards walk past them.</summary>
    private static List<(string File, string Text)> AppSources()
    {
        var root = Path.GetFullPath(Path.Combine(TestsRoot(), "..", "Dashboard"));
        var sources = new List<(string, string)>();

        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);

            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            sources.Add((relative.Replace(Path.AltDirectorySeparatorChar, '/'), File.ReadAllText(path)));
        }

        return sources;
    }

    /// <summary>The folder of the tests, which this file stands in - the way the other guards find the folder they
    /// read.</summary>
    private static string TestsRoot([CallerFilePath] string here = "") => Path.GetDirectoryName(here)!;
}
