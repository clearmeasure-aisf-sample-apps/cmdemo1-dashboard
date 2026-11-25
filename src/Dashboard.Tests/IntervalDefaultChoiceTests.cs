using System.Reflection;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The default of the interval control has one home (#1796): the seconds the page opens on is one of the
/// <c>IntervalChoices</c> of <c>App.razor</c>, named once as the const <c>DefaultIntervalSeconds</c>, which the
/// choices and the <c>DefaultInterval</c> are both built from. So the control still offers 10, 30 and 60 seconds
/// and opens on 30 - no behaviour change - and the two cannot come apart by a changed list, the shape
/// <see cref="TrafficRun.Default"/> gives the traffic button's numbers (#1742).
/// </summary>
/// <remarks>
/// The values are read from the compiled page (private statics of the class <c>App.razor</c> writes, the way
/// <see cref="TrafficFollowIntervalTests"/> reads the panel's follow tick) and the single home from the page's
/// source, the way <see cref="AppHeaderTests"/> reads that file: the drift this guards against - a second thirty
/// beside the list, agreeing only because nobody changed the choices - lives exactly where those two meet. The
/// behaviour of the control itself stays pinned by <see cref="AppHeaderTests"/> and
/// <see cref="HeaderControlHelpTests"/>, which these tests join rather than replace.
/// </remarks>
public partial class IntervalDefaultChoiceTests
{
    // ---------------------------------------------------------------- what the control offers and opens on

    /// <summary>
    /// No behaviour change (#1796): the control offers ten, thirty and sixty seconds, in that order, and the page
    /// opens on thirty.
    /// </summary>
    [Fact]
    public void TheControlOffersTenThirtyAndSixtyAndOpensOnThirty()
    {
        Assert.Equal([10, 30, 60], IntervalChoicesOfThePage());
        Assert.Equal(TimeSpan.FromSeconds(30), DefaultIntervalOfThePage());
    }

    /// <summary>
    /// The default is one of the choices the control offers: the page opens on a value the viewer sees in the
    /// select, written in whole seconds the way the choices name them.
    /// </summary>
    [Fact]
    public void TheDefaultIsOneOfTheChoicesTheControlOffers()
    {
        var choices = IntervalChoicesOfThePage();
        var @default = DefaultIntervalOfThePage();

        Assert.Contains((int)@default.TotalSeconds, choices);
        Assert.Equal(TimeSpan.FromSeconds((int)@default.TotalSeconds), @default);
    }

    // ---------------------------------------------------------------- the one home of the default

    /// <summary>
    /// The seconds the page opens on are written from the named const: the const holds the thirty, the choices are
    /// built from it, and the default is the same const as a duration - the one home the issue asked for.
    /// </summary>
    [Fact]
    public void TheChoicesAndTheDefaultAreBothBuiltFromTheNamedConst()
    {
        var text = MarkupGuards.AppText();

        Assert.Contains("private const int DefaultIntervalSeconds = 30;", text, StringComparison.Ordinal);
        Assert.Contains(
            "private static readonly int[] IntervalChoices = [10, DefaultIntervalSeconds, 60];",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            "private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(DefaultIntervalSeconds);",
            text,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Thirty is written once in the page's code, in the const: neither the choices list nor the default interval
    /// carries a literal thirty of its own that would have to keep agreeing with it by nobody changing the choices
    /// since.
    /// </summary>
    [Fact]
    public void NeitherTheChoicesNorTheDefaultWritesThirtyAgain()
    {
        var text = MarkupGuards.AppText();

        foreach (var declaration in new[]
                 {
                     DeclarationOf(text, "private static readonly int[] IntervalChoices"),
                     DeclarationOf(text, "private static readonly TimeSpan DefaultInterval"),
                 })
        {
            Assert.False(
                 ThirtyWhole().IsMatch(declaration),
                $"thirty is written again in: {declaration}");
        }
    }

    // ---------------------------------------------------------------- the machinery

    /// <summary>The choices as the app compiles them: a private static of the class <c>App.razor</c> writes, which
    /// the app keeps to itself - the app declares no <c>InternalsVisibleTo</c>, so it is read by reflection the way
    /// <see cref="TrafficFollowIntervalTests"/> reads the panel's tick.</summary>
    private static int[] IntervalChoicesOfThePage()
    {
        var field = PageField("IntervalChoices");
        Assert.Equal(typeof(int[]), field.FieldType);
        Assert.True(field.IsInitOnly, "The choices stay a static readonly, so the page cannot retune them between renders.");

        return (int[])field.GetValue(null)!;
    }

    /// <summary>The interval the page opens on, as the app compiles it: a static readonly <see cref="TimeSpan"/>,
    /// the shape the rest of the page's rules keep their durations in.</summary>
    private static TimeSpan DefaultIntervalOfThePage()
    {
        var field = PageField("DefaultInterval");
        Assert.Equal(typeof(TimeSpan), field.FieldType);
        Assert.True(field.IsInitOnly, "The default stays a static readonly TimeSpan, the way the panel's follow tick does.");

        return (TimeSpan)field.GetValue(null)!;
    }

    /// <summary>A static of the page, by name: the class <c>App.razor</c> writes, which the app keeps private.</summary>
    private static FieldInfo PageField(string name)
    {
        var pages = typeof(TrafficRun).Assembly.GetTypes().Where(type => type.Name == "App").ToList();
        Assert.Single(pages);

        var field = pages[0].GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

        Assert.True(field is not null, $"the page's {name} was not read: the guard would be checking nothing");

        return field!;
    }

    /// <summary>A declaration of the page's code, whole: from its start to the semicolon that ends it.</summary>
    private static string DeclarationOf(string text, string start)
    {
        var at = text.IndexOf(start, StringComparison.Ordinal);
        Assert.True(at >= 0, $"the declaration of {start} was not read: the guard would be checking nothing");

        var end = text.IndexOf(';', at);
        Assert.True(end > at, $"the declaration of {start} was never ended: the read is lost");

        return text[at..(end + 1)];
    }

    /// <summary>A thirty that stands as a number of its own, not a digit inside another number.</summary>
    [GeneratedRegex(@"\b30\b")]
    private static partial Regex ThirtyWhole();
}
