using System.Net;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the three things <c>src/Dashboard/Cluster/ClusterReader.cs</c>
/// declares: the five <see cref="SourceState" />s - the words for what became of one reading of one of the cluster
/// view's two files - <see cref="SourceReading{T}" />, the record that holds one reading with its value and its
/// reason, and the <see cref="ClusterReader" />'s own <see cref="ClusterReader.Timeout" />.
/// <para>
/// What the reader does with an address is pinned where it runs: <see cref="ClusterMonitorTests" /> reads both files
/// through a handler the test answers itself, and there stand the reading of a file that was read, the
/// <c>Missing</c> of an HTTP 404, the <c>Unavailable</c> of an HTTP 503 and of a network that refused the answer, the
/// <c>Malformed</c> of an HTML page, the <c>no answer within 10 s</c> of an answer that never came and the
/// <see cref="SourceState.Pending" /> a monitor starts on. What those readings are worth in words is pinned by
/// <see cref="ClusterAssessmentTests" />, <see cref="ClusterDeliveryHelperTests" /> and
/// <see cref="ClusterStateEdgeCaseTests" />. Left open is the rest of the surface, which is what follows: the five
/// states as the numbers, the names and the order they are declared in, and nothing outside them; the reading as a
/// holder that keeps all three of its parts exactly as it was given them - the absent value, the reason that is the
/// empty string, the pair no rule ties together - and as the record it is, equal, hashable, cloneable and printable
/// by its three parts; and the timeout as the one fixed value the reader carries, held for its owner alone and never
/// argued with until a read runs.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the two reads of the reader are the
/// members that need an exchange and a timer, so they are left to <see cref="ClusterMonitorTests" /> and named here
/// only as the surface. The readers below stand on a handler that is never asked and on the tests' own signalling
/// clock, which is never advanced.
/// </para>
/// </summary>
public class SourceStateEdgeCaseTests
{
    /// <summary>The reason of the address that answered HTTP 404, as <see cref="ClusterMonitorTests" /> reads it.</summary>
    private const string NotFound = "the address answered HTTP 404";

    /// <summary>The reason of an answer that is not the file, as <see cref="ClusterMonitorTests" /> reads it.</summary>
    private const string NotJson = "the file is not valid JSON";

    /// <summary>A reading of nothing yet: the state a monitor's file stands on before its first round.</summary>
    private static SourceReading<ClusterStatus> Pending => new(SourceState.Pending);

