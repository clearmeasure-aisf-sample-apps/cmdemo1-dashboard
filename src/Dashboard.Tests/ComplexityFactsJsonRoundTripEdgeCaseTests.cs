using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="ComplexityFacts" /> asked from the side that goes out rather than the side that comes in: what the
/// record's three public parts - the average its methods make, the worst of them and the count of the methods they
/// are of, declared in <c>src/Dashboard/Health/BuildInfo.cs</c> - hand a JSON writer, and what stands at the end of
/// that when the writing is read back the only way these facts can be read, through
/// <see cref="BuildInfo.Parse" /> of a build endpoint's answer.
/// <para>
/// The name this issue asked for is held. <see cref="ComplexityFactsEdgeCaseTests" /> pins the three parts kept as
/// they are given and at the ends the number and the two counts run to, the words the record says them in, the
/// equality, the hash and the operators, the falling apart and the copy, and the facts a build file says at the ends
/// of its numbers; <see cref="ComplexityFactsBoundaryEdgeCaseTests" /> pins the rest of the number line, the parts
/// turned as the record is made, the culture the record's own words follow, the comparer and the dictionary key, and
/// the file's own three numbers; <see cref="ComplexityFactsReadEdgeCaseTests" /> pins the read path - the overflow,
/// the rounding, the exponent, the negative zero, the section that is no section and the part named in another case;
/// and <see cref="ComplexityFactsSurfaceEdgeCaseTests" /> pins the compiled face member by member, the two operators
/// asked in the open and the copy walked to every edge. What none of them writes is the record's own three numbers
/// out through a writer and back through the reader, which is the whole of this file, along with two shapes of the
/// record's own words no other file says: the average said between two unsaid and the methods said between two
/// unsaid.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <see cref="ComplexityFacts" /> holds
/// no moment and calls nothing, every number is one the test brings or one it reads from an answer of its own
/// making, and the one build file read is the text the dashboard's own facts are made of.
/// </para>
/// </summary>
public class ComplexityFactsJsonRoundTripEdgeCaseTests
{
    /// <summary>The three numbers the build file the dashboard serves says of its methods.</summary>
    private static readonly ComplexityFacts Measured = new(1.9, 34, 5210);

    /// <summary>The facts of a build that measured nothing: a record of a measurement, and not the no record a build
    /// without a complexity section is.</summary>
    private static readonly ComplexityFacts Unmeasured = new(null, null, null);

    /// <summary>The names a build file writes its parts under: the endpoint's lower camel case.</summary>
    private static readonly JsonSerializerOptions AsTheEndpointNamesThem = new(JsonSerializerDefaults.Web);

    /// <summary>The same, with the parts nobody said left out of the answer instead of said as nothing - the way an
    /// older app, which measured fewer of the three, writes its section.</summary>
    private static readonly JsonSerializerOptions OmittingTheUnsaid = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>The same, with the endless and the not-a-number written as the words the JSON format allows - and
    /// the dashboard's reader does not.</summary>
    private static readonly JsonSerializerOptions NamingTheEndless = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    // ----- what the three parts hand a writer

    /// <summary>The record's whole way out is its three parts: a writer is handed the three of them, in the order the
    /// declaration names them, each under the name it is declared under and as the very value it stands on - the
    /// average as the number it is, the two counts as the whole numbers they are - the zero written as a number and a
    /// count, and the part nobody said handed over as the nothing it stands on, written in its place rather than
    /// dropped. Told the names a build file uses, the same three parts are the text an endpoint would have written.</summary>
    [Fact]
    public void TheThreePartsAreTheWholeOfWhatTheRecordHandsAWriter()
    {
        var asDeclared = JsonSerializer.Serialize(Measured);

        Assert.Equal("""{"Average":1.9,"Max":34,"Methods":5210}""", asDeclared);
        Assert.Equal(["Average", "Max", "Methods"], KeysOf(asDeclared));

        Assert.Equal("""{"average":1.9,"max":34,"methods":5210}""", Written(Measured));
        Assert.Equal(["average", "max", "methods"], KeysOf(Written(Measured)));

        // The zero of a part is a part; the nothing of one is written, in its place, as nothing.
        Assert.Equal("""{"average":0,"max":0,"methods":0}""", Written(new ComplexityFacts(0.0, 0, 0)));
        Assert.Equal("""{"average":null,"max":34,"methods":null}""", Written(new ComplexityFacts(null, 34, null)));
        Assert.Equal("""{"average":null,"max":null,"methods":null}""", Written(Unmeasured));
        Assert.Equal(["average", "max", "methods"], KeysOf(Written(Unmeasured)));
    }

