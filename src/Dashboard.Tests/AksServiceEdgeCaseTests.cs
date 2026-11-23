namespace Dashboard.Tests;

/// <summary>
/// <see cref="AksService" /> is what the dashboard makes of the file a scheduled workflow publishes several times an
/// hour: a JSON object that may hold almost nothing, read without ever throwing. Its happy path is pinned where it
/// belongs - <see cref="ClusterParserTests" /> reads the shipped <c>aks.json</c> part for part, says what a malformed
/// file is answered with, that every part but the verdicts is optional and that each number of the metrics block may
/// be null or absent - and <see cref="ClusterTextTests" /> holds the 45, 90 and 100 minute points of
/// <c>ClusterText.OldFacts</c>, which is where the age of the file reaches the page.
/// <para>
/// What follows is the edge each of those readings sits next to, and only that: the blank file that is not the empty
/// string (<see langword="null" />, a tab, a newline), the JSON that is a valid something-but-not-an-object
/// (<c>null</c>, <c>true</c>, <c>12</c>, <c>[]</c>), the file that says plenty about its pools and its metrics and
/// nothing about the three verdicts, the state word whose case Azure does not write the way the code compares it, the
/// <c>pools</c> that is not an array and the one whose order is the order of the card, the metrics block that is an
/// object with no number in it, the exact tick <see cref="AksService.IsOld" /> turns on, the eleven parts of the
/// constructor in the order they are declared, the equality of two services - which reaches the pools as the list it
/// was handed rather than as a sequence of pools - and the one line a failing test prints about it.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is - the type has no member that does either: every
/// moment is one the test brings, so the boundaries of <see cref="AksService.OldAfter" /> can be stepped across a
/// tick at a time. The internal <c>Is</c> the comparisons go through is internal to the app, which declares no
/// <c>InternalsVisibleTo</c> (as <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>), so it is pinned through
/// <see cref="AksService.IsStopped" />, the public member that is its only use in this type. The constants of the file
/// are held to their value too: today they are pinned only through the words of another type's sentence, which is a
/// long way round for a word Azure writes into the file.
/// </para>
/// </summary>
public class AksServiceEdgeCaseTests
{
    /// <summary>The moment a file says Azure was read; every age below is counted from it.</summary>
    private static readonly DateTimeOffset Generated = new(2026, 10, 6, 20, 10, 4, TimeSpan.Zero);

    /// <summary>The service with nothing in it, built by hand: the record's own zero, which no file is allowed to be.</summary>
    private static AksService None => new(null, null, null, null, null, null, null, null, null, [], null);

    /// <summary>A file that says one true thing about the service and names one pool: enough for the reader, and the
    /// same text twice over is two readings of it.</summary>
    private const string Twice = """{ "powerState": "Stopped", "pools": [ { "name": "system" } ] }""";

    // ------------------------------------------------------------------------------ the file

    /// <summary>
    /// A file that is not there is one problem said one way, whether the reader was handed <see langword="null" />,
    /// the empty string (which the parser already pins) or a blank of whitespace alone - the body of an answer that
    /// came back with nothing in it.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n  ")]
    public void AFileThatIsBlankIsBlankInEveryWay(string? json)
    {
        var parsed = AksService.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file is empty", parsed.Problem);
    }

    /// <summary>Every JSON that parses and is not an object is refused in the same words: the top-level null and
    /// true, two numbers, an empty array and an array of one object - an answer of the shape the file needs, put
    /// where the file does not have it.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("12")]
    [InlineData("1.5")]
    [InlineData("[]")]
    [InlineData("""[ { "powerState": "Stopped" } ]""")]
    public void AnythingThatIsNotAnObjectIsNamed(string json)
    {
        var parsed = AksService.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file does not contain a JSON object", parsed.Problem);
    }

    /// <summary>An answer that is not JSON at all - a truncated object, an object with nothing but a comma in it,
    /// or an object followed by another opening brace - is the JSON problem, never an exception on its way out.</summary>
    [Theory]
    [InlineData("""{ "powerState": """)]
    [InlineData("""{,}""")]
    [InlineData("""{ "powerState": "Stopped" } }""")]
    public void AFileThatIsNotJsonAtAllIsNamed(string json)
    {
        var parsed = AksService.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file is not valid JSON", parsed.Problem);
    }

