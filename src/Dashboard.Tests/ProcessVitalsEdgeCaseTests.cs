using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="ProcessVitals" /> is what a web app says of its own process in the <c>process</c> object of its
/// telemetry: seven numbers, every one of which the app may leave out, one threshold - the five minutes under which
/// an uptime is a restart - and the one word the seven add up to, <see cref="ProcessVitals.RestartedRecently" />.
/// <see cref="VitalsText" />, the other type of the same file, says those numbers in the words of the tiles.
/// <para>
/// What is pinned where it belongs is walked past here: <see cref="ProcessVitalsTests" /> reads the shipped sample
/// telemetry through <see cref="TelemetrySnapshot.Parse" /> and pins the seven numbers as the seven the sample says,
/// the answers with no <c>process</c> object, with one that is not an object, with an empty one and with the values a
/// number cannot be, the vital that stands alone, the uptime words at the second, the minute, the hour and the day
/// and the five-minute line they cross, the cpu at three and at forty-one, the memory at four hundred twelve and at
/// the gigabyte, the tooltip with the heap and the threads in it, and the tiles the numbers are printed on;
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pins the same four sayers of
/// <see cref="VitalsText" /> on the page's own values; and <see cref="EventDetectorEdgeCaseTests" /> and
/// <see cref="NodeObservationEdgeCaseTests" /> build vitals of an uptime alone to hear a restart in them. The
/// reading of the JSON itself is the internal <c>Read</c>, and the app declares no <c>InternalsVisibleTo</c> (as
/// <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>), so it is pinned through <see cref="TelemetrySnapshot.Parse" />
/// there rather than here.
/// </para>
/// <para>
/// What follows is the record and the words themselves, which all of that walks past: the seven arguments landing on
/// the property of the same name - until now the parts were only ever read as whole records compared with whole
/// records, so no test could tell two swapped parts of a vitals built by hand - a part not given being nothing rather
/// than zero, and each part held as the number it was given at the ends of its own type, the two ends of
/// <see cref="double" />, of <see cref="int" /> and of <see cref="long" />, the smallest number above zero and the
/// number that is no number, with nothing added, subtracted or checked. Then the five minutes as the threshold it is,
/// and the restart as the uptime alone deciding it - negative, at the ends of <see cref="long" />, and unmoved by the
/// other six parts. Then the members every one of those assertions leans on without saying a word about them: two
/// vitals being one when all seven parts are, with the hash that goes with it, boxed and unboxed and as a key, and
/// each part alone telling them apart; the seven parts coming back out in the order they went in; the copy that
/// changes the part it names and nothing else; the words the record says itself, which say the restart it computes
/// along with the parts it holds; and the public surface of both types, so that the day an eighth part or a fifth
/// sayer is added the lists below say so. Then the four sayers of <see cref="VitalsText" /> at the ends their rules
/// leave open - the tenth of a percent the decimal is kept for and the point where keeping it would say the ten
/// twice, the gigabyte the memory crosses at and the 1023.5 that is said in megabytes a whole gigabyte's worth, the
/// uptime of a second that is not there and of the longest <see cref="TimeSpan" /> a <see cref="long" /> can hold,
/// the tooltip's three branches with the heap alone, the working set alone and the threads alone in them, the parts
/// it says nothing of, and the vitals it refuses - none of which the values pinned elsewhere distinguish.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds no moment and calls nothing,
/// the threshold is a fixed five minutes rather than a now, and every number below is one the test brought.
/// </para>
/// </summary>
public class ProcessVitalsEdgeCaseTests
{
    /// <summary>A process that says all seven of its parts, each value named apart from the others so that an
    /// argument landing on the wrong property is seen instead of quietly matching another part of the same record.</summary>
    private static ProcessVitals Full => new(3.2, 412, 96, 41, 2, 0, 5321);

    /// <summary>The seven parts as the constructor names them, in the order it names them: the list the surface
    /// test walks for the setters a part has and a computed word has not, so that a part added to the record
    /// without its name being written here is seen as the shape it is and not as a value that happens to match.</summary>
    private static readonly string[] Parts = ["CpuPercent", "WorkingSetMb", "GcHeapMb", "Threads", "InFlight", "ExceptionsPerMinute", "UptimeSeconds"];

    // -------------------------------------------------------------------------- the seven parts

    /// <summary>The constructor's seven parts, position by position, each come back as the property of the same
    /// name - the order <c>ProcessVitals.Read</c> hands the file's values over in, and the order the tiles and the
    /// tooltip take them out in.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var vitals = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321);

