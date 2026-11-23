using System.Reflection;
using System.Runtime.CompilerServices;

using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// The edge cases of <see cref="RuntimeApplicationChange" /> - the record the runtime view raises when the
/// browser zooms into the box of a deployable, or back out to the diagram again: the environment the address
/// takes the view in, the deployable whose application view is asked for, and the one bit of where the change
/// stands in the browser's history. The record is a carrier of its three members and nothing else: it is
/// sealed, it has one constructor, and it works out nothing - every word and the flag it holds are the very
/// words and the very flag the page was given, and a member that is not there stays not there.
/// <para>
/// What is pinned where it belongs, and is not repeated here: the zoom into a box, the back out to the diagram
/// and the address corrected in place are pinned by the page that makes them - <c>RuntimeView.razor</c> asks
/// for the application view of a deployable, or for its diagram again, and <c>App.razor</c> turns each change
/// into the address it is. What this file pins is the record as it stands once it is made: the three members
/// handed to the property of their name and held as they were held out - the very words, in their own letters,
/// with nothing trimmed, no case changed and nothing shortened, the empty word kept as the empty word, a word
/// of nothing but whitespace kept as the whitespace, a word of a thousand letters kept whole, and the nothing
/// of a word kept as the nothing; the flag holding either of its two answers; and the record fallen apart into
/// its three members in the order they went in.
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a change holds two words and a
/// flag, no moment and no address, and calls nothing at all.
/// </para>
/// </summary>
public class RuntimeApplicationChangeEdgeCaseTests
{
    // ---------------------------------- the three members, as they are handed over and as they are held

    /// <summary>
    /// The constructor hands each of its three arguments to the property of its name, in the order the record
    /// declares them - the environment, the deployable, the flag - and holds the very words it was handed: the
    /// words are the strings the caller made, not copies of them, and the flag is the flag the caller made.
    /// </summary>
    [Fact]
    public void TheThreeMembersComeBackExactlyAsGiven()
    {
        var environment = "prod";
        var deployable = "ui";
        var change = new RuntimeApplicationChange(environment, deployable, Step: true);

        Assert.Same(environment, change.Environment);
        Assert.Same(deployable, change.Deployable);
        Assert.True(change.Step);
        Assert.Equal(("prod", "ui", true), (change.Environment, change.Deployable, change.Step));

        // Named in the other order, in the same three places, and the flag holding the other of its two answers.
        var notStep = new RuntimeApplicationChange(Environment: "prod", Deployable: "ui", Step: false);

        Assert.Same("prod", notStep.Environment);
        Assert.Same("ui", notStep.Deployable);
        Assert.False(notStep.Step);
        Assert.Equal(("prod", "ui", false), (notStep.Environment, notStep.Deployable, notStep.Step));
    }

    /// <summary>
    /// The empty word in the environment is kept as the empty word, and not as the nothing and not as a word of
    /// space: the record trims nothing and re-writes nothing, so the zero letters the caller made are the very
    /// letters the record holds.
    /// </summary>
    [Fact]
    public void AnEmptyEnvironmentIsKeptExactlyAsGiven()
    {
        var change = new RuntimeApplicationChange(string.Empty, "ui", Step: true);

        Assert.Same(string.Empty, change.Environment);
        Assert.Equal(string.Empty, change.Environment);
        Assert.Equal(0, change.Environment.Length);
        Assert.Same("ui", change.Deployable);
        Assert.True(change.Step);
        Assert.NotEqual(change, new RuntimeApplicationChange(null!, "ui", Step: true));
    }

    /// <summary>
    /// A word of nothing but whitespace in the environment is kept as the whitespace: not trimmed to the empty
    /// word, not re-cased and not shortened - the letters the caller made, in the places they were made.
    /// </summary>
    [Fact]
    public void AWhitespaceEnvironmentIsKeptExactlyAsGiven()
    {
        var spaces = "  ";
        var change = new RuntimeApplicationChange(spaces, "ui", Step: true);

        Assert.Same(spaces, change.Environment);
        Assert.Equal("  ", change.Environment);
        Assert.Equal(2, change.Environment.Length);
        Assert.NotEqual(string.Empty, change.Environment);
        Assert.NotEqual("prod", change.Environment);
    }

    /// <summary>
    /// The environment that is not there at all is kept as the nothing, and not as the empty word: the record
    /// declares the environment a word that is not to be absent, but asks nothing of its caller, so a change
    /// with no environment is a change still, and stands apart from every change whose environment is a word.
    /// </summary>
    [Fact]
    public void ANullEnvironmentIsKeptAsNull()
    {
        var change = new RuntimeApplicationChange(null!, "ui", Step: true);

        Assert.Null(change.Environment);
        Assert.Same("ui", change.Deployable);
        Assert.True(change.Step);
        Assert.NotEqual(change, new RuntimeApplicationChange(string.Empty, "ui", Step: true));
        Assert.NotEqual(change, new RuntimeApplicationChange(" ", "ui", Step: true));
    }

