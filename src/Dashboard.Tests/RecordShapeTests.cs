namespace Dashboard.Tests;

/// <summary>
/// The shared <see cref="RecordShape" /> helper, checked against counterexamples: a type that breaks one rule of the
/// record contract must be reported as breaking it. Without these checks, a helper that always answered "fine" would
/// make every test that uses it pass.
/// </summary>
public class RecordShapeTests
{
    [Fact]
    public void APlainSealedRecordIsRecognised()
    {
        Assert.True(RecordShape.IsPlainSealedRecord<Plain>());
        Assert.Equal(["Deconstruct", "Equals", "GetHashCode", "Name", "ToString", "op_Equality", "op_Inequality"], RecordShape.SurfaceOf<Plain>());

        // The Type overload, for files that check several records through one variable, answers the same.
        Type type = new Plain("x").GetType();

        Assert.Equal(RecordShape.SurfaceOf<Plain>(), RecordShape.SurfaceOf(type));
    }

    [Fact]
    public void AnUnsealedRecordIsNotAPlainSealedRecord() =>
        Assert.False(RecordShape.IsPlainSealedRecord<Unsealed>());

    [Fact]
    public void ARecordStructIsNotAPlainSealedRecord() =>
        Assert.False(RecordShape.IsPlainSealedRecord<Struct>());

    [Fact]
    public void ASealedClassThatIsNoRecordIsNotAPlainSealedRecord() =>
        Assert.False(RecordShape.IsPlainSealedRecord<NotARecord>());

    [Fact]
    public void ARecordAnsweringAnotherInterfaceIsNotAPlainSealedRecord() =>
        Assert.False(RecordShape.IsPlainSealedRecord<Named>());

    [Fact]
    public void ARecordStandingOnAnotherRecordIsNotAPlainSealedRecord() =>
        Assert.False(RecordShape.IsPlainSealedRecord<Derived>());

    [Fact]
    public void AnExtraMemberShowsInTheSurface() =>
        Assert.Contains("Shout", RecordShape.SurfaceOf<WithExtra>());

    [Fact]
    public void AMutablePropertyIsNotInitOnlyAndAGetOnlyPropertyIsNotWritable()
    {
        Assert.Equal(
            [
                new RecordPart("Name", typeof(string), Nullable: false, Readable: true, InitOnly: true),
                new RecordPart("Count", typeof(int), Nullable: false, Readable: true, InitOnly: false),
                new RecordPart("Note", typeof(string), Nullable: true, Readable: true, InitOnly: false),
            ],
            RecordShape.PropertiesOf<Mutable>());
    }

    [Fact]
    public void ADefaultedParameterIsOptional()
    {
        Assert.Equal(
            [
                new RecordParameter("Name", typeof(string), Nullable: false, Optional: false),
                new RecordParameter("Note", typeof(string), Nullable: true, Optional: true),
            ],
            RecordShape.ConstructorOf<Defaulted>());
    }

    [Fact]
    public void ATypeWithTwoPublicConstructorsIsRefused()
    {
        var refusal = Assert.Throws<InvalidOperationException>(RecordShape.ConstructorOf<TwoConstructors>);

        Assert.Contains("2 public constructors", refusal.Message, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------------------------ the counterexamples

    public sealed record Plain(string Name);

    public record Unsealed(string Name);

    public readonly record struct Struct(string Name);

    public sealed class NotARecord : IEquatable<NotARecord>
    {
        public bool Equals(NotARecord? other) => ReferenceEquals(this, other);

        public override bool Equals(object? obj) => Equals(obj as NotARecord);

        public override int GetHashCode() => 0;
    }

    public interface INamed
    {
        string Name { get; }
    }

    public sealed record Named(string Name) : INamed;

    public sealed record Derived(string Name) : Unsealed(Name);

    public sealed record WithExtra(string Name)
    {
        public string Shout() => Name.ToUpperInvariant();
    }

    public sealed record Mutable(string Name)
    {
        public int Count { get; set; }

        public string? Note => Name.Length == 0 ? null : Name;
    }

    public sealed record Defaulted(string Name, string? Note = null);

    public sealed record TwoConstructors(string Name)
    {
        public TwoConstructors() : this(string.Empty)
        {
        }
    }
}
