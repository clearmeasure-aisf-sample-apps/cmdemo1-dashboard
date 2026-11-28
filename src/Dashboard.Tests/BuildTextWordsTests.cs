namespace Dashboard.Tests;

/// <summary>
/// The unit words of <see cref="BuildText.Size" /> and the lead word of <see cref="BuildText.Complexity" /> and
/// <see cref="BuildText.Crap" />, pinned arm by arm: the same unit word stands after every line count and every file
/// count the card says, and the same lead word stands before their largest score - including the today's wording of
/// a count of one, which the card does not pluralise.
/// </summary>
public sealed class BuildTextWordsTests
{
    [Fact]
    public void SizeSaysLinesOnlyWhenOnlyLinesAreMeasured() =>
        Assert.Equal("84,210 lines", BuildText.Size(new CodeSize(84210, null, [])));

    [Fact]
    public void SizeSaysLinesInFilesWhenBothAreMeasured() =>
        Assert.Equal("84,210 lines in 1,203 files", BuildText.Size(new CodeSize(84210, 1203, [])));

    [Fact]
    public void SizeSaysFilesOnlyWhenOnlyFilesAreMeasured() =>
        Assert.Equal("1,203 files", BuildText.Size(new CodeSize(null, 1203, [])));

    [Fact]
    public void SizeSaysNothingWhenNeitherIsMeasured() =>
        Assert.Null(BuildText.Size(new CodeSize(null, null, [])));

    [Fact]
    public void SizeDoesNotPluraliseACountOfOne() =>
        Assert.Equal("1 lines in 1 files", BuildText.Size(new CodeSize(1, 1, [])));

    [Fact]
    public void ComplexityLeadsItsLargestScoreWithWorst() =>
        Assert.Equal("worst 34", BuildText.Complexity(new ComplexityFacts(null, 34, null)));

    [Fact]
    public void CrapLeadsItsLargestScoreWithWorst() =>
        Assert.Equal("worst 12", BuildText.Crap(new CrapFacts(12, null, null)));
}
