using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="Shares" /> is the one rule of the dashboard for the percent a part holds of a whole: the part over
/// the whole, on the hundred. The language bar of the "Code" card, the share of the cluster view and the estimate
/// of the cost card all cut their percents by it, and what is pinned here is the rule itself - the order it cuts
/// in, bit for bit, the scale it stands on under its name, and the three sites that route through it - beside
/// the words the cards already say, which <see cref="BuildTextEdgeCaseTests" />, <see cref="ClusterTextTests" />,
/// <see cref="ClusterTextEdgeCaseTests" /> and <see cref="CostTextEdgeCaseTests" /> pin where the fixtures answer
/// them.
/// <para>
/// The order the rule cuts in is the deliberate one: the part over the whole first, then the scale - the order the
/// cluster's own share has always been cut in. The other order, the scale on the part first, differs from it by
/// the last ulp at the rounding boundary: a share that stands exactly on a half percent says the even word cut in
/// the other order and the word below it cut in this one, which is the only word the unification moves, and none
/// of the words the fixtures pin stands on it. The rounding of the number to a word stays the card's own - to the
/// even whole for the bar and the cluster, away from zero for the cost estimate - and is pinned beside those
/// words.
/// </para>
/// </summary>
public class SharesTests
{
    // ----- the rule itself

    /// <summary>The share of a part of a whole is the part over the whole, on the hundred: the even quarters said
    /// as the quarters are, the whole against itself as the hundred, nothing as nothing, a part past the whole as
    /// the number it is, and the sign of a part below nothing kept with it.</summary>
    [Fact]
    public void TheShareOfAPartOfAWholeIsThePartOverTheWholeOnTheHundred()
    {
        Assert.Equal(25, Shares.Of(1, 4));
        Assert.Equal(50, Shares.Of(1, 2));
        Assert.Equal(75, Shares.Of(3, 4));
        Assert.Equal(100, Shares.Of(1, 1));
        Assert.Equal(0, Shares.Of(0, 5));
        Assert.Equal(150, Shares.Of(150, 100));
        Assert.Equal(-25, Shares.Of(-1, 4));

        // The one that is not a whole number is said as the order's own arithmetic gives it, not as a rounded copy.
        Assert.Equal(2d / 3d * 100d, Shares.Of(2, 3));
    }

    /// <summary>The order the rule cuts in is pinned bit for bit: the part over the whole first, then the scale.
    /// The other order - the scale on the part first - lands one ulp away where the one does not, and a share that
    /// stands exactly on a half percent stands a hair below it, the last ulp the unification owns at the rounding
    /// boundary.</summary>
    [Fact]
    public void TheOrderIsThePartOverTheWholeThenTheScale()
    {
        Assert.Equal(1d / 300d * 100d, Shares.Of(1, 300));
        Assert.NotEqual(100d * 1d / 300d, Shares.Of(1, 300));

        // Twenty-three of forty is exactly 57.5 in the arithmetic; the rule stands on the hair below it, so the
        // even whole the bar's rounding reaches is the one below, and the one above is the word the other order
        // said.
        Assert.Equal(23d / 40d * 100d, Shares.Of(23, 40));
        Assert.NotEqual(57.5, Shares.Of(23, 40));
    }

    /// <summary>The scale and the rule are said once, under the names the rule was given: the hundred a share of
    /// a whole is a percent of, and the one static method the part and the whole are handed to - pinned the way
    /// the card's other fixed facts are pinned, by reflection, so the number the percents stand on cannot drift
    /// from the name.</summary>
    [Fact]
    public void TheScaleAndTheRuleAreSaidOnceUnderNames()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var fields = typeof(Shares).GetFields(flags).Where(field => field.IsLiteral && !field.IsPublic).ToList();

        Assert.Contains(fields, field => field.Name == "PercentScale" && Equals(field.GetRawConstantValue(), 100d));

        var of = typeof(Shares).GetMethod("Of", flags);
        Assert.NotNull(of);
        Assert.Equal(typeof(double), of!.ReturnType);
        Assert.Equal([typeof(double), typeof(double)], of.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
    }

    // ----- the three sites that route through the rule

    /// <summary>The bar cuts each part by the rule, bit for bit - the lines of the part over the lines of all
    /// languages - and says the word the rule's number rounds to. The share of twenty-three of forty stood exactly
    /// on the half in the order the bar cut in before the rule, and said the even word above it; cut by the rule
    /// it stands a hair below the half and says the word below it, the one word the unification moves, by the
    /// rounding the rule's doc names.</summary>
    [Fact]
    public void TheBarCutsItsSharesByTheRule()
    {
        var code = new CodeSize(null, null, [new LanguageSize("A", 23, null), new LanguageSize("B", 17, null)]);
        var shares = BuildText.Shares(code);

        Assert.Equal(Shares.Of(23, 40), shares[0].Percent);
        Assert.Equal(Shares.Of(17, 40), shares[1].Percent);
        Assert.Equal("A 57 %, B 42 %", BuildText.Describe(shares));
    }

    /// <summary>The cluster's share is the rule itself, bit for bit - the order it has always been cut in - and
    /// the guard that keeps it a share and not a division stays with it: no number, or a whole that is not more
    /// than zero, is no share at all.</summary>
    [Fact]
    public void TheClusterShareCutsByTheRuleAndKeepsItsGuards()
    {
        Assert.Equal(Shares.Of(812, 3860), ClusterText.Share(812, 3860)!.Value);
        Assert.Equal(Shares.Of(1, 3), ClusterText.Share(1, 3)!.Value);
        Assert.Null(ClusterText.Share(null, 3860));
        Assert.Null(ClusterText.Share(812, null));
        Assert.Null(ClusterText.Share(812, 0));
    }

    /// <summary>The estimate of the cost card says its share by the rule against the whole of one - the share the
    /// file gives is already a part of it - rounded to the whole percent away from zero, the rounding the cost
    /// card says its percents in: the half of one percent reads as the percent it rounds away from the zero to,
    /// and a tenth of that reads as nothing.</summary>
    [Theory]
    [InlineData(0d)]
    [InlineData(0.005d)]
    [InlineData(0.2048d)]
    [InlineData(0.995d)]
    [InlineData(1d)]
    public void TheCostEstimateSaysItsShareByTheRule(double share)
    {
        var estimate = new CostEstimate(share, new CostAmounts(0, 0, 0.5));
        var said = (int)Math.Round(Shares.Of(share, 1d), MidpointRounding.AwayFromZero);

        Assert.EndsWith($"({said} % of what all pods request)", CostText.Estimate(estimate, Report, Now), StringComparison.Ordinal);
    }

    /// <summary>The moment the estimate's line is said, and the report it is said of: the last complete UTC day
    /// before it, in the currency the file named, with nothing the file did not say.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly CostReport Report =
        new(Now, "USD", new DateOnly(2026, 10, 3), new CostAmounts(1.52, 9.8, 11.02), []);
}
