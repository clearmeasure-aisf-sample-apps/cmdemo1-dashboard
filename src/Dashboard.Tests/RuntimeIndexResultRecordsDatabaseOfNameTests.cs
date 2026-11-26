using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The exact-name rule of <c>RuntimeIndexResult.RecordsDatabaseOf</c>: an environment's database view is recorded only
/// for the same <see cref="EnvironmentName" />, case for case. A name that differs only in case - one the page's address
/// would still pick through <see cref="EnvironmentName.Matches" /> - is not recorded. The member is internal and the app
/// declares no <c>InternalsVisibleTo</c>, so it is found by reflection.
/// </summary>
public class RuntimeIndexResultRecordsDatabaseOfNameTests
{
    private static readonly RuntimeDatabaseFiles Database = new("db.svg", "db.json");

    private static RuntimeIndexResult IndexOf(params RuntimeIndexEntry[] entries) =>
        new(RuntimeAvailability.Available, new RuntimeIndex(entries, "1.2026.01.01"), []);

    private static bool RecordsDatabaseOf(RuntimeIndexResult result, string environment)
    {
        var method = typeof(RuntimeIndexResult).GetMethod("RecordsDatabaseOf", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.True(method!.IsAssembly);
        return (bool)method.Invoke(result, [environment])!;
    }

    [Theory]
    [InlineData("UAT", "uat")]
    [InlineData("uat", "UAT")]
    [InlineData("Uat", "uAT")]
    [InlineData("prod", "Prod")]
    public void ANameDifferingOnlyInCaseIsNotRecorded(string indexed, string asked)
    {
        var result = IndexOf(new RuntimeIndexEntry(indexed, "m.json", "d.svg", Database));

        Assert.True(new EnvironmentName(indexed).Matches(asked));
        Assert.False(RecordsDatabaseOf(result, asked));
        Assert.True(RecordsDatabaseOf(result, indexed));
    }

    [Fact]
    public void TwoEnvironmentsDifferingOnlyInCaseAreTwoNamesAndOnlyTheOneWithADatabaseIsRecorded()
    {
        var result = IndexOf(
            new RuntimeIndexEntry("UAT", "UAT.json", "UAT.svg", Database),
            new RuntimeIndexEntry("uat", "uat.json", "uat.svg"));

        Assert.True(RecordsDatabaseOf(result, "UAT"));
        Assert.False(RecordsDatabaseOf(result, "uat"));
        Assert.False(RecordsDatabaseOf(result, "Uat"));
    }

    [Fact]
    public void TheExactNameIsRecordedWhenALaterEntryHasTheDatabase()
    {
        var result = IndexOf(
            new RuntimeIndexEntry("tdd", "a.json", "a.svg"),
            new RuntimeIndexEntry("tdd", "b.json", "b.svg", Database));

        Assert.True(RecordsDatabaseOf(result, "tdd"));
        Assert.False(RecordsDatabaseOf(result, "TDD"));
    }

    [Fact]
    public void AnIndexOfNoEnvironmentsRecordsNoDatabase()
    {
        var result = IndexOf();

        Assert.False(RecordsDatabaseOf(result, "tdd"));
        Assert.False(RecordsDatabaseOf(result, string.Empty));
    }

    [Fact]
    public void ANameWithSurroundingSpaceIsADifferentName()
    {
        var result = IndexOf(new RuntimeIndexEntry("tdd", "m.json", "d.svg", Database));

        Assert.False(RecordsDatabaseOf(result, " tdd"));
        Assert.False(RecordsDatabaseOf(result, "tdd "));
        Assert.True(RecordsDatabaseOf(result, "tdd"));
    }
}
