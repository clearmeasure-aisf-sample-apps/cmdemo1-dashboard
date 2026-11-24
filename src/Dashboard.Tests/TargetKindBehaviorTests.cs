namespace Dashboard.Tests;

/// <summary>
/// <see cref="TargetKindBehavior" /> answers once per <see cref="TargetKind" /> what a check of a tile reads and what
/// its events say (#1307): every kind the enum declares has its own answers, so a third kind cannot be added without
/// answering them, and a tile's answers are its kind's.
/// </summary>
public class TargetKindBehaviorTests
{
    private static readonly Uri Url = new("https://app-west.example.net");

    [Fact]
    public void EveryDeclaredKindHasItsOwnAnswersAndNoKindHasTwo()
    {
        Assert.Equal(Enum.GetValues<TargetKind>(), TargetKindBehavior.Declared.Select(behavior => behavior.Kind));
        Assert.All(Enum.GetValues<TargetKind>(), kind =>
        {
            var behavior = TargetKindBehavior.For(kind);
            Assert.Equal(kind, behavior.Kind);
            Assert.Contains(behavior, TargetKindBehavior.Declared);
        });
    }

    [Fact]
    public void TheFrontDoorIsProbedOnlyIsNoNodeAndIsCalledFrontDoor()
    {
        var behavior = TargetKindBehavior.For(TargetKind.FrontDoor);

        Assert.Same(TargetKindBehavior.FrontDoor, behavior);
        Assert.False(behavior.ReadsTelemetry);
        Assert.False(behavior.ReadsHealthDetail);
        Assert.False(behavior.IsNode);
        Assert.Equal(TargetStatus.FrontDoorName, behavior.FixedLabel);
        Assert.Equal("Front Door", behavior.LabelFor("app-west", "westus3"));
    }

    [Fact]
    public void ANodeReadsItsTelemetryAndDetailIsANodeAndIsCalledByItsRegionOrName()
    {
        var behavior = TargetKindBehavior.For(TargetKind.Node);

        Assert.Same(TargetKindBehavior.Node, behavior);
        Assert.True(behavior.ReadsTelemetry);
        Assert.True(behavior.ReadsHealthDetail);
        Assert.True(behavior.IsNode);
        Assert.Null(behavior.FixedLabel);
        Assert.Equal("westus3", behavior.LabelFor("app-west", "westus3"));
        Assert.Equal("app-west", behavior.LabelFor("app-west", null));
    }

    [Fact]
    public void AKindTheEnumDoesNotNameReadsNothingButTheProbeAndIsCalledByItsRegionOrName()
    {
        var undeclared = (TargetKind)2;
        var behavior = TargetKindBehavior.For(undeclared);

        Assert.Equal(undeclared, behavior.Kind);
        Assert.DoesNotContain(behavior, TargetKindBehavior.Declared);
        Assert.False(behavior.ReadsTelemetry);
        Assert.False(behavior.ReadsHealthDetail);
        Assert.False(behavior.IsNode);
        Assert.Equal("westus3", behavior.LabelFor("app-west", "westus3"));
        Assert.Equal("app-west", new TargetStatus(undeclared, "app-west", Url).Label);
    }

    [Fact]
    public void ATileAnswersWithItsKindsBehavior()
    {
        var frontDoor = new TargetStatus(TargetKind.FrontDoor, "anything", new Uri("https://demo.azurefd.net"), "westus3");
        var node = new TargetStatus(TargetKind.Node, "app-west", Url, "westus3", "primary");

        Assert.Same(TargetKindBehavior.FrontDoor, frontDoor.Behavior);
        Assert.Same(TargetKindBehavior.Node, node.Behavior);
        Assert.Equal(TargetStatus.FrontDoorName, frontDoor.Label);
        Assert.Equal("westus3", node.Label);
    }

    [Fact]
    public void TheMonitorAsksTheKindsBehaviorRatherThanComparingKinds()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "DashboardMonitor.cs")));
        var tile = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "TargetStatus.cs")));

        Assert.DoesNotContain("Kind == TargetKind.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Kind == TargetKind.", tile, StringComparison.Ordinal);
    }
}
