using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The contract the compiler writes for every positional record, said once instead of once per edge-case file:
/// <list type="number">
/// <item>the constructor hands each argument to the property of its name,</item>
/// <item>the parts come out of <c>Deconstruct</c> again in the order they went in,</item>
/// <item>a copy changes the part it names and nothing else,</item>
/// <item>two records are one record when every part is, with the hash that goes with it and as the key of a set and
/// a dictionary, and any one part alone tells two apart,</item>
/// <item>the record says what it is in one line of its own parts, in the declared order.</item>
/// </list>
/// A caller brings two rows of constructor arguments that differ in every part, so a part landing on the wrong
/// property, or a part left out of the equality, is seen. Then the per-type files keep only what the record itself
/// decides.
/// </summary>
internal static class RecordContract
{
    /// <summary>Holds <typeparamref name="T" /> to all five sentences of the contract, using <paramref name="first" /> and
    /// <paramref name="second" />: two rows of arguments for its one public constructor, in declared order, that differ
    /// in every position.</summary>
    public static void Holds<T>(object?[] first, object?[] second) where T : class
    {
        var parameters = RecordSurface.ParametersOf<T>();

        Assert.Equal(parameters.Length, first.Length);
        Assert.Equal(parameters.Length, second.Length);
        Assert.All(Enumerable.Range(0, parameters.Length), index =>
            Assert.False(Equals(first[index], second[index]), $"the two rows agree on {parameters[index].Name}"));

        PartsLandOnThePropertyOfTheirName<T>(first);
        PartsLandOnThePropertyOfTheirName<T>(second);
        PartsDeconstructInDeclaredOrder<T>(first);
        ACopyMovesOnlyThePartItNames<T>(first, second);
        EqualWhenEveryPartIs<T>(first, second);
        SaysItsPartsInOneLine<T>(first);
        SaysItsPartsInOneLine<T>(second);
    }

    /// <summary>A record made from <paramref name="arguments" /> through its one public constructor.</summary>
    public static T Make<T>(object?[] arguments) where T : class =>
        (T)typeof(T).GetConstructors().Single().Invoke(arguments);

    /// <summary>The constructor hands each argument to the property of its name.</summary>
    public static void PartsLandOnThePropertyOfTheirName<T>(object?[] arguments) where T : class
    {
        var record = Make<T>(arguments);
        var parameters = RecordSurface.ParametersOf<T>();

        for (var index = 0; index < parameters.Length; index++)
        {
            Assert.Equal(arguments[index], Part(record, parameters[index].Name!));
        }
    }

    /// <summary>The parts come out of <c>Deconstruct</c> again in the order they went in.</summary>
    public static void PartsDeconstructInDeclaredOrder<T>(object?[] arguments) where T : class
    {
        var record = Make<T>(arguments);
        var deconstruct = typeof(T).GetMethod("Deconstruct", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(deconstruct);

        var outs = new object?[arguments.Length];
        deconstruct.Invoke(record, outs);

        Assert.Equal(arguments, outs);
    }

    /// <summary>For each part, a copy that moves that part to its value in <paramref name="second" /> changes that part and
    /// no other, leaves the original as it was, and is the record made from <paramref name="first" /> with that one
    /// argument swapped.</summary>
    public static void ACopyMovesOnlyThePartItNames<T>(object?[] first, object?[] second) where T : class
    {
        var original = Make<T>(first);
        var parameters = RecordSurface.ParametersOf<T>();

        for (var index = 0; index < parameters.Length; index++)
        {
            var copy = With(original, parameters[index].Name!, second[index]);
            var expected = (object?[])first.Clone();
            expected[index] = second[index];

            Assert.Equal(Make<T>(expected), copy);
            Assert.Equal(Make<T>(first), original);
            Assert.NotSame(original, copy);

            for (var other = 0; other < parameters.Length; other++)
            {
                Assert.Equal(other == index ? second[other] : first[other], Part(copy, parameters[other].Name!));
            }
        }
    }

    /// <summary>Two records made from the same arguments are one record by every comparison, by their hash, as a set
    /// entry and as a dictionary key. A record that differs from them in any one part alone is another record.</summary>
    public static void EqualWhenEveryPartIs<T>(object?[] first, object?[] second) where T : class
    {
        var one = Make<T>(first);
        var same = Make<T>(first);
        var equality = typeof(T).GetMethod("op_Equality")!;
        var inequality = typeof(T).GetMethod("op_Inequality")!;

        Assert.NotSame(one, same);
        Assert.True(one.Equals(same));
        Assert.True((bool)equality.Invoke(null, [one, same])!);
        Assert.False((bool)inequality.Invoke(null, [one, same])!);
        Assert.Equal(one.GetHashCode(), same.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals(new object()));

        var set = new HashSet<T> { one, same };
        Assert.Single(set);

        var keyed = new Dictionary<T, string> { [one] = "one" };
        keyed[same] = "same";
        Assert.Equal("same", Assert.Single(keyed).Value);

        var parameters = RecordSurface.ParametersOf<T>();

        for (var index = 0; index < parameters.Length; index++)
        {
            var arguments = (object?[])first.Clone();
            arguments[index] = second[index];
            var other = Make<T>(arguments);

            Assert.False(one.Equals(other), $"{typeof(T).Name} does not tell {parameters[index].Name} apart");
            Assert.False((bool)equality.Invoke(null, [one, other])!);
            Assert.True((bool)inequality.Invoke(null, [one, other])!);
            Assert.DoesNotContain(other, set);
        }
    }

    /// <summary>The one line the record says: its type name, then each part in declared order as <c>Name = value</c>,
    /// with nothing after the equals sign where a part is nothing. After the parts come the public properties the record
    /// computes from them (<c>HasAny</c>, <c>IsStopped</c>), because the compiler prints every public property it
    /// declares.</summary>
    public static void SaysItsPartsInOneLine<T>(object?[] arguments) where T : class
    {
        var record = Make<T>(arguments);
        var parameters = RecordSurface.ParametersOf<T>();
        var properties = RecordSurface.PropertyNamesOf<T>();

        Assert.Equal(parameters.Select(parameter => parameter.Name), properties.Take(parameters.Length));

        var parts = parameters
            .Select((parameter, index) => $"{parameter.Name} = {arguments[index]}")
            .Concat(properties.Skip(parameters.Length).Select(name => $"{name} = {Part(record, name)}"));

        Assert.Equal($"{typeof(T).Name} {{ {string.Join(", ", parts)} }}", record.ToString());
    }

    private static object? Part(object record, string name) =>
        record.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!.GetValue(record);

    /// <summary>What <c>record with { Name = value }</c> compiles to: the record's clone, then the init of the one part.</summary>
    private static T With<T>(T record, string name, object? value) where T : class
    {
        var clone = (T)typeof(T).GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance)!.Invoke(record, null)!;
        typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!.SetValue(clone, value);

        return clone;
    }
}
