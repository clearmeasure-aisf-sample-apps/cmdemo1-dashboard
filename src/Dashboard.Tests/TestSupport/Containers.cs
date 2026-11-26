using Dashboard.Health;
using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// One home for the <c>containers.json</c> of the Resource Health tests - the shape every one of them used to retype
/// inline, as a raw string literal: the same id prefixes, the same <c>checkedAt</c> and the same three
/// <c>source</c> words. <see cref="Report" /> composes the file in its own words and hands back the
/// <see cref="ResourceHealthReport" /> the app's own read (<see cref="ResourceHealthReport.Parse" />) makes of it, so
/// the builder is exercised by that read and cannot drift from it - a status the parser would refuse, a
/// <c>source</c> it would fold to <see cref="ResourceHealthReading.None"/>, or a summary it would cut to 200 all
/// happen here, the way they happen to the page.
/// </summary>
/// <remarks>
/// Fixed on purpose, the way the fixtures are: the moment the file was read is <see cref="Read"/>, a time the tests
/// chose, and no test or fixture here asks a clock what time it is - no <c>DateTimeOffset.UtcNow</c> and no
/// <see cref="System.TimeProvider"/>.
/// </remarks>
internal static class Containers
{
    /// <summary>When containers.json was read: the report's <c>generated</c>, and the default moment of each reading.</summary>
    public static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    /// <summary>A subscription the pipeline names, shared by the ids below.</summary>
    public const string Subscription = "/subscriptions/s";

    /// <summary>A resource group under the shared subscription.</summary>
    public const string ResourceGroup = "/subscriptions/s/resourceGroups/rg";

    /// <summary>The shared prefix of every resource id the tests name.</summary>
    public const string IdPrefix = "/subscriptions/s/resourceGroups/";

    /// <summary>A key vault id, in the shared prefix's words.</summary>
    public const string KeyVault = IdPrefix + "rg/providers/Microsoft.KeyVault/vaults/kv";

    /// <summary>A user-assigned identity id, in the shared prefix's words.</summary>
    public const string Identity = IdPrefix + "rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id";

    /// <summary>A container app id, in the shared prefix's words.</summary>
    public const string App = "/subscriptions/s/resourceGroups/apps/providers/Microsoft.App/containerApps/app";

    /// <summary>
    /// One reading as the file says it: the id, the status, the source, the moment and what Resource Health said, in
    /// the order the pipeline writes them. The moment defaults to <see cref="Read"/> and the source to
    /// <see cref="ResourceHealthReading.FromResourceHealth"/>, the two words the tests reach for most.
    /// </summary>
    /// <param name="id">The resource id, whatever its case: the report finds it by id regardless.</param>
    /// <param name="status">
    /// The status the file says, in its own letters: the parser lowers it and keeps it a word or two, and refuses it
    /// otherwise - so a status the app would not read is refused here too.
    /// </param>
    /// <param name="source">
    /// <see cref="ResourceHealthReading.FromResourceHealth"/>, <see cref="ResourceHealthReading.FromProvisioningState"/>
    /// or <see cref="ResourceHealthReading.None"/>; anything else the parser folds to <see cref="ResourceHealthReading.None"/>.
    /// </param>
    /// <param name="checkedAt">When the pipeline read it; <see cref="Read"/> unless the test says otherwise.</param>
    /// <param name="summary">What Resource Health said in words; null without it, cut to 200 by the read.</param>
    public static ResourceHealthReading AReading(
        string id,
        string status,
        string source = ResourceHealthReading.FromResourceHealth,
        DateTimeOffset? checkedAt = null,
        string? summary = null)
        => new(status, source, checkedAt ?? Read, summary);

    /// <summary>
    /// containers.json holding these readings, read at <see cref="Read"/>, and parsed by the app's own read. A reading
    /// is found by id whatever its case, and a long summary is cut to 200 by that read; a status the read refuses is
    /// left out of the report, the way the page's file would be.
    /// </summary>
    /// <param name="readings">One <see cref="Reading"/> per resource; with none, a file that was read and said nothing.</param>
    public static ResourceHealthReport Report(params Reading[] readings) => Report(Read, readings);

    /// <summary>
    /// <see cref="Report(Reading[])"/> with a moment the file's content last changed that is not <see cref="Read"/>.
    /// </summary>
    /// <param name="generated">When the file's content last changed.</param>
    /// <param name="readings">One <see cref="Reading"/> per resource; with none, a file that was read and said nothing.</param>
    public static ResourceHealthReport Report(DateTimeOffset generated, params Reading[] readings)
    {
        var records = readings.Length == 0
            ? string.Empty
            : string.Join(",", readings.Select(Json));
        var json = $"{{ \"generated\": \"{Iso(generated)}\", \"resources\": {{ \"nonprod\": [ {records} ] }} }}";
        return ResourceHealthReport.Parse(json)!;
    }

    /// <summary>
    /// The file's words for one reading: its id, then its <c>health</c> - the status, the source, the moment and the
    /// words - in the order the pipeline writes them.
    /// </summary>
    private static string Json(Reading reading) =>
        $"{{ \"id\": \"{reading.Id}\", \"health\": {{ \"status\": \"{reading.Status}\", \"source\": \"{reading.Source}\", \"checkedAt\": \"{Iso(reading.CheckedAt)}\"{Summary(reading.Summary)} }} }}";

    private static string Summary(string? summary) => summary is null ? string.Empty : $", \"summary\": \"{summary}\"";

    private static string Iso(DateTimeOffset at) => at.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

    /// <summary>One reading of one resource, in the file's own words: what <see cref="Report"/> composes and parses.</summary>
    /// <param name="id">The resource id, whatever its case.</param>
    /// <param name="status">The status, in the file's own letters; the read lowers and bounds it.</param>
    /// <param name="source">The source word; the read folds an unknown one to <see cref="ResourceHealthReading.None"/>.</param>
    /// <param name="checkedAt">When the pipeline read it; <see cref="Read"/> unless named.</param>
    /// <param name="summary">What Resource Health said in words; the read cuts it to 200.</param>
    public readonly record struct Reading(string Id, string Status, string Source, DateTimeOffset CheckedAt, string? Summary)
    {
        /// <summary>A reading by its id, its status and its source, read at <see cref="Read"/>.</summary>
        public static Reading Of(string id, string status, string source = ResourceHealthReading.FromResourceHealth, DateTimeOffset? checkedAt = null, string? summary = null)
            => new(id, status, source, checkedAt ?? Read, summary);
    }
}
