namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeploymentActivity" /> at the ends the tests of the
/// words as a whole leave open: <see cref="DeploymentActivityTests" /> pins the lines of a node and of an
/// environment, word for word, in the order of their urgency, and a moment the same day, a week away and sixteen
/// days away; the cut to what a node holds is pinned in <see cref="ClusterDeliveryHelperTests" /> - the cut that
/// fits, the ellipsis, the space it leaves behind, and the length below one it refuses - and the class's two
/// lengths, and the arguments the sayers refuse, stand pinned in the edge tests beside this one.
/// <para>
/// Left open by all of that, and pinned here: a deployment the file gives no time for, said without an age in its
/// four words; the file's order where two lines are of one urgency; the names a node or an environment is asked
/// for that are not there - said nothing, and nothing thrown; the freezes, said in the order they cover - the in
/// force before the one that comes, the one without a start before the one with the earlier one, whatever the
/// file's order - and counted, with every one in the tooltip; a mark that is in flight, which is no mark of the
/// ended line and does not keep it unsaid, and another release in flight, which is no mark of this one at all; a
/// freeze that names no projects, naming the system; the last deployment with a state the page does not know, in
/// the file's own word; the cut, that counts characters and knows no emoji, and the blanks it trims from behind
/// the ellipsis; and the moment, said in the invariant words whatever the viewer's culture.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class DeploymentActivityEdgeCaseTests
{
    /// <summary>Friday noon, UTC: the moment the page is asked, and every day below is counted from it.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static readonly string[] Ui = ["ui"];

    private const string Slug = "demo";

    /// <summary>The words every tooltip of a freeze ends with, said once and joined on to its title.</summary>
    private const string FreezeEnd = " A freeze belongs to the Octopus Deploy instance; a person with the right may override it for one deployment.";

    private static DeploymentsReport Parse(string deployments = "", string recent = "", string freezes = "") =>
        DeploymentsReport.Parse($$"""{ "deployments": [ {{deployments}} ], "recent": [ {{recent}} ], "freezes": [ {{freezes}} ], "missing": [] }""")!;

    private static IReadOnlyList<ActivityLine> Node(DeploymentsReport report, string environment, string deployable = "ui", DateTimeOffset? now = null) =>
        DeploymentActivity.OfNode(report, environment, Slug, deployable, now ?? Now, Utc);

    // ------------------------------------------------- the file gives no time, and the lines say no age

    /// <summary>A deployment the file gives no time for - none at all, or one it cannot read - is said without an
    /// age, in the four words the four in-flight states say it in: the sign-off without the how long, the deploying
    /// without the how far, the queued without the how long, and the state the page does not know, in the file's own
    /// word. The title has the sentence and nothing more: there is no time the how long would count from, and the
    /// page says none it does not have.</summary>
    [Theory]
    [InlineData("""{ "project": "demo-ui", "environment": "prod", "release": "2.4.43", "state": "waiting", "waitsFor": { "kind": "sign-off" } }""", "waiting", "2.4.43 waits for a sign-off", "demo-ui 2.4.43 waits for a sign-off in prod")]
    [InlineData("""{ "project": "demo-ui", "environment": "prod", "release": "2.4.43", "state": "executing" }""", "deploying", "deploying 2.4.43", "deploying demo-ui 2.4.43 to prod")]
    [InlineData("""{ "project": "demo-ui", "environment": "prod", "release": "2.4.43", "state": "queued" }""", "queued", "2.4.43 queued", "demo-ui 2.4.43 is queued for prod")]
    [InlineData("""{ "project": "demo-ui", "environment": "prod", "release": "2.4.43", "state": "paused" }""", "queued", "2.4.43: paused", "demo-ui 2.4.43 in prod: paused")]
    public void ADeploymentTheFileGivesNoTimeForIsSaidWithoutAnAge(string deployment, string kind, string text, string title)
    {
        var line = Assert.Single(Node(Parse(deployment), "prod"));

        Assert.Equal((kind, text, title, (Uri?)null), (line.Kind, line.Text, line.Title, line.Url));
    }

    /// <summary>Two deployments of one urgency are said in the file's order, not the order of their times: the one
    /// queued longer is not more urgent, and the node keeps the order the file wrote them in.</summary>
    [Fact]
    public void OfTheLinesOfOneUrgencyTheFilesOrderStands()
    {
        var report = Parse("""
            { "project": "demo-ui", "environment": "uat", "release": "2", "state": "queued", "since": "2026-10-09T11:50:00Z" },
            { "project": "demo-ui", "environment": "uat", "release": "1", "state": "queued", "since": "2026-10-09T11:55:00Z" }
            """);

        Assert.Equal(["2 queued, 10 min", "1 queued, 5 min"], Node(report, "uat").Select(line => line.Text));
    }

    // ------------------------------------------------------- who started it, said one way

    /// <summary>Who started a deployment is said in the words each line leads to the name with - <c>, by</c> on the
    /// node of a deployment that is executing, <c> by</c> on the node of the last one that ended, <c>, started
    /// by</c> under the environment's name and in the tooltip of the last one - and, where the file does not say,
    /// nothing at all: no leading words left behind without a name.</summary>
    [Theory]
    [InlineData(""", "startedBy": "pat" """, ", by pat", " by pat", ", started by pat")]
    [InlineData("", "", "", "")]
    public void WhoStartedADeploymentIsSaidAfterItsWordsOrNotAtAll(string startedBy, string executing, string ended, string environment)
    {
        var inFlight = Parse(deployments: $$"""{ "project": "demo-ui", "environment": "prod", "release": "2.4.44", "state": "executing", "since": "2026-10-09T11:57:00Z"{{startedBy}} }""");
        Assert.Equal($"deploying 2.4.44, 3 min{executing}", Assert.Single(Node(inFlight, "prod")).Text);

        var last = Parse(recent: $$"""{ "project": "demo-ui", "release": "2.4.43", "environment": "uat", "result": "succeeded", "finished": "2026-10-09T07:00:00Z"{{startedBy}} }""");
        Assert.Equal($"2.4.43 deployed 5 h ago{ended}", Assert.Single(Node(last, "uat")).Text);

        var line = Assert.Single(DeploymentActivity.OfEnvironment(last, "uat", [], Now, Utc));
        Assert.Equal($"last: demo-ui 2.4.43 reached uat 5 h ago{environment}", line.Text);
        Assert.Equal($"The last deployment of demo-ui to uat that ended: demo-ui 2.4.43 reached uat 5 h ago (2026-10-09 07:00 +00:00){environment}.", line.Title);
    }

    // ------------------------------------------------------- the names that are not there

    /// <summary>The environment, the slug and the deployable a node is asked for are no guard's: they may be not
    /// there, and may be empty, and the node says nothing rather than throw - where the file says something about
    /// another name. The environment an environment's lines are asked for stands the same: no lines where the name
    /// is not there, and no lines thrown for it either.</summary>
    [Fact]
    public void ANameThatIsNotThereSaysNothingAndThrowsNothing()
    {
        var report = Parse(deployments: """{ "project": "demo-ui", "environment": "prod", "release": "2.4.43", "state": "executing", "since": "2026-10-09T11:57:00Z" }""");

        // Where the name is, the line is: the report says something about demo-ui in prod.
        Assert.Equal("deploying 2.4.43, 3 min", Assert.Single(Node(report, "prod")).Text);

        // Where it is not - not there, or empty, in any of the three places - nothing is said.
        Assert.Empty(DeploymentActivity.OfNode(report, null!, null!, null!, Now, Utc));
        Assert.Empty(DeploymentActivity.OfNode(report, "", "demo", "ui", Now, Utc));
        Assert.Empty(DeploymentActivity.OfNode(report, "prod", "", "", Now, Utc));
        Assert.Empty(DeploymentActivity.OfEnvironment(report, null!, [], Now, Utc));
        Assert.Empty(DeploymentActivity.OfEnvironment(report, "", [], Now, Utc));
    }

    // ----------------------------------------------------------------- the order the freezes are said in

    /// <summary>The freezes an environment is told of are said in the order they cover it, not the order the file
    /// writes them in: the ones in force before the one that comes, and of the in force the one without a start
    /// before the one with the earlier one - the file's order writing them the other way round. The first is said in
    /// the line, with how many more there are, and every one is said in the tooltip, in the same order, the one
    /// without a start without its from, and the one that comes with how long it is.</summary>
    [Fact]
    public void TheFreezesAreSaidInTheOrderTheyCoverNotInTheOrderTheFileWritesThem()
    {
        // The file writes the coming one first, the in force with the later start next, and the one without a start
        // last: the reverse of the order they are said in.
        var report = Parse(freezes: """
            { "name": "charlie", "from": "2026-10-10T19:00:00Z", "to": "2026-10-11T19:00:00Z", "active": false, "environments": [ "prod" ], "projects": [ "demo-ui" ] },
            { "name": "bravo", "from": "2026-10-09T00:00:00Z", "to": "2026-10-10T00:00:00Z", "active": true, "environments": [ "prod" ], "projects": [ "demo-ui" ] },
            { "name": "alpha", "to": "2026-10-12T00:00:00Z", "active": true, "environments": [ "prod" ], "projects": [ "demo-ui" ] }
            """);

        Assert.Equal(3, report.Freezes.Count);

        var line = Assert.Single(DeploymentActivity.OfEnvironment(report, "prod", [], Now, Utc));

        Assert.Equal(("frozen", "deployment freeze alpha: demo-ui frozen until Mon 00:00, and 2 more", (Uri?)null), (line.Kind, line.Text, line.Url));
        Assert.Equal(
            "Deployment freeze alpha: no deployment of demo-ui to prod until 2026-10-12 00:00 +00:00. In force now." + FreezeEnd + "\n"
            + "Deployment freeze bravo: no deployment of demo-ui to prod from 2026-10-09 00:00 +00:00 until 2026-10-10 00:00 +00:00. In force now." + FreezeEnd + "\n"
            + "Deployment freeze charlie: no deployment of demo-ui to prod from 2026-10-10 19:00 +00:00 until 2026-10-11 19:00 +00:00. Begins in 31 h." + FreezeEnd,
            line.Title);

        // The node carries the two it may carry, in the same order: the coming one is the third, and is not carried.
        Assert.Equal(DeploymentActivity.MostLines, Node(report, "prod").Count);
        Assert.Equal(
            [("frozen", "frozen until Mon 00:00: alpha"), ("frozen", "frozen until Sat 00:00: bravo")],
            Node(report, "prod").Select(line => (line.Kind, line.Text)));
    }

    // ------------------------------------------------- the mark that is in flight, and the ended line

    /// <summary>The ended line is kept unsaid only by a mark that has ended: the mark of the deployment that is
    /// still executing the very release the line would say is in flight, and is no mark of the ended line, which is
    /// said. The same release ended within the mark's ten minutes is the ended line's mark, and keeps it unsaid;
    /// another release in flight is no mark of it at all, and it is said.</summary>
    [Fact]
    public void AMarkThatIsInFlightDoesNotKeepTheEndedLineUnsaid()
    {
        const string Recent = """{ "project": "demo-ui", "release": "2.4.43", "environment": "uat", "result": "succeeded", "finished": "2026-10-09T07:00:00Z" }""";

        // The same release executing now: its mark is in flight, and the line of the one that ended is said.
        var redeployed = Parse(deployments: """{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "state": "executing", "since": "2026-10-09T11:57:00Z" }""", recent: Recent);
        Assert.True(Assert.Single(redeployed.Marks("uat", Slug, Ui, Now)).InFlight);

        var line = Assert.Single(DeploymentActivity.OfEnvironment(redeployed, "uat", redeployed.Marks("uat", Slug, Ui, Now), Now, Utc));

        Assert.Equal(("deployed", "last: demo-ui 2.4.43 reached uat 5 h ago", (Uri?)null), (line.Kind, line.Text, line.Url));
        Assert.Equal("The last deployment of demo-ui to uat that ended: demo-ui 2.4.43 reached uat 5 h ago (2026-10-09 07:00 +00:00).", line.Title);

        // The same release failed five minutes ago: its mark has ended, and says the line in its place.
        var ended = Parse(deployments: """{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "state": "failed", "since": "2026-10-09T11:40:00Z", "finished": "2026-10-09T11:55:00Z" }""", recent: Recent);
        Assert.False(Assert.Single(ended.Marks("uat", Slug, Ui, Now)).InFlight);
        Assert.Empty(DeploymentActivity.OfEnvironment(ended, "uat", ended.Marks("uat", Slug, Ui, Now), Now, Utc));

        // Another release executing is no mark of this one: the line is said.
        var otherRelease = Parse(deployments: """{ "project": "demo-ui", "environment": "uat", "release": "2.4.44", "state": "executing", "since": "2026-10-09T11:57:00Z" }""", recent: Recent);
        Assert.Equal(
            "last: demo-ui 2.4.43 reached uat 5 h ago",
            Assert.Single(DeploymentActivity.OfEnvironment(otherRelease, "uat", otherRelease.Marks("uat", Slug, Ui, Now), Now, Utc)).Text);
    }

    /// <summary>The last deployment with a state the page does not know is said in the file's own word, with the
    /// quiet icon's kind: the result is the state it is not, and the word the file wrote is what the line says.</summary>
    [Fact]
    public void ALastDeploymentWithAStateThePageDoesNotKnowIsSaidInTheFilesWord()
    {
        var report = Parse(recent: """{ "project": "demo-api", "release": "0.9.0", "environment": "uat", "result": "timed out", "finished": "2026-10-09T11:00:00Z" }""");

        var line = Assert.Single(DeploymentActivity.OfEnvironment(report, "uat", [], Now, Utc));

        Assert.Equal(("canceled", "last: demo-api 0.9.0 in uat: timed out, 1 h ago", (Uri?)null), (line.Kind, line.Text, line.Url));
        Assert.Equal("The last deployment of demo-api to uat that ended: demo-api 0.9.0 in uat: timed out, 1 h ago (2026-10-09 11:00 +00:00).", line.Title);
    }

    /// <summary>A freeze that names no projects covers the environment as a whole and names the system: what the
    /// line under the environment's name says of it, and the tooltip alike, is the system, when it names no one. The
    /// node asks for it on behalf of its own project, and the freeze names no project: of a node it says nothing.</summary>
    [Fact]
    public void AFreezeThatNamesNoProjectsNamesTheSystem()
    {
        var report = Parse(freezes: """{ "name": "all", "to": "2026-10-12T00:00:00Z", "active": true, "environments": [ "prod" ], "projects": [ ] }""");

        var line = Assert.Single(DeploymentActivity.OfEnvironment(report, "prod", [], Now, Utc));

        Assert.Equal(("frozen", "deployment freeze all: the system frozen until Mon 00:00", (Uri?)null), (line.Kind, line.Text, line.Url));
        Assert.Equal("Deployment freeze all: no deployment of the system to prod until 2026-10-12 00:00 +00:00. In force now." + FreezeEnd, line.Title);

        // The node asks for its own project, the ui's and the system's alike, and the freeze names none of them.
        Assert.Empty(Node(report, "prod"));
        Assert.Empty(Node(report, "prod", "system"));
    }

    // ----------------------------------------------------------------- the cut, and its characters

    /// <summary>The cut counts characters, and knows no emoji: an emoji is two characters, so a line of two holds
    /// it whole, a line of three holds one of it and the ellipsis, and a cut in the middle of it leaves the half it
    /// fell on - the code unit and no more, since the cut counts UTF-16 code units and the grapheme is not one of
    /// them.</summary>
    [Fact]
    public void TheCutCountsCharactersAndKnowsNoEmoji()
    {
        Assert.Equal("🚀", DeploymentActivity.Cut("🚀", 2));
        Assert.Equal("🚀…", DeploymentActivity.Cut("🚀🚀", 3));
        Assert.Equal(3, DeploymentActivity.Cut("🚀🚀", 3).Length);
        Assert.Equal("a…", DeploymentActivity.Cut("a🚀", 2));

        // The cut in the middle of the emoji leaves the half it fell on, and the ellipsis: two characters, the line's length.
        var half = DeploymentActivity.Cut("🚀a", 2);

        Assert.Equal("\uD83D…", half);
        Assert.Equal(2, half.Length);
    }

    /// <summary>The cut trims every blank it leaves behind the ellipsis, not only the one it falls on: the blanks
    /// the cut keeps end with the ellipsis taking them all, and the line comes out shorter than the length it was
    /// cut to, the blanks gone. The tab is a blank too, and goes with them.</summary>
    [Fact]
    public void TheCutTrimsEveryBlankItLeavesBehind()
    {
        var cut = DeploymentActivity.Cut("abcd     ", 8);

        Assert.Equal("abcd…", cut);
        Assert.Equal(5, cut.Length);

        Assert.Equal("abcd…", DeploymentActivity.Cut("abcd \t ", 6));
    }

    // ------------------------------------------------------------- the moment, and the viewer's culture

    /// <summary>The moment is said in the invariant words whatever the viewer's culture: the day of the week, the
    /// month and the order of them stand as the page writes them for whoever reads, and the German reader is not
    /// told <c>Mo.</c> for Monday or <c>Okt.</c> for October, since the zone the moment is said in is the argument's
    /// and not the reader's.</summary>
    [Fact]
    public void TheMomentIsSaidInTheInvariantWordsWhateverTheViewersCulture()
    {
        var monday = new DateTimeOffset(2026, 10, 12, 0, 0, 0, TimeSpan.Zero);
        var sixteenDays = new DateTimeOffset(2026, 10, 25, 0, 0, 0, TimeSpan.Zero);

        ViewerCulture.German(() =>
        {
            Assert.Equal("Mon 00:00", DeploymentActivity.When(monday, Now, Utc));
            Assert.Equal("25 Oct 00:00", DeploymentActivity.When(sixteenDays, Now, Utc));
        });

        Assert.Equal("Mon 00:00", DeploymentActivity.When(monday, Now, Utc));
        Assert.Equal("25 Oct 00:00", DeploymentActivity.When(sixteenDays, Now, Utc));
    }
}
