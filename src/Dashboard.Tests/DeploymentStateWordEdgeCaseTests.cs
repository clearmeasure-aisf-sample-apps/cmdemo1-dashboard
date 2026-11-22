using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still open for <see cref="DeploymentState" />, declared in
/// <c>src/Dashboard/Health/DeploymentsReport.cs</c>. <see cref="DeploymentStateEdgeCaseTests" /> already holds the
/// enum's own edge cases (its numbers, names, order, unset value, values outside the seven, enum parsing and the
/// declared surface), so these tests sit in a file of their own rather than over that one.
/// <see cref="DeploymentsTests" /> pins the file's seven lower-case words (and <c>Executing</c>) and the shape of the
/// dot for each of the seven states.
/// <para>
/// Left open and pinned here: each of the seven words read in any case and with spaces around it, under a reader's
/// culture whose casing differs from the invariant one; a word that is close to a state but is not one (including the
/// word <c>unknown</c> itself) landing on <see cref="DeploymentState.Unknown" /> with the file's word kept; a state the
/// file leaves blank or does not give as text leaving the entry out; the wait read only for a word that means
/// <see cref="DeploymentState.Waiting" />, in whatever case; and the shape of the dot for numbers outside the seven.
/// </para>
/// <para>
/// Nothing here reaches the network or asks the clock: every word and moment is one the test supplies.
/// </para>
/// </summary>
public class DeploymentStateWordEdgeCaseTests
{
    /// <summary>The single deployment of a file whose one entry has <paramref name="stateJson" /> as its state.</summary>
    private static Deployment? Read(string stateJson, string more = "") =>
        DeploymentsReport.Parse(
            $$"""{ "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1", "state": {{stateJson}}{{more}} } ] }""")!
            .Deployments.SingleOrDefault();

    /// <summary>Runs <paramref name="read" /> with <paramref name="culture" /> as the reader's culture, then restores it.</summary>
    private static T Under<T>(string culture, Func<T> read)
    {
        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            return read();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }

    // ----- the file's words, in any case

    /// <summary>A word of the file means its state in any case, and with spaces around it, which are trimmed from the
    /// word that is kept.</summary>
    [Theory]
    [InlineData("QUEUED", DeploymentState.Queued)]
    [InlineData("Queued", DeploymentState.Queued)]
    [InlineData("EXECUTING", DeploymentState.Executing)]
    [InlineData("eXeCuTiNg", DeploymentState.Executing)]
    [InlineData("WAITING", DeploymentState.Waiting)]
    [InlineData("Waiting", DeploymentState.Waiting)]
    [InlineData("SUCCEEDED", DeploymentState.Succeeded)]
    [InlineData("Succeeded", DeploymentState.Succeeded)]
    [InlineData("FAILED", DeploymentState.Failed)]
    [InlineData("Failed", DeploymentState.Failed)]
    [InlineData("CANCELED", DeploymentState.Canceled)]
    [InlineData("Canceled", DeploymentState.Canceled)]
    public void AWordOfTheFileIsItsStateInAnyCase(string word, DeploymentState expected)
    {
        var deployment = Read($"\"{word}\"")!;
        Assert.Equal(expected, deployment.State);
        Assert.Equal(word, deployment.StateText);

        var spaced = Read($"\"  {word}\\t\"")!;
        Assert.Equal(expected, spaced.State);
        Assert.Equal(word, spaced.StateText);
    }

    /// <summary>The words are compared in the invariant culture: under Turkish, where the capital I lowers to a dotless
    /// ı, <c>WAITING</c> and <c>EXECUTING</c> still mean their states.</summary>
    [Theory]
    [InlineData("tr-TR")]
    [InlineData("az-Latn-AZ")]
    [InlineData("de-DE")]
    public void TheWordsMeanTheirStatesWhateverTheReadersCulture(string culture)
    {
        Assert.Equal(DeploymentState.Waiting, Under(culture, () => Read("\"WAITING\"")!.State));
        Assert.Equal(DeploymentState.Executing, Under(culture, () => Read("\"EXECUTING\"")!.State));
        Assert.Equal(DeploymentState.Failed, Under(culture, () => Read("\"FAILED\"")!.State));
        Assert.Equal(DeploymentState.Queued, Under(culture, () => Read("\"QUEUED\"")!.State));
    }

