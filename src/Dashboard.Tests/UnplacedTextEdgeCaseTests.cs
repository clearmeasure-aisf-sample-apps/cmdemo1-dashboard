using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="UnplacedText" /> - the list "In no environment" under
/// the runtime diagram: the resources the manifest puts in no environment, ordered by type and then by name, each with
/// what Resource Health last said of it in the words of the diagram's boxes. The class has four public members and
/// every one of them is walked to its end: <see cref="UnplacedText.NotRead" /> and
/// <see cref="UnplacedText.NotReadTitle" />, the two fixed words of an entry the manifest gives no resource id for;
/// <see cref="UnplacedText.Summary" />, the one line of the closed list; and <see cref="UnplacedText.Rows" />, the
/// rows themselves.
/// <para>
/// What is pinned elsewhere is walked past on purpose. <see cref="UnplacedTests" /> pins the reading of the list out of
/// the manifest's JSON - an entry of no name skipped, a field of the wrong type left out, a hostile string kept as the
/// text it is, the cap of 500 and the cut of 300 letters, an address that is no absolute https address dropped - and, of
/// this class, that a manifest without the list and one with an empty list both give no rows, that a type-less entry
/// comes after the types while the rest go by type and then by name, that a row says what Resource Health said in the
/// words of a box joined with <c>·</c>, that the five states a file can answer with reach the row, that the summary of
/// five typed entries reads <c>In no environment (5)</c>, and that <see cref="UnplacedText.NotRead" /> is what an entry
/// of no ids says. <see cref="UnplacedRowTests" /> pins <see cref="UnplacedRow" /> itself, the record every row is: its
/// four members, an old reading going <c>muted</c>, a reading told with no time, an id the file does not name, no file
/// at all, the empty id list being the absent one, and the equality, the copy and the words of a row.
/// <see cref="ResourceHealthTextEdgeCaseTests" /> and <see cref="ResourceHealthWordsEdgeCaseTests" /> pin the words this
/// class hands over - the tone on the very tick a reading turns two hours, the zone the tooltip is told in, the eight
/// resources a tooltip names before it counts the rest, and a box of no ids saying nothing at all.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the two fixed words being compile-time constants of the class, letter for
/// letter, said in no colour and no markup and none of them the dash the page writes elsewhere, and the tooltip of an
/// entry of no id staying itself whatever zone, report or moment the list is told at; the count of the summary standing
/// at zero, at one, at the tens and at both ends of the number line, negative included, and written in invariant digits
/// - no reader's thousands separator, no reader's shape of a digit, a Turkish, a German and an Arabic one included; the
/// two refusals of <see cref="UnplacedText.Rows" />, a manifest that is not there and a zone that is not there, each
/// answered by name, the manifest first of the two and the zone refused before the list is looked at, so that a manifest
/// with nothing in it is refused the very same way a full one is; the order being the ordinal one at the end of the
/// name, where a comparison that stopped at a culture's notion of sameness would have let the manifest's own order
/// stand, and an empty type being a type and not the absence of one; a tie in both of the words the list orders by
/// keeping the order the manifest wrote, and the manifest's own list coming back unmoved; every entry of a long list
/// coming out exactly once and the closed line counting it; the report, the moment and the zone being handed to every
/// row rather than to the first; and the whole public surface of the class, so that the day a fifth member is added the
/// list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <see cref="UnplacedText" /> is static,
/// holds no <see cref="HttpClient" />, no address and no moment of its own, and every manifest, report, moment and zone
/// below is one the test brought - the entries built straight from <see cref="RuntimeUnplaced" />, so that this file
/// says nothing of the manifest's JSON, which the files above own. Where the class does ask a clock, it asks the one it
/// is handed, so the whole file is said of fixed inputs and says the same to whoever runs it.
/// </para>
/// </summary>
public class UnplacedTextEdgeCaseTests
{
    /// <summary>The moment the pipeline read what it read.</summary>
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    /// <summary>Forty minutes on: the moment the list is told at unless a test says otherwise.</summary>
    private static readonly DateTimeOffset Now = Read + TimeSpan.FromMinutes(40);

    /// <summary>A zone a day ahead of the reading's own date, and one behind it.</summary>
    private static readonly TimeZoneInfo East = TimeZoneInfo.CreateCustomTimeZone("east-five-thirty", TimeSpan.FromMinutes(330), "East", "East");

    private static readonly TimeZoneInfo West = TimeZoneInfo.CreateCustomTimeZone("west-five", TimeSpan.FromHours(-5), "West", "West");

    private const string KeyVault = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv";
    private const string Registry = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.ContainerRegistry/registries/acr";
    private const string Other = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/st";
    private const string Nic = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.Network/networkInterfaces/ni";

    /// <summary>The only two tones the page dresses a row in, and the words a row must never wear in their place.</summary>
    private static readonly string[] Tones = ["plain", "muted"];