    /// <summary>A reader built on a handler that is never asked and a clock that is never advanced; the timeout is
    /// the only thing it is given beyond its two collaborators.</summary>
    private static (ClusterReader Reader, StubHandler Handler) Unasked(TimeSpan? timeout = null)
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK));
        var reader = timeout is { } span
            ? new ClusterReader(new HttpClient(handler), new SignallingTimeProvider()) { Timeout = span }
            : new ClusterReader(new HttpClient(handler), new SignallingTimeProvider());

        return (reader, handler);
    }

    // ------------------------------------------------------------------------ the five states

    /// <summary>The five words for what became of a reading, in the order they stand in the enum, on the five numbers
    /// that stand from 0 to 4: a sixth would be a state no sentence of the page knows, and the order is the order of
    /// the declaration and nothing to do with how much each state worries the page.</summary>
    [Fact]
    public void TheFiveSourceStatesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Pending", "Read", "Missing", "Unavailable", "Malformed"], EnumVocabulary<SourceState>.Names);
        Assert.Equal([0, 1, 2, 3, 4], EnumVocabulary<SourceState>.Numbers);
        Assert.Equal(5, EnumVocabulary<SourceState>.Count);
        Assert.Equal(typeof(int), EnumVocabulary<SourceState>.UnderlyingType);
        Assert.False(EnumVocabulary<SourceState>.IsFlags);
    }

    /// <summary>Each state on its own number and on its own word, both ways round: the number is the one the name
    /// stands on, the name is the one that number answers to, and every one of the five is defined however it is
    /// arrived at.</summary>
    [Theory]
    [InlineData(SourceState.Pending, 0, "Pending")]
    [InlineData(SourceState.Read, 1, "Read")]
    [InlineData(SourceState.Missing, 2, "Missing")]
    [InlineData(SourceState.Unavailable, 3, "Unavailable")]
    [InlineData(SourceState.Malformed, 4, "Malformed")]
    public void EachSourceStateStandsOnItsOwnNumberAndItsOwnWord(SourceState state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (SourceState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>The first state is what a source state that was never set is - and it is the one that says nothing has
    /// been read yet, which is why a half-built reading looks untouched rather than wrong. Outside the five there is
    /// nothing: neither one below the first nor one above the last nor either end of <see cref="int" /> is a defined
    /// state, and a value the enum does not name has no name of its own - it says the number it stands on.</summary>
    [Fact]
    public void AnUnsetSourceStateIsTheFirstOneAndNothingOutsideTheFiveIsAState()
    {
        var outside = EnumVocabulary<SourceState>.OutsideOf(5);

        Assert.Equal(SourceState.Pending, default);
        Assert.Equal(SourceState.Pending, EnumVocabulary<SourceState>.Of(0));

        Assert.True(Enum.IsDefined(SourceState.Pending));
        Assert.True(Enum.IsDefined(SourceState.Malformed));
        Assert.False(Enum.IsDefined(outside.Below));
        Assert.False(Enum.IsDefined(outside.Above));
        Assert.False(Enum.IsDefined(outside.Lowest));
        Assert.False(Enum.IsDefined(outside.Highest));

        Assert.Null(Enum.GetName(outside.Above));
        Assert.Null(Enum.GetName(outside.Below));
        Assert.Null(Enum.GetName(outside.Lowest));
        Assert.Null(Enum.GetName(outside.Highest));
        Assert.Equal("5", outside.Above.ToString());
        Assert.Equal("-1", outside.Below.ToString());
        Assert.Equal("-2147483648", outside.Lowest.ToString());
        Assert.Equal("2147483647", outside.Highest.ToString());
    }

    /// <summary>The five states apart from one another and compared by the numbers they stand on: each is its own
    /// equal in the box as out of it, an <see cref="int" /> on the same number is no state's equal, and the order is
    /// the declaration - <c>Read</c> before <c>Missing</c> before <c>Unavailable</c> before <c>Malformed</c>, which
    /// says nothing at all about which of them is the worse news.</summary>
    [Fact]
    public void TheSourceStatesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        SourceState[] states =
        [
            SourceState.Pending, SourceState.Read, SourceState.Missing, SourceState.Unavailable, SourceState.Malformed,
        ];

        Assert.All(states, state => Assert.Single(states, other => other == state));
        Assert.Equal(states.Length, states.Distinct().Count());

        Assert.True(SourceState.Pending < SourceState.Read);
        Assert.True(SourceState.Read < SourceState.Missing);
        Assert.True(SourceState.Missing < SourceState.Unavailable);
        Assert.True(SourceState.Unavailable < SourceState.Malformed);
        Assert.False(SourceState.Malformed < SourceState.Unavailable);
        Assert.Equal(1, SourceState.Missing.CompareTo(SourceState.Read));
        Assert.Equal(-1, SourceState.Pending.CompareTo(SourceState.Malformed));
        Assert.Equal(0, SourceState.Unavailable.CompareTo((object)SourceState.Unavailable));

        Assert.Equal((object)SourceState.Malformed, (object)SourceState.Malformed);
        Assert.False(((object)SourceState.Malformed).Equals(null));
        Assert.False(SourceState.Malformed.Equals(null));
        Assert.False(SourceState.Malformed.Equals((object)4));
        Assert.Equal(SourceState.Malformed.GetHashCode(), ((object)SourceState.Malformed).GetHashCode());
    }

    /// <summary>A state is a choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so the operators
    /// work on the numbers underneath rather than on the states. Two of these numbers do combine into a third state -
    /// <c>Read</c> with <c>Missing</c> is <c>Unavailable</c>, at least a word the page has - while the rest land
    /// outside the five, where the page has no word at all and a reading would carry a state no badge and no sentence
    /// knows. Nothing is added by a union with <c>Pending</c>, whose number is zero, and the intersection of two
    /// states that share no bit is <c>Pending</c> too - the word that says nothing was read.</summary>
    [Fact]
    public void TwoSourceStatesCombinedAreAnotherStateOrNoStateAtAll()
    {
        Assert.False(EnumVocabulary<SourceState>.IsFlags);

        Assert.Equal(SourceState.Unavailable, SourceState.Read | SourceState.Missing);
        Assert.Equal(SourceState.Unavailable, SourceState.Missing | SourceState.Read);
        Assert.Equal(SourceState.Pending, SourceState.Pending | SourceState.Pending);
        Assert.Equal(SourceState.Missing, SourceState.Missing | SourceState.Pending);
        Assert.Equal(SourceState.Malformed, SourceState.Malformed | SourceState.Pending);

        Assert.Equal((SourceState)5, SourceState.Read | SourceState.Malformed);
        Assert.False(Enum.IsDefined(SourceState.Read | SourceState.Malformed));
        Assert.Equal((SourceState)6, SourceState.Missing | SourceState.Malformed);
        Assert.False(Enum.IsDefined(SourceState.Missing | SourceState.Malformed));
        Assert.Equal((SourceState)7, SourceState.Read | SourceState.Missing | SourceState.Malformed);
        Assert.False(Enum.IsDefined(SourceState.Read | SourceState.Missing | SourceState.Malformed));

        Assert.Equal(SourceState.Pending, SourceState.Missing & SourceState.Malformed);
        Assert.Equal(SourceState.Pending, SourceState.Unavailable & SourceState.Malformed);
        Assert.Equal(SourceState.Read, SourceState.Read & SourceState.Unavailable);
        Assert.Equal(SourceState.Pending, SourceState.Pending & SourceState.Malformed);
    }

    /// <summary>A state read out of text is the state the text names in the case the enum spells it, or the state the
    /// number it holds stands on: a name is read whole, its surrounding spaces walked past, and a number with a
    /// cipher in front of it is still the small number it says.</summary>
    [Theory]
    [InlineData("Pending", SourceState.Pending)]
    [InlineData("Read", SourceState.Read)]
    [InlineData("Missing", SourceState.Missing)]
    [InlineData("Unavailable", SourceState.Unavailable)]
    [InlineData("Malformed", SourceState.Malformed)]
    [InlineData("  Read  ", SourceState.Read)]
    [InlineData("0", SourceState.Pending)]
    [InlineData("4", SourceState.Malformed)]
    [InlineData("01", SourceState.Read)]
    public void ASourceStateTextIsTheStateItNamesOrTheStateItNumbers(string text, SourceState expected)
    {
        Assert.True(Enum.TryParse<SourceState>(text, out var state));
        Assert.Equal(expected, state);
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>Text that is no state is refused - <see langword="null" />, the empty string, a blank of spaces or of
    /// a tab, a name in another case, a name with something behind it, a fraction, a hexadecimal, a word that means
    /// much the same thing - and a refused parse leaves its answer standing on <see cref="SourceState.Pending" />:
    /// the <c>false</c> is the whole answer, and a caller that reads the value anyway is shown a file nobody has read
    /// yet, the mildest of the five and the one the page draws before its first round.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("read")]
    [InlineData("UNAVAILABLE")]
    [InlineData("Missing?")]
    [InlineData("2.0")]
    [InlineData("0x2")]
    [InlineData("None")]
    public void TextThatIsNoSourceStateIsRefusedAndLeavesPendingBehind(string? text)
    {
        Assert.False(Enum.TryParse<SourceState>(text, out var state));
        Assert.Equal(SourceState.Pending, state);
        Assert.False(Enum.TryParse<SourceState>(text, ignoreCase: false, out _));
    }

    /// <summary>The one text that is no name and is not refused is a number outside the five: the parse answers truly
    /// and hands back a value that is no state at all, which is why a read number is worth nothing until
    /// <see cref="Enum.IsDefined(Enum)" /> has been asked about it as well. A name in a case the file does not write,
    /// on the other hand, is a state as soon as the reader is told to forgive the case - and is nothing until it
    /// is.</summary>
    [Fact]
    public void ANumberOutsideTheFiveIsReadAsAStateThatIsNoState()
    {
        Assert.True(Enum.TryParse<SourceState>("5", out var above));
        Assert.Equal((SourceState)5, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<SourceState>("-1", out var below));
        Assert.Equal((SourceState)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<SourceState>("malformed", out _));
        Assert.True(Enum.TryParse<SourceState>("malformed", ignoreCase: true, out var forgiven));
        Assert.Equal(SourceState.Malformed, forgiven);
    }

    // ------------------------------------------------------------------------ the reading as the three parts it holds

    /// <summary>A reading of a state and nothing else carries nothing else: of the five states, every one built with
    /// its state alone stands with no value and no reason, the two optional parts falling to
    /// <see langword="null" /> rather than to the empty string, and the three parts the record hands back by
    /// <c>Deconstruct</c> are the three it was built with. <see cref="SourceState.Read" /> is no exception - the
    /// state says nothing about the value, which is what a reading that read nothing looks like.</summary>
    [Theory]
    [InlineData(SourceState.Pending)]
    [InlineData(SourceState.Read)]
    [InlineData(SourceState.Missing)]
    [InlineData(SourceState.Unavailable)]
    [InlineData(SourceState.Malformed)]
    public void AReadingOfAStateAloneCarriesNoValueAndNoReason(SourceState state)
    {
        var reading = new SourceReading<ClusterStatus>(state);

        Assert.Equal(state, reading.State);
        Assert.Null(reading.Value);
        Assert.Null(reading.Detail);

        var (carried, value, detail) = reading;

        Assert.Equal(state, carried);
        Assert.Null(value);
        Assert.Null(detail);
    }

    /// <summary>The three parts as they were given, whether named or placed, and each kept as it stands: the file the
    /// reading is of, the reason of a reading that got none, a reason that is the empty string and a reason of spaces
    /// alone kept rather than read as the absence of one, a <see langword="null" /> reason named out loud the same
    /// reading as no reason at all, and a reason of four thousand characters neither cut nor wrapped.</summary>
    [Fact]
    public void AReadingKeepsItsThreePartsExactlyAsGiven()
    {
        var status = ClusterFixture.SampleStatus;

        var byPlace = new SourceReading<ClusterStatus>(SourceState.Read, status, NotJson);
        var byName = new SourceReading<ClusterStatus>(SourceState.Read, Value: status, Detail: NotJson);
        var onlyValue = new SourceReading<ClusterStatus>(SourceState.Read, status);
        var onlyDetail = new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: NotJson);

        Assert.Equal(byPlace, byName);
        Assert.Equal(SourceState.Read, byPlace.State);
        Assert.Same(status, byPlace.Value);
        Assert.Equal(NotJson, byPlace.Detail);
        Assert.Null(onlyValue.Detail);
        Assert.Null(onlyDetail.Value);
        Assert.Equal(SourceState.Malformed, onlyDetail.State);

        Assert.Equal(string.Empty, new SourceReading<ClusterStatus>(SourceState.Missing, Detail: string.Empty).Detail);
        Assert.Equal("   ", new SourceReading<ClusterStatus>(SourceState.Missing, Detail: "   ").Detail);
        Assert.Null(new SourceReading<ClusterStatus>(SourceState.Missing, Detail: null).Detail);
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Missing), new SourceReading<ClusterStatus>(SourceState.Missing, Detail: null));

        var longReason = new string('x', 4_000);

        Assert.Equal(longReason, new SourceReading<AksService>(SourceState.Unavailable, Detail: longReason).Detail);
        Assert.Equal(4_000, new SourceReading<AksService>(SourceState.Unavailable, Detail: longReason).Detail!.Length);
    }

    /// <summary>A reading joins nothing to nothing: the state is a word about the file and the value is the file, and
    /// the record holds both without ever matching them up. A <see cref="SourceState.Missing" /> reading handed a
    /// value keeps the value, a <see cref="SourceState.Read" /> one handed none keeps its <see langword="null" />, a
    /// <see cref="SourceState.Pending" /> one keeps the reason it was given - and that reason is what makes it another
    /// reading than the bare <c>Pending</c> the monitor starts on. Which is why every caller that acts on a reading
    /// looks at the state first, exactly as <see cref="ClusterAssessment" /> does.</summary>
    [Fact]
    public void AReadingJoinsNothingToItsState()
    {
        var wrong = new SourceReading<ClusterStatus>(SourceState.Missing, ClusterFixture.SampleStatus, NotFound);

        Assert.Equal(SourceState.Missing, wrong.State);
        Assert.NotNull(wrong.Value);
        Assert.Equal(NotFound, wrong.Detail);

        Assert.Equal(SourceState.Pending, Pending.State);
        Assert.Null(new SourceReading<ClusterStatus>(SourceState.Read).Value);
        Assert.Equal(NotJson, new SourceReading<ClusterStatus>(SourceState.Pending, Detail: NotJson).Detail);
        Assert.NotEqual(Pending, new SourceReading<ClusterStatus>(SourceState.Pending, Detail: NotJson));

        var ofService = new SourceReading<AksService>(SourceState.Missing, ClusterFixture.SampleService, NotFound);

        Assert.NotNull(ofService.Value);
        Assert.Equal("aks-cmdemo3", ofService.Value!.Name);
        Assert.Equal(SourceState.Missing, ofService.State);
    }

    // ------------------------------------------------------------------------ the reading as a record

    /// <summary>Two readings are one while all three parts are: the equality of the operators and of
    /// <see cref="object.Equals(object)" />, and the hash code that follows them. A difference in any one part - the
    /// state, the value, the reason, a reason made absent or only lengthened - is another reading. The value is
    /// compared by its own equality, which for a status file means the object rather than the words: the very same
    /// file, read twice over out of the same text, is two values with two sets of lists, and two readings of the one
    /// object are one reading however often it is copied by <c>with</c>.</summary>
    [Fact]
    public void TwoReadingsAreOneWhenAllThreePartsAre()
    {
        var one = new SourceReading<ClusterStatus>(SourceState.Missing, null, NotFound);
        var two = new SourceReading<ClusterStatus>(SourceState.Missing, null, NotFound);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one == Pending);

        Assert.NotEqual(one, one with { State = SourceState.Unavailable });
        Assert.NotEqual(one, one with { Detail = NotFound + "." });
        Assert.NotEqual(one, one with { Detail = null });
        Assert.NotEqual(one, new SourceReading<ClusterStatus>(SourceState.Missing, ClusterFixture.SampleStatus, NotFound));

        var status = ClusterFixture.SampleStatus;
        var reread = ClusterFixture.SampleStatus;

        Assert.NotSame(status, reread);
        Assert.Equal(status, status with { });
        Assert.Equal(
            new SourceReading<ClusterStatus>(SourceState.Read, status),
            new SourceReading<ClusterStatus>(SourceState.Read, status with { IntervalSeconds = 15 }));
        Assert.NotEqual(
            new SourceReading<ClusterStatus>(SourceState.Read, status),
            new SourceReading<ClusterStatus>(SourceState.Read, status with { IntervalSeconds = 9_999 }));
        Assert.NotEqual(
            new SourceReading<ClusterStatus>(SourceState.Read, status),
            new SourceReading<ClusterStatus>(SourceState.Read, reread));
    }

    /// <summary><c>with</c> names the part it changes and leaves the other two where they stood - the copy equal to
    /// its original when it names nothing, and never the same object - so a reading can be moved from one state to
    /// another with its reason kept. The value never rides along from a reading that had none, however the copy is
    /// made.</summary>
    [Fact]
    public void WithChangesOnePartAndLeavesTheOtherTwo()
    {
        var reading = new SourceReading<AksService>(SourceState.Malformed, Detail: NotJson);
        var copy = reading with { };

        Assert.Equal(reading, copy);
        Assert.NotSame(reading, copy);

        Assert.Equal(
            new SourceReading<AksService>(SourceState.Unavailable, Detail: NotJson),
            reading with { State = SourceState.Unavailable });
        Assert.Equal(
            new SourceReading<AksService>(SourceState.Malformed, Detail: "no answer within 10 s"),
            reading with { Detail = "no answer within 10 s" });
        Assert.Equal(reading, new SourceReading<AksService>(SourceState.Malformed) with { Detail = NotJson });
        Assert.Null((reading with { State = SourceState.Read }).Value);

        var service = ClusterFixture.SampleService;

        Assert.Equal(
            new SourceReading<AksService>(SourceState.Read, service, NotFound),
            new SourceReading<AksService>(SourceState.Read, service, NotJson) with { Detail = NotFound });
        Assert.Equal(service, (new SourceReading<AksService>(SourceState.Read, service, NotJson) with { State = SourceState.Missing }).Value);
    }

    /// <summary>The reading says its three parts in the order they are declared, each with the value it stands on:
    /// the part that is not there says its name and nothing else, the reason that is the empty string and the reason
    /// that is nothing print alike, a value prints as the value prints - a record of its own, in its own words - and
    /// the file that was read prints as the file prints.</summary>
    [Fact]
    public void AReadingSaysItsThreePartsInTheirOrder()
    {
        Assert.Equal(
            $"SourceReading {{ State = Missing, Value = , Detail = {NotFound} }}",
            new SourceReading<ClusterStatus>(SourceState.Missing, Detail: NotFound).ToString());
        Assert.Equal("SourceReading { State = Pending, Value = , Detail =  }", Pending.ToString());
        Assert.Equal(
            "SourceReading { State = Pending, Value = , Detail =  }",
            new SourceReading<ClusterStatus>(SourceState.Pending, Detail: string.Empty).ToString());
        Assert.Equal(
            "SourceReading { State = Unavailable, Value = , Detail = no answer within 10 s }",
            new SourceReading<AksService>(SourceState.Unavailable, Detail: "no answer within 10 s").ToString());
        Assert.Equal(
            "SourceReading { State = Read, Value = Named { Name = ui }, Detail =  }",
            new SourceReading<Named>(SourceState.Read, new Named("ui")).ToString());
        Assert.Equal(
            $"SourceReading {{ State = Malformed, Value = , Detail = {NotJson} }}",
            new SourceReading<AksService>(SourceState.Malformed, Detail: NotJson).ToString());
        Assert.StartsWith(
            "SourceReading { State = Read, Value = ClusterStatus {",
            ClusterFixture.Read(ClusterFixture.SampleStatus).ToString(),
            StringComparison.Ordinal);
    }

    /// <summary>The reading's own shape, as the running assembly declares it: a sealed record - never a struct, never
    /// a base for another - of exactly one type argument, constrained to a class so that no value type and no
    /// number-alike can ever stand in for a file; three public parts and the record's own four operations, nothing
    /// else; one <see cref="IEquatable{T}" /> of its own kind; and a reading of one file never the equal of a reading
    /// of the other, however alike their states and their reasons.</summary>
    [Fact]
    public void AReadingIsASealedRecordOfOneClassAndOfThreeParts()
    {
        var status = typeof(SourceReading<ClusterStatus>);
        var service = typeof(SourceReading<AksService>);

        Assert.True(status.IsSealed);
        Assert.False(status.IsValueType);
        Assert.True(typeof(IEquatable<SourceReading<ClusterStatus>>).IsAssignableFrom(status));
        Assert.Single(status.GetGenericArguments());
        Assert.Equal(typeof(ClusterStatus), status.GetGenericArguments()[0]);
        Assert.Equal(typeof(SourceReading<>), status.GetGenericTypeDefinition());
        Assert.NotEqual(status, service);
        Assert.True(
            typeof(SourceReading<>).GetGenericArguments()[0].GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint),
            "the type argument of a reading is no longer constrained to a class: any value type could stand in for a file");

        Assert.Equal(
            ["Deconstruct", "Detail", "Equals", "GetHashCode", "State", "ToString", "Value"],
            DeclaredMembers(status));

        Assert.True(new SourceReading<ClusterStatus>(SourceState.Missing, Detail: NotFound)
            .Equals((object)new SourceReading<ClusterStatus>(SourceState.Missing, Detail: NotFound)));
        Assert.False(new SourceReading<ClusterStatus>(SourceState.Missing, Detail: NotFound)
            .Equals((object)new SourceReading<AksService>(SourceState.Missing, Detail: NotFound)));
    }

    // ------------------------------------------------------------------------ the reader's own fixed value

    /// <summary>The reader waits exactly as long as a node does: ten seconds, the length <see cref="NodeProber" />
    /// measures a node by, held as the reader's own default and the same number to the tick. The words that follow
    /// from it - the <c>no answer within 10 s</c> of a file that stops answering - stand in
    /// <see cref="ClusterMonitorTests" />; here it is the length itself, read off a reader that was never asked to
    /// read anything.</summary>
    [Fact]
    public void TheReaderWaitsAsLongAsANodeDoes()
    {
        var (reader, handler) = Unasked();

        Assert.Equal(NodeProber.DefaultTimeout, reader.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(10), reader.Timeout);
        Assert.Equal(100_000_000L, reader.Timeout.Ticks);
        Assert.Equal(10d, reader.Timeout.TotalSeconds);
        Assert.False(reader.Timeout <= TimeSpan.Zero);
        Assert.Empty(handler.RequestedUrls);
    }

    /// <summary>The reader's default wait, said through the shared wait: a reader made without a named timeout
    /// stands at <see cref="ProbeWait.SharedWait" />, the same value <see cref="NodeProber.DefaultTimeout" /> and
    /// the other probes hold, beside the literals the test above still pins.</summary>
    [Fact]
    public void TheReadersDefaultTimeoutStandsAtTheSharedProbeWait()
    {
        var (reader, handler) = Unasked();

        Assert.Equal(ProbeWait.SharedWait(), reader.Timeout);
        Assert.Equal(ProbeWait.SharedWait(), NodeProber.DefaultTimeout);
        Assert.Empty(handler.RequestedUrls);
    }

    /// <summary>The timeout is the reader's own and is held as it is given: a minute for one reader leaves every
    /// other on the ten seconds it started with, and leaves the shared default of the node prober alone as well. The
    /// property argues with nothing - one tick, zero, either end of <see cref="TimeSpan" /> and a length under zero
    /// all stand exactly as they were written - because the price of a nonsense length is paid by the wait a read sets
    /// up, not by the property, and that wait is pinned where it runs. The property is only ever settable while the
    /// reader is being made - the <c>init</c> the compiler enforces at the call site, not at runtime.</summary>
    [Fact]
    public void TheTimeoutIsHeldAsGivenAndBelongsToOneReaderAlone()
    {
        var (reader, handler) = Unasked();

        foreach (var span in new[]
                 {
                     TimeSpan.FromTicks(1),
                     TimeSpan.Zero,
                     TimeSpan.FromMinutes(1),
                     TimeSpan.MaxValue,
                     TimeSpan.MinValue,
                     -TimeSpan.FromSeconds(1),
                 })
        {
            var other = Unasked(span).Reader;

            Assert.Equal(span, other.Timeout);
            Assert.Equal(TimeSpan.FromSeconds(10), reader.Timeout);
        }

        Assert.Equal(TimeSpan.FromSeconds(10), NodeProber.DefaultTimeout);
        Assert.Equal(TimeSpan.FromSeconds(2), Unasked(TimeSpan.FromSeconds(2)).Reader.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(1), Unasked(TimeSpan.FromTicks(TimeSpan.TicksPerSecond)).Reader.Timeout);
        Assert.Empty(handler.RequestedUrls);
    }

    /// <summary>The reader's whole public surface, as the running assembly declares it: one constructor of the two
    /// collaborators it was built for, in order - an <see cref="HttpClient" /> to ask and a
    /// <see cref="TimeProvider" /> to count the wait to - and three members, the timeout and the two reads. Nothing
    /// else may be called on an instance, and the two reads are the only things that ever reach an address; both are
    /// pinned by <see cref="ClusterMonitorTests" />, which answers the handler itself.</summary>
    [Fact]
    public void TheReaderOffersItsTimeoutAndItsTwoReadsAndNothingElse()
    {
        var constructor = Assert.Single(typeof(ClusterReader).GetConstructors());
        var parameters = constructor.GetParameters();

        Assert.Equal(2, parameters.Length);
        Assert.Equal("http", parameters[0].Name);
        Assert.Equal(typeof(HttpClient), parameters[0].ParameterType);
        Assert.Equal("time", parameters[1].Name);
        Assert.Equal(typeof(TimeProvider), parameters[1].ParameterType);

        Assert.Equal(
            ["ReadServiceAsync", "ReadStatusAsync", "Timeout"],
            DeclaredMembers(typeof(ClusterReader)));

        var (reader, handler) = Unasked();

        Assert.True(typeof(ClusterReader).IsSealed);
        Assert.IsType<ClusterReader>(reader);
        Assert.True(typeof(ClusterReader).GetProperty("Timeout")!.CanWrite);
        Assert.Empty(handler.RequestedUrls);
    }

    /// <summary>The public members a type declares itself: properties and the methods that are not the get and the
    /// set of one, with the record's compiler-named clone walked past - what the page and its tests may call.</summary>
    private static List<string> DeclaredMembers(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo or MethodInfo { IsSpecialName: false })
            .Where(member => member.Name.Contains('<', StringComparison.Ordinal) is false)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>A value of a reading, of no file at all: just long enough to show that a reading prints its value by
    /// the value's own words.</summary>
    private sealed record Named(string Name);
}
