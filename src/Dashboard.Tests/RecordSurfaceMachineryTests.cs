using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The tests of the walks <see cref="RecordSurface" /> gathers beside its first one: for every file that still carries
/// its own private copy of a surface-reading helper - the record walk that keeps fields and the static-class walk - the
/// shared walk says exactly what that copy says, so a file may call the shared one and keep its expected list as the
/// pin. The generic readers <see cref="TopologyEdgeCaseTests" /> once carried now live in <see cref="RecordSurface" />
/// alone; their old copies stand at the foot of this file as an independent second reading of the same records.
/// </summary>
public class RecordSurfaceMachineryTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Static;

    [Theory]
    [InlineData(typeof(DeploymentsReportEdgeCaseTests), typeof(DeploymentsReport))]
    [InlineData(typeof(ResourceHealthReadingEdgeCaseTests), typeof(ResourceHealthReading))]
    public void WithFieldsOfSaysWhatEachFilesOwnCopyOfTheWalkSays(Type tests, Type record) =>
        Assert.Equal(CopyOf(tests), RecordSurface.WithFieldsOf(record));

    [Theory]
    [InlineData(typeof(DeploymentFreezeEdgeCaseTests), typeof(DeploymentFreeze))]
    [InlineData(typeof(ProcessVitalsEdgeCaseTests), typeof(ProcessVitals))]
    [InlineData(typeof(ProcessVitalsEdgeCaseTests), typeof(VitalsText))]
    [InlineData(typeof(TelemetrySnapshotEdgeCaseTests), typeof(TelemetrySnapshot))]
    public void WithFieldsOfSaysWhatEachFilesOwnTypedCopyOfTheWalkSays(Type tests, Type record) =>
        Assert.Equal(TypedCopyOf(tests, record), RecordSurface.WithFieldsOf(record));

    [Theory]
    [InlineData(typeof(RuntimeManifestEdgeCaseTests), typeof(RuntimeManifest))]
    [InlineData(typeof(RuntimePayloadEdgeCaseTests), typeof(RuntimePayload))]
    [InlineData(typeof(RuntimePayloadEdgeCaseTests), typeof(RuntimeLink))]
    [InlineData(typeof(VersionAssessmentEdgeCaseTests), typeof(VersionAssessment))]
    [InlineData(typeof(VersionAssessmentEdgeCaseTests), typeof(NodeVersion))]
    [InlineData(typeof(VersionAssessmentEdgeCaseTests), typeof(VersionSummary))]
    public void OfSaysWhatEachFilesOwnTypedCopyOfTheWalkSays(Type tests, Type record) =>
        Assert.Equal(TypedCopyOf(tests, record), RecordSurface.Of(record));

    [Theory]
    [InlineData(typeof(ActivityKindEdgeCaseTests), typeof(ActivityKind))]
    [InlineData(typeof(ApplicationTextEdgeCaseTests), typeof(ApplicationText))]
    [InlineData(typeof(AvailabilityTextEdgeCaseTests), typeof(AvailabilityText))]
    [InlineData(typeof(DeploymentWaitEdgeCaseTests), typeof(DeploymentActivity))]
    [InlineData(typeof(ResourceHealthTextEdgeCaseTests), typeof(ResourceHealthText))]
    [InlineData(typeof(UnplacedTextEdgeCaseTests), typeof(UnplacedText))]
    [InlineData(typeof(WorkColumnsBoundaryEdgeCaseTests), typeof(WorkColumns))]
    public void StaticOfSaysWhatEachFilesOwnCopyOfTheWalkSays(Type tests, Type type) =>
        Assert.Equal(CopyOf(tests), RecordSurface.StaticOf(type));

    public static TheoryData<Type> TopologyRecords =>
        [typeof(Topology), typeof(SystemInfo), typeof(DashboardInfo), typeof(EnvironmentInfo), typeof(ClusterInfo), typeof(PageLink)];

    [Theory]
    [MemberData(nameof(TopologyRecords))]
    public void PropertyNamesOfSaysWhatTopologysNamesOfSays(Type record) =>
        Assert.Equal(OwnReading<string[]>(nameof(NamesOf), record), Shared<string[]>(nameof(RecordSurface.PropertyNamesOf), record));

    [Theory]
    [MemberData(nameof(TopologyRecords))]
    public void OfSaysWhatTopologysPublicSurfaceOfSays(Type record) =>
        Assert.Equal(OwnReading<string[]>(nameof(PublicSurfaceOf), record), RecordSurface.Of(record));

    [Theory]
    [MemberData(nameof(TopologyRecords))]
    public void PropertiesOfSaysWhatTopologysPropertiesOfSays(Type record)
    {
        var copy = OwnReading<Dictionary<string, PropertyInfo>>(nameof(PropertiesOf), record);
        var shared = Shared<Dictionary<string, PropertyInfo>>(nameof(RecordSurface.PropertiesOf), record);

        Assert.Equal(copy.Keys.Order(StringComparer.Ordinal), shared.Keys.Order(StringComparer.Ordinal));
        Assert.All(copy, pair => Assert.Equal(pair.Value, shared[pair.Key]));
    }

    [Theory]
    [MemberData(nameof(TopologyRecords))]
    public void ParametersOfSaysWhatTopologysParametersOfSays(Type record)
    {
        var copy = OwnReading<ParameterInfo[]>(nameof(ParametersOf), record);
        var shared = Shared<ParameterInfo[]>(nameof(RecordSurface.ParametersOf), record);

        Assert.Equal(copy, shared);

        Assert.Equal(
            [.. copy.Select(IsNullable)],
            [.. shared.Select(RecordSurface.IsNullable)]);
    }

    [Theory]
    [MemberData(nameof(TopologyRecords))]
    public void FieldNamesOfSaysWhatTopologysNamesOfFieldsSays(Type record) =>
        Assert.Equal(OwnReading<string[]>(nameof(NamesOfFields), record), Shared<string[]>(nameof(RecordSurface.FieldNamesOf), record));

    [Fact]
    public void SignatureSaysWhatProbeUrlEdgeCaseTestsPrivateCopySays()
    {
        var method = typeof(ProbeUrl).GetMethod("Combine", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);

        var privateCopy = typeof(ProbeUrlEdgeCaseTests).GetMethod("Signature", Private, [typeof(MethodInfo)])!;
        var privateResult = Assert.IsType<string>(privateCopy.Invoke(null, [method]));
        var sharedResult = RecordSurface.Signature(method);

        Assert.Equal(privateResult, sharedResult);
    }

    [Fact]
    public void SignatureSaysWhatTimeTextEdgeCaseTestsPrivateCopySays()
    {
        var method = typeof(TimeText).GetMethod("Clock", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);

        var privateCopy = typeof(TimeTextEdgeCaseTests).GetMethod("Signature", Private, [typeof(MethodInfo)])!;
        var privateResult = Assert.IsType<string>(privateCopy.Invoke(null, [method]));
        var sharedResult = RecordSurface.Signature(method);

        Assert.Equal(privateResult, sharedResult);
    }

    [Fact]
    public void WithFieldsOfKeepsThePublicFieldsTheRecordWalkWalksPast()
    {
        Assert.DoesNotContain("Word", RecordSurface.Of<WithAField>());
        Assert.Contains("Word", RecordSurface.WithFieldsOf(typeof(WithAField)));
        Assert.Equal(
            RecordSurface.Of<WithAField>().Append("Word").Order(StringComparer.Ordinal),
            RecordSurface.WithFieldsOf(typeof(WithAField)));
    }

    [Fact]
    public void StaticOfKeepsFieldsPropertiesAndMethodsAndWalksPastAccessorsAndInstanceMembers()
    {
        Assert.Equal(["Answer", "Ask", "Word"], RecordSurface.StaticOf(typeof(StaticSample)));
        Assert.Equal(["Word"], RecordSurface.StaticOf(typeof(WithAField)));
    }

    [Fact]
    public void FieldNamesOfAndIsNullableReadTheFixedWordsAndTheNullableParts()
    {
        Assert.Equal(["Word"], RecordSurface.FieldNamesOf<WithAField>());
        Assert.Equal(["Left", "Right"], RecordSurface.PropertyNamesOf<WithAField>());
        Assert.Equal([false, true], [.. RecordSurface.ParametersOf<WithAField>().Select(RecordSurface.IsNullable)]);
    }

    private static List<string> CopyOf(Type tests)
    {
        var copy = tests.GetMethod("PublicSurface", Private, Type.EmptyTypes);
        Assert.NotNull(copy);
        return Assert.IsType<List<string>>(copy.Invoke(null, null));
    }

    private static List<string> TypedCopyOf(Type tests, Type type)
    {
        var copy = tests.GetMethod("PublicSurface", Private, [typeof(Type)]);
        Assert.NotNull(copy);
        return Assert.IsType<List<string>>(copy.Invoke(null, [type]));
    }

    private static T OwnReading<T>(string name, Type record)
    {
        var copy = typeof(RecordSurfaceMachineryTests).GetMethod(name, Private, Type.EmptyTypes);
        Assert.NotNull(copy);
        return Assert.IsType<T>(copy.MakeGenericMethod(record).Invoke(null, null));
    }

    private static T Shared<T>(string name, Type record) =>
        Assert.IsType<T>(typeof(RecordSurface).GetMethod(name, Type.EmptyTypes)!.MakeGenericMethod(record).Invoke(null, null));

    // ------------------------------------------- the second reading, as TopologyEdgeCaseTests once wrote it

    /// <summary>The public properties a record declares itself, in the order they stand in it.</summary>
    private static string[] NamesOf<T>() =>
        [.. typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)];

    /// <summary>The public members a record declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept - with the record's compiler-named clone walked past.</summary>
    private static string[] PublicSurfaceOf<T>() =>
        [.. typeof(T)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The public properties of a record, by name, looked up one by one.</summary>
    private static Dictionary<string, PropertyInfo> PropertiesOf<T>() =>
        NamesOf<T>()
            .ToDictionary(
                name => name,
                name => typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!,
                StringComparer.Ordinal);

    /// <summary>The parts a record's one constructor names, in the order it names them.</summary>
    private static ParameterInfo[] ParametersOf<T>() => typeof(T).GetConstructors().Single().GetParameters();

    /// <summary>True when the record says a part may be nothing.</summary>
    private static bool IsNullable(ParameterInfo parameter) =>
        new NullabilityInfoContext().Create(parameter).WriteState == NullabilityState.Nullable;

    /// <summary>The public static fields a record declares itself, in ordinal order.</summary>
    private static string[] NamesOfFields<T>() =>
        [.. typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(field => field.Name)
            .Order(StringComparer.Ordinal)];

    private sealed record WithAField(string Left, string? Right)
    {
        public const string Word = "word";
    }

    private static class StaticSample
    {
        public const string Word = "word";

        public static int Answer => 42;

        public static string Ask() => Word;
    }
}
