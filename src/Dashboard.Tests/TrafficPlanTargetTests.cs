using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="TrafficPlan.Target"/> is the one home of a target's phrase - the deployable at its public host - and
/// <see cref="TrafficPlan.For"/> lists exactly what it says, whichever address wins; <see cref="TrafficPlan.Started"/>
/// joins the targets with the plan's own separator in the order it carries them.
/// </summary>
public class TrafficPlanTargetTests
{
    private static readonly Uri Door = new("https://fd.example.net");
    private static readonly NodeInfo West = new("west", "westus", NodeInfo.StandbyRole, new Uri("https://west.example.net"));
    private static readonly NodeInfo East = new("east", "eastus", NodeInfo.PrimaryRole, new Uri("https://east.example.net"));
    private static readonly NodeInfo North = new("north", "northus", NodeInfo.StandbyRole, new Uri("https://north.example.net"));

    private static DeployableInfo Deployable(string name, Uri? frontDoor, IReadOnlyList<NodeInfo> nodes) =>
        new(name, frontDoor, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, nodes);

    private static string PlanSource() => File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "TrafficPlan.cs")));

    [Fact]
    public void ATargetIsTheDeployableAtItsHost()
    {
        Assert.Equal("ui at uat.example.net", TrafficPlan.Target("ui", "uat.example.net"));
    }

    [Fact]
    public void ThePlanListsTheTargetPhraseForEveryAddressRule()
    {
        var plan = TrafficPlan.For(new EnvironmentInfo("uat", null, [
            Deployable("door", Door, [West, East]),
            Deployable("primary", null, [West, East]),
            Deployable("first", null, [West, North]),
        ]))!;

        Assert.Equal(
            [
                TrafficPlan.Target("door", "fd.example.net"),
                TrafficPlan.Target("primary", "east.example.net"),
                TrafficPlan.Target("first", "west.example.net"),
            ],
            plan.Targets);
    }

    [Fact]
    public void ThreeTargetsAreJoinedWithTheSeparatorInOrder()
    {
        var plan = new TrafficPlan("uat", [new Uri("https://uat.example.net")], ["a at x", "b at y", "c at z"]);

        Assert.Equal(
            "Traffic started: 2 requests a second for 60 s to a at x, b at y, c at z",
            plan.Started(TrafficRun.Default));
    }

    [Fact]
    public void ForBuildsItsTargetsThroughTargetAndThePhraseStandsInOnePlace()
    {
        var source = PlanSource();

        Assert.Contains("targets.Add(Target(", source, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(source, "\" at \""));
        Assert.Single(Regex.Matches(source, @"\{AtWords\}"));
        Assert.DoesNotContain("\" for \"", source.Replace("ForWords = \" for \"", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.DoesNotContain("\" to \"", source.Replace("ToWords = \" to \"", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.DoesNotContain("\", \"", source.Replace("TargetSeparator = \", \"", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
    }
}
