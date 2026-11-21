using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ResourceHealthText" /> - the words the page wears for
/// what Azure Resource Health last said of the resources a box stands for, read out of the hourly
/// <c>containers.json</c> by the pipeline and never asked of Azure here. The class has three public members and every
/// one of them is walked to its end: <see cref="ResourceHealthText.Source" />, the fixed first words of every tooltip;
/// <see cref="ResourceHealthText.Label" />, the badge's word for one reading; and
/// <see cref="ResourceHealthText.Of" />, the four words of a whole box.
/// <para>
/// What is pinned elsewhere is walked past on purpose. <see cref="ResourceHealthTests" /> pins the reading of the
/// file - the id found whatever its case, a status that is no word or a time that is not there leaving the record out,
/// an unknown source falling to <c>none</c>, a summary cut to 200 letters, a file without <c>resources</c> being no
/// report at all - and the seven badge words a file can answer with. <see cref="ResourceHealthTextTests" /> pins the
/// box itself: the word, the source and the age of a box of one of each kind, a box the page has no report for, the
/// worst word and the count of the rest for a box of several, the age being the oldest reading's, a box read over two
/// hours ago going grey, a box without ids saying nothing and a tooltip naming its resource, its state, where the
/// words came from and when. <see cref="ResourceHealthWordsEdgeCaseTests" /> pins the record those four words are kept
/// in - and the one guard of the making, a zone that is not there being refused before the ids are looked at.
/// <see cref="UnplacedTests" />, <see cref="UnplacedRowTests" /> and <see cref="RuntimePayloadBuilderTests" /> pin the
/// same words where the page lands them, the row and the tile, an id looked up in another case than the file spells
/// it among them.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the tooltip's first words, letter for letter, a compile-time constant,
/// said once and once only in any title; the badge of a reading that is not there at all, of a source spelled in
/// another case than the three the file writes, of a status that is the empty word, one letter, a whitespace, a digit,
/// an accent or two hundred letters, and the capitalising of a provisioning state following no reader - a Turkish one
/// included, whose own capital <c>I</c> is dotted; the tone turning on the very tick the reading passes two hours, the
/// two-hour line being said the same on both sides of it; a reading of the minute and one that stands ahead of the
/// clock being <c>just now</c> and plain; the name in the tooltip being the last part of the id, so that a trailing
/// slash, a bare word, a lone slash and no word at all each say what they say; the two tooltip words for where a state
/// came from that no test had ever made the page say - Resource Health having no answer, and neither it nor a
/// provisioning state answering - and the space a summary of no letters leaves behind a summary of nothing does not;
/// an id told twice being counted twice and named twice; a box whose every id is missing saying no age at all; the
/// eight resources a tooltip names before it counts the rest; a box told in a zone five and a half hours east and five
/// hours west of the meridian, the day falling on either side of the reading; an id that is not there at all being
/// refused by the look-up it is asked with, the report's absence aside; and the whole public surface of the class, so
/// that the day a fourth word is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <see cref="ResourceHealthText" /> is
/// static, holds no <see cref="HttpClient" />, no address and no moment of its own, and every time, report, list of
/// ids and zone below is one the test brought. Where the class does ask a clock, it asks the one it is handed, so the
/// whole file is said of fixed inputs and says the same to whoever runs it.
/// </para>
/// </summary>
public class ResourceHealthTextEdgeCaseTests
{
    /// <summary>The moment the pipeline read what it read.</summary>
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    /// <summary>Forty minutes on: the moment every box below is told at unless it says otherwise.</summary>
    private static readonly DateTimeOffset Now = Read + TimeSpan.FromMinutes(40);

    /// <summary>A file the pipeline wrote that names no resource at all.</summary>
    private static readonly ResourceHealthReport NothingRead = new(Read, new Dictionary<string, ResourceHealthReading>(StringComparer.OrdinalIgnoreCase));

    /// <summary>The zone a day ahead of the reading's own date, and the one behind it.</summary>
    private static readonly TimeZoneInfo East = TimeZoneInfo.CreateCustomTimeZone("east-five-thirty", TimeSpan.FromMinutes(330), "East", "East");

    private static readonly TimeZoneInfo West = TimeZoneInfo.CreateCustomTimeZone("west-five", TimeSpan.FromHours(-5), "West", "West");