    /// <summary>The whitespace a file is allowed to sit in: the JSON inside it is read as it would be with nothing
    /// around it, which is what a workflow that writes the object with an editor or a heredoc produces.</summary>
    [Fact]
    public void TheFileIsReadThroughTheWhitespaceAroundIt()
    {
        var service = Read("\t\n { \"powerState\": \"Stopped\" } \r\n");

        Assert.Equal("Stopped", service.PowerState);
        Assert.True(service.IsStopped);
        Assert.Empty(service.Pools);
        Assert.Null(service.Metrics);
        Assert.Null(service.Generated);
    }

    // ---------------------------------------------------------------------------- the verdicts

    /// <summary>Either of the two verdicts no other test tries on its own is enough by itself: an availability
    /// section with a state, and a provisioning state with nothing else in the file. Both leave the rest of the
    /// record empty rather than inventing it.</summary>
    [Fact]
    public void OneVerdictAloneIsEnoughForTheFileToSaySomething()
    {
        var availability = Read("""{ "availability": { "state": "Unavailable", "summary": "The API server does not answer." } }""");

        Assert.Equal(new AksAvailability("Unavailable", "The API server does not answer.", null, null), availability.Availability);
        Assert.Null(availability.PowerState);
        Assert.Null(availability.ProvisioningState);
        Assert.False(availability.IsStopped);

        var provisioning = Read("""{ "provisioningState": "Updating" }""");

        Assert.Equal("Updating", provisioning.ProvisioningState);
        Assert.Null(provisioning.Availability);
        Assert.Null(provisioning.PowerState);
        Assert.Null(provisioning.Metrics);
    }

