namespace Dashboard.Tests;

/// <summary>
/// The line <see cref="ClusterText.Volumes" /> makes out of a namespace's volume claims: the parts are the
/// words <see cref="ClusterText.Volume" /> says each claim in, joined in the order the claims are given with
/// the same separator the rest of the cluster lines join their parts with; nothing is given says the empty
/// word, and the nothing says the nothing it is.
/// </summary>
public class ClusterVolumesLineTests
{
    private const double EightGiB = ClusterFixture.EightGiB;
    private const double OneGiB = 1073741824d;

    /// <summary>Nothing handed to the line is the nothing it is: the null list is refused, named.</summary>
    [Fact]
    public void NothingForAVolumesLineIsRefusedAndNamed()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.Volumes(null!));

        Assert.Equal("volumes", thrown.ParamName);
    }

    /// <summary>The line with no claims in it says the empty word - there is no separator and no label left of it.</summary>
    [Fact]
    public void NoClaimsSaysTheEmptyWord()
    {
        Assert.Equal(string.Empty, ClusterText.Volumes([]));
        Assert.Equal(string.Empty, ClusterText.Volumes(Array.Empty<ClusterVolume>()));
    }

    /// <summary>
    /// One claim says the same line <see cref="ClusterText.Volume" /> says it in: the name, its capacity and
    /// its phase, and the capacity and the phase left out where they are left out.
    /// </summary>
    [Fact]
    public void OneClaimSaysTheSameLineTheVolumeSaysItIn()
    {
        var bound = new ClusterVolume("data-db-0", EightGiB, "Bound");

        Assert.Equal(ClusterText.Volume(bound), ClusterText.Volumes([bound]));
        Assert.Equal("data-db-0 8 GiB, Bound", ClusterText.Volumes([bound]));

        var namelessPhase = new ClusterVolume("logs", OneGiB, null);

        Assert.Equal(ClusterText.Volume(namelessPhase), ClusterText.Volumes([namelessPhase]));
        Assert.Equal("logs 1 GiB", ClusterText.Volumes([namelessPhase]));

        var bare = new ClusterVolume("cache", null, null);

        Assert.Equal("cache", ClusterText.Volumes([bare]));
    }

    /// <summary>
    /// Two claims are joined in the order they are given, with the separator the cluster lines join their
    /// parts with - the first claim whole on the left, the second whole on the right.
    /// </summary>
    [Fact]
    public void TwoClaimsAreJoinedInTheOrderTheyAreGiven()
    {
        var data = new ClusterVolume("data-db-0", EightGiB, "Bound");
        var logs = new ClusterVolume("logs", OneGiB, null);

        Assert.Equal(
            $"{ClusterText.Volume(data)} · {ClusterText.Volume(logs)}",
            ClusterText.Volumes([data, logs]));
        Assert.Equal("data-db-0 8 GiB, Bound · logs 1 GiB", ClusterText.Volumes([data, logs]));

        Assert.Equal("logs 1 GiB · data-db-0 8 GiB, Bound", ClusterText.Volumes([logs, data]));
    }
}
