using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeploymentsReport" /> - what the system's
/// deployments file makes: the moment the system read Octopus Deploy, the deployments that are in flight or ended
/// lately, and the four parts a newer file carries on its own (<see cref="DeploymentsReport.Recent" />,
/// <see cref="DeploymentsReport.Freezes" />, <see cref="DeploymentsReport.Missing" /> and
/// <see cref="DeploymentsReport.SaysActivity" />), the ten minutes a mark stands for, the marks of one environment
/// and the reading of the file.
/// <para>
/// What is pinned where it belongs, and is not pinned again here: <see cref="DeploymentsTests" /> pins the file read
/// part by part and in its order, an entry without its names left out, a time that does not parse no time, a body
/// that is no file at all (nothing, the empty word, a server's answer, a list, an object without its deployments and
/// a document cut off), the empty list a report that marks nothing, the six states and the word for one the page
/// does not know, the order of the marks - what a person has to act on first, then what runs, then what is queued,
/// then what ended - the ten minutes at both of their ends and a clock that runs behind, the deployable a project is
/// named after and the system's own project none at all, no slug and no deployable, the dot's shape per state, the
/// help's words and the monitor reading the file over the network; <see cref="DeploymentActivityTests" /> pins the
/// four parts a newer file carries - the last deployments newest first however the file orders them, the freezes and
/// the missing in the file's words, a part of another kind left out and nothing of the older file lost - and
/// <see cref="DeploymentsReport.SaysActivity" /> true for a file with any of the three lists and false for one with
/// none or with a part that is no list; <see cref="RecentDeploymentEdgeCaseTests" />,
/// <see cref="DeploymentFreezeTests" /> and <see cref="DeploymentWaitEdgeCaseTests" /> pin the entries of those
/// lists and the words the page makes of them - the last of those three under its old name, for it walks the whole
/// of <c>DeploymentActivity</c> - and <see cref="JsonReadTests" /> the reading of a word, a moment in whichever hours
/// it is written and an address the page may link to.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the ten minutes as the fixed value they are and the field that holds
/// them; a body that is nothing but whitespace, a root that is a number, a decimal, a boolean, the word for nothing,
/// a list with entries or two documents in one place, and the document the reader itself refuses - its trailing
/// comma, its missing comma, its closing bracket one too many, the comment a person annotates a copy with, the
/// byte-order mark in front of it, the nesting past the reader's own depth - each no report rather than an error,
/// while whitespace standing about a file is the file's and no reason to refuse it; the deployments list as the one part a report cannot do without even when the other three stand in the
/// file; a list of entries that are no entries at all; the freezes keeping the file's order where the last
/// deployments are put newest first, two of those ending at one instant spelled in other hours keeping the file's
/// order between them, and the missing kept in its own letters - the empty word, the padding, the case - and still
/// saying activity when nothing of it counted; the arguments <see cref="DeploymentsReport.Marks" /> refuses, the one
/// it does not, the deployables walked once however many marks come of them, a deployable named twice or as nothing,
/// and an environment of nothing asked for as the nothing it is; and the record itself, which no test of the file has
/// ever built by hand: its two arguments landing on the properties of their names with the very list held, its four
/// parts of its own empty and unsaying, the copy that turns the part it names, two readings of one file being two
/// reports because the lists are compared as the references they stand in, the two parts read back out in the order
/// they went in, the words the record says about itself and the surface it has.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings,
/// and no reading is asked for one. The file fetched over HTTP is the monitor's, and <see cref="DeploymentsTests" />
/// pins it with fake time.
/// </para>
/// </summary>
public class DeploymentsReportEdgeCaseTests
{
    /// <summary>Thursday a few minutes past four, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 4, 23, 24, TimeSpan.Zero);

    /// <summary>Five minutes before it: the moment a deployment that ended ended.</summary>
    private static readonly DateTimeOffset Finished = new(2026, 10, 8, 4, 18, 0, TimeSpan.Zero);

    private const string Slug = "demo";

    private static readonly string[] Ui = ["ui"];

    /// <summary>The byte-order mark a file copied along with its answer begins with.</summary>
    private const string ByteOrderMark = "\ufeff";

    /// <summary>The file the system writes, read as the page reads it.</summary>
    private static DeploymentsReport Read(string file) => DeploymentsReport.Parse(file)!;

    /// <summary>A deployment of that environment in flight: executing, and with no end.</summary>
    private static Deployment InFlight(string environment = "uat", string project = "demo-ui", string release = "1") =>
        new(project, environment, release, DeploymentState.Executing, "executing", Finished.AddMinutes(-3), null, null);

    /// <summary>A deployment of that environment that ended at <see cref="Finished" />.</summary>
    private static Deployment Ended(string environment, string project = "demo-api", string release = "2") =>
        new(project, environment, release, DeploymentState.Failed, "failed", Finished.AddMinutes(-10), Finished, null);

    // --------------------------------------------------------------- the one length the report names