    /// <summary>The words the record says of itself and the words a writer makes of it leave the unsaid parts out in
    /// two different ways - the record's own words leave the place of an unsaid part empty between the mark and the
    /// comma, the writer says it as <c>null</c> - and both come back to the same record, the unsaid part standing as
    /// the nothing it went as rather than as the zero a part said as a number would stand on. The two shapes of the
    /// record's own words here are the two no other file says: the average said between two unsaid, and the methods
    /// said between two unsaid.</summary>
    [Fact]
    public void TheWordsTheRecordSaysLeaveTheUnsaidOutWhereTheWriterSaysItAsNothing()
    {
        var averageAlone = new ComplexityFacts(1.9, null, null);
        var methodsAlone = new ComplexityFacts(null, null, 5210);

        Assert.Equal("ComplexityFacts { Average = 1.9, Max = , Methods =  }", Said(averageAlone));
        Assert.Equal("ComplexityFacts { Average = , Max = , Methods = 5210 }", Said(methodsAlone));

        Assert.Equal("""{"average":1.9,"max":null,"methods":null}""", Written(averageAlone));
        Assert.Equal("""{"average":null,"max":null,"methods":5210}""", Written(methodsAlone));

        Assert.Equal(averageAlone, ReadBack("""{ "average": 1.9, "max": null, "methods": null }"""));
        Assert.Equal(methodsAlone, ReadBack("""{ "average": null, "max": null, "methods": 5210 }"""));
        Assert.NotEqual(averageAlone, ReadBack("""{ "average": 1.9, "max": 0, "methods": 0 }"""));
    }

    // ----- the way back in

    /// <summary>The names matter as much as the numbers: the record's own face, written under the names it declares
    /// its parts under, is a section the dashboard reads as no complexity at all - every part named in a case the
    /// endpoint does not write is a part left unsaid, and all three left unsaid are no facts - while the same three
    /// numbers under the endpoint's names are the record again, and the answer that leaves the unsaid parts out of it
    /// altogether is the record with those parts unsaid.</summary>
    [Fact]
    public void TheFactsReadBackOnlyUnderTheNamesABuildFileWritesThemUnder()
    {
        Assert.Null(ReadBack(JsonSerializer.Serialize(Measured)));
        Assert.Equal(Measured, ReadBack(Written(Measured)));

        // The section that holds nothing at all, however it came to hold nothing, says no facts.
        Assert.Null(ReadBack(Omitted(Unmeasured)));
        Assert.Null(ReadBack("{}"));
        Assert.Null(ReadBack("""{ "average": null, "max": null, "methods": null }"""));

        // One part said and the two others left out of the answer is the record of the one part said.
        Assert.Equal(new ComplexityFacts(1.9, null, null), ReadBack("""{ "average": 1.9 }"""));
        Assert.Equal(new ComplexityFacts(null, 34, null), ReadBack("""{ "max": 34 }"""));
        Assert.Equal(new ComplexityFacts(null, null, 5210), ReadBack("""{ "methods": 5210 }"""));
    }

    /// <summary>One part said between two unsaid, three ways round: the writer told to leave the unsaid out hands
    /// over the one key the record has to give, the writer not so told hands over all three of them, and either
    /// answer reads back as the record it was written from - the average, the worst and the count each alone in turn,
    /// the two others nothing.</summary>
    [Theory]
    [InlineData(1.9, null, null)]
    [InlineData(null, 34, null)]
    [InlineData(null, null, 5210)]
    public void OnePartSaidBetweenTwoUnsaidGoesOutAndComesBack(double? average, int? max, int? methods)
    {
        var facts = new ComplexityFacts(average, max, methods);
        var omitted = Omitted(facts);

        Assert.Single(KeysOf(omitted));
        Assert.Equal(["average", "max", "methods"], KeysOf(Written(facts)));

        Assert.Equal(facts, RoundTrip(facts));
        Assert.Equal(facts, ReadBack(omitted));
    }

