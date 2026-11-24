using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeDatabaseChange" /> pinned as the carrier it is: the one value handed over when the runtime view
/// asks for an environment's database, or for its diagram again - the environment's name, the one bit of whether the
/// database view is to be shown, and the one bit of whether the change is a step of the browser's history. The record
/// works out nothing and holds nothing else: no address, no file, no word of its own. Every part is the very part it
/// was handed - the name in its own letters, neither trimmed nor re-cased nor shortened, the nothing of a name kept as
/// the nothing - and the two bits hold whichever of their two answers they were given, together in all four answers.
/// <para>
/// What is pinned where it belongs, and is not repeated here: the address a change comes to, and the hash the browser
/// is left at, are <see cref="ViewAddressTests.TheDatabaseViewWritesTheHashThatOpensIt" />; the page that raises a
/// change - <c>RuntimeView.razor</c> - and the page that turns one into an address - <c>App.razor</c> - are pinned by
/// none of the tests below, since nothing here touches <c>DatabaseChanged</c>. What is pinned here is the value, once
/// it is made.
/// </para>
/// <para>
/// Nothing here reaches the network, reads a file or asks a clock what time it is: a change holds a word and two bits
/// and calls nothing at all.
/// </para>
/// </summary>
public class RuntimeDatabaseChangeEdgeCaseTests
{
    /// <summary>
    /// The change a test asks about: the environment it names, the database view shown and the change a step of the
    /// history unless the test says otherwise. The one fixture of this file, so that every test says which part it is
    /// about and re-types nothing; the environment is declared a word that is not to be absent, exactly as the record
    /// declares it.
    /// </summary>
    private static RuntimeDatabaseChange Change(string environment, bool shown = true, bool step = true) =>
        new(environment, Shown: shown, Step: step);

    // ------------------------------ the three arguments, and the property each one of them lands on

    /// <summary>
    /// The three arguments land on the property of their name, in the order the record declares them: written
    /// positionally - the word first, the two bits behind it - the word is the environment's, the first bit is the
    /// shown bit and the second the step bit, so <c>("uat", false, true)</c> is the environment <c>uat</c> with the
    /// view hidden and the change a step. Two bits of the same type in the one constructor are the one place a
    /// swapped pair could hide, and this is where it does not: a record that took the second argument for its step
    /// bit would fail here. Written by name and in the other order, the same three arguments land in the same three
    /// places, and the two changes are the one value.
    /// </summary>
    [Fact]
    public void TheThreeArgumentsLandOnThePropertyOfTheirNameInTheOrderTheRecordDeclaresThem()
    {
        var positional = new RuntimeDatabaseChange("uat", false, true);

        Assert.Equal("uat", positional.Environment);
        Assert.False(positional.Shown);
        Assert.True(positional.Step);

        var named = new RuntimeDatabaseChange(Step: true, Environment: "uat", Shown: false);

        Assert.Equal("uat", named.Environment);
        Assert.False(named.Shown);
        Assert.True(named.Step);

        Assert.Equal(positional, named);
        Assert.Equal(positional.GetHashCode(), named.GetHashCode());
    }

    // ------------------------------------------- the two bits, and the four answers they give together

    /// <summary>
    /// Every answer of the two bits, held as it was given: the shown bit and the step bit each keeping their answer
    /// beside the same environment, and the four answers they give together being four values and not one. With the
    /// environment the same in all four it is the two bits alone that tell them apart, so each answer is distinct from
    /// the other three - a different value (<see cref="Assert.NotEqual{T}" /> and <c>!=</c>) and a different hash -
    /// and each falls apart into its three parts, in the order they went in, remade into the value it came from.
    /// </summary>
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void EveryCombinationOfTheTwoBitsIsHeldDistinctAndFallsApartAsItWentIn(bool shown, bool step)
    {
        var change = Change("uat", shown, step);

        // Held as given: the two bits keeping their answers, beside the name and in the order the record declares them.
        Assert.Equal(("uat", shown, step), (change.Environment, change.Shown, change.Step));

        // Distinct from the other three answers, which differ from it in a bit alone.
        var others = new[]
        {
            Change("uat", shown: true, step: true),
            Change("uat", shown: true, step: false),
            Change("uat", shown: false, step: true),
            Change("uat", shown: false, step: false),
        }.Where(other => other != change).ToList();

        Assert.Equal(3, others.Count);
        Assert.All(others, other =>
        {
            Assert.True(change != other);
            Assert.NotEqual(change, other);
            Assert.NotEqual(change.GetHashCode(), other.GetHashCode());
        });

        // And fallen apart: the three parts out in the order they went in, remade into the value they came from.
        change.Deconstruct(out var environment, out var heldShown, out var heldStep);

        Assert.Equal("uat", environment);
        Assert.Equal(shown, heldShown);
        Assert.Equal(step, heldStep);
        Assert.Equal(change, Change(environment, heldShown, heldStep));
    }

