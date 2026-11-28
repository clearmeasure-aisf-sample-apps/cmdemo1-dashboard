namespace Dashboard.Tests;

/// <summary>
/// The note-kind words and their headings, now owned once on <see cref="WorkNote" /> (internal, so pinned here by
/// value): the page's heading from <see cref="WorkBotText.NoteHeading" /> and the fallback text a
/// <c>latestStatusComment</c> with only a kind gets from <see cref="WorkItemsReport.Parse" /> say the same words.
/// </summary>
public class WorkNoteKindWordsTests
{
    private static WorkNote? NoteOf(string item) =>
        WorkItemsReport.Parse("{ \"items\": [ " + item + " ] }")!.Items.Single().Blocker;

    [Theory]
    [InlineData("BLOCKER", "Blocker")]
    [InlineData("blocker", "Blocker")]
    [InlineData(null, "Status")]
    [InlineData("status", "Status")]
    [InlineData("question", "Status")]
    public void TheHeadingIsBlockerOnlyForTheBlockerWord(string? kind, string heading) =>
        Assert.Equal(heading, WorkBotText.NoteHeading(new WorkNote("Waiting", null, null, kind)));

    [Theory]
    [InlineData("Status", "Status")]
    [InlineData("STATUS", "Status")]
    [InlineData("Blocker", "Blocker")]
    [InlineData("question", "question")]
    public void AStatusCommentWithOnlyAKindIsTextedByItsHeading(string kind, string text)
    {
        var note = NoteOf($$"""{ "number": 490, "latestStatusComment": { "kind": "{{kind}}" } }""");

        Assert.Equal(new WorkNote(text, null, null, kind), note);
    }

    [Fact]
    public void TheFallbackTextAndThePageHeadingAgreeForEachKind()
    {
        foreach (var kind in new[] { "blocker", "status" })
        {
            var note = NoteOf($$"""{ "number": 490, "latestStatusComment": { "kind": "{{kind}}" } }""")!;

            Assert.Equal(WorkBotText.NoteHeading(note), note.Text);
        }
    }

    [Fact]
    public void AStatusCommentThatSaysOnlyAMomentIsStillANote()
    {
        var note = NoteOf("""{ "number": 490, "latestStatusComment": { "at": "2026-11-10T00:00:00Z" } }""");

        Assert.NotNull(note);
        Assert.Null(note.Text);
        Assert.Null(note.Kind);
        Assert.Equal(new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.Zero), note.At);
    }
}
