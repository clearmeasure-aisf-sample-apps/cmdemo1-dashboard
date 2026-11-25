using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The one fixed word <see cref="SystemInfo" /> carries beside its parts - <c>DefaultName</c> - and the reader that
/// reads it: <see cref="TopologyParser" /> shows a system that gives neither a name nor a slug under that word, as
/// <see cref="DashboardInfo.DefaultName" /> is the word <see cref="DashboardInfo" /> carries for a dashboard that
/// names no name. The word the reader writes is the record's, the writing is the reader's: <see cref="SystemInfo" />
/// itself fills in no name of its own. The word is internal and the app declares no <c>InternalsVisibleTo</c>, so
/// it is read by reflection, as the internal constants of the cluster are.
/// </summary>
public class SystemInfoDefaultNameEdgeCaseTests
{
    private static readonly FieldInfo DefaultNameField =
        typeof(SystemInfo).GetField("DefaultName", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

    private static readonly string DefaultName = Assert.IsType<string>(DefaultNameField.GetRawConstantValue());

    private static Topology SystemOf(string system) =>
        TopologyParser.Parse($$"""{ "system": {{system}}, "environments": [] }""") is { IsValid: true } result
            ? result.Topology!
            : throw new InvalidOperationException("the test's own file did not read.");

    /// <summary>The one word the reader shows a nameless system under, read off the record here where a change of it
    /// is a change the tests see - the same letters the reader has always written, of no spaces about them.</summary>
    [Fact]
    public void TheDefaultSystemNameIsTheWordSystem()
    {
        Assert.Equal("System", DefaultName);
        Assert.NotEqual(DefaultName, string.Empty);
        Assert.Equal(DefaultName, DefaultName.Trim());
        Assert.DoesNotContain(" ", DefaultName, StringComparison.Ordinal);
    }

    /// <summary>The word stands on the record as a fixed word, and the pinned surface of <see cref="SystemInfo" />
    /// says what it says with it there: it is a static field the record declares, and it is no part of the record's
    /// public surface - the record's parts are the seven the constructor names, and the word is the reader's to take.</summary>
    [Fact]
    public void TheDefaultNameIsTheRecordsFixedWordAndNotItsPublicSurface()
    {
        Assert.NotNull(DefaultNameField);
        Assert.True(DefaultNameField.IsLiteral);
        Assert.Equal("System", DefaultNameField.GetRawConstantValue());
        Assert.Null(typeof(SystemInfo).GetField("DefaultName", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.DoesNotContain("DefaultName", RecordSurface.Of<SystemInfo>());
        Assert.Empty(RecordSurface.FieldNamesOf<SystemInfo>());
    }

    /// <summary>The reader reads the word off the record: a system that is no object, and a system whose slug and
    /// name are spaces, are both shown under the very word <c>DefaultName</c> stands for - and a system that gives a
    /// name or a slug is shown under its own, not the word.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("{ }")]
    [InlineData("""{ "slug": "   ", "name": "" }""")]
    public void ASystemTheReaderNamesNothingIsShownUnderTheDefaultWord(string system) =>
        Assert.Equal(DefaultName, SystemOf(system).System.Name);

    [Fact]
    public void ASlugNamesTheSystemAndTheDefaultWordStaysTheReadersOnlyWord() =>
        Assert.Equal("cmdemo1", SystemOf("""{ "slug": "cmdemo1" }""").System.Name);
}
