namespace Dashboard.Tests;

/// <summary>
/// The words of a deployment freeze, in the one place they now live (<see cref="FreezeText"/>): the brief dot
/// sentence on a node (<see cref="FreezeText.Brief"/>), the sentence under an environment's name
/// (<see cref="FreezeText.Sentence"/>), and the tooltip (<see cref="FreezeText.Title"/>), pinned word for word, in
/// the force, coming, without a start, and naming no projects.
/// </summary>
public class FreezeTextTests
{
    /// <summary>Friday noon, UTC: the moment the page is asked, and every day below is counted from it.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    /// <summary>The words every tooltip of a freeze ends with, said once and joined on to its title.</summary>
    private const string FreezeEnd = " A freeze belongs to the Octopus Deploy instance; a person with the right may override it for one deployment.";

    /// <summary>A freeze in force, with a known start: the weekend.</summary>
    private static readonly DeploymentFreeze InForce =
        new("prod weekend", new DateTimeOffset(2026, 10, 9, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 12, 0, 0, 0, TimeSpan.Zero), true, ["prod"], ["demo-ui"]);

    /// <summary>A freeze in force without a start: the file's own word decides.</summary>
    private static readonly DeploymentFreeze InForceWithoutAStart =
        new("alpha", null, new DateTimeOffset(2026, 10, 12, 0, 0, 0, TimeSpan.Zero), true, ["prod"], ["demo-ui"]);

    /// <summary>A freeze that comes: release week.</summary>
    private static readonly DeploymentFreeze Coming =
        new("release week", new DateTimeOffset(2026, 10, 10, 19, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 11, 19, 0, 0, TimeSpan.Zero), false, ["uat", "prod"], ["demo-ui", "demo-system"]);

    /// <summary>A freeze in force that names no projects: it covers the system as a whole.</summary>
    private static readonly DeploymentFreeze TheSystem =
        new("all", null, new DateTimeOffset(2026, 10, 12, 0, 0, 0, TimeSpan.Zero), true, ["prod"], []);

    [Fact]
    public void TheBriefSaysTheFreezeInTheNodeAndNothingMore()
    {
        // The node already names the deployable and the environment; the brief says only the freeze.
        Assert.Equal("frozen until Mon 00:00: prod weekend", FreezeText.Brief(InForce, Now, Utc));
        Assert.Equal("freeze from Sat 19:00: release week", FreezeText.Brief(Coming, Now, Utc));
    }

    [Fact]
    public void TheSentenceNamesTheFirstFreezeAndHowManyMore()
    {
        // In force, one more to follow.
        Assert.Equal("deployment freeze prod weekend: demo-ui frozen until Mon 00:00, and 1 more", FreezeText.Sentence(InForce, 2, Now, Utc));
        // In force, alone.
        Assert.Equal("deployment freeze prod weekend: demo-ui frozen until Mon 00:00", FreezeText.Sentence(InForce, 1, Now, Utc));
        // Coming, naming every project it freezes.
        Assert.Equal("deployment freeze release week: demo-ui, demo-system frozen from Sat 19:00 until Sun 19:00", FreezeText.Sentence(Coming, 1, Now, Utc));
        // Coming, two more to follow.
        Assert.Equal("deployment freeze release week: demo-ui, demo-system frozen from Sat 19:00 until Sun 19:00, and 2 more", FreezeText.Sentence(Coming, 3, Now, Utc));
    }

    [Fact]
    public void TheTitleSaysTheFreezeInAFullSentenceWithItsTimes()
    {
        // In force, with a known start.
        Assert.Equal(
            "Deployment freeze prod weekend: no deployment of demo-ui to prod from 2026-10-09 00:00 +00:00 until 2026-10-12 00:00 +00:00. In force now." + FreezeEnd,
            FreezeText.Title(InForce, Now, Utc));

        // In force without a start: said by its end alone.
        Assert.Equal(
            "Deployment freeze alpha: no deployment of demo-ui to prod until 2026-10-12 00:00 +00:00. In force now." + FreezeEnd,
            FreezeText.Title(InForceWithoutAStart, Now, Utc));

        // Coming: when it begins, said in the viewer's zone.
        Assert.Equal(
            "Deployment freeze release week: no deployment of demo-ui, demo-system to uat, prod from 2026-10-10 19:00 +00:00 until 2026-10-11 19:00 +00:00. Begins in 31 h." + FreezeEnd,
            FreezeText.Title(Coming, Now, Utc));
    }

    [Fact]
    public void AFreezeThatNamesNoProjectsNamesTheSystem()
    {
        Assert.Equal("deployment freeze all: the system frozen until Mon 00:00", FreezeText.Sentence(TheSystem, 1, Now, Utc));
        Assert.Equal(
            "Deployment freeze all: no deployment of the system to prod until 2026-10-12 00:00 +00:00. In force now." + FreezeEnd,
            FreezeText.Title(TheSystem, Now, Utc));
    }
}