    /// <summary>
    /// How long a deployment that ended stays marked, as the fixed value it is: ten minutes, whole, held in a field
    /// of the type that is no reference and no one may turn. The two ends of the window it gives are walked with
    /// marks by <see cref="DeploymentsTests" />; what is pinned here is the length itself, since a mark that stood a
    /// minute would hide a deployment a person has to see and one that stood an hour would go on marking a
    /// deployment the page has no business still marking.
    /// </summary>
    [Fact]
    public void WhatEndedIsMarkedForTenMinutes()
    {
        Assert.Equal(TimeSpan.FromMinutes(10), DeploymentsReport.EndedFor);
        Assert.Equal(600d, DeploymentsReport.EndedFor.TotalSeconds);
        Assert.NotEqual(TimeSpan.FromSeconds(10), DeploymentsReport.EndedFor);
        Assert.NotEqual(TimeSpan.FromMinutes(1), DeploymentsReport.EndedFor);
        Assert.NotEqual(TimeSpan.FromMinutes(60), DeploymentsReport.EndedFor);
        Assert.Equal(DeploymentsReport.EndedFor, DeploymentsReport.EndedFor);

        var field = typeof(DeploymentsReport).GetField(nameof(DeploymentsReport.EndedFor), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.Equal(typeof(TimeSpan), field!.FieldType);
        Assert.True(field.IsPublic);
        Assert.True(field.IsStatic);
        Assert.True(field.IsInitOnly);
        Assert.False(field.IsLiteral);
        Assert.Equal(DeploymentsReport.EndedFor, field.GetValue(null));
    }

    // ----------------------------------------------------------- a body that is no file at all

    /// <summary>
    /// A body that says nothing is no file, whether it is the empty word or nothing but the whitespace a proxy pads
    /// an answer with: the page keeps the marks it had rather than drawing an empty file. The empty word and nothing
    /// are pinned where the file is read; the whitespace of its own is pinned here, since it is a body that is not
    /// empty and still says nothing.
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData(" \t\n\r ")]
    public void ABodyOfNothingButWhitespaceIsNoFile(string body) => Assert.Null(DeploymentsReport.Parse(body));

