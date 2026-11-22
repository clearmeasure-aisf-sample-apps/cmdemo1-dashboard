namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still open for <see cref="Deployment" /> as an entry of the
/// deployments file becomes one - declared in <c>src/Dashboard/Health/DeploymentsReport.cs</c>.
/// <para>
/// <see cref="DeploymentEdgeCaseTests" /> already holds the record's own edge cases under the name this issue asked for,
/// and <see cref="DeploymentBoundaryEdgeCaseTests" /> and <see cref="DeploymentStartAndEndEdgeCaseTests" /> the flight
/// and the marking at the ends of the clock, so these stand in a file of their own and leave every one of those as it
/// is. <see cref="DeploymentsTests" /> pins an entry without one of its four names left out, a <c>release</c> given as a
/// number left out, a time that does not parse and an address that is no http address being nothing;
/// <see cref="DeploymentStateWordEdgeCaseTests" /> the state's word in any case and padded, and a state that is no text;
/// <see cref="DeploymentActivityTests" /> the <c>waitsFor</c> shapes.
/// </para>
/// <para>
/// Left open and pinned here: the project, the environment, the release and who started it trimmed as they are read,
/// and kept in their case; who started it given empty, blank, as a number or as nothing being nobody; a project or
/// an environment that is no text leaving the entry out; a time with no offset read as UTC, one with an offset kept in
/// its hours, a date alone read as its midnight, a padded time trimmed, and fractions of a second kept to the tick; an
/// http address kept, an address padded trimmed, and an address in capitals kept as the same address; and two equal
/// entries both read, in the file's order, with fields the page does not know read past.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is written in the file the
/// test brings, and no address is fetched.
/// </para>
/// </summary>
public class DeploymentEntryReadEdgeCaseTests
{
    private const string Tasks = "https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-";

    /// <summary>Twenty minutes past four, UTC: the moment the sample's <c>demo-ui 2.4.43</c> started.</summary>
    private static readonly DateTimeOffset Since = new(2026, 10, 8, 4, 20, 7, TimeSpan.Zero);

    /// <summary>The deployments of a file whose one list is <paramref name="entries" />, written as JSON.</summary>
    private static IReadOnlyList<Deployment> ReadAll(string entries) =>
        DeploymentsReport.Parse($$"""{ "deployments": [ {{entries}} ] }""")!.Deployments;

    /// <summary>The one deployment of a file whose one entry is executing in uat with <paramref name="more" /> beside
    /// its four names; null when the entry is left out.</summary>
    private static Deployment? ReadOne(string more) =>
        ReadAll($$"""{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "state": "executing"{{more}} }""").SingleOrDefault();

    // ----- the words, trimmed as they are read

    /// <summary>The project, the environment, the release and who started it lose the whitespace about them as they
    /// are read and keep their letters and their case: the record keeps whatever it is given, and it is the reading
    /// that trims.</summary>
    [Fact]
    public void TheWordsOfAnEntryAreTrimmedAndKeepTheirCase()
    {
        var deployment = ReadAll("""{ "project": "  Demo-UI\t", "environment": "\n UAT ", "release": " 2.4.43 ", "state": "executing", "startedBy": "  Jeffrey  " }""").Single();

        Assert.Equal("Demo-UI", deployment.Project);
        Assert.Equal("UAT", deployment.Environment);
        Assert.Equal("2.4.43", deployment.Release);
        Assert.Equal("Jeffrey", deployment.StartedBy);
        Assert.Equal(new Deployment("Demo-UI", "UAT", "2.4.43", DeploymentState.Executing, "executing", null, null, null, "Jeffrey"), deployment);
    }

    /// <summary>Who started a deployment is nobody when the file gives it as the empty word, as whitespace, as nothing
    /// or as anything that is no text - and the entry is read all the same, since the person is no part an entry must
    /// give.</summary>
    [Theory]
    [InlineData(""", "startedBy": "" """)]
    [InlineData(""", "startedBy": "   " """)]
    [InlineData(""", "startedBy": "\t\n" """)]
    [InlineData(""", "startedBy": null""")]
    [InlineData(""", "startedBy": 42""")]
    [InlineData(""", "startedBy": true""")]
    [InlineData(""", "startedBy": [ "jeffrey" ]""")]
    [InlineData(""", "startedBy": { "name": "jeffrey" }""")]
    [InlineData("")]
    public void WhoStartedItIsNobodyUnlessTheFileWritesAWord(string more)
    {
        var deployment = ReadOne(more);

        Assert.NotNull(deployment);
        Assert.Null(deployment.StartedBy);
        Assert.Equal("2.4.43", deployment.Release);
    }

    /// <summary>A project or an environment the file gives as anything but a word of letters is no name, and the entry
    /// without it is left out - as the release given as a number is, where the file is read.</summary>
    [Theory]
    [InlineData("""{ "project": 7, "environment": "uat", "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": null, "environment": "uat", "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": [ "demo-ui" ], "environment": "uat", "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": { "name": "demo-ui" }, "environment": "uat", "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": "", "environment": "uat", "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": "demo-ui", "environment": false, "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": "demo-ui", "environment": null, "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": "demo-ui", "environment": "", "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": "demo-ui", "environment": " \t ", "release": "1", "state": "executing" }""")]
    [InlineData("""{ "project": "demo-ui", "environment": "uat", "release": "", "state": "executing" }""")]
    [InlineData("""{ "project": "demo-ui", "environment": "uat", "release": null, "state": "executing" }""")]
    public void ANameThatIsNoWordLeavesTheEntryOut(string entry) => Assert.Empty(ReadAll(entry));

