namespace Dashboard.Tests;

/// <summary>
/// The half of an enum's pin that belongs to the BCL rather than to the enum: what every closed, <see cref="int" />
/// backed vocabulary of the house says about itself whatever its words are. An enum's own edge-case file keeps its
/// names, its numbers, its words and its prose; what it hands here is only the claim the house makes of every such
/// enum - the whole vocabulary in its declared order, an <see cref="int" /> underneath and no set, nothing outside the
/// vocabulary defined, and a value outside it saying the number it stands on.
/// <para>
/// Every failure message names the enum it was asked about, so a broken pin points at the enum and not only at this
/// shared code.
/// </para>
/// <para>
/// It also holds the readings an enum's own file asserts its words against - its names, its numbers, its count, the
/// type underneath, the four values just outside it and the members it declares - so that the BCL calls and the
/// reflection behind them are written here once, and each file says only its enum's words and its claims.
/// </para>
/// </summary>
/// <typeparam name="T">The enum being pinned.</typeparam>
internal static class EnumVocabulary<T> where T : struct, Enum
{
    private static string Type => typeof(T).Name;

    /// <summary>The enum's values, in the order it declares them, are exactly <paramref name="values" />, spelled
    /// exactly <paramref name="names" />, each on its position's number with no number skipped or repeated, and the
    /// value nobody chose is <paramref name="unset" />.</summary>
    public static void AssertWholeVocabulary(T[] values, string[] names, T unset)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(names);

        Assert.True(values.Length == names.Length, $"{Type}: {values.Length} values handed against {names.Length} names.");
        Assert.True(Enum.GetValues<T>().SequenceEqual(values), $"{Type}: the declared values are [{string.Join(", ", Enum.GetValues<T>())}], not [{string.Join(", ", values)}].");
        Assert.True(Enum.GetNames<T>().SequenceEqual(names, StringComparer.Ordinal), $"{Type}: the declared names are [{string.Join(", ", Enum.GetNames<T>())}], not [{string.Join(", ", names)}].");

        for (var position = 0; position < values.Length; position++)
        {
            var value = values[position];
            Assert.True(Number(value) == position, $"{Type}.{value} stands on {Number(value)}, not on its position {position}.");
            Assert.True(names[position] == value.ToString(), $"{Type}.{value} says '{value}', not '{names[position]}'.");
            Assert.True(names[position] == Enum.GetName(value), $"{Type}.{value} is named '{Enum.GetName(value)}', not '{names[position]}'.");
            Assert.True(Enum.IsDefined(value), $"{Type}.{value} is not defined.");
        }

