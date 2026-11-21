using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="SourceReading{T}" /> alone: the three parts one
/// reading of one of the cluster view's two files is made of, and the members every record is asked. The record's
/// place in the file it comes from is pinned by its neighbours - <see cref="SourceStateEdgeCaseTests" /> for the five
/// <see cref="SourceState" />s it stands on, for the three parts kept exactly as they were given, for <c>with</c>, for
/// the equality of two readings that are one and for the words a reading says; <see cref="ClusterReaderEdgeCaseTests" />
/// for every reading the reader hands back at its own edges; <see cref="ClusterMonitorTests" /> and
/// <see cref="ClusterMonitorEdgeCaseTests" /> for the readings the page's rounds live on; and
/// <see cref="ClusterAssessmentTests" /> for what a reading is worth in words once the page has it.
/// <para>
/// What those leave open is the record's own surface seen from the outside, which is what follows: the two operators
/// the record is given and no test of it calls - <c>==</c> and <c>!=</c> at the nothing on either side, from either
/// side, and asked of a copy; the refusals of <see cref="object.Equals(object)" />, which turns down the word, the
/// number, the package of three and the other record that happens to carry the same three parts; the hash, which
/// stands still from one call to the next, boxed and unboxed, and makes a reading one key of a dictionary and one
/// entry of a set; a full reading fallen apart, into places already filled, and the three types those places are
/// written with; a value that is the empty string, which is a file of no characters rather than no file; a value
/// compared by the value's own equality - the very object in and the very object out, and two contents that are no
/// one content; a state that is none of the five, held, compared, fallen apart and said as the number it stands on; a
/// reason said as it stands, with its braces, its newline and its tab; the empty place a value leaves when it says
/// nothing about itself; the same words whatever culture the viewer speaks; and the reading's whole public surface as
/// the running assembly declares it - the three parts written only at the making, the one constructor of which only
/// the state is required, and the record's five operations with its two operators among them.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a reading holds only what the test
/// hands it, and the two members that ever read a file or wait on a timer belong to <see cref="ClusterReader" /> and
/// are pinned where they run.
/// </para>
/// </summary>
public class SourceReadingEdgeCaseTests
{
    /// <summary>The reason of the address that answered HTTP 404, as the reader writes it.</summary>
    private const string NotFound = "the address answered HTTP 404";

    /// <summary>The reason of an answer that is not the file, as the parser writes it.</summary>
    private const string NotJson = "the file is not valid JSON";

    /// <summary>The reason of an answer that never came, as the reader writes it at the node's ten seconds.</summary>
    private const string NoAnswer = "no answer within 10 s";

    // ------------------------------------------------------------------ the two operators the record is given

