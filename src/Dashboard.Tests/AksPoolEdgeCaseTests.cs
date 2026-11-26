namespace Dashboard.Tests;

/// <summary>
/// <see cref="AksPool" /> is one node pool as Azure describes it: eight parts, no rule of its own. The card prints
/// them - <c>AksServiceCard.razor</c> names the pool, its mode, its disk and its two verdicts - and
/// <see cref="ClusterText.PoolSize" /> says its size, so what the record keeps and what it makes of it is what the
/// page shows.
/// <para>
/// What the file makes of it is already pinned where the reading happens: <see cref="ClusterParserTests" /> reads the
/// shipped <c>aks.json</c> pool for pool and says that a pool of nothing but a name is a pool with nothing else said
/// about it, and <see cref="AksServiceEdgeCaseTests" /> holds the pools that are no pools at all (no name, a name of
/// spaces, an element that is text), that the order of the file is the order of the card, and that the name comes back
/// as it was read. Which numbers the file may carry at all - no negative, nothing above <see cref="int" />, a rounded
/// fraction - is <see cref="JsonRead" />'s own rule, pinned in <see cref="JsonReadTests" />.
/// </para>
/// <para>
/// What follows is the record itself, which all of that walks past: the eight arguments landing on the property of
/// the same name (before this file only the name was ever read by name, so no test could tell two swapped parts), the
/// parts held verbatim and unchecked - an empty string is not nothing, spaces are not gone, a name of
/// <see langword="null" /> is kept rather than refused - and zero being a count where nothing is no count. Then the
/// members every assertion above leans on without ever saying a word about them: that two pools are one pool when all
/// eight parts are and that each part alone tells them apart - with the hash that goes with it, which is what makes a
/// pool usable as a key - the line the record says about itself, and the eight parts coming back out in the order they
/// went in.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds no moment and calls nothing, so
/// every value below is one the test brought. The only member of the type that decides anything is the internal
/// <c>Read</c>, and the app declares no <c>InternalsVisibleTo</c> (as <c>Dashboard.Tests.csproj</c> notes of
/// <c>JsonRead</c>), which is why it is pinned through <see cref="AksService.Parse" /> in the two files above rather
/// than here.
/// </para>
/// </summary>
public class AksPoolEdgeCaseTests
{
    /// <summary>A pool with every part filled and the eight values named apart from one another, so that an argument
    /// landing on the wrong property is seen instead of quietly matching another part of the same record.</summary>
    private static AksPool Full => new("pool-alpha", "System", 7, "Standard_D8s_v2", 200, "Running", "Succeeded", "1.31.7");

    /// <summary>A pool that says only its name: the record's own zero, which a file may not be.</summary>
    private static AksPool Only => new("pool-alpha", null, null, null, null, null, null, null);

    // -------------------------------------------------------------------------- the constructor

    /// <summary>The constructor's eight parts, position by position, each come back as the property of the same name -
    /// the order <c>AksPool.Read</c> hands the file over in, and the order the card's row is built in.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var pool = new AksPool("pool-alpha", "System", 7, "Standard_D8s_v2", 200, "Running", "Succeeded", "1.31.7");

