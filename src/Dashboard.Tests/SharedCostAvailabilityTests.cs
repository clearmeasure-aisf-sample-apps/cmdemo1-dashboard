using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// SharedCost places what the cost file holds that no environment of the page owns: what Azure still bills under the
/// name of an environment the topology no longer has, and what the environments share. Without the cost file it has
/// nothing to say, and its whole body stands inside that one guard. Each entry it names is exactly one section,
/// labelled by the id of its own heading.
/// </summary>
public class SharedCostTests
{
    private readonly SignallingTimeProvider _time = new();

    private static string SharedCostComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "SharedCost.razor"));

    [Fact]
    public void WithoutACostReportNothingOfTheBodyIsThere()
    {
        var text = File.ReadAllText(SharedCostComponent());

        var guard = text.IndexOf("@if (Monitor.Cost is { } cost)", StringComparison.Ordinal);
        Assert.True(guard >= 0);
        // Before the guard the file has only its comment: no markup the component could render without the report.
        Assert.True(text.IndexOf('<', StringComparison.Ordinal) > guard);
    }

    [Fact]
    public async Task ATopologyWithoutACostAddressReadsNoCostReport()
    {
        var handler = new StubHandler(request => Optics.Answer(request));
        var monitor = Optics.Monitor(handler, _time);

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Null(monitor.Cost);
        Assert.Equal(0, Optics.Count(handler, Optics.CostPath));
    }

    [Fact]
    public void EachEntryItNamesIsOneSectionLabelledByItsOwnHeading()
    {
        var text = File.ReadAllText(SharedCostComponent());

        const string Loop =
            """foreach (var entry in cost.Others(Monitor.Environments.Select(environment => environment.Name)).Append(cost.Shared).OfType<CostEntry>())""";
        Assert.Contains(Loop, text, StringComparison.Ordinal);
        Assert.Contains("var id = $\"cost-entry-{index++}\";", text, StringComparison.Ordinal);
        Assert.Contains("""<section class="environment environment--cost" aria-labelledby="@id">""", text, StringComparison.Ordinal);
        Assert.Contains("""<h2 id="@id">@entry.Name</h2>""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<span class="chip" title="@(shared ? CostText.SharedHelp : CostText.OtherHelp(entry.Name))">@(shared ? "no environment" : "not in the topology")</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<CostLine Report="cost" Amounts="entry.Amounts" Services="entry.TopServices" Now="Now" />""", text, StringComparison.Ordinal);

        // The loop body is the only place the body is written: one section, one heading and one label per entry.
        Assert.Equal(1, CountOccurrences(text, "<section"));
        Assert.Equal(1, CountOccurrences(text, "<h2"));
        Assert.Equal(1, CountOccurrences(text, "aria-labelledby"));
    }

    [Fact]
    public async Task TheEntriesItNamesAreTheOthersOfTheCostAndItsShared()
    {
        var handler = new StubHandler(request => Optics.Answer(request));
        var monitor = Optics.Monitor(handler, _time, topology: Optics.TopologyWithCost);

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.NotNull(monitor.Cost);
        var cost = monitor.Cost!;
        // The loop's input, from the file the monitor read and the environments the page shows: the entries no
        // environment of the page owns, then the one the environments share.
        var entries = cost.Others(monitor.Environments.Select(environment => environment.Name))
            .Append(cost.Shared)
            .OfType<CostEntry>()
            .ToArray();

        Assert.Equal(["tdd", "uat"], monitor.Environments.Select(environment => environment.Name));
        Assert.Equal(["cost-entry-0", "cost-entry-1"], entries.Select((_, position) => $"cost-entry-{position}"));
        Assert.Equal(["retired", "shared"], entries.Select(entry => entry.Name));

        var retired = entries[0];
        Assert.Equal(new CostAmounts(null, 0.2, 0.2), retired.Amounts);
        Assert.Empty(retired.TopServices);
        Assert.False(CostReport.IsShared(retired.Name));
        Assert.Equal(
            "Azure bills resources with the tag \"environment\" = \"retired\", which is no environment of this topology.",
            CostText.OtherHelp(retired.Name));

        var shared = entries[1];
        Assert.Equal(new CostAmounts(1.1, 7.7, 8.2), shared.Amounts);
        Assert.Equal([new CostService("Azure Front Door Service", 8.11)], shared.TopServices);
        Assert.True(CostReport.IsShared(shared.Name));
    }

    [Fact]
    public void AReportWithNoOthersAndNoSharedGivesTheLoopNothing()
    {
        var report = CostReport.Parse("""
            { "currency": "USD", "asOf": "2026-10-03",
              "environments": [ { "name": "tdd", "yesterday": 0 }, { "name": "uat", "yesterday": 1.52 } ] }
            """)!;

        Assert.Null(report.Shared);
        Assert.Empty(report.Others(["tdd", "uat"]).Append(report.Shared).OfType<CostEntry>());
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }
}

