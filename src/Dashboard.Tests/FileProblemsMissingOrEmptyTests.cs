namespace Dashboard.Tests;

/// <summary>
/// The runtime readers' refusal of a missing required word, <c>{place}: missing or empty.</c>, held once in
/// <see cref="FileProblems.MissingOrEmpty" /> instead of typed out by <see cref="RuntimeManifestParser" /> and
/// <see cref="DatabaseParser" /> (#2165). The words are pinned here so the holder cannot be reworded quietly, and the
/// database manifest's two refusals are walked through to show the readers still say them, in order.
/// </summary>
public class FileProblemsMissingOrEmptyTests
{
    [Fact]
    public void ADottedPlaceEndsInTheSentence()
    {
        Assert.Equal("environments[0].name: missing or empty.", FileProblems.MissingOrEmpty("environments[0].name"));
    }

    [Fact]
    public void ATopLevelWordEndsInTheSentence()
    {
        Assert.Equal("database: missing or empty.", FileProblems.MissingOrEmpty("database"));
    }

    [Fact]
    public void ADatabaseManifestWithoutEitherNameRefusesBothInOrder()
    {
        var result = DatabaseParser.ParseManifest("""{"tables":[]}""");

        Assert.False(result.IsValid);
        Assert.Equal(["environment: missing or empty.", "database: missing or empty."], result.Errors);
        Assert.Equal([FileProblems.MissingOrEmpty("environment"), FileProblems.MissingOrEmpty("database")], result.Errors);
    }

    [Fact]
    public void ARuntimeManifestWithoutItsEnvironmentRefusesItByTheHolder()
    {
        var result = RuntimeManifestParser.ParseManifest("""{"nodes":[],"edges":[]}""");

        Assert.Contains(FileProblems.MissingOrEmpty("environment"), result.Errors);
    }
}
