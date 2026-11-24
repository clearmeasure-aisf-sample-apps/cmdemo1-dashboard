namespace Dashboard.Tests;

/// <summary>
/// One home per delivery fact (#617): <c>DeliveryReport.cs</c> holds the model <c>delivery.json</c> is read into and
/// nothing else, and the words the page makes of it - <see cref="AvailabilityText" />, <see cref="AvailabilityPart" />
/// and <see cref="DeliveryText" /> - stand in <c>DeliveryText.cs</c>, in the same namespace and under the same names.
/// </summary>
public class DeliveryTextHomeTests
{
    private static readonly string[] Model =
    [
        "public sealed record DeliveryReport(",
        "public sealed record DeliveryEnvironment(",
        "public sealed record HealthReports(",
        "public sealed record HealthWindow(",
        "public sealed record DeliveryEntry(",
        "public sealed record PersonSignOff(",
        "public sealed record BehindFirst(",
        "public sealed record FailoverTest(",
    ];

    private static readonly string[] Words =
    [
        "public static class AvailabilityText",
        "public sealed record AvailabilityPart(",
        "public static class DeliveryText",
    ];

    [Fact]
    public void TheReportFileHoldsTheEightRecordsOfTheModelAndNoWords()
    {
        var source = Read("DeliveryReport.cs");

        Assert.All(Model, declaration => Assert.Contains(declaration, source, StringComparison.Ordinal));
        Assert.All(Words, declaration => Assert.DoesNotContain(declaration, source, StringComparison.Ordinal));
        Assert.DoesNotContain("using System.Globalization;", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTextFileHoldsTheWordsOfBothCardsAndNoModel()
    {
        var source = Read("DeliveryText.cs");

        Assert.All(Words, declaration => Assert.Contains(declaration, source, StringComparison.Ordinal));
        Assert.All(Model, declaration => Assert.DoesNotContain(declaration, source, StringComparison.Ordinal));
        Assert.Contains("namespace Dashboard.Health;", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWordTypesKeepTheirNamespaceAndNames()
    {
        Assert.Equal("Dashboard.Health.AvailabilityText", typeof(AvailabilityText).FullName);
        Assert.Equal("Dashboard.Health.AvailabilityPart", typeof(AvailabilityPart).FullName);
        Assert.Equal("Dashboard.Health.DeliveryText", typeof(DeliveryText).FullName);
    }

    /// <summary>
    /// The lookups name what they bind: no lambda of the report reads a deployable as an "entry" of an environment,
    /// nor an environment as one.
    /// </summary>
    [Fact]
    public void TheReportsLookupsBindNoLambdaToEntry()
    {
        var source = Read("DeliveryReport.cs");

        Assert.DoesNotContain("entry =>", source, StringComparison.Ordinal);
    }

    [Fact]
    public void FindAndOthersStillMatchByNameWithoutCase()
    {
        var report = DeliveryReport.Parse("""
            { "environments": [
              { "name": "UAT", "deployables": [ { "name": "ui" }, { "name": "Dashboard" }, { "name": "System" } ] } ] }
            """)!;

        Assert.Equal("ui", report.Find("uat", "UI")!.Name);
        Assert.Null(report.Find("prod", "ui"));
        Assert.Equal(["System", "Dashboard"], report.Others("uat", ["UI"]).Select(deployable => deployable.Name));
        Assert.Empty(report.Others("prod", []));
    }

    private static string Read(string file)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "Dashboard", "Health", file);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
        }

        throw new FileNotFoundException($"src/Dashboard/Health/{file} was not found above the test's output.");
    }
}