/// <summary>
/// The availability line counts what the pipeline's hourly health reports found: the last 24 hours, the last seven
/// days, and when one last failed. Without a single part it has nothing to say, and its whole body stands inside that
/// one guard; with parts it says a label, the parts, and the note that these are hourly checks, not monitoring.
/// </summary>
public class AvailabilityLineTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    private static string AvailabilityLineComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "AvailabilityLine.razor"));

    [Fact]
    public void WithoutPartsNothingOfTheBodyIsThere()
    {
        var text = File.ReadAllText(AvailabilityLineComponent());

        Assert.Contains("var parts = AvailabilityText.Parts(Reports, Now);", text, StringComparison.Ordinal);
        var guard = text.IndexOf("@if (parts.Count > 0)", StringComparison.Ordinal);
        Assert.True(guard >= 0);
        // Before the guard the file has only its comment and the read of the parts: nothing to render without them.
        Assert.True(text.IndexOf('<', StringComparison.Ordinal) > guard);
    }

    [Fact]
    public void ReportsWithoutAWindowGiveNoParts()
    {
        Assert.Empty(AvailabilityText.Parts(new HealthReports(null, null, null), Now));
    }

    [Fact]
    public void TheLineSaysTheLabelItsPartsAndTheNote()
    {
        var text = File.ReadAllText(AvailabilityLineComponent());

        Assert.Contains("""<p class="availability" title="@AvailabilityText.Help(Changed, Zone)">""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="availability__label">@Label</span>""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="availability__part" title="@part.Title">""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<time datetime="@TimeText.Iso(when)" title="@TimeText.DateAndTime(when, Zone)"><strong>@part.Strong</strong></time>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("""<span class="availability__note">@AvailabilityText.Note</span>""", text, StringComparison.Ordinal);
        Assert.Contains("""public string Label { get; set; } = "Availability";""", text, StringComparison.Ordinal);

        // One label and one note; the parts are written only by their loop.
        Assert.Equal(1, CountOccurrences(text, "availability__label"));
        Assert.Equal(1, CountOccurrences(text, "availability__note"));
        Assert.Equal(1, CountOccurrences(text, "availability__part"));
    }

    [Fact]
    public void ThePartsTheNoteAndTheHelpAreSaidInTheWordsOfTheReport()
    {
        var failed = new DateTimeOffset(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);
        var reports = new HealthReports(new HealthWindow(24, 23), new HealthWindow(166, 164), failed);

        Assert.Equal(
            [
                new AvailabilityPart("Healthy in ", "23 of 24", " hourly checks (95.8 %) in 24 hours"),
                new AvailabilityPart(string.Empty, "164 of 166", " in 7 days", "98.8 % of the hourly checks of the last 7 days"),
                new AvailabilityPart("last failure ", "9 h ago", string.Empty, When: failed),
            ],
            AvailabilityText.Parts(reports, Now));

        // What the line writes from them: the note, and the moment of the failure in its two forms.
        Assert.Equal("Hourly checks by the pipeline, not continuous monitoring.", AvailabilityText.Note);
        Assert.Equal("2026-10-06T16:13:00Z", TimeText.Iso(failed));
        Assert.Equal("2026-10-06 16:13 +00:00", TimeText.DateAndTime(failed, TimeZoneInfo.Utc));

        // The line's title, with and without the moment the delivery facts changed.
        var help = AvailabilityText.Help(new DateTimeOffset(2026, 10, 7, 1, 0, 0, TimeSpan.Zero), TimeZoneInfo.Utc);
        Assert.EndsWith("As of the last change of the delivery facts: 2026-10-07 01:00 +00:00.", help, StringComparison.Ordinal);
        Assert.DoesNotContain("As of", AvailabilityText.Help(null, TimeZoneInfo.Utc), StringComparison.Ordinal);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }
}
