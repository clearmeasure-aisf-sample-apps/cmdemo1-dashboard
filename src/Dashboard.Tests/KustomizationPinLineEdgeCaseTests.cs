namespace Dashboard.Tests;

/// <summary>
/// The grammar of one <c>newTag</c> line of a Kustomize file, pinned directly on <see cref="KustomizationPinLine" />
/// (the tests compile its source, as they do <c>JsonRead</c>), and once through
/// <see cref="PinnedVersions.ParseKustomization" /> to show the reader asks this very grammar.
/// </summary>
public class KustomizationPinLineEdgeCaseTests
{
    [Fact]
    public void TheKeyIsNewTag() => Assert.Equal("newTag", KustomizationPinLine.Key);

    [Theory]
    [InlineData("newTag: 2.4.15", "2.4.15")]
    [InlineData("    newTag: 2.4.15   ", "2.4.15")]
    [InlineData("- newTag: 2.4.15", "2.4.15")]
    [InlineData("  -   newTag: 2.4.15", "2.4.15")]
    [InlineData("-\tnewTag: 2.4.15", "2.4.15")]
    [InlineData("newTag : 2.4.15", "2.4.15")]
    [InlineData("newTag:\t2.4.15", "2.4.15")]
    [InlineData("newTag: 2.4.15\r", "2.4.15")]
    public void AnUnquotedValueIsTheRestOfTheLine(string line, string expected) =>
        Assert.Equal(expected, KustomizationPinLine.Of(line));

    [Theory]
    [InlineData("newTag: \"2.4.15\"", "2.4.15")]
    [InlineData("newTag: '2.4.15'", "2.4.15")]
    [InlineData("newTag: \"2.4.15\" # pinned", "2.4.15")]
    [InlineData("newTag: '2.4#15'", "2.4#15")]
    [InlineData("newTag: \"2.4 # 15\"", "2.4 # 15")]
    [InlineData("newTag: \"2.4.15", "2.4.15")]
    [InlineData("newTag: '2.4.15\"", "2.4.15\"")]
    [InlineData("newTag: \"\"", "")]
    [InlineData("newTag: ''", "")]
    public void AQuotedValueEndsAtItsClosingQuoteOrTheLine(string line, string expected) =>
        Assert.Equal(expected, KustomizationPinLine.Of(line));

    [Theory]
    [InlineData("newTag: 2.4.15 # pinned by the deploy", "2.4.15")]
    [InlineData("newTag: 2.4.15\t# pinned", "2.4.15")]
    [InlineData("newTag: 2.4.15#not-a-comment", "2.4.15#not-a-comment")]
    [InlineData("newTag: 2.4.15 #", "2.4.15")]
    public void AnUnquotedCommentStartsOnlyAtAHashAfterWhitespace(string line, string expected) =>
        Assert.Equal(expected, KustomizationPinLine.Of(line));

    [Theory]
    [InlineData("newTag:")]
    [InlineData("newTag:   ")]
    [InlineData("- newTag:")]
    [InlineData("newTag: # none yet")]
    [InlineData("newTag: #")]
    public void AnEntryWithoutAValueIsEmpty(string line) =>
        Assert.Equal(string.Empty, KustomizationPinLine.Of(line));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-")]
    [InlineData("- ")]
    [InlineData("# newTag: 2.4.15")]
    [InlineData("name: dashboard")]
    [InlineData("newName: dashboard")]
    [InlineData("newtag: 2.4.15")]
    [InlineData("NewTag: 2.4.15")]
    [InlineData("newTag")]
    [InlineData("newTag 2.4.15")]
    [InlineData("newTag:2.4.15")]
    [InlineData("newTags: 2.4.15")]
    [InlineData("-newTag: 2.4.15")]
    [InlineData("- - newTag: 2.4.15")]
    [InlineData("images: [ { name: dashboard, newTag: 2.4.15 } ]")]
    public void AnyOtherLineIsNoEntry(string line) =>
        Assert.Null(KustomizationPinLine.Of(line));

    [Fact]
    public void TheReaderTakesTheFirstLineTheGrammarAccepts()
    {
        const string yaml = """
            images:
              - name: dashboard
                # newTag: 0.0.1
                newTag:2.0.0
                newTag: '2.4.15' # pinned
                newTag: 9.9.9
            """;

        var expected = yaml.Split('\n').Select(KustomizationPinLine.Of).First(value => value is not null);
        Assert.Equal("2.4.15", expected);

        var pinned = PinnedVersions.ParseKustomization(yaml, "dashboard");
        Assert.Equal(PinnedVersionsState.Read, pinned.State);
        Assert.Equal(expected, pinned.Of("dashboard"));
    }
}