        Assert.Equal(3.2d, vitals.CpuPercent);
        Assert.Equal(412d, vitals.WorkingSetMb);
        Assert.Equal(96d, vitals.GcHeapMb);
        Assert.Equal(41, vitals.Threads);
        Assert.Equal(2, vitals.InFlight);
        Assert.Equal(0, vitals.ExceptionsPerMinute);
        Assert.Equal(5321, vitals.UptimeSeconds);
    }

    /// <summary>The same seven named at the call rather than standing in a row: every part lands on the property of
    /// the name it is given, which is how <c>RuntimePayloadBuilder</c> and the page take single parts out of a
    /// record whose seven parts they do not all ask for.</summary>
    [Fact]
    public void TheConstructorTakesEveryPartByNameAsWellAsByPosition()
    {
        var vitals = new ProcessVitals(
            ExceptionsPerMinute: 6,
            Threads: 4,
            UptimeSeconds: 7,
            InFlight: 5,
            CpuPercent: 1.5,
            GcHeapMb: 3.5,
            WorkingSetMb: 2.5);

        Assert.Equal(1.5d, vitals.CpuPercent);
        Assert.Equal(2.5d, vitals.WorkingSetMb);
        Assert.Equal(3.5d, vitals.GcHeapMb);
        Assert.Equal(4, vitals.Threads);
        Assert.Equal(5, vitals.InFlight);
        Assert.Equal(6, vitals.ExceptionsPerMinute);
        Assert.Equal(7, vitals.UptimeSeconds);
    }

    /// <summary>A process that says nothing at all is a record of seven nothings - not seven zeros, and not the
    /// record itself being nothing. It is the reading of the file that turns such a record into no vitals at all,
    /// and the tiles that leave out the part that is nothing; the record keeps the two apart.</summary>
    [Fact]
    public void APartThatIsNotGivenIsNothingRatherThanZero()
    {
        var nothing = new ProcessVitals(null, null, null, null, null, null, null);

        Assert.Null(nothing.CpuPercent);
        Assert.Null(nothing.WorkingSetMb);
        Assert.Null(nothing.GcHeapMb);
        Assert.Null(nothing.Threads);
        Assert.Null(nothing.InFlight);
        Assert.Null(nothing.ExceptionsPerMinute);
        Assert.Null(nothing.UptimeSeconds);

        Assert.NotEqual(nothing, new ProcessVitals(0, 0, 0, 0, 0, 0, 0));
        Assert.NotEqual(nothing, new ProcessVitals(null, null, null, null, null, null, 0));
    }

    /// <summary>The three measured parts are the numbers they were given, at every end of
    /// <see cref="double" />: zero, a fraction, the smallest number above zero, a negative, the top and the bottom
    /// of the type, the two infinities and the number that is no number. The record adds nothing to a number, takes
    /// nothing from it and checks nothing of it - a working set below zero or a cpu of no number stands as it was
    /// handed over, since it is the reading of the file, not the record, that a number may not be.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(3.2)]
    [InlineData(-7)]
    [InlineData(double.Epsilon)]
    [InlineData(4503599627370496d)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public void EachMeasuredPartIsTheNumberItWasGivenAtEveryEndOfDouble(double value)
    {
        var vitals = new ProcessVitals(value, value, value, null, null, null, null);

        Assert.Equal((double?)value, vitals.CpuPercent);
        Assert.Equal((double?)value, vitals.WorkingSetMb);
        Assert.Equal((double?)value, vitals.GcHeapMb);
    }

    /// <summary>The three counts are the numbers they were given at the ends of <see cref="int" />, and the uptime
    /// at the ends of <see cref="long" />: a thread count that is the whole of an <see cref="int" /> below zero and
    /// an uptime that is the whole of a <see cref="long" /> above it are still the parts they were, held in the
    /// property they were given and in no other.</summary>
    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(-1, -1, -1, -1)]
    [InlineData(1, 41, 2, 5321)]
    [InlineData(int.MaxValue, int.MinValue, -7, long.MaxValue)]
    [InlineData(int.MinValue, int.MaxValue, 7, long.MinValue)]
    public void EachCountIsTheNumberItWasGivenAtEveryEndOfItsType(
        int threads,
        int inFlight,
        int exceptions,
        long uptime)
    {
        var vitals = new ProcessVitals(null, null, null, threads, inFlight, exceptions, uptime);

        Assert.Equal(threads, vitals.Threads);
        Assert.Equal(inFlight, vitals.InFlight);
        Assert.Equal(exceptions, vitals.ExceptionsPerMinute);
        Assert.Equal(uptime, vitals.UptimeSeconds);
    }

    // ------------------------------------------------------------------------- the five minutes

    /// <summary>The threshold itself, as the fixed length of time it is: five minutes, three hundred seconds, and
    /// the same every time it is asked - no clock is consulted, so the line a process counts as newly restarted
    /// does not depend on when it is asked.</summary>
    [Fact]
    public void TheRecentRestartIsFiveMinutesAndNothingElse()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), ProcessVitals.RecentRestart);
        Assert.Equal(300d, ProcessVitals.RecentRestart.TotalSeconds);
        Assert.Equal(3_000_000_000L, ProcessVitals.RecentRestart.Ticks);
        Assert.Equal(ProcessVitals.RecentRestart, ProcessVitals.RecentRestart);
        Assert.Equal(TimeSpan.FromSeconds(299), ProcessVitals.RecentRestart - TimeSpan.FromSeconds(1));
    }

    /// <summary>The restart is the uptime alone, counted against those five minutes: under the line it is a restart,
    /// over it it is not - at a second below zero and at the whole of a <see cref="long" /> on either side, and with
    /// the six other parts moved as much as they will move. What the line itself is, the last second under it and
    /// the first second on it, is pinned where the uptime is read.</summary>
    [Theory]
    [InlineData(-1L, true)]
    [InlineData(-300L, true)]
    [InlineData(long.MinValue, true)]
    [InlineData(301L, false)]
    [InlineData(3600L, false)]
    [InlineData(long.MaxValue, false)]
    public void TheRestartIsTheUptimeAloneCountedAgainstTheFiveMinutes(long uptime, bool restarted)
    {
        Assert.Equal(restarted, new ProcessVitals(null, null, null, null, null, null, uptime).RestartedRecently);

        // The six other parts decide nothing: a busy process of the same uptime is as restarted as an idle one.
        Assert.Equal(
            restarted,
            new ProcessVitals(100, 8192, 4096, 255, 99, 42, uptime).RestartedRecently);
    }

    /// <summary>No uptime is no restart: a process that says nothing of how long it has run has not restarted,
    /// whichever of the other six parts it says - the tiles leave the line out entirely rather than guess.</summary>
    [Fact]
    public void AnUptimeThatIsNotSaidIsNoRestart()
    {
        Assert.False((Full with { UptimeSeconds = null }).RestartedRecently);
        Assert.False(new ProcessVitals(88.5, null, null, null, null, null, null).RestartedRecently);
        Assert.False(new ProcessVitals(null, null, null, null, null, 42, null).RestartedRecently);
    }

    // ------------------------------------------------------------------------------ the equality

    /// <summary>Two vitals are one when all seven parts are, whichever of the four ways of asking is used -
    /// <c>object.Equals</c>, the record's own <c>Equals</c>, <c>==</c> and <c>!=</c> - and a process read twice from
    /// the same seven numbers is one process. What is not equal to it: the same vitals with any one part moved, or
    /// dropped to nothing, or said as nothing when the other says zero, and <see langword="null" />, and a thing of
    /// another type that happens to carry the same numbers. The hash follows the value: equal vitals hash alike,
    /// boxed or unboxed, and a part that is no number is equal and hashed with the very same no number.</summary>
    [Fact]
    public void TwoVitalsAreOneWhenAllSevenPartsAre()
    {
        var one = Full;
        var same = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321);

        Assert.True(one.Equals(same));
        Assert.True(one == same);
        Assert.False(one != same);
        Assert.True(one.Equals((object)same));
        Assert.Equal(one, same);
        Assert.Equal(one.GetHashCode(), same.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one, one with { CpuPercent = 3.2, WorkingSetMb = 412, GcHeapMb = 96, Threads = 41, InFlight = 2, ExceptionsPerMinute = 0, UptimeSeconds = 5321 });

        ProcessVitals[] different =
        [
            one with { CpuPercent = null },
            one with { CpuPercent = 0 },
            one with { CpuPercent = 3.21 },
            one with { WorkingSetMb = null },
            one with { WorkingSetMb = 411 },
            one with { GcHeapMb = null },
            one with { GcHeapMb = 412 },
            one with { Threads = null },
            one with { Threads = 0 },
            one with { InFlight = null },
            one with { InFlight = 3 },
            one with { ExceptionsPerMinute = null },
            one with { ExceptionsPerMinute = 1 },
            one with { UptimeSeconds = null },
            one with { UptimeSeconds = 5322 },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.True(one != other);
        }

        Assert.False(one.Equals(null));
        Assert.False(one.Equals((ProcessVitals?)null));
        Assert.False(one.Equals(3.2));
        Assert.False(one.Equals(new { CpuPercent = 3.2, WorkingSetMb = 412, GcHeapMb = 96, Threads = 41, InFlight = 2, ExceptionsPerMinute = 0, UptimeSeconds = 5321 }));
        Assert.False(new ProcessVitals(null, null, null, null, null, null, null).Equals(null));

        // Two processes that say no number for the same part say one process, and hash as one.
        var noNumber = new ProcessVitals(double.NaN, null, null, null, null, null, null);
        var alsoNoNumber = new ProcessVitals(double.NaN, null, null, null, null, null, null);

        Assert.True(noNumber.Equals(alsoNoNumber));
        Assert.True(noNumber == alsoNoNumber);
        Assert.Equal(noNumber.GetHashCode(), alsoNoNumber.GetHashCode());
        Assert.NotEqual(noNumber, new ProcessVitals(0, null, null, null, null, null, null));
    }

    /// <summary>As a key, a process is its value: two equal vitals are one entry of a set, vitals that differ in a
    /// single part are another, and the seven numbers a file's reading hands over are interchangeable with the same
    /// seven written out by hand - the reader builds an ordinary record of values, nothing apart. The vitals that
    /// say nothing are a key of their own, distinct from the one that says seven zeros.</summary>
    [Fact]
    public void AVitalsIsAKeyOfItsOwnValue()
    {
        var vitals = new HashSet<ProcessVitals> { Full };
        var sameNumbers = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321);
        var noUptimeSaid = Full with { UptimeSeconds = null };

        Assert.Contains(sameNumbers, vitals);
        Assert.DoesNotContain(noUptimeSaid, vitals);

        vitals.Add(sameNumbers);
        vitals.Add(new ProcessVitals(null, null, null, null, null, null, null));
        Assert.Equal(2, vitals.Count);

        var keyed = new Dictionary<ProcessVitals, string> { [new ProcessVitals(null, null, null, null, null, null, null)] = "nothing" };

        keyed[new ProcessVitals(null, null, null, null, null, null, null)] = "still nothing";
        keyed[Full] = "everything";
        keyed[new ProcessVitals(0, 0, 0, 0, 0, 0, 0)] = "seven zeros";

        Assert.Equal(3, keyed.Count);
        Assert.Equal("still nothing", keyed[new ProcessVitals(null, null, null, null, null, null, null)]);
        Assert.Equal("everything", keyed[sameNumbers]);
        Assert.Equal("seven zeros", keyed[new ProcessVitals(0, 0, 0, 0, 0, 0, 0)]);
    }

    // --------------------------------------------------- the parts come out, the copy stands, the words are said

    /// <summary>The seven parts come back out in the order they went in - the order of the record's declaration,
    /// cpu, working set, heap, threads, in flight, exceptions and uptime, which is the order the reader hands them
    /// over in - and the parts that are nothing come out as nothing rather than as zero.</summary>
    [Fact]
    public void TheSevenPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (cpu, workingSet, heap, threads, inFlight, exceptions, uptime) = Full;

        Assert.Equal(3.2d, cpu);
        Assert.Equal(412d, workingSet);
        Assert.Equal(96d, heap);
        Assert.Equal(41, threads);
        Assert.Equal(2, inFlight);
        Assert.Equal(0, exceptions);
        Assert.Equal(5321, uptime);

        var (noCpu, noWorkingSet, noHeap, noThreads, noInFlight, noExceptions, noUptime) =
            new ProcessVitals(null, null, null, null, null, null, null);

        Assert.Null(noCpu);
        Assert.Null(noWorkingSet);
        Assert.Null(noHeap);
        Assert.Null(noThreads);
        Assert.Null(noInFlight);
        Assert.Null(noExceptions);
        Assert.Null(noUptime);
    }

    /// <summary>A copy changes the part it names and nothing else: the original stands as it stood while the copy
    /// carries the moved part, and a copy built from the vitals that say nothing keeps the six parts it does not
    /// name nothing. A copy that names every part its own value is the one it was copied from, and not the same
    /// one - and the restart the copy computes follows the part it moved, which is the only part it moved.</summary>
    [Fact]
    public void ACopyChangesThePartItNamesAndNothingElse()
    {
        var original = Full;
        var copied = original with { Threads = 80 };

        Assert.Equal(41, original.Threads);
        Assert.Equal(80, copied.Threads);
        Assert.Equal(3.2d, copied.CpuPercent);
        Assert.Equal(412d, copied.WorkingSetMb);
        Assert.Equal(96d, copied.GcHeapMb);
        Assert.Equal(2, copied.InFlight);
        Assert.Equal(0, copied.ExceptionsPerMinute);
        Assert.Equal(5321, copied.UptimeSeconds);
        Assert.NotEqual(original, copied);

        var sameParts = original with
        {
            CpuPercent = 3.2,
            WorkingSetMb = 412,
            GcHeapMb = 96,
            Threads = 41,
            InFlight = 2,
            ExceptionsPerMinute = 0,
            UptimeSeconds = 5321,
        };

        Assert.Equal(original, sameParts);
        Assert.NotSame(original, sameParts);

        var fromNothing = new ProcessVitals(null, null, null, null, null, null, null) with { CpuPercent = 88.5 };

        Assert.Equal(88.5d, fromNothing.CpuPercent);
        Assert.Null(fromNothing.WorkingSetMb);
        Assert.Null(fromNothing.GcHeapMb);
        Assert.Null(fromNothing.Threads);
        Assert.Null(fromNothing.InFlight);
        Assert.Null(fromNothing.ExceptionsPerMinute);
        Assert.Null(fromNothing.UptimeSeconds);

        // A copy of the uptime alone moves the restart with it, and a copy of any other part leaves it standing.
        Assert.False(original.RestartedRecently);
        Assert.True((original with { UptimeSeconds = 60, CpuPercent = 100 }).RestartedRecently);
        Assert.False((original with { UptimeSeconds = 3600, Threads = 1 }).RestartedRecently);
        Assert.False((original with { Threads = 1, ExceptionsPerMinute = 99 }).RestartedRecently);
        Assert.NotEqual(original.RestartedRecently, (original with { UptimeSeconds = 60 }).RestartedRecently);
    }

    /// <summary>The words a process says itself name its seven parts by the names the record declares, in that
    /// order, and the restart it computes along with them - the last of the eight is not one the app sent, so a
    /// reader of the record's own words sees a part no file writes. The parts that are nothing are said as the
    /// nothing they are and not as zero, the number the page would show as though the app had said it.</summary>
    [Fact]
    public void TheVitalsSayTheirSevenPartsAndTheRestartTheyCompute()
    {
        Assert.Equal(
            "ProcessVitals { CpuPercent = 3.2, WorkingSetMb = 412, GcHeapMb = 96, Threads = 41, InFlight = 2, ExceptionsPerMinute = 0, UptimeSeconds = 5321, RestartedRecently = False }",
            Words(Full));

        Assert.Equal(
            "ProcessVitals { CpuPercent = , WorkingSetMb = , GcHeapMb = , Threads = , InFlight = , ExceptionsPerMinute = , UptimeSeconds = , RestartedRecently = False }",
            Words(new ProcessVitals(null, null, null, null, null, null, null)));

        Assert.Equal(
            "ProcessVitals { CpuPercent = 3.2, WorkingSetMb = 412, GcHeapMb = 96, Threads = 41, InFlight = 2, ExceptionsPerMinute = 0, UptimeSeconds = 120, RestartedRecently = True }",
            Words(Full with { UptimeSeconds = 120 }));

        // A second's difference in any one part is a letter's difference in the words, and the words are no measure
        // of the record: two vitals said alike are one record.
        Assert.NotEqual(Words(Full), Words(Full with { Threads = 42 }));
        Assert.NotEqual(Words(new ProcessVitals(null, null, null, null, null, null, null)), Words(new ProcessVitals(0, 0, 0, 0, 0, 0, 0)));
        Assert.Equal(Words(Full), Words(new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321)));
        Assert.DoesNotContain("process", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The seven numbers are numbers, and no reader's culture turns what a record holds; it turns only the
    /// words the record says of them. A reader of German sees the point of <c>3.2</c> as a comma in the record's own
    /// sentence, and still compares the vitals with the ones an invariant reader made and hashes with them - while
    /// the tiles' words, which the next tests are about, keep the invariant point whatever the reader speaks.</summary>
    [Fact]
    public void ThePartsAreHeldAlikeWhateverTheReaderSpeaksAndOnlySaidApart()
    {
        var madeByAGerman = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321);

        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "ProcessVitals { CpuPercent = 3,2, WorkingSetMb = 412, GcHeapMb = 96, Threads = 41, InFlight = 2, ExceptionsPerMinute = 0, UptimeSeconds = 5321, RestartedRecently = False }",
                Full.ToString());

            // The numbers themselves and the restart they add up to are nothing for a reader to turn.
            Assert.Equal(Full, madeByAGerman);
            Assert.True(Full == madeByAGerman);
            Assert.Equal(Full.GetHashCode(), madeByAGerman.GetHashCode());
            Assert.False(Full.RestartedRecently);
        });

        Assert.Equal(Words(Full), Words(madeByAGerman));
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>The process's whole public surface, as it stands: the seven parts the constructor names, in that
    /// order, the restart they add up to, and the five minutes the restart is counted against - which is the one
    /// member of the record that is no part of the answer and the one the record says as a field rather than a
    /// property - and the members every record is asked, falling apart, comparing, hashing and saying itself, and
    /// nothing else to call. The reading of the JSON is internal to an app that declares no
    /// <c>InternalsVisibleTo</c>, so it is no member of this list and is pinned through
    /// <see cref="TelemetrySnapshot.Parse" />. An eighth part, or a threshold that is no longer five minutes, has to
    /// be written here as well, which is the point: the page cannot be shown a vital nobody decided what it means.</summary>
    [Fact]
    public void TheRecordHasSevenPartsARestartAndTheFiveMinutes()
    {
        Assert.Equal(
            ["CpuPercent", "WorkingSetMb", "GcHeapMb", "Threads", "InFlight", "ExceptionsPerMinute", "UptimeSeconds", "RestartedRecently"],
            [.. typeof(ProcessVitals)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [
                "CpuPercent",
                "Deconstruct",
                "Equals",
                "ExceptionsPerMinute",
                "GcHeapMb",
                "GetHashCode",
                "InFlight",
                "RecentRestart",
                "RestartedRecently",
                "Threads",
                "ToString",
                "UptimeSeconds",
                "WorkingSetMb",
                "op_Equality",
                "op_Inequality",
            ],
            PublicSurface(typeof(ProcessVitals)));

        var properties = typeof(ProcessVitals)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(double?), properties["CpuPercent"].PropertyType);
        Assert.Equal(typeof(double?), properties["WorkingSetMb"].PropertyType);
        Assert.Equal(typeof(double?), properties["GcHeapMb"].PropertyType);
        Assert.Equal(typeof(int?), properties["Threads"].PropertyType);
        Assert.Equal(typeof(int?), properties["InFlight"].PropertyType);
        Assert.Equal(typeof(int?), properties["ExceptionsPerMinute"].PropertyType);
        Assert.Equal(typeof(long?), properties["UptimeSeconds"].PropertyType);
        Assert.Equal(typeof(bool), properties["RestartedRecently"].PropertyType);

        // The seven parts may be given a value and the restart may not: it is no part of the answer but what the
        // uptime one of the seven makes.
        Assert.All(Parts, name => Assert.True(properties[name].CanWrite));

        Assert.False(properties["RestartedRecently"].CanWrite);
        Assert.NotNull(properties["RestartedRecently"].GetGetMethod());
        Assert.False(properties["RestartedRecently"].GetGetMethod()!.IsStatic);

        var threshold = typeof(ProcessVitals).GetField("RecentRestart", BindingFlags.Public | BindingFlags.Static)!;

        Assert.True(threshold.IsStatic);
        Assert.True(threshold.IsInitOnly);
        Assert.False(threshold.IsLiteral);
        Assert.Equal(typeof(TimeSpan), threshold.FieldType);

        // Seven parts, and the constructor that takes all seven in that order: none of them optional at the call,
        // which is what makes the nothing a part says the app's own nothing rather than a default the record gave.
        var constructor = typeof(ProcessVitals).GetConstructors().Single();

        Assert.Equal(
            ["CpuPercent", "WorkingSetMb", "GcHeapMb", "Threads", "InFlight", "ExceptionsPerMinute", "UptimeSeconds"],
            [.. constructor.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.All(constructor.GetParameters(), parameter => Assert.False(parameter.IsOptional));
        Assert.Equal(
            [typeof(double?), typeof(double?), typeof(double?), typeof(int?), typeof(int?), typeof(int?), typeof(long?)],
            [.. constructor.GetParameters().Select(parameter => parameter.ParameterType)]);

        // The seven parts come out as the seven types they went in, in the order they went in.
        var deconstruct = typeof(ProcessVitals).GetMethod("Deconstruct")!;

        Assert.True(deconstruct.IsPublic);
        Assert.Equal(
            [typeof(double?), typeof(double?), typeof(double?), typeof(int?), typeof(int?), typeof(int?), typeof(long?)],
            [.. deconstruct.GetParameters()
                .Select(parameter => parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType)]);

        // A record of values: sealed, compared by what it holds, and holding no moment and calling no clock.
        Assert.True(typeof(ProcessVitals).IsSealed);
        Assert.Equal(typeof(object), typeof(ProcessVitals).BaseType);
        Assert.IsAssignableFrom<IEquatable<ProcessVitals>>(Full);
    }

    // ---------------------------------------------------------------------------- the cpu in words

    /// <summary>The cpu at the two ends of the rule that keeps a decimal below ten: the tenth of a percent that is
    /// rounded away before the decimal has anything to say (<c>0.05</c> is <c>0 %</c>, <c>0.95</c> is <c>1 %</c>)
    /// and the one that is kept (<c>0.06</c> is <c>0.1 %</c>); the nine point nine four under the ten that keeps
    /// the decimal and the nine point nine five the decimal would round onto the ten it is kept below - so that a
    /// number under ten is never said as the ten it rounds to while a decimal stands on it, and is said as the ten
    /// when the decimal has nothing left to say; and the ten itself, which keeps no decimal whatever. Below zero
    /// the same rules stand with a minus in front of them, and a number so small it rounds to nothing is the nothing
    /// it rounds to.</summary>
    [Theory]
    [InlineData(0.049999, "0 %")]
    [InlineData(0.05, "0 %")]
    [InlineData(0.06, "0.1 %")]
    [InlineData(0.95, "1 %")]
    [InlineData(9.04, "9 %")]
    [InlineData(9.06, "9.1 %")]
    [InlineData(9.94, "9.9 %")]
    [InlineData(9.95, "10 %")]
    [InlineData(9.96, "10 %")]
    [InlineData(10, "10 %")]
    [InlineData(10.04, "10 %")]
    [InlineData(10.06, "10 %")]
    [InlineData(99.95, "100 %")]
    [InlineData(1234.5, "1235 %")]
    [InlineData(1e-07, "0 %")]
    [InlineData(-0.04, "-0 %")]
    [InlineData(-0.5, "-0.5 %")]
    [InlineData(-3.24, "-3.2 %")]
    [InlineData(-9.96, "-10 %")]
    public void TheCpuKeepsItsDecimalOnlyWhereTheDecimalSaysSomething(double percent, string expected) =>
        Assert.Equal(expected, VitalsText.Cpu(percent));

    /// <summary>The numbers a cpu may be that are no number, and the number that is more than a percent: the
    /// sayers write them as the invariant culture writes them - <c>NaN</c>, <c>Infinity</c> and a share of the
    /// machine of twenty-two digits - and check nothing of them, since it is the reading of the file that a number
    /// may not be, not the words. The percent sign goes with all of them.</summary>
    [Fact]
    public void TheCpuOfANumberThatIsNoNumberIsSaidAsThatNumber()
    {
        Assert.Equal("NaN %", VitalsText.Cpu(double.NaN));
        Assert.Equal("Infinity %", VitalsText.Cpu(double.PositiveInfinity));
        Assert.Equal("-Infinity %", VitalsText.Cpu(double.NegativeInfinity));
        Assert.Equal("1000000000000000000000 %", VitalsText.Cpu(1e+21));
        Assert.DoesNotContain("null", VitalsText.Cpu(0), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------------ the memory in words

    /// <summary>The memory at the change of unit the tiles are written for: a thousand twenty three and a half
    /// megabytes is still said in megabytes, and said as a whole 1024 of them - the number that crosses onto
    /// gigabytes only from the 1024 given whole, which is <c>1.0 GB</c> and never <c>1024 MB</c>. The gigabyte is
    /// kept to one decimal at every size, so the terabyte is said as a thousand of them, and the megabyte below one
    /// is rounded to the nothing or the one it stands on. Below zero the memory is said in megabytes at the whole of
    /// a gigabyte under, since it is the size that is negative and not the unit.</summary>
    [Theory]
    [InlineData(0.49, "0 MB")]
    [InlineData(0.5, "1 MB")]
    [InlineData(0.6, "1 MB")]
    [InlineData(1023.49, "1023 MB")]
    [InlineData(1023.5, "1024 MB")]
    [InlineData(1023.99, "1024 MB")]
    [InlineData(1024, "1.0 GB")]
    [InlineData(1024.4, "1.0 GB")]
    [InlineData(1075.2, "1.1 GB")]
    [InlineData(1535.9, "1.5 GB")]
    [InlineData(1536, "1.5 GB")]
    [InlineData(1048576, "1024.0 GB")]
    [InlineData(-1, "-1 MB")]
    [InlineData(-1024, "-1024 MB")]
    [InlineData(-100000, "-100000 MB")]
    public void TheMemoryIsSaidInMegabytesUpToTheBrinkOfTheGigabyte(double megabytes, string expected) =>
        Assert.Equal(expected, VitalsText.Memory(megabytes));

    /// <summary>The two numbers a memory may be that are no number, and the largest one it may be: the
    /// <see cref="double.NaN" /> and the <see cref="double.NegativeInfinity" /> are said in megabytes, which is the
    /// unit of the branch they fall to, the <see cref="double.PositiveInfinity" /> in gigabytes, which is the
    /// branch it is not under, and the top of the type in the three hundred digits of gigabytes it works out to -
    /// every one of them with the unit and no measure, and none of them turned into a dash the record does not
    /// hold.</summary>
    [Fact]
    public void TheMemoryOfANumberThatIsNoNumberIsSaidAsThatNumber()
    {
        Assert.Equal("NaN MB", VitalsText.Memory(double.NaN));
        Assert.Equal("Infinity GB", VitalsText.Memory(double.PositiveInfinity));
        Assert.Equal("-Infinity MB", VitalsText.Memory(double.NegativeInfinity));
        Assert.EndsWith(" GB", VitalsText.Memory(double.MaxValue), StringComparison.Ordinal);
        Assert.StartsWith("175555970201398", VitalsText.Memory(double.MaxValue), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------------ the uptime in words

    /// <summary>The uptime at the ends of the units its words change at, which the sample's minute, hour and day
    /// leave open: a process that says it has run for less than nothing is said to have restarted none of a second
    /// ago rather than the negative the count would give it, the five minutes and a second is back on the units of
    /// an uptime, the two hours and the two days are the first of the next unit and not the last of the one before,
    /// and the longest <see cref="TimeSpan" /> a <see cref="long" /> of seconds still fits in is said in its
    /// eleven thousand odd days.</summary>
    [Theory]
    [InlineData(-1L, "restarted 0 s ago")]
    [InlineData(-3600L, "restarted 0 s ago")]
    [InlineData(301L, "up 5 min")]
    [InlineData(599L, "up 9 min")]
    [InlineData(3599L, "up 59 min")]
    [InlineData(7200L, "up 2 h")]
    [InlineData(172800L, "up 2 d")]
    [InlineData(8640000L, "up 100 d")]
    [InlineData(31536000L, "up 365 d")]
    [InlineData(922337203685L, "up 10675199 d")]
    public void TheUptimeIsSaidAtTheEndsOfItsUnits(long seconds, string expected) =>
        Assert.Equal(expected, VitalsText.Uptime(seconds));

    /// <summary>The uptime the words cannot say: a number of seconds beyond the reach of a
    /// <see cref="TimeSpan" /> is refused at both ends of <see cref="long" /> - the five minutes of the record are
    /// no measure of it, and no restart and no day count is guessed from the overflow - rather than being wrapped
    /// into a length of the wrong sign. The last number the words do say is the second below the reach, and the
    /// first they refuse is the second above it.</summary>
    [Fact]
    public void TheUptimeRefusesWhatATimeSpanCannotHold()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VitalsText.Uptime(long.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => VitalsText.Uptime(long.MinValue));

        Assert.Equal("up 10675199 d", VitalsText.Uptime(922337203685));
        Assert.Throws<ArgumentOutOfRangeException>(() => VitalsText.Uptime(922337203686));
    }

    // ---------------------------------------------------------------------- the tooltip in words

    /// <summary>The tooltip's three branches, in each of the shapes the file's reading leaves them: the heap alone
    /// says the heap, the working set alone says the working set and no heap, the working set and the heap together
    /// say one part with the heap of it named, and the threads stand at the end of whichever of them there is - or
    /// alone, when neither memory is said. A zero is a number the app said and is written, a negative is written
    /// with its minus, and the one of a single thread is not made plural by the words. The memory keeps the unit its
    /// own sayings give it, so the tooltip of a gigabyte and a half is said in gigabytes.</summary>
    [Theory]
    [InlineData(null, 96.0, null, "Managed heap 96 MB")]
    [InlineData(null, 1023.5, null, "Managed heap 1024 MB")]
    [InlineData(null, 1433.6, null, "Managed heap 1.4 GB")]
    [InlineData(412.0, null, null, "Working set 412 MB")]
    [InlineData(0.49, null, null, "Working set 0 MB")]
    [InlineData(1048576.0, null, null, "Working set 1024.0 GB")]
    [InlineData(412.0, null, 41, "Working set 412 MB; 41 threads")]
    [InlineData(null, null, 41, "41 threads")]
    [InlineData(null, null, 0, "0 threads")]
    [InlineData(null, null, 1, "1 threads")]
    [InlineData(null, null, -3, "-3 threads")]
    [InlineData(null, null, int.MinValue, "-2147483648 threads")]
    [InlineData(0.0, 0.0, 0, "Working set 0 MB, of which managed heap 0 MB; 0 threads")]
    [InlineData(412.0, 96.0, -3, "Working set 412 MB, of which managed heap 96 MB; -3 threads")]
    [InlineData(1433.6, 2048.0, 1, "Working set 1.4 GB, of which managed heap 2.0 GB; 1 threads")]
    public void TheTooltipSaysThePartsItIsGivenAndOnlyThose(
        double? workingSetMb,
        double? gcHeapMb,
        int? threads,
        string expected) =>
        Assert.Equal(expected, VitalsText.MemoryDetail(new ProcessVitals(null, workingSetMb, gcHeapMb, threads, null, null, null)));

    /// <summary>The tooltip says nothing of the four parts that are no memory and no thread count: the share of the
    /// processors, the requests in flight, the exceptions of the minute and the uptime stand outside it, and a
    /// process that says only those says no tooltip - the nothing the page leaves out rather than a title of empty
    /// words, and the nothing that is not the refusal the test below pins.</summary>
    [Theory]
    [InlineData(88.5, null, null, null, null, null, null)]
    [InlineData(3.2, null, null, null, null, 42, 5321L)]
    [InlineData(null, null, null, null, null, 3, 120L)]
    public void TheTooltipSaysNothingOfThePartsItDoesNotWrite(
        double? cpuPercent,
        double? workingSetMb,
        double? gcHeapMb,
        int? threads,
        int? inFlight,
        int? exceptionsPerMinute,
        long? uptimeSeconds) =>
        Assert.Null(VitalsText.MemoryDetail(new ProcessVitals(cpuPercent, workingSetMb, gcHeapMb, threads, inFlight, exceptionsPerMinute, uptimeSeconds)));

    /// <summary>The tooltip refuses a process that is not there, by the name the guard keeps - the refusal of the
    /// argument is one thing and the nothing the tooltip says of a process that is there and says no memory another,
    /// which is why an empty title is no refusal and a refusal is no empty title.</summary>
    [Fact]
    public void TheTooltipRefusesVitalsThatAreNotThere() =>
        Assert.Equal("vitals", Assert.Throws<ArgumentNullException>(() => VitalsText.MemoryDetail(null!)).ParamName);

    /// <summary>The four sayers and nothing else to call: the cpu, the memory, the uptime and the tooltip - all of
    /// them static, all of them saying a number rather than deciding one, and none of them holding a moment, a
    /// threshold of its own or the clock the uptime of the tile is counted from. A fifth sayer added to the file has
    /// to be written here as well, which is the point.</summary>
    [Fact]
    public void TheWordsAreTheFourSayersAndNothingElse()
    {
        Assert.Equal(["Cpu", "Memory", "MemoryDetail", "Uptime"], PublicSurface(typeof(VitalsText)));

        var methods = typeof(VitalsText).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(4, methods.Length);
        Assert.All(methods, method => Assert.True(method.IsStatic));
        Assert.All(methods, method => Assert.True(method.IsPublic));

        Assert.Equal(typeof(string), typeof(VitalsText).GetMethod("Cpu")!.ReturnType);
        Assert.Equal([typeof(double)], [.. typeof(VitalsText).GetMethod("Cpu")!.GetParameters().Select(parameter => parameter.ParameterType)]);

        Assert.Equal(typeof(string), typeof(VitalsText).GetMethod("Memory")!.ReturnType);
        Assert.Equal([typeof(double)], [.. typeof(VitalsText).GetMethod("Memory")!.GetParameters().Select(parameter => parameter.ParameterType)]);

        Assert.Equal(typeof(string), typeof(VitalsText).GetMethod("Uptime")!.ReturnType);
        Assert.Equal([typeof(long)], [.. typeof(VitalsText).GetMethod("Uptime")!.GetParameters().Select(parameter => parameter.ParameterType)]);

        Assert.Equal(typeof(string), typeof(VitalsText).GetMethod("MemoryDetail")!.ReturnType);
        Assert.Equal(
            [typeof(ProcessVitals)],
            [.. typeof(VitalsText).GetMethod("MemoryDetail")!.GetParameters().Select(parameter => parameter.ParameterType)]);

        // A static class: not to be made, and not to be inherited from.
        Assert.True(typeof(VitalsText).IsAbstract);
        Assert.True(typeof(VitalsText).IsSealed);
        Assert.Empty(typeof(VitalsText).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
    }

    /// <summary>The four sayers are the invariant ones for every reader: the decimal point of <c>3.2 %</c>, of
    /// <c>1.4 GB</c> and of the digits of the tooltip is the one the tiles are written with, the thousands are
    /// separated by nothing a reader's own group would add, and the number that is no number keeps the invariant
    /// letters rather than the sign its own culture would put on it.</summary>
    [Fact]
    public void TheWordsKeepTheInvariantPointForAGermanReader() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal("3.2 %", VitalsText.Cpu(3.24));
            Assert.Equal("1234 %", VitalsText.Cpu(1234.4));
            Assert.Equal("Infinity %", VitalsText.Cpu(double.PositiveInfinity));
            Assert.Equal("1.4 GB", VitalsText.Memory(1433.6));
            Assert.Equal("1024.0 GB", VitalsText.Memory(1048576));
            Assert.Equal("up 88 min", VitalsText.Uptime(5321));
            Assert.Equal(
                "Working set 1.4 GB, of which managed heap 2.0 GB; 1234 threads",
                VitalsText.MemoryDetail(new ProcessVitals(null, 1433.6, 2048, 1234, null, null, null)));
        });

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members a type declares itself: its properties and its public fields and the methods
    /// that are not the get and the set of one - the two operators kept although the compiler names them specially,
    /// since they are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone
    /// walked past. The constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is FieldInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
    }

    /// <summary>The words a process says its seven parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the parts are numbers and the sentence is the point, so the culture is only the way
    /// to say it once - which of the two readers turns which part is pinned on its own further up this file.</summary>
    private static string Words(ProcessVitals vitals) => ViewerCulture.Invariant(() => vitals.ToString()!);
}
