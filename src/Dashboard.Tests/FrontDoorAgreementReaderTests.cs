using System.Reflection;

using static Dashboard.Tests.HealthStates;

namespace Dashboard.Tests;

/// <summary>
/// The two readers of <see cref="FrontDoorAgreement" /> that say their own words beside
/// <see cref="ServingAssessment.FrontDoorText"/> (#988): the third line of the runtime's Front Door tile
/// (<c>RuntimePayloadBuilder</c>'s Front Door tile) and the disagreement flag of the deployable section
/// (<c>DeployableSection.razor</c>), which settles <c>var disagrees = assessment.FrontDoor == FrontDoorAgreement.Disagrees;</c>
/// and marks the section with it.
/// <para>
/// <see cref="FrontDoorAgreementEdgeCaseTests"/> pins the four members and the sentences the assessment makes of them,
/// and <see cref="RuntimeTileCharacterizationServingTests"/> pins the tile line of the three agreements a round of
/// checks reaches. What is left open, and pinned here: the tile line of every one of the four members, with the two the
/// tile's switch reaches by its one fallback arm (<c>NotPresent</c> and <c>Pending</c>) said apart on the same line -
/// <c>NotPresent</c> is stood on by a record the decision never produces beside a Front Door target, the only way the
/// arm can be read at all, since a deployable without an endpoint has no Front Door tile to wear the arm on; the tile's
/// note being the <c>FrontDoorText</c> the assessment says, for every one of the four; the disagreement and the wait the
/// built payload reaches when a healthy primary stands behind an endpoint that does not answer or has not been asked;
/// and the section's flag and its markup - the one member the page singles out, the class and the unhealthy mark that
/// follow it, and the stylesheet rules that make the class a visible mark.
/// </para>
/// </summary>
public class FrontDoorAgreementReaderTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private const string FrontDoorUrl = "https://fd-uat.example.net";

    private const string Topology = """
        { "environments": [ { "name": "uat", "deployables": [ {
            "name": "ui", "frontDoor": "https://fd-uat.example.net",
            "nodes": [
              { "name": "uat-west", "region": "westus3", "role": "primary", "url": "https://uat-west.example.net" },
              { "name": "uat-east", "region": "eastus2", "role": "standby", "url": "https://uat-east.example.net" } ] } ] } ] }
        """;

    /// <summary>uat with its "ui": the Front Door endpoint in front of a primary in westus3 and its standby in eastus2.</summary>
    private static EnvironmentStatus Uat() => new(TopologyParser.Parse(Topology).Topology!.Environments[0]);

    /// <summary>The two nodes, in the order of the topology, that leave a healthy primary behind the endpoint.</summary>
    private static NodeHealth[] Nodes => [NodeHealths.Primary(Healthy, "westus3"), NodeHealths.Standby(Healthy, "eastus2")];

    /// <summary>The Front Door node the manifest draws in front of the two apps.</summary>
    private static readonly RuntimeNode Node = new("fd_ui", RuntimeNodeKind.FrontDoor, "fd-uat", new Uri(FrontDoorUrl), "ui");

    /// <summary>The one manifest the built payloads below draw with: the Front Door and the two web apps of "ui".</summary>
    private static readonly RuntimeManifest Manifest = Manifests.Of(
        "uat",
        [
            ManifestNodes.FrontDoor("fd_ui", "fd-uat", FrontDoorUrl, "ui"),
            ManifestNodes.WebApp("app_ui_west", "uat-west", "https://uat-west.example.net", "ui", "primary", "westus3", "region_west"),
            ManifestNodes.WebApp("app_ui_east", "uat-east", "https://uat-east.example.net", "ui", "standby", "eastus2", "region_east"),
        ],
        [
            ManifestEdges.Between("fd_ui", "app_ui_west", RuntimeEdgeKind.Origin, 1),
            ManifestEdges.Between("fd_ui", "app_ui_east", RuntimeEdgeKind.Origin, 2),
        ],
        [
            new RuntimeRegion("region_west", "westus3", ["primary"]),
            new RuntimeRegion("region_east", "eastus2", ["standby"]),
        ]);

    /// <summary>The assessment each member stands on: the three the decision reaches from the healthy primary behind
    /// the endpoint it names, and <see cref="FrontDoorAgreement.NotPresent" /> by hand - a record the decision never
    /// produces beside a Front Door target, which is the only way the tile's fallback arm can be stood on for it.</summary>
    private static ServingAssessment AssessmentOf(FrontDoorAgreement agreement) => agreement switch
    {
        FrontDoorAgreement.Agrees => ServingAssessment.Assess(Nodes, Healthy),
        FrontDoorAgreement.Disagrees => ServingAssessment.Assess(Nodes, Unreachable),
        FrontDoorAgreement.Pending => ServingAssessment.Assess(Nodes, Pending),
        _ => new ServingAssessment(ServingState.Primary, NodeHealths.Primary(Healthy, "westus3"), null, null),
    };

    // The builder's Front Door tile is private, and its entry with it: the only road to the arm the public build
    // never reaches runs through them, stood on the way the repo's other surface tests stand on private members.
    private static readonly MethodInfo FrontDoorTile = typeof(RuntimePayloadBuilder).GetMethod("FrontDoorTile", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly Type Entry = typeof(RuntimePayloadBuilder).GetNestedType("Entry", BindingFlags.NonPublic)!;

    /// <summary>The tile the builder draws for the Front Door node, stood on the deployable and the assessment given:
    /// the endpoint not checked yet, so the tile's words are the agreement's and nothing else of the round's.</summary>
    private static RuntimeTile TileOf(FrontDoorAgreement agreement)
    {
        var uat = Uat();
        var ui = uat.Deployables[0];
        var entry = Activator.CreateInstance(Entry, [ui, ui.FrontDoor!, AssessmentOf(agreement), false])!;
        return (RuntimeTile)FrontDoorTile.Invoke(null, [Node, entry, TimeZoneInfo.Utc])!;
    }

    private static (string Text, string Tone)[] Words(RuntimeTile tile) => [.. tile.Lines.Select(line => (line.Text, line.Tone))];

    // ------------------------------------------------------------ the tile line of each of the four members

    public static TheoryData<FrontDoorAgreement, string, string> TileLines => new()
    {
        { FrontDoorAgreement.NotPresent, "being compared with the web apps", RuntimeLineTone.Muted },
        { FrontDoorAgreement.Pending, "being compared with the web apps", RuntimeLineTone.Muted },
        { FrontDoorAgreement.Agrees, "agrees with the web apps", RuntimeLineTone.Ok },
        { FrontDoorAgreement.Disagrees, "disagrees with the web apps", RuntimeLineTone.Warn },
    };

    /// <summary>The third line of the Front Door tile says each of the four agreements in the tile's words: the two
    /// the switch reaches by its named arms in their own sentence, and the two it reaches by its one fallback arm in
    /// the wait's. The whole tile is pinned, so a line cannot be moved or reordered without the arm moving with it.</summary>
    [Theory]
    [MemberData(nameof(TileLines))]
    public void TheFrontDoorTileSaysEachAgreementInItsOwnLine(FrontDoorAgreement agreement, string line, string tone)
    {
        var tile = TileOf(agreement);

        Assert.Equal(agreement, AssessmentOf(agreement).FrontDoor);
        Assert.Equal(
            [("version not known", RuntimeLineTone.Muted), ("routes to westus3 (priority 1)", RuntimeLineTone.Plain), (line, tone)],
            Words(tile));
    }

    /// <summary>The two agreements the tile's switch reaches by its one fallback arm are said apart on the same line:
    /// <see cref="FrontDoorAgreement.NotPresent" />, the endpoint the deployable has none of, stood on by a record the
    /// decision never produces, and <see cref="FrontDoorAgreement.Pending" />, the endpoint still being checked, which
    /// a round without an answer leaves behind. Each member is stood on for itself, each lands on the same line, and
    /// the members are apart in the note the line carries - so a change to one arm's words moves the other's with it.</summary>
    [Fact]
    public void TheTwoAgreementsTheSwitchReachesByItsFallbackArmAreSaidApartOnTheSameLine()
    {
        var notPresent = TileOf(FrontDoorAgreement.NotPresent);
        var pending = TileOf(FrontDoorAgreement.Pending);

        Assert.Equal(new RuntimeTileLine("being compared with the web apps", RuntimeLineTone.Muted), notPresent.Lines[^1]);
        Assert.Equal(notPresent.Lines[^1], pending.Lines[^1]);

        Assert.Equal(FrontDoorAgreement.NotPresent, AssessmentOf(FrontDoorAgreement.NotPresent).FrontDoor);
        Assert.Equal(FrontDoorAgreement.Pending, AssessmentOf(FrontDoorAgreement.Pending).FrontDoor);
        Assert.NotEqual(AssessmentOf(FrontDoorAgreement.NotPresent), AssessmentOf(FrontDoorAgreement.Pending));

        // The wait's words are the note of one of the two and of none of the other.
        Assert.DoesNotContain("Front Door", notPresent.Title, StringComparison.Ordinal);
        Assert.EndsWith("\nFront Door is being checked.", pending.Title, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- the note is the assessment's words

    public static TheoryData<FrontDoorAgreement, string?> Notes => new()
    {
        { FrontDoorAgreement.NotPresent, null },
        { FrontDoorAgreement.Pending, "Front Door is being checked." },
        { FrontDoorAgreement.Agrees, "Front Door agrees: it is healthy." },
        { FrontDoorAgreement.Disagrees, "Front Door disagrees: it is unreachable although westus3 is healthy." },
    };

    /// <summary>The note of the tile is the <c>FrontDoorText</c> the assessment says, for every one of the four: the
    /// sentence the assessment makes of its member, as the last line of the tile's tooltip, and no note at all where
    /// the assessment says none - the absent endpoint the page says nothing about.</summary>
    [Theory]
    [MemberData(nameof(Notes))]
    public void TheFrontDoorTilesNoteIsTheFrontDoorTextTheAssessmentSays(FrontDoorAgreement agreement, string? note)
    {
        var assessment = AssessmentOf(agreement);
        var tile = TileOf(agreement);

        Assert.Equal(note, assessment.FrontDoorText);
        if (note is { } words)
        {
            Assert.EndsWith($"\n{words}", tile.Title, StringComparison.Ordinal);
        }
        else
        {
            Assert.DoesNotContain("Front Door", tile.Title, StringComparison.Ordinal);
        }
    }

    // ------------------------------------------------- the same states, through the builder's public road

    /// <summary>The disagreement the section marks, reached the road the page reaches it by: a healthy primary and a
    /// healthy standby behind an endpoint that does not answer, at the fixed moment the test brings. The built payload
    /// says it in the tile's words, with the assessment's sentence as the tile's note.</summary>
    [Fact]
    public void AHealthyPrimaryBehindAnUnreachableEndpointIsADisagreementInTheBuiltPayload()
    {
        var uat = Uat();
        var ui = uat.Deployables[0];
        ui.FrontDoor!.Record(ProbeResults.Of(Unreachable, null, Now));
        ui.Nodes[0].Record(ProbeResults.Of(Healthy, "2.4.21", Now));
        ui.Nodes[1].Record(ProbeResults.Of(Healthy, "2.4.21", Now));

        var tile = RuntimePayloadBuilder.Build(Manifest, uat, null, TimeZoneInfo.Utc).Nodes.Single(node => node.Alias == "fd_ui");

        Assert.Equal(new RuntimeTileLine("disagrees with the web apps", RuntimeLineTone.Warn), tile.Lines[^1]);
        Assert.EndsWith("\nFront Door disagrees: it is unreachable although westus3 is healthy.", tile.Title, StringComparison.Ordinal);
    }

    /// <summary>The same healthy primary with the endpoint not asked yet: the wait the fallback arm stands for, in the
    /// built payload, with the wait's sentence as the note.</summary>
    [Fact]
    public void AHealthyPrimaryBehindAnEndpointNotCheckedYetIsBeingComparedInTheBuiltPayload()
    {
        var uat = Uat();
        var ui = uat.Deployables[0];
        ui.Nodes[0].Record(ProbeResults.Of(Healthy, "2.4.21", Now));
        ui.Nodes[1].Record(ProbeResults.Of(Healthy, "2.4.21", Now));

        var tile = RuntimePayloadBuilder.Build(Manifest, uat, null, TimeZoneInfo.Utc).Nodes.Single(node => node.Alias == "fd_ui");

        Assert.Equal(new RuntimeTileLine("being compared with the web apps", RuntimeLineTone.Muted), tile.Lines[^1]);
        Assert.EndsWith("\nFront Door is being checked.", tile.Title, StringComparison.Ordinal);
    }

    // --------------------------------------------------------------- the section's flag of the disagreement

    /// <summary>The deployable section settles one flag of the assessment and its markup follows it: the flag is
    /// <c>Disagrees</c> and none of the other three - the one member the page singles out, named once in the file -
    /// the banner wears the <c>disagrees</c> class and the unhealthy mark beside the assessment's words exactly when
    /// it is, and the stylesheet makes the class a visible mark. The flag is stood on the decision's own road: the
    /// healthy primary behind the endpoint that does not answer.</summary>
    [Fact]
    public void TheSectionMarksOnlyTheAssessmentThatDisagrees()
    {
        var assessment = ServingAssessment.Assess(Nodes, Unreachable);

        Assert.Equal(FrontDoorAgreement.Disagrees, assessment.FrontDoor);
        Assert.Equal("Front Door disagrees: it is unreachable although westus3 is healthy.", assessment.FrontDoorText);

        var text = MarkupGuards.ComponentText("DeployableSection");

        // The flag settles on the one member the page singles out, and on none of the other three.
        Assert.Contains("var disagrees = assessment.FrontDoor == FrontDoorAgreement.Disagrees;", text, StringComparison.Ordinal);
        Assert.Equal(1, MarkupGuards.CountOccurrences(text, "FrontDoorAgreement"));

        // The word the flag stands on is used exactly where the markup follows it: where it is settled, where the
        // banner's class takes it - twice on the one line that wears it - and where the mark is drawn beside the
        // words.
        Assert.Equal(4, MarkupGuards.CountOccurrences(text, "disagrees"));

        // The markup follows the flag, in the order the banner reads it: the class on the serving banner, the words
        // of the assessment only when there is an endpoint to say words of, the unhealthy mark beside them only when
        // the flag is set, and the words after the mark.
        Assert.Contains("""<div class="serving serving--@serving @(disagrees ? "serving--disagrees" : null)">""", text, StringComparison.Ordinal);
        var paragraph = text.IndexOf("<p class=\"serving__frontdoor\">", StringComparison.Ordinal);
        var flag = text.IndexOf("@if (disagrees)", paragraph, StringComparison.Ordinal);
        var mark = text.IndexOf("<StateIcon State=\"HealthState.Unhealthy\" />", flag, StringComparison.Ordinal);
        var words = text.IndexOf("<span>@frontDoor</span>", mark, StringComparison.Ordinal);
        var sentence = text.IndexOf("assessment.FrontDoorText is { } frontDoor", StringComparison.Ordinal);
        Assert.True(sentence < paragraph && paragraph < flag && flag < mark && mark < words,
            $"the section's markup does not follow its flag: sentence {sentence}, paragraph {paragraph}, flag {flag}, mark {mark}, words {words}");

        // The class the section adds is a visible mark in the stylesheet, whatever the nodes say.
        var css = File.ReadAllText(MarkupGuards.SourcePath("wwwroot/css/app.css"));
        Assert.Contains(".serving--disagrees {", css, StringComparison.Ordinal);
        Assert.Contains(".serving--disagrees .serving__frontdoor {", css, StringComparison.Ordinal);
    }
}
