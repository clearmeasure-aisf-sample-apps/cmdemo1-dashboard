using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The three named constructors of <see cref="RuntimeZoomChange"/> the runtime view raises (#1422): each is exactly the
/// change the page made with its bare <c>Step</c> bit before, so the zoom in, the back out and the correction in place
/// come to the same addresses and the same history steps as they did.
/// </summary>
public class RuntimeZoomChangeNamedConstructorTests
{
    [Fact]
    public void ZoomIntoTheDatabaseIsAStepToTheDatabaseView()
    {
        var change = RuntimeZoomChange.ZoomIn("prod", RuntimeZoomTarget.TheDatabase);

        Assert.Equal(new RuntimeZoomChange("prod", RuntimeZoomTarget.TheDatabase, Step: true), change);
        Assert.True(change.Step);
        Assert.Equal("runtime/prod/database", change.Address.ToHash());
    }

    [Fact]
    public void ZoomIntoABoxIsAStepToItsApplicationView()
    {
        var change = RuntimeZoomChange.ZoomIn("prod", RuntimeZoomTarget.Box("web"));

        Assert.Equal(new RuntimeZoomChange("prod", RuntimeZoomTarget.Box("web"), Step: true), change);
        Assert.True(change.Step);
        Assert.Equal("web", change.Address.Application);
        Assert.False(change.Address.Database);
    }

    [Fact]
    public void BackOutIsAStepToTheDiagram()
    {
        var change = RuntimeZoomChange.BackOut("test");

        Assert.Equal(new RuntimeZoomChange("test", RuntimeZoomTarget.Diagram, Step: true), change);
        Assert.True(change.Step);
        Assert.Equal("runtime/test", change.Address.ToHash());
    }

    [Fact]
    public void CorrectInPlaceComesToTheDiagramWithoutAStep()
    {
        var change = RuntimeZoomChange.CorrectInPlace("test");

        Assert.Equal(new RuntimeZoomChange("test", RuntimeZoomTarget.Diagram, Step: false), change);
        Assert.False(change.Step);
        Assert.Equal("runtime/test", change.Address.ToHash());
    }

    [Fact]
    public void BackOutAndCorrectInPlaceDifferOnlyInTheStep()
    {
        var back = RuntimeZoomChange.BackOut("uat");
        var corrected = RuntimeZoomChange.CorrectInPlace("uat");

        Assert.NotEqual(back, corrected);
        Assert.Equal(back.Address, corrected.Address);
        Assert.Equal(back with { Step = false }, corrected);
    }

    [Fact]
    public void TheNamedConstructorsKeepTheEnvironmentInItsOwnLetters()
    {
        Assert.Equal(" Prod ", RuntimeZoomChange.ZoomIn(" Prod ", RuntimeZoomTarget.TheDatabase).Environment);
        Assert.Equal(" Prod ", RuntimeZoomChange.BackOut(" Prod ").Environment);
        Assert.Equal(" Prod ", RuntimeZoomChange.CorrectInPlace(" Prod ").Environment);
    }

    [Fact]
    public void TheRuntimeViewRaisesNoZoomChangeWithABareStepBit()
    {
        var markup = File.ReadAllText(Path.Combine(DashboardRoot(), "Components", "RuntimeView.razor"));

        Assert.DoesNotContain("new RuntimeZoomChange(", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Step: ", markup, StringComparison.Ordinal);
        Assert.Contains("RuntimeZoomChange.ZoomIn(", markup, StringComparison.Ordinal);
        Assert.Contains("RuntimeZoomChange.BackOut(", markup, StringComparison.Ordinal);
        Assert.Contains("RuntimeZoomChange.CorrectInPlace(", markup, StringComparison.Ordinal);
    }

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));
}
