namespace Dashboard.Tests;

/// <summary>
/// The <see cref="ActivityLines" /> object mother in <c>TestSupport.cs</c>, pinned where it lives: the moment the
/// page is asked and the zone it says the moments in, the system the deployments file is read for, the line a test
/// builds from its four words, the default line with all four of its parts and the line without its address, the
/// sayers the lines come from reached with the file's words and the names the file was read for, and the questions
/// a test asks of a line - the kind it says, whether it carries the address of the task, whether its kind is one of
/// the in flight, and what its title says. The sayers' words, cut for cut, are pinned in
/// <see cref="DeploymentActivityTests" /> and <see cref="DeploymentActivityEdgeCaseTests" />; this pins the fixtures
/// and the questions those tests, <see cref="DeploymentActivityNodeLineTests" />,
/// <see cref="DeploymentActivityCountsFromTests" /> and <see cref="ActivityLineEdgeCaseTests" /> all read from.
/// </summary>
public class ActivityLinesObjectMotherTests
{
    /// <summary>The moment the page is asked is Friday noon, UTC; the zone is the one the page says the moments in;
    /// and the file is read for the demo's system and the ui's deployable.</summary>
    [Fact]
    public void TheMomentTheZoneAndTheNamesTheFileIsReadForAreHeldOnce()
    {
        Assert.Equal(new DateTimeOffset(2026, 10, 9, 12, 0, 0, TimeSpan.Zero), ActivityLines.Now);
        Assert.Equal(TimeZoneInfo.Utc, ActivityLines.Utc);
        Assert.Equal("demo", ActivityLines.Slug);
        Assert.Equal(["ui"], ActivityLines.Ui);
    }

    /// <summary>A line is built from its four words in the order the record declares them, and the address left out
    /// is the absence of one: the line of three words is the very line the same three words and an explicit absence
    /// make.</summary>
    [Fact]
    public void TheLineIsBuiltFromItsFourWordsAndTheAddressLeftOutIsNoAddress()
    {
        var line = ActivityLines.Line("queued", "2 queued, 3 min", "demo-ui 2 is queued in uat");

        Assert.Equal(("queued", "2 queued, 3 min", "demo-ui 2 is queued in uat", (Uri?)null), (line.Kind, line.Text, line.Title, line.Url));
        Assert.Equal(line, new ActivityLine("queued", "2 queued, 3 min", "demo-ui 2 is queued in uat", null));
    }

    /// <summary>The default line has all four of its parts - a deployment stopped for a person, on the task it waits
    /// for - and the other line has no address: the shape of a file that names no task.</summary>
    [Fact]
    public void TheDefaultLineHasAllFourPartsAndTheOtherHasNoAddress()
    {
        var full = ActivityLines.Full();

        Assert.Equal(
            ("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", "https://octopus.example.invalid/app#/Spaces-5"),
            (full.Kind, full.Text, full.Title, full.Url!.ToString()));
        Assert.Equal(full, new ActivityLine("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", new Uri("https://octopus.example.invalid/app#/Spaces-5")));

        var without = ActivityLines.WithoutAddress();

        Assert.Null(without.Url);
        Assert.Equal(("queued", "2.4.44 queued, 3 min", "demo-ui 2.4.44 is queued in uat"), (without.Kind, without.Text, without.Title));
    }

    /// <summary>The sayers are reached with the file's words and the names the file was read for: the file that names
    /// nothing says nothing, and the file that names one deployment in flight says it in the words the sayers say
    /// them, counted from the moment the mother holds.</summary>
    [Fact]
    public void TheSayersAreReachedWithTheFilesWordsAndTheNamesTheFileWasReadFor()
    {
        Assert.Empty(ActivityLines.Node(ActivityLines.Parse(), "prod"));
        Assert.Empty(ActivityLines.Environment(ActivityLines.Parse(), "tdd"));

        var inFlight = ActivityLines.Parse(deployments: """{ "project": "demo-ui", "environment": "uat", "release": "2.4.44", "state": "executing", "since": "2026-10-09T11:57:00Z" }""");

        Assert.Equal("deploying 2.4.44, 3 min", Assert.Single(ActivityLines.Node(inFlight, "uat")).Text);
        Assert.Empty(ActivityLines.Node(inFlight, "prod"));
        Assert.Empty(ActivityLines.Environment(inFlight, "prod"));
    }

    /// <summary>Of the eight kinds a line is one of, the three a deployment that is still in flight says are in
    /// flight: the freezes, and what ended, are not. The questions answer what the line says, whatever its kind.</summary>
    [Theory]
    [InlineData("waiting", true)]
    [InlineData("deploying", true)]
    [InlineData("queued", true)]
    [InlineData("frozen", false)]
    [InlineData("freeze", false)]
    [InlineData("deployed", false)]
    [InlineData("failed", false)]
    [InlineData("canceled", false)]
    public void TheQuestionsAnswerWhatTheLineSaysWhateverItsKind(string kind, bool inFlight)
    {
        var line = ActivityLines.Line(kind, "w", "t");

        Assert.Equal(kind, ActivityLines.KindOf(line));
        Assert.Equal("t", ActivityLines.TitleOf(line));
        Assert.False(ActivityLines.HasLink(line));
        Assert.Equal(inFlight, ActivityLines.IsInFlight(line));
    }

    /// <summary>The address is carried where the line leads a reader to the task, and the title says its word in the
    /// case it is written in - the question is ordinal, and the word in another case, and the word that is not
    /// there, are not named.</summary>
    [Fact]
    public void TheAddressIsCarriedAndTheTitleSaysItsWordInItsOwnCase()
    {
        Assert.True(ActivityLines.HasLink(ActivityLines.Full()));
        Assert.False(ActivityLines.HasLink(ActivityLines.WithoutAddress()));
        Assert.True(ActivityLines.TitleMentions(ActivityLines.Full(), "sign-off"));
        Assert.False(ActivityLines.TitleMentions(ActivityLines.Full(), "Sign-off"));
        Assert.False(ActivityLines.TitleMentions(ActivityLines.WithoutAddress(), "sign-off"));
    }
}