        Assert.True(EqualityComparer<T>.Default.Equals(default, unset), $"{Type}: the unset value is {default(T)}, not {unset}.");
    }

    /// <summary>An <see cref="int" /> underneath, and a choice rather than a set: the enum wears no
    /// <see cref="FlagsAttribute" />.</summary>
    public static void AssertUnderlyingTypeAndNotFlags()
    {
        Assert.True(typeof(int) == Enum.GetUnderlyingType(typeof(T)), $"{Type} stands on {Enum.GetUnderlyingType(typeof(T)).Name}, not Int32.");
        Assert.False(typeof(T).IsDefined(typeof(FlagsAttribute), false), $"{Type} is marked [Flags].");
    }

    /// <summary>Every value from <paramref name="first" /> to <paramref name="last" /> is defined, and nothing outside
    /// them is: neither one below the first, nor one above the last, nor either end of <see cref="int" />.</summary>
    public static void AssertNothingOutsideTheVocabularyIsDefined(T first, T last)
    {
        for (var number = Number(first); number <= Number(last); number++)
        {
            Assert.True(Enum.IsDefined(Of(number)), $"{Type}: {number} lies inside the vocabulary yet is not defined.");
        }

        foreach (var outside in Outside(first, last))
        {
            Assert.False(Enum.IsDefined(Of(outside)), $"{Type}: {outside} lies outside the vocabulary yet is defined.");
            Assert.True(Enum.GetName(Of(outside)) is null, $"{Type}: {outside} lies outside the vocabulary yet is named '{Enum.GetName(Of(outside))}'.");
        }
    }

    /// <summary>A value the enum does not name says the number it stands on - the ends of <see cref="int" /> as
    /// <c>-2147483648</c> and <c>2147483647</c> - and that number, as text, is read back as the same value: no name, no
    /// definition, and nothing lost on the way round.</summary>
    public static void AssertSaysItsOwnNumberOutsideTheVocabulary()
    {
        Assert.True(Of(int.MinValue).ToString() == "-2147483648", $"{Type}: int.MinValue says '{Of(int.MinValue)}'.");
        Assert.True(Of(int.MaxValue).ToString() == "2147483647", $"{Type}: int.MaxValue says '{Of(int.MaxValue)}'.");

        var values = Enum.GetValues<T>();
        var first = values.Length == 0 ? 0 : values.Min(Number);
        var last = values.Length == 0 ? -1 : values.Max(Number);

        foreach (var outside in Outside(Of(first), Of(last)))
        {
            var text = outside.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Assert.True(Of(outside).ToString() == text, $"{Type}: {outside} says '{Of(outside)}', not its number.");
            Assert.True(Enum.TryParse<T>(text, out var read), $"{Type}: '{text}' is not read back.");
            Assert.True(Number(read) == outside, $"{Type}: '{text}' is read back as {Number(read)}.");
            Assert.False(Enum.IsDefined(read), $"{Type}: '{text}' is read back as a defined value.");
        }
    }

    // ----- the readings an enum's own file asserts its words against -----

    /// <summary>The names the enum declares, in the order it declares them.</summary>
    public static string[] Names => Enum.GetNames<T>();

    /// <summary>The numbers the enum's values stand on, in the order it declares them.</summary>
    public static int[] Numbers => [.. Enum.GetValues<T>().Select(Number)];

    /// <summary>How many values the enum declares.</summary>
    public static int Count => Enum.GetValues<T>().Length;

    /// <summary>The integral type the enum stands on.</summary>
    public static Type UnderlyingType => Enum.GetUnderlyingType(typeof(T));

    /// <summary>Whether the enum wears <see cref="FlagsAttribute" /> - whether it is a set rather than a choice.</summary>
    public static bool IsFlags => typeof(T).IsDefined(typeof(FlagsAttribute), false);

    /// <summary>The value standing on <paramref name="number" />, named by the enum or not.</summary>
    public static T Of(int number) => (T)Enum.ToObject(typeof(T), number);

    /// <summary>The four values just outside a vocabulary of <paramref name="count" /> values standing from 0: one
    /// below the first (<c>-1</c>), one above the last (<paramref name="count" />), and the two ends of
    /// <see cref="int" />.</summary>
    public static Edges OutsideOf(int count) => new(Of(-1), Of(count), Of(int.MinValue), Of(int.MaxValue));

    /// <summary>The public instance members the enum declares of its own - for an enum, the storage field the
    /// compiler gives it and nothing else.</summary>
    public static System.Reflection.MemberInfo[] InstanceMembers =>
        typeof(T).GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

    /// <summary>The public static fields the enum declares - its values, as the literals the declaration writes, in
    /// the order it writes them.</summary>
    public static System.Reflection.FieldInfo[] Literals =>
        typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);

    /// <summary>The raw constant each of <see cref="Literals" /> holds, unboxed as the <see cref="int" /> it must be:
    /// a literal of any other type throws rather than being widened into agreement.</summary>
    public static int[] LiteralNumbers => [.. Literals.Select(literal => (int)literal.GetRawConstantValue()!)];

    /// <summary>The four values just outside a vocabulary, as <see cref="OutsideOf" /> reads them.</summary>
    /// <param name="Below">One below the first value: <c>-1</c>.</param>
    /// <param name="Above">One above the last value: the vocabulary's count.</param>
    /// <param name="Lowest">The bottom of <see cref="int" />.</param>
    /// <param name="Highest">The top of <see cref="int" />.</param>
    public readonly record struct Edges(T Below, T Above, T Lowest, T Highest);

    private static IEnumerable<int> Outside(T first, T last)
    {
        if (Number(first) > int.MinValue)
        {
            yield return Number(first) - 1;
        }

        if (Number(last) < int.MaxValue)
        {
            yield return Number(last) + 1;
        }

        yield return int.MinValue;
        yield return int.MaxValue;
    }

    private static int Number(T value) => Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
}
