namespace Dashboard.Tests;

/// <summary>
/// The order the marks come in is a numbering of its own, unrelated to the numbers of
/// <see cref="DeploymentState" /> (<c>#1337</c>): the two order the same states by different rules, and nothing in
/// the page converts between them. <see cref="DeploymentsReport.Rank" /> names the places it hands to the sort and
/// matches a state to its place by name; these tests hold the two numberings apart, so an editor who renumbers
/// either one - the states, or the places between which the unknown state stands - is answered at once.
/// <para>
/// <see cref="DeploymentStateEdgeCaseTests" /> pins the enum's own numbers, names and declaration order,
/// <see cref="DeploymentStateRankEdgeCaseTests" /> and <see cref="DeploymentsTests" /> pin the order the marks come
/// in, and neither says that the two orders are not one order. That is what is set down here: the marks of a report
/// of all seven states do not come in the order the states are declared in, a state's number never decides where its
/// mark stands, and the one state <c>Unknown</c> takes two places of the mark order according to whether the file
/// gives it an end - never according to its number, which is the same for both.
/// </para>
/// <para>
/// Nothing here reaches the network or asks the clock: every moment is one the test supplies, and the marks are read
/// through the page's own <see cref="DeploymentsReport.Marks" />, since the places are internal to the page.
/// </para>
/// </summary>
public class DeploymentMarkOrderEdgeCaseTests
{
    /// <summary>Thursday a few minutes past four, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 4, 23, 24, TimeSpan.Zero);

    /// <summary>Three minutes before the page is asked: when a deployment in flight started.</summary>
    private static readonly DateTimeOffset Since = new(2026, 10, 8, 4, 20, 7, TimeSpan.Zero);

    /// <summary>Five minutes before the page is asked: when a deployment that ended ended.</summary>
    private static readonly DateTimeOffset Ended = new(2026, 10, 8, 4, 18, 0, TimeSpan.Zero);

    private const string Slug = "demo";

    private static readonly string[] Ui = ["ui"];

    /// <summary>A deployment of <paramref name="project" /> to uat in <paramref name="state" />, ended at
    /// <paramref name="finished" /> when it gives one.</summary>
    private static Deployment Of(string project, DeploymentState state, DateTimeOffset? finished) =>
        new(project, "uat", "1", state, "retired", Since, finished, null);

    private static IReadOnlyList<DeploymentMark> Marks(params Deployment[] deployments) =>
        new DeploymentsReport(Now, deployments).Marks("uat", Slug, Ui, Now);

    /// <summary>One deployment in each of the eight places a mark stands, in no order of either numbering: the six
    /// states the page knows and <see cref="DeploymentState.Unknown" /> twice, once with no end and once with one.</summary>
    private static Deployment[] OneInEveryPlace() =>
    [
        Of("demo-canceled", DeploymentState.Canceled, Ended),
        Of("demo-executing", DeploymentState.Executing, null),
        Of("demo-failed", DeploymentState.Failed, Ended),
        Of("demo-queued", DeploymentState.Queued, null),
        Of("demo-succeeded", DeploymentState.Succeeded, Ended),
        Of("demo-unknown-ended", DeploymentState.Unknown, Ended),
        Of("demo-unknown-in-flight", DeploymentState.Unknown, null),
        Of("demo-waiting", DeploymentState.Waiting, null),
    ];

    /// <summary>
    /// The marks of a report that has one deployment in every place do not come in the order the states are declared
    /// in: waiting stands first among the marks though the enum numbers it fourth, queued third though the enum
    /// numbers it second, and the two unknown deployments - one number of the enum, <c>0</c>, the first of the seven -
    /// stand in the fourth place and in the last of the eight. Sorting the very same deployments by their state gives
    /// the enum's order, and that order is not the marks' order.
    /// </summary>
    [Fact]
    public void TheMarksDoNotComeInTheOrderTheStatesAreDeclaredIn()
    {
        var deployments = OneInEveryPlace();

        var marks = Marks([.. deployments]);

        Assert.Equal(
            [
                "demo-waiting", "demo-executing", "demo-queued", "demo-unknown-in-flight", "demo-failed",
                "demo-canceled", "demo-succeeded", "demo-unknown-ended",
            ],
            marks.Select(mark => mark.Project));
        Assert.Equal(
            [
                "demo-unknown-ended", "demo-unknown-in-flight", "demo-queued", "demo-executing", "demo-waiting",
                "demo-succeeded", "demo-failed", "demo-canceled",
            ],
            deployments.OrderBy(deployment => deployment.State).Select(deployment => deployment.Project));
        Assert.NotEqual(
            deployments.OrderBy(deployment => deployment.State).Select(deployment => deployment.Project).ToArray(),
            marks.Select(mark => mark.Project).ToArray());
    }

    /// <summary>A state's number decides nothing about where its mark stands: every mark of a report of all seven
    /// states stands where the page names its place, and a report of the same deployments numbered the other way
    /// round would give the very same marks in the very same places.</summary>
    [Fact]
    public void AStateStandsWhereItsPlaceIsNamedAndNotWhereItsNumberFalls()
    {
        var marks = Marks(OneInEveryPlace());

        Assert.Equal(
            [
                DeploymentState.Waiting, DeploymentState.Executing, DeploymentState.Queued, DeploymentState.Unknown,
                DeploymentState.Failed, DeploymentState.Canceled, DeploymentState.Succeeded, DeploymentState.Unknown,
            ],
            marks.Select(mark => mark.State));

        // The numbers the states carry, in the order the marks came in: rising and falling as they please, so no
        // reading of them orders the marks.
        Assert.Equal([3, 2, 1, 0, 5, 6, 4, 0], marks.Select(mark => (int)mark.State));
    }

    /// <summary>The one state the page does not know takes two places of the mark order by the file's own saying -
    /// whether it gives the deployment an end - and never by its number, which is the same for both places: an
    /// unknown still in flight stands after queued and before failed, and one that ended after succeeded, last of
    /// the seven marks of this report.</summary>
    /// <param name="hasEnd">Whether the file gives the unknown deployment an end.</param>
    /// <param name="place">Which of the seven marks of this report the unknown stands in.</param>
    [Theory]
    [InlineData(false, 3)]
    [InlineData(true, 6)]
    public void AnUnknownStateTakesItsPlaceByTheEndAndNotByItsNumber(bool hasEnd, int place)
    {
        var six = OneInEveryPlace().Where(deployment => deployment.State != DeploymentState.Unknown).ToList();
        six.Add(Of("demo-unknown", DeploymentState.Unknown, hasEnd ? Ended : null));

        var marks = Marks([.. six]);

        Assert.Equal(DeploymentState.Unknown, marks[place].State);
        Assert.Equal("demo-unknown", marks[place].Project);

        // The other six stand where they stood, the unknown with them: in flight among the three that run or wait,
        // ended among the three that finished.
        var inFlight = new bool[7];
        for (var i = 0; i < inFlight.Length; i++)
        {
            inFlight[i] = i < (hasEnd ? 3 : 4);
        }

        Assert.Equal(inFlight, marks.Select(mark => mark.InFlight));
    }
}
