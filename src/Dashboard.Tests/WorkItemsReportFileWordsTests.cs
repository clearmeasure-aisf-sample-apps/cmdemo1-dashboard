using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The three work-items file words that two readers each met by hand, now pinned to their one const:
/// <c>WorkItemIdentity.ColumnWord</c>, <c>WorkThroughput.AgentWord</c> and <c>WorkNote.CommentWord</c>.
/// <see cref="WorkItemIdentity.Read" /> and <c>ColumnVisit.Read</c> both read a column;
/// <see cref="WorkThroughput.Read" /> and <see cref="WorkItem.Read" /> both read an agent; and
/// <see cref="WorkNote.Read" />'s text fallback and <see cref="WorkItem" />'s <c>comment</c> section both read a
/// comment. Each word is now the single const, so a file written in that word reads back whole whichever reader met
/// it. The consts are internal and the app declares no <c>InternalsVisibleTo</c>, so they are pinned by reflection,
/// the way <see cref="WorkNoteWordsTests" /> and <see cref="WorkBoardWordsTests" /> pin the other words.
/// </summary>
public class WorkItemsReportFileWordsTests
{
    private static readonly string ColumnWord = ColumnField("ColumnWord");
    private static readonly string AgentWord = ThroughputField("AgentWord");
    private static readonly string CommentWord = NoteField("CommentWord");

    /// <summary>The three words are what the work file has always said them: column, agent, comment - three different words.</summary>
    [Fact]
    public void TheThreeWordsAreTheWorkFileNames()
    {
        Assert.Equal("column", ColumnWord);
        Assert.Equal("agent", AgentWord);
        Assert.Equal("comment", CommentWord);

        Assert.Equal(3, new[] { ColumnWord, AgentWord, CommentWord }
            .Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>An item's current column and the column of a visit in its <c>columnHistory</c> both read back from
    /// the one column word.</summary>
    [Fact]
    public void AnItemsColumnAndAVisitColumnBothReadBack()
    {
        var report = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 490,
                "{{ColumnWord}}": "In progress",
                "columnHistory": [ { "{{ColumnWord}}": "Todo", "enteredAt": "2026-11-08T00:00:00Z" } ] } ] }
            """)!;

        var item = Assert.Single(report.Items);
        Assert.Equal("In progress", item.Identity.Column);

        var visit = Assert.Single(item.ColumnHistory);
        Assert.Equal("Todo", visit.Column);
        Assert.Equal(new DateTimeOffset(2026, 11, 8, 0, 0, 0, TimeSpan.Zero), visit.EnteredAt);
    }

    /// <summary>An item's agent and a throughput row's agent both read back from the one agent word.</summary>
    [Fact]
    public void AnItemsAgentAndAThroughputRowAgentBothReadBack()
    {
        var report = WorkItemsReport.Parse($$"""
            {
              "items": [ { "number": 490, "{{AgentWord}}": "fixer-bot" } ],
              "stats": { "throughput": [ { "{{AgentWord}}": "fixer-bot", "merged": 3, "deployedProd": 1 } ] }
            }
            """)!;

        Assert.Equal("fixer-bot", report.Items.Single().Agent);

        var row = Assert.Single(report.Stats.Throughput!);
        Assert.Equal("fixer-bot", row.Agent);
        Assert.Equal(3, row.Merged);
        Assert.Equal(1, row.DeployedProd);
    }

    /// <summary>An item's <c>comment</c> section, and a note whose words sit under <c>comment</c>, both come back
    /// as the note's text.</summary>
    [Fact]
    public void ACommentSectionAndACommentNoteBothComeBackAsTheNoteText()
    {
        var fromSection = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 490, "{{CommentWord}}": { "text": "Waiting on the review" } } ] }
            """)!;
        var sectionNote = fromSection.Items.Single().Blocker;
        Assert.NotNull(sectionNote);
        Assert.Equal("Waiting on the review", sectionNote.Text);

        var fromWord = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 490, "blocker": { "{{CommentWord}}": "Waiting on the review" } } ] }
            """)!;
        var wordNote = fromWord.Items.Single().Blocker;
        Assert.NotNull(wordNote);
        Assert.Equal("Waiting on the review", wordNote.Text);
    }

    /// <summary>The internal const of that name on <see cref="WorkItemIdentity" />, read the way the other word tests read internal words.</summary>
    private static string ColumnField(string name) =>
        (string)typeof(WorkItemIdentity).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

    /// <summary>The internal const of that name on <see cref="WorkThroughput" />, read the way the other word tests read internal words.</summary>
    private static string ThroughputField(string name) =>
        (string)typeof(WorkThroughput).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

    /// <summary>The internal const of that name on <see cref="WorkNote" />, read the way the other word tests read internal words.</summary>
    private static string NoteField(string name) =>
        (string)typeof(WorkNote).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
}
