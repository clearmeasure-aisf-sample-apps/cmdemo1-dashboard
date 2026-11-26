using System.Reflection;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The default of the probe control has one home (#1815): the probe the page opens on is one of the
/// <c>ProbeChoices</c> of <c>App.razor</c>, named once as the const <c>DefaultProbe</c>, which the choices are
/// built from. So the control still offers Health check and Liveness and opens on Health check - no behaviour
/// change - and the two cannot come apart by a changed list, the shape <see cref="TrafficRun.Default"/> gives the
/// traffic button's numbers (#1742) and the interval's default keeps (#1796).
/// </summary>
/// <remarks>
/// The values are read from the compiled page (a private static of the class <c>App.razor</c> writes and the
/// probe a fresh page opens on, the way <see cref="IntervalDefaultChoiceTests"/> reads the interval's choices) and
/// the single home from the page's source, the way <see cref="AppHeaderTests"/> reads that file: the drift this
/// guards against - a second <c>ProbeKind.Health</c> beside the list, agreeing only because nobody changed the
/// choices - lives exactly where those two meet. The help text and the aria wiring of the control itself stay
/// pinned by <see cref="HeaderControlHelpTests"/>, which these tests join rather than replace.
/// </remarks>
public class ProbeDefaultChoiceTests
{
    // ---------------------------------------------------------------- what the control offers and opens on

    /// <summary>
    /// No behaviour change (#1815): the control offers Health check and Liveness, in that order, with the words the
    /// viewer reads for each, and the page opens on Health check.
    /// </summary>
    [Fact]
    public void TheControlOffersHealthCheckAndLivenessAndOpensOnHealthCheck()
    {
        var choices = ProbeChoicesOfThePage();

        Assert.Equal([ProbeKind.Health, ProbeKind.Liveness], choices.Select(choice => choice.Value).ToList());
        Assert.Equal(["Health check", "Liveness"], choices.Select(choice => choice.Label).ToList());
        Assert.Equal(ProbeKind.Health, OpenProbeOfThePage());
    }

    /// <summary>
    /// The default is one of the choices the control offers: the page opens on a probe the viewer sees in the
    /// select, not on a kind written beside the list.
    /// </summary>
    [Fact]
    public void ThePageOpensOnAProbeTheViewerSeesInTheSelect()
    {
        var choices = ProbeChoicesOfThePage();
        var probe = OpenProbeOfThePage();

        Assert.Contains(probe, choices.Select(choice => choice.Value).ToList());
    }

    // ---------------------------------------------------------------- the one home of the default

    /// <summary>
    /// The probe the page opens on is written from the named const: the const holds the kind, the choices are built
    /// from it, and the field the select binds to is initialised with it - the one home the issue asked for.
    /// </summary>
    [Fact]
    public void TheChoicesAndTheFieldAreBothBuiltFromTheNamedConst()
    {
        var text = MarkupGuards.AppText();

        Assert.Contains("private const ProbeKind DefaultProbe = ProbeKind.Health;", text, StringComparison.Ordinal);
        Assert.Contains(
            "private static readonly ProbeChoice[] ProbeChoices = [new(DefaultProbe, \"Health check\"), new(ProbeKind.Liveness, \"Liveness\")];",
            text,
            StringComparison.Ordinal);
        Assert.Contains("private ProbeKind _probe = DefaultProbe;", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The field the select binds to carries no bare <c>ProbeKind.Health</c> of its own: the kind it opens on is the
    /// named const, and the const is where <c>ProbeKind.Health</c> is written in the page's code for the default.
    /// </summary>
    [Fact]
    public void TheFieldInitializerWritesNoBareProbeKindHealthOfItsOwn()
    {
        var text = MarkupGuards.AppText();

        var declaration = DeclarationOf(text, "private ProbeKind _probe");
        Assert.DoesNotContain("ProbeKind.Health", declaration, StringComparison.Ordinal);
        Assert.Contains("DefaultProbe", declaration, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- the machinery

    /// <summary>The choices as the app compiles them: a private static of the class <c>App.razor</c> writes, which
    /// the app keeps to itself - the app declares no <c>InternalsVisibleTo</c>, so it is read by reflection the way
    /// <see cref="IntervalDefaultChoiceTests"/> reads the interval's choices.</summary>
    private static IReadOnlyList<ProbeChoice> ProbeChoicesOfThePage()
    {
        var field = PageField("ProbeChoices");
        Assert.Equal(typeof(ProbeChoice[]), field.FieldType);
        Assert.True(field.IsInitOnly, "The choices stay a static readonly, so the page cannot retune them between renders.");

        return (ProbeChoice[])field.GetValue(null)!;
    }

    /// <summary>The probe a fresh page opens on, as the app compiles it: the page's own <c>_probe</c> field,
    /// initialised the way the compiled page initialises it.</summary>
    private static ProbeKind OpenProbeOfThePage()
    {
        var pageType = PageType();

        var page = Activator.CreateInstance(pageType)!;
        var field = pageType.GetField("_probe", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.True(field is not null, "the page's _probe was not read: the guard would be checking nothing");

        return (ProbeKind)field!.GetValue(page)!;
    }

    /// <summary>The class <c>App.razor</c> writes, which the app keeps to itself: found by name, the way
    /// <see cref="IntervalDefaultChoiceTests"/> finds it.</summary>
    private static Type PageType()
    {
        var pages = typeof(TrafficRun).Assembly.GetTypes().Where(type => type.Name == "App").ToList();
        Assert.Single(pages);

        return pages[0];
    }

    /// <summary>A static of the page, by name: the class <c>App.razor</c> writes, which the app keeps private.</summary>
    private static FieldInfo PageField(string name)
    {
        var field = PageType().GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

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
}
