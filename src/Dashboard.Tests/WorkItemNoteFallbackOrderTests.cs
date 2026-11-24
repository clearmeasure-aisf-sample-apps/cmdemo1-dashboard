using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The fallback order of an item's note (<see cref="WorkItem.Blocker" />), declared on <c>WorkItem.Note</c> in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c>: <c>latestStatusComment</c>, then <c>blocker</c>, then
/// <c>comment</c>, then the flat <c>status</c> with <c>statusUrl</c> and <c>statusAt</c>.
/// </summary>
public class WorkItemNoteFallbackOrderTests
{
    private const string Latest = """ "latestStatusComment": { "kind": "status", "text": "from latest" } """;
    private const string Blocker = """ "blocker": { "text": "from blocker" } """;
    private const string Comment = """ "comment": { "text": "from comment" } """;
    private const string Status = """ "status": "from status", "statusUrl": "https://example.net/s", "statusAt": "2026-11-09T22:00:00Z" """;

    private static WorkNote? NoteOf(params string[] parts)
    {
        var body = $$"""{ "items": [ { "number": 1, {{string.Join(", ", parts)}} } ] }""";
        return Assert.Single(WorkItemsReport.Parse(body)!.Items).Blocker;
    }

    [Fact]
    public void LatestStatusCommentWinsOverEveryOtherSource() =>
        Assert.Equal("from latest", NoteOf(Latest, Blocker, Comment, Status)!.Text);

    [Fact]
    public void BlockerWinsWhenThereIsNoLatestStatusComment() =>
        Assert.Equal("from blocker", NoteOf(Blocker, Comment, Status)!.Text);

    [Fact]
    public void CommentWinsWhenThereIsNeitherLatestStatusCommentNorBlocker() =>
        Assert.Equal("from comment", NoteOf(Comment, Status)!.Text);

    [Fact]
    public void TheFlatStatusIsTheLastResortAndCarriesItsAddressAndTime()
    {
        var note = NoteOf(Status)!;
        Assert.Equal("from status", note.Text);
        Assert.Equal(new Uri("https://example.net/s"), note.Url);
        Assert.Equal(new DateTimeOffset(2026, 11, 9, 22, 0, 0, TimeSpan.Zero), note.At);
        Assert.Null(note.Kind);
    }

    [Fact]
    public void AnEmptyEarlierSourceFallsThroughToTheNextOne() =>
        Assert.Equal("from comment", NoteOf(""" "latestStatusComment": { } """, """ "blocker": { } """, Comment)!.Text);

    [Fact]
    public void NoSourceAtAllIsNoNote() => Assert.Null(NoteOf(""" "title": "t" """));
}
