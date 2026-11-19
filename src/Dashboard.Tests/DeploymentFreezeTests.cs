namespace Dashboard.Tests;

/// <summary>
/// When a deployment freeze of the Octopus Deploy instance is in force, by the browser's clock
/// (<see cref="DeploymentFreeze.InForceAt"/>), and how far ahead of its start a freeze is named
/// (<see cref="DeploymentFreeze.Ahead"/>).
/// </summary>
public class DeploymentFreezeTests
{
    /// <summary>The weekend freeze: it begins Sat 19:00 and ends Sun 19:00, UTC.</summary>
    private static readonly DateTimeOffset From = new(2026, 10, 10, 19, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 10, 11, 19, 0, 0, TimeSpan.Zero);

    private static DeploymentFreeze Freeze(DateTimeOffset? from, bool active) =>
        new("weekend", from, To, active, ["prod"], ["demo-ui"]);

    [Fact]
    public void AFreezeIsInForceFromItsStartTillItsEnd()
    {
        var freeze = Freeze(From, active: false);

        // Before it begins it is not in force; at its start it is; and just before its end it is still.
        Assert.False(freeze.InForceAt(From - TimeSpan.FromSeconds(1)));
        Assert.True(freeze.InForceAt(From));
        Assert.True(freeze.InForceAt(To - TimeSpan.FromSeconds(1)));

        // At its end it is over, and after its end it is not.
        Assert.False(freeze.InForceAt(To));
        Assert.False(freeze.InForceAt(To + TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void WithoutAStartTheFilesOwnWordDecides()
    {
        var active = Freeze(null, active: true);
        var notActive = Freeze(null, active: false);
        var beforeEnd = To - TimeSpan.FromSeconds(1);

        // Without a start the file's <c>active</c> decides while the freeze has not ended.
        Assert.True(active.InForceAt(beforeEnd));
        Assert.False(notActive.InForceAt(beforeEnd));

        // However the file says it, its end ends it.
        Assert.False(active.InForceAt(To));
        Assert.False(active.InForceAt(To + TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void AFreezeIsNamedSeventyTwoHoursBeforeItBegins() =>
        Assert.Equal(TimeSpan.FromHours(72), DeploymentFreeze.Ahead);
}