    /// <summary>What counts as a verdict, and what does not, to the letter: a section that is not an object, a state
    /// that is blank or absent, and everything else the file may hold - its moment, its names, its versions, a pool
    /// and a window of metrics - still add up to nothing said about the service.</summary>
    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "availability": {} }""")]
    [InlineData("""{ "availability": { "state": "" } }""")]
    [InlineData("""{ "availability": { "state": "   " } }""")]
    [InlineData("""{ "availability": { "state": null } }""")]
    [InlineData("""{ "availability": "Available" }""")]
    [InlineData("""{ "availability": [] }""")]
    [InlineData("""{ "availability": null }""")]
    [InlineData("""{ "powerState": null }""")]
    [InlineData("""{ "provisioningState": "" }""")]
    [InlineData("""{ "generated": "2026-10-06T20:10:04Z", "name": "aks-cmdemo3", "location": "southcentralus", "kubernetesVersion": "1.33.3", "tier": "Free", "pools": [ { "name": "system" } ], "metrics": { "windowMinutes": 15 } }""")]
    public void EverythingExceptTheThreeVerdictsIsNoVerdict(string json)
    {
        var parsed = AksService.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal(
            "the file says nothing about the service: no availability, no powerState and no provisioningState",
            parsed.Problem);
    }

    /// <summary>The state words are trimmed and compared without regard to case, but the value the record keeps is
    /// the word as Azure wrote it, spaces gone: the page prints what it was told, the rules decide on what it means.
    /// The last file is the same facts with a state that only looks like the one the record stops on.</summary>
    [Fact]
    public void TheWordsAreTrimmedAndJudgedWithoutRegardToTheirCase()
    {
        var service = Read("""{ "powerState": "  StOpPeD  ", "provisioningState": "SuCcEeDeD", "availability": { "state": " Available " } }""");

        Assert.Equal("StOpPeD", service.PowerState);
        Assert.True(service.IsStopped);
        Assert.Equal("SuCcEeDeD", service.ProvisioningState);
        Assert.Equal("Available", service.Availability!.State);

        var near = Read("""{ "powerState": "NotStopped" }""");

        Assert.Equal("NotStopped", near.PowerState);
        Assert.False(near.IsStopped);
    }

    /// <summary>Only the word <c>Stopped</c>, in any case, is stopped; every other word is not - the empty one, the
    /// absent one, and the one that merely begins with the word. The record compares what it was handed and trims
    /// nothing itself (the spaces a file wraps a value in are gone before the record sees it, which the test above
    /// pins), and the comparison is the internal <c>Is</c>, reached through the one public member of this type that
    /// calls it.</summary>
    [Theory]
    [InlineData("Stopped", true)]
    [InlineData("stopped", true)]
    [InlineData("STOPPED", true)]
    [InlineData("Running", false)]
    [InlineData("running", false)]
    [InlineData("Deallocated", false)]
    [InlineData("Stopped ", false)]
    [InlineData("stopp", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void OnlyTheWordStoppedIsStopped(string? powerState, bool expected) =>
        Assert.Equal(expected, (None with { PowerState = powerState }).IsStopped);

    /// <summary>Only the word <c>Running</c>, in any case, is running - the question the events ask of a power state
    /// that changed - and the record answers it itself rather than lending its comparison out.</summary>
    [Theory]
    [InlineData("Running", true)]
    [InlineData("running", true)]
    [InlineData("RUNNING", true)]
    [InlineData("Stopped", false)]
    [InlineData("Starting", false)]
    [InlineData("Running ", false)]
    [InlineData("Runn", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void OnlyTheWordRunningIsRunning(string? powerState, bool expected) =>
        Assert.Equal(expected, (None with { PowerState = powerState }).IsRunning);

    /// <summary>Only the word <c>Succeeded</c>, in any case, is provisioned; what Azure is still doing to the
    /// service, a failure, and a provisioning state the file does not have are not.</summary>
    [Theory]
    [InlineData("Succeeded", true)]
    [InlineData("succeeded", true)]
    [InlineData("SUCCEEDED", true)]
    [InlineData("Updating", false)]
    [InlineData("Starting", false)]
    [InlineData("Stopping", false)]
    [InlineData("Failed", false)]
    [InlineData("Succeeded ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void OnlyTheWordSucceededIsProvisioned(string? provisioningState, bool expected) =>
        Assert.Equal(expected, (None with { ProvisioningState = provisioningState }).IsProvisioned);

    /// <summary>Each query reads its own part and no other: a running service may be still provisioning, a stopped
    /// one may be provisioned, and running and stopped are never both true.</summary>
    [Fact]
    public void EachQueryReadsItsOwnPart()
    {
        var updating = None with { PowerState = "Running", ProvisioningState = "Updating" };

        Assert.True(updating.IsRunning);
        Assert.False(updating.IsStopped);
        Assert.False(updating.IsProvisioned);

        var stopped = None with { PowerState = "Stopped", ProvisioningState = "Succeeded" };

        Assert.False(stopped.IsRunning);
        Assert.True(stopped.IsStopped);
        Assert.True(stopped.IsProvisioned);

        Assert.True(ClusterFixture.SampleService.IsRunning);
        Assert.True(ClusterFixture.SampleService.IsProvisioned);
    }

    /// <summary>Which part of the service says "stopped": the power state and nothing else, whatever the rest of it
    /// claims. A stopped cluster Azure still calls available is stopped, and a running one whose provisioning failed
    /// is not.</summary>
    [Fact]
    public void ThePowerStateIsTheOnlyPartThatSaysStopped()
    {
        Assert.True(
            (None with
            {
                PowerState = "Stopped",
                ProvisioningState = "Deleting",
                Availability = new AksAvailability("Available", null, null, null),
                Metrics = new AksMetrics(15, 0, 0, 0),
            }).IsStopped);
        Assert.False(
            (None with
            {
                PowerState = "Running",
                ProvisioningState = "Failed",
                KubernetesVersion = "1.33.3",
                Availability = new AksAvailability("Unavailable", null, null, null),
            }).IsStopped);
    }

    // ------------------------------------------------------------------------------- the pools

    /// <summary>The pool list is a list of nothing, and never <see langword="null" />, when the file has no pools: no
    /// <c>pools</c> at all, an empty array, a value of another type under the name, and an array whose elements are
    /// none of them a pool - an object with no name, text, a number, a name of spaces, a name that is
    /// <see langword="null" />.</summary>
    [Theory]
    [InlineData("""{ "powerState": "Running" }""")]
    [InlineData("""{ "powerState": "Running", "pools": [] }""")]
    [InlineData("""{ "powerState": "Running", "pools": "none" }""")]
    [InlineData("""{ "powerState": "Running", "pools": {} }""")]
    [InlineData("""{ "powerState": "Running", "pools": null }""")]
    [InlineData("""{ "powerState": "Running", "pools": [ {}, "text", 3, { "name": " " }, { "name": null } ] }""")]
    public void ThePoolsAreNoPoolsInAnyOfThoseWays(string json) => Assert.Empty(Read(json).Pools);

    /// <summary>More than one pool is the case the sample file never shows: they come out in the order of the file -
    /// the order the card lists them in - the nameless ones walked past without shifting the rest, and a pool that
    /// says only its name is a pool with nothing else said about it rather than one with a zero count.</summary>
    [Fact]
    public void ThePoolsKeepTheOrderOfTheFileAndTheNamelessAreWalkedPast()
    {
        var service = Read("""
            { "powerState": "Running",
              "pools": [ { "name": "user", "mode": "User" }, { "count": 4 }, "text", { "name": " " }, { "name": "system", "count": 1 } ] }
            """);

        Assert.Equal(2, service.Pools.Count);
        Assert.Equal(["user", "system"], service.Pools.Select(pool => pool.Name));
        Assert.Equal(new AksPool("user", "User", null, null, null, null, null, null), service.Pools[0]);
        Assert.Equal(new AksPool("system", null, 1, null, null, null, null, null), service.Pools[1]);
    }

    // ---------------------------------------------------------------------------- the metrics

    /// <summary>A metrics block that is not an object is no block at all: the card leaves the block out rather than
    /// drawing it empty. An object under the name, however bare, is a block - and a block of nothing is a block
    /// without numbers, which is the case <see cref="AksMetrics.HasAny" /> answers.</summary>
    [Theory]
    [InlineData("""{ "powerState": "Running", "metrics": 15 }""")]
    [InlineData("""{ "powerState": "Running", "metrics": [] }""")]
    [InlineData("""{ "powerState": "Running", "metrics": "the last 15 minutes" }""")]
    [InlineData("""{ "powerState": "Running", "metrics": null }""")]
    public void AMetricsBlockThatIsNotAnObjectIsNoBlock(string json)
    {
        var service = Read(json);

        Assert.Null(service.Metrics);
    }

    /// <summary>The block that is there and says nothing: an empty object, a block whose only number is the window it
    /// averages over, and a percent too large to be a number at all. None of them is a number of the block, so none
    /// of them makes the card draw one.</summary>
    [Fact]
    public void AWindowIsNotANumberOfTheBlock()
    {
        var bare = Read("""{ "powerState": "Running", "metrics": {} }""");
        var window = Read("""{ "powerState": "Running", "metrics": { "windowMinutes": 15 } }""");
        var tooBig = Read("""{ "powerState": "Running", "metrics": { "nodeCpuPercent": 1e999 } }""");
        var largest = Read("""{ "powerState": "Running", "metrics": { "nodeCpuPercent": 1e308 } }""");

        Assert.Equal(new AksMetrics(null, null, null, null), bare.Metrics);
        Assert.False(bare.Metrics!.HasAny);

        Assert.Equal(new AksMetrics(15, null, null, null), window.Metrics);
        Assert.False(window.Metrics!.HasAny);

        Assert.Null(tooBig.Metrics!.NodeCpuPercent);
        Assert.False(tooBig.Metrics.HasAny);

        Assert.Equal(1e308, largest.Metrics!.NodeCpuPercent!.Value);
        Assert.True(largest.Metrics.HasAny);
    }

    /// <summary>Which of the five numbers is enough for the block to be shown: every one of them alone is, the last
    /// two - the API server's - included, and zero is a number the card draws. Only the window minutes, and nothing
    /// at all, is not: and a number the reader put there by hand, however impossible, is still a number, because it
    /// is the file that filters what Azure emits, not the block.</summary>
    [Fact]
    public void AnyOneOfTheFiveNumbersIsEnoughForTheBlockToBeShown()
    {
        Assert.True(new AksMetrics(null, 21.4, null, null, null, null).HasAny);
        Assert.True(new AksMetrics(null, null, 63, null, null, null).HasAny);
        Assert.True(new AksMetrics(null, null, null, 18.2, null, null).HasAny);
        Assert.True(new AksMetrics(null, null, null, null, 4.6, null).HasAny);
        Assert.True(new AksMetrics(null, null, null, null, null, 31.5).HasAny);
        Assert.True(new AksMetrics(15, 0, 0, 0, 0, 0).HasAny);

        Assert.False(new AksMetrics(15, null, null, null).HasAny);
        Assert.False(new AksMetrics(null, null, null, null, null, null).HasAny);

        Assert.True(new AksMetrics(null, -1, null, null).HasAny);
    }

    // ---------------------------------------------------------------------------- the two rules

    /// <summary>The age from which the facts are old, to the minute and to the tick: ninety minutes, a threshold the
    /// page's own words are already pinned against at 45, 90 and 100 minutes. The comparison is strictly greater, so
    /// the point that turns it is one tick past ninety minutes and the tick before it is not old; a file whose moment
    /// has not come yet - two clocks that disagree - is no age at all; and it is the instant that is compared, not
    /// the offset the moment is written with.</summary>
    [Fact]
    public void FactsAreOldAfterNinetyMinutesAndNotOneTickBefore()
    {
        Assert.Equal(TimeSpan.FromMinutes(90), AksService.OldAfter);

        var service = None with { Generated = Generated };

        Assert.False(service.IsOld(Generated));
        Assert.False(service.IsOld(Generated - TimeSpan.FromHours(3)));
        Assert.False(service.IsOld(Generated + AksService.OldAfter - TimeSpan.FromTicks(1)));
        Assert.False(service.IsOld(Generated + AksService.OldAfter));
        Assert.True(service.IsOld(Generated + AksService.OldAfter + TimeSpan.FromTicks(1)));
        Assert.True(service.IsOld(Generated + TimeSpan.FromDays(1)));

        // The very same instant written two hours further east: no age, because no time has passed.
        Assert.False(service.IsOld(new DateTimeOffset(2026, 10, 6, 22, 10, 4, TimeSpan.FromHours(2))));
        Assert.True(service.IsOld(new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero)));
    }

    /// <summary>A file that says when it was read keeps that moment with the offset it was written with, and one that
    /// does not is not old by any clock: the age of a file that does not say is nothing the page may claim.</summary>
    [Fact]
    public void AFileThatDoesNotSayWhenItWasReadIsNeverOld()
    {
        var service = Read("""{ "powerState": "Running", "generated": " 2026-10-06T20:10:04+02:00 " }""");
        var silent = Read("""{ "powerState": "Running", "generated": "yesterday" }""");

        Assert.Equal(new DateTimeOffset(2026, 10, 6, 20, 10, 4, TimeSpan.FromHours(2)), service.Generated);
        Assert.Equal(TimeSpan.FromHours(2), service.Generated!.Value.Offset);
        Assert.True(service.IsOld(service.Generated!.Value.AddMinutes(91)));

        Assert.Null(silent.Generated);
        Assert.False(silent.IsOld(Generated + TimeSpan.FromDays(7)));
        Assert.False(None.IsOld(DateTimeOffset.UnixEpoch));
    }

    /// <summary>The three state words this type compares against, and the four verdicts of Resource Health, are
    /// Azure's own words: pinned to their value, and shown to be the value the sample file really carries.</summary>
    [Fact]
    public void TheWordsInTheCodeAreTheWordsAzureWrites()
    {
        Assert.Equal("Running", AksService.Running);
        Assert.Equal("Stopped", AksService.Stopped);
        Assert.Equal("Succeeded", AksService.Succeeded);
        Assert.NotEqual(AksService.Running, AksService.Stopped);

        Assert.Equal("Available", AksAvailability.Available);
        Assert.Equal("Unavailable", AksAvailability.Unavailable);
        Assert.Equal("Degraded", AksAvailability.Degraded);
        Assert.Equal("Unknown", AksAvailability.Unknown);

        var service = ClusterFixture.SampleService;

        Assert.Equal(AksService.Running, service.PowerState);
        Assert.Equal(AksService.Succeeded, service.ProvisioningState);
        Assert.Equal(AksAvailability.Available, service.Availability!.State);
    }

    // --------------------------------------------------------------------------- the record

    /// <summary>The constructor's eleven parts, in the order the record declares them, each come back as the property
    /// of the same name - the order <see cref="Parse" /> relies on when it hands the file over, position by
    /// position.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var availability = new AksAvailability("Degraded", "Azure is repairing the node.", "Update scheduled", Generated);
        AksPool[] pools = [new AksPool("system", "System", 1, "Standard_D4as_v6", 128, "Running", "Succeeded", "1.33.3")];
        var metrics = new AksMetrics(15, 21.4, 63, 18.2, 4.6, 31.5);

        var service = new AksService(
            Generated,
            "aks-cmdemo3",
            "rg-cmdemo3-cluster",
            "southcentralus",
            availability,
            "Running",
            "Succeeded",
            "1.33.3",
            "Free",
            pools,
            metrics);

        Assert.Equal(Generated, service.Generated);
        Assert.Equal("aks-cmdemo3", service.Name);
        Assert.Equal("rg-cmdemo3-cluster", service.ResourceGroup);
        Assert.Equal("southcentralus", service.Location);
        Assert.Equal(availability, service.Availability);
        Assert.Equal("Running", service.PowerState);
        Assert.Equal("Succeeded", service.ProvisioningState);
        Assert.Equal("1.33.3", service.KubernetesVersion);
        Assert.Equal("Free", service.Tier);
        Assert.Equal(pools, service.Pools);
        Assert.Equal(metrics, service.Metrics);
        Assert.False(service.IsStopped);
    }

    /// <summary>The record with nothing in it is still a record: eleven parts that are null, a list of no pools, no
    /// metrics block at all, and both of its rules answering <see langword="no" /> to whatever they are asked.</summary>
    [Fact]
    public void AServiceOfNothingAtAllIsStillAService()
    {
        var none = None;

        Assert.Null(none.Generated);
        Assert.Null(none.Name);
        Assert.Null(none.ResourceGroup);
        Assert.Null(none.Location);
        Assert.Null(none.Availability);
        Assert.Null(none.PowerState);
        Assert.Null(none.ProvisioningState);
        Assert.Null(none.KubernetesVersion);
        Assert.Null(none.Tier);
        Assert.Empty(none.Pools);
        Assert.Null(none.Metrics);

        Assert.False(none.IsStopped);
        Assert.False(none.IsRunning);
        Assert.False(none.IsProvisioned);
        Assert.False(none.IsOld(Generated));
        Assert.False(none.IsOld(DateTimeOffset.UnixEpoch));
    }

    /// <summary>
    /// The service is its eleven parts, each compared as the part it is - the availability, the metrics and every
    /// pool by their values, since they are records too - and the list of pools as the object it is. So two services
    /// built round one and the same list are one service and hash alike, a part copied unchanged changes nothing,
    /// and each of the eleven parts on its own is enough to tell two apart. What does <em>not</em> come out as one
    /// service is the same file read twice, or a list copied element for element: the pools of a service are the
    /// list the reader was handed, and two readings hold two of them. Nothing asks a service whether it is the
    /// service before it - <see cref="AksService.IsStopped" /> and <see cref="AksService.IsOld" /> each weigh one
    /// reading on its own - so this says which equality a reader may rely on rather than promising one the record
    /// does not have.
    /// </summary>
    [Fact]
    public void TwoServicesAreOneServiceWhenEveryPartAndTheirListAre()
    {
        AksPool[] pools = [new AksPool("system", "System", 1, "Standard_D4as_v6", 128, "Running", "Succeeded", "1.33.3")];
        var one = Built(pools);

        Assert.Equal(one, Built(pools));
        Assert.True(one.Equals(Built(pools)));
        Assert.True(one == Built(pools));
        Assert.False(one != Built(pools));
        Assert.Equal(one.GetHashCode(), Built(pools).GetHashCode());
        Assert.Equal(one, one with { Name = one.Name, Tier = one.Tier });
        Assert.False(one.Equals(null));

        AksService[] different =
        [
            one with { Generated = null },
            one with { Name = null },
            one with { ResourceGroup = null },
            one with { Location = null },
            one with { Availability = null },
            one with { PowerState = null },
            one with { ProvisioningState = null },
            one with { KubernetesVersion = null },
            one with { Tier = null },
            one with { Pools = [] },
            one with { Pools = [new AksPool("user", "User", 2, "Standard_D2s_v5", 60, "Running", "Succeeded", "1.33.3")] },
            one with { Metrics = null },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.True(one != other);
        }

        // The pools are the list they are: a copy of them, element for element, is another list and another service.
        Assert.False(one.Equals(Built([.. pools])));
        Assert.False(one.Equals(Built([new AksPool("system", "System", 1, "Standard_D4as_v6", 128, "Running", "Succeeded", "1.33.3")])));

        // Two readings of one file are two services, and two services with no pools at all are one: the empty list
        // the reader hands over is one and the same empty array.
        Assert.NotEqual(Read(Twice), Read(Twice));
        Assert.Equal(None, None);
        Assert.Equal(None.GetHashCode(), None.GetHashCode());
    }

    /// <summary>The one line the record says about itself - the line a failing test prints - names the type, then its
    /// parts in the order they are declared, and says nothing where a part is nothing: a reader comparing two of the
    /// lines can see which part moved.</summary>
    [Fact]
    public void TheRecordSaysWhatItIsInOneLineOfItsOwnParts()
    {
        var text = (None with { Name = "aks-cmdemo3", PowerState = "Stopped", Tier = "Free" }).ToString();

        Assert.StartsWith("AksService { ", text, StringComparison.Ordinal);
        Assert.Contains("Name = aks-cmdemo3", text, StringComparison.Ordinal);
        Assert.Contains("PowerState = Stopped", text, StringComparison.Ordinal);
        Assert.Contains("Tier = Free", text, StringComparison.Ordinal);
        Assert.Contains("IsStopped = True", text, StringComparison.Ordinal);
        Assert.Contains("Availability = ,", text, StringComparison.Ordinal);

        // The order the parts are declared in is the order they are said in.
        Assert.True(
            text.IndexOf("Name =", StringComparison.Ordinal) < text.IndexOf("ResourceGroup =", StringComparison.Ordinal),
            text);
        Assert.True(
            text.IndexOf("PowerState =", StringComparison.Ordinal) < text.IndexOf("Metrics =", StringComparison.Ordinal),
            text);
    }

    // --------------------------------------------------------------------------------- helpers

    /// <summary>A whole service round the pools it is handed, so that two of them can be given one and the same list.</summary>
    private static AksService Built(IReadOnlyList<AksPool> pools) =>
        new(
            Generated,
            "aks-cmdemo3",
            "rg-cmdemo3-cluster",
            "southcentralus",
            new AksAvailability("Available", null, null, Generated),
            "Running",
            "Succeeded",
            "1.33.3",
            "Free",
            pools,
            new AksMetrics(15, 21.4, 63, 18.2));

    /// <summary>The service a file holds. A file the reader answers with a problem fails here with that problem to
    /// read, so the tests below say what the service is rather than that there is one.</summary>
    private static AksService Read(string json)
    {
        var parsed = AksService.Parse(json);

        Assert.NotNull(parsed.Value);
        Assert.Null(parsed.Problem);

        return parsed.Value!;
    }
}
