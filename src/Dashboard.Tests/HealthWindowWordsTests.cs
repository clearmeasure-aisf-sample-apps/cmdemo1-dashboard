using System.Reflection;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// Pins the two window words <see cref="HealthWindow" /> names for the file - <c>last24Hours</c> and
/// <c>last7Days</c> - and checks that both levels of the <c>health</c> section (the environment's own windows
/// and each container-app entry) pick up both windows when read through <see cref="DeliveryReport.Parse" />.
/// The consts are internal and the app declares no <c>InternalsVisibleTo</c>, so they are read by reflection.
/// </summary>
public class HealthWindowWordsTests
{
    /// <summary>The two window words are the file's words, pinned so a rename here breaks loudly.</summary>
    [Fact]
    public void TheWindowWordsAreTheFileWords()
    {
        var fields = typeof(HealthWindow).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        var day = fields.Single(f => f.Name == "DayWord").GetValue(null)!;
        var week = fields.Single(f => f.Name == "WeekWord").GetValue(null)!;

        Assert.Equal("last24Hours", day);
        Assert.Equal("last7Days", week);
    }

    /// <summary>
    /// A <c>health</c> section keyed by the file's words, with an environment-level window and a container-app
    /// entry, is read by <see cref="DeliveryReport.Parse" /> with both levels picking up both windows.
    /// </summary>
    [Fact]
    public void BothLevelsPickUpBothWindows()
    {
        var json = """
            {
              "environments": [
                {
                  "name": "uat",
                  "health": {
                    "last24Hours": { "reports": 24, "healthy": 23 },
                    "last7Days": { "reports": 168, "healthy": 160 },
                    "lastFailure": "2026-10-06T16:13:00Z",
                    "containerApps": [
                      {
                        "name": "ui",
                        "last": { "result": "healthy", "at": "2026-10-07T01:02:13Z" },
                        "last24Hours": { "reports": 24, "healthy": 24 },
                        "last7Days": { "reports": 168, "healthy": 167 }
                      }
                    ]
                  }
                }
              ]
            }
            """;

        var report = DeliveryReport.Parse(json);

        Assert.NotNull(report);
        var health = report!.HealthOf("uat");

        Assert.NotNull(health);

        // Environment-level windows.
        Assert.Equal(24, health!.Last24Hours!.Reports);
        Assert.Equal(23, health.Last24Hours.Healthy);
        Assert.Equal(168, health.Last7Days!.Reports);
        Assert.Equal(160, health.Last7Days.Healthy);

        // Container-app entry windows.
        var app = Assert.Single(health.ContainerApps!);
        Assert.Equal("ui", app.Name);
        Assert.NotNull(app.Last24Hours);
        Assert.Equal(24, app.Last24Hours!.Reports);
        Assert.Equal(24, app.Last24Hours.Healthy);
        Assert.NotNull(app.Last7Days);
        Assert.Equal(168, app.Last7Days!.Reports);
        Assert.Equal(167, app.Last7Days.Healthy);
    }
}
