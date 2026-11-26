namespace Dashboard.Tests;

/// <summary>
/// The pins of the fixture itself: that its moments are the ones the delivery tests expect, that its windows are
/// the ones the file gives, that its <c>prod</c> report is the report the canonical file reads, and that its
/// builders keep what they are told. The fixture is the home; these tests are the lock on the door.
/// </summary>
public class DeliveryFixtureTests
{
    /// <summary>
    /// The fixture's moments are the ones the tests expect: <see cref="DeliveryFixtures.Failed" /> is on the
    /// meridian, <see cref="DeliveryFixtures.FailedInOtherHours" /> is the same instant five hours east,
    /// <see cref="DeliveryFixtures.Now" /> is nine hours and seventeen minutes after <see cref="DeliveryFixtures.Failed" />,
    /// and <see cref="DeliveryFixtures.Generated" /> is thirty minutes before <see cref="DeliveryFixtures.Now" />.
    /// </summary>
    [Fact]
    public void TheFixtureMomentsAreTheOnesTheTestsExpect()
    {
        Assert.Equal(TimeSpan.Zero, DeliveryFixtures.Failed.Offset);
        Assert.Equal(TimeSpan.FromHours(5), DeliveryFixtures.FailedInOtherHours.Offset);
        Assert.Equal(DeliveryFixtures.Failed, DeliveryFixtures.FailedInOtherHours);
        Assert.Equal(DeliveryFixtures.Now, DeliveryFixtures.Failed.AddHours(9).AddMinutes(17));
        Assert.Equal(DeliveryFixtures.Now, DeliveryFixtures.Generated.AddMinutes(30));
        var elapsed = DeliveryFixtures.Now.Subtract(DeliveryFixtures.Failed);
        Assert.Equal(9, (int)elapsed.TotalHours);
        Assert.Equal(17, elapsed.Minutes);
    }

    /// <summary>
    /// The fixture's windows are the ones the tests expect: <see cref="DeliveryFixtures.Day" /> is 23 of 24 (not
    /// all healthy), <see cref="DeliveryFixtures.Week" /> is 164 of 166 (not all healthy), and the all-healthy
    /// windows the canonical <see cref="DeliveryFixtures.Delivery" /> text reads for <c>tdd</c> are 24 of 24 and
    /// 168 of 168.
    /// </summary>
    [Fact]
    public void TheFixtureWindowsAreTheOnesTheTestsExpect()
    {
        Assert.Equal(24, DeliveryFixtures.Day.Reports);
        Assert.Equal(23, DeliveryFixtures.Day.Healthy);
        Assert.False(DeliveryFixtures.Day.AllHealthy);

        Assert.Equal(166, DeliveryFixtures.Week.Reports);
        Assert.Equal(164, DeliveryFixtures.Week.Healthy);
        Assert.False(DeliveryFixtures.Week.AllHealthy);

        var tdd = DeliveryFixtures.Report.HealthOf("tdd")!;
        Assert.Equal(new HealthWindow(24, 24), tdd.Last24Hours);
        Assert.Equal(new HealthWindow(168, 168), tdd.Last7Days);
        Assert.True(tdd.Last24Hours!.AllHealthy);
        Assert.True(tdd.Last7Days!.AllHealthy);
    }

    /// <summary>
    /// The fixture's <see cref="DeliveryFixtures.Prod" /> report is the report the canonical
    /// <see cref="DeliveryFixtures.Delivery" /> text reads for <c>prod</c>: the same two windows, the same moment,
    /// no container apps on either side - so that a test that builds a report by hand and a test that reads one
    /// from the file stand on the same facts.
    /// </summary>
    [Fact]
    public void TheProdReportIsTheReportTheFileReads()
    {
        var read = DeliveryFixtures.Report.HealthOf("prod")!;

        Assert.Equal(DeliveryFixtures.Prod, read);
        Assert.True(DeliveryFixtures.Prod == read);
        Assert.Equal(DeliveryFixtures.Prod.GetHashCode(), read.GetHashCode());
        Assert.Single(new HashSet<HealthReports> { read, DeliveryFixtures.Prod });
        Assert.Equal(DeliveryFixtures.Day, read.Last24Hours);
        Assert.Equal(DeliveryFixtures.Week, read.Last7Days);
        Assert.Equal(DeliveryFixtures.Failed, read.LastFailure);
        Assert.Null(read.ContainerApps);
    }

