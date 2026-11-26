namespace Dashboard.Tests;

public class DeliveryEnvironmentTests
{
    private const string Delivery = """
        { "generated": "2026-10-04T21:00:00Z",
          "environments": [
            { "name": "tdd",
              "deployables": [ { "name": "ui", "version": "2.4.15", "deployedAt": "2026-10-04T20:30:00Z" } ],
              "health": { "last24Hours": { "reports": 24, "healthy": 24 }, "last7Days": { "reports": 168, "healthy": 168 } } },
            { "name": "uat",
              "deployables": [
                { "name": "dashboard", "version": "1.0.7", "deployedAt": "2026-10-02T10:00:00Z" },
                { "name": "system", "version": "1.0.33", "deployedAt": "2026-10-03T11:00:00Z" },
                { "name": "ui", "version": "2.4.14", "deployedAt": "2026-10-04T16:37:00Z" } ],
              "health": { "last24Hours": { "reports": 23, "healthy": 22 }, "last7Days": { "reports": 167, "healthy": 160 },
                          "lastFailure": "2026-10-03T08:00:00Z",
                          "containerApps": [ { "name": "ui", "last": { "result": "healthy", "at": "2026-10-04T20:00:00Z" },
                                                "last24Hours": { "reports": 23, "healthy": 22 } } ] } },
            { "name": "prod", "deployables": [] } ],
          "failover": { "environment": "uat", "at": "2026-10-04T05:00:00Z", "seconds": 44 } }
        """;

    private static readonly DeliveryReport Report = DeliveryReport.Parse(Delivery)!;

    [Fact]
    public void AnEnvironmentKeepsItsNameItsDeployablesAndItsHealth()
    {
        var uat = Report.Environments[1];

        Assert.Equal("uat", uat.Name);
        Assert.Equal(
            [
                new DeliveryEntry("dashboard", "1.0.7", new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero), null, null, null, null, null, null, null, null, null),
                new DeliveryEntry("system", "1.0.33", new DateTimeOffset(2026, 10, 3, 11, 0, 0, TimeSpan.Zero), null, null, null, null, null, null, null, null, null),
                new DeliveryEntry("ui", "2.4.14", new DateTimeOffset(2026, 10, 4, 16, 37, 0, TimeSpan.Zero), null, null, null, null, null, null, null, null, null),
            ],
            uat.Deployables);
        Assert.Equal(new HealthWindow(23, 22), uat.Health!.Last24Hours);
        Assert.Equal(new HealthWindow(167, 160), uat.Health.Last7Days);
        Assert.Equal(new DateTimeOffset(2026, 10, 3, 8, 0, 0, TimeSpan.Zero), uat.Health.LastFailure);
        Assert.Equal(
            [new ContainerAppReports("ui", new HourlyCheck(HourlyCheckResult.Healthy, new DateTimeOffset(2026, 10, 4, 20, 0, 0, TimeSpan.Zero)), new HealthWindow(23, 22), null)],
            uat.Health.ContainerApps);
        Assert.Equal(new HealthReports(new HealthWindow(24, 24), new HealthWindow(168, 168), null), Report.Environments[0].Health);
        Assert.Empty(Report.Environments[2].Deployables);
        Assert.Null(Report.Environments[2].Health);
    }

    [Fact]
    public void TheReportKeepsWhenItsFactsChangedItsEnvironmentsAndItsFailoverTest()
    {
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 21, 0, 0, TimeSpan.Zero), Report.Generated);
        Assert.Equal(["tdd", "uat", "prod"], Report.Environments.Select(environment => environment.Name));
        Assert.Equal(new FailoverTest("uat", new DateTimeOffset(2026, 10, 4, 5, 0, 0, TimeSpan.Zero), 44), Report.Failover);
    }

    [Fact]
    public void TheLookupsFindAnEnvironmentByItsNameRegardlessOfItsCase()
    {
        Assert.Equal("2.4.14", Report.Find("UAT", "ui")!.Version);
        Assert.Equal("1.0.7", Report.Find("uat", "DASHBOARD")!.Version);
        Assert.Null(Report.Find("UAT", "api"));
        Assert.Equal(new HealthWindow(23, 22), Report.HealthOf("Uat")!.Last24Hours);
        Assert.Equal("ui", Assert.Single(Report.ContainerAppsOf("UAT")!).Name);
        Assert.Equal(["system", "dashboard"], Report.Others("UAT", ["ui"]).Select(entry => entry.Name));
    }

    [Fact]
    public void ANameNoEnvironmentHasFindsNothing()
    {
        Assert.Null(Report.Find("staging", "ui"));
        Assert.Null(Report.Find("staging", "UI"));
        Assert.Null(Report.HealthOf("staging"));
        Assert.Null(Report.ContainerAppsOf("staging"));
        Assert.Empty(Report.Others("staging", ["ui"]));
        Assert.Null(Report.Find("prod", "ui"));
        Assert.Empty(Report.Others("prod", []));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("{ }")]
    [InlineData("""{ "environments": [ ] }""")]
    [InlineData("""{ "environments": [ { "deployables": [ { "name": "ui" } ] } ] }""")]
    [InlineData("""{ "failover": null }""")]
    public void AFileWithoutEnvironmentsAndWithoutAFailoverTestIsNoReport(string? file) =>
        Assert.Null(DeliveryReport.Parse(file));

    [Fact]
    public void AReportWithoutEnvironmentsFindsNothingButItsFailoverTest()
    {
        var report = DeliveryReport.Parse("""{ "failover": { "environment": "uat", "seconds": 44 } }""")!;

        Assert.Null(report.Generated);
        Assert.Empty(report.Environments);
        Assert.Equal(new FailoverTest("uat", null, 44), report.Failover);
        Assert.Null(report.Find("uat", "ui"));
        Assert.Null(report.HealthOf("uat"));
        Assert.Null(report.ContainerAppsOf("uat"));
        Assert.Empty(report.Others("uat", []));
    }

    [Fact]
    public void AnEnvironmentSaysNothingAboutItsHealthUntilTheFileSaysSo()
    {
        var environment = new DeliveryEnvironment("uat", []);

        Assert.Equal("uat", environment.Name);
        Assert.Empty(environment.Deployables);
        Assert.Null(environment.Health);
        var withHealth = environment with { Health = new HealthReports(new HealthWindow(24, 24), null, null) };
        Assert.Equal("uat", withHealth.Name);
        Assert.Empty(withHealth.Deployables);
        Assert.Equal(new HealthWindow(24, 24), withHealth.Health!.Last24Hours);
    }

    [Fact]
    public void TheSystemIsTheEntryNamedSystemWhateverItsCase()
    {
        Assert.Equal("system", DeliveryReport.SystemName);
        Assert.True(DeliveryReport.IsSystem("System"));
        Assert.True(DeliveryReport.IsSystem("SYSTEM"));
        Assert.False(DeliveryReport.IsSystem("uat"));
        Assert.False(DeliveryReport.IsSystem(string.Empty));
    }
}