        Assert.Equal("pool-alpha", pool.Name);
        Assert.Equal("System", pool.Mode);
        Assert.Equal(7, pool.Count);
        Assert.Equal("Standard_D8s_v2", pool.Size);
        Assert.Equal(200, pool.OsDiskGb);
        Assert.Equal("Running", pool.PowerState);
        Assert.Equal("Succeeded", pool.ProvisioningState);
        Assert.Equal("1.31.7", pool.KubernetesVersion);
    }

    /// <summary>The same constructor with nothing after the name leaves a pool that says nothing else: the other seven
    /// parts null - not zero, not the empty string - which is what lets the card leave the mode, the disk and the
    /// verdicts out instead of printing an empty row.</summary>
    [Fact]
    public void APartThatIsNotGivenIsNothingRatherThanZero()
    {
        var pool = Only;

        Assert.Equal("pool-alpha", pool.Name);
        Assert.Null(pool.Mode);
        Assert.Null(pool.Count);
        Assert.Null(pool.Size);
        Assert.Null(pool.OsDiskGb);
        Assert.Null(pool.PowerState);
        Assert.Null(pool.ProvisioningState);
        Assert.Null(pool.KubernetesVersion);
    }

    // ---------------------------------------------------------------------------- the eight parts

    /// <summary>The record checks nothing and trims nothing: the name it is handed is the name it keeps, whether it is
    /// the name of the sample, another case of it, a name with a space in it, the empty string or a run of spaces -
    /// which is exactly what the card would print as a blank. A file can carry none of the last three, since the
    /// reader turns a blank into nothing and the reader of the pools then walks the element past; they are what a pool
    /// built by hand carries.</summary>
    [Theory]
    [InlineData("pool-alpha")]
    [InlineData("Pool-Alpha")]
    [InlineData("pool alpha")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t  ")]
    public void TheNameIsKeptAsItIsGiven(string name)
    {
        var pool = new AksPool(name, null, null, null, null, null, null, null);

        Assert.Equal(name, pool.Name);

        // The name is the pool's whole identity to its own equality: the same record with another one is another
        // pool, and the line it says about itself carries the characters - the spaces of a blank one and all.
        Assert.Equal(name == "pool-alpha", pool == Only);
        Assert.Contains($"Name = {name}", pool.ToString(), StringComparison.Ordinal);
    }

    /// <summary>The part the file cannot leave out is the one the record does not look at: a <see langword="null" />
    /// name is kept, printed as nothing and still tells two pools apart. It is <c>AksPool.Read</c> that walks past
    /// such an element, and <see cref="AksServiceEdgeCaseTests" /> pins that - the record itself refuses nothing.</summary>
    [Fact]
    public void ANameOfNullIsKeptTooBecauseTheRecordRefusesNothing()
    {
        var pool = new AksPool(null!, "System", 1, "Standard_D2s_v5", 60, "Running", "Succeeded", "1.30.3");

        Assert.Null(pool.Name);
        Assert.Equal("System", pool.Mode);
        Assert.Equal(1, pool.Count);
        Assert.StartsWith("AksPool { Name = , Mode = System", pool.ToString(), StringComparison.Ordinal);
        Assert.False(pool.Equals(Only));
    }

    /// <summary>An empty word is not an absent one, to this record: <c>""</c> stays <c>""</c> and is a different pool
    /// from the one whose part is <see langword="null" />. The two are one and the same only by the time a file has
    /// been read, since the reader turns a blank into nothing before the pool ever sees it.</summary>
    [Fact]
    public void AnEmptyWordIsNotAnAbsentOneToTheRecord()
    {
        var blank = new AksPool("pool-alpha", "", 0, "", 0, "", "", "");
        var absent = Only;

        Assert.Equal("", blank.Mode);
        Assert.Equal("", blank.Size);
        Assert.Equal("", blank.PowerState);
        Assert.Equal("", blank.ProvisioningState);
        Assert.Equal("", blank.KubernetesVersion);
        Assert.Equal(0, blank.Count);
        Assert.Equal(0, blank.OsDiskGb);

        Assert.NotEqual(blank, absent);
        Assert.NotEqual(blank with { Mode = null }, blank);
        Assert.NotEqual(blank with { Size = null }, blank);
        Assert.NotEqual(blank with { PowerState = null }, blank);
        Assert.NotEqual(blank with { ProvisioningState = null }, blank);
        Assert.NotEqual(blank with { KubernetesVersion = null }, blank);
        Assert.NotEqual(blank with { Count = null }, blank);
        Assert.NotEqual(blank with { OsDiskGb = null }, blank);
    }

    /// <summary>The mode is Azure's word, kept as Azure wrote it: <c>System</c>, <c>User</c>, and anything else a
    /// future answer carries - the record compares it to no word of its own, so the case the page prints is the case
    /// of the file and the spaces around it survive this far.</summary>
    [Theory]
    [InlineData("System")]
    [InlineData("User")]
    [InlineData("system")]
    [InlineData(" System ")]
    [InlineData("Permanent")]
    [InlineData("")]
    public void TheModeIsTheWordAzureWritesKeptAsItWasWritten(string mode)
    {
        var pool = Full with { Mode = mode };

        Assert.Equal(mode, pool.Mode);
        Assert.Equal(mode == "System", pool == Full);
        Assert.Contains($"Mode = {mode},", pool.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Zero nodes is a number the record keeps, and a different one from no number at all: it is the reader
    /// that answers <see langword="null" /> for a count the file does not carry, and the page that leaves the size
    /// line out on the strength of it. What a file may carry - not a negative, not a fraction, not a number past
    /// <see cref="int.MaxValue" /> - is <see cref="JsonRead" />'s business, pinned in <see cref="JsonReadTests" />.
    /// Handed to the record by hand, even the two ends of an <see cref="int" /> and a negative survive.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(-3)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void ACountIsTheNumberItWasGiven(int count)
    {
        var pool = Full with { Count = count };

        Assert.Equal(count, pool.Count);
        Assert.Equal(count == 7, pool == Full);
    }

    /// <summary>The same for the disk: the record adds no rule, so a zero GiB pool is a pool of zero GiB and not a
    /// pool whose disk is unknown.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(128)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void ADiskIsTheNumberItWasGiven(int osDiskGb)
    {
        var pool = Full with { OsDiskGb = osDiskGb };

        Assert.Equal(osDiskGb, pool.OsDiskGb);
        Assert.Equal(osDiskGb == 200, pool == Full);
    }

    /// <summary>The size is a VM SKU's name and nothing more to this record: no comparison, no parsing of the cores
    /// out of it, the case exactly as it came.</summary>
    [Theory]
    [InlineData("Standard_D8s_v2")]
    [InlineData("standard_d8s_v2")]
    [InlineData("Standard_D4as_v6")]
    [InlineData("GPU")]
    public void TheSizeIsTheVmNameKeptCharacterForCharacter(string size)
    {
        var pool = Full with { Size = size };

        Assert.Equal(size, pool.Size);
        Assert.Equal(size == "Standard_D8s_v2", pool == Full);
    }

    /// <summary>The two verdicts and the version of the pool's agent are held as the three words they are. The record
    /// interprets none of them: unlike <see cref="AksService.IsStopped" />, which weighs the service's own power
    /// state against <see cref="AksService.Stopped" />, a pool says nothing about what its words mean, so
    /// <c>"Stopped"</c> here is text in a property and nothing more - only the service's own power state says a
    /// cluster is stopped.</summary>
    [Fact]
    public void ThePoolHoldsItsVerdictsAndWeighsNoneOfThem()
    {
        var stopped = Full with { PowerState = "Stopped", ProvisioningState = "Deleting", KubernetesVersion = "1.30.3" };

        Assert.Equal("Stopped", stopped.PowerState);
        Assert.Equal("Deleting", stopped.ProvisioningState);
        Assert.Equal("1.30.3", stopped.KubernetesVersion);

        // To the letter: no case rule and no trimming of its own, whatever the service's own verdict does.
        Assert.NotEqual(stopped, stopped with { PowerState = "stopped" });
        Assert.NotEqual(stopped, stopped with { PowerState = "Stopped " });
        Assert.NotEqual(stopped, stopped with { ProvisioningState = "deleting" });
    }

    // ------------------------------------------------------------------------------ the equality

    /// <summary>Two pools are one pool when all eight parts are, whichever of the four ways of asking is used -
    /// <c>object.Equals</c>, the record's own <c>Equals</c>, <c>==</c> and <c>!=</c> - and a pool read twice from the
    /// same words is one pool. What is not equal to it: the same pool with any one part moved or dropped to nothing,
    /// <see langword="null" />, and a thing of another type that happens to carry the same text.</summary>
    [Fact]
    public void TwoPoolsAreOnePoolWhenAllEightPartsAre()
    {
        var one = Full;
        var same = new AksPool("pool-alpha", "System", 7, "Standard_D8s_v2", 200, "Running", "Succeeded", "1.31.7");

        Assert.True(one.Equals(same));
        Assert.True(one == same);
        Assert.False(one != same);
        Assert.True(one.Equals((object)same));
        Assert.Equal(one, same);
        Assert.Equal(one.GetHashCode(), same.GetHashCode());
        Assert.Equal(one, one with { Name = "pool-alpha", Count = 7 });

        AksPool[] different =
        [
            one with { Name = "pool-beta" },
            one with { Name = "POOL-ALPHA" },
            one with { Name = "pool-alpha " },
            one with { Mode = null },
            one with { Mode = "User" },
            one with { Mode = "system" },
            one with { Count = null },
            one with { Count = 0 },
            one with { Count = 8 },
            one with { Size = null },
            one with { Size = "Standard_D16s_v3" },
            one with { OsDiskGb = null },
            one with { OsDiskGb = 199 },
            one with { PowerState = null },
            one with { PowerState = "Stopped" },
            one with { ProvisioningState = null },
            one with { ProvisioningState = "Updating" },
            one with { KubernetesVersion = null },
            one with { KubernetesVersion = "1.31.6" },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.True(one != other);
        }

        Assert.False(one.Equals(null));
        Assert.False(one.Equals((AksPool?)null));
        Assert.False(one.Equals("pool-alpha"));
        Assert.False(one.Equals(new { Name = "pool-alpha", Mode = "System", Count = 7 }));
        Assert.False(Only.Equals(null));
        Assert.True(Only.Equals(new AksPool("pool-alpha", null, null, null, null, null, null, null)));
    }

    /// <summary>As a key, a pool is its value: two equal instances are one entry of a set and one lookup, a pool that
    /// differs in a single part is another entry, and the pool a file's reading hands over is interchangeable with the
    /// same eight words written out by hand - the reader builds an ordinary record of values, nothing apart.</summary>
    [Fact]
    public void APoolIsAKeyOfItsOwnValue()
    {
        var pools = new HashSet<AksPool> { Full };
        var alsoSeven = new AksPool("pool-alpha", "System", 7, "Standard_D8s_v2", 200, "Running", "Succeeded", "1.31.7");
        var noCount = Full with { Count = null };
        var fromTheFile = ClusterFixture.SampleService.Pools[0];
        var writtenByHand = new AksPool("system", "System", 1, "Standard_D4as_v6", 128, "Running", "Succeeded", "1.33.3");

        Assert.Contains(alsoSeven, pools);
        Assert.DoesNotContain(noCount, pools);

        var asAKey = new HashSet<AksPool> { fromTheFile };

        Assert.Contains(writtenByHand, asAKey);
        Assert.DoesNotContain(writtenByHand with { Mode = "User" }, asAKey);

        pools.Add(alsoSeven);
        pools.Add(fromTheFile);
        Assert.Equal(2, pools.Count);

        var keyed = new Dictionary<AksPool, string> { [Only] = "only" };
        keyed[Only] = "still one";
        keyed[Full] = "full";

        Assert.Equal(2, keyed.Count);
        Assert.Equal("still one", keyed[new AksPool("pool-alpha", null, null, null, null, null, null, null)]);
        Assert.Equal("full", keyed[Full]);
    }

    /// <summary>A copied part never moves the original: every one of the eight is init-only to the outside, so the
    /// pool a card was built round stays what it was while the row prints a different one.</summary>
    [Fact]
    public void ACopyChangesThePartItNamesAndNothingElse()
    {
        var pool = Only;
        var copied = pool with
        {
            Mode = "User",
            Count = 2,
            Size = "Standard_D2s_v5",
            OsDiskGb = 60,
            PowerState = "Running",
            ProvisioningState = "Succeeded",
            KubernetesVersion = "1.30.3",
        };

        Assert.Equal("pool-alpha", pool.Name);
        Assert.Null(pool.Mode);
        Assert.Null(pool.Count);
        Assert.Null(pool.Size);
        Assert.Null(pool.OsDiskGb);
        Assert.Null(pool.PowerState);
        Assert.Null(pool.ProvisioningState);
        Assert.Null(pool.KubernetesVersion);

        var everyPartMoved = Full with
        {
            Mode = "User",
            Count = 2,
            Size = "Standard_D2s_v5",
            OsDiskGb = 60,
            KubernetesVersion = "1.30.3",
        };

        Assert.Equal(everyPartMoved, copied);
        Assert.NotEqual(pool, copied);
    }

    // ----------------------------------------------------------------------- the line and the parts

    /// <summary>The one line the record says about itself - the line a failing test prints - names the type and then
    /// its eight parts in the order they are declared, and says nothing where a part is nothing: nothing is left out
    /// and nothing is added, so a reader comparing two of the lines sees which part moved.</summary>
    [Fact]
    public void TheRecordSaysWhatItIsInOneLineOfItsOwnParts()
    {
        Assert.Equal(
            "AksPool { Name = pool-alpha, Mode = System, Count = 7, Size = Standard_D8s_v2, OsDiskGb = 200, PowerState = Running, ProvisioningState = Succeeded, KubernetesVersion = 1.31.7 }",
            Full.ToString());

        var text = Only.ToString();

        Assert.Equal(
            "AksPool { Name = pool-alpha, Mode = , Count = , Size = , OsDiskGb = , PowerState = , ProvisioningState = , KubernetesVersion =  }",
            text);

        // The parts are said in the order they are declared, and a word with spaces in it is said with them.
        Assert.True(
            text.IndexOf("Name = ", StringComparison.Ordinal) < text.IndexOf("KubernetesVersion = ", StringComparison.Ordinal),
            text);
        Assert.Contains("Size = Standard_D4as_v6", (Full with { Size = "Standard_D4as_v6" }).ToString(), StringComparison.Ordinal);
        Assert.Contains("Mode =  System ", (Full with { Mode = " System " }).ToString(), StringComparison.Ordinal);
    }

    /// <summary>The eight parts come back out in the order they went in - the order of the record's declaration, and
    /// the order the line above says them in.</summary>
    [Fact]
    public void TheEightPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (name, mode, count, size, osDiskGb, powerState, provisioningState, kubernetesVersion) = Full;

        Assert.Equal(
            ("pool-alpha", "System", 7, "Standard_D8s_v2", 200, "Running", "Succeeded", "1.31.7"),
            (name, mode, count, size, osDiskGb, powerState, provisioningState, kubernetesVersion));

        var (onlyName, onlyMode, onlyCount, onlySize, onlyDisk, onlyPower, onlyProvisioning, onlyVersion) = Only;

        Assert.Equal(("pool-alpha", null, null, null, null, null, null, null), (onlyName, onlyMode, onlyCount, onlySize, onlyDisk, onlyPower, onlyProvisioning, onlyVersion));
    }

    // ------------------------------------------------------------------ the record's own shape

    /// <summary>
    /// The whole public surface of <see cref="AksPool" /> as it stands: its eight parts in the order the constructor
    /// names them, and the members every record is asked - falling apart, comparing, hashing and saying itself -
    /// and nothing else to call. The pool says nothing of its own about its words: no computed property, no static
    /// field, no helper method. A ninth part, a computed property, or a static word added to the record without a
    /// test is refused here by name.
    /// </summary>
    [Fact]
    public void ThePoolHasEightPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Name", "Mode", "Count", "Size", "OsDiskGb", "PowerState", "ProvisioningState", "KubernetesVersion"],
            RecordSurface.PropertyNamesOf<AksPool>());

        Assert.Equal(
            [
                "Count", "Deconstruct", "Equals", "GetHashCode", "KubernetesVersion", "Mode", "Name",
                "OsDiskGb", "PowerState", "ProvisioningState", "Size", "ToString", "op_Equality", "op_Inequality",
            ],
            RecordSurface.Of<AksPool>());

        var parameters = RecordSurface.ParametersOf<AksPool>();
        Assert.Equal(
            ["Name", "Mode", "Count", "Size", "OsDiskGb", "PowerState", "ProvisioningState", "KubernetesVersion"],
            [.. parameters.Select(p => p.Name!)]);
        Assert.Equal([false, true, true, true, true, true, true, true], RecordSurface.NullableParts<AksPool>());
    }
}
