namespace Dashboard.Tests;

/// <summary>
/// The null-side of <see cref="RuntimeIndexResult" /> that <see cref="RuntimeIndexResultEdgeCaseTests" /> leaves open: the
/// result whose reasons are not a list at all, the comparison of a result against nothing, and the two members a result
/// is asked again and again - the hash it says and the parts it falls apart into.
/// <para>
/// <see cref="RuntimeIndexResultEdgeCaseTests" /> pins the record as a whole: the three parts by the names of their own
/// properties, the equality that weighs them, the copy, the fall apart, the line it says, the cached
/// <see cref="RuntimeIndexResult.NotDeployed" />, the whole public surface and the <see cref="RuntimeAvailability" />
/// vocabulary. This file pins only what that file leaves open: that the line ends with <c>Errors =</c> and no word when
/// the reasons are not a list, that a result is never the same as nothing and never equal to nothing, that the hash it
/// says is the hash it says again, that a fall apart hands back the null it was given, and that a copy of the cached
/// <see cref="RuntimeIndexResult.NotDeployed" /> holds the very list the cache holds.
/// </para>
/// </summary>
public class RuntimeIndexResultNullEdgeCaseTests
{
    private static RuntimeIndexResult Failed => new(RuntimeAvailability.Failed, null, null!);

    [Fact]
    public void TheLineEndsWithNoWordWhenTheReasonsAreNothing()
    {
        Assert.Equal("RuntimeIndexResult { Availability = Failed, Index = , Errors =  }", Failed.ToString());
    }

    [Fact]
    public void AResultIsNeverTheSameAsNothingAndNeverEqualToIt()
    {
        var result = RuntimeIndexResult.NotDeployed;

        Assert.True(result != null);
        Assert.False(result == null);
        Assert.False(null == result);
        Assert.True(null != result);
        Assert.False(result.Equals((object?)null));
        Assert.False(result.Equals((RuntimeIndexResult?)null));
    }

    [Fact]
    public void TheHashItSaysIsTheHashItSaysAgain()
    {
        var result = Failed;

        Assert.Equal(result.GetHashCode(), result.GetHashCode());
    }

    [Fact]
    public void TheFallApartHandsBackTheNullItWasGiven()
    {
        var (availability, index, errors) = Failed;

        Assert.Equal(RuntimeAvailability.Failed, availability);
        Assert.Null(index);
        Assert.Null(errors);
    }

    [Fact]
    public void ACopyOfTheCachedNotDeployedHoldsTheVeryListTheCacheHolds()
    {
        var served = RuntimeIndexResult.NotDeployed with { Availability = RuntimeAvailability.Available };

        Assert.Same(RuntimeIndexResult.NotDeployed.Errors, served.Errors);
        Assert.Null(served.Index);
        Assert.NotEqual(served, RuntimeIndexResult.NotDeployed);
    }
}
