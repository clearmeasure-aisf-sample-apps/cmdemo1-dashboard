using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// One home for assembling a <see cref="ResourceHealthReport" /> from pieces, the way the tests of the resource boxes and
/// of the "In no environment" rows each used to say privately: a reading in the file's own words (<see cref="Said" />),
/// and containers.json holding one reading per id, found whatever case the id is asked in (<see cref="Report" />). Every
/// reading defaults to the moment the report was read, which each test class chooses for itself.
/// </summary>
/// <param name="read">When containers.json was read: the report's <c>Generated</c>, and each reading's default moment.</param>
internal sealed class ResourceHealthReports(DateTimeOffset read)
{
    /// <summary>When containers.json was read.</summary>
    public DateTimeOffset Read { get; } = read;

    /// <summary>A reading as the file's own words give it, at <see cref="Read" /> unless the test says otherwise.</summary>
    public ResourceHealthReading Said(
        string status,
        string source = ResourceHealthReading.FromResourceHealth,
        DateTimeOffset? at = null,
        string? summary = null) => new(status, source, at ?? Read, summary);

    /// <summary>containers.json with one reading per id, found whatever case the box or row asks it in. With no
    /// readings it is a file that was read and said nothing.</summary>
    public ResourceHealthReport Report(params (string Id, ResourceHealthReading Reading)[] readings) =>
        new(Read, readings.ToDictionary(entry => entry.Id, entry => entry.Reading, StringComparer.OrdinalIgnoreCase));

    /// <summary>containers.json as the pipeline writes it, parsed: one <c>prod</c> resource whose health was checked at
    /// 2026-10-12 09:00 UTC.</summary>
    public static ResourceHealthReport OneProdReading(string id, string status, string source) =>
        ResourceHealthReport.Parse($$"""
            { "resources": { "prod": [ { "id": "{{id}}", "health": { "status": "{{status}}", "source": "{{source}}", "checkedAt": "2026-10-12T09:00:00Z" } } ] } }
            """)!;
}
