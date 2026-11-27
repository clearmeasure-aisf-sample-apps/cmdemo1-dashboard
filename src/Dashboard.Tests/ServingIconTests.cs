using System.Collections;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The serving icon's SVG path per <see cref="ServingState"/> (#978): every state has a path, each state has its own,
/// and an undefined state throws rather than falling back.
/// </summary>
public class ServingIconTests
{
    [Fact]
    public void EveryServingStateHasAnIcon()
    {
        foreach (ServingState state in Enum.GetValues<ServingState>())
        {
            var path = ServingIcon.PathData(state);
            Assert.False(string.IsNullOrEmpty(path));
        }
    }

    [Fact]
    public void EachStateHasItsIcon()
    {
        var expected = new Dictionary<ServingState, string>
        {
            [ServingState.Primary] = "M3 8.5l3.2 3.2L13 4.6",
            [ServingState.FailedOver] = "M2 5.5h11M10 2.5l3 3-3 3M14 10.5H3M6 7.5l-3 3 3 3",
            [ServingState.Down] = "M4 4l8 8M12 4l-8 8",
            [ServingState.Pending] = "M3.9 8h.2M7.9 8h.2M11.9 8h.2",
            [ServingState.NoNodes] = "M3.9 8h.2M7.9 8h.2M11.9 8h.2",
        };

        Assert.Equal(Enum.GetValues<ServingState>().OrderBy(s => (int)s).ToArray(), expected.Keys.OrderBy(s => (int)s).ToArray());
        foreach (var (state, path) in expected)
        {
            Assert.Equal(path, ServingIcon.PathData(state));
        }
    }

    [Fact]
    public void AnUndefinedStateThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ServingIcon.PathData((ServingState)99));
    }
}
