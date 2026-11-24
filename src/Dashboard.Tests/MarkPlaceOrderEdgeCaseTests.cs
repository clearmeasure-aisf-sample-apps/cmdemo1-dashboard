using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The order of the marks is a numbering of its own, declared in <see cref="MarkPlace" /> (internal to
/// <c>src/Dashboard/Health/DeploymentsReport.cs</c>), and these tests pin that numbering as it is declared: the
/// eight places in the order they are written, their implicit numbers 0 to 7, the boundary between the four that
/// are in flight and the four that are ended, and the two places the one state <see cref="DeploymentState.Unknown" />
/// takes. <see cref="DeploymentStateRankEdgeCaseTests" /> pins the order the marks come in through a report built by
/// hand; <see cref="DeploymentMarkOrderEdgeCaseTests" /> pins that the marks' order is not the states' order. This
/// file pins the places themselves, which those tests cannot see through <see cref="DeploymentsReport.Marks" />:
/// that nothing was inserted between them, that the unknown pair sits on its own side of the boundary as declared
/// members, and that the type's own numbering matches the order the marks come in.
/// <para>
/// The places are read through reflection over the assembly, as <see cref="SummaryLevelSurfaceEdgeCaseTests" />
/// reads the four summary words, because <see cref="MarkPlace" /> is internal and the app declares no
/// <c>InternalsVisibleTo</c>: the test cannot name the type, so it finds it by its name in the namespace where
/// <c>DeploymentsReport</c> lives.
/// </para>
/// <para>
/// Nothing here reaches the network or asks the clock: the places are a declaration, not a reading, and every
/// thing the tests check is the declaration itself.
/// </para>
/// </summary>
public class MarkPlaceOrderEdgeCaseTests
{
    /// <summary>The eight places a mark stands, in the order the declaration writes them.</summary>
    private static readonly string[] PlaceNames =
    [
        "Waiting", "Executing", "Queued", "UnknownInFlight",
        "Failed", "Canceled", "Succeeded", "UnknownEnded",
    ];

    /// <summary>The four places that are in flight, before the boundary the declaration draws.</summary>
    private static readonly string[] InFlightPlaces = ["Waiting", "Executing", "Queued", "UnknownInFlight"];

    /// <summary>The four places that are ended, after the boundary the declaration draws.</summary>
    private static readonly string[] EndedPlaces = ["Failed", "Canceled", "Succeeded", "UnknownEnded"];

    /// <summary>Finds <see cref="MarkPlace" /> in the assembly by its name in the namespace where
    /// <see cref="DeploymentsReport" /> lives: the type is internal, so it cannot be named directly, but the
    /// assembly can be scanned for it by the name it is declared with.</summary>
    private static Type PlaceType() =>
        typeof(DeploymentsReport).Assembly
            .GetTypes()
            .Single(type => type.IsEnum && type.Namespace == "Dashboard.Health" && type.Name == "MarkPlace");

    // ----- the eight places in their declared order

    /// <summary>The eight places in the order they are declared in, on the numbers 0 to 7, of an
    /// <see cref="int" /> underneath, and a choice and not a set: the enum wears no <see cref="FlagsAttribute" />
    /// and is sealed, so no place is built on and none on another. The declaration order is the whole point of the
    /// type - it is the order the marks come in - and the numbers are implicit because the order is the only thing
    /// the type says.</summary>
    [Fact]
    public void TheEightPlacesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        var type = PlaceType();

        Assert.Equal(PlaceNames, Enum.GetNames(type));
        Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7], Enum.GetValues(type).Cast<int>().ToList());
        Assert.Equal(8, Enum.GetValues(type).Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(type));
        Assert.False(type.IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each place on its own number and its own word, both ways round: the number is the one the name
    /// stands on, the word is the one that number answers to, and every one of the eight is defined however it is
    /// arrived at. The order is the declaration's, and the declaration is the page's own saying of where a mark
    /// stands.</summary>
    [Theory]
    [InlineData("Waiting", 0)]
    [InlineData("Executing", 1)]
    [InlineData("Queued", 2)]
    [InlineData("UnknownInFlight", 3)]
    [InlineData("Failed", 4)]
    [InlineData("Canceled", 5)]
    [InlineData("Succeeded", 6)]
    [InlineData("UnknownEnded", 7)]
    public void EachPlaceStandsOnItsOwnNumberAndItsOwnWord(string name, int number)
    {
        var type = PlaceType();
        var value = Enum.ToObject(type, number);

        Assert.Equal(name, Enum.GetName(type, value));
        Assert.Equal(number, (int)(object)value);
        Assert.True(Enum.IsDefined(type, value));
    }

    // ----- the boundary between in flight and ended

    /// <summary>The declaration draws a boundary between the four places that are in flight and the four that are
    /// ended: the boundary is after <c>UnknownInFlight</c> and before <c>Failed</c>, and it is drawn by the order
    /// the places are written in, not by their numbers. The comment in the declaration says so explicitly: "In
    /// flight." and "Ended." The order pins it, and the numbers follow the order.</summary>
    [Fact]
    public void TheBoundaryBetweenInFlightAndEndedIsDrawnByTheOrder()
    {
        var type = PlaceType();
        var names = Enum.GetNames(type);

        Assert.Equal(InFlightPlaces, names.Take(4).ToArray());
        Assert.Equal(EndedPlaces, names.Skip(4).ToArray());

        // The boundary is after the fourth place and before the fifth: the last in flight is 3, the first ended is 4.
        Assert.Equal(3, (int)(object)Enum.ToObject(type, 3));
        Assert.Equal("UnknownInFlight", Enum.GetName(type, Enum.ToObject(type, 3)));
        Assert.Equal("Failed", Enum.GetName(type, Enum.ToObject(type, 4)));
    }

    /// <summary>The two places the one state <see cref="DeploymentState.Unknown" /> takes are on their own sides of
    /// the boundary: <c>UnknownInFlight</c> among the four that are in flight, after the three known in-flight
    /// places, and <c>UnknownEnded</c> among the four that are ended, after the three known ended places. Nothing
    /// is inserted between the known places and the unknown one on either side - the unknown one stands where the
    /// declaration puts it, not where a number or a later edit might put it.</summary>
    [Fact]
    public void TheUnknownPairSitsOnItsOwnSideOfTheBoundary()
    {
        var type = PlaceType();
        var names = Enum.GetNames(type);

        // UnknownInFlight is the last of the in-flight places, right after Queued and right before Failed.
        Assert.Equal("UnknownInFlight", names[3]);
        Assert.Equal("Queued", names[2]);
        Assert.Equal("Failed", names[4]);

        // UnknownEnded is the last of the ended places, right after Succeeded.
        Assert.Equal("UnknownEnded", names[7]);
        Assert.Equal("Succeeded", names[6]);
    }

    /// <summary>Nothing was inserted between the declared places: the eight names are exactly the eight the
    /// declaration writes, in the order it writes them, with no gaps and no extras. A later edit that inserts a
    /// ninth place or removes one is caught here, not only in the order the marks come in through a report.</summary>
    [Fact]
    public void NothingWasInsertedBetweenTheDeclaredPlaces()
    {
        var type = PlaceType();

        Assert.Equal(8, Enum.GetNames(type).Length);
        Assert.Equal(PlaceNames, Enum.GetNames(type));
    }
}