    // ----- the two moments

    /// <summary>A moment the file writes without an offset is read as UTC, one with an offset keeps the hours it is
    /// written in and is the same instant, a date alone is its midnight in UTC, a moment padded is trimmed, and a
    /// fraction of a second is kept to the tick - in either of the two moments.</summary>
    [Theory]
    [InlineData("2026-10-08T04:20:07Z", 0)]
    [InlineData("2026-10-08T04:20:07", 0)]
    [InlineData("2026-10-08 04:20:07", 0)]
    [InlineData("  2026-10-08T04:20:07Z  ", 0)]
    [InlineData("2026-10-08T09:50:07+05:30", 330)]
    [InlineData("2026-10-07T23:20:07-05:00", -300)]
    public void AMomentIsTheInstantItNamesInTheHoursItIsWrittenIn(string written, int offsetMinutes)
    {
        var deployment = ReadOne($$""", "since": "{{written}}", "finished": "{{written}}" """)!;

        Assert.Equal(Since, deployment.Since);
        Assert.Equal(Since, deployment.Finished);
        Assert.Equal(TimeSpan.FromMinutes(offsetMinutes), deployment.Since!.Value.Offset);
        Assert.Equal(TimeSpan.FromMinutes(offsetMinutes), deployment.Finished!.Value.Offset);
    }

    [Fact]
    public void ADateAloneIsItsMidnightInUtcAndAFractionIsKeptToTheTick()
    {
        var midnight = ReadOne(""", "since": "2026-10-08" """)!;

        Assert.Equal(new DateTimeOffset(2026, 10, 8, 0, 0, 0, TimeSpan.Zero), midnight.Since);
        Assert.Equal(TimeSpan.Zero, midnight.Since!.Value.Offset);

        var exact = ReadOne(""", "finished": "2026-10-08T04:20:07.1234567Z" """)!;

        Assert.Equal(Since.AddTicks(1_234_567), exact.Finished);
        Assert.Null(exact.Since);
    }

    /// <summary>A moment given as nothing, as the empty word, as whitespace or as a number is no moment.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("0")]
    [InlineData("[ \"2026-10-08T04:20:07Z\" ]")]
    [InlineData("{ \"at\": \"2026-10-08T04:20:07Z\" }")]
    public void AMomentThatIsNoWordIsNoMoment(string json)
    {
        var deployment = ReadOne($$""", "since": {{json}}, "finished": {{json}}""")!;

        Assert.Null(deployment.Since);
        Assert.Null(deployment.Finished);
        Assert.True(deployment.InFlight);
    }

    // ----- the address

    /// <summary>An http address is kept as much as an https one, an address padded is trimmed, and an address in
    /// capitals is the same address with its scheme and host in small letters.</summary>
    [Fact]
    public void AnHttpAddressIsKeptPaddedOrInCapitals()
    {
        var plain = ReadOne(""", "url": "http://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-1" """)!;

        Assert.Equal(new Uri("http://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-1"), plain.Url);
        Assert.Equal("http", plain.Url!.Scheme);

        var padded = ReadOne($$""", "url": "  {{Tasks}}1\t" """)!;

        Assert.Equal(new Uri($"{Tasks}1"), padded.Url);

        var capitals = ReadOne(""", "url": "HTTPS://OCTOPUS.EXAMPLE.NET/app#/Spaces-356/tasks/ServerTasks-1" """)!;

        Assert.Equal("https", capitals.Url!.Scheme);
        Assert.Equal("octopus.example.net", capitals.Url.Host);
        Assert.Equal(new Uri($"{Tasks}1"), capitals.Url);
    }

    /// <summary>An address given as nothing, as the empty word, as whitespace, as a number, or with a scheme the page
    /// never links to is no address, and the entry is read all the same.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("12")]
    [InlineData("\"ftp://octopus.example.net/tasks/1\"")]
    [InlineData("\"mailto:ops@example.net\"")]
    [InlineData("\"//octopus.example.net/tasks/1\"")]
    public void AnAddressThatIsNoHttpAddressIsNoAddress(string json)
    {
        var deployment = ReadOne($$""", "url": {{json}}""");

        Assert.NotNull(deployment);
        Assert.Null(deployment.Url);
    }

    // ----- the list

    /// <summary>Two entries that say the same are two deployments, both kept and in the file's order, and a field the
    /// page does not know is read past without changing the deployment.</summary>
    [Fact]
    public void TwoEqualEntriesAreBothReadAndUnknownFieldsAreReadPast()
    {
        var deployments = ReadAll("""
            { "project": "demo-ui", "environment": "uat", "release": "1", "state": "queued", "priority": 3, "tags": [ "x" ] },
            { "project": "demo-ui", "environment": "uat", "release": "1", "state": "queued" },
            { "project": "demo-api", "environment": "uat", "release": "1", "state": "queued", "Project": "ignored" }
            """);

        Assert.Equal(3, deployments.Count);
        Assert.Equal(deployments[0], deployments[1]);
        Assert.NotSame(deployments[0], deployments[1]);
        Assert.Equal(["demo-ui", "demo-ui", "demo-api"], deployments.Select(deployment => deployment.Project));
        Assert.Equal(new Deployment("demo-ui", "uat", "1", DeploymentState.Queued, "queued", null, null, null), deployments[0]);
    }
}
