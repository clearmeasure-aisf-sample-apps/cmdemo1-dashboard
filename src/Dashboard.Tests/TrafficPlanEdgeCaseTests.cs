namespace Dashboard.Tests;

/// <summary>
/// Boundary, null/empty and fixed-value tests for the public members of <see cref="TrafficPlan"/> whose behavior the
/// existing tests leave open (#1045): the start page as the default path, the fixed check interval, the guards of
/// <see cref="TrafficPlan.Choose"/> and <see cref="TrafficPlan.For"/>, the extremes of
/// <see cref="TrafficPlan.IntervalNote"/>, and how <see cref="TrafficPlan.For"/> joins and orders what it calls.
/// </summary>
public class TrafficPlanEdgeCaseTests
{
    private static readonly NodeInfo Primary = new("east", "eastus", NodeInfo.PrimaryRole, new Uri("https://east.example.net"));

    private static DeployableInfo Deployable(string name, Uri? frontDoor, IReadOnlyList<NodeInfo> nodes, IReadOnlyList<string>? paths = null) =>
        new(name, frontDoor, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, nodes, TrafficPaths: paths);

    private static TrafficPlan Plan(string name) => new(name, [new Uri($"https://{name}.example.net/")], [$"ui at {name}.example.net"]);

    // ---------------------------------------------------------------- fixed values

    [Fact]
    public void TheDefaultPathsAreTheStartPageAlone() =>
        Assert.Equal(["/"], TrafficPlan.DefaultPaths);

    [Fact]
    public void TheCheckIntervalIsTenWholeSeconds()
    {
        Assert.Equal(10, TrafficPlan.CheckEverySeconds);
        Assert.Equal(TimeSpan.FromSeconds(10), TrafficPlan.CheckEvery);
    }

    [Fact]
    public void ThePlansNumbersAreTheRunsOwn()
    {
        Assert.Equal(TrafficRun.Values.AnswerWithinSeconds, TrafficPlan.AnswerWithinSeconds);
    }

    [Fact]
    public void TheRecordKeepsWhatItWasGiven()
    {
        IReadOnlyList<Uri> addresses = [new Uri("https://a.example.net/")];
        IReadOnlyList<string> targets = ["ui at a.example.net"];

        var plan = new TrafficPlan("uat", addresses, targets);

        Assert.Equal("uat", plan.Environment);
        Assert.Same(addresses, plan.Addresses);
        Assert.Same(targets, plan.Targets);
    }

    // ---------------------------------------------------------------- Choose

    [Fact]
    public void ChooseRejectsNullPlans() =>
        Assert.Throws<ArgumentNullException>("plans", () => TrafficPlan.Choose(null!, null, null, chosenByViewer: false, running: false));

    [Fact]
    public void WhileTrafficRunsTheChoiceStaysEvenWhenItIsNullOrUnknown()
    {
        IReadOnlyList<TrafficPlan> plans = [Plan("tdd"), Plan("uat")];

        Assert.Null(TrafficPlan.Choose(plans, null, "uat", chosenByViewer: false, running: true));
        Assert.Equal("gone", TrafficPlan.Choose(plans, "gone", "uat", chosenByViewer: true, running: true));
        Assert.Null(TrafficPlan.Choose([], null, null, chosenByViewer: false, running: true));
    }

    [Fact]
    public void AViewersChoiceIsReturnedInThePlansOwnSpelling() =>
        Assert.Equal("uat", TrafficPlan.Choose([Plan("tdd"), Plan("uat")], "UAT", null, chosenByViewer: true, running: false));

    [Fact]
    public void AViewersNullChoiceFallsBackToTheShownEnvironment() =>
        Assert.Equal("uat", TrafficPlan.Choose([Plan("tdd"), Plan("uat")], null, "uat", chosenByViewer: true, running: false));

    [Fact]
    public void ASinglePlanIsTheChoiceWhateverIsShown() =>
        Assert.Equal("prod", TrafficPlan.Choose([Plan("prod")], "x", "y", chosenByViewer: true, running: false));

    [Fact]
    public void WithoutPlansAndNotRunningThereIsNoChoiceEvenForAViewersPick() =>
        Assert.Null(TrafficPlan.Choose([], "uat", "uat", chosenByViewer: true, running: false));

