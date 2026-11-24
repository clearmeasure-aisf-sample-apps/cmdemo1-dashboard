using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// One part a record declares, as the record-contract tests read it: its name, its type, whether the record
/// declares it may be the nothing, whether it can be read, and whether its write is an init and nothing more.
/// Comparing whole parts lets a mismatch name every fact of the part in a single failure.
/// </summary>
internal sealed record RecordPart(string Name, Type Type, bool Nullable, bool Readable, bool InitOnly);

/// <summary>
/// One parameter of a record's constructor, as the record-contract tests read it: its name, its type, whether
/// the record lets it be the nothing, and whether a caller may leave it out.
/// </summary>
internal sealed record RecordParameter(string Name, Type Type, bool Nullable, bool Optional);

/// <summary>
/// The shape a positional record shows to reflection. The record-contract tests ask the same questions of every
/// record: is it sealed, not a struct, standing on <see cref="object" /> and answering only
/// <see cref="IEquatable{T}" />; which public members does it declare itself; are its parts init-only, and with
/// what nullability; and what does its one public constructor take, in what order. Each test file used to write
/// its own copy of this reflection. Here it is written once, so all callers use the same rules and the same
/// failure messages.
/// </summary>
internal static class RecordShape
{
    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    /// <summary>
    /// The public members the record declares itself, sorted ordinally: its properties and the methods that are
    /// not the get or set of one. The two operators are kept although the compiler names them specially, since
    /// they are the <c>==</c> and the <c>!=</c> a caller writes. Compiler-named members such as the clone are
    /// skipped. The constructor is not a method, so <see cref="ConstructorOf{T}" /> reports it instead.
    /// </summary>
    public static List<string> SurfaceOf<T>() => SurfaceOf(typeof(T));

    /// <inheritdoc cref="SurfaceOf{T}" />
    public static List<string> SurfaceOf(Type type) =>
        [.. type
            .GetMembers(Declared)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>
    /// The public instance properties the record declares itself, in declaration order, each read as a
    /// <see cref="RecordPart" />. Nullability is the read state <see cref="NullabilityInfoContext" /> reports.
    /// A part is init-only when its set accessor carries the <see cref="IsExternalInit" /> modifier.
    /// </summary>
    public static IReadOnlyList<RecordPart> PropertiesOf<T>()
    {
        var nullability = new NullabilityInfoContext();

        return
        [
            .. typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => new RecordPart(
                    property.Name,
                    property.PropertyType,
                    nullability.Create(property).ReadState == NullabilityState.Nullable,
                    property.CanRead,
                    IsInitOnly(property))),
        ];
    }

    /// <summary>
    /// The parameters of the record's one public constructor, in order, each read as a
    /// <see cref="RecordParameter" />. Nullability is the write state <see cref="NullabilityInfoContext" />
    /// reports. A parameter is optional when it is marked optional or has a default value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The type declares no public constructor, or more than one.</exception>
    public static IReadOnlyList<RecordParameter> ConstructorOf<T>()
    {
        var constructors = typeof(T).GetConstructors();

        if (constructors.Length != 1)
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} declares {constructors.Length} public constructors; a positional record declares exactly one.");
        }

        var nullability = new NullabilityInfoContext();

        return
        [
            .. constructors[0]
                .GetParameters()
                .Select(parameter => new RecordParameter(
                    parameter.Name!,
                    parameter.ParameterType,
                    nullability.Create(parameter).WriteState == NullabilityState.Nullable,
                    parameter.IsOptional || parameter.HasDefaultValue)),
        ];
    }

    /// <summary>
    /// The type is a plain sealed record: a sealed class rather than a struct, standing directly on
    /// <see cref="object" />, answering no interface but <see cref="IEquatable{T}" /> of its own kind, and
    /// carrying the compiler's record clone.
    /// </summary>
    public static bool IsPlainSealedRecord<T>()
    {
        var type = typeof(T);

        return type.IsSealed
               && !type.IsValueType
               && type.BaseType == typeof(object)
               && type.GetInterfaces().SequenceEqual([typeof(IEquatable<T>)])
               && type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) is not null;
    }

    private static bool IsInitOnly(PropertyInfo property) =>
        property.SetMethod is { } set
        && set.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit));
}

/// <summary>
/// The runtime view's "change" records, made with the values the tests usually use, so that a test names only
/// the part it is about.
/// </summary>
internal static class RuntimeChanges
{
    /// <summary>A zoom into (or back out of) a deployable's application view.</summary>
    public static RuntimeApplicationChange Application(
        string environment = "uat", string? deployable = "demo-ui", bool step = true) =>
        new(environment, deployable, step);

    /// <summary>A zoom into (or back out of) an environment's database view.</summary>
    public static RuntimeDatabaseChange Database(string environment = "uat", bool shown = true, bool step = true) =>
        new(environment, shown, step);
}