    /// <summary>The nothing on either side of the operators every record is asked with: two readings that are no
    /// reading at all are one, a reading and no reading are two - the equality and the inequality each saying it from
    /// either side - and a reading asked about itself says no to the nothing, to the word of its reason, to the
    /// number of its state, to the package of its three parts and to the other record that carries the very same
    /// three. The operators are the reading's own and not <see cref="object" />'s, so a copy is the reading it was
    /// copied from while a tuple that spells it out is nothing of the kind.</summary>
    [Fact]
    public void NoReadingIsOneWithNoReadingAndWithAnyOtherThing()
    {
        SourceReading<ClusterStatus>? nothing = null;
        SourceReading<ClusterStatus>? alsoNothing = null;
        var there = new SourceReading<ClusterStatus>(SourceState.Missing, null, NotFound);

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.Null(default(SourceReading<ClusterStatus>));

        var itself = there;

        Assert.True(there == itself);
        Assert.False(there != itself);
        Assert.True(there.Equals(there));
        Assert.True(there == there with { });
        Assert.False(there != there with { });
        Assert.True(there != there with { Detail = null });
        Assert.True(there != there with { Detail = string.Empty });
        Assert.True(there != there with { State = SourceState.Unavailable });

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((SourceReading<ClusterStatus>?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(((object)there).Equals(null));
        Assert.False(there.Equals(there.State));
        Assert.False(there.Equals(NotFound));
        Assert.False(there.Equals(ValueTuple.Create(SourceState.Missing, (ClusterStatus?)null, NotFound)));
        Assert.False(there.Equals(new Parsed<ClusterStatus>(null, NotFound)));
        Assert.False(there.Equals(new { State = SourceState.Missing, Value = (ClusterStatus?)null, Detail = NotFound }));
        Assert.False(there.Equals(ClusterFixture.SampleStatus));
    }

    /// <summary>Every part is a place of its own to the operators, and the reason is one of them however small a
    /// difference it makes: the same state and the same absent value, with a reason that stands at nothing at all, at
    /// the empty string, at three spaces, at the reader's own words and at those words with a full stop behind them,
    /// are five readings - told apart from either end of the pair of operators, and five entries of a set while a copy
    /// of one of them rides with the reading it was copied from. The empty reason is not the absent one, though both
    /// print the same empty place.</summary>
    [Fact]
    public void AReadingIsAnotherForAnyOneOfItsThreeParts()
    {
        var bare = new SourceReading<ClusterStatus>(SourceState.Missing);
        var empty = new SourceReading<ClusterStatus>(SourceState.Missing, Detail: string.Empty);
        var spaces = new SourceReading<ClusterStatus>(SourceState.Missing, Detail: "   ");
        var stated = new SourceReading<ClusterStatus>(SourceState.Missing, Detail: NotFound);
        var stopped = new SourceReading<ClusterStatus>(SourceState.Missing, Detail: $"{NotFound}.");

        Assert.True(bare != empty);
        Assert.False(bare == empty);
        Assert.True(empty != spaces);
        Assert.True(stated != stopped);
        Assert.True(stated != spaces);
        Assert.True(bare == spaces with { Detail = null });

        var kept = new HashSet<SourceReading<ClusterStatus>> { bare, empty, spaces, stated, stopped, stated with { } };

        Assert.Equal(5, kept.Count);
        Assert.Contains(stated, kept);
    }

    // ------------------------------------------------------------------ the value as the value's own equality has it

    /// <summary>An empty string is a file of no characters rather than no file: the reading that was handed one holds
    /// a value that is there, the reading that was handed nothing holds none, and the two are as far apart as a
    /// reading gets while it says the same words. A reason of the empty string is held the same way - a reason nobody
    /// wrote is the absence of one, and a reason of nothing at all is a reason.</summary>
    [Fact]
    public void AnEmptyFileIsAValueAndNotTheAbsenceOfOne()
    {
        var empty = new SourceReading<string>(SourceState.Read, string.Empty);
        var none = new SourceReading<string>(SourceState.Read);

        Assert.Equal(string.Empty, empty.Value);
        Assert.NotNull(empty.Value);
        Assert.Null(none.Value);
        Assert.True(empty != none);
        Assert.Equal(empty, new SourceReading<string>(SourceState.Read, ""));
        Assert.Equal(empty.ToString(), none.ToString());

        Assert.Equal(string.Empty, new SourceReading<string>(SourceState.Malformed, string.Empty, string.Empty).Detail);
        Assert.True(new SourceReading<string>(SourceState.Malformed, string.Empty, string.Empty) != new SourceReading<string>(SourceState.Malformed, string.Empty));

        // A whole file of letters, kept as the very words it was given.
        var word = new string('n', 300);
        var whole = new SourceReading<string>(SourceState.Read, word, word);

        Assert.Same(word, whole.Value);
        Assert.Same(word, whole.Detail);
        Assert.Equal($"SourceReading {{ State = Read, Value = {word}, Detail = {word} }}", whole.ToString());
    }

    /// <summary>A reading compares its value by the equality the value itself carries, and by nothing else: the very
    /// object in is the very object out - through <c>with</c> as through the property - and two values of the same
    /// words that are two objects are two readings, while a value that knows its own equality makes two references
    /// one reading. A reader that reads the same file twice therefore holds two readings, which is why the page's
    /// rounds hand a new one over rather than say nothing changed.</summary>
    [Fact]
    public void AReadingComparesItsValueByTheEqualityTheValueCarries()
    {
        var box = new Boxed("cluster.json");
        var twin = new Boxed("cluster.json");

        Assert.NotSame(box, twin);
        Assert.Equal(box.Name, twin.Name);

        var reading = new SourceReading<Boxed>(SourceState.Read, box, NotFound);

        Assert.Same(box, reading.Value);
        Assert.Same(box, (reading with { State = SourceState.Unavailable }).Value);
        Assert.True(reading == new SourceReading<Boxed>(SourceState.Read, box, NotFound));
        Assert.False(reading != new SourceReading<Boxed>(SourceState.Read, box, NotFound));
        Assert.True(reading.Equals(new SourceReading<Boxed>(SourceState.Read, box, NotFound)));
        Assert.False(reading.Equals((object)new SourceReading<Boxed>(SourceState.Read, twin, NotFound)));
        Assert.True(reading != new SourceReading<Boxed>(SourceState.Read, twin, NotFound));
        Assert.True(reading != new SourceReading<Boxed>(SourceState.Read, Detail: NotFound));

        // A list is a class as much as a file is one, and it knows no equality of its own either.
        var names = new List<string> { "ui", "api" };
        var list = new SourceReading<List<string>>(SourceState.Read, names);

        Assert.Same(names, list.Value);
        Assert.True(list == new SourceReading<List<string>>(SourceState.Read, names));
        Assert.False(list == new SourceReading<List<string>>(SourceState.Read, ["ui", "api"]));

        // A value that knows its own equality by its letters, spelled twice: two references and one reading, for the
        // reading asks the value and nothing else.
        var spelled = new string("ui".ToCharArray());

        Assert.NotSame(spelled, "ui");
        Assert.True(new SourceReading<string>(SourceState.Read, "ui", NotFound) == new SourceReading<string>(SourceState.Read, spelled, NotFound));
        Assert.Equal(
            new SourceReading<string>(SourceState.Read, "ui", NotFound).GetHashCode(),
            new SourceReading<string>(SourceState.Read, spelled, NotFound).GetHashCode());
    }

    /// <summary>The constraint every reading carries asks for a class, and a class is anything that is not a value
    /// type - so a reading of <see cref="object" /> takes any box at all, and the box keeps the type it was put in
    /// with it. Forty-two of <see cref="int" /> and forty-two of <see cref="long" /> are two readings, and the two
    /// boxes of forty-two are one; a reading of nothing says its empty place, and the number in the words is the
    /// number the box holds.</summary>
    [Fact]
    public void AReadingOfObjectsTakesAnyBoxAtAll()
    {
        var number = new SourceReading<object>(SourceState.Read, 42);
        var again = new SourceReading<object>(SourceState.Read, 42);
        var longer = new SourceReading<object>(SourceState.Read, 42L);
        var text = new SourceReading<object>(SourceState.Read, "42");

        Assert.Equal(42, Assert.IsType<int>(number.Value));
        Assert.Equal(42L, Assert.IsType<long>(longer.Value));
        Assert.True(number == again);
        Assert.False(number != again);
        Assert.False(number == longer);
        Assert.False(number == text);
        Assert.Equal(number.GetHashCode(), again.GetHashCode());
        Assert.Equal("SourceReading { State = Read, Value = 42, Detail =  }", number.ToString());
        Assert.Equal("SourceReading { State = Read, Value = 42, Detail =  }", longer.ToString());
        Assert.Null(new SourceReading<object>(SourceState.Missing, Detail: NoAnswer).Value);
    }

    // ------------------------------------------------------------------ the hash and the reading as a key

    /// <summary>The hash stands still: from one call to the next on the one reading, boxed and unboxed, and on a copy
    /// of the reading. A reading of nothing at all - the state a monitor starts on, no value and no reason - has a
    /// hash like any other, which is what lets the page's rounds hold one in a set; and the readings a round leaves
    /// behind are as many entries as they are readings, however much of them they share.</summary>
    [Fact]
    public void TheHashOfAReadingFollowsItsThreePartsAndStandsStill()
    {
        var status = ClusterFixture.SampleStatus;
        var one = new SourceReading<ClusterStatus>(SourceState.Read, status);
        var two = new SourceReading<ClusterStatus>(SourceState.Read, status);

        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), (one with { }).GetHashCode());

        var bare = new SourceReading<ClusterStatus>(SourceState.Pending);

        Assert.Equal(bare.GetHashCode(), bare.GetHashCode());
        Assert.Equal(bare.GetHashCode(), new SourceReading<ClusterStatus>(SourceState.Pending).GetHashCode());
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Pending).GetHashCode(), (bare with { }).GetHashCode());

        // The six readings a page's file may stand on, and six entries: the state alone, the reason alone, the empty
        // reason and the absent one, the file and no file.
        Assert.Equal(
            6,
            new HashSet<SourceReading<ClusterStatus>>
            {
                bare,
                new(SourceState.Missing),
                new(SourceState.Missing, Detail: string.Empty),
                new(SourceState.Missing, Detail: NotFound),
                new(SourceState.Read, status),
                new(SourceState.Unavailable, null, NoAnswer),
            }.Count);

        // While two of them are the same reading, they are one entry.
        Assert.Single(new HashSet<SourceReading<ClusterStatus>> { one, two, one with { }, new(SourceState.Read, status) });
    }

    /// <summary>A reading is worth as a key: the reading that was put in is found again by one built beside it, by a
    /// copy of itself and by nothing else - a state turned, a reason lengthened and the very same file read twice over
    /// out of the same text are all misses, for the value of a reading is compared by its own equality and two parses
    /// of one file are two values.</summary>
    [Fact]
    public void AReadingIsOneKeyOfADictionaryAndOneEntryOfASet()
    {
        var status = ClusterFixture.SampleStatus;
        var read = new SourceReading<ClusterStatus>(SourceState.Read, status);
        var shown = new Dictionary<SourceReading<ClusterStatus>, string> { [read] = "the round that read it" };

        Assert.Single(shown);
        Assert.Equal("the round that read it", shown[new SourceReading<ClusterStatus>(SourceState.Read, status)]);
        Assert.Equal("the round that read it", shown[read with { }]);
        Assert.Equal("the round that read it", shown[read with { State = SourceState.Read }]);
        Assert.False(shown.ContainsKey(read with { State = SourceState.Missing }));
        Assert.False(shown.ContainsKey(read with { Detail = NotFound }));
        Assert.False(shown.ContainsKey(new SourceReading<ClusterStatus>(SourceState.Read, ClusterFixture.SampleStatus)));
        Assert.False(shown.ContainsKey(new SourceReading<ClusterStatus>(SourceState.Pending)));

        // The absent value and the reason are keys as good as any: a set of the readings one file may stand on holds
        // each of them once, and the monitor's own Pending stands with the rest.
        var set = new HashSet<SourceReading<AksService>>
        {
            new(SourceState.Pending),
            new(SourceState.Pending),
            new(SourceState.Missing, Detail: NotFound),
            new(SourceState.Unavailable, Detail: NoAnswer),
            new(SourceState.Malformed, Detail: NotJson),
            new(SourceState.Read, ClusterFixture.SampleService),
            new(SourceState.Read, ClusterFixture.SampleService),
        };

        Assert.Equal(6, set.Count);
        Assert.Contains(new SourceReading<AksService>(SourceState.Unavailable, Detail: NoAnswer), set);
        Assert.DoesNotContain(new SourceReading<AksService>(SourceState.Unavailable), set);
    }

    // ------------------------------------------------------------------ falling a reading apart

    /// <summary>A full reading falls apart into the three it was given - the state, the file itself and the reason -
    /// and the fall apart writes into the three places it is handed, the absent value and all: a place that stood
    /// filled is emptied by it, so a caller that reuses its variables is shown what the reading holds rather than
    /// what it held before. The three come out in the order the record declares them, in the three types the parts
    /// stand in.</summary>
    [Fact]
    public void AReadingFallsApartIntoTheThreeItWasGiven()
    {
        var service = ClusterFixture.SampleService;
        var reading = new SourceReading<AksService>(SourceState.Unavailable, Detail: NoAnswer);

        var (state, value, detail) = reading;

        Assert.Equal(SourceState.Unavailable, state);
        Assert.Null(value);
        Assert.Equal(NoAnswer, detail);

        SourceState filled = SourceState.Read;
        AksService? file = service;
        string? reason = NotFound;

        (filled, file, reason) = reading;

        Assert.Equal(SourceState.Unavailable, filled);
        Assert.Null(file);
        Assert.Equal(NoAnswer, reason);

        // The very file, out of the reading that was handed it.
        var (readState, readValue, readDetail) = new SourceReading<AksService>(SourceState.Read, service, NotJson);

        Assert.Equal(SourceState.Read, readState);
        Assert.Same(service, readValue);
        Assert.Equal(NotJson, readDetail);

        var deconstruct = typeof(SourceReading<AksService>).GetMethod("Deconstruct")!;

        Assert.True(deconstruct.IsPublic);
        Assert.False(deconstruct.IsStatic);
        Assert.Equal(
            [typeof(SourceState).MakeByRefType(), typeof(AksService).MakeByRefType(), typeof(string).MakeByRefType()],
            [.. deconstruct.GetParameters().Select(parameter => parameter.ParameterType)]);
    }

    // ------------------------------------------------------------------ the words a reading says

    /// <summary>A value that says nothing about itself leaves the same empty place as the value that is not there:
    /// the value whose own words are the empty string, the value that answers the question every object is asked with
    /// nothing at all and the reading handed no value print alike, and only the equality tells the three apart. The
    /// empty place is the reading's own, whatever the value's type - a file of no characters prints it as flatly as
    /// an absent one.</summary>
    [Fact]
    public void AValueThatSaysNothingPrintsAsTheValueThatIsNotThere()
    {
        const string emptyPlace = "SourceReading { State = Read, Value = , Detail =  }";

        Assert.Equal(emptyPlace, new SourceReading<ClusterStatus>(SourceState.Read).ToString());
        Assert.Equal(emptyPlace, new SourceReading<Silent>(SourceState.Read, new Silent()).ToString());
        Assert.Equal(emptyPlace, new SourceReading<Voiceless>(SourceState.Read, new Voiceless()).ToString());
        Assert.Equal(emptyPlace, new SourceReading<string>(SourceState.Read, string.Empty).ToString());

        Assert.NotEqual(new SourceReading<Silent>(SourceState.Read, new Silent()), new SourceReading<Silent>(SourceState.Read));

        // The three print alike and are three readings: a reading of one value is no reading of another, however
        // alike the two values say themselves.
        var voiceless = new SourceReading<Voiceless>(SourceState.Read, new Voiceless());
        var silent = new SourceReading<Silent>(SourceState.Read, new Silent());

        Assert.Equal(voiceless.ToString(), silent.ToString());
        Assert.False(voiceless.Equals((object)silent));
        Assert.False(object.Equals(voiceless, silent));
    }

    /// <summary>Every part is said as it stands and not as it means: a reason that carries the record's own
    /// punctuation, a newline, a tab and a quote is written out as it was given, neither escaped nor quoted nor cut,
    /// and a value is said in its own words with no quotes about it - a word, a number in a box and the whole of a
    /// status file alike. The three parts keep the order the record declares them in, with the reason last of all and
    /// the closing brace behind it.</summary>
    [Fact]
    public void AReadingSaysItsPartsAsTheyStandAndNotAsTheyMean()
    {
        const string rough = "the file has neither {nodes} nor namespaces,\nbut says \"so\"\t";

        Assert.Equal(
            $"SourceReading {{ State = Malformed, Value = ui, Detail = {rough} }}",
            new SourceReading<string>(SourceState.Malformed, "ui", rough).ToString());

        Assert.Equal(
            "SourceReading { State = Missing, Value = ui, Detail =  }",
            new SourceReading<string>(SourceState.Missing, "ui").ToString());

        Assert.Equal(
            "SourceReading { State = Read, Value = 42, Detail = the file is not valid JSON }",
            new SourceReading<object>(SourceState.Read, 42, NotJson).ToString());

        var status = ClusterFixture.SampleStatus;
        var reading = new SourceReading<ClusterStatus>(SourceState.Read, status, NotFound);

        Assert.Equal($"SourceReading {{ State = Read, Value = {status}, Detail = {NotFound} }}", reading.ToString());
        Assert.Contains($"Value = {status}, Detail = {NotFound}", reading.ToString(), StringComparison.Ordinal);
        Assert.EndsWith($"Detail = {NotFound} }}", reading.ToString(), StringComparison.Ordinal);
    }

    /// <summary>A state that is none of the five is a number the reading holds, arrives at how it will: the union of
    /// two states that no sentence of the page knows is the reading its number says it is, equal to the reading built
    /// from that number straight out, fallen apart as the number, and said as the number - the negative of it and the
    /// ends of <see cref="int" /> included, none of which is a word the record looks up.</summary>
    [Fact]
    public void AStateThatIsNoneOfTheFiveIsHeldAndSaidAsTheNumberItStandsOn()
    {
        var above = new SourceReading<ClusterStatus>((SourceState)5, Detail: NotFound);
        var spelled = new SourceReading<ClusterStatus>(SourceState.Read | SourceState.Malformed, Detail: NotFound);

        Assert.Equal((SourceState)5, above.State);
        Assert.False(Enum.IsDefined(above.State));
        Assert.True(above == spelled);
        Assert.False(above != spelled);
        Assert.Equal(above.GetHashCode(), spelled.GetHashCode());
        Assert.Equal("SourceReading { State = 5, Value = , Detail = the address answered HTTP 404 }", above.ToString());

        var (state, value, detail) = above;

        Assert.Equal((SourceState)5, state);
        Assert.Null(value);
        Assert.Equal(NotFound, detail);

        Assert.Equal("SourceReading { State = -1, Value = , Detail =  }", new SourceReading<AksService>((SourceState)(-1)).ToString());
        Assert.Equal(
            $"SourceReading {{ State = {int.MinValue}, Value = , Detail =  }}",
            new SourceReading<AksService>((SourceState)int.MinValue).ToString());
        Assert.Equal(
            $"SourceReading {{ State = {int.MaxValue}, Value = , Detail =  }}",
            new SourceReading<AksService>((SourceState)int.MaxValue).ToString());

        Assert.True(above != new SourceReading<ClusterStatus>((SourceState)(-1), Detail: NotFound));
        Assert.Equal(above, above with { State = (SourceState)5 });
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Missing, Detail: NotFound), above with { State = SourceState.Missing });
    }

    /// <summary>None of a reading's three parts is a number a viewer's culture turns or a word it looks up: the state
    /// is said in the letters the enum spells it, a state outside the five in the plain digits of the number it stands
    /// on with the sign of a negative one in front, and the reason in the very letters it was given - so the words are
    /// the same for a reader who counts with commas and points where an English one does the opposite. The equality
    /// and the hash follow the same letters in either mouth, for nothing of a reading is read out of the culture it
    /// is asked in.</summary>
    [Fact]
    public void TheWordsAndTheEqualityOfAReadingOutliveWhateverCultureTheViewerSpeaks()
    {
        var stated = new SourceReading<ClusterStatus>(SourceState.Read, Detail: "1,234.5 of 10 s");
        var numbered = new SourceReading<AksService>((SourceState)5, Detail: "-1");
        var german = new SourceReading<ClusterStatus>((SourceState)(-1), Detail: "1.234,5");

        Assert.Equal("SourceReading { State = Read, Value = , Detail = 1,234.5 of 10 s }", stated.ToString());
        Assert.Equal("SourceReading { State = 5, Value = , Detail = -1 }", numbered.ToString());

        ViewerCulture.German(() =>
        {
            Assert.Equal("SourceReading { State = Read, Value = , Detail = 1,234.5 of 10 s }", stated.ToString());
            Assert.Equal("SourceReading { State = 5, Value = , Detail = -1 }", numbered.ToString());
            Assert.Equal("SourceReading { State = -1, Value = , Detail = 1.234,5 }", german.ToString());

            // A reading made under the German reader is the reading made under any other, in its words, its equality
            // and its hash.
            var also = new SourceReading<ClusterStatus>((SourceState)(-1), Detail: "1.234,5");

            Assert.Equal(german, also);
            Assert.True(german == also);
            Assert.Equal(german.GetHashCode(), also.GetHashCode());
            Assert.Equal(german.ToString(), also.ToString());
        });

        Assert.Equal("SourceReading { State = -1, Value = , Detail = 1.234,5 }", german.ToString());
    }

    // ------------------------------------------------------------------ the reading's own shape

    /// <summary>The whole public surface of a reading, as the running assembly declares it: the three parts in the
    /// order the record names them, each of them readable and written once - at the making, by the constructor or by
    /// <c>with</c>, and never after; the one constructor of three arguments, of which only the state is required and
    /// each of the other two defaults to the nothing it holds; and the five operations every record is asked, the two
    /// operators among them, both of them the reading's own kind on both sides and a <see cref="bool" /> out. A third
    /// member added to the record has to be written here as well, which is the point: a reading cannot be handed a
    /// part nobody decided what it means. That the record is sealed, of one type argument constrained to a class, and
    /// no reading of one file the equal of a reading of the other is <see cref="SourceStateEdgeCaseTests" />'s; the
    /// parts, the arguments and the operations are here.</summary>
    [Fact]
    public void AReadingOffersItsThreePartsAndTheOperationsEveryRecordIsAsked()
    {
        var reading = typeof(SourceReading<ClusterStatus>);

        Assert.Equal(
            ["Deconstruct", "Detail", "Equals", "GetHashCode", "State", "ToString", "Value", "op_Equality", "op_Inequality"],
            Surface(reading));

        Assert.Equal(["State", "Value", "Detail"], [.. DeclaredProperties(reading).Select(property => property.Name)]);
        Assert.Equal(
            [typeof(SourceState), typeof(ClusterStatus), typeof(string)],
            [.. DeclaredProperties(reading).Select(property => property.PropertyType)]);
        Assert.All(DeclaredProperties(reading), property => Assert.True(property.CanRead, $"{property.Name} cannot be read"));
        Assert.All(DeclaredProperties(reading), property => Assert.True(property.SetMethod is not null, $"{property.Name} cannot be written at the making"));
        Assert.All(
            DeclaredProperties(reading),
            property => Assert.True(WrittenOnlyAtTheMaking(property), $"{property.Name} may be turned after the reading was made"));

        var parameters = Assert.Single(reading.GetConstructors()).GetParameters();

        Assert.Equal(["State", "Value", "Detail"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [typeof(SourceState), typeof(ClusterStatus), typeof(string)],
            [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.False(parameters[0].HasDefaultValue);
        Assert.True(parameters[1].HasDefaultValue);
        Assert.Null(parameters[1].DefaultValue);
        Assert.True(parameters[2].HasDefaultValue);
        Assert.Null(parameters[2].DefaultValue);

        foreach (var name in new[] { "op_Equality", "op_Inequality" })
        {
            var operation = reading.GetMethod(name)!;

            Assert.True(operation.IsPublic);
            Assert.True(operation.IsStatic);
            Assert.False(operation.IsGenericMethod);
            Assert.Equal(typeof(bool), operation.ReturnType);
            Assert.Equal([reading, reading], [.. operation.GetParameters().Select(parameter => parameter.ParameterType)]);
        }

        Assert.Equal(
            ["Object", "SourceReading`1"],
            [.. reading
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("Equals", StringComparison.Ordinal))
                .Select(method => method.GetParameters()[0].ParameterType.Name)
                .Order(StringComparer.Ordinal)]);

        Assert.False(reading.IsAbstract);
        Assert.Equal(typeof(object), reading.BaseType);
        Assert.Equal([typeof(IEquatable<SourceReading<ClusterStatus>>)], [.. reading.GetInterfaces()]);
    }

    /// <summary>A reading cannot be made without its state: the one constructor takes three arguments and no
    /// constructor takes none, so the record has no default to fall back on and a reading nobody gave a state is not a
    /// thing the assembly can be asked for - the reflection that tries is turned down. Asked the three the way the
    /// source asks them, the same constructor makes the same reading the <c>new</c> makes, the state it names and the
    /// two parts it leaves out standing as they stand when nobody names them.</summary>
    [Fact]
    public void AReadingCannotBeMadeWithoutItsState()
    {
        var reading = typeof(SourceReading<ClusterStatus>);

        Assert.Single(reading.GetConstructors());
        Assert.Null(reading.GetConstructor(Type.EmptyTypes));
        Assert.Throws<MissingMethodException>(() => Activator.CreateInstance(reading));

        // The one way to make a reading is by the three, and reflection asks the same way the source does.
        var made = Assert.IsType<SourceReading<ClusterStatus>>(Activator.CreateInstance(reading, SourceState.Pending, null, null));

        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Pending), made);
        Assert.True(made == new SourceReading<ClusterStatus>(SourceState.Pending));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members a reading declares itself: its three parts and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the tests above rather than by this list.</summary>
    private static List<string> Surface(Type reading) =>
        [.. reading
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The public parts a reading declares itself, in the order the record declares them.</summary>
    private static PropertyInfo[] DeclaredProperties(Type reading) =>
        reading.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    /// <summary>Whether a part may be written only while the reading is being made: the record's init-only accessor,
    /// which the compiler marks with the modreq that a <c>with</c> carries and no assignment does.</summary>
    private static bool WrittenOnlyAtTheMaking(PropertyInfo property) =>
        property.SetMethod is { } setter
        && setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier =>
            modifier.FullName is { } name && name.Equals("System.Runtime.CompilerServices.IsExternalInit", StringComparison.Ordinal));

    /// <summary>A value of a reading that knows no equality of its own: two of these saying the same name are two
    /// values, which is how the reading comes to hold the reference it was handed rather than the words it says.</summary>
    private sealed class Boxed(string name)
    {
        public string Name { get; } = name;
    }

    /// <summary>A value whose own words are the empty string - an empty place in a reading's words, with a value
    /// standing in it.</summary>
    private sealed class Silent
    {
        public override string ToString() => string.Empty;
    }

    /// <summary>A value that answers the one question every object is asked with nothing at all: the reading prints
    /// its empty place all the same, for there is nothing else to print.</summary>
    private sealed class Voiceless
    {
        public override string ToString() => null!;
    }
}