    private const string Kv = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv";
    private const string Identity = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id";
    private const string App = "/subscriptions/s/resourceGroups/apps/providers/Microsoft.App/containerApps/app";

    /// <summary>A reading as the file's own words give it, at <see cref="Read" /> unless the test says otherwise.</summary>
    private static ResourceHealthReading Said(
        string status,
        string source = ResourceHealthReading.FromResourceHealth,
        DateTimeOffset? at = null,
        string? summary = null) => new(status, source, at ?? Read, summary);

    /// <summary>containers.json with one reading per id, found whatever case the box asks it in.</summary>
    private static ResourceHealthReport Report(params (string Id, ResourceHealthReading Reading)[] readings) =>
        new(Read, readings.ToDictionary(entry => entry.Id, entry => entry.Reading, StringComparer.OrdinalIgnoreCase));

    // -------------------------------------------------- the tooltip's first words, said once and fixed

    /// <summary>The fixed words every tooltip opens with, letter for letter: where the state came from, that it is the
    /// <c>delivery</c> workflow that reads it, that it does so hourly, that it does so as the system's Reader identity,
    /// and that the page itself asks Azure nothing. A colour is not among them, no markup is, and the sentence is one
    /// line said whole. They are a constant of the class, so no caller can be handed another telling of them.</summary>
    [Fact]
    public void TheTooltipOpensWithTheSameWordsWhateverTheBox()
    {
        const string Expected = "Azure Resource Health, read hourly by the pipeline (workflow delivery, as the system's Reader identity); this page does not ask Azure.";

        Assert.Equal(Expected, ResourceHealthText.Source);
        Assert.DoesNotContain("\n", ResourceHealthText.Source, StringComparison.Ordinal);
        Assert.DoesNotContain("  ", ResourceHealthText.Source, StringComparison.Ordinal);
        Assert.Equal(ResourceHealthText.Source, ResourceHealthText.Source.Trim());
        Assert.EndsWith(".", ResourceHealthText.Source, StringComparison.Ordinal);
        foreach (var said in new[] { "hourly", "delivery", "Reader", "does not ask Azure" })
        {
            Assert.Contains(said, ResourceHealthText.Source, StringComparison.Ordinal);
        }

        // The words a colour would be said in, and the marks of the page, are none of them here.
        foreach (var refused in new[] { "red", "green", "amber", "<", ">", "class=" })
        {
            Assert.DoesNotContain(refused, ResourceHealthText.Source, StringComparison.Ordinal);
        }

        var field = typeof(ResourceHealthText).GetField("Source", BindingFlags.Public | BindingFlags.Static)!;

        Assert.True(field.IsPublic);
        Assert.True(field.IsStatic);
        Assert.True(field.IsLiteral);
        Assert.False(field.IsInitOnly);
        Assert.Equal(typeof(string), field.FieldType);
        Assert.Equal(Expected, (string)field.GetRawConstantValue()!);
    }

