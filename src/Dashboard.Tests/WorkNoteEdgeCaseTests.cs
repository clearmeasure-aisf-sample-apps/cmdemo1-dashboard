using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkNote" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that names a short note the bot left: its latest status, or the
/// comment that says what blocked it. The three required parts - text, url, at - may each be null, and the fourth,
/// kind, is optional and defaults to null.
/// <para>
/// What the other files already pin is left alone. The taking of a note out of <c>workitems.json</c> is
/// <c>WorkNote.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> and is therefore
/// pinned through <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" />
/// says that a note with only a url is still a note; <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" />
/// that a blocker arrives with its text; and <see cref="WorkItemsTests.ThePublishedWorkItemsFileParses" /> that the
/// live feed's notes carry a kind. What a note is worth to the page - the heading it gets - is
/// <see cref="WorkBotText.NoteHeading" /> and <see cref="WorkItemsText.BlockerHeading" />, and is pinned in those
/// files.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each part to the property of its
/// name and holding the very instance it was given; the sixteen ways the three null-ready required parts can be
/// filled (each one can be present or absent); the optional kind part, which defaults to null and distinguishes a
/// status from a blocker; a note with nothing said about any of its parts being another note than no note at all;
/// the equality and hash of two notes that differ only in the optional kind; and the public surface, so that the day
/// a part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a note holds the parts the file gave
/// it and calls nothing at all, so every value below is one the test brought, and every call says the same thing
/// twice.
/// </para>
/// </summary>
public class WorkNoteEdgeCaseTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);

    /// <summary>A note with every part filled, including the optional kind.</summary>
    private static WorkNote Full => new("Waiting on review", new Uri("https://github.com/org/repo/issues/9"), At, "status");

    /// <summary>A note with no optional part.</summary>
    private static WorkNote Minimal => new("Waiting on review", null, null);

    // --------------------------------- the parts, by the names of their own properties

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same name:
    /// text first, url beside it, at next, kind last - the order the page shows them. Each is held as the very value
    /// the note was built of, never as a copy of it, and a new note of the same four parts is another note, not the
    /// one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var url = new Uri("https://github.com/org/repo/issues/9");
        var note = new WorkNote("Waiting on review", url, At, "status");

        Assert.Equal("Waiting on review", note.Text);
        Assert.Same(url, note.Url);
        Assert.Equal(At, note.At);
        Assert.Equal("status", note.Kind);

        var again = new WorkNote("Waiting on review", url, At, "status");

        Assert.Equal(note, again);
        Assert.NotSame(note, again);
        Assert.Same(url, again.Url);

        // A note with no optional kind is another value than the same note with it.
        var noKind = new WorkNote("Waiting on review", url, At);

        Assert.NotEqual(note, noKind);
        Assert.Null(noKind.Kind);
    }

    /// <summary>Each of the three null-ready required parts stands on its own property and nowhere else: setting
    /// text does not set url or at, and vice versa. The four defaults (all null) are one note, and each part set
    /// while the others are not is another.</summary>
    [Theory]
    [InlineData("Waiting", null, null)]
    [InlineData(null, "https://github.com/org/repo/issues/9", null)]
    [InlineData(null, null, "2026-11-09T16:00:00Z")]
    [InlineData("Waiting", "https://github.com/org/repo/issues/9", null)]
    [InlineData("Waiting", null, "2026-11-09T16:00:00Z")]
    [InlineData(null, "https://github.com/org/repo/issues/9", "2026-11-09T16:00:00Z")]
    [InlineData("Waiting", "https://github.com/org/repo/issues/9", "2026-11-09T16:00:00Z")]
    public void EachRequiredPartStandsOnItsOwnPropertyAndNowhereElse(string? text, string? url, string? at)
    {
        Uri? uri = url is null ? null : new Uri(url);
        DateTimeOffset? when = at is null ? null : new DateTimeOffset(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);
        var note = new WorkNote(text, uri, when);

        Assert.Equal(text, note.Text);
        Assert.Equal(uri, note.Url);
        Assert.Equal(when, note.At);
        Assert.Null(note.Kind);
    }

    /// <summary>The optional kind part stands on its own property and defaults to null. A note without a kind is
    /// another note than the same note with a kind, and the kind part does not affect the three required parts.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("status")]
    [InlineData("blocker")]
    [InlineData("other")]
    public void TheOptionalKindPartStandsOnItsOwnPropertyAndNowhereElse(string? kind)
    {
        var note = new WorkNote("Waiting", null, null, kind);

        Assert.Equal("Waiting", note.Text);
        Assert.Null(note.Url);
        Assert.Null(note.At);
        Assert.Equal(kind, note.Kind);
    }

    /// <summary>A note with no kind said is another note than one with a kind that is an empty string. The first
    /// has no kind part; the second has a kind part that says no word. Both are valid notes, and the page treats
    /// them the same way (it shows no kind), but they are not one.</summary>
    [Fact]
    public void ANoteWithNoKindSaidIsStillANote()
    {
        var noKind = new WorkNote("Waiting", null, null);
        var emptyKind = new WorkNote("Waiting", null, null, "");

        Assert.NotEqual(noKind, emptyKind);
        Assert.Null(noKind.Kind);
        Assert.Equal("", emptyKind.Kind);
    }

    // --------------------------------- the parts, together

    /// <summary>Two notes with the same three required parts but different kinds are not one. The kind part is not
    /// decorative; it is part of the value. Two notes with the same three required parts and the same kind are
    /// one.</summary>
    [Fact]
    public void TwoNotesAreOneWhileTheirFourPartsAre()
    {
        var url = new Uri("https://github.com/org/repo/issues/9");

        var note = new WorkNote("Waiting on review", url, At, "status");
        var noteAgain = new WorkNote("Waiting on review", url, At, "status");

        Assert.Equal(note, noteAgain);
        Assert.Equal(note.GetHashCode(), noteAgain.GetHashCode());

        var differentKind = new WorkNote("Waiting on review", url, At, "blocker");
        var differentText = new WorkNote("Blocker: the pipeline is red", url, At, "status");

        Assert.NotEqual(note, differentKind);
        Assert.NotEqual(note, differentText);

        // No note is one with no note, and not with any other thing.
        Assert.NotNull(note);
        Assert.NotEqual((object)"not a note", (object)note);

        // The hash stands still from one call to the next.
        Assert.Equal(note.GetHashCode(), note.GetHashCode());
        Assert.Equal(noteAgain.GetHashCode(), noteAgain.GetHashCode());
    }

    /// <summary>A note that differs in one part is another note. The page depends on this to tell apart two notes
    /// on the same item (a status and a blocker), and on the hash to put the two in a set without confusing them.</summary>
    [Fact]
    public void ANoteThatDiffersInOnePartIsAnotherNote()
    {
        var url = new Uri("https://github.com/org/repo/issues/9");

        var note = new WorkNote("Waiting on review", url, At, "status");
        var differentText = new WorkNote("Blocker: the pipeline is red", url, At, "status");
        var differentUrl = new WorkNote("Waiting on review", new Uri("https://example.com"), At, "status");
        var differentAt = new WorkNote("Waiting on review", url, At.AddHours(1), "status");
        var differentKind = new WorkNote("Waiting on review", url, At, "blocker");

        Assert.NotEqual(note, differentText);
        Assert.NotEqual(note, differentUrl);
        Assert.NotEqual(note, differentAt);
        Assert.NotEqual(note, differentKind);

        Assert.NotEqual(note.GetHashCode(), differentText.GetHashCode());
        Assert.NotEqual(note.GetHashCode(), differentUrl.GetHashCode());
        Assert.NotEqual(note.GetHashCode(), differentAt.GetHashCode());
        Assert.NotEqual(note.GetHashCode(), differentKind.GetHashCode());
    }

    /// <summary>No note is one with no note, and not with any other thing. A set of notes may hold null (it is a
    /// different value) and may hold things that are not notes without confusing them with notes.</summary>
    [Fact]
    public void NoNoteIsOneWithNoNoteAndNotWithAnyOtherThing()
    {
        var note = new WorkNote("Waiting", null, null);

        Assert.NotNull(note);
        Assert.NotEqual((object)"", (object)note);
        Assert.NotEqual((object)9, (object)note);
        Assert.NotEqual((object)true, (object)note);

        var set = new HashSet<WorkNote?>([null, note, new WorkNote("Blocker", null, null)]);

        Assert.Equal(3, set.Count);
    }

    // --------------------------------- the hash, the set, the operators

    /// <summary>The hash follows the parts: two notes that are one have the same hash, and two that are not one
    /// have different hashes. A set of notes holds one copy of each value, and the two operators follow the parts.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var url = new Uri("https://github.com/org/repo/issues/9");

        var note = new WorkNote("Waiting on review", url, At, "status");
        var noteAgain = new WorkNote("Waiting on review", url, At, "status");
        var differentNote = new WorkNote("Blocker: the pipeline is red", null, null, "blocker");

        Assert.Equal(note.GetHashCode(), noteAgain.GetHashCode());
        Assert.Equal(note.GetHashCode(), note.GetHashCode());

        var set = new HashSet<WorkNote>([note, noteAgain, differentNote]);

        Assert.Equal(2, set.Count);
        Assert.Contains(note, set);
        Assert.Contains(differentNote, set);

        Assert.True(note == noteAgain);
        Assert.True(note != differentNote);
    }

    // --------------------------------- the words the note says

    /// <summary>The note says its four parts in the order they are named: text, url, at, kind. This is the
    /// record's own voice, for the debugger and for the log, and it follows the parts as they change.</summary>
    [Fact]
    public void TheRecordSaysItsFourPartsInTheOrderTheyAreNamed()
    {
        var url = new Uri("https://github.com/org/repo/issues/9");

        var note = new WorkNote("Waiting on review", url, At, "status");

        var text = note.ToString();

        Assert.Contains("Text = Waiting on review", text, StringComparison.Ordinal);
        Assert.Contains("Url = ", text, StringComparison.Ordinal);
        Assert.Contains("At = ", text, StringComparison.Ordinal);
        Assert.Contains("Kind = status", text, StringComparison.Ordinal);
    }

    /// <summary>The note's own words follow the reader while the parts themselves do not: the moment written in
    /// the text is the reader's moment, not the one the note carries. A note carried across a border says its parts
    /// in the same words.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var note = new WorkNote("Waiting", null, At, null);

        var english = CultureScope.Says(new CultureInfo("en-US"), () => note.ToString());
        Assert.Contains("11/9/2026", english, StringComparison.Ordinal);

        var german = CultureScope.Says(new CultureInfo("de-DE"), () => note.ToString());
        Assert.Contains("9.11.2026", german, StringComparison.Ordinal);
    }

    // --------------------------------- the surface

    /// <summary>The record has three required parts, one optional part, and the members every record is asked:
    /// the constructor, the properties, ToString, Equals, GetHashCode, the == and != operators, and the copy
    /// constructor. No more, no less. The day a part is added or taken away this test says so.</summary>
    [Fact]
    public void TheRecordHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkNote);

        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single(c => c.GetParameters().Length == 4);

        Assert.Equal("Text", constructor.GetParameters()[0].Name);
        Assert.Equal("Url", constructor.GetParameters()[1].Name);
        Assert.Equal("At", constructor.GetParameters()[2].Name);
        Assert.Equal("Kind", constructor.GetParameters()[3].Name);
    }
}