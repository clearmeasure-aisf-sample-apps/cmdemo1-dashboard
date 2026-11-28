namespace Dashboard.Tests;

/// <summary>
/// The two required names of the database manifest, pinned one at a time (#2192): each missing word refuses with
/// exactly <c>{word}: missing or empty.</c> and only its own refusal, so the read and the refusal cannot drift apart.
/// </summary>
public class DatabaseParserManifestFieldTests
{
    [Fact]
    public void AManifestWithoutItsDatabaseRefusesOnlyTheDatabase()
    {
        var result = DatabaseParser.ParseManifest("""{"environment":"staging","tables":[]}""");

        Assert.False(result.IsValid);
        Assert.Equal(["database: missing or empty."], result.Errors);
    }

    [Fact]
    public void AManifestWithoutItsEnvironmentRefusesOnlyTheEnvironment()
    {
        var result = DatabaseParser.ParseManifest("""{"database":"orders","tables":[]}""");

        Assert.False(result.IsValid);
        Assert.Equal(["environment: missing or empty."], result.Errors);
    }
}