    /// <summary>
    /// The deployable that is not there at all is kept as the nothing - the diagram, as the runtime view asks for
    /// it again - and not as the empty word: a change back to the diagram is a change still, and stands apart
    /// from the view of a deployable, however short that deployable's name is.
    /// </summary>
    [Fact]
    public void ANullDeployableIsKeptAsNullForTheDiagram()
    {
        var change = new RuntimeApplicationChange("prod", null, Step: true);

        Assert.Same("prod", change.Environment);
        Assert.Null(change.Deployable);
        Assert.True(change.Step);
        Assert.NotEqual(change, new RuntimeApplicationChange("prod", string.Empty, Step: true));
    }

    /// <summary>
    /// The empty word in the deployable is kept as the empty word, and not as the nothing: the view of a
    /// deployable whose name is the zero letters the caller made, and no change back to the diagram.
    /// </summary>
    [Fact]
    public void AnEmptyDeployableIsKeptExactlyAsGiven()
    {
        var change = new RuntimeApplicationChange("prod", string.Empty, Step: true);

        Assert.Same("prod", change.Environment);
        Assert.Same(string.Empty, change.Deployable);
        Assert.Equal(0, change.Deployable!.Length);
        Assert.True(change.Step);
        Assert.NotEqual(change, new RuntimeApplicationChange("prod", null, Step: true));
    }

    /// <summary>
    /// A word of a thousand letters in the deployable is held whole, and the flag holds either of its two
    /// answers beside it: nothing is cut off either end, and the thousand letters the caller made are the very
    /// letters the record holds.
    /// </summary>
    [Fact]
    public void AThousandCharacterDeployableIsKeptWholeForEitherStep()
    {
        var thousand = new string('x', 1_000);
        var step = new RuntimeApplicationChange("prod", thousand, Step: true);
        var notStep = new RuntimeApplicationChange("prod", thousand, Step: false);

        Assert.Same(thousand, step.Deployable);
        Assert.Equal(1_000, step.Deployable!.Length);
        Assert.True(step.Step);
        Assert.Same(thousand, notStep.Deployable);
        Assert.Equal(1_000, notStep.Deployable!.Length);
        Assert.False(notStep.Step);
        Assert.NotEqual(step, notStep);
    }

    /// <summary>
    /// The flag holds either of its two answers, and no third: the step the browser's Back undoes, and the
    /// change the browser's Back leaves standing, each beside the same two words, and the two answers standing
    /// apart.
    /// </summary>
    [Fact]
    public void StepTrueAndStepFalseAreKept()
    {
        var step = new RuntimeApplicationChange("prod", "ui", Step: true);
        var notStep = new RuntimeApplicationChange("prod", "ui", Step: false);

        Assert.True(step.Step);
        Assert.False(notStep.Step);
        Assert.Same("prod", step.Environment);
        Assert.Same("prod", notStep.Environment);
        Assert.Same("ui", step.Deployable);
        Assert.Same("ui", notStep.Deployable);
        Assert.NotEqual(step, notStep);
        Assert.True(step != notStep);
    }

    // -------------------------------------------- the record fallen apart into its three members

    /// <summary>
    /// The record falls apart into its three members, in the order it declares them - the environment, the
    /// deployable, the flag - and each under the name it is declared under: a member that is a word comes out as
    /// the word it was held as, and a member that is not there comes out as the nothing, which is how the
    /// diagram again is read apart from the view of a deployable whose name is a word.
    /// </summary>
    [Fact]
    public void DeconstructReturnsTheThreePartsInTheOrderTheyWentIn()
    {
        var environment = "prod";
        var deployable = "ui";
        var change = new RuntimeApplicationChange(environment, deployable, Step: true);

        change.Deconstruct(out var env, out var dep, out var step);

        Assert.Same(environment, env);
        Assert.Same(deployable, dep);
        Assert.True(step);
        Assert.Equal(change, new RuntimeApplicationChange(env, dep, step));

        // The diagram again: the nothing of a deployable comes out as the nothing, and the flag beside it.
        var diagram = new RuntimeApplicationChange("prod", null, Step: false);
        var (diagramEnvironment, diagramDeployable, diagramStep) = diagram;

        Assert.Same("prod", diagramEnvironment);
        Assert.Null(diagramDeployable);
        Assert.False(diagramStep);

        // The nothing of an environment comes out as the nothing, and not as the empty word.
        var noEnvironment = new RuntimeApplicationChange(null!, "ui", Step: true);
        var (noEnvironmentAgain, noDeployable, noStep) = noEnvironment;

        Assert.Null(noEnvironmentAgain);
        Assert.Same("ui", noDeployable);
        Assert.True(noStep);
    }