    /// <summary>A word close to a state is no state: it is <see cref="DeploymentState.Unknown" />, and the file's word
    /// is kept as written. That includes the enum's own name <c>unknown</c>, the British <c>cancelled</c>, a word with
    /// a space inside, a number that is a state's number, and a word with punctuation.</summary>
    [Theory]
    [InlineData("unknown")]
    [InlineData("Unknown")]
    [InlineData("cancelled")]
    [InlineData("queue")]
    [InlineData("execute")]
    [InlineData("succeed")]
    [InlineData("in progress")]
    [InlineData("wait ing")]
    [InlineData("failed!")]
    [InlineData("0")]
    [InlineData("2")]
    [InlineData("6")]
    [InlineData("-1")]
    public void AWordCloseToAStateIsUnknownAndKeptAsWritten(string word)
    {
        var deployment = Read($"\"{word}\"")!;

        Assert.Equal(DeploymentState.Unknown, deployment.State);
        Assert.Equal(word, deployment.StateText);
        Assert.True(deployment.InFlight);
        Assert.Equal($"demo-ui 1 in uat: {word}", DeploymentText.Sentence(deployment, new DateTimeOffset(2026, 10, 8, 4, 23, 24, TimeSpan.Zero)));
    }

    /// <summary>A word with a letter that only lowers to a state's word in some other culture is still no state: the
    /// Turkish dotted capital İ is not an I.</summary>
    [Fact]
    public void ADottedCapitalIIsNoI()
    {
        Assert.Equal(DeploymentState.Unknown, Read("\"WAİTİNG\"")!.State);
        Assert.Equal(DeploymentState.Unknown, Under("tr-TR", () => Read("\"WAİTİNG\"")!.State));
        Assert.Equal("WAİTİNG", Read("\"WAİTİNG\"")!.StateText);
    }

    /// <summary>A state that is empty, blank, null, a number, a boolean, a list or an object gives the entry no state,
    /// so the entry is left out of the deployments; missing the state entirely does the same.</summary>
    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("\"\\t\\n\"")]
    [InlineData("null")]
    [InlineData("3")]
    [InlineData("true")]
    [InlineData("[ \"waiting\" ]")]
    [InlineData("{ \"name\": \"waiting\" }")]
    public void AStateThatIsNoTextLeavesTheEntryOut(string stateJson)
    {
        Assert.Null(Read(stateJson));
        Assert.Empty(DeploymentsReport.Parse("""{ "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1" } ] }""")!.Deployments);
    }

    // ----- the wait follows the state, not the word's case

    /// <summary>The wait is read for a word that means <see cref="DeploymentState.Waiting" /> in any case, and for no
    /// other state, even when the file gives one.</summary>
    [Theory]
    [InlineData("waiting", true)]
    [InlineData("WAITING", true)]
    [InlineData(" Waiting ", true)]
    [InlineData("waited", false)]
    [InlineData("queued", false)]
    [InlineData("executing", false)]
    [InlineData("failed", false)]
    [InlineData("paused", false)]
    public void TheWaitIsReadOnlyForAWordThatMeansWaiting(string word, bool read)
    {
        var deployment = Read($"\"{word}\"", """, "waitsFor": { "kind": "sign-off", "responsible": "demo approvers" }""")!;

        if (read)
        {
            Assert.Equal(DeploymentState.Waiting, deployment.State);
            Assert.Equal(new DeploymentWait("sign-off", null, null, "demo approvers"), deployment.WaitsFor);
        }
        else
        {
            Assert.NotEqual(DeploymentState.Waiting, deployment.State);
            Assert.Null(deployment.WaitsFor);
        }
    }

    // ----- numbers outside the seven

    /// <summary>A number the enum does not name has the dot of what ended, like <see cref="DeploymentState.Unknown" />:
    /// only the three in-flight states have shapes of their own.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void ANumberOutsideTheSevenHasTheShapeOfWhatEnded(int number) =>
        Assert.Equal("ended", DeploymentText.Shape((DeploymentState)number));

    /// <summary>The three in-flight states each have their own shape, the four others all share one, so there are
    /// exactly four shapes across the seven states.</summary>
    [Fact]
    public void TheSevenStatesHaveExactlyFourShapes()
    {
        var shapes = Enum.GetValues<DeploymentState>().Select(DeploymentText.Shape).ToArray();

        Assert.Equal(["ended", "queued", "executing", "waiting", "ended", "ended", "ended"], shapes);
        Assert.Equal(["ended", "executing", "queued", "waiting"], shapes.Distinct().Order());
    }
}
