using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeploymentActivity" /> - the words a node of the
/// runtime view and an environment's name carry from the system's deployments file, at the ends the tests of the
/// words as a whole leave open.
/// <para>
/// What is pinned where it belongs: <c>DeploymentActivityTests</c> pins the lines of a node and of an environment,
/// word for word, in the order of their urgency, the kinds in their order, and a moment the same day, a week away
/// and sixteen days away; <c>ClusterDeliveryHelperTests</c> pins a line cut to what a node holds - the cut that
/// fits, the ellipsis, the space the cut falls on, the empty line, and the length below one the cut refuses;
/// <c>DeploymentFreezeTests</c> pins the freeze's clock - its start, its end, and the seventy-two hours before it.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the two lengths the class names, as the fixed values they are; the
/// moment at the changes of day - the one day away, the six days away, and the seven - and the moment the zone
/// given carries across a date the meridian does not; and the arguments the sayers refuse that are not there, each
/// by the name the guard keeps. The environment is asked for in any case, with the lines it has, and a node is
/// refused its report and its zone before it says anything.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class DeploymentWaitEdgeCaseTests
{
    /// <summary>Friday noon, UTC: the moment the page is asked, and every day below is counted from it.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    /// <summary>A file that says everything a file may say, about nothing: all the lists, all of them empty.</summary>
    private static readonly DeploymentsReport Empty =
        DeploymentsReport.Parse("""{ "deployments": [], "recent": [], "freezes": [], "missing": [] }""")!;

    /// <summary>A file with one freeze coming and one deployment ended: enough for an environment to say two lines.</summary>
    private static readonly DeploymentsReport Small = DeploymentsReport.Parse("""
        { "deployments": [],
          "recent": [ { "project": "demo-ui", "release": "2.4.40", "environment": "prod", "result": "succeeded", "finished": "2026-10-01T15:00:00Z" } ],
          "freezes": [ { "name": "weekend", "from": "2026-10-10T19:00:00Z", "to": "2026-10-11T19:00:00Z", "active": false, "environments": [ "prod" ], "projects": [ "demo-ui" ] } ],
          "missing": [] }
        """)!;

    // ---------------------------------------------------------- the class's own lengths, fixed

    /// <summary>The two lengths the class names, as the fixed values they are: how many lines a node carries and
    /// how many characters a line of it holds.</summary>
    [Fact]
    public void TheNodeHoldsTwoLinesAndThirtySixCharacters()
    {
        Assert.Equal(2, DeploymentActivity.MostLines);
        Assert.Equal(36, DeploymentActivity.NodeLength);
    }

    // ------------------------------------------------------------ a moment, at the changes of day

    /// <summary>The moment at the changes of day takes its words at each change: the one the same day, the one
    /// day away with its day of the week, the six days away that the day of the week still holds, and the seven
    /// days away, forward and back, that the date takes - where the one day before and the one day after the week
    /// the day of the week covers are said in the words the week does not say.</summary>
    [Theory]
    [InlineData(0, "12:00")]
    [InlineData(1, "Sat 12:00")]
    [InlineData(-1, "Thu 12:00")]
    [InlineData(6, "Thu 12:00")]
    [InlineData(-6, "Sat 12:00")]
    [InlineData(7, "16 Oct 12:00")]
    [InlineData(-7, "2 Oct 12:00")]
    public void TheMomentAtTheChangesOfDaySaysTheDayOrTheDate(int days, string expected) =>
        Assert.Equal(expected, DeploymentActivity.When(Now.AddDays(days), Now, Utc));

    /// <summary>The day is counted in the zone the moment is given: the instant two o'clock the next day on the
    /// meridian is a day away there, while five hours west of it the same instant is the evening of the same day,
    /// and the moment takes the short words the same day holds.</summary>
    [Fact]
    public void TheMomentIsCountedInTheZoneGivenNotInTheOneItWasWritten()
    {
        var instant = new DateTimeOffset(2026, 10, 10, 2, 0, 0, TimeSpan.Zero);
        var chicago = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

        Assert.Equal("Sat 02:00", DeploymentActivity.When(instant, Now, Utc));
        Assert.Equal("21:00", DeploymentActivity.When(instant, Now, chicago));
    }

    /// <summary>The moment refuses a zone that is not there, by the name the guard keeps: the day it would count
    /// in is not there either.</summary>
    [Fact]
    public void TheMomentRefusesAZoneThatIsNotThere() =>
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => DeploymentActivity.When(Now, Now, null!)).ParamName);

    // ------------------------------------------------------------- the sayers, and what they refuse

    /// <summary>The lines of a node refuse a file that is not there and a zone that is not there, each by the
    /// name the guard keeps, before they say anything: even the file that says everything a file may say, about
    /// nothing, needs a zone it would say the moments in.</summary>
    [Fact]
    public void TheNodeRefusesAReportOrAZoneThatIsNotThere()
    {
        var noReport = Assert.Throws<ArgumentNullException>(() => DeploymentActivity.OfNode(null!, "uat", "demo", "ui", Now, Utc));

        Assert.Equal("report", noReport.ParamName);

        var noZone = Assert.Throws<ArgumentNullException>(() => DeploymentActivity.OfNode(Empty, "uat", "demo", "ui", Now, null!));

        Assert.Equal("zone", noZone.ParamName);
    }

    /// <summary>The lines under an environment's name refuse a file, the marks and a zone that are not there,
    /// each by the name the guard keeps, in the order the lines are asked for them.</summary>
    [Fact]
    public void TheEnvironmentRefusesAFileTheMarksOrAZoneThatIsNotThere()
    {
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => DeploymentActivity.OfEnvironment(null!, "uat", [], Now, Utc)).ParamName);
        Assert.Equal("marks", Assert.Throws<ArgumentNullException>(() => DeploymentActivity.OfEnvironment(Empty, "uat", null!, Now, Utc)).ParamName);
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => DeploymentActivity.OfEnvironment(Empty, "uat", [], Now, null!)).ParamName);
    }

    /// <summary>The environment is asked for in any case: the lines it has are the same in upper case as in the
    /// case the file writes them in, and the environment the file says nothing about is empty in any case too.</summary>
    [Fact]
    public void TheEnvironmentIsAskedForInAnyCaseAndIsEmptyWhereTheFileHasNothing()
    {
        var upper = DeploymentActivity.OfEnvironment(Small, "PROD", [], Now, Utc);
        var asWritten = DeploymentActivity.OfEnvironment(Small, "prod", [], Now, Utc);

        Assert.Equal(
            [
                ("freeze", "deployment freeze weekend: demo-ui frozen from Sat 19:00 until Sun 19:00", (Uri?)null),
                ("deployed", "last: demo-ui 2.4.40 reached prod 7 d ago", (Uri?)null),
            ],
            upper.Select(line => (line.Kind, line.Text, line.Url)));
        Assert.Equal(
            asWritten.Select(line => (line.Kind, line.Text, line.Url, line.Title)),
            upper.Select(line => (line.Kind, line.Text, line.Url, line.Title)));
        Assert.Empty(DeploymentActivity.OfEnvironment(Small, "TDD", [], Now, Utc));
    }

    /// <summary>The line that is cut refuses a text that is not there, by the name the guard keeps: the cut to
    /// what a node holds is the guard's, and the name it keeps is the text's.</summary>
    [Fact]
    public void TheLineThatIsNotThereIsRefusedByTheNameTheGuardKeeps() =>
        Assert.Equal("text", Assert.Throws<ArgumentNullException>(() => DeploymentActivity.Cut(null!, 10)).ParamName);

    // --------------------------------------------------------------- the class's own shape

    /// <summary>The whole public surface of the words a node and an environment carry, as it stands: the two
    /// lengths, and the four sayers - the lines of a node, the lines of an environment, the moment, and the cut
    /// the line of a node takes - and nothing else to call. A member added to the class has to be written here as
    /// well, which is the point: the page cannot be shown a word nobody decided what it means.</summary>
    [Fact]
    public void TheSurfaceIsTheTwoLengthsAndTheFourSayers() =>
        Assert.Equal(
            ["Cut", "MostLines", "NodeLength", "OfEnvironment", "OfNode", "When"],
            PublicSurface());

    /// <summary>The public members the class declares itself: the two constants as the fields they are and the
    /// four static sayers, in the order the names are said, and nothing else to call.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(DeploymentActivity)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