    // ---------------------------------------------------------------------- the environment's name, held whole

    /// <summary>
    /// The names an environment's word is put to here, every one of them written out at run time rather than written
    /// in the source: two callers writing <c>"uat"</c> in their source hold one and the same interned string, so only
    /// a word made of its own can tell the holding of a word from a copying of it.
    /// </summary>
    public static TheoryData<string> EnvironmentWords() => new()
    {
        { string.Empty },
        { new string(' ', 2) },
        { new string('u', 1_000) },
        { string.Concat("uat", "\U0001F600") },
    };

    /// <summary>
    /// The environment's name is the very string it was handed, and not a copy of it nor a re-written one: the empty
    /// word kept as the empty word and not as the nothing, a word of nothing but spaces kept with its spaces and not
    /// trimmed, a word of a thousand letters kept whole and not shortened either end, and a name carrying a surrogate
    /// pair kept as the one name its caller wrote - the same string (<see cref="Assert.Same" />), the same letters
    /// (<see cref="Assert.Equal(string?, string?)" />), the same length. The two bits beside it hold the answers they
    /// were given. The record asks nothing of a name: no length, no letters, no trimming - what a name means belongs
    /// to the page that reads it.
    /// </summary>
    [Theory]
    [MemberData(nameof(EnvironmentWords))]
    public void TheEnvironmentWordIsTheVeryStringItWasGiven(string environment)
    {
        var change = Change(environment);

        Assert.Same(environment, change.Environment);
        Assert.Equal(environment, change.Environment);
        Assert.Equal(environment.Length, change.Environment.Length);
        Assert.True(change.Shown);
        Assert.True(change.Step);
    }

    /// <summary>
    /// The environment that is not there at all, kept as the nothing: the record declares its environment a word that
    /// is not to be absent and guards nothing - it throws nothing, trims nothing and falls back to no name of its own
    /// - so a change handed the nothing is a change still, holding the nothing where its name would be, and it stands
    /// apart from the change whose name is the empty word (<c>!=</c> and <see cref="Assert.NotEqual{T}" />), and the
    /// one value, every way a change is compared, with every other change whose name is the nothing. What the nothing
    /// means belongs to the address, which is <see cref="ViewAddressTests" /> and not this file: this test says only
    /// that the record keeps the nothing it was given.
    /// </summary>
    [Fact]
    public void ANullEnvironmentIsKeptAsTheNothingItWasGiven()
    {
        var change = Change(null!);

        Assert.Null(change.Environment);
        Assert.True(change.Shown);
        Assert.True(change.Step);

        change.Deconstruct(out var environment, out var shown, out var step);

        Assert.Null(environment);
        Assert.True(shown);
        Assert.True(step);

        Assert.True(change != Change(string.Empty));
        Assert.NotEqual(change, Change(string.Empty));

        Assert.True(change == Change(null!));
        Assert.False(change != Change(null!));
        Assert.Equal(change, Change(null!));
        Assert.Equal(change.GetHashCode(), Change(null!).GetHashCode());
    }

    // -------------------------------------- the value the record is: equality, hash, and the words it prints

