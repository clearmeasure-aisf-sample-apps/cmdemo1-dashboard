using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still open for <see cref="ComplexityFacts" />, the three
/// measurements a build makes of the complexity of its methods, declared in
/// <c>src/Dashboard/Health/ComplexityFacts.cs</c>: the average they make, the worst of them and the count of the methods
/// they are of - pinned here as the compiled type stands, member by member, and at the asks of its public members
/// that no other file has answered with a fixed value of its own.
/// <para>
/// The name this issue asked for is held. <see cref="ComplexityFactsEdgeCaseTests" /> already pins the three parts
/// kept as they are given and at the ends the number and the two counts run to, the not-a-number and the endless the
/// record keeps and the zero they are not, the words the record says them in, the equality, the hash and the two
/// operators asked of two records and of a record beside the nothing, the falling apart of a full record, an empty
/// one and a zeroed one, the copy that turns one part and keeps the other two, and the facts a build file says at
/// the ends of its numbers or not at all; <see cref="ComplexityFactsBoundaryEdgeCaseTests" /> pins the rest of the
/// number line the average stands on, the last digit of it and its neighbour, the parts turned as the record is made,
/// the culture the record's own words follow while the card's refuse to, the comparer and the dictionary key, and the
/// file's own three numbers; and <see cref="ComplexityFactsReadEdgeCaseTests" /> pins the read path - the overflow,
/// the underflow, the rounding at the top of the count and at the half, the exponent, the negative zero, the section
/// that is no section and the part named in another case - while <see cref="BuildInfoTests" /> and
/// <see cref="HealthHelperCoverageTests" /> pin the answer the dashboard serves and the words the card makes of it.
/// </para>
/// <para>
/// What none of them reads, and this file does, is the shape itself: that the record declares its three parts and
/// the members every record is asked and nothing else, that the parts are init-only and the fields behind them
/// private, that the reader of a build file's section is no public way in, what the two operators answer when the
/// nothing stands on either side of them or on both, what falls out of a record with a part said between two unsaid
/// - including through the three out parameters themselves, which keep a nothing a nothing and a sign its sign -
/// what the compiler's own copy does at every edge the three parts run to, and that the record is compared for
/// being one measurement and never ordered for being the one before another.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <see cref="ComplexityFacts" /> holds
/// no moment and calls nothing, and every number is one the test brings or one it reads from an answer of its own
/// making.
/// </para>
/// </summary>
public class ComplexityFactsSurfaceEdgeCaseTests
{
    /// <summary>Every binding the declaration scan needs: only what the type itself declares, public and otherwise,
    /// instance and static.</summary>
    private const BindingFlags DeclaredOnly =
        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>The three numbers the build file the dashboard serves says of its methods: the average they make,
    /// the worst of them and the count of them.</summary>
    private static readonly ComplexityFacts Measured = new(1.9, 34, 5210);

    /// <summary>The facts of a build that measured nothing: the record that says none of its three parts, which is
    /// a record of a measurement and not the no record a build without a complexity section is.</summary>
    private static readonly ComplexityFacts Unmeasured = new(null, null, null);

    /// <summary>The <c>==</c> a caller writes, as the public static method the compiler is asked to put there.</summary>
    private static readonly MethodInfo Equality = Operator("op_Equality");

    /// <summary>The <c>!=</c> a caller writes, in the same dress.</summary>
    private static readonly MethodInfo Inequality = Operator("op_Inequality");