    // -------------------------------------- the value the record is: equality, hash, and the words it prints

    /// <summary>
    /// Two changes holding the same three parts are the same value: the typed Equals, the boxed
    /// Equals(object) and the == operator each agree, and the two changes share the one hash the
    /// value carries - equal values are equal every way they are compared, and carry the same hash.
    /// </summary>
    [Fact]
    public void TwoChangesWithTheSameThreePartsAreEqualInEveryWayAndShareAHash()
    {
        var one = new RuntimeApplicationChange("uat", "demo-ui", Step: true);
        var two = new RuntimeApplicationChange("uat", "demo-ui", Step: true);

        Assert.True(one.Equals(two));
        Assert.True(two.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
    }

    /// <summary>
    /// The environment compared in its letters, ordinal, with no case folded: the very same two words
    /// and flag with the environment's case changed - 'uat' against 'UAT' - are two changes, and the
    /// != operator and NotEqual each say so.
    /// </summary>
    [Fact]
    public void ACaseChangeInTheEnvironmentMakesTwoChangesUnequal()
    {
        var lower = new RuntimeApplicationChange("uat", "demo-ui", Step: true);
        var upper = new RuntimeApplicationChange("UAT", "demo-ui", Step: true);

        Assert.True(lower != upper);
        Assert.NotEqual(lower, upper);
        Assert.True(upper != lower);
        Assert.NotEqual(upper, lower);
    }

    /// <summary>
    /// The deployable compared in its letters, ordinal, with the nothing standing apart from the empty
    /// word: the very same environment and flag with the deployable the nothing against the zero letters
    /// are two changes, and the != operator and NotEqual each say so.
    /// </summary>
    [Fact]
    public void ANullDeployableAgainstAnEmptyDeployableMakesTwoChangesUnequal()
    {
        var none = new RuntimeApplicationChange("uat", null, Step: true);
        var empty = new RuntimeApplicationChange("uat", string.Empty, Step: true);

        Assert.True(none != empty);
        Assert.NotEqual(none, empty);
        Assert.True(empty != none);
        Assert.NotEqual(empty, none);
    }

    /// <summary>
    /// The flag compared in its answer: the very same two words with the flag holding true against the
    /// flag holding false are two changes, and the != operator and NotEqual each say so.
    /// </summary>
    [Fact]
    public void AChangedStepMakesTwoChangesUnequal()
    {
        var step = new RuntimeApplicationChange("uat", "demo-ui", Step: true);
        var notStep = new RuntimeApplicationChange("uat", "demo-ui", Step: false);

        Assert.True(step != notStep);
        Assert.NotEqual(step, notStep);
        Assert.True(notStep != step);
        Assert.NotEqual(notStep, step);
    }

    /// <summary>
    /// The boxed comparison of a change against something that is not a change at all: against the
    /// nothing, and against a word of another type, Equals is false - a change is equal to no value
    /// that is not the same three parts in the same places.
    /// </summary>
    [Fact]
    public void EqualsOfNullAndOfAnotherTypeIsFalse()
    {
        var change = new RuntimeApplicationChange("uat", "demo-ui", Step: true);

        Assert.False(change.Equals((object)null!));
        Assert.False(change.Equals((object)"uat"));
        Assert.False(change.Equals((object)"demo-ui"));
        Assert.False(change.Equals((object)1));
    }

    /// <summary>
    /// The with expression copies the three parts and re-holds the one it is told to, each beside the
    /// other two unchanged: a with that sets the deployable to the nothing keeps the environment and the
    /// flag, a with that sets the flag to false keeps the two words, and a with that sets the environment
    /// to 'prod' keeps the deployable and the flag - and the original change stands untouched beside its
    /// copies.
    /// </summary>
    [Fact]
    public void WithCopiesTheOtherPartsUnchangedAndLeavesTheOriginalUntouched()
    {
        var original = new RuntimeApplicationChange("uat", "demo-ui", Step: true);

        var noDeployable = original with { Deployable = null };
        Assert.Same("uat", noDeployable.Environment);
        Assert.Null(noDeployable.Deployable);
        Assert.True(noDeployable.Step);

        var notStep = original with { Step = false };
        Assert.Same("uat", notStep.Environment);
        Assert.Same("demo-ui", notStep.Deployable);
        Assert.False(notStep.Step);

        var prod = original with { Environment = "prod" };
        Assert.Same("prod", prod.Environment);
        Assert.Same("demo-ui", prod.Deployable);
        Assert.True(prod.Step);

        Assert.Same("uat", original.Environment);
        Assert.Same("demo-ui", original.Deployable);
        Assert.True(original.Step);
    }

    /// <summary>
    /// The change, printed: the record's name, then the three members in the order the record declares
    /// them, each under the name it is declared under - the environment as its letters, the deployable
    /// as its letters, and the flag as True or False; and the deployable that is the nothing printed as
    /// no letters at all.
    /// </summary>
    [Fact]
    public void ToStringPrintsTheRecordNameAndTheThreePartsInTheOrderTheyWentIn()
    {
        var change = new RuntimeApplicationChange("uat", "demo-ui", Step: true);

        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = demo-ui, Step = True }", change.ToString());

        var diagram = new RuntimeApplicationChange("uat", null, Step: false);

        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = , Step = False }", diagram.ToString());
    }

    // --------------------------------------------------- the record's own shape

    /// <summary>
    /// The record as a shape, and not as a value: it is a sealed class and not a struct - it stands on
    /// <see cref="object" /> and answers no interface but the typed equality of its own kind,
    /// <see cref="IEquatable{RuntimeApplicationChange}" />, which is what lets two changes of the same
    /// three parts compare equal and stand as one key of a table. Beyond its three parts, the record
    /// declares only the members every record is asked: falling apart, comparing, hashing, saying
    /// itself, and the two operators - and nothing else. A fourth member, or another interface, added
    /// to the record has to be written here as well, which is the point: the page cannot be shown a
    /// member nobody decided what it means.
    /// </summary>
    [Fact]
    public void TheChangeIsAStructlessSealedRecordAnsweringOnlyTheEqualityOfItsOwnKind()
    {
        Assert.True(typeof(RuntimeApplicationChange).IsSealed);
        Assert.False(typeof(RuntimeApplicationChange).IsValueType);
        Assert.Equal(typeof(object), typeof(RuntimeApplicationChange).BaseType);
        Assert.Equal([typeof(IEquatable<RuntimeApplicationChange>)], [.. typeof(RuntimeApplicationChange).GetInterfaces()]);

        Assert.Equal(
            ["Deconstruct", "Deployable", "Environment", "Equals", "GetHashCode", "Step", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());
    }

    /// <summary>
    /// The three parts, named exactly and typed exactly, in the order the record names them: the
    /// environment as a word of its own, the deployable as a word that may be the nothing - the
    /// nullability <see cref="NullabilityInfoContext" /> says of the part the record holds - and the flag
    /// as a word of neither. Each part is read, and written once, at the making, and not after: the write
    /// of each carries the marker the compiler asks a <c>with</c> to stand on and no assignment does, so
    /// no one holding the change may turn a part after the record was made.
    /// </summary>
    [Fact]
    public void TheThreePartsAreExactlyTheThreeNamedEachOfItsTypeAndInitOnly()
    {
        var properties = typeof(RuntimeApplicationChange)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(["Environment", "Deployable", "Step"], [.. properties.Keys]);

        Assert.Equal(typeof(string), properties["Environment"].PropertyType);
        Assert.Equal(typeof(string), properties["Deployable"].PropertyType);
        Assert.Equal(typeof(bool), properties["Step"].PropertyType);
        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.CanWrite));

        // The nullability the record declares, part by part: a word, a word that may be the nothing, and
        // a word of neither.
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, true, false],
            [.. properties.Values.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        // The write of each part is an init: the accessor is there, and it carries the marker the compiler
        // asks a `with` to stand on - so no one holding the change can turn it after the record was made.
        Assert.All(properties.Values, property =>
        {
            var init = property.GetSetMethod()!;

            Assert.Contains(typeof(IsExternalInit), init.ReturnParameter.GetRequiredCustomModifiers());
        });
    }

    /// <summary>
    /// The one constructor the record declares: the three parts, in the order the record names them - the
    /// environment, the deployable, the flag - each of them required, none of them said for anyone, and
    /// the deployable the only one of them that may be the nothing, which is how the diagram again is
    /// handed in.
    /// </summary>
    [Fact]
    public void TheOneConstructorTakesTheThreePartsInOrderAndNoneIsDefaulted()
    {
        var constructors = typeof(RuntimeApplicationChange).GetConstructors();

        Assert.Single(constructors);
        Assert.True(constructors[0].IsPublic);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Environment", "Deployable", "Step"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(string), typeof(string), typeof(bool)], [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.All(parameters, parameter => Assert.False(parameter.IsOptional));
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));

        // The nullability of the arguments, said once: a word, a word that may be the nothing, and a word
        // of neither.
        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, true, false],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);
    }

    // --------------------------------------------------- the machinery

    /// <summary>
    /// The public members the record declares itself: its properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since
    /// they are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone
    /// walked past. The constructor is no method, so it is named by the tests above rather than by this
    /// list.
    /// </summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeApplicationChange)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