    /// <summary>
    /// Two changes holding the same three parts are the one value: the typed <c>Equals</c>, the boxed
    /// <c>Equals(object)</c> and <c>==</c> each agree, <c>!=</c> refuses, and the two share the one hash the value
    /// carries - so two changes of the same parts are the same key of a table.
    /// </summary>
    [Fact]
    public void TwoChangesWithTheSameThreePartsAreEqualEveryWayAndShareOneHash()
    {
        var one = Change("uat");
        var two = Change("uat");

        Assert.True(one.Equals(two));
        Assert.True(two.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
    }

    /// <summary>
    /// The environment compared in its letters, ordinal, with no case folded: <c>uat</c> and <c>UAT</c> are two names,
    /// so the very same two bits beside them make two changes - <c>!=</c> accepting and <c>==</c> refusing both ways
    /// round, and <see cref="Assert.NotEqual{T}" /> saying so too - and neither name is re-cased on the way in. The
    /// name takes its part in the hash as it takes it in the equality, so the two changes are no one key of a table.
    /// </summary>
    [Fact]
    public void TheEnvironmentIsComparedOrdinalSoItsCaseIsTwoDifferentNames()
    {
        var lower = Change("uat");
        var upper = Change("UAT");

        Assert.True(lower != upper);
        Assert.False(lower == upper);
        Assert.NotEqual(lower, upper);
        Assert.True(upper != lower);
        Assert.False(upper == lower);
        Assert.NotEqual(upper, lower);
        Assert.Equal("UAT", upper.Environment);
        Assert.NotEqual(lower.GetHashCode(), upper.GetHashCode());
    }

    /// <summary>
    /// A change compared with what is no change: the nothing, and values of other types. A change is equal to no value
    /// that is not its own three parts in its own places, however like one of them that value is.
    /// </summary>
    [Fact]
    public void EqualsOfNullAndOfAnotherTypeIsFalse()
    {
        var change = Change("uat");

        Assert.False(change.Equals(null));
        Assert.False(change.Equals((object)null!));
        Assert.False(change.Equals((object)"uat"));
        Assert.False(change.Equals((object)true));
        Assert.False(change.Equals((object)7));
    }

    /// <summary>
    /// The two operators met by the nothing: a change is no nothing, so <c>==</c> refuses the nothing written either
    /// side of it and <c>!=</c> accepts it - and a change held in a variable that is allowed to be the nothing, and
    /// standing empty, is met by the nothing with the one answer, both operators agreeing with
    /// <see cref="object.Equals(object?)" />. The lifted operators of a record ask nothing of their operands and throw
    /// nothing, however they are written.
    /// </summary>
    [Fact]
    public void TheTwoOperatorsAnswerForTheNothingAsTheyAnswerForAChange()
    {
        RuntimeDatabaseChange? change = Change("uat");

        Assert.False(change == null);
        Assert.False(null == change);
        Assert.True(change != null);
        Assert.True(null != change);

        RuntimeDatabaseChange? nothing = null;

        Assert.False(change == nothing);
        Assert.True(change != nothing);
        Assert.True(change == Change("uat"));
        Assert.False(change != Change("uat"));
    }

    /// <summary>
    /// The <c>with</c> expression: it copies the three parts and re-holds the one it is told to, each beside the other
    /// two unchanged - a <c>with</c> that hides the view keeps the name and the step, one that drops the step keeps the
    /// name and the view shown, one that changes the name keeps the two bits - and the original change stands untouched
    /// beside its copies, the value it was made as.
    /// </summary>
    [Fact]
    public void WithCopiesTheOtherTwoPartsAndLeavesTheOriginalUntouched()
    {
        var original = Change("uat");

        var hidden = original with { Shown = false };
        Assert.Equal("uat", hidden.Environment);
        Assert.False(hidden.Shown);
        Assert.True(hidden.Step);

        var inPlace = original with { Step = false };
        Assert.Equal("uat", inPlace.Environment);
        Assert.True(inPlace.Shown);
        Assert.False(inPlace.Step);

        var prod = original with { Environment = "prod" };
        Assert.Equal("prod", prod.Environment);
        Assert.True(prod.Shown);
        Assert.True(prod.Step);

        Assert.Equal("uat", original.Environment);
        Assert.True(original.Shown);
        Assert.True(original.Step);
        Assert.Equal(Change("uat"), original);
    }

    /// <summary>
    /// The change, printed: the record's name, then its three parts in the order the record declares them, each under
    /// the name it is declared under, and the two bits as <c>True</c> and <c>False</c>.
    /// </summary>
    [Fact]
    public void ToStringPrintsTheRecordNameAndTheThreePartsInTheOrderTheyWentIn()
    {
        Assert.Equal(
            "RuntimeDatabaseChange { Environment = uat, Shown = True, Step = False }",
            Change("uat", step: false).ToString());
    }

    /// <summary>
    /// The change with no name, printed: the nothing prints as nothing at all, so the words of a change held without a
    /// name and of one held with the empty word are the selfsame words - the one place of this file where the two are
    /// not told apart, and the reason <c>ToString</c> is what a change is said by and not what it is compared by.
    /// Both print their two bits, which are never the nothing.
    /// </summary>
    [Fact]
    public void ToStringPrintsTheNothingNameAndTheEmptyWordAsTheSameNothing()
    {
        Assert.Equal(
            "RuntimeDatabaseChange { Environment = , Shown = True, Step = True }",
            Change(null!).ToString());
        Assert.Equal(
            "RuntimeDatabaseChange { Environment = , Shown = True, Step = True }",
            Change(string.Empty).ToString());
    }

    // -------------------------------------------------------------------- the record's own shape

    /// <summary>
    /// The record as a shape, and not as a value: a sealed class and not a struct, standing on
    /// <see cref="object" /> and answering no interface but the typed equality of its own kind,
    /// <see cref="IEquatable{RuntimeDatabaseChange}" />, which is what lets two changes of the same three parts
    /// compare equal and stand as one key of a table. Beyond its three parts it declares only what every record is
    /// asked - falling apart, comparing, hashing, saying itself, and the two operators - and nothing else. A fourth
    /// part, or another interface, or a method of its own that works something out, has to be written here as well;
    /// that is the point: a carrier gains a member only when somebody decided what it means.
    /// </summary>
    [Fact]
    public void TheChangeIsASealedRecordDeclaringOnlyItsThreePartsAndWhatEveryRecordIsAsked()
    {
        var type = typeof(RuntimeDatabaseChange);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<RuntimeDatabaseChange>)], [.. type.GetInterfaces()]);

        Assert.Equal(
            ["Deconstruct", "Environment", "Equals", "GetHashCode", "Shown", "Step", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface(type));
    }

    /// <summary>
    /// The three parts, named exactly and typed exactly in the order the record declares them - the environment a
    /// word, the shown bit and the step bit words of neither - and none of them declared nullable, the record saying
    /// of its name that it is there (and, as the test above pins, guarding nothing against a caller that hands it the
    /// nothing anyway). Each part is read, and written once, at the making and not after: the write of each carries
    /// the marker the compiler asks a <c>with</c> to stand on and no assignment does, so nobody holding a change may
    /// turn one of its parts after it was made.
    /// </summary>
    [Fact]
    public void TheThreePartsAreNamedTypedAndInitOnlyAsTheRecordDeclaresThem()
    {
        var properties = typeof(RuntimeDatabaseChange)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(["Environment", "Shown", "Step"], [.. properties.Keys]);

        Assert.Equal(typeof(string), properties["Environment"].PropertyType);
        Assert.Equal(typeof(bool), properties["Shown"].PropertyType);
        Assert.Equal(typeof(bool), properties["Step"].PropertyType);
        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.CanWrite));

        // None of the three is declared nullable.
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false],
            [.. properties.Values.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        // And the write of each is an init: the accessor is there, and it carries the marker the compiler asks a
        // `with` to stand on - so no one holding the change can turn a part after the record was made.
        Assert.All(
            properties.Values,
            property => Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers()));
    }

    /// <summary>
    /// The one constructor the record declares, taking its three parts in the order it declares them - the
    /// environment, the shown bit, the step bit - each of them required, none defaulted, and none of them declared
    /// nullable. Nothing is asked of a caller and nothing is given for one: whatever the three arguments are, that is
    /// the change.
    /// </summary>
    [Fact]
    public void TheOneConstructorTakesTheThreePartsInOrderAndDefaultsNone()
    {
        var constructors = typeof(RuntimeDatabaseChange).GetConstructors();

        Assert.Single(constructors);
        Assert.True(constructors[0].IsPublic);

        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Environment", "Shown", "Step"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(string), typeof(bool), typeof(bool)], [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.All(parameters, parameter => Assert.False(parameter.IsOptional));
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));

        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);
    }

    // ---------------------------------------------------------------------------------------- the machinery

    /// <summary>The public members a record declares itself: its parts, the methods that are not the get and the set of
    /// one, and the two operators kept although the compiler names them specially - since they are the <c>==</c> and
    /// the <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The constructor
    /// is no method, so the test above names it.</summary>
    private static List<string> PublicSurface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