    /// <summary>The numbers a build file may say of these three parts come back the very numbers they went as - the
    /// zero, the least number a double holds, a millionth, a whole number without a point, the sum of a tenth and a
    /// fifth to its last digit, the neighbour of <c>1.9</c> one hair above it, a hundred to the three-hundredth
    /// power, the largest number short of the endless, and the top of the count - the writing holding the last digit
    /// of them and the reading taking it back, so a record that went out as one measurement never comes back as
    /// another.</summary>
    [Theory]
    [InlineData(1.9, 34, 5210)]
    [InlineData(0.0, 0, 0)]
    [InlineData(2d, 2, 2)]
    [InlineData(5E-324, 1, 1)]
    [InlineData(1e-06, 0, 1)]
    [InlineData(0.30000000000000004, 2147483647, 2147483647)]
    [InlineData(1.9000000000000001, 34, 5210)]
    [InlineData(1E+300, 40, 40)]
    [InlineData(1.7976931348623157E+308, 1, 5210)]
    public void TheFactsAFileCanSayGoOutAndComeBackTheSameFacts(double average, int max, int methods)
    {
        var facts = new ComplexityFacts(average, max, methods);
        var back = RoundTrip(facts);

        Assert.NotNull(back);
        Assert.Equal(facts, back);
        Assert.NotSame(facts, back!);
        Assert.Equal(facts.GetHashCode(), back.GetHashCode());
        Assert.Equal(Said(facts), Said(back));

        Assert.Equal(average, back.Average);
        Assert.Equal(max, back.Max);
        Assert.Equal(methods, back.Methods);
    }

    // ----- where going out and coming back stop being one another

    /// <summary>The endless and the not-a-number are numbers the record keeps and numbers no writer may hand over:
    /// asked to write them as the answer's number, the writer refuses and says why. Told to name them instead - the
    /// one way JSON lets them be written - they go out as the words <c>NaN</c>, <c>Infinity</c> and
    /// <c>-Infinity</c>, and those words are a part the dashboard's reader leaves unsaid: the two counts beside them
    /// come back and the average does not, and the answer that says the endless as the bare word the format does not
    /// allow is no answer at all, the same as the error page beside it. What the record stands on and what a build
    /// file may say are therefore not the same three numbers - which is the point of the reader's refusing them.</summary>
    [Fact]
    public void TheEndlessAndTheNotANumberAreNoNumberAWriterWillHandOver()
    {
        var notANumber = new ComplexityFacts(double.NaN, 34, 5210);
        var endless = new ComplexityFacts(double.PositiveInfinity, 34, 5210);
        var neverEnding = new ComplexityFacts(double.NegativeInfinity, 34, 5210);

        Assert.Throws<ArgumentException>(() => Written(notANumber));
        Assert.Throws<ArgumentException>(() => Written(endless));
        Assert.Throws<ArgumentException>(() => Written(neverEnding));

        Assert.True(notANumber.Average is { } theNaN && double.IsNaN(theNaN));
        Assert.Equal("""{"average":"NaN","max":34,"methods":5210}""", Named(notANumber));
        Assert.Equal("""{"average":"Infinity","max":34,"methods":5210}""", Named(endless));
        Assert.Equal("""{"average":"-Infinity","max":34,"methods":5210}""", Named(neverEnding));

        // Named, they are no longer numbers, and a part that is not a number is a part left unsaid.
        Assert.Equal(new ComplexityFacts(null, 34, 5210), ReadBack(Named(notANumber)));
        Assert.Equal(new ComplexityFacts(null, 34, 5210), ReadBack(Named(endless)));
        Assert.Equal(new ComplexityFacts(null, 34, 5210), ReadBack(Named(neverEnding)));
        Assert.Null(ReadBack("""{ "average": "NaN" }"""));

        // The bare word, which no JSON answer holds, is no answer to read at all.
        Assert.Null(BuildInfo.Parse("""{ "version": "2.4.15", "complexity": { "average": NaN } }"""));
        Assert.Equal(Measured, BuildInfo.Parse("""{ "version": "2.4.15", "complexity": { "average": 1.9, "max": 34, "methods": 5210 } }""")!.Complexity);
    }