    private static readonly string[] Colours = ["red", "green", "amber", "grey", "gray", "blue", "yellow", "colour", "color"];

    // ---------------------------------------------------------------------------- the two fixed words of no id

    /// <summary>The two words an entry the manifest gives no resource id for wears, letter for letter: <c>not read</c>
    /// where the health stands, and the whole sentence of the tooltip. Both are constants of the class -
    /// <c>IsLiteral</c>, not a <c>static readonly</c> field a caller could find holding another telling - and neither
    /// says a colour, a date, a time or a markup tag, because there is nothing read to say one of. The health is two
    /// lowercase words and no separator, since it is said alone and never joined to a source with the
    /// <c>·</c> the rows that have a reading are written with; the tooltip is one sentence that ends, blames the
    /// manifest, and points at Azure rather than at the file the pipeline publishes.</summary>
    [Fact]
    public void TheTwoWordsOfAnEntryWithNoIdAreFixedAndSayNothingElse()
    {
        const string Words = "not read";
        const string Tip = "The manifest has no resource id for it, so the page cannot say what Azure Resource Health said of it.";

        Assert.Equal(Words, UnplacedText.NotRead);
        Assert.Equal(Tip, UnplacedText.NotReadTitle);
        Assert.NotEqual(UnplacedText.NotRead, UnplacedText.NotReadTitle);

        foreach (var (name, value) in new[] { ("NotRead", UnplacedText.NotRead), ("NotReadTitle", UnplacedText.NotReadTitle) })
        {
            var field = typeof(UnplacedText).GetField(name, BindingFlags.Public | BindingFlags.Static)!;

            Assert.True(field.IsPublic);
            Assert.True(field.IsStatic);
            Assert.True(field.IsLiteral, $"{name} is a constant of the class, not a field that could hold a value of its own");
            Assert.False(field.IsInitOnly);
            Assert.Equal(typeof(string), field.FieldType);
            Assert.Equal(value, (string)field.GetRawConstantValue()!);

            // One line, said whole, trimmed, and never the dash the page writes for a fact it does not have.
            Assert.DoesNotContain("\n", value, StringComparison.Ordinal);
            Assert.DoesNotContain("\r", value, StringComparison.Ordinal);
            Assert.DoesNotContain("  ", value, StringComparison.Ordinal);
            Assert.Equal(value, value.Trim());
            Assert.DoesNotContain("-", value, StringComparison.Ordinal);
            Assert.DoesNotContain("<", value, StringComparison.Ordinal);
            Assert.DoesNotContain(">", value, StringComparison.Ordinal);
            foreach (var colour in Colours)
            {
                Assert.DoesNotContain(colour, value, StringComparison.OrdinalIgnoreCase);
            }
        }

        Assert.Equal(UnplacedText.NotRead, UnplacedText.NotRead.ToLowerInvariant());
        Assert.Equal(2, UnplacedText.NotRead.Split(' ').Length);
        Assert.DoesNotContain("·", UnplacedText.NotRead, StringComparison.Ordinal);
        Assert.DoesNotContain(".", UnplacedText.NotRead, StringComparison.Ordinal);

        Assert.EndsWith(".", UnplacedText.NotReadTitle, StringComparison.Ordinal);
        Assert.Single(UnplacedText.NotReadTitle.Split('.', StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains("resource id", UnplacedText.NotReadTitle, StringComparison.Ordinal);
        Assert.Contains("manifest", UnplacedText.NotReadTitle, StringComparison.Ordinal);
        Assert.Contains("Azure Resource Health", UnplacedText.NotReadTitle, StringComparison.Ordinal);
        Assert.DoesNotContain("containers.json", UnplacedText.NotReadTitle, StringComparison.Ordinal);
    }

    /// <summary>The two fixed words are what a row is actually built out of, and nothing of the rest of the list
    /// disturbs them: an entry of no id among two that have one, told at three different moments and in three zones,
    /// with a report that names one of its neighbours and not the other. Its tone is the grey of nothing recent, its
    /// health and its tooltip are the constants, letter for letter, and the neighbours are told in the words of the
    /// report beside them.</summary>
    [Fact]
    public void TheFixedWordsAreTheOnesAnEntryWithNoIdWears()
    {
        var manifest = Manifest(
            new RuntimeUnplaced("acr", Type: "Container registry", Ids: [Registry]),
            new RuntimeUnplaced("kv", Type: "Key vault"),
            new RuntimeUnplaced("st", Type: "Storage account", Ids: [Other]));
        var report = Report((Registry, Said("available")), (Other, Said("unavailable")));

        foreach (var now in new DateTimeOffset?[] { Now, Read + TimeSpan.FromHours(30), null })
        {
            foreach (var zone in new[] { TimeZoneInfo.Utc, East, West })
            {
                var rows = UnplacedText.Rows(manifest, report, now, zone);

                Assert.Equal(3, rows.Count);
                Assert.Equal(["acr", "kv", "st"], rows.Select(row => row.Entry.Name));

                // The entry with no id: the two fixed words, and nothing of the report, the moment or the zone in them.
                Assert.Equal(UnplacedText.NotRead, rows[1].Health);
                Assert.Equal(UnplacedText.NotReadTitle, rows[1].HealthTitle);
                Assert.Equal("muted", rows[1].HealthTone);

                // Its neighbours are told from the report, which names both of them.
                Assert.StartsWith("Available · Resource Health", rows[0].Health, StringComparison.Ordinal);
                Assert.StartsWith("Unavailable · Resource Health", rows[2].Health, StringComparison.Ordinal);
            }
        }

        // Told forty minutes on, the two neighbours say the age the report and the moment make between them.
        var told = UnplacedText.Rows(manifest, report, Now, TimeZoneInfo.Utc);

        Assert.Equal(["Available · Resource Health, 40 min ago", UnplacedText.NotRead, "Unavailable · Resource Health, 40 min ago"], told.Select(row => row.Health));

        // No clock, no file: the entry of no id says the same two words as the one the page has no reading for says its own.
        var blind = UnplacedText.Rows(manifest, null, null, TimeZoneInfo.Utc);

        Assert.Equal(UnplacedText.NotRead, blind[1].Health);
        Assert.Equal(UnplacedText.NotReadTitle, blind[1].HealthTitle);
        Assert.Equal("Not known · no Resource Health read yet", blind[0].Health);
    }

    // ----------------------------------------------------------------------- the one line of the closed list

    /// <summary>The line of a closed list, at the ends of the count it carries: nothing in no environment is said as
    /// <c>In no environment (0)</c> rather than left unsaid or said as no line at all, one is <c>(1)</c> and never
    /// <c>(1s)</c>, the tens, the hundreds and the thousands keep their letters unbroken, and the number line's two ends
    /// and the negative half - which no list can hold and this member does not refuse - are written out whole rather
    /// than clamped, cut or turned into the word for nothing.</summary>
    [Theory]
    [InlineData(0, "In no environment (0)")]
    [InlineData(1, "In no environment (1)")]
    [InlineData(2, "In no environment (2)")]
    [InlineData(9, "In no environment (9)")]
    [InlineData(10, "In no environment (10)")]
    [InlineData(99, "In no environment (99)")]
    [InlineData(100, "In no environment (100)")]
    [InlineData(500, "In no environment (500)")]
    [InlineData(501, "In no environment (501)")]
    [InlineData(1000, "In no environment (1000)")]
    [InlineData(1234567, "In no environment (1234567)")]
    [InlineData(-1, "In no environment (-1)")]
    [InlineData(-500, "In no environment (-500)")]
    [InlineData(int.MaxValue, "In no environment (2147483647)")]
    [InlineData(int.MinValue, "In no environment (-2147483648)")]
    public void TheCountOfTheLineStandsWhereTheNumberLineEnds(int count, string expected) =>
        Assert.Equal(expected, UnplacedText.Summary(count));

    /// <summary>The count is written in invariant digits: no reader's thousands separator, no reader's shape of a
    /// digit, no reader's minus sign. The very same lines come back under a German reader, who groups a thousand with a
    /// point, an American one, who groups it with a comma, a Turkish one, whose language cases <c>i</c> and <c>I</c>
    /// apart, and an Arabic one, whose own digits are not the Latin ones - and the words around the count are one fixed
    /// sentence with the number inside brackets at its end.</summary>
    [Fact]
    public void TheLineIsWrittenInInvariantDigitsAndFollowsNoReader()
    {
        foreach (var language in new[] { "de-DE", "en-US", "fr-FR", "tr-TR", "ar-EG", "hi-IN" })
        {
            Assert.Equal("In no environment (1234)", AsAReaderOf(language, () => UnplacedText.Summary(1234)));
            Assert.Equal("In no environment (0)", AsAReaderOf(language, () => UnplacedText.Summary(0)));
            Assert.Equal("In no environment (-1)", AsAReaderOf(language, () => UnplacedText.Summary(-1)));
            Assert.Equal("In no environment (1000000)", AsAReaderOf(language, () => UnplacedText.Summary(1_000_000)));
        }

        var line = UnplacedText.Summary(1234567);

        Assert.DoesNotContain(",", line, StringComparison.Ordinal);
        Assert.DoesNotContain(".", line, StringComparison.Ordinal);
        Assert.Equal("In no environment (1234567)", line);
        Assert.All(line.SkipWhile(character => character != '(').Skip(1).Take(7), character => Assert.True(char.IsAsciiDigit(character), $"{character} is no ASCII digit"));
        Assert.StartsWith("In no environment (", line, StringComparison.Ordinal);
        Assert.EndsWith(")", line, StringComparison.Ordinal);
        Assert.Equal(line, UnplacedText.Summary(1234567));
    }

    /// <summary>The line is one line and carries nothing but the count: no second bracket, no markup, no colour, no
    /// word for health or state - the page puts it in a <c>&lt;summary&gt;</c> and has nothing else to say there - and
    /// the fixed words stand at both ends of it whatever the count between them.</summary>
    [Fact]
    public void TheLineIsOneLineAndCarriesNothingButTheCount()
    {
        var line = UnplacedText.Summary(7);

        Assert.Equal("In no environment (7)", line);
        Assert.DoesNotContain("\n", line, StringComparison.Ordinal);
        Assert.DoesNotContain("<", line, StringComparison.Ordinal);
        Assert.DoesNotContain(">", line, StringComparison.Ordinal);
        Assert.DoesNotContain("resource", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("healthy", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not read", line, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, line.Split('(').Length);
        Assert.Equal(1, line.Count(character => character == '('));
        Assert.Equal(1, line.Count(character => character == ')'));
        Assert.StartsWith("In no environment (", line, StringComparison.Ordinal);

        foreach (var count in new[] { 0, 1, 42, 500 })
        {
            var said = UnplacedText.Summary(count);

            Assert.Equal($"In no environment ({count})", said);
            Assert.StartsWith("In no environment (", said, StringComparison.Ordinal);
            Assert.EndsWith($"{count})", said, StringComparison.Ordinal);
        }
    }

    // ------------------------------------------------------------------------- the two refusals of the list

    /// <summary>The two arguments <see cref="UnplacedText.Rows" /> will not work without, refused by name rather than
    /// answered with a row of nothing: a manifest that is not there, and a zone that is not there. The manifest is
    /// refused first of the two, and the zone is refused before the list is looked at - so a manifest that holds no
    /// unplaced list, and one that holds an empty list, are refused the very same way a full one is rather than quietly
    /// answering with no rows. The report and the moment, both of them optional, are not refused: a page that has read
    /// no file, and has no clock of its own, still gets its rows.</summary>
    [Fact]
    public void RowsRefuseAManifestOrAZoneThatIsNotThereAndNoOtherNothing()
    {
        var manifest = Manifest(new RuntimeUnplaced("kv", Type: "Key vault", Ids: [KeyVault]));
        var report = Report((KeyVault, Said("available")));

        var noManifest = Assert.Throws<ArgumentNullException>(() => UnplacedText.Rows(null!, report, Now, TimeZoneInfo.Utc));
        var neither = Assert.Throws<ArgumentNullException>(() => UnplacedText.Rows(null!, null, null, null!));

        Assert.Equal("manifest", noManifest.ParamName);
        Assert.Equal("manifest", neither.ParamName);

        var noZone = Assert.Throws<ArgumentNullException>(() => UnplacedText.Rows(manifest, report, Now, null!));

        Assert.Equal("zone", noZone.ParamName);

        // The list is not looked at first: an empty one is refused like a full one, in both of its empty shapes.
        foreach (var empty in new[] { NoList(), Manifest([]) })
        {
            var refused = Assert.Throws<ArgumentNullException>(() => UnplacedText.Rows(empty, null, null, null!));

            Assert.Equal("zone", refused.ParamName);
        }

        // The report's absence and the moment's are both worked with, and neither is refused.
        Assert.Single(UnplacedText.Rows(manifest, null, null, TimeZoneInfo.Utc));
        Assert.Single(UnplacedText.Rows(manifest, report, null, TimeZoneInfo.Utc));
        Assert.Single(UnplacedText.Rows(manifest, null, Now, TimeZoneInfo.Utc));

        // And the manifest may be missing the list in either of its two ways without a word of complaint.
        Assert.Empty(UnplacedText.Rows(NoList(), null, null, TimeZoneInfo.Utc));
        Assert.Empty(UnplacedText.Rows(Manifest([]), report, Now, East));
    }

    // --------------------------------------------------------------------------------------- the order of rows

    /// <summary>Two entries whose names are the same letters in another case are told by their ordinal values, the last
    /// of the four keys the list is ordered by: the all-caps name, then the capitalised one, then the lowercase one.
    /// Nothing of that is decided by the order the manifest wrote them in - they are handed over in the reverse of the
    /// telling, so a comparison that let the case-differing names stand equal and stopped there would give the reverse
    /// order back - and a reader whose own language cases <c>i</c> and <c>I</c> apart is shown the same order as anyone
    /// else, because the comparison asks no reader.</summary>
    [Fact]
    public void TwoNamesOfTheSameLettersInAnotherCaseAreToldByTheirOrdinalValues()
    {
        var manifest = Manifest(
            new RuntimeUnplaced("image", Type: "Container registry"),
            new RuntimeUnplaced("Image", Type: "Container registry"),
            new RuntimeUnplaced("acr", Type: "Container registry"),
            new RuntimeUnplaced("Acr", Type: "Container registry"),
            new RuntimeUnplaced("ACR", Type: "Container registry"));

        var rows = UnplacedText.Rows(manifest, null, Now, TimeZoneInfo.Utc);
        var read = AsAReaderOf("tr-TR", () => UnplacedText.Rows(manifest, null, Now, TimeZoneInfo.Utc).Select(row => row.Entry.Name).ToList());

        Assert.Equal(["ACR", "Acr", "acr", "Image", "image"], rows.Select(row => row.Entry.Name));
        Assert.Equal(["ACR", "Acr", "acr", "Image", "image"], read);
        Assert.All(rows, row => Assert.Equal("Container registry", row.Entry.Type));
    }

    /// <summary>The types are grouped without regard to case, so a manifest that writes <c>Key Vault</c> here and
    /// <c>key vault</c> there puts the two in one group - and within it the names order, which is why the entry called
    /// <c>a</c> comes before the one called <c>b</c> below although its own type is the one spelled in lowercase. An
    /// empty type is a type: it is not the absence of one, so it groups with the types rather than after them and orders
    /// before every named one. Only the entries with no type at all come last, and in their own name order.</summary>
    [Fact]
    public void AnEmptyTypeIsATypeAndTheUntypedEntriesComeAfterTheTypes()
    {
        var manifest = Manifest(
            new RuntimeUnplaced("untyped-zulu", Type: null),
            new RuntimeUnplaced("b", Type: "Key Vault"),
            new RuntimeUnplaced("z", Type: string.Empty),
            new RuntimeUnplaced("a", Type: "key vault"),
            new RuntimeUnplaced("untyped-alpha", Type: null));

        var rows = UnplacedText.Rows(manifest, null, Now, TimeZoneInfo.Utc);

        Assert.Equal(["z", "a", "b", "untyped-alpha", "untyped-zulu"], rows.Select(row => row.Entry.Name));
        Assert.Equal([string.Empty, "key vault", "Key Vault", "(no type)", "(no type)"], rows.Select(row => row.Entry.Type ?? "(no type)"));
        Assert.Equal(string.Empty, rows[0].Entry.Type);
        Assert.Null(rows[3].Entry.Type);
        Assert.Equal("In no environment (5)", UnplacedText.Summary(rows.Count));
    }

    /// <summary>A tie in both of the words the list orders by - the same type and the same name - is broken by nothing:
    /// the entries are told in the order the manifest wrote them, and the same two handed over the other way round come
    /// back the other way round. Two entries that differ only in the words they are in no environment for are two rows,
    /// neither folded into the other, and each row carries the very entry it was written for. The list itself is read
    /// and never rearranged.</summary>
    [Fact]
    public void TwoEntriesOfOneTypeAndOneNameKeepTheOrderTheManifestWroteThemIn()
    {
        var first = new RuntimeUnplaced("kv", Type: "Key vault", Group: "rg-first", Words: "no environment tag");
        var second = new RuntimeUnplaced("kv", Type: "Key vault", Group: "rg-second", Words: "tagged uat: no such environment");

        var forwards = UnplacedText.Rows(Manifest(first, second), null, Now, TimeZoneInfo.Utc);
        var backwards = UnplacedText.Rows(Manifest(second, first), null, Now, TimeZoneInfo.Utc);

        Assert.Equal(2, forwards.Count);
        Assert.Equal(["rg-first", "rg-second"], forwards.Select(row => row.Entry.Group));
        Assert.Equal(["rg-second", "rg-first"], backwards.Select(row => row.Entry.Group));
        Assert.Same(first, forwards[0].Entry);
        Assert.Same(second, forwards[1].Entry);
        Assert.NotEqual(forwards[0], forwards[1]);
        Assert.Equal("In no environment (2)", UnplacedText.Summary(forwards.Count));

        // The manifest's own list comes back as it went in: the order is worked out, never written over.
        var source = new List<RuntimeUnplaced> { second, first };

        Assert.Equal(["rg-second", "rg-first"], UnplacedText.Rows(new RuntimeManifest("tdd", [], [], [], null, source), null, Now, TimeZoneInfo.Utc).Select(row => row.Entry.Group));
        Assert.Equal([second, first], source);
    }

    /// <summary>A long list comes out whole: every entry of it exactly once, in the order the four keys give, and the
    /// closed line counting the number of rows. Nothing is cut, capped or deduplicated here - the cap of 500 and the cut
    /// of 300 letters belong to the manifest's reader - so 257 entries written backwards and typed in a rotating order
    /// are told back as 257 rows, grouped by type and named in order within each group.</summary>
    [Fact]
    public void ALongListComesOutWholeAndSaysItsOwnCount()
    {
        const int Count = 257;

        var entries = Enumerable.Range(0, Count).Select(number => new RuntimeUnplaced(
            Name: $"r{(Count - number - 1):D3}",
            Type: (number % 3) switch
            {
                0 => "Key vault",
                1 => "Container registry",
                _ => "Storage account",
            })).ToList();

        var rows = UnplacedText.Rows(new RuntimeManifest("tdd", [], [], [], null, entries), null, Now, TimeZoneInfo.Utc);

        Assert.Equal(Count, rows.Count);
        Assert.Equal(Count, rows.Select(row => row.Entry.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("In no environment (257)", UnplacedText.Summary(rows.Count));
        Assert.All(rows, row => Assert.Equal(UnplacedText.NotRead, row.Health));

        // Every third name falls in one type, so each group is three letters apart and the whole list is the three of
        // them, told type by type: registries, vaults, storage accounts.
        Assert.Equal(["r000", "r003", "r006", "r009"], rows.Where(row => row.Entry.Type == "Container registry").Select(row => row.Entry.Name).Take(4));
        Assert.Equal(["r001", "r004", "r007", "r010"], rows.Where(row => row.Entry.Type == "Key vault").Select(row => row.Entry.Name).Take(4));
        Assert.Equal(["r002", "r005", "r008", "r011"], rows.Where(row => row.Entry.Type == "Storage account").Select(row => row.Entry.Name).Take(4));
        Assert.Equal("r000", rows[0].Entry.Name);
        Assert.Equal("r254", rows[^1].Entry.Name);
        Assert.Equal(86, rows.Count(row => row.Entry.Type == "Key vault"));
        Assert.Equal(86, rows.Count(row => row.Entry.Type == "Container registry"));
        Assert.Equal(85, rows.Count(row => row.Entry.Type == "Storage account"));
    }

    // ------------------------------------------------------------------- the report, the moment and the zone

    /// <summary>The report, the moment and the zone are handed to every row and not to the first of them: the same four
    /// entries, told at the very same instant in a zone five hours and a half east of the meridian and in one five hours
    /// west of it, say the same health - the age is a length of time and not a place - while the tooltip of each row that
    /// has a reading names it on either side of the reading's own date. The two rows without one keep their fixed words
    /// in either zone, and the whole list is told in the order of its types in all three.</summary>
    [Fact]
    public void EveryRowIsToldInTheZoneTheListIsToldIn()
    {
        var manifest = Manifest(
            new RuntimeUnplaced("acr", Type: "Container registry", Ids: [Registry]),
            new RuntimeUnplaced("kv", Type: "Key vault", Ids: [KeyVault]),
            new RuntimeUnplaced("st", Type: "Storage account", Ids: [Other]),
            new RuntimeUnplaced("sg", Type: "Test resource"));
        var report = Report((Registry, Said("available")), (KeyVault, Said("unavailable")));

        var east = UnplacedText.Rows(manifest, report, Now, East);
        var west = UnplacedText.Rows(manifest, report, Now, West);
        var utc = UnplacedText.Rows(manifest, report, Now, TimeZoneInfo.Utc);

        Assert.Equal(["acr", "kv", "st", "sg"], east.Select(row => row.Entry.Name));
        Assert.Equal(east.Select(row => row.Health), west.Select(row => row.Health));
        Assert.Equal(east.Select(row => row.HealthTone), west.Select(row => row.HealthTone));
        Assert.Equal(["Available · Resource Health, 40 min ago", "Unavailable · Resource Health, 40 min ago", "Not known · not in the hourly reading", "not read"], east.Select(row => row.Health));
        Assert.Equal(["plain", "plain", "muted", "muted"], east.Select(row => row.HealthTone));

        // The two rows with a reading name it in the zone they were told in, a day either side of its own date.
        Assert.Contains("acr: available (by Resource Health), read 2026-10-05 03:07 +05:30 (40 min ago).", east[0].HealthTitle, StringComparison.Ordinal);
        Assert.Contains("kv: unavailable (by Resource Health), read 2026-10-05 03:07 +05:30 (40 min ago).", east[1].HealthTitle, StringComparison.Ordinal);
        Assert.Contains("acr: available (by Resource Health), read 2026-10-04 16:37 -05:00 (40 min ago).", west[0].HealthTitle, StringComparison.Ordinal);
        Assert.Contains("kv: unavailable (by Resource Health), read 2026-10-04 16:37 -05:00 (40 min ago).", west[1].HealthTitle, StringComparison.Ordinal);
        Assert.Contains("read 2026-10-04 21:37 +00:00 (40 min ago).", utc[0].HealthTitle, StringComparison.Ordinal);
        Assert.DoesNotContain("read 2026-10-04 21:37 +00:00", east[0].HealthTitle, StringComparison.Ordinal);

        // The two rows with nothing to tell say the same words in every zone, and none of them a date.
        foreach (var rows in new[] { east, west, utc })
        {
            Assert.Equal($"{ResourceHealthText.Source}\nst: not in the hourly reading.", rows[2].HealthTitle);
            Assert.Equal(UnplacedText.NotReadTitle, rows[3].HealthTitle);
            Assert.DoesNotContain("2026", rows[2].Health, StringComparison.Ordinal);
            Assert.DoesNotContain("2026", rows[3].Health, StringComparison.Ordinal);
        }
    }

    /// <summary>The moment the list is told at reaches every row that has a reading, and its absence takes the age out
    /// of every one of them: the same report, the same entries, told at the reading's own minute, forty minutes on,
    /// thirty hours on and with no clock at all. The tone follows the age behind it - plain while the reading is at most
    /// two hours old, muted past that, and muted when there is no moment to measure it against - while the entry of no
    /// ids stands at <c>not read</c> and <c>muted</c> through all four.</summary>
    [Fact]
    public void TheMomentTheListIsToldAtReachesEveryRow()
    {
        var manifest = Manifest(
            new RuntimeUnplaced("kv", Type: "Key vault", Ids: [KeyVault]),
            new RuntimeUnplaced("st", Type: "Storage account"));
        var report = Report((KeyVault, Said("available")));

        var told = UnplacedText.Rows(manifest, report, Read, TimeZoneInfo.Utc);

        Assert.Equal(["Available · Resource Health, just now", "not read"], told.Select(row => row.Health));
        Assert.Equal(["plain", "muted"], told.Select(row => row.HealthTone));

        var old = UnplacedText.Rows(manifest, report, Read + TimeSpan.FromHours(30), TimeZoneInfo.Utc);

        Assert.Equal(["Available · Resource Health, 30 h ago", "not read"], old.Select(row => row.Health));
        Assert.Equal(["muted", "muted"], old.Select(row => row.HealthTone));

        var clockless = UnplacedText.Rows(manifest, report, null, TimeZoneInfo.Utc);

        Assert.Equal(["Available · Resource Health", "not read"], clockless.Select(row => row.Health));
        Assert.Equal(["muted", "muted"], clockless.Select(row => row.HealthTone));

        // A moment ahead of the reading - two clocks that do not quite agree - is still <c>just now</c>, and still plain.
        var early = UnplacedText.Rows(manifest, report, Read - TimeSpan.FromHours(6), TimeZoneInfo.Utc);

        Assert.Equal(["Available · Resource Health, just now", "not read"], early.Select(row => row.Health));
        Assert.Equal(["plain", "muted"], early.Select(row => row.HealthTone));
    }

    /// <summary>Words, never a colour: whatever the reading says - the worst state Azure gives, a provisioning state
    /// standing in for Resource Health having no answer, neither of them answering, nothing read at all, the entry the
    /// file does not name, the entry with no id - the health of a row and the tone it is dressed in name no colour, the
    /// tone is only ever the one of the two the page's stylesheet knows, and no word of the list is left empty. The
    /// page's whole claim for this list, pinned across every state a file can answer with and at three moments.</summary>
    [Fact]
    public void NoRowOfTheListEverNamesAColour()
    {
        var manifest = Manifest(
            new RuntimeUnplaced("acr", Type: "Container registry", Ids: [Registry]),
            new RuntimeUnplaced("kv", Type: "Key vault", Ids: [KeyVault]),
            new RuntimeUnplaced("ni", Type: "Network interface", Ids: [Nic]),
            new RuntimeUnplaced("st", Type: "Storage account", Ids: [Other]),
            new RuntimeUnplaced("sg", Type: "Test resource"));
        var report = Report(
            (Registry, Said("succeeded", ResourceHealthReading.FromProvisioningState)),
            (KeyVault, Said("available")),
            (Other, Said("unknown", ResourceHealthReading.None)));

        foreach (var now in new DateTimeOffset?[] { Now, Read + TimeSpan.FromHours(30), null })
        {
            var rows = UnplacedText.Rows(manifest, report, now, TimeZoneInfo.Utc);

            Assert.Equal(5, rows.Count);
            Assert.All(rows, row =>
            {
                Assert.Contains(row.HealthTone, Tones);
                Assert.False(string.IsNullOrWhiteSpace(row.Health));
                Assert.False(string.IsNullOrWhiteSpace(row.HealthTitle));
                Assert.DoesNotContain("\n", row.Health, StringComparison.Ordinal);
                foreach (var colour in Colours)
                {
                    Assert.DoesNotContain(colour, $"{row.Health} {row.HealthTone}", StringComparison.OrdinalIgnoreCase);
                }
            });
        }

        // The five entries, in the order of their types: the state a provisioning state answers with, the state
        // Resource Health answers with, an id the file does not name, neither of them answering, and no id at all.
        var told = UnplacedText.Rows(manifest, report, Now, TimeZoneInfo.Utc);

        Assert.Equal(["acr", "kv", "ni", "st", "sg"], told.Select(row => row.Entry.Name));
        Assert.Equal(
            [
                "Provisioned · provisioning state, 40 min ago",
                "Available · Resource Health, 40 min ago",
                "Not known · not in the hourly reading",
                "Not known · no Resource Health, 40 min ago",
                "not read",
            ],
            told.Select(row => row.Health));
        Assert.Equal(["plain", "plain", "muted", "plain", "muted"], told.Select(row => row.HealthTone));

        // A report that is not there at all is said in words too, and none of them a colour.
        var unread = UnplacedText.Rows(manifest, null, Now, TimeZoneInfo.Utc);

        Assert.Equal(
            ["Not known · no Resource Health read yet", "not read"],
            unread.Select(row => row.Health).Distinct(StringComparer.Ordinal).ToArray());
        Assert.All(unread, row => Assert.Equal("muted", row.HealthTone));
    }

    // ------------------------------------------------------------- the class's own shape, and nothing else

    /// <summary>The whole public surface of the class, as it stands: the two fixed words of an entry of no id, the one
    /// line of a closed list and the rows of an open one - and nothing else to call. A member added to the class has to
    /// be written here as well, which is the point: nothing here reaches Azure, a clock or a file, and a fifth word
    /// nobody decided the meaning of would show itself in this list before it reached the page.</summary>
    [Fact]
    public void TheSurfaceIsTheFourWordsTheClassSaysAndNothingElse()
    {
        var type = typeof(UnplacedText);

        Assert.True(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed, "the list under the diagram is a static class, not a thing to hold");
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Empty(type.GetInterfaces());
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(["NotRead", "NotReadTitle", "Rows", "Summary"], PublicSurface());

        var rows = type.GetMethod("Rows")!;
        var summary = type.GetMethod("Summary")!;

        Assert.Equal(1, Overloads("Rows"));
        Assert.Equal(1, Overloads("Summary"));
        Assert.All(new[] { rows, summary }, method =>
        {
            Assert.True(method.IsStatic);
            Assert.True(method.IsPublic);
            Assert.False(method.IsGenericMethod);
            Assert.False(method.ReturnType.Name.StartsWith("Task", StringComparison.Ordinal));
            Assert.False(method.ReturnType.Name.StartsWith("ValueTask", StringComparison.Ordinal));
            Assert.DoesNotContain(method.GetParameters(), parameter => parameter.ParameterType == typeof(CancellationToken));
            Assert.All(method.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue, $"{parameter.Name} is asked with a default of its own"));
        });

        // The rows out of a manifest, a report and two moments of the caller's: no address, no client, and no default
        // of a time that would go and ask the machine what o'clock it is.
        Assert.Equal(typeof(IReadOnlyList<UnplacedRow>), rows.ReturnType);
        Assert.Equal(["manifest", "report", "now", "zone"], rows.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(
            [typeof(RuntimeManifest), typeof(ResourceHealthReport), typeof(DateTimeOffset?), typeof(TimeZoneInfo)],
            rows.GetParameters().Select(parameter => parameter.ParameterType).ToArray());

        Assert.Equal(typeof(string), summary.ReturnType);
        Assert.Equal(["count"], summary.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(typeof(int), Assert.Single(summary.GetParameters()).ParameterType);

        // Nothing kept between one page and the next: the only fields of the class are its two constants, and they are
        // literal, and the one record it hands out is the row of its own name.
        Assert.DoesNotContain(type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly), field => !field.IsLiteral);
        Assert.Equal(typeof(UnplacedRow), rows.ReturnType.GetGenericArguments()[0]);
    }

    // ------------------------------------------------------------------------------ the machinery

    /// <summary>The public members the class declares itself: its constants and its methods, the compiler's own
    /// getters walked past, in the order a reader of the surface finds them.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(UnplacedText)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>How many public methods of the class carry the given name.</summary>
    private static int Overloads(string name) =>
        typeof(UnplacedText)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Count(method => method.Name.Equals(name, StringComparison.Ordinal));

    /// <summary>A manifest of these unplaced entries and nothing else: the nodes, the regions and the edges are what
    /// <see cref="UnplacedText.Rows" /> does not read, so each one is empty here.</summary>
    private static RuntimeManifest Manifest(params RuntimeUnplaced[] entries) => new("tdd", [], [], [], null, entries);

    /// <summary>A manifest from before the list was drawn: no list at all, which is a different nothing from an empty
    /// one and gives the same no rows.</summary>
    private static RuntimeManifest NoList() => new("tdd", [], [], [], null, null);

    /// <summary>A reading as the file's own words give it, at <see cref="Read" /> unless the test says otherwise.</summary>
    private static ResourceHealthReading Said(
        string status,
        string source = ResourceHealthReading.FromResourceHealth,
        DateTimeOffset? at = null) => new(status, source, at ?? Read);

    /// <summary>containers.json with one reading per id, found whatever case the row asks it in.</summary>
    private static ResourceHealthReport Report(params (string Id, ResourceHealthReading Reading)[] readings) =>
        new(Read, readings.ToDictionary(entry => entry.Id, entry => entry.Reading, StringComparer.OrdinalIgnoreCase));

    /// <summary>Asking for the rows under a reader whose language cases and orders letters differently - and handing the
    /// thread's own culture back afterwards, whatever the asking did.</summary>
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
