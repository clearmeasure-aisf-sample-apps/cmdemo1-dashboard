using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary tests still open for <see cref="TargetKind" />, the two words for what end of a deployable a tile
/// stands for, declared in <c>src/Dashboard/Health/TargetStatus.cs</c>: the surface the enum declares itself, as the
/// running assembly holds it.
/// <para>
/// The name this issue asked for is held. <see cref="TargetKindEdgeCaseTests" /> already pins the two kinds as the
/// numbers, the names and the order they are declared in, the unset kind, the names as words, and the text that is
/// or is not a kind; <see cref="TargetKindBoundaryEdgeCaseTests" /> pins the two kinds apart, sorted, combined as
/// numbers and stepped past their ends, the kind a tile carries, and the kind that is absent. What none of them
/// reads - everything reflection sees of the type the enum is compiled into - is set down here, a file of its own
/// over a name that stands, as <see cref="SummaryLevelSurfaceEdgeCaseTests" /> stands over
/// <see cref="SummaryLevelEdgeCaseTests" /> and <see cref="SummaryLevelBoundaryEdgeCaseTests" />.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every field and member below is one
/// the test brought.
/// </para>
/// </summary>
public class TargetKindSurfaceEdgeCaseTests
{
    /// <summary>The two names, in the order the enum declares them: the public address first, the node behind it.</summary>
    private static readonly string[] Names = ["FrontDoor", "Node"];

    /// <summary>The number each name in <see cref="Names" /> stands on, in the same order.</summary>
    private static readonly int[] Numbers = [0, 1];

    // ----- the surface the enum declares -----

    /// <summary>The whole of what <see cref="TargetKind" /> declares itself, as the running assembly holds it: an
    /// <see cref="int" /> underneath, sealed and no set, and exactly its two kinds and nothing else - each one a
    /// public static literal of its own type, in the order the declaration writes them, on the number it stands on;
    /// and no instance member of its own beyond the storage every value type is given, so a kind carries nothing but
    /// the number it is. A third kind added to the file has to be written here as well, which is the point: a tile
    /// cannot be shown for an end of a deployable nobody decided what it means.</summary>
    [Fact]
    public void TheEnumDeclaresItsTwoKindsAndNothingElse()
    {
        var type = typeof(TargetKind);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);
        Assert.Equal(typeof(TargetKind), TargetKind.Node.GetType());

        // The only instance member a kind carries of its own is the storage the compiler gives every value type.
        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(Names, fields.Select(field => field.Name));
        Assert.Equal(Numbers, fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });
    }
}
