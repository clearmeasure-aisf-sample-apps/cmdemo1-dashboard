using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The note words a status comment and a blocker note are both described by, pinned in one place: the consts
/// <c>WorkNote.TextWord</c>, <c>WorkNote.UrlWord</c> and <c>WorkNote.AtWord</c>. <c>WorkNote.Read</c> and
/// <see cref="WorkItem" />'s <c>latestStatusComment</c> reader both read through them, so a file written in those
/// words reads back the same note whichever reader met it. The consts are internal and the app declares no
/// <c>InternalsVisibleTo</c>, so they are pinned by reflection, the way <see cref="WorkBoardWordsTests" /> pins
/// the board words.
/// </summary>
public class WorkNoteWordsTests
{
    private static readonly string TextWord = Word("TextWord");
    private static readonly string UrlWord = Word("UrlWord");
    private static readonly string AtWord = Word("AtWord");

    /// <summary>The three words are what the work file has always said them: text, url, at - three different words.</summary>
    [Fact]
    public void TheThreeWordsAreTheWorkFileNames()
    {
        Assert.Equal("text", TextWord);
        Assert.Equal("url", UrlWord);
        Assert.Equal("at", AtWord);

        Assert.Equal(3, new[] { TextWord, UrlWord, AtWord }
            .Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>A <c>latestStatusComment</c> note and a <c>blocker</c> note written in the three words come back
    /// with the same text, page and time: one note shape, read alike by both readers.</summary>
    [Fact]
    public void AStatusCommentAndABlockerWrittenInTheWordsReadAlike()
    {
        var status = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 490, "latestStatusComment": {
                "{{TextWord}}": "Waiting on the review",
                "{{UrlWord}}": "https://github.com/acme/board/issues/490",
                "{{AtWord}}": "2026-11-10T00:00:00Z" } } ] }
            """)!;
        var blocker = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 490, "blocker": {
                "{{TextWord}}": "Waiting on the review",
                "{{UrlWord}}": "https://github.com/acme/board/issues/490",
                "{{AtWord}}": "2026-11-10T00:00:00Z" } } ] }
            """)!;

        var statusNote = status!.Items.Single().Blocker;
        var blockerNote = blocker!.Items.Single().Blocker;
        Assert.NotNull(statusNote);
        Assert.NotNull(blockerNote);

        Assert.Equal(statusNote.Text, blockerNote.Text);
        Assert.Equal(statusNote.Url, blockerNote.Url);
        Assert.Equal(statusNote.At, blockerNote.At);
        Assert.Equal("Waiting on the review", statusNote.Text);
        Assert.Equal(new Uri("https://github.com/acme/board/issues/490"), statusNote.Url);
        Assert.Equal(new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.Zero), statusNote.At);
    }

    /// <summary>The internal const of that name on <see cref="WorkNote" />, read the way the other word tests read internal words.</summary>
    private static string Word(string name) =>
        (string)typeof(WorkNote).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
}
