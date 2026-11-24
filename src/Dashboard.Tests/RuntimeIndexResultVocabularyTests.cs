using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The questions <see cref="RuntimeIndexResult" /> answers itself, so <c>RuntimeView.razor</c> asks rather than
/// assembles them from the raw parts, and the two named constructors the loader builds a result with. They are
/// internal (the record's public surface is pinned by <see cref="RuntimeIndexResultEdgeCaseTests" />) and the app
/// declares no <c>InternalsVisibleTo</c>, so they are found by reflection.
/// </summary>
public class RuntimeIndexResultVocabularyTests
{
    private const BindingFlags Internal = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags InternalStatic = BindingFlags.NonPublic | BindingFlags.Static;

    private static readonly RuntimeIndexEntry WithDatabase = new("tdd", "tdd.json", "tdd.svg", new RuntimeDatabaseFiles("db.svg", "db.json"));
    private static readonly RuntimeIndexEntry WithoutDatabase = new("prod", "prod.json", "prod.svg");
    private static readonly RuntimeIndex TwoEnvironments = new([WithDatabase, WithoutDatabase], "1.2026.01.01");
    private static readonly RuntimeIndex NoEnvironments = new([], "1.2026.01.01");

    private static bool Ask(RuntimeIndexResult result, string question)
    {
        var property = typeof(RuntimeIndexResult).GetProperty(question, Internal);
        Assert.NotNull(property);
        Assert.True(property!.GetMethod!.IsAssembly);
        return (bool)property.GetValue(result)!;
    }

    private static RuntimeIndexResult Build(string name, object argument)
    {
        var method = typeof(RuntimeIndexResult).GetMethod(name, InternalStatic);
        Assert.NotNull(method);
        Assert.True(method!.IsAssembly);
        return Assert.IsType<RuntimeIndexResult>(method.Invoke(null, [argument]));
    }

    private static bool RecordsDatabaseOf(RuntimeIndexResult result, string environment)
    {
        var method = typeof(RuntimeIndexResult).GetMethod("RecordsDatabaseOf", Internal);
        Assert.NotNull(method);
        return (bool)method!.Invoke(result, [environment])!;
    }

    public static TheoryData<string, bool, bool, bool> States => new()
    {
        // state, IsNotDeployed, IsValid, HasEnvironments
        { "available", false, true, true },
        { "empty", false, true, false },
        { "notDeployed", true, false, false },
        { "failed", false, false, false },
    };

    private static RuntimeIndexResult Of(string state) => state switch
    {
        "available" => new RuntimeIndexResult(RuntimeAvailability.Available, TwoEnvironments, []),
        "empty" => new RuntimeIndexResult(RuntimeAvailability.Available, NoEnvironments, []),
        "notDeployed" => RuntimeIndexResult.NotDeployed,
        _ => new RuntimeIndexResult(RuntimeAvailability.Failed, null, ["runtime/index.json: not JSON."]),
    };

    [Theory]
    [MemberData(nameof(States))]
    public void EachOutcomeAnswersTheQuestionsThePageAsks(string state, bool notDeployed, bool valid, bool hasEnvironments)
    {
        var result = Of(state);

        Assert.Equal(notDeployed, Ask(result, "IsNotDeployed"));
        Assert.Equal(valid, Ask(result, "IsValid"));
        Assert.Equal(hasEnvironments, Ask(result, "HasEnvironments"));
    }

    [Fact]
    public void TheQuestionsAddNoPartSoEqualityAndWordsAreUnchanged()
    {
        var result = Of("available");

        Assert.Equal(result, new RuntimeIndexResult(RuntimeAvailability.Available, TwoEnvironments, result.Errors));
        Assert.DoesNotContain("IsValid", result.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("HasEnvironments", result.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("IsNotDeployed", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ReadIsAvailableWithTheIndexAndNoErrors()
    {
        var result = Build("Read", TwoEnvironments);

        Assert.Equal(RuntimeAvailability.Available, result.Availability);
        Assert.Same(TwoEnvironments, result.Index);
        Assert.Empty(result.Errors);
        Assert.True(Ask(result, "IsValid"));
    }

    [Fact]
    public void FailedHasNoIndexAndTheVeryErrorsGiven()
    {
        IReadOnlyList<string> errors = ["The server answered HTTP 500 for runtime/index.json."];

        var result = Build("Failed", errors);

        Assert.Equal(RuntimeAvailability.Failed, result.Availability);
        Assert.Null(result.Index);
        Assert.Same(errors, result.Errors);
        Assert.False(Ask(result, "IsValid"));
        Assert.False(Ask(result, "IsNotDeployed"));
    }

    [Fact]
    public void RecordsDatabaseOfAnswersOnlyForTheExactNameOfAnEnvironmentWithADatabase()
    {
        var result = Of("available");

        Assert.True(RecordsDatabaseOf(result, "tdd"));
        Assert.False(RecordsDatabaseOf(result, "TDD"));
        Assert.False(RecordsDatabaseOf(result, "prod"));
        Assert.False(RecordsDatabaseOf(result, "uat"));
        Assert.False(RecordsDatabaseOf(Of("notDeployed"), "tdd"));
        Assert.False(RecordsDatabaseOf(Of("failed"), "tdd"));
    }
}