    /// <summary>The sign a writer hands over is a sign a reader refuses: a record standing on a negative average and
    /// two negative counts writes them as plainly as the numbers they are, keeps them as they were given, and reads
    /// back as no facts at all, every one of its three parts left unsaid by a build file that could not say them.
    /// The least number there is fares the same way on its own, and loses only its average; the largest number and
    /// the top of the count go out and come back whole, so it is the sign and not the size that parts the writer
    /// from the reader.</summary>
    [Fact]
    public void TheSignAWriterHandsOverIsNoPartTheReaderTakesBack()
    {
        var under = new ComplexityFacts(-1.5, -34, int.MinValue);

        Assert.Equal("""{"average":-1.5,"max":-34,"methods":-2147483648}""", Written(under));
        Assert.Equal(-1.5, under.Average);
        Assert.Equal(-34, under.Max);
        Assert.Equal(int.MinValue, under.Methods);
        Assert.Null(RoundTrip(under));

        var least = new ComplexityFacts(double.MinValue, 0, 0);

        Assert.Equal("""{"average":-1.7976931348623157E+308,"max":0,"methods":0}""", Written(least));
        Assert.Equal(new ComplexityFacts(null, 0, 0), RoundTrip(least));

        var most = new ComplexityFacts(double.MaxValue, int.MaxValue, int.MaxValue);

        Assert.Equal("""{"average":1.7976931348623157E+308,"max":2147483647,"methods":2147483647}""", Written(most));
        Assert.Equal(most, RoundTrip(most));
    }

    /// <summary>The complexity of the build file the dashboard serves, gone out and back in: the three numbers the
    /// file writes make the record the file stands for, the record writes that same text back under the names the
    /// file names them under, and the record that comes out of the answer's own reading of that text is the record
    /// that went in, said by the card in the one line the card says it in.</summary>
    [Fact]
    public void TheComplexityOfTheBuildFileTheDashboardServesGoesOutAndComesBack()
    {
        var facts = BuildInfo.Parse(Optics.Build)!.Complexity!;

        Assert.Equal(Measured, facts);
        Assert.Equal("""{"average":1.9,"max":34,"methods":5210}""", Written(facts));

        var back = RoundTrip(facts)!;

        Assert.Equal(facts, back);
        Assert.Equal(1.9, back.Average);
        Assert.Equal(34, back.Max);
        Assert.Equal(5210, back.Methods);
        Assert.Equal("average 1.9, worst 34 (5,210 methods)", BuildText.Complexity(back));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The complexity one answer holds, as the dashboard reads it: the version stands so that the answer is
    /// a build whatever its section says, and the section under the question is the text the test brings.</summary>
    private static ComplexityFacts? ReadBack(string section) =>
        BuildInfo.Parse($$"""{ "version": "2.4.15", "complexity": {{section}} }""")!.Complexity;

    /// <summary>The facts as a build file would write them, under the names the endpoint writes its parts under.</summary>
    private static string Written(ComplexityFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        return JsonSerializer.Serialize(facts, AsTheEndpointNamesThem);
    }

    /// <summary>The facts written with the endless and the not-a-number named as words, which is the one way a writer
    /// may hand them over at all - and the one way no reader of a build endpoint's answer takes them back.</summary>
    private static string Named(ComplexityFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        return JsonSerializer.Serialize(facts, NamingTheEndless);
    }

    /// <summary>The facts written with the parts nobody said left out of the answer altogether, the way an app that
    /// measured fewer of the three writes its section.</summary>
    private static string Omitted(ComplexityFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        return JsonSerializer.Serialize(facts, OmittingTheUnsaid);
    }

    /// <summary>The round trip of the record's own three parts: written under the endpoint's names, held out to the
    /// reader of a build endpoint's answer, and read back.</summary>
    private static ComplexityFacts? RoundTrip(ComplexityFacts facts) => ReadBack(Written(facts));

    /// <summary>The names a writing holds, in the order the writing holds them - which is the order the record
    /// declares its three parts in, since the record has nothing else to hand a writer.</summary>
    private static string[] KeysOf(string json)
    {
        using var document = JsonDocument.Parse(json);

        return [.. document.RootElement.EnumerateObject().Select(property => property.Name)];
    }

    /// <summary>The words a record says its three parts in, taken in the invariant culture whatever the reader of
    /// this test happens to stand in: the record's own words follow the reader, which
    /// <see cref="ComplexityFactsBoundaryEdgeCaseTests" /> pins, so the strings here are the point and the culture
    /// only the way to say them once.</summary>
    private static string Said(ComplexityFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return facts.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