    /// <summary>
    /// The fixture's <see cref="DeliveryFixtures.Reports" /> builder: the default windows are the fixture's
    /// <see cref="DeliveryFixtures.Day" /> and <see cref="DeliveryFixtures.Week" />, the default failure is
    /// <see cref="DeliveryFixtures.Failed" />, and the default apps are no apps. A part told is the part told; a
    /// part not told is the fixture's default or no part.
    /// </summary>
    [Fact]
    public void TheReportsBuilderKeepsTheFixturesDefaults()
    {
        // No part told: no window, no failure, no apps — the builder keeps what it is told, and nothing else.
        var none = new HealthReports(null, null, null);
        Assert.Equal(none, DeliveryFixtures.Reports());
        Assert.Equal(none, DeliveryFixtures.Reports(day: null, week: null, failure: null, apps: null));
        Assert.Null(DeliveryFixtures.Reports().Last24Hours);

        // The fixture's report is the one told the fixture's facts.
        Assert.Equal(DeliveryFixtures.Prod, DeliveryFixtures.Reports(DeliveryFixtures.Day, DeliveryFixtures.Week, DeliveryFixtures.Failed));

        // A part told is the part told.
        var day = new HealthWindow(1, 1);
        Assert.Same(day, DeliveryFixtures.Reports(day: day).Last24Hours);
        var failure = DateTimeOffset.MinValue;
        Assert.Equal(failure, DeliveryFixtures.Reports(failure: failure).LastFailure);

        // The apps: no list by default, an empty list kept as the empty list.
        Assert.Null(DeliveryFixtures.Reports().ContainerApps);
        var empty = Array.Empty<ContainerAppReports>();
        Assert.Same(empty, DeliveryFixtures.Reports(apps: empty).ContainerApps);
    }

    /// <summary>
    /// The fixture's <see cref="DeliveryFixtures.ContainerApp" /> builder: the name is kept as given, the check
    /// and the two windows are the very instances told, and a part not told is no part.
    /// </summary>
    [Fact]
    public void TheContainerAppBuilderKeepsWhatItIsTold()
    {
        var last = new HourlyCheck(HourlyCheck.Healthy, DeliveryFixtures.Now);
        var day = new HealthWindow(24, 24);
        var week = new HealthWindow(168, 167);
        var app = DeliveryFixtures.ContainerApp("ui", last, day, week);

        Assert.Equal("ui", app.Name);
        Assert.Same(last, app.Last);
        Assert.Same(day, app.Last24Hours);
        Assert.Same(week, app.Last7Days);

        var bare = DeliveryFixtures.ContainerApp("scorecard");
        Assert.Null(bare.Last);
        Assert.Null(bare.Last24Hours);
        Assert.Null(bare.Last7Days);
    }

    /// <summary>
    /// The canonical <see cref="DeliveryFixtures.Delivery" /> text parses to four environments in the order the
    /// file writes them, the <c>generated</c> moment is <see cref="DeliveryFixtures.Generated" />, and there is
    /// no failover test. The <c>uat</c> environment has no health section, and <c>dev</c> has neither a health
    /// section nor a failover test.
    /// </summary>
    [Fact]
    public void TheDeliveryTextParsesToFourEnvironments()
    {
        var report = DeliveryFixtures.Report;

        Assert.Equal(DeliveryFixtures.Generated, report.Generated);
        Assert.Equal(["tdd", "prod", "uat", "dev"], report.Environments.Select(environment => environment.Name));
        Assert.Null(report.Failover);

        Assert.NotNull(report.HealthOf("tdd"));
        Assert.NotNull(report.HealthOf("prod"));
        Assert.Null(report.HealthOf("uat"));
        Assert.Null(report.HealthOf("dev"));

        Assert.Equal("2.4.15", report.Find("tdd", "ui")!.Version);
        Assert.Equal("2.4.14", report.Find("uat", "ui")!.Version);
        Assert.Equal("2.4.14", report.Find("dev", "ui")!.Version);
        Assert.Empty(report.Environments[1].Deployables);
    }
}