    /// <summary>
    /// The deployments file is the object it is written as: a root that is a whole number, a decimal, a boolean, the
    /// word for nothing or a word of its own is no report, and neither is a list - not the empty list, which is a
    /// file of another shape, nor a list with entries in it, which are nobody's deployments - nor two documents
    /// written in one place, which the reader stops at the end of the first. An error is never what comes back: a
    /// page reads the last file it could read.
    /// </summary>
    [Theory]
    [InlineData("0")]
    [InlineData("42")]
    [InlineData("-1.5")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    [InlineData("\"deployments\"")]
    [InlineData("[ 1, 2 ]")]
    [InlineData("""[ { "project": "demo-ui", "environment": "uat", "release": "1", "state": "executing" } ]""")]
    [InlineData("""{ "deployments": [] }{ "deployments": [] }""")]
    [InlineData("""{ "deployments": [] } }""")]
    public void ARootThatIsNoObjectIsNoFile(string body) => Assert.Null(DeploymentsReport.Parse(body));

    /// <summary>
    /// The document the reader itself refuses is no report rather than an error: left with a trailing comma the
    /// file's writer may put there, inside the list and inside an entry, annotated with the comment a person may add
    /// to a copy of it, and closed one bracket too many. None of them is a file the page may read, and none of them
    /// is a fault the page is told about - the document cut off before it ends being pinned where the file is read.
    /// The bodies below are each whole up to the point they are wrong, so that none of them repeats that pin.
    /// </summary>
    [Theory]
    [InlineData("""{ "deployments": [], }""")]
    [InlineData("""{ "deployments": [ { "project": "demo-ui", } ] }""")]
    [InlineData("""{ /* the sample */ "deployments": [] }""")]
    [InlineData("""{ "deployments": [ ] ] }""")]
    [InlineData("""{ "deployments": [] "generated": "2026-10-08T04:22:24Z" }""")]
    public void ADocumentTheReaderRefusesIsNoFile(string body) => Assert.Null(DeploymentsReport.Parse(body));

    /// <summary>
    /// A byte-order mark in front of the file is not whitespace the reader skips: the answer a server copies with its
    /// mark is no report, the same answer as it stands being one - which is what the page shows as a file it could
    /// not read rather than a fault it did not expect.
    /// </summary>
    [Fact]
    public void AFileThatBeginsWithAMarkIsNoFile()
    {
        const string File = """{ "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1", "state": "executing" } ] }""";

        Assert.NotNull(DeploymentsReport.Parse(File));
        Assert.Null(DeploymentsReport.Parse(ByteOrderMark + File));
        Assert.Null(DeploymentsReport.Parse(" " + ByteOrderMark + File));
    }

    /// <summary>
    /// A document nested past the depth the reader goes to is no file: the list of deployments a file carries is one
    /// level deep, and a body that goes a hundred levels down is a body nobody wrote - refused quietly, as everything
    /// else that is no file is, while the list of a list as a file may really carry is a report of no deployments.
    /// </summary>
    [Fact]
    public void ADocumentNestedPastWhatTheReaderGoesIsNoFile()
    {
        var deep = "{ \"deployments\": [" + new string('[', 100) + new string(']', 100) + "] }";

        Assert.Null(DeploymentsReport.Parse(deep));

        // One level, as the file writes it: a report, with nothing of the nesting left in it.
        var shallow = """{ "deployments": [ [ ] ] }""";
        Assert.Empty(DeploymentsReport.Parse(shallow)!.Deployments);
    }

    /// <summary>
    /// The <c>deployments</c> list is the one part a report cannot do without: the file that carries the three newer
    /// lists and no deployments is no report, the deployments of a kind that is no array - an object, a number, a
    /// boolean, a word - likewise, and a name that only looks like it is no name. The other three lists never make a
    /// report on their own, whatever stands in them.
    /// </summary>
    [Theory]
    [InlineData("""{ "recent": [], "freezes": [], "missing": [] }""")]
    [InlineData("""{ "recent": [ 1 ], "freezes": [ 2 ], "missing": [ "freezes" ] }""")]
    [InlineData("""{ "generated": "2026-10-08T04:22:24Z", "system": "demo", "octopus": "https://octopus.example.net" }""")]
    [InlineData("""{ "deployment": [] }""")]
    [InlineData("""{ "Deployments": [] }""")]
    [InlineData("""{ "deployments": {} }""")]
    [InlineData("""{ "deployments": { "project": "demo-ui" } }""")]
    [InlineData("""{ "deployments": 3 }""")]
    [InlineData("""{ "deployments": true }""")]
    [InlineData("""{ "deployments": "[]" }""")]
    public void AFileWithoutItsDeploymentsListIsNoFile(string body) => Assert.Null(DeploymentsReport.Parse(body));

    /// <summary>
    /// Whitespace about a file is the file's and no reason to refuse it: a body padded, newlined and tabbed as an
    /// answer copied through a proxy is read for what it says, while a body that is only that whitespace is nothing.
    /// </summary>
    [Fact]
    public void WhitespaceAboutAFileIsTheFiles()
    {
        var report = Read("\t\n { \"deployments\": [ { \"project\": \"demo-ui\", \"environment\": \"uat\", \"release\": \"1\", \"state\": \"executing\" } ] }\r\n ");

        Assert.Null(report.Generated);
        Assert.Equal(["demo-ui"], report.Deployments.Select(deployment => deployment.Project));
        Assert.Single(report.Marks("uat", Slug, Ui, Now));
    }

    /// <summary>
    /// A list of entries that are no entries is a report that marks nothing rather than no report: a name written as
    /// nothing but whitespace is no name where an entry is read, so the entry that carries nothing else is left out,
    /// and a file whose whole list is such entries and words, numbers and lists stands as a file with nothing in it -
    /// its moment kept, its deployments none, its marks none, its four parts of its own empty and unsaying. The
    /// entries read without their names at all are pinned where the file is read; what is pinned here is that a list
    /// of nothing that counts is still a list, as the empty <c>deployments</c> list is.
    /// </summary>
    [Fact]
    public void AListOfEntriesThatAreNoEntriesIsAFileThatMarksNothing()
    {
        var report = Read("""
            { "generated": "2026-10-08T04:22:24Z",
              "deployments": [ { }, { "project": "   " }, { "project": "demo-ui", "environment": "uat", "release": "1", "state": "   " }, true, [ ], 1, "demo-ui", null ] }
            """);

        Assert.Equal(new DateTimeOffset(2026, 10, 8, 4, 22, 24, TimeSpan.Zero), report.Generated);
        Assert.Empty(report.Deployments);
        Assert.Empty(report.Marks("uat", Slug, Ui, Now));
        Assert.Empty(report.Recent);
        Assert.Empty(report.Freezes);
        Assert.Empty(report.Missing);
        Assert.False(report.SaysActivity);
    }

    // ------------------------------------------------------- the three lists, each in its own order

    /// <summary>
    /// The last deployments of each project in each environment come out newest first, whatever the file's order -
    /// and two of them that ended at one instant keep the file's order between them, since the sort is stable and
    /// nothing else decides. That one instant is the instant and not the hours it is written in: the entry spelled
    /// five hours and a half east of the meridian stands at the same moment as its neighbour on the meridian, holds
    /// the hours it was written in, and is neither moved before it nor after it.
    /// </summary>
    [Fact]
    public void TheLastDeploymentsComeNewestFirstAndTwoAtOneInstantKeepTheFilesOrder()
    {
        var report = Read("""
            { "deployments": [],
              "recent": [
                { "project": "demo-ui", "environment": "uat", "release": "middle", "result": "succeeded", "finished": "2026-10-08T04:00:00Z" },
                { "project": "demo-ui", "environment": "uat", "release": "east", "result": "succeeded", "finished": "2026-10-08T09:30:00+05:30" },
                { "project": "demo-ui", "environment": "uat", "release": "written last", "result": "succeeded", "finished": "2026-10-08T04:00:00Z" },
                { "project": "demo-ui", "environment": "uat", "release": "newest", "result": "succeeded", "finished": "2026-10-08T06:00:00Z" },
                { "project": "demo-ui", "environment": "uat", "release": "oldest", "result": "succeeded", "finished": "2026-10-08T02:00:00Z" } ] }
            """);

        Assert.Equal(["newest", "middle", "east", "written last", "oldest"], report.Recent.Select(recent => recent.Release));
        Assert.Equal(TimeSpan.FromMinutes(330), report.Recent[2].Finished.Offset);
        Assert.Equal(new DateTimeOffset(2026, 10, 8, 4, 0, 0, TimeSpan.Zero), report.Recent[2].Finished.ToUniversalTime());
        Assert.True(report.Recent[1].Finished == report.Recent[2].Finished);
        Assert.True(report.Recent[2].Finished == report.Recent[3].Finished);
        Assert.Equal(6, report.Recent[0].Finished.Hour);
        Assert.Equal(9, report.Recent[2].Finished.Hour);
        Assert.Equal(4, report.Recent[2].Finished.ToUniversalTime().Hour);
        Assert.True(report.Recent[0].Finished > report.Recent[1].Finished);
        Assert.True(report.Recent[3].Finished > report.Recent[4].Finished);
    }

    /// <summary>
    /// The freezes are the only list of the file the report leaves where the file put them: no freeze is more urgent
    /// than another by its hours, and what says which of them is in force is the moment the page is asked, which the
    /// lines of an environment and of a node work out. A file that writes the later freeze first is read with the
    /// later freeze first.
    /// </summary>
    [Fact]
    public void TheFreezesKeepTheFilesOrderAndTakeNoOrderOfTheirOwn()
    {
        var report = Read("""
            { "deployments": [],
              "freezes": [
                { "name": "later", "from": "2026-11-01T00:00:00Z", "to": "2026-11-02T00:00:00Z", "active": false, "environments": [ "uat" ], "projects": [ "demo-ui" ] },
                { "name": "soonest", "from": "2026-10-08T04:00:00Z", "to": "2026-10-08T05:00:00Z", "active": true, "environments": [ "uat" ], "projects": [ "demo-ui" ] },
                { "name": "earlier", "from": "2026-10-01T00:00:00Z", "to": "2026-10-02T00:00:00Z", "active": false, "environments": [ "uat" ], "projects": [ "demo-ui" ] } ] }
            """);

        Assert.Equal(["later", "soonest", "earlier"], report.Freezes.Select(freeze => freeze.Name));
        Assert.Equal([false, true, false], report.Freezes.Select(freeze => freeze.Active));
    }

    /// <summary>
    /// What the system could not read is kept in the file's own letters: the empty word is a word it said, a name
    /// padded with whitespace keeps its padding, and a name in another case is another name - the report trims and
    /// cases nothing of it, since the page says the parts back in the words the file wrote them in. That the two
    /// spellings of one word are two entries of the list is what that costs, and what a mark and a line are built of.
    /// </summary>
    [Fact]
    public void WhatIsMissingIsKeptInItsOwnLetters()
    {
        var report = Read("""{ "deployments": [], "missing": [ "", "  freezes  ", "Freezes", "freezes", "startedBy" ] }""");

        Assert.Equal(["", "  freezes  ", "Freezes", "freezes", "startedBy"], report.Missing);
        Assert.Equal(5, report.Missing.Count);
        Assert.True(report.SaysActivity);
        Assert.Equal(report.Missing, Read("""{ "deployments": [], "missing": [ "", "  freezes  ", "Freezes", "freezes", "startedBy" ] }""").Missing);
    }

    /// <summary>
    /// A <c>missing</c> of nothing that counts is still a list the file wrote: the number, the word for nothing, the
    /// list, the object and the boolean all fall out, and what is left is an empty list in a report that says its
    /// activity - the page then says that the system read what it could, and read nothing more.
    /// </summary>
    [Fact]
    public void AMissingListOfNothingThatCountsStillSaysActivity()
    {
        var report = Read("""{ "deployments": [], "missing": [ 1, null, [ "freezes" ], { "name": "freezes" }, true ] }""");

        Assert.Empty(report.Missing);
        Assert.True(report.SaysActivity);
    }

    // ------------------------------------------------------------------------- the marks of an environment

    /// <summary>
    /// The marks of an environment refuse a slug and a deployables list that are not there, each by the name the
    /// guard keeps and in the order the arguments stand - the slug first, so a call that gives neither is told of the
    /// slug. The environment asked for is nobody's guard: nothing, the empty word and a name padded with whitespace
    /// are names no deployment of a file carries, and each marks nothing rather than faulting the page.
    /// </summary>
    [Fact]
    public void TheMarksRefuseASlugAndDeployablesThatAreNotThere()
    {
        var report = Read("""{ "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1", "state": "executing" } ] }""");

        Assert.Equal("slug", Assert.Throws<ArgumentNullException>(() => report.Marks("uat", null!, Ui, Now)).ParamName);
        Assert.Equal("deployables", Assert.Throws<ArgumentNullException>(() => report.Marks("uat", Slug, null!, Now)).ParamName);
        Assert.Equal("slug", Assert.Throws<ArgumentNullException>(() => report.Marks("uat", null!, null!, Now)).ParamName);

        Assert.Empty(report.Marks(null!, Slug, Ui, Now));
        Assert.Empty(report.Marks(string.Empty, Slug, Ui, Now));
        Assert.Empty(report.Marks(" uat ", Slug, Ui, Now));
        Assert.Empty(report.Marks("uat, prod", Slug, Ui, Now));
        Assert.Single(report.Marks("uat", Slug, Ui, Now));
    }

    /// <summary>
    /// The deployables are walked once, however many marks come of them and whether the environment marks anything at
    /// all: the list is taken to hand before the first deployment is looked at, so a caller may hand over a projection
    /// of its topology - a sequence that counts, a sequence that reads a dictionary - and be asked for it once rather
    /// than once per deployment.
    /// </summary>
    [Fact]
    public void TheDeployablesAreWalkedOnceHoweverManyMarksComeOfThem()
    {
        var report = Read("""
            { "deployments": [
                { "project": "demo-ui", "environment": "uat", "release": "1", "state": "executing" },
                { "project": "demo-api", "environment": "uat", "release": "2", "state": "queued" },
                { "project": "demo-ui", "environment": "uat", "release": "3", "state": "waiting" } ] }
            """);
        var walks = 0;

        IEnumerable<string> Counted()
        {
            walks++;
            yield return "ui";
            yield return "api";
        }

        Assert.Equal(3, report.Marks("uat", Slug, Counted(), Now).Count);
        Assert.Equal(1, walks);

        Assert.Empty(report.Marks("prod", Slug, Counted(), Now));
        Assert.Equal(2, walks);

        // Walked to the end once, and its own order kept: the first name of the list is the deployable a mark takes.
        Assert.Equal(["ui", "ui", "api"], report.Marks("uat", Slug, Counted(), Now).Select(mark => mark.Deployable));
        Assert.Equal(3, walks);
    }

    /// <summary>
    /// A deployable named twice in the list handed over is marked as the first of them, in the list's own order and
    /// by a name that ignores case; a name that is nothing matches no project, and a slug of nothing but whitespace
    /// names no deployable and no system's own project, since the slug is compared as it is given and never trimmed.
    /// </summary>
    [Fact]
    public void ADeployableNamedTwiceOrAsNothingIsTheFirstOfThemOrNoneOfThem()
    {
        var report = Read("""
            { "deployments": [
                { "project": "demo-ui", "environment": "uat", "release": "1", "state": "executing" },
                { "project": "demo-api", "environment": "uat", "release": "2", "state": "executing" },
                { "project": "demo-system", "environment": "uat", "release": "9", "state": "executing" } ] }
            """);

        Assert.Equal(["UI", "Api", null], report.Marks("uat", Slug, ["UI", "ui", null!, "Api"], Now).Select(mark => mark.Deployable));

        // A deployable that is nothing matches no project: it is passed over and never handed to a mark.
        Assert.Equal(["ui", null, null], report.Marks("uat", Slug, [null!, "ui"], Now).Select(mark => mark.Deployable));

        // The whitespace slug, and the slug with whitespace about it, are slugs that name nothing.
        Assert.All(report.Marks("uat", " ", Ui, Now), mark =>
        {
            Assert.Null(mark.Deployable);
            Assert.False(mark.OfSystem);
        });
        Assert.All(report.Marks("uat", "demo ", Ui, Now), mark => Assert.Null(mark.Deployable));
        Assert.Equal(3, report.Marks("uat", " ", Ui, Now).Count);
    }

    /// <summary>
    /// An environment of nothing is asked for as the nothing it is: the record keeps the environment it is given, and
    /// a deployment built by hand with none is marked when nothing is asked for and by nothing else - no name, not
    /// even the empty word. A file cannot give one, since it is the reading that leaves an entry without its
    /// environment out; only the page's own construction of a deployment can.
    /// </summary>
    [Fact]
    public void AnEnvironmentOfNothingIsAskedForAsTheNothingItIs()
    {
        var nowhere = new Deployment("demo-ui", null!, "1", DeploymentState.Executing, "executing", Finished, null, null);
        var report = new DeploymentsReport(Now, [nowhere]);

        var mark = Assert.Single(report.Marks(null!, Slug, Ui, Now));

        Assert.Null(mark.Environment);
        Assert.Equal("demo-ui", mark.Project);
        Assert.Equal("deploying demo-ui 1 to ", mark.Sentence);
        Assert.Empty(report.Marks(string.Empty, Slug, Ui, Now));
        Assert.Empty(report.Marks("uat", Slug, Ui, Now));
    }

    /// <summary>
    /// The marks are the deployments and none of the other three parts: a file that names a deployment of an
    /// environment in its <c>recent</c>, in its <c>freezes</c> and in its <c>missing</c> while its
    /// <c>deployments</c> list stands empty marks nothing, and a report built of the same four parts by hand marks
    /// nothing either. What is in flight is marked at either end of the moment asked for, and what ended is marked
    /// while it is lately ended and not marked long after - on parts the test built rather than on a file, while the
    /// two ends of the ten minutes themselves are pinned where the file is read.
    /// </summary>
    [Fact]
    public void TheMarksAreTheDeploymentsAndNoneOfTheOtherThreeParts()
    {
        var report = Read("""
            { "deployments": [],
              "recent": [ { "project": "demo-ui", "environment": "uat", "release": "9", "result": "succeeded", "finished": "2026-10-08T04:22:00Z" } ],
              "freezes": [ { "name": "now", "from": "2026-10-08T04:00:00Z", "to": "2026-10-08T05:00:00Z", "active": true, "environments": [ "uat" ], "projects": [ "demo-ui" ] } ],
              "missing": [ "deployments" ] }
            """);

        Assert.Single(report.Recent);
        Assert.Single(report.Freezes);
        Assert.Single(report.Missing);
        Assert.True(report.SaysActivity);
        Assert.Empty(report.Marks("uat", Slug, Ui, Now));

        var built = new DeploymentsReport(Now, [])
        {
            Recent = [new RecentDeployment("demo-ui", "uat", "9", DeploymentState.Succeeded, "succeeded", Finished, null, null)],
            Freezes = [new DeploymentFreeze("now", Now.AddMinutes(-20), Now.AddMinutes(20), true, ["uat"], ["demo-ui"])],
            Missing = ["deployments"],
            SaysActivity = true,
        };

        Assert.Empty(built.Marks("uat", Slug, Ui, Now));

        // In flight, marked however far the moment asked for stands from the file; ended, marked for ten minutes.
        var both = new DeploymentsReport(Now, [InFlight(), Ended("prod")]);

        Assert.Single(both.Marks("uat", Slug, Ui, Now.AddYears(-100)));
        Assert.Single(both.Marks("uat", Slug, Ui, Now.AddYears(100)));
        Assert.Single(both.Marks("prod", Slug, Ui, Now));
        Assert.Empty(both.Marks("prod", Slug, Ui, Now.AddYears(100)));
        Assert.Equal(2, both.Deployments.Count);
    }

    // ------------------------------------------------------------------ the report built by hand

    /// <summary>
    /// A report is the two arguments it is made of and the four parts of its own: each argument lands on the property
    /// of the same name - the moment held as the instant and the hours it is given, at either end of the clock and
    /// nowhere at all - and the list is held as the very reference it was handed, never copied and never asked
    /// anything of. The four parts a file from before them does not have are empty and say nothing, which is what a
    /// report built for a test, or built by the page's own code, starts out as.
    /// </summary>
    [Fact]
    public void TheReportIsItsTwoArgumentsAndFourPartsOfItsOwn()
    {
        var deployments = new[] { InFlight(), Ended("prod") };
        var at = new DateTimeOffset(2026, 10, 8, 9, 22, 24, TimeSpan.FromMinutes(330));
        var report = new DeploymentsReport(at, deployments);

        Assert.Equal(at, report.Generated);
        Assert.Equal(TimeSpan.FromMinutes(330), report.Generated!.Value.Offset);
        Assert.Equal(9, report.Generated!.Value.Hour);
        Assert.Equal(new DateTimeOffset(2026, 10, 8, 3, 52, 24, TimeSpan.Zero), report.Generated!.Value.ToUniversalTime());
        Assert.Same(deployments, report.Deployments);
        Assert.Equal(2, report.Deployments.Count);

        Assert.Empty(report.Recent);
        Assert.Empty(report.Freezes);
        Assert.Empty(report.Missing);
        Assert.False(report.SaysActivity);

        Assert.Equal(DateTimeOffset.MinValue, new DeploymentsReport(DateTimeOffset.MinValue, deployments).Generated);
        Assert.Equal(DateTimeOffset.MaxValue, new DeploymentsReport(DateTimeOffset.MaxValue, deployments).Generated);
        Assert.Null(new DeploymentsReport(null, deployments).Generated);
        Assert.Same(deployments, new DeploymentsReport(null, deployments).Deployments);

        // The moment the report carries is no part of the marking, which asks for a moment of its own.
        Assert.Single(report.Marks("uat", Slug, Ui, Now));
        Assert.Single(report.Marks("uat", Slug, Ui, at));
        Assert.Single(report.Marks("uat", Slug, Ui, DateTimeOffset.MinValue));
    }

    /// <summary>
    /// Any of the six parts may be turned as the report is copied, and stands where it is turned to while the rest
    /// stay the very values the report was made of: the list handed to the copy is the same reference, the original
    /// is untouched, and a copy that names nothing its own is the report it was copied from - another reference of
    /// one report, since nothing of it differs. A part turned to nothing is nothing rather than the empty word or an
    /// empty list of its own making.
    /// </summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheRest()
    {
        var deployments = new[] { InFlight() };
        var report = new DeploymentsReport(Now, deployments);

        var said = report with { SaysActivity = true, Missing = ["freezes", "startedBy"] };

        Assert.True(said.SaysActivity);
        Assert.False(report.SaysActivity);
        Assert.Equal(["freezes", "startedBy"], said.Missing);
        Assert.Empty(report.Missing);
        Assert.Same(deployments, said.Deployments);
        Assert.Equal(Now, said.Generated);
        Assert.Empty(said.Recent);
        Assert.Empty(said.Freezes);
        Assert.NotSame(report, said);
        Assert.NotEqual(report, said);

        var dated = report with { Generated = Finished };

        Assert.Equal(Finished, dated.Generated);
        Assert.Equal(Now, report.Generated);
        Assert.Same(deployments, dated.Deployments);
        Assert.NotEqual(report, dated);

        var unchanged = report with { };

        Assert.Equal(report, unchanged);
        Assert.NotSame(report, unchanged);
        Assert.Same(report.Deployments, unchanged.Deployments);
        Assert.Same(report.Recent, unchanged.Recent);
        Assert.Same(report.Freezes, unchanged.Freezes);
        Assert.Same(report.Missing, unchanged.Missing);

        var emptied = report with { Generated = null, Deployments = [], Recent = [], Freezes = [], Missing = [], SaysActivity = false };

        Assert.Null(emptied.Generated);
        Assert.Empty(emptied.Deployments);
        Assert.Empty(emptied.Marks("uat", Slug, Ui, Now));
        Assert.False(emptied.SaysActivity);
        Assert.Single(report.Marks("uat", Slug, Ui, Now));
    }

    /// <summary>
    /// Two readings of one file are two reports and not one: the lists a report holds are compared as the references
    /// they stand in, so a report of the same moment and the same deployments, read again, is another report - and
    /// one list handed to two reports makes them one report, of one hash, one of a set and one of a dictionary. The
    /// four parts of the report's own stand in the equality as the two it is made of do: any of them turned is
    /// another report.
    /// </summary>
    [Fact]
    public void TwoReadingsOfOneFileAreTwoReportsAndOneListTwiceIsOneReport()
    {
        const string File = """{ "generated": "2026-10-08T04:22:24Z", "deployments": [], "missing": [] }""";
        var one = Read(File);
        var again = Read(File);

        Assert.Equal(one.Generated, again.Generated);
        Assert.Equal(one.Deployments.Count, again.Deployments.Count);
        Assert.Equal(one.Missing, again.Missing);
        Assert.NotEqual(one, again);
        Assert.False(one == again);
        Assert.True(one != again);

        var deployments = new[] { InFlight(), Ended("prod") };
        var hand = new DeploymentsReport(Now, deployments);
        var ofTheSameList = new DeploymentsReport(Now, hand.Deployments);
        var ofAnotherListOfTheSameDeployment = new DeploymentsReport(Now, new[] { deployments[0], deployments[1] });

        Assert.Equal(hand, ofTheSameList);
        Assert.True(hand == ofTheSameList);
        Assert.False(hand != ofTheSameList);
        Assert.True(hand.Equals((object)ofTheSameList));
        Assert.NotSame(hand, ofTheSameList);
        Assert.Equal(hand.GetHashCode(), ofTheSameList.GetHashCode());
        Assert.Single(new HashSet<DeploymentsReport> { hand, ofTheSameList });

        Assert.NotEqual(hand, ofAnotherListOfTheSameDeployment);
        Assert.True(hand != ofAnotherListOfTheSameDeployment);
        Assert.Equal(2, new HashSet<DeploymentsReport> { hand, ofAnotherListOfTheSameDeployment }.Count);

        var keyed = new Dictionary<DeploymentsReport, string> { [hand] = "the reading" };

        keyed[ofTheSameList] = "still the reading";

        Assert.Single(keyed);
        Assert.Equal("still the reading", keyed[ofAnotherListOfTheSameDeployment with { Deployments = deployments }]);

        Assert.NotEqual(hand, hand with { SaysActivity = true });
        Assert.NotEqual(hand, hand with { Missing = ["freezes"] });
        Assert.NotEqual(hand, hand with { Freezes = [new DeploymentFreeze("now", null, Now, true, [], [])] });
        Assert.NotEqual(hand, hand with { Recent = [new RecentDeployment("demo-ui", "uat", "9", DeploymentState.Succeeded, "succeeded", Finished, null, null)] });
        Assert.NotEqual(hand, hand with { Generated = Finished });
    }

    /// <summary>
    /// The report falls apart into its two parts, in the order it was made: the moment first and the list with it,
    /// the very list it holds - and what came out goes back in the same way, to the same report. A report dated
    /// nowhere and carrying nothing falls apart to nothing and nothing.
    /// </summary>
    [Fact]
    public void TheReportFallsApartIntoItsTwoPartsInTheOrderTheyWereMade()
    {
        var deployments = new[] { InFlight(), Ended("prod") };
        var report = new DeploymentsReport(Now, deployments);

        var (generated, listed) = report;

        Assert.Equal(Now, generated);
        Assert.Same(deployments, listed);
        Assert.Equal(2, listed.Count);
        Assert.Equal(report, new DeploymentsReport(generated, listed));

        var (nowhere, none) = new DeploymentsReport(null, deployments);

        Assert.Null(nowhere);
        Assert.Same(deployments, none);

        var (noMoment, noDeployments) = new DeploymentsReport(null, []);

        Assert.Null(noMoment);
        Assert.Empty(noDeployments);
        Assert.NotEqual(report, new DeploymentsReport(noMoment, noDeployments));
    }

    /// <summary>
    /// The words the report says its six parts in are the record's and not the page's: its own name at the front, the
    /// parts in the order they stand in, the three lists as the list each of them is - named by its type and never
    /// opened - and the activity said as the boolean it is. No project, no release and no sentence of a deployment
    /// is in them, which is what lets a log line carry a report without carrying the file; the moment in them follows
    /// whoever reads them, while the page's own words follow the viewer's zone and not the reader's culture.
    /// </summary>
    [Fact]
    public void TheReportSaysItsPartsAndNotWhatItCarries()
    {
        const string Spoken =
            "DeploymentsReport { Generated = 12/31/9999 23:59:59 +00:00, Deployments = Dashboard.Health.Deployment[], "
            + "Recent = Dashboard.Health.RecentDeployment[], Freezes = Dashboard.Health.DeploymentFreeze[], "
            + "Missing = System.String[], SaysActivity = False }";

        var deployments = new[] { InFlight(), Ended("prod") };
        var report = new DeploymentsReport(DateTimeOffset.MaxValue, deployments);

        var missing = new[] { "freezes" };

        Assert.Equal(Spoken, Words(report));
        Assert.StartsWith("DeploymentsReport { Generated = , Deployments = Dashboard.Health.Deployment[], ", Words(new DeploymentsReport(null, deployments)), StringComparison.Ordinal);
        Assert.EndsWith(", SaysActivity = True }", Words(report with { SaysActivity = true }), StringComparison.Ordinal);
        Assert.Contains("Missing = System.String[], ", Words(report with { Missing = missing }), StringComparison.Ordinal);

        foreach (var word in new[] { "demo-ui", "demo-api", "deploying", "failed", "2.4.43", "Not live", " min", "ago" })
        {
            Assert.DoesNotContain(word, Words(report), StringComparison.Ordinal);
        }

        ViewerCulture.German(() =>
        {
            Assert.Contains("Generated = 31.12.9999 23:59:59 +00:00", report.ToString(), StringComparison.Ordinal);
            Assert.Contains("SaysActivity = False", report.ToString(), StringComparison.Ordinal);
        });

        Assert.Contains("Generated = 12/31/9999 23:59:59 +00:00", Words(report), StringComparison.Ordinal);
    }

    /// <summary>
    /// The nothing on either side of the members every record is asked with: a report is not equal to a word, to a
    /// deployment it carries, to a report read from a file or to nothing at all, and two reports that are no report
    /// are one - the equality and the inequality each saying it, from either side.
    /// </summary>
    [Fact]
    public void NoReportIsOneWithNoReportAndNotWithAnyOtherThing()
    {
        var deployments = new[] { InFlight() };
        var there = new DeploymentsReport(Now, deployments);

        Assert.True(there.Equals(there));
        Assert.True(there.Equals((object)there));
        Assert.False(there.Equals((object)"demo-ui"));
        Assert.False(there.Equals((object)deployments));
        Assert.False(there.Equals((object)deployments[0]));
        Assert.False(there.Equals((object)Read("""{ "deployments": [] }""")));
        Assert.False(there.Equals((object)Slug));
        Assert.Null(default(DeploymentsReport));

        DeploymentsReport? nothing = null;
        DeploymentsReport? alsoNothing = null;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there.Equals(nothing));
    }