    [Fact]
    public void AnEmptyShownNameIsNoEnvironment() =>
        Assert.Equal("tdd", TrafficPlan.Choose([Plan("tdd"), Plan("uat")], null, string.Empty, chosenByViewer: false, running: false));

    // ---------------------------------------------------------------- IntervalNote

    [Fact]
    public void AZeroOrNegativeIntervalGetsNoNote()
    {
        Assert.Null(TrafficPlan.IntervalNote(running: true, TimeSpan.Zero));
        Assert.Null(TrafficPlan.IntervalNote(running: true, TimeSpan.FromSeconds(-30)));
        Assert.Null(TrafficPlan.IntervalNote(running: true, TimeSpan.MinValue));
    }

    [Fact]
    public void TheNoteStandsOneTickAboveTheRuleAndAtTheLongestInterval()
    {
        Assert.Equal("every 10 s while traffic runs", TrafficPlan.IntervalNote(running: true, TrafficPlan.CheckEvery + TimeSpan.FromTicks(1)));
        Assert.Equal("every 10 s while traffic runs", TrafficPlan.IntervalNote(running: true, TimeSpan.MaxValue));
        Assert.Null(TrafficPlan.IntervalNote(running: false, TimeSpan.MaxValue));
    }

    // ---------------------------------------------------------------- For

    [Fact]
    public void ForRejectsANullEnvironment() =>
        Assert.Throws<ArgumentNullException>("environment", () => TrafficPlan.For(null!));

    [Fact]
    public void AnEnvironmentWithoutDeployablesHasNoPlan() =>
        Assert.Null(TrafficPlan.For(new EnvironmentInfo("uat", null, [])));

    [Fact]
    public void ThePlanCarriesTheEnvironmentsNameAsWritten()
    {
        var plan = TrafficPlan.For(new EnvironmentInfo("UAT-West", "uat", [Deployable("ui", null, [Primary])]))!;

        Assert.Equal("UAT-West", plan.Environment);
    }

    [Fact]
    public void WithoutTrafficPathsTheStartPageIsCalledOnce()
    {
        var plan = TrafficPlan.For(new EnvironmentInfo("uat", null, [Deployable("ui", null, [Primary])]))!;

        Assert.Equal(["https://east.example.net/"], plan.Addresses.Select(address => address.AbsoluteUri));
    }

    [Fact]
    public void PathsAreJoinedWithOneSlashOntoAFrontDoorThatHasAPath()
    {
        var door = new Uri("https://fd.example.net/app/");
        var plan = TrafficPlan.For(new EnvironmentInfo("uat", null, [Deployable("ui", door, [], ["api/summary", "/health"])]))!;

        Assert.Equal(
            ["https://fd.example.net/app/api/summary", "https://fd.example.net/app/health"],
            plan.Addresses.Select(address => address.AbsoluteUri));
        Assert.Equal(["ui at fd.example.net"], plan.Targets);
    }

    [Fact]
    public void ARepeatedPathIsCalledAsOftenAsItIsListed()
    {
        var plan = TrafficPlan.For(new EnvironmentInfo("uat", null, [Deployable("ui", null, [Primary], ["/", "/"])]))!;

        Assert.Equal(2, plan.Addresses.Count);
        Assert.Single(plan.Targets);
    }

    [Fact]
    public void DeployablesAreCalledInTopologyOrderWithOneTargetEach()
    {
        var plan = TrafficPlan.For(new EnvironmentInfo("uat", null,
        [
            Deployable("api", new Uri("https://api.example.net"), [], ["/a", "/b"]),
            Deployable("ui", null, [Primary]),
        ]))!;

        Assert.Equal(
            ["https://api.example.net/a", "https://api.example.net/b", "https://east.example.net/"],
            plan.Addresses.Select(address => address.AbsoluteUri));
        Assert.Equal(["api at api.example.net", "ui at east.example.net"], plan.Targets);
    }

    [Fact]
    public void AFrontDoorAloneWithoutNodesIsEnoughToCall()
    {
        var plan = TrafficPlan.For(new EnvironmentInfo("uat", null, [Deployable("ui", new Uri("https://fd.example.net"), [])]))!;

        Assert.Equal(["https://fd.example.net/"], plan.Addresses.Select(address => address.AbsoluteUri));
    }
}