    /// <summary>Of a box of one, of several, of ids the file does not name: the first line of the tooltip is these
    /// words, and they are said there once and not again - the source stands above the resources, not beside each of
    /// them, and neither the badge nor the line repeats it.</summary>
    [Fact]
    public void EveryTooltipOpensWithThoseWordsAndSaysThemOnce()
    {
        var report = Report((Kv, Said("available", summary: "No incidents reported.")), (Identity, Said("succeeded", ResourceHealthReading.FromProvisioningState)));
        IReadOnlyList<string>[] boxes = [[Kv], [Kv, Identity], [Identity, "/s/missing"]];

        foreach (var ids in boxes)
        {
            var words = ResourceHealthText.Of(ids, report, Now, TimeZoneInfo.Utc)!;
            var lines = words.Title.Split('\n');

            Assert.Equal(ResourceHealthText.Source, lines[0]);
            Assert.DoesNotContain(ResourceHealthText.Source, lines.Skip(1), StringComparer.Ordinal);
            Assert.DoesNotContain(ResourceHealthText.Source, words.Line, StringComparison.Ordinal);
            Assert.DoesNotContain(ResourceHealthText.Source, words.Label, StringComparison.Ordinal);
        }

        // A box the page has no file for opens with them too, and says its own nothing after them.
        var unread = ResourceHealthText.Of([Kv], null, Now, TimeZoneInfo.Utc)!;

        Assert.StartsWith($"{ResourceHealthText.Source} ", unread.Title, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------- the badge's word for one reading

    /// <summary>The badge of a reading that is not there at all, and of every reading whose source is neither of the
    /// two the file writes or whose state is not one Resource Health gives: <c>Not known</c>. The nothing is not the
    /// empty word - a provisioning state of no letters is said as no letters, since the badge of a state the pipeline
    /// fell back to is the state, and nothing is invented over an empty one.</summary>
    [Fact]
    public void ABoxWithNoReadingOfItsOwnSaysNotKnown()
    {
        Assert.Equal("Not known", ResourceHealthText.Label(null));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("available", "ResourceHealth")));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("available", "resourcehealth")));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("available", "Resource Health")));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("available", "ProvisioningState")));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("available", string.Empty)));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("available", ResourceHealthReading.None)));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("", ResourceHealthReading.None)));
        Assert.Equal("Not known", ResourceHealthText.Label(Said("rebooting")));
        Assert.Equal("Not known", ResourceHealthText.Label(Said(string.Empty)));

        Assert.Equal(string.Empty, ResourceHealthText.Label(Said(string.Empty, ResourceHealthReading.FromProvisioningState)));
        Assert.NotEqual("Not known", ResourceHealthText.Label(Said(string.Empty, ResourceHealthReading.FromProvisioningState)));
    }

    /// <summary>The three states Resource Health gives and the provisioning state's <c>succeeded</c> are matched on the
    /// nail of their letters, because it is the reading of the file that lowers what the answer said and this member is
    /// handed what it is handed: a status or a source in another case is no longer one of them. <c>Available</c> with a
    /// capital is not <c>available</c> and says <c>Not known</c>; <c>Succeeded</c> is a provisioning state of its own
    /// name, said as <c>Succeeded</c> rather than turned into the badge word <c>Provisioned</c>; and a status padded at
    /// either end is neither trimmed into a match nor refused for it.</summary>
    [Theory]
    [InlineData("available", ResourceHealthReading.FromResourceHealth, "Available")]
    [InlineData("Available", ResourceHealthReading.FromResourceHealth, "Not known")]
    [InlineData("AVAILABLE", ResourceHealthReading.FromResourceHealth, "Not known")]
    [InlineData(" available", ResourceHealthReading.FromResourceHealth, "Not known")]
    [InlineData("available ", ResourceHealthReading.FromResourceHealth, "Not known")]
    [InlineData("succeeded", ResourceHealthReading.FromResourceHealth, "Not known")]
    [InlineData("unavailable", ResourceHealthReading.FromProvisioningState, "Unavailable")]
    [InlineData("succeeded", "ProvisioningState", "Not known")]
    [InlineData("succeeded", ResourceHealthReading.FromProvisioningState, "Provisioned")]
    [InlineData("Succeeded", ResourceHealthReading.FromProvisioningState, "Succeeded")]
    [InlineData("succeeded ", ResourceHealthReading.FromProvisioningState, "Succeeded ")]
    [InlineData("deleting", ResourceHealthReading.FromProvisioningState, "Deleting")]
    public void TheBadgeIsMatchedOnTheNailOfItsLetters(string status, string source, string label) =>
        Assert.Equal(label, ResourceHealthText.Label(Said(status, source)));

    /// <summary>The capitalising of a provisioning state, at its ends: no letters, one letter, a letter already
    /// capital, a digit, a mark, a whitespace at the front, an accent, a space within, and a letter that has no capital
    /// of its own to be turned to. Only the first letter is touched and nothing else is done to the word - not trimmed,
    /// not folded, not cut, not replaced by the word for nothing - so a state of no letters is a badge of no letters
    /// and a state that begins with a space is a badge that begins with a space.</summary>
    [Theory]
    [InlineData("", "")]
    [InlineData("f", "F")]
    [InlineData("failed", "Failed")]
    [InlineData("Failed", "Failed")]
    [InlineData("FAILED", "FAILED")]
    [InlineData("1st", "1st")]
    [InlineData("-", "-")]
    [InlineData(" creating", " creating")]
    [InlineData("\tcreating", "\tcreating")]
    [InlineData("creating now", "Creating now")]
    [InlineData("élan", "Élan")]
    [InlineData("ıtri", "ıtri")]
    public void AProvisioningStateIsCapitalizedAtItsFirstLetterOnly(string status, string label) =>
        Assert.Equal(label, ResourceHealthText.Label(Said(status, ResourceHealthReading.FromProvisioningState)));

    /// <summary>A status of any length is a badge of any length: the 200-letter cut belongs to the reading of the file
    /// and the 40-letter bound that refuses a status is the reading's own, so nothing of either reaches this member -
    /// the whole of what it is handed comes back with its first letter turned, markup and all.</summary>
    [Fact]
    public void AStatusOfAnyLengthIsSaidWholeAndCutByNothing()
    {
        foreach (var letters in new[] { 1, 2, 39, 40, 41, 200, 201, 1_000 })
        {
            var status = new string('a', letters);
            var label = ResourceHealthText.Label(Said(status, ResourceHealthReading.FromProvisioningState));

            Assert.Equal(letters, label.Length);
            Assert.Equal("A" + new string('a', letters - 1), label);
        }

        // A status the file would have refused, said straight to the badge: the bound is the reading's business.
        Assert.Equal("Not known", ResourceHealthText.Label(Said("<b>up</b>")));
        Assert.Equal("<b>up</b>", ResourceHealthText.Label(Said("<b>up</b>", ResourceHealthReading.FromProvisioningState)));
    }

    /// <summary>The badge follows no reader at all: the match on a status and on a source is the letters and not a
    /// culture's notion of sameness, and the one letter the badge turns is turned by the invariant rule - so a reader
    /// of German, and a reader whose language capitalises an <c>i</c> with a dot over it, are shown the same badge the
    /// file wrote: <c>Idle</c> with a plain <c>I</c>, and not the dotted capital that reader's own rules would make.</summary>
    [Fact]
    public void TheBadgeFollowsNoReaderAtAll()
    {
        var available = Said("available");
        var idle = Said("idle", ResourceHealthReading.FromProvisioningState);
        var accent = Said("élan", ResourceHealthReading.FromProvisioningState);

        foreach (var language in new[] { "de-DE", "tr-TR", "en-US" })
        {
            Assert.Equal("Available", AsAReaderOf(language, () => ResourceHealthText.Label(available)));
            Assert.Equal("Idle", AsAReaderOf(language, () => ResourceHealthText.Label(idle)));
            Assert.Equal("Élan", AsAReaderOf(language, () => ResourceHealthText.Label(accent)));
            Assert.Equal("Not known", AsAReaderOf(language, () => ResourceHealthText.Label(null)));
            Assert.Equal(string.Empty, AsAReaderOf(language, () => ResourceHealthText.Label(Said(string.Empty, ResourceHealthReading.FromProvisioningState))));
        }

        // The Turkish reader's own capital of an i is dotted; the badge's is, because it asks no reader.
        Assert.Equal("İ", "i".ToUpper(new CultureInfo("tr-TR")));
        Assert.DoesNotContain("İ", ResourceHealthText.Label(idle), StringComparison.Ordinal);
    }

    // ------------------------------------------------- the tone, on the very tick the reading turns two

    /// <summary>The tone on the hour the pipeline keeps a reading for: a box read exactly two hours ago is still
    /// <c>plain</c>, one read a tick older is <c>muted</c> and one a tick younger is <c>plain</c> - while the line says
    /// <c>2 h ago</c> on both sides of the turn, since the age counts whole minutes below two hours and whole hours
    /// from them up. The bound is <see cref="HourlyCheckText.Recent" />, which the box asks and never changes, and it
    /// is the oldest reading of a box of several that is asked, not the newest.</summary>
    [Fact]
    public void TheToneTurnsTheMomentTheOldestReadingPassesTwoHours()
    {
        var report = Report((Kv, Said("available")));

        var justUnder = ResourceHealthText.Of([Kv], report, Read + TimeSpan.FromHours(2) - TimeSpan.FromTicks(1), TimeZoneInfo.Utc)!;
        var onTheHour = ResourceHealthText.Of([Kv], report, Read + TimeSpan.FromHours(2), TimeZoneInfo.Utc)!;
        var justPast = ResourceHealthText.Of([Kv], report, Read + TimeSpan.FromHours(2) + TimeSpan.FromTicks(1), TimeZoneInfo.Utc)!;

        Assert.Equal(("Available", "Resource Health, 119 min ago", "plain"), (justUnder.Label, justUnder.Line, justUnder.Tone));
        Assert.Equal(("Available", "Resource Health, 2 h ago", "plain"), (onTheHour.Label, onTheHour.Line, onTheHour.Tone));
        Assert.Equal(("Available", "Resource Health, 2 h ago", "muted"), (justPast.Label, justPast.Line, justPast.Tone));

        // The same turn for a box of several, told by the older of its two readings while the newer is an hour young.
        var mixed = Report((Kv, Said("available")), (Identity, Said("succeeded", ResourceHealthReading.FromProvisioningState, at: Read + TimeSpan.FromHours(1))));
        var bound = ResourceHealthText.Of([Kv, Identity], mixed, Read + TimeSpan.FromHours(2), TimeZoneInfo.Utc)!;
        var over = ResourceHealthText.Of([Kv, Identity], mixed, Read + TimeSpan.FromHours(2) + TimeSpan.FromMinutes(1), TimeZoneInfo.Utc)!;

        Assert.Equal(("1 provisioned · 1 available, 2 h ago", "plain"), (bound.Line, bound.Tone));
        Assert.Equal(("1 provisioned · 1 available, 2 h ago", "muted"), (over.Line, over.Tone));
    }

    /// <summary>A reading of the very minute the box is told at, and a reading that stands ahead of that moment - the
    /// pipeline's clock and the page's not quite agreeing - are both <c>just now</c> and both <c>plain</c>: the age
    /// never goes negative and never says a length of time for a moment the clock has not reached, whatever the run of
    /// hours a reading stands in front of.</summary>
    [Fact]
    public void AReadingOfTheMinuteAndOneAheadOfTheClockAreBothSaidToBeNow()
    {
        var report = Report((Kv, Said("available")));

        foreach (var moment in new[] { Read, Read - TimeSpan.FromSeconds(59), Read - TimeSpan.FromMinutes(1), Read - TimeSpan.FromHours(3), Read - TimeSpan.FromDays(1) })
        {
            var words = ResourceHealthText.Of([Kv], report, moment, TimeZoneInfo.Utc)!;

            Assert.Equal(("Available", "Resource Health, just now", "plain"), (words.Label, words.Line, words.Tone));
        }

        // A box of several is plain on the same rule, and its age is the oldest reading's, not the one ahead of the clock.
        var several = Report((Kv, Said("available")), (Identity, Said("succeeded", ResourceHealthReading.FromProvisioningState, at: Read + TimeSpan.FromDays(3))));
        var box = ResourceHealthText.Of([Kv, Identity], several, Read, TimeZoneInfo.Utc)!;

        Assert.Equal(("1 provisioned · 1 available, just now", "plain"), (box.Line, box.Tone));
    }

    // ------------------------------------------------------------- the four words of a box, at their ends

    /// <summary>The names a tooltip gives are the last part of the id it was handed, cut at the slashes and trimmed of
    /// the ones it ends with: a trailing slash names the resource and not nothing, a bare word with no slash at all
    /// names itself, and an id that is a lone slash or no letters at all names nothing - a tooltip line that opens with
    /// its colon. None of them is refused, dropped or invented over.</summary>
    [Fact]
    public void TheNameOfAResourceIsTheLastPartOfTheIdItEndsWith()
    {
        var report = Report(("/s/apps/web/", Said("available")), ("kv", Said("available")), ("a/b/c", Said("available")));
        var ids = new[] { "/s/apps/web/", "kv", "a/b/c", "/", string.Empty };

        var words = ResourceHealthText.Of(ids, report, Now, TimeZoneInfo.Utc)!;
        var said = "available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago).";

        Assert.Equal(
            string.Join('\n', ResourceHealthText.Source, $"web: {said}", $"kv: {said}", $"c: {said}", ": not in the hourly reading.", ": not in the hourly reading."),
            words.Title);

        // The two names that are nothing still stand in the tooltip, and still count in the line under the badge.
        Assert.Equal(("Not known", "2 not known · 3 available, 40 min ago", "plain"), (words.Label, words.Line, words.Tone));
        Assert.Equal(6, words.Title.Split('\n').Length);
    }

    /// <summary>The two tooltip words for where a state came from that no test had the page say before: a provisioning
    /// state standing in for Resource Health having no answer, and <c>none</c> saying that neither of them answered.
    /// Both are said with the state, the time and the age, and with the file's words after them when the file had
    /// any - and a reading given the empty word as its summary is not one given nothing: the space before a summary
    /// stays behind, because the record keeps what it was handed and this member adds no rule of its own.</summary>
    [Fact]
    public void TheTooltipSaysWhereEachStateCameFromInItsOwnWords()
    {
        var report = Report(
            (Kv, Said("available", summary: "No incidents reported.")),
            (Identity, Said("succeeded", ResourceHealthReading.FromProvisioningState)),
            (App, Said("unknown", ResourceHealthReading.None, summary: "Resource Health does not cover this resource type.")));

        var words = ResourceHealthText.Of([Kv, Identity, App], report, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(
            string.Join(
                '\n',
                ResourceHealthText.Source,
                "kv: available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago). No incidents reported.",
                "id: succeeded (Resource Health has no answer for it; its provisioning state), read 2026-10-04 21:37 +00:00 (40 min ago).",
                "app: unknown (neither Resource Health nor a provisioning state answered), read 2026-10-04 21:37 +00:00 (40 min ago). Resource Health does not cover this resource type."),
            words.Title);

        var read = ResourceHealthText.Of([Kv], Report((Kv, Said("available"))), Now, TimeZoneInfo.Utc)!;
        var emptied = ResourceHealthText.Of([Kv], Report((Kv, Said("available", summary: string.Empty))), Now, TimeZoneInfo.Utc)!;

        Assert.Equal($"{ResourceHealthText.Source}\nkv: available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago).", read.Title);
        Assert.Equal($"{ResourceHealthText.Source}\nkv: available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago). ", emptied.Title);
        Assert.NotEqual(read.Title, emptied.Title);
    }

    /// <summary>The line of a box counts each id as many times as the box names it, and the tooltip names it as often:
    /// a manifest that lists one resource twice is a box of two readings of it, its worst word first. Nothing here
    /// de-duplicates an id, folds a reading or shortens the tooltip to the resources that are distinct - and a box of
    /// one id told twice is a box of several, said with its counts rather than with the source of its one reading.</summary>
    [Fact]
    public void AnIdToldTwiceIsCountedTwiceAndNamedTwice()
    {
        var report = Report((Kv, Said("available")), (Identity, Said("succeeded", ResourceHealthReading.FromProvisioningState)));

        var twice = ResourceHealthText.Of([Kv, Kv, Identity], report, Now, TimeZoneInfo.Utc)!;
        var once = ResourceHealthText.Of([Kv, Identity], report, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Provisioned", "1 provisioned · 2 available, 40 min ago", "plain"), (twice.Label, twice.Line, twice.Tone));
        Assert.Equal(("Provisioned", "1 provisioned · 1 available, 40 min ago", "plain"), (once.Label, once.Line, once.Tone));
        Assert.Equal(4, twice.Title.Split('\n').Length);
        Assert.Equal(2, twice.Title.Split('\n').Count(line => line.StartsWith("kv: ", StringComparison.Ordinal)));
        Assert.NotEqual(twice, once);

        var pair = ResourceHealthText.Of([Kv, Kv], report, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Available", "2 available, 40 min ago", "plain"), (pair.Label, pair.Line, pair.Tone));
    }

    /// <summary>A box whose every id the file does not name has no reading to take an age from, so its line says no age
    /// at all, its tone is the grey of nothing recent, its badge is <c>Not known</c> and its tooltip is one line of
    /// not-in-the-reading per id: a report that is there and says nothing is one case, and no report at all another.</summary>
    [Fact]
    public void ABoxWhoseEveryIdIsMissingSaysNoAgeAndGoesGrey()
    {
        var words = ResourceHealthText.Of(["/s/a", "/s/b"], NothingRead, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Not known", "2 not known", "muted"), (words.Label, words.Line, words.Tone));
        Assert.Equal(
            $"{ResourceHealthText.Source}\na: not in the hourly reading.\nb: not in the hourly reading.",
            words.Title);

        // The same ids, one of them read after all: the age the box has now is that reading's.
        var read = ResourceHealthText.Of(["/s/a", "/s/b"], Report(("/s/a", Said("available"))), Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Not known", "1 not known · 1 available, 40 min ago", "plain"), (read.Label, read.Line, read.Tone));
    }

    /// <summary>The eight resources a tooltip names, and no more: eight ids fill the tooltip with their own lines and
    /// say nothing of a tail, nine add the <c>And 1 more.</c> that was not there a line before, and eleven name the
    /// first eight in the order the box was handed them and count the three they leave out - while the line under the
    /// badge still counts all eleven, and twenty-two still leave fourteen of them unnamed.</summary>
    [Fact]
    public void ATooltipNamesEightResourcesAndCountsTheRest()
    {
        static string Storage(int n) => $"/subscriptions/s/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/st{n}";
        static string Named(int n) => $"st{n}: available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago).";

        (string Id, ResourceHealthReading Reading)[] readings = [.. Enumerable.Range(1, 11).Select(n => (Id: Storage(n), Reading: Said("available")))];
        var report = Report(readings);
        var ids = Enumerable.Range(1, 11).Select(Storage).ToList();

        var eight = ResourceHealthText.Of([.. ids.Take(8)], report, Now, TimeZoneInfo.Utc)!;
        var nine = ResourceHealthText.Of([.. ids.Take(9)], report, Now, TimeZoneInfo.Utc)!;
        var eleven = ResourceHealthText.Of(ids, report, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(string.Join('\n', [ResourceHealthText.Source, .. Enumerable.Range(1, 8).Select(Named)]), eight.Title);
        Assert.Equal(9, eight.Title.Split('\n').Length);
        Assert.DoesNotContain("And ", eight.Title, StringComparison.Ordinal);
        Assert.Equal("8 available, 40 min ago", eight.Line);

        Assert.Equal(string.Join('\n', [ResourceHealthText.Source, .. Enumerable.Range(1, 8).Select(Named), "And 1 more."]), nine.Title);
        Assert.Equal(string.Join('\n', [ResourceHealthText.Source, .. Enumerable.Range(1, 8).Select(Named), "And 3 more."]), eleven.Title);
        Assert.Equal("11 available, 40 min ago", eleven.Line);
        Assert.DoesNotContain("st9", eleven.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("st10", eleven.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("st11", eleven.Title, StringComparison.Ordinal);

        // The tail is counted and never named, and the count says the ids the box was handed, not the distinct ones.
        var doubled = ResourceHealthText.Of([.. ids, .. ids], report, Now, TimeZoneInfo.Utc)!;

        Assert.EndsWith("And 14 more.", doubled.Title, StringComparison.Ordinal);
        Assert.Equal(10, doubled.Title.Split('\n').Length);
        Assert.Equal("22 available, 40 min ago", doubled.Line);
        Assert.Equal("Available", doubled.Label);
    }

    /// <summary>The tooltip tells its hours in the zone it is handed and nowhere else: the same reading, the same
    /// instant, a day ahead of the file's own date five and a half hours east of the meridian and the same date five
    /// hours west of it - while the age, which is a length of time and not a place, says the forty minutes it always
    /// says and the badge and the tone stand where they stood.</summary>
    [Fact]
    public void TheTooltipTellsItsHoursInTheZoneItIsGiven()
    {
        var report = Report((Kv, Said("available", summary: "No incidents reported.")));

        var east = ResourceHealthText.Of([Kv], report, Now, East)!;
        var west = ResourceHealthText.Of([Kv], report, Now, West)!;
        var utc = ResourceHealthText.Of([Kv], report, Now, TimeZoneInfo.Utc)!;

        Assert.Equal($"{ResourceHealthText.Source}\nkv: available (by Resource Health), read 2026-10-05 03:07 +05:30 (40 min ago). No incidents reported.", east.Title);
        Assert.Equal($"{ResourceHealthText.Source}\nkv: available (by Resource Health), read 2026-10-04 16:37 -05:00 (40 min ago). No incidents reported.", west.Title);
        Assert.Equal($"{ResourceHealthText.Source}\nkv: available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago). No incidents reported.", utc.Title);

        Assert.Equal(utc.Line, east.Line);
        Assert.Equal(utc.Tone, west.Tone);
        Assert.Equal(utc.Label, east.Label);
    }

    /// <summary>An id that is not there at all inside the box's list: a box the page has no file for is still answered
    /// in words, because nothing is looked up, while a box with a file in hand asks that file for the nothing - and the
    /// file's own look-up refuses it by the name of the part it is missing rather than saying
    /// <c>not in the hourly reading</c>. The empty word and a word of whitespace, both of them ids a manifest can hold,
    /// are asked like any other and answer as missing ones - and the whitespace is told in the tooltip as it stands,
    /// since the only cut the name is given is at the slashes.</summary>
    [Fact]
    public void AnIdThatIsNotThereAtAllIsRefusedByTheLookUpItIsAskedWith()
    {
        var report = Report((Kv, Said("available")));

        var noFile = ResourceHealthText.Of([null!], null, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Not known", "no Resource Health read yet", "muted"), (noFile.Label, noFile.Line, noFile.Tone));
        Assert.StartsWith($"{ResourceHealthText.Source} ", noFile.Title, StringComparison.Ordinal);

        var threw = Assert.Throws<ArgumentNullException>(() => ResourceHealthText.Of([null!], report, Now, TimeZoneInfo.Utc));

        Assert.Equal("key", threw.ParamName);

        // The empty word is an id like any other to the look-up: nothing in it, and so nothing found by it.
        var blanks = ResourceHealthText.Of([string.Empty, "   "], report, Now, TimeZoneInfo.Utc)!;

        Assert.Equal(("Not known", "2 not known", "muted"), (blanks.Label, blanks.Line, blanks.Tone));
        Assert.Equal($"{ResourceHealthText.Source}\n: not in the hourly reading.\n   : not in the hourly reading.", blanks.Title);
    }

    // ------------------------------------------------------------- the class's own shape, and nothing else

    /// <summary>The whole public surface of the class, as it stands: the fixed first words of every tooltip, the one
    /// way of making a box's four words and the one way of naming a reading on its own - and nothing else to call. A
    /// word added to the class has to be written here as well, which is the point: nothing here reaches Azure, a clock
    /// or a file, and a fourth word nobody decided the meaning of would show itself in this list before the page.</summary>
    [Fact]
    public void TheSurfaceIsTheThreeWordsTheClassSaysAndNothingElse()
    {
        var type = typeof(ResourceHealthText);

        Assert.True(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed, "the words of a box are a static class, not a thing to hold");
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Empty(type.GetInterfaces());
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(["Label", "Of", "Source"], PublicSurface());

        var of = type.GetMethod("Of")!;
        var label = type.GetMethod("Label")!;

        Assert.Equal(1, Overloads("Of"));
        Assert.Equal(1, Overloads("Label"));
        Assert.All(new[] { of, label }, method =>
        {
            Assert.True(method.IsStatic);
            Assert.True(method.IsPublic);
            Assert.False(method.IsGenericMethod);
            Assert.False(method.ReturnType.Name.StartsWith("Task", StringComparison.Ordinal));
            Assert.False(method.ReturnType.Name.StartsWith("ValueTask", StringComparison.Ordinal));
            Assert.DoesNotContain(method.GetParameters(), parameter => parameter.ParameterType == typeof(CancellationToken));
        });

        // The box's four words out of ids, a report and two moments of the caller's, and the badge out of one reading:
        // no address, no client, and no default of a time that would go and ask the machine what o'clock it is.
        Assert.Equal(typeof(ResourceHealthWords), of.ReturnType);
        Assert.Equal(["ids", "report", "now", "zone"], of.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal([typeof(IReadOnlyList<string>), typeof(ResourceHealthReport), typeof(DateTimeOffset?), typeof(TimeZoneInfo)], of.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.All(of.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue, $"{parameter.Name} is asked with a default of its own"));

        Assert.Equal(typeof(string), label.ReturnType);
        Assert.Equal(typeof(ResourceHealthReading), Assert.Single(label.GetParameters()).ParameterType);

        // Nothing of the page's own kept between one box and the next: only the constants are statics, and they are literal.
        Assert.DoesNotContain(type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly), field => !field.IsLiteral);
    }

    // ------------------------------------------------------------------------------ the machinery

    /// <summary>The public members the class declares itself: its constants and its methods, the compiler's own
    /// getters walked past, in the order a reader of the surface finds them.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ResourceHealthText)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>How many public methods of the class carry the given name.</summary>
    private static int Overloads(string name) =>
        typeof(ResourceHealthText)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Count(method => method.Name.Equals(name, StringComparison.Ordinal));

    /// <summary>Asking for a badge under a reader whose language turns letters differently - and handing the thread's
    /// own culture back afterwards, whatever the asking did.</summary>
    private static T AsAReaderOf<T>(string language, Func<T> what)
    {
        ArgumentNullException.ThrowIfNull(what);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            return what();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