    // ---------------------------------------------------------------------- the report's own shape

    /// <summary>
    /// The whole public surface of the report, as it stands: the two parts it is made of and the four of its own, the
    /// one length it names, the reading of the file and the marks of an environment - and the members every record is
    /// asked, comparing, hashing, saying itself and falling apart. The type is sealed, each of its six properties has
    /// one getter and one setter of the names the compiler gives them, and a part added to the report has to be
    /// written here as well: the page cannot be shown a part of the file nobody decided what it means.
    /// </summary>
    [Fact]
    public void TheSurfaceIsTheSixPartsTheLengthTheReadingAndTheMarking()
    {
        Assert.Equal(
            ["Deconstruct", "Deployments", "EndedFor", "Equals", "Freezes", "Generated", "GetHashCode", "Marks", "Missing", "Parse", "Recent", "SaysActivity", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.True(typeof(DeploymentsReport).IsSealed);
        Assert.Equal([typeof(IEquatable<DeploymentsReport>)], typeof(DeploymentsReport).GetInterfaces());

        Assert.Equal(
            ["Generated", "Deployments", "Recent", "Freezes", "Missing", "SaysActivity"],
            [.. typeof(DeploymentsReport)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        var properties = typeof(DeploymentsReport)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(
            [typeof(DateTimeOffset?), typeof(IReadOnlyList<Deployment>), typeof(IReadOnlyList<RecentDeployment>),
                typeof(IReadOnlyList<DeploymentFreeze>), typeof(IReadOnlyList<string>), typeof(bool)],
            [.. properties.Values.Select(property => property.PropertyType)]);
        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.CanWrite));
        Assert.All(properties.Values, property => Assert.True(property.GetMethod!.IsPublic));
        Assert.All(properties.Values, property => Assert.NotNull(property.SetMethod));
        Assert.All(properties.Values, property => Assert.False(property.GetMethod!.IsStatic));

        Assert.All(properties.Values, property => Assert.True(property.SetMethod!.IsPublic));
        Assert.All(properties.Values, property => Assert.True(property.SetMethod!.IsSpecialName));
        Assert.All(properties.Values, property => Assert.Equal($"set_{property.Name}", property.SetMethod!.Name));
        Assert.All(properties.Values, property => Assert.Equal($"get_{property.Name}", property.GetMethod!.Name));
        Assert.All(properties.Values, property => Assert.Empty(property.GetIndexParameters()));

        var arguments = typeof(DeploymentsReport).GetConstructors().Single().GetParameters();

        Assert.Equal(["Generated", "Deployments"], [.. arguments.Select(argument => argument.Name!)]);
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));

        var marks = typeof(DeploymentsReport).GetMethod(nameof(DeploymentsReport.Marks))!;

        Assert.False(marks.IsStatic);
        Assert.Equal(typeof(IReadOnlyList<DeploymentMark>), marks.ReturnType);
        Assert.Equal(["environment", "slug", "deployables", "now"], [.. marks.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(IEnumerable<string>), typeof(DateTimeOffset)],
            [.. marks.GetParameters().Select(parameter => parameter.ParameterType)]);

        var parse = typeof(DeploymentsReport).GetMethod(nameof(DeploymentsReport.Parse))!;

        Assert.True(parse.IsStatic);
        Assert.Equal(typeof(DeploymentsReport), parse.ReturnType);
        Assert.Equal([typeof(string)], [.. parse.GetParameters().Select(parameter => parameter.ParameterType)]);

        var deconstruct = typeof(DeploymentsReport).GetMethod(nameof(DeploymentsReport.Deconstruct))!;

        Assert.Equal(
            [typeof(DateTimeOffset?).MakeByRefType(), typeof(IReadOnlyList<Deployment>).MakeByRefType()],
            [.. deconstruct.GetParameters().Select(parameter => parameter.ParameterType)]);
    }

    /// <summary>
    /// The public members the report declares itself: its six properties, its one length as the field it is, the
    /// reading and the marking, and the methods that are not the get and the set of a property - the two operators
    /// kept although the compiler names them specially, since they are the <c>==</c> and the <c>!=</c> a caller
    /// writes - with the record's compiler-named clone and its printing walked past.
    /// </summary>
    private static List<string> PublicSurface() =>
        [.. typeof(DeploymentsReport)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is FieldInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>
    /// The words a report says its six parts in, taken in the invariant culture whatever the reader of this test
    /// happens to be: the record's own words follow whoever reads them, which is pinned on its own further up this
    /// file, so the strings there are the point and the culture only the way to say them once.
    /// </summary>
    private static string Words(DeploymentsReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return report.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
