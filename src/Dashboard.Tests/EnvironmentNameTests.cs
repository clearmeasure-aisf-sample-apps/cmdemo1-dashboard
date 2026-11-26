using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// An environment's name: ordinal identity (equality and hashing), a case-insensitive match against the page's
/// environment, and a null or default name read as the empty name.
/// </summary>
public class EnvironmentNameTests
{
    [Fact]
    public void TheSameNameCaseForCaseIsTheSameEnvironment()
    {
        var first = new EnvironmentName("UAT");
        var second = new EnvironmentName("UAT");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Theory]
    [InlineData("UAT", "uat")]
    [InlineData("Prod", "PROD")]
    [InlineData("tdd", "TDD")]
    public void NamesThatDifferOnlyInCaseAreDifferentEnvironments(string name, string other)
    {
        var first = new EnvironmentName(name);
        var second = new EnvironmentName(other);

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Theory]
    [InlineData("UAT", "UAT")]
    [InlineData("UAT", "uat")]
    [InlineData("Prod", "PROD")]
    [InlineData("tdd", "TdD")]
    public void ThePageMatchesTheNameInAnyCase(string name, string page)
    {
        Assert.True(new EnvironmentName(name).Matches(page));
    }

    [Theory]
    [InlineData("UAT", "Prod")]
    [InlineData("UAT", "UAT2")]
    [InlineData("UAT", " UAT")]
    [InlineData("UAT", "")]
    public void ThePageDoesNotMatchAnotherName(string name, string page)
    {
        Assert.False(new EnvironmentName(name).Matches(page));
    }

    [Fact]
    public void AMissingPageEnvironmentMatchesNoName()
    {
        Assert.False(new EnvironmentName("UAT").Matches(null));
        Assert.False(new EnvironmentName(null).Matches(null));
    }

    [Fact]
    public void ANullNameIsTheEmptyName()
    {
        var name = new EnvironmentName(null);

        Assert.Equal(string.Empty, name.Value);
        Assert.Equal(string.Empty, name.ToString());
        Assert.Equal(new EnvironmentName(string.Empty), name);
        Assert.True(name.Matches(string.Empty));
    }

    [Fact]
    public void TheDefaultNameIsTheEmptyName()
    {
        var name = default(EnvironmentName);

        Assert.Equal(string.Empty, name.Value);
        Assert.Equal(string.Empty, name.ToString());
        Assert.Equal(new EnvironmentName(string.Empty), name);
        Assert.Equal(new EnvironmentName(null).GetHashCode(), name.GetHashCode());
    }

    [Theory]
    [InlineData("UAT")]
    [InlineData("prod")]
    [InlineData("")]
    public void ToStringIsTheNameAsWritten(string value)
    {
        Assert.Equal(value, new EnvironmentName(value).ToString());
        Assert.Equal(value, new EnvironmentName(value).Value);
    }

    [Fact]
    public void ASetOfNamesKeepsNamesThatDifferInCaseApartAndFoldsDuplicates()
    {
        var names = new HashSet<EnvironmentName>
        {
            new("UAT"),
            new("uat"),
            new("UAT"),
            new("Prod"),
        };

        Assert.Equal(3, names.Count);
        Assert.Contains(new EnvironmentName("uat"), names);
        Assert.DoesNotContain(new EnvironmentName("PROD"), names);
    }

    [Fact]
    public void AnEnvironmentNameKeysADictionaryOrdinally()
    {
        var bySlot = new Dictionary<EnvironmentName, int> { [new("UAT")] = 1, [new("uat")] = 2 };

        Assert.Equal(1, bySlot[new EnvironmentName("UAT")]);
        Assert.Equal(2, bySlot[new EnvironmentName("uat")]);
        Assert.False(bySlot.ContainsKey(new EnvironmentName("Uat")));
    }
}