    /// <summary>The copy every <c>with</c> walks, held as the public method of the compiler's naming it is:
    /// <see cref="ComplexityFactsEdgeCaseTests" /> pins what a copy says, this file pins the method it goes through.</summary>
    private static readonly MethodInfo TheCopy =
        typeof(ComplexityFacts).GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance)!;

    /// <summary>The writing behind the words the record says of itself, which is the compiler's private one.</summary>
    private static readonly MethodInfo TheWriting =
        typeof(ComplexityFacts).GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(StringBuilder)], null)!;

    /// <summary>The three parts at the ends they run to and at the numbers between them that nothing else pins the
    /// way of: the record that says nothing, the record that says the zero of them all, the endless, the not-a-number,
    /// the smallest number there is and the largest, the least count and the most of it, and the zero with a sign of
    /// its own.</summary>
    private static readonly ComplexityFacts[] Edges =
    [
        Unmeasured,
        new ComplexityFacts(0.0, 0, 0),
        Measured,
        new ComplexityFacts(double.NaN, null, null),
        new ComplexityFacts(double.PositiveInfinity, int.MaxValue, int.MaxValue),
        new ComplexityFacts(double.MinValue, int.MinValue, int.MinValue),
        new ComplexityFacts(double.Epsilon, null, null),
        new ComplexityFacts(-0.0, 34, null),
    ];

    // ----- the shape the compiled record holds

    /// <summary>The record's whole public face, as the assembly holds it: sealed, of <see cref="object" />, carrying
    /// the one interface every record carries and no other; its three parts named in the order the declaration names
    /// them and each held as the number it stands on - a number and two counts, none of them ever a zero the record
    /// made up for a part nobody said - each readable and each written only as the making or a copy writes it; the
    /// fields behind them private and no field public; the members every record is asked, comparing, hashing, saying
    /// itself, falling apart and copying with the two operators a caller writes as <c>==</c> and <c>!=</c>, and
    /// nothing else to call at all. The contract and the writing are the compiler's private ones, the clone carries
    /// the name only the compiler writes, and the equality is asked two ways - of the record and of an object.
    /// A fourth part, or one of the three turned away from the type it is held as, has to be written here as well,
    /// which is the point: the card cannot be shown a part nobody decided what it means.</summary>
    [Fact]
    public void TheFactsDeclareThreePartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(ComplexityFacts);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<ComplexityFacts>)], type.GetInterfaces());
        Assert.Empty(type.GetNestedTypes(DeclaredOnly));
        Assert.Empty(type.GetEvents(DeclaredOnly));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));

        var parts = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Average", "Max", "Methods"], parts.Select(part => part.Name));
        Assert.Equal([typeof(double?), typeof(int?), typeof(int?)], parts.Select(part => part.PropertyType));

        Assert.All(parts, part =>
        {
            Assert.True(part.CanRead);
            Assert.True(part.CanWrite);
            Assert.Empty(part.GetIndexParameters());
            Assert.Equal(type, part.GetMethod!.DeclaringType);
            Assert.Equal(type, part.SetMethod!.DeclaringType);
            Assert.True(part.GetMethod.IsPublic);
            Assert.False(part.GetMethod.IsVirtual);
            Assert.True(part.SetMethod.IsPublic);
            Assert.False(part.SetMethod.IsVirtual);

            // The parts are said once - at the making or at the copy - and never turned after: init, not set.
            Assert.Contains(typeof(IsExternalInit), part.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        });

        Assert.Equal(
            ["Average", "Deconstruct", "Equals", "GetHashCode", "Max", "Methods", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        // The two operators are the only public statics the record has, and the equality is asked two ways.
        Assert.Equal(
            ["op_Equality", "op_Inequality"],
            type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));

        // The contract a record compares by and the writing behind its words are the compiler's private ones.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Null(type.GetMethod("PrintMembers", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(TheWriting);
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);
        Assert.DoesNotContain("PrintMembers", PublicSurface(), StringComparer.Ordinal);

        // And the words the record says of itself are the frame the compiler writes around what the record writes:
        // the private writing asked on its own hands over the three names and the numbers they stand on, in order.
        var page = new StringBuilder();
        object?[] onto = [page];

        Assert.True((bool)TheWriting.Invoke(Measured, onto)!);
        Assert.Equal($"ComplexityFacts {{ {page} }}", Said(Measured));

        // The way in is one: the three parts, none of them left out and none of them defaulted.
        var made = Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var arguments = made.GetParameters();

        Assert.Equal(["Average", "Max", "Methods"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(double?), typeof(int?), typeof(int?)], arguments.Select(argument => argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));
        Assert.Null(type.GetConstructor(Type.EmptyTypes));

        // The hand-over a copy is made by is private, and the falling apart is the one method the record declares.
        var handed = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, handed.GetParameters().Single().ParameterType);

        var apart = type.GetMethod("Deconstruct", BindingFlags.Public | BindingFlags.Instance)!;

        Assert.Equal(typeof(void), apart.ReturnType);
        Assert.Equal(["Average", "Max", "Methods"], apart.GetParameters().Select(parameter => parameter.Name));
        Assert.All(apart.GetParameters(), parameter => Assert.True(parameter.IsOut));
        Assert.Equal(
            [typeof(double?), typeof(int?), typeof(int?)],
            apart.GetParameters().Select(parameter => parameter.ParameterType.GetElementType()!));

        // And nothing of a moment, an address or another record of the build anywhere in the public face: the record
        // is three numbers and the object every record is asked about.
        Assert.Equal(
            ["Double", "Int32", "Object"],
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters())
                .Select(parameter => parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType)
                .Select(asked => Nullable.GetUnderlyingType(asked) ?? asked)
                .Where(asked => asked != type)
                .Select(asked => asked.Name)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal));
    }

    /// <summary>The reader of a build file's section is no public way in: the record's <c>Read</c> is the app's own -
    /// internal, static, told the section and handing back the facts or nothing at all - so a caller outside the app
    /// reaches these facts only through <see cref="BuildInfo.Parse" />, which is how every other file of the three
    /// reads them. The constructor is the public way, and the record it is handed the file's three numbers is the
    /// record the file stands for.</summary>
    [Fact]
    public void TheReaderOfABuildFileIsNoPublicWayIn()
    {
        var type = typeof(ComplexityFacts);

        Assert.Null(type.GetMethod("Read", BindingFlags.Public | BindingFlags.Static));

        var read = type.GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Static)!;

        Assert.True(read.IsStatic);
        Assert.True(read.IsAssembly);
        Assert.False(read.IsPublic);
        Assert.Equal(typeof(ComplexityFacts), read.ReturnType);

        var section = read.GetParameters().Single();

        Assert.Equal("section", section.Name);
        Assert.Equal(typeof(JsonElement?), section.ParameterType);

        // The public way in, taken with the very numbers the file the dashboard serves writes, is that file's record.
        object?[] given = [1.9, 34, 5210];
        var made = Assert.IsType<ComplexityFacts>(
            type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single().Invoke(given));

        Assert.Equal(ComplexityOf("""{ "average": 1.9, "max": 34, "methods": 5210 }"""), made);
        Assert.Equal(Measured, made);
        Assert.NotSame(Measured, made);
        Assert.Equal(Said(Measured), Said(made));
    }

    // ----- the two operators, asked as the members they are

    /// <summary>What the two operators answer when the nothing stands on either side of them, or on both: the
    /// <c>==</c> of nothing and nothing is a yes and the <c>!=</c> of them a no, the nothing beside a record is never
    /// the record however the question is turned, and the record beside itself is one - the very same reference, a
    /// second record of the same three numbers, or the record that says nothing of its three parts beside another
    /// that says nothing. Every one of these is answered through the public static method a call compiles to, which
    /// is the member a caller never writes by name and which stands or falls with the two marks.</summary>
    [Fact]
    public void TheTwoOperatorsAnswerForTheNothingOnEitherSide()
    {
        Assert.True(Answered(Equality, null, null));
        Assert.False(Answered(Inequality, null, null));

        Assert.False(Answered(Equality, null, Measured));
        Assert.False(Answered(Equality, Measured, null));
        Assert.True(Answered(Inequality, null, Measured));
        Assert.True(Answered(Inequality, Measured, null));

        Assert.True(Answered(Equality, Measured, Measured));
        Assert.True(Answered(Equality, Measured, new ComplexityFacts(1.9, 34, 5210)));
        Assert.True(Answered(Equality, Unmeasured, new ComplexityFacts(null, null, null)));
        Assert.False(Answered(Equality, Unmeasured, null));
        Assert.True(Answered(Inequality, Unmeasured, null));

        // The same asks written the way a caller writes them answer the same, the nothing on the left included.
        ComplexityFacts? nothing = null;

        Assert.True(nothing == null);
        Assert.False(nothing != null);
        Assert.True(Measured == Measured with { });
        Assert.True(Measured != Measured with { Methods = 5211 });
    }

    // ----- the parts coming back out

    /// <summary>The parts coming back out with one said between two unsaid - the average left unsaid and the worst
    /// said, the worst left unsaid and the average and the count said - and at the edges the numbers run to: the
    /// zero with a sign of its own keeping its sign through the three out parameters, the not-a-number staying no
    /// number and the least of the counts staying the least. Taken through the method itself, the same three
    /// parameters are the ones that come back: a part nobody said comes back as the nothing it is rather than as
    /// the zero that stood in the array before the ask, and the numbers a record stands on come back as themselves.
    /// A record read out of a build file falls apart no differently.</summary>
    [Fact]
    public void TheFactsFallApartWithAPartSaidBetweenTwoUnsaid()
    {
        var (average, max, methods) = new ComplexityFacts(null, 34, null);

        Assert.Null(average);
        Assert.Equal(34, max);
        Assert.Null(methods);

        var (said, unsaid, counted) = new ComplexityFacts(1.9, null, 5210);

        Assert.Equal(1.9, said);
        Assert.Null(unsaid);
        Assert.Equal(5210, counted);

        var (signed, least, unsaidAgain) = new ComplexityFacts(-0.0, int.MinValue, null);

        Assert.True(signed is { } theZero && double.IsNegative(theZero), "the negative zero lost its sign on the way out");
        Assert.Equal(int.MinValue, least);
        Assert.Null(unsaidAgain);

        var (noNumber, worst, _) = new ComplexityFacts(double.NaN, 34, null);

        Assert.True(noNumber is { } theNumber && double.IsNaN(theNumber), "the not-a-number came back as a number");
        Assert.Equal(34, worst);

        // The same three asks on the method itself: out parameters filled from the record, in the order it names them.
        var apart = typeof(ComplexityFacts).GetMethod("Deconstruct", BindingFlags.Public | BindingFlags.Instance)!;
        object?[] three = [0.0, 0, 0];

        apart.Invoke(Measured, three);

        Assert.Equal(1.9, Assert.IsType<double>(three[0]));
        Assert.Equal(34, Assert.IsType<int>(three[1]));
        Assert.Equal(5210, Assert.IsType<int>(three[2]));

        apart.Invoke(Unmeasured, three);

        Assert.All(three, part => Assert.Null(part));

        var (fromTheFile, worstFromTheFile, countedFromTheFile) = ComplexityOf("""{ "max": 2147483647 }""")!;

        Assert.Null(fromTheFile);
        Assert.Equal(int.MaxValue, worstFromTheFile);
        Assert.Null(countedFromTheFile);
    }

    // ----- the copy, at every edge the parts run to

    /// <summary>The copy the compiler writes, taken at every edge the three parts run to through the <c>with</c> a
    /// caller writes and through the public method that <c>with</c> walks: either way the copy is the facts again -
    /// the same words, the same hash, the same record - and never the same reference, the endless and the
    /// not-a-number, the least of the counts and the largest of them, the zero with a sign and the record that says
    /// nothing of its three parts all included. A copy of the nothing that stands for no measurement is that nothing
    /// again, and still a record of it.</summary>
    [Fact]
    public void TheCopyWalksEveryEdgeThePartsRunTo()
    {
        foreach (var edge in Edges)
        {
            var copy = edge with { };

            Assert.IsType<ComplexityFacts>(copy);
            Assert.NotSame(edge, copy);
            Assert.Equal(edge, copy);
            Assert.Equal(Said(edge), Said(copy));
            Assert.Equal(edge.GetHashCode(), copy.GetHashCode());

            var cloned = Assert.IsType<ComplexityFacts>(TheCopy.Invoke(edge, Array.Empty<object?>()));

            Assert.NotSame(edge, cloned);
            Assert.Equal(copy, cloned);
            Assert.Equal(Said(cloned), Said(edge));
        }

        // The method the copy walks is the compiler's public one, of no argument and of the record's own return.
        Assert.True(TheCopy.IsPublic);
        Assert.False(TheCopy.IsStatic);
        Assert.False(TheCopy.IsVirtual);
        Assert.Equal(typeof(ComplexityFacts), TheCopy.ReturnType);
        Assert.Empty(TheCopy.GetParameters());
    }

    // ----- compared, and never ranked

    /// <summary>The last thing the public face promises and no other file reads: these facts are compared for being
    /// one measurement and are never ordered for being the one before another. The record declares no
    /// <c>CompareTo</c>, carries neither <see cref="IComparable" /> nor <see cref="IComparable{T}" /> beside the
    /// equality every record carries, and so refuses the ranking: a set of them counts the measurements it is given
    /// and holds one entry for one measurement, while a sort of them is refused where the comparison is asked - the
    /// average of a build being no more "before" another build's than the worst of its methods is. What the card does
    /// with the numbers is the card's, and the record keeps the three of them as they were measured.</summary>
    [Fact]
    public void TheFactsAreComparedForOneAndNeverForBefore()
    {
        var type = typeof(ComplexityFacts);

        Assert.Null(type.GetMethod("CompareTo", DeclaredOnly));
        Assert.False(typeof(IComparable).IsAssignableFrom(type));
        Assert.False(typeof(IComparable<ComplexityFacts>).IsAssignableFrom(type));

        Assert.Equal(2, new HashSet<ComplexityFacts> { Measured, Unmeasured, new(1.9, 34, 5210) }.Count);
        Assert.Contains(Measured, new HashSet<ComplexityFacts>(Edges));
        Assert.DoesNotContain(new ComplexityFacts(1.9, 34, 5211), Edges);

        var refused = Assert.Throws<InvalidOperationException>(() => new[] { Measured, Unmeasured }.OrderBy(facts => facts).ToList());

        Assert.Contains("Failed to compare", refused.Message, StringComparison.Ordinal);
        Assert.IsType<ArgumentException>(refused.InnerException);
        Assert.Contains("IComparable", refused.InnerException!.Message, StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() => Comparer<ComplexityFacts>.Default.Compare(Measured, Unmeasured));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the record declares itself: its three parts and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The two
    /// constructors are no methods, so they are named by the tests above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ComplexityFacts)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>One of the two operators, as the public static method the compiler put it in.</summary>
    private static MethodInfo Operator(string name) =>
        typeof(ComplexityFacts).GetMethod(name, BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>An operator asked in the open, with whatever stands on either side of it - a record, another record
    /// of the same three numbers, or the nothing that is no record at all.</summary>
    private static bool Answered(MethodInfo ask, ComplexityFacts? first, ComplexityFacts? second)
    {
        ArgumentNullException.ThrowIfNull(ask);

        object?[] both = [first, second];

        return (bool)ask.Invoke(null, both)!;
    }

    /// <summary>The words a record says its three parts in, taken in the invariant culture whatever the reader of
    /// this test happens to stand in: the record's own words follow the reader, which
    /// <see cref="ComplexityFactsBoundaryEdgeCaseTests" /> pins, so the strings here are the point and the culture
    /// only the way to say them once.</summary>
    private static string Said(ComplexityFacts complexity)
    {
        ArgumentNullException.ThrowIfNull(complexity);

        return CultureScope.Words(complexity);
    }

    /// <summary>The complexity one answer reports, as the dashboard reads it: the version stands so that the answer
    /// is a build whatever its section says, and the section under the question is the text the test brings.</summary>
    private static ComplexityFacts? ComplexityOf(string complexity) =>
        BuildInfo.Parse($$"""{ "version": "2.4.15", "complexity": {{complexity}} }""")!.Complexity;
}
